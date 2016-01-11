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
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndHelpChooser.xaml
    /// </summary>
    public partial class WndHelpChooser
    {
        public WndHelpChooser()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private string m_FileToOpen = "";
        private string m_Progress = "";

        private void DlgHelpChooser_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                Title = PNLang.Instance.GetCaptionText("help_chooser", "Help file chooser");
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgHelpChooser_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;
            e.Handled = true;
            Close();
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (optGoOnlineHelp.IsChecked != null && optGoOnlineHelp.IsChecked.Value)
                {
                    PNStatic.LoadPage(PNStrings.URL_HELP);
                    DialogResult = true;
                }
                else if (optGetCHM.IsChecked != null && optGetCHM.IsChecked.Value)
                {
                    m_FileToOpen = Path.Combine(System.Windows.Forms.Application.StartupPath, PNStrings.CHM_FILE);
                    downloadFile(PNStrings.URL_DOWNLOAD_ROOT + PNStrings.CHM_FILE);
                }
                else
                {
                    m_FileToOpen = Path.Combine(System.Windows.Forms.Application.StartupPath, PNStrings.PDF_FILE);
                    downloadFile(PNStrings.URL_DOWNLOAD_ROOT + PNStrings.PDF_FILE);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void disableControls()
        {
            try
            {
                lblHelpMissing.IsEnabled =
                    optGetCHM.IsEnabled =
                        optGetPDF.IsEnabled = optGoOnlineHelp.IsEnabled = cmdOK.IsEnabled = cmdCancel.IsEnabled = false;
                elpProgress.Visibility = lblDownloadInProgress.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void downloadFile(string url)
        {
            try
            {
                disableControls();
                m_Progress = lblDownloadInProgress.Text;
                var wr = WebRequest.Create(url);
                try
                {
                    wr.Method = "HEAD";
                    wr.GetResponse();
                }
                catch (Exception ex)
                {
                    PNStatic.LogException(ex);
                    DialogResult = false;
                }
                using (var wc = new WebClient())
                {
                    wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                    wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                    var uri = new Uri(url);

                    wc.DownloadFileAsync(uri, m_FileToOpen);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                lblDownloadInProgress.Text = m_Progress + @" " + e.ProgressPercentage + @"%";
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(m_FileToOpen);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
