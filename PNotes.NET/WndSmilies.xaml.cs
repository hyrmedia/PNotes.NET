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
using System.Windows.Media.Imaging;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSmilies.xaml
    /// </summary>
    public partial class WndSmilies
    {
        internal event EventHandler<SmilieSelectedEventArgs> SmilieSelected;

        public WndSmilies()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private void DlgSmilies_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                loadSmilies();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadSmilies()
        {
            try
            {
                for (var i = 1; i <= 121; i++)
                {
                    var image = new Image
                    {
                        Source = new BitmapImage(new Uri(PNStrings.SMILIES_PREFIX + i + ".png")),
                        Cursor = Cursors.Hand,
                        Margin = new Thickness(2),
                        Stretch = Stretch.None
                    };
                    image.MouseLeftButtonDown += image_MouseLeftButtonDown;
                    grdSmilies.Children.Add(image);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var btn = sender as Image;
                if (SmilieSelected != null)
                {
                    if (btn != null)
                        SmilieSelected(this, new SmilieSelectedEventArgs(PNStatic.ImageToDrawingImage(btn.Source as BitmapImage)));
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgSmilies_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
