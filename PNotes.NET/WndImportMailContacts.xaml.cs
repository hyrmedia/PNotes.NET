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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndImportMailContacts.xaml
    /// </summary>
    public partial class WndImportMailContacts
    {
        internal event EventHandler<ContactsImportedEventArgs> ContactsImported;

        public WndImportMailContacts()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }
        
        internal WndImportMailContacts(ImportContacts target, IEnumerable<PNMailContact> contacts, string targetName)
            : this()
        {
            _Target = target;
            _TargetName = targetName;
            _Contacts = contacts;
        }

        private sealed class MailC : INotifyPropertyChanged
        {
            private bool _Selected;
            public string DispName { get; private set; }
            public string Address { get; private set; }
            public bool Selected {
                get { return _Selected; }
                set
                {
                    if (_Selected == value) return;
                    _Selected = value;
                    OnPropertyChanged("Selected");
                }
            }

            public MailC(string dispName, string address)
            {
                DispName = dispName;
                Address = address;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

            }
        }

        private readonly ImportContacts _Target;
        private readonly string _TargetName;
        private readonly IEnumerable<PNMailContact> _Contacts;
        private readonly ObservableCollection<MailC> _MailContacts = new ObservableCollection<MailC>();
 
        private void DlgImportMailContacts_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                pnsImportContacts.Text += @" " + _TargetName;
                grdMailContacts.ItemsSource = _MailContacts;
                stkImport.Width = stkImport.ActualWidth;
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
                if (ContactsImported != null)
                {
                    var selected = _MailContacts.Where(c => c.Selected);
                    var contacts = selected.Select(r => Tuple.Create(Convert.ToString(r.DispName), Convert.ToString(r.Address)));
                    ContactsImported(this, new ContactsImportedEventArgs(contacts));
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdLoadImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;
                List<Tuple<string, string>> contacts = null;
                chkAll.IsChecked = false;
                _MailContacts.Clear();

                switch (_Target)
                {
                    case ImportContacts.Outlook:
                        contacts = PNOffice.GetOutlookContacts();
                        break;
                    case ImportContacts.Gmail:
                        contacts = PNGoogle.GetGoogleContacts();
                        break;
                    case ImportContacts.Lotus:
                        contacts = PNLotus.GetLotusContacts(this);
                        break;
                }
                if (contacts == null) return;
                var rg = new Regex(PNStrings.MAIL_PATTERN, RegexOptions.IgnoreCase);
                foreach (var tc in contacts.Where(t => !string.IsNullOrWhiteSpace(t.Item2))
                    .Where(
                        tc =>
                            chkNoDuplicates.IsChecked != null && (!chkNoDuplicates.IsChecked.Value ||
                                                                  !_Contacts.Any(c => c.DisplayName == tc.Item1 && c.Address == tc.Item2)))
                    .Where(tc => rg.IsMatch(tc.Item2)))
                {
                    _MailContacts.Add(new MailC(tc.Item1, tc.Item2));
                }
                chkAll.IsEnabled = _MailContacts.Count > 0;
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

        private void CheckBoxChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                var checkBox = sender as CheckBox;
                if (checkBox == null) return;
                var item = grdMailContacts.GetObjectAtPoint<ListViewItem>(Mouse.GetPosition(grdMailContacts)) as MailC;
                if (item == null) return;
                grdMailContacts.SelectedItem = item;
                cmdOK.IsEnabled = _MailContacts.Any(c => c.Selected);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void HeaderChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkAll.IsChecked == null) return;
                foreach (var c in _MailContacts)
                {
                    c.Selected = chkAll.IsChecked.Value;
                }
                cmdOK.IsEnabled = _MailContacts.Any(c => c.Selected);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
