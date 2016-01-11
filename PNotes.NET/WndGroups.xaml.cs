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
    /// Interaction logic for WndGroups.xaml
    /// </summary>
    public partial class WndGroups
    {
        internal event EventHandler<ContactGroupChangedEventArgs> ContactGroupChanged;

        public WndGroups()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndGroups(int newId):this()
        {
            m_Mode = AddEditMode.Add;
            m_NewId = newId;
        }

        internal WndGroups(PNContactGroup cg):this()
        {
            m_Mode = AddEditMode.Edit;
            m_Group = cg;
        }

        private readonly AddEditMode m_Mode;
        private readonly PNContactGroup m_Group;
        private readonly int m_NewId;

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContactGroupChangedEventArgs ce;
                if (m_Mode == AddEditMode.Add)
                {
                    var cg = new PNContactGroup { Name = txtGroupName.Text.Trim(), ID = m_NewId };
                    ce = new ContactGroupChangedEventArgs(cg, m_Mode);
                }
                else
                {
                    m_Group.Name = txtGroupName.Text.Trim();
                    ce = new ContactGroupChangedEventArgs(m_Group, m_Mode);
                }
                if (ContactGroupChanged != null)
                {
                    ContactGroupChanged(this, ce);
                }
                if (!ce.Accepted)
                {
                    txtGroupName.SelectAll();
                    txtGroupName.Focus();
                    return;
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgGroups_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                if (m_Mode == AddEditMode.Add)
                {
                    Title = PNLang.Instance.GetCaptionText("group_new", "New group of contacts");
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("group_edit", "Edit group of contacts");
                    txtGroupName.Text = m_Group.Name;
                }
                txtGroupName.SelectAll();
                txtGroupName.Focus();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtGroupName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                cmdOK.IsEnabled = txtGroupName.Text.Trim().Length > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
