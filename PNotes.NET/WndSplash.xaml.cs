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
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using Application = System.Windows.Forms.Application;
using Timer = System.Timers.Timer;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSplash.xaml
    /// </summary>
    public partial class WndSplash
    {
        public WndSplash()
        {
            InitializeComponent();
            DataContext = PNStatic.SpTextProvider;
        }

        private readonly Timer _Timer = new Timer {Interval = 100};
        private bool _StartTimer = true;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Width = imgSplash.ActualWidth;
                Height = imgSplash.ActualHeight;
                _Timer.Elapsed += _Timer_Elapsed;
                _Timer.Start();
                var fontSegoe = PNStatic.PrivateFonts.Families.FirstOrDefault(f => f.Name.ToUpper().StartsWith("SEGOE SCRIPT"));
                var family = fontSegoe != null ? fontSegoe.Name : "Segoe Script";
                FontFamily = new FontFamily(family);
                FontSize = 16;

                tbVersion.Text = Application.ProductVersion;
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
            }
        }

        private delegate void TimerDelegate(object sender, ElapsedEventArgs e);
        void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _Timer.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    TimerDelegate d = _Timer_Elapsed;
                    Dispatcher.Invoke(d, sender, e);
                }
                else
                {
                    if (PNStatic.HideSplash)
                    {
                        _StartTimer = false;
                        Close();
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
            }
            finally
            {
                if (_StartTimer)
                    _Timer.Start();
            }
        }
    }
}
