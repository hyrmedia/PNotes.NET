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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PNStaticFonts;
using FontWeight = System.Windows.FontWeight;
using Point = System.Windows.Point;
using PointConverter = System.Windows.PointConverter;
using Size = System.Drawing.Size;
using SizeConverter = System.Windows.SizeConverter;

namespace PNotes.NET
{
    internal class NoteConverter : TypeConverter
    {
        private const char DEL_NOTE = '\f';
        private const char DEL_INNER = '\a';

        private enum Fields
        {
            Changed,
            Completed,
            CustomOpacity,
            DateCreated,
            DateDeleted,
            DateReceived,
            DateSaved,
            DateSent,
            DockStatus,
            EditSize,
            Favorite,
            FromDB,
            GroupID,
            ID,
            LinkedNotes,
            Name,
            NoteLocation,
            NoteSize,
            Opacity,
            PasswordString,
            Pinned,
            PrevGroupID,
            Priority,
            Protected,
            ReceivedFrom,
            Rolled,
            Schedule,
            SentReceived,
            SentTo,
            SkinName,
            Skinless,
            Tags,
            Topmost,
            Visible,
            XFactor,
            YFactor,
            PinClass,
            PinText
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var str = value as string;
            if (str != null)
            {
                var note = new PNote();
                var values = str.Split(DEL_NOTE);
                if (values.Length == Enum.GetValues(typeof(Fields)).Length)
                {
                    string[] arr;
                    var dtc = new DateTimeConverter();
                    var szc = new SizeConverter();
                    var scd = new System.Drawing.SizeConverter();
                    var ptc = new PointConverter();
                    var scv = new ScheduleConverter();
                    var skc = new SkinlessConverter();

                    note.Changed = Convert.ToBoolean(values[(int)Fields.Changed]);
                    note.Completed = Convert.ToBoolean(values[(int)Fields.Completed]);
                    note.CustomOpacity = Convert.ToBoolean(values[(int)Fields.CustomOpacity]);
                    var dateFromString = dtc.ConvertFromString(null, PNStatic.CultureInvariant, values[(int)Fields.DateCreated]);
                    if (dateFromString != null)
                        note.DateCreated = (DateTime)dateFromString;
                    dateFromString = dtc.ConvertFromString(null, PNStatic.CultureInvariant, values[(int)Fields.DateDeleted]);
                    if (dateFromString != null)
                        note.DateDeleted = (DateTime)dateFromString;
                    dateFromString = dtc.ConvertFromString(null, PNStatic.CultureInvariant, values[(int)Fields.DateReceived]);
                    if (dateFromString != null)
                        note.DateReceived = (DateTime)dateFromString;
                    dateFromString = dtc.ConvertFromString(null, PNStatic.CultureInvariant, values[(int)Fields.DateSaved]);
                    if (dateFromString != null)
                        note.DateSaved = (DateTime)dateFromString;
                    dateFromString = dtc.ConvertFromString(null, PNStatic.CultureInvariant, values[(int)Fields.DateSent]);
                    if (dateFromString != null)
                        note.DateSent = (DateTime)dateFromString;
                    note.DockStatus = (DockStatus)Convert.ToInt32(values[(int)Fields.DockStatus]);
                    var sizeFromString = scd.ConvertFromString(null, PNStatic.CultureInvariant, values[(int)Fields.EditSize]);
                    if (sizeFromString != null)
                        note.EditSize = (Size)sizeFromString;
                    note.Favorite = Convert.ToBoolean(values[(int)Fields.Favorite]);
                    note.FromDB = Convert.ToBoolean(values[(int)Fields.FromDB]);
                    note.GroupID = Convert.ToInt32(values[(int)Fields.GroupID]);
                    note.ID = values[(int)Fields.ID];
                    var temp = values[(int)Fields.LinkedNotes];
                    if (temp != "")
                    {
                        arr = temp.Split(DEL_INNER);
                        foreach (var s in arr)
                        {
                            note.LinkedNotes.Add(s);
                        }
                    }
                    note.Name = values[(int)Fields.Name];
                    var convertFromString = ptc.ConvertFromString(null, PNStatic.CultureInvariant, values[(int)Fields.NoteLocation]);
                    if (convertFromString != null)
                        note.NoteLocation = (Point)convertFromString;
                    var fromString = szc.ConvertFromString(null, PNStatic.CultureInvariant, values[(int)Fields.NoteSize]);
                    if (fromString != null)
                        note.NoteSize = (System.Windows.Size)fromString;
                    note.Opacity = Convert.ToDouble(values[(int)Fields.Opacity], PNStatic.CultureInvariant);
                    note.PasswordString = values[(int)Fields.PasswordString];
                    note.Pinned = Convert.ToBoolean(values[(int)Fields.Pinned]);
                    note.PrevGroupID = Convert.ToInt32(values[(int)Fields.PrevGroupID]);
                    note.Priority = Convert.ToBoolean(values[(int)Fields.Priority]);
                    note.Protected = Convert.ToBoolean(values[(int)Fields.Protected]);
                    note.ReceivedFrom = values[(int)Fields.ReceivedFrom];
                    note.Rolled = Convert.ToBoolean(values[(int)Fields.Rolled]);
                    note.Schedule = (PNNoteSchedule)scv.ConvertFromString(values[(int)Fields.Schedule]);
                    note.SentReceived = (SendReceiveStatus)Convert.ToInt32(values[(int)Fields.SentReceived]);
                    note.SentTo = values[(int)Fields.SentTo];
                    temp = values[(int)Fields.SkinName];
                    if (temp != PNSkinDetails.NO_SKIN && temp != "")
                    {
                        // TODO - get skin properties
                    }
                    note.Skinless = (PNSkinlessDetails)skc.ConvertFromString(values[(int)Fields.Skinless]);
                    temp = values[(int)Fields.Tags];
                    if (temp != "")
                    {
                        arr = temp.Split(DEL_INNER);
                        foreach (var s in arr)
                        {
                            note.Tags.Add(s);
                        }
                    }
                    note.Topmost = Convert.ToBoolean(values[(int)Fields.Topmost]);
                    note.Visible = Convert.ToBoolean(values[(int)Fields.Visible]);
                    note.XFactor = Convert.ToDouble(values[(int)Fields.XFactor], PNStatic.CultureInvariant);
                    note.YFactor = Convert.ToDouble(values[(int)Fields.YFactor], PNStatic.CultureInvariant);
                    note.PinClass = values[(int)Fields.PinClass];
                    note.PinText = values[(int)Fields.PinText];
                }
                return note;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof (string)) return base.ConvertTo(context, culture, value, destinationType);
            var note = value as PNote;
            if (note == null) return "";
            var bt = new StringBuilder();
            var dtc = new DateTimeConverter();
            var szc = new SizeConverter();
            var scd = new System.Drawing.SizeConverter();
            var ptc = new PointConverter();
            var scv = new ScheduleConverter();
            var skc = new SkinlessConverter();
            var sb = new StringBuilder();

