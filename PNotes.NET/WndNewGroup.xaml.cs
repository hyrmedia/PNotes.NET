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

using PNStaticFonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndNewGroup.xaml
    /// </summary>
    public partial class WndNewGroup
    {
        internal event EventHandler<GroupChangedEventArgs> GroupChanged;

        public WndNewGroup()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndNewGroup(PNGroup group, PNTreeItem treeItem)
            : this()
        {
            if (group != null)
            {
                m_Group = (PNGroup)group.Clone();
                m_Mode = AddEditMode.Edit;
            }
            else
            {
                m_Group = new PNGroup {IsDefaultImage = true};
            }
            _TreeItem=treeItem;
        }

        private readonly PNGroup m_Group;
        private readonly PNTreeItem _TreeItem;
        private readonly AddEditMode m_Mode = AddEditMode.Add;
        private bool m_Loaded;
        private bool _Shown;

        private void DlgNewGroup_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                loadSkinsList();
                Title = m_Mode == AddEditMode.Add
                           ? PNLang.Instance.GetCaptionText("add_new_group", "Add new group")
                           : PNLang.Instance.GetCaptionText("edit_group", "Edit group") + @" [" + m_Group.Name + @"]";
                loadLogFonts();

                fillGroupProperties();

                cboFontColor.IsDropDownOpen = true;
                cboFontColor.IsDropDownOpen = false;

                cboFontSize.IsDropDownOpen = true;
                cboFontSize.IsDropDownOpen = false;

                m_Loaded = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            try
            {
                base.OnContentRendered(e);
                if (_Shown) return;
                _Shown = true;
                txtName.SelectAll();
                txtName.Focus();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdChangeIcon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlgIcons = new WndFolderIcons {Owner = this};
                dlgIcons.GroupPropertyChanged += dlgIcons_GroupPropertyChanged;
                var showDialog = dlgIcons.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgIcons.GroupPropertyChanged -= dlgIcons_GroupPropertyChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdStandard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                m_Loaded = false;
                var gr = (PNGroup) m_Group.Clone();

                m_Group.Clear();
                m_Group.Name = gr.ID == 0 ? PNLang.Instance.GetGroupName("general", "General") : gr.Name;
                m_Group.ID = gr.ID;
                m_Group.ParentID = gr.ParentID;
                var image = TryFindResource("gr") as BitmapImage;
                    //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "gr.png"));
                m_Group.Image = image;
                m_Group.IsDefaultImage = true;

                imgGroupIcon.Source = m_Group.Image;
                fillGroupProperties();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                m_Loaded = true;
            }
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GroupChanged != null)
                {
                    GroupChanged(this, new GroupChangedEventArgs(m_Group, m_Mode, _TreeItem));
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void pckBGSknls_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            try
            {
                if (!m_Loaded) return;
                m_Group.Skinless.BackColor = e.NewValue;
                PNStatic.DrawSkinlessPreview(m_Group, brdFrame, blkCaption, brdBody, blkBody);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdFontSknls_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fc = new WndFontChooser(m_Group.Skinless.CaptionFont, m_Group.Skinless.CaptionColor) { Owner = this };
                var showDialog = fc.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    m_Group.Skinless.CaptionFont = fc.SelectedFont;
                    m_Group.Skinless.CaptionColor = fc.SelectedColor;
                    PNStatic.DrawSkinlessPreview(m_Group, brdFrame, blkCaption, brdBody, blkBody);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstSkins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lstSkins.SelectedIndex < 0)
                {
                    imgSkin.Source = null;
                    return;
                }
                var item = lstSkins.SelectedItem as PNListBoxItem;
                if (item == null) return;
                if (m_Group.Skin.SkinName != item.Text)
                {
                    m_Group.Skin.SkinName = item.Text;
                    var path = Path.Combine(PNPaths.Instance.SkinsDir, m_Group.Skin.SkinName + PNStrings.SKIN_EXTENSION);
                    if (File.Exists(path))
                    {
                        PNSkinsOperations.LoadSkin(path, m_Group.Skin);
                    }
                }
                if (m_Group.Skin.SkinName != PNSkinDetails.NO_SKIN)
                {
                    PNStatic.DrawSkinPreview(m_Group, m_Group.Skin, imgSkin);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadSkinsList()
        {
            try
            {
                if (!Directory.Exists(PNPaths.Instance.SkinsDir)) return;
                var di = new DirectoryInfo(PNPaths.Instance.SkinsDir);
                var fi = di.GetFiles("*.pnskn");
                var image = TryFindResource("skins") as BitmapImage;//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "skins.png"));
                foreach (var f in fi)
                {
                    lstSkins.Items.Add(new PNListBoxItem(image, Path.GetFileNameWithoutExtension(f.Name), f.FullName));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadLogFonts()
        {
            try
            {
                var list = new List<LOGFONT>();
                PNStaticFonts.Fonts.GetFontsList(list);
                var ordered = list.OrderBy(f => f.lfFaceName);
                foreach (var lf in ordered)
                {
                    cboFonts.Items.Add(lf);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillGroupProperties()
        {
            try
            {
                imgGroupIcon.Source = m_Group.Image;
                txtName.Text = m_Group.Name;
                if (m_Group.ID == 0)
                {
                    //general
                    txtName.IsReadOnly = true;
                }
                pckBGSknls.SelectedColor = m_Group.Skinless.BackColor;
                PNStatic.DrawSkinlessPreview(m_Group, brdFrame, blkCaption, brdBody, blkBody);
                if (m_Group.Skin.SkinName != PNSkinDetails.NO_SKIN)
                {
                    lstSkins.SelectedItem =
                        lstSkins.Items.OfType<PNListBoxItem>().FirstOrDefault(it => it.Text == m_Group.Skin.SkinName);
                }
                else
                {
                    lstSkins.SelectedIndex = -1;
                }
                cboFonts.SelectedItem =
                    cboFonts.Items.OfType<LOGFONT>().FirstOrDefault(lf => lf.lfFaceName == m_Group.Font.lfFaceName);
                foreach (var t in from object t in cboFontColor.Items
                    let rc = t as Rectangle
                    where rc != null
                    let sb = rc.Fill as SolidColorBrush
                    where sb != null
                    where
                        sb.Color ==
                        Color.FromArgb(m_Group.FontColor.A, m_Group.FontColor.R, m_Group.FontColor.G,
                            m_Group.FontColor.B)
                    select t)
                {
                    cboFontColor.SelectedItem = t;
                    break;
                }

                var fontHeight = m_Group.Font.GetFontSize();
                cboFontSize.SelectedItem = fontHeight;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                cmdOK.IsEnabled = txtName.Text.Trim().Length > 0;
                if (txtName.Text.Trim().Length > 0)
                {
                    m_Group.Name = txtName.Text.Trim();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!m_Loaded || cboFonts.SelectedIndex < 0) return;
                var lf = (LOGFONT)e.AddedItems[0];
                var logF = new LOGFONT();
                logF.Init();
                logF.SetFontFace(lf.lfFaceName);
                logF.SetFontSize((int) cboFontSize.SelectedItem);
                m_Group.Font = logF;
                PNStatic.DrawSkinlessPreview(m_Group, brdFrame, blkCaption, brdBody, blkBody);
                if (lstSkins.SelectedIndex >= 0)
                {
                    PNStatic.DrawSkinPreview(m_Group, m_Group.Skin, imgSkin);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgIcons_GroupPropertyChanged(object sender, GroupPropertyChangedEventArgs e)
        {
            try
            {
                var d = sender as WndFolderIcons;
                if (d != null) d.GroupPropertyChanged -= dlgIcons_GroupPropertyChanged;
                imgGroupIcon.Source = (BitmapImage)e.NewStateObject;
                m_Group.Image = (BitmapImage)e.NewStateObject;
                m_Group.IsDefaultImage = false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!m_Loaded || cboFontColor.SelectedIndex < 0) return;
                var rc = cboFontColor.SelectedItem as Rectangle;
                if (rc == null) return;
                var sb = rc.Fill as SolidColorBrush;
                if (sb == null) return;
                m_Group.FontColor = System.Drawing.Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B);
                PNStatic.DrawSkinlessPreview(m_Group, brdFrame, blkCaption, brdBody, blkBody);
                if (lstSkins.SelectedIndex >= 0)
                {
                    PNStatic.DrawSkinPreview(m_Group, m_Group.Skin, imgSkin);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!m_Loaded || cboFontSize.SelectedIndex < 0) return;
                var logF = new LOGFONT();
                logF.Init();
                logF.SetFontFace(m_Group.Font.lfFaceName);
                logF.SetFontSize((int)cboFontSize.SelectedItem);
                m_Group.Font = logF;
                PNStatic.DrawSkinlessPreview(m_Group, brdFrame, blkCaption, brdBody, blkBody);
                if (lstSkins.SelectedIndex >= 0)
                {
                    PNStatic.DrawSkinPreview(m_Group, m_Group.Skin, imgSkin);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
