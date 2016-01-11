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
using System.Windows.Media;

namespace PNotes.NET
{
    internal class HSLColor
    {
        private double _Hue, _Lightness, _Saturation;

        internal HSLColor(double h, double s, double l)
        {
            _Hue = h;
            _Saturation = s;
            _Lightness = l;
        }

        internal double Hue
        {
            get { return _Hue; }
            set { _Hue = value; }
        }

        internal double Lightness
        {
            get { return _Lightness; }
            set { _Lightness = value; }
        }

        internal double Saturation
        {
            get { return _Saturation; }
            set { _Saturation = value; }
        }

        internal RGBColor RGBColor()
        {
            double r, g, b, m2;
            if (_Lightness <= 0.5)
                m2 = _Lightness * (1 + _Saturation);
            else
                m2 = _Lightness + _Saturation - _Lightness * _Saturation;

            double m1 = 2 * _Lightness - m2;
            if (Math.Abs(_Saturation - 0) < double.Epsilon)
            {
                r = _Lightness;
                g = _Lightness;
                b = _Lightness;
            }
            else
            {
                r = RGB(_Hue + 120, m1, m2);
                g = RGB(_Hue, m1, m2);
                b = RGB(_Hue - 120, m1, m2);
            }
            return new RGBColor(r * 255, g * 255, b * 255);
        }

        private double RGB(double h, double m1, double m2)
        {
            double value;

            if (h < 0)
                h += 360;
            else if (h > 360)
                h -= 360;
            if (h < 60)
                value = m1 + (m2 - m1) * h / 60.0;
            else if (60 <= h && h < 180)
                value = m2;
            else if (180 <= h && h < 240)
                value = m1 + (m2 - m1) * (240 - h) / 60.0;
            else //240 <= H && H <= 360
                value = m1;

            return value;
        }
    }

    internal class RGBColor
    {
        private double _r, _g, _b;

        internal RGBColor(double r, double g, double b)
        {
            _r = (r < 0) ? 0 : (r > 255) ? 255 : r;
            _g = (g < 0) ? 0 : (g > 255) ? 255 : g;
            _b = (b < 0) ? 0 : (b > 255) ? 255 : b;
        }

        internal RGBColor(Color c)
        {
            _r = c.R;
            _g = c.G;
            _b = c.B;
        }

        internal double R
        {
            get { return _r; }
            set { _r = (value < 0) ? 0 : (value > 255) ? 255 : value; }
        }

        internal double G
        {
            get { return _g; }
            set { _g = (value < 0) ? 0 : (value > 255) ? 255 : value; }
        }

        internal double B
        {
            get { return _b; }
            set { _b = (value < 0) ? 0 : (value > 255) ? 255 : value; }
        }

        internal Color Color
        {
            get
            {
                return Color.FromArgb(255, (byte)Math.Round(_r, MidpointRounding.AwayFromZero),
                    (byte)Math.Round(_g, MidpointRounding.AwayFromZero),
                    (byte)Math.Round(_b, MidpointRounding.AwayFromZero));
            }
        }
    }
}
