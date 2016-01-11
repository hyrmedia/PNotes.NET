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

namespace PNotes.NET
{
    internal enum DockStatus
    {
        None,
        Left,
        Top,
        Right,
        Bottom
    }

    [Flags]
    internal enum SendReceiveStatus
    {
        None,
        Sent,
        Received,
        Both
    }

    internal enum AddEditMode
    {
        Add,
        Edit
    }

    internal enum NoteStartPosition
    {
        Center,
        Left,
        Top,
        Right,
        Bottom
    }

    internal enum PluginType
    {
        Social,
        Sync
    }

    internal enum PasswordDlgMode
    {
        DeleteMain,
        LoginMain,
        DeleteGroup,
        LoginGroup,
        DeleteNote,
        LoginNote
    }

    // Summary:
    //     Specifies the border drag direction
    internal enum PNBorderDragDirection
    {
        // Summary:
        //     No direction
        None = 0,
        //
        // Summary:
        //     Direction left
        Left = 1,
        //
        // Summary:
        //     Direction up
        Up = 2,
        //
        // Summary:
        //     Direction right
        Right = 3,
        //
        // Summary:
        //     Direction down
        Down = 4,
        //
        // Summary:
        //     Direction east-north
        EastNorth = 5,
        EastSouth=6,
        //
        // Summary:
        //     Direction west-south
        WestNorth = 7,
        //
        // Summary:
        //     Direction east-south
        WestSouth = 8,
    }

    public enum BaloonMode
    {
        FirstRun,
        NewVersion,
        NoteSent,
        NoteReceived,
        Error,
        CriticalUpdates,
        Information
    }

    internal enum NoteBooleanTypes
    {
        Visible,
        Favorite,
        //Schedule,
        Change,
        Protection,
        Complete,
        Priority,
        Password,
        Pin,
        Topmost,
        Roll,
        FromDB,
        Scrambled
    }

    internal enum NoteDeleteType
    {
        None,
        Bin,
        Complete
    }

    internal enum NoteDateType
    {
        Creation,
        Saving,
        Sending,
        Receiving,
        Deletion
    }

    internal enum GroupChangeType
    {
        ParentID,
        Image,
        Name,
        Password,
        Skin,
        BackColor,
        CaptionFont,
        CaptionColor,
        Font,
        FontColor
    }

    internal enum SearchReplace
    {
        Search,
        Replace
    }

    internal enum SearchMode
    {
        Normal,
        RegularExp
    }

    internal enum SpecialGroups
    {
        AllGroups = -1,
        RecycleBin = -2,
        Diary = -3,
        SearchResults = -4,
        Backup = -5,
        Favorites = -6,
        Incoming = -7,
        Docking = -8,
        DummyGroup = -777
    }

    internal enum NewNoteMode
    {
        None,
        Identificator,
        File,
        Clipboard,
        Duplication,
        Diary
    }

    internal enum CPCols
    {
        Name,
        ID,
        Priority,
        Completed,
        Protected,
        Password,
        Pin,
        Favorites,
        SendReceive,
        Group,
        Created,
        Saved,
        Schedule,
        Deleted,
        Tags,
        Content,
        SentTo,
        SentAt,
        ReceiveFrom,
        ReceivedAt,
        Original,
        TimeSaved
    }

    internal enum MainDialogAction
    {
        NewNote,
        NewNoteInGroup,
        NoteFromClipboard,
        NoteFromClipboardInGroup,
        LoadNotes,
        LoadNotesInGroup,
        SearchInNotes,
        SearchByTags,
        SearchByDates,
        ShowAll,
        HideAll,
        BringToFront,
        DockAllNone,
        DockAllLeft,
        DockAllTop,
        DockAllRight,
        DockAllBottom,
        FullBackupCreate,
        FullBackupRestore,
        LocalSync,
        SaveAll,
        Preferences,
        Help,
        Support,
        About,
        ImportNotes,
        ImportSettings,
        ReloadAll,
        Restart,
        ImportFonts,
        ImportDictionaries
    }

