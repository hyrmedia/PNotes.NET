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
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndFontChooser.xaml
    /// </summary>
    public partial class WndFontChooser
    {
        private PNFont _SelectedFont = new PNFont();
        private Color _SelectedColor = Colors.Black;

        public WndFontChooser()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndFontChooser(PNFont font)
            : this()
        {
            _SelectedFont.FontFamily = font.FontFamily;
            _SelectedFont.FontSize = font.FontSize;
            _SelectedFont.FontStretch = font.FontStretch;
            _SelectedFont.FontStyle = font.FontStyle;
            _SelectedFont.FontWeight = font.FontWeight;
            grpColors.Visibility = Visibility.Collapsed;
        }

        internal WndFontChooser(PNFont font, Color color)
            : this()
        {
            _SelectedFont.FontFamily = font.FontFamily;
            _SelectedFont.FontSize = font.FontSize;
            _SelectedFont.FontStretch = font.FontStretch;
            _SelectedFont.FontStyle = font.FontStyle;
            _SelectedFont.FontWeight = font.FontWeight;
            _SelectedColor = color;
        }

        public PNFont SelectedFont
        {
            get { return _SelectedFont; }
            set { _SelectedFont = value; }
        }

        public Color SelectedColor
        {
            get { return _SelectedColor; }
            set { _SelectedColor = value; }
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _SelectedFont.FontFamily = lblFontSample.FontFamily;
                _SelectedFont.FontSize = lblFontSample.FontSize;
                _SelectedFont.FontStretch = lblFontSample.FontStretch;
                _SelectedFont.FontStyle = lblFontSample.FontStyle;
                _SelectedFont.FontWeight = lblFontSample.FontWeight;
                var solidColorBrush = lblFontSample.Foreground as SolidColorBrush;
                if (solidColorBrush != null)
                    _SelectedColor = solidColorBrush.Color;
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgFontChooser_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                lstFonts.SelectedItem = _SelectedFont.FontFamily;
                lstFonts.ScrollIntoView(lstFonts.SelectedItem);
                lstSizes.SelectedItem = _SelectedFont.FontSize;
                lstSizes.ScrollIntoView(lstSizes.SelectedItem);
                lstStyle.SelectedItem = _SelectedFont.FontStyle;
                lstStyle.ScrollIntoView(lstStyle.SelectedItem);
                lstWeight.SelectedItem = _SelectedFont.FontWeight;
                lstWeight.ScrollIntoView(lstWeight.SelectedItem);
                lstStretch.SelectedItem = _SelectedFont.FontStretch;
                lstStretch.ScrollIntoView(lstStretch.SelectedItem);
                foreach (var item in from object item in cboColors.Items
                                     let arr = item.ToString().Split(' ')
                                     where arr.Length == 2
                                     let convertFromString = ColorConverter.ConvertFromString(arr[1])
                                     where convertFromString != null
                                     let clr = (Color)convertFromString
                                     where clr == _SelectedColor
                                     select item)
                {
                    cboColors.SelectedItem = item;
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
