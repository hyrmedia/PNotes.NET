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
    /// Interaction logic for WndLocalSync.xaml
    /// </summary>
    public partial class WndLocalSync
    {
        public WndLocalSync()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
            _Loaded = true;
        }

        private readonly bool _Loaded;
        private void DlgLocalSync_Loaded(object sender, RoutedEventArgs e)
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

        private void Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                enableOK();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
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
                if (chkUseDataDir.IsChecked != null)
                    cmdDBDir.IsEnabled = txtDBDir.IsEnabled = !chkUseDataDir.IsChecked.Value;
                enableOK();
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
                var paths = new string[2];
                paths[0] = txtDataDir.Text.Trim();
                paths[1] = "";
                if (chkUseDataDir.IsChecked != null && chkUseDataDir.IsChecked.Value)
                {
                    paths[1] = txtDBDir.Text.Trim();
                }
                var ds = new WndSync(paths) {Owner = this};
                DialogResult = ds.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void enableOK()
        {
            try
            {
                if (!_Loaded) return;
                if (chkUseDataDir.IsChecked != null && chkUseDataDir.IsChecked.Value)
                {
                    cmdOK.IsEnabled = txtDataDir.Text.Trim().Length > 0;
                }
                else
                {
                    cmdOK.IsEnabled = txtDataDir.Text.Trim().Length > 0 && txtDBDir.Text.Trim().Length > 0;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
