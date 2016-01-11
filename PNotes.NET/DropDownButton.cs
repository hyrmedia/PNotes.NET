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
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PNotes.NET
{
    public class DropDownButton : ToggleButton
    {
        public event EventHandler DropDownOpened;

        public static readonly DependencyProperty DropDownMenuProperty = DependencyProperty.Register("DropDownMenu",
            typeof (ContextMenu), typeof (DropDownButton), new FrameworkPropertyMetadata(null, OnDropDownMenuChanged));

        static DropDownButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownButton), new FrameworkPropertyMetadata(typeof(DropDownButton)));
        }

        public ContextMenu DropDownMenu
        {
            get { return (ContextMenu) GetValue(DropDownMenuProperty); }
            set { SetValue(DropDownMenuProperty, value); }
        }

        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            if (IsChecked != null && IsChecked.Value)
            {
                if (DropDownMenu == null || DropDownMenu.Items.Count == 0)
                    IsChecked = false;
                else
                    DropDownMenu.IsOpen = true;
            }
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private static void OnDropDownMenuChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var ddb = o as DropDownButton;
            if (ddb == null) return;
            var prevDropDown = e.OldValue as ContextMenu;
            if (prevDropDown != null)
            {
                prevDropDown.Closed -= dropDown_Closed;
                prevDropDown.Opened -= dropDown_Opened;
            }
            if (e.NewValue == null) return;
            var dropDown = e.NewValue as ContextMenu;
            if (dropDown == null) return;
            dropDown.Closed += dropDown_Closed;
            dropDown.Opened += dropDown_Opened;
            dropDown.PlacementTarget = ddb;
            dropDown.Placement = PlacementMode.Bottom;
        }

        static void dropDown_Opened(object sender, RoutedEventArgs e)
        {
            var ctm = sender as ContextMenu;
            if (ctm == null) return;
            var target = ctm.PlacementTarget as DropDownButton;
            if (target == null) return;
            if (target.DropDownOpened == null) return;
            target.DropDownOpened(target, new EventArgs());
        }

        static void dropDown_Closed(object sender, RoutedEventArgs e)
        {
            var ctm = sender as ContextMenu;
            if (ctm == null) return;
            var target = ctm.PlacementTarget as DropDownButton;
            if (target == null) return;
            target.IsChecked = false;
        }
    }
}
