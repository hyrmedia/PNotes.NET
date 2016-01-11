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
using System.Linq;
using System.Windows;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSelectContacts.xaml
    /// </summary>
    public partial class WndSelectContacts
    {
        internal event EventHandler<ContactsSelectedEventArgs> ContactsSelected;

        public WndSelectContacts()
        {
            InitializeComponent();
        }

        private void DlgSelectContacts_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Title = PNLang.Instance.GetControlText("lblContacts", "Contacts");
                PNLang.Instance.ApplyControlLanguage(this);
                foreach (var c in PNStatic.Contacts)
                {
                    var pti = new PNListBoxItem(null, c.Name, c, c.Name, false);
                    pti.ListBoxItemCheckChanged += pti_ListBoxItemCheckChanged;
                    lstContacts.Items.Add(pti);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        void pti_ListBoxItemCheckChanged(object sender, ListBoxItemCheckChangedEventArgs e)
        {
            try
            {
                cmdOK.IsEnabled =
                    lstContacts.Items.OfType<PNListBoxItem>().Any(p => p.IsChecked.HasValue && p.IsChecked.Value);
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
                var cse = new ContactsSelectedEventArgs();
                foreach (
                    var pti in
                        lstContacts.Items.OfType<PNListBoxItem>().Where(p => p.IsChecked.HasValue && p.IsChecked.Value))
                {
                    cse.Contacts.Add(pti.Tag as PNContact);
                }
                if (ContactsSelected != null)
                {
                    ContactsSelected(this, cse);
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