    internal enum MAPIErrorCode
    {
        MAPI_SUCCESS = 0,
        MAPI_USER_ABORT = 1,
        MAPI_E_FAILURE = 2,
        MAPI_E_LOGIN_FAILURE = 3,
        MAPI_E_DISK_FULL = 4,
        MAPI_E_INSUFFICIENT_MEMORY = 5,
        MAPI_E_BLK_TOO_SMALL = 6,
        MAPI_E_TOO_MANY_SESSIONS = 8,
        MAPI_E_TOO_MANY_FILES = 9,
        MAPI_E_TOO_MANY_RECIPIENTS = 10,
        MAPI_E_ATTACHMENT_NOT_FOUND = 11,
        MAPI_E_ATTACHMENT_OPEN_FAILURE = 12,
        MAPI_E_ATTACHMENT_WRITE_FAILURE = 13,
        MAPI_E_UNKNOWN_RECIPIENT = 14,
        MAPI_E_BAD_RECIPTYPE = 15,
        MAPI_E_NO_MESSAGES = 16,
        MAPI_E_INVALID_MESSAGE = 17,
        MAPI_E_TEXT_TOO_LARGE = 18,
        MAPI_E_INVALID_SESSION = 19,
        MAPI_E_TYPE_NOT_SUPPORTED = 20,
        MAPI_E_AMBIGUOUS_RECIPIENT = 21,
        MAPI_E_MESSAGE_IN_USE = 22,
        MAPI_E_NETWORK_FAILURE = 23,
        MAPI_E_INVALID_EDITFIELDS = 24,
        MAPI_E_INVALID_RECIPS = 25,
        MAPI_E_NOT_SUPPORTED = 26,
        MAPI_E_NO_LIBRARY = 999,
        MAPI_E_INVALID_PARAMETER = 998,
        MAPI_E_NO_MAIL_CLIENT = -2147467259
    }

    public enum DockArrow
    {
        LeftUp,
        LeftDown,
        TopLeft,
        TopRight,
        RightUp,
        RightDown,
        BottomLeft,
        BottomRight
    }

    internal enum ScheduleType
    {
        None,
        Once,
        EveryDay,
        RepeatEvery,
        Weekly,
        After,
        MonthlyExact,
        MonthlyDayOfWeek,
        MultipleAlerts
    }

    internal enum ScheduleStart
    {
        ExactTime,
        ProgramStart
    }

    internal enum DatePart
    {
        Year,
        Month,
        Day,
        Hour,
        Minute,
        Second,
        Millisecond
    }

    internal enum DayOrdinal
    {
        First = 1,
        Second = 2,
        Third = 3,
        Fourth = 4,
        Last = 5
    }

    internal enum SyncStatus
    {
        None,
        CopyTo
    }

    internal enum LocalSyncResult
    {
        None,
        Reload,
        AbortVersion,
        Error
    }

    internal enum OfficeApp
    {
        Outlook,
        Word,
        Excel,
        OneNote
    }

    internal enum ShowHideResult
    {
        NotSuccess,
        Success
    }

    internal enum PinClickAction
    {
        Toggle,
        ShowWindow
    }

    internal enum TrayMouseAction
    {
        None,
        NewNote,
        ControlPanel,
        Preferences,
        SearchInNotes,
        NewNoteInGroup,
        LoadNote,
        NoteFromClipboard,
        BringAllToFront,
        SaveAll,
        AllShowHide,
        SearchByTags,
        SearchByDates
    }

    internal enum DefaultNaming
    {
        FirstCharacters,
        DateTime,
        DateTimeAndFirstCharacters
    }

    internal enum MenuType
    {
        Main,
        Note,
        Edit,
        ControlPanel
    }

    internal enum ImportContacts
    {
        None,
        Outlook,
        Gmail,
        Lotus
    }

    internal enum ScrambleMode
    {
        Scramble,
        Unscramble
    }

    internal enum CompleteDeletionSource
    {
        SingleNote,
        EmptyBin,
        AutomaticCleanBin
    }

    [Flags]
    internal enum CriticalUpdateAction
    {
        None = 0,
        Program = 1,
        Plugins = 2
    }

    internal enum ExchangeLists
    {
        Tags,
        LinkedNotes
    }

    internal enum DeleteContactsGroupBehavior
    {
        Move,
        DeleteAll
    }

    internal enum PasswordProtectionMode
    {
        None,
        Note,
        Group
    }

    internal enum NoteState
    {
        OrdinalVisible,
        OrdinalVisibleChanged,
        OrdinalVisibleScheduled,
        OrdinalVisibleChangedScheduled,
        OrdinalHidden,
        OrdinalHiddenChanged,
        OrdinalHiddenScheduled,
        OrdinalHiddenChangedScheduled,
        New,
        NewChanged,
        NewScheduled,
        NewChangedScheduled,
        Deleted,
        Backup
    }

    public enum SmallBarButtonType
    {
        Add,
        Edit,
        Remove,
        Apply,
        Run,
        Clean,
        User
    }

    public enum NotesPanelOrientation
    {
        Left,
        Top
    }

    public enum PanelRemoveMode
    {
        SingleClick,
        DoubleClick
    }

    public enum CopyDataType
    {
        NewNote,
        LoadNotes,
        ShowNoteById
    }

    public enum ContactConnection
    {
        Disconnected,
        Connected
    }

    public enum SearchCriteria
    {
        None = -1,
        EntireString,
        EveryWord,
        AtLeastOneWord
    }

    public enum SearchScope
    {
        None = -1,
        Text,
        Titles,
        TextAndTitles
    }
}
