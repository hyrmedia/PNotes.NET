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

namespace PNotes.NET
{
    public class PNUtils
    {
        public static readonly DependencyProperty IsBigIconProperty = DependencyProperty.RegisterAttached("IsBigIcon",
            typeof (bool), typeof (PNUtils), new FrameworkPropertyMetadata(false, null));

        public static readonly DependencyProperty SmallButtonTypeProperty =
            DependencyProperty.RegisterAttached("SmallButtonType", typeof (SmallBarButtonType), typeof (PNUtils),
                new FrameworkPropertyMetadata(SmallBarButtonType.Add));

        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.RegisterAttached("IsHighlighted", typeof (bool), typeof (PNUtils),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty GroupDataProperty = DependencyProperty.RegisterAttached("GroupData",
            typeof (string), typeof (PNUtils), new FrameworkPropertyMetadata(""));

        public static readonly DependencyProperty FormatTypeProperty = DependencyProperty.RegisterAttached("FormatType",
            typeof(FormatType), typeof(PNUtils), new FrameworkPropertyMetadata(FormatType.FontFamily));

        public static readonly DependencyProperty MarkTypeProperty = DependencyProperty.RegisterAttached("MarkType",
            typeof (MarkType), typeof (PNUtils), new FrameworkPropertyMetadata(MarkType.Change));

        public static readonly DependencyProperty IsResourceImageProperty =
            DependencyProperty.RegisterAttached("IsResourceImage", typeof (bool), typeof (PNUtils),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty ImageSourceKeyProperty =
            DependencyProperty.RegisterAttached("ImageSourceKey", typeof (object), typeof (PNUtils),
                new FrameworkPropertyMetadata(String.Empty, ImageSourceKeyChanged));

        public static readonly DependencyProperty PanelOrientationProperty =
            DependencyProperty.RegisterAttached("PanelOrientation", typeof (NotesPanelOrientation), typeof (PNUtils),
                new FrameworkPropertyMetadata(NotesPanelOrientation.Top));

        public static NotesPanelOrientation GetPanelOrientation(DependencyObject obj)
        {
            return (NotesPanelOrientation) obj.GetValue(PanelOrientationProperty);
        }

        public static void SetPanelOrientation(DependencyObject obj, NotesPanelOrientation value)
        {
            obj.SetValue(PanelOrientationProperty, value);
        }

        public static object GetImageSourceKey(DependencyObject obj)
        {
            return obj.GetValue(ImageSourceKeyProperty);
        }

        public static void SetImageSourceKey(DependencyObject obj, object value)
        {
            obj.SetValue(ImageSourceKeyProperty, value);
        }

        private static void ImageSourceKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var image = d as Image;
            if (image == null) return;
            image.SetResourceReference(Image.SourceProperty, e.NewValue);
        }

        public static bool GetIsResourceImage(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsResourceImageProperty);
        }

        public static void SetIsResourceImage(DependencyObject obj, bool value)
        {
            obj.SetValue(IsResourceImageProperty, value);
        }

        public static MarkType GetMarkType(DependencyObject obj)
        {
            return (MarkType) obj.GetValue(MarkTypeProperty);
        }

        public static void SetMarkType(DependencyObject obj, MarkType value)
        {
            obj.SetValue(MarkTypeProperty, value);
        }

        public static FormatType GetFormatType(DependencyObject obj)
        {
            return (FormatType) obj.GetValue(FormatTypeProperty);
        }

        public static void SetFormatType(DependencyObject obj, FormatType value)
        {
            obj.SetValue(FormatTypeProperty, value);
        }

        public static string GetGroupData(DependencyObject obj)
        {
            return (string) obj.GetValue(GroupDataProperty);
        }

        public static void SetGroupData(DependencyObject obj, string value)
        {
            obj.SetValue(GroupDataProperty, value);
        }

        public static bool GetIsHighlighted(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsHighlightedProperty);
        }

        public static void SetIsHighlighted(DependencyObject obj, bool value)
        {
            obj.SetValue(IsHighlightedProperty, value);
        }

        public static SmallBarButtonType GetSmallButtonType(DependencyObject obj)
        {
            return (SmallBarButtonType) obj.GetValue(SmallButtonTypeProperty);
        }

        public static void SetSmallButtonType(DependencyObject obj, SmallBarButtonType value)
        {
            obj.SetValue(SmallButtonTypeProperty, value);
        }

        public static bool GetIsBigIcon(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsBigIconProperty);
        }

        public static void SetIsBigIcon(DependencyObject obj, bool value)
        {
            obj.SetValue(IsBigIconProperty, value);
        }
    }
}
