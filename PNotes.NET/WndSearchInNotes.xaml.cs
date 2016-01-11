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

using PNRichEdit;
using SQLiteWrapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSearchInNotes.xaml
    /// </summary>
    public partial class WndSearchInNotes
    {
        public WndSearchInNotes()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private readonly PNRichEditBox _HiddenEdit = new PNRichEditBox
        {
            BorderStyle = System.Windows.Forms.BorderStyle.None,
            ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None
        };
        private string _Column;
        private SearchCriteria _Criteria = SearchCriteria.None;
        private string _Line;
        private SearchScope _Scope = SearchScope.None;
        private string _TextAndTitle;
        private string _TextOnly;
        private string _TitleOnly;
        private readonly ObservableCollection<PNTreeItem> _FoundItems = new ObservableCollection<PNTreeItem>();

        private void DlgSearchInNotes_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                applyLanguage();
                PNStatic.FormMain.LanguageChanged += FormMain_LanguageChanged;
                PNStatic.FormMain.NoteBooleanChanged += FormMain_NoteBooleanChanged;
                PNStatic.FormMain.NoteNameChanged += FormMain_NoteNameChanged;
                PNStatic.FormMain.NoteScheduleChanged += FormMain_NoteScheduleChanged;
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    using (var t = oData.FillDataTable("SELECT FIND, REPLACE FROM FIND_REPLACE"))
                    {
                        if (t.Rows.Count <= 0) return;
                        if (!PNData.IsDBNull(t.Rows[0]["FIND"]))
                        {
                            var values = Convert.ToString(t.Rows[0]["FIND"]).Split(',');
                            foreach (var s in values)
                                cboFind.Items.Add(s);
                        }
                        if (!PNData.IsDBNull(t.Rows[0]["REPLACE"]))
                        {
                            var values = Convert.ToString(t.Rows[0]["REPLACE"]).Split(',');
                            foreach (var s in values)
                                cboReplace.Items.Add(s);
                        }
                    }
                }
                tvwResults.ItemsSource = _FoundItems;
                setSettings();
                cboFind.Focus();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgSearchInNotes_Closed(object sender, EventArgs e)
        {
            PNStatic.FormMain.LanguageChanged -= FormMain_LanguageChanged;
            PNStatic.FormMain.NoteBooleanChanged -= FormMain_NoteBooleanChanged;
            PNStatic.FormMain.NoteNameChanged -= FormMain_NoteNameChanged;
            PNStatic.FormMain.NoteScheduleChanged -= FormMain_NoteScheduleChanged;
            PNStatic.FormSearchInNotes = null;
        }

        private void DlgSearchInNotes_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Combo_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableReplaceButton();
        }

        private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            enableReplaceButton();
        }

        private void cmdReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _FoundItems.Clear();
                if (chkIncludeHidden.IsChecked != null && chkIncludeHidden.IsChecked.Value)
                {
                    string message = PNLang.Instance.GetMessageText("replace_hidden_1", "You have selected option");
                    message += " \"" + chkIncludeHidden.Content + "\"\n";
                    message += PNLang.Instance.GetMessageText("replace_hidden_2",
                                                              "All matched entries in hidden notes will be replaced and saved. Continue?");
                    if (
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                        MessageBoxResult.No)
                    {
                        return;
                    }
                }
                saveSettings();
                startReplace();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdFind_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                saveFind();
                saveSettings();
                startFind();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdClearHistory_Click(object sender, RoutedEventArgs e)
        {
            clearSearchHistory();
        }

        private void FormMain_NoteNameChanged(object sender, NoteNameChangedEventArgs e)
        {
            var note = sender as PNote;
            changeNodeName(note);
        }

        private void FormMain_NoteBooleanChanged(object sender, NoteBooleanChangedEventArgs e)
        {
            var note = sender as PNote;
            changeNodeImage(note);
        }

        void FormMain_NoteScheduleChanged(object sender, EventArgs e)
        {
            var note = sender as PNote;
            changeNodeImage(note);
        }

        private void FormMain_LanguageChanged(object sender, EventArgs e)
        {
            applyLanguage();
        }

        private void changeNodeName(PNote note)
        {
            try
            {
                foreach (
                    var tn in
                        tvwResults.Items.OfType<PNTreeItem>().Where(tn => tn.Tag != null && (string)tn.Tag == note.ID))
                {
                    tn.Text = note.Name;
                    break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void changeNodeImage(PNote note)
        {
            try
            {
                foreach (
                    var tn in
                        tvwResults.Items.OfType<PNTreeItem>().Where(tn => tn.Tag != null && (string)tn.Tag == note.ID))
                {
                    tn.SetImageResource(PNNotesOperations.GetNoteImage(note));
                    break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyLanguage()
        {
            try
            {
                var criteria = cboSearchСriteria.SelectedIndex;
                var scope = cboSearchScope.SelectedIndex;

                PNLang.Instance.ApplyControlLanguage(this);
                Title = PNLang.Instance.GetCaptionText("search_in_notes", "Search in notes");
                _Line = PNLang.Instance.GetMiscText("line", "Line:");
                _Column = PNLang.Instance.GetMiscText("column", "Column:");
                _TextAndTitle = PNLang.Instance.GetMiscText("text_and_title", "(text and title)");
                _TitleOnly = PNLang.Instance.GetMiscText("title_only", "(title only)");
                _TextOnly = PNLang.Instance.GetMiscText("text_only", "(text_only)");
                cboSearchСriteria.SelectedIndex = criteria == -1 ? 0 : criteria;
                cboSearchScope.SelectedIndex = scope == -1 ? 0 : scope;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void enableReplaceButton()
        {
            try
            {
                cmdReplaceAll.IsEnabled = cboFind.Text.Trim().Length > 0 && cboReplace.Text.Trim().Length > 0 &&
                                        _Scope == SearchScope.Text && _Criteria == SearchCriteria.EntireString;
                cmdFind.IsEnabled = cboFind.Text.Trim().Length > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setSettings()
        {
            try
            {
                chkWholeWord.IsChecked = PNStatic.Settings.Config.SearchNotesSettings.WholewWord;
                chkMatchCase.IsChecked = PNStatic.Settings.Config.SearchNotesSettings.MatchCase;
                chkIncludeHidden.IsChecked = PNStatic.Settings.Config.SearchNotesSettings.IncludeHidden;
                cboSearchСriteria.SelectedIndex = PNStatic.Settings.Config.SearchNotesSettings.Criteria;
                cboSearchScope.SelectedIndex = PNStatic.Settings.Config.SearchNotesSettings.Scope;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void saveSettings()
        {
            try
            {
                if (chkWholeWord.IsChecked != null)
                    PNStatic.Settings.Config.SearchNotesSettings.WholewWord = chkWholeWord.IsChecked.Value;
                if (chkMatchCase.IsChecked != null)
                    PNStatic.Settings.Config.SearchNotesSettings.MatchCase = chkMatchCase.IsChecked.Value;
                if (chkIncludeHidden.IsChecked != null)
                    PNStatic.Settings.Config.SearchNotesSettings.IncludeHidden = chkIncludeHidden.IsChecked.Value;
                PNStatic.Settings.Config.SearchNotesSettings.Criteria = cboSearchСriteria.SelectedIndex;
                PNStatic.Settings.Config.SearchNotesSettings.Scope = cboSearchScope.SelectedIndex;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private string saveFind()
        {
            try
            {
                var str = cboFind.Text.Trim();
                if (cboFind.Items.Contains(str)) return str;
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
                return str;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private void startReplace()
        {
            try
            {
                var searchString = saveFind();
                var replaceString = saveReplace();
                var options = System.Windows.Forms.RichTextBoxFinds.NoHighlight;
                var total = 0;

                if (!PNStatic.Settings.GeneralSettings.AutoHeight ||
                                         PNStatic.Settings.GeneralSettings.UseSkins)
                {
                    _HiddenEdit.ScrollBars = PNStatic.Settings.GeneralSettings.ShowScrollbar;
                }
                else
                {
                    _HiddenEdit.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
                }
                _HiddenEdit.WordWrap = _HiddenEdit.ScrollBars != System.Windows.Forms.RichTextBoxScrollBars.Horizontal &&
                                       _HiddenEdit.ScrollBars != System.Windows.Forms.RichTextBoxScrollBars.Both;

                if (!PNStatic.Settings.GeneralSettings.UseSkins)
                {
                    _HiddenEdit.SetMargins(PNStatic.Settings.GeneralSettings.MarginWidth);
                }

                if (chkMatchCase.IsChecked != null && chkMatchCase.IsChecked.Value)
                {
                    options |= System.Windows.Forms.RichTextBoxFinds.MatchCase;
                }
                if (chkWholeWord.IsChecked != null && chkWholeWord.IsChecked.Value)
                {
                    options |= System.Windows.Forms.RichTextBoxFinds.WholeWord;
                }

                var notes = chkIncludeHidden.IsChecked != null && chkIncludeHidden.IsChecked.Value
                                        ? PNStatic.Notes.Where(n => n.GroupID != (int)SpecialGroups.RecycleBin)
                                        : PNStatic.Notes.Where(n => n.GroupID != (int)SpecialGroups.RecycleBin && n.Visible);
                foreach (var note in notes)
                {
                    var count = 0;
                    replaceEntireString(note, searchString, replaceString, options, ref count);
                    total += count;
                }
                var message = PNLang.Instance.GetMessageText("replace_complete", "Search and replace complete.");
                if (total > 0)
                {
                    message += '\n' + PNLang.Instance.GetMessageText("matches_replaced", "Matches replaced:") + " " +
                               total.ToString(CultureInfo.InvariantCulture);
                    tvwResults.ExpandAll();
                }
                else
                {
                    message += '\n' + PNLang.Instance.GetMessageText("nothing_found", "Nothing found.");
                }
                PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void startFind()
        {
            try
            {
                var searchString = saveFind();
                var options = System.Windows.Forms.RichTextBoxFinds.NoHighlight;
                var total = 0;

                _FoundItems.Clear();

                if (!PNStatic.Settings.GeneralSettings.AutoHeight ||
                                         PNStatic.Settings.GeneralSettings.UseSkins)
                {
                    _HiddenEdit.ScrollBars = PNStatic.Settings.GeneralSettings.ShowScrollbar;
                }
                else
                {
                    _HiddenEdit.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
                }
                _HiddenEdit.WordWrap = _HiddenEdit.ScrollBars != System.Windows.Forms.RichTextBoxScrollBars.Horizontal &&
                                       _HiddenEdit.ScrollBars != System.Windows.Forms.RichTextBoxScrollBars.Both;

                if (!PNStatic.Settings.GeneralSettings.UseSkins)
                {
                    _HiddenEdit.SetMargins(PNStatic.Settings.GeneralSettings.MarginWidth);
                }

                if (chkMatchCase.IsChecked != null && chkMatchCase.IsChecked.Value)
                {
                    options |= System.Windows.Forms.RichTextBoxFinds.MatchCase;
                }
                if (chkWholeWord.IsChecked != null && chkWholeWord.IsChecked.Value)
                {
                    options |= System.Windows.Forms.RichTextBoxFinds.WholeWord;
                }

                var notes = chkIncludeHidden.IsChecked != null && chkIncludeHidden.IsChecked.Value
                                        ? PNStatic.Notes.Where(n => n.GroupID != (int)SpecialGroups.RecycleBin)
                                        : PNStatic.Notes.Where(n => n.GroupID != (int)SpecialGroups.RecycleBin && n.Visible);
                foreach (var note in notes)
                {
                    var count = 0;
                    switch (_Criteria)
                    {
                        case SearchCriteria.EntireString:
                            findInNoteEntireString(note, searchString, options, ref count);
                            break;
                        case SearchCriteria.EveryWord:
                            findInNoteEveryWord(note, searchString, options, ref count);
                            break;
                        case SearchCriteria.AtLeastOneWord:
                            findInNoteAtLeastOneWord(note, searchString, options, ref count);
                            break;
                    }
                    total += count;
                }
                var message = PNLang.Instance.GetMessageText("search_complete", "Search complete.");
                if (total > 0)
                {
                    message += '\n' + PNLang.Instance.GetMessageText("matches_found", "Matches found:") + " " +
                               total.ToString(CultureInfo.InvariantCulture);
                    tvwResults.ExpandAll();
                }
                else
                {
                    message += '\n' + PNLang.Instance.GetMessageText("nothing_found", "Nothing found.");
                }
                PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private string saveReplace()
        {
            try
            {
                var str = cboReplace.Text.Trim();
                if (cboReplace.Items.Contains(str)) return str;
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
                return str;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private void replaceEntireString(PNote note, string searchString, string replaceString, System.Windows.Forms.RichTextBoxFinds options,
                                         ref int count)
        {
            try
            {
                var start = 0;
                var foundText = false;
                PNRichEditBox edit;

                if (note.Visible)
                {
                    edit = note.Dialog.Edit;
                }
                else
                {
                    var path = Path.Combine(PNPaths.Instance.DataDir, note.ID + PNStrings.NOTE_EXTENSION);
                    _HiddenEdit.Size = note.EditSize;
                    PNNotesOperations.LoadNoteFile(_HiddenEdit, path);
                    edit = _HiddenEdit;
                }

                var stop = edit.TextLength;
                var index = edit.Find(searchString, start, options);
                if (index > -1)
                {
                    foundText = true;
                    var current = 0;
                    while (index > -1 && current <= index)
                    {
                        count++;
                        current = index;

                        edit.Select(index, searchString.Length);
                        edit.SelectedText = replaceString;

                        if (searchString.Length > 1)
                        {
                            start = index + searchString.Length - 1;
                        }
                        else
                        {
                            start = index + searchString.Length;
                        }
                        if (start >= stop)
                        {
                            break;
                        }
                        index = edit.Find(searchString, start, options);
                    }
                }
                //save hidden note
                if (foundText && !note.Visible)
                {
                    var path = Path.Combine(PNPaths.Instance.DataDir, note.ID + PNStrings.NOTE_EXTENSION);
                    PNNotesOperations.SaveNoteFile(edit, path);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void findInNoteEntireString(PNote note, string searchString, System.Windows.Forms.RichTextBoxFinds options, ref int count)
        {
            try
            {
                var start = 0;
                bool foundTitle = false, foundText = false;
                PNRichEditBox edit;
                var key = PNNotesOperations.GetNoteImage(note);
                var tn = new PNTreeItem(key, note.Name, note.ID);

                switch (_Scope)
                {
                    case SearchScope.Titles:
                        if (note.Name.IndexOf(searchString, StringComparison.Ordinal) < 0) return;
                        count++;
                        _FoundItems.Add(tn);
                        return;
                    case SearchScope.TextAndTitles:
                        if (note.Name.IndexOf(searchString, StringComparison.Ordinal) >= 0)
                        {
                            count++;
                            foundTitle = true;
                        }
                        break;
                }

                if (note.Visible)
                {
                    edit = note.Dialog.Edit;
                }
                else
                {
                    var path = Path.Combine(PNPaths.Instance.DataDir, note.ID + PNStrings.NOTE_EXTENSION);
                    _HiddenEdit.Size = note.EditSize;
                    PNNotesOperations.LoadNoteFile(_HiddenEdit, path);
                    edit = _HiddenEdit;
                }

                var stop = edit.TextLength;
                var index = edit.Find(searchString, start, options);
                if (index > -1)
                {
                    foundText = true;
                    var current = 0;
                    while (index > -1 && current <= index)
                    {
                        count++;
                        current = index;
                        var line = edit.GetLineFromCharIndex(index);
                        var col = index - edit.GetFirstCharIndexFromLine(line);
                        var t = new PNTreeItem("searchloc",
                            searchString + " (" + _Line + " " + (line + 1).ToString(CultureInfo.InvariantCulture) +
                            ", " + _Column + " " + (col + 1).ToString(CultureInfo.InvariantCulture) + ")",
                            new[] { index, searchString.Length });
                        tn.Items.Add(t);
                        if (searchString.Length > 1)
                        {
                            start = index + searchString.Length - 1;
                        }
                        else
                        {
                            start = index + searchString.Length;
                        }
                        if (start >= stop)
                        {
                            break;
                        }
                        index = edit.Find(searchString, start, options);
                    }
                }
                if (!foundTitle && !foundText) return;
                if (_Scope == SearchScope.TextAndTitles)
                {
                    if (foundTitle && foundText)
                    {
                        tn.Text += @" " + _TextAndTitle;
                    }
                    else if (foundTitle)
                    {
                        tn.Text += @" " + _TitleOnly;
                    }
                    else
                    {
                        tn.Text += @" " + _TextOnly;
                    }
                }
                _FoundItems.Add(tn);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void findInNoteEveryWord(PNote note, string searchString, System.Windows.Forms.RichTextBoxFinds options, ref int count)
        {
            try
            {
                var foundTitle = false;
                PNRichEditBox edit;
                var key = PNNotesOperations.GetNoteImage(note);
                var tn = new PNTreeItem(key, note.Name, note.ID);
                var strings = searchString.Split(' ');
                var counter = strings.Length;

                switch (_Scope)
                {
                    case SearchScope.Titles:
                        {
                            var temp = counter;
                            foreach (var s in strings.Where(s => note.Name.IndexOf(s, StringComparison.Ordinal) >= 0))
                            {
                                temp--;
                            }
                            if (temp != 0) return;
                            count++;
                            _FoundItems.Add(tn);
                            return;
                        }
                    case SearchScope.TextAndTitles:
                        {
                            var temp = counter;
                            foreach (var s in strings.Where(s => note.Name.IndexOf(s, StringComparison.Ordinal) >= 0))
                            {
                                temp--;
                            }
                            if (temp == 0)
                            {
                                count++;
                                foundTitle = true;
                            }
                        }
                        break;
                }

                if (note.Visible)
                {
                    edit = note.Dialog.Edit;
                }
                else
                {
                    var path = Path.Combine(PNPaths.Instance.DataDir, note.ID + PNStrings.NOTE_EXTENSION);
                    _HiddenEdit.Size = note.EditSize;
                    PNNotesOperations.LoadNoteFile(_HiddenEdit, path);
                    edit = _HiddenEdit;
                }

                var tempNodes = new List<PNTreeItem>();
                var tempCount = 0;

                var stop = edit.TextLength;
                foreach (var s in strings)
                {
                    var start = 0;
                    var index = edit.Find(s, start, options);
                    if (index <= -1) continue;
                    counter--;
                    var current = 0;
                    while (index > -1 && current <= index)
                    {
                        tempCount++;
                        current = index;
                        var line = edit.GetLineFromCharIndex(index);
                        var col = index - edit.GetFirstCharIndexFromLine(line);
                        var t = new PNTreeItem("searchloc",
                            s + " (" + _Line + " " + (line + 1).ToString(CultureInfo.InvariantCulture) + ", " +
                            _Column + " " +
                            (col + 1).ToString(CultureInfo.InvariantCulture) + ")", new[] { index, s.Length });
                        tempNodes.Add(t);
                        if (s.Length > 1)
                        {
                            start = index + s.Length - 1;
                        }
                        else
                        {
                            start = index + s.Length;
                        }
                        if (start >= stop)
                        {
                            break;
                        }
                        index = edit.Find(s, start, options);
                    }
                }
                if (counter == 0 || foundTitle)
                {
                    if (counter == 0)
                    {
                        foreach (var t in tempNodes)
                            tn.Items.Add(t);
                        count += tempCount;
                    }
                    if (_Scope == SearchScope.TextAndTitles)
                    {
                        if (counter == 0 && foundTitle)
                        {
                            tn.Text += @" " + _TextAndTitle;
                        }
                        else if (counter == 0)
                        {
                            tn.Text += @" " + _TextOnly;
                        }
                        else
                        {
                            tn.Text += @" " + _TitleOnly;
                        }
                    }
                    _FoundItems.Add(tn);
                    return;
                }
                count = 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void findInNoteAtLeastOneWord(PNote note, string searchString, System.Windows.Forms.RichTextBoxFinds options, ref int count)
        {
            try
            {
                var foundTitle = false;
                PNRichEditBox edit;
                var strings = searchString.Split(' ');
                var counter = 0;
                var key = PNNotesOperations.GetNoteImage(note);
                var tn = new PNTreeItem(key, note.Name, note.ID);

                switch (_Scope)
                {
                    case SearchScope.Titles:
                        if (!strings.Any(s => note.Name.Contains(s))) return;
                        count++;
                        _FoundItems.Add(tn);
                        return;
                    case SearchScope.TextAndTitles:
                        if (strings.Any(s => note.Name.Contains(s)))
                        {
                            count++;
                            foundTitle = true;
                        }
                        break;
                }

                if (note.Visible)
                {
                    edit = note.Dialog.Edit;
                }
                else
                {
                    var path = Path.Combine(PNPaths.Instance.DataDir, note.ID + PNStrings.NOTE_EXTENSION);
                    _HiddenEdit.Size = note.EditSize;
                    PNNotesOperations.LoadNoteFile(_HiddenEdit, path);
                    edit = _HiddenEdit;
                }

                var stop = edit.TextLength;
                foreach (var s in strings)
                {
                    var start = 0;
                    var index = edit.Find(s, start, options);
                    if (index <= -1) continue;
                    counter++;
                    var current = 0;
                    while (index > -1 && current <= index)
                    {
                        count++;
                        current = index;
                        var line = edit.GetLineFromCharIndex(index);
                        var col = index - edit.GetFirstCharIndexFromLine(line);
                        var t = new PNTreeItem("searchloc",
                            s + " (" + _Line + " " + (line + 1).ToString(CultureInfo.InvariantCulture) + ", " +
                            _Column + " " +
                            (col + 1).ToString(CultureInfo.InvariantCulture) + ")", new[] { index, s.Length });
                        tn.Items.Add(t);
                        if (s.Length > 1)
                        {
                            start = index + s.Length - 1;
                        }
                        else
                        {
                            start = index + s.Length;
                        }
                        if (start >= stop)
                        {
                            break;
                        }
                        index = edit.Find(s, start, options);
                    }
                }
                if (counter > 0 || foundTitle)
                {
                    if (_Scope == SearchScope.TextAndTitles)
                    {
                        if (counter > 0 && foundTitle)
                        {
                            tn.Text += @" " + _TextAndTitle;
                        }
                        else if (counter > 0)
                        {
                            tn.Text += @" " + _TextOnly;
                        }
                        else
                        {
                            tn.Text += @" " + _TitleOnly;
                        }
                    }
                    _FoundItems.Add(tn);
                    return;
                }
                count = 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void clearSearchHistory()
        {
            try
            {
                var message = PNLang.Instance.GetMessageText("clear_search_history", "Clear search history?");
                if (PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    oData.Execute("UPDATE FIND_REPLACE SET FIND = NULL, REPLACE = NULL");
                    cboFind.Items.Clear();
                    cboReplace.Items.Clear();
                    cboFind.Text = "";
                    cboReplace.Text = "";
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboSearchСriteria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _Criteria = (SearchCriteria)cboSearchСriteria.SelectedIndex;
                enableReplaceButton();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboSearchScope_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _Scope = (SearchScope)cboSearchScope.SelectedIndex;
                enableReplaceButton();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void PNTreeView_PNTreeViewLeftMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = tvwResults.GetHierarchyObjectAtPoint<PNTreeItem>(e.GetPosition(tvwResults)) as PNTreeItem;
                if (item == null) return;
                if (item.Parent == null)
                {
                    var note = PNStatic.Notes.Note((string)item.Tag);
                    if (note != null)
                    {
                        PNNotesOperations.ShowHideSpecificNote(note, true);
                    }
                }
                else if (item.Tag != null)
                {
                    var parent = item.Parent as PNTreeItem;
                    if (parent == null) return;
                    var note = PNStatic.Notes.Note((string)parent.Tag);
                    if (note == null) return;
                    if (PNNotesOperations.ShowHideSpecificNote(note, true) != ShowHideResult.Success) return;
                    var range = (int[])item.Tag;
                    note.Dialog.Edit.SelectionStart = range[0];
                    note.Dialog.Edit.SelectionLength = range[1];
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdClearSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PNStatic.Settings.Config.SearchNotesSettings = new SearchNotesPrefs();
                PNData.SaveSearchNotesSettings();
                setSettings();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
