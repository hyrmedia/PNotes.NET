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

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for SkinnableHeader.xaml
    /// </summary>
    public partial class SkinnableHeader : IHeader
    {
        internal event EventHandler<HeaderButtonClickedEventArgs> HideDeleteButtonClicked;

        public SkinnableHeader()
        {
            InitializeComponent();
        }

        private ImageSource _HideImageSource;

        private void HideImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void DeleteImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        public double InitialLeft { private get; set; }

        public void SetButtonVisibility(HeaderButtonType type, Visibility value)
        {
            switch (type)
            {
                case HeaderButtonType.Delete:
                    if (DeleteImage.Visibility == value) return;
                    DeleteImage.Visibility = value;
                    break;
                case HeaderButtonType.Hide:
                    if (HideImage.Visibility == value) return;
                    HideImage.Visibility = value;
                    break;
            }
            if (HideImage.Visibility == Visibility.Visible &&
                DeleteImage.Visibility == Visibility.Visible)
                Canvas.SetLeft(this, InitialLeft);
            else if (HideImage.Visibility == Visibility.Visible)
                Canvas.SetLeft(this, InitialLeft + HideImage.ActualWidth);
            else if (DeleteImage.Visibility == Visibility.Visible)
                Canvas.SetLeft(this, InitialLeft + DeleteImage.ActualWidth);
        }

        public void SetButtonTooltip(HeaderButtonType type, string toolTip)
        {
            switch (type)
            {
                case HeaderButtonType.Delete:
                    DeleteImage.ToolTip = toolTip;
                    break;
                case HeaderButtonType.Hide:
                    HideImage.ToolTip = toolTip;
                    break;
            }
        }

        public void SetAlternative(bool value)
        {
            if (value)
            {
                _HideImageSource = HideImage.Source;
                HideImage.Source = DeleteImage.Source;
            }
            else
            {
                HideImage.Source = _HideImageSource;
            }
        }

        public string Title { get; set; }

        public void SetPNFont(PNFont font)
        {
            //do nothing
        }

        private void HideImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (HideDeleteButtonClicked != null)
                HideDeleteButtonClicked(this, new HeaderButtonClickedEventArgs(HeaderButtonType.Hide));
            e.Handled = true;
        }

        private void DeleteImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (HideDeleteButtonClicked != null)
                HideDeleteButtonClicked(this, new HeaderButtonClickedEventArgs(HeaderButtonType.Delete));
            e.Handled = true;
        }
    }
}
