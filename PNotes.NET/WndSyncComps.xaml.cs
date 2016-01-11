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
using System.Windows;
using System.Windows.Controls;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSyncComps.xaml
    /// </summary>
    public partial class WndSyncComps
    {
        public WndSyncComps()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        
        internal WndSyncComps(WndSettings prefs, AddEditMode mode):this()
        {
            _Prefs = prefs;
            _Mode = mode;
        }
        internal WndSyncComps(WndSettings prefs, PNSyncComp sc, AddEditMode mode):this()
        {
            _Prefs = prefs;
            _Mode = mode;
            _SyncComp = sc;
        }

        private readonly WndSettings _Prefs;
        private readonly AddEditMode _Mode;
        private readonly PNSyncComp _SyncComp;

        private void DlgSyncComps_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                if (_Mode == AddEditMode.Add)
                {
                    Title = PNLang.Instance.GetCaptionText("sync_comps_add", "New local synchronization target");
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("sync_comps_edit", "Edit local synchronization target");
                    txtCompName.IsReadOnly = true;
                    txtCompName.Text = _SyncComp.CompName;
                    txtDataDir.Text = _SyncComp.DataDir;
                    txtDBDir.Text = _SyncComp.DBDir;
                    chkUseDataDir.IsChecked = _SyncComp.UseDataDir;
                }
                txtCompName.SelectAll();
                txtCompName.Focus();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtCompName_TextChanged(object sender, TextChangedEventArgs e)
        {
            cmdOK.IsEnabled = txtCompName.Text.Trim().Length > 0 && txtDataDir.Text.Trim().Length > 0;
        }

        private void txtDataDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            cmdOK.IsEnabled = txtCompName.Text.Trim().Length > 0 && txtDataDir.Text.Trim().Length > 0;
        }

        private void cmdDataDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fbd = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = PNLang.Instance.GetCaptionText("choose_dir", "Choose directory")
                };
                if (txtDataDir.Text.Trim().Length > 0)
                    fbd.SelectedPath = txtDataDir.Text.Trim();
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtDataDir.Text = fbd.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDBDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fbd = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = PNLang.Instance.GetCaptionText("choose_dir", "Choose directory")
                };
                if (txtDBDir.Text.Trim().Length > 0)
                    fbd.SelectedPath = txtDBDir.Text.Trim();
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtDBDir.Text = fbd.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void chkUseDataDir_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkUseDataDir.IsChecked != null && chkUseDataDir.IsChecked.Value)
                {
                    txtDBDir.Text = "";
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Mode == AddEditMode.Add && _Prefs.SyncCompExists(txtCompName.Text.Trim()))
                {
                    string message = PNLang.Instance.GetMessageText("sync_comp_exists", "Local synchronization target with this name already exists");
                    PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                    //_PreventUnload = true;
                }
                else
                {
                    var sc = new PNSyncComp { CompName = txtCompName.Text.Trim(), DataDir = txtDataDir.Text.Trim(), UseDataDir = chkUseDataDir.IsChecked != null && chkUseDataDir.IsChecked.Value };
                    if (chkUseDataDir.IsChecked != null && !chkUseDataDir.IsChecked.Value)
                    {
                        sc.DBDir = txtDBDir.Text.Trim();
                    }
                    if (_Mode == AddEditMode.Add)
                        _Prefs.SyncCompAdd(sc);
                    else
                        _Prefs.SyncCompReplace(sc);
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
