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
    public class FixedWidthColumn : GridViewColumn
    {
        public event EventHandler<GridColumnVisibilityChangedEventArgs> VisibilityChanged;
        public event EventHandler<GridColumnWidthChangedEventArgs> WidthChanged;

        public static readonly DependencyProperty FixedWidthProperty = DependencyProperty.Register("FixedWidth",
            typeof(double), typeof(FixedWidthColumn),
            new FrameworkPropertyMetadata(double.NaN, OnFixedWidthChanged));

        public static readonly DependencyProperty AllowResizeProperty = DependencyProperty.Register("AllowResize",
            typeof(bool), typeof(FixedWidthColumn), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty SavedWidthProperty = DependencyProperty.Register("SavedWidth",
            typeof(double), typeof(FixedWidthColumn), new FrameworkPropertyMetadata(double.NaN));

        public static readonly DependencyProperty DefaultWidthProperty = DependencyProperty.Register("DefaultWidth",
            typeof (double), typeof (FixedWidthColumn), new FrameworkPropertyMetadata(double.NaN));

        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility",
            typeof(Visibility), typeof(FixedWidthColumn),
            new FrameworkPropertyMetadata(Visibility.Visible, OnVisibilityChanged));

        public static readonly DependencyProperty DisplayIndexProperty = DependencyProperty.Register("DisplayIndex",
            typeof (int), typeof (FixedWidthColumn), new FrameworkPropertyMetadata(0));

        public static readonly DependencyProperty OriginalIndexProperty = DependencyProperty.Register("OriginalIndex",
            typeof(int), typeof(FixedWidthColumn), new FrameworkPropertyMetadata(0));

        private static bool _savedAllowResize;

        static FixedWidthColumn()
        {
            WidthProperty.OverrideMetadata(typeof(FixedWidthColumn),
                new FrameworkPropertyMetadata(null, OnCoerceWidth));
        }

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == "ActualWidth" && Math.Abs(ActualWidth) > double.Epsilon && AllowResize)
            {
                if (WidthChanged != null) WidthChanged(this, new GridColumnWidthChangedEventArgs(ActualWidth));
            }
        }

        public double DefaultWidth
        {
            get { return (double) GetValue(DefaultWidthProperty); }
            set { SetValue(DefaultWidthProperty, value); }
        }

        public int DisplayIndex
        {
            get { return (int) GetValue(DisplayIndexProperty); }
            set { SetValue(DisplayIndexProperty, value); }
        }

        public int OriginalIndex
        {
            get { return (int)GetValue(OriginalIndexProperty); }
            set { SetValue(OriginalIndexProperty, value); }
        }

        public Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }

        public double SavedWidth
        {
            get { return (double)GetValue(SavedWidthProperty); }
            set { SetValue(SavedWidthProperty, value); }
        }

        public bool AllowResize
        {
            get { return (bool)GetValue(AllowResizeProperty); }
            set { SetValue(AllowResizeProperty, value); }
        }

        public double FixedWidth
        {
            get { return (double)GetValue(FixedWidthProperty); }
            set { SetValue(FixedWidthProperty, value); }
        }

        private static void OnVisibilityChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var fwc = o as FixedWidthColumn;
            if (fwc == null) return;
            if ((Visibility) e.NewValue == Visibility.Visible)
            {
                fwc.FixedWidth = fwc.SavedWidth;
                fwc.SavedWidth = fwc.FixedWidth;
                fwc.AllowResize = _savedAllowResize;
            }
            else
            {
                _savedAllowResize = fwc.AllowResize;
                fwc.FixedWidth = 0;
                fwc.AllowResize = false;
            }
            fwc.Width = fwc.ActualWidth;
            if (fwc.VisibilityChanged != null)
                fwc.VisibilityChanged(fwc,
                    new GridColumnVisibilityChangedEventArgs((Visibility) e.OldValue, (Visibility) e.NewValue));
        }

        private static void OnFixedWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var fwc = o as FixedWidthColumn;
            if (fwc == null) return;
            fwc.CoerceValue(WidthProperty);
            fwc.SavedWidth = (double)e.OldValue;
        }

        private static object OnCoerceWidth(DependencyObject o, object baseValue)
        {
            var fwc = o as FixedWidthColumn;
            if (fwc != null)
                return !fwc.AllowResize ? fwc.FixedWidth : baseValue;
            return baseValue;
        }
    }

    public class GridColumnWidthChangedEventArgs : EventArgs
    {
        public double ActualWidth { get; private set; }

        public GridColumnWidthChangedEventArgs(double width)
        {
            ActualWidth = width;
        }
    }

    public class GridColumnVisibilityChangedEventArgs : EventArgs
    {
        public Visibility OldValue { get; private set; }
        public Visibility NewValue { get; private set; }

        public GridColumnVisibilityChangedEventArgs(Visibility oldValue, Visibility newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
