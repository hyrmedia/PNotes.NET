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

using Microsoft.Win32;
using PluginsCore;
using PNotes.NET.Annotations;
using PNRichEdit;
using PNStaticFonts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using Path = System.IO.Path;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSettings.xaml
    /// </summary>
    public partial class WndSettings
    {
        [Flags]
        private enum _ChangesAction
        {
            None = 0,
            SkinsReload = 1,
            Restart = 2
        }

        private class _Language
        {
            public string LangName;
            public string LangFile;
            public string Culture;

            public override string ToString()
            {
                return LangName;
            }
        }

        private class _ButtonSize
        {
            public ToolStripButtonSize ButtonSize;
            public string Name;

            public override string ToString()
            {
                return Name;
            }
        }

        public class SProvider
        {
            public string Name { get; private set; }
            public string Query { get; private set; }

            public SProvider(string n, string q)
            {
                Name = n;
                Query = q;
            }
        }

        public class SExternal
        {
            public string Name { get; private set; }
            public string Prog { get; private set; }
            public string CommLine { get; private set; }

            public SExternal(string n, string p, string c)
            {
                Name = n;
                Prog = p;
                CommLine = c;
            }
        }

        public class SContact : INotifyPropertyChanged
        {
            private ContactConnection _ConnectionStatus;
            public string Name { get; private set; }
            public string CompName { get; private set; }
            public string IpAddress { get; private set; }
            public ImageSource Icon { get; private set; }
            public int ID { get; private set; }

            public ContactConnection ConnectionStatus
            {
                get { return _ConnectionStatus; }
                set
                {
                    if (value == _ConnectionStatus) return;
                    _ConnectionStatus = value;
                    OnPropertyChanged("ConnectionStatus");
                }
            }

            public SContact(int id, string n, string cn, string ip, ImageSource ic)
            {
                ID = id;
                Name = n;
                CompName = cn;
                IpAddress = ip;
                Icon = ic;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class SSmtpClient : INotifyPropertyChanged
        {
            private bool _Selected;
            public bool Selected
            {
                get { return _Selected; }
                set
                {
                    if (_Selected == value) return;
                    _Selected = value;
                    OnPropertyChanged("Selected");
                }
            }
            public string Name { get; private set; }
            public string DispName { get; private set; }
            public string Address { get; private set; }
            public int Port { get; private set; }
            public int Id { get; private set; }

            public SSmtpClient(bool sl, string n, string dn, string a, int p, int id)
            {
                Selected = sl;
                Name = n;
                DispName = dn;
                Address = a;
                Port = p;
                Id = id;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class SMailContact
        {
            public string DispName { get; private set; }
            public string Address { get; private set; }
            public int Id { get; private set; }

            public SMailContact(string dispname, string address, int id)
            {
                DispName = dispname;
                Address = address;
                Id = id;
            }
        }

        public class SSyncComp
        {
            public string CompName { get; private set; }
            public string NotesFile { get; private set; }
            public string DbFile { get; private set; }

            public SSyncComp(string compName, string nfile, string dbfile)
            {
                CompName = compName;
                NotesFile = nfile;
                DbFile = dbfile;
            }
        }

        public class PanelRemove : INotifyPropertyChanged
        {
            private string _name;

            public string Name
            {
                get { return _name; }
                set
                {
                    if (value == _name) return;
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }

            public PanelRemoveMode Mode { get; set; }

            public override string ToString()
            {
                return Name;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class PanelOrientation : INotifyPropertyChanged
        {
            private string _name;

            public string Name
            {
                get { return _name; }
                set
                {
                    if (value == _name) return;
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }

            public NotesPanelOrientation Orientation { get; set; }

            public override string ToString()
            {
                return Name;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public WndSettings()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private readonly string[] _DoubleSingleActions =
        {
            "(No Action)",
            "New Note",
            "Control Panel",
            "Preferences",
            "Search In Notes",
            "Load Note",
            "New Note From Clipboard",
            "Bring All To Front",
            "Save All",
            "Show All/Hide All",
            "Search By Tags",
            "Search By Dates"
        };

        private readonly string[] _DefNames =
        {
            "First characters of note",
            "Current date/time",
            "Current date/time and first characters of note"
        };

        private readonly string[] _PinsUnpins =
        {
            "Unpin",
            "Show pinned window"
        };

        private readonly string[] _StartPos =
        {
            "Center of screen",
            "Left docked",
            "Top docked",
            "Right docked",
            "Bottom docked"
        };

        private bool _Loaded;
        private bool _ChangeLanguage = true;
        private bool _InDefSettingsClick;
        private readonly List<ScrollViewer> _Panels = new List<ScrollViewer>();
        private readonly List<RadioButton> _Radios = new List<RadioButton>();
        private readonly List<_ButtonSize> _ButtonSizes = new List<_ButtonSize>();
        private PNSettings _TempSettings;
        private PNGroup _TempDocking;
        private List<PNSyncComp> _SyncComps;
        private List<PNContactGroup> _Groups;
        private List<PNContact> _Contacts;
        private List<PNExternal> _Externals;
        private List<PNSearchProvider> _SProviders;
        private List<PNSmtpProfile> _SmtpClients;
        private List<PNMailContact> _MailContacts;
        private List<string> _Tags;
        private List<string> _SocialPlugins;
        private List<string> _SyncPlugins;

        private readonly Timer _TimerConnections = new Timer(3000);

        private readonly ObservableCollection<SProvider> _SearchProvidersList = new ObservableCollection<SProvider>();
        private readonly ObservableCollection<SExternal> _ExternalList = new ObservableCollection<SExternal>();
        private readonly ObservableCollection<SContact> _ContactsList = new ObservableCollection<SContact>();
        private readonly ObservableCollection<PNTreeItem> _GroupsList = new ObservableCollection<PNTreeItem>();
        private readonly ObservableCollection<SSmtpClient> _SmtpsList = new ObservableCollection<SSmtpClient>();
        private readonly ObservableCollection<SMailContact> _MailContactsList = new ObservableCollection<SMailContact>();
        private readonly ObservableCollection<SSyncComp> _SyncCompsList = new ObservableCollection<SSyncComp>();

        private readonly ObservableCollection<PanelOrientation> _PanelOrientations =
            new ObservableCollection<PanelOrientation>
            {
                new PanelOrientation {Name = "", Orientation = NotesPanelOrientation.Left},
                new PanelOrientation {Name = "", Orientation = NotesPanelOrientation.Top}
            };

        private readonly ObservableCollection<PanelRemove> _PanelRemoves = new ObservableCollection<PanelRemove>
        {
            new PanelRemove {Name = "", Mode = PanelRemoveMode.SingleClick},
            new PanelRemove {Name = "", Mode = PanelRemoveMode.DoubleClick}
        };

        #region Internal procedures
        internal bool ContactAction(PNContact cn, AddEditMode mode)
        {
            try
            {
                if (mode == AddEditMode.Add)
                {
                    if (_Contacts.Any(c => c.Name == cn.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("contact_exists",
                                                                 "Contact with this name already exists");
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                    _Contacts.Add(cn);
                }
                else
                {
                    var c = _Contacts.FirstOrDefault(con => con.ID == cn.ID);
                    if (c != null)
                    {
                        c.Name = cn.Name;
                        c.ComputerName = cn.ComputerName;
                        c.IpAddress = cn.IpAddress;
                        c.UseComputerName = cn.UseComputerName;
                        c.GroupID = cn.GroupID;
                    }
                }
                fillContacts(false);
                fillGroups(false);
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal bool ContactGroupAction(PNContactGroup cg, AddEditMode mode)
        {
            try
            {
                if (mode == AddEditMode.Add)
                {
                    if (_Groups.Any(g => g.Name == cg.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("group_exists",
                                                                 "Contacts group with this name already exists");
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                    _Groups.Add(cg);
                }
                else
                {
                    var g = _Groups.FirstOrDefault(gr => gr.ID == cg.ID);
                    if (g != null)
                    {
                        g.Name = cg.Name;
                    }
                }
                fillGroups(false);
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal bool ExternalExists(string extName)
        {
            return _Externals.Any(e => e.Name == extName);
        }

        internal void ExternalAdd(PNExternal ext)
        {
            try
            {
                _Externals.Add(ext);
                fillExternals(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ExternalReplace(PNExternal ext)
        {
            try
            {
                var e = _Externals.FirstOrDefault(ex => ex.Name == ext.Name);
                if (e == null) return;
                e.Program = ext.Program;
                e.CommandLine = ext.CommandLine;
                fillExternals(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal bool SearchProviderExists(string spName)
        {
            return _SProviders.Any(s => s.Name == spName);
        }

        internal void SearchProviderAdd(PNSearchProvider sp)
        {
            try
            {
                _SProviders.Add(sp);
                fillSearchProviders(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void SearchProviderReplace(PNSearchProvider sp)
        {
            try
            {
                var s = _SProviders.FirstOrDefault(spv => spv.Name == sp.Name);
                if (s != null)
                {
                    s.QueryString = sp.QueryString;
                }
                fillSearchProviders(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal bool SyncCompExists(string compName)
        {
            return _SyncComps.Any(sc => sc.CompName == compName);
        }

        internal void SyncCompAdd(PNSyncComp sc)
        {
            try
            {
                _SyncComps.Add(sc);
                fillSyncComps(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void SyncCompReplace(PNSyncComp sc)
        {
            try
            {
                var s = _SyncComps.FirstOrDefault(scd => scd.CompName == sc.CompName);
                if (s == null) return;
                s.DataDir = sc.DataDir;
                s.DBDir = sc.DBDir;
                s.UseDataDir = sc.UseDataDir;
                fillSyncComps(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void PanelAutohideChanged()
        {
            try
            {
                chkPanelAutoHide.IsChecked =
                        _TempSettings.Behavior.PanelAutoHide = PNStatic.Settings.Behavior.PanelAutoHide;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void PanelOrientationChanged()
        {
            try
            {
                cboPanelDock.SelectedIndex = (int)PNStatic.Settings.Behavior.NotesPanelOrientation;
                _TempSettings.Behavior.NotesPanelOrientation = PNStatic.Settings.Behavior.NotesPanelOrientation;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Private procedures

        private void fadePanels(ScrollViewer panelHide, ScrollViewer panelShow)
        {
            try
            {
                var storyBoard = TryFindResource("FadeInOut") as Storyboard;
                if (storyBoard == null) return;
                //storyBoard.Children[0].SetValue(Storyboard.TargetNameProperty, panelHide.Name);
                //storyBoard.Children[1].SetValue(Storyboard.TargetNameProperty, panelHide.Name);
                //storyBoard.Children[2].SetValue(Storyboard.TargetNameProperty, panelHide.Name);
                //storyBoard.Children[3].SetValue(Storyboard.TargetNameProperty, panelShow.Name);
                //storyBoard.Children[4].SetValue(Storyboard.TargetNameProperty, panelShow.Name);
                ////storyBoard.Children[5].SetValue(Storyboard.TargetNameProperty, panelShow.Name);
                //storyBoard.Children[1].SetValue(DoubleAnimation.FromProperty, panelHide.ActualWidth);
                ////storyBoard.Children[5].SetValue(DoubleAnimation.ToProperty, panelHide.ActualWidth);
                storyBoard.Children[0].SetValue(Storyboard.TargetNameProperty, panelHide.Name);
                storyBoard.Children[1].SetValue(Storyboard.TargetNameProperty, panelHide.Name);
                storyBoard.Children[2].SetValue(Storyboard.TargetNameProperty, panelShow.Name);
                storyBoard.Children[3].SetValue(Storyboard.TargetNameProperty, panelShow.Name);
                storyBoard.Begin();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNSyncComp getSelectedSyncComp()
        {
            try
            {
                var item = grdLocalSync.SelectedItem as SSyncComp;
                return item == null ? null : _SyncComps.FirstOrDefault(s => s.CompName == item.CompName);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void editSyncComp()
        {
            try
            {
                var sc = getSelectedSyncComp();
                if (sc == null) return;
                var d = new WndSyncComps(this, sc, AddEditMode.Edit);
                var showDialog = d.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillSyncComps(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillSyncComps(bool firstTime = true)
        {
            try
            {
                _SyncCompsList.Clear();
                cmdEditComp.IsEnabled = cmdRemoveComp.IsEnabled = false;
                foreach (var sc in _SyncComps)
                {
                    _SyncCompsList.Add(new SSyncComp(sc.CompName, sc.DataDir, sc.DBDir));
                }
                if (firstTime)
                    grdLocalSync.ItemsSource = _SyncCompsList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void enableFullBackupTime()
        {
            try
            {
                var daysCheck = stkFullBackup.Children.OfType<CheckBox>();
                lblFullBackup.IsEnabled = dtpFullBackup.IsEnabled = daysCheck.Any(c => c.IsChecked != null && c.IsChecked.Value);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void removePlugin(PluginType type, int index)
        {
            try
            {
                if (PNMessageBox.Show(
                    PNLang.Instance.GetMessageText("confir_plugin_remove",
                        "Do you really want to remove selected plugin?"),
                    PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
                var message =
                    PNLang.Instance.GetMessageText("confir_plugin_remove_1",
                        "Plugin removal requires program restart. Do you want to restart the program now?") +
                    '\n' +
                    PNLang.Instance.GetMessageText("confir_plugin_remove_2",
                        "OK - to restart now, No - to restart later, Cancel - to cancel removal");
                var result = PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Cancel:
                        return;
                    case MessageBoxResult.Yes:
                    case MessageBoxResult.No:
                        var addRemove = false;
                        var filePreRun = Path.Combine(Path.GetTempPath(), PNStrings.PRE_RUN_FILE);
                        var xdoc = File.Exists(filePreRun) ? XDocument.Load(filePreRun) : new XDocument();
                        var xroot = xdoc.Root ?? new XElement(PNStrings.ELM_PRE_RUN);
                        var xrem = xroot.Element(PNStrings.ELM_REMOVE);
                        if (xrem == null)
                        {
                            addRemove = true;
                            xrem = new XElement(PNStrings.ELM_REMOVE);
                        }
                        PNListBoxItem item = null;
                        switch (type)
                        {
                            case PluginType.Sync:
                                item = lstSyncPlugins.Items[index] as PNListBoxItem;
                                break;
                            case PluginType.Social:
                                item = lstSocial.Items[index] as PNListBoxItem;
                                break;
                        }
                        if (item == null) return;
                        var plugin = item.Tag as IPlugin;
                        if (plugin == null) return;
                        var pluginDir = PNPlugins.GetPluginDirectory(plugin.Name, PNPaths.Instance.PluginsDir);
                        var xdir = xrem.Elements(PNStrings.ELM_DIR).FirstOrDefault(e => e.Value == pluginDir);
                        if (xdir == null)
                        {
                            xrem.Add(new XElement(PNStrings.ELM_DIR, pluginDir));
                        }
                        if (addRemove)
                        {
                            xroot.Add(xrem);
                        }
                        if (xdoc.Root == null)
                            xdoc.Add(xroot);
                        xdoc.Save(filePreRun);
                        switch (type)
                        {
                            case PluginType.Social:
                                _SocialPlugins.RemoveAll(p => p == plugin.Name);
                                PNPlugins.Instance.SocialPlugins.RemoveAll(p => p.Name == plugin.Name);
                                PNData.SaveSocialPlugins();
                                lstSocial.Items.RemoveAt(index);
                                break;
                            case PluginType.Sync:
                                _SyncPlugins.RemoveAll(p => p == plugin.Name);
                                PNPlugins.Instance.SyncPlugins.RemoveAll(p => p.Name == plugin.Name);
                                PNData.SaveSyncPlugins();
                                lstSyncPlugins.Items.RemoveAt(index);
                                break;
                        }
                        if (result == MessageBoxResult.Yes)
                        {
                            PNStatic.FormMain.ApplyAction(MainDialogAction.Restart, null);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkForNewPluginsVersion()
        {
            try
            {
                if (PNSingleton.Instance.PluginsDownload || PNSingleton.Instance.PluginsChecking ||
                    PNSingleton.Instance.VersionChecking || PNSingleton.Instance.CriticalChecking ||
                    PNSingleton.Instance.ThemesDownload || PNSingleton.Instance.ThemesChecking) return;
                var updater = new PNUpdateChecker();
                updater.PluginsUpdateFound += updater_PluginsUpdateFound;
                updater.IsLatestVersion += updater_IsLatestVersion;
                updater.CheckPluginsNewVersion();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updater_IsLatestVersion(object sender, EventArgs e)
        {
            try
            {
                var updater = sender as PNUpdateChecker;
                if (updater != null)
                {
                    updater.IsLatestVersion -= updater_IsLatestVersion;
                }
                var message = PNLang.Instance.GetMessageText("plugins_latest_version",
                                                                "All plugins are up-to-date.");
                PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updater_PluginsUpdateFound(object sender, PluginsUpdateFoundEventArgs e)
        {
            try
            {
                var updater = sender as PNUpdateChecker;
                if (updater != null)
                {
                    updater.PluginsUpdateFound -= updater_PluginsUpdateFound;
                }
                var d = new WndGetPlugins(e.PluginsList) { Owner = this };
                var showDialog = d.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    promptToRestart();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNMailContact getSelectedMailContact()
        {
            try
            {
                var item = grdMailContacts.SelectedItem as SMailContact;
                return item == null ? null : _MailContacts.FirstOrDefault(c => c.Id == item.Id);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void editMailContact()
        {
            try
            {
                var item = getSelectedMailContact();
                if (item == null) return;
                var dlgMailContact = new WndMailContact(item) { Owner = this };
                dlgMailContact.MailContactChanged += dlgMailContact_MailContactChanged;
                var showDialog = dlgMailContact.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    dlgMailContact.MailContactChanged -= dlgMailContact_MailContactChanged;
                cmdClearMailContacts.IsEnabled = grdMailContacts.Items.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNContact getSelectedContact()
        {
            try
            {
                var item = grdContacts.SelectedItem as SContact;
                return item == null ? null : _Contacts.FirstOrDefault(c => c.ID == item.ID);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void editContact()
        {
            try
            {
                var cont = getSelectedContact();
                if (cont == null) return;
                var dlgContact = new WndContacts(cont, _Groups) { Owner = this };
                dlgContact.ContactChanged += dlgContact_ContactChanged;
                var showDialog = dlgContact.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgContact.ContactChanged -= dlgContact_ContactChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void editSmtp()
        {
            try
            {
                var smtp = getSelectedSmtp();
                if (smtp == null) return;
                var smtpDlg = new WndSmtp(smtp) { Owner = this };
                smtpDlg.SmtpChanged += smtpDlg_SmtpChanged;
                var showDialog = smtpDlg.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    smtpDlg.SmtpChanged -= smtpDlg_SmtpChanged;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNSmtpProfile getSelectedSmtp()
        {
            try
            {
                var item = grdSmtp.SelectedItem as SSmtpClient;
                return item == null ? null : _SmtpClients.FirstOrDefault(s => s.SenderAddress == item.Address);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private PNContactGroup getSelectedContactsGroup()
        {
            try
            {
                var item = tvwContactsGroups.SelectedItem as PNTreeItem;
                if (item == null || item.Tag == null || Convert.ToInt32(item.Tag) == -1) return null;
                return _Groups.FirstOrDefault(g => g.ID == Convert.ToInt32(item.Tag));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void editContactsGroup()
        {
            try
            {
                var gr = getSelectedContactsGroup();
                if (gr == null) return;
                var dlgContactGroup = new WndGroups(gr) { Owner = this };
                dlgContactGroup.ContactGroupChanged += dlgContactGroup_ContactGroupChanged;
                var showDialog = dlgContactGroup.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                {
                    dlgContactGroup.ContactGroupChanged -= dlgContactGroup_ContactGroupChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillGroups(bool firstTime = true)
        {
            try
            {
                _GroupsList.Clear();
                var imageC = TryFindResource("contact") as BitmapImage;// new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "contact.png"));
                var imageG = TryFindResource("group") as BitmapImage;// new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "group.png"));

                var itNone = new PNTreeItem(imageG, PNLang.Instance.GetCaptionText("no_cont_group", PNStrings.NO_GROUP),
                    -1);
                foreach (var cn in _Contacts.Where(c => c.GroupID == -1))
                {
                    itNone.Items.Add(new PNTreeItem(imageC, cn.Name, null));
                }
                _GroupsList.Add(itNone);

                foreach (var gc in _Groups)
                {
                    var id = gc.ID;
                    var it = new PNTreeItem(imageG, gc.Name, id);
                    foreach (var cn in _Contacts.Where(c => c.GroupID == id))
                    {
                        it.Items.Add(new PNTreeItem(imageC, cn.Name, null));
                    }
                    _GroupsList.Add(it);
                }

                if (firstTime)
                    tvwContactsGroups.ItemsSource = _GroupsList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillMailContacts(bool firstTime = true)
        {
            try
            {
                _MailContactsList.Clear();
                cmdEditMailContact.IsEnabled = cmdDeleteMailContact.IsEnabled = false;
                foreach (var mc in _MailContacts)
                    _MailContactsList.Add(new SMailContact(mc.DisplayName, mc.Address, mc.Id));
                cmdClearMailContacts.IsEnabled = _MailContactsList.Count > 0;
                if (firstTime)
                    grdMailContacts.ItemsSource = _MailContactsList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillSmtpClients(bool firstTime = true)
        {
            try
            {
                _SmtpsList.Clear();
                cmdEditSmtp.IsEnabled = cmdDeleteSmtp.IsEnabled = false;
                foreach (var sm in _SmtpClients)
                {
                    _SmtpsList.Add(new SSmtpClient(sm.Active, sm.HostName, sm.DisplayName, sm.SenderAddress, sm.Port, sm.Id));
                }
                if (firstTime)
                    grdSmtp.ItemsSource = _SmtpsList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillPlugins()
        {
            try
            {
                foreach (var p in PNPlugins.Instance.SocialPlugins)
                {
                    var socItem = new PNListBoxItem(null, p.Name, p, _SocialPlugins.Contains(p.Name));
                    socItem.ListBoxItemCheckChanged += socIte_ListBoxItemCheckChanged;
                    lstSocial.Items.Add(socItem);
                }
                foreach (var p in PNPlugins.Instance.SyncPlugins)
                {
                    var syncPlugin = new PNListBoxItem(null, p.Name, p, _SyncPlugins.Contains(p.Name));
                    syncPlugin.ListBoxItemCheckChanged += syncPlugin_ListBoxItemCheckChanged;
                    lstSyncPlugins.Items.Add(syncPlugin);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void syncPlugin_ListBoxItemCheckChanged(object sender, ListBoxItemCheckChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                var item = sender as PNListBoxItem;
                if (item == null) return;
                var plugin = item.Tag as ISyncPlugin;
                if (plugin == null) return;
                if (e.State)
                {
                    _SyncPlugins.Add(plugin.Name);
                }
                else
                {
                    _SyncPlugins.Remove(plugin.Name);
                }
                cmdSyncNow.IsEnabled =
                    lstSyncPlugins.Items.OfType<PNListBoxItem>()
                        .Any(it => it.IsChecked != null && it.IsChecked.Value);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void socIte_ListBoxItemCheckChanged(object sender, ListBoxItemCheckChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                var item = sender as PNListBoxItem;
                if (item == null) return;
                var plugin = item.Tag as IPostPlugin;
                if (plugin == null) return;
                if (e.State)
                {
                    _SocialPlugins.Add(plugin.Name);
                }
                else
                {
                    _SocialPlugins.Remove(plugin.Name);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillContacts(bool firstTime = true)
        {
            try
            {
                _ContactsList.Clear();
                cmdEditContact.IsEnabled = cmdDeleteContact.IsEnabled = false;
                var image = TryFindResource("contact") as BitmapImage;//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "contact.png"));
                foreach (var c in _Contacts)
                {
                    _ContactsList.Add(new SContact(c.ID, c.Name, c.ComputerName, c.IpAddress, image));
                }

                if (firstTime)
                    grdContacts.ItemsSource = _ContactsList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillTags()
        {
            try
            {
                lstTags.Items.Clear();
                foreach (var t in _Tags)
                    lstTags.Items.Add(t);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillExternals(bool firstTime = true)
        {
            try
            {
                _ExternalList.Clear();
                cmdEditExt.IsEnabled = cmdDeleteExt.IsEnabled = false;
                foreach (var ext in _Externals)
                {
                    _ExternalList.Add(new SExternal(ext.Name, ext.Program, ext.CommandLine));
                }
                if (firstTime)
                    grdExternals.ItemsSource = _ExternalList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillSearchProviders(bool firstTime = true)
        {
            try
            {
                _SearchProvidersList.Clear();
                cmdEditProv.IsEnabled = cmdDeleteProv.IsEnabled = false;
                foreach (var sp in _SProviders)
                {
                    _SearchProvidersList.Add(new SProvider(sp.Name, sp.QueryString));
                }
                if (firstTime)
                    grdSearchProvs.ItemsSource = _SearchProvidersList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNSearchProvider getSelectedSearchProvider()
        {
            try
            {
                var sp = grdSearchProvs.SelectedItem as SProvider;
                return sp != null ? _SProviders.FirstOrDefault(p => p.Name == sp.Name) : null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private PNExternal getSelectedExternal()
        {
            try
            {
                var ext = grdExternals.SelectedItem as SExternal;
                return ext != null ? _Externals.FirstOrDefault(e => e.Name == ext.Name) : null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void editSearchProvider(PNSearchProvider sp)
        {
            try
            {
                if (sp == null) return;
                var dsp = new WndSP(this, sp) { Owner = this };
                var showDialog = dsp.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillSearchProviders(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void editExternal(PNExternal ext)
        {
            try
            {
                if (ext == null) return;
                var dex = new WndExternals(this, ext) { Owner = this };
                var showDialog = dex.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillExternals(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadRemoveFromBinPeriods()
        {
            try
            {
                cboDeleteBin.Items.Clear();
                for (var i = 1; i <= 10; i++)
                {
                    cboDeleteBin.Items.Add(i.ToString(CultureInfo.InvariantCulture));
                }
                cboDeleteBin.Items.Add("20");
                cboDeleteBin.Items.Add("30");
                cboDeleteBin.Items.Add("60");
                cboDeleteBin.Items.Add("120");
                cboDeleteBin.Items.Add("360");
                cboDeleteBin.Items.Insert(0, PNLang.Instance.GetMiscText("never", "(Never)"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyFirstTimeLanguage()
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(navBar);
                PNLang.Instance.ApplyControlLanguage(navWarnings);
                PNLang.Instance.ApplyControlLanguage(navButtons);

                cmdSetFontUI.Content = PNLang.Instance.GetMenuText("settings_menu", "cmdSetFontUI", "Change");
                cmdRestoreFontUI.Content = PNLang.Instance.GetMenuText("settings_menu", "cmdResetFontUI", "Reset");

                foreach (var pnl in _Panels)
                {
                    applyPanelLanguage(pnl, pnl.Visibility == Visibility.Visible);
                }

                Title = PNLang.Instance.GetControlText("DlgSettings", "Preferences") + @" [" + getSelectedPanelText() +
                        @"]";

                _PanelOrientations[0].Name = PNLang.Instance.GetMenuText("main_menu", "mnuDAllLeft", "Left");
                _PanelOrientations[1].Name = PNLang.Instance.GetMenuText("main_menu", "mnuDAllTop", "Top");
                _PanelRemoves[0].Name = PNLang.Instance.GetCaptionText("panel_remove_single", "Single click");
                _PanelRemoves[1].Name = PNLang.Instance.GetCaptionText("panel_remove_double", "Double click");
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyLanguage(bool checkName = true)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(navBar);
                PNLang.Instance.ApplyControlLanguage(navWarnings);
                PNLang.Instance.ApplyControlLanguage(navButtons);

                cmdSetFontUI.Content = PNLang.Instance.GetMenuText("settings_menu", "cmdSetFontUI", "Change");
                cmdRestoreFontUI.Content = PNLang.Instance.GetMenuText("settings_menu", "cmdResetFontUI", "Reset");
                var pnl = _Panels.FirstOrDefault(p => p.Visibility == Visibility.Visible);
                if (pnl != null)
                {
                    applyPanelLanguage(pnl, checkName);
                }

                Title = PNLang.Instance.GetControlText("DlgSettings", "Preferences") + @" [" + getSelectedPanelText() +
                        @"]";

                _PanelOrientations[0].Name = PNLang.Instance.GetMenuText("main_menu", "mnuDAllLeft", "Left");
                _PanelOrientations[1].Name = PNLang.Instance.GetMenuText("main_menu", "mnuDAllTop", "Top");
                _PanelRemoves[0].Name = PNLang.Instance.GetCaptionText("panel_remove_single", "Single click");
                _PanelRemoves[1].Name = PNLang.Instance.GetCaptionText("panel_remove_double", "Double click");
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyPanelLanguage(IFrameworkInputElement pnl, bool checkName = true)
        {
            try
            {
                var spnl = PNStatic.Panels.FirstOrDefault(p => p.Name == pnl.Name);
                if (spnl == null) return;
                if (spnl.Language == PNStatic.Settings.GeneralSettings.Language && checkName) return;
                PNLang.Instance.ApplyControlLanguage(pnl);
                if (spnl.Name == pnlGeneral.Name)
                {
                    //special cases
                    if (cboDeleteBin.Items.Count > 0)
                    {
                        var index = cboDeleteBin.SelectedIndex;
                        cboDeleteBin.Items.RemoveAt(0);
                        cboDeleteBin.Items.Insert(0, PNLang.Instance.GetMiscText("never", "(Never"));
                        cboDeleteBin.SelectedIndex = index;
                    }
                    _ButtonSizes[0].Name = PNLang.Instance.GetCaptionText("b_size_normal", "Normal");
                    _ButtonSizes[1].Name = PNLang.Instance.GetCaptionText("b_size_large", "Large");
                    if (cboButtonsSize.Items.Count > 0)
                    {
                        var index = cboButtonsSize.SelectedIndex;
                        cboButtonsSize.Items.Clear();
                        foreach (var bs in _ButtonSizes)
                        {
                            cboButtonsSize.Items.Add(bs);
                        }
                        cboButtonsSize.SelectedIndex = index;
                    }
                }
                else if (pnl.Name == pnlProtection.Name)
                {
                    var daysChecks = stkFullBackup.Children.OfType<CheckBox>();
                    var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                    foreach (var c in daysChecks)
                    {
                        c.Content = ci.DateTimeFormat.GetAbbreviatedDayName((DayOfWeek)Convert.ToInt32(c.Tag));
                    }
                }
                spnl.Language = PNStatic.Settings.GeneralSettings.Language;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private string getSelectedPanelText()
        {
            try
            {
                var opt = _Radios.FirstOrDefault(r => r.IsChecked != null && r.IsChecked.Value);
                if (opt == null) return "";
                var st = opt.Content as StackPanel;
                if (st == null) return "";
                foreach (var c in st.Children.OfType<TextBlock>())
                    return c.Text;
                return "";
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private void prepareLists()
        {
            try
            {
                _ButtonSizes.Add(new _ButtonSize { ButtonSize = ToolStripButtonSize.Normal, Name = "Normal" });
                _ButtonSizes.Add(new _ButtonSize { ButtonSize = ToolStripButtonSize.Large, Name = "Large" });

                if (PNStatic.Panels.Count == 0)
                {
                    PNStatic.Panels.Add(new SettingsPanel { Name = pnlGeneral.Name, Language = "" });
                    PNStatic.Panels.Add(new SettingsPanel { Name = pnlSchedule.Name, Language = "" });
                    PNStatic.Panels.Add(new SettingsPanel { Name = pnlAppearance.Name, Language = "" });
                    PNStatic.Panels.Add(new SettingsPanel { Name = pnlBehavior.Name, Language = "" });
                    PNStatic.Panels.Add(new SettingsPanel { Name = pnlNetwork.Name, Language = "" });
                    PNStatic.Panels.Add(new SettingsPanel { Name = pnlProtection.Name, Language = "" });
                }
                else
                {
                    foreach (var p in PNStatic.Panels)
                        p.Language = "";
                }
                _Panels.Add(pnlGeneral);
                _Panels.Add(pnlSchedule);
                _Panels.Add(pnlAppearance);
                _Panels.Add(pnlBehavior);
                _Panels.Add(pnlNetwork);
                _Panels.Add(pnlProtection);

                _Radios.Add(cmdGeneralSettings);
                _Radios.Add(cmdScheduleSettings);
                _Radios.Add(cmdAppearanceSettings);
                _Radios.Add(cmdBehaviorSettings);
                _Radios.Add(cmdNetworkSettings);
                _Radios.Add(cmdProtectionSettings);

                _Radios[PNStatic.Settings.Config.LastPage].IsChecked = true;

                _Panels[PNStatic.Settings.Config.LastPage].Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addLanguages()
        {
            try
            {
                if (!Directory.Exists(PNPaths.Instance.LangDir)) return;
                var di = new DirectoryInfo(PNPaths.Instance.LangDir);
                var files = di.GetFiles("*.xml").OrderBy(f => f.Name);
                foreach (var fi in files)
                {
                    var xd = XDocument.Load(fi.FullName, LoadOptions.PreserveWhitespace);
                    if (xd.Root == null) continue;
                    var ci = new CultureInfo(xd.Root.Attribute("culture").Value);
                    var name = ci.NativeName;
                    name = name.Substring(0, 1).ToUpper() + name.Substring(1);
                    cboLanguage.Items.Add(new _Language { LangName = name, LangFile = fi.Name, Culture = ci.Name });
                }
                for (var i = 0; i < cboLanguage.Items.Count; i++)
                {
                    var ln = cboLanguage.Items[i] as _Language;
                    if (ln == null || ln.Culture != PNLang.Instance.GetLanguageCulture()) continue;
                    cboLanguage.SelectedIndex = i;
                    break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void allowToHideTrayIcon()
        {
            try
            {
                if (!_TempSettings.Protection.HideTrayIcon) return;
                var hk = PNStatic.HotKeysMain.FirstOrDefault(h => h.MenuName == "mnuLockProg");
                if (hk == null || hk.Shortcut != "") return;
                var message = PNLang.Instance.GetMessageText("hide_on_lock_warning",
                    "In order to allow the tray icon to be hidden when program is locked you have to set a hot key for \"Lock Program\" menu item");
                PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setDefaultSkin(PNTreeItem node, string path)
        {
            try
            {
                foreach (var n in node.Items.OfType<PNTreeItem>())
                {
                    setDefaultSkin(n, path);
                }
                var gr = node.Tag as PNGroup;
                if (gr == null || gr.Skin.SkinName != PNSkinDetails.NO_SKIN) return;
                gr.Skin.SkinName = Path.GetFileNameWithoutExtension(path);
                PNSkinsOperations.LoadSkin(path, gr.Skin);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkAndApplyGroupChanges(PNTreeItem node, List<int> changedSkins)
        {
            try
            {
                var gr = node.Tag as PNGroup;
                if (gr == null) return;
                var rg = PNStatic.Groups.GetGroupByID(gr.ID);
                if (rg != null)
                {
                    if (gr != rg)
                    {
                        if (gr.Skin.SkinName != rg.Skin.SkinName)
                        {
                            changedSkins.Add(gr.ID);
                        }
                        gr.CopyTo(rg);
                        PNData.SaveGroupChanges(rg);
                    }
                    foreach (var n in node.Items.OfType<PNTreeItem>())
                    {
                        checkAndApplyGroupChanges(n, changedSkins);
                    }
                }
                else
                {
                    if (gr.ID != (int)SpecialGroups.Docking) return;
                    if (gr == PNStatic.Docking) return;
                    changedSkins.Add((int)SpecialGroups.Docking);
                    gr.CopyTo(PNStatic.Docking);
                    PNData.SaveGroupChanges(PNStatic.Docking);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool saveCollections()
        {
            try
            {
                if (PNStatic.SyncComps.Inequals(_SyncComps))
                {
                    PNStatic.SyncComps = _SyncComps.PNClone();
                    PNData.SaveSyncComps();
                }
                if (PNStatic.ContactGroups.Inequals(_Groups))
                {
                    PNStatic.ContactGroups = _Groups.PNClone();
                    PNData.SaveContactGroups();
                }
                if (PNStatic.Contacts.Inequals(_Contacts))
                {
                    PNStatic.Contacts = _Contacts.PNClone();
                    PNData.SaveContacts();
                }
                if (PNStatic.Externals.Inequals(_Externals))
                {
                    PNStatic.Externals = _Externals.PNClone();
                    PNData.SaveExternals();
                }
                if (PNStatic.SearchProviders.Inequals(_SProviders))
                {
                    PNStatic.SearchProviders = _SProviders.PNClone();
                    PNData.SaveSearchProviders();
                }
                if (PNStatic.SmtpProfiles.Inequals(_SmtpClients))
                {
                    PNStatic.SmtpProfiles = _SmtpClients.PNClone();
                    PNData.SaveSmtpClients();
                }
                if (PNStatic.MailContacts.Inequals(_MailContacts))
                {
                    PNStatic.MailContacts = _MailContacts.PNClone();
                    PNData.SaveMailContacts();
                }
                if (PNStatic.Tags.Inequals(_Tags))
                {
                    if (PNNotesOperations.ResetNotesTags(_Tags))
                    {
                        PNStatic.Tags = _Tags.PNClone();
                        PNData.SaveTags();
                    }
                }
                if (PNStatic.PostPlugins.Inequals(_SocialPlugins))
                {
                    PNStatic.PostPlugins = _SocialPlugins.PNClone();
                    PNData.SaveSocialPlugins();
                }
                if (PNStatic.SyncPlugins.Inequals(_SyncPlugins))
                {
                    PNStatic.SyncPlugins = _SyncPlugins.PNClone();
                    PNData.SaveSyncPlugins();
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool saveProtection()
        {
            try
            {
                //check for pasword and encrypt/decryp notes if needed
                if (PNStatic.Settings.Protection.PasswordString == _TempSettings.Protection.PasswordString)
                {
                    string passwordString = PNStatic.Settings.Protection.PasswordString;
                    if (passwordString.Length > 0)
                    {
                        // check encryption settings
                        if (!PNStatic.Settings.Protection.StoreAsEncrypted &&
                            _TempSettings.Protection.StoreAsEncrypted)
                        {
                            // no encryption before and encryption after - encrypt all notes
                            PNNotesOperations.EncryptAllNotes(passwordString);
                        }
                        else if (PNStatic.Settings.Protection.StoreAsEncrypted &&
                                 !_TempSettings.Protection.StoreAsEncrypted)
                        {
                            // encryption before and no encryption after - decrypt all notes
                            PNNotesOperations.DecryptAllNotes(passwordString);
                        }
                    }
                }
                else
                {
                    if (PNStatic.Settings.Protection.PasswordString.Length == 0 &&
                        _TempSettings.Protection.PasswordString.Length > 0)
                    {
                        // no password before and password ater - check encryption settings
                        if (_TempSettings.Protection.StoreAsEncrypted)
                        {
                            // encryp all notes
                            PNNotesOperations.EncryptAllNotes(_TempSettings.Protection.PasswordString);
                        }
                    }
                    else if (PNStatic.Settings.Protection.PasswordString.Length > 0 &&
                             _TempSettings.Protection.PasswordString.Length == 0)
                    {
                        // password before and no password after
                        if (PNStatic.Settings.Protection.StoreAsEncrypted)
                        {
                            // decrypt all notes
                            PNNotesOperations.DecryptAllNotes(PNStatic.Settings.Protection.PasswordString);
                        }
                    }
                    else if (PNStatic.Settings.Protection.PasswordString.Length > 0 &&
                             _TempSettings.Protection.PasswordString.Length > 0)
                    {
                        // password has been changed
                        if (PNStatic.Settings.Protection.StoreAsEncrypted &&
                            _TempSettings.Protection.StoreAsEncrypted)
                        {
                            // decrypt all notes using old password
                            PNNotesOperations.DecryptAllNotes(
                                PNStatic.Settings.Protection.PasswordString);
                            // encrypt all notes using new password
                            PNNotesOperations.EncryptAllNotes(_TempSettings.Protection.PasswordString);
                        }
                        else if (PNStatic.Settings.Protection.StoreAsEncrypted)
                        {
                            // decrypt all notes using old password
                            PNNotesOperations.DecryptAllNotes(
                                PNStatic.Settings.Protection.PasswordString);
                        }
                        else if (_TempSettings.Protection.StoreAsEncrypted)
                        {
                            // encrypt all notes using new password
                            PNNotesOperations.EncryptAllNotes(_TempSettings.Protection.PasswordString);
                        }
                    }
                }
                var raiseEvent = PNStatic.Settings.Protection.DontShowContent != _TempSettings.Protection.DontShowContent;
                PNStatic.Settings.Protection = _TempSettings.Protection.PNClone();
                PNData.SaveProtectionSettings();
                if (raiseEvent)
                {
                    PNStatic.FormMain.RaiseContentDisplayChangedEevent();
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool saveNetwork()
        {
            try
            {
                // no exchange before, exchange after
                if (!PNStatic.Settings.Network.EnableExchange && _TempSettings.Network.EnableExchange)
                {
                    PNStatic.Settings.Network.ExchangePort = _TempSettings.Network.ExchangePort;
                    PNStatic.FormMain.StartWCFHosting();
                }
                // exchange before, no exchange after
                else if (PNStatic.Settings.Network.EnableExchange && !_TempSettings.Network.EnableExchange)
                {
                    PNStatic.Settings.Network.ExchangePort = _TempSettings.Network.ExchangePort;
                    PNStatic.FormMain.StopWCFHosting();
                }
                // port number of exchange changed
                else if (PNStatic.Settings.Network.ExchangePort != _TempSettings.Network.ExchangePort)
                {
                    PNStatic.Settings.Network.ExchangePort = _TempSettings.Network.ExchangePort;
                    PNStatic.FormMain.StopWCFHosting();
                    PNStatic.FormMain.StartWCFHosting();
                }
                PNStatic.Settings.Network = _TempSettings.Network.PNClone();
                PNData.SaveNetworkSettings();
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool saveBehavior(ref _ChangesAction result)
        {
            try
            {
                if (PNStatic.Settings.Behavior.HideMainWindow != _TempSettings.Behavior.HideMainWindow)
                {
                    result |= _ChangesAction.Restart;
                }
                if (Math.Abs(PNStatic.Settings.Behavior.Opacity - _TempSettings.Behavior.Opacity) >
                    double.Epsilon)
                {
                    PNNotesOperations.ApplyTransparency(_TempSettings.Behavior.Opacity);
                }
                if (PNStatic.Settings.Behavior.BigIconsOnCP != _TempSettings.Behavior.BigIconsOnCP)
                {
                    if (PNStatic.FormCP != null)
                    {
                        PNStatic.FormCP.SetToolbarIcons(_TempSettings.Behavior.BigIconsOnCP);
                    }
                }
                if (PNStatic.Settings.Behavior.HideCompleted != _TempSettings.Behavior.HideCompleted)
                {
                    if (_TempSettings.Behavior.HideCompleted)
                    {
                        //hide all notes marked as complete
                        var notes = PNStatic.Notes.Where(n => n.Visible && n.Completed);
                        foreach (PNote n in notes)
                        {
                            n.Dialog.ApplyHideNote(n);
                        }
                    }
                }
                var dblActionChanged = PNStatic.Settings.Behavior.DoubleClickAction !=
                                       _TempSettings.Behavior.DoubleClickAction;

                if (PNStatic.Settings.Behavior.KeepVisibleOnShowDesktop !=
                    _TempSettings.Behavior.KeepVisibleOnShowDesktop)
                {
                    PNNotesOperations.ApplyKeepVisibleOnShowDesktop(_TempSettings.Behavior.KeepVisibleOnShowDesktop);
                }

                if (PNStatic.Settings.Behavior.Theme != _TempSettings.Behavior.Theme)
                {
                    //PNStatic.ApplyTheme(_TempSettings.Behavior.Theme);
                    result |= _ChangesAction.Restart;
                }

                var panelFlag = 0;
                if (PNStatic.Settings.Behavior.ShowNotesPanel != _TempSettings.Behavior.ShowNotesPanel)
                {
                    PNNotesOperations.ApplyPanelButtonVisibility(_TempSettings.Behavior.ShowNotesPanel);
                    if (PNStatic.Settings.Behavior.ShowNotesPanel && !_TempSettings.Behavior.ShowNotesPanel)
                    {
                        //panel before and no panel after
                        PNStatic.FormPanel.RemoveAllThumbnails();
                        PNStatic.FormPanel.Hide();
                    }
                    else if (!PNStatic.Settings.Behavior.ShowNotesPanel && _TempSettings.Behavior.ShowNotesPanel)
                    {
                        //no panel before and panel after
                        panelFlag |= 1;
                    }
                }
                if (PNStatic.Settings.Behavior.NotesPanelOrientation != _TempSettings.Behavior.NotesPanelOrientation)
                {
                    panelFlag |= 2;
                }
                if (PNStatic.Settings.Behavior.PanelAutoHide != _TempSettings.Behavior.PanelAutoHide)
                {
                    panelFlag |= 4;
                }

                //unroll all rolled notes if RollOnDoubleClick discarded
                if (PNStatic.Settings.Behavior.RollOnDblClick != _TempSettings.Behavior.RollOnDblClick &&
                    !_TempSettings.Behavior.RollOnDblClick)
                {
                    var notes = PNStatic.Notes.Where(n => n.Rolled);
                    foreach (var note in notes)
                    {
                        if (note.Visible)
                            note.Dialog.ApplyRollUnroll(note);
                        else
                            PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Roll, false, null);
                    }
                }

                PNStatic.Settings.Behavior = _TempSettings.Behavior.PNClone();

                if ((panelFlag & 1) == 1)
                {
                    PNStatic.FormPanel.Show();
                }
                if ((panelFlag & 1) == 1 || (panelFlag & 2) == 2 || (panelFlag & 4) == 4)
                {
                    PNStatic.FormPanel.SetPanelPlacement();
                }
                if ((panelFlag & 2) == 2)
                {
                    PNStatic.FormPanel.UpdateOrientationImageBinding();
                }
                if ((panelFlag & 4) == 4)
                {
                    PNStatic.FormPanel.UpdateAutoHideImageBinding();
                }

                if (dblActionChanged)
                {
                    PNStatic.FormMain.ApplyNewDefaultMenu();
                }

                PNData.SaveBehaviorSettings();
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool saveGeneral(ref _ChangesAction result, ref bool changeDockSize)
        {
            try
            {
                var applyAutoHeight = false;
                if (PNStatic.Settings.GeneralSettings.Language != _TempSettings.GeneralSettings.Language)
                {
                    PNStatic.FormMain.ApplyNewLanguage(_TempSettings.GeneralSettings.Language);
                }
                if (PNStatic.Settings.GeneralSettings.UseSkins != _TempSettings.GeneralSettings.UseSkins)
                {
                    //apply or remove skins
                    PNStatic.Settings.GeneralSettings.UseSkins = _TempSettings.GeneralSettings.UseSkins;
                    result |= _ChangesAction.SkinsReload;
                }
                // hide toolbar
                if (PNStatic.Settings.GeneralSettings.HideToolbar != _TempSettings.GeneralSettings.HideToolbar &&
                    _TempSettings.GeneralSettings.HideToolbar)
                {
                    PNNotesOperations.ApplyHideToolbar();
                }
                // hide or show delete button
                if (PNStatic.Settings.GeneralSettings.HideDeleteButton !=
                    _TempSettings.GeneralSettings.HideDeleteButton)
                {
                    PNNotesOperations.ApplyDeleteButtonVisibility(!_TempSettings.GeneralSettings.HideDeleteButton);
                }
                // change hide to delete
                if (PNStatic.Settings.GeneralSettings.ChangeHideToDelete !=
                    _TempSettings.GeneralSettings.ChangeHideToDelete)
                {
                    PNNotesOperations.ApplyUseAlternative(_TempSettings.GeneralSettings.ChangeHideToDelete);
                }
                // hide or show hide button
                if (PNStatic.Settings.GeneralSettings.HideHideButton !=
                    _TempSettings.GeneralSettings.HideHideButton)
                {
                    PNNotesOperations.ApplyHideButtonVisibility(!_TempSettings.GeneralSettings.HideHideButton);
                }
                // scroll bars
                if (PNStatic.Settings.GeneralSettings.ShowScrollbar !=
                    _TempSettings.GeneralSettings.ShowScrollbar)
                {
                    if (_TempSettings.GeneralSettings.ShowScrollbar == System.Windows.Forms.RichTextBoxScrollBars.None)
                        PNNotesOperations.ApplyShowScrollBars(_TempSettings.GeneralSettings.ShowScrollbar);
                    else
                    {
                        if (!_TempSettings.GeneralSettings.AutoHeight)
                            PNNotesOperations.ApplyShowScrollBars(_TempSettings.GeneralSettings.ShowScrollbar);
                    }
                }
                // auto height
                if (PNStatic.Settings.GeneralSettings.AutoHeight != _TempSettings.GeneralSettings.AutoHeight)
                {
                    // auto height after
                    if (_TempSettings.GeneralSettings.AutoHeight)
                    {
                        // scroll bars after (and may be before)
                        if (_TempSettings.GeneralSettings.ShowScrollbar != System.Windows.Forms.RichTextBoxScrollBars.None)
                        {
                            // remove scroll bars
                            PNNotesOperations.ApplyShowScrollBars(System.Windows.Forms.RichTextBoxScrollBars.None);
                        }
                        // apply auto height
                        applyAutoHeight = true;
                    }
                    else
                    {
                        // scroll bars after (and may be before)
                        if (_TempSettings.GeneralSettings.ShowScrollbar != System.Windows.Forms.RichTextBoxScrollBars.None)
                        {
                            // restore scroll bars
                            PNNotesOperations.ApplyShowScrollBars(_TempSettings.GeneralSettings.ShowScrollbar);
                        }
                    }
                }
                // buttons size
                if (PNStatic.Settings.GeneralSettings.ButtonsSize != _TempSettings.GeneralSettings.ButtonsSize)
                {
                    PNNotesOperations.ApplyButtonsSize(_TempSettings.GeneralSettings.ButtonsSize);
                }
                // custom fonts
                if (PNStatic.Settings.GeneralSettings.UseCustomFonts !=
                    _TempSettings.GeneralSettings.UseCustomFonts)
                {
                    if (_TempSettings.GeneralSettings.UseCustomFonts)
                    {
                        PNInterop.AddCustomFonts();
                    }
                    else
                    {
                        PNInterop.RemoveCustomFonts();
                    }
                }
                // margins
                if (PNStatic.Settings.GeneralSettings.MarginWidth != _TempSettings.GeneralSettings.MarginWidth)
                {
                    if (!PNStatic.Settings.GeneralSettings.UseSkins)
                    {
                        PNNotesOperations.ApplyMarginsWidth(_TempSettings.GeneralSettings.MarginWidth);
                    }
                }
                // docked notes width and/or height
                if (PNStatic.Settings.GeneralSettings.DockWidth != _TempSettings.GeneralSettings.DockWidth ||
                    PNStatic.Settings.GeneralSettings.DockHeight != _TempSettings.GeneralSettings.DockHeight)
                {
                    changeDockSize = true;
                }
                // spell check color
                if (PNStatic.Settings.GeneralSettings.SpellColor != _TempSettings.GeneralSettings.SpellColor)
                {
                    if (Spellchecking.Initialized)
                    {
                        Spellchecking.ColorUnderlining = _TempSettings.GeneralSettings.SpellColor;
                        PNNotesOperations.ApplySpellColor();
                    }
                }
                // autosave
                if (PNStatic.Settings.GeneralSettings.Autosave != _TempSettings.GeneralSettings.Autosave)
                {
                    if (_TempSettings.GeneralSettings.Autosave)
                    {
                        PNStatic.FormMain.TimerAutosave.Interval =
                            _TempSettings.GeneralSettings.AutosavePeriod * 60000;
                        PNStatic.FormMain.TimerAutosave.Start();
                    }
                    else
                    {
                        PNStatic.FormMain.TimerAutosave.Stop();
                    }
                }
                else
                {
                    if (PNStatic.Settings.GeneralSettings.AutosavePeriod !=
                        _TempSettings.GeneralSettings.AutosavePeriod)
                    {
                        if (PNStatic.Settings.GeneralSettings.Autosave)
                        {
                            PNStatic.FormMain.TimerAutosave.Stop();
                            PNStatic.FormMain.TimerAutosave.Interval =
                                _TempSettings.GeneralSettings.AutosavePeriod * 60000;
                            PNStatic.FormMain.TimerAutosave.Start();
                        }
                    }
                }
                // clean bin
                if (PNStatic.Settings.GeneralSettings.RemoveFromBinPeriod !=
                    _TempSettings.GeneralSettings.RemoveFromBinPeriod)
                {
                    if (_TempSettings.GeneralSettings.RemoveFromBinPeriod == 0)
                    {
                        // stop timer
                        PNStatic.FormMain.TimerCleanBin.Stop();
                    }
                    else if (PNStatic.Settings.GeneralSettings.RemoveFromBinPeriod == 0)
                    {
                        // start timer
                        PNStatic.FormMain.TimerCleanBin.Start();
                    }
                }

                //create or delete shortcut
                string shortcutFile = Environment.GetFolderPath(Environment.SpecialFolder.Startup) +
                                      PNStrings.SHORTCUT_FILE;
                if (PNStatic.Settings.GeneralSettings.RunOnStart != _TempSettings.GeneralSettings.RunOnStart)
                {
                    if (_TempSettings.GeneralSettings.RunOnStart)
                    {
                        //create shortcut
                        if (!File.Exists(shortcutFile))
                        {
                            using (var link = new PNShellLink())
                            {
                                link.ShortcutFile = shortcutFile;
                                link.Target = System.Windows.Forms.Application.ExecutablePath;
                                link.WorkingDirectory = System.Windows.Forms.Application.StartupPath;
                                link.IconPath = System.Windows.Forms.Application.ExecutablePath;
                                link.IconIndex = 0;
                                link.Save();
                            }
                        }
                    }
                    else
                    {
                        //delete shortcut
                        if (File.Exists(shortcutFile))
                        {
                            File.Delete(shortcutFile);
                        }
                    }
                }
                PNStatic.Settings.GeneralSettings = (PNGeneralSettings)_TempSettings.GeneralSettings.Clone();
                if (applyAutoHeight)
                {
                    PNNotesOperations.ApplyAutoHeight();
                }
                PNData.SaveGeneralSettings();
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void restoreDefaultValues()
        {
            try
            {
                if (
                    PNMessageBox.Show(
                        PNLang.Instance.GetMessageText("def_warning",
                                                "You are about to reset ALL program settings to their default values. Continue?"),
                        @"PNotes.NET", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
                {
                    //preserve language
                    var language = PNStatic.Settings.GeneralSettings.Language;
                    //preserve password
                    var password = PNStatic.Settings.Protection.PasswordString;

                    _TempSettings.Dispose();
                    _TempSettings = new PNSettings
                    {
                        GeneralSettings = { Language = language },
                        Protection = { PasswordString = password }
                    };

                    initPageGeneral(false);
                    initPageSchedule(false);
                    initPageBehavior(false);
                    initPageNetwork(false);
                    initPageProtection(false);
                    initPageAppearance(false);
                    ((PNTreeItem)tvwGroups.Items[0]).IsSelected = true;
                    //remove active smtp client
                    foreach (var sm in _SmtpClients)
                        sm.Active = false;

                    chkNoSplash.IsChecked = false;

                    cmdRestoreFontUI.PerformClick();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private _ChangesAction applyChanges()
        {
            try
            {
                var result = _ChangesAction.None;
                var changeDockSize = false;

                // hide tray icon checking
                allowToHideTrayIcon();

                var splashFile = Path.Combine(PNPaths.Instance.DataDir, PNStrings.NOSPLASH);
                if (chkNoSplash.IsChecked != null && chkNoSplash.IsChecked.Value)
                {
                    if (!File.Exists(splashFile))
                    {
                        using (new StreamWriter(splashFile, false))
                        {
                        }
                    }
                }
                else
                {
                    if (File.Exists(splashFile))
                    {
                        File.Delete(splashFile);
                    }
                }

                //groups
                if (PNStatic.Settings.GeneralSettings.UseSkins != _TempSettings.GeneralSettings.UseSkins)
                {
                    if (!PNStatic.Settings.GeneralSettings.UseSkins)
                    {
                        if (Directory.Exists(PNPaths.Instance.SkinsDir))
                        {
                            var di = new DirectoryInfo(PNPaths.Instance.SkinsDir);
                            var fi = di.GetFiles("*.pnskn");
                            if (fi.Length > 0)
                            {
                                foreach (var n in tvwGroups.Items.OfType<PNTreeItem>())
                                {
                                    setDefaultSkin(n, fi[0].FullName);
                                }
                            }
                        }
                    }
                }

                var changedSkins = new List<int>();
                foreach (var n in tvwGroups.Items.OfType<PNTreeItem>())
                {
                    checkAndApplyGroupChanges(n, changedSkins);
                }

                //collections
                if (!saveCollections())
                {
                    return _ChangesAction.None;
                }

                //settings
                if (PNStatic.Settings != _TempSettings)
                {
                    //general settings
                    if (PNStatic.Settings.GeneralSettings != _TempSettings.GeneralSettings)
                    {
                        if (!saveGeneral(ref result, ref changeDockSize)) return _ChangesAction.None;
                    }
                    //schedule settings
                    if (PNStatic.Settings.Schedule != _TempSettings.Schedule)
                    {
                        PNStatic.Settings.Schedule = (PNSchedule)_TempSettings.Schedule.Clone();
                        PNData.SaveScheduleSettings();
                    }
                    //behavior
                    if (PNStatic.Settings.Behavior != _TempSettings.Behavior)
                    {
                        if (!saveBehavior(ref result)) return _ChangesAction.None;
                    }
                    //network
                    if (PNStatic.Settings.Network != _TempSettings.Network)
                    {
                        if (!saveNetwork()) return _ChangesAction.None;
                    }
                    //protection
                    if (PNStatic.Settings.Protection != _TempSettings.Protection)
                    {
                        if (!saveProtection()) return _ChangesAction.None;
                    }
                    //diary
                    if (PNStatic.Settings.Diary != _TempSettings.Diary)
                    {
                        PNStatic.Settings.Diary = _TempSettings.Diary.PNClone();
                        PNData.SaveDiarySettings();
                    }
                }

                if ((result & _ChangesAction.SkinsReload) != _ChangesAction.SkinsReload)
                {
                    if (PNStatic.Settings.GeneralSettings.UseSkins && changedSkins.Count > 0)
                    {
                        // change skins for notes if we don't have to reload them
                        var notes = PNStatic.Notes.Where(n => n.Visible);
                        foreach (var n in notes)
                        {
                            if (n.Dialog != null && n.Skin == null)
                            {
                                PNSkinsOperations.ApplyNoteSkin(n.Dialog, n);
                            }
                        }
                    }
                    else if (!PNStatic.Settings.GeneralSettings.UseSkins && changeDockSize)
                    {
                        PNNotesOperations.ChangeDockSize();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return _ChangesAction.None;
            }
        }

        private void promptToRestart()
        {
            try
            {
                var message = PNLang.Instance.GetMessageText("confirm_restart_1",
                                                                 "In order new settings to take effect you have to restart the program.");
                message += '\n';
                message += PNLang.Instance.GetMessageText("confirm_restart_2",
                                                          "Press 'Yes' to restart it now, or 'No' to restart later.");
                if (
                    PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo,
                                    MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    PNStatic.FormMain.ApplyAction(MainDialogAction.Restart, null);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void reloadAllOnSkinsChanges()
        {
            try
            {
                var visible = PNStatic.Notes.Where(n => n.Visible);
                var notes = new List<PNote>();
                foreach (var n in visible)
                {
                    notes.Add(n);
                    PNNotesOperations.ShowHideSpecificNote(n, false);
                }
                foreach (var n in notes)
                {
                    PNNotesOperations.ShowHideSpecificNote(n, true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cleanUpGroups(PNTreeItem node)
        {
            try
            {
                var group = node.Tag as PNGroup;
                if (group != null)
                {
                    group.Dispose();
                }
                foreach (var n in node.Items.OfType<PNTreeItem>())
                {
                    cleanUpGroups(n);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addGroupToTree(PNGroup group, PNTreeItem node)
        {
            try
            {
                var temp = (PNGroup)group.Clone();
                group.CopyTo(temp);

                var n = new PNTreeItem(temp.Image, temp.Name, temp);
                if (node == null)
                {
                    tvwGroups.Items.Add(n);
                }
                else
                {
                    node.Items.Add(n);
                }
                foreach (var g in group.Subgroups.OrderBy(gr => gr.Name))
                {
                    addGroupToTree(g, n);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadSkinsList()
        {
            try
            {
                if (!Directory.Exists(PNPaths.Instance.SkinsDir)) return;
                var di = new DirectoryInfo(PNPaths.Instance.SkinsDir);
                var fi = di.GetFiles("*.pnskn");
                var image = TryFindResource("skins") as BitmapImage;//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "skins.png"));
                foreach (var f in fi)
                {
                    lstSkins.Items.Add(new PNListBoxItem(image, Path.GetFileNameWithoutExtension(f.Name), f.FullName));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadLogFonts()
        {
            try
            {
                var list = new List<LOGFONT>();
                PNStaticFonts.Fonts.GetFontsList(list);
                var ordered = list.OrderBy(f => f.lfFaceName);
                foreach (var lf in ordered)
                {
                    cboFonts.Items.Add(lf);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNGroup selectedGroup()
        {
            try
            {
                var item = tvwGroups.SelectedItem as PNTreeItem;
                if (item != null)
                    return item.Tag as PNGroup;
                return null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void initializeComboBoxes()
        {
            try
            {
                if (!string.IsNullOrEmpty(PNStatic.Settings.GeneralSettings.Language))
                    return;
                //fill only if there is no language setting
                foreach (var s in _DoubleSingleActions)
                {
                    cboDblAction.Items.Add(s);
                    cboSingleAction.Items.Add(s);
                }
                foreach (var s in _DefNames)
                {
                    cboDefName.Items.Add(s);
                }
                foreach (var s in _PinsUnpins)
                {
                    cboPinClick.Items.Add(s);
                }
                foreach (var s in _StartPos)
                {
                    cboNoteStartPosition.Items.Add(s);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Initialization staff
        private void initPageProtection(bool firstTime)
        {
            try
            {
                chkStoreEncrypted.IsChecked = _TempSettings.Protection.StoreAsEncrypted;
                chkHideTrayIcon.IsChecked = _TempSettings.Protection.HideTrayIcon;
                chkBackupBeforeSaving.IsChecked = _TempSettings.Protection.BackupBeforeSaving;
                chkSilentFullBackup.IsChecked = _TempSettings.Protection.SilentFullBackup;
                updBackup.Value = _TempSettings.Protection.BackupDeepness;
                chkDonotShowProtected.IsChecked = _TempSettings.Protection.DontShowContent;
                chkIncludeBinInLocalSync.IsChecked = _TempSettings.Protection.IncludeBinInSync;
                if (PNStatic.Settings.Protection.PasswordString.Trim().Length == 0)
                {
                    cmdChangePwrd.IsEnabled =
                        cmdRemovePwrd.IsEnabled = chkStoreEncrypted.IsEnabled = chkHideTrayIcon.IsEnabled = false;
                    cmdCreatePwrd.IsEnabled = true;
                }
                else
                {
                    cmdChangePwrd.IsEnabled =
                        cmdRemovePwrd.IsEnabled = chkStoreEncrypted.IsEnabled = chkHideTrayIcon.IsEnabled = true;
                    cmdCreatePwrd.IsEnabled = false;
                }
                if (firstTime)
                {
                    fillSyncComps();
                }
                var daysChecks = stkFullBackup.Children.OfType<CheckBox>();
                foreach (var c in daysChecks)
                {
                    c.IsChecked = _TempSettings.Protection.FullBackupDays.Contains((DayOfWeek)Convert.ToInt32(c.Tag));
                }
                enableFullBackupTime();

                if (_TempSettings.Protection.FullBackupTime != DateTime.MinValue)
                {
                    dtpFullBackup.DateValue = _TempSettings.Protection.FullBackupTime;
                }
                chkPromptForPassword.IsChecked = _TempSettings.Protection.PromptForPassword;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initPageNetwork(bool firstTime)
        {
            try
            {
                chkIncludeBinInSync.IsChecked = _TempSettings.Network.IncludeBinInSync;
                chkSyncOnStart.IsChecked = _TempSettings.Network.SyncOnStart;
                chkSaveBeforeSync.IsChecked = _TempSettings.Network.SaveBeforeSync;
                chkEnableExchange.IsChecked = _TempSettings.Network.EnableExchange;
                chkAllowPing.IsChecked = _TempSettings.Network.AllowPing;
                chkSaveBeforeSending.IsChecked = _TempSettings.Network.SaveBeforeSending;
                chkNoNotifyOnArrive.IsChecked = _TempSettings.Network.NoNotificationOnArrive;
                chkShowRecOnClick.IsChecked = _TempSettings.Network.ShowReceivedOnClick;
                chkShowIncomingOnClick.IsChecked = _TempSettings.Network.ShowIncomingOnClick;
                chkNoSoundOnArrive.IsChecked = _TempSettings.Network.NoSoundOnArrive;
                chkNoNotifyOnSend.IsChecked = _TempSettings.Network.NoNotificationOnSend;
                chkShowAfterReceiving.IsChecked = _TempSettings.Network.ShowAfterArrive;
                chkHideAfterSending.IsChecked = _TempSettings.Network.HideAfterSending;
                chkNoContInContextMenu.IsChecked = _TempSettings.Network.NoContactsInContextMenu;
                chkRecOnTop.IsChecked = _TempSettings.Network.ReceivedOnTop;

                cmdSyncNow.IsEnabled = lstSyncPlugins.SelectedIndex > -1 &&
                                       lstSyncPlugins.SelectedItems.OfType<PNTreeItem>().Any(it => it.IsChecked != null && it.IsChecked.Value);

                txtExchPort.Value = _TempSettings.Network.ExchangePort;

                if (firstTime)
                {
                    fillContacts();
                    fillGroups();
                    fillPlugins();
                    fillSmtpClients();
                    fillMailContacts();
                }
                for (var i = 0; i < cboPostCount.Items.Count; i++)
                {
                    if (Convert.ToInt32(cboPostCount.Items[i]) != _TempSettings.Network.PostCount) continue;
                    cboPostCount.SelectedIndex = i;
                    break;
                }
                cboPostCount.IsEnabled = lstSocial.Items.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initPageBehavior(bool firstTime)
        {
            try
            {
                if (firstTime)
                {
                    for (var i = 1; i <= 128; i++)
                    {
                        cboLengthOfContent.Items.Add(i);
                        cboLengthOfName.Items.Add(i);
                    }
                    cboPanelDock.ItemsSource = _PanelOrientations;
                    cboPanelRemove.ItemsSource = _PanelRemoves;
                }
                chkNewOnTop.IsChecked = _TempSettings.Behavior.NewNoteAlwaysOnTop;
                chkRelationalPosition.IsChecked = _TempSettings.Behavior.RelationalPositioning;
                chkHideCompleted.IsChecked = _TempSettings.Behavior.HideCompleted;
                chkShowBigIcons.IsChecked = _TempSettings.Behavior.BigIconsOnCP;
                chkDontShowList.IsChecked = _TempSettings.Behavior.DoNotShowNotesInList;
                chkKeepVisibleOnShowdesktop.IsChecked = _TempSettings.Behavior.KeepVisibleOnShowDesktop;
                chkHideFluently.IsChecked = _TempSettings.Behavior.HideFluently;
                chkPlaySoundOnHide.IsChecked = _TempSettings.Behavior.PlaySoundOnHide;
                chkShowSeparateNotes.IsChecked = _TempSettings.Behavior.ShowSeparateNotes;
                var os = Environment.OSVersion;
                var vs = os.Version;
                if (os.Platform == PlatformID.Win32NT)
                {
                    if (vs.Major < 6)
                    {
                        chkKeepVisibleOnShowdesktop.IsEnabled = false;
                    }
                }
                else
                {
                    chkKeepVisibleOnShowdesktop.IsEnabled = false;
                }
                cboDblAction.SelectedIndex = Convert.ToInt32(_TempSettings.Behavior.DoubleClickAction);
                cboSingleAction.SelectedIndex = Convert.ToInt32(_TempSettings.Behavior.SingleClickAction);
                cboDefName.SelectedIndex = Convert.ToInt32(_TempSettings.Behavior.DefaultNaming);
                cboLengthOfName.SelectedItem = _TempSettings.Behavior.DefaultNameLength;
                cboLengthOfContent.SelectedItem = _TempSettings.Behavior.ContentColumnLength;
                trkTrans.Value = 100 - Convert.ToInt32((_TempSettings.Behavior.Opacity * 100));
                chkRandBack.IsChecked = _TempSettings.Behavior.RandomBackColor;
                chkInvertText.IsChecked = _TempSettings.Behavior.InvertTextColor;
                chkRoll.IsChecked = _TempSettings.Behavior.RollOnDblClick;
                chkFitRolled.IsChecked = _TempSettings.Behavior.FitWhenRolled;
                cboPinClick.SelectedIndex = Convert.ToInt32(_TempSettings.Behavior.PinClickAction);
                cboNoteStartPosition.SelectedIndex = Convert.ToInt32(_TempSettings.Behavior.StartPosition);
                chkHideMainWindow.IsChecked = _TempSettings.Behavior.HideMainWindow;
                chkPreventResizing.IsChecked = _TempSettings.Behavior.PreventAutomaticResizing;
                chkShowPanel.IsChecked = _TempSettings.Behavior.ShowNotesPanel;
                cboPanelDock.SelectedItem =
                    _PanelOrientations.FirstOrDefault(po => po.Orientation == _TempSettings.Behavior.NotesPanelOrientation);
                cboPanelRemove.SelectedItem =
                    _PanelRemoves.First(pr => pr.Mode == _TempSettings.Behavior.PanelRemoveMode);
                chkPanelAutoHide.IsChecked = _TempSettings.Behavior.PanelAutoHide;
                chkPanelSwitchOffAnimation.IsChecked = _TempSettings.Behavior.PanelSwitchOffAnimation;
                for (var i = 0; i < cboPanelDelay.Items.Count; i++)
                {
                    var dl = (double)cboPanelDelay.Items[i];
                    if (!(Math.Abs(_TempSettings.Behavior.PanelEnterDelay / 1000.0 - dl) < double.Epsilon)) continue;
                    cboPanelDelay.SelectedIndex = i;
                    break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initPageAppearance(bool firstTime)
        {
            try
            {
                if (!PNStatic.CheckSkinsExistance()) optSkinnable.IsEnabled = false;
                if (_TempSettings.GeneralSettings.UseSkins)
                {
                    optSkinnable.IsChecked = true;
                }
                else
                {
                    optSkinless.IsChecked = true;
                }
                txtDockWidth.Value = _TempSettings.GeneralSettings.DockWidth;
                txtDockHeight.Value = _TempSettings.GeneralSettings.DockHeight;
                chkAddWeekdayName.IsChecked = _TempSettings.Diary.AddWeekday;
                chkFullWeekdayName.IsChecked = _TempSettings.Diary.FullWeekdayName;
                chkWeekdayAtTheEnd.IsChecked = _TempSettings.Diary.WeekdayAtTheEnd;
                chkNoPreviousDiary.IsChecked = _TempSettings.Diary.DoNotShowPrevious;
                chkDiaryAscOrder.IsChecked = _TempSettings.Diary.AscendingOrder;

                cboNumberOfDiaries.SelectedItem = _TempSettings.Diary.NumberOfPages;
                cboDiaryNaming.SelectedItem = _TempSettings.Diary.DateFormat;
                if (firstTime)
                {
                    foreach (var g in PNStatic.Groups[0].Subgroups.OrderBy(gr => gr.Name))
                    {
                        addGroupToTree(g, null);
                    }
                    var gd = PNStatic.Groups.GetGroupByID(Convert.ToInt32(SpecialGroups.Diary));
                    if (gd != null)
                    {
                        addGroupToTree(gd, null);
                    }
                    addGroupToTree(_TempDocking, null);
                    loadSkinsList();
                    loadLogFonts();
                    lstThemes.ItemsSource = PNStatic.Themes.Keys;
                }
                ((TreeViewItem)tvwGroups.Items[0]).IsSelected = true;
                if (PNStatic.Themes.Keys.Contains(_TempSettings.Behavior.Theme))
                {
                    lstThemes.SelectedItem =
                        lstThemes.Items.OfType<string>().FirstOrDefault(s => s == _TempSettings.Behavior.Theme);
                }
                else
                {
                    lstThemes.SelectedIndex = 0;
                }
                if (firstTime)
                {
                    cboFontColor.IsDropDownOpen = true;
                    cboFontColor.IsDropDownOpen = false;
                    cboFontSize.IsDropDownOpen = true;
                    cboFontSize.IsDropDownOpen = false;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initPageSchedule(bool firstTime)
        {
            try
            {
                if (firstTime)
                {
                    var image = TryFindResource("loudspeaker") as BitmapImage;// new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "loudspeaker.png"));
                    clbSounds.Items.Add(
                        new PNListBoxItem(image, PNSchedule.DEF_SOUND, PNSchedule.DEF_SOUND, PNSchedule.DEF_SOUND));
                    if (Directory.Exists(PNPaths.Instance.SoundsDir))
                    {
                        var fi = new DirectoryInfo(PNPaths.Instance.SoundsDir).GetFiles("*.wav");
                        foreach (var f in fi)
                        {
                            clbSounds.Items.Add(new PNListBoxItem(image, Path.GetFileNameWithoutExtension(f.Name),
                                f.FullName));
                        }
                    }
                    foreach (var s in PNStatic.Voices)
                    {
                        lstVoices.Items.Add(new PNListBoxItem(image, s, s));
                    }
                }
                chkAllowSound.IsChecked = _TempSettings.Schedule.AllowSoundAlert;
                chkVisualNotify.IsChecked = _TempSettings.Schedule.VisualNotification;
                chkTrackOverdue.IsChecked = _TempSettings.Schedule.TrackOverdue;
                chkCenterScreen.IsChecked = _TempSettings.Schedule.CenterScreen;
                var item =
                    clbSounds.Items.OfType<PNListBoxItem>()
                        .FirstOrDefault(li => li.Text == _TempSettings.Schedule.Sound);
                if (item != null)
                    clbSounds.SelectedItem = item;
                else
                {
                    clbSounds.SelectedIndex = 0;
                    _TempSettings.Schedule.Sound = PNSchedule.DEF_SOUND;
                }
                if (PNStatic.Voices.Count > 0)
                {

                    item = lstVoices.Items.OfType<PNListBoxItem>()
                        .FirstOrDefault(li => li.Text == _TempSettings.Schedule.Voice);
                    if (item != null)
                    {
                        lstVoices.SelectedItem = item;
                    }
                    else
                    {
                        lstVoices.SelectedIndex = 0;
                    }
                    txtVoiceSample.Text = "";
                }
                trkVolume.Value = _TempSettings.Schedule.VoiceVolume;
                trkSpeed.Value = _TempSettings.Schedule.VoiceSpeed;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initPageGeneral(bool firstTime)
        {
            try
            {
                if (firstTime)
                {
                    addLanguages();
                    fillSearchProviders();
                    fillExternals();
                    fillTags();
                    loadRemoveFromBinPeriods();
                }
                chkHideToolbar.IsChecked = _TempSettings.GeneralSettings.HideToolbar;
                chkCustomFont.IsChecked = _TempSettings.GeneralSettings.UseCustomFonts;
                chkHideDelete.IsChecked = _TempSettings.GeneralSettings.HideDeleteButton;
                chkChangeHideToDelete.IsChecked = _TempSettings.GeneralSettings.ChangeHideToDelete;
                chkHideHide.IsChecked = _TempSettings.GeneralSettings.HideHideButton;
                cboIndent.SelectedItem = (int)_TempSettings.GeneralSettings.BulletsIndent;
                cboParIndent.SelectedItem = _TempSettings.GeneralSettings.ParagraphIndent;
                cboMargins.SelectedItem = (int)_TempSettings.GeneralSettings.MarginWidth;
                txtDTFShort.Text = _TempSettings.GeneralSettings.DateFormat;
                txtTFLong.Text = _TempSettings.GeneralSettings.TimeFormat;
                chkSaveOnExit.IsChecked = _TempSettings.GeneralSettings.SaveOnExit;
                chkConfirmDelete.IsChecked = _TempSettings.GeneralSettings.ConfirmBeforeDeletion;
                chkConfirmSave.IsChecked = _TempSettings.GeneralSettings.ConfirmSaving;
                chkSaveWithoutConfirm.IsChecked = _TempSettings.GeneralSettings.SaveWithoutConfirmOnHide;
                chkAutosave.IsChecked = _TempSettings.GeneralSettings.Autosave;
                updAutosave.Value = _TempSettings.GeneralSettings.AutosavePeriod;
                cboDeleteBin.SelectedIndex = _TempSettings.GeneralSettings.RemoveFromBinPeriod > 0
                                                 ? cboDeleteBin.Items.IndexOf(
                                                     _TempSettings.GeneralSettings.RemoveFromBinPeriod.ToString(
                                                         PNStatic.CultureInvariant))
                                                 : 0;
                chkWarnBeforeEmptyBin.IsChecked = _TempSettings.GeneralSettings.WarnOnAutomaticalDelete;
                chkWarnBeforeEmptyBin.IsEnabled = cboDeleteBin.SelectedIndex > 0;
                chkRunOnStart.IsChecked = _TempSettings.GeneralSettings.RunOnStart;
                chkShowCPOnStart.IsChecked = _TempSettings.GeneralSettings.ShowCPOnStart;
                chkCheckNewVersionOnStart.IsChecked = _TempSettings.GeneralSettings.CheckNewVersionOnStart;
                chkCheckCriticalOnStart.IsChecked = _TempSettings.GeneralSettings.CheckCriticalOnStart;
                chkCheckCriticalPeriodically.IsChecked = _TempSettings.GeneralSettings.CheckCriticalPeriodically;
                chkShowPriority.IsChecked = _TempSettings.GeneralSettings.ShowPriorityOnStart;
                txtWidthSknlsDef.Value = _TempSettings.GeneralSettings.Width;
                txtHeightSknlsDef.Value = _TempSettings.GeneralSettings.Height;
                pckSpell.SelectedColor = Color.FromArgb(_TempSettings.GeneralSettings.SpellColor.A,
                    _TempSettings.GeneralSettings.SpellColor.R, _TempSettings.GeneralSettings.SpellColor.G,
                    _TempSettings.GeneralSettings.SpellColor.B);
                foreach (var bs in _ButtonSizes)
                {
                    cboButtonsSize.Items.Add(bs);
                }
                for (var i = 0; i < cboButtonsSize.Items.Count; i++)
                {
                    var bs = cboButtonsSize.Items[i] as _ButtonSize;
                    if (bs == null || bs.ButtonSize != _TempSettings.GeneralSettings.ButtonsSize) continue;
                    cboButtonsSize.SelectedIndex = i;
                    break;
                }
                for (var i = 0; i < cboScrollBars.Items.Count; i++)
                {
                    var scb = (System.Windows.Forms.RichTextBoxScrollBars)i;
                    if (_TempSettings.GeneralSettings.ShowScrollbar != scb) continue;
                    cboScrollBars.SelectedIndex = i;
                    break;
                }
                chkAutomaticSmilies.IsChecked = _TempSettings.GeneralSettings.AutomaticSmilies;
                updSpace.Value = _TempSettings.GeneralSettings.SpacePoints;
                chkNoSplash.IsChecked = File.Exists(Path.Combine(PNPaths.Instance.DataDir, PNStrings.NOSPLASH));
                chkRestoreAutomatically.IsChecked = _TempSettings.GeneralSettings.RestoreAuto;
                chkAutoHeight.IsChecked = _TempSettings.GeneralSettings.AutoHeight;
                chkDeleteShortExit.IsChecked = _TempSettings.GeneralSettings.DeleteShortcutsOnExit;
                chkRestoreShortStart.IsChecked = _TempSettings.GeneralSettings.RestoreShortcutsOnStart;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Event handlers
        private void FormMain_LanguageChanged(object sender, EventArgs e)
        {
            if (_ChangeLanguage)
            {
                applyLanguage(false);
            }
        }
        #endregion

        #region Window staff
        private void DlgSettings_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNStatic.FormSettings = this;
                prepareLists();
                initializeComboBoxes();

                var f = new PNFont();
                cmdRestoreFontUI.IsEnabled = f != PNSingleton.Instance.FontUser;

                applyFirstTimeLanguage();
                //applyLanguage();

                PNStatic.FormMain.LanguageChanged += FormMain_LanguageChanged;

                _TempSettings = PNStatic.Settings.PNClone();
                _SyncComps = PNStatic.SyncComps.PNClone();
                _Groups = PNStatic.ContactGroups.PNClone();
                _Contacts = PNStatic.Contacts.PNClone();
                _Externals = PNStatic.Externals.PNClone();
                _SProviders = PNStatic.SearchProviders.PNClone();
                _SmtpClients = PNStatic.SmtpProfiles.PNClone();
                _MailContacts = PNStatic.MailContacts.PNClone();
                _Tags = PNStatic.Tags.PNClone();
                _TempDocking = (PNGroup)PNStatic.Docking.Clone();
                _SocialPlugins = PNStatic.PostPlugins.PNClone();
                _SyncPlugins = PNStatic.SyncPlugins.PNClone();

                initPageGeneral(true);
                initPageSchedule(true);
                initPageAppearance(true);
                initPageBehavior(true);
                initPageNetwork(true);
                initPageProtection(true);

                _TimerConnections.Elapsed += _TimerConnections_Elapsed;
                if (_TempSettings.Network.EnableExchange)
                    _TimerConnections.Start();

                _Loaded = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgSettings_Closed(object sender, EventArgs e)
        {
            try
            {
                PNStatic.FormSettings = null;
                PNStatic.FormMain.LanguageChanged -= FormMain_LanguageChanged;
                PNData.SaveLastPage();
                foreach (var n in tvwGroups.Items.OfType<PNTreeItem>())
                {
                    cleanUpGroups(n);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgSettings_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Escape) cmdCancel.PerformClick();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgSettings_Activated(object sender, EventArgs e)
        {
            try
            {
                PNStatic.DeactivateNotesWindows();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Navigation buttons staff
        private void OptionButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Radios.Count == 0) return;
                var opt = sender as RadioButton;
                if (opt == null) return;
                var indexHide = PNStatic.Settings.Config.LastPage;
                PNStatic.Settings.Config.LastPage = _Radios.IndexOf(opt);
                var indexShow = PNStatic.Settings.Config.LastPage;
                applyPanelLanguage(_Panels[PNStatic.Settings.Config.LastPage]);
                Title = PNLang.Instance.GetControlText("DlgSettings", "Preferences") + @" [" + getSelectedPanelText() +
                        @"]";
                var pnlHide = _Panels[indexHide];
                var pnlShow = _Panels[indexShow];
                fadePanels(pnlHide, pnlShow);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Common buttons staff
        private void cmdDef_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _InDefSettingsClick = true;
                restoreDefaultValues();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _InDefSettingsClick = false;
            }
        }

        private void cmdSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ChangeLanguage = false;
                var changes = applyChanges();
                if ((changes & _ChangesAction.Restart) == _ChangesAction.Restart)
                {
                    promptToRestart();
                }
                if ((changes & _ChangesAction.SkinsReload) == _ChangesAction.SkinsReload)
                {
                    PNSingleton.Instance.InSkinReload = true;
                    reloadAllOnSkinsChanges();
                    PNSingleton.Instance.InSkinReload = false;
                }
                Close();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cmdApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var changes = applyChanges();
                if ((changes & _ChangesAction.Restart) == _ChangesAction.Restart)
                {
                    promptToRestart();
                }
                if ((changes & _ChangesAction.SkinsReload) == _ChangesAction.SkinsReload)
                {
                    PNSingleton.Instance.InSkinReload = true;
                    reloadAllOnSkinsChanges();
                    PNSingleton.Instance.InSkinReload = false;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region General staff
        private void cboLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboLanguage.SelectedIndex <= -1) return;
                var lang = cboLanguage.Items[cboLanguage.SelectedIndex] as _Language;
                if (lang != null) _TempSettings.GeneralSettings.Language = lang.LangFile;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddLang_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PNStatic.LoadPage(PNStrings.URL_LANGS);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdSetFontUI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fd = new WndFontChooser(PNSingleton.Instance.FontUser) { Owner = this };
                var showDialog = fd.ShowDialog();
                if (showDialog == null || !showDialog.Value) return;
                PNSingleton.Instance.FontUser.FontFamily = fd.SelectedFont.FontFamily;
                PNSingleton.Instance.FontUser.FontSize = fd.SelectedFont.FontSize;
                PNSingleton.Instance.FontUser.FontStretch = fd.SelectedFont.FontStretch;
                PNSingleton.Instance.FontUser.FontStyle = fd.SelectedFont.FontStyle;
                PNSingleton.Instance.FontUser.FontWeight = fd.SelectedFont.FontWeight;
                PNData.SaveFontUi();
                cmdRestoreFontUI.IsEnabled = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdRestoreFontUI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var f = new PNFont();
                PNSingleton.Instance.FontUser.FontFamily = f.FontFamily;
                PNSingleton.Instance.FontUser.FontSize = f.FontSize;
                PNSingleton.Instance.FontUser.FontStretch = f.FontStretch;
                PNSingleton.Instance.FontUser.FontStyle = f.FontStyle;
                PNSingleton.Instance.FontUser.FontWeight = f.FontWeight;
                PNData.SaveFontUi();
                cmdRestoreFontUI.IsEnabled = false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CheckGeneral_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as CheckBox;
                if (cb == null || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkRunOnStart":
                        _TempSettings.GeneralSettings.RunOnStart = cb.IsChecked.Value;
                        break;
                    case "chkShowCPOnStart":
                        _TempSettings.GeneralSettings.ShowCPOnStart = cb.IsChecked.Value;
                        break;
                    case "chkCheckNewVersionOnStart":
                        _TempSettings.GeneralSettings.CheckNewVersionOnStart = cb.IsChecked.Value;
                        break;
                    case "chkHideToolbar":
                        _TempSettings.GeneralSettings.HideToolbar = cb.IsChecked.Value;
                        break;
                    case "chkCustomFont":
                        _TempSettings.GeneralSettings.UseCustomFonts = cb.IsChecked.Value;
                        break;
                    case "chkHideDelete":
                        _TempSettings.GeneralSettings.HideDeleteButton = cb.IsChecked.Value;
                        if (chkChangeHideToDelete.IsChecked != null && (!cb.IsChecked.Value && chkChangeHideToDelete.IsChecked.Value))
                        {
                            chkChangeHideToDelete.IsChecked = false;
                        }
                        break;
                    case "chkHideHide":
                        _TempSettings.GeneralSettings.HideHideButton = cb.IsChecked.Value;
                        break;
                    case "chkChangeHideToDelete":
                        _TempSettings.GeneralSettings.ChangeHideToDelete = cb.IsChecked.Value;
                        break;
                    case "chkAutosave":
                        _TempSettings.GeneralSettings.Autosave = cb.IsChecked.Value;
                        break;
                    case "chkSaveOnExit":
                        _TempSettings.GeneralSettings.SaveOnExit = cb.IsChecked.Value;
                        break;
                    case "chkConfirmSave":
                        _TempSettings.GeneralSettings.ConfirmSaving = cb.IsChecked.Value;
                        break;
                    case "chkConfirmDelete":
                        _TempSettings.GeneralSettings.ConfirmBeforeDeletion = cb.IsChecked.Value;
                        break;
                    case "chkSaveWithoutConfirm":
                        _TempSettings.GeneralSettings.SaveWithoutConfirmOnHide = cb.IsChecked.Value;
                        break;
                    case "chkWarnBeforeEmptyBin":
                        _TempSettings.GeneralSettings.WarnOnAutomaticalDelete = cb.IsChecked.Value;
                        break;
                    case "chkShowPriority":
                        _TempSettings.GeneralSettings.ShowPriorityOnStart = cb.IsChecked.Value;
                        break;
                    case "chkAutomaticSmilies":
                        _TempSettings.GeneralSettings.AutomaticSmilies = cb.IsChecked.Value;
                        break;
                    case "chkRestoreAutomatically":
                        _TempSettings.GeneralSettings.RestoreAuto = cb.IsChecked.Value;
                        break;
                    case "chkAutoHeight":
                        _TempSettings.GeneralSettings.AutoHeight = cb.IsChecked.Value;
                        break;
                    case "chkCheckCriticalOnStart":
                        _TempSettings.GeneralSettings.CheckCriticalOnStart = cb.IsChecked.Value;
                        break;
                    case "chkCheckCriticalPeriodically":
                        _TempSettings.GeneralSettings.CheckCriticalPeriodically = cb.IsChecked.Value;
                        break;
                    case "chkDeleteShortExit":
                        _TempSettings.GeneralSettings.DeleteShortcutsOnExit = cb.IsChecked.Value;
                        break;
                    case "chkRestoreShortStart":
                        _TempSettings.GeneralSettings.RestoreShortcutsOnStart = cb.IsChecked.Value;
                        break;
                    case "chkCloseOnShortcut":
                        _TempSettings.GeneralSettings.CloseOnShortcut = cb.IsChecked.Value;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdNewVersion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var updater = new PNUpdateChecker();
                updater.NewVersionFound += updater_PNNewVersionFound;
                updater.IsLatestVersion += updater_PNIsLatestVersion;
                updater.CheckNewVersion(System.Windows.Forms.Application.ProductVersion);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updater_PNIsLatestVersion(object sender, EventArgs e)
        {
            try
            {
                var updater = sender as PNUpdateChecker;
                if (updater != null)
                {
                    updater.IsLatestVersion -= updater_PNIsLatestVersion;
                }
                var message = PNLang.Instance.GetMessageText("latest_version",
                                                                "You are using the latest version of PNotes.NET.");
                PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updater_PNNewVersionFound(object sender, NewVersionFoundEventArgs e)
        {
            try
            {
                var updater = sender as PNUpdateChecker;
                if (updater != null)
                {
                    updater.NewVersionFound -= updater_PNNewVersionFound;
                }
                var message =
                    PNLang.Instance.GetMessageText("new_version_1",
                                                   "New version of PNotes.NET is available - %PLACEHOLDER1%.")
                          .Replace(PNStrings.PLACEHOLDER1, e.Version);
                message += "\n";
                message += PNLang.Instance.GetMessageText("new_version_2", "Click 'OK' in order to instal new version (restart of program is required).");
                if (
                    PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OKCancel, MessageBoxImage.Information) !=
                    MessageBoxResult.OK) return;
                if (PNStatic.PrepareNewVersionCommandLine())
                {
                    PNStatic.FormMain.Close();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtWidthSknlsDef_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.GeneralSettings.Width = (int)txtWidthSknlsDef.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtHeightSknlsDef_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.GeneralSettings.Height = (int)txtHeightSknlsDef.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboButtonsSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded || cboButtonsSize.SelectedIndex <= -1) return;
                var bs = cboButtonsSize.Items[cboButtonsSize.SelectedIndex] as _ButtonSize;
                if (bs != null)
                {
                    _TempSettings.GeneralSettings.ButtonsSize = bs.ButtonSize;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboIndent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboIndent.SelectedIndex > -1 &&
                    (int)cboIndent.SelectedItem != _TempSettings.GeneralSettings.BulletsIndent)
                {
                    _TempSettings.GeneralSettings.BulletsIndent = Convert.ToInt16(cboIndent.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboMargins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboMargins.SelectedIndex > -1 &&
                    (int)cboMargins.SelectedItem != _TempSettings.GeneralSettings.MarginWidth)
                {
                    _TempSettings.GeneralSettings.MarginWidth = Convert.ToInt16(cboMargins.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboParIndent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboParIndent.SelectedIndex > -1 &&
                    (int)cboParIndent.SelectedItem != _TempSettings.GeneralSettings.ParagraphIndent)
                {
                    _TempSettings.GeneralSettings.ParagraphIndent = (int)cboParIndent.SelectedItem;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void pckSpell_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            try
            {
                if (_Loaded)
                {
                    _TempSettings.GeneralSettings.SpellColor = System.Drawing.Color.FromArgb(pckSpell.SelectedColor.A,
                        pckSpell.SelectedColor.R, pckSpell.SelectedColor.G, pckSpell.SelectedColor.B);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updSpace_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (_Loaded)
                {
                    _TempSettings.GeneralSettings.SpacePoints = Convert.ToInt32(updSpace.Value);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updAutosave_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (_Loaded)
                {
                    _TempSettings.GeneralSettings.AutosavePeriod = Convert.ToInt32(updAutosave.Value);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboDeleteBin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                if (cboDeleteBin.SelectedIndex > -1)
                {
                    _TempSettings.GeneralSettings.RemoveFromBinPeriod = cboDeleteBin.SelectedIndex == 0
                        ? 0
                        : Convert.ToInt32(cboDeleteBin.SelectedItem);
                }
                chkWarnBeforeEmptyBin.IsEnabled = cboDeleteBin.SelectedIndex > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtDTFShort_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txtDTFShort.Text.Trim().Length == 0) return;
                var format = txtDTFShort.Text.Trim().Length > 1
                    ? txtDTFShort.Text.Trim().Replace("/", "'/'")
                    : "%" + txtDTFShort.Text.Trim().Replace("/", "'/'");
                _TempSettings.GeneralSettings.DateFormat = format;
                lblDTShort.Text = DateTime.Now.ToString(_TempSettings.GeneralSettings.DateFormat);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtDTFShort_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.D:
                    case Key.Y:
                    case Key.Space:
                    case Key.Subtract:
                    case Key.Divide:
                    case Key.Decimal:
                    case Key.Delete:
                    case Key.Back:
                    case Key.Left:
                    case Key.Right:
                        break;
                    case Key.Oem2:
                    case Key.OemPeriod:
                    case Key.OemMinus:
                        if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                            e.Handled = true;
                        break;
                    case Key.M:
                        if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
                            e.Handled = true;
                        break;
                    default:
                        e.Handled = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDTFShort_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PNMessageBox.Show(PNLang.Instance.GetDateFormatsText(),
                    PNLang.Instance.GetCaptionText("date_formats", "Possible date formats"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtTFLong_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txtTFLong.Text.Trim().Length == 0) return;
                var format = txtTFLong.Text.Trim().Length > 1 ? txtTFLong.Text.Trim() : "%" + txtTFLong.Text.Trim();
                _TempSettings.GeneralSettings.TimeFormat = format;
                lblTFLong.Text = DateTime.Now.ToString(_TempSettings.GeneralSettings.TimeFormat);
            }
            catch (FormatException)
            {
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtTFLong_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.M:
                    case Key.S:
                    case Key.F:
                    case Key.T:
                    case Key.Space:
                    case Key.Subtract:
                    case Key.Divide:
                    case Key.Decimal:
                    case Key.Delete:
                    case Key.Back:
                    case Key.Left:
                    case Key.Right:
                    case Key.H:
                        break;
                    case Key.OemPeriod:
                    case Key.OemMinus:
                        if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                            e.Handled = true;
                        break;
                    case Key.Oem1:
                        if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
                            e.Handled = true;
                        break;
                    default:
                        e.Handled = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdTFLong_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PNMessageBox.Show(PNLang.Instance.GetTimeFormatsText(),
                            PNLang.Instance.GetCaptionText("time_formats", "Possible time formats"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdSearchProvs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cmdEditProv.IsEnabled = cmdDeleteProv.IsEnabled = grdSearchProvs.SelectedItems.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdSearchProvs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = grdSearchProvs.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdSearchProvs)) as SProvider;
                if (item == null) return;
                editSearchProvider(_SProviders.FirstOrDefault(p => p.Name == item.Name));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdExternals_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cmdEditExt.IsEnabled = cmdDeleteExt.IsEnabled = grdExternals.SelectedItems.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdExternals_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = grdExternals.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdExternals)) as SExternal;
                if (item == null) return;
                editExternal(_Externals.FirstOrDefault(ext => ext.Name == item.Name));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtTag_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                cmdAddTag.IsEnabled = txtTag.Text.Trim().Length > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cmdDeleteTag.IsEnabled = lstTags.SelectedIndex >= 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboScrollBars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.GeneralSettings.ShowScrollbar = (System.Windows.Forms.RichTextBoxScrollBars)cboScrollBars.SelectedIndex;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddProv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dsp = new WndSP(this) { Owner = this };
                var showDialog = dsp.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillSearchProviders(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdEditProv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                editSearchProvider(getSelectedSearchProvider());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDeleteProv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sp = getSelectedSearchProvider();
                if (sp == null) return;
                var message = PNLang.Instance.GetMessageText("sp_delete", "Delete selected serach provider?");
                if (PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                _SProviders.Remove(sp);
                fillSearchProviders(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddExt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dex = new WndExternals(this) { Owner = this };
                var showDialog = dex.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillExternals();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdEditExt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                editExternal(getSelectedExternal());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDeleteExt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ext = getSelectedExternal();
                if (ext == null) return;
                var message = PNLang.Instance.GetMessageText("ext_delete", "Delete selected external program?");
                if (PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                _Externals.Remove(ext);
                fillExternals(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Tags.Contains(txtTag.Text.Trim()))
                {
                    string message = PNLang.Instance.GetMessageText("tag_exists", "The same tag already exists");
                    PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                    txtTag.SelectAll();
                    txtTag.Focus();
                }
                else
                {
                    lstTags.Items.Add(txtTag.Text.Trim());
                    _Tags.Add(txtTag.Text.Trim());
                    txtTag.Text = "";
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDeleteTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstTags.SelectedIndex < 0) return;
                var message = PNLang.Instance.GetMessageText("tag_delete", "Delete selected tag?");
                if (PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                _Tags.Remove((string)lstTags.SelectedItem);
                lstTags.Items.Remove(lstTags.SelectedItem);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Schedule staff
        private void CheckSchedule_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as CheckBox;
                if (cb == null || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkAllowSound":
                        _TempSettings.Schedule.AllowSoundAlert = cb.IsChecked.Value;
                        break;
                    case "chkVisualNotify":
                        _TempSettings.Schedule.VisualNotification = cb.IsChecked.Value;
                        break;
                    case "chkCenterScreen":
                        _TempSettings.Schedule.CenterScreen = cb.IsChecked.Value;
                        break;
                    case "chkTrackOverdue":
                        _TempSettings.Schedule.TrackOverdue = cb.IsChecked.Value;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void clbSounds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cmdRemoveSound.IsEnabled = cmdListenSound.IsEnabled = false;
                if (clbSounds.SelectedIndex < 0) return;
                cmdListenSound.IsEnabled = true;
                var item = clbSounds.SelectedItem as PNListBoxItem;
                if (item != null)
                    _TempSettings.Schedule.Sound = item.Text;
                if (clbSounds.SelectedIndex > 0)
                    cmdRemoveSound.IsEnabled = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstVoices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cmdVoiceSample.IsEnabled = txtVoiceSample.Text.Trim().Length > 0 && lstVoices.SelectedIndex >= 0;
                if (lstVoices.SelectedIndex < 0) return;
                var voice = lstVoices.Items[lstVoices.SelectedIndex] as PNListBoxItem;
                if (voice == null) return;
                cmdDefVoice.IsEnabled = voice.Text != _TempSettings.Schedule.Voice;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDefVoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstVoices.SelectedIndex < 0) return;
                var voice = lstVoices.Items[lstVoices.SelectedIndex] as PNListBoxItem;
                if (voice == null) return;
                if (_TempSettings.Schedule.Voice == voice.Text) return;
                _TempSettings.Schedule.Voice = voice.Text;
                cmdDefVoice.IsEnabled = false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdVoiceSample_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var voice = lstVoices.Items[lstVoices.SelectedIndex] as PNListBoxItem;
                if (voice == null) return;
                using (var sp = new SpeechSynthesizer())
                {
                    sp.SelectVoice(voice.Text);
                    sp.Volume = _TempSettings.Schedule.VoiceVolume;
                    sp.Rate = _TempSettings.Schedule.VoiceSpeed;
                    sp.Speak(txtVoiceSample.Text.Trim());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtVoiceSample_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                cmdVoiceSample.IsEnabled = txtVoiceSample.Text.Trim().Length > 0 && lstVoices.SelectedIndex >= 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void trkVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                var slider = sender as Slider;
                if (slider == null) return;
                slider.Value = Math.Round(e.NewValue, 0);
                _TempSettings.Schedule.VoiceVolume = (int)trkVolume.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void trkSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                var slider = sender as Slider;
                if (slider == null) return;
                slider.Value = Math.Round(e.NewValue, 0);
                _TempSettings.Schedule.VoiceSpeed = (int)trkSpeed.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddSound_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addString = true;
                var ofd = new OpenFileDialog
                {
                    Filter = @"Windows audio files|*.wav",
                    Title = PNLang.Instance.GetCaptionText("choose_sound", "Choose sound")
                };
                if (!ofd.ShowDialog(this).Value) return;
                if (!Directory.Exists(PNPaths.Instance.SoundsDir))
                    Directory.CreateDirectory(PNPaths.Instance.SoundsDir);
                if (File.Exists(PNPaths.Instance.SoundsDir + @"\" + Path.GetFileName(ofd.FileName)))
                {
                    if (
                        PNMessageBox.Show(
                            PNLang.Instance.GetMessageText("sound_exists",
                                "The file already exists in your 'sounds' directory. Copy anyway?"),
                            PNLang.Instance.GetCaptionText("confirm", "Confirmation"), MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.No)
                        return;
                    addString = false;
                }
                var path2 = Path.GetFileName(ofd.FileName);
                if (path2 == null) return;
                File.Copy(ofd.FileName, Path.Combine(PNPaths.Instance.SoundsDir, path2), true);
                if (!addString) return;
                var image = TryFindResource("loudspeaker") as BitmapImage;//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "loudspeaker.png"));
                clbSounds.Items.Add(new PNListBoxItem(image, Path.GetFileNameWithoutExtension(ofd.FileName),
                    ofd.FileName));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdRemoveSound_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNMessageBox.Show(PNLang.Instance.GetMessageText("sound_delete", "Delete selected sound?"),
                    PNLang.Instance.GetCaptionText("confirm", "Confirmation"), MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
                var index = clbSounds.SelectedIndex;
                var item = clbSounds.SelectedItem as PNListBoxItem;
                if (item == null) return;
                var sound = item.Text;
                clbSounds.SelectedIndex = clbSounds.SelectedIndex - 1;
                clbSounds.Items.RemoveAt(index);
                File.Delete(Path.Combine(PNPaths.Instance.SoundsDir, sound + ".wav"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdListenSound_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_TempSettings.Schedule.Sound == PNSchedule.DEF_SOUND)
                {
                    PNSound.PlayDefaultSound();
                }
                else
                {
                    PNSound.PlaySound(Path.Combine(PNPaths.Instance.SoundsDir, _TempSettings.Schedule.Sound + ".wav"));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Appearance staff
        private void OptionAppearance_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (optSkinnable.IsChecked != null)
                    _TempSettings.GeneralSettings.UseSkins = optSkinnable.IsChecked.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwGroups_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var item = e.NewValue as PNTreeItem;
                if (item == null)
                    return;
                var gr = item.Tag as PNGroup;
                if (gr == null)
                    return;
                switch (gr.ID)
                {
                    case (int)SpecialGroups.Diary:
                        pnsDiaryCust.IsEnabled = true;
                        pnsMiscDocking.IsEnabled = false;
                        //stkDiaryCust.IsEnabled = true;
                        //stkDockCust.IsEnabled = false;
                        break;
                    case (int)SpecialGroups.Docking:
                        pnsDiaryCust.IsEnabled = false;
                        pnsMiscDocking.IsEnabled = true;
                        //stkDiaryCust.IsEnabled = false;
                        //stkDockCust.IsEnabled = true;
                        break;
                    default:
                        pnsDiaryCust.IsEnabled = false;
                        pnsMiscDocking.IsEnabled = false;
                        //stkDiaryCust.IsEnabled = false;
                        //stkDockCust.IsEnabled = false;
                        break;
                }
                PNStatic.DrawSkinlessPreview(gr, brdFrame, blkCaption, brdBody, blkBody);
                pckBGSknls.SelectedColor = gr.Skinless.BackColor;
                if (gr.Skin.SkinName != PNSkinDetails.NO_SKIN)
                {
                    lstSkins.SelectedItem =
                        lstSkins.Items.OfType<PNListBoxItem>().FirstOrDefault(it => it.Text == gr.Skin.SkinName);
                }
                else
                {
                    lstSkins.SelectedIndex = -1;
                }
                cboFonts.SelectedItem =
                    cboFonts.Items.OfType<LOGFONT>().FirstOrDefault(lf => lf.lfFaceName == gr.Font.lfFaceName);
                foreach (var t in from object t in cboFontColor.Items
                                  let rc = t as Rectangle
                                  where rc != null
                                  let sb = rc.Fill as SolidColorBrush
                                  where sb != null
                                  where
                                      sb.Color ==
                                      Color.FromArgb(gr.FontColor.A, gr.FontColor.R, gr.FontColor.G,
                                          gr.FontColor.B)
                                  select t)
                {
                    cboFontColor.SelectedItem = t;
                    break;
                }
                cboFontSize.SelectedItem = gr.Font.GetFontSize();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void pckBGSknls_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            try
            {
                var gr = selectedGroup();
                if (gr == null) return;
                gr.Skinless.BackColor = e.NewValue;
                PNStatic.DrawSkinlessPreview(gr, brdFrame, blkCaption, brdBody, blkBody);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdFontSknls_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var gr = selectedGroup();
                if (gr == null) return;
                var fc = new WndFontChooser(gr.Skinless.CaptionFont, gr.Skinless.CaptionColor) { Owner = this };
                var showDialog = fc.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    gr.Skinless.CaptionFont = fc.SelectedFont;
                    gr.Skinless.CaptionColor = fc.SelectedColor;
                    PNStatic.DrawSkinlessPreview(gr, brdFrame, blkCaption, brdBody, blkBody);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstSkins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lstSkins.SelectedIndex < 0)
                {
                    imgSkin.Source = null;
                    return;
                }
                var item = lstSkins.SelectedItem as PNListBoxItem;
                if (item == null) return;
                var gr = selectedGroup();
                if (gr == null) return;
                if (gr.Skin.SkinName != item.Text)
                {
                    gr.Skin.SkinName = item.Text;
                    var path = Path.Combine(PNPaths.Instance.SkinsDir, gr.Skin.SkinName + PNStrings.SKIN_EXTENSION);
                    if (File.Exists(path))
                    {
                        PNSkinsOperations.LoadSkin(path, gr.Skin);
                    }
                }
                if (gr.Skin.SkinName != PNSkinDetails.NO_SKIN)
                {
                    PNStatic.DrawSkinPreview(gr, gr.Skin, imgSkin);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lblMoreSkins_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                PNStatic.LoadPage(PNStrings.URL_SKINS);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CheckAppearance_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as CheckBox;
                if (cb == null || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkAddWeekdayName":
                        _TempSettings.Diary.AddWeekday = cb.IsChecked.Value;
                        break;
                    case "chkFullWeekdayName":
                        _TempSettings.Diary.FullWeekdayName = cb.IsChecked.Value;
                        break;
                    case "chkWeekdayAtTheEnd":
                        _TempSettings.Diary.WeekdayAtTheEnd = cb.IsChecked.Value;
                        break;
                    case "chkNoPreviousDiary":
                        _TempSettings.Diary.DoNotShowPrevious = cb.IsChecked.Value;
                        break;
                    case "chkDiaryAscOrder":
                        _TempSettings.Diary.AscendingOrder = cb.IsChecked.Value;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded || cboFonts.SelectedIndex < 0) return;
                var lf = (LOGFONT)e.AddedItems[0];
                var gr = selectedGroup();
                if (gr == null) return;
                var logF = new LOGFONT();
                logF.Init();
                logF.SetFontFace(lf.lfFaceName);
                logF.SetFontSize((int)cboFontSize.SelectedItem);
                gr.Font = logF;
                PNStatic.DrawSkinlessPreview(gr, brdFrame, blkCaption, brdBody, blkBody);
                if (lstSkins.SelectedIndex >= 0)
                {
                    PNStatic.DrawSkinPreview(gr, gr.Skin, imgSkin);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded || cboFontColor.SelectedIndex < 0) return;
                var gr = selectedGroup();
                if (gr == null) return;
                var rc = cboFontColor.SelectedItem as Rectangle;
                if (rc == null) return;
                var sb = rc.Fill as SolidColorBrush;
                if (sb == null) return;
                gr.FontColor = System.Drawing.Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B);
                PNStatic.DrawSkinlessPreview(gr, brdFrame, blkCaption, brdBody, blkBody);
                if (lstSkins.SelectedIndex >= 0)
                {
                    PNStatic.DrawSkinPreview(gr, gr.Skin, imgSkin);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }


        private void cboFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded || cboFontSize.SelectedIndex < 0) return;
                var gr = selectedGroup();
                if (gr == null) return;
                var logF = new LOGFONT();
                logF.Init();
                logF.SetFontFace(gr.Font.lfFaceName);
                logF.SetFontSize((int)cboFontSize.SelectedItem);
                gr.Font = logF;
                PNStatic.DrawSkinlessPreview(gr, brdFrame, blkCaption, brdBody, blkBody);
                if (lstSkins.SelectedIndex >= 0)
                {
                    PNStatic.DrawSkinPreview(gr, gr.Skin, imgSkin);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboNumberOfDiaries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboNumberOfDiaries.SelectedIndex > -1)
                {
                    _TempSettings.Diary.NumberOfPages = (int)cboNumberOfDiaries.SelectedItem;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboDiaryNaming_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboDiaryNaming.SelectedIndex > -1)
                {
                    _TempSettings.Diary.DateFormat = (string)cboDiaryNaming.SelectedItem;
                    lblDiaryExample.Text = DateTime.Today.ToString(_TempSettings.Diary.DateFormat.Replace("/", "'/'"));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtDockWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.GeneralSettings.DockWidth = Convert.ToInt32(e.NewValue);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtDockHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.GeneralSettings.DockHeight = Convert.ToInt32(e.NewValue);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Behavior staff
        private void cmdHotkeys_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dhk = new WndHotkeys { Owner = this };
                dhk.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdMenus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dm = new WndMenusManager { Owner = this };
                dm.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CheckBehavior_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as CheckBox;
                if (cb == null || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkNewOnTop":
                        _TempSettings.Behavior.NewNoteAlwaysOnTop = cb.IsChecked.Value;
                        break;
                    case "chkRelationalPosition":
                        _TempSettings.Behavior.RelationalPositioning = cb.IsChecked.Value;
                        break;
                    case "chkHideCompleted":
                        _TempSettings.Behavior.HideCompleted = cb.IsChecked.Value;
                        break;
                    case "chkShowBigIcons":
                        _TempSettings.Behavior.BigIconsOnCP = cb.IsChecked.Value;
                        break;
                    case "chkDontShowList":
                        _TempSettings.Behavior.DoNotShowNotesInList = cb.IsChecked.Value;
                        break;
                    case "chkKeepVisibleOnShowdesktop":
                        _TempSettings.Behavior.KeepVisibleOnShowDesktop = cb.IsChecked.Value;
                        break;
                    case "chkHideFluently":
                        _TempSettings.Behavior.HideFluently = cb.IsChecked.Value;
                        break;
                    case "chkPlaySoundOnHide":
                        _TempSettings.Behavior.PlaySoundOnHide = cb.IsChecked.Value;
                        break;
                    case "chkRandBack":
                        _TempSettings.Behavior.RandomBackColor = cb.IsChecked.Value;
                        break;
                    case "chkInvertText":
                        _TempSettings.Behavior.InvertTextColor = cb.IsChecked.Value;
                        break;
                    case "chkRoll":
                        _TempSettings.Behavior.RollOnDblClick = cb.IsChecked.Value;
                        break;
                    case "chkFitRolled":
                        _TempSettings.Behavior.FitWhenRolled = cb.IsChecked.Value;
                        break;
                    case "chkShowSeparateNotes":
                        _TempSettings.Behavior.ShowSeparateNotes = cb.IsChecked.Value;
                        break;
                    case "chkHideMainWindow":
                        _TempSettings.Behavior.HideMainWindow = cb.IsChecked.Value;
                        break;
                    case "chkPreventResizing":
                        _TempSettings.Behavior.PreventAutomaticResizing = cb.IsChecked.Value;
                        return;
                    case "chkShowPanel":
                        _TempSettings.Behavior.ShowNotesPanel = cb.IsChecked.Value;
                        break;
                    case "chkPanelAutoHide":
                        _TempSettings.Behavior.PanelAutoHide = cb.IsChecked.Value;
                        break;
                    case "chkPanelSwitchOffAnimation":
                        _TempSettings.Behavior.PanelSwitchOffAnimation = cb.IsChecked.Value;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void trkTrans_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                var slider = sender as Slider;
                if (slider == null) return;
                slider.Value = Math.Round(e.NewValue, 0);
                lblTransPerc.Text = trkTrans.Value.ToString(PNStatic.CultureInvariant) + @"%";
                _TempSettings.Behavior.Opacity = (100.0 - trkTrans.Value) / 100.0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboDblAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboDblAction.SelectedIndex < 0) return;
                if (cboDblAction.SelectedIndex == cboSingleAction.SelectedIndex && !_InDefSettingsClick)
                {
                    var message = PNLang.Instance.GetMessageText("same_actions",
                        "You can not choose the same action for double click and single click");
                    PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    cboDblAction.SelectedItem = e.RemovedItems[0];
                    return;
                }
                _TempSettings.Behavior.DoubleClickAction = (TrayMouseAction)cboDblAction.SelectedIndex;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboSingleAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboSingleAction.SelectedIndex < 0) return;
                if (cboDblAction.SelectedIndex == cboSingleAction.SelectedIndex && !_InDefSettingsClick)
                {
                    var message = PNLang.Instance.GetMessageText("same_actions",
                        "You can not choose the same action for double click and single click");
                    PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    cboSingleAction.SelectedItem = e.RemovedItems[0];
                    return;
                }
                _TempSettings.Behavior.SingleClickAction = (TrayMouseAction)cboSingleAction.SelectedIndex;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboDefName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboDefName.SelectedIndex > -1)
                {
                    _TempSettings.Behavior.DefaultNaming = (DefaultNaming)cboDefName.SelectedIndex;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboLengthOfName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboLengthOfName.SelectedIndex > -1)
                {
                    _TempSettings.Behavior.DefaultNameLength = Convert.ToInt32(cboLengthOfName.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboLengthOfContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboLengthOfContent.SelectedIndex > -1)
                {
                    _TempSettings.Behavior.ContentColumnLength = Convert.ToInt32(cboLengthOfContent.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboPinClick_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboPinClick.SelectedIndex != -1)
                {
                    _TempSettings.Behavior.PinClickAction = (PinClickAction)cboPinClick.SelectedIndex;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboNoteStartPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboNoteStartPosition.SelectedIndex != -1)
                {
                    _TempSettings.Behavior.StartPosition = (NoteStartPosition)cboNoteStartPosition.SelectedIndex;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboPanelDock_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.Behavior.NotesPanelOrientation = (NotesPanelOrientation)cboPanelDock.SelectedIndex;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboPanelRemove_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.Behavior.PanelRemoveMode = (PanelRemoveMode)cboPanelRemove.SelectedIndex;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboPanelDelay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.Behavior.PanelEnterDelay = Convert.ToInt32((double)cboPanelDelay.SelectedItem * 1000);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                checkThemesUpdate();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkThemesUpdate()
        {
            try
            {
                if (PNSingleton.Instance.PluginsDownload || PNSingleton.Instance.PluginsChecking ||
                    PNSingleton.Instance.VersionChecking || PNSingleton.Instance.CriticalChecking ||
                    PNSingleton.Instance.ThemesDownload || PNSingleton.Instance.ThemesChecking) return;
                var updater = new PNUpdateChecker();
                updater.ThemesUpdateFound += updater_ThemesUpdateFound;
                updater.IsLatestVersion += updaterThemes_IsLatestVersion;
                Mouse.OverrideCursor = Cursors.Wait;
                updater.CheckThemesNewVersion();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void updaterThemes_IsLatestVersion(object sender, EventArgs e)
        {
            try
            {
                var updater = sender as PNUpdateChecker;
                if (updater != null)
                {
                    updater.IsLatestVersion -= updater_IsLatestVersion;
                }
                Mouse.OverrideCursor = null;
                var message = PNLang.Instance.GetMessageText("themes_latest_version",
                                                                "All themes are up-to-date.");
                PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updater_ThemesUpdateFound(object sender, ThemesUpdateFoundEventArgs e)
        {
            try
            {
                var updater = sender as PNUpdateChecker;
                if (updater != null) updater.ThemesUpdateFound -= updater_ThemesUpdateFound;
                Mouse.OverrideCursor = null;
                var d = new WndGetThemes(e.ThemesList) { Owner = this };
                var showDialog = d.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    promptToRestart();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var key = lstThemes.SelectedItem as string;
                if (key == null) return;
                var de = PNStatic.Themes[key];
                if (de == null) return;
                imgTheme.Source = de.Item3;
                if (!_Loaded) return;
                if (lstThemes.SelectedIndex >= 0)
                {
                    _TempSettings.Behavior.Theme = (string)lstThemes.SelectedItem;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Network staff
        private void CheckNetwork_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as CheckBox;
                if (cb == null || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkIncludeBinInSync":
                        _TempSettings.Network.IncludeBinInSync = cb.IsChecked.Value;
                        break;
                    case "chkSyncOnStart":
                        _TempSettings.Network.SyncOnStart = cb.IsChecked.Value;
                        break;
                    case "chkSaveBeforeSync":
                        _TempSettings.Network.SaveBeforeSync = cb.IsChecked.Value;
                        break;
                    case "chkEnableExchange":
                        _TempSettings.Network.EnableExchange = cb.IsChecked.Value;
                        if (_TempSettings.Network.EnableExchange)
                            _TimerConnections.Start();
                        else
                            _TimerConnections.Stop();
                        break;
                    case "chkSaveBeforeSending":
                        _TempSettings.Network.SaveBeforeSending = cb.IsChecked.Value;
                        break;
                    case "chkNoNotifyOnArrive":
                        _TempSettings.Network.NoNotificationOnArrive = cb.IsChecked.Value;
                        break;
                    case "chkShowRecOnClick":
                        _TempSettings.Network.ShowReceivedOnClick = cb.IsChecked.Value;
                        break;
                    case "chkShowIncomingOnClick":
                        _TempSettings.Network.ShowIncomingOnClick = cb.IsChecked.Value;
                        break;
                    case "chkNoSoundOnArrive":
                        _TempSettings.Network.NoSoundOnArrive = cb.IsChecked.Value;
                        break;
                    case "chkNoNotifyOnSend":
                        _TempSettings.Network.NoNotificationOnSend = cb.IsChecked.Value;
                        break;
                    case "chkShowAfterReceiving":
                        _TempSettings.Network.ShowAfterArrive = cb.IsChecked.Value;
                        break;
                    case "chkHideAfterSending":
                        _TempSettings.Network.HideAfterSending = cb.IsChecked.Value;
                        break;
                    case "chkNoContInContextMenu":
                        _TempSettings.Network.NoContactsInContextMenu = cb.IsChecked.Value;
                        break;
                    case "chkAllowPing":
                        _TempSettings.Network.AllowPing = cb.IsChecked.Value;
                        break;
                    case "chkRecOnTop":
                        _TempSettings.Network.ReceivedOnTop = cb.IsChecked.Value;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwContactsGroups_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                cmdEditContactGroup.IsEnabled = cmdDeleteContactGroup.IsEnabled = false;
                var item = e.NewValue as PNTreeItem;
                if (item == null || item.Tag == null || Convert.ToInt32(item.Tag) == -1) return;
                cmdEditContactGroup.IsEnabled = cmdDeleteContactGroup.IsEnabled = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwContactsGroups_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = tvwContactsGroups.GetObjectAtPoint<TreeViewItem>(e.GetPosition(tvwContactsGroups)) as PNTreeItem;
                if (item == null) return;
                editContactsGroup();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgContactGroup_ContactGroupChanged(object sender, ContactGroupChangedEventArgs e)
        {
            try
            {
                var dg = sender as WndGroups;
                if (dg != null)
                {
                    dg.ContactGroupChanged -= dlgContactGroup_ContactGroupChanged;
                }
                if (e.Mode == AddEditMode.Add)
                {
                    if (_Groups.Any(g => g.Name == e.Group.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("group_exists",
                            "Contacts group with this name already exists");
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                        e.Accepted = false;
                        return;
                    }
                    _Groups.Add(e.Group);
                }
                else
                {
                    var g = _Groups.FirstOrDefault(gr => gr.ID == e.Group.ID);
                    if (g != null)
                    {
                        g.Name = e.Group.Name;
                    }
                }
                fillGroups(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cmdEditContact.IsEnabled = cmdDeleteContact.IsEnabled = grdContacts.SelectedItems.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdContacts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = grdContacts.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdContacts)) as SContact;
                if (item == null) return;
                editContact();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgContact_ContactChanged(object sender, ContactChangedEventArgs e)
        {
            try
            {
                var dc = sender as WndContacts;
                if (dc != null)
                {
                    dc.ContactChanged -= dlgContact_ContactChanged;
                }
                if (e.Mode == AddEditMode.Add)
                {
                    if (_Contacts.Any(c => c.Name == e.Contact.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("contact_exists",
                            "Contact with this name already exists");
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                        e.Accepted = false;
                        return;
                    }
                    _Contacts.Add(e.Contact);
                }
                else
                {
                    var c = _Contacts.FirstOrDefault(con => con.ID == e.Contact.ID);
                    if (c == null) return;
                    c.Name = e.Contact.Name;
                    c.ComputerName = e.Contact.ComputerName;
                    c.IpAddress = e.Contact.IpAddress;
                    c.UseComputerName = e.Contact.UseComputerName;
                    c.GroupID = e.Contact.GroupID;
                }
                fillContacts(false);
                fillGroups(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtExchPort_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.Network.ExchangePort = Convert.ToInt32(txtExchPort.Value);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstSocial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var width = pnsSocPluginDetails.ActualWidth;
                lblSocPAuthor.Text = lblSocPVersion.Text = lblSocPInfo.Text = "";
                cmdRemovePostPlugin.IsEnabled = lstSocial.SelectedIndex > -1;
                if (lstSocial.SelectedIndex < 0) return;
                var item = lstSocial.SelectedItem as PNListBoxItem;
                if (item == null) return;
                var plugin = item.Tag as IPostPlugin;
                if (plugin == null) return;
                lblSocPAuthor.Text = plugin.Author;
                lblSocPVersion.Text = plugin.Version;
                lblSocPInfo.Text = plugin.AdditionalInfo;
                pnsSocPluginDetails.Width = width;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdCheckSocPlugUpdate_Click(object sender, RoutedEventArgs e)
        {
            checkForNewPluginsVersion();
        }

        private void cmdRemovePostPlugin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                removePlugin(PluginType.Social, lstSocial.SelectedIndex);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboPostCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_Loaded && cboPostCount.SelectedIndex > -1)
                {
                    _TempSettings.Network.PostCount = Convert.ToInt32(cboPostCount.Items[cboPostCount.SelectedIndex]);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstSyncPlugins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var width = pnsSyncPluginDetails.ActualWidth;
                lblSyncPAuthor.Text = lblSyncPVersion.Text = lblSyncPInfo.Text = "";
                cmdRemoveSyncPlugin.IsEnabled = lstSyncPlugins.SelectedIndex > -1;
                if (lstSyncPlugins.SelectedIndex < 0) return;
                var item = lstSyncPlugins.SelectedItem as PNListBoxItem;
                if (item == null) return;
                var plugin = item.Tag as ISyncPlugin;
                if (plugin != null)
                {
                    lblSyncPAuthor.Text = plugin.Author;
                    lblSyncPVersion.Text = plugin.Version;
                    lblSyncPInfo.Text = plugin.AdditionalInfo;
                }
                cmdSyncNow.IsEnabled =
                    lstSyncPlugins.Items.OfType<PNListBoxItem>()
                        .Any(it => it.IsChecked != null && it.IsChecked.Value);
                pnsSyncPluginDetails.Width = width;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdCheckSyncPlugUpdate_Click(object sender, RoutedEventArgs e)
        {
            checkForNewPluginsVersion();
        }

        private void cmdRemoveSyncPlugin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                removePlugin(PluginType.Sync, lstSyncPlugins.SelectedIndex);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdSyncNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstSyncPlugins.SelectedIndex <= -1) return;
                var item = lstSyncPlugins.Items[lstSyncPlugins.SelectedIndex] as PNListBoxItem;
                if (item == null) return;
                var plugin = PNPlugins.Instance.SyncPlugins.FirstOrDefault(p => p.Name == item.Text);
                if (plugin == null) return;
                switch (plugin.Synchronize())
                {
                    case SyncResult.None:
                        PNMessageBox.Show(PNLang.Instance.GetMessageText("sync_complete", "Syncronization completed successfully"), plugin.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case SyncResult.Reload:
                        PNMessageBox.Show(PNLang.Instance.GetMessageText("sync_complete_reload", "Syncronization completed successfully. The program has to be restarted for applying all changes."), plugin.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                        PNStatic.FormMain.ApplyAction(MainDialogAction.Restart, null);
                        break;
                    case SyncResult.AbortVersion:
                        PNMessageBox.Show(PNLang.Instance.GetMessageText("diff_versions", "Current version of database is different from previously synchronized version. Synchronization cannot be performed."), plugin.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;
                    case SyncResult.Error:
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdSmtp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cmdEditSmtp.IsEnabled = cmdDeleteSmtp.IsEnabled = grdSmtp.SelectedItems.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdSmtp_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                editSmtp();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdMailContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmdEditMailContact.IsEnabled = cmdDeleteMailContact.IsEnabled = grdMailContacts.SelectedItems.Count > 0;
        }

        private void grdMailContacts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            editMailContact();
        }

        private void smtpDlg_SmtpChanged(object sender, SmtpChangedEventArgs e)
        {
            try
            {
                if (e.Mode == AddEditMode.Add)
                {
                    if (_SmtpClients.Any(sm => sm.SenderAddress == e.Profile.SenderAddress))
                    {
                        PNMessageBox.Show(
                            PNLang.Instance.GetMessageText("smtp_same_address",
                                "There is already SMTP profile with the same address"), PNStrings.PROG_NAME,
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        e.Accepted = false;
                        return;
                    }
                    e.Profile.Id = _SmtpClients.Any() ? _SmtpClients.Max(c => c.Id) + 1 : 0;
                    _SmtpClients.Add(e.Profile);
                    fillSmtpClients(false);
                }
                else
                {
                    if (
                        _SmtpClients.Any(
                            sm => sm.SenderAddress == e.Profile.SenderAddress && sm.Id != e.Profile.Id))
                    {
                        PNMessageBox.Show(
                            PNLang.Instance.GetMessageText("smtp_same_address",
                                "There is already SMTP profile with the same address"), PNStrings.PROG_NAME,
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        e.Accepted = false;
                        return;
                    }
                    var client = _SmtpClients.FirstOrDefault(c => c.Id == e.Profile.Id);
                    if (client == null) return;
                    client.HostName = e.Profile.HostName;
                    client.SenderAddress = e.Profile.SenderAddress;
                    client.Port = e.Profile.Port;
                    client.Password = e.Profile.Password;
                    client.DisplayName = e.Profile.DisplayName;
                    fillSmtpClients(false);
                }
                var d = sender as WndSmtp;
                if (d != null) d.SmtpChanged -= smtpDlg_SmtpChanged;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgMailContact_MailContactChanged(object sender, MailContactChangedEventArgs e)
        {
            try
            {
                switch (e.Mode)
                {
                    case AddEditMode.Add:
                        if (
                            _MailContacts.Any(
                                mc => mc.DisplayName == e.Contact.DisplayName && mc.Address == e.Contact.Address))
                        {
                            PNMessageBox.Show(
                            PNLang.Instance.GetMessageText("mail_contact_same_address",
                                "There is already mail contact with the same name and address"), PNStrings.PROG_NAME,
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            e.Accepted = false;
                            return;
                        }
                        e.Contact.Id = _MailContacts.Any() ? _MailContacts.Max(mc => mc.Id) + 1 : 0;
                        _MailContacts.Add(e.Contact);
                        fillMailContacts(false);
                        break;
                    case AddEditMode.Edit:
                        if (
                            _MailContacts.Any(
                                mc =>
                                    mc.DisplayName == e.Contact.DisplayName && mc.Address == e.Contact.Address &&
                                    mc.Id != e.Contact.Id))
                        {
                            PNMessageBox.Show(
                                PNLang.Instance.GetMessageText("mail_contact_same_address",
                                    "There is already mail contact with the same name and address"), PNStrings.PROG_NAME,
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            e.Accepted = false;
                            return;
                        }
                        var contact = _MailContacts.FirstOrDefault(mc => mc.Id == e.Contact.Id);
                        if (contact == null) break;
                        contact.DisplayName = e.Contact.DisplayName;
                        contact.Address = e.Contact.Address;
                        fillMailContacts(false);
                        break;
                }
                var d = sender as WndMailContact;
                if (d == null) return;
                d.MailContactChanged -= dlgMailContact_MailContactChanged;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdSmtpCheck_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null) return;
            var item = grdSmtp.GetObjectAtPoint<ListViewItem>(Mouse.GetPosition(grdSmtp)) as SSmtpClient;
            if (item == null) return;
            grdSmtp.SelectedItem = item;

            foreach (var smtp in _SmtpClients)
            {
                if (checkBox.IsChecked != null)
                    smtp.Active = smtp.SenderAddress == item.Address && checkBox.IsChecked.Value;
                else
                    smtp.Active = false;
            }
        }

        private void mnuImpOutlook_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlgImport = new WndImportMailContacts(ImportContacts.Outlook, _MailContacts,
                    mnuImpOutlook.Header.ToString()) { Owner = this };
                dlgImport.ContactsImported += dlgImport_ContactsImported;
                var showDialog = dlgImport.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    dlgImport.ContactsImported -= dlgImport_ContactsImported;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuImpGmail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlgImport = new WndImportMailContacts(ImportContacts.Gmail, _MailContacts,
                        mnuImpGmail.Header.ToString()) { Owner = this };
                dlgImport.ContactsImported += dlgImport_ContactsImported;
                var showDialog = dlgImport.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    dlgImport.ContactsImported -= dlgImport_ContactsImported;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuImpLotus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlgImport = new WndImportMailContacts(ImportContacts.Lotus, _MailContacts,
                        mnuImpLotus.Header.ToString()) { Owner = this };
                dlgImport.ContactsImported += dlgImport_ContactsImported;
                var showDialog = dlgImport.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    dlgImport.ContactsImported -= dlgImport_ContactsImported;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgImport_ContactsImported(object sender, ContactsImportedEventArgs e)
        {
            try
            {
                var d = sender as WndImportMailContacts;
                if (d != null) d.ContactsImported -= dlgImport_ContactsImported;
                var id = _MailContacts.Any() ? _MailContacts.Max(c => c.Id) + 1 : 0;
                foreach (var tc in e.Contacts.Where(tc => !_MailContacts.Any(c => c.DisplayName == tc.Item1 && c.Address == tc.Item2)))
                {
                    _MailContacts.Add(new PNMailContact { Id = id, DisplayName = tc.Item1, Address = tc.Item2 });
                    _MailContactsList.Add(new SMailContact(tc.Item1, tc.Item2, id));
                    id++;
                }
                cmdClearMailContacts.IsEnabled = _MailContactsList.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void ctmImpContacts_Opened(object sender, RoutedEventArgs e)
        {
            //check for Office >= 2003
            mnuImpOutlook.IsEnabled = PNOffice.GetOfficeAppVersion(OfficeApp.Outlook) >= 11;
            //check for IBM Notes
            mnuImpLotus.IsEnabled = PNLotus.IsLotusInstalled();
        }

        private bool _WorkInProgress;
        private delegate void _TimerDelegate(object sender, ElapsedEventArgs e);
        private void _TimerConnections_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _TimerConnections.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    _TimerDelegate d = _TimerConnections_Elapsed;
                    try
                    {
                        Dispatcher.Invoke(d, sender, e);
                    }
                    catch (ObjectDisposedException)
                    {
                        // do nothing when main form is disposed
                    }
                    catch (Exception ex)
                    {
                        PNStatic.LogException(ex);
                    }
                }
                else
                {
                    if (_WorkInProgress) return;
                    _WorkInProgress = true;
                    var bgw = new BackgroundWorker();
                    bgw.DoWork += bgw_DoWork;
                    bgw.RunWorkerCompleted += bgw_RunWorkerCompleted;
                    bgw.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _TimerConnections.Start();
            }
        }

        private void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _WorkInProgress = false;
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                foreach (var sc in _ContactsList)
                    sc.ConnectionStatus = PNConnections.CheckContactConnection(sc.IpAddress);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddContactGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newID = 0;
                if (_Groups.Count > 0)
                {
                    newID = _Groups.Max(g => g.ID) + 1;
                }
                var dlgContactGroup = new WndGroups(newID) { Owner = this };
                dlgContactGroup.ContactGroupChanged += dlgContactGroup_ContactGroupChanged;
                var showDialog = dlgContactGroup.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                {
                    dlgContactGroup.ContactGroupChanged -= dlgContactGroup_ContactGroupChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdEditContactGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                editContactsGroup();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDeleteContactGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cg = getSelectedContactsGroup();
                if (cg == null) return;
                if (_Contacts.All(c => c.GroupID != cg.ID))
                {
                    var message = PNLang.Instance.GetMessageText("group_delete", "Delete selected group of contacts?");
                    if (
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                        MessageBoxResult.Yes) return;
                    _Groups.Remove(cg);
                    fillGroups(false);
                    return;
                }

                var dlg = new WndDeleteContactsGroup { Owner = this };
                var showDialog = dlg.ShowDialog();
                if (showDialog == null || !showDialog.Value) return;
                switch (dlg.DeleteBehavior)
                {
                    case DeleteContactsGroupBehavior.DeleteAll:
                        //delete all contacts
                        _Contacts.RemoveAll(c => c.GroupID == cg.ID);
                        break;
                    case DeleteContactsGroupBehavior.Move:
                        //move all contacts to '(none)'
                        foreach (var c in _Contacts.Where(c => c.GroupID == cg.ID))
                        {
                            c.GroupID = -1;
                        }
                        break;
                }
                _Groups.Remove(cg);
                fillGroups(false);
                fillContacts(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newID = 0;
                if (_Contacts.Count > 0)
                {
                    newID = _Contacts.Max(c => c.ID) + 1;
                }
                var dlgContact = new WndContacts(newID, _Groups) { Owner = this };
                dlgContact.ContactChanged += dlgContact_ContactChanged;
                var showDialog = dlgContact.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgContact.ContactChanged -= dlgContact_ContactChanged;
                    return;
                }
                fillContacts(false);
                fillGroups(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdEditContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                editContact();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDeleteContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cn = getSelectedContact();
                if (cn == null) return;
                var message = PNLang.Instance.GetMessageText("contact_delete", "Delete selected contact?");
                if (PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                _Contacts.Remove(cn);
                fillContacts(false);
                fillGroups(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddSmtp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var smtpDlg = new WndSmtp(null) { Owner = this };
                smtpDlg.SmtpChanged += smtpDlg_SmtpChanged;
                var showDialog = smtpDlg.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    smtpDlg.SmtpChanged -= smtpDlg_SmtpChanged;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdEditSmtp_Click(object sender, RoutedEventArgs e)
        {
            editSmtp();
        }

        private void cmdDeleteSmtp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var smtp = getSelectedSmtp();
                if (smtp == null) return;
                var profile = _SmtpClients.FirstOrDefault(sm => sm.Id == smtp.Id);
                if (profile == null) return;
                if (
                    PNMessageBox.Show(
                        PNLang.Instance.GetMessageText("remove_smtp", "Remove selected SMTP profile?"),
                        PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
                _SmtpClients.Remove(profile);
                fillSmtpClients(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddMailContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlgMailContact = new WndMailContact(null) { Owner = this };
                dlgMailContact.MailContactChanged += dlgMailContact_MailContactChanged;
                var showDialog = dlgMailContact.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    dlgMailContact.MailContactChanged -= dlgMailContact_MailContactChanged;
                cmdClearMailContacts.IsEnabled = grdMailContacts.Items.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdEditMailContact_Click(object sender, RoutedEventArgs e)
        {
            editMailContact();
        }

        private void cmdDeleteMailContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var contact = getSelectedMailContact();
                if (contact == null) return;
                if (
                    PNMessageBox.Show(
                        PNLang.Instance.GetMessageText("remove_mail_contact", "Remove selected mail contact?"),
                        PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
                _MailContacts.Remove(contact);
                fillMailContacts(false);
                cmdClearMailContacts.IsEnabled = grdMailContacts.Items.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdClearMailContacts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _MailContactsList.Clear();
                _MailContacts.Clear();
                cmdClearMailContacts.IsEnabled = cmdEditMailContact.IsEnabled = cmdDeleteMailContact.IsEnabled = false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdImportMailContact_Click(object sender, RoutedEventArgs e)
        {
            ctmImpContacts.IsOpen = true;
        }
        #endregion

        #region Protection staff
        private void cmdCreatePwrd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // if there is no password - show standard password creation dialog
                if (PNStatic.Settings.Protection.PasswordString.Trim().Length == 0)
                {
                    var dpc = new WndPasswordCreate();
                    dpc.PasswordChanged += dpc_PasswordChanged;
                    var showDialog = dpc.ShowDialog();
                    if (showDialog == null || !showDialog.Value)
                    {
                        dpc.PasswordChanged -= dpc_PasswordChanged;
                    }
                }
                else
                {
                    // otherwise, if password has been deleted but not applied yet, show password changing dialog using old password
                    cmdChangePwrd.PerformClick();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdChangePwrd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // if new password has been applied or old password has not been changed - show standard password changing dialog
                if (PNStatic.Settings.Protection.PasswordString == _TempSettings.Protection.PasswordString)
                {
                    var dpc = new WndPasswordChange();
                    dpc.PasswordChanged += dpc_PasswordChanged;
                    var showDialog = dpc.ShowDialog();
                    if (showDialog == null || !showDialog.Value)
                    {
                        dpc.PasswordChanged -= dpc_PasswordChanged;
                    }
                }
                else
                {
                    // if new password has not been applied and old password is empty - show standard password creation dialog
                    if (PNStatic.Settings.Protection.PasswordString.Trim().Length == 0)
                    {
                        var dpc = new WndPasswordCreate();
                        dpc.PasswordChanged += dpc_PasswordChanged;
                        var showDialog = dpc.ShowDialog();
                        if (showDialog == null || !showDialog.Value)
                        {
                            dpc.PasswordChanged -= dpc_PasswordChanged;
                        }
                    }
                    else
                    {
                        // otherwise show standard password changing dialog using old password
                        var dpc = new WndPasswordChange();
                        dpc.PasswordChanged += dpc_PasswordChanged;
                        var showDialog = dpc.ShowDialog();
                        if (showDialog == null || !showDialog.Value)
                        {
                            dpc.PasswordChanged -= dpc_PasswordChanged;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdRemovePwrd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // if new password has been applied or old password has not been changed - show standard password removing dialog
                if (PNStatic.Settings.Protection.PasswordString == _TempSettings.Protection.PasswordString)
                {
                    var dpd = new WndPasswordDelete(PasswordDlgMode.DeleteMain);
                    dpd.PasswordDeleted += dpd_PasswordDeleted;
                    var showDialog = dpd.ShowDialog();
                    if (showDialog == null || !showDialog.Value)
                    {
                        dpd.PasswordDeleted -= dpd_PasswordDeleted;
                    }
                }
                else
                {
                    // if new password has not been applied and old password is empty - just remove new password
                    if (PNStatic.Settings.Protection.PasswordString.Trim().Length == 0)
                    {
                        dpd_PasswordDeleted(this, new EventArgs());
                    }
                    else
                    {
                        // otherwise show standard password removing dialog using old password
                        var dpd = new WndPasswordDelete(PasswordDlgMode.DeleteMain);
                        dpd.PasswordDeleted += dpd_PasswordDeleted;
                        var showDialog = dpd.ShowDialog();
                        if (showDialog == null || !showDialog.Value)
                        {
                            dpd.PasswordDeleted -= dpd_PasswordDeleted;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dpd_PasswordDeleted(object sender, EventArgs e)
        {
            _TempSettings.Protection.PasswordString = "";
            cmdChangePwrd.IsEnabled =
                cmdRemovePwrd.IsEnabled =
                chkStoreEncrypted.IsEnabled = chkHideTrayIcon.IsEnabled = false;
            chkStoreEncrypted.IsChecked = chkHideTrayIcon.IsChecked = false;
            cmdCreatePwrd.IsEnabled = true;

            var dlgPasswordDelete = sender as WndPasswordDelete;
            if (dlgPasswordDelete != null)
                dlgPasswordDelete.PasswordDeleted -= dpd_PasswordDeleted;
        }

        private void dpc_PasswordChanged(object sender, PasswordChangedEventArgs e)
        {
            _TempSettings.Protection.PasswordString = e.NewPassword;
            cmdChangePwrd.IsEnabled = cmdRemovePwrd.IsEnabled = chkStoreEncrypted.IsEnabled = chkHideTrayIcon.IsEnabled = true;
            cmdCreatePwrd.IsEnabled = false;

            if (sender.GetType() == typeof(WndPasswordCreate))
            {
                var dlgPasswordCreate = sender as WndPasswordCreate;
                if (dlgPasswordCreate != null)
                    dlgPasswordCreate.PasswordChanged -= dpc_PasswordChanged;
            }
            else
            {
                var dlgPasswordChange = sender as WndPasswordChange;
                if (dlgPasswordChange != null)
                    dlgPasswordChange.PasswordChanged -= dpc_PasswordChanged;
            }
        }

        private void CheckProtection_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as CheckBox;
                if (cb == null || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkStoreEncrypted":
                        _TempSettings.Protection.StoreAsEncrypted = cb.IsChecked.Value;
                        break;
                    case "chkHideTrayIcon":
                        _TempSettings.Protection.HideTrayIcon = cb.IsChecked.Value;
                        break;
                    case "chkBackupBeforeSaving":
                        _TempSettings.Protection.BackupBeforeSaving = cb.IsChecked.Value;
                        break;
                    case "chkSilentFullBackup":
                        _TempSettings.Protection.SilentFullBackup = cb.IsChecked.Value;
                        break;
                    case "chkDonotShowProtected":
                        _TempSettings.Protection.DontShowContent = cb.IsChecked.Value;
                        break;
                    case "chkIncludeBinInLocalSync":
                        _TempSettings.Protection.IncludeBinInSync = cb.IsChecked.Value;
                        break;
                    case "chkPromptForPassword":
                        _TempSettings.Protection.PromptForPassword = cb.IsChecked.Value;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updBackup_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            if (!_Loaded) return;
            _TempSettings.Protection.BackupDeepness = Convert.ToInt32(updBackup.Value);
        }

        private void chkW0_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                var cb = sender as CheckBox;
                if (cb == null || cb.IsChecked == null) return;
                var dw = (DayOfWeek)Convert.ToInt32(cb.Tag);
                if (cb.IsChecked.Value)
                {
                    if (!_TempSettings.Protection.FullBackupDays.Contains(dw))
                        _TempSettings.Protection.FullBackupDays.Add(dw);
                }
                else
                {
                    _TempSettings.Protection.FullBackupDays.RemoveAll(d => d == dw);
                }
                enableFullBackupTime();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dtpFullBackup_DateValueChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            try
            {
                if (_Loaded)
                {
                    _TempSettings.Protection.FullBackupTime = dtpFullBackup.DateValue;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdLocalSync_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmdEditComp.IsEnabled = cmdRemoveComp.IsEnabled = grdLocalSync.SelectedItems.Count > 0;
        }

        private void grdLocalSync_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            editSyncComp();
        }

        private void cmdAddComp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var d = new WndSyncComps(this, AddEditMode.Add);
                var showDialog = d.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillSyncComps(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdEditComp_Click(object sender, RoutedEventArgs e)
        {
            editSyncComp();
        }

        private void cmdRemoveComp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var message = PNLang.Instance.GetMessageText("sync_comp_delete", "Delete selected synchronization target?");
                if (PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                var sc = getSelectedSyncComp();
                if (sc == null) return;
                _SyncComps.Remove(sc);
                fillSyncComps(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion       
    }
}
