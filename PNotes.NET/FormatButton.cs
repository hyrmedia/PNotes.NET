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

using System.Windows;
using System.Windows.Controls;

namespace PNotes.NET
{
    public enum FormatType
    {
        FontFamily,
        FontSize,
        FontColor,
        FontBold,
        FontItalic,
        FontUnderline,
        FontStrikethrough,
        Highlight,
        Left,
        Center,
        Right,
        Bullets
    }

    public class FormatButton : Button
    {
        public static readonly DependencyProperty ButtonTypeProperty;
        public static readonly DependencyProperty ButtonSizeProperty;

        static FormatButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FormatButton), new FrameworkPropertyMetadata(typeof(FormatButton)));
            ButtonTypeProperty = DependencyProperty.Register("ButtonType", typeof(FormatType),
                typeof(FormatButton),
                new FrameworkPropertyMetadata(FormatType.FontFamily, OnButtonTypeChanged));
            ButtonSizeProperty = DependencyProperty.Register("ButtonSize", typeof(ToolStripButtonSize),
                typeof(FormatButton),
                new FrameworkPropertyMetadata(ToolStripButtonSize.Normal, OnButtonSizeChanged));
        }

        public FormatType ButtonType
        {
            get { return (FormatType)GetValue(ButtonTypeProperty); }
            set { SetValue(ButtonTypeProperty, value); }
        }

        private static void OnButtonTypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }

        public ToolStripButtonSize ButtonSize
        {
            get { return (ToolStripButtonSize)GetValue(ButtonSizeProperty); }
            set { SetValue(ButtonSizeProperty, value); }
        }

        private static void OnButtonSizeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
