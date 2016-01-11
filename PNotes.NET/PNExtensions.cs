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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace PNotes.NET
{
    internal static class PNExtensions
    {
        //public static Brush SetBrightness(this Brush original, double brightness)
        //{
        //    if (brightness < 0 || brightness > 1)
        //        throw new ArgumentOutOfRangeException("brightness",
        //        @"Brightness should be between 0 and 1");

        //    Brush result;

        //    var solidColorBrush = original as SolidColorBrush;
        //    if (solidColorBrush != null)
        //    {
        //        var hsb = HSBColor.FromColor(solidColorBrush.Color);
        //        hsb.B = brightness;
        //        result = new SolidColorBrush(hsb.ToColor());
        //    }
        //    else
        //    {
        //        var gradientBrush = original as LinearGradientBrush;
        //        if (gradientBrush != null)
        //        {
        //            result = gradientBrush.Clone();
        //            // change brightness of every gradient stop
        //            foreach (var gs in ((GradientBrush)result).GradientStops)
        //            {
        //                var hsb = HSBColor.FromColor(gs.Color);
        //                hsb.B = brightness;
        //                gs.Color = hsb.ToColor();
        //            }
        //        }
        //        else
        //        {
        //            result = original.Clone();
        //        }
        //    }

        //    return result;
        //}

        internal static Brush SetLuminance(this Brush original, double luminance)
        {
            Brush result;
            var solidColorBrush = original as SolidColorBrush;
            if (solidColorBrush != null)
            {
                var clr = Color.FromArgb(solidColorBrush.Color.A, solidColorBrush.Color.R,
                    solidColorBrush.Color.G,
                    solidColorBrush.Color.B);
                var hsb = new HSLColor(clr.GetHue(), clr.GetSaturation(), clr.GetBrightness());
                hsb.Lightness *= luminance;
                result = new SolidColorBrush(hsb.RGBColor().Color);
            }
            else
            {
                var gradientBrush = original as LinearGradientBrush;
                if (gradientBrush != null)
                {
                    result = gradientBrush.Clone();
                    foreach (var gs in ((GradientBrush)result).GradientStops)
                    {
                        var clr = Color.FromArgb(gs.Color.A, gs.Color.R,
                            gs.Color.G,
                            gs.Color.B);
                        var hsb = new HSLColor(clr.GetHue(), clr.GetSaturation(), clr.GetBrightness());
                        hsb.Lightness *= luminance;
                        gs.Color = hsb.RGBColor().Color;
                    }
                }
                else
                {
                    return original.Clone();
                }
            }
            return result;
        }

        internal static T PNClone<T>(this T instance)
        {
            var bfs = new BinaryFormatter();
            var bfd = new BinaryFormatter();
            T result;
            using (var ms = new MemoryStream())
            {
                bfs.Serialize(ms, instance);
                ms.Position = 0;
                result = (T)bfd.Deserialize(ms);
            }
            return result;
        }

        internal static void SetFont(this Control t, PNFont pnf)
        {
            t.FontFamily = pnf.FontFamily;
            t.FontSize = pnf.FontSize;
            t.FontStretch = pnf.FontStretch;
            t.FontStyle = pnf.FontStyle;
            t.FontWeight = pnf.FontWeight;
        }

        internal static void SetFont(this TextBlock t, PNFont pnf)
        {
            t.FontFamily = pnf.FontFamily;
            t.FontSize = pnf.FontSize;
            t.FontStretch = pnf.FontStretch;
            t.FontStyle = pnf.FontStyle;
            t.FontWeight = pnf.FontWeight;
        }

        internal static void PerformClick(this Button b)
        {
            if (!b.IsEnabled) return;
            b.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        internal static void PerformClick(this MenuItem m)
        {
            if (!m.IsEnabled) return;
            m.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        internal static Size GetSize(this Window w)
        {
            return new Size(w.ActualWidth, w.ActualHeight);
        }

        internal static Point GetLocation(this Window w)
        {
            return new Point(w.Left, w.Top);
        }

        internal static void SetLocation(this Window w, Point pt)
        {
            w.Left = pt.X;
            w.Top = pt.Y;
        }

        internal static void SetLocation(this Window w, double x, double y)
        {
            w.Left = x;
            w.Top = y;
        }

        internal static void SetSize(this Window w, double width, double height)
        {
            w.Width = width;
            w.Height = height;
        }

        internal static void SetSize(this Window w, Size sz)
        {
            w.Width = sz.Width;
            w.Height = sz.Height;
        }

        internal static double Right(this Window w)
        {
            return w.Left + w.ActualWidth;
        }

        internal static double Bottom(this Window w)
        {
            return w.Top + w.ActualHeight;
        }

        internal static void BringToFront(this Window w)
        {
            w.Activate();
            w.Topmost = true;
            w.Topmost = false;
            w.Focus();
        }

        internal static void PlaceOnScreen(this PNote note)
        {
            var size = PNStatic.AllScreensSize();
            var rect = PNStatic.AllScreensBounds();
            if (PNStatic.Settings.Behavior.RelationalPositioning)
            {
                note.Dialog.SetLocation(new Point((int)Math.Floor(size.Width * note.XFactor),
                    (int)Math.Floor(size.Height * note.YFactor)));

                while (note.Dialog.Left + note.Dialog.Width > size.Width)
                    note.Dialog.Left--;
                if (rect.X >= 0)
                    while (note.Dialog.Left < 0)
                        note.Dialog.Left++;
                while (note.Dialog.Top + note.Dialog.Height > size.Height)
                    note.Dialog.Top--;
                if (rect.Y >= 0)
                    while (note.Dialog.Top < 0)
                        note.Dialog.Top++;
            }
            else
            {
                if (rect.IntersectsWith(new Rect(note.NoteLocation, note.NoteSize)))
                {
                    note.Dialog.SetLocation(note.NoteLocation);
                }
                else
                {
                    PNNotesOperations.CentralizeNotes(new[] { note });
                }
            }
        }

        internal static bool Inequals(this List<PNSyncComp> l1, List<PNSyncComp> l2)
        {
            return l1.Count != l2.Count ||
                   l1.Where(
                       (t, i) =>
                           t.CompName != l2[i].CompName || t.DataDir != l2[i].DataDir || t.DBDir != l2[i].DBDir ||
                           t.UseDataDir != l2[i].UseDataDir).Any();
        }

        internal static bool Inequals(this List<PNContactGroup> l1, List<PNContactGroup> l2)
        {
            return l1.Count != l2.Count || l1.Where((t, i) => t.Name != l2[i].Name || t.ID != l2[i].ID).Any();
        }

        internal static bool Inequals(this List<PNContact> l1, List<PNContact> l2)
        {
            return l1.Count != l2.Count ||
                   l1.Where(
                       (t, i) =>
                           t.Name != l2[i].Name || t.ComputerName != l2[i].ComputerName || t.GroupID != l2[i].GroupID ||
                           t.IpAddress != l2[i].IpAddress || t.UseComputerName != l2[i].UseComputerName ||
                           t.ID != l2[i].ID).Any();
        }

        internal static bool Inequals(this List<PNSearchProvider> l1, List<PNSearchProvider> l2)
        {
            return l1.Count != l2.Count ||
                   l1.Where((t, i) => t.Name != l2[i].Name || t.QueryString != l2[i].QueryString).Any();
        }

        internal static bool Inequals(this List<PNSmtpProfile> l1, List<PNSmtpProfile> l2)
        {
            return l1.Count != l2.Count || l1.Where(
                (t, i) =>
                    t.HostName != l2[i].HostName || t.SenderAddress != l2[i].SenderAddress || t.Port != l2[i].Port ||
                    t.Password != l2[i].Password || t.Active != l2[i].Active || t.DisplayName != l2[i].DisplayName)
                .Any();
        }

        internal static bool Inequals(this List<PNMailContact> l1, List<PNMailContact> l2)
        {
            return l1.Count != l2.Count ||
                   l1.Where((t, i) => t.DisplayName != l2[i].DisplayName || t.Address != l2[i].Address).Any();
        }

        internal static bool Inequals(this List<PNExternal> l1, List<PNExternal> l2)
        {
            return l1.Count != l2.Count ||
                   l1.Where(
                       (t, i) =>
                           t.Name != l2[i].Name || t.CommandLine != l2[i].CommandLine || t.Program != l2[i].Program)
                       .Any();
        }

        internal static bool Inequals(this List<string> l1, List<string> l2)
        {
            return l1.Count != l2.Count || l1.Where((t, i) => t != l2[i]).Any();
        }

        internal static bool Inequals(this List<DayOfWeek> l1, List<DayOfWeek> l2)
        {
            return l1.Count != l2.Count || l1.Where((t, i) => t != l2[i]).Any();
        }

        internal static string ToCommaSeparatedString<T>(this IEnumerable<T> list)
        {
            var sb = new StringBuilder();
            foreach (var s in list)
            {
                sb.Append(s);
                sb.Append(",");
            }
            if (sb.Length > 0)
            {
                sb.Length -= 1;
            }
            return sb.ToString();
        }

        internal static string ToCommaSeparatedString(this IEnumerable<string> list)
        {
            return string.Join(",", list);
            //StringBuilder sb = new StringBuilder();
            //foreach (string s in listNames)
            //{
            //    sb.Append(s);
            //    sb.Append(",");
            //}
            //if (sb.Length > 0)
            //{
            //    sb.Length -= 1;
            //}
            //return sb.ToString();
        }

        internal static string ToCommaSeparatedSQLString(this List<string> list)
        {
            var sb = new StringBuilder();
            foreach (var s in list)
            {
                sb.Append("'");
                sb.Append(s);
                sb.Append("',");
            }
            if (sb.Length > 0)
            {
                sb.Length -= 1;
            }
            return sb.ToString();
        }

        internal static PNGroup SpecialGroup(this List<PNGroup> groups, SpecialGroups type)
        {
            return groups.FirstOrDefault(g => g.ID == (int)type);
        }

        internal static PNGroup GetGroupByID(this List<PNGroup> groups, int id)
        {
            return groups.Select(g => g.GetGroupById(id)).FirstOrDefault(result => result != null);
        }

        internal static PNote Note(this List<PNote> notes, string id)
        {
            return notes.FirstOrDefault(n => n.ID == id);
        }

        internal static PNote Note(this List<PNote> notes, Guid handle)
        {
            return notes.FirstOrDefault(n => n.Dialog != null && !n.Dialog.IsDisposed && n.Dialog.Handle == handle);
        }

        internal static bool IsEqual(this BitmapImage image, BitmapImage other)
        {
            if (image == null || other == null) return false;
            return image.ToBytes().SequenceEqual(other.ToBytes());
        }

        internal static byte[] ToBytes(this BitmapImage image)
        {
            var data = new byte[] { };
            if (image != null)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                using (var ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    data = ms.ToArray();
                }
            }
            return data;
        }

        internal static bool IsDateEqual(this DateTime d1, DateTime d2)
        {
            if (PNStatic.Settings.GeneralSettings.DateBits[DatePart.Year] && d1.Year != d2.Year)
                return false;
            if (PNStatic.Settings.GeneralSettings.DateBits[DatePart.Month] && d1.Month != d2.Month)
                return false;
            if (PNStatic.Settings.GeneralSettings.DateBits[DatePart.Day] && d1.Day != d2.Day)
                return false;
            if (PNStatic.Settings.GeneralSettings.DateBits[DatePart.Hour] && d1.Hour != d2.Hour)
                return false;
            if (PNStatic.Settings.GeneralSettings.DateBits[DatePart.Minute] && d1.Minute != d2.Minute)
                return false;
            if (PNStatic.Settings.GeneralSettings.DateBits[DatePart.Second] && d1.Second != d2.Second)
                return false;
            if (PNStatic.Settings.GeneralSettings.DateBits[DatePart.Millisecond] && d1.Millisecond != d2.Millisecond)
                return false;
            return true;
        }

        internal static bool IsTimeEqual(this DateTime t1, DateTime t2)
        {
            if (PNStatic.Settings.GeneralSettings.TimeBits[DatePart.Hour] && t1.Hour != t2.Hour)
                return false;
            if (PNStatic.Settings.GeneralSettings.TimeBits[DatePart.Minute] && t1.Minute != t2.Minute)
                return false;
            if (PNStatic.Settings.GeneralSettings.TimeBits[DatePart.Second] && t1.Second != t2.Second)
                return false;
            if (PNStatic.Settings.GeneralSettings.TimeBits[DatePart.Millisecond] && t1.Millisecond != t2.Millisecond)
                return false;
            return true;
        }

        internal static bool IsTimeMore(this DateTime t1, DateTime t2)
        {
            var tf = new DateTime(1, 1, 1);
            var tt = new DateTime(1, 1, 1);
            if (PNStatic.Settings.GeneralSettings.TimeBits[DatePart.Hour])
            {
                tf = tf.AddHours(t1.Hour);
                tt = tt.AddHours(t2.Hour);
            }
            if (PNStatic.Settings.GeneralSettings.TimeBits[DatePart.Minute])
            {
                tf = tf.AddMinutes(t1.Minute);
                tt = tt.AddMinutes(t2.Minute);
            }
            if (PNStatic.Settings.GeneralSettings.TimeBits[DatePart.Second])
            {
                tf = tf.AddSeconds(t1.Second);
                tt = t2.AddSeconds(t2.Second);
            }
            if (PNStatic.Settings.GeneralSettings.TimeBits[DatePart.Millisecond])
            {
                tf = tf.AddMilliseconds(t1.Millisecond);
                tt = tt.AddMilliseconds(t2.Millisecond);
            }
            return tf > tt;
        }

        internal static bool IsEqual(this List<MultiAlert> list1, List<MultiAlert> list2)
        {
            var count = list1.Count();
            if (count != list2.Count()) return false;
            for (var i = 0; i < count; i++)
            {
                if (list1[i].Raised != list2[i].Raised || list1[i].Date != list2[i].Date)
                    return false;
            }
            return true;
        }

        internal static string[] Split(this string str, string delimeter)
        {
            var arr = new string[0];
            int pos = 0, start = 0;
            while (pos > -1)
            {
                pos = str.IndexOf(delimeter, start, StringComparison.Ordinal);
                Array.Resize(ref arr, arr.Length + 1);
                if (pos > -1)
                    arr[arr.Length - 1] = str.Substring(start, pos - start);
                else
                    arr[arr.Length - 1] = str.Substring(start);
                start = pos + delimeter.Length;
            }
            return arr;
        }

        internal static bool In<T>(this T member, params T[] values)
        {
            return values.Contains(member);
        }

        internal static string ContactNameByComputerName(this List<PNContact> contacts, string computerName)
        {
            var result = computerName;
            var contact = contacts.FirstOrDefault(c => c.ComputerName == computerName) ??
                          contacts.FirstOrDefault(c => c.IpAddress == computerName);
            if (contact != null)
            {
                result = contact.Name;
            }
            return result;
        }

        internal static bool IsEmpty(this Point p)
        {
            return (double.IsNaN(p.X) || double.IsNaN(p.Y));
        }

        internal static void ExpandAll(this TreeView tvw)
        {
            foreach (var it in tvw.Items.OfType<PNTreeItem>())
                expandTreeItem(it);
        }

        private static void expandTreeItem(PNTreeItem item)
        {
            foreach (var it in item.Items.OfType<PNTreeItem>())
                expandTreeItem(it);
            item.IsExpanded = true;
        }

        internal static ImageSource ToImageSource(this Icon icon)
        {
            var bitmap = icon.ToBitmap();
            var hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!PNInterop.DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }

        internal static object GetHierarchyObjectAtPoint<TItemContainer>(this ItemsControl control, Point p)
                                     where TItemContainer : DependencyObject
        {
            // ItemContainer - can be ListViewItem, or TreeViewItem and so on(depends on control)
            var obj = GetContainerAtPoint<TItemContainer>(control, p);
            if (obj == null)
                return null;
            // control.ItemContainerGenerator.ItemFromContainer(obj) may return null for child treeview items 
            var item = control.ItemContainerGenerator.ItemFromContainer(obj);
            return item != DependencyProperty.UnsetValue ? item : obj;
        }

        internal static object GetObjectAtPoint<TItemContainer>(this ItemsControl control, Point p)
                                     where TItemContainer : DependencyObject
        {
            // ItemContainer - can be ListViewItem, or TreeViewItem and so on(depends on control)
            var obj = GetContainerAtPoint<TItemContainer>(control, p);
            return obj == null ? null : control.ItemContainerGenerator.ItemFromContainer(obj);
        }

        internal static TItemContainer GetContainerAtPoint<TItemContainer>(this ItemsControl control, Point p)
                                 where TItemContainer : DependencyObject
        {
            var result = VisualTreeHelper.HitTest(control, p);
            var obj = result.VisualHit;

            while (VisualTreeHelper.GetParent(obj) != null && !(obj is TItemContainer))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }

            // Will return null if not found
            return obj as TItemContainer;
        }
    }
}
