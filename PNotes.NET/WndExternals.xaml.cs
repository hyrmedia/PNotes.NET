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

using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndExternals.xaml
    /// </summary>
    public partial class WndExternals
    {
        public WndExternals()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndExternals(WndSettings prefs)
            : this()
        {
            m_Prefs = prefs;
            m_Mode = AddEditMode.Add;
        }

        internal WndExternals(WndSettings prefs, PNExternal ext)
            : this()
        {
            m_Prefs = prefs;
            m_Mode = AddEditMode.Edit;
            m_Ext = ext;
        }

        private readonly WndSettings m_Prefs;
        private readonly AddEditMode m_Mode;
        private PNExternal m_Ext;

        private void enableOk()
        {
            cmdOK.IsEnabled = txtExtName.Text.Trim().Length > 0 && txtExtProg.Text.Trim().Length > 0;
        }

        private void DlgExternals_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                if (m_Mode == AddEditMode.Add)
                {
                    Title = PNLang.Instance.GetCaptionText("ext_new", "New external program");
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("ext_edit", "Edit external program");
                    txtExtName.IsReadOnly = true;
                    txtExtName.Text = m_Ext.Name;
                    txtExtProg.Text = m_Ext.Program;
                    txtCommandLine.Text = m_Ext.CommandLine;
                }
                txtExtName.Focus();
                txtExtName.SelectAll();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtExtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableOk();
        }

        private void txtExtProg_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableOk();
        }

        private void cmdProg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    Filter = @"Programs|*.exe",
                    Title = PNLang.Instance.GetCaptionText("choose_new_ext", "Choose external program")
                };
                if (ofd.ShowDialog(this).Value)
                {
                    txtExtProg.Text = ofd.FileName;
                }
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
                var name = txtExtName.Text.Trim();
                if (m_Mode == AddEditMode.Add && m_Prefs.ExternalExists(name))
                {
                    var message = PNLang.Instance.GetMessageText("ext_exists", "External program with this name already exists");
                    PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    if (m_Mode == AddEditMode.Add)
                    {
                        m_Ext = new PNExternal { Name = name, Program = txtExtProg.Text.Trim(), CommandLine = txtCommandLine.Text.Trim() };
                        m_Prefs.ExternalAdd(m_Ext);
                    }
                    else
                    {
                        m_Ext.Program = txtExtProg.Text.Trim();
                        m_Ext.CommandLine = txtCommandLine.Text.Trim();
                        m_Prefs.ExternalReplace(m_Ext);
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
