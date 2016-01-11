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

using Domino;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PNotes.NET
{
    internal class PNLotus
    {
        private static List<Tuple<string, string>> _contacts;

        internal static bool IsLotusInstalled()
        {
            const string regKey = @"Software\Lotus\Notes";
            using (var key = Registry.LocalMachine)
            {
                using (var lk = key.OpenSubKey(regKey))
                {
                    if (lk != null)
                        return true;
                }
            }
            return false;
        }

        internal static List<Tuple<string, string>> GetLotusContacts(Window owner)
        {
            try
            {
                var dlgLotus = new WndLotusCredentials { Owner = owner };
                dlgLotus.LotusCredentialSet += dlgLotus_LotusCredentialSet;
                var showDialog = dlgLotus.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgLotus.LotusCredentialSet -= dlgLotus_LotusCredentialSet;
                }
                return _contacts;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        static void dlgLotus_LotusCredentialSet(object sender, LotusCredentialSetEventArgs e)
        {
            try
            {
                var dlgLotus = sender as WndLotusCredentials;
                if(dlgLotus!=null)
                    dlgLotus.LotusCredentialSet -= dlgLotus_LotusCredentialSet;
                _contacts = new List<Tuple<string, string>>();
                loadContacts(e.ContactsView);
                loadContacts(e.PeopleView);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static void loadContacts(IView view)
        {
            if (view == null)
                return;
            var notesViewCollection = view.AllEntries;
            for (var i = 1; i <= notesViewCollection.Count; i++)
            {
                //Get the nth entry of the selected view according to the iteration.
                var viewEntry = notesViewCollection.GetNthEntry(i);
                //Get the first document of particular entry.
                var document = viewEntry.Document;

                var documentItems = document.Items;
                var itemArray = (Array)documentItems;
                if (
                    itemArray.Cast<object>()
                        .Select((t, k) => (NotesItem)itemArray.GetValue(k))
                        .All(notesItem => string.IsNullOrEmpty(notesItem.Text))) continue;
                var name = "";
                var address = "";
                for (var n = 0; n < itemArray.Length; n++)
                {
                    var searchedNotesItem = (NotesItem)itemArray.GetValue(n);
                    switch (searchedNotesItem.Name)
                    {
                        case "FullName":
                            name = searchedNotesItem.Text;
                            break;
                        case "MailAddress":
                            address = searchedNotesItem.Text;
                            break;
                    }
                }
                _contacts.Add(Tuple.Create(name, address));
            }
        }
    }
}