            sb.Append(note.Changed);
            sb.Append(DEL_NOTE);
            sb.Append(note.Completed);
            sb.Append(DEL_NOTE);
            sb.Append(note.CustomOpacity);
            sb.Append(DEL_NOTE);
            sb.Append(dtc.ConvertToString(null, PNStatic.CultureInvariant, note.DateCreated));
            sb.Append(DEL_NOTE);
            sb.Append(dtc.ConvertToString(null, PNStatic.CultureInvariant, note.DateDeleted));
            sb.Append(DEL_NOTE);
            sb.Append(dtc.ConvertToString(null, PNStatic.CultureInvariant, note.DateReceived));
            sb.Append(DEL_NOTE);
            sb.Append(dtc.ConvertToString(null, PNStatic.CultureInvariant, note.DateSaved));
            sb.Append(DEL_NOTE);
            sb.Append(dtc.ConvertToString(null, PNStatic.CultureInvariant, note.DateSent));
            sb.Append(DEL_NOTE);
            sb.Append((int)note.DockStatus);
            sb.Append(DEL_NOTE);
            sb.Append(scd.ConvertToString(null, PNStatic.CultureInvariant, note.EditSize));
            sb.Append(DEL_NOTE);
            sb.Append(note.Favorite);
            sb.Append(DEL_NOTE);
            sb.Append(note.FromDB);
            sb.Append(DEL_NOTE);
            sb.Append(note.GroupID);
            sb.Append(DEL_NOTE);
            sb.Append(note.ID);
            sb.Append(DEL_NOTE);
            foreach (var s in note.LinkedNotes)
            {
                bt.Append(s);
                bt.Append(DEL_INNER);
            }
            if (bt.Length > 0)
                bt.Length -= 1;
            sb.Append(bt);
            sb.Append(DEL_NOTE);
            sb.Append(note.Name);
            sb.Append(DEL_NOTE);
            sb.Append(ptc.ConvertToString(null, PNStatic.CultureInvariant, note.NoteLocation));
            sb.Append(DEL_NOTE);
            sb.Append(szc.ConvertToString(null, PNStatic.CultureInvariant, note.NoteSize));
            sb.Append(DEL_NOTE);
            sb.Append(note.Opacity.ToString(PNStatic.CultureInvariant));
            sb.Append(DEL_NOTE);
            sb.Append(note.PasswordString);
            sb.Append(DEL_NOTE);
            sb.Append(note.Pinned);
            sb.Append(DEL_NOTE);
            sb.Append(note.PrevGroupID);
            sb.Append(DEL_NOTE);
            sb.Append(note.Priority);
            sb.Append(DEL_NOTE);
            sb.Append(note.Protected);
            sb.Append(DEL_NOTE);
            sb.Append(note.ReceivedFrom);
            sb.Append(DEL_NOTE);
            sb.Append(note.Rolled);
            sb.Append(DEL_NOTE);
            sb.Append(scv.ConvertToString(note.Schedule));
            sb.Append(DEL_NOTE);
            sb.Append((int)note.SentReceived);
            sb.Append(DEL_NOTE);
            sb.Append(note.SentTo);
            sb.Append(DEL_NOTE);
            sb.Append(note.Skin != null ? note.Skin.SkinName : "");
            sb.Append(DEL_NOTE);
            sb.Append(note.Skinless != null ? skc.ConvertToString(note.Skinless) : "");
            sb.Append(DEL_NOTE);
            bt = new StringBuilder();
            foreach (var s in note.Tags)
            {
                bt.Append(s);
                bt.Append(DEL_INNER);
            }
            if (bt.Length > 0)
                bt.Length -= 1;
            sb.Append(bt);
            sb.Append(DEL_NOTE);
            sb.Append(note.Topmost);
            sb.Append(DEL_NOTE);
            sb.Append(note.Visible);
            sb.Append(DEL_NOTE);
            sb.Append(note.XFactor.ToString(PNStatic.CultureInvariant));
            sb.Append(DEL_NOTE);
            sb.Append(note.YFactor.ToString(PNStatic.CultureInvariant));
            sb.Append(DEL_NOTE);
            sb.Append(note.PinClass);
            sb.Append(DEL_NOTE);
            sb.Append(note.PinText);
            return sb.ToString();
        }
    }

