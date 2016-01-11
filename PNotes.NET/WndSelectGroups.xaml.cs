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
    /// Interaction logic for WndSelectGroups.xaml
    /// </summary>
    public partial class WndSelectGroups
    {
        internal event EventHandler<ContactsSelectedEventArgs> ContactsSelected;

        private class _Group
        {
            public string Name;
            public int ID;
            public override string ToString()
            {
                return Name;
            }
        }

        public WndSelectGroups()
        {
            InitializeComponent();
        }

        private void DlgSelectGroups_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Title = PNLang.Instance.GetControlText(Name, "Contacts groups");
                PNLang.Instance.ApplyControlLanguage(this);
                if (PNStatic.Contacts.Any(c => c.GroupID == -1))
                {
                    var gr = new _Group { Name = "(None)", ID = -1 };
                    var item = new PNListBoxItem(null, gr.Name, gr, gr.Name, false);
                    item.ListBoxItemCheckChanged += item_ListBoxItemCheckChanged;
                    lstGroups.Items.Add(item);
                }
                foreach (var cg in PNStatic.ContactGroups.Where(cg => PNStatic.Contacts.Any(c => c.GroupID == cg.ID)))
                {
                    var gr = new _Group { Name = cg.Name, ID = cg.ID };
                    var item = new PNListBoxItem(null, gr.Name, gr, gr.Name, false);
                    item.ListBoxItemCheckChanged += item_ListBoxItemCheckChanged;
                    lstGroups.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        void item_ListBoxItemCheckChanged(object sender, ListBoxItemCheckChangedEventArgs e)
        {
            try
            {
                cmdOK.IsEnabled =
                    lstGroups.Items.OfType<PNListBoxItem>().Any(p => p.IsChecked.HasValue && p.IsChecked.Value);
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
                foreach (var pg in lstGroups.Items.OfType<PNListBoxItem>().Where(p => p.IsChecked.HasValue && p.IsChecked.Value))
                {
                    var g = pg.Tag as _Group;
                    var contacts = PNStatic.Contacts.Where(c => g != null && c.GroupID == g.ID);
                    foreach (var c in contacts)
                    {
                        cse.Contacts.Add(c);
                    }
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
