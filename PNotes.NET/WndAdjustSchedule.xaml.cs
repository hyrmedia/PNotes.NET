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

using PNDateTimePicker;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndAdjustSchedule.xaml
    /// </summary>
    public partial class WndAdjustSchedule
    {
        private class DwReal
        {
            public string Name;
            public DayOfWeek DayW;
            public override string ToString()
            {
                return Name;
            }
        }

        public WndAdjustSchedule()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndAdjustSchedule(PNote note)
            : this()
        {
            _Schedule = (PNNoteSchedule)note.Schedule.Clone();
            _Name = note.Name;
            _Note = note;
        }

        private readonly PNote _Note;
        private PNNoteSchedule _Schedule;
        private readonly string _Name = "";
        private bool _Loaded;
        private readonly Dictionary<int, int> _StopValues = new Dictionary<int, int> { { 0, -1 }, { 1, 5 }, { 2, 10 }, { 3, 15 }, { 4, 30 }, { 5, 45 }, { 6, 60 }, { 7, 120 }, { 8, 180 }, { 9, 300 }, { 10, 900 }, { 11, 1800 } };
        private readonly List<Border> _Panels = new List<Border>();
        private string[] _DayNamesFull, _DayNamesAbbr;
        private DayOfWeek[] _DaysOfWeek;
        private readonly DwReal[] _DwRealFull = new DwReal[7];
        private readonly DwReal[] _DwRealAbbr = new DwReal[7];

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                setUpSchedule();
                _Note.Schedule = (PNNoteSchedule)_Schedule.Clone();
                if (_Schedule.CloseOnNotification && _Schedule.Type != ScheduleType.None && _Note.Visible)
                    PNNotesOperations.ShowHideSpecificNote(_Note, false);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgAdjustSchedule_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                Title += @" - " + _Name;
                addPanels();
                fillArrays();
                fillScheduleTypes();
                fillCommonProperties();

                _Loaded = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #region Private procedures
        private void addPanels()
        {
            try
            {
                _Panels.Add(pnlNone);
                _Panels.Add(pnlOnce);
                _Panels.Add(pnlEveryDay);
                _Panels.Add(pnlRepeat);
                _Panels.Add(pnlWeekly);
                _Panels.Add(pnlAfter);
                _Panels.Add(pnlMontlyExact);
                _Panels.Add(pnlMonthlyDW);
                _Panels.Add(pnlMulti);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillArrays()
        {
            try
            {
                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                var first = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

                _DayNamesAbbr = ci.DateTimeFormat.AbbreviatedDayNames;
                _DayNamesFull = ci.DateTimeFormat.DayNames;
                _DaysOfWeek = Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().ToArray();

                var diff = (int)first - (int)DayOfWeek.Sunday;
                if (diff > 0)
                {
                    var tDayW = new DayOfWeek[diff];
                    Array.Copy(_DaysOfWeek, 0, tDayW, 0, diff);
                    var tAbbr = new string[diff];
                    Array.Copy(_DayNamesAbbr, 0, tAbbr, 0, diff);
                    var tFull = new string[diff];
                    Array.Copy(_DayNamesFull, 0, tFull, 0, diff);
                    for (int i = 0, j = diff; j < _DayNamesFull.Length; i++, j++)
                    {
                        _DaysOfWeek[i] = _DaysOfWeek[j];
                        _DayNamesAbbr[i] = _DayNamesAbbr[j];
                        _DayNamesFull[i] = _DayNamesFull[j];
                    }
                    Array.Copy(tDayW, 0, _DaysOfWeek, _DaysOfWeek.Length - diff, diff);
                    Array.Copy(tAbbr, 0, _DayNamesAbbr, _DayNamesAbbr.Length - diff, diff);
                    Array.Copy(tFull, 0, _DayNamesFull, _DayNamesFull.Length - diff, diff);
                    for (var i = 0; i < _DayNamesFull.Length; i++)
                    {
                        _DwRealFull[i] = new DwReal { Name = _DayNamesFull[i], DayW = _DaysOfWeek[i] };
                        _DwRealAbbr[i] = new DwReal { Name = _DayNamesAbbr[i], DayW = _DaysOfWeek[i] };
                    }
                }
                else
                {
                    for (var i = 0; i < _DayNamesFull.Length; i++)
                    {
                        _DwRealFull[i] = new DwReal { Name = _DayNamesFull[i], DayW = _DaysOfWeek[i] };
                        _DwRealAbbr[i] = new DwReal { Name = _DayNamesAbbr[i], DayW = _DaysOfWeek[i] };
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillScheduleTypes()
        {
            try
            {
                var types = Enum.GetValues(typeof(ScheduleType)).OfType<ScheduleType>().ToArray();
                for (var i = 0; i < types.Length; i++)
                {
                    cboScheduleType.Items.Add(PNLang.Instance.GetScheduleDescription(types[i]));
                    if (i == (int)_Schedule.Type)
                    {
                        cboScheduleType.SelectedIndex = i;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillCommonProperties()
        {
            try
            {
                int index;

                foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
                    cboTimeZone.Items.Add(tz);

                cboSoundFile.Items.Add(PNSchedule.DEF_SOUND);
                if (Directory.Exists(PNPaths.Instance.SoundsDir))
                {
                    var fi = new DirectoryInfo(PNPaths.Instance.SoundsDir).GetFiles("*.wav");
                    foreach (
                        var fName in
                            from f in fi
                            where !string.IsNullOrEmpty(f.Name)
                            select Path.GetFileNameWithoutExtension(f.Name))
                    {
                        cboSoundFile.Items.Add(fName);
                    }
                }

                foreach (var s in PNStatic.Voices)
                {
                    cboSoundText.Items.Add(s);
                }
                chkTrak.IsChecked = !_Schedule.Track;
                optSoundFile.IsChecked = !_Schedule.UseTts;
                optSoundText.IsChecked = _Schedule.UseTts;
                chkRepeat.IsChecked = _Schedule.SoundInLoop;
                if (!_Schedule.UseTts)
                {
                    index = cboSoundFile.Items.IndexOf(_Schedule.Sound);
                    cboSoundFile.SelectedIndex = index > -1 ? index : 0;
                }
                else
                {
                    index = cboSoundText.Items.IndexOf(_Schedule.Sound);
                    cboSoundText.SelectedIndex = index > -1 ? index : 0;
                }
                cboStopAlert.SelectedIndex = _StopValues.ContainsValue(_Schedule.StopAfter)
                    ? _StopValues.FirstOrDefault(sv => sv.Value == _Schedule.StopAfter).Key
                    : 0;
                cboRunExternal.Items.Add(PNLang.Instance.GetControlText("noExternal", "(Run nothing)"));
                foreach (var ext in PNStatic.Externals)
                {
                    cboRunExternal.Items.Add(ext.Name);
                }
                index = cboRunExternal.Items.IndexOf(_Schedule.ProgramToRunOnAlert);
                cboRunExternal.SelectedIndex = index > 0 ? index : 0;

                chkHideUntilAlert.IsChecked = _Schedule.CloseOnNotification;

                chkW0.Content = _DayNamesAbbr[0];
                chkW0.Tag = _DaysOfWeek[0];
                chkW1.Content = _DayNamesAbbr[1];
                chkW1.Tag = _DaysOfWeek[1];
                chkW2.Content = _DayNamesAbbr[2];
                chkW2.Tag = _DaysOfWeek[2];
                chkW3.Content = _DayNamesAbbr[3];
                chkW3.Tag = _DaysOfWeek[3];
                chkW4.Content = _DayNamesAbbr[4];
                chkW4.Tag = _DaysOfWeek[4];
                chkW5.Content = _DayNamesAbbr[5];
                chkW5.Tag = _DaysOfWeek[5];
                chkW6.Content = _DayNamesAbbr[6];
                chkW6.Tag = _DaysOfWeek[6];

                cboExactDate.SelectedIndex = 0;

                foreach (var dwr in _DwRealFull)
                {
                    cboDW.Items.Add(dwr);
                }
                cboDW.SelectedIndex = 0;
                cboOrdinal.SelectedIndex = 0;

                switch (_Schedule.Type)
                {
                    case ScheduleType.Once:
                        dtpOnce.DateValue = _Schedule.AlarmDate;
                        break;
                    case ScheduleType.RepeatEvery:
                        dtpRepeat.DateValue = _Schedule.StartDate;
                        updRepeatYears.Value = _Schedule.AlarmAfter.Years;
                        updRepeatMonths.Value = _Schedule.AlarmAfter.Months;
                        updRepeatWeeks.Value = _Schedule.AlarmAfter.Weeks;
                        updRepeatDays.Value = _Schedule.AlarmAfter.Days;
                        updRepeatHours.Value = _Schedule.AlarmAfter.Hours;
                        updRepeatMinutes.Value = _Schedule.AlarmAfter.Minutes;
                        updRepeatSeconds.Value = _Schedule.AlarmAfter.Seconds;
                        if (_Schedule.StartFrom == ScheduleStart.ExactTime)
                        {
                            optRepeatExact.IsChecked = true;
                            dtpEvery.DateValue = _Schedule.StartDate;
                        }
                        else
                        {
                            optRepeatProgram.IsChecked = true;
                            dtpEvery.IsEnabled = false;
                        }
                        break;
                    case ScheduleType.After:
                        dtpAfter.DateValue = _Schedule.StartDate;
                        updAfterYears.Value = _Schedule.AlarmAfter.Years;
                        updAfterMonths.Value = _Schedule.AlarmAfter.Months;
                        updAfterWeeks.Value = _Schedule.AlarmAfter.Weeks;
                        updAfterDays.Value = _Schedule.AlarmAfter.Days;
                        updAfterHours.Value = _Schedule.AlarmAfter.Hours;
                        updAfterMinutes.Value = _Schedule.AlarmAfter.Minutes;
                        updAfterSeconds.Value = _Schedule.AlarmAfter.Seconds;
                        if (_Schedule.StartFrom == ScheduleStart.ExactTime)
                        {
                            optAfterExact.IsChecked = true;
                            dtpAfter.DateValue = _Schedule.StartDate;
                        }
                        else
                        {
                            optAfterProgram.IsChecked = true;
                            dtpAfter.IsEnabled = false;
                        }
                        break;
                    case ScheduleType.EveryDay:
                        dtpEvery.DateValue = _Schedule.AlarmDate;
                        break;
                    case ScheduleType.Weekly:
                        dtpWeekly.DateValue = _Schedule.AlarmDate;
                        chkW0.IsChecked = _Schedule.Weekdays.Contains(_DaysOfWeek[0]);
                        chkW1.IsChecked = _Schedule.Weekdays.Contains(_DaysOfWeek[1]);
                        chkW2.IsChecked = _Schedule.Weekdays.Contains(_DaysOfWeek[2]);
                        chkW3.IsChecked = _Schedule.Weekdays.Contains(_DaysOfWeek[3]);
                        chkW4.IsChecked = _Schedule.Weekdays.Contains(_DaysOfWeek[4]);
                        chkW5.IsChecked = _Schedule.Weekdays.Contains(_DaysOfWeek[5]);
                        chkW6.IsChecked = _Schedule.Weekdays.Contains(_DaysOfWeek[6]);
                        break;
                    case ScheduleType.MonthlyDayOfWeek:
                        for (var i = 0; i < cboDW.Items.Count; i++)
                        {
                            var dwReal = cboDW.Items[i] as DwReal;
                            if (dwReal == null || dwReal.DayW != _Schedule.MonthDay.WeekDay) continue;
                            cboDW.SelectedIndex = i;
                            break;
                        }
                        cboOrdinal.SelectedIndex = (int)_Schedule.MonthDay.OrdinalNumber - 1;
                        dtpDW.DateValue = _Schedule.AlarmDate;
                        break;
                    case ScheduleType.MonthlyExact:
                        cboExactDate.SelectedIndex = _Schedule.AlarmDate.Day - 1;
                        dtpMonthExact.DateValue = _Schedule.AlarmDate;
                        break;
                    case ScheduleType.MultipleAlerts:
                        var alerts = _Schedule.MultiAlerts.OrderBy(a => a.Date).ToArray();
                        for (var i = 0; i < alerts.Length; i++)
                            grdAlerts.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        var row = 0;
                        foreach (var ma in alerts)
                        {
                            var dtp = new DateTimePicker
                            {
                                Format = DateBoxFormat.ShortDateAndShortTime,
                                DateValue = ma.Date,
                                VerticalAlignment = VerticalAlignment.Center,
                                Width = cboSoundFile.ActualWidth,
                                Margin = new Thickness(4)
                            };
                            if (ma.Raised)
                            {
                                dtp.BlackoutDates.Add(new CalendarDateRange(ma.Date));
                            }
                            var st = new StackPanel {Orientation = Orientation.Horizontal};
                            var check = new CheckBox
                            {
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(4)
                            };
                            check.Checked += check_Checked;
                            check.Unchecked += check_Unchecked;
                            st.Children.Add(check);
                            st.Children.Add(dtp);
                            Grid.SetRow(st, row++);
                            grdAlerts.Children.Add(st);
                        }
                        break;
                }

                for (var i = 0; i < cboTimeZone.Items.Count; i++)
                {
                    var tz = cboTimeZone.Items[i] as TimeZoneInfo;
                    if (tz == null || _Schedule.TimeZone.Id != tz.Id) continue;
                    cboTimeZone.SelectedIndex = i;
                    break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setUpSchedule()
        {
            try
            {
                var temp = (PNNoteSchedule)_Schedule.Clone();
                if ((ScheduleType)cboScheduleType.SelectedIndex == ScheduleType.None)
                {
                    _Schedule = new PNNoteSchedule();
                    return;
                }
                _Schedule = new PNNoteSchedule
                {
                    Type = (ScheduleType)cboScheduleType.SelectedIndex,
                    RepeatCount = temp.RepeatCount,
                    StopAfter = temp.StopAfter,
                    Sound = temp.Sound,
                    SoundInLoop = temp.SoundInLoop,
                    Track = temp.Track,
                    UseTts = temp.UseTts,
                    ProgramToRunOnAlert = temp.ProgramToRunOnAlert,
                    CloseOnNotification = temp.CloseOnNotification,
                    TimeZone = (TimeZoneInfo)cboTimeZone.SelectedItem
                };

                switch (_Schedule.Type)
                {
                    case ScheduleType.Once:
                        _Schedule.AlarmDate = dtpOnce.DateValue;
                        break;
                    case ScheduleType.After:
                        _Schedule.StartFrom = optAfterExact.IsChecked != null && optAfterExact.IsChecked.Value ? ScheduleStart.ExactTime : ScheduleStart.ProgramStart;
                        _Schedule.StartDate = dtpAfter.DateValue;
                        _Schedule.AlarmAfter.Years = (int)updAfterYears.Value;
                        _Schedule.AlarmAfter.Months = (int)updAfterMonths.Value;
                        _Schedule.AlarmAfter.Weeks = (int)updAfterWeeks.Value;
                        _Schedule.AlarmAfter.Days = (int)updAfterDays.Value;
                        _Schedule.AlarmAfter.Hours = (int)updAfterHours.Value;
                        _Schedule.AlarmAfter.Minutes = (int)updAfterMinutes.Value;
                        _Schedule.AlarmAfter.Seconds = (int)updAfterSeconds.Value;
                        break;
                    case ScheduleType.EveryDay:
                        _Schedule.AlarmDate = dtpEvery.DateValue;
                        break;
                    case ScheduleType.RepeatEvery:
                        _Schedule.StartFrom = optRepeatExact.IsChecked != null && optRepeatExact.IsChecked.Value ? ScheduleStart.ExactTime : ScheduleStart.ProgramStart;
                        _Schedule.StartDate = dtpRepeat.DateValue;
                        _Schedule.AlarmAfter.Years = (int)updRepeatYears.Value;
                        _Schedule.AlarmAfter.Months = (int)updRepeatMonths.Value;
                        _Schedule.AlarmAfter.Weeks = (int)updRepeatWeeks.Value;
                        _Schedule.AlarmAfter.Days = (int)updRepeatDays.Value;
                        _Schedule.AlarmAfter.Hours = (int)updRepeatHours.Value;
                        _Schedule.AlarmAfter.Minutes = (int)updRepeatMinutes.Value;
                        _Schedule.AlarmAfter.Seconds = (int)updRepeatSeconds.Value;
                        break;
                    case ScheduleType.Weekly:
                        _Schedule.Weekdays.Clear();
                        if (chkW0.IsChecked != null && chkW0.IsChecked.Value)
                        {
                            _Schedule.Weekdays.Add((DayOfWeek) chkW0.Tag);
                        }
                        if (chkW1.IsChecked != null && chkW1.IsChecked.Value)
                        {
                            _Schedule.Weekdays.Add((DayOfWeek) chkW1.Tag);
                        }
                        if (chkW2.IsChecked != null && chkW2.IsChecked.Value)
                        {
                            _Schedule.Weekdays.Add((DayOfWeek) chkW2.Tag);
                        }
                        if (chkW3.IsChecked != null && chkW3.IsChecked.Value)
                        {
                            _Schedule.Weekdays.Add((DayOfWeek) chkW3.Tag);
                        }
                        if (chkW4.IsChecked != null && chkW4.IsChecked.Value)
                        {
                            _Schedule.Weekdays.Add((DayOfWeek) chkW4.Tag);
                        }
                        if (chkW5.IsChecked != null && chkW5.IsChecked.Value)
                        {
                            _Schedule.Weekdays.Add((DayOfWeek) chkW5.Tag);
                        }
                        if (chkW6.IsChecked != null && chkW6.IsChecked.Value)
                        {
                            _Schedule.Weekdays.Add((DayOfWeek) chkW6.Tag);
                        }
                        _Schedule.AlarmDate = dtpWeekly.DateValue;
                        _Schedule.StartDate = DateTime.Today;
                        break;
                    case ScheduleType.MonthlyExact:
                        _Schedule.StartDate = DateTime.Today;
                        _Schedule.AlarmDate = new DateTime(2000, 1, cboExactDate.SelectedIndex + 1,
                                dtpMonthExact.DateValue.Hour, dtpMonthExact.DateValue.Minute,
                                dtpMonthExact.DateValue.Second);
                        break;
                    case ScheduleType.MonthlyDayOfWeek:
                        _Schedule.StartDate = DateTime.Today;
                        _Schedule.AlarmDate = dtpDW.DateValue;
                        _Schedule.MonthDay.OrdinalNumber = (DayOrdinal)(cboOrdinal.SelectedIndex + 1);
                        var dwReal = cboDW.SelectedItem as DwReal;
                        if (dwReal != null)
                            _Schedule.MonthDay.WeekDay = dwReal.DayW;
                        break;
                    case ScheduleType.MultipleAlerts:
                        foreach (var st in grdAlerts.Children.OfType<StackPanel>())
                        {
                            var dtp = st.Children[1] as DateTimePicker;
                            if (dtp == null) continue;
                            var date = dtp.DateValue;
                            if (_Schedule.MultiAlerts.All(a => a.Date != date))
                            {
                                _Schedule.MultiAlerts.Add(new MultiAlert
                                {
                                    Raised = dtp.IsBlackoutDate,
                                    Date = date
                                });
                            }
                        }
                        break;
                }
                var prog = Convert.ToString(cboRunExternal.SelectedItem);
                var ext = PNStatic.Externals.FirstOrDefault(p => p.Name == prog);
                _Schedule.ProgramToRunOnAlert = ext == null ? "" : ext.Name;
                if (chkHideUntilAlert.IsChecked != null)
                {
                    _Schedule.CloseOnNotification = chkHideUntilAlert.IsChecked.Value;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void soundTypeChanged()
        {
            try
            {
                int index;
                if (_Schedule.UseTts)
                {
                    cboSoundFile.SelectedIndex = -1;
                    index = cboSoundText.Items.IndexOf(_Schedule.Sound);
                    cboSoundText.SelectedIndex = index > -1 ? index : 0;
                }
                else
                {
                    cboSoundText.SelectedIndex = -1;
                    index = cboSoundFile.Items.IndexOf(_Schedule.Sound);
                    cboSoundFile.SelectedIndex = index > -1 ? index : 0;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Common
        private void cboScheduleType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                foreach (var b in _Panels)
                    b.Visibility = Visibility.Hidden;
                if (cboScheduleType.SelectedIndex <= -1) return;
                _Panels[cboScheduleType.SelectedIndex].Visibility = Visibility.Visible;
                expAdvanced.IsEnabled = cboScheduleType.SelectedIndex > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void chkTrak_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Loaded)
                {
                    if (chkTrak.IsChecked != null) _Schedule.Track = !chkTrak.IsChecked.Value;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void optSoundFile_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (optSoundFile.IsChecked != null && optSoundFile.IsChecked.Value)
                {
                    cboSoundText.IsEnabled = lblSoundText.IsEnabled = false;
                    cboSoundFile.IsEnabled = lblSoundFile.IsEnabled = true;
                }
                if (!_Loaded) return;
                if (optSoundFile.IsChecked != null) _Schedule.UseTts = !optSoundFile.IsChecked.Value;
                soundTypeChanged();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void optSoundText_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (optSoundText.IsChecked != null && optSoundText.IsChecked.Value)
                {
                    cboSoundText.IsEnabled = lblSoundText.IsEnabled = true;
                    cboSoundFile.IsEnabled = lblSoundFile.IsEnabled = false;
                }
                if (!_Loaded) return;
                if (optSoundText.IsChecked != null) _Schedule.UseTts = optSoundText.IsChecked.Value;
                soundTypeChanged();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lblSoundFile_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                PNStatic.PlayAlarmSound(_Schedule.Sound, false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lblSoundText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (cboSoundText.SelectedIndex > -1)
                {
                    PNStatic.SpeakNote(_Note, (string)cboSoundText.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboSoundFile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                var index = cboSoundFile.SelectedIndex;
                if (index > -1)
                {
                    _Schedule.Sound = (string)cboSoundFile.Items[index];
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboSoundText_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                var index = cboSoundText.SelectedIndex;
                if (index > -1)
                {
                    _Schedule.Sound = (string)cboSoundText.Items[index];
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void chkRepeat_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Loaded)
                {
                    if (chkRepeat.IsChecked != null) _Schedule.SoundInLoop = chkRepeat.IsChecked.Value;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboStopAlert_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                var index = cboStopAlert.SelectedIndex;
                if (index <= -1 || !_StopValues.ContainsKey(index)) return;
                _Schedule.StopAfter = _StopValues[index];
                if (_Schedule.StopAfter > 0)
                {
                    _Schedule.StopAfter *= 1000;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        } 
        #endregion

        private void optAfterExact_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Loaded)
                {
                    if (optAfterExact.IsChecked != null) dtpAfter.IsEnabled = optAfterExact.IsChecked.Value;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void optRepeatExact_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Loaded)
                {
                    if (optRepeatExact.IsChecked != null) dtpRepeat.IsEnabled = optRepeatExact.IsChecked.Value;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void check_Unchecked(object sender, RoutedEventArgs e)
        {
            cmdDeleteAlert.IsEnabled = enableDeleteButton();
        }

        private void check_Checked(object sender, RoutedEventArgs e)
        {
            cmdDeleteAlert.IsEnabled = true;
        }

        private bool enableDeleteButton()
        {
            try
            {
                return grdAlerts.Children.OfType<StackPanel>()
                    .Select(st => st.Children[0])
                    .OfType<CheckBox>()
                    .Any(check => check.IsChecked != null && check.IsChecked.Value);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void cmdAddAlert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dtp = new DateTimePicker
                {
                    DateValue = DateTime.Now,
                    Format = DateBoxFormat.ShortDateAndShortTime,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 200,
                    Margin = new Thickness(4)
                };
                grdAlerts.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var st = new StackPanel { Orientation = Orientation.Horizontal };
                var check = new CheckBox
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(4)
                };
                check.Checked += check_Checked;
                check.Unchecked += check_Unchecked;
                st.Children.Add(check);
                st.Children.Add(dtp);
                Grid.SetRow(st, grdAlerts.RowDefinitions.Count - 1);
                grdAlerts.Children.Add(st);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDeleteAlert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var stacks = grdAlerts.Children.OfType<StackPanel>().ToArray();
                for (var i = stacks.Length - 1; i >= 0; i--)
                {
                    var check = stacks[i].Children[0] as CheckBox;
                    if (check == null) continue;
                    if (check.IsChecked != null && check.IsChecked.Value)
                    {
                        grdAlerts.Children.Remove(stacks[i]);
                    }
                }
                cmdDeleteAlert.IsEnabled = enableDeleteButton();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
