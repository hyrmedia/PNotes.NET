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
    /// Interaction logic for WndPasswordCreate.xaml
    /// </summary>
    public partial class WndPasswordCreate
    {
        public WndPasswordCreate()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndPasswordCreate(string textAddition)
            : this()
        {
            m_TextAddition = textAddition;
        }

        internal event EventHandler<PasswordChangedEventArgs> PasswordChanged;

        private readonly string m_TextAddition = "";

        private void DlgPasswordCreate_Loaded(object sender, RoutedEventArgs e)
        {
            PNLang.Instance.ApplyControlLanguage(this);
            Title = PNLang.Instance.GetCaptionText("pwrd_create", "Create password") + m_TextAddition;
            txtEnterPwrd.Focus();
            ToolTip = Title;
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtEnterPwrd.Password != txtConfirmPwrd.Password)
                {
                    var message = PNLang.Instance.GetMessageText("pwrd_not_identical", "Both password strings should be identical. Please, check the spelling.");
                    PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtEnterPwrd.Focus();
                    txtEnterPwrd.SelectAll();
                    return;
                }
                string hash = PNEncryptor.GetHashString(txtEnterPwrd.Password.Trim());
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

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            cmdOK.IsEnabled = txtEnterPwrd.Password.Trim().Length > 0 && txtConfirmPwrd.Password.Trim().Length > 0;
        }
    }
}
