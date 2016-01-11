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
using System.Windows;
using System.Windows.Controls;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSP.xaml
    /// </summary>
    public partial class WndSP
    {
        public WndSP()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndSP(WndSettings prefs)
            : this()
        {
            _Prefs = prefs;
            _Mode = AddEditMode.Add;
        }

        internal WndSP(WndSettings prefs, PNSearchProvider sp)
            : this()
        {
            _Prefs = prefs;
            _Mode = AddEditMode.Edit;
            _SearchProviders = sp;
        }

        readonly WndSettings _Prefs;
        readonly AddEditMode _Mode;
        PNSearchProvider _SearchProviders;

        private void enableOk()
        {
            cmdOK.IsEnabled = txtSPName.Text.Trim().Length > 0 && txtSPQuery.Text.Trim().Length > 0;
        }

        private void DlgSP_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                if (_Mode == AddEditMode.Add)
                {
                    Title = PNLang.Instance.GetCaptionText("sp_new", "New search provider");
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("sp_edit", "Edit search provider");
                    txtSPName.Text = _SearchProviders.Name;
                    txtSPQuery.Text = _SearchProviders.QueryString;
                    txtSPName.IsReadOnly = true;
                }
                txtSPName.Focus();
                txtSPName.SelectAll();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtSPName_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableOk();
        }

        private void txtSPQuery_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableOk();
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var name = txtSPName.Text.Trim();
                if (_Mode == AddEditMode.Add && _Prefs.SearchProviderExists(name))
                {
                    var message = PNLang.Instance.GetMessageText("sp_exists", "Search provider with this name already exists");
                    PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    if (_Mode == AddEditMode.Add)
                    {
                        _SearchProviders = new PNSearchProvider { Name = name, QueryString = txtSPQuery.Text.Trim() };
                        _Prefs.SearchProviderAdd(_SearchProviders);
                    }
                    else
                    {
                        _SearchProviders.QueryString = txtSPQuery.Text.Trim();
                        _Prefs.SearchProviderReplace(_SearchProviders);
                    }
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
