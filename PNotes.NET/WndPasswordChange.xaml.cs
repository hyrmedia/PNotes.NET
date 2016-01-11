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

using PNEncryption;
using System;
using System.Windows;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndPasswordChange.xaml
    /// </summary>
    public partial class WndPasswordChange
    {
        internal event EventHandler<PasswordChangedEventArgs> PasswordChanged;

        public WndPasswordChange()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private void DlgPasswordChange_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                Title = PNLang.Instance.GetCaptionText("pwrd_edit", "Edit password");
                txtOldPwrd.Focus();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtEnterPwrd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            cmdOK.IsEnabled = txtEnterPwrd.Password.Trim().Length > 0 && txtConfirmPwrd.Password.Trim().Length > 0 &&
                              txtOldPwrd.Password.Trim().Length > 0;
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string hash = PNEncryptor.GetHashString(txtOldPwrd.Password.Trim());
                if (hash != null)
                {
                    if (hash != PNStatic.Settings.Protection.PasswordString)
                    {
                        var message = PNLang.Instance.GetMessageText("pwrd_old_not_match", "Invalid old password");
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                        txtOldPwrd.Focus();
                        txtOldPwrd.SelectAll();
                        return;
                    }
                }
                if (txtEnterPwrd.Password != txtConfirmPwrd.Password)
                {
                    var message = PNLang.Instance.GetMessageText("pwrd_not_identical", "Both password strings should be identical. Please, check the spelling.");
                    PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtEnterPwrd.Focus();
                    txtEnterPwrd.SelectAll();
                    return;
                }
                hash = PNEncryptor.GetHashString(txtEnterPwrd.Password.Trim());
                if (hash != null)
                {
                    if (PasswordChanged != null)
                    {
                        PasswordChanged(this, new PasswordChangedEventArgs(hash));
                    }
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
