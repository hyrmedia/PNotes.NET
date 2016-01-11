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
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace PNotes.NET.styles
{
    public enum CustomWindowBorderStyle
    {
        Normal,
        MessageBox,
        NoBorder
    }

    internal static class LocalExtensions
    {
        public static void ForWindowFromChild(this object childDependencyObject, Action<Window> action)
        {
            var element = childDependencyObject as DependencyObject;
            while (element != null)
            {
                element = VisualTreeHelper.GetParent(element);
                if (element is Window) { action(element as Window); break; }
            }
        }

        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<Window> action)
        {
            var window = ((FrameworkElement)templateFrameworkElement).TemplatedParent as Window;
            if (window != null) action(window);
        }

        public static Window WindowFromTemplate(this object templateFrameworkElement)
        {
            return ((FrameworkElement)templateFrameworkElement).TemplatedParent as Window;
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            var helper = new WindowInteropHelper(window);
            return helper.Handle;
        }
    }

    public partial class CustomWindowStyle
    {
        public static readonly DependencyProperty WindowBorderBroperty =
            DependencyProperty.RegisterAttached("WindowBorder", typeof (CustomWindowBorderStyle),
                typeof (CustomWindowStyle), new UIPropertyMetadata(CustomWindowBorderStyle.Normal));

        public static CustomWindowBorderStyle GetWindowBorder(DependencyObject obj)
        {
            return (CustomWindowBorderStyle)obj.GetValue(WindowBorderBroperty);
        }

        public static void SetWindowBorder(DependencyObject obj, CustomWindowBorderStyle value)
        {
            obj.SetValue(WindowBorderBroperty, value);
        }

        #region sizing event handlers

        void OnSizeSouth(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.South); }
        void OnSizeNorth(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.North); }
        void OnSizeEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.East); }
        void OnSizeWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.West); }
        void OnSizeNorthWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.NorthWest); }
        void OnSizeNorthEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.NorthEast); }
        void OnSizeSouthEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.SouthEast); }
        void OnSizeSouthWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.SouthWest); }

        void OnSize(object sender, SizingAction action)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                sender.ForWindowFromTemplate(w =>
                {
                    if (w.WindowState == WindowState.Normal)
                        DragSize(w.GetWindowHandle(), action);
                });
            }
        }

        void IconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                sender.ForWindowFromTemplate(w => w.Close());
            }
            else
            {
                sender.ForWindowFromTemplate(w =>
                    SendMessage(w.GetWindowHandle(), WM_SYSCOMMAND, (IntPtr)SC_KEYMENU, (IntPtr)' '));
            }
        }

        void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => w.Close());
        }

        void MinButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => w.WindowState = WindowState.Minimized);
        }

        void MaxButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(
                w =>
                {
                    w.WindowState = (w.WindowState == WindowState.Maximized)
                        ? WindowState.Normal
                        : WindowState.Maximized;
                });

        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            sender.ForWindowFromTemplate(
                w =>
                {
                    if (w.WindowState == WindowState.Maximized)
                    {
                        w.MaxHeight = SystemParameters.WorkArea.Height + 14;
                        w.MaxWidth = SystemParameters.WorkArea.Width + 14;
                        //var thickness = (Thickness) w.TryFindResource("WindowBorderThickness");
                        //if (thickness.Left > 0)
                        //{
                        //    w.MaxHeight = SystemParameters.WorkArea.Height + 14 ;
                        //    w.MaxWidth = SystemParameters.WorkArea.Width + 14;
                        //}
                        //else
                        //{
                        //    w.MaxHeight = SystemParameters.WorkArea.Height + 14;
                        //    w.MaxWidth = SystemParameters.WorkArea.Width + 14;
                        //}
                    }
                    //else
                    //{
                    //    w.Margin = new Thickness(7);
                    //}
                });
        }

        void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var window = sender.WindowFromTemplate();
            if (window == null) return;
            if (e.ClickCount > 1 && (window.ResizeMode == ResizeMode.CanResize ||
                window.ResizeMode == ResizeMode.CanResizeWithGrip))
            {
                MaxButtonClick(sender, e);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                sender.ForWindowFromTemplate(w => w.DragMove());
            }
        }

        void TitleBarMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                sender.ForWindowFromTemplate(w =>
                {
                    if (w.WindowState != WindowState.Maximized) return;
                    w.BeginInit();
                    const double adjustment = 40.0;
                    var mouse1 = e.MouseDevice.GetPosition(w);
                    var width1 = Math.Max(w.ActualWidth - 2 * adjustment, adjustment);
                    w.WindowState = WindowState.Normal;
                    var width2 = Math.Max(w.ActualWidth - 2 * adjustment, adjustment);
                    w.Left = (mouse1.X - adjustment) * (1 - width2 / width1);
                    w.Top = -7;
                    w.EndInit();
                    w.DragMove();
                });
            }
        }

        #endregion

        #region P/Invoke

        const int WM_SYSCOMMAND = 0x112;
        const int SC_SIZE = 0xF000;
        const int SC_KEYMENU = 0xF100;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        void DragSize(IntPtr handle, SizingAction sizingAction)
        {
            SendMessage(handle, WM_SYSCOMMAND, (IntPtr)(SC_SIZE + sizingAction), IntPtr.Zero);
            SendMessage(handle, 514, IntPtr.Zero, IntPtr.Zero);
        }

        public enum SizingAction
        {
            North = 3,
            South = 6,
            East = 2,
            West = 1,
            NorthEast = 5,
            NorthWest = 4,
            SouthEast = 8,
            SouthWest = 7
        }

        #endregion
    }
}
