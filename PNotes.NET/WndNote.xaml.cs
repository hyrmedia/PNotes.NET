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

using PNRichEdit;
using PNStaticFonts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;
using Brush = System.Drawing.Brush;
using Clipboard = System.Windows.Clipboard;
using Color = System.Windows.Media.Color;
using ComboBox = System.Windows.Controls.ComboBox;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Cursors = System.Windows.Input.Cursors;
using Fonts = PNStaticFonts.Fonts;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Path = System.IO.Path;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using TextDataFormat = System.Windows.TextDataFormat;
using Timer = System.Timers.Timer;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndNote.xaml
    /// </summary>
    public partial class WndNote
    {
        #region Comparers
        private class AscStringComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return string.Compare(x, y, StringComparison.Ordinal);
            }
        }

        private class DescStringComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return string.Compare(y, x, StringComparison.Ordinal);
            }
        }
        #endregion

        private const int MIN_HEIGHT = 72;

        private enum DropCase
        {
            None,
            Object,
            Content,
            Link
        }

        private readonly string m_InputString = "";
        private readonly NewNoteMode _Mode = NewNoteMode.None;
        private readonly PNote _Note;
        private readonly Timer _StopAlarmTimer = new Timer();
        private bool _Active;
        private WndAlarm _DlgAlarm;
        private bool _Loaded;
        private bool _InDrop;
        private PNSkinDetails _RuntimeSkin = new PNSkinDetails();
        private SaveAsNoteNameSetEventArgs _SaveArgs;
        private PinClass _PinClass;
        private DropCase _DropCase;
        private readonly DayOfWeekStruct[] _DaysOfWeek = new DayOfWeekStruct[7];
        private bool _Closing;
        private bool _InResize;
        private bool _InRoll;
        private bool _InDock;

        private readonly Dictionary<string, System.Windows.Forms.ToolStripMenuItem> _PostPluginsMenus =
            new Dictionary<string, System.Windows.Forms.ToolStripMenuItem>();
        private DependencyPropertyDescriptor _BackgroundDescriptor;
        private PNRichEditBox _Edit;
        private EditControl _EditControl;
        private HwndSource _HwndSource;
        private bool _InAlarm;
        private bool _SizeChangedFirstTime;

        #region Constructors
        public WndNote()
        {
            NoteVisual = new DrawingBrush();
            InDockProcess = false;
            InitializeComponent();
            initializaFields();
            applyMenusLanguage();
            if (!PNStatic.Settings.GeneralSettings.UseSkins)
            {
                PHeader = SkinlessHeader;
                PFooter = SkinlessFooter;
                PToolbar = SkinlessToolbar;
            }
            else
            {
                PHeader = SkinnableHeader;
                PFooter = SkinnableFooter;
                PToolbar = SkinnableToolbar;
            }
        }

        internal WndNote(PNote note, string id, NewNoteMode mode)
        {
            NoteVisual = new DrawingBrush();
            InDockProcess = false;
            InitializeComponent();
            initializaFields();
            _Note = note;
            m_InputString = id;
            _Mode = mode;
            if (!PNStatic.Settings.GeneralSettings.UseSkins)
            {
                PHeader = SkinlessHeader;
                PFooter = SkinlessFooter;
                PToolbar = SkinlessToolbar;
            }
            else
            {
                PHeader = SkinnableHeader;
                PFooter = SkinnableFooter;
                PToolbar = SkinnableToolbar;
            }
        }

        internal WndNote(PNote note, NewNoteMode mode)
        {
            NoteVisual = new DrawingBrush();
            InDockProcess = false;
            InitializeComponent();
            initializaFields();
            _Note = note;
            _Mode = mode;
            if (!PNStatic.Settings.GeneralSettings.UseSkins)
            {
                PHeader = SkinlessHeader;
                PFooter = SkinlessFooter;
                PToolbar = SkinlessToolbar;
            }
            else
            {
                PHeader = SkinnableHeader;
                PFooter = SkinnableFooter;
                PToolbar = SkinnableToolbar;
            }
        }
        #endregion

        #region Public properties

        public static DependencyProperty IsRolledProperty = DependencyProperty.Register("IsRolled", typeof(bool),
            typeof(WndNote), new FrameworkPropertyMetadata(false));
        public bool IsRolled
        {
            get { return (bool)GetValue(IsRolledProperty); }
            private set { SetValue(IsRolledProperty, value); }
        }

        #endregion

        #region Internal procedures

        internal void SetThumbnail()
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle) ?? _Note;
                if (note == null) return;

                if (note.DockStatus != DockStatus.None)
                {
                    UndockNote(note);
                }

                PNStatic.DoEvents();
                System.Windows.Forms.Application.DoEvents();

                NoteVisual = takeSnapshot();

                runThumbnailAnimation(note);

                PNStatic.FormPanel.Thumbnails.Add(new ThumbnailButton { ThumbnailBrush = NoteVisual, Id = note.ID, ThumbnailName = note.Name });
                if (!note.Thumbnail)
                {
                    note.Thumbnail = true;
                    PNNotesOperations.SaveNoteThumbnail(note);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void HideToolbar()
        {
            PToolbar.Visibility = Visibility.Collapsed;
        }

        internal void DockNote(PNote note, DockStatus status, bool fromLoad)
        {
            try
            {
                _InDock = true;
                if (!fromLoad)
                {
                    if (note.Thumbnail && PNStatic.Settings.Behavior.ShowNotesPanel)
                    {
                        PNStatic.FormPanel.RemoveThumbnail(note);
                    }

                    var prevStatus = note.DockStatus;
                    note.DockStatus = status;
                    if (PNStatic.DockedNotes[status].Count > 0)
                        note.DockOrder = PNStatic.DockedNotes[status].Max(n => n.DockOrder) + 1;
                    else
                        note.DockOrder = 0;
                    PNNotesOperations.ShiftPreviousDock(note, prevStatus, false);
                }
                var wa = PNStatic.AllScreensBounds();

                var multiplier = fromLoad
                    ? note.DockOrder
                    : PNStatic.DockedNotes[status].Count(n => n.DockOrder < note.DockOrder);
                int w, h;

                if (!PNStatic.Settings.GeneralSettings.UseSkins)
                {
                    w = PNStatic.Settings.GeneralSettings.DockWidth;
                    h = PNStatic.Settings.GeneralSettings.DockHeight;
                    PHeader.SetPNFont(PNStatic.Docking.Skinless.CaptionFont);
                    Foreground = new SolidColorBrush(PNStatic.Docking.Skinless.CaptionColor);
                    Background = new SolidColorBrush(PNStatic.Docking.Skinless.BackColor);
                    this.SetSize(new Size(w, h));
                }
                else
                {
                    w = PNStatic.Docking.Skin.BitmapSkin.Width;
                    h = PNStatic.Docking.Skin.BitmapSkin.Height;
                    Hide();
                    SetRuntimeSkin(PNStatic.Docking.Skin);
                    Show();
                }
                var stb = TryFindResource("DockStoryboard") as Storyboard;
                if (stb != null)
                {
                    var anim1 = stb.Children[0] as DoubleAnimation;
                    var anim2 = stb.Children[1] as DoubleAnimation;
                    if (anim1 == null || anim2 == null) return;
                    switch (note.DockStatus)
                    {
                        case DockStatus.Left:
                            anim1.To = wa.Left;
                            anim2.To = wa.Top + multiplier * h;
                            break;
                        case DockStatus.Top:
                            anim1.To = wa.Left + multiplier * w;
                            anim2.To = wa.Top;
                            break;
                        case DockStatus.Right:
                            anim1.To = wa.Right - w;
                            anim2.To = wa.Top + multiplier * h;
                            break;
                        case DockStatus.Bottom:
                            anim1.To = wa.Left + multiplier * w;
                            anim2.To = wa.Bottom - h;
                            break;
                    }
                    stb.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
                }
                else
                {
                    switch (note.DockStatus)
                    {
                        case DockStatus.Left:
                            this.SetLocation(new Point(wa.Left, wa.Top + multiplier * h));
                            break;
                        case DockStatus.Top:
                            this.SetLocation(new Point(wa.Left + multiplier * w, wa.Top));
                            break;
                        case DockStatus.Right:
                            this.SetLocation(new Point(wa.Right - w, wa.Top + multiplier * h));
                            break;
                        case DockStatus.Bottom:
                            this.SetLocation(new Point(wa.Left + multiplier * w, wa.Bottom - h));
                            break;
                    }
                }
                Topmost = true;
                if (!fromLoad && note.FromDB)
                {
                    PNNotesOperations.SaveNoteDockStatus(note);
                }
                PNStatic.DockedNotes[status].Add(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _InDock = false;
            }
        }

        internal void ApplyBackColor(Color color)
        {
            try
            {
                Background = new SolidColorBrush(color);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyCaptionFont(PNFont font)
        {
            try
            {
                PHeader.SetPNFont(font);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyCaptionFontColor(Color color)
        {
            try
            {
                Foreground = new SolidColorBrush(color);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplySentReceivedStatus(bool visibility)
        {
            PFooter.SetMarkButtonVisibility(MarkType.Mail, visibility);
            if (visibility)
            {
                SetSendReceiveTooltip();
            }
        }

        internal void ApplyAppearanceAdjustment(NoteAppearanceAdjustedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note != null)
                {
                    note.Opacity = e.Opacity;
                    note.CustomOpacity = e.CustomOpacity;
                    Opacity = note.Opacity;
                    PNNotesOperations.SaveNoteOpacity(note);

                    if (e.CustomSkinless)
                    {
                        note.Skinless = (PNSkinlessDetails)e.Skinless.Clone();
                        if (!PNStatic.Settings.GeneralSettings.UseSkins)
                        {
                            PHeader.SetPNFont(note.Skinless.CaptionFont);
                            Foreground = new SolidColorBrush(note.Skinless.CaptionColor);
                            Background = new SolidColorBrush(note.Skinless.BackColor);
                        }
                    }
                    else
                    {
                        note.Skinless = null;
                        PNGroup group = PNStatic.Groups.GetGroupByID(note.GroupID);
                        if (group != null)
                        {
                            if (!PNStatic.Settings.GeneralSettings.UseSkins)
                            {
                                PHeader.SetPNFont(group.Skinless.CaptionFont);
                                Foreground = new SolidColorBrush(group.Skinless.CaptionColor);
                                Background = new SolidColorBrush(group.Skinless.BackColor);
                            }
                        }
                    }
                    PNNotesOperations.SaveNoteSkinless(note);

                    if (e.CustomSkin)
                    {
                        note.Skin = e.Skin.PNClone();
                        if (PNStatic.Settings.GeneralSettings.UseSkins)
                        {
                            PNSkinsOperations.ApplyNoteSkin(this, note);
                        }
                    }
                    else
                    {
                        note.Skin = null;
                        PNGroup group = PNStatic.Groups.GetGroupByID(note.GroupID);
                        if (group != null)
                        {
                            if (PNStatic.Settings.GeneralSettings.UseSkins)
                            {
                                PNSkinsOperations.ApplyNoteSkin(this, note);
                            }
                        }
                    }
                    PNNotesOperations.SaveNoteSkin(note);

                    if (!e.CustomOpacity && !e.CustomSkin && !e.CustomSkinless)
                    {
                        PNNotesOperations.RemoveCustomNotesSettings(note.ID);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyScramble()
        {
            mnuScramble.PerformClick();
        }

        internal void ApplyRollUnroll(PNote note)
        {
            try
            {
                _InRoll = true;
                if (!note.Rolled)
                {
                    rollUnrollNote(note, true);
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Roll, true, null);
                }
                else
                {
                    rollUnrollNote(note, false);
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Roll, false, null);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _InRoll = false;
            }
        }

        internal void ApplySaveNote(bool rename)
        {
            showSaveAsDialog(rename);
        }

        internal void ApplySave(PNote note, bool showQuestion)
        {
            try
            {
                if (note.Changed)
                {
                    var results = MessageBoxResult.Yes;
                    if (showQuestion && PNStatic.Settings.GeneralSettings.ConfirmSaving)
                    {
                        string message = PNLang.Instance.GetMessageText("save_note_1", "Note has been changed:");
                        message += "\n<" + note.Name + ">\n";
                        message += PNLang.Instance.GetMessageText("save_note_2", "Do you want to save it?");
                        results = PNMessageBox.Show(this, message, PNStrings.PROG_NAME, MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                    }
                    if (results == MessageBoxResult.Yes)
                    {
                        showSaveAsDialog(false);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void SendWindowToForeground()
        {
            PNInterop.BringWindowToTop(_HwndSource.Handle);
        }

        internal void ApplyHideNote(PNote note)
        {
            try
            {
                if (note.Changed)
                {
                    var results = MessageBoxResult.Yes;
                    if (!PNStatic.Settings.GeneralSettings.SaveWithoutConfirmOnHide)
                    {
                        string message = PNLang.Instance.GetMessageText("save_note_1", "Note has been changed:");
                        message += "\n<" + note.Name + ">\n";
                        message += PNLang.Instance.GetMessageText("save_note_2", "Do you want to save it?");
                        results = PNMessageBox.Show(this, message, PNStrings.PROG_NAME, MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                    }
                    if (results == MessageBoxResult.Yes)
                    {
                        showSaveAsDialog(false);
                    }
                }
                if (note.DockStatus != DockStatus.None && !PNSingleton.Instance.InSkinReload)
                {
                    PNNotesOperations.PreUndockNote(note);
                }

                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Visible, false, null);

                if (PNStatic.Settings.Behavior.PlaySoundOnHide)
                {
                    if (!PNSingleton.Instance.InSkinReload)
                    {
                        playSoundOnHide();
                    }
                }
                if (PNStatic.Settings.Behavior.HideFluently)
                {
                    if (!PNSingleton.Instance.InSkinReload)
                    {
                        var sb = TryFindResource("FadeAway") as Storyboard;
                        if (sb == null)
                        {
                            note.Dialog = null;
                            Close();
                        }
                        else
                        {
                            sb.Begin();
                        }
                    }
                    else
                    {
                        note.Dialog = null;
                        Close();
                    }
                }
                else
                {
                    note.Dialog = null;
                    Close();
                }
                if (note.Thumbnail && PNStatic.Settings.Behavior.ShowNotesPanel)
                {
                    PNStatic.FormPanel.RemoveThumbnail(note, false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyPrintNote()
        {
            mnuPrint.PerformClick();
        }

        internal void ApplyShowScrollBars(System.Windows.Forms.RichTextBoxScrollBars value)
        {
            _Edit.ScrollBars = value;
            _Edit.WordWrap = value != System.Windows.Forms.RichTextBoxScrollBars.Horizontal &&
                             value != System.Windows.Forms.RichTextBoxScrollBars.Both;
        }

        internal void ApplyHideButtonVisibility(bool visibility)
        {
            PHeader.SetButtonVisibility(HeaderButtonType.Hide,
                visibility ? Visibility.Visible : Visibility.Collapsed);
        }

        internal void ApplyPanelButtonVisibility(bool visibility)
        {
            PHeader.SetButtonVisibility(HeaderButtonType.Panel,
                visibility ? Visibility.Visible : Visibility.Collapsed);
        }

        internal void ApplySwitch(NoteBooleanTypes sw)
        {
            try
            {
                switch (sw)
                {
                    case NoteBooleanTypes.Complete:
                        mnuMarkAsComplete.PerformClick();
                        break;
                    case NoteBooleanTypes.Priority:
                        mnuToggleHighPriority.PerformClick();
                        break;
                    case NoteBooleanTypes.Protection:
                        mnuToggleProtectionMode.PerformClick();
                        break;
                    case NoteBooleanTypes.Roll:
                        mnuRollUnroll.PerformClick();
                        break;
                    case NoteBooleanTypes.Topmost:
                        mnuOnTop.PerformClick();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyDeleteButtonVisibility(bool visibility)
        {
            PHeader.SetButtonVisibility(HeaderButtonType.Delete,
                visibility ? Visibility.Visible : Visibility.Collapsed);
        }

        internal void ApplyUseAlternative(bool value)
        {
            PHeader.SetAlternative(value);
        }

        internal void ApplyAutoHeight()
        {
            resizeOnAutoheight();
        }

        internal void ApplyMarginsWidth(short marginSize)
        {
            _Edit.SetMargins(marginSize);
        }

        internal void UndockNote(PNote note)
        {
            try
            {
                _InDock = true;
                PNNotesOperations.PreUndockNote(note);

                this.SetSize(PNStatic.Settings.GeneralSettings.AutoHeight
                    ? new Size(note.NoteSize.Width, note.AutoHeight)
                    : note.NoteSize);
                if (!PNStatic.Settings.GeneralSettings.UseSkins)
                {
                    if (note.Skinless != null)
                    {
                        PHeader.SetPNFont(note.Skinless.CaptionFont);
                        Foreground = new SolidColorBrush(note.Skinless.CaptionColor);
                        Background = new SolidColorBrush(note.Skinless.BackColor);
                    }
                    else
                    {
                        PNGroup group = PNStatic.Groups.GetGroupByID(note.GroupID);
                        if (group != null)
                        {
                            PHeader.SetPNFont(group.Skinless.CaptionFont);
                            Foreground = new SolidColorBrush(group.Skinless.CaptionColor);
                            Background = new SolidColorBrush(group.Skinless.BackColor);
                        }
                    }
                    if (note.Rolled)
                    {
                        rollUnrollNote(note, true);
                        PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Roll, true, null);
                    }
                }
                else
                {
                    PNSkinsOperations.ApplyNoteSkin(this, note);
                }

                var stb = TryFindResource("DockStoryboard") as Storyboard;
                if (stb != null)
                {
                    var anim1 = stb.Children[0] as DoubleAnimation;
                    var anim2 = stb.Children[1] as DoubleAnimation;
                    if (anim1 == null || anim2 == null) return;
                    anim1.To = note.NoteLocation.X;
                    anim2.To = note.NoteLocation.Y;
                    stb.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
                }
                else
                {
                    this.SetLocation(note.NoteLocation);
                }

                Topmost = note.Topmost;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _InDock = false;
            }
        }

        internal bool SaveNoteFile(PNote note)
        {
            try
            {
                PFooter.SetMarkButtonVisibility(MarkType.Change, false);
                mnuSave.IsEnabled = false;
                _Edit.Modified = false;
                //first save back copy
                if (PNStatic.Settings.Protection.BackupBeforeSaving)
                {
                    saveBackCopy(note);
                }
                string path = Path.Combine(PNPaths.Instance.DataDir, note.ID);
                path += PNStrings.NOTE_EXTENSION;
                PNNotesOperations.SaveNoteFile(_Edit, path);
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal void SetSendReceiveTooltip()
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle) ?? _Note;
                if (note == null) return;
                string text;
                switch (note.SentReceived)
                {
                    case SendReceiveStatus.Received:
                        text = PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Received_From", "Received From");
                        text += ":";
                        text += Environment.NewLine;
                        text += note.ReceivedFrom;
                        text += " ";
                        text += note.DateReceived.ToString(PNStatic.Settings.GeneralSettings.DateFormat);
                        text += " ";
                        text += note.DateReceived.ToString(PNStatic.Settings.GeneralSettings.TimeFormat);
                        PFooter.SetMarkButtonTooltip(MarkType.Mail, text);
                        break;
                    case SendReceiveStatus.Sent:
                        text = PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Sent_To", "Sent To");
                        text += ":";
                        text += Environment.NewLine;
                        text += note.SentTo;
                        text += " ";
                        text += note.DateSent.ToString(PNStatic.Settings.GeneralSettings.DateFormat);
                        text += " ";
                        text += note.DateSent.ToString(PNStatic.Settings.GeneralSettings.TimeFormat);
                        PFooter.SetMarkButtonTooltip(MarkType.Mail, text);
                        break;
                    case SendReceiveStatus.Both:
                        text = PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Sent_To", "Sent To");
                        text += ":";
                        text += Environment.NewLine;
                        text += note.SentTo;
                        text += " ";
                        text += note.DateSent.ToString(PNStatic.Settings.GeneralSettings.DateFormat);
                        text += " ";
                        text += note.DateSent.ToString(PNStatic.Settings.GeneralSettings.TimeFormat);
                        text += Environment.NewLine;
                        text += PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Received_From", "Received From");
                        text += ":";
                        text += Environment.NewLine;
                        text += note.ReceivedFrom;
                        text += " ";
                        text += note.DateReceived.ToString(PNStatic.Settings.GeneralSettings.DateFormat);
                        text += " ";
                        text += note.DateReceived.ToString(PNStatic.Settings.GeneralSettings.TimeFormat);
                        PFooter.SetMarkButtonTooltip(MarkType.Mail, text);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyTooltip()
        {
            try
            {
                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                var note = PNStatic.Notes.Note(Handle) ?? _Note;
                if (note == null) return;
                if (!PNStatic.Settings.GeneralSettings.UseSkins)
                {
                    PHeader.ToolTip = note.Name +
                                      (note.DateSaved != DateTime.MinValue
                                          ? " - " +
                                            note.DateSaved.ToString(PNStatic.Settings.GeneralSettings.DateFormat, ci)
                                          : "");
                }
                else
                {
                    PHeader.ToolTip = note.Name +
                                      (note.DateSaved != DateTime.MinValue
                                          ? " - " +
                                            note.DateSaved.ToString(PNStatic.Settings.GeneralSettings.DateFormat, ci)
                                          : "");
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void SetFooterButtonSize(ToolStripButtonSize bs)
        {
            try
            {
                PFooter.SetButtonSize(bs);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void DeactivateWindow()
        {
            try
            {
                if (_Closing) return;

                _Active = false;
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                Opacity = note.CustomOpacity ? note.Opacity : PNStatic.Settings.Behavior.Opacity;

                if (PNStatic.Settings.GeneralSettings.HideToolbar) return;
                PToolbar.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void HotKeyClick(PNHotKey hk)
        {
            try
            {
                switch (hk.MenuName)
                {
                    case "mnuRename":
                        mnuRename.PerformClick();
                        break;
                    case "mnuSaveAsText":
                        mnuSaveAsText.PerformClick();
                        break;
                    case "mnuRestoreFromBackup":
                        mnuRestoreFromBackup.PerformClick();
                        break;
                    case "mnuDuplicateNote":
                        mnuDuplicateNote.PerformClick();
                        break;
                    case "mnuAdjustAppearance":
                        mnuAdjustAppearance.PerformClick();
                        break;
                    case "mnuAdjustSchedule":
                        mnuAdjustSchedule.PerformClick();
                        break;
                    case "mnuHideNote":
                        mnuHideNote.PerformClick();
                        break;
                    case "mnuDeleteNote":
                        mnuDeleteNote.PerformClick();
                        break;
                    case "mnuDockNone":
                        mnuDockNone.PerformClick();
                        break;
                    case "mnuDockLeft":
                        mnuDockLeft.PerformClick();
                        break;
                    case "mnuDockTop":
                        mnuDockTop.PerformClick();
                        break;
                    case "mnuDockRight":
                        mnuDockRight.PerformClick();
                        break;
                    case "mnuDockBottom":
                        mnuDockBottom.PerformClick();
                        break;
                    case "mnuSendAsText":
                        mnuSendAsText.PerformClick();
                        break;
                    case "mnuSendAsAttachment":
                        mnuSendAsAttachment.PerformClick();
                        break;
                    case "mnuSendZip":
                        mnuSendZip.PerformClick();
                        break;
                    case "mnuAddContact":
                        mnuAddContact.PerformClick();
                        break;
                    case "mnuAddGroup":
                        mnuAddGroup.PerformClick();
                        break;
                    case "mnuSelectContact":
                        mnuSelectContact.PerformClick();
                        break;
                    case "mnuSelectGroup":
                        mnuSelectGroup.PerformClick();
                        break;
                    case "mnuReply":
                        mnuReply.PerformClick();
                        break;
                    case "mnuTags":
                        mnuTags.PerformClick();
                        break;
                    case "mnuManageLinks":
                        mnuManageLinks.PerformClick();
                        break;
                    case "mnuAddToFavorites":
                        mnuAddToFavorites.PerformClick();
                        break;
                    case "mnuRemoveFromFavorites":
                        mnuRemoveFromFavorites.PerformClick();
                        break;
                    case "mnuOnTop":
                        mnuOnTop.PerformClick();
                        break;
                    case "mnuToggleHighPriority":
                        mnuToggleHighPriority.PerformClick();
                        break;
                    case "mnuToggleProtectionMode":
                        mnuToggleProtectionMode.PerformClick();
                        break;
                    case "mnuSetPassword":
                        mnuSetPassword.PerformClick();
                        break;
                    case "mnuRemovePassword":
                        mnuRemovePassword.PerformClick();
                        break;
                    case "mnuMarkAsComplete":
                        mnuMarkAsComplete.PerformClick();
                        break;
                    case "mnuRollUnroll":
                        mnuRollUnroll.PerformClick();
                        break;
                    case "mnuPin":
                        mnuPin.PerformClick();
                        break;
                    case "mnuUnpin":
                        mnuUnpin.PerformClick();
                        break;
                    case "mnuCopyPlain":
                        mnuCopyPlain.PerformClick();
                        break;
                    case "mnuPastePlain":
                        mnuPastePlain.PerformClick();
                        break;
                    case "mnuInsertSmiley":
                        mnuInsertSmiley.PerformClick();
                        break;
                    case "mnuInsertPicture":
                        mnuInsertPicture.PerformClick();
                        break;
                    case "mnuDrawing":
                        mnuDrawing.PerformClick();
                        break;
                    case "mnuInsertDT":
                        mnuInsertDT.PerformClick();
                        break;
                    case "mnuToUpper":
                        mnuToUpper.PerformClick();
                        break;
                    case "mnuToLower":
                        mnuToLower.PerformClick();
                        break;
                    case "mnuCapSent":
                        mnuCapSent.PerformClick();
                        break;
                    case "mnuCapWord":
                        mnuCapWord.PerformClick();
                        break;
                    case "mnuBullets":
                        mnuBullets.PerformClick();
                        break;
                    case "mnuLatinBig":
                        mnuLatinBig.PerformClick();
                        break;
                    case "mnuLatinSmall":
                        mnuLatinSmall.PerformClick();
                        break;
                    case "mnuNoBullets":
                        mnuNoBullets.PerformClick();
                        break;
                    case "mnuArabicParts":
                        mnuArabicParts.PerformClick();
                        break;
                    case "mnuArabicPoint":
                        mnuArabicPoint.PerformClick();
                        break;
                    case "mnuSmallLettersPart":
                        mnuSmallLettersPart.PerformClick();
                        break;
                    case "mnuSmallLettersPoint":
                        mnuSmallLettersPoint.PerformClick();
                        break;
                    case "mnuInsertTable":
                        mnuInsertTable.PerformClick();
                        break;
                    case "mnuInsertSpecialSymbol":
                        mnuInsertSpecialSymbol.PerformClick();
                        break;
                    case "mnuSubscript":
                        mnuSubscript.PerformClick();
                        break;
                    case "mnuSuperscript":
                        mnuSuperscript.PerformClick();
                        break;
                    case "mnuAddSpaceBefore":
                        mnuAddSpaceBefore.PerformClick();
                        break;
                    case "mnuAddSpaceAfter":
                        mnuAddSpaceAfter.PerformClick();
                        break;
                    case "mnuRemoveSpaceBefore":
                        mnuRemoveSpaceBefore.PerformClick();
                        break;
                    case "mnuRemoveSpaceAfter":
                        mnuRemoveSpaceAfter.PerformClick();
                        break;
                    case "mnuSpace10":
                        mnuSpace10.PerformClick();
                        break;
                    case "mnuSpace15":
                        mnuSpace15.PerformClick();
                        break;
                    case "mnuSpace20":
                        mnuSpace20.PerformClick();
                        break;
                    case "mnuSpace30":
                        mnuSpace30.PerformClick();
                        break;
                    case "mnuScramble":
                        mnuScramble.PerformClick();
                        break;
                    case "mnuSortAscending":
                        mnuSortAscending.PerformClick();
                        break;
                    case "mnuSortDescending":
                        mnuSortDescending.PerformClick();
                        break;
                    case "mnuIncreaseIndent":
                        mnuIncreaseIndent.PerformClick();
                        break;
                    case "mnuDecreaseIndent":
                        mnuDecreaseIndent.PerformClick();
                        break;
                    case "mnuBold":
                        mnuBold.PerformClick();
                        break;
                    case "mnuUnderline":
                        mnuUnderline.PerformClick();
                        break;
                    case "mnuItalic":
                        mnuItalic.PerformClick();
                        break;
                    case "mnuStrikethrough":
                        mnuStrikethrough.PerformClick();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Internal properties

        internal DrawingBrush NoteVisual { get; set; }

        internal void SetRuntimeSkin(PNSkinDetails value, bool fromLoad = false)
        {
            try
            {
                _RuntimeSkin = value;

                this.SetSize(_RuntimeSkin.BitmapPattern.Size.Width, _RuntimeSkin.BitmapPattern.Size.Height);
                SkinImage.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapSkin, _RuntimeSkin.MaskColor);

                EditTargetSkinnable.Width = _RuntimeSkin.PositionEdit.Width;
                EditTargetSkinnable.Height = _RuntimeSkin.PositionEdit.Height;
                Canvas.SetLeft(EditTargetSkinnable, _RuntimeSkin.PositionEdit.X);
                Canvas.SetTop(EditTargetSkinnable, _RuntimeSkin.PositionEdit.Y);
                _EditControl.WinForm.BackgroundImage = editBackgroundImage();
                //header
                Canvas.SetLeft(SkinnableHeader, _RuntimeSkin.PositionDelHide.X);
                Canvas.SetTop(SkinnableHeader, _RuntimeSkin.PositionDelHide.Y);
                SkinnableHeader.InitialLeft = _RuntimeSkin.PositionDelHide.X;
                SkinnableHeader.HideImage.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapDelHide,
                    _RuntimeSkin.MaskColor, 0, 0, _RuntimeSkin.BitmapDelHide.Width / 2,
                    _RuntimeSkin.BitmapDelHide.Height);
                SkinnableHeader.DeleteImage.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapDelHide,
                    _RuntimeSkin.MaskColor, _RuntimeSkin.BitmapDelHide.Width / 2, 0, _RuntimeSkin.BitmapDelHide.Width / 2,
                    _RuntimeSkin.BitmapDelHide.Height);
                SkinnableHeader.Width = _RuntimeSkin.PositionDelHide.Width;
                SkinnableHeader.Height = _RuntimeSkin.PositionDelHide.Height;
                //toolbar
                var width = _RuntimeSkin.BitmapCommands.Width / 12;
                var x = 0;
                Canvas.SetLeft(SkinnableToolbar, _RuntimeSkin.PositionToolbar.X);
                Canvas.SetTop(SkinnableToolbar, _RuntimeSkin.PositionToolbar.Y);
                SkinnableToolbar.FontFamilyButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.FontSizeButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.FontColorButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.FontBoldButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.FontItalicButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.FontUnderlineButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.FontStrikethroughButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.HighlightButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.LeftButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.CenterButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.RightButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.BulletsButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                SkinnableToolbar.Width = _RuntimeSkin.PositionToolbar.Width;
                SkinnableToolbar.Height = _RuntimeSkin.PositionToolbar.Height;

                //footer
                x = 0;
                width = _RuntimeSkin.BitmapMarks.Width / _RuntimeSkin.MarksCount;
                Canvas.SetLeft(SkinnableFooter, _RuntimeSkin.PositionMarks.X);
                Canvas.SetTop(SkinnableFooter, _RuntimeSkin.PositionMarks.Y);
                SkinnableFooter.ScheduleButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.ChangeButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.ProtectedButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.PriorityButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.CompleteButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.PasswordButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.PinButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.MailButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.EncryptedButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                SkinnableFooter.Width = _RuntimeSkin.PositionMarks.Width;
                SkinnableFooter.Height = _RuntimeSkin.PositionMarks.Height;

                BorderThickness = new Thickness(0);
                pnlkSkinless.Visibility = Visibility.Hidden;
                cnvSkin.Visibility = Visibility.Visible;
                Background = new SolidColorBrush(Colors.Transparent);
                ResizeMode = ResizeMode.NoResize;
                lnSizeEast.Visibility =
                    lnSizeNorth.Visibility =
                        lnSizeSouth.Visibility =
                            lnSizeWest.Visibility =
                                rectSizeNorthEast.Visibility =
                                    rectSizeNorthWest.Visibility =
                                        rectSizeSouthWest.Visibility =
                                            gripSize.Visibility = Visibility.Hidden;
                if (!fromLoad)
                {
                    updateThumbnail();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal bool InDockProcess { get; set; }

        internal Guid Handle { get; private set; }

        internal bool IsDisposed
        {
            get { return _HwndSource.IsDisposed; }
        }

        internal bool InAlarm
        {
            get { return _InAlarm; }
            set
            {
                var prevAlarm = _InAlarm;
                _InAlarm = value;
                if (_InAlarm && !prevAlarm)
                {
                    startAlarm();
                }
                else
                {
                    stopAlarm();
                }
            }
        }

        internal bool Active
        {
            get { return _Active; }
        }

        internal PNRichEditBox Edit
        {
            get { return _Edit; }
        }

        internal ContextMenu EditMenu
        {
            get { return ctmEdit; }
        }

        internal ContextMenu NoteMenu
        {
            get { return ctmNote; }
        }

        internal IHeader PHeader { get; set; }

        internal IFooter PFooter { get; set; }

        internal IToolbar PToolbar { get; set; }

        #endregion

        #region Window procedures
        private void Window_Activated(object sender, EventArgs e)
        {
            try
            {
                if (PNStatic.Settings.GeneralSettings.UseSkins || !PNStatic.Settings.Behavior.RollOnDblClick)
                {
                    _Edit.Focus();
                    System.Windows.Forms.Application.DoEvents();
                }
                else
                {
                    activateWindow();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || PNStatic.FindParent<SkinlessHeader>(e.OriginalSource) == null)
                return;
            if (PNStatic.Settings.Behavior.RollOnDblClick)
            {
                e.Handled = true;
                mnuRollUnroll.PerformClick();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_InAlarm)
            {
                InAlarm = false;
            }

            var resizeMode = ResizeMode;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null || note.DockStatus != DockStatus.None) return;
                if (PNStatic.Settings.Behavior.PreventAutomaticResizing)
                {
                    ResizeMode = ResizeMode.NoResize;
                }

                e.Handled = true;

                PNInterop.DragWindow(_HwndSource.Handle);

                PNNotesOperations.SaveNoteLocation(note, this.GetLocation());

                if (PNStatic.Settings.Behavior.PreventAutomaticResizing)
                {
                    ResizeMode = resizeMode;
                }

                if (PNStatic.Settings.GeneralSettings.UseSkins || !PNStatic.Settings.Behavior.RollOnDblClick)
                {
                    _Edit.Focus();
                }
                else
                {
                    activateWindow();
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                activateWindow();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var invertTextColor = false;
                var dockFromLoad = true;
                prepareControls();
                createDaysOfWeekArray();

                _StopAlarmTimer.Elapsed += m_StopAlarmTimer_Elapsed;

                //hide possible hidden menus
                PNStatic.HideMenus(ctmNote, PNStatic.HiddenMenus.Where(hm => hm.Type == MenuType.Note).ToArray());
                PNStatic.HideMenus(ctmEdit, PNStatic.HiddenMenus.Where(hm => hm.Type == MenuType.Edit).ToArray());

                //check buttons size
                if (!PNStatic.Settings.GeneralSettings.UseSkins &&
                    PToolbar.GetButtonSize() != PNStatic.Settings.GeneralSettings.ButtonsSize)
                {
                    SetFooterButtonSize(PNStatic.Settings.GeneralSettings.ButtonsSize);
                }

                if (PNStatic.Settings.GeneralSettings.HideToolbar)
                {
                    PToolbar.Visibility = Visibility.Collapsed;
                }

                Opacity = !_Note.CustomOpacity ? PNStatic.Settings.Behavior.Opacity : _Note.Opacity;

                var noteGroup = PNStatic.Groups.GetGroupByID(_Note.GroupID) ?? PNStatic.Groups.GetGroupByID(0);

                if (!PNStatic.Settings.GeneralSettings.UseSkins)
                {
                    if (_Note.Skinless == null)
                    {
                        PHeader.SetPNFont(noteGroup.Skinless.CaptionFont);
                        if (!PNStatic.Settings.Behavior.RandomBackColor || _Note.FromDB)
                        {
                            Foreground =
                                new SolidColorBrush(noteGroup.Skinless.CaptionColor);
                            Background =
                                new SolidColorBrush(noteGroup.Skinless.BackColor);
                        }
                        else
                        {
                            var clr = randomColor();
                            Background = new SolidColorBrush(clr);
                            _Note.Skinless =
                                (PNSkinlessDetails)PNStatic.Groups.GetGroupByID(_Note.GroupID).Skinless.Clone();
                            _Note.Skinless.BackColor = clr;
                            if (PNStatic.Settings.Behavior.InvertTextColor)
                            {
                                clr = invertColor(((SolidColorBrush)Background).Color);
                                Foreground = new SolidColorBrush(clr);
                                _Note.Skinless.CaptionColor = clr;
                                invertTextColor = true;
                            }
                        }
                    }
                    else
                    {
                        PHeader.SetPNFont(_Note.Skinless.CaptionFont);
                        Foreground = new SolidColorBrush(_Note.Skinless.CaptionColor);
                        Background = new SolidColorBrush(_Note.Skinless.BackColor);
                    }
                    if (!PNStatic.Settings.GeneralSettings.AutoHeight)
                    {
                        if (_Note.NoteSize == Size.Empty)
                        {
                            this.SetSize(PNStatic.Settings.GeneralSettings.Width,
                                PNStatic.Settings.GeneralSettings.Height);
                        }
                        else
                        {
                            this.SetSize(_Note.NoteSize);
                        }
                    }
                    else
                    {
                        Width = _Note.NoteSize == Size.Empty
                            ? PNStatic.Settings.GeneralSettings.Width
                            : _Note.NoteSize.Width;
                    }
                }
                else
                {
                    PNSkinsOperations.ApplyNoteSkin(this, _Note, true);
                }

                //set note location
                if (!_Note.NoteLocation.IsEmpty())
                {
                    //if (!m_Note.Thumbnail)
                    _Note.PlaceOnScreen();
                }
                else if (!_Note.FromDB && PNStatic.Settings.Behavior.StartPosition != NoteStartPosition.Center &&
                         _Note.NoteLocation.IsEmpty())
                {
                    switch (PNStatic.Settings.Behavior.StartPosition)
                    {
                        case NoteStartPosition.Left:
                            _Note.DockStatus = DockStatus.Left;
                            dockFromLoad = false;
                            break;
                        case NoteStartPosition.Top:
                            _Note.DockStatus = DockStatus.Top;
                            dockFromLoad = false;
                            break;
                        case NoteStartPosition.Right:
                            _Note.DockStatus = DockStatus.Right;
                            dockFromLoad = false;
                            break;
                        case NoteStartPosition.Bottom:
                            _Note.DockStatus = DockStatus.Bottom;
                            dockFromLoad = false;
                            break;
                    }
                    //store initial center position for possible future undocking
                    var wr = System.Windows.Forms.Screen.FromHandle(_HwndSource.Handle).WorkingArea;
                    _Note.NoteLocation = new Point((wr.Width - PNSettings.DEF_WIDTH) / 2.0,
                        (wr.Height - PNSettings.DEF_HEIGHT) / 2.0);
                    _Note.NoteSize = new Size(PNSettings.DEF_WIDTH, PNSettings.DEF_HEIGHT);
                }

                if (!_Note.FromDB)
                {
                    if (m_InputString == "")
                    {
                        _Edit.SetFontByFont(noteGroup.Font);
                        _Edit.SelectionColor = noteGroup.FontColor;
                        if (_Mode == NewNoteMode.Clipboard)
                        {
                            _Edit.Paste();
                        }
                    }
                    else
                    {
                        switch (_Mode)
                        {
                            case NewNoteMode.Identificator:
                                string path = Path.Combine(PNPaths.Instance.DataDir, m_InputString) +
                                              PNStrings.NOTE_EXTENSION;
                                if (File.Exists(path))
                                    _Edit.LoadFile(path, System.Windows.Forms.RichTextBoxStreamType.RichText);
                                break;
                            case NewNoteMode.File:
                                if (File.Exists(m_InputString))
                                    _Edit.LoadFile(m_InputString, System.Windows.Forms.RichTextBoxStreamType.RichText);
                                break;
                            case NewNoteMode.Clipboard:
                                break;
                        }
                    }
                    if (invertTextColor)
                    {
                        var brush = (SolidColorBrush)Foreground;
                        _Edit.SelectionColor = System.Drawing.Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G,
                            brush.Color.B);
                    }
                    _Note.Topmost = PNStatic.Settings.Behavior.NewNoteAlwaysOnTop;
                }
                else
                {
                    var path = Path.Combine(PNPaths.Instance.DataDir, m_InputString) + PNStrings.NOTE_EXTENSION;
                    if (File.Exists(path))
                        PNNotesOperations.LoadNoteFile(_Edit, path);
                }

                if (PNStatic.Settings.GeneralSettings.HideDeleteButton)
                {
                    PHeader.SetButtonVisibility(HeaderButtonType.Delete, Visibility.Collapsed);
                }
                if (PNStatic.Settings.GeneralSettings.HideHideButton)
                {
                    PHeader.SetButtonVisibility(HeaderButtonType.Hide, Visibility.Collapsed);
                }
                PHeader.SetButtonVisibility(HeaderButtonType.Panel,
                    PNStatic.Settings.Behavior.ShowNotesPanel ? Visibility.Visible : Visibility.Collapsed);

                if (PNStatic.Settings.GeneralSettings.HideDeleteButton &&
                    PNStatic.Settings.GeneralSettings.ChangeHideToDelete)
                {
                    PHeader.SetAlternative(true);
                }

                PFooter.SetMarkButtonVisibility(MarkType.Schedule, _Note.Schedule.Type != ScheduleType.None);
                PFooter.SetMarkButtonVisibility(MarkType.Priority, _Note.Priority);
                PFooter.SetMarkButtonVisibility(MarkType.Protection, _Note.Protected);
                PFooter.SetMarkButtonVisibility(MarkType.Pin, _Note.Pinned);
                PFooter.SetMarkButtonVisibility(MarkType.Password, _Note.PasswordString.Length > 0);
                PFooter.SetMarkButtonVisibility(MarkType.Complete, _Note.Completed);
                PFooter.SetMarkButtonVisibility(MarkType.Mail,
                    (_Note.SentReceived & SendReceiveStatus.Received) == SendReceiveStatus.Received ||
                    (_Note.SentReceived & SendReceiveStatus.Sent) == SendReceiveStatus.Sent);
                PFooter.SetMarkButtonVisibility(MarkType.Encrypted, _Note.Scrambled);
                applyNoteLanguage();

                if (!PNStatic.Settings.GeneralSettings.AutoHeight &&
                    PNStatic.Settings.GeneralSettings.ShowScrollbar != System.Windows.Forms.RichTextBoxScrollBars.None)
                {
                    _Edit.ScrollBars = PNStatic.Settings.GeneralSettings.ShowScrollbar;
                }
                _Edit.WordWrap = _Edit.ScrollBars != System.Windows.Forms.RichTextBoxScrollBars.Horizontal &&
                                 _Edit.ScrollBars != System.Windows.Forms.RichTextBoxScrollBars.Both;

                if (!PNStatic.Settings.GeneralSettings.UseSkins)
                {
                    _Edit.SetMargins(PNStatic.Settings.GeneralSettings.MarginWidth);
                }
                _Edit.ReadOnly = _Note.Protected || _Note.Scrambled;
                _Edit.CheckSpellingAutomatically = PNStatic.Settings.GeneralSettings.SpellMode == 1;
                _Edit.Modified = false;

                if (!PNStatic.Settings.GeneralSettings.UseSkins && PNStatic.Settings.GeneralSettings.AutoHeight)
                {
                    resizeOnAutoheight();
                }

                if (_Note.Rolled && !PNStatic.Settings.GeneralSettings.UseSkins)
                {
                    rollUnrollNote(_Note, true);
                }

                PNInterop.BringWindowToTop(_HwndSource.Handle);
                _Loaded = true;

                if (_Mode == NewNoteMode.Duplication || _Mode == NewNoteMode.Clipboard)
                {
                    setChangedSign(_Note);
                }

                if (_Note.DockStatus != DockStatus.None)
                {
                    DockNote(_Note, _Note.DockStatus, dockFromLoad);
                }

                if (_Note.Schedule.Type != ScheduleType.None && !_Note.Timer.Enabled)
                {
                    _Note.Timer.Start();
                }

                setScheduleTooltip(_Note);

                PNMenus.CheckAndApplyNewMenusOrder(ctmNote);
                PNMenus.CheckAndApplyNewMenusOrder(ctmEdit);

                PNStatic.FormMain.LanguageChanged += FormMain_LanguageChanged;
                PNStatic.FormMain.HotKeysChanged += FormMain_HotKeysChanged;
                PNStatic.FormMain.SpellCheckingStatusChanged += FormMain_SpellCheckingStatusChanged;
                PNStatic.FormMain.SpellCheckingDictionaryChanged += FormMain_SpellCheckingDictionaryChanged;
                PNStatic.FormMain.NoteScheduleChanged += FormMain_NoteScheduleChanged;
                PNStatic.FormMain.MenusOrderChanged += FormMain_MenusOrderChanged;

                if (_Note.DockStatus == DockStatus.None)
                {
                    Topmost = _Note.Topmost;
                }
                if (_Note.Thumbnail && PNStatic.Settings.Behavior.ShowNotesPanel)
                {
                    SetThumbnail();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _Edit.Focus();
            }
        }

        void TopAnimationThumbnail_Completed(object sender, EventArgs e)
        {
            BeginAnimation(TopProperty, null);
            Top = -(SystemParameters.WorkArea.Height + 1);
        }

        void LeftAnimationThumbnail_Completed(object sender, EventArgs e)
        {
            BeginAnimation(LeftProperty, null);
            Left = -(SystemParameters.WorkArea.Width + 1);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                removeControls();
                PNStatic.FormMain.LanguageChanged -= FormMain_LanguageChanged;
                PNStatic.FormMain.HotKeysChanged -= FormMain_HotKeysChanged;
                PNStatic.FormMain.SpellCheckingStatusChanged -= FormMain_SpellCheckingStatusChanged;
                PNStatic.FormMain.SpellCheckingDictionaryChanged -= FormMain_SpellCheckingDictionaryChanged;
                PNStatic.FormMain.NoteScheduleChanged -= FormMain_NoteScheduleChanged;
                PNStatic.FormMain.MenusOrderChanged -= FormMain_MenusOrderChanged;
                _RuntimeSkin.Dispose();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var handle = (new WindowInteropHelper(this)).Handle;
            _HwndSource = HwndSource.FromHwnd(handle);
            if (PNStatic.Settings.Behavior.DoNotShowNotesInList)
            {
                int exStyle = PNInterop.GetWindowLong(handle, PNInterop.GWL_EXSTYLE);
                exStyle |= PNInterop.WS_EX_TOOLWINDOW;
                PNInterop.SetWindowLong(handle, PNInterop.GWL_EXSTYLE, exStyle);
            }
            if (PNStatic.Settings.Behavior.KeepVisibleOnShowDesktop)
            {
                PNStatic.ToggleAeroPeek(handle, true);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _Closing = true;
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null || note.DockStatus == DockStatus.None) return;
                var wa = System.Windows.Forms.Screen.GetWorkingArea(new System.Drawing.Point((int)Left, (int)Top));
                switch (note.DockStatus)
                {
                    case DockStatus.Left:
                        if ((Top + Height >= wa.Bottom)
                            &&
                            (PNStatic.DockedNotes[note.DockStatus].Any(
                                n => (n.Dialog.Top + n.Dialog.Height) > wa.Bottom)))
                        {
                            PNStatic.DockArrows[DockArrow.LeftDown].Show();
                        }
                        else if ((Top <= wa.Top) &&
                                 (PNStatic.DockedNotes[note.DockStatus].Any(n => n.Dialog.Top < wa.Top)))
                        {
                            PNStatic.DockArrows[DockArrow.LeftUp].Show();
                        }
                        break;
                    case DockStatus.Top:
                        if ((Left + Width >= wa.Right)
                            &&
                            (PNStatic.DockedNotes[note.DockStatus].Any(
                                n => (n.Dialog.Left + n.Dialog.Width) > wa.Right)))
                        {
                            PNStatic.DockArrows[DockArrow.TopRight].Show();
                        }
                        else if ((Left <= wa.Left) &&
                                 (PNStatic.DockedNotes[note.DockStatus].Any(n => n.Dialog.Left < wa.Left)))
                        {
                            PNStatic.DockArrows[DockArrow.TopLeft].Show();
                        }
                        break;
                    case DockStatus.Right:
                        if ((Top + Height >= wa.Bottom)
                            &&
                            (PNStatic.DockedNotes[note.DockStatus].Any(
                                n => (n.Dialog.Top + n.Dialog.Height) > wa.Bottom)))
                        {
                            PNStatic.DockArrows[DockArrow.RightDown].Show();
                        }
                        else if ((Top <= wa.Top) &&
                                 (PNStatic.DockedNotes[note.DockStatus].Any(n => n.Dialog.Top < wa.Top)))
                        {
                            PNStatic.DockArrows[DockArrow.RightUp].Show();
                        }
                        break;
                    case DockStatus.Bottom:
                        if ((Left + Width >= wa.Right)
                            &&
                            (PNStatic.DockedNotes[note.DockStatus].Any(
                                n => (n.Dialog.Left + n.Dialog.Width) > wa.Right)))
                        {
                            PNStatic.DockArrows[DockArrow.BottomRight].Show();
                        }
                        else if ((Left <= wa.Left) &&
                                 (PNStatic.DockedNotes[note.DockStatus].Any(n => n.Dialog.Left < wa.Left)))
                        {
                            PNStatic.DockArrows[DockArrow.BottomLeft].Show();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                var r = new Rect(this.GetLocation(), this.GetSize());
                var point = System.Windows.Forms.Control.MousePosition;
                var pw = new Point(point.X, point.Y);
                if (r.Contains(pw)) return;
                var note = PNStatic.Notes.Note(Handle);
                if (note == null || note.DockStatus == DockStatus.None) return;
                switch (note.DockStatus)
                {
                    case DockStatus.Left:
                        if (PNStatic.DockArrows[DockArrow.LeftUp].IsVisible)
                            PNStatic.DockArrows[DockArrow.LeftUp].Hide();
                        if (PNStatic.DockArrows[DockArrow.LeftDown].IsVisible)
                            PNStatic.DockArrows[DockArrow.LeftDown].Hide();
                        break;
                    case DockStatus.Top:
                        if (PNStatic.DockArrows[DockArrow.TopLeft].IsVisible)
                            PNStatic.DockArrows[DockArrow.TopLeft].Hide();
                        if (PNStatic.DockArrows[DockArrow.TopRight].IsVisible)
                            PNStatic.DockArrows[DockArrow.TopRight].Hide();
                        break;
                    case DockStatus.Right:
                        if (PNStatic.DockArrows[DockArrow.RightUp].IsVisible)
                            PNStatic.DockArrows[DockArrow.RightUp].Hide();
                        if (PNStatic.DockArrows[DockArrow.RightDown].IsVisible)
                            PNStatic.DockArrows[DockArrow.RightDown].Hide();
                        break;
                    case DockStatus.Bottom:
                        if (PNStatic.DockArrows[DockArrow.BottomLeft].IsVisible)
                            PNStatic.DockArrows[DockArrow.BottomLeft].Hide();
                        if (PNStatic.DockArrows[DockArrow.BottomRight].IsVisible)
                            PNStatic.DockArrows[DockArrow.BottomRight].Hide();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Size procedures
        void OnSizeSouth(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(PNBorderDragDirection.Down);
            e.Handled = true;
        }
        void OnSizeNorth(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(PNBorderDragDirection.Up);
            e.Handled = true;
        }
        void OnSizeEast(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(PNBorderDragDirection.Right);
            e.Handled = true;
        }
        void OnSizeWest(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(PNBorderDragDirection.Left);
            e.Handled = true;
        }
        void OnSizeNorthWest(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(PNBorderDragDirection.WestNorth);
            e.Handled = true;
        }
        void OnSizeNorthEast(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(PNBorderDragDirection.EastNorth);
            e.Handled = true;
        }
        void OnSizeSouthEast(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(PNBorderDragDirection.EastSouth);
            e.Handled = true;
        }
        void OnSizeSouthWest(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(PNBorderDragDirection.WestSouth);
            e.Handled = true;
        }

        private void resizeWindow(PNBorderDragDirection direction)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null || note.Rolled || note.DockStatus != DockStatus.None) return;
                if (PNStatic.Settings.GeneralSettings.AutoHeight &&
                    (direction != PNBorderDragDirection.Left && direction != PNBorderDragDirection.Right)) return;
                _InResize = true;
                PNInterop.ResizeWindowByBorder(direction, _HwndSource.Handle);
                _InResize = false;
                if (PNStatic.Settings.GeneralSettings.AutoHeight)
                {
                    resizeOnAutoheight();
                }
                _Edit.Refresh();
                PNNotesOperations.SaveNoteSize(note, this.GetSize(), _Edit.Size);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _InResize = false;
            }
        }

        #endregion

        #region Timers
        private delegate void _Elapsed(object sender, ElapsedEventArgs e);

        private void m_StopAlarmTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _StopAlarmTimer.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    _Elapsed d = m_StopAlarmTimer_Elapsed;
                    Dispatcher.Invoke(d, sender, e);
                }
                else
                {
                    if (_InAlarm)
                    {
                        InAlarm = false;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Event handlers

        private void OnBackgroundChanged(object sender, EventArgs e)
        {
            try
            {
                if (PNStatic.Settings.GeneralSettings.UseSkins) return;
                var brush = Background as SolidColorBrush;
                if (brush == null) return;
                var clr = System.Drawing.Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G,
                    brush.Color.B);
                _EditControl.WinForm.BackColor = clr;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_NoteScheduleChanged(object sender, EventArgs e)
        {
            try
            {
                var nt = PNStatic.Notes.Note(Handle);
                var note = sender as PNote;
                if (note != null && nt != null && note.ID == nt.ID)
                {
                    PFooter.SetMarkButtonVisibility(MarkType.Schedule, note.Schedule.Type != ScheduleType.None);
                    setScheduleTooltip(note);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_HotKeysChanged(object sender, EventArgs e)
        {
            try
            {
                foreach (var ti in ctmNote.Items.OfType<MenuItem>())
                {
                    applyNoteHotkeys(ti, HotkeyType.Note);
                }

                foreach (var ti in ctmEdit.Items.OfType<MenuItem>())
                {
                    applyNoteHotkeys(ti, HotkeyType.Edit);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_LanguageChanged(object sender, EventArgs e)
        {
            applyNoteLanguage();
            PNMenus.PrepareDefaultMenuStrip(ctmNote, MenuType.Note, false);
            PNMenus.PrepareDefaultMenuStrip(ctmNote, MenuType.Note, true);
            PNMenus.PrepareDefaultMenuStrip(ctmEdit, MenuType.Edit, false);
            PNMenus.PrepareDefaultMenuStrip(ctmEdit, MenuType.Edit, true);
        }

        private void FormMain_SpellCheckingStatusChanged(object sender, SpellCheckingStatusChangedEventArgs e)
        {
            _Edit.CheckSpellingAutomatically = e.Status;
            _Edit.Invalidate();
            UpdateLayout();
        }

        private void FormMain_SpellCheckingDictionaryChanged(object sender, EventArgs e)
        {
            _Edit.Invalidate();
            UpdateLayout();
        }

        private void FormMain_MenusOrderChanged(object sender, MenusOrderChangedEventArgs e)
        {
            if (e.Note)
                rearrangeMenu(ctmNote, MenuType.Note);
            if (e.Edit)
                rearrangeMenu(ctmEdit, MenuType.Edit);
        }
        #endregion

        #region Private procedures

        private void activateWindow()
        {
            try
            {
                if (_Closing) return;
                _Active = true;
                Opacity = 1.0;
                if (!PNStatic.Settings.GeneralSettings.HideToolbar)
                {
                    PToolbar.Visibility = Visibility.Visible;
                }

                if (_InAlarm)
                {
                    InAlarm = false;
                }

                PNStatic.DeactivateNotesWindows(Handle);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private DrawingBrush takeSnapshot()
        {
            var dwb = new DrawingBrush();
            try
            {
                var dg = new DrawingGroup();
                dg.Children.Add(new GeometryDrawing
                {
                    Brush = new VisualBrush { Visual = this },
                    Geometry = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight))
                });
                using (var bmp = new System.Drawing.Bitmap(_Edit.Width, _Edit.Height))
                {
                    var pt = PointFromScreen(new Point(_EditControl.WinForm.Left, _EditControl.WinForm.Top));
                    _Edit.PrintToBitmap(bmp);
                    //_Edit.DrawToBitmap(bmp, new System.Drawing.Rectangle((int)pt.X, (int)pt.Y, _Edit.Width, _Edit.Height));

                    var bsource = PNStatic.ImageFromDrawingImage(bmp);
                    dg.Children.Add(new GeometryDrawing
                    {
                        Brush = new ImageBrush(bsource),
                        Geometry = new RectangleGeometry(new Rect(pt.X, pt.Y, _Edit.Width, _Edit.Height))
                    });
                    dwb.Drawing = dg;
                }

                return dwb;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return dwb;
            }
        }

        private void startAlarm()
        {
            try
            {
                if (_StopAlarmTimer.Enabled)
                {
                    _StopAlarmTimer.Stop();
                }
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;

                if (note.DockStatus != DockStatus.None)
                {
                    UndockNote(note);
                }

                if (note.Thumbnail && PNStatic.Settings.Behavior.ShowNotesPanel)
                {
                    PNStatic.FormPanel.RemoveThumbnail(note);
                }

                if (note.Rolled)
                {
                    rollUnrollNote(note, false);
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Roll, false, null);
                }

                if (PNStatic.Settings.Schedule.CenterScreen)
                {
                    var wa = System.Windows.Forms.Screen.GetWorkingArea(new System.Drawing.Point((int)Left, (int)Top));
                    this.SetLocation(new Point(wa.Left + (wa.Width - Width) / 2, wa.Top + (wa.Height - Height) / 2));
                }

                if (PNStatic.Settings.Schedule.VisualNotification)
                {
                    _DlgAlarm = new WndAlarm { Owner = this };
                    _DlgAlarm.Show();
                }

                if (PNStatic.Settings.Schedule.AllowSoundAlert)
                {
                    if (!note.Schedule.UseTts)
                    {
                        PNStatic.PlayAlarmSound(note.Schedule.Sound, note.Schedule.SoundInLoop);
                    }
                    else
                    {
                        PNStatic.SpeakNote(note, note.Schedule.Sound);
                    }
                }

                if (!string.IsNullOrEmpty(note.Schedule.ProgramToRunOnAlert))
                {
                    var ext = PNStatic.Externals.FirstOrDefault(p => p.Name == note.Schedule.ProgramToRunOnAlert);
                    if (ext != null)
                        PNStatic.RunExternalProgram(ext.Program, ext.CommandLine);
                }

                if (note.Schedule.StopAfter > 0)
                {
                    _StopAlarmTimer.Interval = note.Schedule.StopAfter;
                    _StopAlarmTimer.Start();
                }

                switch (note.Schedule.Type)
                {
                    case ScheduleType.Once:
                    case ScheduleType.After:
                        note.Schedule = new PNNoteSchedule();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void stopAlarm()
        {
            try
            {
                PNSound.StopSound();
                if (_DlgAlarm != null)
                {
                    _DlgAlarm.Close();
                    _DlgAlarm = null;
                }
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                if (note.Schedule.CloseOnNotification)
                {
                    if (note.Schedule.Type != ScheduleType.None && note.Schedule.Type != ScheduleType.Once &&
                        note.Schedule.Type != ScheduleType.After)
                    {
                        ApplyHideNote(note);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void StoryboardClose_Completed(object sender, EventArgs e)
        {
            var note = PNStatic.Notes.Note(Handle);
            if (note != null)
                note.Dialog = null;
            Close();
            if (note == null) return;
            if (note.Thumbnail && PNStatic.Settings.Behavior.ShowNotesPanel)
            {
                PNStatic.FormPanel.RemoveThumbnail(note);
            }
        }

        private void initializaFields()
        {
            try
            {
                Handle = Guid.NewGuid();
                //add handler for background changed event
                _BackgroundDescriptor =
                        DependencyPropertyDescriptor.FromProperty(BackgroundProperty, typeof(Window));
                _BackgroundDescriptor.AddValueChanged(this, OnBackgroundChanged);

                initializeEdit();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadLogFonts()
        {
            try
            {
                PToolbar.LoadLogFonts();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void saveBackCopy(PNote note)
        {
            try
            {
                var di = new DirectoryInfo(PNPaths.Instance.BackupDir);
                FileInfo[] fis = di.GetFiles(note.ID + "*" + PNStrings.NOTE_BACK_EXTENSION);
                int lenght = fis.Length;
                if (lenght == 0)
                {
                    //just save the first back copy
                    string src = Path.Combine(PNPaths.Instance.DataDir, note.ID) + PNStrings.NOTE_EXTENSION;
                    string dest = Path.Combine(PNPaths.Instance.BackupDir, note.ID) + "_1" +
                                  PNStrings.NOTE_BACK_EXTENSION;
                    if (File.Exists(src))
                    {
                        File.Copy(src, dest, true);
                    }
                }
                else if (lenght < PNStatic.Settings.Protection.BackupDeepness)
                {
                    //shift all copies backward and save current note as first
                    fis = fis.OrderByDescending(f => f.Name).ToArray();
                    int pfx = lenght + 1;
                    string dest;
                    foreach (FileInfo fi in fis)
                    {
                        dest = Path.Combine(PNPaths.Instance.BackupDir, note.ID) + "_" +
                               pfx.ToString(PNStatic.CultureInvariant) +
                               PNStrings.NOTE_BACK_EXTENSION;
                        File.Move(fi.FullName, dest);
                        pfx--;
                    }
                    string src = Path.Combine(PNPaths.Instance.DataDir, note.ID) + PNStrings.NOTE_EXTENSION;
                    dest = Path.Combine(PNPaths.Instance.BackupDir, note.ID) + "_1" + PNStrings.NOTE_BACK_EXTENSION;
                    if (File.Exists(src))
                    {
                        File.Copy(src, dest, true);
                    }
                }
                else
                {
                    //remove last copy, shift all copies backward and save current note as first
                    fis = fis.OrderByDescending(f => f.Name).ToArray();
                    File.Delete(fis[0].FullName);
                    int pfx = PNStatic.Settings.Protection.BackupDeepness;
                    string dest;
                    for (int i = 1; i < lenght; i++)
                    {
                        dest = Path.Combine(PNPaths.Instance.BackupDir, note.ID) + "_" +
                               pfx.ToString(PNStatic.CultureInvariant) +
                               PNStrings.NOTE_BACK_EXTENSION;
                        File.Move(fis[i].FullName, dest);
                        pfx--;
                    }
                    string src = Path.Combine(PNPaths.Instance.DataDir, note.ID) + PNStrings.NOTE_EXTENSION;
                    dest = Path.Combine(PNPaths.Instance.BackupDir, note.ID) + "_1" + PNStrings.NOTE_BACK_EXTENSION;
                    if (File.Exists(src))
                    {
                        File.Copy(src, dest, true);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool showSaveAsDialog(bool rename)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return false;
                if (!note.FromDB)
                {
                    if (note.GroupID != (int)SpecialGroups.Diary)
                    {
                        //prompt to save
                        string name = "", text;
                        switch (PNStatic.Settings.Behavior.DefaultNaming)
                        {
                            case DefaultNaming.FirstCharacters:
                                text = _Edit.Text;
                                text = text.Replace('\n', ' ').Replace('\r', ' ');
                                name = text.Length < PNStatic.Settings.Behavior.DefaultNameLength
                                    ? text
                                    : text.Substring(0, PNStatic.Settings.Behavior.DefaultNameLength);
                                break;
                            case DefaultNaming.DateTime:
                                name = DateTime.Now.ToString(PNStatic.Settings.GeneralSettings.DateFormat);
                                break;
                            case DefaultNaming.DateTimeAndFirstCharacters:
                                name = DateTime.Now.ToString(PNStatic.Settings.GeneralSettings.DateFormat);
                                name += " ";
                                text = _Edit.Text;
                                text = text.Replace('\n', ' ').Replace('\r', ' ');
                                if (text.Length < PNStatic.Settings.Behavior.DefaultNameLength)
                                {
                                    name += text;
                                }
                                else
                                {
                                    name += text.Substring(0, PNStatic.Settings.Behavior.DefaultNameLength);
                                }
                                break;
                        }
                        var dlgSaveAs = new WndSaveAs(name, note.GroupID) { Owner = this };
                        dlgSaveAs.SaveAsNoteNameSet += dlgSaveAs_SaveAsNoteNameSet;
                        var showDialog = dlgSaveAs.ShowDialog();
                        if (showDialog != null && showDialog.Value == false)
                        {
                            dlgSaveAs.SaveAsNoteNameSet -= dlgSaveAs_SaveAsNoteNameSet;
                            return false;
                        }
                    }
                    else
                    {
                        _SaveArgs = new SaveAsNoteNameSetEventArgs(note.Name, note.GroupID);
                    }
                }
                else
                {
                    if (rename)
                    {
                        var dlgSaveAs = new WndSaveAs(note.Name, note.GroupID) { Owner = this };
                        dlgSaveAs.SaveAsNoteNameSet += dlgSaveAs_SaveAsNoteNameSet;
                        var showDialog = dlgSaveAs.ShowDialog();
                        if (showDialog != null && showDialog.Value == false)
                        {
                            dlgSaveAs.SaveAsNoteNameSet -= dlgSaveAs_SaveAsNoteNameSet;
                            return false;
                        }
                    }
                    else
                    {
                        _SaveArgs = new SaveAsNoteNameSetEventArgs(note.Name, note.GroupID);
                    }
                }
                if (!SaveNoteFile(note)) return false;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Change, false, _SaveArgs);
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
            finally
            {
                ApplyTooltip();
            }
        }

        private void dlgSaveAs_SaveAsNoteNameSet(object sender, SaveAsNoteNameSetEventArgs e)
        {
            var note = PNStatic.Notes.Note(Handle);
            var dlgSaveAs = sender as WndSaveAs;
            if (dlgSaveAs != null) dlgSaveAs.SaveAsNoteNameSet -= dlgSaveAs_SaveAsNoteNameSet;
            PHeader.Title = Title = e.Name;
            _SaveArgs = new SaveAsNoteNameSetEventArgs(e.Name, e.GroupID);
            if (note != null && note.GroupID != e.GroupID)
            {
                PNGroup group = PNStatic.Groups.GetGroupByID(e.GroupID);
                if (group != null)
                {
                    PNNotesOperations.ChangeNoteLookOnGroupChange(this, group);
                }
            }
        }

        private void resizeOnAutoheight()
        {
            try
            {
                edit_ContentsResized(_Edit,
                    new System.Windows.Forms.ContentsResizedEventArgs(new System.Drawing.Rectangle(0, 0, _Edit.Width, getEditContentHeight() + (int)PFooter.ActualHeight)));
                scrollToFirstVisibleLine();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private int getEditContentHeight()
        {
            try
            {
                var fontHeight = _Edit.GetFontSize();

                if (_Edit.TextLength == 0) return fontHeight + 16; //_Edit.Height;

                var pos1 = _Edit.GetPositionFromCharIndex(0);
                var pos2 = _Edit.GetPositionFromCharIndex(_Edit.TextLength - 1);

                if (pos2.Y < pos1.Y) return fontHeight + 16; //_Edit.Height;

                var selStart = _Edit.SelectionStart;
                var selLength = _Edit.SelectionLength;
                _Edit.SelectionStart = _Edit.TextLength - 1;
                _Edit.SelectionLength = 0;
                //var fontHeight = _Edit.GetFontSize();
                _Edit.SelectionStart = selStart;
                _Edit.SelectionLength = selLength;

                return pos2.Y - pos1.Y + fontHeight + 16;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return _Edit.Height;
            }
        }

        private void scrollToFirstVisibleLine()
        {
            try
            {
                var selStart = _Edit.SelectionStart;
                var selLength = _Edit.SelectionLength;
                _Edit.SelectionStart = 0;
                _Edit.SelectionLength = 0;
                _Edit.ScrollToCaret();
                _Edit.SelectionStart = selStart;
                _Edit.SelectionLength = selLength;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }


        private System.Drawing.TextureBrush skinTextureBrush(int width, int height)
        {
            try
            {
                var pos = _Edit.GetPositionFromCharIndex(_Edit.SelectionStart);
                pos.X += _RuntimeSkin.PositionEdit.X;
                pos.Y += _RuntimeSkin.PositionEdit.Y;
                using (var bmp = new System.Drawing.Bitmap(width, height))
                {
                    using (var g = System.Drawing.Graphics.FromImage(bmp))
                    {
                        g.DrawImage(_RuntimeSkin.BitmapSkin, new System.Drawing.Rectangle(0, 0, width, height),
                            new System.Drawing.Rectangle(pos.X, pos.Y, width, height), System.Drawing.GraphicsUnit.Pixel);
                    }
                    return new System.Drawing.TextureBrush(bmp);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private System.Drawing.Image editBackgroundImage()
        {
            try
            {
                var bmp = new System.Drawing.Bitmap(_RuntimeSkin.BitmapSkin.Width, _RuntimeSkin.BitmapSkin.Height);
                using (var g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.DrawImage(_RuntimeSkin.BitmapSkin,
                        new System.Drawing.Rectangle(0, 0, _RuntimeSkin.BitmapSkin.Width,
                            _RuntimeSkin.BitmapSkin.Height),
                        new System.Drawing.Rectangle(_RuntimeSkin.PositionEdit.X, _RuntimeSkin.PositionEdit.Y,
                            _RuntimeSkin.BitmapSkin.Width, _RuntimeSkin.BitmapSkin.Height),
                        System.Drawing.GraphicsUnit.Pixel);
                }
                return bmp;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void insertImage(System.Drawing.Image image, bool isTransparent = false)
        {
            try
            {
                using (var bmp = new System.Drawing.Bitmap(image.Width, image.Height))
                {
                    using (var g = System.Drawing.Graphics.FromImage(bmp))
                    {
                        var clr = ((SolidColorBrush)Background).Color;
                        var backColor = System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
                        Brush br;
                        if (!PNStatic.Settings.GeneralSettings.UseSkins)
                            br = new System.Drawing.SolidBrush(backColor);
                        else
                            br = skinTextureBrush(image.Width, image.Height);
                        try
                        {
                            g.FillRectangle(br, 0, 0, bmp.Width, bmp.Height);
                            if (!isTransparent)
                            {
                                using (var b = new System.Drawing.Bitmap(image))
                                {
                                    var brush = br as System.Drawing.SolidBrush;
                                    if (brush != null)
                                        b.MakeTransparent(brush.Color);
                                    g.DrawImage(b, 0, 0);
                                }
                            }
                            else
                            {
                                g.DrawImage(image, 0, 0);
                            }
                        }
                        finally
                        {
                            if (br != null) br.Dispose();
                        }

                    }
                    _Edit.InsertImage(bmp);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void rearrangeMenu(ContextMenu ctm, MenuType type)
        {
            try
            {
                PNMenus.RearrangeMenus(ctm);
                PNMenus.PrepareDefaultMenuStrip(ctm, type, true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyNoteHotkeys(MenuItem ti, HotkeyType type)
        {
            try
            {
                foreach (var t in ti.Items.OfType<MenuItem>())
                {
                    applyNoteHotkeys(t, type);
                }
                PNHotKey hk;
                switch (type)
                {
                    case HotkeyType.Note:
                        hk = PNStatic.HotKeysNote.FirstOrDefault(h => h.MenuName == ti.Name);
                        if (hk != null && ti.InputGestureText == "")
                        {
                            ti.InputGestureText = hk.Shortcut;
                        }
                        break;
                    case HotkeyType.Edit:
                        hk = PNStatic.HotKeysEdit.FirstOrDefault(h => h.MenuName == ti.Name);
                        if (hk != null && ti.InputGestureText == "")
                        {
                            ti.InputGestureText = hk.Shortcut;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void playSoundOnHide()
        {
            try
            {
                var stream = Application.GetResourceStream(new Uri("sounds/dice.wav", UriKind.Relative));
                if (stream == null) return;
                var player = new SoundPlayer(stream.Stream);
                player.Play();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updateThumbnail()
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle) ?? _Note;
                if (note == null) return;
                if (!note.Thumbnail) return;

                var index = PNStatic.FormPanel.RemoveThumbnail(note, false);

                PNStatic.DoEvents();
                System.Windows.Forms.Application.DoEvents();

                NoteVisual = takeSnapshot();

                runThumbnailAnimation(note);

                PNStatic.FormPanel.Thumbnails.Insert(index, new ThumbnailButton { ThumbnailBrush = NoteVisual, Id = note.ID, ThumbnailName = note.Name });
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }


        private void runThumbnailAnimation(PNote note)
        {
            try
            {
                var stb = TryFindResource("ThumbnailStoryboard") as Storyboard;
                if (stb != null)
                {
                    var dbaLeft = stb.Children[0] as DoubleAnimation;
                    var dbaTop = stb.Children[1] as DoubleAnimation;
                    if (dbaLeft == null || dbaTop == null)
                    {
                        this.SetLocation(-(SystemParameters.WorkArea.Width + 1), -(SystemParameters.WorkArea.Height + 1));
                    }
                    else
                    {
                        switch (PNStatic.Settings.Behavior.NotesPanelOrientation)
                        {
                            case NotesPanelOrientation.Left:
                                dbaLeft.To = SystemParameters.WorkArea.Left - (note.NoteLocation.X + 1);
                                dbaTop.To = (SystemParameters.WorkArea.Height - note.NoteSize.Height) / 2;
                                break;
                            case NotesPanelOrientation.Top:
                                dbaLeft.To = (SystemParameters.WorkArea.Width - note.NoteSize.Width) / 2;
                                dbaTop.To = SystemParameters.WorkArea.Top - (note.NoteLocation.Y + 1);
                                break;
                        }
                        BeginStoryboard(stb);
                    }
                }
                else
                {
                    this.SetLocation(-(SystemParameters.WorkArea.Width + 1), -(SystemParameters.WorkArea.Height + 1));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setScheduleTooltip(PNote note)
        {
            try
            {
                PFooter.SetMarkButtonTooltip(MarkType.Schedule,
                    note.Schedule.Type != ScheduleType.None
                        ? PNLang.Instance.GetNoteScheduleDescription(note.Schedule, _DaysOfWeek)
                        : "");
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setChangedSign(PNote note)
        {
            try
            {
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Change, true, null);
                mnuSave.IsEnabled = true;
                if (!PFooter.IsMarkButtonVisible(MarkType.Change))
                {
                    PFooter.SetMarkButtonVisibility(MarkType.Change, true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void rollUnrollNote(PNote note, bool roll)
        {
            try
            {
                _InRoll = true;
                if (note.DockStatus != DockStatus.None)
                {
                    return;
                }
                if (roll)
                {
                    _EditControl.WinForm.Visible = false;
                    Height = MinHeight;
                    if (PNStatic.Settings.Behavior.FitWhenRolled)
                    {
                        Width = MinWidth;
                    }
                    IsRolled = true;
                }
                else
                {
                    this.SetSize(PNStatic.Settings.GeneralSettings.AutoHeight
                        ? new Size(note.NoteSize.Width, note.AutoHeight)
                        : note.NoteSize);
                    IsRolled = false;
                    _EditControl.WinForm.Visible = true;
                    var pt = this.GetLocation();
                    var totalWidth = System.Windows.Forms.Screen.AllScreens.Sum(sc => sc.WorkingArea.Width);
                    var diffX = (Left + note.NoteSize.Width) - totalWidth;
                    var diffY = (Top + note.NoteSize.Height) -
                                System.Windows.Forms.Screen.GetWorkingArea(new System.Drawing.Rectangle((int)Left, (int)Top, (int)Width,
                                    (int)Height)).Height;
                    if (diffX <= 0 && diffY <= 0) return;
                    if (diffX > 0)
                    {
                        pt.X -= diffX;
                    }
                    if (diffY > 0)
                    {
                        pt.Y -= diffY;
                    }
                    this.SetLocation(pt);
                    PNNotesOperations.SaveNoteLocation(note, this.GetLocation());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _InRoll = false;
            }
        }

        private Color randomColor()
        {
            var color = PNStatic.Groups.GetGroupByID(0).Skinless.BackColor;
            try
            {
                var rand = new Random();
                var bytes = new byte[3];
                rand.NextBytes(bytes);
                color = Color.FromArgb(255, bytes[0], bytes[1], bytes[2]);
                return color;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return color;
            }
        }

        private Color invertColor(Color srcColor)
        {
            try
            {
                return Color.FromArgb(255, (byte)~srcColor.R, (byte)~srcColor.G, (byte)~srcColor.B);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return srcColor;
            }
        }

        private void createDaysOfWeekArray()
        {
            try
            {
                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                var values = Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().ToArray();
                for (var i = 0; i < values.Length; i++)
                {
                    _DaysOfWeek[i] = new DayOfWeekStruct { DayOfW = values[i], Name = ci.DateTimeFormat.DayNames[i] };
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initializeEdit()
        {
            try
            {
                _EditControl = PNStatic.Settings.GeneralSettings.UseSkins
                    ? new EditControl(EditTargetSkinnable)
                    : new EditControl(EditTargetSkinless);
                _Edit = _EditControl.EditBox;
                _Edit.HideSelection = false;
                _Edit.MouseUp += edit_MouseUp;
                _Edit.ContentsResized += edit_ContentsResized;
                _Edit.DragDrop += edit_DragDrop;
                _Edit.KeyDown += edit_KeyDown;
                _Edit.KeyPress += edit_KeyPress;
                _Edit.LinkClicked += edit_LinkClicked;
                _Edit.PNREActivatedByMouse += edit_PNREActivatedByMouse;
                _Edit.TextChanged += edit_TextChanged;
                _Edit.GotFocus += edit_GotFocus;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void closeContextMenus()
        {
            try
            {
                if (ctmBullets.IsOpen) ctmBullets.IsOpen = false;
                if (ctmFontColor.IsOpen) ctmFontColor.IsOpen = false;
                if (ctmFontHighlight.IsOpen) ctmFontHighlight.IsOpen = false;
                if (ctmFontSize.IsOpen) ctmFontSize.IsOpen = false;
                if (ctmDrop.IsOpen) ctmDrop.IsOpen = false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyNoteLanguage()
        {
            try
            {
                PToolbar.SetButtonTooltip(FormatType.FontFamily,
                    PNLang.Instance.GetNoteString("cmdFontFamily", "Font family"));

                PToolbar.SetButtonTooltip(FormatType.FontSize,
                    PNLang.Instance.GetNoteString("cmdFontSize", "Font size"));
                PToolbar.SetButtonTooltip(FormatType.FontColor,
                    PNLang.Instance.GetNoteString("cmdFontColor", "Font color"));
                PToolbar.SetButtonTooltip(FormatType.FontBold,
                    PNLang.Instance.GetNoteString("cmdBold", "Bold") + " (Ctrl+B))");
                PToolbar.SetButtonTooltip(FormatType.FontItalic,
                    PNLang.Instance.GetNoteString("cmdItalic", "Italic") + " (Ctrl+I)");
                PToolbar.SetButtonTooltip(FormatType.FontUnderline,
                    PNLang.Instance.GetNoteString("cmdUnderline", "Underline") + " (Ctrl+U)");
                PToolbar.SetButtonTooltip(FormatType.FontStrikethrough,
                    PNLang.Instance.GetNoteString("cmdStrikethrough", "Strikethrough") + " (Ctrl+K)");
                PToolbar.SetButtonTooltip(FormatType.Highlight,
                    PNLang.Instance.GetNoteString("cmdHighlight", "Highlight"));
                PToolbar.SetButtonTooltip(FormatType.Left,
                    PNLang.Instance.GetNoteString("cmdLeft", "Left") + " (Ctrl+L)");
                PToolbar.SetButtonTooltip(FormatType.Center,
                    PNLang.Instance.GetNoteString("cmdCenter", "Center") + " (Ctrl+E)");
                PToolbar.SetButtonTooltip(FormatType.Right,
                    PNLang.Instance.GetNoteString("cmdRight", "Right") + " (Ctrl+R)");
                PToolbar.SetButtonTooltip(FormatType.Bullets,
                    PNLang.Instance.GetNoteString("cmdBullets", "Bullets"));
                PHeader.SetButtonTooltip(HeaderButtonType.Hide, PNLang.Instance.GetNoteString("cmdHide", "Hide"));
                PHeader.SetButtonTooltip(HeaderButtonType.Delete, PNLang.Instance.GetNoteString("cmdDelete", "Delete"));
                PHeader.SetButtonTooltip(HeaderButtonType.Panel, PNLang.Instance.GetNoteString("cmdPanel", "Put In Panel"));

                if (!_Note.FromDB)
                {
                    if (_Mode == NewNoteMode.Duplication || _Mode == NewNoteMode.Diary)
                    {
                        PHeader.Title = Title = _Note.Name;
                    }
                    else
                    {
                        PHeader.Title = Title = PNLang.Instance.GetNoteString("def_caption", "Untitled");
                    }
                }
                else
                {
                    PHeader.Title = Title = _Note.Name;
                }
                SetSendReceiveTooltip();
                PFooter.SetMarkButtonTooltip(MarkType.Password,
                    PNLang.Instance.GetControlText("cmdRemovePwrd", "Remove password"));
                applyMenusLanguage();
                ApplyTooltip();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyMenusLanguage()
        {
            try
            {
                foreach (var mi in ctmNote.Items.OfType<MenuItem>())
                    PNLang.Instance.ApplyMenuItemLanguage(mi, "note_menu");
                foreach (var mi in ctmEdit.Items.OfType<MenuItem>())
                    PNLang.Instance.ApplyMenuItemLanguage(mi, "edit_menu");
                foreach (var mi in ctmBullets.Items.OfType<MenuItem>())
                    PNLang.Instance.ApplyMenuItemLanguage(mi, "edit_menu");
                foreach (var mi in ctmFontColor.Items.OfType<MenuItem>())
                    PNLang.Instance.ApplyMenuItemLanguage(mi, "edit_menu");
                foreach (var mi in ctmFontHighlight.Items.OfType<MenuItem>())
                    PNLang.Instance.ApplyMenuItemLanguage(mi, "edit_menu");
                foreach (var mi in ctmDrop.Items.OfType<MenuItem>())
                    PNLang.Instance.ApplyMenuItemLanguage(mi, "insert_menu");
                foreach (var ti in ctmNote.Items.OfType<MenuItem>())
                    applyNoteHotkeys(ti, HotkeyType.Note);
                foreach (var ti in ctmEdit.Items.OfType<MenuItem>())
                    applyNoteHotkeys(ti, HotkeyType.Edit);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void prepareControls()
        {
            try
            {
                PToolbar.FontComboDropDownClosed += cboFonts_DropDownClosed;
                _EditControl.EditControlSizeChanged += _EditControl_EditControlSizeChanged;
                ctmEdit.DataContext =
                    ctmNote.DataContext =
                        ctmBullets.DataContext =
                            ctmFontSize.DataContext =
                                ctmFontHighlight.DataContext =
                                    ctmFontColor.DataContext = ctmDrop.DataContext = PNSingleton.Instance.FontUser;

                createBulletsMenu();
                createSizeMenu();
                createFontColorMenu();
                createFontHighlightMenu();

                loadLogFonts();

                applyMenusLanguage();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        void removeControls()
        {
            try
            {
                _BackgroundDescriptor.RemoveValueChanged(this, OnBackgroundChanged);

                foreach (var m in mnuBulletsNumbering.Items.OfType<MenuItem>())
                {
                    m.Click -= BulletsMenu_Click;
                }
                foreach (var m in mnuFontSize.Items.OfType<MenuItem>())
                {
                    m.Click -= SizeMenu_Click;
                }
                foreach (var m in mnuFontColor.Items.OfType<MenuItem>())
                {
                    m.Click -= FontColorMenu_Click;
                }
                foreach (var m in mnuHighlight.Items.OfType<MenuItem>())
                {
                    m.Click -= FontHighlightMenu_Click;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createFontHighlightMenu()
        {
            try
            {
                if (mnuHighlight.HasItems) return;
                foreach (
                    var item in
                        ctmFontHighlight.Items.OfType<MenuItem>()
                            .Select(
                                mi =>
                                    new MenuItem
                                    {
                                        Name = mi.Name,
                                        Header = mi.Header,
                                        Background = mi.Background
                                    }))
                {
                    item.Click += FontHighlightMenu_Click;
                    mnuHighlight.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createFontColorMenu()
        {
            try
            {
                if (mnuFontColor.HasItems) return;
                foreach (
                    var item in
                        ctmFontColor.Items.OfType<MenuItem>()
                            .Select(
                                mi =>
                                    new MenuItem
                                    {
                                        Name = mi.Name,
                                        Header = mi.Header,
                                        IsCheckable = mi.IsCheckable,
                                        Background = mi.Background
                                    }))
                {
                    item.Click += FontColorMenu_Click;
                    mnuFontColor.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createBulletsMenu()
        {
            try
            {
                if (mnuBulletsNumbering.HasItems) return;
                foreach (
                    var item in
                        ctmBullets.Items.OfType<MenuItem>()
                            .Select(
                                mi => new MenuItem { Name = mi.Name, Header = mi.Header, IsCheckable = mi.IsCheckable }))
                {
                    item.Click += BulletsMenu_Click;
                    mnuBulletsNumbering.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createSizeMenu()
        {
            try
            {
                if (mnuFontSize.HasItems) return;
                foreach (
                    var item in
                        ctmFontSize.Items.OfType<MenuItem>()
                            .Select(mi => new MenuItem { Header = mi.Header, IsCheckable = mi.IsCheckable }))
                {
                    item.Click += SizeMenu_Click;
                    mnuFontSize.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkFontSize()
        {
            try
            {
                var fontSize = _Edit.GetFontSize();
                foreach (var ti in ctmFontSize.Items.OfType<MenuItem>())
                {
                    ti.IsEnabled = !_Edit.ReadOnly;
                    ti.IsChecked = ti.Header.ToString() == fontSize.ToString(PNStatic.CultureInvariant);
                }
                foreach (var ti in mnuFontSize.Items.OfType<MenuItem>())
                {
                    ti.IsEnabled = !_Edit.ReadOnly;
                    ti.IsChecked = ti.Header.ToString() == fontSize.ToString(PNStatic.CultureInvariant);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkHighlight()
        {
            try
            {
                foreach (var mi in ctmFontHighlight.Items.OfType<MenuItem>())
                    mi.IsEnabled = !_Edit.ReadOnly;
                foreach (var mi in mnuHighlight.Items.OfType<MenuItem>())
                    mi.IsEnabled = !_Edit.ReadOnly;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkBullets()
        {
            try
            {
                var bstyle = _Edit.CurrentBulletStyle();
                var menuName = "";
                switch (bstyle)
                {
                    case 0:
                        menuName = "mnuNoBullets";
                        break;
                    case 1:
                        menuName = "mnuBullets";
                        break;
                    case 2:
                        menuName = "mnuArabicPoint";
                        break;
                    case 3:
                        menuName = "mnuArabicParts";
                        break;
                    case 4:
                        menuName = "mnuSmallLettersPoint";
                        break;
                    case 5:
                        menuName = "mnuSmallLettersPart";
                        break;
                    case 6:
                        menuName = "mnuBigLettersPoint";
                        break;
                    case 7:
                        menuName = "mnuBigLettersParts";
                        break;
                    case 8:
                        menuName = "mnuLatinSmall";
                        break;
                    case 9:
                        menuName = "mnuLatinBig";
                        break;
                }
                foreach (var mi in ctmBullets.Items.OfType<MenuItem>())
                {
                    mi.IsEnabled = !_Edit.ReadOnly;
                    mi.IsChecked = mi.Name == menuName;
                }
                foreach (var mi in mnuBulletsNumbering.Items.OfType<MenuItem>())
                {
                    mi.IsEnabled = !_Edit.ReadOnly;
                    mi.IsChecked = mi.Name == menuName;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkFontColor()
        {
            try
            {
                var found = false;
                var clr = _Edit.SelectionColor;
                var fontColor = Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
                foreach (var mi in ctmFontColor.Items.OfType<MenuItem>())
                {
                    mi.IsEnabled = !_Edit.ReadOnly;
                    mi.IsChecked = false;
                    if (found || ((SolidColorBrush)mi.Background).Color != fontColor) continue;
                    mi.IsChecked = true;
                    found = true;
                }
                if (!found)
                    ((MenuItem)ctmFontColor.Items[0]).IsChecked = true;
                found = false;
                foreach (var mi in mnuFontColor.Items.OfType<MenuItem>())
                {
                    mi.IsEnabled = !_Edit.ReadOnly;
                    mi.IsChecked = false;
                    if (found || ((SolidColorBrush)mi.Background).Color != fontColor) continue;
                    mi.IsChecked = true;
                    found = true;
                }
                if (!found)
                    ((MenuItem)mnuFontColor.Items[0]).IsChecked = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Header and footer events
        private void PHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //try
            //{
            //    var now = TimeSpan.FromTicks(DateTime.Now.Ticks);
            //    var diff = now - _ClickTime;
            //    _ClickTime = now;

            //    if (diff.TotalMilliseconds > SystemInformation.DoubleClickTime) return;

            //    e.Handled = true;
            //    if (PNStatic.Settings.Behavior.RollOnDblClick)
            //    {
            //        mnuRollUnroll.PerformClick();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    PNStatic.LogException(ex);
            //}
        }

        private void PHeader_HideDeleteButtonClicked(object sender, HeaderButtonClickedEventArgs e)
        {
            try
            {
                PNote note;
                switch (e.ButtonType)
                {
                    case HeaderButtonType.Delete:
                        note = PNStatic.Notes.Note(Handle);
                        if (note != null)
                        {
                            var type = PNNotesOperations.DeletionWarning(HotkeysStatic.LeftShiftDown(), 1,
                                note);
                            if (type != NoteDeleteType.None)
                            {
                                if (PNNotesOperations.DeleteNote(type, note))
                                {
                                    var sb = TryFindResource("FadeAway") as Storyboard;
                                    if (sb == null)
                                    {
                                        note.Dialog = null;
                                        Close();
                                        if (note.Thumbnail && PNStatic.Settings.Behavior.ShowNotesPanel)
                                        {
                                            PNStatic.FormPanel.RemoveThumbnail(note);
                                        }
                                    }
                                    else
                                    {
                                        sb.Begin();
                                    }
                                }
                            }
                        }
                        break;
                    case HeaderButtonType.Hide:
                        if (PNStatic.Settings.GeneralSettings.ChangeHideToDelete)
                        {
                            PHeader_HideDeleteButtonClicked(PHeader,
                                new HeaderButtonClickedEventArgs(HeaderButtonType.Delete));
                        }
                        else
                        {
                            note = PNStatic.Notes.Note(Handle);
                            if (note != null)
                            {
                                ApplyHideNote(note);
                            }
                        }
                        break;
                    case HeaderButtonType.Panel:
                        SetThumbnail();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void PFooter_MarkButtonClicked(object sender, MarkButtonClickedEventArgs e)
        {
            try
            {
                switch (e.Buttontype)
                {
                    case MarkType.Schedule:
                        if (mnuAdjustSchedule.IsEnabled)
                            mnuAdjustSchedule.PerformClick();
                        break;
                    case MarkType.Change:
                        mnuSave.PerformClick();
                        break;
                    case MarkType.Complete:
                        mnuMarkAsComplete.PerformClick();
                        break;
                    case MarkType.Encrypted:
                        mnuScramble.PerformClick();
                        break;
                    case MarkType.Mail:
                        break;
                    case MarkType.Password:
                        mnuRemovePassword.PerformClick();
                        break;
                    case MarkType.Pin:
                        if (PNStatic.Settings.Behavior.PinClickAction == PinClickAction.Toggle)
                        {
                            mnuUnpin.PerformClick();
                        }
                        else
                        {
                            var note = PNStatic.Notes.Note(Handle);
                            if (note != null)
                            {
                                _PinClass = new PinClass
                                {
                                    Class = note.PinClass,
                                    Pattern = PNStatic.CreatePinRegexPattern(note.PinText)
                                };
                                PNInterop.EnumWindowsProcDelegate enumProc = EnumWindowsProc;
                                PNInterop.EnumWindows(enumProc, 0);
                                if (!_PinClass.Hwnd.Equals(IntPtr.Zero))
                                {
                                    PNInterop.BringWindowToTop(_PinClass.Hwnd);
                                }
                            }
                        }
                        break;
                    case MarkType.Priority:
                        mnuToggleHighPriority.PerformClick();
                        break;
                    case MarkType.Protection:
                        mnuToggleProtectionMode.PerformClick();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void PFooter_FormatButtonClicked(object sender, FormatButtonClickedEventArgs e)
        {
            try
            {
                switch (e.Buttontype)
                {
                    case FormatType.Bullets:
                        if (_Edit.ReadOnly) break;
                        checkBullets();
                        ctmBullets.IsOpen = true;
                        break;
                    case FormatType.FontSize:
                        if (_Edit.ReadOnly) break;
                        checkFontSize();
                        ctmFontSize.IsOpen = true;
                        break;
                    case FormatType.FontColor:
                        if (_Edit.ReadOnly) break;
                        checkFontColor();
                        ctmFontColor.IsOpen = true;
                        break;
                    case FormatType.Highlight:
                        if (_Edit.ReadOnly) break;
                        checkHighlight();
                        ctmFontHighlight.IsOpen = true;
                        break;
                    case FormatType.FontFamily:
                        if (_Edit.ReadOnly) break;
                        showFontComboBox();
                        break;
                    case FormatType.FontBold:
                        if (_Edit.ReadOnly) break;
                        _Edit.SetFontDecoration(REFDecorationMask.CFM_BOLD, REFDecorationStyle.CFE_BOLD);
                        break;
                    case FormatType.FontItalic:
                        if (_Edit.ReadOnly) break;
                        _Edit.SetFontDecoration(REFDecorationMask.CFM_ITALIC, REFDecorationStyle.CFE_ITALIC);
                        break;
                    case FormatType.FontUnderline:
                        if (_Edit.ReadOnly) break;
                        _Edit.SetFontDecoration(REFDecorationMask.CFM_UNDERLINE, REFDecorationStyle.CFE_UNDERLINE);
                        break;
                    case FormatType.FontStrikethrough:
                        if (_Edit.ReadOnly) break;
                        _Edit.SetFontDecoration(REFDecorationMask.CFM_STRIKEOUT, REFDecorationStyle.CFE_STRIKEOUT);
                        break;
                    case FormatType.Left:
                        if (_Edit.ReadOnly) break;
                        _Edit.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
                        break;
                    case FormatType.Right:
                        if (_Edit.ReadOnly) break;
                        _Edit.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Right;
                        break;
                    case FormatType.Center:
                        if (_Edit.ReadOnly) break;
                        _Edit.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void showFontComboBox()
        {
            try
            {
                var fontString = _Edit.GetFontName();
                PToolbar.ShowFontsComboBox(fontString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboFonts_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                var cbo = sender as ComboBox;
                if (cbo == null) return;
                if (!(cbo.SelectedItem is LOGFONT)) return;
                var lf = (LOGFONT)cbo.SelectedItem;
                if (_Edit.GetFontName() == lf.lfFaceName) return;
                _Edit.SetFontByName(lf);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Edit events
        void edit_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_Loaded && _Mode != NewNoteMode.Clipboard && _Mode != NewNoteMode.Duplication) return;
                var note = PNStatic.Notes.Note(Handle);
                if (note != null && !note.Changed)
                {
                    setChangedSign(note);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        void edit_PNREActivatedByMouse(object sender, PNREActivatedByMouseEventArgs e)
        {
            try
            {
                if (_InAlarm)
                {
                    InAlarm = false;
                }
                if (!_Active)
                {
                    int index = _Edit.GetCharIndexFromPosition(e.Position);
                    string text = _Edit.Text;
                    if (index == text.Length - 1)
                    {
                        int width = _Edit.GetTextWidth(index, index + 1);
                        if (width > 0)
                        {
                            System.Drawing.Point point = _Edit.GetPositionFromCharIndex(text.Length - 1);
                            _Edit.Select(e.Position.X > point.X + width / 4 ? text.Length : index, 0);
                        }
                        else
                        {
                            _Edit.Select(index, 0);
                        }
                    }
                    else
                    {
                        _Edit.Select(index, 0);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                //do nothing - this may occur when user clicks on rich edit while note becomes hidden
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        void edit_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
        {
            try
            {
                var link = e.LinkText;
                if (e.LinkText.StartsWith("file:"))
                {
                    link = link.Remove(link.IndexOf("file:", StringComparison.Ordinal), "file:".Length);
                    try
                    {
                        var path = Path.GetFullPath(link);
                        if (!File.Exists(path) && !Directory.Exists(path))
                        {
                            PNMessageBox.Show(
                                PNLang.Instance.GetMessageText("file_not_exist",
                                    "The link is pointing to not existing file (directory)"),
                                PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    catch (ArgumentException aex)
                    {
                        PNStatic.LogException(aex, false);
                        PNMessageBox.Show(
                            PNLang.Instance.GetMessageText("inv_file_link", "Invalid link to file system object"),
                            PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    catch (SecurityException sex)
                    {
                        PNStatic.LogException(sex, false);
                        PNMessageBox.Show(
                            PNLang.Instance.GetMessageText("sec_ex_link", "You have no permissions to work with file"),
                            PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    catch (NotSupportedException nsex)
                    {
                        PNStatic.LogException(nsex, false);
                        PNMessageBox.Show(
                            PNLang.Instance.GetMessageText("inv_file_link", "Invalid link to file system object"),
                            PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    catch (PathTooLongException ptlex)
                    {
                        PNStatic.LogException(ptlex, false);
                        PNMessageBox.Show(
                            PNLang.Instance.GetMessageText("inv_file_link", "Invalid link to file system object"),
                            PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else if (e.LinkText.StartsWith("mailto:"))
                {
                    var recipients = link.Remove(link.IndexOf("mailto:", StringComparison.Ordinal), "mailto:".Length);
                    var note = PNStatic.Notes.Note(Handle);
                    if (note == null) return;
                    PNNotesOperations.SendNoteAsText(note, recipients, link);
                    return;
                }
                PNStatic.LoadPage(link);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        void edit_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            try
            {

                //prevent RichEdit built-in Ctrl+I shortcut from being applied (it inserts TAB instead of applying italic style)
                var mod = HotkeysStatic.GetModifiers();
                var key = HotkeysStatic.GetKey();
                if (mod == HotkeyModifiers.MOD_CONTROL && key == HotkeysStatic.VK_I)
                {
                    e.Handled = true;
                    return;
                }
                if (!PNStatic.Settings.GeneralSettings.AutomaticSmilies || _Edit.SelectionStart <= 0) return;
                BitmapImage image = null;
                switch (e.KeyChar)
                {
                    case ')':
                        {
                            var c =
                                _Edit.Text[_Edit.SelectionStart - 1];
                            if (c == ':')
                            {
                                image = TryFindResource("happy") as BitmapImage;//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "happy.png"));
                            }
                        }
                        break;
                    case '(':
                        {
                            var c =
                                _Edit.Text[_Edit.SelectionStart - 1];
                            if (c == ':')
                            {
                                image = TryFindResource("sad") as BitmapImage;//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "sad.png"));
                            }
                        }
                        break;
                }
                if (image != null)
                {
                    _Edit.SelectionStart--;
                    _Edit.SelectionLength = 1;
                    insertImage(PNStatic.ImageToDrawingImage(image));
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        void edit_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            try
            {
                closeContextMenus();
                var mod = HotkeysStatic.GetModifiers();
                var key = HotkeysStatic.GetKey();
                if ((mod == HotkeyModifiers.MOD_NONE && (key >= HotkeysStatic.VK_F1 && key <= HotkeysStatic.VK_F24)) ||
                    (mod != HotkeyModifiers.MOD_NONE && key != 0))
                {
                    if (key == HotkeysStatic.VK_F3)
                    {
                        if (mnuFindNext.IsEnabled)
                        {
                            mnuFindNext.PerformClick();
                            e.Handled = true;
                            return;
                        }
                    }
                    //PNHotKey hk = PNStatic.HotKeysNote.FirstOrDefault(h => h.Modifiers == mod && h.VK == key);
                    //if (hk != null &&
                    //    !PNStatic.HiddenMenus.Any(hm => hm.Type == MenuType.Note && hm.Name == hk.MenuName))
                    //{
                    //    e.Handled = true;
                    //    HotKeyClick(hk);
                    //    return;
                    //}
                    //hk = PNStatic.HotKeysEdit.FirstOrDefault(h => h.Modifiers == mod && h.VK == key);
                    //if (hk != null &&
                    //    !PNStatic.HiddenMenus.Any(hm => hm.Type == MenuType.Edit && hm.Name == hk.MenuName))
                    //{
                    //    e.Handled = true;
                    //    HotKeyClick(hk);
                    //    return;
                    //}
                }
                if (mod == HotkeyModifiers.MOD_CONTROL)
                {
                    switch (key)
                    {
                        case HotkeysStatic.VK_C:
                            var note = PNStatic.Notes.Note(Handle);
                            if (PNStatic.Settings.Protection.PromptForPassword)
                            {
                                if (note == null) return;
                                if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                                {
                                    e.Handled = true;
                                    return;
                                }
                                _Edit.Copy();
                            }
                            return;
                        case HotkeysStatic.VK_S:
                            if (mnuSave.IsEnabled)
                            {
                                mnuSave.PerformClick();
                            }
                            e.Handled = true;
                            return;
                        case HotkeysStatic.VK_P:
                            if (mnuPrint.IsEnabled)
                            {
                                mnuPrint.PerformClick();
                            }
                            e.Handled = true;
                            return;
                        case HotkeysStatic.VK_F:
                            if (mnuFind.IsEnabled)
                            {
                                mnuFind.PerformClick();
                            }
                            e.Handled = true;
                            return;
                        case HotkeysStatic.VK_H:
                            if (mnuReplace.IsEnabled)
                            {
                                mnuReplace.PerformClick();
                            }
                            e.Handled = true;
                            return;
                        case HotkeysStatic.VK_B:
                            mnuBold.PerformClick();
                            e.Handled = true;
                            return;
                        case HotkeysStatic.VK_I:
                            mnuItalic.PerformClick();
                            e.Handled = true;
                            return;
                        case HotkeysStatic.VK_U:
                            mnuUnderline.PerformClick();
                            e.Handled = true;
                            return;
                        case HotkeysStatic.VK_K:
                            mnuStrikethrough.PerformClick();
                            e.Handled = true;
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        void edit_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            try
            {
                if (!e.Data.GetDataPresent("FileDrop")) return;
                var files = (string[])e.Data.GetData("FileDrop");
                if (files.Length <= 0) return;
                if (files.Length > 1)
                {
                    var message = PNLang.Instance.GetMessageText("many_files_dropped",
                        "Only one file may be dropped onto note");
                    PNMessageBox.Show(this, message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Effect = System.Windows.Forms.DragDropEffects.None;
                    return;
                }
                _InDrop = true;
                _DropCase = DropCase.None;
                ctmDrop.IsOpen = true;
                while (_InDrop)
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                switch (_DropCase)
                {
                    case DropCase.Content:
                        if (Path.GetExtension(files[0])
                            .In(".bmp", ".png", ".gif", ".jpg", ".jpeg", ".ico", ".emf", ".wmf"))
                        {
                            using (var image = System.Drawing.Image.FromFile(files[0]))
                            {
                                insertImage(image);
                            }
                        }
                        else
                        {
                            using (var sr = new StreamReader(files[0]))
                            {
                                _Edit.SelectedText = sr.ReadToEnd();
                            }
                        }
                        e.Effect = System.Windows.Forms.DragDropEffects.None;
                        break;
                    case DropCase.Object:
                        //just insert object
                        break;
                    case DropCase.Link:
                        var link = files[0];
                        if (link.Contains(' '))
                        {
                            link = " <file:" + link + "> ";
                        }
                        else
                        {
                            link = " file:" + link + " ";
                        }
                        _Edit.SelectedText = link;
                        e.Effect = System.Windows.Forms.DragDropEffects.None;
                        break;
                    default:
                        e.Effect = System.Windows.Forms.DragDropEffects.None;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                e.Effect = System.Windows.Forms.DragDropEffects.None;
            }
        }

        void edit_ContentsResized(object sender, System.Windows.Forms.ContentsResizedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle) ?? _Note;
                if (_Edit.Height == e.NewRectangle.Height)
                {
                    if (note != null)
                    {
                        note.AutoHeight = Height;
                    }
                    return;
                }

                if (!_SizeChangedFirstTime) return;

                if (PNStatic.Settings.GeneralSettings.UseSkins || !PNStatic.Settings.GeneralSettings.AutoHeight ||
                    _InResize || _InRoll || _InDock || note.Rolled) return;

                var offset = e.NewRectangle.Height - _Edit.Height;
                var tempHeight = Height;
                if (tempHeight + offset >= MIN_HEIGHT)
                {
                    tempHeight += offset;
                }
                else
                {
                    tempHeight = MIN_HEIGHT;
                }
                if (note.DockStatus == DockStatus.None)
                    Height = tempHeight;
                note.AutoHeight = tempHeight;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        void edit_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right) return;
            ctmEdit.IsOpen = true;
        }

        void edit_GotFocus(object sender, EventArgs e)
        {
            try
            {
                activateWindow();
                PNInterop.BringWindowToTop(_EditControl.WinForm.Handle);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void _EditControl_EditControlSizeChanged(object sender, EditControlSizeChangedEventArgs e)
        {
            try
            {
                if (_SizeChangedFirstTime) return;
                _SizeChangedFirstTime = true;
                var note = PNStatic.Notes.Note(Handle) ?? _Note;
                if (!PNStatic.Settings.GeneralSettings.AutoHeight || PNStatic.Settings.GeneralSettings.UseSkins) return;
                //var offset = e.NewRectangle.Height - edit.Height;
                //if (tempHeight + offset >= MIN_HEIGHT)
                //{
                //    tempHeight += offset;
                //}
                //else
                //{
                //    tempHeight = MIN_HEIGHT;
                //}
                double tempHeight = getEditContentHeight() + (int)PFooter.ActualHeight + (int)PHeader.ActualHeight;
                if (note.DockStatus == DockStatus.None && !note.Rolled)
                    Height = tempHeight;
                note.AutoHeight = tempHeight;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Note menu clicks

        private void ctmEdit_Opened(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                checkFontSize();
                checkFontColor();
                checkBullets();
                checkHighlight();
                if ((_Edit.CanPaste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.Bitmap)) ||
                     _Edit.CanPaste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.EnhancedMetafile))
                     || _Edit.CanPaste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.Html)) ||
                     _Edit.CanPaste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.MetafilePict))
                     || _Edit.CanPaste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.OemText)) ||
                     _Edit.CanPaste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.Rtf))
                     || _Edit.CanPaste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.StringFormat)) ||
                     _Edit.CanPaste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.Text))
                     || _Edit.CanPaste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.UnicodeText))) && !_Edit.ReadOnly)
                {
                    mnuPaste.IsEnabled = true;
                }
                else
                {
                    mnuPaste.IsEnabled = false;
                }
                if ((_Edit.CanPaste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.Html)) ||
                     _Edit.CanPaste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.OemText))
                     || _Edit.CanPaste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.Rtf)) ||
                     _Edit.CanPaste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.StringFormat))
                     || _Edit.CanPaste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.Text)) ||
                     _Edit.CanPaste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.UnicodeText)))
                    && !_Edit.ReadOnly)
                {
                    mnuPastePlain.IsEnabled = true;
                }
                else
                {
                    mnuPastePlain.IsEnabled = false;
                }
                mnuUndo.IsEnabled = _Edit.CanUndo && !_Edit.ReadOnly;
                mnuRedo.IsEnabled = _Edit.CanRedo && !_Edit.ReadOnly;
                mnuCopy.IsEnabled = mnuCopyPlain.IsEnabled = _Edit.SelectedText.Length > 0;
                mnuCut.IsEnabled = _Edit.SelectedText.Length > 0 && !_Edit.ReadOnly;
                mnuSelectAll.IsEnabled = _Edit.TextLength > 0;
                mnuInsert.IsEnabled =
                    mnuFormat.IsEnabled =
                        mnuReplace.IsEnabled = !_Edit.ReadOnly;
                mnuToUpper.IsEnabled =
                    mnuToLower.IsEnabled = mnuCapSent.IsEnabled = mnuCapWord.IsEnabled = mnuToggleCase.IsEnabled = _Edit.SelectedText.Length > 0 && !_Edit.ReadOnly;
                if (Directory.Exists(PNPaths.Instance.DictDir))
                {
                    createDictMenu();
                }
                else
                {
                    mnuSpell.IsEnabled = false;
                }
                _PostPluginsMenus.Clear();
                if (PNStatic.PostPlugins.Count > 0)
                {
                    createPostPluginsMenu(mnuInsertPost);
                    mnuInsertPost.IsEnabled = !_Edit.ReadOnly;
                }
                else
                {
                    mnuInsertPost.IsEnabled = false;
                }
                if (_Edit.SelectionLength > 0)
                {
                    createSearchProvidersMenu();
                    mnuSearchWeb.IsEnabled = true;
                    if (PNStatic.PostPlugins.Count > 0)
                    {
                        createPostPluginsMenu(mnuPostOn);
                        mnuPostOn.IsEnabled = true;
                    }
                    else
                    {
                        mnuPostOn.IsEnabled = false;
                    }
                }
                else
                {
                    mnuSearchWeb.IsEnabled = false;
                    mnuPostOn.IsEnabled = false;
                }
                var spaces = _Edit.GetParagraphSpacing();
                if (spaces[REParaSpace.Before])
                {
                    mnuAddSpaceBefore.Visibility = Visibility.Collapsed;
                    mnuRemoveSpaceBefore.Visibility = Visibility.Visible;
                }
                else
                {
                    mnuAddSpaceBefore.Visibility = Visibility.Visible;
                    mnuRemoveSpaceBefore.Visibility = Visibility.Collapsed;
                }
                if (spaces[REParaSpace.After])
                {
                    mnuRemoveSpaceAfter.Visibility = Visibility.Visible;
                    mnuAddSpaceAfter.Visibility = Visibility.Collapsed;
                }
                else
                {
                    mnuRemoveSpaceAfter.Visibility = Visibility.Collapsed;
                    mnuAddSpaceAfter.Visibility = Visibility.Visible;
                }
                mnuSpace10.IsChecked = mnuSpace15.IsChecked = mnuSpace20.IsChecked = mnuSpace30.IsChecked = false;
                var lineSpacing = _Edit.GetLineSpacing();
                switch (lineSpacing)
                {
                    case RELineSpacing.Single:
                        mnuSpace10.IsChecked = true;
                        break;
                    case RELineSpacing.OneAndHalf:
                        mnuSpace15.IsChecked = true;
                        break;
                    case RELineSpacing.Double:
                        mnuSpace20.IsChecked = true;
                        break;
                    case RELineSpacing.Triple:
                        mnuSpace30.IsChecked = true;
                        break;
                }
                var dec = _Edit.GetFontDecoration();
                mnuBold.IsChecked = (dec & REFDecorationStyle.CFE_BOLD) == REFDecorationStyle.CFE_BOLD;
                mnuItalic.IsChecked = (dec & REFDecorationStyle.CFE_ITALIC) == REFDecorationStyle.CFE_ITALIC;
                mnuUnderline.IsChecked = (dec & REFDecorationStyle.CFE_UNDERLINE) == REFDecorationStyle.CFE_UNDERLINE;
                mnuStrikethrough.IsChecked = (dec & REFDecorationStyle.CFE_STRIKEOUT) == REFDecorationStyle.CFE_STRIKEOUT;
                mnuSubscript.IsChecked = (dec & REFDecorationStyle.CFE_SUBSCRIPT) == REFDecorationStyle.CFE_SUBSCRIPT;
                mnuSuperscript.IsChecked = (dec & REFDecorationStyle.CFE_SUPERSCRIPT) ==
                                         REFDecorationStyle.CFE_SUPERSCRIPT;
                mnuAlignLeft.IsChecked = _Edit.SelectionAlignment == System.Windows.Forms.HorizontalAlignment.Left;
                mnuAlignCenter.IsChecked = _Edit.SelectionAlignment == System.Windows.Forms.HorizontalAlignment.Center;
                mnuAlignRight.IsChecked = _Edit.SelectionAlignment == System.Windows.Forms.HorizontalAlignment.Right;

                mnuSortAscending.IsEnabled = mnuSortDescending.IsEnabled = _Edit.Lines.Length > 1 && !_Edit.ReadOnly;

                mnuClearFormat.IsEnabled = _Edit.SelectionLength > 0 && !_Edit.ReadOnly;

                mnuAlignLeft.IsEnabled = mnuAlignCenter.IsEnabled = mnuAlignRight.IsEnabled =
                    mnuSuperscript.IsEnabled =
                        mnuSubscript.IsEnabled =
                                mnuSpace10.IsEnabled =
                                    mnuSpace15.IsEnabled =
                                        mnuSpace20.IsEnabled =
                                            mnuSpace30.IsEnabled =
                                                mnuBold.IsEnabled =
                                                    mnuItalic.IsEnabled =
                                                        mnuUnderline.IsEnabled =
                                                            mnuStrikethrough.IsEnabled =
                                                                mnuIncreaseIndent.IsEnabled =
                                                                    mnuDecreaseIndent.IsEnabled = !_Edit.ReadOnly;
                if (mnuCheckSpellNow.IsEnabled)
                {
                    mnuCheckSpellNow.IsEnabled = !_Edit.ReadOnly;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createSearchProvidersMenu()
        {
            try
            {
                var count = mnuSearchWeb.Items.Count;
                if (count > 0)
                {
                    for (var i = count - 1; i >= 0; i--)
                    {
                        var mi = mnuSearchWeb.Items[i] as MenuItem;
                        if (mi != null)
                        {
                            mi.Click -= menu_Click;
                        }
                        mnuSearchWeb.Items.RemoveAt(i);
                    }
                }
                foreach (
                    var ti in
                        PNStatic.SearchProviders.Select(psp => new MenuItem { Header = psp.Name, Tag = psp.QueryString })
                    )
                {
                    ti.Click += menu_Click;
                    mnuSearchWeb.Items.Add(ti);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            showSaveAsDialog(false);
        }

        private void mnuRename_Click(object sender, RoutedEventArgs e)
        {
            showSaveAsDialog(true);
        }

        private void mnuSaveAsText_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note != null)
                {
                    PNNotesOperations.SaveNoteAsTextFile(note, this);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuRestoreFromBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note != null)
                {
                    PNNotesOperations.LoadBackupCopy(note, this);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuDuplicateNote_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note != null)
                {
                    PNStatic.FormMain.DuplicateNote(note);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSaveAsShortcut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                PNNotesOperations.SaveAsShortcut(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                if (PNStatic.Settings.Protection.PromptForPassword)
                    if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                        return;
                _Edit.Print(note.Name);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuAdjustAppearance_Click(object sender, EventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                var dlgAdjustAppearance = new WndAdjustAppearance(note) { Owner = this };
                dlgAdjustAppearance.NoteAppearanceAdjusted += dlgAdjustAppearance_NoteAppearanceAdjusted;
                var result = dlgAdjustAppearance.ShowDialog();
                if (!result.HasValue || !result.Value)
                {
                    dlgAdjustAppearance.NoteAppearanceAdjusted -= dlgAdjustAppearance_NoteAppearanceAdjusted;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgAdjustAppearance_NoteAppearanceAdjusted(object sender, NoteAppearanceAdjustedEventArgs e)
        {
            try
            {
                var dlgAdjustAppearance = sender as WndAdjustAppearance;
                if (dlgAdjustAppearance != null)
                    dlgAdjustAppearance.NoteAppearanceAdjusted -= dlgAdjustAppearance_NoteAppearanceAdjusted;
                ApplyAppearanceAdjustment(e);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuAdjustSchedule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle) ?? _Note;
                if (note == null) return;
                PNNotesOperations.AdjustNoteSchedule(note, this);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuHideNote_Click(object sender, RoutedEventArgs e)
        {
            PHeader_HideDeleteButtonClicked(PHeader, new HeaderButtonClickedEventArgs(HeaderButtonType.Hide));
        }

        private void mnuDeleteNote_Click(object sender, RoutedEventArgs e)
        {
            PHeader_HideDeleteButtonClicked(PHeader, new HeaderButtonClickedEventArgs(HeaderButtonType.Delete));
        }

        private void mnuDockNone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note != null)
                {
                    if (note.DockStatus != DockStatus.None)
                    {
                        UndockNote(note);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuDockLeft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note != null)
                {
                    if (note.DockStatus != DockStatus.Left)
                    {
                        DockNote(note, DockStatus.Left, false);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuDockTop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note != null)
                {
                    if (note.DockStatus != DockStatus.Top)
                    {
                        DockNote(note, DockStatus.Top, false);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuDockRight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note != null)
                {
                    if (note.DockStatus != DockStatus.Right)
                    {
                        DockNote(note, DockStatus.Right, false);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuDockBottom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note != null)
                {
                    if (note.DockStatus != DockStatus.Bottom)
                    {
                        DockNote(note, DockStatus.Bottom, false);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSendAsText_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                if (PNStatic.Settings.Protection.PromptForPassword)
                    if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                        return;
                PNNotesOperations.SendNoteAsText(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSendAsAttachment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                if (PNStatic.Settings.Protection.PromptForPassword)
                    if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                        return;
                var path = Path.Combine(PNPaths.Instance.DataDir, note.ID + PNStrings.NOTE_EXTENSION);
                if (!File.Exists(path)) return;
                var files = new List<string> { path };
                PNNotesOperations.SendNotesAsAttachments(files);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSendZip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                if (PNStatic.Settings.Protection.PromptForPassword)
                    if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                        return;
                var path = Path.Combine(PNPaths.Instance.DataDir, note.ID + PNStrings.NOTE_EXTENSION);
                if (!File.Exists(path)) return;
                var files = new List<string> { path };
                var dzip = new WndArchName(files) { Owner = this };
                dzip.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuPanel_Click(object sender, RoutedEventArgs e)
        {
            SetThumbnail();
        }

        private void mnuAddContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newID = 0;
                if (PNStatic.Contacts.Count > 0)
                {
                    newID = PNStatic.Contacts.Max(c => c.ID) + 1;
                }
                var dlgContact = new WndContacts(newID, PNStatic.ContactGroups) { Owner = this };
                dlgContact.ContactChanged += dlgContact_ContactChanged;
                var showDialog = dlgContact.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                {
                    dlgContact.ContactChanged -= dlgContact_ContactChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgContact_ContactChanged(object sender, ContactChangedEventArgs e)
        {
            try
            {
                var dc = sender as WndContacts;
                if (dc != null)
                {
                    dc.ContactChanged -= dlgContact_ContactChanged;
                }
                if (e.Mode == AddEditMode.Add)
                {
                    if (PNStatic.Contacts.Any(c => c.Name == e.Contact.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("contact_exists",
                            "Contact with this name already exists");
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                        e.Accepted = false;
                        return;
                    }
                    PNStatic.Contacts.Add(e.Contact);
                }
                else
                {
                    var c = PNStatic.Contacts.FirstOrDefault(con => con.ID == e.Contact.ID);
                    if (c != null)
                    {
                        c.Name = e.Contact.Name;
                        c.ComputerName = e.Contact.ComputerName;
                        c.IpAddress = e.Contact.IpAddress;
                        c.UseComputerName = e.Contact.UseComputerName;
                        c.GroupID = e.Contact.GroupID;
                    }
                }
                if (PNStatic.FormSettings != null)
                {
                    PNStatic.FormSettings.ContactAction(e.Contact, e.Mode);
                }
                PNData.SaveContacts();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuAddGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newID = 0;
                if (PNStatic.ContactGroups.Count > 0)
                {
                    newID = PNStatic.ContactGroups.Max(g => g.ID) + 1;
                }
                var dlgContactGroup = new WndGroups(newID) { Owner = this };
                dlgContactGroup.ContactGroupChanged += dlgContactGroup_ContactGroupChanged;
                var showDialog = dlgContactGroup.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                {
                    dlgContactGroup.ContactGroupChanged -= dlgContactGroup_ContactGroupChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgContactGroup_ContactGroupChanged(object sender, ContactGroupChangedEventArgs e)
        {
            try
            {
                var dg = sender as WndGroups;
                if (dg != null)
                {
                    dg.ContactGroupChanged -= dlgContactGroup_ContactGroupChanged;
                }
                if (e.Mode == AddEditMode.Add)
                {
                    if (PNStatic.ContactGroups.Any(g => g.Name == e.Group.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("group_exists",
                            "Contacts group with this name already exists");
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                        e.Accepted = false;
                        return;
                    }
                    PNStatic.ContactGroups.Add(e.Group);
                }
                else
                {
                    var g = PNStatic.ContactGroups.FirstOrDefault(gr => gr.ID == e.Group.ID);
                    if (g != null)
                    {
                        g.Name = e.Group.Name;
                    }
                }
                if (PNStatic.FormSettings != null)
                {
                    PNStatic.FormSettings.ContactGroupAction(e.Group, e.Mode);
                }
                PNData.SaveContactGroups();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSelectContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlgSelectC = new WndSelectContacts { Owner = this };
                dlgSelectC.ContactsSelected += dlgSelectCOrG_ContactsSelected;
                var showDialog = dlgSelectC.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                {
                    dlgSelectC.ContactsSelected -= dlgSelectCOrG_ContactsSelected;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgSelectCOrG_ContactsSelected(object sender, ContactsSelectedEventArgs e)
        {
            try
            {
                var d = sender as WndSelectContacts;
                if (d != null)
                {
                    d.ContactsSelected -= dlgSelectCOrG_ContactsSelected;
                }
                else
                {
                    var gd = sender as WndSelectGroups;
                    if (gd != null)
                    {
                        gd.ContactsSelected -= dlgSelectCOrG_ContactsSelected;
                    }
                }
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                var notes = new List<string>();
                var recipients = new List<string>();
                foreach (var cn in e.Contacts)
                {
                    if (cn == null) continue;
                    if (!PNStatic.FormMain.SendNotesViaNetwork(new List<PNote> { note }, cn)) continue;
                    if (PNStatic.Settings.Network.NoNotificationOnSend) continue;
                    notes.Add(note.Name);
                    recipients.Add(note.SentTo);
                }

                if (!PNStatic.Settings.Network.NoNotificationOnSend)
                {
                    PNStatic.FormMain.ShowSentNotification(notes, recipients);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSelectGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlgSelectG = new WndSelectGroups { Owner = this };
                dlgSelectG.ContactsSelected += dlgSelectCOrG_ContactsSelected;
                var showDialog = dlgSelectG.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                {
                    dlgSelectG.ContactsSelected -= dlgSelectCOrG_ContactsSelected;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuReply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                var cn = (PNStatic.Contacts.FirstOrDefault(c => c.Name == note.ReceivedFrom) ??
                          PNStatic.Contacts.FirstOrDefault(c => c.ComputerName == note.ReceivedFrom)) ??
                         PNStatic.Contacts.FirstOrDefault(c => c.IpAddress == note.ReceivedIp);
                if (cn == null) return;
                if (!PNStatic.FormMain.SendNotesViaNetwork(new List<PNote> { note }, cn)) return;
                if (PNStatic.Settings.Network.NoNotificationOnSend) return;
                var notes = new List<string>();
                var recipients = new List<string>();
                notes.Add(note.Name);
                recipients.Add(note.SentTo);
                PNStatic.FormMain.ShowSentNotification(notes, recipients);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuExportOutlookNote_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PNOffice.ExportToOutlookNote(_Edit.Text);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuTags_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                var d = new WndExchangeLists(note.ID, ExchangeLists.Tags) { Owner = this };
                d.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuManageLinks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                var d = new WndExchangeLists(note.ID, ExchangeLists.LinkedNotes) { Owner = this };
                d.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuAddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note != null && !note.Favorite)
                {
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Favorite, true, null);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuRemoveFromFavorites_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note != null && note.Favorite)
                {
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Favorite, false, null);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuOnTop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                var value = !note.Topmost;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Topmost, value, null);
                Topmost = value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuToggleHighPriority_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                var value = !note.Priority;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Priority, value, null);
                PFooter.SetMarkButtonVisibility(MarkType.Priority, value);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuToggleProtectionMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                var value = !note.Protected;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Protection, value, null);
                PFooter.SetMarkButtonVisibility(MarkType.Protection, value);

                _Edit.ReadOnly = value || (note.Scrambled);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSetPassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note != null)
                {
                    string text = " [" + PNLang.Instance.GetCaptionText("note", "Note") + " \"" + note.Name + "\"]";
                    var pwrdCrweate = new WndPasswordCreate(text)
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    pwrdCrweate.PasswordChanged += pwrdCrweate_PasswordChanged;
                    var showDialog = pwrdCrweate.ShowDialog();
                    if (showDialog != null && !showDialog.Value)
                    {
                        pwrdCrweate.PasswordChanged -= pwrdCrweate_PasswordChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void pwrdCrweate_PasswordChanged(object sender, PasswordChangedEventArgs e)
        {
            try
            {
                var pwrdCrweate = sender as WndPasswordCreate;
                if (pwrdCrweate != null) pwrdCrweate.PasswordChanged -= pwrdCrweate_PasswordChanged;
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Password, true, e.NewPassword);
                PFooter.SetMarkButtonVisibility(MarkType.Password, true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuRemovePassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                var text = " [" + PNLang.Instance.GetCaptionText("note", "Note") + " \"" + note.Name + "\"]";
                var pwrdDelete = new WndPasswordDelete(PasswordDlgMode.DeleteNote, text,
                    note.PasswordString)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                pwrdDelete.PasswordDeleted += pwrdDelete_PasswordDeleted;
                var showDialog = pwrdDelete.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                {
                    pwrdDelete.PasswordDeleted -= pwrdDelete_PasswordDeleted;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void pwrdDelete_PasswordDeleted(object sender, EventArgs e)
        {
            try
            {
                var pwrdDelete = sender as WndPasswordDelete;
                if (pwrdDelete != null) pwrdDelete.PasswordDeleted -= pwrdDelete_PasswordDeleted;
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Password, false, null);
                PFooter.SetMarkButtonVisibility(MarkType.Password, false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuMarkAsComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                var value = !note.Completed;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Complete, value, null);
                PFooter.SetMarkButtonVisibility(MarkType.Complete, value);
                if (PNStatic.Settings.Behavior.HideCompleted)
                {
                    ApplyHideNote(note);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuRollUnroll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null || note.DockStatus != DockStatus.None) return;
                if (!note.Rolled)
                {
                    rollUnrollNote(note, true);
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Roll, true, null);
                }
                else
                {
                    rollUnrollNote(note, false);
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Roll, false, null);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuPin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                var dlgPin = new WndPin(note.Name) { Owner = this };
                dlgPin.PinnedWindowChanged += dlgPin_PinnedWindowChanged;
                var showDialog = dlgPin.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                {
                    dlgPin.PinnedWindowChanged -= dlgPin_PinnedWindowChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgPin_PinnedWindowChanged(object sender, PinnedWindowChangedEventArgs e)
        {
            try
            {
                var d = sender as WndPin;
                if (d != null)
                {
                    d.PinnedWindowChanged -= dlgPin_PinnedWindowChanged;
                }
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                note.PinText = e.PinText;
                note.PinClass = e.PinClass;
                note.Pinned = true;
                PNNotesOperations.SaveNotePin(note);
                PFooter.SetMarkButtonVisibility(MarkType.Pin, true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuUnpin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                note.PinText = "";
                note.PinClass = "";
                note.Pinned = false;
                PNNotesOperations.SaveNotePin(note);
                PFooter.SetMarkButtonVisibility(MarkType.Pin, false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuScramble_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                var text = "";
                if (!note.FromDB)
                    text = _Edit.Rtf;
                // prevent "change" mark from being showed/hidden
                //PNInterop.LockWindowUpdate(footer.Handle);
                var dlg = new WndScramble(note.Scrambled ? ScrambleMode.Unscramble : ScrambleMode.Scramble, note)
                {
                    Owner = this
                };
                var showDialog = dlg.ShowDialog();
                if (showDialog == null || !showDialog.Value) return;

                if (note.FromDB)
                    mnuSave.PerformClick();
                else
                {
                    if (!showSaveAsDialog(false))
                    {
                        _Edit.Rtf = text;
                        return;
                    }
                }
                var value = !note.Scrambled;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Scrambled, value, null);
                _Edit.ReadOnly = value || (note.Protected);
                PFooter.SetMarkButtonVisibility(MarkType.Encrypted, note.Scrambled);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            //finally
            //{
            //    PNInterop.LockWindowUpdate(IntPtr.Zero);
            //}
        }

        private void ctmDrop_Closed(object sender, RoutedEventArgs e)
        {
            _InDrop = false;
        }

        private void ctmNote_Opened(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note == null) return;
                createLinksMenu(note);
                clearContactsMenu();
                if (PNStatic.Settings.Network.EnableExchange && !PNStatic.Settings.Network.NoContactsInContextMenu)
                {
                    createContactsMenu();
                }
                if (PNStatic.Settings.Network.EnableExchange)
                {
                    mnuSendNetwork.IsEnabled = true;
                    //var gesture = mnuReply.InputGestureText;
                    if ((note.SentReceived & SendReceiveStatus.Received) ==
                        SendReceiveStatus.Received)
                    {
                        mnuReply.IsEnabled = true;
                        mnuReply.Header = PNLang.Instance.GetMenuText("note_menu", "mnuReply", "Reply");
                        mnuReply.Header += " (" + note.ReceivedFrom + ")";
                        //if (!string.IsNullOrEmpty(gesture))
                        //    mnuReply.Header += "\t" + gesture;
                    }
                    else
                    {
                        mnuReply.IsEnabled = false;
                        mnuReply.Header = PNLang.Instance.GetMenuText("note_menu", "mnuReply", "Reply");
                        //if (!string.IsNullOrEmpty(gesture))
                        //    mnuReply.Header += "\t" + gesture;
                    }
                }
                else
                {
                    mnuSendNetwork.IsEnabled = false;
                    mnuReply.IsEnabled = false;
                }
                mnuSave.IsEnabled = note.Changed;
                mnuRename.IsEnabled = (note.FromDB && note.GroupID != (int)SpecialGroups.Diary);
                mnuDuplicateNote.IsEnabled = (note.GroupID != (int)SpecialGroups.Diary);
                mnuSaveAsShortcut.IsEnabled = note.FromDB;
                mnuAdjustAppearance.IsEnabled =
                    mnuOnTop.IsEnabled = mnuRollUnroll.IsEnabled = (note.DockStatus == DockStatus.None);
                mnuAddToFavorites.Visibility = !note.Favorite &&
                                               !PNStatic.HiddenMenus.Any(
                                                   hm => hm.Type == MenuType.Note && hm.Name == mnuAddToFavorites.Name)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                mnuAddToFavorites.IsEnabled = (note.GroupID != (int)SpecialGroups.Diary);
                mnuRemoveFromFavorites.Visibility = note.Favorite &&
                                                    !PNStatic.HiddenMenus.Any(
                                                        hm =>
                                                            hm.Type == MenuType.Note &&
                                                            hm.Name == mnuRemoveFromFavorites.Name)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                mnuRemoveFromFavorites.IsEnabled = (note.GroupID != (int)SpecialGroups.Diary);
                mnuSetPassword.Visibility = !(note.PasswordString.Length > 0) &&
                                            !PNStatic.HiddenMenus.Any(
                                                hm => hm.Type == MenuType.Note && hm.Name == mnuSetPassword.Name)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                mnuRemovePassword.Visibility = (note.PasswordString.Length > 0) &&
                                               !PNStatic.HiddenMenus.Any(
                                                   hm => hm.Type == MenuType.Note && hm.Name == mnuRemovePassword.Name)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                mnuPin.Visibility = !note.Pinned &&
                                    !PNStatic.HiddenMenus.Any(
                                        hm => hm.Type == MenuType.Note && hm.Name == mnuPin.Name)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                mnuUnpin.Visibility = note.Pinned &&
                                      !PNStatic.HiddenMenus.Any(
                                          hm => hm.Type == MenuType.Note && hm.Name == mnuUnpin.Name)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                mnuOnTop.IsChecked = note.Topmost;
                mnuToggleHighPriority.IsChecked = note.Priority;
                mnuToggleProtectionMode.IsChecked = note.Protected;
                mnuMarkAsComplete.IsChecked = note.Completed;
                mnuRollUnroll.IsEnabled = !PNStatic.Settings.GeneralSettings.UseSkins;
                //check for Office 2003 or 2007
                mnuExportOutlookNote.IsEnabled = PNOffice.GetOfficeAppVersion(OfficeApp.Outlook).In(11, 12);
                mnuExportToOffice.IsEnabled =
                    mnuExportToOffice.Items.OfType<MenuItem>().Any(ti => ti.IsEnabled);
                if (PNStatic.PostPlugins.Count > 0)
                {
                    _PostPluginsMenus.Clear();
                    if (_Edit.TextLength > 0)
                    {
                        createPostPluginsMenu(mnuPostNote);
                        mnuPostNote.IsEnabled = true;
                    }
                    else
                    {
                        mnuPostNote.IsEnabled = false;
                    }
                    createPostPluginsMenu(mnuReplacePost);
                    mnuReplacePost.IsEnabled = !_Edit.ReadOnly;
                }
                else
                {
                    mnuPostNote.IsEnabled = false;
                    mnuReplacePost.IsEnabled = false;
                }
                if (!note.Scrambled)
                {
                    mnuScramble.Header = PNLang.Instance.GetMenuText("note_menu", "mnuScramble", "Encrypt text");
                    mnuScramble.Icon = new Image
                    {
                        Source = TryFindResource("scramble") as BitmapImage//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "scramble.png"))
                    };
                }
                else
                {
                    mnuScramble.Header = PNLang.Instance.GetMenuText("note_menu", "mnuUnscramble", "Decrypt text");
                    mnuScramble.Icon = new Image
                    {
                        Source = TryFindResource("unscramble") as BitmapImage//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "unscramble.png"))
                    };
                }
                mnuRestoreFromBackup.IsEnabled = !_Edit.ReadOnly;
                mnuPanel.IsEnabled = PNStatic.Settings.Behavior.ShowNotesPanel;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createPostPluginsMenu(MenuItem menu)
        {
            try
            {
                var count = menu.Items.Count;
                if (count > 0)
                {
                    for (var i = count - 1; i >= 0; i--)
                    {
                        var mi = menu.Items[i] as MenuItem;
                        if (mi != null)
                        {
                            mi.Click -= menu_Click;
                        }
                        menu.Items.RemoveAt(i);
                    }
                }
                foreach (var p in PNPlugins.Instance.SocialPlugins)
                {
                    if (!PNStatic.PostPlugins.Contains(p.Name)) continue;
                    MenuItem mi = null;
                    switch (menu.Name)
                    {
                        case "mnuPostOn":
                            _PostPluginsMenus.Add(p.Name + menu.Name, p.MenuPostPartial);
                            mi = new MenuItem
                            {
                                Header = p.MenuPostPartial.Text,
                                Icon =
                                    new Image
                                    {
                                        Source = PNStatic.ImageFromDrawingImage(p.MenuPostPartial.Image)
                                    },
                                Tag = p.Name + menu.Name
                            };
                            break;
                        case "mnuPostNote":
                            _PostPluginsMenus.Add(p.Name + menu.Name, p.MenuPostFull);
                            mi = new MenuItem
                            {
                                Header = p.MenuPostFull.Text,
                                Icon =
                                    new Image
                                    {
                                        Source = PNStatic.ImageFromDrawingImage(p.MenuPostFull.Image)
                                    },
                                Tag = p.Name + menu.Name
                            };
                            break;
                        case "mnuReplacePost":
                            _PostPluginsMenus.Add(p.Name + menu.Name, p.MenuGetFull);
                            mi = new MenuItem
                            {
                                Header = p.MenuGetFull.Text,
                                Icon =
                                    new Image
                                    {
                                        Source = PNStatic.ImageFromDrawingImage(p.MenuGetFull.Image)
                                    },
                                Tag = p.Name + menu.Name,
                                IsEnabled = !_Edit.ReadOnly
                            };
                            break;
                        case "mnuInsertPost":
                            _PostPluginsMenus.Add(p.Name + menu.Name, p.MenuGetPartial);
                            mi = new MenuItem
                            {
                                Header = p.MenuGetPartial.Text,
                                Icon =
                                    new Image
                                    {
                                        Source = PNStatic.ImageFromDrawingImage(p.MenuGetPartial.Image)
                                    },
                                Tag = p.Name + menu.Name,
                                IsEnabled = !_Edit.ReadOnly
                            };
                            mi.IsEnabled = !_Edit.ReadOnly;
                            break;
                    }

                    if (mi == null) continue;
                    mi.Click += postPluginMenu_Click;
                    menu.Items.Add(mi);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void postPluginMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var menu = sender as MenuItem;
                if (menu == null) return;
                var tag = Convert.ToString(menu.Tag);
                if (_PostPluginsMenus.Keys.All(k => k != tag)) return;
                var pmenu = _PostPluginsMenus[tag];
                if (pmenu == null) return;
                pmenu.PerformClick();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createLinksMenu(PNote note)
        {
            try
            {
                int count = mnuLinked.Items.Count;
                if (count > 1)
                {
                    for (int i = count - 1; i > 0; i--)
                    {
                        var menu = mnuLinked.Items[i] as MenuItem;
                        if (menu != null)
                        {
                            menu.Click -= menu_Click;
                        }
                        mnuLinked.Items.RemoveAt(i);
                    }
                }
                if (note.LinkedNotes.Count > 0)
                {
                    mnuLinked.Items.Add(new Separator());
                    foreach (string l in note.LinkedNotes)
                    {
                        PNote n = PNStatic.Notes.Note(l);
                        if (n != null)
                        {
                            var ti = new MenuItem { Header = n.Name, Tag = l };
                            ti.Click += menu_Click;
                            mnuLinked.Items.Add(ti);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void clearContactsMenu()
        {
            try
            {
                int count = mnuSendNetwork.Items.Count;
                if (count > 5)
                {
                    for (int i = count - 3; i > 2; i--)
                    {
                        var menu = mnuSendNetwork.Items[i] as MenuItem;
                        if (menu != null && menu.Items.Count == 0)
                        {
                            menu.Click -= contactMenu_Click;
                        }
                        else
                        {
                            if (menu != null)
                            {
                                foreach (MenuItem mi in menu.Items.OfType<MenuItem>())
                                {
                                    mi.Click -= contactMenu_Click;
                                }
                            }
                        }
                        mnuSendNetwork.Items.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createContactsMenu()
        {
            try
            {
                var index = 3;
                // all contacts from png (None)
                var contactsGroup = PNStatic.Contacts.Where(c => c.GroupID == -1);
                foreach (var mi in contactsGroup.Select(c => new MenuItem
                {
                    Header = c.Name,
                    Tag = c.ID,
                    Icon = new Image
                    {
                        Source = TryFindResource("contact") as BitmapImage//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "contact.png"))
                    }
                }))
                {
                    mi.Click += contactMenu_Click;
                    mnuSendNetwork.Items.Insert(index++, mi);
                }
                // get all other groups
                var groups = PNStatic.ContactGroups.Where(g => g.ID != -1);
                foreach (var pgroup in groups)
                {
                    var mgi = new MenuItem
                    {
                        Header = pgroup.Name,
                        IsEnabled = false,
                        Icon = new Image
                        {
                            Source = TryFindResource("group") as BitmapImage//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "group.png"))
                        }
                    };
                    mnuSendNetwork.Items.Insert(index++, mgi);
                    var pg = pgroup;
                    contactsGroup = PNStatic.Contacts.Where(c => c.GroupID == pg.ID);
                    foreach (var mi in contactsGroup.Select(c => new MenuItem
                    {
                        Header = c.Name,
                        Tag = c.ID,
                        Icon = new Image
                        {
                            Source = TryFindResource("contact") as BitmapImage//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "contact.png"))
                        }
                    }))
                    {
                        mi.Click += contactMenu_Click;
                        mgi.Items.Add(mi);
                        mgi.IsEnabled = true;
                    }
                }
                if (index > 3)
                {
                    mnuSendNetwork.Items.Insert(index, new Separator());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void contactMenu_Click(object sender, EventArgs e)
        {
            try
            {
                var note = PNStatic.Notes.Note(Handle);
                if (note != null)
                {
                    var mi = sender as MenuItem;
                    if (mi != null)
                    {
                        PNContact cn = PNStatic.Contacts.FirstOrDefault(c => c.ID == (int)mi.Tag);
                        if (cn != null)
                        {
                            if (PNStatic.FormMain.SendNotesViaNetwork(new List<PNote> { note }, cn))
                            //if (PNNotesOperations.SendNoteViaNetwork(note, cn))
                            {
                                if (!PNStatic.Settings.Network.NoNotificationOnSend)
                                {
                                    var notes = new List<string>();
                                    var recipients = new List<string>();
                                    notes.Add(note.Name);
                                    recipients.Add(note.SentTo);
                                    PNStatic.FormMain.ShowSentNotification(notes, recipients);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void menu_Click(object sender, EventArgs e)
        {
            try
            {
                var ti = sender as MenuItem;
                if (ti == null) return;
                var parent = ti.Parent as MenuItem;
                if (parent == null) return;
                switch (parent.Name)
                {
                    case "mnuLinked":
                        var id = (string)ti.Tag;
                        var note = PNStatic.Notes.Note(id);
                        if (note != null)
                        {
                            PNNotesOperations.ShowHideSpecificNote(note, true);
                        }
                        break;
                    case "mnuSearchWeb":
                        var query = (string)ti.Tag;
                        query += _Edit.SelectedText;
                        PNStatic.LoadPage(query);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private bool EnumWindowsProc(IntPtr hwnd, int lParam)
        {
            try
            {
                var sbClass = new StringBuilder(1024);
                PNInterop.GetClassName(hwnd, sbClass, sbClass.Capacity);
                if (sbClass.ToString() != _PinClass.Class) return true;
                if (!PNInterop.IsWindowVisible(hwnd)) return true;
                var count = PNInterop.GetWindowTextLength(hwnd);
                if (count <= 0) return true;
                var sb = new StringBuilder(count + 1);
                PNInterop.GetWindowText(hwnd, sb, count + 1);
                if (!Regex.IsMatch(sb.ToString(), _PinClass.Pattern)) return true;
                _PinClass.Hwnd = hwnd;
                return false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        #endregion

        #region Edit menu clicks
        private void mnuUndo_Click(object sender, RoutedEventArgs e)
        {
            if (_Edit.CanUndo)
            {
                _Edit.Undo();
            }
        }

        private void mnuRedo_Click(object sender, RoutedEventArgs e)
        {
            if (_Edit.CanRedo)
            {
                _Edit.Redo();
            }
        }

        private void mnuCut_Click(object sender, RoutedEventArgs e)
        {
            _Edit.Cut();
        }

        private void mnuCopy_Click(object sender, RoutedEventArgs e)
        {
            _Edit.Copy();
        }

        private void mnuPaste_Click(object sender, RoutedEventArgs e)
        {
            _Edit.Paste();
        }

        private void mnuCopyPlain_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_Edit.SelectedText);
        }

        private void mnuPastePlain_Click(object sender, RoutedEventArgs e)
        {
            var text = Clipboard.GetText(TextDataFormat.UnicodeText);
            _Edit.SelectedText = text;
        }

        private void mnuToUpper_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                if (_Edit.SelectionType.HasFlag(System.Windows.Forms.RichTextBoxSelectionTypes.Object) ||
                    _Edit.SelectionType.HasFlag(System.Windows.Forms.RichTextBoxSelectionTypes.MultiObject))
                {
                    var message = PNLang.Instance.GetMessageText("selection_objects_warning",
                        "Selected text contains one or more non-text objects (pictures etc). If you continue, these objects will be deleted. Continue anyway?");
                    if (
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                        MessageBoxResult.No)
                    {
                        return;
                    }
                }
                var start = _Edit.SelectionStart;
                var length = _Edit.SelectionLength;
                _Edit.SelectedText = _Edit.SelectedText.ToUpper();
                _Edit.Select(start, length);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuToLower_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                if (_Edit.SelectionType.HasFlag(System.Windows.Forms.RichTextBoxSelectionTypes.Object) ||
                    _Edit.SelectionType.HasFlag(System.Windows.Forms.RichTextBoxSelectionTypes.MultiObject))
                {
                    var message = PNLang.Instance.GetMessageText("selection_objects_warning",
                        "Selected text contains one or more non-text objects (pictures etc). If you continue, these objects will be deleted. Continue anyway?");
                    if (
                        MessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                        MessageBoxResult.No)
                    {
                        return;
                    }
                }
                var start = _Edit.SelectionStart;
                var length = _Edit.SelectionLength;
                _Edit.SelectedText = _Edit.SelectedText.ToLower();
                _Edit.Select(start, length);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuCapSent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                var index = 0;
                char[] delimiters = { '.' };
                var start = _Edit.SelectionStart;
                var length = _Edit.SelectionLength;
                var position = start;
                var selectedText = _Edit.SelectedText;
                var tokens = selectedText.Split(delimiters);
                if (start > 0 && !delimiters.Contains(_Edit.Text[start - 1]))
                {
                    index = 1;
                    position += tokens[0].Length;
                }
                for (; index < tokens.Length; index++)
                {
                    position = _Edit.Text.IndexOf(tokens[index], position, StringComparison.Ordinal);
                    if (string.IsNullOrWhiteSpace(tokens[index]))
                    {
                        position += tokens[index].Length;
                        continue;
                    }
                    if (tokens[index].Length > 1)
                    {
                        var firstIndex = 0;
                        for (var i = 0; i < tokens[index].Length; i++)
                        {
                            if (!char.IsWhiteSpace(tokens[index][i]))
                                break;
                            firstIndex++;
                        }
                        tokens[index] = tokens[index].Substring(0, firstIndex) +
                                        tokens[index].Substring(firstIndex, 1).ToUpper() +
                                        tokens[index].Substring(firstIndex + 1);
                    }
                    else
                    {
                        tokens[index] = tokens[index].ToUpper();
                    }
                    _Edit.Select(position, tokens[index].Length);
                    _Edit.SelectedText = tokens[index];
                    position += tokens[index].Length;
                }
                _Edit.Select(start, length);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuCapWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                var index = 0;
                var delimiters = Spellchecking.PNRE_DELIMITERS.ToCharArray();
                var start = _Edit.SelectionStart;
                var length = _Edit.SelectionLength;
                var position = start;
                var selectedText = _Edit.SelectedText;
                var tokens = selectedText.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                if (start > 0 && !delimiters.Contains(_Edit.Text[start - 1]))
                {
                    index = 1;
                    position += tokens[0].Length;
                }
                for (; index < tokens.Length; index++)
                {
                    position = _Edit.Text.IndexOf(tokens[index], position, StringComparison.Ordinal);
                    if (tokens[index].Length > 1)
                    {
                        tokens[index] = tokens[index].Substring(0, 1).ToUpper() + tokens[index].Substring(1);
                    }
                    else
                    {
                        tokens[index] = tokens[index].ToUpper();
                    }
                    _Edit.Select(position, tokens[index].Length);
                    _Edit.SelectedText = tokens[index];
                    position += tokens[index].Length;
                }
                _Edit.Select(start, length);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuToggleCase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                if (_Edit.SelectionType.HasFlag(System.Windows.Forms.RichTextBoxSelectionTypes.Object) ||
                    _Edit.SelectionType.HasFlag(System.Windows.Forms.RichTextBoxSelectionTypes.MultiObject))
                {
                    var message = PNLang.Instance.GetMessageText("selection_objects_warning",
                        "Selected text contains one or more non-text objects (pictures etc). If you continue, these objects will be deleted. Continue anyway?");
                    if (
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                        MessageBoxResult.No)
                    {
                        return;
                    }
                }
                var start = _Edit.SelectionStart;
                var length = _Edit.SelectionLength;
                var arr = _Edit.SelectedText.ToCharArray();
                for (var i = 0; i < arr.Length; i++)
                {
                    if (char.IsLower(arr[i]))
                        arr[i] = char.ToUpper(arr[i]);
                    else
                        arr[i] = char.ToLower(arr[i]);
                }
                _Edit.SelectedText = new string(arr);
                _Edit.Select(start, length);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuFont_Click(object sender, RoutedEventArgs e)
        {
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.FontFamily));
        }

        private void mnuBold_Click(object sender, RoutedEventArgs e)
        {
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.FontBold));
        }

        private void mnuItalic_Click(object sender, RoutedEventArgs e)
        {
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.FontItalic));
        }

        private void mnuUnderline_Click(object sender, RoutedEventArgs e)
        {
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.FontUnderline));
        }

        private void mnuStrikethrough_Click(object sender, RoutedEventArgs e)
        {
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.FontStrikethrough));
        }

        private void mnuAlignLeft_Click(object sender, RoutedEventArgs e)
        {
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.Left));
        }

        private void mnuAlignCenter_Click(object sender, RoutedEventArgs e)
        {
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.Center));
        }

        private void mnuAlignRight_Click(object sender, RoutedEventArgs e)
        {
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.Right));
        }

        private void mnuSpace10_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                _Edit.SetLineSpacing(RELineSpacing.Single);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSpace15_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                _Edit.SetLineSpacing(RELineSpacing.OneAndHalf);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSpace20_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                _Edit.SetLineSpacing(RELineSpacing.Double);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSpace30_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                _Edit.SetLineSpacing(RELineSpacing.Triple);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuAddSpaceBefore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                _Edit.SetParagraphSpacing(REParaSpace.Before, PNStatic.Settings.GeneralSettings.SpacePoints);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuRemoveSpaceBefore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                _Edit.SetParagraphSpacing(REParaSpace.Before, 0);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuRemoveSpaceAfter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                _Edit.SetParagraphSpacing(REParaSpace.After, 0);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuAddSpaceAfter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                _Edit.SetParagraphSpacing(REParaSpace.After, PNStatic.Settings.GeneralSettings.SpacePoints);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSubscript_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                _Edit.SetFontDecoration(REFDecorationMask.CFM_SUBSCRIPT, REFDecorationStyle.CFE_SUBSCRIPT);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSuperscript_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                _Edit.SetFontDecoration(REFDecorationMask.CFM_SUPERSCRIPT, REFDecorationStyle.CFE_SUPERSCRIPT);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuClearFormat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                _Edit.RemoveHighlightColor();
                _Edit.SelectionColor = System.Drawing.SystemColors.WindowText;
                var note = PNStatic.Notes.Note(Handle);
                if (note != null)
                {
                    var gr = PNStatic.Groups.GetGroupByID(note.GroupID);
                    if (gr != null)
                    {
                        _Edit.SetFontByFont(gr.Font);
                    }
                }
                _Edit.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
                _Edit.SetFontDecoration(
                    REFDecorationMask.CFM_UNDERLINE | REFDecorationMask.CFM_BOLD | REFDecorationMask.CFM_ITALIC |
                    REFDecorationMask.CFM_STRIKEOUT | REFDecorationMask.CFM_SUBSCRIPT |
                    REFDecorationMask.CFM_SUPERSCRIPT, REFDecorationStyle.CFE_NONE);
                _Edit.ClearBullets();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuIncreaseIndent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                _Edit.SetIndent(PNStatic.Settings.GeneralSettings.ParagraphIndent);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuDecreaseIndent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                _Edit.SetIndent(-PNStatic.Settings.GeneralSettings.ParagraphIndent);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuInsertPicture_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                var filter = PNLang.Instance.GetCaptionText("image_filter", "Image files");
                filter +=
                    " (*.bmp;*.png;*.gif;*.jpg;*.jpeg;*.ico;*.emf;*.wmf)|*.bmp;*.png;*.gif;*.jpg;*.jpeg;*.ico;*.emf;*.wmf";
                var ofd = new OpenFileDialog { Filter = filter };
                if (!ofd.ShowDialog(this).Value) return;
                using (var image = System.Drawing.Image.FromFile(ofd.FileName))
                {
                    insertImage(image);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuInsertSmiley_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                var smilies = new WndSmilies { Owner = this };
                smilies.SmilieSelected += smilies_ImageSelected;
                var showDialog = smilies.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                {
                    smilies.SmilieSelected -= smilies_ImageSelected;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void smilies_ImageSelected(object sender, SmilieSelectedEventArgs e)
        {
            try
            {
                var smilies = sender as WndSmilies;
                if (smilies != null) smilies.SmilieSelected -= smilies_ImageSelected;
                insertImage(e.Image);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuInsertDT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                var text = DateTime.Now.ToString(PNStatic.Settings.GeneralSettings.DateFormat, ci);
                _Edit.SelectedText = text;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuInsertTable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                var dlgTable = new WndAddTable { Owner = this };
                dlgTable.TableReady += dlgTable_TableReady;
                var showDialog = dlgTable.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                    dlgTable.TableReady -= dlgTable_TableReady;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgTable_TableReady(object sender, TableReadyEventArgs e)
        {
            try
            {
                var d = sender as WndAddTable;
                if (d != null) d.TableReady -= dlgTable_TableReady;
                _Edit.SelectedRtf = e.TableRtf;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuInsertSpecialSymbol_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                var dsps = new WndSpecialSymbols { Owner = this };
                dsps.SpecialSymbolSelected += dsps_SpecialSymbolSelected;
                var showDialog = dsps.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                {
                    dsps.SpecialSymbolSelected -= dsps_SpecialSymbolSelected;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dsps_SpecialSymbolSelected(object sender, SpecialSymbolSelectedEventArgs e)
        {
            try
            {
                var d = sender as WndSpecialSymbols;
                if (d != null) d.SpecialSymbolSelected -= dsps_SpecialSymbolSelected;
                if (!Fonts.IsUnicodeCharAvailable(e.Symbol[0], _Edit.GetFontName()))
                {
                    setDefFont();
                }
                _Edit.SelectedText = e.Symbol;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setDefFont()
        {
            try
            {
                var lf = new LOGFONT();
                lf.Init();
                lf.lfFaceName = PNStrings.DEFAULT_FONT_NAME;
                lf.SetFontSize(_Edit.GetFontSize());
                _Edit.SetFontByName(lf);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuDrawing_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Edit.ReadOnly) return;
                var dlgCanvas = new WndCanvas(this,
                    PNStatic.Settings.GeneralSettings.UseSkins ? Colors.White : ((SolidColorBrush)Background).Color);
                dlgCanvas.CanvasSaved += dlgCanvas_CanvasSaved;
                var showDialog = dlgCanvas.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                    dlgCanvas.CanvasSaved -= dlgCanvas_CanvasSaved;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgCanvas_CanvasSaved(object sender, CanvasSavedEventArgs e)
        {
            try
            {
                var d = sender as WndCanvas;
                if (d != null) d.CanvasSaved -= dlgCanvas_CanvasSaved;
                if (e.Image != null)
                    insertImage(e.Image, true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuCheckSpellNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Spellchecking.CheckRESpelling(_Edit, PHeader.Title))
                {
                    PNMessageBox.Show(PNLang.Instance.GetSpellText("msgComplete", "Spell checking complete"));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuCheckSpellAuto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PNStatic.Settings.GeneralSettings.SpellMode = !mnuCheckSpellAuto.IsChecked ? 1 : 0;
                PNData.SaveSpellSettings();
                PNStatic.FormMain.ApplySpellStatusChange(!mnuCheckSpellAuto.IsChecked);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuDownloadDict_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var updater = new PNUpdateChecker();
                IEnumerable<DictData> list;

                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    list = updater.GetListDictionaries();
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }

                if (list != null)
                {
                    var d = new WndGetDicts(list) {Owner = PNStatic.FormMain};
                    d.ShowDialog();
                }
                else
                {
                    if (
                        PNMessageBox.Show(
                            PNLang.Instance.GetMessageText("dict_connection_problem",
                                "The dictionaries download server is unavailable. Open dictionaries page in browser?"),
                            PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                        MessageBoxResult.Yes)
                    {
                        PNStatic.LoadPage(PNStrings.URL_DICTIONARIES);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuFind_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new WndSearchReplace(SearchReplace.Search, _Edit) { Owner = this };
                dlg.Show();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuFindNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(PNStatic.FindString)) return;
                int position;
                switch (PNStatic.SearchMode)
                {
                    case SearchMode.Normal:
                        position = _Edit.SelectionStart == _Edit.TextLength
                            ? PNStatic.FindEditString(_Edit, 0)
                            : PNStatic.FindEditString(_Edit, _Edit.SelectionStart + 1);
                        if (position == -1)
                        {
                            PNStatic.FindEditString(_Edit, 0);
                        }
                        break;
                    case SearchMode.RegularExp:
                        var reverse = (PNStatic.FindOptions & System.Windows.Forms.RichTextBoxFinds.Reverse) == System.Windows.Forms.RichTextBoxFinds.Reverse;
                        position = _Edit.SelectionStart == _Edit.TextLength
                            ? PNStatic.FindEditStringByRegExp(_Edit, 0, reverse)
                            : PNStatic.FindEditStringByRegExp(_Edit, _Edit.SelectionStart + 1, reverse);
                        if (position == -1)
                        {
                            if (reverse)
                                PNStatic.FindEditStringByRegExp(_Edit, _Edit.TextLength - 1, true);
                            else
                                PNStatic.FindEditStringByRegExp(_Edit, 0, false);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuReplace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new WndSearchReplace(SearchReplace.Replace, _Edit) { Owner = this };
                dlg.Show();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSelectAll_Click(object sender, RoutedEventArgs e)
        {
            _Edit.SelectAll();
        }

        private void mnuSortAscending_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sortEdit(System.Windows.Forms.SortOrder.Ascending);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuSortDescending_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sortEdit(System.Windows.Forms.SortOrder.Descending);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuInsertContent_Click(object sender, RoutedEventArgs e)
        {
            _DropCase = DropCase.Content;
        }

        private void mnuInsertObject_Click(object sender, RoutedEventArgs e)
        {
            _DropCase = DropCase.Object;
        }

        private void mnuInsertLink_Click(object sender, RoutedEventArgs e)
        {
            _DropCase = DropCase.Link;
        }

        private void createDictMenu()
        {
            try
            {
                if (Spellchecking.Initialized && !string.IsNullOrWhiteSpace(PNStatic.Settings.GeneralSettings.SpellDict))
                {
                    mnuCheckSpellNow.IsEnabled = true;
                    mnuCheckSpellAuto.IsEnabled = true;
                }
                else
                {
                    mnuCheckSpellNow.IsEnabled = false;
                    mnuCheckSpellAuto.IsEnabled = false;
                }
                mnuCheckSpellAuto.IsChecked = PNStatic.Settings.GeneralSettings.SpellMode == 1;

                var count = mnuSpell.Items.Count;
                for (var i = count - 1; i > 4; i--)
                {
                    var ti = mnuSpell.Items[i] as MenuItem;
                    if (ti != null)
                    {
                        ti.Click -= dictMenu_Click;
                    }
                    mnuSpell.Items.RemoveAt(i);
                }

                var files = new DirectoryInfo(PNPaths.Instance.DictDir).GetFiles("*.dic");
                if (files.Length > 0)
                    mnuSpell.Items.Add(new Separator());
                var orderedFiles = files.OrderBy(f => f.Name);
                foreach (var fi in orderedFiles)
                {
                    if (!File.Exists(Path.ChangeExtension(fi.FullName, "aff"))) continue;
                    var text = Path.GetFileNameWithoutExtension(fi.Name);
                    try
                    {
                        if (PNStatic.Dictionaries.Root != null)
                        {
                            var xe = PNStatic.Dictionaries.Root.Element(text);
                            if (xe != null)
                            {
                                text = xe.Value;
                            }
                        }
                    }
                    catch
                    {
                    }
                    var mi = new MenuItem { Header = text, Tag = fi.Name };
                    mi.Click += dictMenu_Click;
                    if (PNStatic.Settings.GeneralSettings.SpellDict == fi.Name)
                    {
                        mi.IsChecked = true;
                    }
                    mnuSpell.Items.Add(mi);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dictMenu_Click(object sender, EventArgs e)
        {
            try
            {
                var mi = sender as MenuItem;
                if (mi == null) return;
                if (!mi.IsChecked)
                {
                    PNStatic.Settings.GeneralSettings.SpellDict = (string)mi.Tag;
                    Spellchecking.HuspellStop();
                    var fileDict = Path.Combine(PNPaths.Instance.DictDir, PNStatic.Settings.GeneralSettings.SpellDict);
                    var fileAff = Path.Combine(PNPaths.Instance.DictDir,
                        Path.ChangeExtension(PNStatic.Settings.GeneralSettings.SpellDict,
                            ".aff"));
                    Spellchecking.HunspellInit(fileDict, fileAff);
                }
                else
                {
                    PNStatic.Settings.GeneralSettings.SpellDict = "";
                    PNStatic.Settings.GeneralSettings.SpellMode = 0;
                    Spellchecking.HuspellStop();
                }
                PNData.SaveSpellSettings();
                PNStatic.FormMain.ApplySpellDictionaryChange();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void BulletsMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var menu = sender as MenuItem;
                if (menu == null) return;
                switch (menu.Name)
                {
                    case "mnuNoBullets":
                        _Edit.ClearBullets();
                        break;
                    case "mnuBullets":
                        _Edit.SetBullets(RENumbering.PFN_BULLET, 0, PNStatic.Settings.GeneralSettings.BulletsIndent);
                        break;
                    case "mnuArabicPoint":
                        _Edit.SetBullets(RENumbering.PFN_ARABIC, RENumberingStyle.PFNS_PERIOD,
                            PNStatic.Settings.GeneralSettings.BulletsIndent);
                        break;
                    case "mnuArabicParts":
                        _Edit.SetBullets(RENumbering.PFN_ARABIC, RENumberingStyle.PFNS_PAREN,
                            PNStatic.Settings.GeneralSettings.BulletsIndent);
                        break;
                    case "mnuSmallLettersPoint":
                        _Edit.SetBullets(RENumbering.PFN_LCLETTER, RENumberingStyle.PFNS_PERIOD,
                            PNStatic.Settings.GeneralSettings.BulletsIndent);
                        break;
                    case "mnuSmallLettersPart":
                        _Edit.SetBullets(RENumbering.PFN_LCLETTER, RENumberingStyle.PFNS_PAREN,
                            PNStatic.Settings.GeneralSettings.BulletsIndent);
                        break;
                    case "mnuBigLettersPoint":
                        _Edit.SetBullets(RENumbering.PFN_UCLETTER, RENumberingStyle.PFNS_PERIOD,
                            PNStatic.Settings.GeneralSettings.BulletsIndent);
                        break;
                    case "mnuBigLettersParts":
                        _Edit.SetBullets(RENumbering.PFN_UCLETTER, RENumberingStyle.PFNS_PAREN,
                            PNStatic.Settings.GeneralSettings.BulletsIndent);
                        break;
                    case "mnuLatinSmall":
                        _Edit.SetBullets(RENumbering.PFN_LCROMAN, RENumberingStyle.PFNS_PERIOD,
                            PNStatic.Settings.GeneralSettings.BulletsIndent);
                        break;
                    case "mnuLatinBig":
                        _Edit.SetBullets(RENumbering.PFN_UCROMAN, RENumberingStyle.PFNS_PERIOD,
                            PNStatic.Settings.GeneralSettings.BulletsIndent);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void SizeMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    var mi = sender as MenuItem;
                    if (mi == null) return;
                    var fontSize = int.Parse(mi.Header.ToString(), PNStatic.CultureInvariant);
                    _Edit.SetFontSize(fontSize);
                }
                catch (Exception ex)
                {
                    PNStatic.LogException(ex);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FontColorMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mi = sender as MenuItem;
                if (mi == null) return;
                var brush = mi.Background as SolidColorBrush;
                _Edit.SelectionColor = mi.Name != "mnuColorAutomatic"
                    ? (brush != null
                        ? System.Drawing.Color.FromArgb(brush.Color.A, brush.Color.R,
                            brush.Color.G, brush.Color.B)
                        : System.Drawing.SystemColors.WindowText)
                    : System.Drawing.SystemColors.WindowText;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FontHighlightMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mi = sender as MenuItem;
                if (mi == null) return;
                if (mi.Name == "mnuNoHighlight")
                {
                    _Edit.RemoveHighlightColor();
                }
                else
                {
                    var brush = mi.Background as SolidColorBrush;
                    if (brush != null)
                        _Edit.SelectionBackColor = System.Drawing.Color.FromArgb(brush.Color.A, brush.Color.R,
                            brush.Color.G, brush.Color.B);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void sortEdit(System.Windows.Forms.SortOrder order)
        {
            try
            {
                if (order == System.Windows.Forms.SortOrder.None) return;
                var hiddenEdit = new PNRichEditBox();
                var isTable = _Edit.Rtf.Contains("\\trowd\\trgaph");
                var sortedLines = order == System.Windows.Forms.SortOrder.Ascending
                    ? new SortedList<string, Tuple<int, int, bool>>(new AscStringComparer())
                    : new SortedList<string, Tuple<int, int, bool>>(new DescStringComparer());
                int start = 0, index = 0;
                var tableLine = "";
                var tableStart = 0;
                var addTable = false;
                foreach (var line in _Edit.Lines)
                {
                    if (isTable)
                    {
                        _Edit.Select(start, line.Length);
                        var tbl = _Edit.SelectedRtf.Contains("\\trowd\\trgaph");
                        if (line.Trim().Length == 0 && tbl && !line.Contains('\t'))
                        {
                            start += line.Length + 1;
                            continue;
                        }
                        if (tbl)
                        {
                            tableLine += line;
                            if (tableStart == 0) tableStart = start;
                            addTable = true;
                        }
                        else
                        {
                            if (addTable)
                            {
                                sortedLines.Add(tableLine + index++, Tuple.Create(tableStart, tableLine.Length, true));
                                tableLine = "";
                                tableStart = 0;
                                addTable = false;
                            }
                            sortedLines.Add(line + index++, Tuple.Create(start, line.Length, false));
                        }
                    }
                    else
                    {
                        sortedLines.Add(line + index++, Tuple.Create(start, line.Length, false));
                    }
                    start += line.Length + 1;
                }
                index = 1;
                foreach (var sl in sortedLines)
                {
                    if (sl.Value.Item2 > 0)
                    {
                        _Edit.Select(sl.Value.Item1, sl.Value.Item2);
                        _Edit.Copy();
                        hiddenEdit.Paste();
                    }
                    if (index < sortedLines.Count && !sl.Value.Item3)
                        hiddenEdit.AppendText("\r\n");
                    index++;
                }
                hiddenEdit.SelectAll();
                hiddenEdit.Copy();
                _Edit.SelectAll();
                _Edit.Paste();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion
    }
}
