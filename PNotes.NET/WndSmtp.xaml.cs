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
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSmtp.xaml
    /// </summary>
    public partial class WndSmtp
    {
        internal event EventHandler<SmtpChangedEventArgs> SmtpChanged;

        public WndSmtp()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndSmtp(PNSmtpProfile client)
            : this()
        {
            _Mode = client == null ? AddEditMode.Add : AddEditMode.Edit;
            _Client = client == null ? new PNSmtpProfile() : client.PNClone();
        }

        private readonly AddEditMode _Mode;
        private readonly PNSmtpProfile _Client;

        private void DlgSmtp_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                if (_Mode == AddEditMode.Edit)
                {
                    Title = PNLang.Instance.GetCaptionText("smtp_edit", "Edit SMTP client");
                    txtSmtpHost.Text = _Client.HostName;
                    txtSmtpAddress.Text = _Client.SenderAddress;
                    txtSmtpDisplayName.Text = _Client.DisplayName;
                    txtSmtpPort.Text = _Client.Port.ToString(CultureInfo.InvariantCulture);
                    using (var encryptor = new PNEncryptor(PNKeys.ENC_KEY))
                    {
                        txtSmtpPassword.Password = encryptor.DecryptString(_Client.Password);
                    }
                    cmdOK.IsEnabled = true;
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("smtp_new", "New SMTP client");
                }
                txtSmtpHost.SelectAll();
                txtSmtpHost.Focus();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtSmtpPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            enableOK();
        }

        private void chkSmtpShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkSmtpShowPassword.IsChecked == null) return;
                if (chkSmtpShowPassword.IsChecked.Value)
                {
                    txtKey.Visibility = Visibility.Visible;
                    txtSmtpPassword.Visibility = Visibility.Hidden;
                }
                else
                {
                    txtKey.Visibility = Visibility.Hidden;
                    txtSmtpPassword.Visibility = Visibility.Visible;
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
                var hostNameType = Uri.CheckHostName(txtSmtpHost.Text.Trim());
                if (hostNameType == UriHostNameType.Unknown)
                {
                    PNMessageBox.Show(PNLang.Instance.GetMessageText("invalid_host", "Invalid host name"),
                        PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtSmtpHost.SelectAll();
                    txtSmtpHost.Focus();
                    return;
                }
                var rg = new Regex(PNStrings.MAIL_PATTERN, RegexOptions.IgnoreCase);
                var match = rg.Match(txtSmtpAddress.Text.Trim());
                if (!match.Success)
                {
                    PNMessageBox.Show(PNLang.Instance.GetMessageText("invalid_email", "Invalid e-mail address"),
                       PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtSmtpAddress.SelectAll();
                    txtSmtpAddress.Focus();
                    return;
                }
                if (txtSmtpPort.Text.Trim().StartsWith("0") || Convert.ToInt32(txtSmtpPort.Text.Trim()) > 65535)
                {
                    PNMessageBox.Show(PNLang.Instance.GetMessageText("invalid_port", "Invalid port number"),
                       PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtSmtpPort.SelectAll();
                    txtSmtpPort.Focus();
                    return;
                }
                _Client.HostName = txtSmtpHost.Text.Trim();
                _Client.SenderAddress = txtSmtpAddress.Text.Trim();
                using (var encryptor = new PNEncryptor(PNKeys.ENC_KEY))
                {
                    _Client.Password = encryptor.EncryptString(txtSmtpPassword.Password);
                }
                _Client.Port = Convert.ToInt32(txtSmtpPort.Text.Trim());
                _Client.DisplayName = txtSmtpDisplayName.Text.Trim().Length > 0 ? txtSmtpDisplayName.Text.Trim() : _Client.SenderAddress;
                if (SmtpChanged != null)
                {
                    var ev = new SmtpChangedEventArgs(_Client, _Mode);
                    SmtpChanged(this, ev);
                    if (!ev.Accepted)
                    {
                        txtSmtpAddress.SelectAll();
                        txtSmtpAddress.Focus();
                        return;
                    }
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtSmtpHost_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableOK();
        }

        private void txtSmtpAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableOK();
        }

        private void txtSmtpPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableOK();
        }

        private void enableOK()
        {
            cmdOK.IsEnabled = txtSmtpAddress.Text.Trim().Length > 0 && txtSmtpHost.Text.Trim().Length > 0 &&
                            txtSmtpPassword.Password.Trim().Length > 0 && txtSmtpPort.Text.Trim().Length > 0;
        }

        private void txtSmtpPort_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    e.Handled = true;
                    return;
                }
                if (!((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
                      e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right ||
                      e.Key == Key.Tab || e.Key == Key.Escape))
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
