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

using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndGetPlugins.xaml
    /// </summary>
    public partial class WndGetPlugins
    {
        public WndGetPlugins()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        public WndGetPlugins(IEnumerable<PluginsUpdate> pluginsList)
            : this()
        {
            PNSingleton.Instance.PluginsDownload = true;
            var plugins = pluginsList as PluginsUpdate[] ?? pluginsList.ToArray();
            foreach (var p in plugins.Where(p => p.Type == 0))
            {
                var item = new PNListBoxItem(null, p.Name + " " + p.Suffix, p, false);
                item.ListBoxItemCheckChanged += item_ListBoxItemCheckChanged;
                lstSocial.Items.Add(item);
            }
            foreach (var p in plugins.Where(p => p.Type == 1))
            {
                var item = new PNListBoxItem(null, p.Name + " " + p.Suffix, p, false);
                item.ListBoxItemCheckChanged += item_ListBoxItemCheckChanged;
                lstSync.Items.Add(item);
            }
        }

        private const string ZIP_SUFFIX = "_bin.zip";

        private List<Tuple<string, string, string>> _FilesList;
        private WebClient _WebClient;
        private int _Index;

        private void DlgGetPlugins_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cmdDownload.IsEnabled = cmdCancel.IsEnabled = false;
                if (!prepareDownloadList() || _FilesList == null || _FilesList.Count == 0) return;
                downloadFiles();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void item_ListBoxItemCheckChanged(object sender, ListBoxItemCheckChangedEventArgs e)
        {
            enableDownload();
        }

        private void enableDownload()
        {
            try
            {
                cmdDownload.IsEnabled =
                    lstSocial.Items.OfType<PNListBoxItem>().Any(it => it.IsChecked != null && it.IsChecked.Value) ||
                    lstSync.Items.OfType<PNListBoxItem>().Any(it => it.IsChecked != null && it.IsChecked.Value);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool prepareDownloadList()
        {
            try
            {
                _FilesList = new List<Tuple<string, string, string>>();
                var tempDir = Path.GetTempPath();
                foreach (var item in lstSocial.Items.OfType<PNListBoxItem>().Where(it=>it.IsChecked != null && it.IsChecked.Value))
                {
                    var sb = new StringBuilder();
                    var pu = item.Tag as PluginsUpdate;
                    if (pu == null) continue;
                    sb.Append(PNStrings.URL_DOWNLOAD_DIR);
                    sb.Append(pu.ProductName.Replace(" ", ""));
                    sb.Append(ZIP_SUFFIX);
                    var tuple = Tuple.Create(sb.ToString(),
                                             Path.Combine(tempDir, pu.ProductName.Replace(" ", "") + ZIP_SUFFIX),
                                             Path.Combine(PNPaths.Instance.PluginsDir,
                                                          pu.ProductName.Replace(" ", "")));
                    _FilesList.Add(tuple);
                }
                foreach (var item in lstSync.Items.OfType<PNListBoxItem>().Where(it => it.IsChecked != null && it.IsChecked.Value))
                {
                    var sb = new StringBuilder();
                    var pu = item.Tag as PluginsUpdate;
                    if (pu == null) continue;
                    sb.Append(PNStrings.URL_DOWNLOAD_DIR);
                    sb.Append(pu.ProductName.Replace(" ", ""));
                    sb.Append(ZIP_SUFFIX);
                    var tuple = Tuple.Create(sb.ToString(),
                                             Path.Combine(tempDir, pu.ProductName.Replace(" ", "") + ZIP_SUFFIX),
                                             Path.Combine(PNPaths.Instance.PluginsDir,
                                                          pu.ProductName.Replace(" ", "")));
                    _FilesList.Add(tuple);
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void downloadFiles()
        {
            try
            {
                _WebClient = new WebClient();
                _WebClient.DownloadFileCompleted += _WebClient_DownloadFileCompleted;
                _WebClient.DownloadProgressChanged += _WebClient_DownloadProgressChanged;
                if (File.Exists(_FilesList[0].Item2)) File.Delete(_FilesList[0].Item2);
                _WebClient.DownloadFileAsync(new Uri(_FilesList[0].Item1), _FilesList[0].Item2,
                                             Path.GetFileName(_FilesList[0].Item2));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void _WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                lblDownload.Text = Convert.ToString(e.UserState) + @" " +
                                       e.ProgressPercentage.ToString(PNStatic.CultureInvariant) + @"%";
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void _WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                if (_Index >= _FilesList.Count) return;
                using (var zipFile = new ZipFile(_FilesList[_Index].Item2))
                {
                    var tempDir = Path.GetTempPath();
                    zipFile.ExtractAll(tempDir, ExtractExistingFileAction.OverwriteSilently);
                }
                File.Delete(_FilesList[_Index].Item2);
                _Index++;
                if (_Index < _FilesList.Count)
                {
                    if (File.Exists(_FilesList[_Index].Item2)) File.Delete(_FilesList[_Index].Item2);
                    _WebClient.DownloadFileAsync(new Uri(_FilesList[_Index].Item1), _FilesList[_Index].Item2,
                                                 Path.GetFileName(_FilesList[_Index].Item2));
                }
                else
                {
                    cmdCancel.IsEnabled = false;
                    preparePreRunXml();
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void preparePreRunXml()
        {
            try
            {
                var filePreRun = Path.Combine(Path.GetTempPath(), PNStrings.PRE_RUN_FILE);
                var xdoc = File.Exists(filePreRun) ? XDocument.Load(filePreRun) : new XDocument();
                var xroot = xdoc.Root ?? new XElement(PNStrings.ELM_PRE_RUN);
                var addCopy = false;
                var xcopies = xroot.Element(PNStrings.ELM_COPY_PLUGS);
                if (xcopies == null)
                {
                    addCopy = true;
                    xcopies = new XElement(PNStrings.ELM_COPY_PLUGS);
                }
                else
                {
                    xcopies.RemoveAll();
                }
                foreach (var tuple in _FilesList)
                {
                    var fromPath = tuple.Item2.Substring(0, tuple.Item2.Length - ZIP_SUFFIX.Length);
                    var name = string.IsNullOrEmpty(fromPath) ? "" : Path.GetFileName(fromPath);

                    var xc = new XElement(PNStrings.ELM_COPY);
                    xc.Add(new XAttribute(PNStrings.ATT_NAME, name));
                    xc.Add(new XAttribute(PNStrings.ATT_FROM, fromPath));
                    xc.Add(new XAttribute(PNStrings.ATT_TO, tuple.Item3));
                    xcopies.Add(xc);
                }
                if (addCopy)
                {
                    xroot.Add(xcopies);
                }
                if (xdoc.Root == null)
                    xdoc.Add(xroot);
                xdoc.Save(filePreRun);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgGetPlugins_Unloaded(object sender, RoutedEventArgs e)
        {
            PNSingleton.Instance.PluginsDownload = false;
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DlgGetPlugins_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) cmdCancel.PerformClick();
        }
    }
}
