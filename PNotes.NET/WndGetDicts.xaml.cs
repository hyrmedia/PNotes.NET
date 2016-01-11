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

using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndGetDicts.xaml
    /// </summary>
    public partial class WndGetDicts
    {
        public WndGetDicts()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndGetDicts(IEnumerable<DictData> list)
            : this()
        {
            foreach (var dc in list)
            {
                var item = new PNListBoxItem(null, dc.LangName, dc.ZipFile, false);
                item.ListBoxItemCheckChanged += item_ListBoxItemCheckChanged;
                lstDicts.Items.Add(item);
            }
        }

        private List<Tuple<string, string>> _FilesList;
        private WebClient _WebClient;
        private int _Index;

        private void item_ListBoxItemCheckChanged(object sender, ListBoxItemCheckChangedEventArgs e)
        {
            cmdDownload.IsEnabled = lstDicts.Items.OfType<PNListBoxItem>().Any(it => it.IsChecked != null && it.IsChecked.Value);
        }

        private void DlgGetDicts_Loaded(object sender, RoutedEventArgs e)
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

        private void DlgGetDicts_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) cmdCancel.PerformClick();
        }

        private void cmdDownload_Click(object sender, RoutedEventArgs e)
        {
            cmdDownload.IsEnabled = cmdCancel.IsEnabled = false;
            if (!prepareDownloadList() || _FilesList == null || _FilesList.Count == 0) return;
            downloadFiles();
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void downloadFiles()
        {
            try
            {
                _WebClient = new WebClient();
                _WebClient.DownloadFileCompleted += _WebClient_DownloadFileCompleted;
                _WebClient.DownloadProgressChanged += _WebClient_DownloadProgressChanged;
                if (File.Exists(_FilesList[0].Item2)) File.Delete(_FilesList[0].Item2);
                _WebClient.DownloadFileAsync(new Uri(_FilesList[0].Item1), _FilesList[0].Item2,
                                             Path.GetFileName(_FilesList[0].Item2));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void _WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                lblDownload.Text = Convert.ToString(e.UserState) + @" " +
                                       e.ProgressPercentage.ToString(PNStatic.CultureInvariant) + @"%";
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void _WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                if (_Index >= _FilesList.Count) return;
                using (var zipFile = ZipFile.Read(_FilesList[_Index].Item2))
                {
                    zipFile.ExtractSelectedEntries("name = *.aff or name = *.dic", null,
                        Path.Combine(Path.GetTempPath(), PNStrings.TEMP_DICT_DIR),
                        ExtractExistingFileAction.OverwriteSilently);
                }
                File.Delete(_FilesList[_Index].Item2);
                _Index++;
                if (_Index < _FilesList.Count)
                {
                    if (File.Exists(_FilesList[_Index].Item2)) File.Delete(_FilesList[_Index].Item2);
                    _WebClient.DownloadFileAsync(new Uri(_FilesList[_Index].Item1), _FilesList[_Index].Item2,
                        Path.GetFileName(_FilesList[_Index].Item2));
                }
                else
                {
                    cmdCancel.IsEnabled = false;
                    var di = new DirectoryInfo(Path.Combine(Path.GetTempPath(), PNStrings.TEMP_DICT_DIR));
                    var files = di.GetFiles().Select(f => f.FullName);
                    foreach (var f in files)
                    {
                        if (string.IsNullOrEmpty(f)) continue;
                        var destFile = Path.Combine(PNPaths.Instance.DictDir, Path.GetFileName(f));
                        File.Copy(f, destFile, true);
                        File.Delete(f);
                    }
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool prepareDownloadList()
        {
            try
            {
                _FilesList = new List<Tuple<string, string>>();
                var tempDir = Path.GetTempPath();
                foreach (
                    var item in
                        lstDicts.Items.OfType<PNListBoxItem>().Where(it => it.IsChecked != null && it.IsChecked.Value))
                {
                    var sb = new StringBuilder(PNStrings.URL_DICT_FTP);
                    sb.Append("/");
                    sb.Append(item.Tag);
                    var tuple = Tuple.Create(sb.ToString(), Path.Combine(tempDir, item.Tag.ToString()));
                    _FilesList.Add(tuple);
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void cmdBrowser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PNStatic.LoadPage(PNStrings.URL_DICTIONARIES);
                DialogResult = false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
