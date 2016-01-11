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
using Microsoft.Win32;
using PluginsCore;
using PNEncryption;
using PNRichEdit;
using PNStaticFonts;
using PNWCFLib;
using SQLiteWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Control = System.Windows.Controls.Control;
using Cursors = System.Windows.Input.Cursors;
using MenuItem = System.Windows.Controls.MenuItem;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Path = System.IO.Path;
using Point = System.Windows.Point;
using RichTextBox = System.Windows.Forms.RichTextBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using Screen = System.Windows.Forms.Screen;
using SystemColors = System.Windows.SystemColors;
using TextDataFormat = System.Windows.TextDataFormat;
using Timer = System.Timers.Timer;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WndMain : IPluginsHost
    {
        internal event EventHandler<NoteBooleanChangedEventArgs> NoteBooleanChanged;
        internal event EventHandler LanguageChanged;
        internal event EventHandler HotKeysChanged;
        internal event EventHandler<NoteNameChangedEventArgs> NoteNameChanged;
        internal event EventHandler<NoteGroupChangedEventArgs> NoteGroupChanged;
        internal event EventHandler<NoteDateChangedEventArgs> NoteDateChanged;
        internal event EventHandler<NoteDockStatusChangedEventArgs> NoteDockStatusChanged;
        internal event EventHandler<NoteSendReceiveStatusChangedEventArgs> NoteSendReceiveStatusChanged;
        internal event EventHandler NoteTagsChanged;
        internal event EventHandler ContentDisplayChanged;
        internal event EventHandler<NoteDeletedCompletelyEventArgs> NoteDeletedCompletely;
        internal event EventHandler<SpellCheckingStatusChangedEventArgs> SpellCheckingStatusChanged;
        internal event EventHandler SpellCheckingDictionaryChanged;
        internal event EventHandler<NewNoteCreatedEventArgs> NewNoteCreated;
        internal event EventHandler NoteScheduleChanged;
        internal event EventHandler NotesReceived;
        internal event EventHandler<MenusOrderChangedEventArgs> MenusOrderChanged;

        private const int CRITICAL_CHECK_INTERVAL = 600;    //600 seconds (10 minutes)
        private const int WM_HOTKEY = 0x0312;

        private bool _ShowHide;
        private bool _InDblClick;
        private int _Elapsed;
        private bool _FirstRun;
        private readonly Timer _TimerAutosave = new Timer();
        private readonly Timer _TimerCleanBin = new Timer(1000);
        private readonly Timer _TimerPin = new Timer(100);
        private readonly Timer _TimerBackup = new Timer(20000);
        private readonly Timer _TimerMonitor = new Timer(1000);
        private readonly System.Windows.Forms.Timer _TmrDblClick = new System.Windows.Forms.Timer { Interval = 100 };
        private List<string> _ReceivedNotes;
        private readonly IEnumerable<PNote> _PinnedNotes = PNStatic.Notes.Where(n => n.Pinned);
        private PNWCFHostRunner _HostRunner;
        private PinClass _PinClass;
        private bool _UnsubscribedCritical;
        private int _CheckCriticalTimeElapsed;
        private HwndSource _HwndSource;

        private readonly Dictionary<string, ToolStripMenuItem> _SyncPluginsMenus =
            new Dictionary<string, ToolStripMenuItem>();

        public WndMain()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        #region Window procedures
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNStatic.Settings.Protection.PasswordString != "")
                {
                    var d = new WndPasswordDelete(PasswordDlgMode.LoginMain);
                    var result = d.ShowDialog();
                    if (result == null || !result.Value)
                    {
                        PNStatic.HideSplash = true;
                        Close();
                    }
                }

                PNStatic.FormPanel = new WndPanel();

                //set double click timer handler
                _TmrDblClick.Tick += tmrDblClick_Tick;

                PNSingleton.Instance.FontUser.PropertyChanged += FontUser_PropertyChanged;
                //save monitors count
                PNSingleton.Instance.MonitorsCount = Screen.AllScreens.Length;

                //save screens rectangle
                PNSingleton.Instance.ScreenRect = PNStatic.AllScreensBounds();

                ApplyNewDefaultMenu();

                // get local ip address
                PNSingleton.Instance.IpAddress = PNStatic.GetLocalIPv4(NetworkInterfaceType.Wireless80211) ??
                                                 PNStatic.GetLocalIPv4(NetworkInterfaceType.Ethernet);

                //var ips = Dns.GetHostEntry(Dns.GetHostName());
                //// Select the first entry. I hope it's this maschines IP
                //var ipAddress =
                //    ips.AddressList.FirstOrDefault(
                //        ip => ip.AddressFamily == AddressFamily.InterNetwork);
                //if (ipAddress != null)
                //{
                //    _IpAddress = ipAddress.ToString();
                //}

                //check startup shortcut
                checkStartUpShortcut();

                //subscribe to system events
                SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
                SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

                createPaths();
                applyLanguage();
                //load/create database
                loadDatabase();

                //#if !DEBUG
                if (PNStatic.Settings.GeneralSettings.CheckCriticalOnStart)
                {
                    //check for critical updates (synchronously)
                    PNStatic.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("critical_check",
                        "Checking for critical updates");
                    var result = checkCriticalUpdates();
                    if (result != CriticalUpdateAction.None)
                    {
                        PNStatic.HideSplash = true;
                        if ((result & CriticalUpdateAction.Program) == CriticalUpdateAction.Program)
                        {
                            Close();
                            return;
                        }
                        ApplyAction(MainDialogAction.Restart, null);
                        return;
                    }
                }
                //#endif
                PNStatic.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("load_hotkeys", "Applying hot keys");
                //apply possible hot keys
                ApplyNewHotkeys();
                //register hot keys
                registerMainHotkeys();
                // register custom fonts
                if (PNStatic.Settings.GeneralSettings.UseCustomFonts)
                {
                    PNInterop.AddCustomFonts();
                }
                PNStatic.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("load_spellchecker",
                    "Initializaing spell checker");
                //init spell checker
                initSpeller();

                //prepare docking collections
                PNStatic.DockedNotes.Add(DockStatus.Left, new List<PNote>());
                PNStatic.DockedNotes.Add(DockStatus.Top, new List<PNote>());
                PNStatic.DockedNotes.Add(DockStatus.Right, new List<PNote>());
                PNStatic.DockedNotes.Add(DockStatus.Bottom, new List<PNote>());
                //init dock arrows
                initDockArrows();

                // check exit flag and autosaved notes
                if (PNStatic.Settings.Config.ExitFlag != 0)
                {
                    restoreAutosavedNotes();
                }
                // clear all autosaved notes
                clearAutosavedNotes();

                //execute possible synchronization
                if (PNStatic.Settings.Network.SyncOnStart)
                {
                    PNStatic.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("sync_in_progress",
                        "Synchronization in progress...");
                    var plugins = PNPlugins.Instance.SyncPlugins.Where(p => PNStatic.SyncPlugins.Contains(p.Name));
                    foreach (var p in plugins)
                    {
                        switch (p.Synchronize())
                        {
                            case SyncResult.Reload:
                                PNData.UpdateTablesAfterSync();
                                break;
                            case SyncResult.AbortVersion:
                                PNMessageBox.Show(
                                    PNLang.Instance.GetMessageText("diff_versions",
                                        "Current version of database is different from previously synchronized version. Synchronization cannot be performed."),
                                    p.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                break;
                        }
                    }
                }

                // define autosave timer procedures
                _TimerAutosave.Elapsed += TimerAutosave_Elapsed;
                if (PNStatic.Settings.GeneralSettings.Autosave)
                {
                    _TimerAutosave.Interval = PNStatic.Settings.GeneralSettings.AutosavePeriod * 60000;
                    _TimerAutosave.Start();
                }

                //check skins existance
                if (PNStatic.Settings.GeneralSettings.UseSkins)
                {
                    PNStatic.Settings.GeneralSettings.UseSkins = PNStatic.CheckSkinsExistance();
                }

                if (PNStatic.Settings.Behavior.ShowNotesPanel)
                {
                    PNStatic.FormPanel.Show();
                }

                PNStatic.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("load_notes", "Loading notes");
                loadNotes();

                PNStatic.HideSplash = true;

                showPriorityOnStart();

                // define clean bin timer procedures
                _TimerCleanBin.Elapsed += TimerCleanBin_Elapsed;
                if (PNStatic.Settings.GeneralSettings.RemoveFromBinPeriod > 0)
                {
                    _TimerCleanBin.Start();
                }

                PNStatic.StartTime = DateTime.Now;

                adjustStartTimes();

                // start listening thread
                if (PNStatic.Settings.Network.EnableExchange)
                {
                    //StartListening();
                    StartWCFHosting();
                }

                // check overdue notes
                checkOverdueNotes();

                ntfPN.Visibility = Visibility.Visible;

                if (_FirstRun)
                {
                    var sb =
                        new StringBuilder(PNLang.Instance.GetMessageText("first_baloon_caption",
                            "Thank you for using PNotes.NET!"));
                    sb.AppendLine();
                    sb.Append(PNLang.Instance.GetMessageText("first_baloon_message",
                        "Right click on system tray icon to begin the work."));
                    var baloon = new Baloon(BaloonMode.FirstRun) { BaloonText = sb.ToString() };
                    baloon.BaloonLinkClicked += baloon_BaloonLinkClicked;
                    ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, 15000);
                }
                else if (PNSingleton.Instance.PlatformChanged)
                {
                    var sb =
                        new StringBuilder(PNLang.Instance.GetMessageText("new_edition",
                            "You have started the new edition of PNotes.NET. All your notes and settings from previous edition have been saved as  ZIP archive at:"));
                    sb.AppendLine();
                    sb.Append(System.Windows.Forms.Application.StartupPath);
                    sb.Append(@"\");
                    sb.Append(PNStrings.OLD_EDITION_ARCHIVE);
                    var baloon = new Baloon(BaloonMode.Information) { BaloonText = sb.ToString() };
                    baloon.BaloonLinkClicked += baloon_BaloonLinkClicked;
                    ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, 15000);
                }
                //show Control Panel
                if (PNStatic.Settings.GeneralSettings.ShowCPOnStart)
                {
                    mnuCP.PerformClick();
                }

                //enable pin timer
                _TimerPin.Elapsed += TimerPin_Elapsed;
                _TimerPin.Start();

                //enable backup timer
                _TimerBackup.Elapsed += TimerBackup_Elapsed;
                _TimerBackup.Start();

                //enable monitor timer
                _TimerMonitor.Elapsed += _TimerMonitor_Elapsed;
                _TimerMonitor.Start();

                //hide possible hidden menus
                PNStatic.HideMenus(ctmPN, PNStatic.HiddenMenus.Where(hm => hm.Type == MenuType.Main).ToArray());

                //check for new version
                if (PNStatic.Settings.GeneralSettings.CheckNewVersionOnStart)
                {
                    checkForNewVersion();
                }

                //create dropper cursor
                PNSingleton.Instance.DropperCursor = PNStatic.CreateCursorFromResource("cursors/dropper.cur");

                var criticalLog = Path.Combine(Path.GetTempPath(), PNStrings.CRITICAL_UPDATE_LOG);
                if (File.Exists(criticalLog))
                {
                    using (var sr = new StreamReader(criticalLog))
                    {
                        while (sr.Peek() != -1)
                        {
                            PNStatic.LogThis("Critical udate has been applied for " + sr.ReadLine());
                        }
                    }
                    File.Delete(criticalLog);
                    var sb =
                        new StringBuilder(PNLang.Instance.GetMessageText("critical_applied",
                            "The program has restarted for applying critical updates."));
                    sb.AppendLine();
                    sb.Append(PNLang.Instance.GetMessageText("send_error_2",
                        "Please, refer to log file for details."));
                    var baloon = new Baloon(BaloonMode.CriticalUpdates) { BaloonText = sb.ToString() };
                    baloon.BaloonLinkClicked += baloon_BaloonLinkClicked;
                    ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, 10000);
                }
                if (PNSingleton.Instance.NoteFromShortcut != "")
                {
                    showNoteFromShortcut(PNSingleton.Instance.NoteFromShortcut);
                    PNSingleton.Instance.NoteFromShortcut = "";
                }
                restoreNotesShortcuts();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                PNSingleton.Instance.IsMainWindowLoaded = true;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                PNSingleton.Instance.AppClosed = true;
                if (_HwndSource != null) _HwndSource.RemoveHook(WndProc);
                // stop timers
                _TimerAutosave.Stop();
                _TimerCleanBin.Stop();
                _TimerPin.Stop();
                _TimerBackup.Stop();
                _TimerMonitor.Stop();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                // stop listening
                StopWCFHosting();

                //unsubscribe to system events
                SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
                SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
                //stop spell checking
                Spellchecking.HuspellStop();

                //save all notes if needed
                if (!PNSingleton.Instance.ExitWithoutSaving)
                {
                    if (PNStatic.Settings.GeneralSettings.SaveOnExit)
                    {
                        PNNotesOperations.SaveAllNotes(!PNSingleton.Instance.DoNotAskIfSave);
                    }
                }
                //save all notes shortcuts if needed
                if (PNStatic.Settings.GeneralSettings.DeleteShortcutsOnExit)
                {
                    PNNotesOperations.DeleteAllNotesShortcuts();
                }
                // unregister custom fonts
                if (PNStatic.Settings.GeneralSettings.UseCustomFonts)
                {
                    PNInterop.RemoveCustomFonts();
                }
                //unregister hot keys
                unregisterMainHotkeys();
                //free all groups
                foreach (var g in PNStatic.Groups)
                {
                    g.Dispose();
                }
                //free all notes
                foreach (var n in PNStatic.Notes)
                {
                    n.Dispose();
                }

                //save exit flag
                PNData.SaveExitFlag(0);

#if !DEBUG
                // clean registry
                PNRegistry.CleanRegRunMRU();
                PNRegistry.CleanRegMUICache();
                PNRegistry.CleanRegOpenWithList();
                PNRegistry.CleanRegOpenSaveMRU();