    internal class SkinlessConverter : TypeConverter
    {
        private const char DEL_INNER = '\a';

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var str = value as string;
            if (str == null) return base.ConvertFrom(context, culture, value);
            var skl = new PNSkinlessDetails();
            var values = str.Split(DEL_INNER);
            if (values.Length != 3) return skl;
            var cr = new ColorConverter();
            var wpfFontConverter = new WPFFontConverter();
            var lfc = new LogFontConverter();
            object convertFromString;

            try
            {
                convertFromString = cr.ConvertFromString(null, PNStatic.CultureInvariant, values[0]);
                if (convertFromString != null)
                    skl.BackColor = (Color)convertFromString;
            }
            catch (FormatException)
            {
                var drawingColorConverter = new System.Drawing.ColorConverter();
                convertFromString = drawingColorConverter.ConvertFromString(null, PNStatic.CultureInvariant,
                    values[0]);
                if (convertFromString != null)
                {
                    var clr = (System.Drawing.Color)convertFromString;
                    skl.BackColor = Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
                }
            }

            try
            {
                convertFromString = cr.ConvertFromString(null, PNStatic.CultureInvariant, values[1]);
                if (convertFromString != null)
                    skl.CaptionColor = (Color)convertFromString;
            }
            catch (FormatException)
            {
                var drawingColorConverter = new System.Drawing.ColorConverter();
                convertFromString = drawingColorConverter.ConvertFromString(null, PNStatic.CultureInvariant,
                    values[1]);
                if (convertFromString != null)
                {
                    var clr = (System.Drawing.Color)convertFromString;
                    skl.CaptionColor = Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
                }
            }

