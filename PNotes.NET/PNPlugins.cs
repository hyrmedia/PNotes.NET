// PNotes.NET - open source desktop notes manager
// Copyright (C) 2015 Andrey Gruber

// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using PluginsCore;

namespace PNotes.NET
{
    internal sealed class PNPlugins
    {
        private static readonly Lazy<PNPlugins> LazyF = new Lazy<PNPlugins>(() => new PNPlugins());

        private PNPlugins()
        {
        }

        internal static PNPlugins Instance { get { return LazyF.Value; } }

        private readonly List<IPostPlugin> _PostPlugins = new List<IPostPlugin>();
        private readonly List<ISyncPlugin> _SyncPlugins = new List<ISyncPlugin>();
        private IPluginsHost _Host;
        private RichTextBox _TempTextBox;

        internal List<IPostPlugin> SocialPlugins
        {
            get { return Instance._PostPlugins; }
        }

        internal List<ISyncPlugin> SyncPlugins
        {
            get { return Instance._SyncPlugins; }
        }

        internal void LoadPlugins(IPluginsHost host, string pluginsDir)
        {
            _Host = host;

            var dir = new DirectoryInfo(pluginsDir);
            var plugins = new PluginsComposition(dir.GetDirectories().Select(d => d.FullName));

            foreach (var type in plugins.Factory.GetPlugins().Select(p => p.GetType()))
            {
                if (type.GetInterface("PluginsCore.ISyncPlugin") != null)
                    SyncPlugins.Add((ISyncPlugin)Activator.CreateInstance(type));
                else if (type.GetInterface("PluginsCore.IPostPlugin") != null)
                    SocialPlugins.Add((IPostPlugin)Activator.CreateInstance(type));
            }

            //var dirs = new DirectoryInfo(pluginsDir).GetDirectories();
            //foreach (
            //    var type in
            //        dirs.Select(d => d.GetFiles(d.Name + ".dll"))
            //            .SelectMany(
            //                files =>
            //                files.Select(f => Assembly.Load(AssemblyName.GetAssemblyName(f.FullName)))
            //                     .Select(
            //                         ass =>
            //                         ass.GetTypes().FirstOrDefault(t => t.GetInterface("PluginsCore.IPlugin") != null))
            //                     .Where(type => type != null)))
            //{
            //    if (type.GetInterface("PluginsCore.ISyncPlugin") != null)
            //        SyncPlugins.Add((ISyncPlugin)Activator.CreateInstance(type));
            //    else if (type.GetInterface("PluginsCore.IPostPlugin") != null)
            //        PostPlugins.Add((IPostPlugin)Activator.CreateInstance(type));
            //}
            foreach (var p in SyncPlugins)
            {
                p.Init(host);
                p.BeforeSync += sync_BeforeSync;
                p.SyncComplete += sync_SyncComplete;
            }
            foreach (var p in SocialPlugins)
            {
                p.Init(host);
                p.PostPerformed += plugin_PostPerformed;
                p.GotPostsPartial += plugin_GotPostsPartial;
                p.GotPostsFull += plugin_GotPostsFull;
            }
        }

