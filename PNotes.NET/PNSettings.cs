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
//using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using SystemColors = System.Windows.SystemColors;

namespace PNotes.NET
{
    [Serializable]
    internal class PNSettings : IDisposable
    {
        internal const int DEF_WIDTH = 256;
        internal const int DEF_HEIGHT = 256;

        private PNGeneralSettings _GeneralSettings = new PNGeneralSettings();
        private PNSchedule _Schedule = new PNSchedule();
        private PNBehavior _Behavior = new PNBehavior();
        private PNProtection _Protection = new PNProtection();
        private PNDiary _Diary = new PNDiary();
        private PNNetwork _Network = new PNNetwork();
        [NonSerialized]
        private PNConfig _Config = new PNConfig();

        internal PNGeneralSettings GeneralSettings
        {
            get { return _GeneralSettings; }
            set { _GeneralSettings = value; }
        }
        internal PNConfig Config
        {
            get { return _Config; }
            set { _Config = value; }
        }
        internal PNNetwork Network
        {
            get { return _Network; }
            set { _Network = value; }
        }
        internal PNDiary Diary
        {
            get { return _Diary; }
            set { _Diary = value; }
        }
        internal PNProtection Protection
        {
            get { return _Protection; }
            set { _Protection = value; }
        }
        internal PNBehavior Behavior
        {
            get { return _Behavior; }
            set { _Behavior = value; }
        }
        internal PNSchedule Schedule
        {
            get { return _Schedule; }
            set { _Schedule = value; }
        }

        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            PNSettings set = obj as PNSettings;
            if ((object)set == null)
                return false;
            return (_GeneralSettings == set._GeneralSettings
                && _Schedule == set._Schedule
                && _Behavior == set._Behavior
                && _Protection == set._Protection
                && _Diary == set._Diary
                && _Network == set.Network);
        }

        public bool Equals(PNSettings set)
        {
            if ((object)set == null)
                return false;
            return (_GeneralSettings == set._GeneralSettings
                && _Schedule == set._Schedule
                && _Behavior == set._Behavior
                && _Protection == set._Protection
                && _Diary == set._Diary
                && _Network == set.Network);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += GeneralSettings.GetHashCode();
            result *= 37;
            result += Schedule.GetHashCode();
            result *= 37;
            result += Behavior.GetHashCode();
            result *= 37;
            result += Protection.GetHashCode();
            result *= 37;
            result += Diary.GetHashCode();
            result *= 37;
            result += Network.GetHashCode();
            return result;
        }

        public static bool operator ==(PNSettings a, PNSettings b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return (a._GeneralSettings == b._GeneralSettings
                && a._Schedule == b._Schedule
                && a._Behavior == b._Behavior
                && a._Protection == b._Protection
                && a._Diary == b._Diary
                && a.Network == b.Network);
        }

