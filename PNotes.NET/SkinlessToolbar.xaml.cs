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
using System.Windows;
using PNStaticFonts;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for SkinlessToolbar.xaml
    /// </summary>
    public partial class SkinlessToolbar : IToolbar
    {
        internal event EventHandler<FormatButtonClickedEventArgs> FormatButtonClicked;
        public event EventHandler FontComboDropDownClosed;

        public SkinlessToolbar()
        {
            InitializeComponent();
        }

        private readonly List<FormatButton> _FormatButtons = new List<FormatButton>();

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            _FormatButtons.Add(FontFamilyButton);
            _FormatButtons.Add(FontSizeButton);
            _FormatButtons.Add(FontColorButton);
            _FormatButtons.Add(FontBoldButton);
            _FormatButtons.Add(FontItalicButton);
            _FormatButtons.Add(FontUnderlineButton);
            _FormatButtons.Add(FontStrikethroughButton);
            _FormatButtons.Add(HighlightButton);
            _FormatButtons.Add(LeftButton);
            _FormatButtons.Add(CenterButton);
            _FormatButtons.Add(RightButton);
            _FormatButtons.Add(BulletsButton);
        }

        private void FormatButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FormatButton;
            if (button == null || FormatButtonClicked == null) return;
            FormatButtonClicked(this, new FormatButtonClickedEventArgs(button.ButtonType));
        }

        public void LoadLogFonts()
        {
            var list = new List<LOGFONT>();
            Fonts.GetFontsList(list);
            var ordered = list.OrderBy(f => f.lfFaceName);
            foreach (var lf in ordered)
            {
                cboFonts.Items.Add(lf);
            }
        }

        public void ShowFontsComboBox(string fontString)
        {
            PopUpFont.IsOpen = true;
            var item = cboFonts.Items.OfType<LOGFONT>().FirstOrDefault(lf => lf.lfFaceName == fontString);
            if (item != default(LOGFONT))
            {
                var index = cboFonts.Items.IndexOf(item);
                cboFonts.SelectedIndex = index;
            }
            else
            {
                cboFonts.SelectedIndex = -1;
            }
        }

        public ToolStripButtonSize GetButtonSize()
        {
            return FontFamilyButton.ButtonSize;
        }

        public void SetButtonSize(ToolStripButtonSize size)
        {
            Height = (int)size + 6;
            foreach (var b in _FormatButtons)
                b.ButtonSize = size;
        }

        //public void SetButtonsVisibility(Visibility value)
        //{
        //    foreach (var b in _FormatButtons)
        //        b.Visibility = value;
        //}

        //public void SetButtonVisibility(FormatType type, Visibility value)
        //{
        //    var b = _FormatButtons.FirstOrDefault(f => f.ButtonType == type);
        //    if (b != null)
        //        b.Visibility = value;
        //}

        public void SetButtonTooltip(FormatType type, string toolTip)
        {
            var b = _FormatButtons.FirstOrDefault(f => f.ButtonType == type);
            if (b != null)
                b.ToolTip = toolTip;
        }

        private void cboFonts_DropDownClosed(object sender, EventArgs e)
        {
            PopUpFont.IsOpen = false;
            if (FontComboDropDownClosed != null)
            {
                FontComboDropDownClosed(sender, e);
            }
        }

        //public double GetButtonOffset(FormatType type)
        //{
        //    var b = _FormatButtons.FirstOrDefault(f => f.ButtonType == type);
        //    if (b == null) return 0;
        //    var p = b.TransformToAncestor(this).Transform(new Point(0, 0));
        //    return p.X;
        //}
    }
}
