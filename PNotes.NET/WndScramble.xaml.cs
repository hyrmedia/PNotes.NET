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
using System.Windows.Controls;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndScramble.xaml
    /// </summary>
    public partial class WndScramble
    {
        public WndScramble()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndScramble(ScrambleMode mode, PNote note)
            : this()
        {
            _Mode = mode;
            _Note = note;
        }

        private readonly ScrambleMode _Mode;
        private readonly PNote _Note;

        private void DlgScramble_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                if (_Mode == ScrambleMode.Scramble)
                {
                    Title = PNLang.Instance.GetCaptionText("scramble_caption", "Encrypt note:") + @" " + _Note.Name;
                    lblScrambleWarning.Text = PNLang.Instance.GetCaptionText("scramble_warning",
                        "Encryption of note's text will remove all rich text formatting!");
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("unscramble_caption", "Decrypt note:") + @" " + _Note.Name;
                    lblScrambleWarning.Text = "";
                }
                pwrdKey.Focus();
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
                using (var enc = new PNEncryptor(txtKey.Text.Trim()))
                {
                    _Note.Dialog.Edit.Text = _Mode == ScrambleMode.Scramble
                        ? enc.EncryptStringWithTrim(_Note.Dialog.Edit.Text.Trim())
                        : enc.DecryptStringWithTrim(_Note.Dialog.Edit.Text);
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
                PNMessageBox.Show(this, PNLang.Instance.GetMessageText("pwrd_not_match", "Invalid password"),
                    PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void chkSmtpShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            pwrdKey.Visibility = Visibility.Collapsed;
            txtKey.Visibility = Visibility.Visible;
            txtKey.Focus();
        }

        private void chkSmtpShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            pwrdKey.Visibility = Visibility.Visible;
            txtKey.Visibility = Visibility.Collapsed;
            pwrdKey.Focus();
        }

        private void pwrdKey_PasswordChanged(object sender, RoutedEventArgs e)
        {
            cmdOK.IsEnabled = txtKey.Text.Trim().Length > 0;
        }

        private void txtKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            cmdOK.IsEnabled = txtKey.Text.Trim().Length > 0;
        }
    }
}
