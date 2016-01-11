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

using PNotes.NET.Annotations;
using PNStaticFonts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using FontWeight = System.Windows.FontWeight;

namespace PNotes.NET
{
    [Serializable]
    internal class PNSyncComp
    {
        private string _CompName = "";
        private string _DataDir = "";
        private string _DbDir = "";
        private bool _UseDataDir = true;

        internal bool UseDataDir
        {
            get { return _UseDataDir; }
            set { _UseDataDir = value; }
        }
        internal string DBDir
        {
            get { return _DbDir; }
            set { _DbDir = value; }
        }
        internal string DataDir
        {
            get { return _DataDir; }
            set { _DataDir = value; }
        }
        internal string CompName
        {
            get { return _CompName; }
            set { _CompName = value; }
        }
    }

    [Serializable]
    internal class PNContact
    {
        private string _Name = "";
        private string _ComputerName = "";
        private string _IpAddress = "";
        private bool _UseComputerName = true;
        private int _GroupID = -1;
        private int _ID;

        internal int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }
        internal int GroupID
        {
            get { return _GroupID; }
            set { _GroupID = value; }
        }
        internal bool UseComputerName
        {
            get { return _UseComputerName; }
            set { _UseComputerName = value; }
        }
        internal string IpAddress
        {
            get { return _IpAddress; }
            set { _IpAddress = value; }
        }
        internal string ComputerName
        {
            get { return _ComputerName; }
            set { _ComputerName = value; }
        }
        internal string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public override string ToString()
        {
            return _Name;
        }
    }

    [Serializable]
    internal class PNContactGroup
    {
        private string _Name = "";
        private int _ID;

        internal int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }
        internal string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public override string ToString()
        {
            return _Name;
        }
    }

    [Serializable]
    internal class PNSmtpProfile
    {
        internal int Id { get; set; }
        internal bool Active { get; set; }
        internal string HostName { get; set; }
        internal string SenderAddress { get; set; }
        internal string Password { get; set; }
        internal int Port { get; set; }
        internal string DisplayName { get; set; }
    }

    [Serializable]
    internal class PNMailContact
    {
        internal int Id { get; set; }
        internal string DisplayName { get; set; }
        internal string Address { get; set; }
    }

    [Serializable]
    internal class PNSearchProvider
    {
        private string _Name = "";
        private string _QueryString = "";

        internal string QueryString
        {
            get { return _QueryString; }
            set { _QueryString = value; }
        }
        internal string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
    }

    [Serializable]
    internal class PNExternal
    {
        private string _Name = "";
        private string _Program = "";
        private string _CommandLine = "";

        internal string CommandLine
        {
            get { return _CommandLine; }
            set { _CommandLine = value; }
        }
        internal string Program
        {
            get { return _Program; }
            set { _Program = value; }
        }
        internal string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
    }

    [Serializable]
    internal class PNHotKey : ICloneable
    {
        private string _MenuName = "";
        private int _ID;
        private HotkeyModifiers _Modifiers = 0;
        private uint _vk;
        private string _Shortcut = "";
        private HotkeyType _Type = HotkeyType.Main;

        internal HotkeyType Type
        {
            get { return _Type; }
            set { _Type = value; }
        }
        internal string Shortcut
        {
            get { return _Shortcut; }
            set { _Shortcut = value; }
        }
        internal uint VK
        {
            get { return _vk; }
            set { _vk = value; }
        }
        internal HotkeyModifiers Modifiers
        {
            get { return _Modifiers; }
            set { _Modifiers = value; }
        }
        internal int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }
        internal string MenuName
        {
            get { return _MenuName; }
            set { _MenuName = value; }
        }
        internal void CopyFrom(PNHotKey hk)
        {
            _Modifiers = hk.Modifiers;
            _Shortcut = hk._Shortcut;
            _vk = hk._vk;
        }
        internal void Clear()
        {
            _Modifiers = HotkeyModifiers.MOD_NONE;
            _Shortcut = "";
            _vk = 0;
        }
        public override bool Equals(object obj)
        {
            var hk = obj as PNHotKey;
            if ((object) hk == null)
                return false;
            return (_vk == hk._vk
                && _Shortcut == hk._Shortcut
                && _Modifiers == hk._Modifiers
                && _MenuName == hk._MenuName
                && _ID == hk._ID
                && _Type == hk._Type);
        }

        public bool Equals(PNHotKey hk)
        {
            if ((object)hk == null)
                return false;
            return (_vk == hk._vk
                && _Shortcut == hk._Shortcut
                && _Modifiers == hk._Modifiers
                && _MenuName == hk._MenuName
                && _ID == hk._ID
                && _Type == hk._Type);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += ID.GetHashCode();
            result *= 37;
            result += MenuName.GetHashCode();
            result *= 37;
            result += Modifiers.GetHashCode();
            result *= 37;
            result += Shortcut.GetHashCode();
            result *= 37;
            result += VK.GetHashCode();
            result *= 37;
            result += Type.GetHashCode();
            return result;
        }

        public static bool operator ==(PNHotKey a, PNHotKey b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return (a._vk == b._vk
                && a._Shortcut == b._Shortcut
                && a._Modifiers == b._Modifiers
                && a._MenuName == b._MenuName
                && a._ID == b._ID
                && a._Type == b._Type);
        }

        public static bool operator !=(PNHotKey a, PNHotKey b)
        {
            return (!(a == b));
        }

        #region ICloneable Members

        public object Clone()
        {
            var hk = new PNHotKey
            {
                _ID = _ID,
                _MenuName = _MenuName,
                _Modifiers = _Modifiers,
                _Shortcut = _Shortcut,
                _vk = _vk,
                _Type = _Type
            };
            return hk;
        }

        #endregion
    }

    [Serializable]
    internal class PNMenu
    {
        public string Name { get; private set; }
        public string Text { get; private set; }
        public string ParentName { get; private set; }
        public List<PNMenu> Items { get; private set; }
        public string ContextName { get; private set; }

        internal PNMenu(string name, string text, string parentName, string context)
        {
            Name = name;
            Text = text;
            ParentName = parentName;
            ContextName = context;
            Items = new List<PNMenu>();
        }
    }

    [Serializable]
    internal class PNHiddenMenu
    {
        public string Name { get; private set; }
        public MenuType Type { get; private set; }

        internal PNHiddenMenu(string name, MenuType type)
        {
            Name = name;
            Type = type;
        }
    }

    [Serializable]
    internal class AlarmAfterValues
    {
        private int _Years;
        private int _Months;
        private int _Weeks;
        private int _Days;
        private int _Hours;
        private int _Minutes;
        private int _Seconds;

        internal int Seconds
        {
            get { return _Seconds; }
            set { _Seconds = value; }
        }
        internal int Minutes
        {
            get { return _Minutes; }
            set { _Minutes = value; }
        }
        internal int Hours
        {
            get { return _Hours; }
            set { _Hours = value; }
        }
        internal int Days
        {
            get { return _Days; }
            set { _Days = value; }
        }
        internal int Weeks
        {
            get { return _Weeks; }
            set { _Weeks = value; }
        }
        internal int Months
        {
            get { return _Months; }
            set { _Months = value; }
        }
        internal int Years
        {
            get { return _Years; }
            set { _Years = value; }
        }
        internal long TotalSeconds
        {
            get
            {
                return (_Years * TimeSpan.TicksPerDay * 365) / TimeSpan.TicksPerSecond
                   + (_Months * TimeSpan.TicksPerDay * 30) / TimeSpan.TicksPerSecond
                   + (_Weeks * TimeSpan.TicksPerDay * 7) / TimeSpan.TicksPerSecond
                   + (_Days * TimeSpan.TicksPerDay) / TimeSpan.TicksPerSecond
                   + (_Hours * TimeSpan.TicksPerHour) / TimeSpan.TicksPerSecond
                   + (_Minutes * TimeSpan.TicksPerMinute) / TimeSpan.TicksPerSecond
                   + _Seconds;
            }
        }

        public override bool Equals(object obj)
        {
            var aa = obj as AlarmAfterValues;
            if ((object) aa == null)
                return false;
            return (TotalSeconds == aa.TotalSeconds);
        }

        public bool Equals(AlarmAfterValues aa)
        {
            if ((object)aa == null)
                return false;
            return (TotalSeconds == aa.TotalSeconds);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += TotalSeconds.GetHashCode();
            return result;
        }

        public static bool operator ==(AlarmAfterValues a, AlarmAfterValues b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return (a.TotalSeconds == b.TotalSeconds);
        }

        public static bool operator !=(AlarmAfterValues a, AlarmAfterValues b)
        {
            return (!(a == b));
        }
    }

    [Serializable]
    internal class MonthDay
    {
        private DayOfWeek _WeekDay = new DateTimeFormatInfo().FirstDayOfWeek;
        private DayOrdinal _OrdinalNumber = DayOrdinal.First;

        internal DayOfWeek WeekDay
        {
            get { return _WeekDay; }
            set { _WeekDay = value; }
        }
        internal DayOrdinal OrdinalNumber
        {
            get { return _OrdinalNumber; }
            set { _OrdinalNumber = value; }
        }

        public override bool Equals(object obj)
        {
            var md = obj as MonthDay;
            if ((object) md == null)
                return false;
            return (_OrdinalNumber == md._OrdinalNumber
                && _WeekDay == md._WeekDay);
        }

        public bool Equals(MonthDay md)
        {
            if ((object)md == null)
                return false;
            return (_OrdinalNumber == md._OrdinalNumber
                && _WeekDay == md._WeekDay);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += OrdinalNumber.GetHashCode();
            result *= 37;
            result += WeekDay.GetHashCode();
            return result;
        }

        public static bool operator ==(MonthDay a, MonthDay b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return (a._OrdinalNumber == b._OrdinalNumber
                && a._WeekDay == b._WeekDay);
        }

        public static bool operator !=(MonthDay a, MonthDay b)
        {
            return (!(a == b));
        }
    }

    [Serializable]
    internal class PNNoteSchedule : ICloneable
    {
        private ScheduleType _Type = ScheduleType.None;
        private DateTime _AlarmDate;
        private DateTime _StartDate;
        private DateTime _LastRun = DateTime.MinValue;
        private string _Sound = PNSchedule.DEF_SOUND;
        private int _StopAfter = -1;
        private bool _Track = true;
        private int _RepeatCount;
        private bool _SoundInLoop = true;
        private bool _UseTts;
        private ScheduleStart _StartFrom = ScheduleStart.ExactTime;
        private MonthDay _MonthDay = new MonthDay();
        private AlarmAfterValues _AlarmAfter = new AlarmAfterValues();
        private List<DayOfWeek> _Weekdays = new List<DayOfWeek>();
        private List<MultiAlert> _MultiAlerts = new List<MultiAlert>();
        private TimeZoneInfo _TimeZone = TimeZoneInfo.Local;

        internal TimeZoneInfo TimeZone
        {
            get { return _TimeZone; }
            set { _TimeZone = value; }
        }
        internal List<MultiAlert> MultiAlerts
        {
            get { return _MultiAlerts; }
            set { _MultiAlerts = value; }
        }
        internal string ProgramToRunOnAlert { get; set; }
        internal bool CloseOnNotification { get; set; }

        internal List<DayOfWeek> Weekdays
        {
            get { return _Weekdays; }
            set { _Weekdays = value; }
        }
        internal AlarmAfterValues AlarmAfter
        {
            get { return _AlarmAfter; }
            set { _AlarmAfter = value; }
        }
        internal MonthDay MonthDay
        {
            get { return _MonthDay; }
            set { _MonthDay = value; }
        }
        internal ScheduleStart StartFrom
        {
            get { return _StartFrom; }
            set { _StartFrom = value; }
        }
        internal bool UseTts
        {
            get { return _UseTts; }
            set { _UseTts = value; }
        }
        internal bool SoundInLoop
        {
            get { return _SoundInLoop; }
            set { _SoundInLoop = value; }
        }
        internal int RepeatCount
        {
            get { return _RepeatCount; }
            set { _RepeatCount = value; }
        }
        internal bool Track
        {
            get { return _Track; }
            set { _Track = value; }
        }
        internal int StopAfter
        {
            get { return _StopAfter; }
            set { _StopAfter = value; }
        }
        internal string Sound
        {
            get { return _Sound; }
            set { _Sound = value; }
        }
        internal DateTime LastRun
        {
            get { return _LastRun; }
            set { _LastRun = value; }
        }
        internal DateTime StartDate
        {
            get { return _StartDate; }
            set { _StartDate = value; }
        }
        internal DateTime AlarmDate
        {
            get { return _AlarmDate; }
            set { _AlarmDate = value; }
        }
        internal ScheduleType Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        public override bool Equals(object obj)
        {
            var sc = obj as PNNoteSchedule;
            if ((object) sc == null)
                return false;
            return (_Type == sc._Type
                && _AlarmDate.IsDateEqual(sc._AlarmDate)
                && _Sound == sc._Sound
                && _StopAfter == sc._StopAfter
                && _Track == sc._Track
                && _SoundInLoop == sc._SoundInLoop
                && _RepeatCount == sc._RepeatCount
                && _UseTts == sc._UseTts
                && _StartFrom == sc._StartFrom
                && _MonthDay == sc._MonthDay
                && _AlarmAfter == sc._AlarmAfter
                && !_Weekdays.Inequals(sc._Weekdays)
                && ProgramToRunOnAlert == sc.ProgramToRunOnAlert
                && CloseOnNotification == sc.CloseOnNotification
                && _MultiAlerts.IsEqual(sc._MultiAlerts)
                && _TimeZone == sc._TimeZone);
        }

        public bool Equals(PNNoteSchedule sc)
        {
            if ((object)sc == null)
                return false;
            return (_Type == sc._Type
                && _AlarmDate.IsDateEqual(sc._AlarmDate)
                && _Sound == sc._Sound
                && _StopAfter == sc._StopAfter
                && _Track == sc._Track
                && _SoundInLoop == sc._SoundInLoop
                && _RepeatCount == sc._RepeatCount
                && _UseTts == sc._UseTts
                && _StartFrom == sc._StartFrom
                && _MonthDay == sc._MonthDay
                && _AlarmAfter == sc._AlarmAfter
                && !_Weekdays.Inequals(sc._Weekdays)
                && ProgramToRunOnAlert == sc.ProgramToRunOnAlert
                && CloseOnNotification == sc.CloseOnNotification
                && _MultiAlerts.IsEqual(sc._MultiAlerts)
                && _TimeZone == sc._TimeZone);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += Type.GetHashCode();
            result *= 37;
            result += AlarmDate.GetHashCode();
            result *= 37;
            result += Sound.GetHashCode();
            result *= 37;
            result += StopAfter.GetHashCode();
            result *= 37;
            result += Track.GetHashCode();
            result *= 37;
            result += SoundInLoop.GetHashCode();
            result *= 37;
            result += RepeatCount.GetHashCode();
            result *= 37;
            result += UseTts.GetHashCode();
            result *= 37;
            result += StartFrom.GetHashCode();
            result *= 37;
            result += MonthDay.GetHashCode();
            result *= 37;
            result += Weekdays.GetHashCode();
            result *= 37;
            result += ProgramToRunOnAlert.GetHashCode();
            result *= 37;
            result += CloseOnNotification.GetHashCode();
            result *= 37;
            result += MultiAlerts.GetHashCode();
            result *= 37;
            result += TimeZone.GetHashCode();
            return result;
        }

        public static bool operator ==(PNNoteSchedule a, PNNoteSchedule b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return (a._Type == b._Type
                && a._AlarmDate.IsDateEqual(b._AlarmDate)
                && a._Sound == b._Sound
                && a._StopAfter == b._StopAfter
                && a._Track == b._Track
                && a._SoundInLoop == b._SoundInLoop
                && a._RepeatCount == b._RepeatCount
                && a._UseTts == b._UseTts
                && a._StartFrom == b._StartFrom
                && a._MonthDay == b._MonthDay
                && a._AlarmAfter == b._AlarmAfter
                && !a._Weekdays.Inequals(b._Weekdays)
                && a.ProgramToRunOnAlert == b.ProgramToRunOnAlert
                && a.CloseOnNotification == b.CloseOnNotification
                && a._MultiAlerts.IsEqual(b._MultiAlerts)
                && a._TimeZone == b._TimeZone);
        }

        public static bool operator !=(PNNoteSchedule a, PNNoteSchedule b)
        {
            return (!(a == b));
        }

        #region ICloneable Members

        public object Clone()
        {
            PNNoteSchedule sd = this.PNClone();
            sd._LastRun = DateTime.MinValue;
            return sd;
        }

        #endregion
    }

    [Serializable]
    internal class MultiAlert
    {
        internal DateTime Date { get; set; }
        internal bool Raised { get; set; }
        internal bool Checked { get; set; }
    }

    [Serializable]
    internal class PNGroup : ICloneable, IDisposable
    {
        internal event EventHandler<GroupPropertyChangedEventArgs> GroupPropertyChanged;

        private int _Id = -1;
        private int _ParentID = -1;
        private string _Name = "";
        private string _PasswordString = "";
        private List<PNGroup> _Subgroups = new List<PNGroup>();
        private PNSkinDetails _Skin = new PNSkinDetails();
        private PNSkinlessDetails _Skinless = new PNSkinlessDetails();
        private LOGFONT _Font;
        private Color _FontColor = PNStatic.DefaultFontColor;

        internal PNGroup()
        {
            //in order to register 'application' and 'pack' schemes if they are not registered yet
            var c = Application.Current;
            Image = new FrameworkElement().TryFindResource("gr") as BitmapImage;
            _Font.Init();
            _Font.lfFaceName = PNStrings.DEFAULT_FONT_NAME;
        }

        internal string ImageName
        {
            get
            {
                var arr = Image.ToString().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                return arr.Length > 0 ? Path.GetFileNameWithoutExtension(arr[arr.Length - 1]) : "";
            }
        }

        internal BitmapImage Image { get; set; }

        internal List<PNGroup> Subgroups
        {
            get { return _Subgroups; }
            set { _Subgroups = value; }
        }
        internal PNSkinDetails Skin
        {
            get { return _Skin; }
            set { _Skin = value; }
        }
        internal PNSkinlessDetails Skinless
        {
            get { return _Skinless; }
            set { _Skinless = value; }
        }
        internal LOGFONT Font
        {
            get { return _Font; }
            set { _Font = value; }
        }
        internal Color FontColor
        {
            get { return _FontColor; }
            set { _FontColor = value; }
        }
        internal string PasswordString
        {
            get { return _PasswordString; }
            set { _PasswordString = value; }
        }
        internal int ParentID
        {
            get { return _ParentID; }
            set { _ParentID = value; }
        }
        internal string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        internal int ID
        {
            get { return _Id; }
            set { _Id = value; }
        }

        internal bool IsDefaultImage { get; set; }

        internal PNGroup GetGroupById(int id)
        {
            return _Id == id ? this : subGroupByID(this, id);
        }

        private PNGroup subGroupByID(PNGroup parent, int id)
        {
            foreach (var g in parent._Subgroups)
            {
                if (g._Id == id)
                {
                    return g;
                }
                var result = subGroupByID(g, id);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        internal void Clear()
        {
            _Skin.Dispose();
            _Skin = new PNSkinDetails();
            _Skinless = new PNSkinlessDetails();
            var lf = new LOGFONT();
            lf.Init();
            lf.SetFontFace(PNStrings.DEFAULT_FONT_NAME);
            lf.SetFontSize(PNStatic.DEFAULT_FONTSIZE);
            _Font = lf;
            _FontColor = PNStatic.DefaultFontColor;
        }

        internal void CopyTo(PNGroup pg)
        {
            var changed = (pg._Skinless != _Skinless);
            var changedBackColor = (pg._Skinless.BackColor != _Skinless.BackColor);
            var changedCaptionFont = (!pg._Skinless.CaptionFont.Equals(_Skinless.CaptionFont));
            var changedCaptionColor = (pg._Skinless.CaptionColor != _Skinless.CaptionColor);

            pg._Skinless = (PNSkinlessDetails)_Skinless.Clone();
            if (changed && pg.GroupPropertyChanged != null)
            {
                if (changedBackColor)
                {
                    pg.GroupPropertyChanged(pg, new GroupPropertyChangedEventArgs(pg._Skinless.BackColor, GroupChangeType.BackColor));
                }
                if (changedCaptionFont)
                {
                    pg.GroupPropertyChanged(pg, new GroupPropertyChangedEventArgs(pg._Skinless.CaptionFont, GroupChangeType.CaptionFont));
                }
                if (changedCaptionColor)
                {
                    pg.GroupPropertyChanged(pg, new GroupPropertyChangedEventArgs(pg._Skinless.CaptionColor, GroupChangeType.CaptionColor));
                }
            }
            changed = (pg._Skin.SkinName != _Skin.SkinName);
            pg._Skin.Dispose();
            pg._Skin = _Skin.PNClone();
            if (changed && pg.GroupPropertyChanged != null)
            {
                pg.GroupPropertyChanged(pg, new GroupPropertyChangedEventArgs(pg._Skin.SkinName, GroupChangeType.Skin));
            }
            changed = (!pg._Font.Equals(_Font));
            pg._Font = _Font;
            if (changed && pg.GroupPropertyChanged != null)
            {
                pg.GroupPropertyChanged(pg, new GroupPropertyChangedEventArgs(pg._Font, GroupChangeType.Font));
            }
            changed = (pg._FontColor != _FontColor);
            pg._FontColor = _FontColor;
            if (changed && pg.GroupPropertyChanged != null)
            {
                pg.GroupPropertyChanged(pg, new GroupPropertyChangedEventArgs(pg._FontColor, GroupChangeType.FontColor));
            }
            changed = (!pg.Image.Equals(Image));
            pg.Image = Image.Clone();
            pg.IsDefaultImage = IsDefaultImage;
            if (changed && pg.GroupPropertyChanged != null)
            {
                pg.GroupPropertyChanged(pg, new GroupPropertyChangedEventArgs(pg.Image, GroupChangeType.Image));
            }
        }

        public override string ToString()
        {
            return _Name;
        }

        public override bool Equals(object obj)
        {
            var gr = obj as PNGroup;
            if ((object) gr == null)
                return false;
            return (_Skin == gr._Skin
                && _Skinless == gr._Skinless
                && _PasswordString == gr._PasswordString
                && _FontColor == gr._FontColor
                && _Font == gr._Font
                && Image.Equals(gr.Image)
                && IsDefaultImage == gr.IsDefaultImage);
        }

        public bool Equals(PNGroup gr)
        {
            if ((object)gr == null)
                return false;
            return (_Skin == gr._Skin
                && _Skinless == gr._Skinless
                && _PasswordString == gr._PasswordString
                && _FontColor == gr._FontColor
                && _Font == gr._Font
                && Image.Equals(gr.Image)
                && IsDefaultImage == gr.IsDefaultImage);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += ID.GetHashCode();
            result *= 37;
            result += Name.GetHashCode();
            result *= 37;
            result += Skinless.GetHashCode();
            result *= 37;
            result += Skin.GetHashCode();
            result *= 37;
            result += Image.GetHashCode();
            result *= 37;
            result += PasswordString.GetHashCode();
            result *= 37;
            result += Font.GetHashCode();
            result *= 37;
            result += FontColor.GetHashCode();
            result *= 37;
            result += IsDefaultImage.GetHashCode();

            return result;
        }

        public static bool operator ==(PNGroup a, PNGroup b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return (a.Image.IsEqual(b.Image)
                && a._Skin == b._Skin
                && a._Skinless == b._Skinless
                && a._PasswordString == b._PasswordString
                && a._FontColor == b._FontColor
                && a._Font == b._Font
                && a.IsDefaultImage == b.IsDefaultImage);
        }

        public static bool operator !=(PNGroup a, PNGroup b)
        {
            return (!(a == b));
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (PNGroup g in _Subgroups)
            {
                g.Dispose();
            }
            if (_Skin != null)
            {
                _Skin.Dispose();
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            var g = new PNGroup
            {
                _Skinless = (PNSkinlessDetails)_Skinless.Clone(),
                _Skin = _Skin.PNClone(),
                _PasswordString = _PasswordString,
                _ParentID = _ParentID,
                _Name = _Name,
                _Id = _Id,
                _FontColor = _FontColor,
                Image = Image.Clone(),
                IsDefaultImage = IsDefaultImage,
                _Font = _Font
            };
            return g;
        }

        #endregion
    }

    internal class SearchNotesPrefs
    {
        internal bool WholewWord { get; set; }
        internal bool MatchCase { get; set; }
        internal bool IncludeHidden { get; set; }
        internal int Criteria { get; set; }
        internal int Scope { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(WholewWord);
            sb.Append("|");
            sb.Append(MatchCase);
            sb.Append("|");
            sb.Append(IncludeHidden);
            sb.Append("|");
            sb.Append(Criteria);
            sb.Append("|");
            sb.Append(Scope);
            return sb.ToString();
        }
    }

    internal class GRDSort
    {
        private ListSortDirection _SortOrder = ListSortDirection.Ascending;

        internal ListSortDirection SortOrder
        {
            get { return _SortOrder; }
            set { _SortOrder = value; }
        }

        internal bool LastSorted { get; set; }
        internal string Key { get; set; }
    }

    public class ThemesUpdate
    {
        internal string FriendlyName { get; set; }
        internal string Name { get; set; }
        internal string FileName { get; set; }
        internal string Suffix { get; set; }
        public override string ToString()
        {
            return FriendlyName + " " + Suffix;
        }
    }

    public class PluginsUpdate
    {
        internal string Name { get; set; }
        internal string ProductName { get; set; }
        internal string Suffix { get; set; }
        internal int Type { get; set; }

        public override string ToString()
        {
            return Name + " " + Suffix;
        }
    }

    internal class CriticalPluginUpdate
    {
        internal string ProductName { get; set; }
        internal string FileName { get; set; }
    }

    internal class DictData
    {
        internal string LangName { get; set; }
        internal string ZipFile { get; set; }
    }

    public class SplashTextProvider : INotifyPropertyChanged
    {
        private string _SplashText;
        public event PropertyChangedEventHandler PropertyChanged;

        public string SplashText
        {
            get { return _SplashText; }
            set
            {
                if (_SplashText == value) return;
                _SplashText = value;
                OnSplashTextChanged("SplashText");
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnSplashTextChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Serializable]
    public class PNFont : INotifyPropertyChanged
    {
        private FontFamily _FontFamily = new FontFamily(PNStrings.DEF_CAPTION_FONT);
        private double _FontSize = 12;
        private FontStyle _FontStyle = FontStyles.Normal;
        private FontWeight _FontWeight = FontWeights.Normal;
        private FontStretch _FontStretch = FontStretches.Normal;

        public FontFamily FontFamily
        {
            get { return _FontFamily; }
            set
            {
                if (Equals(_FontFamily, value)) return;
                _FontFamily = value;
                OnPropertyChanged("FontFamily");
            }
        }

        public double FontSize
        {
            get { return _FontSize; }
            set
            {
                if (!(Math.Abs(_FontSize - value) > double.Epsilon)) return;
                _FontSize = value;
                OnPropertyChanged("FontSize");
            }
        }

        public FontStyle FontStyle
        {
            get { return _FontStyle; }
            set
            {
                if (Equals(_FontStyle, value)) return;
                _FontStyle = value;
                OnPropertyChanged("FontStyle");
            }
        }

        public FontWeight FontWeight
        {
            get { return _FontWeight; }
            set
            {
                if (Equals(_FontWeight, value)) return;
                _FontWeight = value;
                OnPropertyChanged("FontWeight");
            }
        }

        public FontStretch FontStretch
        {
            get { return _FontStretch; }
            set
            {
                if (Equals(_FontStretch, value)) return;
                _FontStretch = value;
                OnPropertyChanged("FontStretch");
            }
        }

        public override bool Equals(object obj)
        {
            var pf = obj as PNFont;
            if (pf == null)
                return false;
            return (Equals(_FontFamily, pf._FontFamily) && Math.Abs(_FontSize - pf._FontSize) < double.Epsilon && _FontStretch == pf._FontStretch &&
                    _FontStyle == pf._FontStyle && _FontWeight == pf._FontWeight);
        }

        public bool Equals(PNFont pf)
        {
            if (pf == null)
                return false;
            return (Equals(_FontFamily, pf._FontFamily) && Math.Abs(_FontSize - pf._FontSize) < double.Epsilon && _FontStretch == pf._FontStretch &&
                    _FontStyle == pf._FontStyle && _FontWeight == pf._FontWeight);
        }

        public override int GetHashCode()
        {
            var result = 17;
            result *= 37;
            result += FontFamily.GetHashCode();
            result *= 37;
            result += FontSize.GetHashCode();
            result *= 37;
            result += FontStretch.GetHashCode();
            result *= 37;
            result += FontStyle.GetHashCode();
            result *= 37;
            result += FontWeight.GetHashCode();

            return result;
        }

        public override string ToString()
        {
            return _FontFamily.ToString();
        }

        public static bool operator ==(PNFont a, PNFont b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return (Equals(a.FontFamily, b.FontFamily) && Math.Abs(a.FontSize - b.FontSize) < double.Epsilon && a.FontStretch == b.FontStretch &&
                    a.FontStyle == b.FontStyle && a.FontWeight == b.FontWeight);
        }

        public static bool operator !=(PNFont a, PNFont b)
        {
            return (!(a == b));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PNListBoxItem : ListBoxItem
    {
        internal event EventHandler<ListBoxItemCheckChangedEventArgs> ListBoxItemCheckChanged;

        private readonly CheckBox _CheckBox = new CheckBox
        {
            Margin = new Thickness(2),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Visibility = Visibility.Collapsed,
            Focusable = false
        };

        public PNListBoxItem(ImageSource imageSource, string text, object tag, string key, bool isChecked)
        {
            Image = imageSource;
            _CheckBox.IsChecked = isChecked;
            _CheckBox.Checked += chk_Checked;
            _CheckBox.Unchecked += chk_Unchecked;
            _CheckBox.PreviewMouseLeftButtonDown += chk_PreviewMouseLeftButtonDown;
            _CheckBox.Visibility = Visibility.Visible;
            var img = new Image
            {
                Source = imageSource,
                Stretch = Stretch.None,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var tb = new TextBlock
            {
                Text = text,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var st = new StackPanel { Orientation = Orientation.Horizontal };
            st.Children.Add(_CheckBox);
            st.Children.Add(img);
            st.Children.Add(tb);
            Key = key;
            IsChecked = isChecked;
            Tag = tag;
            Text = text;
            Content = st;
        }



        public PNListBoxItem(ImageSource imageSource, string text, object tag, bool isChecked)
            : this(imageSource, text, tag, "", isChecked)
        {
        }

        public PNListBoxItem(ImageSource imageSource, string text, object tag, string key)
            : this(imageSource, text, tag)
        {
            Key = key;
        }

        public PNListBoxItem(ImageSource imageSource, string text, object tag)
        {
            Image = imageSource;
            var img = new Image
            {
                Source = imageSource,
                Stretch = Stretch.None,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var tb = new TextBlock
            {
                Text = text,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var st = new StackPanel { Orientation = Orientation.Horizontal };
            st.Children.Add(img);
            st.Children.Add(tb);
            Tag = tag;
            Text = text;
            Content = st;
        }

        public int Index
        {
            get
            {
                var parent = ItemsControl.ItemsControlFromItemContainer(this);
                var listBox = parent as ListBox;
                if (listBox != null)
                {
                    return listBox.Items.IndexOf(this);
                }
                return -1;
            }
        }

        public ImageSource Image { get; private set; }

        public string Key { get; private set; }

        public bool? IsChecked
        {
            get { return _CheckBox.IsChecked; }
            set { _CheckBox.IsChecked = value; }
        }

        public string Text { get; private set; }

        private void chk_Unchecked(object sender, RoutedEventArgs e)
        {
            IsChecked = false;
            if (ListBoxItemCheckChanged != null)
                ListBoxItemCheckChanged(this, new ListBoxItemCheckChangedEventArgs(false));
        }

        private void chk_Checked(object sender, RoutedEventArgs e)
        {
            IsChecked = true;
            if (ListBoxItemCheckChanged != null)
                ListBoxItemCheckChanged(this, new ListBoxItemCheckChangedEventArgs(true));
        }

        private void chk_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsSelected = true;
            Focus();
        }
    }

    public class PNTreeItem : TreeViewItem
    {
        internal event EventHandler<TreeViewItemCheckChangedEventArgs> TreeViewItemCheckChanged;

        private readonly CheckBox _CheckBox = new CheckBox
        {
            Margin = new Thickness(2),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Visibility = Visibility.Collapsed,
            Focusable = false
        };

        private readonly Image _Image;
        private readonly TextBlock _TextBlock;

        public PNTreeItem(ImageSource imageSource, string text, object tag, string key, bool isChecked)
        {
            _CheckBox.IsChecked = isChecked;
            _CheckBox.Checked += chk_Checked;
            _CheckBox.Unchecked += chk_Unchecked;
            _CheckBox.PreviewMouseLeftButtonDown += _CheckBox_PreviewMouseLeftButtonDown;
            _CheckBox.Visibility = Visibility.Visible;
            _Image = new Image
            {
                Source = imageSource,
                Stretch = Stretch.None,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _TextBlock = new TextBlock
            {
                Text = text,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var st = new StackPanel { Orientation = Orientation.Horizontal };
            st.Children.Add(_CheckBox);
            st.Children.Add(_Image);
            st.Children.Add(_TextBlock);
            Key = key;
            IsChecked = isChecked;
            Tag = tag;
            Header = st;
        }


        public PNTreeItem(ImageSource imageSource, string text, object tag, bool isChecked)
            : this(imageSource, text, tag, "", isChecked)
        {
        }

        public PNTreeItem(string imageResource, string text, object tag, bool isChecked)
            : this(imageResource, text, tag, "", isChecked)
        {
        }

        public PNTreeItem(string imageResource, string text, object tag, string key, bool isChecked)
        {
            _CheckBox.IsChecked = isChecked;
            _CheckBox.Checked += chk_Checked;
            _CheckBox.Unchecked += chk_Unchecked;
            _CheckBox.PreviewMouseLeftButtonDown += _CheckBox_PreviewMouseLeftButtonDown;
            _CheckBox.Visibility = Visibility.Visible;
            _Image = new Image
            {
                Stretch = Stretch.None,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _Image.SetResourceReference(System.Windows.Controls.Image.SourceProperty, imageResource);
            PNUtils.SetIsResourceImage(this, true);
            _TextBlock = new TextBlock
            {
                Text = text,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var st = new StackPanel { Orientation = Orientation.Horizontal };
            st.Children.Add(_CheckBox);
            st.Children.Add(_Image);
            st.Children.Add(_TextBlock);
            Key = key;
            IsChecked = isChecked;
            Tag = tag;
            Header = st;
        }

        public PNTreeItem(ImageSource imageSource, string text, object tag, string key)
            : this(imageSource, text, tag)
        {
            Key = key;
        }

        public PNTreeItem(ImageSource imageSource, string text, object tag)
        {
            _Image = new Image
            {
                Source = imageSource,
                Stretch = Stretch.None,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _TextBlock = new TextBlock
            {
                Text = text,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var st = new StackPanel { Orientation = Orientation.Horizontal };
            st.Children.Add(_Image);
            st.Children.Add(_TextBlock);
            Tag = tag;
            Header = st;
        }

        public PNTreeItem(string imageResource, string text, object tag, string key)
            : this(imageResource, text, tag)
        {
            Key = key;
        }

        public PNTreeItem(string imageResource, string text, object tag)
        {
            _Image = new Image
            {
                Stretch = Stretch.None,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _Image.SetResourceReference(System.Windows.Controls.Image.SourceProperty, imageResource);
            PNUtils.SetIsResourceImage(this, true);
            _TextBlock = new TextBlock
            {
                Text = text,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var st = new StackPanel { Orientation = Orientation.Horizontal };
            st.Children.Add(_Image);
            st.Children.Add(_TextBlock);
            Tag = tag;
            Header = st;
        }

        public PNTreeItem Clone()
        {
            PNTreeItem pti;
            if (PNUtils.GetIsResourceImage(this))
            {
                var arr = _Image.Source.ToString().Split(new[] { @"/" }, StringSplitOptions.RemoveEmptyEntries);
                pti = new PNTreeItem(Path.GetFileNameWithoutExtension(arr[arr.Length - 1]), Text, Tag, Key,
                    IsChecked.HasValue && IsChecked.Value)
                {
                    IsExpanded = IsExpanded,
                    IsEnabled = IsEnabled,
                    IsSelected = IsSelected
                };
                PNUtils.SetIsResourceImage(pti, true);
            }
            else
            {
                pti = new PNTreeItem(Image, Text, Tag, Key, IsChecked.HasValue && IsChecked.Value)
                {
                    IsExpanded = IsExpanded,
                    IsEnabled = IsEnabled,
                    IsSelected = IsSelected
                };
            }

            foreach (var p in Items.OfType<PNTreeItem>())
            {
                pti.Items.Add(p.Clone());
            }
            return pti;
        }

        public void SetImageResource(string key)
        {
            _Image.SetResourceReference(System.Windows.Controls.Image.SourceProperty, key);
            PNUtils.SetIsResourceImage(this, true);
        }

        public ImageSource Image
        {
            get { return _Image.Source; }
            set { _Image.Source = value; }
        }

        public string Key { get; private set; }

        public bool? IsChecked
        {
            get { return _CheckBox.IsChecked; }
            set { _CheckBox.IsChecked = value; }
        }

        public string Text
        {
            get { return _TextBlock.Text; }
            set { _TextBlock.Text = value; }
        }

        public int Index
        {
            get
            {
                var parent = ItemsControlFromItemContainer(this);
                var item = parent as PNTreeItem;
                if (item != null)
                {
                    return item.Items.IndexOf(this);
                }
                var tree = parent as TreeView;
                if (tree != null)
                {
                    return tree.Items.IndexOf(this);
                }
                return -1;
            }
        }

        public PNTreeItem ItemParent
        {
            get
            {
                var parent = ItemsControlFromItemContainer(this);
                return parent as PNTreeItem;
            }
        }

        private void chk_Unchecked(object sender, RoutedEventArgs e)
        {
            IsChecked = false;
            var parent = ItemsControlFromItemContainer(this);
            while (!(parent is TreeView))
            {
                parent = ItemsControlFromItemContainer(parent);
            }
            var parentTreeviee = parent as TreeView;
            if (TreeViewItemCheckChanged != null)
                TreeViewItemCheckChanged(this, new TreeViewItemCheckChangedEventArgs(false, parentTreeviee));
        }

        private void chk_Checked(object sender, RoutedEventArgs e)
        {
            IsChecked = true;
            var parent = ItemsControlFromItemContainer(this);
            while (!(parent is TreeView))
            {
                parent = ItemsControlFromItemContainer(parent);
            }
            var parentTreeviee = parent as TreeView;
            if (TreeViewItemCheckChanged != null)
                TreeViewItemCheckChanged(this, new TreeViewItemCheckChangedEventArgs(true, parentTreeviee));
        }

        private void _CheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsSelected = true;
            Focus();
        }
    }

    public class PNTreeView : TreeView
    {
        public event EventHandler<MouseButtonEventArgs> PNTreeViewLeftMouseDoubleClick;

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (PNTreeViewLeftMouseDoubleClick != null) PNTreeViewLeftMouseDoubleClick(this, e);
                e.Handled = true;
                return;
            }
            base.OnPreviewMouseDoubleClick(e);
        }
    }

    internal class ColumnProps
    {
        internal string Name;
        internal Visibility Visibility;
        internal double Width;
    }

    internal struct TempColor
    {
        internal string Id;
        internal string Color;
    }

    [Serializable]
    internal struct DateBitField
    {
        private int _bits;

        internal DateBitField(int initValue)
        {
            _bits = initValue;
        }

        internal bool this[DatePart index]
        {
            get { return (_bits & (1 << (int)index)) != 0; }
            set
            {
                if (value)
                {
                    _bits |= (1 << (int)index);
                }
                else
                {
                    _bits &= ~(1 << (int)index);
                }
            }
        }
    }

    internal struct DayOfWeekStruct
    {
        internal DayOfWeek DayOfW;
        internal string Name;
    }

    internal class SettingsPanel
    {
        internal string Name;
        internal string Language;
    }

    public class PinWindow
    {
        public string TextWnd { get; set; }
        public string ClassWnd { get; set; }
    }

    internal struct PinClass
    {
        internal string Class;
        internal string Pattern;
        internal IntPtr Hwnd;
    }
}
