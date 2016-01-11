using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using Ionic.Zip;
using Path = System.IO.Path;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndGetThemes.xaml
    /// </summary>
    public partial class WndGetThemes : Window
    {
        public WndGetThemes()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndGetThemes(IEnumerable<ThemesUpdate> listThemes)
            : this()
        {
            PNSingleton.Instance.ThemesDownload = true;
            foreach (var th in listThemes)
            {
                var item = new PNListBoxItem(null, th.FriendlyName + " " + th.Suffix, th, false);
                item.ListBoxItemCheckChanged += item_ListBoxItemCheckChanged;
                lstThemes.Items.Add(item);
            }
        }

        private const string ZIP_SUFFIX = ".zip";

        private List<Tuple<string, string, string, string>> _FilesList;
        private WebClient _WebClient;
        private int _Index;

        private void item_ListBoxItemCheckChanged(object sender, ListBoxItemCheckChangedEventArgs e)
        {
            cmdDownload.IsEnabled = lstThemes.Items.OfType<PNListBoxItem>().Any(it => it.IsChecked != null && it.IsChecked.Value);
        }

        private void DlgGetThemes_Loaded(object sender, RoutedEventArgs e)
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

        private void DlgGetThemes_Unloaded(object sender, RoutedEventArgs e)
        {
            PNSingleton.Instance.ThemesDownload = false;
        }

        private void DlgGetThemes_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) cmdCancel.PerformClick();
        }

        private void cmdDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cmdDownload.IsEnabled = cmdCancel.IsEnabled = false;
                if (!prepareDownloadList() || _FilesList == null || _FilesList.Count == 0) return;
                downloadFiles();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
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

        private void _WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                if (_Index >= _FilesList.Count) return;
                using (var zipFile = new ZipFile(_FilesList[_Index].Item2))
                {
                    zipFile.ExtractAll(Path.Combine(Path.GetTempPath(), PNStrings.TEMP_THEMES_DIR), ExtractExistingFileAction.OverwriteSilently);
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
                    preparePreRunXml();
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private bool prepareDownloadList()
        {
            try
            {
                _FilesList = new List<Tuple<string, string, string, string>>();
                var tempDir = Path.GetTempPath();
                foreach (var item in lstThemes.Items.OfType<PNListBoxItem>().Where(it => it.IsChecked != null && it.IsChecked.Value))
                {
                    var sb = new StringBuilder();
                    var tu = item.Tag as ThemesUpdate;
                    if (tu == null) continue;
                    sb.Append(PNStrings.URL_DOWNLOAD_DIR);
                    sb.Append(tu.Name);
                    sb.Append(ZIP_SUFFIX);
                    var tuple = Tuple.Create(sb.ToString(),
                        Path.Combine(tempDir, tu.Name + ZIP_SUFFIX),
                        Path.Combine(PNPaths.Instance.ThemesDir, tu.FileName), tu.FileName);
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

        private void preparePreRunXml()
        {
            try
            {
                var filePreRun = Path.Combine(Path.GetTempPath(), PNStrings.PRE_RUN_FILE);
                var xdoc = File.Exists(filePreRun) ? XDocument.Load(filePreRun) : new XDocument();
                var xroot = xdoc.Root ?? new XElement(PNStrings.ELM_PRE_RUN);
                var addCopy = false;
                var xcopies = xroot.Element(PNStrings.ELM_COPY_THEMES);
                if (xcopies == null)
                {
                    addCopy = true;
                    xcopies = new XElement(PNStrings.ELM_COPY_THEMES);
                }
                else
                {
                    xcopies.RemoveAll();
                }
                foreach (var tuple in _FilesList)
                {
                    var fromPath =
                        Path.Combine(
                            Path.Combine(Path.Combine(Path.GetTempPath(), PNStrings.TEMP_THEMES_DIR), "themes"),
                            tuple.Item4);
                    var name = string.IsNullOrEmpty(fromPath) ? "" : Path.GetFileName(fromPath);

                    var xc = new XElement(PNStrings.ELM_COPY);
                    xc.Add(new XAttribute(PNStrings.ATT_NAME, tuple.Item4));
                    xc.Add(new XAttribute(PNStrings.ATT_FROM, fromPath));
                    xc.Add(new XAttribute(PNStrings.ATT_TO, tuple.Item3));
                    xcopies.Add(xc);
                }
                if (addCopy)
                {
                    xroot.Add(xcopies);
                }
                if (xdoc.Root == null)
                    xdoc.Add(xroot);
                xdoc.Save(filePreRun);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
