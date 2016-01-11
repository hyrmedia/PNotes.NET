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

using System.Drawing;
using System.Windows;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndMessageBox.xaml
    /// </summary>
    public partial class WndMessageBox
    {
        public WndMessageBox()
        {
            Result = MessageBoxResult.None;
            InitializeComponent();
        }

        public WndMessageBox(string text = "", string title = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, Window owner = null)
            : this()
        {
            Title = title;
            txbText.Text = text;
            
            if (button != MessageBoxButton.OK && button != MessageBoxButton.OKCancel)
                cmdOK.Visibility = Visibility.Collapsed;
            if (button != MessageBoxButton.YesNo && button != MessageBoxButton.YesNoCancel)
            {
                cmdYes.Visibility = Visibility.Collapsed;
                cmdNo.Visibility = Visibility.Collapsed;
                if (button == MessageBoxButton.OK)
                {
                    cmdOK.IsDefault = true;
                    cmdOK.IsCancel = true;
                }
            }
            if (button != MessageBoxButton.OKCancel && button != MessageBoxButton.YesNoCancel)
                cmdCancel.Visibility = Visibility.Collapsed;
            switch (image)
            {
                case MessageBoxImage.Information:
                    imgIcon.Source = SystemIcons.Information.ToImageSource();
                    break;
                case MessageBoxImage.Question:
                    imgIcon.Source = SystemIcons.Question.ToImageSource();
                    break;
                case MessageBoxImage.Exclamation:
                    imgIcon.Source = SystemIcons.Exclamation.ToImageSource();
                    break;
                case MessageBoxImage.Error:
                    imgIcon.Source = SystemIcons.Error.ToImageSource();
                    break;
            }
            if (owner != null)
                Owner = owner;
        }

        internal MessageBoxResult Result { get; private set; }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void cmdYes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void cmdNo_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }
    }
}
