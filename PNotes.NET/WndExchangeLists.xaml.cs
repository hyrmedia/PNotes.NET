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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndTags.xaml
    /// </summary>
    public partial class WndExchangeLists
    {
        public WndExchangeLists()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndExchangeLists(string id, ExchangeLists mode)
            : this()
        {
            _ID = id;
            _Mode = mode;
        }

        private readonly string _ID = "";
        private readonly ExchangeLists _Mode;
        private readonly SortedList<string, TextBlock> _ValuesAv = new SortedList<string, TextBlock>();
        private readonly SortedList<string, TextBlock> _ValuesCurr = new SortedList<string, TextBlock>();

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(_ID);
                if (note != null)
                {
                    switch (_Mode)
                    {
                        case ExchangeLists.Tags:
                            note.Tags.Clear();
                            foreach (var n in lstCurrent.Items.OfType<KeyValuePair<string, TextBlock>>())
                            {
                                note.Tags.Add(n.Value.Text);
                            }
                            PNNotesOperations.SaveNoteTags(note);
                            note.RaiseTagsChangedEvent();
                            break;
                        case ExchangeLists.LinkedNotes:
                            note.LinkedNotes.Clear();
                            foreach (var n in lstCurrent.Items.OfType<KeyValuePair<string, TextBlock>>())
                            {
                                note.LinkedNotes.Add((string)n.Value.Tag);
                            }
                            PNNotesOperations.SaveLinkedNotes(note);
                            break;
                    }
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgTags_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(_ID);
                if (note == null) return;
                if (_Mode == ExchangeLists.LinkedNotes)
                {
                    lblAvailableTags.Name = "lblAvailableLinks";
                    lblCurrentTags.Name = "lblCurrentLinks";
                }
                PNLang.Instance.ApplyControlLanguage(this);
                switch (_Mode)
                {
                    case ExchangeLists.Tags:
                        Title = PNLang.Instance.GetCaptionText("tags", "Tags") + @" - [" + note.Name + @"]";
                        var tags = PNStatic.Tags.Where(t => !note.Tags.Contains(t));
                        foreach (var t in tags)
                        {
                            _ValuesAv.Add(t, new TextBlock {Tag = t, Text = t, Margin = new Thickness(4)});
                        }
                        foreach (var t in note.Tags)
                        {
                            _ValuesCurr.Add(t, new TextBlock { Tag = t, Text = t, Margin = new Thickness(4) });
                        }
                        break;
                    case ExchangeLists.LinkedNotes:
                        Title = PNLang.Instance.GetCaptionText("linked_notes", "Linked notes") + @" - [" + note.Name + @"]";
                        var links = PNStatic.Notes.Where(n => n.GroupID != (int)SpecialGroups.RecycleBin && n.ID != _ID && !note.LinkedNotes.Contains(n.ID));
                        foreach (var n in links)
                        {
                            _ValuesAv.Add(n.ID, new TextBlock { Tag = n.ID, Text = n.Name, Margin = new Thickness(4) });
                        }
                        foreach (var n in note.LinkedNotes.Select(l => PNStatic.Notes.Note(l)).Where(n => n != null))
                        {
                            _ValuesCurr.Add(n.ID, new TextBlock { Tag = n.ID, Text = n.Name, Margin = new Thickness(4) });
                        }
                        break;
                }

                lstAvailabe.ItemsSource = _ValuesAv;
                lstAvailabe.DisplayMemberPath = "(Value).(Text)";
                lstCurrent.ItemsSource = _ValuesCurr;
                lstCurrent.DisplayMemberPath = "(Value).(Text)";
                enableButtons();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAvToCurr_Click(object sender, RoutedEventArgs e)
        {
            moveAvToCurr();
        }

        private void cmdCurrToAv_Click(object sender, RoutedEventArgs e)
        {
            moveCurrToAv();
        }

        private void lstAvailabe_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            moveAvToCurr();
        }

        private void lstCurrent_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            moveCurrToAv();
        }

        private void lstAvailabe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            enableButtons();
        }

        private void lstCurrent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            enableButtons();
        }

        private void enableButtons()
        {
            cmdAvToCurr.IsEnabled = lstAvailabe.Items.Count > 0 && lstAvailabe.SelectedIndex > -1;
            cmdCurrToAv.IsEnabled = lstCurrent.Items.Count > 0 && lstCurrent.SelectedIndex > -1;
        }

        private void moveAvToCurr()
        {
            try
            {
                var count = lstAvailabe.SelectedItems.Count;
                for (var i = count - 1; i >= 0; i--)
                {
                    var item = _ValuesAv.FirstOrDefault(t => t.Key == (string)((KeyValuePair<string, TextBlock>)lstAvailabe.SelectedItems[i]).Value.Tag);
                    if (item.Equals(default(KeyValuePair<string, TextBlock>))) continue;
                    _ValuesCurr.Add(item.Key, item.Value);
                    _ValuesAv.Remove(item.Key);
                }
                if (lstAvailabe.Items.Count > 0)
                {
                    lstAvailabe.SelectedIndex = lstAvailabe.Items.Count - 1;
                }
                lstCurrent.Items.Refresh();
                lstAvailabe.Items.Refresh();
                enableButtons();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void moveCurrToAv()
        {
            try
            {
                var count = lstCurrent.SelectedItems.Count;
                for (var i = count - 1; i >= 0; i--)
                {
                    var item = _ValuesCurr.FirstOrDefault(t => t.Key == (string)((KeyValuePair<string, TextBlock>)lstCurrent.SelectedItems[i]).Value.Tag);
                    if (item.Equals(default(KeyValuePair<string, TextBlock>))) continue;
                    _ValuesAv.Add(item.Key, item.Value);
                    _ValuesCurr.Remove(item.Key);
                }
                if (lstCurrent.Items.Count > 0)
                {
                    lstCurrent.SelectedIndex = lstCurrent.Items.Count - 1;
                }
                lstCurrent.Items.Refresh();
                lstAvailabe.Items.Refresh();
                enableButtons();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

    }
}