        internal static string GetPluginDirectory(string pluginName, string pluginsDir)
        {
            try
            {
                pluginName = pluginName.ToLower().Replace(" ", "");
                var dirInfo = new DirectoryInfo(pluginsDir);
                var dirs = dirInfo.GetDirectories();
                foreach (var d in dirs.Where(d => d.Name.ToLower().Contains(pluginName)))
                {
                    return d.FullName;
                }
                foreach (
                    var d in
                        from d in dirs
                        let files = d.GetFiles()
                        where files.Any(f => f.Name.ToLower().Contains(pluginName))
                        select d)
                {
                    return d.FullName;
                }
                return "";
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private void sync_SyncComplete(object sender, SyncCompleteEventArgs e)
        {
            var caption = PNStrings.PROG_NAME;
            var plugin = sender as ISyncPlugin;
            if (plugin != null)
            {
                caption = plugin.Name;
            }

            switch (e.Result)
            {
                case SyncResult.None:
                    PNMessageBox.Show(PNLang.Instance.GetMessageText("sync_complete", "Synchronization completed successfully"), caption, MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case SyncResult.Reload:
                    PNMessageBox.Show(PNLang.Instance.GetMessageText("sync_complete_reload", "Synchronization completed successfully. The program has to be restarted for applying all changes."), caption, MessageBoxButton.OK, MessageBoxImage.Information);
                    PNData.UpdateTablesAfterSync();
                    PNStatic.FormMain.ApplyAction(MainDialogAction.Restart, null);
                    break;
                case SyncResult.AbortVersion:
                    PNMessageBox.Show(PNLang.Instance.GetMessageText("diff_versions", "Current version of database is different from previously synchronized version. Synchronization cannot be performed."), caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    break;
                case SyncResult.Error:
                    var sb =
                        new StringBuilder(PNLang.Instance.GetMessageText("sync_error_1",
                            "An error occurred during synchronization."));
                    sb.Append(" (");
                    sb.Append(caption);
                    sb.Append(")");
                    sb.AppendLine();
                    sb.Append(PNLang.Instance.GetMessageText("sync_error_2",
                            "Please, refer to log file of appropriate plugin."));
                    var baloon = new Baloon(BaloonMode.Error) { BaloonText = sb.ToString() };
                    PNStatic.FormMain.ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, 10000);
                    break;
            }
        }

        private void sync_BeforeSync(object sender, BeforeSyncEventArgs e)
        {
            if (PNStatic.Settings.Network.SaveBeforeSync)
            {
                PNStatic.FormMain.ApplyAction(MainDialogAction.SaveAll, null);
            }
        }

        private void plugin_PostPerformed(object sender, PostPerformedEventArgs e)
        {
            var p = sender as IPostPlugin;
            if (p == null) return;
            if (!e.Result) return;
            var message =
                PNLang.Instance.GetMessageText("message_posted", "Successfully posted on") + " " +
                p.Name;
            PNMessageBox.Show(message);
        }

        private void plugin_GotPostsFull(object sender, GotPostsEventArgs e)
        {
            var plugin = sender as IPostPlugin;
            if (plugin != null && e.Details != null && e.Details.Count > 0)
            {
                _TempTextBox = _Host.ActiveTextBox;
                var dRepPost = new WndPosts(e.Details.OrderByDescending(p => p.PostDate), plugin.Name, _Host.ActiveNoteName);
                dRepPost.PostSelected += dRepPost_PostSelected;
                var showDialog = dRepPost.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dRepPost.PostSelected -= dRepPost_PostSelected;
                    _TempTextBox = null;
                }
            }
            else
            {
                var name = plugin != null ? plugin.Name : "";
                PNMessageBox.Show(
                    PNLang.Instance.GetMessageText("no_posts", "There are no posts available") + @" [" + name + @"]",
                    PNStrings.PROG_NAME);
            }
        }

        private void plugin_GotPostsPartial(object sender, GotPostsEventArgs e)
        {
            var plugin = sender as IPostPlugin;
            if (plugin != null && e.Details != null && e.Details.Count > 0)
            {
                _TempTextBox = _Host.ActiveTextBox;
                var dInsPost = new WndPosts(e.Details.OrderByDescending(p => p.PostDate), plugin.Name, _Host.ActiveNoteName);
                dInsPost.PostSelected += dInsPost_PostSelected;
                var showDialog = dInsPost.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dInsPost.PostSelected -= dInsPost_PostSelected;
                    _TempTextBox = null;
                }
            }
            else
            {
                var name = plugin != null ? plugin.Name : "";
                PNMessageBox.Show(
                    PNLang.Instance.GetMessageText("no_posts", "There are no posts available") + @" [" + name + @"]",
                    PNStrings.PROG_NAME);
            }
        }

        private void dInsPost_PostSelected(object sender, PostSelectedEventArgs e)
        {
            try
            {
                var d = sender as WndPosts;
                if (d != null)
                {
                    d.PostSelected -= dInsPost_PostSelected;
                }
                if (_TempTextBox != null)
                {
                    _TempTextBox.SelectedText = e.PostText;
                }
                _TempTextBox = null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dRepPost_PostSelected(object sender, PostSelectedEventArgs e)
        {
            try
            {
                var d = sender as WndPosts;
                if (d != null)
                {
                    d.PostSelected -= dRepPost_PostSelected;
                }
                if (_TempTextBox != null)
                {
                    _TempTextBox.Text = e.PostText;
                }
                _TempTextBox = null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }

    [Export(typeof(IPluginsFactory))]
    public class PostFactory : IPluginsFactory
    {
        [ImportMany]
        IEnumerable<Lazy<IPlugin>> plugins;

        #region IPostFactory Members

        public List<IPlugin> GetPlugins()
        {
            return plugins.Select(p => p.Value).ToList();
        }

        #endregion
    }

    internal class PluginsComposition
    {
        private readonly CompositionContainer _Container;

        [Import(typeof(IPluginsFactory))]
        public PostFactory Factory;

        internal PluginsComposition(IEnumerable<string> pluginsDirs)
        {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the Program class
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));
            foreach (var d in pluginsDirs)
            {
                catalog.Catalogs.Add(new DirectoryCatalog(d));
            }

            //Create the CompositionContainer with the parts in the catalog
            _Container = new CompositionContainer(catalog);

            //Fill the imports of this object
            try
            {
                _Container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                PNStatic.LogException(compositionException, false);
            }
            catch (ReflectionTypeLoadException rex)
            {
                PNStatic.LogException(rex, false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
            }
        }
    }
}
