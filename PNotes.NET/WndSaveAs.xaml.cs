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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSaveAs.xaml
    /// </summary>
    public partial class WndSaveAs
    {
        public WndSaveAs()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        public WndSaveAs(string noteName, int noteGroup)
            : this()
        {
            m_NoteName = noteName;
            m_GroupID = noteGroup;
        }

        internal event EventHandler<SaveAsNoteNameSetEventArgs> SaveAsNoteNameSet;

        private readonly string m_NoteName;
        private readonly int m_GroupID = -1;
        private readonly List<PNTreeItem> _Items = new List<PNTreeItem>();

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            var item = tvwGroups.SelectedItem as PNTreeItem;
            if (item == null) return;
            var gr = item.Tag as PNGroup;
            if (gr == null) return;
            if (SaveAsNoteNameSet != null)
            {
                SaveAsNoteNameSet(this, new SaveAsNoteNameSetEventArgs(txtName.Text.Trim(), gr.ID));
                DialogResult = true;
            }
        }

        private void tvwGroups_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            enableOk();
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableOk();
        }

        private void enableOk()
        {
            try
            {
                cmdOK.IsEnabled = txtName.Text.Trim().Length > 0 && tvwGroups.SelectedItem != null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgSaveAs_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                Title = PNLang.Instance.GetCaptionText("save_as", "Save note as...");
                txtName.Text = m_NoteName;
                foreach (PNGroup g in PNStatic.Groups[0].Subgroups)
                {
                    loadGroup(g, null);
                }
                tvwGroups.ItemsSource = _Items;
                txtName.SelectAll();
                txtName.Focus();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadGroup(PNGroup pgroup, PNTreeItem item)
        {
            try
            {
                var ti = new PNTreeItem(pgroup.Image, pgroup.Name, pgroup) { IsExpanded = true };
                foreach (var sg in pgroup.Subgroups)
                {
                    loadGroup(sg, ti);
                }

                if (pgroup.ID == m_GroupID)
                {
                    ti.IsSelected = true;
                }
                else if (pgroup.ID == 0 && m_GroupID == -1)
                {
                    ti.IsSelected = true;
                }

                if (item == null)
                    _Items.Add(ti);
                else
                    item.Items.Add(ti);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwGroups_PNTreeViewLeftMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = tvwGroups.GetHierarchyObjectAtPoint<PNTreeItem>(e.GetPosition(tvwGroups)) as PNTreeItem;
            if (item != null)
            {
                cmdOK.PerformClick();
            }
        }
    }
}
