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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for AboutControl.xaml
    /// </summary>
    public partial class AboutControl
    {
        public event EventHandler<AboutLinkClickedEventArgs> AboutLinkClicked;

        public AboutControl()
        {
            InitializeComponent();
        }

        private readonly Storyboard _Storyboard = new Storyboard();

        public double Duration { get; set; }

        public void StartAnimation()
        {
            canvas.UpdateLayout();
            var da = new DoubleAnimation(canvas.ActualHeight, -panel.ActualHeight - 20, new Duration(TimeSpan.FromSeconds(Duration)))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(da, panel);
            Storyboard.SetTargetProperty(da, new PropertyPath(Canvas.TopProperty));
            _Storyboard.Children.Add(da);
            _Storyboard.Begin(this, true);
        }

        public void AddTextBlock(string text, Thickness margin = default(Thickness))
        {
            panel.Children.Add(new TextBlock
            {
                Text = text,
                Margin = margin,
                TextWrapping = TextWrapping.Wrap
            });
        }

        public void AddUrl(string url)
        {
            var tb = new TextBlock
            {
                Text = url,
                TextWrapping = TextWrapping.Wrap,
                Cursor = Cursors.Hand,
                Margin = new Thickness(8, 0, 0, 16),
                TextDecorations = TextDecorations.Underline,
                Foreground = Brushes.Red
            };
            tb.MouseLeftButtonDown += Link_MouseLeftButtonDown;
            panel.Children.Add(tb);
        }

        private void panel_MouseEnter(object sender, MouseEventArgs e)
        {
            _Storyboard.Pause(this);
        }

        private void panel_MouseLeave(object sender, MouseEventArgs e)
        {
            _Storyboard.Resume(this);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetTop(panel, canvas.ActualHeight);
        }

        private void Link_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBlock;
            if (tb == null) return;
            if (AboutLinkClicked == null) return;
            AboutLinkClicked(this, new AboutLinkClickedEventArgs(tb.Text.Trim()));
        }
    }

    public class AboutLinkClickedEventArgs : EventArgs
    {
        public string Link { get; private set; }

        public AboutLinkClickedEventArgs(string link)
        {
            Link = link;
        }
    }
}
