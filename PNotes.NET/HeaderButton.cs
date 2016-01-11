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
    public enum HeaderButtonType
    {
        Hide,
        Delete,
        Panel
    }

    public class HeaderButton : Button
    {
        public static readonly DependencyProperty ButtonTypeProperty;
        public static readonly DependencyProperty IsAlternatedProperty;

        static HeaderButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderButton), new FrameworkPropertyMetadata(typeof(HeaderButton)));
            ButtonTypeProperty = DependencyProperty.Register("ButtonType", typeof (HeaderButtonType),
                typeof (HeaderButton),
                new FrameworkPropertyMetadata(HeaderButtonType.Hide, OnButtonTypeChanged));
            IsAlternatedProperty = DependencyProperty.Register("IsAlternated", typeof(bool),
                typeof (HeaderButton),
                new FrameworkPropertyMetadata(false, OnIsAlternatedChanged));
        }

        public HeaderButtonType ButtonType 
        {
            get { return (HeaderButtonType) GetValue(ButtonTypeProperty); }
            set { SetValue(ButtonTypeProperty, value); }
        }

        public bool IsAlternated
        {
            get { return (bool)GetValue(IsAlternatedProperty); }
            set { SetValue(IsAlternatedProperty, value); }
        }

        private static void OnButtonTypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }

        private static void OnIsAlternatedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
