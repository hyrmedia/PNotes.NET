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

using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for Baloon.xaml
    /// </summary>
    public partial class Baloon
    {
        internal event EventHandler<BaloonClickedEventArgs> BaloonLinkClicked;

        public Baloon()
        {
            InitializeComponent();
            TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
        }

        internal Baloon(BaloonMode mode)
        {
            Mode = mode;
            InitializeComponent();
            TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
        }

        private bool _IsClosing;

        public static readonly DependencyProperty BalloonTextProperty =
            DependencyProperty.Register("BaloonText",
                typeof(string),
                typeof(Baloon),
                new FrameworkPropertyMetadata(""));
        public static readonly DependencyProperty BalloonLinkProperty =
            DependencyProperty.Register("BaloonLink",
                typeof(string),
                typeof(Baloon),
                new FrameworkPropertyMetadata(""));

        public string BaloonLink
        {
            get { return (string)GetValue(BalloonLinkProperty); }
            set { SetValue(BalloonLinkProperty, value); }
        }

        public string BaloonText
        {
            get { return (string)GetValue(BalloonTextProperty); }
            set { SetValue(BalloonTextProperty, value); }
        }

        public BaloonMode Mode { get; set; }

        private void OnBalloonClosing(object sender, RoutedEventArgs e)
        {
            e.Handled = true; //suppresses the popup from being closed immediately
            _IsClosing = true;
        }

        private void BaloonGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            //if we're already running the fade-out animation, do not interrupt anymore
            //(makes things too complicated for the sample)
            if (_IsClosing) return;

            //the tray icon assigned this attached property to simplify access
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.ResetBalloonCloseTimer();
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            var pp = Parent as Popup;
            if (pp != null) pp.IsOpen = false;
        }

        private void BaloonLinkRun_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //the tray icon assigned this attached property to simplify access
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.CloseBalloon();
            if (BaloonLinkClicked != null)
            {
                BaloonLinkClicked(this, new BaloonClickedEventArgs(Mode));
            }
        }

        //private void CancelImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    //the tray icon assigned this attached property to simplify access
        //    TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
        //    taskbarIcon.CloseBalloon();
        //}

        private void PnBaloon_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var newHeight = cmdCancel.ActualHeight + 20 + 26 + 20 + BaloonTextBlock.ActualHeight;
            if (Math.Abs(e.NewSize.Height - newHeight) > double.Epsilon)
            {
                Height = newHeight;
            }
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            //the tray icon assigned this attached property to simplify access
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.CloseBalloon();
        }
    }
}
