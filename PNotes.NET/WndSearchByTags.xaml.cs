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

using PNotes.NET.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSearchByTags.xaml
    /// </summary>
    public partial class WndSearchByTags
    {
        public WndSearchByTags()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal class AvTag : INotifyPropertyChanged
        {
            private bool _Selected;

            public bool Selected
            {
                get { return _Selected; }
                set
                {
                    if (_Selected == value) return;
                    _Selected = value;
                    OnPropertyChanged("Selected");
                }
            }

            public string Tag { get; private set; }

            public AvTag(string tag)
            {
                Tag = tag;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal class FoundNote : INotifyPropertyChanged
        {
            private string _iconSource;
            private string _Name;
            private string _Tags;

            public string IconSource
            {
                get { return _iconSource; }
                set
                {
                    if (Equals(_iconSource, value)) return;
                    _iconSource = value;
                    OnPropertyChanged("IconSource");
                }
            }

            public string Name
            {
                get { return _Name; }
                set
                {
                    if (_Name == value) return;
                    _Name = value;
                    OnPropertyChanged("Name");
                }
            }

            public string Tags
            {
                get { return _Tags; }
                set
                {
                    if (value == _Tags) return;
                    _Tags = value;
                    OnPropertyChanged("Tags");
                }
            }

            public string Id { get; private set; }

            public FoundNote(string image, string name, string tags, string id)
            {
                IconSource = image;
                Name = name;
                Tags = tags;
                Id = id;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private readonly List<AvTag> _Tags = new List<AvTag>();
        private readonly ObservableCollection<FoundNote> _Notes = new ObservableCollection<FoundNote>();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                Title = PNLang.Instance.GetCaptionText("serach_tags", "Search by tags");
                loadTags();
                PNStatic.FormMain.NoteBooleanChanged += FormMain_NoteBooleanChanged;
                PNStatic.FormMain.NoteNameChanged += FormMain_NoteNameChanged;
                PNStatic.FormMain.NoteTagsChanged += FormMain_NoteTagsChanged;
                PNStatic.FormMain.LanguageChanged += FormMain_LanguageChanged;
                PNStatic.FormMain.NoteScheduleChanged += FormMain_NoteScheduleChanged;
                grdTagsResults.ItemsSource = _Notes;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            PNStatic.FormMain.NoteBooleanChanged -= FormMain_NoteBooleanChanged;
            PNStatic.FormMain.NoteNameChanged -= FormMain_NoteNameChanged;
            PNStatic.FormMain.NoteTagsChanged -= FormMain_NoteTagsChanged;
            PNStatic.FormMain.LanguageChanged -= FormMain_LanguageChanged;
            PNStatic.FormMain.NoteScheduleChanged -= FormMain_NoteScheduleChanged;
            PNStatic.FormSearchByTags = null;
        }

        private void cmdFind_Click(object sender, RoutedEventArgs e)
        {
            findNotes();
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkAll.IsChecked == null) return;
                foreach (var t in _Tags)
                {
                    t.Selected = chkAll.IsChecked.Value;
                }
                enableFind();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            enableFind();
        }

        private void grdTagsResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = grdTagsResults.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdTagsResults)) as FoundNote;
                if (item == null) return;
                var note = PNStatic.Notes.Note(item.Id);
                if (note != null)
                {
                    PNNotesOperations.ShowHideSpecificNote(note, true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadTags()
        {
            try
            {
                foreach (var t in PNStatic.Tags)
                {
                    _Tags.Add(new AvTag(t));
                }
                grdAvailableTags.ItemsSource = _Tags;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void enableFind()
        {
            try
            {
                cmdFind.IsEnabled = _Tags.Any(t => t.Selected);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void findNotes()
        {
            try
            {
                _Notes.Clear();
                var tags = _Tags.Where(t => t.Selected).Select(t => t.Tag);
                var notes = PNStatic.Notes.Where(n => n.GroupID != (int)SpecialGroups.RecycleBin);
                foreach (var note in notes)
                {
                    if (!note.Tags.Any(nt => tags.Any(t => t == nt))) continue;
                    var key = PNNotesOperations.GetNoteImage(note);
                    _Notes.Add(new FoundNote(key, note.Name, note.Tags.ToCommaSeparatedString(), note.ID));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;
            e.Handled = true;
            Close();
        }

        void FormMain_NoteScheduleChanged(object sender, EventArgs e)
        {
            try
            {
                var note = sender as PNote;
                if (note == null || _Notes.Count == 0) return;
                var item = _Notes.FirstOrDefault(n => n.Id == note.ID);
                if (item == null) return;
                item.IconSource = PNNotesOperations.GetNoteImage(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_NoteBooleanChanged(object sender, NoteBooleanChangedEventArgs e)
        {
            try
            {
                var note = sender as PNote;
                if (note == null || _Notes.Count == 0) return;
                var item = _Notes.FirstOrDefault(n => n.Id == note.ID);
                if (item == null) return;
                item.IconSource = PNNotesOperations.GetNoteImage(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_NoteNameChanged(object sender, NoteNameChangedEventArgs e)
        {
            try
            {
                var note = sender as PNote;
                if (note == null || _Notes.Count == 0) return;
                var item = _Notes.FirstOrDefault(n => n.Id == note.ID);
                if (item == null) return;
                item.Name = note.Name;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_NoteTagsChanged(object sender, EventArgs e)
        {
            try
            {
                var note = sender as PNote;
                if (note == null || _Notes.Count == 0) return;
                var item = _Notes.FirstOrDefault(n => n.Id == note.ID);
                var tags = _Tags.Where(t => t.Selected).Select(t => t.Tag);
                if (item == null)   //note was not in list
                {
                    if (!note.Tags.Any(nt => tags.Any(t => t == nt))) return;
                    var key = PNNotesOperations.GetNoteImage(note);
                    _Notes.Add(new FoundNote(key, note.Name, note.Tags.ToCommaSeparatedString(), note.ID));
                }
                else   //note was in list
                {
                    if (!note.Tags.Any(nt => tags.Any(t => t == nt)))
                    {
                        _Notes.Remove(item);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_LanguageChanged(object sender, EventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                Title = PNLang.Instance.GetCaptionText("serach_tags", "Search by tags");
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
