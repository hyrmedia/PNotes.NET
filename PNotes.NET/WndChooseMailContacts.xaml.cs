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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndChooseMailContacts.xaml
    /// </summary>
    public partial class WndChooseMailContacts
    {
        internal event EventHandler<MailRecipientsChosenEventArgs> MailRecipientsChosen;

        private sealed class MailC : INotifyPropertyChanged
        {
            private bool _Selected;
            public string DispName { get; private set; }
            public string Address { get; private set; }
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

        public WndChooseMailContacts()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private readonly ObservableCollection<MailC> _MailContacts = new ObservableCollection<MailC>();

        private void DlgChooseMailContacts_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                grdMailContacts.ItemsSource = _MailContacts;
                foreach (var c in PNStatic.MailContacts)
                    _MailContacts.Add(new MailC(c.DisplayName, c.Address));
                chkAll.IsEnabled = _MailContacts.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
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

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MailRecipientsChosen != null)
                {
                    var selected = _MailContacts.Where(mc => mc.Selected);
                    var list = selected.Select(s => new PNMailContact
                    {
                        DisplayName = s.DispName,
                        Address = s.Address
                    });
                    MailRecipientsChosen(this, new MailRecipientsChosenEventArgs(list));
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
