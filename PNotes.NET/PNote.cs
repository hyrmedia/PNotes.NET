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
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Timers;
using System.Windows.Media;
using System.Linq;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace PNotes.NET
{
    [Serializable]
    internal class PNote : ICloneable, IDisposable
    {
        internal event EventHandler<NoteBooleanChangedEventArgs> NoteBooleanChanged;
        internal event EventHandler<NoteNameChangedEventArgs> NoteNameChanged;
        internal event EventHandler<NoteDateChangedEventArgs> NoteDateChanged;
        internal event EventHandler<NoteGroupChangedEventArgs> NoteGroupChanged;
        internal event EventHandler<NoteDockStatusChangedEventArgs> NoteDockStatusChanged;
        internal event EventHandler<NoteSendReceiveStatusChangedEventArgs> NoteSendReceiveStatusChanged;
        internal event EventHandler NoteTagsChanged;
        internal event EventHandler NoteScheduleChanged;

        internal PNote()
        {
            _ID = DateTime.Now.ToString("yyMMddHHmmssfff");
            _Opacity = PNStatic.Settings.Behavior.Opacity;
            _Name = PNLang.Instance.GetNoteString("def_caption", "Untitled");
            var group = PNStatic.Groups.GetGroupByID(_GroupID);
            if (group != null)
            {
                group.GroupPropertyChanged += group_GroupPropertyChanged;
            }
            initFields();
        }

        internal PNote(int groupID)
        {
            _ID = DateTime.Now.ToString("yyMMddHHmmssfff");
            _Opacity = PNStatic.Settings.Behavior.Opacity;
            if (groupID != (int)SpecialGroups.Diary)
            {
                _Name = PNLang.Instance.GetNoteString("def_caption", "Untitled");
            }
            else
            {
                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                _Name = DateTime.Today.ToString(PNStatic.Settings.Diary.DateFormat, ci);
            }
            _GroupID = groupID;
            var group = PNStatic.Groups.GetGroupByID(_GroupID);
            if (group != null)
            {
                group.GroupPropertyChanged += group_GroupPropertyChanged;
            }
            initFields();
        }

        internal PNote(PNote src)
        {
            _ID = DateTime.Now.ToString("yyMMddHHmmssfff");
            if (src._Skin != null)
            {
                _Skin = src._Skin.PNClone();
            }
            if (src._Skinless != null)
            {
                _Skinless = (PNSkinlessDetails) src._Skinless.Clone();
            }
            _CustomOpacity = src._CustomOpacity;
            _GroupID = src._GroupID;
            _PrevGroupID = src._PrevGroupID;
            _Opacity = src._Opacity;
            _Schedule = (PNNoteSchedule)src._Schedule.Clone();
            _Protected = src._Protected;
            _Completed = src._Completed;
            _Priority = src._Priority;
            _Pinned = src._Pinned;
            _Name = src._Name;
            _Topmost = src._Topmost;
            _Tags = new List<string>(src._Tags);
            _LinkedNotes = new List<string>(src._LinkedNotes);
            _PinClass = src._PinClass;
            _PinText = src._PinText;
            _Scrambled = src._Scrambled;

            var group = PNStatic.Groups.GetGroupByID(_GroupID);
            if (group != null)
            {
                group.GroupPropertyChanged += group_GroupPropertyChanged;
            }
            initFields();
        }

        private WndNote _Dialog;
        private string _ID;
        private string _Name;
        private int _GroupID;
        private int _PrevGroupID;
        private bool _Visible;
        private bool _FromDB;
        private bool _Favorite;
        private bool _Changed;
        private bool _Protected;
        private bool _Completed;
        private bool _Priority;
        private bool _CustomOpacity;
        private bool _Topmost;
        private bool _Rolled;
        private bool _Pinned;
        private DateTime _DateCreated = DateTime.Now;
        private DateTime _DateSaved;
        private DateTime _DateSent;
        private DateTime _DateReceived;
        private DateTime _DateDeleted;
        private PNSkinDetails _Skin;
        private PNSkinlessDetails _Skinless;
        private Size _NoteSize = Size.Empty;
        private Point _NoteLocation = new Point(double.NaN, double.NaN);
        private System.Drawing.Size _EditSize = System.Drawing.Size.Empty;
        private double _Opacity;
        private string _PasswordString = "";
        private DockStatus _DockStatus = DockStatus.None;
        private int _DockOrder = -1;
        private SendReceiveStatus _SentReceived = SendReceiveStatus.None;
        private double _XFactor;
        private double _YFactor;
        private List<string> _Tags = new List<string>();
        private List<string> _LinkedNotes = new List<string>();
        private PNNoteSchedule _Schedule = new PNNoteSchedule();
        private string _SentTo = "";
        private string _ReceivedFrom = "";
        private string _PinClass = "";
        private string _PinText = "";
        private bool _Scrambled;
        private string _ReceivedIp = "";
        private readonly Timer _Timer = new Timer(300);

        internal Timer Timer
        {
            get { return _Timer; }
        }

        public bool Scrambled
        {
            get { return _Scrambled; }
            set
            {
                var temp = _Scrambled;
                _Scrambled = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Scrambled, null));
                }
            }
        }

        internal bool Thumbnail { get; set; }

        internal double AutoHeight { get; set; }

        internal int DockOrder
        {
            get { return _DockOrder; }
            set { _DockOrder = value; }
        }
        internal string PinText
        {
            get { return _PinText; }
            set { _PinText = value; }
        }
        internal string PinClass
        {
            get { return _PinClass; }
            set { _PinClass = value; }
        }
        internal string ReceivedFrom
        {
            get { return _ReceivedFrom; }
            set { _ReceivedFrom = value; }
        }

        internal string ReceivedIp
        {
            get { return _ReceivedIp; }
            set { _ReceivedIp = value; }
        }

        internal string SentTo
        {
            get { return _SentTo; }
            set { _SentTo = value; }
        }
        internal PNNoteSchedule Schedule
        {
            get { return _Schedule; }
            set
            {
                if (_Schedule != value)
                {
                    _Schedule = value;

                    if (_Timer.Enabled)
                    {
                        _Timer.Stop();
                    }
                    PNNotesOperations.SaveNoteSchedule(this);

                    if (_Schedule.Type != ScheduleType.None)
                    {
                        _Timer.Start();
                    }
                    if (NoteScheduleChanged != null)
                    {
                        NoteScheduleChanged(this, new EventArgs());
                    }
                }
            }
        }
        internal System.Drawing.Size EditSize
        {
            get { return _EditSize; }
            set { _EditSize = value; }
        }
        internal List<string> LinkedNotes
        {
            get { return _LinkedNotes; }
            set { _LinkedNotes = value; }
        }
        internal List<string> Tags
        {
            get { return _Tags; }
            set { _Tags = value; }
        }
        internal bool CustomOpacity
        {
            get { return _CustomOpacity; }
            set { _CustomOpacity = value; }
        }
        internal double YFactor
        {
            get { return _YFactor; }
            set { _YFactor = value; }
        }
        internal double XFactor
        {
            get { return _XFactor; }
            set { _XFactor = value; }
        }
        internal DateTime DateDeleted
        {
            get { return _DateDeleted; }
            set
            {
                DateTime temp = _DateDeleted;
                _DateDeleted = value;
                if (NoteDateChanged != null && temp != value)
                {
                    NoteDateChanged(this, new NoteDateChangedEventArgs(value, temp, NoteDateType.Deletion));
                }
            }
        }
        internal DateTime DateReceived
        {
            get { return _DateReceived; }
            set
            {
                DateTime temp = _DateReceived;
                _DateReceived = value;
                if (NoteDateChanged != null && temp != value)
                {
                    NoteDateChanged(this, new NoteDateChangedEventArgs(value, temp, NoteDateType.Receiving));
                }
            }
        }
        internal DateTime DateSent
        {
            get { return _DateSent; }
            set
            {
                DateTime temp = _DateSent;
                _DateSent = value;
                if (NoteDateChanged != null && temp != value)
                {
                    NoteDateChanged(this, new NoteDateChangedEventArgs(value, temp, NoteDateType.Sending));
                }
            }
        }
        internal DateTime DateSaved
        {
            get { return _DateSaved; }
            set
            {
                DateTime temp = _DateSaved;
                _DateSaved = value;
                if (NoteDateChanged != null && temp != value)
                {
                    NoteDateChanged(this, new NoteDateChangedEventArgs(value, temp, NoteDateType.Saving));
                }
            }
        }
        internal DateTime DateCreated
        {
            get { return _DateCreated; }
            set
            {
                DateTime temp = _DateCreated;
                _DateCreated = value;
                if (NoteDateChanged != null && temp != value)
                {
                    NoteDateChanged(this, new NoteDateChangedEventArgs(value, temp, NoteDateType.Creation));
                }
            }
        }
        internal SendReceiveStatus SentReceived
        {
            get { return _SentReceived; }
            set
            {
                var temp = _SentReceived;
                _SentReceived = value;
                if (NoteSendReceiveStatusChanged == null || temp == value) return;
                NoteSendReceiveStatusChanged(this, new NoteSendReceiveStatusChangedEventArgs(value, temp));
                if (_Dialog == null) return;
                if (value == SendReceiveStatus.Sent || value == SendReceiveStatus.Received || value == SendReceiveStatus.Both)
                {
                    _Dialog.ApplySentReceivedStatus(true);
                }
                else
                {
                    _Dialog.ApplySentReceivedStatus(false);
                }
            }
        }
        internal DockStatus DockStatus
        {
            get { return _DockStatus; }
            set
            {
                DockStatus temp = _DockStatus;
                _DockStatus = value;
                if (NoteDockStatusChanged != null && temp != value)
                {
                    NoteDockStatusChanged(this, new NoteDockStatusChangedEventArgs(value, temp));
                }
            }
        }
        internal bool Rolled
        {
            get { return _Rolled; }
            set
            {
                bool temp = _Rolled;
                _Rolled = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Roll, null));
                }
            }
        }
        internal bool Topmost
        {
            get { return _Topmost; }
            set
            {
                bool temp = _Topmost;
                _Topmost = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Topmost, null));
                }
            }
        }
        internal string Name
        {
            get { return _Name; }
            set
            {
                string temp = _Name;
                _Name = value;
                if (NoteNameChanged != null && temp != value)
                {
                    NoteNameChanged(this, new NoteNameChangedEventArgs(temp, value));
                }
            }
        }
        internal bool Pinned
        {
            get { return _Pinned; }
            set
            {
                bool temp = _Pinned;
                _Pinned = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Pin, null));
                }
            }
        }
        internal string PasswordString
        {
            get { return _PasswordString; }
            set
            {
                string temp = _PasswordString;
                _PasswordString = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs((value.Trim().Length > 0), NoteBooleanTypes.Password, null));
                }
            }
        }
        internal bool Priority
        {
            get { return _Priority; }
            set
            {
                bool temp = _Priority;
                _Priority = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Priority, null));
                }
            }
        }
        internal bool Completed
        {
            get { return _Completed; }
            set
            {
                bool temp = _Completed;
                _Completed = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Complete, null));
                }
            }
        }
        internal bool Protected
        {
            get { return _Protected; }
            set
            {
                bool temp = _Protected;
                _Protected = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Protection, null));
                }
            }
        }
        internal bool Changed
        {
            get { return _Changed; }
            set
            {
                bool temp = _Changed;
                _Changed = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    var se = new SaveAsNoteNameSetEventArgs(_Name, _GroupID);
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Change, se));
                }
            }
        }
        internal int PrevGroupID
        {
            get { return _PrevGroupID; }
            set { _PrevGroupID = value; }
        }
        internal bool Favorite
        {
            get { return _Favorite; }
            set
            {
                bool temp = _Favorite;
                _Favorite = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Favorite, null));
                }
            }
        }
        internal bool FromDB
        {
            get { return _FromDB; }
            set
            {
                bool temp = _FromDB;
                _FromDB = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.FromDB, null));
                }
            }
        }
        internal bool Visible
        {
            get { return _Visible; }
            set
            {
                bool temp = _Visible;
                _Visible = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Visible, null));
                }
            }
        }
        internal double Opacity
        {
            get { return _Opacity; }
            set { _Opacity = value; }
        }
        internal WndNote Dialog
        {
            get { return _Dialog; }
            set { _Dialog = value; }
        }
        internal string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }
        internal int GroupID
        {
            get { return _GroupID; }
            set
            {
                _PrevGroupID = _GroupID;
                _GroupID = value;
                if (_GroupID != _PrevGroupID)
                {
                    PNGroup group = PNStatic.Groups.GetGroupByID(_PrevGroupID);
                    if (group != null)
                    {
                        group.GroupPropertyChanged -= group_GroupPropertyChanged;
                    }
                    group = PNStatic.Groups.GetGroupByID(_GroupID);
                    if (group != null)
                    {
                        group.GroupPropertyChanged += group_GroupPropertyChanged;
                    }
                    if (NoteGroupChanged != null)
                    {
                        NoteGroupChanged(this, new NoteGroupChangedEventArgs(_GroupID, _PrevGroupID));
                    }
                }
            }
        }
        internal PNSkinlessDetails Skinless
        {
            get { return _Skinless; }
            set { _Skinless = value; }
        }
        internal PNSkinDetails Skin
        {
            get { return _Skin; }
            set { _Skin = value; }
        }
        internal Point NoteLocation
        {
            get { return _NoteLocation; }
            set { _NoteLocation = value; }
        }
        internal Size NoteSize
        {
            get { return _NoteSize; }
            set { _NoteSize = value; }
        }

        internal System.Drawing.Color DrawingColor()
        {
            var noteGroup = PNStatic.Groups.GetGroupByID(_GroupID) ?? PNStatic.Groups.GetGroupByID(0);
            var skn = _Skinless ?? noteGroup.Skinless;
            return System.Drawing.Color.FromArgb(skn.BackColor.A, skn.BackColor.R,
                skn.BackColor.G, skn.BackColor.B);
        }

        internal SolidColorBrush Background()
        {
            var noteGroup = PNStatic.Groups.GetGroupByID(_GroupID) ?? PNStatic.Groups.GetGroupByID(0);
            var skn = _Skinless ?? noteGroup.Skinless;
            return new SolidColorBrush(skn.BackColor);
        }

        internal void RaiseTagsChangedEvent()
        {
            if (NoteTagsChanged != null)
            {
                NoteTagsChanged(this, new EventArgs());
            }
        }

        //internal void RaiseDeleteCompletelyEvent()
        //{
        //    if (NoteDeletedCompletely != null)
        //    {
        //        NoteDeletedCompletely(this, new NoteDeletedCompletelyEventArgs(_ID, _GroupID));
        //    }
        //}

        private void group_GroupPropertyChanged(object sender, GroupPropertyChangedEventArgs e)
        {
            if (!_Visible || _Dialog == null) return;
            switch (e.Type)
            {
                case GroupChangeType.BackColor:
                    if (_Skinless == null && !PNStatic.Settings.GeneralSettings.UseSkins)
                    {
                        _Dialog.ApplyBackColor((Color)e.NewStateObject);
                    }
                    break;
                case GroupChangeType.CaptionColor:
                    if (_Skinless == null && !PNStatic.Settings.GeneralSettings.UseSkins)
                    {
                        _Dialog.ApplyCaptionFontColor((Color)e.NewStateObject);
                    }
                    break;
                case GroupChangeType.CaptionFont:
                    if (_Skinless == null && !PNStatic.Settings.GeneralSettings.UseSkins)
                    {
                        _Dialog.ApplyCaptionFont((PNFont)e.NewStateObject);
                    }
                    break;
                case GroupChangeType.Skin:
                    if (_Skin == null && PNStatic.Settings.GeneralSettings.UseSkins)
                    {
                        PNSkinsOperations.ApplyNoteSkin(_Dialog, this);
                    }
                    break;
            }
        }

        private void initFields()
        {
            try
            {
                _Timer.Elapsed += _Timer_Elapsed;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void _ElapsedDelegate(object sender, ElapsedEventArgs e);

        private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool continueNext = true;
            try
            {
                var now = e.SignalTime;
                _Timer.Stop();
                var doAlarm = false;
                long seconds;
                DateTime start, alarmDate;
                var changeZone = _Schedule.TimeZone != TimeZoneInfo.Local;

                switch (_Schedule.Type)
                {
                    case ScheduleType.Once:
                        alarmDate = changeZone
                            ? TimeZoneInfo.ConvertTime(_Schedule.AlarmDate, TimeZoneInfo.Local, _Schedule.TimeZone)
                            : _Schedule.AlarmDate;
                        if (alarmDate.IsDateEqual(now))
                        {
                            doAlarm = true;
                            continueNext = false;
                        }
                        break;
                    case ScheduleType.After:
                        start = _Schedule.StartFrom == ScheduleStart.ExactTime
                            ? (changeZone
                                ? TimeZoneInfo.ConvertTime(_Schedule.StartDate, TimeZoneInfo.Local, _Schedule.TimeZone)
                                : _Schedule.StartDate)
                            : PNStatic.StartTime;
                        PNStatic.NormalizeStartDate(ref start);
                        seconds = (now - start).Ticks / TimeSpan.TicksPerSecond;
                        if (seconds == _Schedule.AlarmAfter.TotalSeconds)
                        {
                            doAlarm = true;
                            continueNext = false;
                        }
                        break;
                    case ScheduleType.RepeatEvery:
                        if (_Schedule.LastRun == DateTime.MinValue)
                        {
                            start = _Schedule.StartFrom == ScheduleStart.ExactTime
                                ? (changeZone
                                    ? TimeZoneInfo.ConvertTime(_Schedule.StartDate, TimeZoneInfo.Local,
                                        _Schedule.TimeZone)
                                    : _Schedule.StartDate)
                                : PNStatic.StartTime;
                        }
                        else
                        {
                            start = _Schedule.LastRun;
                        }
                        PNStatic.NormalizeStartDate(ref start);

                        seconds = (now - start).Ticks / TimeSpan.TicksPerSecond;

                        if (seconds == _Schedule.AlarmAfter.TotalSeconds || (seconds > 0 && seconds % _Schedule.AlarmAfter.TotalSeconds == 0))
                        {
                            doAlarm = true;
                        }
                        break;
                    case ScheduleType.EveryDay:
                        alarmDate = changeZone
                            ? TimeZoneInfo.ConvertTime(_Schedule.AlarmDate, TimeZoneInfo.Local, _Schedule.TimeZone)
                            : _Schedule.AlarmDate;
                        if (alarmDate.IsTimeEqual(now)
                            && (_Schedule.LastRun == DateTime.MinValue || _Schedule.LastRun <= now.AddDays(-1)))
                        {
                            doAlarm = true;
                        }
                        break;
                    case ScheduleType.Weekly:
                        alarmDate = changeZone
                            ? TimeZoneInfo.ConvertTime(_Schedule.AlarmDate, TimeZoneInfo.Local, _Schedule.TimeZone)
                            : _Schedule.AlarmDate;
                        if (alarmDate.IsTimeEqual(now))
                        {
                            if (_Schedule.Weekdays.Contains(now.DayOfWeek)
                                && (_Schedule.LastRun == DateTime.MinValue || _Schedule.LastRun <= now.AddDays(-1)))
                            {
                                doAlarm = true;
                            }
                        }
                        break;
                    case ScheduleType.MonthlyExact:
                        alarmDate = changeZone
                            ? TimeZoneInfo.ConvertTime(_Schedule.AlarmDate, TimeZoneInfo.Local, _Schedule.TimeZone)
                            : _Schedule.AlarmDate;
                        if (alarmDate.Day == now.Day)
                        {
                            if (alarmDate.IsTimeEqual(now))
                            {
                                if (_Schedule.LastRun == DateTime.MinValue
                                    || _Schedule.LastRun.Month < now.Month
                                    || _Schedule.LastRun.Year < now.Year)
                                {
                                    doAlarm = true;
                                }
                            }
                        }
                        break;
                    case ScheduleType.MonthlyDayOfWeek:
                        if (now.DayOfWeek == _Schedule.MonthDay.WeekDay)
                        {
                            alarmDate = changeZone
                                ? TimeZoneInfo.ConvertTime(_Schedule.AlarmDate, TimeZoneInfo.Local, _Schedule.TimeZone)
                                : _Schedule.AlarmDate;
                            if (alarmDate.IsTimeEqual(now))
                            {
                                bool isLast = false;
                                int ordinal = PNStatic.WeekdayOrdinal(now, _Schedule.MonthDay.WeekDay, ref isLast);
                                if (_Schedule.MonthDay.OrdinalNumber == DayOrdinal.Last)
                                {
                                    if (isLast)
                                    {
                                        if (_Schedule.LastRun == DateTime.MinValue
                                            || _Schedule.LastRun.Month < now.Month
                                            || _Schedule.LastRun.Year < now.Year)
                                        {
                                            doAlarm = true;
                                        }
                                    }
                                }
                                else
                                {
                                    if ((int)_Schedule.MonthDay.OrdinalNumber == ordinal)
                                    {
                                        if (_Schedule.LastRun == DateTime.MinValue
                                            || _Schedule.LastRun.Month < now.Month
                                            || _Schedule.LastRun.Year < now.Year)
                                        {
                                            doAlarm = true;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case ScheduleType.MultipleAlerts:
                        foreach (var ma in _Schedule.MultiAlerts.Where(a => !a.Raised))
                        {
                            alarmDate = changeZone
                                ? TimeZoneInfo.ConvertTime(ma.Date, TimeZoneInfo.Local, _Schedule.TimeZone)
                                : ma.Date;
                            if (!alarmDate.IsDateEqual(now)) continue;
                            ma.Checked = true;
                            doAlarm = true;
                            break;
                        }
                        break;
                }
                if (doAlarm)
                {
                    if (!PNStatic.FormMain.Dispatcher.CheckAccess())
                    {
                        _ElapsedDelegate d = _Timer_Elapsed;
                        PNStatic.FormMain.Dispatcher.Invoke(d, sender, e);
                    }
                    else
                    {
                        bool save = false;
                        switch (_Schedule.Type)
                        {
                            case ScheduleType.EveryDay:
                            case ScheduleType.RepeatEvery:
                            case ScheduleType.Weekly:
                            case ScheduleType.MonthlyExact:
                            case ScheduleType.MonthlyDayOfWeek:
                                _Schedule.LastRun = now;
                                save = true;
                                break;
                            case ScheduleType.MultipleAlerts:
                                var ma = _Schedule.MultiAlerts.FirstOrDefault(a => a.Checked);
                                if (ma != null)
                                {
                                    ma.Checked = false;
                                    ma.Raised = true;
                                }
                                save = true;
                                break;
                        }
                        PNStatic.FormMain.ApplyDoAlarm(this);
                        if (save)
                        {
                            PNNotesOperations.SaveNoteSchedule(this);
                        }
                        if (!continueNext)
                        {
                            PNNotesOperations.DeleteNoteSchedule(this);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (continueNext)
                {
                    _Timer.Start();
                }
            }
        }

        #region ICloneable Members

        public object Clone()
        {
            var bfs = new BinaryFormatter();
            var bfd = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bfs.Serialize(ms, this);
                ms.Position = 0;
                var note = (PNote)bfd.Deserialize(ms);
                note.Dialog = null;
                note._ID = DateTime.Now.ToString("yyMMddHHmmssfff");
                return note;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            PNGroup group = PNStatic.Groups.GetGroupByID(_GroupID);
            if (group != null)
            {
                group.GroupPropertyChanged -= group_GroupPropertyChanged;
            }
            if (_Skin != null)
            {
                _Skin.Dispose();
            }
        }

        #endregion
    }
}