#endif
                //compact databases
                PNData.CompactDatabases();

                //dispose dropper cursor
                if (PNSingleton.Instance.DropperCursor != null)
                    PNSingleton.Instance.DropperCursor.Dispose();

                ntfPN.Dispose();

                //close all windows
                foreach (Window w in Application.Current.Windows)
                    w.Close();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var handle = (new WindowInteropHelper(this)).Handle;
            _HwndSource = HwndSource.FromHwnd(handle);
            if (_HwndSource != null) _HwndSource.AddHook(WndProc);
            if (!PNStatic.Settings.Behavior.HideMainWindow) return;
            var exStyle = PNInterop.GetWindowLong(handle, PNInterop.GWL_EXSTYLE);
            exStyle |= PNInterop.WS_EX_TOOLWINDOW;
            PNInterop.SetWindowLong(handle, PNInterop.GWL_EXSTYLE, exStyle);
        }

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_HOTKEY:
                    var id = wParam.ToInt32();
                    var hk = PNStatic.HotKeysMain.FirstOrDefault(h => h.ID == id);
                    if (hk != null)
                    {
                        if (PNStatic.HiddenMenus.Any(hm => hm.Type == MenuType.Main && hm.Name == hk.MenuName))
                            return IntPtr.Zero;
                        switch (hk.MenuName)
                        {
                            case "mnuNewNote":
                                if (mnuNewNote.IsEnabled)
                                {
                                    mnuNewNote.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuNewNoteInGroup":
                                if (mnuNewNoteInGroup.IsEnabled)
                                {
                                    mnuNewNoteInGroup.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuNoteFromClipboard":
                                if (mnuNoteFromClipboard.IsEnabled)
                                {
                                    mnuNoteFromClipboard.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuLoadNote":
                                if (mnuLoadNote.IsEnabled)
                                {
                                    mnuLoadNote.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuTodayDiary":
                                if (mnuTodayDiary.IsEnabled)
                                {
                                    mnuTodayDiary.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuPrefs":
                                if (mnuPrefs.IsEnabled)
                                {
                                    mnuPrefs.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuCP":
                                if (mnuCP.IsEnabled)
                                {
                                    mnuCP.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuHotkeys":
                                if (mnuHotkeys.IsEnabled)
                                {
                                    mnuHotkeys.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuMenusManagement":
                                if (mnuMenusManagement.IsEnabled)
                                {
                                    mnuMenusManagement.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuShowAll":
                                if (mnuShowAll.IsEnabled)
                                {
                                    mnuShowAll.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuShowIncoming":
                                if (mnuShowIncoming.IsEnabled)
                                {
                                    mnuShowIncoming.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuHideAll":
                                if (mnuHideAll.IsEnabled)
                                {
                                    mnuHideAll.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuHideIncoming":
                                if (mnuHideIncoming.IsEnabled)
                                {
                                    mnuHideIncoming.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuTodayLast":
                                if (mnuTodayLast.IsEnabled)
                                {
                                    mnuTodayLast.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuAllToFront":
                                if (mnuAllToFront.IsEnabled)
                                {
                                    mnuAllToFront.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSaveAll":
                                if (mnuSaveAll.IsEnabled)
                                {
                                    mnuSaveAll.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuBackupCreate":
                                if (mnuBackupCreate.IsEnabled)
                                {
                                    mnuBackupCreate.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuBackupRestore":
                                if (mnuBackupRestore.IsEnabled)
                                {
                                    mnuBackupRestore.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSyncLocal":
                                if (mnuSyncLocal.IsEnabled)
                                {
                                    mnuSyncLocal.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuImportFromOld":
                                if (mnuImportNotes.IsEnabled)
                                {
                                    mnuImportNotes.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuReloadAll":
                                if (mnuReloadAll.IsEnabled)
                                {
                                    mnuReloadAll.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuDAllNone":
                                if (mnuDAllNone.IsEnabled)
                                {
                                    mnuDAllNone.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuDAllLeft":
                                if (mnuDAllLeft.IsEnabled)
                                {
                                    mnuDAllLeft.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuDAllTop":
                                if (mnuDAllTop.IsEnabled)
                                {
                                    mnuDAllTop.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuDAllRight":
                                if (mnuDAllRight.IsEnabled)
                                {
                                    mnuDAllRight.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuDAllBottom":
                                if (mnuDAllBottom.IsEnabled)
                                {
                                    mnuDAllBottom.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSOnHighPriority":
                                if (mnuSOnHighPriority.IsEnabled)
                                {
                                    mnuSOnHighPriority.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSOnProtection":
                                if (mnuSOnProtection.IsEnabled)
                                {
                                    mnuSOnProtection.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSOnComplete":
                                if (mnuSOnComplete.IsEnabled)
                                {
                                    mnuSOnComplete.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSOnRoll":
                                if (mnuSOnRoll.IsEnabled)
                                {
                                    mnuSOnRoll.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSOnOnTop":
                                if (mnuSOnOnTop.IsEnabled)
                                {
                                    mnuSOnOnTop.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSOffHighPriority":
                                if (mnuSOffHighPriority.IsEnabled)
                                {
                                    mnuSOffHighPriority.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSOffProtection":
                                if (mnuSOffProtection.IsEnabled)
                                {
                                    mnuSOffProtection.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSOffComplete":
                                if (mnuSOffComplete.IsEnabled)
                                {
                                    mnuSOffComplete.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSOffUnroll":
                                if (mnuSOffUnroll.IsEnabled)
                                {
                                    mnuSOffUnroll.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSOffOnTop":
                                if (mnuSOffOnTop.IsEnabled)
                                {
                                    mnuSOffOnTop.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSearchInNotes":
                                if (mnuSearchInNotes.IsEnabled)
                                {
                                    mnuSearchInNotes.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSearchByTags":
                                if (mnuSearchByTags.IsEnabled)
                                {
                                    mnuSearchByTags.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSearchByDates":
                                if (mnuSearchByDates.IsEnabled)
                                {
                                    mnuSearchByDates.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuShowAllFavorites":
                                if (mnuShowAllFavorites.IsEnabled)
                                {
                                    mnuShowAllFavorites.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuLockProg":
                                if (mnuLockProg.IsEnabled)
                                {
                                    mnuLockProg.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuHelp":
                                if (mnuHelp.IsEnabled)
                                {
                                    mnuHelp.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuAbout":
                                if (mnuAbout.IsEnabled)
                                {
                                    mnuAbout.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuHomepage":
                                if (mnuHomepage.IsEnabled)
                                {
                                    mnuHomepage.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuSupport":
                                if (mnuSupport.IsEnabled)
                                {
                                    mnuSupport.PerformClick();
                                    handled = true;
                                }
                                break;
                            case "mnuExit":
                                if (mnuExit.IsEnabled)
                                {
                                    mnuExit.PerformClick();
                                    handled = true;
                                }
                                break;
                        }
                    }
                    else
                    {
                        hk = PNStatic.HotKeysNote.FirstOrDefault(h => h.ID == id);
                        if (hk != null)
                        {
                            var d = Application.Current.Windows.OfType<WndNote>().FirstOrDefault(w => w.Active);
                            if (d == null) return IntPtr.Zero;
                            d.HotKeyClick(hk);
                            handled = true;
                        }
                        else
                        {
                            hk = PNStatic.HotKeysEdit.FirstOrDefault(h => h.ID == id);
                            if (hk != null)
                            {
                                var d = Application.Current.Windows.OfType<WndNote>().FirstOrDefault(w => w.Active);
                                if (d == null) return IntPtr.Zero;
                                d.HotKeyClick(hk);
                                handled = true;
                            }
                            else
                            {
                                hk = PNStatic.HotKeysGroups.FirstOrDefault(h => h.ID == id);
                                if (hk == null) return IntPtr.Zero;
                                if (!hk.MenuName.Contains('_')) return IntPtr.Zero;
                                var pos = hk.MenuName.IndexOf('_');
                                var groupId = int.Parse(hk.MenuName.Substring(0, pos), PNStatic.CultureInvariant);
                                var action = hk.MenuName.Substring(pos + 1);
                                PNNotesOperations.ShowHideSpecificGroup(groupId, action.ToUpper() == "SHOW");
                                handled = true;
                            }
                        }
                    }
                    break;
                case PNInterop.WM_ACTIVATEAPP:
                    if (wParam.ToInt32() == 0)
                    {
                        PNStatic.DeactivateNotesWindows();
                    }
                    handled = true;
                    break;
                case PNInterop.WPM_CLOSE_PROG:
                    mnuExit.PerformClick();
                    handled = true;
                    break;
                case PNInterop.WPM_CLOSE_SILENT_SAVE:
                    PNSingleton.Instance.DoNotAskIfSave = true;
                    mnuExit.PerformClick();
                    handled = true;
                    break;
                case PNInterop.WPM_CLOSE_SILENT_WO_SAVE:
                    PNSingleton.Instance.ExitWithoutSaving = true;
                    mnuExit.PerformClick();
                    handled = true;
                    break;
                case PNInterop.WPM_NEW_NOTE:
                    mnuNewNote.PerformClick();
                    handled = true;
                    break;
                case PNInterop.WPM_NEW_NOTE_FROM_CB:
                    mnuNoteFromClipboard.PerformClick();
                    handled = true;
                    break;
                case PNInterop.WPM_NEW_DIARY:
                    mnuTodayDiary.PerformClick();
                    handled = true;
                    break;
                case PNInterop.WPM_RELOAD_NOTES:
                    mnuReloadAll.PerformClick();
                    handled = true;
                    break;
                case PNInterop.WPM_START_FROM_ANOTHER_INSTANCE:
                    bool? result = true;
                    if (PNStatic.Settings.Protection.PasswordString != "" && PNSingleton.Instance.IsLocked)
                    {
                        var d = new WndPasswordDelete(PasswordDlgMode.LoginMain);
                        result = d.ShowDialog();
                    }
                    if (result == null || !result.Value) return IntPtr.Zero;
                    actionDoubleSingle(PNStatic.Settings.Behavior.DoubleClickAction);
                    handled = true;
                    break;
                case PNInterop.WM_COPYDATA:
                    var m = Message.Create(hWnd, msg, wParam, lParam);
                    var cpdata =
                        (PNInterop.COPYDATASTRUCT)m.GetLParam(typeof(PNInterop.COPYDATASTRUCT));
                    switch ((CopyDataType)cpdata.dwData.ToInt32())
                    {
                        case CopyDataType.NewNote:
                            var note = newNote();
                            if (note != null && note.Dialog != null)
                            {
                                var arr = cpdata.lpData.Split(new[] { PNStrings.DEL_CHAR }, StringSplitOptions.RemoveEmptyEntries);
                                note.Dialog.Edit.Text = arr[0];
                                if (arr.Length >= 2)
                                {
                                    note.Name = arr[1];
                                    note.Dialog.PHeader.Title = note.Dialog.Title = note.Name;
                                    note.Dialog.SaveNoteFile(note);
                                    PNNotesOperations.SaveNewNote(note);
                                    note.Dialog.ApplyTooltip();
                                    note.Changed = false;
                                }
                                if (arr.Length >= 3)
                                {
                                    note.Tags.AddRange(arr[2].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                                    PNData.AddNewTags(note.Tags);
                                    PNNotesOperations.SaveNoteTags(note);
                                    note.RaiseTagsChangedEvent();
                                }
                            }
                            handled = true;
                            break;
                        case CopyDataType.LoadNotes:
                            if (cpdata.cbData == 0)
                            {
                                handled = true;
                                break;
                            }
                            var files = cpdata.lpData.Split(new[] { PNStrings.DEL_CHAR },
                                StringSplitOptions.RemoveEmptyEntries);
                            loadNotesFromFilesList(files);
                            handled = true;
                            break;
                        case CopyDataType.ShowNoteById:
                            if (cpdata.cbData == 0)
                            {
                                handled = true;
                                break;
                            }
                            if (PNSingleton.Instance.IsMainWindowLoaded)
                            {
                                var noteToShow = PNStatic.Notes.FirstOrDefault(n => n.ID == cpdata.lpData);
                                if (noteToShow != null)
                                {
                                    PNNotesOperations.ShowHideSpecificNote(noteToShow, true);
                                }
                            }
                            else
                            {
                                PNSingleton.Instance.NoteFromShortcut = cpdata.lpData;
                            }
                            handled = true;
                            break;
                    }
                    break;
                case PNInterop.WPM_BACKUP:
                    createFullBackup(true);
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

        #endregion

        #region Timers
        private void tmrDblClick_Tick(object sender, EventArgs e)
        {
            try
            {
                _Elapsed += 100;
                if (_Elapsed < SystemInformation.DoubleClickTime) return;
                _TmrDblClick.Stop();
                _Elapsed = 0;
                actionDoubleSingle(PNStatic.Settings.Behavior.SingleClickAction);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void _TimerDelegate(object sender, ElapsedEventArgs e);

        private void TimerPin_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (PNSingleton.Instance.AppClosed) return;
                _TimerPin.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    _TimerDelegate d = TimerPin_Elapsed;
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
                    foreach (var note in _PinnedNotes)
                    {
                        _PinClass = new PinClass
                        {
                            Class = note.PinClass,
                            Pattern = PNStatic.CreatePinRegexPattern(note.PinText)
                        };
                        PNInterop.EnumWindowsProcDelegate enumProc = EnumWindowsProc;
                        PNInterop.EnumWindows(enumProc, 0);
                        if (!_PinClass.Hwnd.Equals(IntPtr.Zero))
                        {
                            var wp = new PNInterop.WINDOWPLACEMENT();
                            wp.length = (uint)Marshal.SizeOf(wp);
                            PNInterop.GetWindowPlacement(_PinClass.Hwnd, ref wp);
                            if (wp.showCmd != PNInterop.SW_SHOWMINIMIZED)
                            {
                                if (!_PinClass.Hwnd.Equals(PNInterop.GetForegroundWindow())) continue;
                                // show note if it is not visible
                                if (!note.Visible)
                                {
                                    PNNotesOperations.ShowHideSpecificNote(note, true);
                                }
                                if (note.Dialog == null) continue;
                                var topMost = note.Dialog.Topmost;
                                note.Dialog.Topmost = true;
                                note.Dialog.Topmost = topMost;
                            }
                            else
                            {
                                // hide pinned note if appropriate window is minimized and note is not in alarm state
                                if (note.Visible && note.Dialog != null && !note.Dialog.InAlarm)
                                {
                                    PNNotesOperations.ShowHideSpecificNote(note, false);
                                }
                            }
                        }
                        else
                        {
                            // hide pinned note if appropriate window was not found and note is not in alarm state
                            if (note.Visible && note.Dialog != null && !note.Dialog.InAlarm)
                            {
                                PNNotesOperations.ShowHideSpecificNote(note, false);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (!PNSingleton.Instance.AppClosed)
                    _TimerPin.Start();
            }
        }

        private void TimerBackup_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (PNSingleton.Instance.AppClosed) return;
                _TimerBackup.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    _TimerDelegate d = TimerBackup_Elapsed;
                    try
                    {
                        Dispatcher.Invoke(d, sender, e);
                    }
                    catch (ObjectDisposedException)
                    {
                        // do nothing when main form is disposed
                    }
                }
                else
                {
                    var now = DateTime.Now;
                    if (PNStatic.Settings.Protection.FullBackupDays.Contains(now.DayOfWeek) &&
                        now.TimeOfDay >= PNStatic.Settings.Protection.FullBackupTime.TimeOfDay &&
                        now.Date != PNStatic.Settings.Protection.FullBackupDate.Date)
                    {
                        createFullBackup(true);
                        PNStatic.Settings.Protection.FullBackupDate = now.Date;
                        PNData.SaveProtectionSettings();
                    }
                    // check for critical updates here
                    if (PNSingleton.Instance.AppClosed) return;
                    if (_CheckCriticalTimeElapsed >= CRITICAL_CHECK_INTERVAL)
                    {
                        _CheckCriticalTimeElapsed = 0;
                        if (!PNStatic.Settings.GeneralSettings.CheckCriticalPeriodically) return;
                        var result = checkCriticalUpdates();
                        if ((result & CriticalUpdateAction.Program) == CriticalUpdateAction.Program)
                        {
                            Close();
                        }
                        else if ((result & CriticalUpdateAction.Plugins) == CriticalUpdateAction.Plugins)
                        {
                            ApplyAction(MainDialogAction.Restart, null);
                        }
                    }
                    else
                    {
                        _CheckCriticalTimeElapsed += (int)_TimerBackup.Interval / 1000;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (!PNSingleton.Instance.AppClosed)
                    _TimerBackup.Start();
            }
        }

        private void _TimerMonitor_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (PNSingleton.Instance.AppClosed) return;
                _TimerMonitor.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    _TimerDelegate d = _TimerMonitor_Elapsed;
                    try
                    {
                        Dispatcher.Invoke(d, sender, e);
                    }
                    catch (ObjectDisposedException)
                    {
                        // do nothing when main form is disposed
                    }
                }
                else
                {
                    if (isMonitorPluggedUnplugged()) return;
                    isScreenRectangleChanged();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (!PNSingleton.Instance.AppClosed)
                    _TimerMonitor.Start();
            }
        }

        private void TimerCleanBin_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (PNSingleton.Instance.AppClosed) return;
                _TimerCleanBin.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    _TimerDelegate d = TimerCleanBin_Elapsed;
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
                    var now = DateTime.Now;
                    var notes = PNStatic.Notes.Where(n => n.GroupID == (int)SpecialGroups.RecycleBin && (now - n.DateDeleted).Days >= PNStatic.Settings.GeneralSettings.RemoveFromBinPeriod);
                    var pNotes = notes as PNote[] ?? notes.ToArray();
                    if (!pNotes.Any()) return;
                    var proceed = true;
                    if (PNStatic.Settings.GeneralSettings.WarnOnAutomaticalDelete)
                    {
                        var sb = new StringBuilder();
                        sb.Append(PNLang.Instance.GetMessageText("clean_bin_1", "The following notes will be permanently deleted from Recycle Bin:"));
                        foreach (var n in pNotes)
                        {
                            sb.AppendLine();
                            sb.Append(n.Name);
                        }
                        sb.AppendLine();
                        sb.Append(PNLang.Instance.GetMessageText("clean_bin_2", "Choose \"Yes\" to continue or \"No\" to postpone the deletion to a future time."));
                        if (PNMessageBox.Show(sb.ToString(), PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        {
                            proceed = false;
                        }
                    }
                    if (proceed)
                    {
                        var aNotes = pNotes.ToArray();
                        for (var i = aNotes.Length - 1; i >= 0; i--)
                        {
                            PNNotesOperations.RemoveDeletedNoteFromLists(aNotes[i]);
                            var id = aNotes[i].ID;
                            var groupId = aNotes[i].GroupID;
                            PNNotesOperations.SaveNoteDeletedState(aNotes[i], false);
                            PNNotesOperations.DeleteNoteCompletely(aNotes[i], CompleteDeletionSource.AutomaticCleanBin);
                            RaiseDeletedCompletelyEvent(id, groupId);
                        }
                    }
                    else
                    {
                        foreach (var n in pNotes)
                        {
                            n.DateDeleted = now;
                            PNNotesOperations.SaveNoteDeletedDate(n);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (!PNSingleton.Instance.AppClosed)
                    _TimerCleanBin.Start();
            }
        }

        private void TimerAutosave_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (PNSingleton.Instance.AppClosed) return;
                _TimerAutosave.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    _TimerDelegate d = TimerAutosave_Elapsed;
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
                    var notes = PNStatic.Notes.Where(n => n.FromDB && n.Changed && n.Visible);
                    foreach (var n in notes)
                    {
                        var destPath = Path.Combine(PNPaths.Instance.DataDir, n.ID + PNStrings.NOTE_AUTO_BACK_EXTENSION);
                        if (File.Exists(destPath))
                        {
                            File.SetAttributes(destPath, FileAttributes.Normal);
                        }
                        PNNotesOperations.SaveNoteFile(n.Dialog.Edit, destPath);
                        File.SetAttributes(destPath, FileAttributes.Hidden);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (!PNSingleton.Instance.AppClosed)
                    _TimerAutosave.Start();
            }
        }
        #endregion

        #region Internal properties

        internal IntPtr Handle
        {
            get { return _HwndSource != null ? _HwndSource.Handle : IntPtr.Zero; }
        }

        internal Timer TimerCleanBin
        {
            get { return _TimerCleanBin; }
        }

        internal Timer TimerAutosave
        {
            get { return _TimerAutosave; }
        }
        #endregion

        #region Internal procedures

        internal void RaiseContentDisplayChangedEevent()
        {
            try
            {
                if (ContentDisplayChanged != null)
                    ContentDisplayChanged(this, new EventArgs());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void DuplicateNote(PNote src)
        {
            try
            {
                if (PNStatic.Settings.Protection.PromptForPassword)
                    if (!PNNotesOperations.LogIntoNoteOrGroup(src))
                        return;
                var note = new PNote(src);
                note.Dialog = new WndNote(note, NewNoteMode.Duplication);
                PNStatic.Notes.Add(note);
                note.Visible = true;

                if (src.Visible)
                    note.NoteSize = src.Dialog.GetSize();
                else
                    note.NoteSize = new Size(PNStatic.Settings.GeneralSettings.Width,
                        PNStatic.Settings.GeneralSettings.Height);
                note.NoteLocation = note.Dialog.GetLocation();
                note.EditSize = note.Dialog.Edit.Size;
                note.Dialog.Show();

                if (src.Visible)
                {
                    note.Dialog.Edit.Rtf = src.Dialog.Edit.Rtf;
                }
                else
                {
                    string path = Path.Combine(PNPaths.Instance.DataDir, src.ID) + PNStrings.NOTE_EXTENSION;
                    PNNotesOperations.LoadNoteFile(note.Dialog.Edit, path);
                }

                if (NewNoteCreated != null)
                {
                    NewNoteCreated(this, new NewNoteCreatedEventArgs(note));
                }
                subscribeToNoteEvents(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void RaiseDeletedCompletelyEvent(string id, int groupId)
        {
            if (NoteDeletedCompletely != null)
            {
                NoteDeletedCompletely(this, new NoteDeletedCompletelyEventArgs(id, groupId));
            }
        }

        internal void RefreshHiddenMenus()
        {
            try
            {
                var hiddens = PNStatic.HiddenMenus.Where(hm => hm.Type == MenuType.Main).ToArray();
                PNStatic.HideMenus(ctmPN, hiddens);
                PNNotesOperations.RefreshHiddenMenus();
                if (PNStatic.FormCP != null)
                {
                    hiddens = PNStatic.HiddenMenus.Where(hm => hm.Type == MenuType.ControlPanel).ToArray();
                    PNStatic.HideMenus(PNStatic.FormCP.CPMenu, hiddens);
                    PNStatic.FormCP.HideToolbarButtons(hiddens);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ShowSentNotification(List<string> notes, List<string> recipients)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(PNLang.Instance.GetCaptionText("sent", "Notes successfully sent"));
                sb.Append(": ");
                foreach (string s in notes)
                {
                    sb.Append(s);
                    sb.Append(";");
                }
                if (sb.Length > 1) sb.Length -= 1;
                sb.AppendLine();
                sb.Append(PNLang.Instance.GetMessageText("recipient", "Recipient(s):"));
                sb.Append(" ");
                foreach (string s in recipients)
                {
                    sb.Append(s);
                    sb.Append(";");
                }
                if (sb.Length > 1) sb.Length -= 1;

                var baloon = new Baloon(BaloonMode.NoteSent) { BaloonText = sb.ToString() };
                ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, 5000);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal bool SendNotesViaNetwork(List<PNote> notes, PNContact cn)
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var note in notes)
                {
                    if (PNStatic.Settings.Protection.PromptForPassword)
                        if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                            continue;
                    string text, tempPath = "";
                    var newNote = false;
                    var nc = new NoteConverter();

                    // decrypt note file to temp file if note is encrypted
                    var path = Path.Combine(PNPaths.Instance.DataDir, note.ID);
                    path += PNStrings.NOTE_EXTENSION;

                    // save note first
                    if (note.Dialog != null && note.Changed)
                    {
                        if (note.FromDB)
                        {
                            if (PNStatic.Settings.Network.SaveBeforeSending)
                            {
                                note.Dialog.ApplySaveNote(false);
                            }
                        }
                        else
                        {
                            path = Path.Combine(Path.GetTempPath(), note.ID);
                            path += PNStrings.NOTE_EXTENSION;
                            note.Dialog.Edit.SaveFile(path, RichTextBoxStreamType.RichText);
                            newNote = true;
                        }
                    }

                    if (PNStatic.Settings.Protection.PasswordString.Length > 0 &&
                        PNStatic.Settings.Protection.StoreAsEncrypted && !newNote)
                    {
                        var fileName = Path.GetFileName(path);
                        if (string.IsNullOrEmpty(fileName))
                            continue;
                        tempPath = Path.Combine(Path.GetTempPath(), fileName);
                        File.Copy(path, tempPath, true);
                        using (var pne = new PNEncryptor(PNStatic.Settings.Protection.PasswordString))
                        {
                            pne.DecryptTextFile(tempPath);
                        }
                        path = tempPath;
                    }
                    // read note file content
                    using (var sr = new StreamReader(path))
                    {
                        text = sr.ReadToEnd();
                    }
                    // remove temp file
                    if (tempPath != "")
                    {
                        File.Delete(tempPath);
                    }
                    //remove temporary file created for new note
                    if (newNote)
                    {
                        File.Delete(path);
                    }
                    sb.Append(text);
                    sb.Append(PNStrings.END_OF_TEXT);
                    sb.Append(nc.ConvertToString(note));
                    sb.Append(PNStrings.END_OF_NOTE);
                }
                if (sb.Length <= 0) return false;

                string ipAddress;
                if (!cn.UseComputerName || !string.IsNullOrEmpty(cn.IpAddress))
                {
                    ipAddress = cn.IpAddress;
                }
                else
                {
                    IPHostEntry ipHostInfo;
                    try
                    {
                        ipHostInfo = Dns.GetHostEntry(cn.ComputerName);
                    }
                    catch (SocketException)
                    {
                        var msg = PNLang.Instance.GetMessageText("host_unknown", "Computer %PLACEHOLDER1% cannot be found on network");
                        msg = msg.Replace(PNStrings.PLACEHOLDER1, cn.ComputerName);
                        PNMessageBox.Show(msg, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    var address = ipHostInfo.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                    if (address == null) return false;
                    ipAddress = address.ToString();
                }
                //check whether contact's computer is on network
                if (PNConnections.CheckContactConnection(ipAddress) == ContactConnection.Disconnected)
                {
                    var msg = PNLang.Instance.GetMessageText("contact_disconnected",
                        "Computer of contact %PLACEHOLDER1% is not connected to network");
                    PNMessageBox.Show(msg.Replace("%PLACEHOLDER1%", cn.Name), PNStrings.PROG_NAME, MessageBoxButton.OK);
                    return false;
                }

                var clientRunner = new PNWCFClientRunner();
                clientRunner.PNDataError += WCFClient_PNDataError;
                clientRunner.NotesSent += WCFClient_NotesSent;
                if (ipAddress == PNSingleton.Instance.IpAddress.ToString())  //we are on intranet, so most probably this is the same computer
                {
                    Task.Factory.StartNew(() =>
                        clientRunner.SendNotes(cn.Name, PNSingleton.Instance.IpAddress.ToString(), sb.ToString(),
                            PNStatic.Settings.Network.ExchangePort.ToString(CultureInfo.InvariantCulture),
                            notes));
                    //var t = new Thread(() => clientRunner.SendNotes(cn.Name, ipAddress.ToString(), sb.ToString(), PNStatic.Settings.Network.ExchangePort.ToString(CultureInfo.InvariantCulture), notes));
                    //t.Start();
                    return true;
                }
                else
                {
                    return clientRunner.SendNotes(cn.Name, ipAddress, sb.ToString(), PNStatic.Settings.Network.ExchangePort.ToString(CultureInfo.InvariantCulture), notes);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private delegate void _NotesSentDelegate(object sender, NotesSentEventArgs e);
        void WCFClient_NotesSent(object sender, NotesSentEventArgs e)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    _NotesSentDelegate d = WCFClient_NotesSent;
                    Dispatcher.Invoke(d, sender, e);
                }
                else
                {
                    var client = sender as PNWCFClientRunner;
                    if (client != null)
                    {
                        client.NotesSent -= WCFClient_NotesSent;
                        client.PNDataError -= WCFClient_PNDataError;
                    }
                    var now = DateTime.Now;
                    foreach (PNote note in e.Notes)
                    {
                        note.DateSent = now;
                        note.SentTo = PNStatic.Contacts.ContactNameByComputerName(e.SentTo);
                        note.SentReceived |= SendReceiveStatus.Sent;
                        PNNotesOperations.SaveNoteOnSend(note);
                        if (PNStatic.Settings.Network.HideAfterSending && note.Visible)
                        {
                            PNNotesOperations.ShowHideSpecificNote(note, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void WCFClient_PNDataError(object sender, PNDataErrorEventArgs e)
        {
            PNStatic.LogException(e.Exception, false);
            var baloon = new Baloon(BaloonMode.Error)
            {
                BaloonText = PNLang.Instance.GetMessageText("send_error_1",
                    "An error occurred during note(s) sending.") + "\n" +
                    PNLang.Instance.GetMessageText("send_error_2",
                    "Please, refer to log file for details.")
            };
            ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, 10000);
        }

        internal void StartWCFHosting()
        {
            try
            {
                _HostRunner = new PNWCFHostRunner();
                _HostRunner.PNDataReceived += _HostRunner_PNDataReceived;
                _HostRunner.PNDataError += _HostRunner_PNDataError;
                Task.Factory.StartNew(
                    () =>
                        _HostRunner.StartHosting(
                            PNStatic.Settings.Network.ExchangePort.ToString(CultureInfo.InvariantCulture)));
                //var t = new Thread(() => _HostRunner.StartHosting(PNStatic.Settings.Network.ExchangePort.ToString(CultureInfo.InvariantCulture)));
                //t.Start();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void StopWCFHosting()
        {
            try
            {
                if (_HostRunner != null)
                {
                    _HostRunner.StopHosting();
                    _HostRunner.PNDataReceived -= _HostRunner_PNDataReceived;
                    _HostRunner.PNDataError -= _HostRunner_PNDataError;
                    _HostRunner = null;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        void _HostRunner_PNDataError(object sender, PNDataErrorEventArgs e)
        {
            PNStatic.LogException(e.Exception);
        }

        private delegate void _PNDataReceivedDelegate(object sender, PNDataReceivedEventArgs e);
        void _HostRunner_PNDataReceived(object sender, PNDataReceivedEventArgs e)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    _PNDataReceivedDelegate d = _HostRunner_PNDataReceived;
                    Dispatcher.Invoke(d, sender, e);
                }
                else
                {
                    receiveNote(e.Data);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyNewLanguage(string languageName)
        {
            try
            {
                PNLang.Instance.LoadLanguage(Path.Combine(PNPaths.Instance.LangDir, languageName));
                applyLanguage();
                applyStandardGroupsNames();

                PNMenus.PrepareDefaultMenuStrip(ctmPN, MenuType.Main, false);
                PNMenus.PrepareDefaultMenuStrip(ctmPN, MenuType.Main, true);

                if (LanguageChanged != null)
                {
                    LanguageChanged(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyNewHotkeys()
        {
            try
            {
                foreach (var ti in ctmPN.Items.OfType<MenuItem>())
                {
                    applyMenuHotkey(ti);
                }
                if (HotKeysChanged != null)
                {
                    HotKeysChanged(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplySpellStatusChange(bool status)
        {
            try
            {
                if (SpellCheckingStatusChanged != null)
                {
                    SpellCheckingStatusChanged(this, new SpellCheckingStatusChangedEventArgs(status));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplySpellDictionaryChange()
        {
            try
            {
                if (SpellCheckingDictionaryChanged != null)
                {
                    SpellCheckingDictionaryChanged(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyDoAlarm(PNote note)
        {
            try
            {
                if (PNNotesOperations.ShowHideSpecificNote(note, true) == ShowHideResult.Success)
                {
                    note.Dialog.InAlarm = true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyAction(MainDialogAction action, object data)
        {
            try
            {
                switch (action)
                {
                    case MainDialogAction.Restart:
                        PNSingleton.Instance.Restart = true;
                        Close();
                        break;
                    case MainDialogAction.ReloadAll:
                        mnuReloadAll.PerformClick();
                        break;
                    case MainDialogAction.Help:
                        mnuHelp.PerformClick();
                        break;
                    case MainDialogAction.Preferences:
                        mnuPrefs.PerformClick();
                        break;
                    case MainDialogAction.SaveAll:
                        mnuSaveAll.PerformClick();
                        break;
                    case MainDialogAction.Support:
                        mnuSupport.PerformClick();
                        break;
                    case MainDialogAction.FullBackupCreate:
                        mnuBackupCreate.PerformClick();
                        break;
                    case MainDialogAction.FullBackupRestore:
                        mnuBackupRestore.PerformClick();
                        break;
                    case MainDialogAction.LocalSync:
                        mnuSyncLocal.PerformClick();
                        break;
                    case MainDialogAction.ShowAll:
                        mnuShowAll.PerformClick();
                        break;
                    case MainDialogAction.HideAll:
                        mnuHideAll.PerformClick();
                        break;
                    case MainDialogAction.BringToFront:
                        mnuAllToFront.PerformClick();
                        break;
                    case MainDialogAction.DockAllNone:
                        mnuDAllNone.PerformClick();
                        break;
                    case MainDialogAction.DockAllLeft:
                        mnuDAllLeft.PerformClick();
                        break;
                    case MainDialogAction.DockAllTop:
                        mnuDAllTop.PerformClick();
                        break;
                    case MainDialogAction.DockAllRight:
                        mnuDAllRight.PerformClick();
                        break;
                    case MainDialogAction.DockAllBottom:
                        mnuDAllBottom.PerformClick();
                        break;
                    case MainDialogAction.SearchInNotes:
                        mnuSearchInNotes.PerformClick();
                        break;
                    case MainDialogAction.SearchByTags:
                        mnuSearchByTags.PerformClick();
                        break;
                    case MainDialogAction.SearchByDates:
                        mnuSearchByDates.PerformClick();
                        break;
                    case MainDialogAction.NewNote:
                        mnuNewNote.PerformClick();
                        break;
                    case MainDialogAction.NewNoteInGroup:
                        newNoteInGroup(Convert.ToInt32(data));
                        break;
                    case MainDialogAction.NoteFromClipboard:
                        mnuNoteFromClipboard.PerformClick();
                        break;
                    case MainDialogAction.NoteFromClipboardInGroup:
                        newNoteFromClipboardInGroup(Convert.ToInt32(data));
                        break;
                    case MainDialogAction.LoadNotes:
                        loadNotesAsFiles(0);
                        break;
                    case MainDialogAction.LoadNotesInGroup:
                        loadNotesAsFiles(Convert.ToInt32(data));
                        break;
                    case MainDialogAction.ImportNotes:
                        mnuImportNotes.PerformClick();
                        break;
                    case MainDialogAction.ImportSettings:
                        mnuImportSettings.PerformClick();
                        break;
                    case MainDialogAction.ImportFonts:
                        mnuImportFonts.PerformClick();
                        break;
                    case MainDialogAction.ImportDictionaries:
                        mnuImportDictionaries.PerformClick();
                        break;
                    case MainDialogAction.About:
                        mnuAbout.PerformClick();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyNewMenusOrder(MenusOrderChangedEventArgs e)
        {
            try
            {
                if (e.Main)
                {
                    PNMenus.RearrangeMenus(ctmPN);
                    PNMenus.PrepareDefaultMenuStrip(ctmPN, MenuType.Main, true);
                }
                if (MenusOrderChanged != null)
                    MenusOrderChanged(this, e);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyNewDefaultMenu()
        {
            try
            {
                if (PNSingleton.Instance.FontUser.FontFamily.FamilyTypefaces.All(tf => tf.Weight != FontWeights.Bold)) return;
                switch (PNStatic.Settings.Behavior.DoubleClickAction)
                {
                    case TrayMouseAction.BringAllToFront:
                        mnuAllToFront.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.ControlPanel:
                        mnuCP.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.LoadNote:
                        mnuLoadNote.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.NewNote:
                        mnuNewNote.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.NewNoteInGroup:
                        mnuNewNoteInGroup.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.NoteFromClipboard:
                        mnuNoteFromClipboard.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.Preferences:
                        mnuPrefs.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.SaveAll:
                        mnuSaveAll.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.SearchByDates:
                        mnuSearchByDates.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.SearchByTags:
                        mnuSearchByTags.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.SearchInNotes:
                        mnuSearchInNotes.FontWeight = FontWeights.Bold;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void LoadNotesByList(List<string> listId, bool show)
        {
            try
            {
                foreach (string id in listId)
                {
                    loadSingleNote(id, show);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Private procedures

        private void restoreNotesShortcuts()
        {
            try
            {
                if (PNStatic.Settings.GeneralSettings.RestoreShortcutsOnStart)
                {
                    var ids = PNData.GetNotesWithShortcuts();
                    if (ids != null)
                    {
                        foreach (var note in ids.Select(id => PNStatic.Notes.FirstOrDefault(n => n.ID == id)).Where(note => note != null))
                        {
                            PNNotesOperations.SaveAsShortcut(note);
                        }
                    }
                }
                //clear field
                PNData.SaveNotesWithShortcuts(null);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void showNoteFromShortcut(string id)
        {
            try
            {
                var noteToShow = PNStatic.Notes.FirstOrDefault(n => n.ID == id);
                if (noteToShow != null)
                {
                    PNNotesOperations.ShowHideSpecificNote(noteToShow, true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void showRecentNotes(int dateDifference)
        {
            try
            {
                var notes =
                    PNStatic.Notes.Where(
                        n => n.GroupID != (int)SpecialGroups.RecycleBin &&
                             (Math.Abs(n.DateCreated.Subtract(DateTime.Now).Days) <= dateDifference ||
                              Math.Abs(n.DateSaved.Subtract(DateTime.Now).Days) <= dateDifference));
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

        private bool lockProgram(bool value)
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Visible);
                if (value)
                {
                    foreach (var n in notes.Where(n => n.Dialog != null))
                    {
                        n.Dialog.Hide();
                    }
                    ntfPN.ContextMenu = null;
                }
                else
                {
                    foreach (var n in notes.Where(n => n.Dialog != null))
                    {
                        n.Dialog.Show();
                    }
                    ntfPN.ContextMenu = ctmPN;
                }
                var vis = value ? Visibility.Hidden : Visibility.Visible;
                if (PNStatic.FormCP != null)
                {
                    PNStatic.FormCP.Visibility = vis;
                }
                if (PNStatic.FormSearchByDates != null)
                {
                    PNStatic.FormSearchByDates.Visibility = vis;
                }
                if (PNStatic.FormSearchByTags != null)
                {
                    PNStatic.FormSearchByTags.Visibility = vis;
                }
                if (PNStatic.FormSearchInNotes != null)
                {
                    PNStatic.FormSearchInNotes.Visibility = vis;
                }
                if (PNStatic.FormSettings != null)
                {
                    PNStatic.FormSettings.Visibility = vis;
                }
                if (PNStatic.FormPanel != null)
                {
                    PNStatic.FormPanel.Visibility = vis;
                }
                if (PNStatic.Settings.Protection.HideTrayIcon)
                {
                    if (value)
                    {
                        var hk = PNStatic.HotKeysMain.FirstOrDefault(h => h.MenuName == "mnuLockProg");
                        if (hk != null && hk.Shortcut == "")
                        {
                            var message = PNLang.Instance.GetMessageText("hide_on_lock_warning", "In order to allow the tray icon to be hidden when program is locked you have to set a hot key for \"Lock Program\" menu item");
                            PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return true;
                        }
                    }
                    ntfPN.Visibility = vis;
                }
                return value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return !value;
            }
        }

        private void actionDoubleSingle(TrayMouseAction action)
        {
            try
            {
                switch (action)
                {
                    case TrayMouseAction.AllShowHide:
                        if (_ShowHide)
                        {
                            //hide all
                            mnuHideAll.PerformClick();
                            _ShowHide = false;
                        }
                        else
                        {
                            //show all
                            mnuShowAll.PerformClick();
                            _ShowHide = true;
                        }
                        break;
                    case TrayMouseAction.BringAllToFront:
                        mnuAllToFront.PerformClick();
                        break;
                    case TrayMouseAction.ControlPanel:
                        mnuCP.PerformClick();
                        break;
                    case TrayMouseAction.LoadNote:
                        mnuLoadNote.PerformClick();
                        break;
                    case TrayMouseAction.NewNote:
                        mnuNewNote.PerformClick();
                        break;
                    case TrayMouseAction.NewNoteInGroup:
                        mnuNewNoteInGroup.PerformClick();
                        break;
                    case TrayMouseAction.NoteFromClipboard:
                        mnuNoteFromClipboard.PerformClick();
                        break;
                    case TrayMouseAction.Preferences:
                        mnuPrefs.PerformClick();
                        break;
                    case TrayMouseAction.SaveAll:
                        mnuSaveAll.PerformClick();
                        break;
                    case TrayMouseAction.SearchByDates:
                        mnuSearchByDates.PerformClick();
                        break;
                    case TrayMouseAction.SearchByTags:
                        mnuSearchByTags.PerformClick();
                        break;
                    case TrayMouseAction.SearchInNotes:
                        mnuSearchInNotes.PerformClick();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createSyncMenu()
        {
            try
            {
                _SyncPluginsMenus.Clear();
                var plugins = PNPlugins.Instance.SyncPlugins.Where(p => PNStatic.SyncPlugins.Contains(p.Name)).ToArray();
                for (var i = mnuBackup.Items.Count - 1; i >= 0; i--)
                {
                    var item = mnuBackup.Items[i] as MenuItem;
                    if (item != null)
                    {
                        if (string.IsNullOrEmpty(item.Name))
                        {
                            item.Click -= syncMenu_Click;
                            mnuBackup.Items.RemoveAt(i);
                        }
                    }
                    var sep = mnuBackup.Items[i] as Separator;
                    if (sep != null && string.IsNullOrEmpty(sep.Name))
                        mnuBackup.Items.RemoveAt(i);
                }
                var index = mnuBackup.Items.IndexOf(sepBackup);
                foreach (var p in plugins)
                {
                    if (_SyncPluginsMenus.Keys.All(k => k != p.MenuSync.Name + p.Name))
                        _SyncPluginsMenus.Add(p.MenuSync.Name + p.Name, p.MenuSync);
                    if (mnuBackup.Items.OfType<MenuItem>().Any(mi => (string)mi.Header == p.MenuSync.Text)) continue;
                    var syncMenu = new MenuItem
                    {
                        Header = p.MenuSync.Text,
                        Icon =
                            new Image
                            {
                                Source = PNStatic.ImageFromDrawingImage(p.MenuSync.Image)
                            },
                        Tag = p.MenuSync.Name + p.Name
                    };
                    syncMenu.Click += syncMenu_Click;
                    mnuBackup.Items.Insert(++index, syncMenu);
                }
                if (plugins.Any() && index != mnuBackup.Items.IndexOf(sepBackup))
                    mnuBackup.Items.Insert(++index, new Separator());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void syncMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var menu = sender as MenuItem;
                if (menu == null) return;
                var tag = Convert.ToString(menu.Tag);
                if (_SyncPluginsMenus.Keys.All(k => k != tag)) return;
                var pmenu = _SyncPluginsMenus[tag];
                if (pmenu == null) return;
                pmenu.PerformClick();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createTagsMenus()
        {
            try
            {
                //show tags
                foreach (
                    var ti in
                        mnuShowByTag.Items.OfType<MenuItem>())
                {
                    ti.Click -= menuClick;
                }
                mnuShowByTag.Items.Clear();
                foreach (var ti in PNStatic.Tags.Select(t => new MenuItem { Header = t }))
                {
                    ti.Click += menuClick;
                    mnuShowByTag.Items.Add(ti);
                }
                //hide tags
                foreach (
                    var ti in
                        mnuHideByTag.Items.OfType<MenuItem>())
                {
                    ti.Click -= menuClick;
                }
                mnuHideByTag.Items.Clear();
                foreach (var ti in PNStatic.Tags.Select(t => new MenuItem { Header = t }))
                {
                    ti.Click += menuClick;
                    mnuHideByTag.Items.Add(ti);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createFavoritesMenu()
        {
            try
            {
                var count = mnuFavorites.Items.Count;
                if (count > 1)
                {
                    for (var i = count - 1; i > 0; i--)
                    {
                        var mi = mnuFavorites.Items[i] as MenuItem;
                        if (mi != null)
                            mi.Click -= menuClick;
                        mnuFavorites.Items.RemoveAt(i);
                    }
                }
                var notes =
                    PNStatic.Notes.Where(n => n.Favorite && n.GroupID != (int)SpecialGroups.RecycleBin).ToList();
                if (notes.Any())
                {
                    mnuShowAllFavorites.IsEnabled = true;
                    mnuFavorites.Items.Add(new Separator());
                    foreach (var ti in notes.Select(note => new MenuItem { Header = note.Name, Tag = note.ID }))
                    {
                        ti.Click += menuClick;
                        mnuFavorites.Items.Add(ti);
                    }
                }
                else
                {
                    mnuShowAllFavorites.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addNoteToShowHideMenu(MenuItem mi, PNote note)
        {
            try
            {
                var ti = new MenuItem { Header = note.Name, Tag = note.ID };
                ti.Click += menuClick;
                mi.Items.Add(ti);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addGroupToShowHideMenu(MenuItem mi, PNGroup group, bool show)
        {
            try
            {
                var ti = new MenuItem { Header = group.Name, Tag = @group.ID };
                if (PNStatic.Settings.Behavior.ShowSeparateNotes)
                {
                    var notes = PNStatic.Notes.Where(n => n.GroupID == group.ID);
                    var pNotes = notes as PNote[] ?? notes.ToArray();
                    if (!pNotes.Any())
                    {
                        ti.IsEnabled = false;
                    }
                    else
                    {
                        var tt = new MenuItem();
                        if (show)
                        {
                            tt.Header = PNLang.Instance.GetMenuText("main_menu", "mnuShowAll", "Show All");
                            tt.Tag = group.ID + "_show";
                        }
                        else
                        {
                            tt.Header = PNLang.Instance.GetMenuText("main_menu", "mnuHideAll", "Hide All");
                            tt.Tag = group.ID + "_hide";
                        }
                        tt.Click += menuClick;
                        ti.Items.Add(tt);
                        ti.Items.Add(new Separator());

                        foreach (PNote n in pNotes)
                        {
                            addNoteToShowHideMenu(ti, n);
                        }
                    }
                }
                else
                {
                    ti.Click += menuClick;
                }
                mi.Items.Insert(2, ti);

                foreach (PNGroup g in group.Subgroups)
                {
                    addGroupToShowHideMenu(mi, g, show);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createShowHideMenus()
        {
            try
            {
                var count = mnuShowGroups.Items.Count;
                if (count > 3)
                {
                    for (var i = count - 2; i > 1; i--)
                    {
                        var ti = mnuShowGroups.Items[i] as MenuItem;
                        if (ti != null)
                        {
                            if (ti.Items.Count > 0)
                            {
                                for (int j = ti.Items.Count - 1; j >= 0; j--)
                                {
                                    if (ti.Items[j].GetType() == typeof(MenuItem))
                                    {
                                        ((MenuItem)ti.Items[j]).Click -= menuClick;
                                    }
                                    ti.Items.RemoveAt(j);
                                }
                            }
                            else
                            {
                                ti.Click -= menuClick;
                            }
                        }
                        mnuShowGroups.Items.RemoveAt(i);
                    }
                }
                mnuShowGroups.Items.Insert(2, new Separator());
                foreach (var g in PNStatic.Groups[0].Subgroups)
                {
                    addGroupToShowHideMenu(mnuShowGroups, g, true);
                }

                count = mnuHideGroups.Items.Count;
                if (count > 3)
                {
                    for (var i = count - 2; i > 1; i--)
                    {
                        var ti = mnuHideGroups.Items[i] as MenuItem;
                        if (ti != null)
                        {
                            if (ti.Items.Count > 0)
                            {
                                for (var j = ti.Items.Count - 1; j >= 0; j--)
                                {
                                    if (ti.Items[j].GetType() == typeof(MenuItem))
                                    {
                                        ((MenuItem)ti.Items[j]).Click -= menuClick;
                                    }
                                    ti.Items.RemoveAt(j);
                                }
                            }
                            else
                            {
                                ti.Click -= menuClick;
                            }
                        }
                        mnuHideGroups.Items.RemoveAt(i);
                    }
                }
                mnuHideGroups.Items.Insert(2, new Separator());
                foreach (var g in PNStatic.Groups[0].Subgroups)
                {
                    addGroupToShowHideMenu(mnuHideGroups, g, false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createRunMenu()
        {
            try
            {
                foreach (var ti in mnuRun.Items.OfType<MenuItem>())
                {
                    ti.Click -= menuClick;
                }
                mnuRun.Items.Clear();
                foreach (
                    var ti in
                        PNStatic.Externals.Select(
                            ext => new MenuItem { Header = ext.Name, Tag = ext.Program + "|" + ext.CommandLine }))
                {
                    ti.Click += menuClick;
                    mnuRun.Items.Add(ti);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createDiaryMenu()
        {
            try
            {
                for (var i = mnuDiary.Items.Count - 1; i > 0; i--)
                {
                    ((MenuItem)mnuDiary.Items[i]).Click -= menuClick;
                    mnuDiary.Items.RemoveAt(i);
                }
                var diaries = PNStatic.Notes.Where(n => n.GroupID == (int)SpecialGroups.Diary && n.DateCreated < DateTime.Today);
                var pNotes = diaries.ToList().OrderByDescending(n => n.DateCreated);
                foreach (var ti in pNotes.Select(n => new MenuItem { Header = n.Name, Tag = n.ID }))
                {
                    ti.Click += menuClick;
                    mnuDiary.Items.Add(ti);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void menuClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var ti = sender as MenuItem;
                if (ti == null) return;
                var parent = ti.Parent as MenuItem;
                if (parent == null) return;
                switch (parent.Name)
                {
                    case "mnuRun":
                        var args = ((string)ti.Tag).Split('|');
                        PNStatic.RunExternalProgram(args[0], args[1]);
                        break;
                    case "mnuShowGroups":
                        {
                            var id = (int)ti.Tag;
                            PNNotesOperations.ShowHideSpecificGroup(id, true);
                            break;
                        }
                    case "mnuHideGroups":
                        {
                            var id = (int)ti.Tag;
                            PNNotesOperations.ShowHideSpecificGroup(id, false);
                            break;
                        }
                    case "mnuFavorites":
                        {
                            var id = (string)ti.Tag;
                            var note = PNStatic.Notes.FirstOrDefault(n => n.ID == id);
                            if (note != null)
                            {
                                PNNotesOperations.ShowHideSpecificNote(note, true);
                            }
                            break;
                        }
                    case "mnuShowByTag":
                        {
                            var notes = PNStatic.Notes.Where(n => n.Tags.Contains(ti.Header.ToString()) && n.GroupID != (int)SpecialGroups.RecycleBin);
                            foreach (var note in notes)
                            {
                                PNNotesOperations.ShowHideSpecificNote(note, true);
                            }
                            break;
                        }
                    case "mnuHideByTag":
                        {
                            var notes = PNStatic.Notes.Where(n => n.Tags.Contains(ti.Header.ToString()) && n.GroupID != (int)SpecialGroups.RecycleBin);
                            foreach (var note in notes)
                            {
                                PNNotesOperations.ShowHideSpecificNote(note, false);
                            }
                            break;
                        }
                    case "mnuDiary":
                        {
                            var id = (string)ti.Tag;
                            var note = PNStatic.Notes.FirstOrDefault(n => n.ID == id);
                            if (note != null)
                            {
                                PNNotesOperations.ShowHideSpecificNote(note, true);
                            }
                            break;
                        }
                    default:
                        var totalParent = parent.Parent as MenuItem;
                        if (totalParent != null)
                        {
                            var tag = (string)ti.Tag;

                            switch (totalParent.Name)
                            {
                                case "mnuShowGroups":
                                    {
                                        var pos = tag.IndexOf("_show", StringComparison.Ordinal);
                                        if (pos >= 0)
                                        {
                                            var id = Convert.ToInt32(tag.Substring(0, pos));
                                            PNNotesOperations.ShowHideSpecificGroup(id, true);
                                        }
                                        else
                                        {
                                            var note = PNStatic.Notes.Note(tag);
                                            if (note != null)
                                            {
                                                PNNotesOperations.ShowHideSpecificNote(note, true);
                                            }
                                        }
                                    }
                                    break;
                                case "mnuHideGroups":
                                    {
                                        var pos = tag.IndexOf("_hide", StringComparison.Ordinal);
                                        if (pos >= 0)
                                        {
                                            var id = Convert.ToInt32(tag.Substring(0, pos));
                                            PNNotesOperations.ShowHideSpecificGroup(id, false);
                                        }
                                        else
                                        {
                                            var note = PNStatic.Notes.Note(tag);
                                            if (note != null)
                                            {
                                                PNNotesOperations.ShowHideSpecificNote(note, false);
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void baloon_BaloonLinkClicked(object sender, BaloonClickedEventArgs e)
        {
            try
            {
                switch (e.Mode)
                {
                    case BaloonMode.NewVersion:
                        if (PNStatic.PrepareNewVersionCommandLine())
                        {
                            Close();
                        }
                        break;
                    case BaloonMode.NoteReceived:
                        if (PNStatic.Settings.Network.ShowReceivedOnClick)
                        {
                            foreach (var id in _ReceivedNotes)
                            {
                                PNNotesOperations.ShowHideSpecificNote(PNStatic.Notes.Note(id), true);
                            }
                        }
                        if (PNStatic.Settings.Network.ShowIncomingOnClick)
                        {
                            mnuCP.PerformClick();
                            PNStatic.FormCP.SelectSpecificGroup((int)SpecialGroups.Incoming);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNote newNote()
        {
            try
            {
                var note = new PNote();
                note.Dialog = new WndNote(note, NewNoteMode.None);
                note.Dialog.Show();
                note.Visible = true;
                //note size and location for setting other than NoteStartPosition.Center are set at loading
                if (PNStatic.Settings.Behavior.StartPosition == NoteStartPosition.Center)
                {
                    note.NoteSize = note.Dialog.GetSize();
                    note.NoteLocation = note.Dialog.GetLocation();
                }
                note.EditSize = note.Dialog.Edit.Size;

                PNStatic.Notes.Add(note);
                if (NewNoteCreated != null)
                {
                    NewNoteCreated(this, new NewNoteCreatedEventArgs(note));
                }
                subscribeToNoteEvents(note);
                return note;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void checkForNewVersion()
        {
            try
            {
                var updater = new PNUpdateChecker();
                updater.NewVersionFound += updater_PNNewVersionFound;
                updater.CheckNewVersion(System.Windows.Forms.Application.ProductVersion);
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
                var text = PNLang.Instance.GetMessageText("new_version_1",
                    "New version of PNotes.NET is available - %PLACEHOLDER1%.")
                    .Replace(PNStrings.PLACEHOLDER1, e.Version);
                var link = PNLang.Instance.GetMessageText("new_version_3",
                    "Click here in order to instal new version (restart of program is required).");
                var baloon = new Baloon(BaloonMode.NewVersion) { BaloonText = text, BaloonLink = link };
                baloon.BaloonLinkClicked += baloon_BaloonLinkClicked;
                ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, 10000);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool EnumWindowsProc(IntPtr hwnd, int lParam)
        {
            try
            {
                var sbClass = new StringBuilder(1024);
                PNInterop.GetClassName(hwnd, sbClass, sbClass.Capacity);
                if (sbClass.ToString() != _PinClass.Class) return true;
                if (!PNInterop.IsWindowVisible(hwnd)) return true;
                var count = PNInterop.GetWindowTextLength(hwnd);
                if (count <= 0) return true;
                var sb = new StringBuilder(count + 1);
                PNInterop.GetWindowText(hwnd, sb, count + 1);
                if (!Regex.IsMatch(sb.ToString(), _PinClass.Pattern)) return true;
                _PinClass.Hwnd = hwnd;
                return false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void createFullBackup(bool silent)
        {
            try
            {
                var sfd = new SaveFileDialog();
                string fileBackup;
                if (silent)
                {
                    fileBackup = Path.Combine(PNPaths.Instance.BackupDir, DateTime.Now.ToString("yyyyMMddHHmmss") + PNStrings.FULL_BACK_EXTENSION);
                }
                else
                {
                    sfd.Title = PNLang.Instance.GetCaptionText("full_backup", "Create full backup copy");
                    var sb = new StringBuilder();
                    sb.Append(PNLang.Instance.GetCaptionText("full_backup_filter", "PNotes full backup files"));
                    sb.Append(" (");
                    sb.Append("*" + PNStrings.FULL_BACK_EXTENSION);
                    sb.Append(")|");
                    sb.Append("*" + PNStrings.FULL_BACK_EXTENSION);
                    sfd.Filter = sb.ToString();
                    if (!sfd.ShowDialog(this).Value)
                    {
                        return;
                    }
                    fileBackup = sfd.FileName;
                }
                if (!PNStatic.CreateFullBackup(fileBackup)) return;
                if (silent) return;
                var message = PNLang.Instance.GetMessageText("full_backup_complete", "Full backup operation completed successfully");
                PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void restoreFromFullBackup()
        {
            var hash = "";

            try
            {
                var ofd = new OpenFileDialog
                {
                    Title = PNLang.Instance.GetCaptionText("resture_full_backup", "Restore from full backup")
                };
                var sb = new StringBuilder();
                sb.Append(PNLang.Instance.GetCaptionText("full_backup_filter", "PNotes full backup files"));
                sb.Append(" (");
                sb.Append("*" + PNStrings.FULL_BACK_EXTENSION);
                sb.Append(")|");
                sb.Append("*" + PNStrings.FULL_BACK_EXTENSION);
                ofd.Filter = sb.ToString();
                ofd.Multiselect = false;
                if (!ofd.ShowDialog(this).Value)
                {
                    return;
                }
                var packageFile = ofd.FileName;
                var message = PNLang.Instance.GetMessageText("full_restore_warning",
                    "ATTENTION! All existing notes will ber removed and replaced by notes from backup copy. Continue?");
                if (
                    PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) ==
                    MessageBoxResult.No)
                {
                    return;
                }
                // store existing files for possible crash
                if (Directory.Exists(PNPaths.Instance.TempDir))
                {
                    Directory.Delete(PNPaths.Instance.TempDir, true);
                }
                Directory.CreateDirectory(PNPaths.Instance.TempDir);
                var di = new DirectoryInfo(PNPaths.Instance.DataDir);
                var files = di.GetFiles("*" + PNStrings.NOTE_EXTENSION);
                string path;
                foreach (var f in files)
                {
                    path = Path.Combine(PNPaths.Instance.TempDir, f.Name);
                    File.Move(f.FullName, path);
                }
                var fi = new FileInfo(PNPaths.Instance.DBPath);
                path = Path.Combine(PNPaths.Instance.TempDir, fi.Name);
                File.Move(fi.FullName, path);

                // start reading package
                using (var package = Package.Open(packageFile, FileMode.Open, FileAccess.Read))
                {
                    // build parameters file URI
                    var uriData = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), new Uri("data.xml", UriKind.Relative));
                    // get parameters file
                    var documentPart = package.GetPart(uriData);
                    // load XDocument
                    var xdoc = XDocument.Load(documentPart.GetStream(), LoadOptions.None);
                    if (xdoc.Root != null)
                    {
                        var xhash = xdoc.Root.Element("hash");
                        if (xhash != null)
                        {
                            hash = xhash.Value;
                        }
                    }
                    foreach (var prs in package.GetRelationships())
                    {
                        var noteExtracted = false;
                        var uriDocumentTarget = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), prs.TargetUri);
                        documentPart = package.GetPart(uriDocumentTarget);
                        if (prs.RelationshipType.EndsWith(PNStrings.NOTE_EXTENSION))
                        {
                            path = Path.Combine(PNPaths.Instance.DataDir, prs.RelationshipType);
                            noteExtracted = true;
                        }
                        else
                        {
                            path = Path.Combine(PNPaths.Instance.DataDir, prs.RelationshipType);
                        }
                        extractPackagePart(documentPart.GetStream(), path);
                        if (!noteExtracted) continue;
                        // check whether note has been added to backup as encrypted
                        if (hash != "")
                        {
                            // decrypt note
                            using (var pne = new PNEncryptor(hash))
                            {
                                pne.DecryptTextFile(path);
                            }
                        }
                        // check whether note should be encrypted
                        if (!PNStatic.Settings.Protection.StoreAsEncrypted) continue;
                        // encrypt note
                        using (var pne = new PNEncryptor(PNStatic.Settings.Protection.PasswordString))
                        {
                            pne.EncryptTextFile(path);
                        }
                    }
                }
                reloadNotes();
                message = PNLang.Instance.GetMessageText("full_restore_complete",
                    "Restoration from full backup copy completed successfully");
                PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                if (Directory.Exists(PNPaths.Instance.TempDir))
                {
                    getFilesBack(PNPaths.Instance.TempDir);
                }
            }
            finally
            {
                if (Directory.Exists(PNPaths.Instance.TempDir))
                {
                    Directory.Delete(PNPaths.Instance.TempDir, true);
                }
            }
        }

        private void extractPackagePart(Stream source, string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                PNStatic.CopyStream(source, fileStream);
            }
        }

        private void getFilesBack(string tempFolder)
        {
            try
            {
                var di = new DirectoryInfo(tempFolder);
                var files = di.GetFiles("*" + PNStrings.NOTE_EXTENSION);
                foreach (var f in files)
                {
                    var path = Path.Combine(PNPaths.Instance.DataDir, f.Name);
                    File.Copy(f.FullName, path, true);
                }
                var dbTemp = Path.Combine(tempFolder, PNStrings.DB_FILE);
                File.Copy(dbTemp, PNPaths.Instance.DBPath, true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
            }
        }

        private void showHelp()
        {
            try
            {
                var fileCHM = Path.Combine(System.Windows.Forms.Application.StartupPath, PNStrings.CHM_FILE);
                var filePDF = Path.Combine(System.Windows.Forms.Application.StartupPath, PNStrings.PDF_FILE);
                if (File.Exists(fileCHM))
                {
                    Process.Start(fileCHM);
                }
                else if (File.Exists(filePDF))
                {
                    Process.Start(filePDF);
                }
                else
                {
                    var d = new WndHelpChooser { Owner = this };
                    d.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void reloadNotes()
        {
            try
            {
                foreach (var note in PNStatic.Notes.Where(note => note.Visible))
                {
                    note.Dialog.Close();
                }
                PNStatic.Notes.Clear();
                var controlPanel = false;
                if (PNStatic.FormCP != null)
                {
                    controlPanel = true;
                    PNStatic.FormCP.Close();
                }
                loadNotes();
                if (controlPanel)
                {
                    mnuCP.PerformClick();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool isMonitorPluggedUnplugged()
        {
            try
            {
                if (PNSingleton.Instance.MonitorsCount == Screen.AllScreens.Length) return false;
                var currentMonitorsCount = PNSingleton.Instance.MonitorsCount;
                PNSingleton.Instance.MonitorsCount = Screen.AllScreens.Length;
                if (Screen.AllScreens.Length >= currentMonitorsCount) return false;
                PNNotesOperations.RelocateAllNotesOnScreenPlug();
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void isScreenRectangleChanged()
        {
            try
            {
                var currentRect = PNStatic.AllScreensBounds();
                if (PNSingleton.Instance.ScreenRect == currentRect) return;
                PNSingleton.Instance.ScreenRect = currentRect;
                PNNotesOperations.RedockNotes();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void adjustStartTimes()
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Schedule.Type == ScheduleType.After || n.Schedule.Type == ScheduleType.RepeatEvery);
                foreach (
                    var n in
                        notes.Where(
                            n =>
                                n.Schedule.StartFrom == ScheduleStart.ProgramStart &&
                                n.Schedule.LastRun != DateTime.MinValue))
                {
                    n.Schedule.LastRun = PNStatic.StartTime;
                    PNNotesOperations.SaveNoteSchedule(n);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void showPriorityOnStart()
        {
            try
            {
                if (!PNStatic.Settings.GeneralSettings.ShowPriorityOnStart) return;
                var notes = PNStatic.Notes.Where(n => n.Priority && !n.Visible);
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

        private void loadSingleNote(string id, bool show)
        {
            try
            {
                var groups = new List<int>();
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    var sqlQuery = "SELECT * FROM NOTES WHERE ID = '" + id + "'";
                    using (var t = oData.FillDataTable(sqlQuery))
                    {
                        if (t.Rows.Count <= 0) return;
                        var note = new PNote();
                        if (!PNNotesOperations.LoadNoteProperties(note, t.Rows[0]))
                        {
                            return;
                        }
                        //load custom settings
                        sqlQuery = "SELECT * FROM CUSTOM_NOTES_SETTINGS WHERE NOTE_ID = '" + note.ID + "'";
                        using (var tc = oData.FillDataTable(sqlQuery))
                        {
                            if (tc.Rows.Count > 0)
                            {
                                PNNotesOperations.LoadNoteCustomProperties(note, tc.Rows[0]);
                            }
                        }
                        //load tags
                        PNNotesOperations.LoadNoteTags(note);
                        //load linked notes
                        PNNotesOperations.LoadLinkedNotes(note);
                        //create new note window
                        if (note.Visible && show)
                        {
                            var showNote = true;
                            var pnGroup = PNStatic.Groups.GetGroupByID(note.GroupID);
                            if (pnGroup != null && !groups.Contains(pnGroup.ID))
                            {
                                showNote = PNNotesOperations.LoginToGroup(pnGroup, ref groups);
                            }
                            if (showNote)
                            {
                                if (note.PasswordString.Trim().Length > 0)
                                {
                                    showNote &= PNNotesOperations.LoginToNote(note);
                                }
                                if (showNote)
                                {
                                    note.Dialog = new WndNote(note, note.ID, NewNoteMode.Identificator);
                                    note.Dialog.Show();
                                }
                                else
                                {
                                    note.Visible = false;
                                }
                            }
                        }
                        PNStatic.Notes.Add(note);
                        if (NewNoteCreated != null)
                        {
                            NewNoteCreated(this, new NewNoteCreatedEventArgs(note));
                        }
                        subscribeToNoteEvents(note);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadNotes()
        {
            try
            {
                var di = new DirectoryInfo(PNPaths.Instance.DataDir);
                var files = di.GetFiles("*" + PNStrings.NOTE_EXTENSION);
                foreach (var fi in files)
                {
                    loadSingleNote(Path.GetFileNameWithoutExtension(fi.Name), true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static void restoreAutosavedNotes()
        {
            try
            {
                var di = new DirectoryInfo(PNPaths.Instance.DataDir);
                var files = di.GetFiles("*" + PNStrings.NOTE_AUTO_BACK_EXTENSION);
                if (files.Length <= 0) return;
                var result = MessageBoxResult.Yes;
                if (!PNStatic.Settings.GeneralSettings.RestoreAuto)
                    result =
                        PNMessageBox.Show(
                            PNLang.Instance.GetMessageText("load_autosaved",
                                "Program did not finish correctly last time. Would you like to load autosaved notes?"),
                            PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;
                foreach (var f in files)
                {
                    File.SetAttributes(f.FullName, FileAttributes.Normal);
                    var id = Path.GetFileNameWithoutExtension(f.Name);
                    var path = Path.Combine(PNPaths.Instance.DataDir, id + PNStrings.NOTE_EXTENSION);
                    File.Copy(f.FullName, path, true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static void clearAutosavedNotes()
        {
            try
            {
                var di = new DirectoryInfo(PNPaths.Instance.DataDir);
                var files = di.GetFiles("*" + PNStrings.NOTE_AUTO_BACK_EXTENSION);
                foreach (var f in files)
                {
                    File.SetAttributes(f.FullName, FileAttributes.Normal);
                    File.Delete(f.FullName);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initDockArrows()
        {
            try
            {
                PNStatic.DockArrows.Add(DockArrow.LeftUp, new WndArrow(DockArrow.LeftUp));
                PNStatic.DockArrows.Add(DockArrow.LeftDown, new WndArrow(DockArrow.LeftDown));
                PNStatic.DockArrows.Add(DockArrow.TopLeft, new WndArrow(DockArrow.TopLeft));
                PNStatic.DockArrows.Add(DockArrow.TopRight, new WndArrow(DockArrow.TopRight));
                PNStatic.DockArrows.Add(DockArrow.RightUp, new WndArrow(DockArrow.RightUp));
                PNStatic.DockArrows.Add(DockArrow.RightDown, new WndArrow(DockArrow.RightDown));
                PNStatic.DockArrows.Add(DockArrow.BottomLeft, new WndArrow(DockArrow.BottomLeft));
                PNStatic.DockArrows.Add(DockArrow.BottomRight, new WndArrow(DockArrow.BottomRight));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initSpeller()
        {
            try
            {
                if (PNStatic.Settings.GeneralSettings.SpellDict == "") return;
                var fileDict = Path.Combine(PNPaths.Instance.DictDir, PNStatic.Settings.GeneralSettings.SpellDict);
                var fileAff = Path.Combine(PNPaths.Instance.DictDir, Path.ChangeExtension(PNStatic.Settings.GeneralSettings.SpellDict, ".aff"));
                Spellchecking.HunspellInit(fileDict, fileAff);
                if (Spellchecking.Initialized)
                {
                    Spellchecking.ColorUnderlining = PNStatic.Settings.GeneralSettings.SpellColor;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void registerMainHotkeys()
        {
            try
            {
                var keys = PNStatic.HotKeysMain.Where(h => h.VK != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.RegisterHK(_HwndSource.Handle, hk);
                }
                keys = PNStatic.HotKeysGroups.Where(h => h.VK != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.RegisterHK(_HwndSource.Handle, hk);
                }
                keys = PNStatic.HotKeysNote.Where(h => h.VK != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.RegisterHK(_HwndSource.Handle, hk);
                }
                keys = PNStatic.HotKeysEdit.Where(h => h.VK != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.RegisterHK(_HwndSource.Handle, hk);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void unregisterMainHotkeys()
        {
            try
            {
                var keys = PNStatic.HotKeysMain.Where(h => h.VK != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.UnregisterHK(_HwndSource.Handle, hk.ID);
                }
                keys = PNStatic.HotKeysGroups.Where(h => h.VK != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.UnregisterHK(_HwndSource.Handle, hk.ID);
                }
                keys = PNStatic.HotKeysNote.Where(h => h.VK != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.UnregisterHK(_HwndSource.Handle, hk.ID);
                }
                keys = PNStatic.HotKeysEdit.Where(h => h.VK != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.UnregisterHK(_HwndSource.Handle, hk.ID);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyMenuHotkey(MenuItem mi)
        {
            try
            {
                var hk = PNStatic.HotKeysMain.FirstOrDefault(h => h.MenuName == mi.Name);
                if (hk != null)
                {
                    mi.InputGestureText = hk.Shortcut;
                }
                foreach (var ti in mi.Items.OfType<MenuItem>())
                {
                    applyMenuHotkey(ti);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void prepareHotkeysTable(SQLiteDataObject oData)
        {
            var d = new WndNote();
            try
            {
                var id = PNData.HK_START;
                var o = oData.GetScalar("SELECT MAX(ID) FROM HOT_KEYS");
                if (o != null && !DBNull.Value.Equals(o))
                {
                    id = (int)(long)o + 1;
                }
                startPreparingMenuHotkeys(HotkeyType.Main, oData, ctmPN, ref id);
                startPreparingMenuHotkeys(HotkeyType.Note, oData, d.NoteMenu, ref id);
                startPreparingMenuHotkeys(HotkeyType.Edit, oData, d.EditMenu, ref id);
                startPreparingGroupsHotkeys(HotkeyType.Group, oData, ref id);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                d.Close();
            }
        }

        private void startPreparingGroupsHotkeys(HotkeyType type, SQLiteDataObject oData, ref int id)
        {
            try
            {
                //delete possible duplicates first
                var sqlQuery = "SELECT MAX(GROUP_ID) FROM GROUPS";
                var obj = oData.GetScalar(sqlQuery);
                if (obj == null || PNData.IsDBNull(obj))
                    return;
                var maxId = Convert.ToInt32(obj);
                var deleteList = new List<string>();
                sqlQuery = "SELECT MENU_NAME, ID FROM HOT_KEYS WHERE HK_TYPE = 3";
                using (var t = oData.FillDataTable(sqlQuery))
                {
                    foreach (DataRow r in t.Rows)
                    {
                        var arr = Convert.ToString(r["MENU_NAME"]).Split('_');
                        if (Convert.ToInt32(arr[0]) <= maxId) continue;
                        var sb = new StringBuilder("DELETE FROM HOT_KEYS WHERE MENU_NAME = '");
                        sb.Append(r["MENU_NAME"]);
                        sb.Append("' AND ID = ");
                        sb.Append(r["ID"]);
                        deleteList.Add(sb.ToString());
                    }
                }
                if (deleteList.Count > 0)
                {
                    PNData.ExecuteTransactionForList(deleteList, oData.ConnectionString);
                }

                sqlQuery = "SELECT MENU_NAME FROM HOT_KEYS WHERE HK_TYPE = " + ((int)type).ToString(CultureInfo.InvariantCulture);
                using (var t = oData.FillDataTable(sqlQuery))
                {
                    var names = (from DataRow r in t.Rows select (string)r[0]).ToList();
                    var group = PNStatic.Groups.GetGroupByID((int)SpecialGroups.AllGroups);
                    if (group != null)
                    {
                        foreach (var g in group.Subgroups)
                        {
                            prepareSingleGroupHotKey(g, oData, names, HotkeyType.Group, ref id);
                        }
                    }
                }
                loadSpecificHotKeys(type, oData);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void startPreparingMenuHotkeys(HotkeyType type, SQLiteDataObject oData, ContextMenu ctm, ref int id)
        {
            try
            {
                var sqlQuery = "SELECT MENU_NAME FROM HOT_KEYS WHERE HK_TYPE = " + ((int)type).ToString(CultureInfo.InvariantCulture);
                using (var t = oData.FillDataTable(sqlQuery))
                {
                    var names = (from DataRow r in t.Rows select (string)r[0]).ToList();
                    foreach (var ti in ctm.Items.OfType<MenuItem>())
                    {
                        prepareSingleMenuHotKey(ti, oData, names, type, ref id);
                    }
                }
                loadSpecificHotKeys(type, oData);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadSpecificHotKeys(HotkeyType type, SQLiteDataObject oData)
        {
            try
            {
                var sqlQuery = "SELECT MENU_NAME, ID, MODIFIERS, VK, SHORTCUT FROM HOT_KEYS WHERE HK_TYPE = " + ((int)type).ToString(CultureInfo.InvariantCulture);
                using (var t = oData.FillDataTable(sqlQuery))
                {
                    foreach (DataRow r in t.Rows)
                    {
                        switch (type)
                        {
                            case HotkeyType.Main:
                                PNStatic.HotKeysMain.Add(new PNHotKey { MenuName = (string)r["MENU_NAME"], ID = (int)r["ID"], Modifiers = (HotkeyModifiers)(int)r["MODIFIERS"], VK = (uint)(int)r["VK"], Shortcut = (string)r["SHORTCUT"], Type = type });
                                break;
                            case HotkeyType.Note:
                                PNStatic.HotKeysNote.Add(new PNHotKey { MenuName = (string)r["MENU_NAME"], ID = (int)r["ID"], Modifiers = (HotkeyModifiers)(int)r["MODIFIERS"], VK = (uint)(int)r["VK"], Shortcut = (string)r["SHORTCUT"], Type = type });
                                break;
                            case HotkeyType.Edit:
                                PNStatic.HotKeysEdit.Add(new PNHotKey { MenuName = (string)r["MENU_NAME"], ID = (int)r["ID"], Modifiers = (HotkeyModifiers)(int)r["MODIFIERS"], VK = (uint)(int)r["VK"], Shortcut = (string)r["SHORTCUT"], Type = type });
                                break;
                            case HotkeyType.Group:
                                PNStatic.HotKeysGroups.Add(new PNHotKey { MenuName = (string)r["MENU_NAME"], ID = (int)r["ID"], Modifiers = (HotkeyModifiers)(int)r["MODIFIERS"], VK = (uint)(int)r["VK"], Shortcut = (string)r["SHORTCUT"], Type = type });
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void prepareSingleGroupHotKey(PNGroup group, SQLiteDataObject oData, List<string> names, HotkeyType type, ref int id)
        {
            try
            {
                foreach (var g in group.Subgroups)
                {
                    prepareSingleGroupHotKey(g, oData, names, HotkeyType.Group, ref id);
                }
                var prefix = group.ID + "_show";
                if (names.All(n => n != prefix))
                {
                    var sqlQuery = "INSERT INTO HOT_KEYS (HK_TYPE, MENU_NAME, ID, SHORTCUT) VALUES(" + ((int)type).ToString(CultureInfo.InvariantCulture) + ",'" + prefix + "'," + id.ToString(CultureInfo.InvariantCulture) + ",'')";
                    oData.Execute(sqlQuery);
                    id++;
                }
                prefix = group.ID + "_hide";
                if (names.All(n => n != prefix))
                {
                    string sqlQuery = "INSERT INTO HOT_KEYS (HK_TYPE, MENU_NAME, ID, SHORTCUT) VALUES(" + ((int)type).ToString(CultureInfo.InvariantCulture) + ",'" + prefix + "'," + id.ToString(CultureInfo.InvariantCulture) + ",'')";
                    oData.Execute(sqlQuery);
                    id++;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void prepareSingleMenuHotKey(MenuItem mi, SQLiteDataObject oData, List<string> names, HotkeyType type, ref int id)
        {
            try
            {
                foreach (var ti in mi.Items.OfType<MenuItem>())
                {
                    prepareSingleMenuHotKey(ti, oData, names, type, ref id);
                }
                if (names.All(s => s != mi.Name))
                {
                    string sqlQuery = "INSERT INTO HOT_KEYS (HK_TYPE, MENU_NAME, ID, SHORTCUT) VALUES(" + ((int)type).ToString(CultureInfo.InvariantCulture) + ",'" + mi.Name + "'," + id.ToString(CultureInfo.InvariantCulture) + ",'" + mi.InputGestureText + "')";
                    oData.Execute(sqlQuery);
                    id++;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private CriticalUpdateAction checkCriticalUpdates()
        {
            try
            {
                _UnsubscribedCritical = false;
                var critical = new PNUpdateChecker();
                critical.CriticalUpdatesFound += critical_CriticalUpdatesFound;
                var result = critical.CheckCriticalUpdates(System.Windows.Forms.Application.ProductVersion);
                if (!_UnsubscribedCritical)
                {
                    critical.CriticalUpdatesFound -= critical_CriticalUpdatesFound;
                }
                return result;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return CriticalUpdateAction.None;
            }
        }

        void critical_CriticalUpdatesFound(object sender, CriticalUpdatesFoundEventArgs e)
        {
            try
            {
                var pnu = sender as PNUpdateChecker;
                if (pnu != null) pnu.CriticalUpdatesFound -= critical_CriticalUpdatesFound;
                _UnsubscribedCritical = true;
                var commandLinePrepared = !string.IsNullOrWhiteSpace(e.ProgramFileName) &&
                                          prepareCriticalVersionUpdateCommandLine(e.ProgramFileName);
                var pluginsXmlPrepared = e.Plugins != null && e.Plugins.Any() &&
                                         preparePreRunCriticalPluginsXml(e.Plugins);
                e.Accepted = commandLinePrepared | pluginsXmlPrepared;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool preparePreRunCriticalPluginsXml(IEnumerable<CriticalPluginUpdate> plugins)
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
                var tempDir = Path.GetTempPath();
                using (var wc = new WebClient())
                {
                    foreach (var pl in plugins)
                    {
                        var caption = PNLang.Instance.GetCaptionText("downloading", "Downloading:") + @" " + pl.ProductName;
                        PNStatic.SpTextProvider.SplashText = caption;
                        var tempFile = Path.Combine(tempDir, pl.FileName);
                        if (File.Exists(tempFile)) File.Delete(tempFile);
                        wc.DownloadFile(new Uri(PNStrings.URL_DOWNLOAD_DIR + pl.FileName), tempFile);
                        caption = PNLang.Instance.GetCaptionText("extracting", "Extracting:") + @" " + pl.ProductName;
                        PNStatic.SpTextProvider.SplashText = caption;
                        using (var zip = new ZipFile(tempFile))
                        {
                            zip.ExtractAll(tempDir, ExtractExistingFileAction.OverwriteSilently);
                        }
                        File.Delete(tempFile);

                        var fromPath = Path.Combine(Path.GetTempPath(), pl.ProductName);
                        var name = pl.ProductName;
                        var xc = new XElement(PNStrings.ELM_COPY);
                        xc.Add(new XAttribute(PNStrings.ATT_NAME, name));
                        xc.Add(new XAttribute(PNStrings.ATT_FROM, fromPath));
                        xc.Add(new XAttribute(PNStrings.ATT_TO,
                            Path.Combine(PNPaths.Instance.PluginsDir, pl.ProductName)));
                        xc.Add(new XAttribute(PNStrings.ATT_DEL_DIR, "false"));
                        xc.Add(new XAttribute(PNStrings.ATT_IS_CRITICAL, "true"));
                        xcopies.Add(xc);
                    }
                }
                if (addCopy)
                {
                    xroot.Add(xcopies);
                }
                if (xdoc.Root == null)
                    xdoc.Add(xroot);
                xdoc.Save(filePreRun);
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool prepareCriticalVersionUpdateCommandLine(string updateZip)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("0 ");
                sb.Append("\"");
                sb.Append(PNLang.Instance.GetCaptionText("update_progress", "Update in progress..."));
                sb.Append(",");
                sb.Append(PNLang.Instance.GetCaptionText("downloading", "Downloading:"));
                sb.Append(",");
                sb.Append(PNLang.Instance.GetCaptionText("extracting", "Extracting:"));
                sb.Append(",");
                sb.Append(PNLang.Instance.GetCaptionText("copying", "Coping:"));
                sb.Append("\" \"");
                sb.Append(PNStrings.URL_DOWNLOAD_DIR);
                sb.Append("\" \"");
                sb.Append(System.Windows.Forms.Application.ExecutablePath);
                sb.Append("\" \"");
                sb.Append("\" \"");
                sb.Append(System.Windows.Forms.Application.StartupPath);
                sb.Append("\" \"");
                sb.Append(updateZip);
                sb.Append("\"");

                PNSingleton.Instance.UpdaterCommandLine = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void loadDatabase()
        {
            try
            {
                if (!File.Exists(PNPaths.Instance.DBPath))
                {
                    PNStatic.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("create_database", "Creating database");
                    _FirstRun = true;
                }
                else
                {
                    PNStatic.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("load_database", "Loading database");
                }
                PNData.ConnectionString = SQLiteDataObject.CheckAndCreateDatabase(PNPaths.Instance.DBPath);
                if (PNData.ConnectionString != "")
                {
                    using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                    {
                        //notes
                        var sqlQuery = "CREATE TABLE IF NOT EXISTS [NOTES] ([ID] TEXT PRIMARY KEY NOT NULL, [NAME] TEXT NOT NULL, [GROUP_ID] INT NOT NULL, [PREV_GROUP_ID] INT, [OPACITY] REAL, [VISIBLE] BOOLEAN, [FAVORITE] BOOLEAN, [PROTECTED] BOOLEAN, [COMPLETED] BOOLEAN, [PRIORITY] BOOLEAN, [PASSWORD_STRING] TEXT, [PINNED] BOOLEAN, [TOPMOST] BOOLEAN, [ROLLED] BOOLEAN, [DOCK_STATUS] INT, [DOCK_ORDER] INT, [SEND_RECEIVE_STATUS] INT, [DATE_CREATED] TEXT, [DATE_SAVED] TEXT, [DATE_SENT] TEXT, [DATE_RECEIVED] TEXT, [DATE_DELETED] TEXT, [SIZE] TEXT, [LOCATION] TEXT, [EDIT_SIZE] TEXT, [REL_X] REAL, [REL_Y] REAL, [UPD_DATE] TEXT DEFAULT(strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' )), [SENT_TO] TEXT, [RECEIVED_FROM] TEXT, [PIN_CLASS] TEXT, [PIN_TEXT] TEXT, [SCRAMBLED] BOOLEAN DEFAULT (0), [THUMBNAIL] BOOLEAN DEFAULT (0), [RECEIVED_IP] TEXT)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.GetSchema("Columns"))
                        {
                            var rows = t.Select("COLUMN_NAME = 'SCRAMBLED' AND TABLE_NAME = 'NOTES'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NOTES ADD COLUMN [SCRAMBLED] BOOLEAN DEFAULT (0)";
                                oData.Execute(sqlQuery);
                                sqlQuery = "UPDATE NOTES SET SCRAMBLED = 0";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'THUMBNAIL' AND TABLE_NAME = 'NOTES'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NOTES ADD COLUMN [THUMBNAIL] BOOLEAN DEFAULT (0)";
                                oData.Execute(sqlQuery);
                                sqlQuery = "UPDATE NOTES SET THUMBNAIL = 0";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'RECEIVED_IP' AND TABLE_NAME = 'NOTES'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NOTES ADD COLUMN [RECEIVED_IP] TEXT";
                                oData.Execute(sqlQuery);
                                sqlQuery = "UPDATE NOTES SET THUMBNAIL = 0";
                                oData.Execute(sqlQuery);
                            }
                        }
                        if (!PNSingleton.Instance.PlatformChanged)
                        {
                            //custom notes settings
                            sqlQuery =
                                "CREATE TABLE IF NOT EXISTS [CUSTOM_NOTES_SETTINGS] ([NOTE_ID] TEXT NOT NULL, [BACK_COLOR] TEXT, [CAPTION_FONT_COLOR] TEXT, [CAPTION_FONT] TEXT, [SKIN_NAME] TEXT, [CUSTOM_OPACITY] BOOLEAN, [UPD_DATE] TEXT DEFAULT(strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' )))";
                            oData.Execute(sqlQuery);
                        }
                        else
                        {
                            if (oData.TableExists("CUSTOM_NOTES_SETTINGS"))
                            {
                                PNData.NormalizeCustomNotesTable(oData);
                            }
                            else
                            {
                                sqlQuery =
                                    "CREATE TABLE [CUSTOM_NOTES_SETTINGS] ([NOTE_ID] TEXT NOT NULL, [BACK_COLOR] TEXT, [CAPTION_FONT_COLOR] TEXT, [CAPTION_FONT] TEXT, [SKIN_NAME] TEXT, [CUSTOM_OPACITY] BOOLEAN, [UPD_DATE] TEXT DEFAULT(strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' )))";
                                oData.Execute(sqlQuery);
                            }
                        }
                        //notes tags
                        sqlQuery = "CREATE TABLE IF NOT EXISTS [NOTES_TAGS] ([NOTE_ID] TEXT NOT NULL, [TAG] TEXT NOT NULL, [UPD_DATE] TEXT DEFAULT(strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' )))";
                        oData.Execute(sqlQuery);
                        //notes schedule
                        sqlQuery = "CREATE TABLE IF NOT EXISTS [NOTES_SCHEDULE] ([NOTE_ID] TEXT NOT NULL, [SCHEDULE_TYPE] INT, [ALARM_DATE] TEXT, [START_DATE] TEXT, [LAST_RUN] TEXT, [SOUND] TEXT, [STOP_AFTER] INT, [TRACK] BOOLEAN, [REPEAT_COUNT] INT, [SOUND_IN_LOOP] BOOLEAN, [USE_TTS] BOOLEAN, [START_FROM] INT, [MONTH_DAY] TEXT, [ALARM_AFTER] TEXT, [WEEKDAYS] TEXT, [UPD_DATE] TEXT DEFAULT(strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' )), [PROG_TO_RUN] TEXT, [CLOSE_ON_NOTIFICATION] BOOLEAN)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.GetSchema("Columns"))
                        {
                            var rows = t.Select("COLUMN_NAME = 'PROG_TO_RUN' AND TABLE_NAME = 'NOTES_SCHEDULE'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NOTES_SCHEDULE ADD COLUMN [PROG_TO_RUN] TEXT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'CLOSE_ON_NOTIFICATION' AND TABLE_NAME = 'NOTES_SCHEDULE'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NOTES_SCHEDULE ADD COLUMN [CLOSE_ON_NOTIFICATION] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'MULTI_ALERTS' AND TABLE_NAME = 'NOTES_SCHEDULE'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NOTES_SCHEDULE ADD COLUMN [MULTI_ALERTS] TEXT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'TIME_ZONE' AND TABLE_NAME = 'NOTES_SCHEDULE'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NOTES_SCHEDULE ADD COLUMN [TIME_ZONE] TEXT";
                                oData.Execute(sqlQuery);
                            }
                        }
                        //linked notes
                        sqlQuery = "CREATE TABLE IF NOT EXISTS [LINKED_NOTES] ([NOTE_ID] TEXT NOT NULL, [LINK_ID] TEXT NOT NULL, [UPD_DATE] TEXT DEFAULT(strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' )))";
                        oData.Execute(sqlQuery);
                        //groups
                        if (!oData.TableExists("GROUPS"))
                        {
                            sqlQuery =
                                "CREATE TABLE [GROUPS] ([GROUP_ID] INT PRIMARY KEY NOT NULL UNIQUE, [PARENT_ID] INT, [GROUP_NAME] TEXT NOT NULL, [ICON] TEXT, [BACK_COLOR] TEXT NOT NULL, [CAPTION_FONT_COLOR] TEXT NOT NULL, [CAPTION_FONT] TEXT NOT NULL, [SKIN_NAME] TEXT NOT NULL, [PASSWORD_STRING] TEXT, [FONT] TEXT, [FONT_COLOR] TEXT, [UPD_DATE] TEXT DEFAULT(strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' )), [IS_DEFAULT_IMAGE] BOOLEAN)";
                            oData.Execute(sqlQuery);
                            var sb = new StringBuilder();

                            sb.Append(prepareGroupInsert((int)SpecialGroups.AllGroups, (int)SpecialGroups.DummyGroup,
                                "All groups", "gr_all.png"));
                            sb.Append(prepareGroupInsert(0, (int)SpecialGroups.AllGroups, "General", "gr.png"));
                            sb.Append(prepareGroupInsert((int)SpecialGroups.RecycleBin, (int)SpecialGroups.DummyGroup,
                                "Recycle Bin", "gr_bin.png"));
                            sb.Append(prepareGroupInsert((int)SpecialGroups.Diary, (int)SpecialGroups.DummyGroup,
                                "Diary", "gr_diary.png"));
                            sb.Append(prepareGroupInsert((int)SpecialGroups.SearchResults,
                                (int)SpecialGroups.DummyGroup, "Search Results", "gr_search.png"));
                            sb.Append(prepareGroupInsert((int)SpecialGroups.Backup, (int)SpecialGroups.DummyGroup,
                                "Backup", "gr_back.png"));
                            sb.Append(prepareGroupInsert((int)SpecialGroups.Favorites, (int)SpecialGroups.DummyGroup,
                                "Favorites", "gr_fav.png"));
                            sb.Append(prepareGroupInsert((int)SpecialGroups.Incoming, (int)SpecialGroups.DummyGroup,
                                "Incoming", "gr_inc.png"));
                            sb.Append(prepareGroupInsert((int)SpecialGroups.Docking, (int)SpecialGroups.DummyGroup,
                                "Docking", "dockall.png"));

                            sqlQuery = sb.ToString();
                            oData.Execute(sqlQuery);
                        }
                        else
                        {
                            using (var t = oData.GetSchema("Columns"))
                            {
                                var rows = t.Select("COLUMN_NAME = 'IS_DEFAULT_IMAGE' AND TABLE_NAME = 'GROUPS'");
                                if (rows.Length == 0)
                                {
                                    sqlQuery = "ALTER TABLE GROUPS ADD COLUMN [IS_DEFAULT_IMAGE] BOOLEAN";
                                    oData.Execute(sqlQuery);
                                    sqlQuery = "UPDATE GROUPS SET IS_DEFAULT_IMAGE = 1";
                                    oData.Execute(sqlQuery);
                                }
                            }
                            if (PNSingleton.Instance.PlatformChanged)
                            {
                                PNData.NormalizeGroupsTable(oData, false);
                                upgradeGroups(oData);
                            }
                        }
                        var groupsWithoutParent = new List<PNGroup>();

                        sqlQuery = "SELECT GROUP_ID, PARENT_ID, GROUP_NAME, ICON, BACK_COLOR, CAPTION_FONT_COLOR, CAPTION_FONT, SKIN_NAME, PASSWORD_STRING, FONT, FONT_COLOR, IS_DEFAULT_IMAGE FROM GROUPS ORDER BY GROUP_ID ASC";
                        using (var t = oData.FillDataTable(sqlQuery))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                if ((int)r["GROUP_ID"] < 0)
                                {
                                    if ((int)r["GROUP_ID"] != (int)SpecialGroups.Docking)
                                    {
                                        var group = new PNGroup();
                                        fillGroup(group, r);
                                        PNStatic.Groups.Insert(0, group);
                                    }
                                    else
                                    {
                                        fillGroup(PNStatic.Docking, r);
                                    }
                                }
                                else
                                {
                                    var gr = new PNGroup();
                                    fillGroup(gr, r);

                                    var parentExists = PNStatic.Groups.Select(grp => grp.GetGroupById(gr.ParentID)).Any(pg => pg != null);
                                    if (!parentExists)
                                    {
                                        groupsWithoutParent.Add(gr);
                                        continue;
                                    }

                                    foreach (var parent in PNStatic.Groups.Select(g => g.GetGroupById(gr.ParentID)).Where(parent => parent != null))
                                    {
                                        parent.Subgroups.Add(gr);
                                        break;
                                    }
                                }
                            }
                        }
                        while (groupsWithoutParent.Count > 0)
                        {
                            for (var i = groupsWithoutParent.Count - 1; i >= 0; i--)
                            {
                                if (PNStatic.Groups.Select(grp => grp.GetGroupById(groupsWithoutParent[i].ParentID)).All(pg => pg == null))
                                    continue;
                                var i1 = i;
                                foreach (
                                    var parent in
                                        PNStatic.Groups.Select(g => g.GetGroupById(groupsWithoutParent[i1].ParentID))
                                            .Where(parent => parent != null))
                                {
                                    parent.Subgroups.Add(groupsWithoutParent[i]);
                                    groupsWithoutParent.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                        applyStandardGroupsNames();

                        //sync comps
                        sqlQuery = "CREATE TABLE IF NOT EXISTS [SYNC_COMPS] ([COMP_NAME] TEXT NOT NULL, [DATA_DIR] TEXT NOT NULL, [DB_DIR] TEXT NOT NULL, [USE_DATA_DIR] TEXT NOT NULL)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT * FROM SYNC_COMPS"))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNStatic.SyncComps.Add(new PNSyncComp
                                {
                                    CompName = (string)r["COMP_NAME"],
                                    DataDir = (string)r["DATA_DIR"],
                                    DBDir = (string)r["DB_DIR"],
                                    UseDataDir = bool.Parse((string)r["USE_DATA_DIR"])
                                });
                            }
                        }
                        //contact groups
                        sqlQuery = "CREATE TABLE IF NOT EXISTS [CONTACT_GROUPS] ([ID] INT PRIMARY KEY ON CONFLICT REPLACE NOT NULL UNIQUE ON CONFLICT REPLACE, [GROUP_NAME] TEXT NOT NULL UNIQUE ON CONFLICT REPLACE)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT * FROM CONTACT_GROUPS"))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNStatic.ContactGroups.Add(new PNContactGroup { ID = (int)r["ID"], Name = (string)r["GROUP_NAME"] });
                            }
                        }
                        //contacts
                        sqlQuery = "CREATE TABLE IF NOT EXISTS [CONTACTS] ([ID] INT PRIMARY KEY ON CONFLICT REPLACE NOT NULL UNIQUE ON CONFLICT REPLACE, [GROUP_ID] INT NOT NULL, [CONTACT_NAME] TEXT NOT NULL UNIQUE ON CONFLICT REPLACE, [COMP_NAME] TEXT NOT NULL, [IP_ADDRESS] TEXT NOT NULL, [USE_COMP_NAME] BOOLEAN NOT NULL)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT ID, GROUP_ID, CONTACT_NAME, COMP_NAME, IP_ADDRESS, USE_COMP_NAME FROM CONTACTS"))
                        {
                            foreach (var cont in from DataRow r in t.Rows
                                                 select new PNContact
                                                 {
                                                     Name = (string)r["CONTACT_NAME"],
                                                     ComputerName = (string)r["COMP_NAME"],
                                                     IpAddress = (string)r["IP_ADDRESS"],
                                                     GroupID = (int)r["GROUP_ID"],
                                                     UseComputerName = (bool)r["USE_COMP_NAME"],
                                                     ID = (int)r["ID"]
                                                 })
                            {
                                PNStatic.Contacts.Add(cont);
                            }
                            Task.Factory.StartNew(updateContactsWithoutIp);
                        }
                        //externals
                        sqlQuery = "CREATE TABLE IF NOT EXISTS [EXTERNALS] ([EXT_NAME] TEXT PRIMARY KEY ON CONFLICT REPLACE NOT NULL UNIQUE ON CONFLICT REPLACE, [PROGRAM] TEXT NOT NULL, [COMMAND_LINE] TEXT NOT NULL)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT EXT_NAME, PROGRAM, COMMAND_LINE FROM EXTERNALS "))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNStatic.Externals.Add(new PNExternal
                                {
                                    Name = (string)r["EXT_NAME"],
                                    Program = (string)r["PROGRAM"],
                                    CommandLine = (string)r["COMMAND_LINE"]
                                });
                            }
                        }
                        //search providers
                        if (!oData.TableExists("SEARCH_PROVIDERS"))
                        {
                            sqlQuery = "CREATE TABLE [SEARCH_PROVIDERS] ([SP_NAME] TEXT PRIMARY KEY ON CONFLICT REPLACE NOT NULL UNIQUE ON CONFLICT REPLACE, [SP_QUERY] TEXT NOT NULL)";
                            oData.Execute(sqlQuery);
                            //insert two default provider for the first time
                            sqlQuery = "INSERT INTO SEARCH_PROVIDERS VALUES('Google', 'http://www.google.com/search?q=')";
                            oData.Execute(sqlQuery);
                            sqlQuery = "INSERT INTO SEARCH_PROVIDERS VALUES('Yahoo', 'http://search.yahoo.com/search?p=')";
                            oData.Execute(sqlQuery);
                        }
                        using (var t = oData.FillDataTable("SELECT * FROM SEARCH_PROVIDERS"))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNStatic.SearchProviders.Add(new PNSearchProvider
                                {
                                    Name = (string)r["SP_NAME"],
                                    QueryString = (string)r["SP_QUERY"]
                                });
                            }
                        }
                        //tags
                        sqlQuery = "CREATE TABLE IF NOT EXISTS [TAGS] ([TAG] TEXT PRIMARY KEY ON CONFLICT REPLACE NOT NULL UNIQUE ON CONFLICT REPLACE)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT * FROM TAGS"))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNStatic.Tags.Add((string)r["TAG"]);
                            }
                        }
                        //plugins
                        sqlQuery = "CREATE TABLE IF NOT EXISTS [POST_PLUGINS] ([PLUGIN] TEXT PRIMARY KEY NOT NULL UNIQUE)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT * FROM POST_PLUGINS"))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNStatic.PostPlugins.Add((string)r["PLUGIN"]);
                            }
                        }
                        sqlQuery = "CREATE TABLE IF NOT EXISTS [SYNC_PLUGINS] ([PLUGIN] TEXT PRIMARY KEY NOT NULL UNIQUE)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT * FROM SYNC_PLUGINS"))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNStatic.SyncPlugins.Add((string)r["PLUGIN"]);
                            }
                        }
                        //hotkeys
                        sqlQuery = "CREATE TABLE IF NOT EXISTS [HOT_KEYS] ([HK_TYPE] INT NOT NULL, [MENU_NAME] TEXT NOT NULL, [ID] INT NOT NULL DEFAULT(0), [MODIFIERS] INT NOT NULL DEFAULT(0), [VK] INT NOT NULL DEFAULT(0), [SHORTCUT] TEXT NOT NULL DEFAULT(''), PRIMARY KEY ([HK_TYPE], [MENU_NAME]))";
                        oData.Execute(sqlQuery);
                        prepareHotkeysTable(oData);
                        //find/replace
                        if (!oData.TableExists("FIND_REPLACE"))
                        {
                            sqlQuery = "CREATE TABLE [FIND_REPLACE] ([FIND] TEXT, [REPLACE] TEXT); ";
                            sqlQuery += "INSERT INTO FIND_REPLACE VALUES(NULL, NULL)";
                        }
                        oData.Execute(sqlQuery);
                        //remove possible program version table from previous versions
                        if (oData.TableExists("PROGRAM_VERSION"))
                        {
                            sqlQuery = "DROP TABLE [PROGRAM_VERSION]; ";
                            oData.Execute(sqlQuery);
                        }
                        //hidden menus
                        sqlQuery = "CREATE TABLE IF NOT EXISTS [HIDDEN_MENUS] ([MENU_NAME] TEXT NOT NULL, [MENU_TYPE] INT NOT NULL, PRIMARY KEY ([MENU_NAME], [MENU_TYPE]))";
                        oData.Execute(sqlQuery);
                        PNData.LoadHiddenMenus();
                        //menus order
                        if (!oData.TableExists("MENUS_ORDER"))
                        {
                            sqlQuery =
                                "CREATE TABLE [MENUS_ORDER] ([CONTEXT_NAME] TEXT NOT NULL, [MENU_NAME] TEXT NOT NULL, [ORDER_ORIGINAL] INT NOT NULL, [ORDER_NEW] INT NOT NULL, [PARENT_NAME] TEXT, PRIMARY KEY ( [CONTEXT_NAME], [MENU_NAME]));";
                            oData.Execute(sqlQuery);
                            createMenusOrder(oData, true);
                        }
                        else
                        {
                            createMenusOrder(oData, false);
                        }
                        //SMTP profiles
                        sqlQuery =
                            "CREATE TABLE IF NOT EXISTS [SMTP_PROFILES] ([ID] INT UNIQUE ON CONFLICT REPLACE, [ACTIVE] BOOLEAN, [HOST_NAME] TEXT, [DISPLAY_NAME] TEXT, [SENDER_ADDRESS] TEXT PRIMARY KEY UNIQUE, [PASSWORD] TEXT, [PORT] INT )";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT * FROM SMTP_PROFILES"))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNStatic.SmtpProfiles.Add(new PNSmtpProfile
                                {
                                    Active = Convert.ToBoolean(r["ACTIVE"]),
                                    Id = Convert.ToInt32(r["ID"]),
                                    HostName = Convert.ToString(r["HOST_NAME"]),
                                    SenderAddress = Convert.ToString(r["SENDER_ADDRESS"]),
                                    Password = Convert.ToString(r["PASSWORD"]),
                                    Port = Convert.ToInt32(r["PORT"]),
                                    DisplayName = Convert.ToString(r["DISPLAY_NAME"])
                                });
                            }
                        }
                        //mail contacts
                        sqlQuery = "CREATE TABLE IF NOT EXISTS [MAIL_CONTACTS] ([ID] INT UNIQUE ON CONFLICT REPLACE, [DISPLAY_NAME] TEXT, [ADDRESS] TEXT )";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT * FROM MAIL_CONTACTS"))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNStatic.MailContacts.Add(new PNMailContact
                                {
                                    DisplayName = Convert.ToString(r["DISPLAY_NAME"]),
                                    Address = Convert.ToString(r["ADDRESS"]),
                                    Id = Convert.ToInt32(r["ID"]),
                                });
                            }
                        }
                        //services
                        sqlQuery =
                            "CREATE TABLE IF NOT EXISTS SERVICES ( APP_NAME TEXT PRIMARY KEY, CLIENT_ID TEXT, CLIENT_SECRET TEXT, ACCESS_TOKEN TEXT, REFRESH_TOKEN TEXT, TOKEN_EXPIRY TEXT )";
                        oData.Execute(sqlQuery);
                        prepareServices(oData);
                        //triggers
                        sqlQuery = PNStrings.CREATE_TRIGGERS;
                        oData.Execute(sqlQuery);
                    }
                }
                //load plugins
                PNPlugins.Instance.LoadPlugins(this, PNPaths.Instance.PluginsDir);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updateContactsWithoutIp()
        {
            //check whether ip address appears for each contact
            try
            {
                foreach (
                    var cn in
                        PNStatic.Contacts.Where(c => c.UseComputerName && string.IsNullOrEmpty(c.IpAddress))
                            .Where(PNStatic.SetContactIpAddress))
                {
                    PNData.UpdateContact(cn);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void upgradeGroupIcon(PNGroup gr, int groupId, string iconString)
        {
            try
            {
                var sb = new StringBuilder("UPDATE GROUPS SET ICON = ");
                string groupIcon;
                switch (groupId)
                {
                    case (int)SpecialGroups.AllGroups:
                        groupIcon = "gr_all.png";
                        break;
                    case 0:
                        groupIcon = "gr.png";
                        break;
                    case (int)SpecialGroups.Diary:
                        groupIcon = "gr_diary.png";
                        break;
                    case (int)SpecialGroups.SearchResults:
                        groupIcon = "gr_search.png";
                        break;
                    case (int)SpecialGroups.Backup:
                        groupIcon = "gr_back.png";
                        break;
                    case (int)SpecialGroups.Favorites:
                        groupIcon = "gr_fav.png";
                        break;
                    case (int)SpecialGroups.Incoming:
                        groupIcon = "gr_inc.png";
                        break;
                    case (int)SpecialGroups.Docking:
                        groupIcon = "dockall.png";
                        break;
                    case (int)SpecialGroups.RecycleBin:
                        groupIcon = "gr_bin.png";
                        break;
                    default:
                        if (iconString.StartsWith("resource."))
                        {
                            groupIcon = iconString.Substring("resource.".Length);
                            groupIcon += ".png";
                        }
                        else
                        {
                            return;
                        }
                        break;
                }
                sb.Append("'");
                sb.Append(groupIcon);
                sb.Append("', IS_DEFAULT_IMAGE = 1");
                sb.Append(" WHERE GROUP_ID = ");
                sb.Append(groupId);
                PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                gr.Image = TryFindResource(Path.GetFileNameWithoutExtension(groupIcon)) as BitmapImage;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void upgradeGroups(SQLiteDataObject oData)
        {
            try
            {
                var list = new List<string>();
                using (
                    var t =
                        oData.FillDataTable(
                            "SELECT GROUP_ID, ICON FROM GROUPS"))
                {
                    foreach (DataRow r in t.Rows)
                    {
                        var sb = new StringBuilder("UPDATE GROUPS SET ");
                        var isDefImage = 1;
                        switch (Convert.ToInt32(r["GROUP_ID"]))
                        {
                            case (int)SpecialGroups.AllGroups:
                                sb.Append(" ICON = 'gr_all.png', ");
                                break;
                            case 0:
                                sb.Append(" ICON = 'gr.png', ");
                                break;
                            case (int)SpecialGroups.Diary:
                                sb.Append(" ICON = 'gr_diary.png', ");
                                break;
                            case (int)SpecialGroups.SearchResults:
                                sb.Append(" ICON = 'gr_search.png', ");
                                break;
                            case (int)SpecialGroups.Backup:
                                sb.Append(" ICON = 'gr_back.png', ");
                                break;
                            case (int)SpecialGroups.Favorites:
                                sb.Append(" ICON = 'gr_fav.png', ");
                                break;
                            case (int)SpecialGroups.Incoming:
                                sb.Append(" ICON = 'gr_inc.png', ");
                                break;
                            case (int)SpecialGroups.Docking:
                                sb.Append(" ICON = 'dockall.png', ");
                                break;
                            case (int)SpecialGroups.RecycleBin:
                                sb.Append(" ICON = 'gr_bin.png', ");
                                break;
                            default:
                                var iconString = Convert.ToString(r["ICON"]);
                                if (iconString.StartsWith("resource."))
                                {
                                    var imageName = iconString.Substring("resource.".Length);
                                    imageName = imageName + ".png";
                                    sb.Append(" ICON = '");
                                    sb.Append(imageName);
                                    sb.Append("', ");
                                }
                                else
                                {
                                    isDefImage = 0;
                                }
                                break;
                        }
                        sb.Append(" IS_DEFAULT_IMAGE = ");
                        sb.Append(isDefImage);
                        sb.Append(" WHERE GROUP_ID = ");
                        sb.Append(r["GROUP_ID"]);
                        list.Add(sb.ToString());
                    }
                }
                PNData.ExecuteTransactionForList(list, oData.ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void prepareServices(SQLiteDataObject oData)
        {
            try
            {
                //google data
                var obj = oData.GetScalar("SELECT COUNT(APP_NAME) FROM SERVICES WHERE APP_NAME = 'PNContactsLoader'");
                if (obj == null || PNData.IsDBNull(obj) || Convert.ToInt32(obj) == 0)
                {
                    oData.Execute(
                        "INSERT INTO SERVICES (APP_NAME, CLIENT_ID, CLIENT_SECRET) VALUES('PNContactsLoader', '924809411382-kn585uq0fptek2kduvm6jj85cpdgjj5v.apps.googleusercontent.com','rY40+ZKA34bL+1wExbD3gxSEZTO8DE/iPmzHN/+qOUU=')");
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private string buildUpdateMenuString(SQLiteDataObject oData, string contextName, string menuName, int index)
        {
            try
            {
                var sb = new StringBuilder("SELECT ORDER_ORIGINAL FROM MENUS_ORDER WHERE CONTEXT_NAME = '");
                sb.Append(contextName);
                sb.Append("' AND MENU_NAME = '");
                sb.Append(menuName);
                sb.Append("'");
                var obj = oData.GetScalar(sb.ToString());
                if (obj != null && !PNData.IsDBNull(obj))
                {
                    if (Convert.ToInt32(obj) == index)
                        return "";
                }
                sb = new StringBuilder("UPDATE MENUS_ORDER SET ORDER_ORIGINAL = ");
                sb.Append(index);
                sb.Append(" WHERE CONTEXT_NAME = '");
                sb.Append(contextName);
                sb.Append("' AND MENU_NAME = '");
                sb.Append(menuName);
                sb.Append("'");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private void newMenusOrder(SQLiteDataObject oData, string contextName, Control item, Control parent,
            int index, IEnumerable<string> names, List<string> updateList)
        {
            try
            {
                var tmi = item as MenuItem;
                if (tmi == null)
                {
                    var sep = item as Separator;
                    if (sep == null) return;
                }

                var enumerable = names as string[] ?? names.ToArray();
                if (enumerable.All(n => n != item.Name))
                    insertMenusOrder(oData, contextName, item, parent, index);
                else
                    updateList.Add(buildUpdateMenuString(oData, contextName, item.Name, index));

                if (tmi == null) return;
                foreach (var ti in tmi.Items.OfType<Control>())
                    newMenusOrder(oData, contextName, ti, tmi, tmi.Items.IndexOf(ti), enumerable, updateList);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void insertMenusOrder(SQLiteDataObject oData, string contextName, Control item,
            object parent, int index)
        {
            try
            {
                var tmi = item as MenuItem;
                if (tmi == null)
                {
                    var sep = item as Separator;
                    if (sep == null) return;
                }

                var sb =
                    new StringBuilder(
                        "INSERT INTO MENUS_ORDER (CONTEXT_NAME, MENU_NAME, ORDER_ORIGINAL, ORDER_NEW, PARENT_NAME) VALUES('");
                sb.Append(contextName);
                sb.Append("', '");
                sb.Append(item.Name);
                sb.Append("', ");
                sb.Append(index);
                sb.Append(", ");
                sb.Append(index);
                sb.Append(", ");

                if (parent == null)
                {
                    sb.Append("'')");
                }
                else
                {
                    sb.Append("'");
                    var prItem = parent as Control;
                    if (prItem != null)
                        sb.Append(prItem.Name);
                    else if (parent is string)
                        sb.Append(parent);
                    else
                        return;
                    sb.Append("')");
                }
                oData.Execute(sb.ToString());

                sb =
                    new StringBuilder("UPDATE MENUS_ORDER SET ORDER_NEW = ORDER_NEW + 1 WHERE ORDER_NEW >= ");
                sb.Append(index);
                sb.Append(" AND CONTEXT_NAME = '");
                sb.Append(contextName);
                sb.Append("' AND MENU_NAME <> '");
                sb.Append(item.Name);
                sb.Append("' AND PARENT_NAME = ");
                if (parent == null)
                {
                    sb.Append("''");
                }
                else
                {
                    sb.Append("'");
                    var prItem = parent as Control;
                    if (prItem != null)
                        sb.Append(prItem.Name);
                    else sb.Append(parent);
                    sb.Append("'");
                }
                oData.Execute(sb.ToString());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addMenusOrder(SQLiteDataObject oData, string contextName, Control item, object parent, int index)
        {
            try
            {
                insertMenusOrder(oData, contextName, item, parent, index);

                var tmi = item as MenuItem;
                if (tmi == null) return;
                foreach (var ti in tmi.Items.OfType<Control>())
                    addMenusOrder(oData, contextName, ti, tmi, tmi.Items.IndexOf(ti));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createMenusOrder(SQLiteDataObject oData, bool firstTime)
        {
            var dn = new WndNote();
            var dcp = new WndCP(false);
            try
            {
                var ctmNote = dn.NoteMenu;
                var ctmEdit = dn.EditMenu;
                var ctmList = dcp.CPMenu;

                if (firstTime)
                {
                    PNStatic.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("load_menus", "Preparing menus order table");
                    foreach (var ti in ctmPN.Items.OfType<Control>())
                    {
                        addMenusOrder(oData, ctmPN.Name, ti, null, ctmPN.Items.IndexOf(ti));
                    }
                    foreach (var ti in ctmNote.Items.OfType<Control>())
                    {
                        addMenusOrder(oData, ctmNote.Name, ti, null, ctmNote.Items.IndexOf(ti));
                    }
                    foreach (var ti in ctmEdit.Items.OfType<Control>())
                    {
                        addMenusOrder(oData, ctmEdit.Name, ti, null, ctmEdit.Items.IndexOf(ti));
                    }
                    foreach (var ti in ctmList.Items.OfType<Control>())
                    {
                        addMenusOrder(oData, ctmList.Name, ti, null, ctmList.Items.IndexOf(ti));
                    }
                }
                else
                {
                    var updateList = new List<string>();
                    const string SQL = "SELECT MENU_NAME FROM MENUS_ORDER WHERE CONTEXT_NAME = '%CONTEXT_NAME%'";
                    using (var t = oData.FillDataTable(SQL.Replace("%CONTEXT_NAME%", ctmPN.Name)))
                    {
                        var names = t.AsEnumerable().Select(r => Convert.ToString(r["MENU_NAME"]));
                        foreach (var ti in ctmPN.Items.OfType<Control>())
                        {
                            newMenusOrder(oData, ctmPN.Name, ti, null, ctmPN.Items.IndexOf(ti), names, updateList);
                        }
                    }
                    using (var t = oData.FillDataTable(SQL.Replace("%CONTEXT_NAME%", ctmNote.Name)))
                    {
                        var names = t.AsEnumerable().Select(r => Convert.ToString(r["MENU_NAME"]));
                        foreach (var ti in ctmNote.Items.OfType<Control>())
                        {
                            newMenusOrder(oData, ctmNote.Name, ti, null, ctmNote.Items.IndexOf(ti), names, updateList);
                        }
                    }
                    using (var t = oData.FillDataTable(SQL.Replace("%CONTEXT_NAME%", ctmEdit.Name)))
                    {
                        var names = t.AsEnumerable().Select(r => Convert.ToString(r["MENU_NAME"]));
                        foreach (var ti in ctmEdit.Items.OfType<Control>())
                        {
                            newMenusOrder(oData, ctmEdit.Name, ti, null, ctmEdit.Items.IndexOf(ti), names, updateList);
                        }
                    }
                    using (var t = oData.FillDataTable(SQL.Replace("%CONTEXT_NAME%", ctmList.Name)))
                    {
                        var names = t.AsEnumerable().Select(r => Convert.ToString(r["MENU_NAME"]));
                        foreach (var ti in ctmList.Items.OfType<Control>())
                        {
                            newMenusOrder(oData, ctmList.Name, ti, null, ctmList.Items.IndexOf(ti), names, updateList);
                        }
                    }

                    updateList.RemoveAll(string.IsNullOrWhiteSpace);
                    if (updateList.Count > 0)
                    {
                        PNStatic.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("update_menus_order",
                            "Updating menus indexes");
                        foreach (var s in updateList)
                        {
                            oData.Execute(s);
                        }
                    }
                }
                PNMenus.PrepareDefaultMenuStrip(ctmPN, MenuType.Main, false);
                PNMenus.PrepareDefaultMenuStrip(ctmNote, MenuType.Note, false);
                PNMenus.PrepareDefaultMenuStrip(ctmEdit, MenuType.Edit, false);
                PNMenus.PrepareDefaultMenuStrip(ctmList, MenuType.ControlPanel, false);
                PNMenus.PrepareDefaultMenuStrip(ctmPN, MenuType.Main, true);
                PNMenus.PrepareDefaultMenuStrip(ctmNote, MenuType.Note, true);
                PNMenus.PrepareDefaultMenuStrip(ctmEdit, MenuType.Edit, true);
                PNMenus.PrepareDefaultMenuStrip(ctmList, MenuType.ControlPanel, true);
                PNMenus.CheckAndApplyNewMenusOrder(ctmPN);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                dn.Close();
                dcp.Close();
            }
        }
        private void applyStandardGroupsNames()
        {
            try
            {
                //change groups names
                var values = Enum.GetValues(typeof(SpecialGroups));
                foreach (var e in values)
                {
                    var name = Enum.GetName(typeof(SpecialGroups), e);
                    var g = PNStatic.Groups.GetGroupByID((int)e);
                    if (g != null)
                    {
                        g.Name = PNLang.Instance.GetGroupName(name, g.Name);
                    }
                }
                var gg = PNStatic.Groups.GetGroupByID(0);
                if (gg != null)
                {
                    gg.Name = PNLang.Instance.GetGroupName("General", gg.Name);
                }
                PNStatic.Docking.Name = PNLang.Instance.GetGroupName("Docking", PNStatic.Docking.Name);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillGroup(PNGroup gr, DataRow r)
        {
            try
            {
                var c = new ColorConverter();
                var wfc = new WPFFontConverter();
                var lfc = new LogFontConverter();
                var dcc = new System.Drawing.ColorConverter();

                gr.ID = (int)r["GROUP_ID"];
                gr.ParentID = (int)r["PARENT_ID"];
                gr.Name = (string)r["GROUP_NAME"];
                gr.PasswordString = (string)r["PASSWORD_STRING"];
                gr.IsDefaultImage = (bool)r["IS_DEFAULT_IMAGE"];
                if (!PNData.IsDBNull(r["ICON"]))
                {
                    var base64String = (string)r["ICON"];
                    if (!base64String.EndsWith(PNStrings.PNG_EXT))
                    {
                        try
                        {
                            var buffer = Convert.FromBase64String(base64String);
                            if (gr.ID.In((int)SpecialGroups.AllGroups, 0, (int)SpecialGroups.Diary,
                                (int)SpecialGroups.Backup, (int)SpecialGroups.SearchResults,
                                (int)SpecialGroups.Favorites, (int)SpecialGroups.Incoming, (int)SpecialGroups.Docking,
                                (int)SpecialGroups.RecycleBin) || base64String.StartsWith("resource."))
                            {
                                //possible image data stored as string when data directory just copied into new edition folder
                                upgradeGroupIcon(gr, gr.ID, (string)r["ICON"]);
                            }
                            else
                            {
                                using (var ms = new MemoryStream(buffer))
                                {
                                    ms.Position = 0;
                                    gr.Image = new BitmapImage();
                                    gr.Image.BeginInit();
                                    gr.Image.CacheOption = BitmapCacheOption.OnLoad;
                                    gr.Image.StreamSource = ms;
                                    gr.Image.EndInit();
                                }
                                if (gr.IsDefaultImage)
                                {
                                    gr.IsDefaultImage = false;
                                    var sb = new StringBuilder("UPDATE GROUPS SET IS_DEFAULT_IMAGE = 0");
                                    PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                                }
                            }
                        }
                        catch (FormatException)
                        {
                            //possible exception when data directory just copied into new edition folder
                            upgradeGroupIcon(gr, gr.ID, (string)r["ICON"]);
                        }
                    }
                    else
                    {
                        gr.Image = TryFindResource(Path.GetFileNameWithoutExtension(base64String)) as BitmapImage;// new BitmapImage(new Uri(base64String));
                    }
                }

                try
                {
                    var clr = c.ConvertFromString(null, PNStatic.CultureInvariant, (string)r["BACK_COLOR"]);
                    if (clr != null)
                        gr.Skinless.BackColor = (Color)clr;
                }
                catch (FormatException)
                {
                    //possible FormatException after synchronization with old database
                    var clr = dcc.ConvertFromString(null, PNStatic.CultureInvariant, (string)r["BACK_COLOR"]);
                    if (clr != null)
                    {
                        var drawingColor = (System.Drawing.Color)clr;
                        gr.Skinless.BackColor = Color.FromArgb(drawingColor.A, drawingColor.R,
                            drawingColor.G, drawingColor.B);
                        var sb = new StringBuilder("UPDATE GROUPS SET BACK_COLOR = '");
                        sb.Append(c.ConvertToString(null, PNStatic.CultureInvariant, gr.Skinless.BackColor));
                        sb.Append("' WHERE GROUP_ID = ");
                        sb.Append(gr.ID);
                        PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                    }
                }

                try
                {
                    var clr = c.ConvertFromString(null, PNStatic.CultureInvariant, (string)r["CAPTION_FONT_COLOR"]);
                    if (clr != null)
                        gr.Skinless.CaptionColor = (Color)clr;
                }
                catch (FormatException)
                {
                    //possible FormatException after synchronization with old database
                    var clr = dcc.ConvertFromString(null, PNStatic.CultureInvariant, (string)r["CAPTION_FONT_COLOR"]);
                    if (clr != null)
                    {
                        var drawingColor = (System.Drawing.Color)clr;
                        gr.Skinless.CaptionColor = Color.FromArgb(drawingColor.A, drawingColor.R,
                            drawingColor.G, drawingColor.B);
                        var sb = new StringBuilder("UPDATE GROUPS SET CAPTION_FONT_COLOR = '");
                        sb.Append(c.ConvertToString(null, PNStatic.CultureInvariant, gr.Skinless.CaptionColor));
                        sb.Append("' WHERE GROUP_ID = ");
                        sb.Append(gr.ID);
                        PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                    }
                }

                var fontString = (string)r["CAPTION_FONT"];
                //try
                //{
                var fonts = new InstalledFontCollection();
                var arr = fontString.Split(',');
                if (fontString.Any(ch => ch == '^'))
                {
                    //old format font string
                    var lf = lfc.ConvertFromString(fontString);
                    gr.Skinless.CaptionFont = PNStatic.FromLogFont(lf);
                    var sb = new StringBuilder("UPDATE GROUPS SET CAPTION_FONT = '");
                    sb.Append(wfc.ConvertToString(null, PNStatic.CultureInvariant, gr.Skinless.CaptionFont));
                    sb.Append("' WHERE GROUP_ID = ");
                    sb.Append(gr.ID);
                    PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                }
                else if (fonts.Families.Any(ff => ff.Name == arr[0]))
                {
                    //normal font string
                    gr.Skinless.CaptionFont = (PNFont)wfc.ConvertFromString(fontString);
                }
                else
                {
                    //possible not existing font name
                    arr[0] = PNStrings.DEF_CAPTION_FONT;
                    fontString = string.Join(",", arr);
                    gr.Skinless.CaptionFont = (PNFont)wfc.ConvertFromString(fontString);
                    var sb = new StringBuilder("UPDATE GROUPS SET CAPTION_FONT = '");
                    sb.Append(fontString);
                    sb.Append("' WHERE GROUP_ID = ");
                    sb.Append(gr.ID);
                    PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                }
                //}
                //catch (IndexOutOfRangeException)
                //{
                //    //possible IndexOutOfRangeException after synchronization with old database
                //    var lf = lfc.ConvertFromString(fontString);
                //    gr.Skinless.CaptionFont = PNStatic.FromLogFont(lf);
                //    var sb = new StringBuilder("UPDATE GROUPS SET CAPTION_FONT = '");
                //    sb.Append(wfc.ConvertToString(null, PNStatic.CultureInvariant, gr.Skinless.CaptionFont));
                //    sb.Append("' WHERE GROUP_ID = ");
                //    sb.Append(gr.ID);
                //    PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                //}

                var skinName = (string)r["SKIN_NAME"];
                if (skinName != PNSkinDetails.NO_SKIN)
                {
                    gr.Skin.SkinName = skinName;
                    //load skin
                    var path = Path.Combine(PNPaths.Instance.SkinsDir, gr.Skin.SkinName) + ".pnskn";
                    if (File.Exists(path))
                    {
                        PNSkinsOperations.LoadSkin(path, gr.Skin);
                    }
                }
                if (!PNData.IsDBNull(r["FONT"]))
                {
                    gr.Font = lfc.ConvertFromString((string)r["FONT"]);
                }
                if (!PNData.IsDBNull(r["FONT_COLOR"]))
                {
                    var clr = dcc.ConvertFromString(null, PNStatic.CultureInvariant, (string)r["FONT_COLOR"]);
                    if (clr != null)
                        gr.FontColor = (System.Drawing.Color)clr;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private string prepareGroupInsert(int id, int parentID, string name, string imageName)
        {
            try
            {
                var c = new ColorConverter();
                var lfc = new WPFFontConverter();
                var sb = new StringBuilder();
                sb.Append("INSERT INTO GROUPS (GROUP_ID, PARENT_ID, GROUP_NAME, ICON, BACK_COLOR, CAPTION_FONT_COLOR, CAPTION_FONT, SKIN_NAME, PASSWORD_STRING, IS_DEFAULT_IMAGE) VALUES(");
                sb.Append(id);
                sb.Append(",");
                sb.Append(parentID);
                sb.Append(",'");
                sb.Append(name.Replace("'", "''"));
                sb.Append("','");
                //sb.Append(PNStrings.RESOURCE_PREFIX);
                sb.Append(imageName);
                sb.Append("','");
                sb.Append(c.ConvertToString(null, PNStatic.CultureInvariant, PNSkinlessDetails.DefColor));
                sb.Append("','");
                sb.Append(c.ConvertToString(null, PNStatic.CultureInvariant, SystemColors.ControlTextColor));
                sb.Append("','");
                var f = new PNFont { FontWeight = FontWeights.Bold };
                sb.Append(lfc.ConvertToString(f));
                sb.Append("','");
                sb.Append(PNSkinDetails.NO_SKIN);
                sb.Append("','',1");
                sb.Append("); ");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private void applyLanguage()
        {
            try
            {
                applyMainMenuLanguage();
                var xspell = PNLang.Instance.GetLangElement("spellchecking");
                if (xspell != null)
                {
                    Spellchecking.SetLocalization(xspell);
                }
                ntfPN.ToolTipText = PNLang.Instance.GetMiscText("tray_tooltip",
                                                         "PNotes - your virtual desktop notes organizer");

                //change schedule descriptions
                PNLang.Instance.ChangeScheduleDescriptions();
                //change commands language
                PNLang.Instance.ApplyCommandsLanguage();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyMainMenuLanguage()
        {
            try
            {
                foreach (var ti in ctmPN.Items.OfType<MenuItem>())
                {
                    PNLang.Instance.ApplyMenuItemLanguage(ti, "main_menu");
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void importDictionaries()
        {
            var oldWin = new Form();
            try
            {
                var found = false;
                var fbd = new FolderBrowserDialog
                {
                    Description =
                        PNLang.Instance.GetCaptionText("import_dicts_caption", "PNotes dictionaries directory")
                };
                if (fbd.ShowDialog(oldWin) != System.Windows.Forms.DialogResult.OK) return;
                var files =
                    Directory.EnumerateFiles(fbd.SelectedPath, "*.*")
                        .Where(f => f.EndsWith(".aff", StringComparison.InvariantCultureIgnoreCase) ||
                                    f.EndsWith(".dic", StringComparison.InvariantCultureIgnoreCase));
                foreach (var f in files)
                {
                    var name = Path.GetFileName(f);
                    if (string.IsNullOrEmpty(name)) continue;
                    var destPath = Path.Combine(PNPaths.Instance.DictDir, name);
                    if (File.Exists(destPath)) continue;
                    File.Copy(f, destPath);
                    found = true;
                }
                if (found)
                {
                    var message = PNLang.Instance.GetMessageText("dicts_found",
                        "Applying new dictionaries requires program restart. Do you want to restart now?");
                    if (
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) ==
                        MessageBoxResult.Yes)
                    {
                        ApplyAction(MainDialogAction.Restart, null);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                oldWin.Close();
            }
        }

        private void importFonts()
        {
            var oldWin = new Form();
            try
            {
                var fbd = new FolderBrowserDialog
                {
                    Description = PNLang.Instance.GetCaptionText("import_fonts_caption", "PNotes fonts directory")
                };
                if (fbd.ShowDialog(oldWin) != System.Windows.Forms.DialogResult.OK) return;
                var files =
                    Directory.EnumerateFiles(fbd.SelectedPath, "*.*")
                        .Where(f => f.EndsWith(".fon", StringComparison.InvariantCultureIgnoreCase) ||
                                    f.EndsWith(".fnt", StringComparison.InvariantCultureIgnoreCase) ||
                                    f.EndsWith(".ttf", StringComparison.InvariantCultureIgnoreCase) ||
                                    f.EndsWith(".ttc", StringComparison.InvariantCultureIgnoreCase) ||
                                    f.EndsWith(".fot", StringComparison.InvariantCultureIgnoreCase) ||
                                    f.EndsWith(".otf", StringComparison.InvariantCultureIgnoreCase));
                foreach (var f in files)
                {
                    var name = Path.GetFileName(f);
                    if (string.IsNullOrEmpty(name)) continue;
                    var destPath = Path.Combine(PNPaths.Instance.FontsDir, name);
                    if (File.Exists(destPath)) continue;
                    File.Copy(f, destPath);
                    if (PNInterop.AddFontResourceEx(destPath, PNInterop.FR_PRIVATE, 0))
                    {
                        PNStatic.CustomFonts.Add(destPath);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                oldWin.Close();
            }
        }

        private void createPaths()
        {
            try
            {
                if (!Directory.Exists(PNPaths.Instance.BackupDir))
                    Directory.CreateDirectory(PNPaths.Instance.BackupDir);
                if (!Directory.Exists(PNPaths.Instance.FontsDir))
                    Directory.CreateDirectory(PNPaths.Instance.FontsDir);
                if (!Directory.Exists(PNPaths.Instance.SoundsDir))
                    Directory.CreateDirectory(PNPaths.Instance.SoundsDir);
                if (!Directory.Exists(PNPaths.Instance.DataDir))
                    Directory.CreateDirectory(PNPaths.Instance.DataDir);
                if (!Directory.Exists(PNPaths.Instance.PluginsDir))
                    Directory.CreateDirectory(PNPaths.Instance.PluginsDir);
                if (!Directory.Exists(PNPaths.Instance.DictDir))
                    Directory.CreateDirectory(PNPaths.Instance.DictDir);
                if (!Directory.Exists(PNPaths.Instance.ThemesDir))
                    Directory.CreateDirectory(PNPaths.Instance.ThemesDir);

                if (!File.Exists(Path.Combine(PNPaths.Instance.DictDir, "dictionaries.xml")))
                {
                    var streamResourceInfo = Application.GetResourceStream(new Uri(@"xml/dictionaries.xml", UriKind.Relative));
                    if (streamResourceInfo != null)
                    {
                        XDocument doc =
                            XDocument.Load(
                                streamResourceInfo.Stream);
                        doc.Save(Path.Combine(PNPaths.Instance.DictDir, "dictionaries.xml"));
                    }
                }
                PNStatic.Dictionaries = XDocument.Load(Path.Combine(PNPaths.Instance.DictDir, "dictionaries.xml"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadNotesAsFiles(int groupId)
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    Filter = @"PNotes.NET notes|*" + PNStrings.NOTE_EXTENSION,
                    Multiselect = true,
                    Title = PNLang.Instance.GetCaptionText("load_notes", "Load notes")
                };
                if (ofd.ShowDialog(this).Value)
                {
                    loadNotesFromFilesList(ofd.FileNames, groupId);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadNotesFromFilesList(IEnumerable<string> files, int groupId = 0)
        {
            try
            {
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    foreach (var fileName in files)
                    {
                        var id = Path.GetFileNameWithoutExtension(fileName);
                        var sqlQuery = "SELECT ID FROM NOTES WHERE ID = '" + id + "'";
                        var o = oData.GetScalar(sqlQuery);
                        if (o != null && !PNData.IsDBNull(o))
                        {
                            var message = PNLang.Instance.GetMessageText("id_exists", "The note with the same ID already exists");
                            message += " (" + id + ")";
                            PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }
                        else
                        {
                            var note = groupId > 0 ? new PNote(groupId) : new PNote();
                            note.ID = id;
                            note.Visible = true;
                            note.Dialog = new WndNote(note, fileName, NewNoteMode.File);
                            note.Dialog.Show();
                            note.NoteSize = note.Dialog.GetSize();
                            note.NoteLocation = note.Dialog.GetLocation();
                            note.EditSize = note.Dialog.Edit.Size;
                            PNStatic.Notes.Add(note);

                            subscribeToNoteEvents(note);

                            PNNotesOperations.SaveNewNote(note);

                            if (id == null) continue;
                            var newPath = Path.Combine(PNPaths.Instance.DataDir, id) + PNStrings.NOTE_EXTENSION;
                            if (!File.Exists(newPath))
                                File.Copy(fileName, newPath, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void newNoteFromClipboardInGroup(int groupId)
        {
            try
            {
                var note = new PNote(groupId);
                note.Dialog = new WndNote(note, NewNoteMode.Clipboard);
                note.Dialog.Show();
                note.Visible = true;
                note.NoteSize = note.Dialog.GetSize();
                note.NoteLocation = note.Dialog.GetLocation();
                note.EditSize = note.Dialog.Edit.Size;

                PNStatic.Notes.Add(note);
                if (NewNoteCreated != null)
                {
                    NewNoteCreated(this, new NewNoteCreatedEventArgs(note));
                }
                subscribeToNoteEvents(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void newNoteInGroup(int groupId)
        {
            try
            {
                var note = new PNote(groupId);
                note.Dialog = groupId != (int)SpecialGroups.Diary
                                  ? new WndNote(note, NewNoteMode.None)
                                  : new WndNote(note, NewNoteMode.Diary);
                note.Dialog.Show();
                note.Visible = true;
                note.NoteSize = note.Dialog.GetSize();
                note.NoteLocation = note.Dialog.GetLocation();
                note.EditSize = note.Dialog.Edit.Size;

                PNStatic.Notes.Add(note);
                if (NewNoteCreated != null)
                {
                    NewNoteCreated(this, new NewNoteCreatedEventArgs(note));
                }
                subscribeToNoteEvents(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkOverdueNotes()
        {
            try
            {
                if (!PNStatic.Settings.Schedule.TrackOverdue) return;
                if (PNSingleton.Instance.InOverdueChecking) return;
                PNSingleton.Instance.InOverdueChecking = true;
                var now = DateTime.Now;
                var list = new List<PNote>();
                var days = 0;

                var notes = PNStatic.Notes.Where(n => n.Schedule.Type != ScheduleType.None && n.Schedule.Track);
                foreach (var n in notes)
                {
                    DateTime start, alarmDate, startDate;
                    long seconds;
                    var changeZone = n.Schedule.TimeZone != TimeZoneInfo.Local;

                    switch (n.Schedule.Type)
                    {
                        case ScheduleType.Once:
                            alarmDate = changeZone
                                ? TimeZoneInfo.ConvertTime(n.Schedule.AlarmDate, TimeZoneInfo.Local, n.Schedule.TimeZone)
                                : n.Schedule.AlarmDate;
                            if (now > alarmDate)
                            {
                                list.Add(n);
                            }
                            break;
                        case ScheduleType.EveryDay:
                            alarmDate = changeZone
                                ? TimeZoneInfo.ConvertTime(n.Schedule.AlarmDate, TimeZoneInfo.Local, n.Schedule.TimeZone)
                                : n.Schedule.AlarmDate;
                            if ((n.Schedule.LastRun == DateTime.MinValue || n.Schedule.LastRun <= now.AddDays(-1))
                                && now.IsTimeMore(alarmDate))
                            {
                                list.Add(n);
                            }
                            break;
                        case ScheduleType.After:
                            start = n.Schedule.StartFrom == ScheduleStart.ExactTime
                                ? (changeZone
                                    ? TimeZoneInfo.ConvertTime(n.Schedule.StartDate, TimeZoneInfo.Local,
                                        n.Schedule.TimeZone)
                                    : n.Schedule.StartDate)
                                : PNStatic.StartTime;
                            PNStatic.NormalizeStartDate(ref start);
                            seconds = (now - start).Ticks / TimeSpan.TicksPerSecond;
                            if (seconds > n.Schedule.AlarmAfter.TotalSeconds)
                            {
                                list.Add(n);
                            }
                            break;
                        case ScheduleType.RepeatEvery:
                            if (n.Schedule.LastRun == DateTime.MinValue)
                            {
                                start = n.Schedule.StartFrom == ScheduleStart.ExactTime
                                    ? (changeZone
                                        ? TimeZoneInfo.ConvertTime(n.Schedule.StartDate, TimeZoneInfo.Local,
                                            n.Schedule.TimeZone)
                                        : n.Schedule.StartDate)
                                    : PNStatic.StartTime;
                            }
                            else
                            {
                                start = n.Schedule.LastRun;
                            }
                            PNStatic.NormalizeStartDate(ref start);
                            seconds = (now - start).Ticks / TimeSpan.TicksPerSecond;
                            if (seconds > n.Schedule.AlarmAfter.TotalSeconds)
                            {
                                list.Add(n);
                            }
                            break;
                        case ScheduleType.Weekly:
                            alarmDate = changeZone
                                ? TimeZoneInfo.ConvertTime(n.Schedule.AlarmDate, TimeZoneInfo.Local, n.Schedule.TimeZone)
                                : n.Schedule.AlarmDate;
                            var dayMin = n.Schedule.Weekdays.Min();
                            // if schedule has not been triggered yet
                            if (n.Schedule.LastRun == DateTime.MinValue
                                &&
                                (now.DayOfWeek > dayMin ||
                                 (now.DayOfWeek == dayMin && now.IsTimeMore(alarmDate))))
                            {
                                list.Add(n);
                            }
                            // else
                            else if (now > n.Schedule.LastRun.AddDays(-1))
                            {
                                var diffDays = (int)Math.Floor((now - n.Schedule.LastRun).TotalDays);
                                if (n.Schedule.Weekdays.Count == 1)
                                {
                                    days = 7;
                                }
                                else
                                {
                                    for (var i = 0; i < n.Schedule.Weekdays.Count; i++)
                                    {
                                        if (n.Schedule.Weekdays[i] != now.DayOfWeek) continue;
                                        if (i == n.Schedule.Weekdays.Count - 1)
                                            days =
                                                Math.Abs((int)n.Schedule.Weekdays[0] - (int)n.Schedule.Weekdays[i]);
                                        else
                                            days = (int)n.Schedule.Weekdays[i + 1] - (int)n.Schedule.Weekdays[i];
                                        break;
                                    }
                                }
                                if (diffDays > days)
                                {
                                    list.Add(n);
                                }
                                else if (diffDays == days)
                                {
                                    if (now.IsTimeMore(alarmDate))
                                    {
                                        list.Add(n);
                                    }
                                }
                            }
                            break;
                        case ScheduleType.MonthlyDayOfWeek:
                            // if schedule has not been triggered yet
                            if (n.Schedule.LastRun == DateTime.MinValue)
                            {
                                alarmDate = changeZone
                                    ? TimeZoneInfo.ConvertTime(n.Schedule.AlarmDate, TimeZoneInfo.Local,
                                        n.Schedule.TimeZone)
                                    : n.Schedule.AlarmDate;
                                startDate = changeZone
                                    ? TimeZoneInfo.ConvertTime(n.Schedule.StartDate, TimeZoneInfo.Local,
                                        n.Schedule.TimeZone)
                                    : n.Schedule.StartDate;
                                if (now.DayOfWeek == n.Schedule.MonthDay.WeekDay)
                                {
                                    var isLast = false;
                                    var ordinal = PNStatic.WeekdayOrdinal(now, n.Schedule.MonthDay.WeekDay, ref isLast);
                                    if (n.Schedule.MonthDay.OrdinalNumber == DayOrdinal.Last)
                                    {
                                        if (isLast)
                                        {
                                            if (now.IsTimeMore(alarmDate))
                                            {
                                                list.Add(n);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if ((int)n.Schedule.MonthDay.OrdinalNumber == ordinal)
                                        {
                                            if (now.IsTimeMore(alarmDate))
                                            {
                                                list.Add(n);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (weekdayOccured(startDate, now.AddDays(-1),
                                        n.Schedule.MonthDay.WeekDay, n.Schedule.MonthDay.OrdinalNumber))
                                    {
                                        list.Add(n);
                                    }
                                }
                            }
                            else
                            {
                                if (weekdayOccured(n.Schedule.LastRun.AddDays(1), now, n.Schedule.MonthDay.WeekDay,
                                    n.Schedule.MonthDay.OrdinalNumber))
                                {
                                    list.Add(n);
                                }
                            }
                            break;
                        case ScheduleType.MonthlyExact:
                            alarmDate = changeZone
                                ? TimeZoneInfo.ConvertTime(n.Schedule.AlarmDate, TimeZoneInfo.Local,
                                    n.Schedule.TimeZone)
                                : n.Schedule.AlarmDate;
                            startDate = changeZone
                                ? TimeZoneInfo.ConvertTime(n.Schedule.StartDate, TimeZoneInfo.Local,
                                    n.Schedule.TimeZone)
                                : n.Schedule.StartDate;
                            // if schedule has not been triggered yet
                            if (n.Schedule.LastRun == DateTime.MinValue)
                            {
                                // if now is exactly the day
                                if (now.Day == alarmDate.Day)
                                {
                                    if (now.IsTimeMore(alarmDate))
                                    {
                                        list.Add(n);
                                    }
                                }
                                else
                                {
                                    // check for day occurence
                                    if (dayOcurred(startDate, now.AddDays(-1), alarmDate.Day))
                                    {
                                        list.Add(n);
                                    }
                                }
                            }
                            else
                            {
                                // check for day occurence
                                if (dayOcurred(n.Schedule.LastRun, now, alarmDate.Day))
                                {
                                    list.Add(n);
                                }
                            }
                            break;
                        case ScheduleType.MultipleAlerts:
                            foreach (var ma in n.Schedule.MultiAlerts.Where(a => !a.Raised))
                            {
                                alarmDate = changeZone
                                    ? TimeZoneInfo.ConvertTime(ma.Date, TimeZoneInfo.Local,
                                        n.Schedule.TimeZone)
                                    : ma.Date;
                                if (now <= alarmDate) continue;
                                list.Add(n);
                                break;
                            }
                            break;
                    }
                }
                if (list.Count <= 0) return;
                var dov = new WndOverdue(list);
                dov.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                PNSingleton.Instance.InOverdueChecking = false;
            }
        }

        private bool dayOcurred(DateTime start, DateTime end, int day)
        {
            try
            {
                var dateCurrent = start;
                while (dateCurrent < end)
                {
                    if (dateCurrent.Day == day)
                    {
                        return true;
                    }
                    dateCurrent = dateCurrent.AddDays(1);
                }
                return false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool weekdayOccured(DateTime start, DateTime end, DayOfWeek dof, DayOrdinal ordinal)
        {
            try
            {
                var dateCurrent = start;
                while (dateCurrent < end)
                {
                    if (dateCurrent.DayOfWeek == dof)
                    {
                        DayOrdinal ordTemp;
                        var isLast = false;
                        var ordCurrent = PNStatic.WeekdayOrdinal(dateCurrent, dateCurrent.DayOfWeek, ref isLast);
                        if (isLast)
                        {
                            ordTemp = DayOrdinal.Last;
                        }
                        else
                        {
                            ordTemp = (DayOrdinal)ordCurrent;
                        }
                        if (ordTemp == ordinal)
                        {
                            return true;
                        }
                    }
                    dateCurrent = dateCurrent.AddDays(1);
                }
                return false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void checkStartUpShortcut()
        {
            try
            {
                var shortcutFile = Environment.GetFolderPath(Environment.SpecialFolder.Startup) +
                                              PNStrings.SHORTCUT_FILE;
                if (PNStatic.Settings.GeneralSettings.RunOnStart && !File.Exists(shortcutFile))
                {
                    // create shortcut
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
                else if (!PNStatic.Settings.GeneralSettings.RunOnStart && File.Exists(shortcutFile))
                {
                    // delete shortcut
                    File.Delete(shortcutFile);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyLogOn()
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Visible && n.Dialog != null);
                foreach (var note in notes)
                {
                    note.Dialog.Hide();
                    note.Dialog.Show();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);
        private void runProcessInAnotherThread(string fileName)
        {
            try
            {
                var ptr = new IntPtr();
                var sucessfullyDisabledWow64Redirect = false;

                // Disable x64 directory virtualization if we're on x64,
                // otherwise keyboard launch will fail.
                if (Environment.Is64BitOperatingSystem)
                {
                    sucessfullyDisabledWow64Redirect = Wow64DisableWow64FsRedirection(ref ptr);
                }

                // osk.exe is in windows/system folder. So we can directky call it without path
                using (var osk = new Process())
                {
                    osk.StartInfo.FileName = fileName;//"osk.exe";
                    osk.StartInfo.UseShellExecute = !Environment.Is64BitOperatingSystem;
                    osk.Start();
                }

                // Re-enable directory virtualisation if it was disabled.
                if (!Environment.Is64BitOperatingSystem) return;
                if (sucessfullyDisabledWow64Redirect)
                    Wow64RevertWow64FsRedirection(ptr);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void receiveNote(string data)
        {
            try
            {
                var temp = data.Split(PNStrings.END_OF_ADDRESS);
                var receivedFrom = PNStatic.Contacts.ContactNameByComputerName(temp[0]);
                var addresses = Dns.GetHostAddresses(temp[0]);
                // because we are on intranet, sender's ip which is equal to ourself ip is most probably ip of our computer
                var recIp = (addresses.Any(ip => ip.Equals(PNSingleton.Instance.IpAddress)))
                    ? PNSingleton.Instance.IpAddress
                    : addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                var sb = new StringBuilder();
                _ReceivedNotes = new List<string>();

                var rawData = temp[1].Split(PNStrings.END_OF_NOTE);
                //rawData[rawData.Length - 1] = rawData[rawData.Length - 1].Substring(0, rawData[rawData.Length - 1].IndexOf(PNStrings.END_OF_FILE));
                for (var i = 0; i < rawData.Length - 1; i++)
                {
                    temp = rawData[i].Split(PNStrings.END_OF_TEXT);
                    var nc = new NoteConverter();
                    var note = (PNote)nc.ConvertFromString(temp[1]);
                    if (note == null) continue;
                    note.ID = DateTime.Now.ToString("yyMMddHHmmssfff");
                    //note.NoteLocation = new Point(0, 0);
                    note.GroupID = (int)SpecialGroups.Incoming;
                    note.PrevGroupID = note.GroupID;
                    note.SentReceived = SendReceiveStatus.Received;
                    note.DateReceived = DateTime.Now;
                    note.ReceivedFrom = receivedFrom;
                    note.ReceivedIp = recIp != null ? recIp.ToString() : "";
                    note.NoteLocation =
                        new Point(
                            (Screen.GetWorkingArea(new System.Drawing.Point((int)Left,
                                (int)Top)).Width - note.NoteSize.Width) / 2,
                            (Screen.GetWorkingArea(new System.Drawing.Point((int)Left,
                                (int)Top)).Height - note.NoteSize.Height) / 2);

                    if (PNStatic.Settings.Network.ReceivedOnTop)
                    {
                        note.Topmost = true;
                    }

                    _ReceivedNotes.Add(note.ID);
                    sb.Append(note.Name);
                    sb.Append(";");
                    //sb.AppendLine();

                    if (!PNStatic.Settings.Network.ShowAfterArrive)
                    {
                        note.Visible = false;
                    }

                    var path = Path.Combine(PNPaths.Instance.DataDir, note.ID) + PNStrings.NOTE_EXTENSION;
                    using (var sw = new StreamWriter(path, false))
                    {
                        sw.Write(temp[0]);
                    }
                    if (PNStatic.Settings.Protection.PasswordString.Length > 0 && PNStatic.Settings.Protection.StoreAsEncrypted)
                    {
                        using (var pne = new PNEncryptor(PNStatic.Settings.Protection.PasswordString))
                        {
                            pne.EncryptTextFile(path);
                        }
                    }
                    if (note.Visible)
                    {
                        note.Dialog = new WndNote(note, note.ID, NewNoteMode.Identificator);
                        note.Dialog.Show();
                    }
                    PNStatic.Notes.Add(note);
                    if (NewNoteCreated != null)
                    {
                        NewNoteCreated(this, new NewNoteCreatedEventArgs(note));
                    }
                    subscribeToNoteEvents(note);

                    // save received note
                    PNNotesOperations.SaveNewNote(note);
                    PNNotesOperations.SaveNoteTags(note);
                    if (note.Schedule != null)
                    {
                        PNNotesOperations.SaveNoteSchedule(note);
                    }
                    note.Changed = false;
                }
                if (!PNStatic.Settings.Network.NoSoundOnArrive)
                {
                    PNSound.PlayMailSound();
                }

                if (!PNStatic.Settings.Network.NoNotificationOnArrive)
                {
                    var sbb = new StringBuilder(PNLang.Instance.GetCaptionText("received", "New notes received"));
                    sbb.Append(": ");
                    sbb.Append(sb);
                    if (sbb.Length > 1) sbb.Length -= 1;
                    sbb.AppendLine();
                    sbb.Append(PNLang.Instance.GetMessageText("sender", "Sender:"));
                    sbb.Append(" ");
                    sbb.Append(receivedFrom);
                    var baloon = new Baloon(BaloonMode.NoteReceived);
                    if (PNStatic.Settings.Network.ShowReceivedOnClick || PNStatic.Settings.Network.ShowIncomingOnClick)
                    {
                        baloon.BaloonLink = sbb.ToString();
                    }
                    else
                    {
                        baloon.BaloonText = sbb.ToString();
                    }
                    baloon.BaloonLinkClicked += baloon_BaloonLinkClicked;
                    ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, 10000);
                }
                if (NotesReceived != null)
                {
                    NotesReceived(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region System events
        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            try
            {
                switch (e.Reason)
                {
                    case SessionSwitchReason.SessionLogon:
                    case SessionSwitchReason.RemoteConnect:
                    case SessionSwitchReason.ConsoleConnect:
                        checkOverdueNotes();
                        break;
                    case SessionSwitchReason.RemoteDisconnect:
                        applyLogOn();
                        break;
                    case SessionSwitchReason.SessionUnlock:
                        checkOverdueNotes();
                        applyLogOn();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            try
            {
                switch (e.Mode)
                {
                    case PowerModes.Resume:
                        checkOverdueNotes();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Event handlers
        private void subscribeToNoteEvents(PNote note)
        {
            note.NoteBooleanChanged += note_NoteBooleanChanged;
            note.NoteNameChanged += note_NoteNameChanged;
            note.NoteGroupChanged += note_NoteGroupChanged;
            note.NoteDateChanged += note_NoteDateChanged;
            note.NoteDockStatusChanged += note_NoteDockStatusChanged;
            note.NoteSendReceiveStatusChanged += note_NoteSendReceiveStatusChanged;
            note.NoteTagsChanged += note_NoteTagsChanged;
            //note.NoteDeletedCompletely += note_NoteDeletedCompletely;
            note.NoteScheduleChanged += note_NoteScheduleChanged;
        }

        void FontUser_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FontFamily")
            {
                ApplyNewDefaultMenu();
            }
        }

        void note_NoteScheduleChanged(object sender, EventArgs e)
        {
            if (NoteScheduleChanged != null)
            {
                NoteScheduleChanged(sender, e);
            }
        }

        //void note_NoteDeletedCompletely(object sender, NoteDeletedCompletelyEventArgs e)
        //{
        //    if (NoteDeletedCompletely != null)
        //    {
        //        NoteDeletedCompletely(sender, e);
        //    }
        //}

        void note_NoteTagsChanged(object sender, EventArgs e)
        {
            if (NoteTagsChanged != null)
            {
                NoteTagsChanged(sender, e);
            }
        }

        void note_NoteSendReceiveStatusChanged(object sender, NoteSendReceiveStatusChangedEventArgs e)
        {
            if (NoteSendReceiveStatusChanged != null)
            {
                NoteSendReceiveStatusChanged(sender, e);
            }
        }

        void note_NoteDockStatusChanged(object sender, NoteDockStatusChangedEventArgs e)
        {
            if (NoteDockStatusChanged != null)
            {
                NoteDockStatusChanged(sender, e);
            }
        }

        private void note_NoteDateChanged(object sender, NoteDateChangedEventArgs e)
        {
            if (NoteDateChanged != null)
            {
                NoteDateChanged(sender, e);
            }
        }

        private void note_NoteGroupChanged(object sender, NoteGroupChangedEventArgs e)
        {
            if (NoteGroupChanged != null)
            {
                NoteGroupChanged(sender, e);
            }
        }

        private void note_NoteBooleanChanged(object sender, NoteBooleanChangedEventArgs e)
        {
            if (NoteBooleanChanged != null)
            {
                NoteBooleanChanged(sender, e);
            }
        }
        private void note_NoteNameChanged(object sender, NoteNameChangedEventArgs e)
        {
            if (NoteNameChanged != null)
            {
                NoteNameChanged(sender, e);
            }
        }
        #endregion

        #region Private event handlers
        private void dlgNewInGroup_NoteGroupChanged(object sender, NoteGroupChangedEventArgs e)
        {
            var dlgNewInGroup = sender as WndNewInGroup;
            if (dlgNewInGroup != null)
                dlgNewInGroup.NoteGroupChanged -= dlgNewInGroup_NoteGroupChanged;
            newNoteInGroup(e.NewGroup);
        }
        #endregion

        #region Menu clicks
        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void mnuNewNote_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                newNote();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuToPanelAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PNNotesOperations.ApplyThumbnails();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuFromPanelAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNStatic.FormPanel == null) return;
                PNStatic.FormPanel.RemoveAllThumbnails();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuOnScreenKbrd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var oskWindow = PNInterop.FindWindow("OSKMainClass", null);
                if (!oskWindow.Equals(IntPtr.Zero))
                    PNInterop.SendMessage(oskWindow, PNInterop.WM_CLOSE, 0, 0);
                else
                {
                    Task.Factory.StartNew(() => runProcessInAnotherThread("osk.exe"));
                    //var t = new Thread(() => runProcessInAnotherThread("osk.exe"));
                    //t.Start();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuMagnifier_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var magWindow = PNInterop.FindWindow("MagUIClass", null);
                if (!magWindow.Equals(IntPtr.Zero))
                    PNInterop.SendMessage(magWindow, PNInterop.WM_CLOSE, 0, 0);
                else
                {
                    Task.Factory.StartNew(() => runProcessInAnotherThread("magnify.exe"));
                    //var t = new Thread(() => runProcessInAnotherThread("magnify.exe"));
                    //t.Start();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuNewNoteInGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlgNewInGroup = new WndNewInGroup();
                dlgNewInGroup.NoteGroupChanged += dlgNewInGroup_NoteGroupChanged;
                var showDialog = dlgNewInGroup.ShowDialog();
                if (!showDialog.HasValue || !showDialog.Value)
                {
                    dlgNewInGroup.NoteGroupChanged -= dlgNewInGroup_NoteGroupChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuLoadNote_Click(object sender, RoutedEventArgs e)
        {
            loadNotesAsFiles(0);
        }

        private void mnuNoteFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = new PNote();
                note.Dialog = new WndNote(note, NewNoteMode.Clipboard);
                note.Dialog.Show();
                note.Visible = true;
                note.NoteSize = note.Dialog.GetSize();
                note.NoteLocation = note.Dialog.GetLocation();
                note.EditSize = note.Dialog.Edit.Size;

                PNStatic.Notes.Add(note);
                if (NewNoteCreated != null)
                {
                    NewNoteCreated(this, new NewNoteCreatedEventArgs(note));
                }
                subscribeToNoteEvents(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuTodayDiary_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.CreateOrShowTodayDiary();
        }

        private void mnuPrefs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNStatic.FormSettings == null)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Mouse.OverrideCursor = Cursors.Wait;
                    PNStatic.FormSettings = new WndSettings();
                    PNStatic.FormSettings.Show();
                }
                else
                {
                    if (PNStatic.FormSettings.WindowState == WindowState.Minimized)
                        PNStatic.FormSettings.WindowState = WindowState.Minimized;
                }
                PNStatic.FormSettings.BringToFront();
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

        private void mnuCP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNStatic.FormCP == null)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Mouse.OverrideCursor = Cursors.Wait;
                    PNStatic.FormCP = new WndCP();
                    PNStatic.FormCP.Show();
                }
                else
                {
                    if (PNStatic.FormCP.WindowState == WindowState.Minimized)
                    {
                        if (Math.Abs(PNStatic.Settings.Config.CPLocation.X - (-1)) < double.Epsilon &&
                            Math.Abs(PNStatic.Settings.Config.CPLocation.Y - (-1)) < double.Epsilon &&
                            Math.Abs(PNStatic.Settings.Config.CPSize.Width - (-1)) < double.Epsilon &&
                            Math.Abs(PNStatic.Settings.Config.CPSize.Height - (-1)) < double.Epsilon)
                        {
                            PNStatic.FormCP.WindowState = WindowState.Maximized;
                        }
                        else
                        {
                            PNStatic.FormCP.WindowState = WindowState.Normal;
                        }
                    }
                }
                PNStatic.FormCP.BringToFront();
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

        private void mnuHotkeys_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNStatic.FormHotkeys == null)
                {
                    var d = new WndHotkeys { Owner = this };
                    d.ShowDialog();
                }
                else
                {
                    PNStatic.FormHotkeys.Activate();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuMenusManagement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNStatic.FormMenus == null)
                {
                    var d = new WndMenusManager { Owner = this };
                    d.ShowDialog();
                }
                else
                {
                    PNStatic.FormMenus.Activate();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuShowAll_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ShowHideAllGroups(true);
        }

        private void mnuShowIncoming_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ShowHideSpecificGroup((int)SpecialGroups.Incoming, true);
        }

        private void mnuHideAll_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ShowHideAllGroups(false);
        }

        private void mnuHideIncoming_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ShowHideSpecificGroup((int)SpecialGroups.Incoming, false);
        }

        private void mnuTodayLast_Click(object sender, RoutedEventArgs e)
        {
            showRecentNotes(0);
        }

        private void mnu1DayAgo_Click(object sender, RoutedEventArgs e)
        {
            showRecentNotes(1);
        }

        private void mnu2DaysAgo_Click(object sender, RoutedEventArgs e)
        {
            showRecentNotes(2);
        }

        private void mnu3DaysAgo_Click(object sender, RoutedEventArgs e)
        {
            showRecentNotes(3);
        }

        private void mnu4DaysAgo_Click(object sender, RoutedEventArgs e)
        {
            showRecentNotes(4);
        }

        private void mnu5DaysAgo_Click(object sender, RoutedEventArgs e)
        {
            showRecentNotes(5);
        }

        private void mnu6DaysAgo_Click(object sender, RoutedEventArgs e)
        {
            showRecentNotes(6);
        }

        private void mnu7DaysAgo_Click(object sender, RoutedEventArgs e)
        {
            showRecentNotes(7);
        }

        private void mnuAllToFront_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Visible);
                foreach (var note in notes)
                {
                    note.Dialog.SendWindowToForeground();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSaveAll_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.SaveAllNotes(false);
        }

        private void mnuBackupCreate_Click(object sender, RoutedEventArgs e)
        {
            createFullBackup(PNStatic.Settings.Protection.SilentFullBackup);
        }

        private void mnuBackupRestore_Click(object sender, RoutedEventArgs e)
        {
            restoreFromFullBackup();
        }

        private void mnuSyncLocal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dls = new WndLocalSync { Owner = this };
                var showDialog = dls.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    ApplyAction(MainDialogAction.Restart, null);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuImportNotes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var d = new WndImportNotes { Owner = this };
                var showDialog = d.ShowDialog();
                if (showDialog == null || !showDialog.Value) return;
                if (PNStatic.FormCP == null) return;
                PNStatic.FormCP.Close();
                mnuCP.PerformClick();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuImportSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var d = new WndImportSettings { Owner = this };
                var showDialog = d.ShowDialog();
                if (showDialog == null || !showDialog.Value) return;
                if (PNStatic.FormSettings == null) return;
                PNStatic.FormSettings.Close();
                mnuPrefs.PerformClick();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuImportFonts_Click(object sender, RoutedEventArgs e)
        {
            importFonts();
        }

        private void mnuImportDictionaries_Click(object sender, RoutedEventArgs e)
        {
            importDictionaries();
        }

        private void mnuReloadAll_Click(object sender, RoutedEventArgs e)
        {
            reloadNotes();
        }

        private void mnuDAllNone_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ApplyDocking(DockStatus.None);
        }

        private void mnuDAllLeft_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ApplyDocking(DockStatus.Left);
        }

        private void mnuDAllTop_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ApplyDocking(DockStatus.Top);
        }

        private void mnuDAllRight_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ApplyDocking(DockStatus.Right);
        }

        private void mnuDAllBottom_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ApplyDocking(DockStatus.Bottom);
        }

        private void mnuSOnHighPriority_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Priority, true);
        }

        private void mnuSOnProtection_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Protection, true);
        }

        private void mnuSOnComplete_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Complete, true);
        }

        private void mnuSOnRoll_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Roll, true);
        }

        private void mnuSOnOnTop_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Topmost, true);
        }

        private void mnuSOffHighPriority_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Priority, false);
        }

        private void mnuSOffProtection_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Protection, false);
        }

        private void mnuSOffComplete_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Complete, false);
        }

        private void mnuSOffUnroll_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Roll, false);
        }

        private void mnuSOffOnTop_Click(object sender, RoutedEventArgs e)
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Topmost, false);
        }

        private void mnuSearchInNotes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNStatic.FormSearchInNotes == null)
                {
                    PNStatic.FormSearchInNotes = new WndSearchInNotes();
                    PNStatic.FormSearchInNotes.Show();
                }
                PNStatic.FormSearchInNotes.BringToFront();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSearchByTags_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNStatic.FormSearchByTags == null)
                {
                    PNStatic.FormSearchByTags = new WndSearchByTags();
                    PNStatic.FormSearchByTags.Show();
                }
                PNStatic.FormSearchByTags.BringToFront();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSearchByDates_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNStatic.FormSearchByDates == null)
                {
                    PNStatic.FormSearchByDates = new WndSearchByDates();
                    PNStatic.FormSearchByDates.Show();
                }
                PNStatic.FormSearchByDates.BringToFront();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuShowAllFavorites_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Favorite && n.GroupID != (int)SpecialGroups.RecycleBin);
                foreach (var note in notes)
                {
                    PNNotesOperations.ShowHideSpecificNote(note, true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuLockProg_Click(object sender, RoutedEventArgs e)
        {
            PNSingleton.Instance.IsLocked = lockProgram(!PNSingleton.Instance.IsLocked);
        }

        private void mnuHelp_Click(object sender, RoutedEventArgs e)
        {
            showHelp();
        }

        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            var d = new WndAbout { Owner = this };
            d.ShowDialog();
        }

        private void mnuSupport_Click(object sender, RoutedEventArgs e)
        {
            PNStatic.LoadPage(PNStrings.URL_PAYPAL);
        }

        private void mnuHomepage_Click(object sender, RoutedEventArgs e)
        {
            PNStatic.LoadPage(PNStrings.URL_MAIN);
        }

        #endregion

        public RichTextBox ActiveTextBox
        {
            get
            {
                var d = Application.Current.Windows.OfType<WndNote>().FirstOrDefault(w => w.Active);
                return d == null ? null : d.Edit;
            }
        }

        public string ActiveCulture
        {
            get { return PNLang.Instance.GetLanguageCulture(); }
        }

        public string ActiveNoteName
        {
            get
            {
                var d = Application.Current.Windows.OfType<WndNote>().FirstOrDefault(w => w.Active);
                if (d == null) return "";
                var note = PNStatic.Notes.Note(d.Handle);
                return note != null ? note.Name : "";
            }
        }

        public int LimitToGet
        {
            get { return PNStatic.Settings.Network.PostCount; }
        }

        public Dictionary<string, string> SyncParameters
        {
            get
            {
                var parameters = new Dictionary<string, string>
                    {
                        {"createTriggers", PNStrings.CREATE_TRIGGERS},
                        {"dropTriggers", PNStrings.DROP_TRIGGERS},
                        {"dataPath", PNPaths.Instance.DataDir},
                        {"dbPath", PNPaths.Instance.DBPath},
                        {"includeDeleted", PNStatic.Settings.Network.IncludeBinInSync.ToString()},
                        {"noteExt", PNStrings.NOTE_EXTENSION}
                    };
                return parameters;
            }
        }

        private void ntfPN_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bool? result = true;
                if (PNStatic.Settings.Protection.PasswordString != "" && PNSingleton.Instance.IsLocked)
                {
                    var d = new WndPasswordDelete(PasswordDlgMode.LoginMain);
                    result = d.ShowDialog();
                }
                if (result == null || !result.Value)
                {
                    return;
                }
                _InDblClick = true;
                _TmrDblClick.Stop();
                _Elapsed = 0;
                actionDoubleSingle(PNStatic.Settings.Behavior.DoubleClickAction);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void ntfPN_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_InDblClick)
                    _InDblClick = false;
                else
                    _TmrDblClick.Start();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void ntfPN_TrayRightMouseDown(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNStatic.Settings.Protection.PasswordString == "" || !PNSingleton.Instance.IsLocked) return;
                var d = new WndPasswordDelete(PasswordDlgMode.LoginMain);
                var result = d.ShowDialog();
                if (result != null && result.Value)
                {
                    PNSingleton.Instance.IsLocked = lockProgram(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void ntfPN_TrayContextMenuOpen(object sender, RoutedEventArgs e)
        {
            try
            {
                createDiaryMenu();
                createRunMenu();
                createShowHideMenus();
                createFavoritesMenu();
                createTagsMenus();
                createSyncMenu();

                mnuPanelAll.IsEnabled =
                    mnuToPanelAll.IsEnabled = mnuFromPanelAll.IsEnabled = PNStatic.Settings.Behavior.ShowNotesPanel;

                mnuLockProg.IsEnabled = PNStatic.Settings.Protection.PasswordString.Trim().Length > 0;
                mnuRun.IsEnabled = PNStatic.Externals.Count > 0;
                mnuNoteFromClipboard.IsEnabled = Clipboard.ContainsText(TextDataFormat.Text) ||
                                                 Clipboard.ContainsText(TextDataFormat.UnicodeText) ||
                                                 Clipboard.ContainsText(TextDataFormat.Rtf) ||
                                                 Clipboard.ContainsText(TextDataFormat.Xaml) ||
                                                 Clipboard.ContainsText(TextDataFormat.CommaSeparatedValue);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
