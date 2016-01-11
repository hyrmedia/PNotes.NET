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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndAdjustAppearance.xaml
    /// </summary>
    public partial class WndAdjustAppearance
    {
        public static DependencyProperty SkinlessProperty = DependencyProperty.Register("Skinless",
            typeof(PNSkinlessDetails), typeof(WndAdjustAppearance),
            new FrameworkPropertyMetadata(new PNSkinlessDetails()));

        public static DependencyProperty SkinProperty = DependencyProperty.Register("Skin", typeof(PNSkinDetails),
            typeof(WndAdjustAppearance), new FrameworkPropertyMetadata(new PNSkinDetails()));

        internal event EventHandler<NoteAppearanceAdjustedEventArgs> NoteAppearanceAdjusted;

        public WndAdjustAppearance()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndAdjustAppearance(PNote note)
            : this()
        {
            _Note = note;
        }

        private readonly PNote _Note;
        private string _TransCaption;
        private double _Opacity;
        private bool _CustomOpacity, _CustomSkinless, _CustomSkin;
        PNGroup _Group;

        public PNSkinlessDetails Skinless
        {
            get { return (PNSkinlessDetails)GetValue(SkinlessProperty); }
            set { SetValue(SkinlessProperty, value); }
        }

        public PNSkinDetails Skin
        {
            get { return (PNSkinDetails)GetValue(SkinProperty); }
            set { SetValue(SkinProperty, value); }
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NoteAppearanceAdjusted != null)
                {
                    NoteAppearanceAdjusted(this, new NoteAppearanceAdjustedEventArgs(_CustomOpacity, _CustomSkinless, _CustomSkin, _Opacity, Skinless, Skin));
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdStandard_Click(object sender, RoutedEventArgs e)
        {
            Skinless = (PNSkinlessDetails)_Group.Skinless.Clone();
            Skin = _Group.Skin.PNClone();
            _Opacity = PNStatic.Settings.Behavior.Opacity;
            trkTrans.Value = (int)(100 - (_Opacity * 100));
            pckBGSknls.SelectedColor = Skinless.BackColor;
            blkCaption.DataContext = Skinless.CaptionFont;
            if (GridSkinnable.Visibility == Visibility.Visible)
            {
                if (Skin.SkinName != PNSkinDetails.NO_SKIN)
                {
                    lstSkins.SelectedItem =
                        lstSkins.Items.OfType<PNListBoxItem>().FirstOrDefault(it => it.Text == Skin.SkinName);
                }
                else
                {
                    lstSkins.SelectedIndex = -1;
                }
            }
            _CustomOpacity = _CustomSkin = _CustomSkinless = false;
        }

        private void cmdFontSknls_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fontChooser = new WndFontChooser(Skinless.CaptionFont, Skinless.CaptionColor) { Owner = this };
                var result = fontChooser.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    Skinless = new PNSkinlessDetails
                    {
                        CaptionFont = fontChooser.SelectedFont,
                        CaptionColor = fontChooser.SelectedColor,
                        BackColor = Skinless.BackColor
                    };
                    blkCaption.DataContext = Skinless.CaptionFont;
                    _CustomSkinless = true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgAdjustAppearance_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNStatic.Settings.GeneralSettings.UseSkins)
                {
                    GridSkinless.Visibility = Visibility.Hidden;
                    GridSkinnable.Visibility = Visibility.Visible;
                }
                _CustomOpacity = _Note.CustomOpacity;
                applyLanguage();
                if (_CustomOpacity)
                {
                    trkTrans.Value = (int)(100 - (_Note.Opacity * 100));
                    _Opacity = _Note.Opacity;
                }
                else
                {
                    trkTrans.Value = (int)(100 - (PNStatic.Settings.Behavior.Opacity * 100));
                    _Opacity = PNStatic.Settings.Behavior.Opacity;
                }
                _Group = PNStatic.Groups.GetGroupByID(_Note.GroupID);
                if (_Group == null)
                {
                    throw new Exception("Group cannot be null");
                }
                if (_Note.Skinless != null)
                {
                    _CustomSkinless = true;
                    Skinless = (PNSkinlessDetails)_Note.Skinless.Clone();
                }
                else
                {
                    Skinless = (PNSkinlessDetails)_Group.Skinless.Clone();
                }

                blkCaption.DataContext = Skinless.CaptionFont;

                pckBGSknls.SelectedColor = Skinless.BackColor;

                if (GridSkinnable.Visibility == Visibility.Visible)
                {
                    if (_Note.Skin != null)
                    {
                        _CustomSkin = true;
                        Skin = _Note.Skin.PNClone();
                    }
                    else
                    {
                        Skin = _Group.Skin.PNClone();
                    }
                    loadSkinsList();
                    if (Skin.SkinName != PNSkinDetails.NO_SKIN)
                    {
                        lstSkins.SelectedItem =
                            lstSkins.Items.OfType<PNListBoxItem>().FirstOrDefault(it => it.Text == Skin.SkinName);
                    }
                    else
                    {
                        lstSkins.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyLanguage()
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                Title += @" - " + _Note.Name;
                _TransCaption = PNLang.Instance.GetCaptionText("transparency", "Transparency");
                if (_CustomOpacity)
                {
                    lblTransPerc.Text = _TransCaption + @": " + (100.0 - (_Note.Opacity * 100)).ToString("0%");
                }
                else
                {
                    lblTransPerc.Text = _TransCaption + @": " + (100.0 - (PNStatic.Settings.Behavior.Opacity * 100)).ToString("0%");
                }
                blkCaption.Text = PNLang.Instance.GetControlText("previewCaption", "Caption");
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
                var image = TryFindResource("skins") as BitmapImage;// new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "skins.png"));
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

        private void pckBGSknls_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            try
            {
                Skinless = new PNSkinlessDetails
                {
                    CaptionFont = Skinless.CaptionFont,
                    CaptionColor = Skinless.CaptionColor,
                    BackColor = e.NewValue
                };
                blkCaption.DataContext = Skinless.CaptionFont;
                _CustomSkinless = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void trkTrans_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                var slider = sender as Slider;
                if (slider == null) return;
                slider.Value = Math.Round(e.NewValue, 0);
                _CustomOpacity = true;
                _Opacity = (100.0 - e.NewValue) / 100.0;
                lblTransPerc.Text = _TransCaption + @": " + trkTrans.Value.ToString(PNStatic.CultureInvariant) + @"%";
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
                if (Skin.SkinName != item.Text)
                {
                    _CustomSkin = true;
                    Skin.SkinName = item.Text;
                    var path = Path.Combine(PNPaths.Instance.SkinsDir, Skin.SkinName + PNStrings.SKIN_EXTENSION);
                    if (File.Exists(path))
                    {
                        PNSkinsOperations.LoadSkin(path, Skin);
                    }
                }
                if (Skin.SkinName != PNSkinDetails.NO_SKIN)
                {
                    PNStatic.DrawSkinPreview(_Group, Skin, imgSkin);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
