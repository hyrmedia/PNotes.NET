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
    /// Interaction logic for WndPasswordDelete.xaml
    /// </summary>
    public partial class WndPasswordDelete
    {
        public WndPasswordDelete()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndPasswordDelete(PasswordDlgMode mode)
            : this()
        {
            m_Mode = mode;
            if (mode == PasswordDlgMode.LoginMain)
            {
                Topmost = true;
            }
        }

        internal WndPasswordDelete(PasswordDlgMode mode, string additionalText, string hash)
            : this()
        {
            m_Mode = mode;
            m_AdditionalText = additionalText;
            m_Hash = hash;
            if (mode == PasswordDlgMode.LoginMain)
            {
                Topmost = true;
            }
        }

        internal event EventHandler PasswordDeleted;

        private readonly PasswordDlgMode m_Mode;
        private readonly string m_AdditionalText = "";
        private readonly string m_Hash = "";

        private void txtEnterPwrd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            cmdOK.IsEnabled = txtEnterPwrd.Password.Trim().Length > 0;
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var hash = PNEncryptor.GetHashString(txtEnterPwrd.Password.Trim());
                var hashCheck = (m_Mode == PasswordDlgMode.DeleteMain || m_Mode == PasswordDlgMode.LoginMain)
                    ? PNStatic.Settings.Protection.PasswordString
                    : m_Hash;
                if (hash != null)
                {
                    if (hash != hashCheck)
                    {
                        var message = PNLang.Instance.GetMessageText("pwrd_not_match", "Invalid password");
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                        txtEnterPwrd.Focus();
                        txtEnterPwrd.SelectAll();
                        return;
                    }
                }
                if (m_Mode == PasswordDlgMode.DeleteMain || m_Mode == PasswordDlgMode.DeleteGroup || m_Mode == PasswordDlgMode.DeleteNote)
                {
                    if (PasswordDeleted != null)
                    {
                        PasswordDeleted(this, new EventArgs());
                    }
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgPasswordDelete_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                if (m_Mode == PasswordDlgMode.DeleteMain || m_Mode == PasswordDlgMode.DeleteGroup || m_Mode == PasswordDlgMode.DeleteNote)
                {
                    Title = PNLang.Instance.GetCaptionText("pwrd_delete", "Delete password");
                    if (m_Mode != PasswordDlgMode.DeleteGroup && m_Mode != PasswordDlgMode.DeleteNote)
                    {
                        Activate();
                        txtEnterPwrd.Focus();
                        return;
                    }
                    Title += m_AdditionalText;
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("pwrd_login", "Enter password");
                    if (m_Mode != PasswordDlgMode.LoginGroup && m_Mode != PasswordDlgMode.LoginNote)
                    {
                        Activate();
                        txtEnterPwrd.Focus();
                        return;
                    }
                    Title += m_AdditionalText;
                }
                ToolTip = Title;
                Activate();
                txtEnterPwrd.Focus();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
