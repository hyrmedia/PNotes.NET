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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using PNEncryption;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSendSmtp.xaml
    /// </summary>
    public partial class WndSendSmtp
    {
        public WndSendSmtp()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndSendSmtp(PNSmtpProfile client, string noteName, string body, IEnumerable<string> attachment)
            : this()
        {
            _Client = client;
            _Attachment = attachment;
            _Body = body;
            _NoteName = noteName;
        }

        internal WndSendSmtp(PNSmtpProfile client, string noteName, string recipient, string body, IEnumerable<string> attachment)
            : this(client, noteName, body, attachment)
        {
            _Recipients = recipient;
        }

        private readonly PNSmtpProfile _Client;
        private readonly IEnumerable<string> _Attachment;
        private readonly string _Body;
        private readonly string _NoteName;
        private readonly string _Recipients;

        private void cmdShowContacts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlgChoose = new WndChooseMailContacts {Owner = this};
                dlgChoose.MailRecipientsChosen += dlgChoose_MailRecipientsChosen;
                var showDialog = dlgChoose.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    dlgChoose.MailRecipientsChosen -= dlgChoose_MailRecipientsChosen;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgChoose_MailRecipientsChosen(object sender, MailRecipientsChosenEventArgs e)
        {
            try
            {
                var d = sender as WndChooseMailContacts;
                if (d != null) d.MailRecipientsChosen -= dlgChoose_MailRecipientsChosen;
                var recips = string.Join("; ", e.Recipients.Select(r => r.Address).ToArray());
                if (txtSmtpRecipients.Text.Trim().EndsWith(";"))
                    txtSmtpRecipients.AppendText(recips);
                else
                {
                    if (txtSmtpRecipients.Text.Trim().Length > 0)
                        txtSmtpRecipients.AppendText("; " + recips);
                    else
                        txtSmtpRecipients.AppendText(recips);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            sendMail();
        }

        private void DlgSendSmtp_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                txtSmtpSubject.Text = !string.IsNullOrEmpty(_NoteName)
                    ? PNLang.Instance.GetMessageText("mail_subject_text", "Sent from PNotes. Note name:") +
                      @" " + _NoteName
                    : PNLang.Instance.GetMessageText("mail_subject_attachment", "Sent from PNotes.");
                var lines = _Body != null ? _Body.Split('\n') : new string[] { };
                foreach (var l in lines)
                {
                    txtSmtpBody.Text += l;
                    txtSmtpBody.Text += '\n';
                }
                if (_Attachment == null)
                {
                    txtSmtpBody.IsReadOnly = true;
                }
                else
                {
                    foreach (var s in _Attachment)
                        txtSmtpAttachments.AppendText(Path.GetFileName(s) + @"  ");
                }
                if(!string.IsNullOrEmpty(_Recipients))
                {
                    txtSmtpRecipients.Text = _Recipients;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void enableOk()
        {
            cmdOK.IsEnabled = txtSmtpSubject.Text.Trim().Length > 0 && txtSmtpRecipients.Text.Trim().Length > 0;
        }

        private void sendMail()
        {
            try
            {
                Cursor = Cursors.Wait;
                string password;
                using (var encryptor = new PNEncryptor(PNKeys.ENC_KEY))
                {
                    password = encryptor.DecryptString(_Client.Password);
                }
                var smtp = new SmtpClient(_Client.HostName, _Client.Port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(_Client.SenderAddress, password),
                    Timeout = 20000
                };
                using (
                    var message = new MailMessage
                    {
                        Body = txtSmtpBody.Text.Trim(),
                        Subject = txtSmtpSubject.Text.Trim()
                    })
                {
                    message.From = new MailAddress(_Client.SenderAddress, _Client.DisplayName);
                    var recipients = txtSmtpRecipients.Text.Trim()
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var ma in recipients.Select(r => resolveName(r.Trim())).Where(ma => ma != null))
                    {
                        message.To.Add(ma);
                    }
                    if (_Attachment != null)
                    {
                        var ext = "";
                        var extension = Path.GetExtension(_Attachment.First());
                        if (extension != null)
                        {
                            ext = extension.ToUpper();
                        }
                        var mt = ext.EndsWith("PNOTE")
                            ? MediaTypeNames.Application.Rtf
                            : (ext.EndsWith("ZIP") ? MediaTypeNames.Application.Zip : MediaTypeNames.Application.Octet);
                        foreach (var s in _Attachment)
                        {
                            message.Attachments.Add(new Attachment(s, mt));
                        }
                    }
                    var attempts = 0;
                    while (attempts < 5)
                    {
                        try
                        {
                            smtp.Send(message);
                            break;
                        }
                        catch (SmtpException smtex)
                        {
                            if (smtex.Message.Contains("The operation has timed out"))
                            {
                                PNStatic.LogException(smtex, false);
                                attempts++;
                            }
                            else
                            {
                                PNStatic.LogException(smtex, false);
                                var sb = new StringBuilder(PNLang.Instance.GetMessageText("send_error_1",
                                    "An error occurred during note(s) sending."));
                                sb.AppendLine();
                                sb.Append(PNLang.Instance.GetMessageText("send_error_2",
                                    "Please, refer to log file for details."));
                                var baloon = new Baloon(BaloonMode.Error) {BaloonText = sb.ToString()};
                                PNStatic.FormMain.ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, 10000);
                                return;
                            }
                        }
                    }
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private MailAddress resolveName(string name)
        {
            try
            {
                var rg = new Regex(PNStrings.MAIL_PATTERN, RegexOptions.IgnoreCase);
                if (rg.IsMatch(name))
                {
                    var mc = PNStatic.MailContacts.FirstOrDefault(c => c.Address == name);
                    if (mc != null)
                    {
                        return !string.IsNullOrWhiteSpace(mc.DisplayName)
                            ? new MailAddress(name, mc.DisplayName)
                            : new MailAddress(name);
                    }
                    return new MailAddress(name);
                }
                else
                {
                    var mc = PNStatic.MailContacts.FirstOrDefault(c => c.DisplayName == name);
                    if (mc != null)
                    {
                        return !string.IsNullOrWhiteSpace(mc.Address)
                            ? new MailAddress(mc.Address, mc.DisplayName)
                            : null;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void text_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            enableOk();
        }
    }
}
