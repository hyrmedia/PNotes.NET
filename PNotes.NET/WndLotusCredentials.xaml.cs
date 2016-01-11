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
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndLotusCredentials.xaml
    /// </summary>
    public partial class WndLotusCredentials : Window
    {
        internal event EventHandler<LotusCredentialSetEventArgs> LotusCredentialSet;

        public WndLotusCredentials()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private NotesSession _LocalNotesSession;
        private NotesSession _ServerNotesSession;

        private void DlgLotusCred_Loaded(object sender, RoutedEventArgs e)
        {
            PNLang.Instance.ApplyControlLanguage(this);
            txtPassword.Focus();
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_LocalNotesSession == null)
                {
                    _LocalNotesSession = new NotesSessionClass();
                    //Initializing Lotus Notes Session
                    try
                    {
                        _LocalNotesSession.Initialize(txtPassword.Password);
                    }
                    catch (COMException cex)
                    {
                        if (cex.ErrorCode != -2147217504) throw;
                        PNMessageBox.Show(PNLang.Instance.GetMessageText("pwrd_not_match", "Invalid password"),
                            PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                //Creating Lotus Notes DataBase Object
                var localDatabase = _LocalNotesSession.GetDatabase("", "names.nsf", false);

                if (_ServerNotesSession == null)
                {
                    _ServerNotesSession = new NotesSessionClass();
                    //Initializing Lotus Notes Session
                    try
                    {
                        _ServerNotesSession.Initialize(txtPassword.Password);
                    }
                    catch (COMException cex)
                    {
                        if (cex.ErrorCode != -2147217504) throw;
                        PNMessageBox.Show(PNLang.Instance.GetMessageText("pwrd_not_match", "Invalid password"),
                            PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                //Creating Lotus Notes DataBase Object
                var serverDatabase = _ServerNotesSession.GetDatabase(txtServer.Text.Trim(), "names.nsf", false);

                //creating Lotus Notes Contact View
                NotesView contactsView = null, peopleView = null;
                if (localDatabase != null)
                    contactsView = localDatabase.GetView("Contacts");
                if (serverDatabase != null)
                    peopleView = serverDatabase.GetView("$People");

                if (LotusCredentialSet != null)
                {
                    LotusCredentialSet(this, new LotusCredentialSetEventArgs(contactsView, peopleView));
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            cmdOK.IsEnabled = txtPassword.Password.Trim().Length > 0;
        }
    }
}
