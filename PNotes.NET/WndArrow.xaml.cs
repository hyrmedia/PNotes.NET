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
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndArrow.xaml
    /// </summary>
    public partial class WndArrow
    {
        public WndArrow()
        {
            InitializeComponent();
        }

        internal WndArrow(DockArrow dockArrow) : this()
        {
            DockDirection = dockArrow;
        }

        public DockArrow DockDirection { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var wa = PNStatic.AllScreensBounds();
                switch (DockDirection)
                {
                    case DockArrow.LeftUp:
                    case DockArrow.TopLeft:
                        this.SetLocation(new Point(wa.Left, wa.Top));
                        break;
                    case DockArrow.LeftDown:
                    case DockArrow.BottomLeft:
                        this.SetLocation( new Point(wa.Left, wa.Bottom - ActualHeight));
                        break;
                    case DockArrow.RightUp:
                    case DockArrow.TopRight:
                        this.SetLocation( new Point(wa.Right - ActualWidth, wa.Top));
                        break;
                    case DockArrow.RightDown:
                    case DockArrow.BottomRight:
                        this.SetLocation(new Point(wa.Right - ActualWidth, wa.Bottom - ActualHeight));
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                switch (DockDirection)
                {
                    case DockArrow.LeftUp:
                    case DockArrow.RightUp:
                        PNNotesOperations.ShiftDockDown(DockDirection);
                        break;
                    case DockArrow.LeftDown:
                    case DockArrow.RightDown:
                        PNNotesOperations.ShiftDockUp(DockDirection);
                        break;
                    case DockArrow.TopLeft:
                    case DockArrow.BottomLeft:
                        PNNotesOperations.ShiftDockRight(DockDirection);
                        break;
                    case DockArrow.TopRight:
                    case DockArrow.BottomRight:
                        PNNotesOperations.ShiftDockLeft(DockDirection);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
