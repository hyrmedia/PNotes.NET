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
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PNotes.NET
{

    public class ThumbnailButton : Button
    {
        public static readonly DependencyProperty ThumbnailBrushProperty = DependencyProperty.Register(
            "ThumbnailBrush", typeof (Brush), typeof (ThumbnailButton), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ThumbnailNameProperty = DependencyProperty.Register("ThumbnailName",
            typeof (string), typeof (ThumbnailButton), new FrameworkPropertyMetadata(""));
        static ThumbnailButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ThumbnailButton), new FrameworkPropertyMetadata(typeof(ThumbnailButton)));
        }

        public string ThumbnailName
        {
            get { return (string) GetValue(ThumbnailNameProperty); }
            set { SetValue(ThumbnailNameProperty, value); }
        }

        public Brush ThumbnailBrush
        {
            get { return (Brush) GetValue(ThumbnailBrushProperty); }
            set { SetValue(ThumbnailBrushProperty, value); }
        }

        public string Id { get; set; }

        public ThumbnailButton Prev { get; set; }

        public ThumbnailButton Next { get; set; }

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            if (!PNStatic.Settings.Behavior.PanelSwitchOffAnimation)
            {
                var sb = TryFindResource("ThumbnailEnter") as Storyboard;
                if (sb != null)
                {
                    BeginStoryboard(sb);
                }
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            if (!PNStatic.Settings.Behavior.PanelSwitchOffAnimation)
            {
                var sb = TryFindResource("ThumbnailLeave") as Storyboard;
                if (sb != null)
                {
                    BeginStoryboard(sb);
                }
            }
            base.OnMouseLeave(e);
        }
    }
}