            if (values[2].Any(c => c == '^'))
            {
                var logFont = lfc.ConvertFromString(values[2]);
                skl.CaptionFont = PNStatic.FromLogFont(logFont);
            }
            else
            {
                skl.CaptionFont = (PNFont) wpfFontConverter.ConvertFromString(values[2]);
            }
            return skl;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof (string)) return base.ConvertTo(context, culture, value, destinationType);
            var skl = value as PNSkinlessDetails;
            if (skl == null) return "";
            var cr = new ColorConverter();
            var lfc = new WPFFontConverter();

            var sb = new StringBuilder();
            sb.Append(cr.ConvertToString(null, PNStatic.CultureInvariant, skl.BackColor));
            sb.Append(DEL_INNER);
            sb.Append(cr.ConvertToString(null, PNStatic.CultureInvariant, skl.CaptionColor));
            sb.Append(DEL_INNER);
            sb.Append(lfc.ConvertToString(skl.CaptionFont));
            return sb.ToString();
        }
    }

    internal class ScheduleConverter : TypeConverter
    {
        private const char DEL_INNER = '\a';

        private enum Fields
        {
            AlarmAfter,
            AlarmDate,
            LastRun,
            MonthDay,
            RepeatCount,
            Sound,
            SoundInLoop,
            StartDate,
            StartFrom,
            StopAfter,
            Track,
            Type,
            UseTTS,
            Weekdays,
            ExtRun,
            CloseOnNotify
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var str = value as string;
            if (str != null)
            {
                var dwc = new DaysOfWeekConverter();
                var mc = new MonthDayConverter();
                var avc = new AlarmAfterValuesConverter();
                var dtc = new DateTimeConverter();

                var sch = new PNNoteSchedule();
                var values = str.Split(DEL_INNER);
                if (values.Length == Enum.GetValues(typeof(Fields)).Length)
                {
                    sch.AlarmAfter = (AlarmAfterValues)avc.ConvertFromString(values[(int)Fields.AlarmAfter]);
                    var dateFromString = dtc.ConvertFromString(null, PNStatic.CultureInvariant, values[(int)Fields.AlarmDate]);
                    if (dateFromString != null)
                        sch.AlarmDate = (DateTime)dateFromString;
                    dateFromString = dtc.ConvertFromString(null, PNStatic.CultureInvariant, values[(int)Fields.LastRun]);
                    if (dateFromString != null)
                        sch.LastRun = (DateTime)dateFromString;
                    sch.MonthDay = (MonthDay)mc.ConvertFromString(values[(int)Fields.MonthDay]);
                    sch.RepeatCount = Convert.ToInt32(values[(int)Fields.RepeatCount]);
                    sch.Sound = values[(int)Fields.Sound];
                    sch.SoundInLoop = Convert.ToBoolean(values[(int)Fields.SoundInLoop]);
                    dateFromString = dtc.ConvertFromString(null, PNStatic.CultureInvariant, values[(int)Fields.StartDate]);
                    if (dateFromString != null)
                        sch.StartDate = (DateTime)dateFromString;
                    sch.StartFrom = (ScheduleStart)Convert.ToInt32(values[(int)Fields.StartFrom]);
                    sch.StopAfter = Convert.ToInt32(values[(int)Fields.StopAfter]);
                    sch.Track = Convert.ToBoolean(values[(int)Fields.Track]);
                    sch.Type = (ScheduleType)Convert.ToInt32(values[(int)Fields.Type]);
                    sch.UseTts = Convert.ToBoolean(values[(int)Fields.UseTTS]);
                    sch.Weekdays = (List<DayOfWeek>)dwc.ConvertFromString(values[(int)Fields.Weekdays]);
                    sch.ProgramToRunOnAlert = values[(int)Fields.ExtRun];
                    sch.CloseOnNotification = Convert.ToBoolean(values[(int)Fields.CloseOnNotify]);
                }
                return sch;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value == null)
                {
                    return "";
                }
                var sch = value as PNNoteSchedule;
                if (sch != null)
                {
                    var dwc = new DaysOfWeekConverter();
                    var mc = new MonthDayConverter();
                    var avc = new AlarmAfterValuesConverter();
                    var dtc = new DateTimeConverter();

                    var sb = new StringBuilder();
                    sb.Append(avc.ConvertToString(sch.AlarmAfter));
                    sb.Append(DEL_INNER);
                    sb.Append(dtc.ConvertToString(null, PNStatic.CultureInvariant, sch.AlarmDate));
                    sb.Append(DEL_INNER);
                    sb.Append(dtc.ConvertToString(null, PNStatic.CultureInvariant, sch.LastRun));
                    sb.Append(DEL_INNER);
                    sb.Append(mc.ConvertToString(sch.MonthDay));
                    sb.Append(DEL_INNER);
                    sb.Append(sch.RepeatCount);
                    sb.Append(DEL_INNER);
                    sb.Append(sch.Sound);
                    sb.Append(DEL_INNER);
                    sb.Append(sch.SoundInLoop);
                    sb.Append(DEL_INNER);
                    sb.Append(dtc.ConvertToString(null, PNStatic.CultureInvariant, sch.StartDate));
                    sb.Append(DEL_INNER);
                    sb.Append((int)sch.StartFrom);
                    sb.Append(DEL_INNER);
                    sb.Append(sch.StopAfter);
                    sb.Append(DEL_INNER);
                    sb.Append(sch.Track);
                    sb.Append(DEL_INNER);
                    sb.Append((int)sch.Type);
                    sb.Append(DEL_INNER);
                    sb.Append(sch.UseTts);
                    sb.Append(DEL_INNER);
                    sb.Append(dwc.ConvertToString(sch.Weekdays));
                    sb.Append(DEL_INNER);
                    sb.Append(sch.ProgramToRunOnAlert);
                    sb.Append(DEL_INNER);
                    sb.Append(sch.CloseOnNotification);
                    return sb.ToString();
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    internal class DaysOfWeekConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var str = value as string;
            if (str != null)
            {
                var list = new List<DayOfWeek>();
                var values = str.Split(',');
                if (str.Trim().Length > 0 && values.Length > 0)
                {
                    list.AddRange(values.Select(t => (DayOfWeek)int.Parse(t, PNStatic.CultureInvariant)));
                }
                return list;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var sb = new StringBuilder();
                var list = (List<DayOfWeek>)value;
                if (list.Count > 0)
                {
                    foreach (var d in list)
                    {
                        sb.Append((int)d);
                        sb.Append(",");
                    }
                    sb.Length -= 1;
                }
                return sb.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    internal class MonthDayConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var s = value as string;
            if (s != null)
            {
                var v = s.Split(',');
                return new MonthDay { WeekDay = (DayOfWeek)int.Parse(v[0], PNStatic.CultureInvariant), OrdinalNumber = (DayOrdinal)int.Parse(v[1], PNStatic.CultureInvariant) };
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return ((int)((MonthDay)value).WeekDay).ToString(CultureInfo.InvariantCulture) + "," + (int)((MonthDay)value).OrdinalNumber;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    internal class AlarmAfterValuesConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var s = value as string;
            if (s != null)
            {
                var v = s.Split(',');
                return new AlarmAfterValues { Years = int.Parse(v[0], PNStatic.CultureInvariant), Months = int.Parse(v[1], PNStatic.CultureInvariant), Weeks = int.Parse(v[2], PNStatic.CultureInvariant), Days = int.Parse(v[3], PNStatic.CultureInvariant), Hours = int.Parse(v[4], PNStatic.CultureInvariant), Minutes = int.Parse(v[5], PNStatic.CultureInvariant), Seconds = int.Parse(v[6], PNStatic.CultureInvariant) };
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var v = (AlarmAfterValues)value;
                return v.Years.ToString(CultureInfo.InvariantCulture) + "," +
                       v.Months.ToString(CultureInfo.InvariantCulture) + "," +
                       v.Weeks.ToString(CultureInfo.InvariantCulture) + "," +
                       v.Days.ToString(CultureInfo.InvariantCulture) + "," +
                       v.Hours.ToString(CultureInfo.InvariantCulture) + "," +
                       v.Minutes.ToString(CultureInfo.InvariantCulture) + "," +
                       v.Seconds.ToString(CultureInfo.InvariantCulture);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    internal class BrushBrightnessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = value as Brush;
            if (brush == null) return value;
            var luminance = parameter is string ? System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture) : 1.0;
            return brush.SetLuminance(luminance);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class DockArrowToAngelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var da = (DockArrow)value;
            switch (da)
            {
                case DockArrow.LeftUp:
                case DockArrow.RightUp:
                    return 0;
                case DockArrow.LeftDown:
                case DockArrow.RightDown:
                    return 180;
                case DockArrow.TopLeft:
                case DockArrow.BottomLeft:
                    return 270;
                case DockArrow.TopRight:
                case DockArrow.BottomRight:
                    return 90;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class BaloonImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is BaloonMode)) return null;
            var mode = (BaloonMode)value;
            var fe = new FrameworkElement();
            switch (mode)
            {
                case BaloonMode.FirstRun:
                    return fe.TryFindResource("thumbs_up") as BitmapImage;
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "thumbs_up.png"));
                case BaloonMode.NewVersion:
                case BaloonMode.CriticalUpdates:
                    return fe.TryFindResource("package_new") as BitmapImage;
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "package_new.png"));
                case BaloonMode.NoteSent:
                    return fe.TryFindResource("outbox_out") as BitmapImage;
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "outbox_out.png"));
                case BaloonMode.NoteReceived:
                    return fe.TryFindResource("inbox_into") as BitmapImage;
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "inbox_into.png"));
                case BaloonMode.Error:
                    return fe.TryFindResource("error") as BitmapImage;
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "error.png"));
                case BaloonMode.Information:
                    return fe.TryFindResource("information") as BitmapImage;
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "information.png"));
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class ColorBrightnessToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Color)) return null;

            var luminance = parameter is string ? System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture) : 1.0;
            var solidColorBrush = new SolidColorBrush((Color)value).SetLuminance(luminance) as SolidColorBrush;
            if (solidColorBrush != null)
                return solidColorBrush.Color;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class BrushBrightnessToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = value as SolidColorBrush;
            if (brush == null) return null;
            var luminance = parameter is string ? System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture) : 1.0;
            return ((SolidColorBrush)brush.SetLuminance(luminance)).Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    internal class WPFFontConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var s = value as string;
            if (s == null) return base.ConvertFrom(context, culture, value);
            var v = s.Split(',');

            var wpf = new PNFont { FontFamily = new FontFamily(v[0]), FontSize = double.Parse(v[1], CultureInfo.InvariantCulture) };
            switch (v[2])
            {
                case "Oblique":
                    wpf.FontStyle = FontStyles.Oblique;
                    break;
                case "Italic":
                    wpf.FontStyle = FontStyles.Italic;
                    break;
                default:
                    wpf.FontStyle = FontStyles.Normal;
                    break;
            }
            switch (v[3])
            {
                case "Black":
                    wpf.FontWeight = FontWeights.Black;
                    break;
                case "Bold":
                    wpf.FontWeight = FontWeights.Bold;
                    break;
                case "DemiBold":
                    wpf.FontWeight = FontWeights.DemiBold;
                    break;
                case "ExtraBlack":
                    wpf.FontWeight = FontWeights.ExtraBlack;
                    break;
                case "ExtraBold":
                    wpf.FontWeight = FontWeights.ExtraBold;
                    break;
                case "ExtraLight":
                    wpf.FontWeight = FontWeights.ExtraLight;
                    break;
                case "Heavy":
                    wpf.FontWeight = FontWeights.Heavy;
                    break;
                case "Light":
                    wpf.FontWeight = FontWeights.Light;
                    break;
                case "Medium":
                    wpf.FontWeight = FontWeights.Medium;
                    break;
                case "Regular":
                    wpf.FontWeight = FontWeights.Regular;
                    break;
                case "SemiBold":
                    wpf.FontWeight = FontWeights.SemiBold;
                    break;
                case "Thin":
                    wpf.FontWeight = FontWeights.Thin;
                    break;
                case "UltraBlack":
                    wpf.FontWeight = FontWeights.UltraBlack;
                    break;
                case "UltraBold":
                    wpf.FontWeight = FontWeights.UltraBold;
                    break;
                case "UltraLight":
                    wpf.FontWeight = FontWeights.UltraLight;
                    break;
                default:
                    wpf.FontWeight = FontWeights.Normal;
                    break;
            }
            switch (v[4])
            {
                case "Condensed":
                    wpf.FontStretch = FontStretches.Condensed;
                    break;
                case "Expanded":
                    wpf.FontStretch = FontStretches.Expanded;
                    break;
                case "ExtraCondensed":
                    wpf.FontStretch = FontStretches.ExtraCondensed;
                    break;
                case "ExtraExpanded":
                    wpf.FontStretch = FontStretches.ExtraExpanded;
                    break;
                case "Medium":
                    wpf.FontStretch = FontStretches.Medium;
                    break;
                case "SemiCondensed":
                    wpf.FontStretch = FontStretches.SemiCondensed;
                    break;
                case "SemiExpanded":
                    wpf.FontStretch = FontStretches.SemiExpanded;
                    break;
                case "UltraCondensed":
                    wpf.FontStretch = FontStretches.UltraCondensed;
                    break;
                case "UltraExpanded":
                    wpf.FontStretch = FontStretches.UltraExpanded;
                    break;
                default:
                    wpf.FontStretch = FontStretches.Normal;
                    break;
            }
            return wpf;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var v = (PNFont)value;
                var sb = new StringBuilder(v.FontFamily.ToString());
                sb.Append(",");
                sb.Append(v.FontSize.ToString("0.0#", CultureInfo.InvariantCulture));
                sb.Append(",");
                sb.Append(v.FontStyle);
                sb.Append(",");
                sb.Append(v.FontWeight);
                sb.Append(",");
                sb.Append(v.FontStretch);
                return sb.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    internal class HeaderTextHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(values[0] is string) || !(values[1] is FontFamily) || !(values[2] is FontStyle) ||
                !(values[3] is FontWeight) || !(values[4] is FontStretch) || !(values[5] is double) ||
                !(values[6] is Brush)) return 16;
            if (((string)values[0]).Length == 0)
                return 16;
            var sz = PNStatic.MeasureTextSize((string)values[0], (FontFamily)values[1], (FontStyle)values[2],
                (FontWeight)values[3], (FontStretch)values[4], (double)values[5], (Brush)values[6]);
            sz.Height += 4;
            return sz.Height;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class HeaderTextWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //parameter is header button width
            const double margins = 16;
            if (!(values[0] is string) || !(values[1] is FontFamily) || !(values[2] is FontStyle) ||
                !(values[3] is FontWeight) || !(values[4] is FontStretch) || !(values[5] is double) ||
                !(values[6] is Brush) || !(parameter is string)) return 0;
            if (((string)values[0]).Length == 0)
                return 0;
            var sz = PNStatic.MeasureTextSize((string)values[0], (FontFamily)values[1], (FontStyle)values[2],
                (FontWeight)values[3], (FontStretch)values[4], (double)values[5], (Brush)values[6]);
            return sz.Width + System.Convert.ToDouble(parameter) * 3 + margins;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class SizeRelationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double) || !(parameter is double))
                return value;
            return (double)value * (double)parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class RolledVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool)) return Visibility.Visible;
            return (bool)value ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class CheckedToEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var check = value as bool?;
            return check != null && check.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class CheckedToEnableOppsiteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var check = value as bool?;
            return check != null && !check.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class CheckedToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var check = value as bool?;
            if (check == null) return Visibility.Collapsed;
            return check.Value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class DateToDisplayStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime)) return "";
            var d = (DateTime)value;
            if (d == DateTime.MinValue) return "";
            return
                d.ToString(Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern + " " +
                           Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongTimePattern);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class PropertyToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if (!(bool)value || !(parameter is string)) return "";
                switch (System.Convert.ToString(parameter))
                {
                    case "Priority":
                        return "lspriority";
                    //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lspriority.png"));
                    case "Completed":
                        return "lscomplete";
                    //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lscomplete.png"));
                    case "Protected":
                        return "lsprotect";
                    //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsprotect.png"));
                    case "Pinned":
                        return "lspin";
                    //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lspin.png"));
                    case "Favorites":
                        return "lsfav";
                    //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsfav.png"));
                    case "Encrypted":
                        return "lsscrambled";
                    //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsscrambled.png"));
                }
            }
            else if (value is SendReceiveStatus)
            {
                switch ((SendReceiveStatus)value)
                {
                    case SendReceiveStatus.Sent:
                        return "lssend";
                    //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lssend.png"));
                    case SendReceiveStatus.Received:
                        return "lsrec";
                    //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsrec.png"));
                    case SendReceiveStatus.Both:
                        return "lssendrec";
                    //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lssendrec.png"));
                }
            }
            else if (value is PasswordProtectionMode)
            {
                switch ((PasswordProtectionMode)value)
                {
                    case PasswordProtectionMode.Note:
                        return "lspass";
                    //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lspass.png"));
                    case PasswordProtectionMode.Group:
                        return "lspassgroup";
                    //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lspassgroup.png"));
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class NoteStateToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is NoteState)) return null;
            switch ((NoteState)value)
            {
                case NoteState.Deleted:
                    return "lsdel";
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsdel.png"));
                case NoteState.New:
                    return "lsvnew";
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvnew.png"));
                case NoteState.NewChanged:
                    return "lsvnew_change";
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvnew_change.png"));
                case NoteState.NewChangedScheduled:
                    return "lsvnew_change_sch";
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvnew_change_sch.png"));
                case NoteState.NewScheduled:
                    return "lsvnew_sch";
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvnew_sch.png"));
                case NoteState.OrdinalHidden:
                    return "lshord";
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lshord.png"));
                case NoteState.OrdinalHiddenChanged:
                    return "lshord_change";
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lshord_change.png"));
                case NoteState.OrdinalHiddenChangedScheduled:
                    return "lshord_change_sch";
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lshord_change_sch.png"));
                case NoteState.OrdinalHiddenScheduled:
                    return "lshord_sch";
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lshord_sch.png"));
                case NoteState.OrdinalVisible:
                    return "lsvord";
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvord.png"));
                case NoteState.OrdinalVisibleChanged:
                    return "lsvord_change";
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvord_change.png"));
                case NoteState.OrdinalVisibleChangedScheduled:
                    return "lsvord_change_sch";
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvord_change_sch.png"));
                case NoteState.OrdinalVisibleScheduled:
                    return "lsvord_sch";
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvord_sch.png"));
                case NoteState.Backup:
                    return "lsback";
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsback.png"));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class EnabledToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool)) return null;
            var fe = new FrameworkElement();
            return (bool)value ? fe.TryFindResource("NormalTextBrush") : fe.TryFindResource("DisabledTextBrush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class EnabledToSmallButtonColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool)) return null;
            var fe = new FrameworkElement();
            return (bool)value ? fe.TryFindResource("SmallButtonBrush") : fe.TryFindResource("DisabledTextBrush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class AutoHideToAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return PNStatic.Settings.Behavior.PanelAutoHide ? 0 : -90;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class OrientationToAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return PNStatic.Settings.Behavior.NotesPanelOrientation == NotesPanelOrientation.Left ? 0 : -90;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class TooltipForImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string)) return "";
            switch ((string) value)
            {
                case "imgAutoHide":
                    return PNStatic.Settings.Behavior.PanelAutoHide
                        ? PNLang.Instance.GetMiscText("auto_hide_remove", "Fix position")
                        : PNLang.Instance.GetMiscText("auto_hide_set", "Hide automatically");
                case "imgOrientation":
                    switch (PNStatic.Settings.Behavior.NotesPanelOrientation)
                    {
                        case NotesPanelOrientation.Top:
                            return PNLang.Instance.GetMiscText("orient_left", "On left");
                        case NotesPanelOrientation.Left:
                            return PNLang.Instance.GetMiscText("orient_top", "On top");
                    }
                    break;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class ConnectionStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fe = new FrameworkElement();
            if (!(value is ContactConnection))
                return fe.TryFindResource("ContDisconnected");
            switch ((ContactConnection)value)
            {
                case ContactConnection.Connected:
                    return fe.TryFindResource("ContConnected");
                default:
                    return fe.TryFindResource("ContDisconnected");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