        public static bool operator !=(PNSettings a, PNSettings b)
        {
            return (!(a == b));
        }
    }

    [Serializable]
    internal class PNGeneralSettings : ICloneable
    {
        private bool _RunOnStart;
        private bool _ShowCPOnStart;
        private bool _CheckNewVersionOnStart;
        private string _Language = "english_us.xml";
        private bool _HideToolbar;
        private bool _UseCustomFonts;
        private System.Windows.Forms.RichTextBoxScrollBars _ShowScrollbar;
        private bool _HideDeleteButton;
        private bool _ChangeHideToDelete;
        private bool _HideHideButton;
        private short _BulletsIndent = 400;
        private short _MarginWidth = 4;
        private bool _SaveOnExit = true;
        private bool _ConfirmSaving = true;
        private bool _ConfirmBeforeDeletion = true;
        private bool _SaveWithoutConfirmOnHide;
        private bool _WarnOnAutomaticalDelete;
        private int _RemoveFromBinPeriod;
        private bool _Autosave;
        private int _AutosavePeriod = 5;
        private string _DateFormat = "dd MMM yyyy";
        private string _TimeFormat = "HH:mm";
        private int _Width = PNSettings.DEF_WIDTH;
        private int _Height = PNSettings.DEF_HEIGHT;
        private System.Drawing.Color _SpellColor = System.Drawing.Color.FromArgb(255, 255, 0, 0);
        private bool _UseSkins;
        private int _SpellMode;
        private string _SpellDict = "";
        private int _DockWidth = PNSettings.DEF_WIDTH;
        private int _DockHeight = PNSettings.DEF_HEIGHT;
        private DateBitField _DateBits = new DateBitField(0x1f);
        private DateBitField _TimeBits = new DateBitField(0x3);
        private bool _ShowPriorityOnStart;
        private ToolStripButtonSize _ButtonsSize = ToolStripButtonSize.Normal;
        private bool _AutomaticSmilies;
        private int _SpacePoints = 12;
        private bool _RestoreAuto;
        private int _ParagraphIndent = 400;
        private bool _CheckCriticalOnStart;
        private bool _CheckCriticalPeriodically;

        internal bool CloseOnShortcut { get; set; }
        internal bool DeleteShortcutsOnExit { get; set; }
        internal bool RestoreShortcutsOnStart { get; set; }
        internal bool AutoHeight { get; set; }
        internal int ParagraphIndent
        {
            get { return _ParagraphIndent; }
            set { _ParagraphIndent = value; }
        }
        internal bool RestoreAuto
        {
            get { return _RestoreAuto; }
            set { _RestoreAuto = value; }
        }
        internal int SpacePoints
        {
            get { return _SpacePoints; }
            set { _SpacePoints = value; }
        }
        internal bool AutomaticSmilies
        {
            get { return _AutomaticSmilies; }
            set { _AutomaticSmilies = value; }
        }
        internal ToolStripButtonSize ButtonsSize
        {
            get { return _ButtonsSize; }
            set { _ButtonsSize = value; }
        }
        internal bool ShowPriorityOnStart
        {
            get { return _ShowPriorityOnStart; }
            set { _ShowPriorityOnStart = value; }
        }
        internal DateBitField DateBits
        {
            get { return _DateBits; }
        }
        internal DateBitField TimeBits
        {
            get { return _TimeBits; }
        }
        internal int DockHeight
        {
            get { return _DockHeight; }
            set { _DockHeight = value; }
        }
        internal int DockWidth
        {
            get { return _DockWidth; }
            set { _DockWidth = value; }
        }
        internal string SpellDict
        {
            get { return _SpellDict; }
            set { _SpellDict = value; }
        }
        internal int SpellMode
        {
            get { return _SpellMode; }
            set { _SpellMode = value; }
        }
        internal bool UseSkins
        {
            get { return _UseSkins; }
            set { _UseSkins = value; }
        }
        internal System.Drawing.Color SpellColor
        {
            get { return _SpellColor; }
            set { _SpellColor = value; }
        }
        internal int Height
        {
            get { return _Height; }
            set { _Height = value; }
        }
        internal int Width
        {
            get { return _Width; }
            set { _Width = value; }
        }
        internal string TimeFormat
        {
            get { return _TimeFormat; }
            set
            {
                _TimeFormat = value;
                _TimeBits[DatePart.Hour] = _TimeFormat.Contains("H");
                _TimeBits[DatePart.Minute] = _TimeFormat.Contains("m");
                _TimeBits[DatePart.Second] = _TimeFormat.Contains("s");
                _TimeBits[DatePart.Millisecond] = _TimeFormat.Contains("f");
            }
        }
        internal string DateFormat
        {
            get { return _DateFormat; }
            set
            {
                _DateFormat = value;
                _DateBits[DatePart.Year] = _DateFormat.Contains("y");
                _DateBits[DatePart.Month] = _DateFormat.Contains("M");
                _DateBits[DatePart.Day] = _DateFormat.Contains("d");
                _DateBits[DatePart.Hour] = _TimeFormat.Contains("H");
                _DateBits[DatePart.Minute] = _TimeFormat.Contains("m");
                _DateBits[DatePart.Second] = _TimeFormat.Contains("s");
                _DateBits[DatePart.Millisecond] = _TimeFormat.Contains("f");
            }
        }
        internal int AutosavePeriod
        {
            get { return _AutosavePeriod; }
            set { _AutosavePeriod = value; }
        }
        internal bool Autosave
        {
            get { return _Autosave; }
            set { _Autosave = value; }
        }
        internal int RemoveFromBinPeriod
        {
            get { return _RemoveFromBinPeriod; }
            set { _RemoveFromBinPeriod = value; }
        }
        internal bool WarnOnAutomaticalDelete
        {
            get { return _WarnOnAutomaticalDelete; }
            set { _WarnOnAutomaticalDelete = value; }
        }
        internal bool SaveWithoutConfirmOnHide
        {
            get { return _SaveWithoutConfirmOnHide; }
            set { _SaveWithoutConfirmOnHide = value; }
        }
        internal bool ConfirmBeforeDeletion
        {
            get { return _ConfirmBeforeDeletion; }
            set { _ConfirmBeforeDeletion = value; }
        }
        internal bool ConfirmSaving
        {
            get { return _ConfirmSaving; }
            set { _ConfirmSaving = value; }
        }
        internal bool SaveOnExit
        {
            get { return _SaveOnExit; }
            set { _SaveOnExit = value; }
        }
        internal short MarginWidth
        {
            get { return _MarginWidth; }
            set { _MarginWidth = value; }
        }
        internal short BulletsIndent
        {
            get { return _BulletsIndent; }
            set { _BulletsIndent = value; }
        }
        internal bool HideHideButton
        {
            get { return _HideHideButton; }
            set { _HideHideButton = value; }
        }
        internal bool ChangeHideToDelete
        {
            get { return _ChangeHideToDelete; }
            set { _ChangeHideToDelete = value; }
        }
        internal bool HideDeleteButton
        {
            get { return _HideDeleteButton; }
            set { _HideDeleteButton = value; }
        }
        internal System.Windows.Forms.RichTextBoxScrollBars ShowScrollbar
        {
            get { return _ShowScrollbar; }
            set { _ShowScrollbar = value; }
        }
        internal bool UseCustomFonts
        {
            get { return _UseCustomFonts; }
            set { _UseCustomFonts = value; }
        }
        internal bool HideToolbar
        {
            get { return _HideToolbar; }
            set { _HideToolbar = value; }
        }
        internal string Language
        {
            get { return _Language; }
            set { _Language = value; }
        }
        internal bool CheckNewVersionOnStart
        {
            get { return _CheckNewVersionOnStart; }
            set { _CheckNewVersionOnStart = value; }
        }
        internal bool ShowCPOnStart
        {
            get { return _ShowCPOnStart; }
            set { _ShowCPOnStart = value; }
        }
        internal bool RunOnStart
        {
            get { return _RunOnStart; }
            set { _RunOnStart = value; }
        }

        internal bool CheckCriticalOnStart
        {
            get { return _CheckCriticalOnStart; }
            set { _CheckCriticalOnStart = value; }
        }

        internal bool CheckCriticalPeriodically
        {
            get { return _CheckCriticalPeriodically; }
            set { _CheckCriticalPeriodically = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var o = obj as PNGeneralSettings;
            if ((object)o == null)
                return false;
            return (_CheckNewVersionOnStart == o._CheckNewVersionOnStart
                && _Language == o._Language
                && _RunOnStart == o._RunOnStart
                && _ShowCPOnStart == o._ShowCPOnStart
                && _BulletsIndent == o._BulletsIndent
                && _ChangeHideToDelete == o._ChangeHideToDelete
                && _HideDeleteButton == o._HideDeleteButton
                && _HideHideButton == o._HideHideButton
                && _HideToolbar == o._HideToolbar
                && _MarginWidth == o._MarginWidth
                && _UseCustomFonts == o._UseCustomFonts
                && _ShowScrollbar == o._ShowScrollbar
                && _Autosave == o._Autosave
                && _AutosavePeriod == o._AutosavePeriod
                && _ConfirmBeforeDeletion == o._ConfirmBeforeDeletion
                && _ConfirmSaving == o._ConfirmSaving
                && _RemoveFromBinPeriod == o._RemoveFromBinPeriod
                && _SaveOnExit == o._SaveOnExit
                && _SaveWithoutConfirmOnHide == o._SaveWithoutConfirmOnHide
                && _WarnOnAutomaticalDelete == o._WarnOnAutomaticalDelete
                && _DateFormat == o._DateFormat
                && _TimeFormat == o._TimeFormat
                && _Height == o._Height
                && _Width == o._Width
                && _SpellColor == o._SpellColor
                && _UseSkins == o._UseSkins
                && _SpellDict == o._SpellDict
                && _SpellMode == o._SpellMode
                && _DockWidth == o._DockWidth
                && _DockHeight == o._DockHeight
                && _ShowPriorityOnStart == o._ShowPriorityOnStart
                && _ButtonsSize == o._ButtonsSize
                && _AutomaticSmilies == o._AutomaticSmilies
                && _SpacePoints == o._SpacePoints
                && _RestoreAuto == o._RestoreAuto
                && _ParagraphIndent == o._ParagraphIndent
                && AutoHeight == o.AutoHeight
                && _CheckCriticalOnStart == o._CheckCriticalOnStart
                && _CheckCriticalPeriodically == o._CheckCriticalPeriodically
                && DeleteShortcutsOnExit == o.DeleteShortcutsOnExit
                && RestoreShortcutsOnStart == o.RestoreShortcutsOnStart
                && CloseOnShortcut == o.CloseOnShortcut);
        }

        public bool Equals(PNGeneralSettings o)
        {
            if ((object)o == null)
                return false;
            return (_CheckNewVersionOnStart == o._CheckNewVersionOnStart
                && _Language == o._Language
                && _RunOnStart == o._RunOnStart
                && _ShowCPOnStart == o._ShowCPOnStart
                && _BulletsIndent == o._BulletsIndent
                && _ChangeHideToDelete == o._ChangeHideToDelete
                && _HideDeleteButton == o._HideDeleteButton
                && _HideHideButton == o._HideHideButton
                && _HideToolbar == o._HideToolbar
                && _MarginWidth == o._MarginWidth
                && _UseCustomFonts == o._UseCustomFonts
                && _ShowScrollbar == o._ShowScrollbar
                && _Autosave == o._Autosave
                && _AutosavePeriod == o._AutosavePeriod
                && _ConfirmBeforeDeletion == o._ConfirmBeforeDeletion
                && _ConfirmSaving == o._ConfirmSaving
                && _RemoveFromBinPeriod == o._RemoveFromBinPeriod
                && _SaveOnExit == o._SaveOnExit
                && _SaveWithoutConfirmOnHide == o._SaveWithoutConfirmOnHide
                && _WarnOnAutomaticalDelete == o._WarnOnAutomaticalDelete
                && _DateFormat == o._DateFormat
                && _TimeFormat == o._TimeFormat
                && _Height == o._Height
                && _Width == o._Width
                && _SpellColor == o._SpellColor
                && _UseSkins == o._UseSkins
                && _SpellDict == o._SpellDict
                && _SpellMode == o._SpellMode
                && _DockWidth == o._DockWidth
                && _DockHeight == o._DockHeight
                && _ShowPriorityOnStart == o._ShowPriorityOnStart
                && _ButtonsSize == o._ButtonsSize
                && _AutomaticSmilies == o._AutomaticSmilies
                && _SpacePoints == o._SpacePoints
                && _RestoreAuto == o._RestoreAuto
                && _ParagraphIndent == o._ParagraphIndent
                && AutoHeight == o.AutoHeight
                && _CheckCriticalOnStart == o._CheckCriticalOnStart
                && _CheckCriticalPeriodically == o._CheckCriticalPeriodically
                && DeleteShortcutsOnExit == o.DeleteShortcutsOnExit
                && RestoreShortcutsOnStart == o.RestoreShortcutsOnStart
                && CloseOnShortcut == o.CloseOnShortcut);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += ShowCPOnStart.GetHashCode();
            result *= 37;
            result += RunOnStart.GetHashCode();
            result *= 37;
            result += Language.GetHashCode();
            result *= 37;
            result += CheckNewVersionOnStart.GetHashCode();
            result *= 37;
            result += BulletsIndent.GetHashCode();
            result *= 37;
            result += ChangeHideToDelete.GetHashCode();
            result *= 37;
            result += HideDeleteButton.GetHashCode();
            result *= 37;
            result += HideHideButton.GetHashCode();
            result *= 37;
            result += HideToolbar.GetHashCode();
            result *= 37;
            result += MarginWidth.GetHashCode();
            result *= 37;
            result += ShowScrollbar.GetHashCode();
            result *= 37;
            result += UseCustomFonts.GetHashCode();
            result *= 37;
            result += WarnOnAutomaticalDelete.GetHashCode();
            result *= 37;
            result += SaveWithoutConfirmOnHide.GetHashCode();
            result *= 37;
            result += SaveOnExit.GetHashCode();
            result *= 37;
            result += RemoveFromBinPeriod.GetHashCode();
            result *= 37;
            result += ConfirmSaving.GetHashCode();
            result *= 37;
            result += ConfirmBeforeDeletion.GetHashCode();
            result *= 37;
            result += AutosavePeriod.GetHashCode();
            result *= 37;
            result += Autosave.GetHashCode();
            result *= 37;
            result += DateFormat.GetHashCode();
            result *= 37;
            result += TimeFormat.GetHashCode();
            result *= 37;
            result += Height.GetHashCode();
            result *= 37;
            result += Width.GetHashCode();
            result *= 37;
            result += SpellColor.GetHashCode();
            result *= 37;
            result += UseSkins.GetHashCode();
            result *= 37;
            result += SpellMode.GetHashCode();
            result *= 37;
            result += SpellDict.GetHashCode();
            result *= 37;
            result += DockWidth.GetHashCode();
            result *= 37;
            result += DockHeight.GetHashCode();
            result *= 37;
            result += ShowPriorityOnStart.GetHashCode();
            result *= 37;
            result += ButtonsSize.GetHashCode();
            result *= 37;
            result += AutomaticSmilies.GetHashCode();
            result *= 37;
            result += SpacePoints.GetHashCode();
            result *= 37;
            result += RestoreAuto.GetHashCode();
            result *= 37;
            result += ParagraphIndent.GetHashCode();
            result *= 37;
            result += AutoHeight.GetHashCode();
            result *= 37;
            result += CheckCriticalOnStart.GetHashCode();
            result *= 37;
            result += CheckCriticalPeriodically.GetHashCode();
            result *= 37;
            result += DeleteShortcutsOnExit.GetHashCode();
            result *= 37;
            result += RestoreShortcutsOnStart.GetHashCode();
            result *= 37;
            result += CloseOnShortcut.GetHashCode();

            return result;
        }

        public static bool operator ==(PNGeneralSettings a, PNGeneralSettings b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return (a._CheckNewVersionOnStart == b._CheckNewVersionOnStart
                && a._Language == b._Language
                && a._RunOnStart == b._RunOnStart
                && a._ShowCPOnStart == b._ShowCPOnStart
                && a._BulletsIndent == b._BulletsIndent
                && a._ChangeHideToDelete == b._ChangeHideToDelete
                && a._HideDeleteButton == b._HideDeleteButton
                && a._HideHideButton == b._HideHideButton
                && a._HideToolbar == b._HideToolbar
                && a._MarginWidth == b._MarginWidth
                && a._ShowScrollbar == b._ShowScrollbar
                && a._UseCustomFonts == b._UseCustomFonts
                && a._Autosave == b._Autosave
                && a._AutosavePeriod == b._AutosavePeriod
                && a._ConfirmBeforeDeletion == b._ConfirmBeforeDeletion
                && a._ConfirmSaving == b._ConfirmSaving
                && a._RemoveFromBinPeriod == b._RemoveFromBinPeriod
                && a._SaveOnExit == b._SaveOnExit
                && a._SaveWithoutConfirmOnHide == b._SaveWithoutConfirmOnHide
                && a._WarnOnAutomaticalDelete == b._WarnOnAutomaticalDelete
                && a._DateFormat == b._DateFormat
                && a._TimeFormat == b._TimeFormat
                && a._Height == b._Height
                && a._Width == b._Width
                && a._SpellColor == b._SpellColor
                && a._UseSkins == b._UseSkins
                && a._SpellDict == b._SpellDict
                && a._SpellMode == b._SpellMode
                && a._DockWidth == b._DockWidth
                && a._DockHeight == b._DockHeight
                && a._ShowPriorityOnStart == b._ShowPriorityOnStart
                && a._ButtonsSize == b._ButtonsSize
                && a._AutomaticSmilies == b._AutomaticSmilies
                && a._SpacePoints == b._SpacePoints
                && a._RestoreAuto == b._RestoreAuto
                && a._ParagraphIndent == b._ParagraphIndent
                && a.AutoHeight == b.AutoHeight
                && a._CheckCriticalOnStart == b._CheckCriticalOnStart
                && a._CheckCriticalPeriodically == b._CheckCriticalPeriodically
                && a.DeleteShortcutsOnExit == b.DeleteShortcutsOnExit
                && a.RestoreShortcutsOnStart == b.RestoreShortcutsOnStart
                && a.CloseOnShortcut == b.CloseOnShortcut);
        }

        public static bool operator !=(PNGeneralSettings a, PNGeneralSettings b)
        {
            return (!(a == b));
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
                return bfd.Deserialize(ms);
            }
        }

        #endregion
    }

    [Serializable]
    public class PNSkinlessDetails : ICloneable
    {
        internal static readonly Color DefColor = Color.FromArgb(255, 242, 221, 116);
        private PNFont _CaptionFont = new PNFont { FontWeight = FontWeights.Bold };
        private Color _BackColor = DefColor;
        private Color _CaptionColor = SystemColors.ControlTextColor;

        //internal PNSkinlessDetails()
        //{
        //    _CaptionFont.Init();
        //}

        public Color CaptionColor
        {
            get { return _CaptionColor; }
            set
            {
                _CaptionColor = value;
            }
        }
        public Color BackColor
        {
            get { return _BackColor; }
            set { _BackColor = value; }
        }
        public PNFont CaptionFont
        {
            get { return _CaptionFont; }
            set
            {
                _CaptionFont = value;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var sk = obj as PNSkinlessDetails;
            if ((object)sk == null)
                return false;
            return (_BackColor == sk._BackColor
                && _CaptionFont == sk._CaptionFont
                && _CaptionColor == sk._CaptionColor);
        }

        public bool Equals(PNSkinlessDetails sk)
        {
            if ((object)sk == null)
                return false;
            return (_BackColor == sk._BackColor
                && _CaptionFont == sk._CaptionFont
                && _CaptionColor == sk._CaptionColor);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += BackColor.GetHashCode();
            result *= 37;
            result += CaptionFont.GetHashCode();
            result *= 37;
            result += CaptionColor.GetHashCode();
            return result;
        }

        public object Clone()
        {
            var sk = new PNSkinlessDetails
            {
                BackColor = _BackColor,
                CaptionColor = _CaptionColor,
                _CaptionFont =
                {
                    FontFamily = _CaptionFont.FontFamily,
                    FontSize = _CaptionFont.FontSize,
                    FontStretch = _CaptionFont.FontStretch,
                    FontStyle = _CaptionFont.FontStyle,
                    FontWeight = _CaptionFont.FontWeight
                }
            };
            return sk;
        }

        public static bool operator ==(PNSkinlessDetails a, PNSkinlessDetails b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return (a._BackColor == b._BackColor
                && a._CaptionFont == b._CaptionFont
                && a._CaptionColor == b._CaptionColor);
        }

        public static bool operator !=(PNSkinlessDetails a, PNSkinlessDetails b)
        {
            return (!(a == b));
        }
    }

    [Serializable]
    public class PNSkinDetails : IDisposable
    {
        public const string NO_SKIN = "(no skin)";

        private string _SkinName = NO_SKIN;
        private string _SkinInfo = "";
        private bool _VerticalToolbar;
        private System.Drawing.Bitmap _BitmapSkin;
        private System.Drawing.Bitmap _BitmapMarks;
        private System.Drawing.Bitmap _BitmapDelHide;
        private System.Drawing.Bitmap _BitmapCommands;
        private System.Drawing.Rectangle _PositionDelHide;
        private System.Drawing.Rectangle _PositionMarks;
        private System.Drawing.Rectangle _PositionToolbar;
        private System.Drawing.Rectangle _PositionEdit;
        private System.Drawing.Rectangle _PositionTooltip;
        private int _MarksCount = 7;
        private System.Drawing.Color _MaskColor = System.Drawing.Color.FromArgb(255, 255, 0, 255);
        private System.Drawing.Bitmap _BitmapPattern;
        private System.Drawing.Bitmap _BitmapInactivePattern;

        public System.Drawing.Bitmap BitmapInactivePattern
        {
            get { return _BitmapInactivePattern; }
            set { _BitmapInactivePattern = value; }
        }
        public System.Drawing.Bitmap BitmapPattern
        {
            get { return _BitmapPattern; }
            set { _BitmapPattern = value; }
        }
        public System.Drawing.Rectangle PositionTooltip
        {
            get { return _PositionTooltip; }
            set { _PositionTooltip = value; }
        }
        public System.Drawing.Rectangle PositionEdit
        {
            get { return _PositionEdit; }
            set { _PositionEdit = value; }
        }
        public System.Drawing.Rectangle PositionToolbar
        {
            get { return _PositionToolbar; }
            set { _PositionToolbar = value; }
        }
        public System.Drawing.Rectangle PositionMarks
        {
            get { return _PositionMarks; }
            set { _PositionMarks = value; }
        }
        public System.Drawing.Rectangle PositionDelHide
        {
            get { return _PositionDelHide; }
            set { _PositionDelHide = value; }
        }
        public System.Drawing.Color MaskColor
        {
            get { return _MaskColor; }
            set { _MaskColor = value; }
        }
        public int MarksCount
        {
            get { return _MarksCount; }
            set { _MarksCount = value; }
        }
        public System.Drawing.Bitmap BitmapCommands
        {
            get { return _BitmapCommands; }
            set { _BitmapCommands = value; }
        }
        public System.Drawing.Bitmap BitmapDelHide
        {
            get { return _BitmapDelHide; }
            set { _BitmapDelHide = value; }
        }
        public System.Drawing.Bitmap BitmapMarks
        {
            get { return _BitmapMarks; }
            set { _BitmapMarks = value; }
        }
        public System.Drawing.Bitmap BitmapSkin
        {
            get { return _BitmapSkin; }
            set { _BitmapSkin = value; }
        }
        public bool VerticalToolbar
        {
            get { return _VerticalToolbar; }
            set { _VerticalToolbar = value; }
        }
        public string SkinInfo
        {
            get { return _SkinInfo; }
            set { _SkinInfo = value; }
        }
        public string SkinName
        {
            get { return _SkinName; }
            set { _SkinName = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            PNSkinDetails sk = obj as PNSkinDetails;
            if ((object)sk == null)
                return false;
            return (_SkinName == sk._SkinName);
        }

        public bool Equals(PNSkinDetails sk)
        {
            if ((object)sk == null)
                return false;
            return (_SkinName == sk._SkinName);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += SkinName.GetHashCode();
            return result;
        }

        public static bool operator ==(PNSkinDetails a, PNSkinDetails b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return (a._SkinName == b._SkinName);
        }

        public static bool operator !=(PNSkinDetails a, PNSkinDetails b)
        {
            return (!(a == b));
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_BitmapCommands != null)
                _BitmapCommands.Dispose();
            if (_BitmapDelHide != null)
                _BitmapDelHide.Dispose();
            if (_BitmapSkin != null)
                _BitmapSkin.Dispose();
            if (_BitmapMarks != null)
                _BitmapMarks.Dispose();
            if (_BitmapPattern != null)
                _BitmapPattern.Dispose();
            if (_BitmapInactivePattern != null)
                _BitmapInactivePattern.Dispose();
        }

        #endregion
    }

    [Serializable]
    internal class PNSchedule : ICloneable
    {
        internal const string DEF_SOUND = "(Default)";

        private string _Sound = DEF_SOUND;
        private string _Voice = "";
        private bool _AllowSoundAlert = true;
        private bool _VisualNotification = true;
        private bool _TrackOverdue;
        private bool _CenterScreen = true;
        private int _VoiceVolume = 100;
        private int _VoiceSpeed;
        private int _VoicePitch;

        internal PNSchedule()
        {
            if (PNStatic.Voices.Count > 0)
                _Voice = PNStatic.Voices[0];
        }

        public bool CenterScreen
        {
            get { return _CenterScreen; }
            set { _CenterScreen = value; }
        }
        public bool TrackOverdue
        {
            get { return _TrackOverdue; }
            set { _TrackOverdue = value; }
        }
        public bool VisualNotification
        {
            get { return _VisualNotification; }
            set { _VisualNotification = value; }
        }
        public bool AllowSoundAlert
        {
            get { return _AllowSoundAlert; }
            set { _AllowSoundAlert = value; }
        }
        internal string Voice
        {
            get { return _Voice; }
            set { _Voice = value; }
        }
        public string Sound
        {
            get { return _Sound; }
            set { _Sound = value; }
        }
        public int VoiceVolume
        {
            get { return _VoiceVolume; }
            set { _VoiceVolume = value; }
        }
        public int VoiceSpeed
        {
            get { return _VoiceSpeed; }
            set { _VoiceSpeed = value; }
        }
        public int VoicePitch
        {
            get { return _VoicePitch; }
            set { _VoicePitch = value; }
        }

        #region ICloneable Members

        public object Clone()
        {
            BinaryFormatter bfs = new BinaryFormatter();
            BinaryFormatter bfd = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bfs.Serialize(ms, this);
                ms.Position = 0;
                return bfd.Deserialize(ms);
            }
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            PNSchedule sch = obj as PNSchedule;
            if ((object)sch == null)
                return false;
            return (_Sound == sch.Sound
                && _Voice == sch._Voice
                && _AllowSoundAlert == sch._AllowSoundAlert
                && _CenterScreen == sch._CenterScreen
                && _TrackOverdue == sch._TrackOverdue
                && _VisualNotification == sch._VisualNotification
                && _VoicePitch == sch._VoicePitch
                && _VoiceSpeed == sch._VoiceSpeed
                && _VoiceVolume == sch._VoiceVolume);
        }

        public bool Equals(PNSchedule sch)
        {
            if ((object)sch == null)
                return false;
            return (_Sound == sch.Sound
               && _Voice == sch._Voice
               && _AllowSoundAlert == sch._AllowSoundAlert
               && _CenterScreen == sch._CenterScreen
               && _TrackOverdue == sch._TrackOverdue
               && _VisualNotification == sch._VisualNotification
               && _VoicePitch == sch._VoicePitch
               && _VoiceSpeed == sch._VoiceSpeed
               && _VoiceVolume == sch._VoiceVolume);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += Sound.GetHashCode();
            result *= 37;
            result += VisualNotification.GetHashCode();
            result *= 37;
            result += TrackOverdue.GetHashCode();
            result *= 37;
            result += CenterScreen.GetHashCode();
            result *= 37;
            result += AllowSoundAlert.GetHashCode();
            result *= 37;
            result += VoiceVolume.GetHashCode();
            result *= 37;
            result += VoiceSpeed.GetHashCode();
            result *= 37;
            result += VoicePitch.GetHashCode();
            return result;
        }

        public static bool operator ==(PNSchedule a, PNSchedule b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return (a._Sound == b.Sound
               && a._Voice == b._Voice
               && a._AllowSoundAlert == b._AllowSoundAlert
               && a._CenterScreen == b._CenterScreen
               && a._TrackOverdue == b._TrackOverdue
               && a._VisualNotification == b._VisualNotification
               && a._VoicePitch == b._VoicePitch
               && a._VoiceSpeed == b._VoiceSpeed
               && a._VoiceVolume == b._VoiceVolume);
        }

        public static bool operator !=(PNSchedule a, PNSchedule b)
        {
            return (!(a == b));
        }
    }

    [Serializable]
    internal class PNDiary
    {
        private bool _CustomSettings;
        private bool _AddWeekday;
        private bool _FullWeekdayName;
        private bool _WeekdayAtTheEnd;
        private bool _DoNotShowPrevious;
        private bool _AscendingOrder;
        private int _NumberOfPages = 7;
        private string _DateFormat = "MMMM dd, yyyy";

        internal string DateFormat
        {
            get { return _DateFormat; }
            set { _DateFormat = value; }
        }
        internal int NumberOfPages
        {
            get { return _NumberOfPages; }
            set { _NumberOfPages = value; }
        }
        internal bool AscendingOrder
        {
            get { return _AscendingOrder; }
            set { _AscendingOrder = value; }
        }
        internal bool DoNotShowPrevious
        {
            get { return _DoNotShowPrevious; }
            set { _DoNotShowPrevious = value; }
        }
        internal bool WeekdayAtTheEnd
        {
            get { return _WeekdayAtTheEnd; }
            set { _WeekdayAtTheEnd = value; }
        }
        internal bool FullWeekdayName
        {
            get { return _FullWeekdayName; }
            set { _FullWeekdayName = value; }
        }
        internal bool AddWeekday
        {
            get { return _AddWeekday; }
            set { _AddWeekday = value; }
        }
        internal bool CustomSettings
        {
            get { return _CustomSettings; }
            set { _CustomSettings = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            PNDiary dia = obj as PNDiary;
            if ((object)dia == null)
                return false;
            return (_AddWeekday == dia._AddWeekday
                && _AscendingOrder == dia._AscendingOrder
                && _CustomSettings == dia._CustomSettings
                && _DateFormat == dia._DateFormat
                && _DoNotShowPrevious == dia._DoNotShowPrevious
                && _FullWeekdayName == dia._FullWeekdayName
                && _NumberOfPages == dia._NumberOfPages
                && _WeekdayAtTheEnd == dia._WeekdayAtTheEnd);
        }

        public bool Equals(PNDiary dia)
        {
            if ((object)dia == null)
                return false;
            return (_AddWeekday == dia._AddWeekday
                && _AscendingOrder == dia._AscendingOrder
                && _CustomSettings == dia._CustomSettings
                && _DateFormat == dia._DateFormat
                && _DoNotShowPrevious == dia._DoNotShowPrevious
                && _FullWeekdayName == dia._FullWeekdayName
                && _NumberOfPages == dia._NumberOfPages
                && _WeekdayAtTheEnd == dia._WeekdayAtTheEnd);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += WeekdayAtTheEnd.GetHashCode();
            result *= 37;
            result += NumberOfPages.GetHashCode();
            result *= 37;
            result += FullWeekdayName.GetHashCode();
            result *= 37;
            result += DoNotShowPrevious.GetHashCode();
            result *= 37;
            result += DateFormat.GetHashCode();
            result *= 37;
            result += CustomSettings.GetHashCode();
            result *= 37;
            result += AscendingOrder.GetHashCode();
            result *= 37;
            result += AddWeekday.GetHashCode();
            return result;
        }

        public static bool operator ==(PNDiary a, PNDiary b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return (a._AddWeekday == b._AddWeekday
                && a._AscendingOrder == b._AscendingOrder
                && a._CustomSettings == b._CustomSettings
                && a._DateFormat == b._DateFormat
                && a._DoNotShowPrevious == b._DoNotShowPrevious
                && a._FullWeekdayName == b._FullWeekdayName
                && a._NumberOfPages == b._NumberOfPages
                && a._WeekdayAtTheEnd == b._WeekdayAtTheEnd);
        }

        public static bool operator !=(PNDiary a, PNDiary b)
        {
            return (!(a == b));
        }
    }

    [Serializable]
    internal class PNBehavior
    {
        private bool _NewNoteAlwaysOnTop;
        private bool _RelationalPositioning;
        private bool _HideCompleted;
        private bool _BigIconsOnCP;
        private bool _DoNotShowNotesInList;
        private bool _KeepVisibleOnShowDesktop;
        private TrayMouseAction _DoubleClickAction = TrayMouseAction.NewNote;
        private TrayMouseAction _SingleClickAction = TrayMouseAction.None;
        private DefaultNaming _DefaultNaming = DefaultNaming.FirstCharacters;
        private int _DefaultNameLength = 128;
        private int _ContentColumnLength = 24;
        private bool _HideFluently;
        private bool _PlaySoundOnHide;
        private double _Opacity = 1.0;
        private bool _RandomBackColor;
        private bool _InvertTextColor;
        private bool _RollOnDblClick;
        private bool _FitWhenRolled;
        private bool _ShowSeparateNotes;
        private PinClickAction _PinClickAction = PinClickAction.Toggle;
        private NoteStartPosition _StartPosition = NoteStartPosition.Center;
        private bool _HideMainWindow = true;
        private string _Theme = PNStrings.DEF_THEME;
        private bool _PreventAutomaticResizing = true;
        private bool _ShowNotesPanel;
        private NotesPanelOrientation _NotesPanelDock = NotesPanelOrientation.Top;
        private bool _PanelAutoHide;
        private PanelRemoveMode _PanelRemoveMode = PanelRemoveMode.SingleClick;
        private bool _PanelSwitchOffAnimation;

        internal int PanelEnterDelay { get; set; }

        internal PanelRemoveMode PanelRemoveMode
        {
            get { return _PanelRemoveMode; }
            set { _PanelRemoveMode = value; }
        }

        internal bool PanelAutoHide
        {
            get { return _PanelAutoHide; }
            set { _PanelAutoHide = value; }
        }

        internal NotesPanelOrientation NotesPanelOrientation
        {
            get { return _NotesPanelDock; }
            set { _NotesPanelDock = value; }
        }

        internal bool ShowNotesPanel
        {
            get { return _ShowNotesPanel; }
            set { _ShowNotesPanel = value; }
        }

        internal bool PreventAutomaticResizing
        {
            get { return _PreventAutomaticResizing; }
            set { _PreventAutomaticResizing = value; }
        }

        internal bool HideMainWindow
        {
            get { return _HideMainWindow; }
            set { _HideMainWindow = value; }
        }
        internal NoteStartPosition StartPosition
        {
            get { return _StartPosition; }
            set { _StartPosition = value; }
        }
        internal PinClickAction PinClickAction
        {
            get { return _PinClickAction; }
            set { _PinClickAction = value; }
        }
        internal bool ShowSeparateNotes
        {
            get { return _ShowSeparateNotes; }
            set { _ShowSeparateNotes = value; }
        }
        internal bool FitWhenRolled
        {
            get { return _FitWhenRolled; }
            set { _FitWhenRolled = value; }
        }
        internal bool RollOnDblClick
        {
            get { return _RollOnDblClick; }
            set { _RollOnDblClick = value; }
        }
        internal bool InvertTextColor
        {
            get { return _InvertTextColor; }
            set { _InvertTextColor = value; }
        }
        internal bool RandomBackColor
        {
            get { return _RandomBackColor; }
            set { _RandomBackColor = value; }
        }
        internal double Opacity
        {
            get { return _Opacity; }
            set { _Opacity = value; }
        }
        internal bool PlaySoundOnHide
        {
            get { return _PlaySoundOnHide; }
            set { _PlaySoundOnHide = value; }
        }
        internal bool HideFluently
        {
            get { return _HideFluently; }
            set { _HideFluently = value; }
        }
        internal int ContentColumnLength
        {
            get { return _ContentColumnLength; }
            set { _ContentColumnLength = value; }
        }
        internal int DefaultNameLength
        {
            get { return _DefaultNameLength; }
            set { _DefaultNameLength = value; }
        }
        internal DefaultNaming DefaultNaming
        {
            get { return _DefaultNaming; }
            set { _DefaultNaming = value; }
        }
        internal TrayMouseAction SingleClickAction
        {
            get { return _SingleClickAction; }
            set { _SingleClickAction = value; }
        }
        internal TrayMouseAction DoubleClickAction
        {
            get { return _DoubleClickAction; }
            set { _DoubleClickAction = value; }
        }
        internal bool KeepVisibleOnShowDesktop
        {
            get { return _KeepVisibleOnShowDesktop; }
            set { _KeepVisibleOnShowDesktop = value; }
        }
        internal bool DoNotShowNotesInList
        {
            get { return _DoNotShowNotesInList; }
            set { _DoNotShowNotesInList = value; }
        }
        internal bool BigIconsOnCP
        {
            get { return _BigIconsOnCP; }
            set { _BigIconsOnCP = value; }
        }
        internal bool HideCompleted
        {
            get { return _HideCompleted; }
            set { _HideCompleted = value; }
        }
        internal bool RelationalPositioning
        {
            get { return _RelationalPositioning; }
            set { _RelationalPositioning = value; }
        }
        internal bool NewNoteAlwaysOnTop
        {
            get { return _NewNoteAlwaysOnTop; }
            set { _NewNoteAlwaysOnTop = value; }
        }
        internal string Theme
        {
            get { return _Theme; }
            set { _Theme = value; }
        }

        internal bool PanelSwitchOffAnimation
        {
            get { return _PanelSwitchOffAnimation; }
            set { _PanelSwitchOffAnimation = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var bh = obj as PNBehavior;
            if ((object)bh == null)
                return false;
            return (_FitWhenRolled == bh._FitWhenRolled
                && _InvertTextColor == bh._InvertTextColor
                && _RandomBackColor == bh._RandomBackColor
                && _RollOnDblClick == bh._RollOnDblClick
                && _BigIconsOnCP == bh._BigIconsOnCP
                && _ContentColumnLength == bh._ContentColumnLength
                && _DefaultNameLength == bh._DefaultNameLength
                && _DefaultNaming == bh._DefaultNaming
                && _DoNotShowNotesInList == bh._DoNotShowNotesInList
                && _DoubleClickAction == bh._DoubleClickAction
                && _HideCompleted == bh._HideCompleted
                && _KeepVisibleOnShowDesktop == bh._KeepVisibleOnShowDesktop
                && _NewNoteAlwaysOnTop == bh._NewNoteAlwaysOnTop
                && _RelationalPositioning == bh._RelationalPositioning
                && _SingleClickAction == bh._SingleClickAction
                && _HideFluently == bh._HideFluently
                && _PlaySoundOnHide == bh._PlaySoundOnHide
                && Math.Abs(_Opacity - bh._Opacity) < double.Epsilon
                && _ShowSeparateNotes == bh._ShowSeparateNotes
                && _PinClickAction == bh._PinClickAction
                && _StartPosition == bh._StartPosition
                && _HideMainWindow == bh._HideMainWindow
                && _Theme == bh._Theme
                && _PreventAutomaticResizing == bh._PreventAutomaticResizing
                && _ShowNotesPanel == bh._ShowNotesPanel
                && _NotesPanelDock == bh._NotesPanelDock
                && _PanelAutoHide == bh._PanelAutoHide
                && _PanelRemoveMode == bh._PanelRemoveMode
                && _PanelSwitchOffAnimation == bh._PanelSwitchOffAnimation
                && PanelEnterDelay == bh.PanelEnterDelay);
        }

        public bool Equals(PNBehavior bh)
        {
            if ((object)bh == null)
                return false;
            return (_FitWhenRolled == bh._FitWhenRolled
                && _InvertTextColor == bh._InvertTextColor
                && _RandomBackColor == bh._RandomBackColor
                && _RollOnDblClick == bh._RollOnDblClick
                && _BigIconsOnCP == bh._BigIconsOnCP
                && _ContentColumnLength == bh._ContentColumnLength
                && _DefaultNameLength == bh._DefaultNameLength
                && _DefaultNaming == bh._DefaultNaming
                && _DoNotShowNotesInList == bh._DoNotShowNotesInList
                && _DoubleClickAction == bh._DoubleClickAction
                && _HideCompleted == bh._HideCompleted
                && _KeepVisibleOnShowDesktop == bh._KeepVisibleOnShowDesktop
                && _NewNoteAlwaysOnTop == bh._NewNoteAlwaysOnTop
                && _RelationalPositioning == bh._RelationalPositioning
                && _SingleClickAction == bh._SingleClickAction
                && _HideFluently == bh._HideFluently
                && _PlaySoundOnHide == bh._PlaySoundOnHide
                && Math.Abs(_Opacity - bh._Opacity) < double.Epsilon
                && _ShowSeparateNotes == bh._ShowSeparateNotes
                && _PinClickAction == bh._PinClickAction
                && _StartPosition == bh._StartPosition
                && _HideMainWindow == bh._HideMainWindow
                && _Theme == bh._Theme
                && _PreventAutomaticResizing == bh._PreventAutomaticResizing
                && _ShowNotesPanel == bh._ShowNotesPanel
                && _NotesPanelDock == bh._NotesPanelDock
                && _PanelAutoHide == bh._PanelAutoHide
                && _PanelRemoveMode == bh._PanelRemoveMode
                && _PanelSwitchOffAnimation == bh._PanelSwitchOffAnimation
                && PanelEnterDelay == bh.PanelEnterDelay);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += FitWhenRolled.GetHashCode();
            result *= 37;
            result += InvertTextColor.GetHashCode();
            result *= 37;
            result += RandomBackColor.GetHashCode();
            result *= 37;
            result += RollOnDblClick.GetHashCode();
            result *= 37;
            result += SingleClickAction.GetHashCode();
            result *= 37;
            result += RelationalPositioning.GetHashCode();
            result *= 37;
            result += NewNoteAlwaysOnTop.GetHashCode();
            result *= 37;
            result += KeepVisibleOnShowDesktop.GetHashCode();
            result *= 37;
            result += HideCompleted.GetHashCode();
            result *= 37;
            result += DoubleClickAction.GetHashCode();
            result *= 37;
            result += DoNotShowNotesInList.GetHashCode();
            result *= 37;
            result += DefaultNaming.GetHashCode();
            result *= 37;
            result += DefaultNameLength.GetHashCode();
            result *= 37;
            result += ContentColumnLength.GetHashCode();
            result *= 37;
            result += BigIconsOnCP.GetHashCode();
            result *= 37;
            result += PlaySoundOnHide.GetHashCode();
            result *= 37;
            result += HideFluently.GetHashCode();
            result *= 37;
            result += Opacity.GetHashCode();
            result *= 37;
            result += ShowSeparateNotes.GetHashCode();
            result *= 37;
            result += PinClickAction.GetHashCode();
            result *= 37;
            result += StartPosition.GetHashCode();
            result *= 37;
            result += HideMainWindow.GetHashCode();
            result *= 37;
            result += Theme.GetHashCode();
            result *= 37;
            result += PreventAutomaticResizing.GetHashCode();
            result *= 37;
            result += ShowNotesPanel.GetHashCode();
            result *= 37;
            result += NotesPanelOrientation.GetHashCode();
            result *= 37;
            result += PanelAutoHide.GetHashCode();
            result *= 37;
            result += PanelRemoveMode.GetHashCode();
            result *= 37;
            result += PanelSwitchOffAnimation.GetHashCode();
            result *= 37;
            result += PanelEnterDelay.GetHashCode();

            return result;
        }

        public static bool operator ==(PNBehavior a, PNBehavior b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return (a._FitWhenRolled == b._FitWhenRolled
                && a._InvertTextColor == b._InvertTextColor
                && a._RandomBackColor == b._RandomBackColor
                && a._RollOnDblClick == b._RollOnDblClick
                && a._BigIconsOnCP == b._BigIconsOnCP
                && a._ContentColumnLength == b._ContentColumnLength
                && a._DefaultNameLength == b._DefaultNameLength
                && a._DefaultNaming == b._DefaultNaming
                && a._DoNotShowNotesInList == b._DoNotShowNotesInList
                && a._DoubleClickAction == b._DoubleClickAction
                && a._HideCompleted == b._HideCompleted
                && a._KeepVisibleOnShowDesktop == b._KeepVisibleOnShowDesktop
                && a._NewNoteAlwaysOnTop == b._NewNoteAlwaysOnTop
                && a._RelationalPositioning == b._RelationalPositioning
                && a._SingleClickAction == b._SingleClickAction
                && a._HideFluently == b._HideFluently
                && a._PlaySoundOnHide == b._PlaySoundOnHide
                && Math.Abs(a._Opacity - b._Opacity) < double.Epsilon
                && a._ShowSeparateNotes == b._ShowSeparateNotes
                && a._PinClickAction == b._PinClickAction
                && a._StartPosition == b._StartPosition
                && a._HideMainWindow == b._HideMainWindow
                && a._Theme == b._Theme
                && a._PreventAutomaticResizing == b._PreventAutomaticResizing
                && a._ShowNotesPanel == b._ShowNotesPanel
                && a._NotesPanelDock == b._NotesPanelDock
                && a._PanelAutoHide == b._PanelAutoHide
                && a._PanelRemoveMode == b._PanelRemoveMode
                && a._PanelSwitchOffAnimation == b._PanelSwitchOffAnimation
                && a.PanelEnterDelay == b.PanelEnterDelay);
        }

        public static bool operator !=(PNBehavior a, PNBehavior b)
        {
            return (!(a == b));
        }
    }

    [Serializable]
    internal class PNProtection
    {
        private bool _StoreAsEncrypted;
        private bool _HideTrayIcon;
        private bool _BackupBeforeSaving;
        private bool _SilentFullBackup;
        private int _BackupDeepness = 3;
        private bool _DontShowContent;
        private bool _IncludeBinInSync;
        private string _PasswordString = "";
        private List<DayOfWeek> _FullBackupDays = new List<DayOfWeek>();
        private DateTime _FullBackupTime = DateTime.MinValue;
        private DateTime _FullBackupDate = DateTime.MinValue;
        private bool _PromptForPassword;

        internal bool PromptForPassword
        {
            get { return _PromptForPassword; }
            set { _PromptForPassword = value; }
        }
        internal DateTime FullBackupDate
        {
            get { return _FullBackupDate; }
            set { _FullBackupDate = value; }
        }
        internal DateTime FullBackupTime
        {
            get { return _FullBackupTime; }
            set { _FullBackupTime = value; }
        }
        internal List<DayOfWeek> FullBackupDays
        {
            get { return _FullBackupDays; }
            set { _FullBackupDays = value; }
        }
        internal string PasswordString
        {
            get { return _PasswordString; }
            set { _PasswordString = value; }
        }
        internal bool IncludeBinInSync
        {
            get { return _IncludeBinInSync; }
            set { _IncludeBinInSync = value; }
        }
        internal bool DontShowContent
        {
            get { return _DontShowContent; }
            set { _DontShowContent = value; }
        }
        internal int BackupDeepness
        {
            get { return _BackupDeepness; }
            set { _BackupDeepness = value; }
        }
        internal bool SilentFullBackup
        {
            get { return _SilentFullBackup; }
            set { _SilentFullBackup = value; }
        }
        internal bool BackupBeforeSaving
        {
            get { return _BackupBeforeSaving; }
            set { _BackupBeforeSaving = value; }
        }
        internal bool HideTrayIcon
        {
            get { return _HideTrayIcon; }
            set { _HideTrayIcon = value; }
        }
        internal bool StoreAsEncrypted
        {
            get { return _StoreAsEncrypted; }
            set { _StoreAsEncrypted = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            PNProtection bh = obj as PNProtection;
            if ((object)bh == null)
                return false;
            return (_BackupBeforeSaving == bh._BackupBeforeSaving
                && _BackupDeepness == bh._BackupDeepness
                && _DontShowContent == bh._DontShowContent
                && _HideTrayIcon == bh._HideTrayIcon
                && _IncludeBinInSync == bh._IncludeBinInSync
                && _SilentFullBackup == bh._SilentFullBackup
                && _StoreAsEncrypted == bh._StoreAsEncrypted
                && _PasswordString == bh._PasswordString
                && !_FullBackupDays.Inequals(bh._FullBackupDays)
                && _FullBackupTime == bh._FullBackupTime
                && _FullBackupDate == bh._FullBackupDate
                && _PromptForPassword == bh._PromptForPassword);
        }

        public bool Equals(PNProtection bh)
        {
            if ((object)bh == null)
                return false;
            return (_BackupBeforeSaving == bh._BackupBeforeSaving
                && _BackupDeepness == bh._BackupDeepness
                && _DontShowContent == bh._DontShowContent
                && _HideTrayIcon == bh._HideTrayIcon
                && _IncludeBinInSync == bh._IncludeBinInSync
                && _SilentFullBackup == bh._SilentFullBackup
                && _StoreAsEncrypted == bh._StoreAsEncrypted
                && _PasswordString == bh._PasswordString
                && !_FullBackupDays.Inequals(bh._FullBackupDays)
                && _FullBackupTime == bh._FullBackupTime
                && _FullBackupDate == bh._FullBackupDate
                && _PromptForPassword == bh._PromptForPassword);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += StoreAsEncrypted.GetHashCode();
            result *= 37;
            result += SilentFullBackup.GetHashCode();
            result *= 37;
            result += IncludeBinInSync.GetHashCode();
            result *= 37;
            result += HideTrayIcon.GetHashCode();
            result *= 37;
            result += DontShowContent.GetHashCode();
            result *= 37;
            result += BackupDeepness.GetHashCode();
            result *= 37;
            result += BackupBeforeSaving.GetHashCode();
            result *= 37;
            result += PasswordString.GetHashCode();
            result *= 37;
            result += FullBackupDate.GetHashCode();
            result *= 37;
            result += FullBackupDays.GetHashCode();
            result *= 37;
            result += FullBackupTime.GetHashCode();
            result *= 37;
            result += PromptForPassword.GetHashCode();
            return result;
        }

        public static bool operator ==(PNProtection a, PNProtection b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return (a._BackupBeforeSaving == b._BackupBeforeSaving
                && a._BackupDeepness == b._BackupDeepness
                && a._DontShowContent == b._DontShowContent
                && a._HideTrayIcon == b._HideTrayIcon
                && a._IncludeBinInSync == b._IncludeBinInSync
                && a._SilentFullBackup == b._SilentFullBackup
                && a._StoreAsEncrypted == b._StoreAsEncrypted
                && a._PasswordString == b._PasswordString
                && !a._FullBackupDays.Inequals(b._FullBackupDays)
                && a._FullBackupTime == b._FullBackupTime
                && a._FullBackupDate == b._FullBackupDate
                && a._PromptForPassword == b._PromptForPassword);
        }

        public static bool operator !=(PNProtection a, PNProtection b)
        {
            return (!(a == b));
        }
    }

    [Serializable]
    internal class PNConfig
    {
        private int _LastPage;
        private int _ExitFlag;
        private int _CPLastGroup = -1;
        private bool _Skinnable;
        private Color _CPPvwColor = Color.FromArgb(255, 242, 221, 116);
        private bool _CPUseCustPvwColor;
        private Size _CPSize = Size.Empty;
        private Point _CPLocation = default(Point);
        private string _ControlsStyle = "";
        private bool _CPPvwRight;
        private bool _CPPvwShow = true;
        private bool _CPGroupsShow = true;
        private SearchNotesPrefs _SearchNotesSettings = new SearchNotesPrefs();

        internal bool CPPvwRight
        {
            get { return _CPPvwRight; }
            set { _CPPvwRight = value; }
        }
        internal string ControlsStyle
        {
            get { return _ControlsStyle; }
            set { _ControlsStyle = value; }
        }
        internal Point CPLocation
        {
            get { return _CPLocation; }
            set { _CPLocation = value; }
        }
        internal Size CPSize
        {
            get { return _CPSize; }
            set { _CPSize = value; }
        }
        internal bool CPUseCustPvwColor
        {
            get { return _CPUseCustPvwColor; }
            set { _CPUseCustPvwColor = value; }
        }
        internal Color CPPvwColor
        {
            get { return _CPPvwColor; }
            set { _CPPvwColor = value; }
        }
        internal bool Skinnable
        {
            get { return _Skinnable; }
            set { _Skinnable = value; }
        }
        internal int CPLastGroup
        {
            get { return _CPLastGroup; }
            set { _CPLastGroup = value; }
        }
        internal int ExitFlag
        {
            get { return _ExitFlag; }
            set { _ExitFlag = value; }
        }

        internal int LastPage
        {
            get { return _LastPage; }
            set { _LastPage = value; }
        }

        internal bool CPPvwShow
        {
            get { return _CPPvwShow; }
            set { _CPPvwShow = value; }
        }

        internal bool CPGroupsShow
        {
            get { return _CPGroupsShow; }
            set { _CPGroupsShow = value; }
        }

        internal SearchNotesPrefs SearchNotesSettings
        {
            get { return _SearchNotesSettings; }
            set { _SearchNotesSettings = value; }
        }
    }

    [Serializable]
    internal class PNNetwork
    {
        internal const int PORT_EXCHANGE = 27951;
        private bool _IncludeBinInSync;
        private bool _SyncOnStart;
        private bool _SaveBeforeSync;
        private bool _EnableExchange;
        private bool _SaveBeforeSending;
        private bool _NoNotificationOnArrive;
        private bool _ShowReceivedOnClick;
        private bool _ShowIncomingOnClick;
        private bool _NoSoundOnArrive;
        private bool _NoNotificationOnSend;
        private bool _ShowAfterArrive;
        private bool _HideAfterSending;
        private bool _NoContactsInContextMenu;
        private int _ExchangePort = PORT_EXCHANGE;
        private int _PostCount = 20;
        private bool _AllowPing = true;
        private bool _ReceivedOnTop = true;

        internal int PostCount
        {
            get { return _PostCount; }
            set { _PostCount = value; }
        }
        internal int ExchangePort
        {
            get { return _ExchangePort; }
            set { _ExchangePort = value; }
        }
        internal bool NoContactsInContextMenu
        {
            get { return _NoContactsInContextMenu; }
            set { _NoContactsInContextMenu = value; }
        }
        internal bool HideAfterSending
        {
            get { return _HideAfterSending; }
            set { _HideAfterSending = value; }
        }
        internal bool ShowAfterArrive
        {
            get { return _ShowAfterArrive; }
            set { _ShowAfterArrive = value; }
        }
        internal bool NoNotificationOnSend
        {
            get { return _NoNotificationOnSend; }
            set { _NoNotificationOnSend = value; }
        }
        internal bool NoSoundOnArrive
        {
            get { return _NoSoundOnArrive; }
            set { _NoSoundOnArrive = value; }
        }
        internal bool ShowIncomingOnClick
        {
            get { return _ShowIncomingOnClick; }
            set { _ShowIncomingOnClick = value; }
        }
        internal bool ShowReceivedOnClick
        {
            get { return _ShowReceivedOnClick; }
            set { _ShowReceivedOnClick = value; }
        }
        internal bool NoNotificationOnArrive
        {
            get { return _NoNotificationOnArrive; }
            set { _NoNotificationOnArrive = value; }
        }
        internal bool SaveBeforeSending
        {
            get { return _SaveBeforeSending; }
            set { _SaveBeforeSending = value; }
        }
        internal bool EnableExchange
        {
            get { return _EnableExchange; }
            set { _EnableExchange = value; }
        }
        internal bool SaveBeforeSync
        {
            get { return _SaveBeforeSync; }
            set { _SaveBeforeSync = value; }
        }
        internal bool SyncOnStart
        {
            get { return _SyncOnStart; }
            set { _SyncOnStart = value; }
        }
        internal bool IncludeBinInSync
        {
            get { return _IncludeBinInSync; }
            set { _IncludeBinInSync = value; }
        }
        internal bool AllowPing
        {
            get { return _AllowPing; }
            set { _AllowPing = value; }
        }

        internal bool ReceivedOnTop
        {
            get { return _ReceivedOnTop; }
            set { _ReceivedOnTop = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            PNNetwork nt = obj as PNNetwork;
            if ((object)nt == null)
                return false;
            return (_EnableExchange == nt._EnableExchange
                && _ExchangePort == nt._ExchangePort
                && _HideAfterSending == nt._HideAfterSending
                && _IncludeBinInSync == nt._IncludeBinInSync
                && _NoContactsInContextMenu == nt._NoContactsInContextMenu
                && _NoNotificationOnArrive == nt._NoNotificationOnArrive
                && _NoNotificationOnSend == nt._NoNotificationOnSend
                && _NoSoundOnArrive == nt._NoSoundOnArrive
                && _SaveBeforeSending == nt._SaveBeforeSending
                && _SaveBeforeSync == nt._SaveBeforeSync
                && _ShowAfterArrive == nt._ShowAfterArrive
                && _ShowIncomingOnClick == nt._ShowIncomingOnClick
                && _ShowReceivedOnClick == nt._ShowReceivedOnClick
                && _SyncOnStart == nt._SyncOnStart
                && _PostCount == nt._PostCount
                && _AllowPing == nt._AllowPing
                && _ReceivedOnTop == nt._ReceivedOnTop);
        }

        public bool Equals(PNNetwork nt)
        {
            if ((object)nt == null)
                return false;
            return (_EnableExchange == nt._EnableExchange
                && _ExchangePort == nt._ExchangePort
                && _HideAfterSending == nt._HideAfterSending
                && _IncludeBinInSync == nt._IncludeBinInSync
                && _NoContactsInContextMenu == nt._NoContactsInContextMenu
                && _NoNotificationOnArrive == nt._NoNotificationOnArrive
                && _NoNotificationOnSend == nt._NoNotificationOnSend
                && _NoSoundOnArrive == nt._NoSoundOnArrive
                && _SaveBeforeSending == nt._SaveBeforeSending
                && _SaveBeforeSync == nt._SaveBeforeSync
                && _ShowAfterArrive == nt._ShowAfterArrive
                && _ShowIncomingOnClick == nt._ShowIncomingOnClick
                && _ShowReceivedOnClick == nt._ShowReceivedOnClick
                && _SyncOnStart == nt._SyncOnStart
                && _PostCount == nt._PostCount
                && _AllowPing == nt._AllowPing
                && _ReceivedOnTop == nt._ReceivedOnTop);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += SyncOnStart.GetHashCode();
            result *= 37;
            result += ShowReceivedOnClick.GetHashCode();
            result *= 37;
            result += ShowIncomingOnClick.GetHashCode();
            result *= 37;
            result += ShowAfterArrive.GetHashCode();
            result *= 37;
            result += SaveBeforeSync.GetHashCode();
            result *= 37;
            result += SaveBeforeSending.GetHashCode();
            result *= 37;
            result += NoSoundOnArrive.GetHashCode();
            result *= 37;
            result += NoNotificationOnSend.GetHashCode();
            result *= 37;
            result += NoNotificationOnArrive.GetHashCode();
            result *= 37;
            result += NoContactsInContextMenu.GetHashCode();
            result *= 37;
            result += IncludeBinInSync.GetHashCode();
            result *= 37;
            result += HideAfterSending.GetHashCode();
            result *= 37;
            result += ExchangePort.GetHashCode();
            result *= 37;
            result += EnableExchange.GetHashCode();
            result *= 37;
            result += PostCount.GetHashCode();
            result *= 37;
            result += AllowPing.GetHashCode();
            result *= 37;
            result += ReceivedOnTop.GetHashCode();
            return result;
        }

        public static bool operator ==(PNNetwork a, PNNetwork b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return (a._EnableExchange == b._EnableExchange
                && a._ExchangePort == b._ExchangePort
                && a._HideAfterSending == b._HideAfterSending
                && a._IncludeBinInSync == b._IncludeBinInSync
                && a._NoContactsInContextMenu == b._NoContactsInContextMenu
                && a._NoNotificationOnArrive == b._NoNotificationOnArrive
                && a._NoNotificationOnSend == b._NoNotificationOnSend
                && a._NoSoundOnArrive == b._NoSoundOnArrive
                && a._SaveBeforeSending == b._SaveBeforeSending
                && a._SaveBeforeSync == b._SaveBeforeSync
                && a._ShowAfterArrive == b._ShowAfterArrive
                && a._ShowIncomingOnClick == b._ShowIncomingOnClick
                && a._ShowReceivedOnClick == b._ShowReceivedOnClick
                && a._SyncOnStart == b._SyncOnStart
                && a._PostCount == b._PostCount
                && a._AllowPing == b._AllowPing
                && a._ReceivedOnTop == b._ReceivedOnTop);
        }

        public static bool operator !=(PNNetwork a, PNNetwork b)
        {
            return (!(a == b));
        }
    }
}
