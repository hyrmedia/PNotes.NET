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

using PNRichEdit;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;

namespace PNotes.NET
{
    class EditControl
    {
        internal event EventHandler<EditControlSizeChangedEventArgs> EditControlSizeChanged;

        private readonly Window _WpfWindow;
        private readonly FrameworkElement _PlacementTarget;
        private readonly EditContainer _WinForm; // the top-level window holding the PNRichEditBox control

        private readonly PNRichEditBox _EditBox = new PNRichEditBox
        {
            ScrollBars = RichTextBoxScrollBars.None,
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            EnableAutoDragDrop = true
        };

        /// <summary>
        /// Returns PNRichEditBox control
        /// </summary>
        public PNRichEditBox EditBox
        {
            get { return _EditBox; }
        }
        /// <summary>
        /// Returns the WinForm holding the PNRichEditBox control
        /// </summary>
        public Form WinForm
        {
            get { return _WinForm; }
        }
        /// <summary>
        /// Creates new instance of EditControl
        /// </summary>
        /// <param name="placementTarget">FrameworkElement on WPF Window holding the EditControl</param>
        public EditControl(FrameworkElement placementTarget)
        {
            //set placement target
            _PlacementTarget = placementTarget;
            //get WPF window
            var owner = Window.GetWindow(placementTarget);

            if (owner == null) return;
            //store WPF window
            _WpfWindow = owner;
            //create new WinForm
            _WinForm = new EditContainer
            {
                Opacity = owner.Opacity,
                StartPosition = FormStartPosition.CenterScreen,
                ShowInTaskbar = false,
                FormBorderStyle = FormBorderStyle.None,
                AllowDrop = true
            };
            //get WPF window background
            var brush = owner.Background as SolidColorBrush;
            //set WinForm backcolor
            if (brush != null)
            {
                _WinForm.BackColor = Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
            }
            //add PNRichEditBox to WinForm controls
            _WinForm.Controls.Add(_EditBox);

            //set event handlers for size and location changes
            owner.LocationChanged += delegate { OnSizeLocationChanged(); };
            _PlacementTarget.SizeChanged += delegate { OnSizeLocationChanged(); };

            if (owner.IsVisible)
                //if WPF window is visible - show WinForm
                InitialShow();
            else
            {
                //otherwise set handlers for SourceInitialized (in order to show WinForm) and Loaded events
                owner.SourceInitialized += delegate { InitialShow(); };
                owner.Loaded += delegate { _WinForm.Opacity = owner.Opacity; };
            }
            //add handler for WPF window opacity changes
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(UIElement.OpacityProperty, typeof(Window));
            dpd.AddValueChanged(owner, delegate { _WinForm.Opacity = _WpfWindow.Opacity; });
            //add handler for WinForm FormClosing event
            _WinForm.FormClosing += delegate { _WpfWindow.Close(); };
        }

        void InitialShow()
        {
            //create new NativeWindow
            var nativeWindow = new NativeWindow();
            //extract HwndSource from WPF Window
            var hwndSource = (HwndSource)PresentationSource.FromVisual(_WpfWindow);
            if (hwndSource != null)
            {
                //assign handle to NativeWindow
                nativeWindow.AssignHandle(hwndSource.Handle);
            }
            _WinForm.Opacity = 0;
            //show WinForm settin newly created NativeWindow as owner
            _WinForm.Show(nativeWindow);
            //release NativeWindow handle
            nativeWindow.ReleaseHandle();
        }

        DispatcherOperation _repositionCallback;

        void OnSizeLocationChanged()
        {
            //begin invoke reposition
            if (_repositionCallback == null)
                _repositionCallback = _WpfWindow.Dispatcher.BeginInvoke(Reposition);
        }

        void Reposition()
        {
            //release DispatcherOperation
            _repositionCallback = null;
            //get WPF Window location
            var translatePoint = _PlacementTarget.TranslatePoint(new Point(), _WpfWindow);
            //get size of placement target
            var translateSize = new Point(_PlacementTarget.ActualWidth, _PlacementTarget.ActualHeight);
            //get HwndSource of WPF Window
            var hwndSource = (HwndSource)PresentationSource.FromVisual(_WpfWindow);
            if (hwndSource == null) return;
            //get the visual manager of target window
            CompositionTarget ct = hwndSource.CompositionTarget;
            if (ct == null) return;
            //transform location and size
            translatePoint = ct.TransformToDevice.Transform(translatePoint);
            translateSize = ct.TransformToDevice.Transform(translateSize);

            var screenLocation = new PNInterop.POINTINT(translatePoint);
            PNInterop.ClientToScreen(hwndSource.Handle, ref screenLocation);
            var screenSize = new PNInterop.POINTINT(translateSize);
            //move WinForm
            PNInterop.MoveWindow(_WinForm.Handle, screenLocation.X, screenLocation.Y, screenSize.X, screenSize.Y, true);
            if (EditControlSizeChanged != null)
            {
                //raise EditControlSizeChanged event providing new PNRichEditBox size
                EditControlSizeChanged(this,
                    new EditControlSizeChangedEventArgs(new Rectangle(new System.Drawing.Point(0, 0),
                        _EditBox.Size)));
            }
        }
    }
}
