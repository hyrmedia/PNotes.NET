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
using System.Threading;
using System.Windows;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSync.xaml
    /// </summary>
    public partial class WndSync
    {
        public WndSync()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndSync(IList<string> folders)
            : this()
        {
            _Folders[0] = folders[0];
            _Folders[1] = folders[1];
        }

        private readonly string[] _Folders = { "", "" };

        private void DlgSync_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                Title += @" [" + PNLang.Instance.GetCaptionText("sync_local", "Local") + @"]";
                System.Windows.Forms.Application.DoEvents();
                var sync = new PNSync();
                sync.SyncComplete += sync_SyncComplete;
                var t = new Thread(sync.SyncLocal);
                t.Start(_Folders);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void SyncCompleteDelegate(object sender, LocalSyncCompleteEventArgs e);
        private void sync_SyncComplete(object sender, LocalSyncCompleteEventArgs e)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    SyncCompleteDelegate d = sync_SyncComplete;
                    Dispatcher.Invoke(d, sender, e);
                }
                else
                {
                    Topmost = false;
                    var sync = (PNSync)sender;
                    sync.SyncComplete -= sync_SyncComplete;
                    switch (e.Result)
                    {
                        case LocalSyncResult.None:
                            PNMessageBox.Show(PNLang.Instance.GetMessageText("sync_complete", "Syncronization completed successfully"), PNStrings.PROG_NAME, MessageBoxButton.OK);
                            DialogResult = false;
                            break;
                        case LocalSyncResult.Reload:
                            PNMessageBox.Show(PNLang.Instance.GetMessageText("sync_complete_reload", "Syncronization completed successfully. The program has to be restarted for applying all changes."), PNStrings.PROG_NAME, MessageBoxButton.OK);
                            PNData.UpdateTablesAfterSync();
                            DialogResult = true;
                            break;
                        case LocalSyncResult.AbortVersion:
                            PNMessageBox.Show(PNLang.Instance.GetMessageText("diff_versions", "Current version of database is different from previously synchronized version. Synchronization cannot be performed."), PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            DialogResult = false;
                            break;
                        case LocalSyncResult.Error:
                            DialogResult = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
