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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PNotes.NET
{
    public enum ListSort
    {
        None,
        Ascending,
        Descending
    }

    public class PNGridViewHelper
    {
        public static readonly DependencyProperty ShowSortArrowProperty =
            DependencyProperty.RegisterAttached("ShowSortArrow", typeof (bool), typeof (PNGridViewHelper),
                new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.RegisterAttached("SortDirection", typeof (ListSort), typeof (PNGridViewHelper),
                new FrameworkPropertyMetadata(ListSort.None));

        public static readonly DependencyProperty ColumnNameProperty = DependencyProperty.RegisterAttached(
            "ColumnName", typeof (string), typeof (PNGridViewHelper), new FrameworkPropertyMetadata(""));

        public static readonly DependencyProperty ColumnTagProperty = DependencyProperty.RegisterAttached("ColumnTag",
            typeof (object), typeof (PNGridViewHelper), new FrameworkPropertyMetadata(null));

        // Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.RegisterAttached(
                "PropertyName",
                typeof(string),
                typeof(PNGridViewHelper),
                new UIPropertyMetadata(null)
            );

        // Using a DependencyProperty as the backing store for AutoSort.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoSortProperty =
            DependencyProperty.RegisterAttached(
                "AutoSort",
                typeof(bool),
                typeof(PNGridViewHelper),
                new UIPropertyMetadata(
                    false,
                    (o, e) =>
                    {
                        var listView = o as ListView;
                        if (listView == null) return;
                        if (GetCommand(listView) != null) return;
                        var oldValue = (bool)e.OldValue;
                        var newValue = (bool)e.NewValue;
                        if (oldValue && !newValue)
                        {
                            listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                        }
                        if (!oldValue && newValue)
                        {
                            listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                        }
                    }
                )
            );

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(PNGridViewHelper),
                new UIPropertyMetadata(
                    null,
                    (o, e) =>
                    {
                        var listView = o as ItemsControl;
                        if (listView == null) return;
                        if (GetAutoSort(listView)) return;
                        if (e.OldValue != null && e.NewValue == null)
                        {
                            listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                        }
                        if (e.OldValue == null && e.NewValue != null)
                        {
                            listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                        }
                    }
                )
            );

        public static readonly DependencyProperty ShowGridLinesProperty =
            DependencyProperty.RegisterAttached("ShowGridLines", typeof(bool), typeof(PNGridViewHelper),
                new UIPropertyMetadata(false));

        public static bool GetShowGridLines(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowGridLinesProperty);
        }

        public static void SetShowGridLines(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowGridLinesProperty, value);
        }

        public static ListSort GetSortDirection(DependencyObject obj)
        {
            if (obj == null) return ListSort.None;
            try
            {
                var sort = obj.GetValue(SortDirectionProperty);
                if (sort == null) return ListSort.None;
                return (ListSort)sort;
            }
            catch
            {
                return ListSort.None;
            }
        }

        public static void SetSortDirection(DependencyObject obj, ListSort value)
        {
            obj.SetValue(SortDirectionProperty, value);
        }

        public static bool GetShowSortArrow(DependencyObject obj)
        {
            if (obj == null) return false;
            return (bool) obj.GetValue(ShowSortArrowProperty);
        }

        public static void SetShowSortArrow(DependencyObject obj, bool value)
        {
            if (obj == null) return;
            obj.SetValue(ShowSortArrowProperty, value);
        }

        public static object GetColumnTag(DependencyObject obj)
        {
            return obj.GetValue(ColumnTagProperty);
        }

        public static void SetColumnTag(DependencyObject obj, object value)
        {
            obj.SetValue(ColumnTagProperty, value);
        }

        public static string GetColumnName(DependencyObject obj)
        {
            return (string) obj.GetValue(ColumnNameProperty);
        }

        public static void SetColumnName(DependencyObject obj, string value)
        {
            obj.SetValue(ColumnNameProperty, value);
        }

        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        public static bool GetAutoSort(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoSortProperty);
        }

        public static void SetAutoSort(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoSortProperty, value);
        }

        public static string GetPropertyName(DependencyObject obj)
        {
            if (obj == null)
                return null;
            try
            {
                var propertyName = (string) obj.GetValue(PropertyNameProperty);
                if (!string.IsNullOrEmpty(propertyName)) return propertyName;
                var viewColumn = obj as GridViewColumn;
                if (viewColumn == null) return propertyName;
                if (viewColumn.DisplayMemberBinding != null)
                {
                    propertyName = ((Binding)viewColumn.DisplayMemberBinding).Path.Path;
                }
                return propertyName;
            }
            catch
            {
                return null;
            }
        }

        public static void SetPropertyName(DependencyObject obj, string value)
        {
            obj.SetValue(PropertyNameProperty, value);
        }

        private static void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked == null || headerClicked.Role == GridViewColumnHeaderRole.Padding ||
                headerClicked.Role == GridViewColumnHeaderRole.Floating) return;

            SetShowSortArrow(headerClicked, GetShowSortArrow(headerClicked.Column));

            var propertyName = GetPropertyName(headerClicked.Column);
            if (string.IsNullOrEmpty(propertyName)) return;
            var listView = GetAncestor<ListView>(headerClicked);
            if (listView == null) return;

            var grid = listView.View as GridView;
            
            var command = GetCommand(listView);
            if (command != null)
            {
                if (command.CanExecute(propertyName))
                {
                    command.Execute(propertyName);
                }
            }
            else if (GetAutoSort(listView))
            {
                var currentSort = ApplySort(listView.Items, propertyName);
                if (grid != null)
                {
                    foreach (
                        var h in
                            GetVisualChildren<GridViewColumnHeader>(listView)
                                .Where(h => h.Role != GridViewColumnHeaderRole.Padding && !Equals(h, headerClicked)))
                    {
                        SetSortDirection(h, ListSort.None);
                    }
                }
                
                SetSortDirection(headerClicked, currentSort);
            }
        }

        public static T GetAncestor<T>(DependencyObject reference) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(reference);
            while (!(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return (T)parent;
        }

        public static IEnumerable<T> GetVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var kid = child as T;
                if (kid != null)
                    yield return kid;

                foreach (var descendant in GetVisualChildren<T>(child))
                    yield return descendant;
            }
        }

        public static ListSort ApplySort(ICollectionView view, string propertyName)
        {
            var direction = ListSortDirection.Ascending;
            if (view.SortDescriptions.Count > 0)
            {
                var currentSort = view.SortDescriptions[0];
                if (currentSort.PropertyName == propertyName)
                {
                    direction = currentSort.Direction == ListSortDirection.Ascending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }
                view.SortDescriptions.Clear();
            }
            if (!string.IsNullOrEmpty(propertyName))
            {
                view.SortDescriptions.Add(new SortDescription(propertyName, direction));
            }
            return direction == ListSortDirection.Ascending ? ListSort.Ascending : ListSort.Descending;
        }
    }

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
            typeof(double), typeof(FixedWidthColumn), new FrameworkPropertyMetadata(double.NaN));

        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility",
            typeof(Visibility), typeof(FixedWidthColumn),
            new FrameworkPropertyMetadata(Visibility.Visible, OnVisibilityChanged));

        public static readonly DependencyProperty DisplayIndexProperty = DependencyProperty.Register("DisplayIndex",
            typeof(int), typeof(FixedWidthColumn), new FrameworkPropertyMetadata(0));

        public static readonly DependencyProperty OriginalIndexProperty = DependencyProperty.Register("OriginalIndex",
            typeof(int), typeof(FixedWidthColumn), new FrameworkPropertyMetadata(0));

        private static bool _savedAllowResize;

        static FixedWidthColumn()
        {
            WidthProperty.OverrideMetadata(typeof(FixedWidthColumn),
                new FrameworkPropertyMetadata(null, OnCoerceWidth));
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == "ActualWidth" && Math.Abs(ActualWidth) > double.Epsilon && AllowResize)
            {
                if (WidthChanged != null) WidthChanged(this, new GridColumnWidthChangedEventArgs(ActualWidth));
            }
        }

        public double DefaultWidth
        {
            get { return (double)GetValue(DefaultWidthProperty); }
            set { SetValue(DefaultWidthProperty, value); }
        }

        public int DisplayIndex
        {
            get { return (int)GetValue(DisplayIndexProperty); }
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
            if ((Visibility)e.NewValue == Visibility.Visible)
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
                    new GridColumnVisibilityChangedEventArgs((Visibility)e.OldValue, (Visibility)e.NewValue));
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

    public class GridViewRowPresenterWithGridLines : GridViewRowPresenter
    {
        private readonly List<FrameworkElement> _lines = new List<FrameworkElement>();

        private IEnumerable<FrameworkElement> Children
        {
            get { return LogicalTreeHelper.GetChildren(this).OfType<FrameworkElement>(); }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var size = base.ArrangeOverride(arrangeSize);
            var children = Children.ToList();
            var parent = FindParent<ListView>(this);
            var visibility = parent == null
                ? Visibility.Collapsed
                : (PNGridViewHelper.GetShowGridLines(parent) ? Visibility.Visible : Visibility.Hidden);
            EnsureLines(children.Count);
            for (var i = 0; i < _lines.Count; i++)
            {
                var child = children[i];
                var x = child.TransformToAncestor(this).Transform(new Point(0, 0)).X - child.Margin.Left;
                var rect = new Rect(x, -Margin.Top, 0.75, size.Height + Margin.Top + Margin.Bottom);
                var line = _lines[i];
                line.Measure(rect.Size);
                line.Arrange(rect);
                line.Visibility = visibility;
            }
            return size;
        }

        private void EnsureLines(int count)
        {
            count = count - _lines.Count;
            var style = (Style)TryFindResource("GridLineVert");
            for (var i = 0; i < count; i++)
            {
                FrameworkElement line = new Rectangle
                {
                    Style = style
                };
                AddVisualChild(line);
                _lines.Add(line);
            }
        }

        protected override int VisualChildrenCount
        {
            get { return base.VisualChildrenCount + _lines.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            var count = base.VisualChildrenCount;
            return index < count ? base.GetVisualChild(index) : _lines[index - count];
        }

        private static T FindParent<T>(object child) where T : DependencyObject
        {
            if (!(child is DependencyObject)) return null;

            var parent = VisualTreeHelper.GetParent((DependencyObject)child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
    }

    #region Events
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
    #endregion
}
