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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Windows.Point;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndCanvas.xaml
    /// </summary>
    public partial class WndCanvas
    {
        internal event EventHandler<CanvasSavedEventArgs> CanvasSaved;

        public WndCanvas()
        {
            InitializeComponent();
        }

        public WndCanvas(Window parent, Color canvasColor)
            : this()
        {
            InitializeComponent();
            _Parent = parent;
            _Color = canvasColor;
        }

        private readonly Window _Parent;
        private readonly Color _Color;
        private double _PenSize = 2;
        private bool _Loaded;

        private void cmdLine_Click(object sender, RoutedEventArgs e)
        {
            ctmLines.IsOpen = true;
        }

        private void palette_SelectedBrushChanged(object sender, RoutedPropertyChangedEventArgs<SolidColorBrush> e)
        {
            inkCanvas.DefaultDrawingAttributes.Color = e.NewValue.Color;
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            saveCanvas();
        }

        private void mnuLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var menu = sender as MenuItem;
                if (menu == null || menu.IsChecked) return;
                foreach (var b in ctmLines.Items.OfType<MenuItem>().Where(b => b.Name != menu.Name))
                {
                    b.IsChecked = false;
                }
                menu.IsChecked = true;
                _PenSize = double.Parse(menu.Name.Substring(menu.Name.Length - 1));
                switch ((int)_PenSize)
                {
                    case 1:
                        imgLine.Source = TryFindResource("line1") as BitmapImage;//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "line1.png"));
                        break;
                    case 2:
                        imgLine.Source = TryFindResource("line2") as BitmapImage;//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "line2.png"));
                        break;
                    case 4:
                        imgLine.Source = TryFindResource("line4") as BitmapImage;//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "line4.png"));
                        break;
                    case 6:
                        imgLine.Source = TryFindResource("line6") as BitmapImage;//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "line6.png"));
                        break;
                    case 8:
                        imgLine.Source = TryFindResource("line8") as BitmapImage;//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "line8.png"));
                        break;
                }
                inkCanvas.DefaultDrawingAttributes.Width = inkCanvas.DefaultDrawingAttributes.Height = _PenSize;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void inkCanvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Escape)
                {
                    DialogResult = false;
                }
                else if (e.Key == Key.Enter && cmdOK.IsEnabled)
                {
                    cmdOK.PerformClick();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void inkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            try
            {
                cmdOK.IsEnabled = inkCanvas.Strokes.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void inkCanvas_StrokeErased(object sender, RoutedEventArgs e)
        {
            try
            {
                cmdOK.IsEnabled = inkCanvas.Strokes.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var loc = _Parent.GetLocation();
                var scr = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int) loc.X, (int) loc.Y));
                if (scr.WorkingArea.Left < _Parent.Left - ActualWidth)
                    Left = _Parent.Left - ActualWidth;
                else
                    Left = _Parent.Left + _Parent.ActualWidth;
                var anim = new DoubleAnimation(-100, _Parent.Top, new Duration(TimeSpan.FromMilliseconds(300)))
                {
                    AccelerationRatio = 0.1
                };
                BeginAnimation(TopProperty, anim);

                inkCanvas.Background =
                    new SolidColorBrush(_Color);
                inkCanvas.DefaultDrawingAttributes = new DrawingAttributes
                {
                    Color = Colors.Black,
                    Height = _PenSize,
                    Width = _PenSize
                };
                _Loaded = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void saveCanvas()
        {
            try
            {
                var bounds = VisualTreeHelper.GetDescendantBounds(inkCanvas);
                const double dpi = 96d;

                var rtb = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, PixelFormats.Default);
                var dv = new DrawingVisual();
                using (var dc = dv.RenderOpen())
                {
                    var vb = new VisualBrush(inkCanvas);
                    dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
                }

                rtb.Render(dv);

                //var width = (int)inkCanvas.ActualWidth;
                //var height = (int)inkCanvas.ActualHeight;
                ////render ink to bitmap
                //var rtb =
                //    new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Pbgra32);
                //rtb.Render(inkCanvas);
                //save the ink to a memory stream
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));

                using (var ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    //get the bitmap bytes from the memory stream
                    ms.Position = 0;
                    using (var bitmap = System.Drawing.Image.FromStream(ms))
                    {
                        using (var transBitma = makeTransparentBitmap(bitmap, System.Drawing.Color.FromArgb(_Color.A,_Color.R,_Color.G,_Color.B)))
                        {
                            var trimmedBitmap = trimBitmap(transBitma);
                            if (CanvasSaved == null) return;
                            CanvasSaved(this, new CanvasSavedEventArgs(trimmedBitmap));
                            DialogResult = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static System.Drawing.Bitmap makeTransparentBitmap(System.Drawing.Image image, System.Drawing.Color mask)
        {
            try
            {
                var bmp = new System.Drawing.Bitmap(image.Width, image.Height);
                using (var g = System.Drawing.Graphics.FromImage(bmp))
                {
                    var imgAttribute = new ImageAttributes();
                    imgAttribute.SetColorKey(mask, mask);
                    g.DrawImage(image, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width,
                        image.Height,
                        System.Drawing.GraphicsUnit.Pixel, imgAttribute);
                }
                return bmp;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private static System.Drawing.Bitmap trimBitmap(System.Drawing.Bitmap source)
        {
            try
            {
                System.Drawing.Rectangle srcRect;
                BitmapData data = null;
                try
                {
                    data = source.LockBits(new System.Drawing.Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);
                    var buffer = new byte[data.Height * data.Stride];
                    Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);

                    int xMin = int.MaxValue,
                        xMax = int.MinValue,
                        yMin = int.MaxValue,
                        yMax = int.MinValue;

                    var foundPixel = false;

                    // Find xMin
                    for (var x = 0; x < data.Width; x++)
                    {
                        var stop = false;
                        for (var y = 0; y < data.Height; y++)
                        {
                            var alpha = buffer[y * data.Stride + 4 * x + 3];
                            if (alpha == 0) continue;
                            xMin = x;
                            stop = true;
                            foundPixel = true;
                            break;
                        }
                        if (stop)
                            break;
                    }

                    // Image is empty...
                    if (!foundPixel)
                        return null;

                    // Find yMin
                    for (var y = 0; y < data.Height; y++)
                    {
                        var stop = false;
                        for (var x = xMin; x < data.Width; x++)
                        {
                            var alpha = buffer[y * data.Stride + 4 * x + 3];
                            if (alpha == 0) continue;
                            yMin = y;
                            stop = true;
                            break;
                        }
                        if (stop)
                            break;
                    }

                    // Find xMax
                    for (var x = data.Width - 1; x >= xMin; x--)
                    {
                        var stop = false;
                        for (var y = yMin; y < data.Height; y++)
                        {
                            var alpha = buffer[y * data.Stride + 4 * x + 3];
                            if (alpha == 0) continue;
                            xMax = x;
                            stop = true;
                            break;
                        }
                        if (stop)
                            break;
                    }

                    // Find yMax
                    for (var y = data.Height - 1; y >= yMin; y--)
                    {
                        var stop = false;
                        for (var x = xMin; x <= xMax; x++)
                        {
                            var alpha = buffer[y * data.Stride + 4 * x + 3];
                            if (alpha == 0) continue;
                            yMax = y;
                            stop = true;
                            break;
                        }
                        if (stop)
                            break;
                    }

                    srcRect = System.Drawing.Rectangle.FromLTRB(xMin, yMin, xMax, yMax);
                }
                finally
                {
                    if (data != null)
                        source.UnlockBits(data);
                }

                var dest = new System.Drawing.Bitmap(srcRect.Width, srcRect.Height);
                var destRect = new System.Drawing.Rectangle(0, 0, srcRect.Width, srcRect.Height);
                using (var graphics = System.Drawing.Graphics.FromImage(dest))
                {
                    graphics.DrawImage(source, destRect, srcRect, System.Drawing.GraphicsUnit.Pixel);
                }
                return dest;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void cmdPen_Checked(object sender, RoutedEventArgs e)
        {
            if (!_Loaded) return;
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
        }

        private void cmdEraser_Checked(object sender, RoutedEventArgs e)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
            inkCanvas.EraserShape = new RectangleStylusShape(16, 16);
        }
    }
}
