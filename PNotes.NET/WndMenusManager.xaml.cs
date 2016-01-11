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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndMenusManager.xaml
    /// </summary>
    public partial class WndMenusManager
    {
        public WndMenusManager()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private enum UpDown
        {
            Up,
            Down
        }

        private readonly List<string> _UncheckedNodes = new List<string>();
        private readonly List<string> _OrderSql = new List<string>();
        private readonly MenusOrderChangedEventArgs _MenusOrderChanged = new MenusOrderChangedEventArgs();

        private readonly Dictionary<MenuType, Tuple<List<string>, List<string>>> _OrderLists =
            new Dictionary<MenuType, Tuple<List<string>, List<string>>>
            {
                {
                    MenuType.Main,
                    Tuple.Create(new List<string>(), new List<string>())
                },
                {
                    MenuType.Note,
                    Tuple.Create(new List<string>(), new List<string>())
                },
                {
                    MenuType.Edit,
                    Tuple.Create(new List<string>(), new List<string>())
                },
                {
                    MenuType.ControlPanel,
                    Tuple.Create(new List<string>(), new List<string>())
                }
            };

        private readonly Dictionary<MenuType, Tuple<List<string>, List<string>>> _HiddenLists =
            new Dictionary<MenuType, Tuple<List<string>, List<string>>>
            {
                {
                    MenuType.Main, 
                    Tuple.Create(new List<string>(), new List<string>())},
                {
                    MenuType.Note,
                    Tuple.Create(new List<string>(), new List<string>())
                },
                {
                    MenuType.Edit,
                    Tuple.Create(new List<string>(), new List<string>())
                },
                {
                    MenuType.ControlPanel,
                    Tuple.Create(new List<string>(), new List<string>())
                }
            };

        private readonly List<PNHiddenMenu> _TempHiddenMenus = new List<PNHiddenMenu>();

        private readonly List<PNTreeItem> _ItemsMain = new List<PNTreeItem>();
        private readonly List<PNTreeItem> _ItemsNote = new List<PNTreeItem>();
        private readonly List<PNTreeItem> _ItemsEdit = new List<PNTreeItem>();
        private readonly List<PNTreeItem> _ItemsCP = new List<PNTreeItem>();

        private void DlgMenusManager_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNStatic.FormMenus = this;
                PNLang.Instance.ApplyControlLanguage(this);
                //load main menu
                foreach (var ti in PNMenus.CurrentMainMenus)
                {
                    loadMenus(ti, null, MenuType.Main);
                }
                foreach (var pti in _ItemsMain)
                    tvwMain.Items.Add(pti);
                ((TreeViewItem)tvwMain.Items[0]).IsSelected = true;
                foreach (var pti in tvwMain.Items.OfType<PNTreeItem>())
                {
                    fillNodesList(pti, _OrderLists[MenuType.Main].Item1, _HiddenLists[MenuType.Main].Item1);
                }
                //load note menu
                foreach (var ti in PNMenus.CurrentNoteMenus)
                {
                    loadMenus(ti, null, MenuType.Note);
                }
                foreach (var pti in _ItemsNote)
                    tvwNote.Items.Add(pti);
                ((TreeViewItem)tvwNote.Items[0]).IsSelected = true;
                foreach (var pti in tvwNote.Items.OfType<PNTreeItem>())
                {
                    fillNodesList(pti, _OrderLists[MenuType.Note].Item1, _HiddenLists[MenuType.Note].Item1);
                }
                //load edit area menu
                foreach (var ti in PNMenus.CurrentEditMenus)
                {
                    loadMenus(ti, null, MenuType.Edit);
                }
                foreach (var pti in _ItemsEdit)
                    tvwEdit.Items.Add(pti);
                ((TreeViewItem)tvwEdit.Items[0]).IsSelected = true;
                foreach (var pti in tvwEdit.Items.OfType<PNTreeItem>())
                {
                    fillNodesList(pti, _OrderLists[MenuType.Edit].Item1, _HiddenLists[MenuType.Edit].Item1);
                }
                //load control panel menu
                foreach (var ti in PNMenus.CurrentCPMenus)
                {
                    loadMenus(ti, null, MenuType.ControlPanel);
                }
                foreach (var pti in _ItemsCP)
                    tvwCP.Items.Add(pti);
                if (tvwCP.Items.Count > 0) ((TreeViewItem)tvwCP.Items[0]).IsSelected = true;
                foreach (var pti in tvwCP.Items.OfType<PNTreeItem>())
                {
                    fillNodesList(pti, _OrderLists[MenuType.ControlPanel].Item1, _HiddenLists[MenuType.ControlPanel].Item1);
                }

            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tabMenus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TreeView tvw = getActiveTreeView();
                if (tvw == null) return;
                cmdResetCurrent.IsEnabled = tvw.SelectedItem != null;
                enableUpDown(tvw.SelectedItem as PNTreeItem);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                moveUpDown(UpDown.Up);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                moveUpDown(UpDown.Down);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdRestoreOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (tabMenus.SelectedIndex)
                {
                    case 0:
                        beforeSaveUncheckedNodes(tvwMain);
                        tvwMain.Items.Clear();
                        foreach (var pti in _ItemsMain)
                            tvwMain.Items.Add(pti);
                        foreach (var pti in tvwMain.Items.OfType<PNTreeItem>())
                            restoreUncheckedNodes(pti);
                        break;
                    case 1:
                        beforeSaveUncheckedNodes(tvwNote);
                        tvwNote.Items.Clear();
                        foreach (var pti in _ItemsNote)
                            tvwNote.Items.Add(pti);
                        foreach (var pti in tvwNote.Items.OfType<PNTreeItem>())
                            restoreUncheckedNodes(pti);
                        break;
                    case 2:
                        beforeSaveUncheckedNodes(tvwEdit);
                        tvwEdit.Items.Clear();
                        foreach (var pti in _ItemsEdit)
                            tvwEdit.Items.Add(pti);
                        foreach (var pti in tvwEdit.Items.OfType<PNTreeItem>())
                            restoreUncheckedNodes(pti);
                        break;
                    case 3:
                        beforeSaveUncheckedNodes(tvwCP);
                        tvwCP.Items.Clear();
                        foreach (var pti in _ItemsCP)
                            tvwCP.Items.Add(pti);
                        foreach (var pti in tvwCP.Items.OfType<PNTreeItem>())
                            restoreUncheckedNodes(pti);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdResetAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var pti in tvwMain.Items.OfType<PNTreeItem>())
                    resetChecks(pti);
                foreach (var pti in tvwNote.Items.OfType<PNTreeItem>())
                    resetChecks(pti);
                foreach (var pti in tvwEdit.Items.OfType<PNTreeItem>())
                    resetChecks(pti);
                foreach (var pti in tvwCP.Items.OfType<PNTreeItem>())
                    resetChecks(pti);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdResetCurrent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (tabMenus.SelectedIndex)
                {
                    case 0:
                        foreach (var pti in tvwMain.Items.OfType<PNTreeItem>())
                            resetChecks(pti);
                        break;
                    case 1:
                        foreach (var pti in tvwNote.Items.OfType<PNTreeItem>())
                            resetChecks(pti);
                        break;
                    case 2:
                        foreach (var pti in tvwEdit.Items.OfType<PNTreeItem>())
                            resetChecks(pti);
                        break;
                    case 3:
                        foreach (var pti in tvwCP.Items.OfType<PNTreeItem>())
                            resetChecks(pti);
                        break;
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
                if (saveMenus())
                    DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var tvw = sender as TreeView;
                if (tvw == null) return;
                cmdResetCurrent.IsEnabled = tvw.SelectedItem != null;
                enableUpDown(tvw.SelectedItem as PNTreeItem);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void moveUpDown(UpDown direction)
        {
            try
            {
                var tvw = getActiveTreeView();
                if (tvw == null) return;
                var item = tvw.SelectedItem as PNTreeItem;
                if (item == null) return;
                var parent = item.Parent;
                var index = item.Index;
                var parentItem = parent as PNTreeItem;
                var items = parentItem != null ? parentItem.Items : tvw.Items;
                var duplicate = parentItem != null;

                items.RemoveAt(index);
                if (direction == UpDown.Up)
                {
                    if (duplicate)
                    {
                        item = item.Clone();
                    }
                    items.Insert(index - 1, item);
                }
                else
                {
                    if (duplicate)
                    {
                        item = item.Clone();
                    }
                    items.Insert(index + 1, item);
                }
                item.IsSelected = true;
                item.BringIntoView();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private TreeView getActiveTreeView()
        {
            try
            {
                switch (tabMenus.SelectedIndex)
                {
                    case 0:
                        return tvwMain;
                    case 1:
                        return tvwNote;
                    case 2:
                        return tvwEdit;
                    case 3:
                        return tvwCP;
                }
                return null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void enableUpDown(PNTreeItem item)
        {
            try
            {
                if (item == null)
                {
                    cmdUp.IsEnabled = cmdDown.IsEnabled = false;
                }
                else
                {
                    var index = item.Index;
                    cmdUp.IsEnabled = index > 0;
                    var parent = ItemsControl.ItemsControlFromItemContainer(item);
                    cmdDown.IsEnabled = index < parent.Items.Count - 1;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadMenus(PNMenu mnu, PNTreeItem item, MenuType type)
        {
            try
            {
                //PNStrings.MENU_SEPARATOR_STRING
                var treeItem = mnu.Text != PNStrings.MENU_SEPARATOR_STRING
                    ? new PNTreeItem("mnu", mnu.Text, mnu, mnu.Name,
                        PNStatic.HiddenMenus.Where(m => m.Type == type).All(m => m.Name != mnu.Name))
                    {
                        IsExpanded = true
                    }
                    : new PNTreeItem("mnu", mnu.Text, mnu, mnu.Name);
                treeItem.TreeViewItemCheckChanged += treeItem_TreeViewItemCheckChanged;
                foreach (var sg in mnu.Items)
                {
                    loadMenus(sg, treeItem, type);
                }
                if (item == null)
                {
                    switch (type)
                    {
                        case MenuType.Main:
                            _ItemsMain.Add(treeItem);
                            break;
                        case MenuType.Note:
                            _ItemsNote.Add(treeItem);
                            break;
                        case MenuType.Edit:
                            _ItemsEdit.Add(treeItem);
                            break;
                        case MenuType.ControlPanel:
                            _ItemsCP.Add(treeItem);
                            break;
                    }
                }
                else
                    item.Items.Add(treeItem);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillNodesList(PNTreeItem item, List<string> listNames, List<string> listHidden)
        {
            try
            {
                foreach (var pti in item.Items.OfType<PNTreeItem>())
                {
                    fillNodesList(pti, listNames, listHidden);
                }
                listNames.Add(item.Key);
                if (listHidden == null) return;
                if (!item.IsChecked.HasValue || !item.IsChecked.Value)
                    listHidden.Add(item.Key);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void resetChecks(PNTreeItem item)
        {
            try
            {
                foreach (PNTreeItem n in item.Items)
                {
                    resetChecks(n);
                }
                item.IsChecked = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void beforeSaveUncheckedNodes(TreeView tvw)
        {
            try
            {
                _UncheckedNodes.Clear();
                foreach (var n in tvw.Items.OfType<PNTreeItem>())
                    saveUncheckedNodes(n);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void saveUncheckedNodes(PNTreeItem item)
        {
            try
            {
                foreach (var n in item.Items.OfType<PNTreeItem>())
                {
                    saveUncheckedNodes(n);
                }
                if (item.IsChecked != null && !item.IsChecked.Value)
                    _UncheckedNodes.Add(item.Key);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void restoreUncheckedNodes(PNTreeItem item)
        {
            try
            {
                foreach (var pti in item.Items.OfType<PNTreeItem>())
                {
                    restoreUncheckedNodes(pti);
                }
                if (_UncheckedNodes.Any(s => s == item.Key))
                {
                    item.IsChecked = false;
                    return;
                }
                item.IsChecked = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool preSaveMenus(MenuType type, TreeView tvw)
        {
            try
            {
                foreach(var pti in tvw.Items.OfType<PNTreeItem>())
                {
                    fillNodesList(pti, _OrderLists[type].Item2, _HiddenLists[type].Item2);
                }
                if (_OrderLists[type].Item1.Where((t, i) => t != _OrderLists[type].Item2[i]).Any())
                {
                    foreach (var pti in tvw.Items.OfType<PNTreeItem>())
                    {
                        createOrderSql(pti);
                    }
                    switch (type)
                    {
                        case MenuType.Main:
                            _MenusOrderChanged.Main = true;
                            break;
                        case MenuType.Note:
                            {
                                var d = new WndNote();
                                try
                                {
                                    _MenusOrderChanged.Note = true;
                                    PNMenus.PrepareDefaultMenuStrip(d.NoteMenu, MenuType.Note, true);
                                }
                                finally
                                {
                                    d.Close();
                                }
                            }
                            break;
                        case MenuType.Edit:
                            {
                                var d = new WndNote();
                                try
                                {
                                    _MenusOrderChanged.Edit = true;
                                    PNMenus.PrepareDefaultMenuStrip(d.EditMenu, MenuType.Edit, true);
                                }
                                finally
                                {
                                    d.Close();
                                }
                            }
                            break;
                        case MenuType.ControlPanel:
                            {
                                var d = new WndCP();
                                try
                                {
                                    _MenusOrderChanged.ControlPanel = true;
                                    PNMenus.PrepareDefaultMenuStrip(d.CPMenu, MenuType.ControlPanel, true);
                                }
                                finally
                                {
                                    d.Close();
                                }
                            }
                            break;
                    }
                }

                _TempHiddenMenus.AddRange(
                    _HiddenLists[type].Item2.Select(s => new PNHiddenMenu(s, type)));

                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void createOrderSql(PNTreeItem node)
        {
            try
            {
                foreach (var pti in node.Items.OfType<PNTreeItem>())
                {
                    createOrderSql(pti);
                }
                var pnm = node.Tag as PNMenu;
                if (pnm == null) return;
                var sb = new StringBuilder("UPDATE MENUS_ORDER SET ORDER_NEW = ");
                sb.Append(node.Index);
                sb.Append(" WHERE CONTEXT_NAME = '");
                sb.Append(pnm.ContextName);
                sb.Append("' AND MENU_NAME = '");
                sb.Append(pnm.Name);
                sb.Append("'");
                _OrderSql.Add(sb.ToString());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool saveMenus()
        {
            try
            {
                _OrderSql.Clear();

                _TempHiddenMenus.Clear();

                _MenusOrderChanged.Main = _MenusOrderChanged.Note = _MenusOrderChanged.Edit = _MenusOrderChanged.ControlPanel = false;

                if (!preSaveMenus(MenuType.Main, tvwMain)) return false;

                if (!preSaveMenus(MenuType.Note, tvwNote)) return false;

                if (!preSaveMenus(MenuType.Edit, tvwEdit)) return false;

                if (!preSaveMenus(MenuType.ControlPanel, tvwCP)) return false;

                PNStatic.HiddenMenus.Clear();

                PNStatic.HiddenMenus.AddRange(_TempHiddenMenus);

                PNData.SaveHiddenMenus();
                PNStatic.FormMain.RefreshHiddenMenus();
                if (_OrderSql.Count > 0)
                {
                    if (PNData.ExecuteTransactionForList(_OrderSql, PNData.ConnectionString))
                    {
                        PNStatic.FormMain.ApplyNewMenusOrder(_MenusOrderChanged);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void treeItem_TreeViewItemCheckChanged(object sender, TreeViewItemCheckChangedEventArgs e)
        {
            try
            {
                var item = sender as PNTreeItem;
                if (item == null || e.ParentTreeView == null) return;
                switch (e.ParentTreeView.Name)
                {
                    case "tvwMain":
                        if (e.State)
                            _HiddenLists[MenuType.Main].Item1.Remove(item.Key);
                        else
                            _HiddenLists[MenuType.Main].Item1.Add(item.Key);
                        break;
                    case "tvwNote":
                        if (e.State)
                            _HiddenLists[MenuType.Note].Item1.Remove(item.Key);
                        else
                            _HiddenLists[MenuType.Note].Item1.Add(item.Key);
                        break;
                    case "tvwEdit":
                        if (e.State)
                            _HiddenLists[MenuType.Edit].Item1.Remove(item.Key);
                        else
                            _HiddenLists[MenuType.Edit].Item1.Add(item.Key);
                        break;
                    case "tvwCP":
                        if (e.State)
                            _HiddenLists[MenuType.ControlPanel].Item1.Remove(item.Key);
                        else
                            _HiddenLists[MenuType.ControlPanel].Item1.Add(item.Key);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgMenusManager_Closed(object sender, EventArgs e)
        {
            PNStatic.FormMenus = null;
        }
    }
}
