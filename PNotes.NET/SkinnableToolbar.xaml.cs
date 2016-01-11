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
using System.Windows.Controls;
using System.Windows.Input;
using PNStaticFonts;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for SkinnableToolbar.xaml
    /// </summary>
    public partial class SkinnableToolbar : IToolbar
    {
        internal event EventHandler<FormatButtonClickedEventArgs> FormatButtonClicked;
        public event EventHandler FontComboDropDownClosed;

        public SkinnableToolbar()
        {
            InitializeComponent();
        }

        private readonly List<Image> _FormatButtons = new List<Image>();

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

        private void FormatButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //otherwise mose up is not raised
            e.Handled = true;
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
            //do nothing
            return ToolStripButtonSize.Normal;
        }

        public void SetButtonSize(ToolStripButtonSize size)
        {
            //do nothing
        }

        public void SetButtonTooltip(FormatType type, string toolTip)
        {
            var b = _FormatButtons.FirstOrDefault(f => PNUtils.GetFormatType(f) == type);
            if (b != null)
                b.ToolTip = toolTip;
        }

        private void FormatButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;
            if (image == null || FormatButtonClicked == null) return;
            var formatType = PNUtils.GetFormatType(image);
            FormatButtonClicked(this, new FormatButtonClickedEventArgs(formatType));
            e.Handled = true;
        }

        private void cboFonts_DropDownClosed(object sender, EventArgs e)
        {
            PopUpFont.IsOpen = false;
            if (FontComboDropDownClosed != null)
            {
                FontComboDropDownClosed(sender, e);
            }
        }
    }
}
