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

using SQLiteWrapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndHotkeys.xaml
    /// </summary>
    public partial class WndHotkeys
    {
        public class DefKey
        {
            internal DefKey(int id, HotkeyType menuType, string menuName)
            {
                Id = id;
                MenuType = menuType;
                MenuName = menuName;
            }

            internal HotkeyType MenuType { get; private set; }
            internal string MenuName { get; private set; }
            public int Id { get; private set; }
            public BitmapSource Icon { get; set; }
            public string MenuText { get; set; }
            public string Shortcut { get; set; }
            public string MenuRange { get; set; }
        }

        public WndHotkeys()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private List<PNHotKey> m_KeysMain, m_KeysNote, m_KeysEdit, m_KeysGroups;
        private readonly List<PNTreeItem> _ItemsMain = new List<PNTreeItem>();
        private readonly List<PNTreeItem> _ItemsNote = new List<PNTreeItem>();
        private readonly List<PNTreeItem> _ItemsEdit = new List<PNTreeItem>();
        private readonly List<PNTreeItem> _ItemsGroup = new List<PNTreeItem>();
        private readonly ObservableCollection<DefKey> _DefKeys = new ObservableCollection<DefKey>();
        private readonly BitmapImage _ImageMenu = new BitmapImage(new Uri("images/mnu.png", UriKind.Relative));
        private readonly BitmapImage _ImageSubmenu = new BitmapImage(new Uri("images/submnu.png", UriKind.Relative));

        private HwndSource _HwndSource;

        #region Internal procedures
        internal void SetHotkey(PNHotKey hk)
        {
            PNHotKey phk = null;
            switch (tabHK.SelectedIndex)
            {
                case 0:
                    phk = m_KeysMain.FirstOrDefault(h => h.MenuName == hk.MenuName);
                    break;
                case 1:
                    phk = m_KeysNote.FirstOrDefault(h => h.MenuName == hk.MenuName);
                    break;
                case 2:
                    phk = m_KeysEdit.FirstOrDefault(h => h.MenuName == hk.MenuName);
                    break;
                case 3:
                    phk = m_KeysGroups.FirstOrDefault(h => h.MenuName == hk.MenuName);
                    break;
            }
            if (phk != null)
            {
                phk.CopyFrom(hk);
            }
        }
        internal bool HotkeyExists(PNHotKey hk)
        {
            PNHotKey phk = null;
            switch (tabHK.SelectedIndex)
            {
                case 0:
                    phk = m_KeysMain.FirstOrDefault(h => h.Shortcut == hk.Shortcut && h.ID != hk.ID);
                    break;
                case 1:
                    phk = m_KeysNote.FirstOrDefault(h => h.Shortcut == hk.Shortcut && h.ID != hk.ID);
                    break;
                case 2:
                    phk = m_KeysEdit.FirstOrDefault(h => h.Shortcut == hk.Shortcut && h.ID != hk.ID);
                    break;
                case 3:
                    phk = m_KeysGroups.FirstOrDefault(h => h.Shortcut == hk.Shortcut && h.ID != hk.ID);
                    break;
            }
            if (phk != null)
            {
                return true;
            }
            return false;
        }
        #endregion

        private void tabHK_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                PNHotKey hk = null;
                PNTreeItem tvi;
                switch (tabHK.SelectedIndex)
                {
                    case 0:
                        tvi = tvwHKMain.SelectedItem as PNTreeItem;
                        if (tvi == null) break;
                        hk = m_KeysMain.FirstOrDefault(h => h.MenuName == (string)tvi.Tag);
                        setButtonsEnabled(tvi);
                        break;
                    case 1:
                        tvi = tvwHKNote.SelectedItem as PNTreeItem;
                        if (tvi == null) break;
                        hk = m_KeysNote.FirstOrDefault(h => h.MenuName == (string)tvi.Tag);
                        setButtonsEnabled(tvi);
                        break;
                    case 2:
                        tvi = tvwHKEdit.SelectedItem as PNTreeItem;
                        if (tvi == null) break;
                        hk = m_KeysEdit.FirstOrDefault(h => h.MenuName == (string)tvi.Tag);
                        setButtonsEnabled(tvi);
                        break;
                    case 3:
                        tvi = tvwHKGroups.SelectedItem as PNTreeItem;
                        if (tvi == null) break;
                        hk = m_KeysGroups.FirstOrDefault(h => h.MenuName == (string)tvi.Tag);
                        setButtonsEnabled(tvi);
                        break;
                }
                if (hk != null)
                {
                    txtHotKey.Text = hk.Shortcut;
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
                if (saveHotkeys())
                {
                    PNStatic.FormMain.ApplyNewHotkeys();
                    DialogResult = true;
                }
                else
                {
                    DialogResult = false;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgHotkeys_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNStatic.FormHotkeys = this;
                PNLang.Instance.ApplyControlLanguage(this);
                Title = PNLang.Instance.GetCaptionText("dlg_hot_keys", "Hot keys");
                lblRestrictedHotkeys.Text += @" " + string.Join("  ", PNStrings.RestrictedHotkeys);

                m_KeysMain = PNStatic.HotKeysMain.PNClone();
                m_KeysNote = PNStatic.HotKeysNote.PNClone();
                m_KeysEdit = PNStatic.HotKeysEdit.PNClone();
                m_KeysGroups = PNStatic.HotKeysGroups.PNClone();

                foreach (var pm in PNMenus.CurrentMainMenus)
                    loadMenus(pm, null, HotkeyType.Main);
                tvwHKMain.ItemsSource = _ItemsMain;
                ((TreeViewItem)tvwHKMain.Items[0]).IsSelected = true;

                foreach (var pm in PNMenus.CurrentNoteMenus)
                    loadMenus(pm, null, HotkeyType.Note);
                tvwHKNote.ItemsSource = _ItemsNote;
                ((TreeViewItem)tvwHKNote.Items[0]).IsSelected = true;

                foreach (var pm in PNMenus.CurrentEditMenus)
                    loadMenus(pm, null, HotkeyType.Edit);
                tvwHKEdit.ItemsSource = _ItemsEdit;
                ((TreeViewItem)tvwHKEdit.Items[0]).IsSelected = true;

                var group = PNStatic.Groups.GetGroupByID((int)SpecialGroups.AllGroups);
                var captionShow = PNLang.Instance.GetCaptionText("show_group", "Show group");
                var captionHide = PNLang.Instance.GetCaptionText("hide_group", "Hide group");
                if (group != null)
                {
                    foreach (var g in group.Subgroups)
                    {
                        loadGroups(g, null, captionShow, captionHide);
                    }
                }
                tvwHKGroups.ItemsSource = _ItemsGroup;
                ((TreeViewItem)tvwHKGroups.Items[0]).IsSelected = true;

                grdDefHotkeys.ItemsSource = _DefKeys;
                loadDeterminatedKeys();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadGroups(PNGroup gr, PNTreeItem item, string captionShow, string captionHide)
        {
            try
            {
                var ti = new PNTreeItem("submnu", gr.Name, gr.ID.ToString(CultureInfo.InvariantCulture)) { IsExpanded = true };
                ti.Items.Add(new PNTreeItem("mnu", captionShow,
                    gr.ID.ToString(CultureInfo.InvariantCulture) + "_show"));
                ti.Items.Add(new PNTreeItem("mnu", captionHide,
                    gr.ID.ToString(CultureInfo.InvariantCulture) + "_hide"));
                if (item == null)
                {
                    _ItemsGroup.Add(ti);
                }
                else
                {
                    item.Items.Add(ti);
                }
                foreach (PNGroup g in gr.Subgroups)
                {
                    loadGroups(g, ti, captionShow, captionHide);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool isHotkeyInDatabase(List<PNHotKey> keys, PNHotKey hk)
        {
            return keys.Any(h => h.ID == hk.ID && h.Shortcut == hk.Shortcut && h.MenuName == hk.MenuName);
        }

        private void loadDeterminatedKeys()
        {
            try
            {
                foreach (var hk in m_KeysMain.Where(hk => hk.VK != 0))
                {
                    _DefKeys.Add(new DefKey(hk.ID, hk.Type, hk.MenuName)
                    {
                        Icon =
                            isHotkeyInDatabase(PNStatic.HotKeysMain, hk)
                                ? (BitmapSource)TryFindResource("check")
                                : null,
                        MenuRange = tbpHKMain.Header.ToString(),
                        MenuText = PNLang.Instance.GetMenuText("main_menu", hk.MenuName, hk.MenuName),
                        Shortcut = hk.Shortcut
                    });
                }
                foreach (var hk in m_KeysNote.Where(hk => hk.VK != 0))
                {
                    _DefKeys.Add(new DefKey(hk.ID, hk.Type, hk.MenuName)
                    {
                        Icon =
                            isHotkeyInDatabase(PNStatic.HotKeysNote, hk)
                                ? (BitmapSource)TryFindResource("check")
                                : null,
                        MenuRange = tbpHKNote.Header.ToString(),
                        MenuText = PNLang.Instance.GetMenuText("note_menu", hk.MenuName, hk.MenuName),
                        Shortcut = hk.Shortcut
                    });
                }
                foreach (var hk in m_KeysEdit.Where(hk => hk.VK != 0))
                {
                    _DefKeys.Add(new DefKey(hk.ID, hk.Type, hk.MenuName)
                    {
                        Icon =
                            isHotkeyInDatabase(PNStatic.HotKeysEdit, hk)
                                ? (BitmapSource)TryFindResource("check")
                                : null,
                        MenuRange = tbpHKEdit.Header.ToString(),
                        MenuText = PNLang.Instance.GetMenuText("edit_menu", hk.MenuName, hk.MenuName),
                        Shortcut = hk.Shortcut
                    });
                }
                foreach (var hk in m_KeysGroups.Where(hk => hk.VK != 0))
                {
                    var arr = hk.MenuName.Split('_');
                    if (arr.Length != 2) continue;
                    var gr = PNStatic.Groups.GetGroupByID(Convert.ToInt32(arr[0]));
                    if (gr == null) continue;
                    var caption = arr[1] == "show"
                        ? PNLang.Instance.GetCaptionText("show_group", "Show group")
                        : PNLang.Instance.GetCaptionText("hide_group", "Hide group");
                    _DefKeys.Add(new DefKey(hk.ID, hk.Type, hk.MenuName)
                    {
                        Icon =
                            isHotkeyInDatabase(PNStatic.HotKeysGroups, hk)
                                ? (BitmapSource)TryFindResource("check")
                                : null,
                        MenuRange = tbpHKGroups.Header.ToString(),
                        MenuText = gr.Name + "/" + caption,
                        Shortcut = hk.Shortcut
                    });
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadMenus(PNMenu mnu, PNTreeItem item, HotkeyType type)
        {
            try
            {
                if (mnu.Text == PNStrings.MENU_SEPARATOR_STRING) return;
                switch (mnu.Name)
                {
                    case "":
                    case "mnuSave":
                    case "mnuPrint":
                    case "mnuGroups":
                    case "mnuRemoveFromFavorites":
                    case "mnuRemovePassword":
                    case "mnuUnpin":
                    case "mnuUndo":
                    case "mnuRedo":
                    case "mnuCut":
                    case "mnuCopy":
                    case "mnuPaste":
                    case "mnuFind":
                    case "mnuFindNext":
                    case "mnuReplace":
                    case "mnuSearchWeb":
                    case "mnuSelectAll":
                    case "mnuShowByTag":
                    case "mnuHideByTag":
                    case "mnuFont":
                    case "mnuFontSize":
                    case "mnuFontColor":
                    case "mnuBold":
                    case "mnuItalic":
                    case "mnuUnderline":
                    case "mnuStrikethrough":
                    case "mnuHighlight":
                    case "mnuAlignLeft":
                    case "mnuAlignCenter":
                    case "mnuAlignRight":
                    case "mnuPostOn":
                    case "mnuPostNote":
                    case "mnuReplacePost":
                    case "mnuInsertPost":
                    case "mnuRun":
                        return;
                }
                var ti = new PNTreeItem(mnu.Items.Any() ? "submnu" : "mnu", mnu.Text, mnu.Name) { IsExpanded = true };
                foreach (var sg in mnu.Items)
                {
                    loadMenus(sg, ti, type);
                }
                if (item == null)
                {
                    switch (type)
                    {
                        case HotkeyType.Main:
                            _ItemsMain.Add(ti);
                            break;
                        case HotkeyType.Note:
                            _ItemsNote.Add(ti);
                            break;
                        case HotkeyType.Edit:
                            _ItemsEdit.Add(ti);
                            break;
                        case HotkeyType.Group:
                            _ItemsGroup.Add(ti);
                            break;
                    }
                }
                else
                    item.Items.Add(ti);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setButtonsEnabled(PNTreeItem node)
        {
            try
            {
                if (node.Image.Equals(_ImageSubmenu))
                {
                    //cmdAddHK.IsEnabled = false;
                    //cmdEditHK.IsEnabled = false;
                    cmdDeleteHK.IsEnabled = false;
                    txtHotKey.Visibility = Visibility.Hidden;
                }
                else
                {
                    var name = (string)node.Tag;
                    PNHotKey hk = null;
                    switch (tabHK.SelectedIndex)
                    {
                        case 0:
                            hk = m_KeysMain.FirstOrDefault(h => h.MenuName == name);
                            break;
                        case 1:
                            hk = m_KeysNote.FirstOrDefault(h => h.MenuName == name);
                            break;
                        case 2:
                            hk = m_KeysEdit.FirstOrDefault(h => h.MenuName == name);
                            break;
                        case 3:
                            hk = m_KeysGroups.FirstOrDefault(h => h.MenuName == name);
                            break;
                    }

                    if (hk == null) return;
                    cmdDeleteHK.IsEnabled = hk.VK != 0;
                    txtHotKey.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool saveHotkeys()
        {
            try
            {
                var sqlList = new List<string>();

                var count = m_KeysMain.Count;
                for (var i = 0; i < count; i++)
                {
                    if (PNStatic.HotKeysMain[i] == m_KeysMain[i]) continue;
                    //first unregister existing hot key
                    if (PNStatic.HotKeysMain[i].VK != 0)
                    {
                        HotkeysStatic.UnregisterHK(PNStatic.FormMain.Handle, PNStatic.HotKeysMain[i].ID);
                    }
                    //if new hot key has been set
                    if (m_KeysMain[i].VK != 0)
                    {
                        //now try to register
                        if (!HotkeysStatic.RegisterHK(PNStatic.FormMain.Handle, m_KeysMain[i])) continue;
                        //copy hot key
                        PNStatic.HotKeysMain[i].CopyFrom(m_KeysMain[i]);
                        var sqlQuery = createHotKeyUpdate(m_KeysMain[i]);
                        if (sqlQuery != "")
                        {
                            sqlList.Add(sqlQuery);
                        }
                    }
                    else
                    {
                        //hot key has been deleted
                        PNStatic.HotKeysMain[i].CopyFrom(m_KeysMain[i]);
                        var sqlQuery = createHotKeyDelete(m_KeysMain[i]);
                        if (sqlQuery != "")
                        {
                            sqlList.Add(sqlQuery);
                        }
                    }
                }

                count = m_KeysNote.Count;
                for (int i = 0; i < count; i++)
                {
                    if (PNStatic.HotKeysNote[i] == m_KeysNote[i]) continue;

                    //first unregister existing hot key
                    if (PNStatic.HotKeysNote[i].VK != 0)
                    {
                        HotkeysStatic.UnregisterHK(PNStatic.FormMain.Handle, PNStatic.HotKeysNote[i].ID);
                    }

                    //if new hot key has been set
                    if (m_KeysNote[i].VK != 0)
                    {
                        //now try to register
                        if (!HotkeysStatic.RegisterHK(PNStatic.FormMain.Handle, m_KeysNote[i])) continue;
                        //copy hot key
                        PNStatic.HotKeysNote[i].CopyFrom(m_KeysNote[i]);
                        var sqlQuery = createHotKeyUpdate(m_KeysNote[i]);
                        if (sqlQuery != "")
                        {
                            sqlList.Add(sqlQuery);
                        }
                    }
                    else
                    {
                        //hot key has been deleted
                        PNStatic.HotKeysNote[i].CopyFrom(m_KeysNote[i]);
                        var sqlQuery = createHotKeyDelete(m_KeysNote[i]);
                        if (sqlQuery != "")
                        {
                            sqlList.Add(sqlQuery);
                        }
                    }

                    ////copy hot key
                    //PNStatic.HotKeysNote[i].CopyFrom(m_KeysNote[i]);
                    //var sqlQuery = m_KeysNote[i].VK != 0
                    //    ? createHotKeyUpdate(m_KeysNote[i])
                    //    : createHotKeyDelete(m_KeysNote[i]);
                    //if (sqlQuery != "")
                    //{
                    //    sqlList.Add(sqlQuery);
                    //}
                }

                count = m_KeysEdit.Count;
                for (int i = 0; i < count; i++)
                {
                    if (PNStatic.HotKeysEdit[i] == m_KeysEdit[i]) continue;

                    //first unregister existing hot key
                    if (PNStatic.HotKeysEdit[i].VK != 0)
                    {
                        HotkeysStatic.UnregisterHK(PNStatic.FormMain.Handle, PNStatic.HotKeysEdit[i].ID);
                    }
                    //if new hot key has been set
                    if (m_KeysEdit[i].VK != 0)
                    {
                        //now try to register
                        if (!HotkeysStatic.RegisterHK(PNStatic.FormMain.Handle, m_KeysEdit[i])) continue;
                        //copy hot key
                        PNStatic.HotKeysEdit[i].CopyFrom(m_KeysEdit[i]);
                        var sqlQuery = createHotKeyUpdate(m_KeysEdit[i]);
                        if (sqlQuery != "")
                        {
                            sqlList.Add(sqlQuery);
                        }
                    }
                    else
                    {
                        //hot key has been deleted
                        PNStatic.HotKeysEdit[i].CopyFrom(m_KeysEdit[i]);
                        var sqlQuery = createHotKeyDelete(m_KeysEdit[i]);
                        if (sqlQuery != "")
                        {
                            sqlList.Add(sqlQuery);
                        }
                    }

                    ////copy hot key
                    //PNStatic.HotKeysEdit[i].CopyFrom(m_KeysEdit[i]);
                    //var sqlQuery = m_KeysEdit[i].VK != 0
                    //    ? createHotKeyUpdate(m_KeysEdit[i])
                    //    : createHotKeyDelete(m_KeysEdit[i]);
                    //if (sqlQuery != "")
                    //{
                    //    sqlList.Add(sqlQuery);
                    //}
                }

                count = m_KeysGroups.Count;
                for (var i = 0; i < count; i++)
                {
                    if (PNStatic.HotKeysGroups[i] == m_KeysGroups[i]) continue;
                    //first unregister existing hot key
                    if (PNStatic.HotKeysGroups[i].VK != 0)
                    {
                        HotkeysStatic.UnregisterHK(PNStatic.FormMain.Handle, PNStatic.HotKeysGroups[i].ID);
                    }
                    //if new hot key has been set
                    if (m_KeysGroups[i].VK != 0)
                    {
                        //now try to register
                        if (!HotkeysStatic.RegisterHK(PNStatic.FormMain.Handle, m_KeysGroups[i])) continue;
                        //copy hot key
                        PNStatic.HotKeysGroups[i].CopyFrom(m_KeysGroups[i]);
                        var sqlQuery = createHotKeyUpdate(m_KeysGroups[i]);
                        if (sqlQuery != "")
                        {
                            sqlList.Add(sqlQuery);
                        }
                    }
                    else
                    {
                        //hot key has been deleted
                        PNStatic.HotKeysGroups[i].CopyFrom(m_KeysGroups[i]);
                        var sqlQuery = createHotKeyDelete(m_KeysGroups[i]);
                        if (sqlQuery != "")
                        {
                            sqlList.Add(sqlQuery);
                        }
                    }
                }

                if (sqlList.Count <= 0) return false;
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    foreach (var s in sqlList)
                    {
                        oData.Execute(s);
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

        private string createHotKeyUpdate(PNHotKey hk)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("UPDATE HOT_KEYS SET MODIFIERS = ");
                sb.Append(((int)hk.Modifiers));
                sb.Append(", VK = ");
                sb.Append(hk.VK);
                sb.Append(", SHORTCUT = '");
                sb.Append(hk.Shortcut);
                sb.Append("' WHERE MENU_NAME = '");
                sb.Append(hk.MenuName);
                sb.Append("' AND HK_TYPE = ");
                sb.Append((int)hk.Type);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private string createHotKeyDelete(PNHotKey hk)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("UPDATE HOT_KEYS SET MODIFIERS = 0, VK = 0, SHORTCUT = ''");
                sb.Append(" WHERE MENU_NAME = '");
                sb.Append(hk.MenuName);
                sb.Append("' AND HK_TYPE = ");
                sb.Append((int)hk.Type);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private void tvwHKMain_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                txtHotKey.Text = "";
                var ti = e.NewValue as PNTreeItem;
                if (ti == null) return;
                txtHotKey.IsEnabled = !ti.Image.ToString().EndsWith("submnu.png");
                var hk = m_KeysMain.FirstOrDefault(h => h.MenuName == (string)ti.Tag);
                if (hk != null)
                {
                    txtHotKey.Text = hk.Shortcut;
                }
                setButtonsEnabled(ti);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwHKNote_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                txtHotKey.Text = "";
                var ti = e.NewValue as PNTreeItem;
                if (ti == null) return;
                txtHotKey.IsEnabled = !ti.Image.ToString().EndsWith("submnu.png");
                var hk = m_KeysNote.FirstOrDefault(h => h.MenuName == (string)ti.Tag);
                if (hk != null)
                {
                    txtHotKey.Text = hk.Shortcut;
                }
                setButtonsEnabled(ti);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwHKEdit_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                txtHotKey.Text = "";
                var ti = e.NewValue as PNTreeItem;
                if (ti == null) return;
                txtHotKey.IsEnabled = !ti.Image.ToString().EndsWith("submnu.png");
                var hk = m_KeysEdit.FirstOrDefault(h => h.MenuName == (string)ti.Tag);
                if (hk != null)
                {
                    txtHotKey.Text = hk.Shortcut;
                }
                setButtonsEnabled(ti);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwHKGroups_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                txtHotKey.Text = "";
                var ti = e.NewValue as PNTreeItem;
                if (ti == null) return;
                txtHotKey.IsEnabled = !ti.Image.ToString().EndsWith("submnu.png");
                var hk = m_KeysGroups.FirstOrDefault(h => h.MenuName == (string)ti.Tag);
                if (hk != null)
                {
                    txtHotKey.Text = hk.Shortcut;
                }
                setButtonsEnabled(ti);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtHotKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {

                var check = false;
                var modString = "";
                var keyString = "";
                var hk = new PNHotKey
                {
                    Modifiers = HotkeysStatic.GetModifiers(ref modString),
                    VK = HotkeysStatic.GetKey(ref keyString),
                    Shortcut = modString + keyString
                };

                if (hk.Modifiers == HotkeyModifiers.MOD_NONE)
                {
                    if (hk.VK >= HotkeysStatic.VK_F1 && hk.VK <= HotkeysStatic.VK_F24)
                    {
                        check = true;
                    }
                }
                else
                {
                    if (hk.VK > 0)
                    {
                        check = true;
                    }
                }
                if (!check)
                {
                    return;
                }
                string message, caption;
                if (PNStrings.RestrictedHotkeys.Contains(hk.Shortcut) ||
                    PNStatic.HotKeysEdit.Any(h => h.Modifiers == hk.Modifiers && h.VK == hk.VK) ||
                    PNStatic.HotKeysGroups.Any(h => h.Modifiers == hk.Modifiers && h.VK == hk.VK) ||
                    PNStatic.HotKeysNote.Any(h => h.Modifiers == hk.Modifiers && h.VK == hk.VK) ||
                    hk.Modifiers == HotkeyModifiers.MOD_SHIFT && (hk.VK >= HotkeysStatic.VK_A && hk.VK <= HotkeysStatic.VK_Z))
                {
                    message = hk.Shortcut + '\n';
                    message += PNLang.Instance.GetMessageText("hk_registered_1", "This combination of keys is already registered.");
                    message += '\n';
                    message += PNLang.Instance.GetMessageText("hk_registered_2", "Choose another one, please.");
                    caption = PNLang.Instance.GetCaptionText("restricted_keys", "Invalid keys");
                    PNMessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    e.Handled = true;
                    return;
                }
                if (HotkeysStatic.RegisterHK(_HwndSource.Handle, hk))
                {
                    HotkeysStatic.UnregisterHK(_HwndSource.Handle, hk.ID);
                }
                else
                {
                    message = hk.Shortcut + '\n';
                    message += PNLang.Instance.GetMessageText("hk_registered_1", "This combination of keys is already registered.");
                    message += '\n';
                    message += PNLang.Instance.GetMessageText("hk_registered_2", "Choose another one, please.");
                    caption = PNLang.Instance.GetCaptionText("restricted_keys", "Invalid keys");
                    PNMessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    e.Handled = true;
                    return;
                }
                PNTreeItem tvi = null;
                PNHotKey hkCurrent = null;
                switch (tabHK.SelectedIndex)
                {
                    case 0:
                        tvi = tvwHKMain.SelectedItem as PNTreeItem;
                        if (tvi == null) break;
                        hkCurrent = m_KeysMain.FirstOrDefault(h => h.MenuName == (string)tvi.Tag);
                        setButtonsEnabled(tvi);
                        break;
                    case 1:
                        tvi = tvwHKNote.SelectedItem as PNTreeItem;
                        if (tvi == null) break;
                        hkCurrent = m_KeysNote.FirstOrDefault(h => h.MenuName == (string)tvi.Tag);
                        setButtonsEnabled(tvi);
                        break;
                    case 2:
                        tvi = tvwHKEdit.SelectedItem as PNTreeItem;
                        if (tvi == null) break;
                        hkCurrent = m_KeysEdit.FirstOrDefault(h => h.MenuName == (string)tvi.Tag);
                        setButtonsEnabled(tvi);
                        break;
                    case 3:
                        tvi = tvwHKGroups.SelectedItem as PNTreeItem;
                        if (tvi == null) break;
                        hkCurrent = m_KeysGroups.FirstOrDefault(h => h.MenuName == (string)tvi.Tag);
                        setButtonsEnabled(tvi);
                        break;
                }
                if (hkCurrent != null)
                {
                    hkCurrent.Modifiers = hk.Modifiers;
                    hkCurrent.Shortcut = hk.Shortcut;
                    hkCurrent.VK = hk.VK;
                    setButtonsEnabled(tvi);
                    txtHotKey.Text = hk.Shortcut;
                    var defKey = new DefKey(hkCurrent.ID, hkCurrent.Type, hkCurrent.MenuName)
                    {
                        Icon =
                            isHotkeyInDatabase(
                                tabHK.SelectedIndex == 0
                                    ? PNStatic.HotKeysMain
                                    : (tabHK.SelectedIndex == 1
                                        ? PNStatic.HotKeysNote
                                        : (tabHK.SelectedIndex == 2 ? PNStatic.HotKeysEdit : PNStatic.HotKeysGroups)),
                                hk)
                                ? (BitmapSource)TryFindResource("check")
                                : null,
                        MenuRange =
                            tabHK.SelectedIndex == 0
                                ? tbpHKMain.Header.ToString()
                                : (tabHK.SelectedIndex == 1
                                    ? tbpHKNote.Header.ToString()
                                    : (tabHK.SelectedIndex == 2
                                        ? tbpHKEdit.Header.ToString()
                                        : tbpHKGroups.Header.ToString())),
                        MenuText =
                            tabHK.SelectedIndex < 3
                                ? PNLang.Instance.GetMenuText(
                                    tabHK.SelectedIndex == 0
                                        ? "main_menu"
                                        : (tabHK.SelectedIndex == 1 ? "note_menu" : "edit_menu"), hkCurrent.MenuName,
                                    hkCurrent.MenuName)
                                : hkCurrent.MenuName,
                        Shortcut = hkCurrent.Shortcut
                    };
                    if (tabHK.SelectedIndex == 3)
                    {
                        var arr = hkCurrent.MenuName.Split('_');
                        if (arr.Length == 2)
                        {
                            var gr = PNStatic.Groups.GetGroupByID(Convert.ToInt32(arr[0]));
                            if (gr != null)
                            {
                                var defCap = arr[1] == "show"
                                    ? PNLang.Instance.GetCaptionText("show_group", "Show group")
                                    : PNLang.Instance.GetCaptionText("hide_group", "Hide group");
                                defKey.MenuText = gr.Name + "/" + defCap;
                            }
                        }
                    }
                    //remove existing key
                    var existingKey = _DefKeys.FirstOrDefault(k => k.Id == defKey.Id);
                    if (existingKey != null)
                        _DefKeys.Remove(existingKey);
                    _DefKeys.Add(defKey);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgHotkeys_SourceInitialized(object sender, EventArgs e)
        {
            var handle = (new WindowInteropHelper(this)).Handle;
            _HwndSource = HwndSource.FromHwnd(handle);
        }

        private void txbWatermark_GotFocus(object sender, RoutedEventArgs e)
        {
            txtHotKey.Focus();
        }

        private void txtHotKey_LostFocus(object sender, RoutedEventArgs e)
        {
            txbWatermark.Visibility = string.IsNullOrEmpty(txtHotKey.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        private void txtHotKey_GotFocus(object sender, RoutedEventArgs e)
        {
            txbWatermark.Visibility = Visibility.Hidden;
        }

        private void txtHotKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            txbWatermark.Visibility = string.IsNullOrEmpty(txtHotKey.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        private void txbWatermark_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtHotKey.Focus();
        }

        private void DlgHotkeys_Closed(object sender, EventArgs e)
        {
            PNStatic.FormHotkeys = null;
        }

        private void cmdDeleteHK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var message = PNLang.Instance.GetMessageText("delete_hot_keys", "Delete selected hot keys combination?");
                if (PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                PNHotKey hk = null;
                PNTreeItem tvi = null;
                switch (tabHK.SelectedIndex)
                {
                    case 0:
                        tvi = tvwHKMain.SelectedItem as PNTreeItem;
                        if (tvi == null) break;
                        hk = m_KeysMain.FirstOrDefault(h => h.MenuName == (string)tvi.Tag);
                        setButtonsEnabled(tvi);
                        break;
                    case 1:
                        tvi = tvwHKNote.SelectedItem as PNTreeItem;
                        if (tvi == null) break;
                        hk = m_KeysNote.FirstOrDefault(h => h.MenuName == (string)tvi.Tag);
                        setButtonsEnabled(tvi);
                        break;
                    case 2:
                        tvi = tvwHKEdit.SelectedItem as PNTreeItem;
                        if (tvi == null) break;
                        hk = m_KeysEdit.FirstOrDefault(h => h.MenuName == (string)tvi.Tag);
                        setButtonsEnabled(tvi);
                        break;
                    case 3:
                        tvi = tvwHKGroups.SelectedItem as PNTreeItem;
                        if (tvi == null) break;
                        hk = m_KeysGroups.FirstOrDefault(h => h.MenuName == (string)tvi.Tag);
                        setButtonsEnabled(tvi);
                        break;
                }
                if (hk == null) return;
                var defKey = _DefKeys.FirstOrDefault(dk => dk.Id == hk.ID);
                if (defKey != null)
                {
                    _DefKeys.Remove(defKey);
                }
                hk.Clear();
                txtHotKey.Text = "";
                setButtonsEnabled(tvi);
                if (txtHotKey.IsFocused) PNStatic.SendKey(Key.Tab);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdDefHotkeys_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = grdDefHotkeys.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdDefHotkeys)) as DefKey;
                if (item == null) return;
                switch (item.MenuType)
                {
                    case HotkeyType.Main:
                        findTreeItembyHotKey(tvwHKMain, item.MenuName);
                        tabHK.SelectedIndex = 0;
                        break;
                    case HotkeyType.Note:
                        findTreeItembyHotKey(tvwHKNote, item.MenuName);
                        tabHK.SelectedIndex = 1;
                        break;
                    case HotkeyType.Edit:
                        findTreeItembyHotKey(tvwHKEdit, item.MenuName);
                        tabHK.SelectedIndex = 2;
                        break;
                    case HotkeyType.Group:
                        findTreeItembyHotKey(tvwHKGroups, item.MenuName);
                        tabHK.SelectedIndex = 3;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void findTreeItembyHotKey(ItemsControl parent, string menuName)
        {
            try
            {
                foreach (var item in parent.Items.OfType<PNTreeItem>())
                {
                    if (item.Tag != null && Convert.ToString(item.Tag) == menuName)
                    {
                        item.IsSelected = true;
                        item.BringIntoView();
                        return;
                    }
                    findTreeItembyHotKey(item, menuName);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
