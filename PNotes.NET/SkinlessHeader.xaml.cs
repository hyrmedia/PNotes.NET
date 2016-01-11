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

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for Header.xaml
    /// </summary>
    public partial class SkinlessHeader : IHeader
    {
        internal event EventHandler<HeaderButtonClickedEventArgs> HeaderButtonClicked;

        public SkinlessHeader()
        {
            InitializeComponent();
        }

        public string Title { get; set; }

        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (HeaderButtonClicked != null)
                HeaderButtonClicked(this, new HeaderButtonClickedEventArgs(HeaderButtonType.Hide));
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (HeaderButtonClicked != null)
                HeaderButtonClicked(this, new HeaderButtonClickedEventArgs(HeaderButtonType.Delete));
        }

        public void SetButtonVisibility(HeaderButtonType type, Visibility value)
        {
            switch (type)
            {
                case HeaderButtonType.Delete:
                    DeleteButton.Visibility = value;
                    break;
                case HeaderButtonType.Hide:
                    HideButton.Visibility = value;
                    break;
                case HeaderButtonType.Panel:
                    PanelButton.Visibility = value;
                    break;
            }
        }

        public void SetButtonTooltip(HeaderButtonType type, string toolTip)
        {
            switch (type)
            {
                case HeaderButtonType.Delete:
                    DeleteButton.ToolTip = toolTip;
                    break;
                case HeaderButtonType.Hide:
                    HideButton.ToolTip = toolTip;
                    break;
                case HeaderButtonType.Panel:
                    PanelButton.ToolTip = toolTip;
                    break;
            }
        }

        public void SetAlternative(bool value)
        {
            HideButton.IsAlternated = value;
        }

        public void SetPNFont(PNFont font)
        {
            this.SetFont(font);
        }

        private void PanelButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (HeaderButtonClicked != null)
                HeaderButtonClicked(this, new HeaderButtonClickedEventArgs(HeaderButtonType.Panel));
        }
    }

    internal class HeaderButtonClickedEventArgs : EventArgs
    {
        internal HeaderButtonType ButtonType { get; private set; }

        internal HeaderButtonClickedEventArgs(HeaderButtonType type)
        {
            ButtonType = type;
        }
    }
}
