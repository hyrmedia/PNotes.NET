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

using SQLiteWrapper;
using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSearchReplace.xaml
    /// </summary>
    public partial class WndSearchReplace
    {
        public WndSearchReplace()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndSearchReplace(SearchReplace mode, PNRichEdit.PNRichEditBox edit):this()
        {
            _Mode = mode;
            _Edit = edit;
        }

        private readonly SearchReplace _Mode;
        private readonly PNRichEdit.PNRichEditBox _Edit;

        private void enableButtons()
        {
            try
            {
                cmdFindNext.IsEnabled = cboFind.Text.Trim().Length > 0;
                cmdReplace.IsEnabled = cmdReplaceAll.IsEnabled = (cmdFindNext.IsEnabled && cboReplace.Text.Trim().Length > 0);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setFindOptions()
        {
            try
            {
                var options =System.Windows.Forms.RichTextBoxFinds.None;
                if (chkMatchCase.IsChecked != null && chkMatchCase.IsChecked.Value)
                {
                    options |= System.Windows.Forms.RichTextBoxFinds.MatchCase;
                }
                if (chkWholeWord.IsChecked != null && chkWholeWord.IsChecked.Value)
                {
                    options |= System.Windows.Forms.RichTextBoxFinds.WholeWord;
                }
                if (chkSearchUp.IsChecked != null && chkSearchUp.IsChecked.Value)
                {
                    options |= System.Windows.Forms.RichTextBoxFinds.Reverse;
                }
                PNStatic.FindOptions = options;
                PNStatic.SearchMode = chkRegExp.IsChecked != null && chkRegExp.IsChecked.Value ? SearchMode.RegularExp : SearchMode.Normal;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void saveFind()
        {
            try
            {
                var str = cboFind.Text.Trim();
                if (cboFind.Items.Contains(str)) return;
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    var sb = new StringBuilder();
                    sb.Append("UPDATE FIND_REPLACE SET FIND = '");
                    sb.Append(str);
                    foreach (string s in cboFind.Items)
                    {
                        sb.Append(",");
                        sb.Append(s);
                    }
                    sb.Append("'");
                    oData.Execute(sb.ToString());
                    cboFind.Items.Insert(0, str);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void saveReplace()
        {
            try
            {
                var str = cboReplace.Text.Trim();
                if (cboReplace.Items.Contains(str)) return;
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    var sb = new StringBuilder();
                    sb.Append("UPDATE FIND_REPLACE SET REPLACE = '");
                    sb.Append(str);
                    foreach (string s in cboReplace.Items)
                    {
                        sb.Append(",");
                        sb.Append(s);
                    }
                    sb.Append("'");
                    oData.Execute(sb.ToString());
                    cboReplace.Items.Insert(0, str);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private int replaceEditString()
        {
            try
            {
                var position = 0;
                switch (PNStatic.SearchMode)
                {
                    case SearchMode.Normal:
                        position = PNStatic.FindEditString(_Edit, _Edit.SelectionStart);
                        if (position > -1)
                        {
                            _Edit.SelectedText = PNStatic.ReplaceString;
                            position = PNStatic.FindEditString(_Edit, _Edit.SelectionStart);
                            if (position > -1)
                            {
                                _Edit.Select(position, PNStatic.FindString.Length);
                            }
                        }
                        break;
                    case SearchMode.RegularExp:
                        var reverse = (PNStatic.FindOptions & System.Windows.Forms.RichTextBoxFinds.Reverse) == System.Windows.Forms.RichTextBoxFinds.Reverse;
                        position = PNStatic.FindEditStringByRegExp(_Edit, _Edit.SelectionStart, reverse);
                        if (position > -1)
                        {
                            _Edit.SelectedText = PNStatic.ReplaceString;
                            position = PNStatic.FindEditStringByRegExp(_Edit, _Edit.SelectionStart, reverse);
                        }
                        break;
                }

                return position;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return -1;
            }
        }

        private int replaceAllEditStrings()
        {
            try
            {
                var count = 0;
                int position;
                switch (PNStatic.SearchMode)
                {
                    case SearchMode.Normal:
                        position = PNStatic.FindEditString(_Edit, 0);
                        while (position > -1)
                        {
                            count++;
                            _Edit.SelectedText = PNStatic.ReplaceString;
                            position = PNStatic.FindEditString(_Edit, _Edit.SelectionStart);
                        }
                        break;
                    case SearchMode.RegularExp:
                        position = PNStatic.FindEditStringByRegExp(_Edit, 0, false);
                        while (position > -1)
                        {
                            count++;
                            _Edit.SelectedText = PNStatic.ReplaceString;
                            position = PNStatic.FindEditStringByRegExp(_Edit, _Edit.SelectionStart, false);
                        }
                        break;
                }

                return count;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return -1;
            }
        }

        private void DlgSearchReplace_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);

                if (_Mode == SearchReplace.Search)
                {
                    Title = PNLang.Instance.GetCaptionText("dlg_search_find", "Find");
                    lblReplace.Visibility =
                        cboReplace.Visibility =
                            cmdReplace.Visibility = cmdReplaceAll.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("dlg_search_replace", "Replace");
                }
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    using (var t = oData.FillDataTable("SELECT FIND, REPLACE FROM FIND_REPLACE"))
                    {
                        if (t.Rows.Count > 0)
                        {
                            if (!PNData.IsDBNull(t.Rows[0]["FIND"]))
                            {
                                var values = (Convert.ToString(t.Rows[0]["FIND"])).Split(',');
                                foreach (var s in values)
                                    cboFind.Items.Add(s);
                            }
                            if (!PNData.IsDBNull(t.Rows[0]["REPLACE"]))
                            {
                                var values = (Convert.ToString(t.Rows[0]["REPLACE"])).Split(',');
                                foreach (var s in values)
                                    cboReplace.Items.Add(s);
                            }
                        }
                    }
                }
                cboFind.Text = _Edit.SelectionLength > 0 ? _Edit.SelectedText : PNStatic.FindString;
                cboReplace.Text = PNStatic.ReplaceString;
                chkRegExp.IsChecked = PNStatic.SearchMode == SearchMode.RegularExp;
                cboFind.Focus();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdFindNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                setFindOptions();
                saveFind();
                PNStatic.FindString = cboFind.Text.Trim();
                int position;
                switch (PNStatic.SearchMode)
                {
                    case SearchMode.Normal:
                        position = _Edit.SelectionStart == _Edit.TextLength
                            ? PNStatic.FindEditString(_Edit, 0)
                            : PNStatic.FindEditString(_Edit, _Edit.SelectionStart + 1);
                        if (position == -1)
                        {
                            PNMessageBox.Show(PNLang.Instance.GetMessageText("nothing_found", "Nothing found"), PNStrings.PROG_NAME, MessageBoxButton.OK);
                        }
                        break;
                    case SearchMode.RegularExp:
                        position = _Edit.SelectionStart == _Edit.TextLength
                            ? PNStatic.FindEditStringByRegExp(_Edit, 0,
                                (PNStatic.FindOptions & System.Windows.Forms.RichTextBoxFinds.Reverse) ==
                                System.Windows.Forms.RichTextBoxFinds.Reverse)
                            : PNStatic.FindEditStringByRegExp(_Edit, _Edit.SelectionStart + 1,
                                (PNStatic.FindOptions & System.Windows.Forms.RichTextBoxFinds.Reverse) ==
                                System.Windows.Forms.RichTextBoxFinds.Reverse);
                        if (position == -1)
                        {
                            PNMessageBox.Show(PNLang.Instance.GetMessageText("nothing_found", "Nothing found"),
                                PNStrings.PROG_NAME, MessageBoxButton.OK);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdReplace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                setFindOptions();
                saveFind();
                saveReplace();
                PNStatic.FindString = cboFind.Text.Trim();
                PNStatic.ReplaceString = cboReplace.Text.Trim();
                if (replaceEditString() == -1)
                {
                    PNMessageBox.Show(PNLang.Instance.GetMessageText("nothing_found", "Nothing found"), PNStrings.PROG_NAME, MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                setFindOptions();
                saveFind();
                saveReplace();
                PNStatic.FindString = cboFind.Text.Trim();
                PNStatic.ReplaceString = cboReplace.Text.Trim();
                var count = replaceAllEditStrings();
                var message = PNLang.Instance.GetMessageText("replace_complete", "Search and replace complete.");
                message += '\n';
                message += PNLang.Instance.GetMessageText("matches_replaced", "Matches replaced:");
                message += " " + count.ToString(CultureInfo.InvariantCulture);
                PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgSearchReplace_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void cboFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableButtons();
        }

        private void cboReplace_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableButtons();
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void chkRegExp_Checked(object sender, RoutedEventArgs e)
        {
            if (chkRegExp.IsChecked != null) chkWholeWord.IsEnabled = !chkRegExp.IsChecked.Value;
        }
    }
}
