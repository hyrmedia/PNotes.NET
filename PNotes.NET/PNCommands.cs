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
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace PNotes.NET
{
    internal enum CommandType
    {
        None,
        NewNote,
        LoadNote,
        NoteFromClipboard,
        DuplicateNote,
        SaveAsShortcut,
        Diary,
        Today,
        Save,
        SaveAs,
        SaveAsText,
        RestoreFromBackup,
        Print,
        Adjust,
        AdjustAppearance,
        AdjustSchedule,
        Delete,
        SaveAll,
        BackupSync,
        BackupCreate,
        BackupRestore,
        SyncLocal,
        ImportNotes,
        ImportSettings,
        ImportFonts,
        ImportDictionaries,
        Placement,
        DockAll,
        DockAllNone,
        DockAllLeft,
        DockAllTop,
        DockAllRight,
        DockAllBottom,
        Visibility,
        ShowNote,
        ShowAll,
        HideNote,
        HideAll,
        AllToFront,
        Centralize,
        SendAsText,
        SendAsAttachment,
        SendAsZip,
        SendNetwork,
        ContactAdd,
        ContactGroupAdd,
        ContactSelect,
        ContactGroupSelect,
        Tags,
        TagsCurrent,
        TagsShowBy,
        TagsHideBy,
        Switches,
        OnTop,
        HighPriority,
        ProtectionMode,
        SetNotePassword,
        RemoveNotePassword,
        MarkAsComplete,
        RollUnroll,
        Pin,
        Unpin,
        Scramble,
        Unscramble,
        AddFavorites,
        RemoveFavorites,
        Run,
        EmptyBin,
        RestoreNote,
        Preview,
        PreviewFromMenu,
        PreviewSettings,
        UseCustColor,
        ChooseCustColor,
        PreviewRight,
        PreviewRightFromMenu,
        ShowGroups,
        ShowGroupsFromMenu,
        ColReset,
        HotkeysCP,
        MenusManagementCP,
        Preferences,
        Password,
        Search,
        QuickSearch,
        Help,
        Support,
        About,
        PwrdCreate,
        PwrdChange,
        PwrdRemove,
        SearchInNotes,
        SearchByTags,
        SearchByDates,
        IncBinInQSearch,
        ClearQSearch,
        GroupAdd,
        GroupAddSubgroup,
        GroupEdit,
        GroupRemove,
        GroupShow,
        GroupShowAll,
        GroupHide,
        GroupHideAll,
        GroupPassAdd,
        GroupPassRemove
    }

    public class PNCommands
    {
        static PNCommands()
        {
            _NewNote = new PNRoutedUICommand("New Note", "cmdNewNote", typeof(PNCommands)) { Type = CommandType.NewNote };
            _LoadNote = new PNRoutedUICommand("Load Note", "cmdLoadNotes", typeof(PNCommands))
            {
                Type = CommandType.LoadNote
            };
            _NoteFromClipboard = new PNRoutedUICommand("New Note From Clipboard", "cmdNoteFromCB", typeof(PNCommands))
            {
                Type = CommandType.NoteFromClipboard
            };
            _DuplicateNote = new PNRoutedUICommand("Duplicate Note", "cmdDuplicate", typeof(PNCommands))
            {
                Type = CommandType.DuplicateNote
            };
            _SaveAsShortcut = new PNRoutedUICommand("Save As Desktop Shortcut", "mnuSaveAsShortcut", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.SaveAsShortcut
            };
            _Diary = new PNRoutedUICommand("Diary", "cmdDiary", typeof(PNCommands)) { Type = CommandType.Diary };
            _Today = new PNRoutedUICommand("Today", "mnuTodayDiary", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.Today
            };
            _Save = new PNRoutedUICommand("Save", "cmdSave", typeof(PNCommands)) { Type = CommandType.Save };
            _SaveAs = new PNRoutedUICommand("Save As", "cmdSaveAs", typeof(PNCommands)) { Type = CommandType.SaveAs };
            _SaveAsText = new PNRoutedUICommand("Save As Text File", "cmdSaveAsText", typeof(PNCommands))
            {
                Type = CommandType.SaveAsText
            };
            _RestoreFromBackup = new PNRoutedUICommand("Restore From Backup", "cmdRestoreFromBackup",
                typeof(PNCommands)) { Type = CommandType.RestoreFromBackup };
            _Print = new PNRoutedUICommand("Print", "cmdPrint", typeof(PNCommands)) { Type = CommandType.Print };
            _Adjust = new PNRoutedUICommand("Adjust", "cmdAdjust", typeof(PNCommands)) { Type = CommandType.Adjust };
            _AdjustAppearance = new PNRoutedUICommand("Adjust Appearance", "mnuAdjustAppearance", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.AdjustAppearance
            };
            _AdjustSchedule = new PNRoutedUICommand("Adjust Schedule", "mnuAdjustSchedule", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.AdjustSchedule
            };
            _Delete = new PNRoutedUICommand("Delete Note", "cmdDelete", typeof(PNCommands)) { Type = CommandType.Delete };
            _SaveAll = new PNRoutedUICommand("Save All", "cmdSaveAll", typeof(PNCommands)) { Type = CommandType.SaveAll };
            _BackupSync = new PNRoutedUICommand("Backup/Synchronization", "cmdBackup", typeof(PNCommands))
            {
                Type = CommandType.BackupSync
            };
            _BackupCreate = new PNRoutedUICommand("Create Full Backup", "mnuBackupCreate", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.BackupCreate
            };
            _BackupRestore = new PNRoutedUICommand("Restore From Full Backup", "mnuBackupRestore", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.BackupRestore
            };
            _SyncLocal = new PNRoutedUICommand("Manual Local Synchronization", "mnuSyncLocal", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.SyncLocal
            };
            _ImportNotes = new PNRoutedUICommand("Import Notes From PNotes", "mnuImportNotes", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.ImportNotes
            };
            _ImportSettings = new PNRoutedUICommand("Import Settings From PNotes", "mnuImportSettings", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.ImportSettings
            };
            _ImportFonts = new PNRoutedUICommand("Import Fonts From PNotes", "mnuImportFonts", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.ImportFonts
            };
            _ImportDictionaries = new PNRoutedUICommand("Import Dictionaries From PNotes", "mnuImportDictionaries", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.ImportDictionaries
            };
            _Placement = new PNRoutedUICommand("Placement/Visibility", "cmdPlacement", typeof(PNCommands))
            {
                Type = CommandType.Placement
            };
            _DockAll = new PNRoutedUICommand("Docking (All Notes)", "mnuDockAll", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.DockAll
            };
            _DockAllNone = new PNRoutedUICommand("None", "mnuDAllNone", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.DockAllNone
            };
            _DockAllLeft = new PNRoutedUICommand("Left", "mnuDAllLeft", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.DockAllLeft
            };
            _DockAllTop = new PNRoutedUICommand("Top", "mnuDAllTop", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.DockAllTop
            };
            _DockAllRight = new PNRoutedUICommand("Right", "mnuDAllRight", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.DockAllRight
            };
            _DockAllBottom = new PNRoutedUICommand("Bottom", "mnuDAllBottom", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.DockAllBottom
            };
            _Visibility = new PNRoutedUICommand("Visibility", "mnuVisibility", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.Visibility
            };
            _ShowNote = new PNRoutedUICommand("Show", "mnuShowNote", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.ShowNote
            };
            _ShowAll = new PNRoutedUICommand("Show All", "mnuShowAll", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.ShowAll
            };
            _HideNote = new PNRoutedUICommand("Hide", "mnuHideNote", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.HideNote
            };
            _HideAll = new PNRoutedUICommand("Hide All", "mnuHideAll", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.HideAll
            };
            _AllToFront = new PNRoutedUICommand("Bring All To Front", "mnuAllToFront", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.AllToFront
            };
            _Centralize = new PNRoutedUICommand("Centralize", "mnuCentralize", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.Centralize
            };
            _SendAsText = new PNRoutedUICommand("Send As Text", "cmdSendAsText", typeof(PNCommands))
            {
                Type = CommandType.SendAsText
            };
            _SendAsAttachment = new PNRoutedUICommand("Send As Attachment", "cmdSendAsAttachment", typeof(PNCommands))
            {
                Type = CommandType.SendAsAttachment
            };
            _SendAsZip = new PNRoutedUICommand("Send In ZIP Archive", "cmdSendZip", typeof(PNCommands))
            {
                Type = CommandType.SendAsZip
            };
            _SendNetwork = new PNRoutedUICommand("Send Via Network", "cmdSendNetwork", typeof(PNCommands))
            {
                Type = CommandType.SendNetwork
            };
            _ContactAdd = new PNRoutedUICommand("Add Contact", "mnuAddContact", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.ContactAdd
            };
            _ContactGroupAdd = new PNRoutedUICommand("Add Group Of Contacts", "mnuAddGroup", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.ContactGroupAdd
            };
            _ContactSelect = new PNRoutedUICommand("Select Contacts", "mnuSelectContact", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.ContactSelect
            };
            _ContactGroupSelect = new PNRoutedUICommand("Select Groups Of Contacts", "mnuSelectGroup", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.ContactGroupSelect
            };
            _Tags = new PNRoutedUICommand("Tags", "cmdTags", typeof(PNCommands))
            {
                Type = CommandType.Tags
            };
            _TagsCurrent = new PNRoutedUICommand("Tags (current note)", "mnuTagsCurrent", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.TagsCurrent
            };
            _TagsShowBy = new PNRoutedUICommand("Show By Tag", "mnuShowByTag", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.TagsShowBy
            };
            _TagsHideBy = new PNRoutedUICommand("Hide By Tag", "mnuHideByTag", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.TagsHideBy
            };
            _Switches = new PNRoutedUICommand("Switches", "cmdSwitches", typeof(PNCommands))
            {
                Type = CommandType.Switches
            };
            _OnTop = new PNRoutedUICommand("On Top", "mnuOnTop", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.OnTop
            };
            _HighPriority = new PNRoutedUICommand("Toggle High Priority", "mnuToggleHighPriority", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.HighPriority
            };
            _ProtectionMode = new PNRoutedUICommand("Toggle Protection Mode", "mnuToggleProtectionMode", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.ProtectionMode
            };
            _SetNotePassword = new PNRoutedUICommand("Set Note Password", "mnuSetPassword", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.SetNotePassword
            };
            _RemoveNotePassword = new PNRoutedUICommand("Remove Note Password", "mnuRemovePassword", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.RemoveNotePassword
            };
            _MarkAsComplete = new PNRoutedUICommand("Mark As Complete", "mnuMarkAsComplete", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.MarkAsComplete
            };
            _RollUnroll = new PNRoutedUICommand("Roll/Unroll", "mnuRollUnroll", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.RollUnroll
            };
            _Pin = new PNRoutedUICommand("Pin To Window", "mnuPin", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.Pin
            };
            _Unpin = new PNRoutedUICommand("Unpin", "mnuUnpin", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.Unpin
            };
            _Scramble = new PNRoutedUICommand("Encrypt text", "mnuScramble", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.Scramble
            };
            _Unscramble = new PNRoutedUICommand("Decrypt text", "mnuUnscramble", typeof(PNCommands))
            {
                Section = "note_menu",
                Type = CommandType.Unscramble
            };
            _AddFavorites = new PNRoutedUICommand("Add To Favorites", "cmdFavorites", typeof(PNCommands))
            {
                Type = CommandType.AddFavorites
            };
            _RemoveFavorites = new PNRoutedUICommand("Remove From Favorites", "cmdRemoveFromFavorites", typeof(PNCommands))
            {
                Type = CommandType.RemoveFavorites
            };
            _Run = new PNRoutedUICommand("Run", "cmdRun", typeof(PNCommands))
            {
                Type = CommandType.Run
            };
            _EmptyBin = new PNRoutedUICommand("Empty Recycle Bin", "cmdEmptyBin", typeof(PNCommands))
            {
                Type = CommandType.EmptyBin
            };
            _RestoreNote = new PNRoutedUICommand("Restore Note", "cmdRestoreNote", typeof(PNCommands))
            {
                Type = CommandType.RestoreNote
            };
            _Preview = new PNRoutedUICommand("Preview", "cmdPreview", typeof(PNCommands))
            {
                Type = CommandType.Preview
            };
            _PreviewFromMenu = new PNRoutedUICommand("Preview", "cmdPreview", typeof(PNCommands))
            {
                Type = CommandType.PreviewFromMenu
            };
            _PreviewSettings = new PNRoutedUICommand("Preview Window Background Settings", "cmdPreviewSettings", typeof(PNCommands))
            {
                Type = CommandType.PreviewSettings
            };
            _UseCustColor = new PNRoutedUICommand("Use Custom Color", "mnuUseCustColor", typeof(PNCommands))
            {
                Section = "cp_menu",
                Type = CommandType.UseCustColor
            };
            _ChooseCustColor = new PNRoutedUICommand("Choose Custom Color", "mnuChooseCustColor", typeof(PNCommands))
            {
                Section = "cp_menu",
                Type = CommandType.ChooseCustColor
            };
            _PreviewRight = new PNRoutedUICommand("Preview Window On The Right", "cmdPreviewRight", typeof(PNCommands))
            {
                Type = CommandType.PreviewRight
            };
            _PreviewRightFromMenu = new PNRoutedUICommand("Preview Window On The Right", "cmdPreviewRight", typeof(PNCommands))
            {
                Type = CommandType.PreviewRightFromMenu
            };
            _ShowGroups = new PNRoutedUICommand("Show Groups", "cmdShowGroups", typeof(PNCommands))
            {
                Type = CommandType.ShowGroups
            };
            _ShowGroupsFromMenu = new PNRoutedUICommand("Show Groups", "cmdShowGroups", typeof(PNCommands))
            {
                Type = CommandType.ShowGroupsFromMenu
            };
            _ColReset = new PNRoutedUICommand("Reset Columns Width/Visibility", "cmdColReset", typeof(PNCommands))
            {
                Type = CommandType.ColReset
            };
            _HotkeysCP = new PNRoutedUICommand("Hot Keys Management", "cmdHotkeysCP", typeof(PNCommands))
            {
                Type = CommandType.HotkeysCP
            };
            _MenusManagementCP = new PNRoutedUICommand("Menus Management", "cmdMenusManagementCP", typeof(PNCommands))
            {
                Type = CommandType.MenusManagementCP
            };
            _Preferences = new PNRoutedUICommand("Preferences", "cmdPreferences", typeof(PNCommands))
            {
                Type = CommandType.Preferences
            };
            _Password = new PNRoutedUICommand("Password", "cmdPassword", typeof(PNCommands))
            {
                Type = CommandType.Password
            };
            _Search = new PNRoutedUICommand("Search", "cmdSearch", typeof(PNCommands))
            {
                Type = CommandType.Search
            };
            _QuickSearch = new PNRoutedUICommand("Quick Search", "cmdQuickSearch", typeof(PNCommands))
            {
                Type = CommandType.QuickSearch
            };
            _Help = new PNRoutedUICommand("Help", "cmdHelp", typeof(PNCommands))
            {
                Type = CommandType.Help
            };
            _Support = new PNRoutedUICommand("Support PNotes.NET Project", "cmdSupport", typeof(PNCommands))
            {
                Type = CommandType.Support
            };
            _About = new PNRoutedUICommand("About", "mnuAbout", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.About
            };
            _PwrdCreate = new PNRoutedUICommand("Create Password", "mnuPwrdCreate", typeof(PNCommands))
            {
                Section = "cp_menu",
                Type = CommandType.PwrdCreate
            };
            _PwrdChange = new PNRoutedUICommand("Change Password", "mnuPwrdChange", typeof(PNCommands))
            {
                Section = "cp_menu",
                Type = CommandType.PwrdChange
            };
            _PwrdRemove = new PNRoutedUICommand("Remove Password", "mnuPwrdRemove", typeof(PNCommands))
            {
                Section = "cp_menu",
                Type = CommandType.PwrdRemove
            };
            _SearchInNotes = new PNRoutedUICommand("Search In Notes", "mnuSearchInNotes", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.SearchInNotes
            };
            _SearchByTags = new PNRoutedUICommand("Search By Tags", "mnuSearchByTags", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.SearchByTags
            };
            _SearchByDates = new PNRoutedUICommand("Search By Dates", "mnuSearchByDates", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.SearchByDates
            };
            _IncBinInQSearch = new PNRoutedUICommand("Include Notes From Recycle Bin in 'Quick Search'", "mnuIncBinInQSearch", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.IncBinInQSearch
            };
            _ClearQSearch = new PNRoutedUICommand("Clear 'Quick Search'", "mnuClearQSearch", typeof(PNCommands))
            {
                Section = "main_menu",
                Type = CommandType.ClearQSearch
            };
            _GroupAdd = new PNRoutedUICommand("Add top level group", "cmdAddTopGroup", typeof(PNCommands))
            {
                Type = CommandType.GroupAdd
            };
            _GroupAddSubgroup = new PNRoutedUICommand("Add subgroup to selected group", "cmdAddSubgroup",
                typeof(PNCommands)) { Type = CommandType.GroupAddSubgroup };
            _GroupEdit = new PNRoutedUICommand("Edit group", "cmdEditGroup", typeof(PNCommands))
            {
                Type = CommandType.GroupEdit
            };
            _GroupRemove = new PNRoutedUICommand("Delete group", "cmdRemoveGroup", typeof(PNCommands))
            {
                Type = CommandType.GroupRemove
            };
            _GroupShow = new PNRoutedUICommand("Show all notes from selected group", "cmdShowAllFromGroup",
                typeof(PNCommands)) { Type = CommandType.GroupShow };
            _GroupShowAll = new PNRoutedUICommand("Show group (include subgroups)", "cmdShowAllIncSubgroups",
                typeof(PNCommands)) { Type = CommandType.GroupShowAll };
            _GroupHide = new PNRoutedUICommand("Hide all notes from selected group", "cmdHideAllFromGroup",
                typeof(PNCommands)) { Type = CommandType.GroupHide };
            _GroupHideAll = new PNRoutedUICommand("Hide group (include subgroups)", "cmdHideAllIncSubgroups",
                typeof(PNCommands)) { Type = CommandType.GroupHideAll };
            _GroupPassAdd = new PNRoutedUICommand("Set group password", "cmdSetGroupPassword", typeof(PNCommands))
            {
                Type = CommandType.GroupPassAdd
            };
            _GroupPassRemove = new PNRoutedUICommand("Remove group password", "cmdRemoveGroupPassword",
                typeof(PNCommands)) { Type = CommandType.GroupPassRemove };
        }

        private readonly static PNRoutedUICommand _NewNote;
        private readonly static PNRoutedUICommand _LoadNote;
        private readonly static PNRoutedUICommand _NoteFromClipboard;
        private static readonly PNRoutedUICommand _DuplicateNote;
        private static readonly PNRoutedUICommand _SaveAsShortcut;
        private static readonly PNRoutedUICommand _Diary;
        private static readonly PNRoutedUICommand _Today;
        private static readonly PNRoutedUICommand _Save;
        private static readonly PNRoutedUICommand _SaveAs;
        private static readonly PNRoutedUICommand _SaveAsText;
        private static readonly PNRoutedUICommand _RestoreFromBackup;
        private static readonly PNRoutedUICommand _Print;
        private static readonly PNRoutedUICommand _Adjust;
        private static readonly PNRoutedUICommand _AdjustAppearance;
        private static readonly PNRoutedUICommand _AdjustSchedule;
        private static readonly PNRoutedUICommand _Delete;
        private static readonly PNRoutedUICommand _SaveAll;
        private static readonly PNRoutedUICommand _BackupSync;
        private static readonly PNRoutedUICommand _BackupCreate;
        private static readonly PNRoutedUICommand _BackupRestore;
        private static readonly PNRoutedUICommand _SyncLocal;
        private static readonly PNRoutedUICommand _ImportNotes;
        private static readonly PNRoutedUICommand _ImportSettings;
        private static readonly PNRoutedUICommand _ImportFonts;
        private static readonly PNRoutedUICommand _ImportDictionaries;
        private static readonly PNRoutedUICommand _Placement;
        private static readonly PNRoutedUICommand _DockAll;
        private static readonly PNRoutedUICommand _DockAllNone;
        private static readonly PNRoutedUICommand _DockAllLeft;
        private static readonly PNRoutedUICommand _DockAllTop;
        private static readonly PNRoutedUICommand _DockAllRight;
        private static readonly PNRoutedUICommand _DockAllBottom;
        private static readonly PNRoutedUICommand _Visibility;
        private static readonly PNRoutedUICommand _ShowNote;
        private static readonly PNRoutedUICommand _ShowAll;
        private static readonly PNRoutedUICommand _HideNote;
        private static readonly PNRoutedUICommand _HideAll;
        private static readonly PNRoutedUICommand _AllToFront;
        private static readonly PNRoutedUICommand _Centralize;
        private static readonly PNRoutedUICommand _SendAsText;
        private static readonly PNRoutedUICommand _SendAsAttachment;
        private static readonly PNRoutedUICommand _SendAsZip;
        private static readonly PNRoutedUICommand _SendNetwork;
        private static readonly PNRoutedUICommand _ContactAdd;
        private static readonly PNRoutedUICommand _ContactGroupAdd;
        private static readonly PNRoutedUICommand _ContactSelect;
        private static readonly PNRoutedUICommand _ContactGroupSelect;
        private static readonly PNRoutedUICommand _Tags;
        private static readonly PNRoutedUICommand _TagsCurrent;
        private static readonly PNRoutedUICommand _TagsShowBy;
        private static readonly PNRoutedUICommand _TagsHideBy;
        private static readonly PNRoutedUICommand _Switches;
        private static readonly PNRoutedUICommand _OnTop;
        private static readonly PNRoutedUICommand _HighPriority;
        private static readonly PNRoutedUICommand _ProtectionMode;
        private static readonly PNRoutedUICommand _SetNotePassword;
        private static readonly PNRoutedUICommand _RemoveNotePassword;
        private static readonly PNRoutedUICommand _MarkAsComplete;
        private static readonly PNRoutedUICommand _RollUnroll;
        private static readonly PNRoutedUICommand _Pin;
        private static readonly PNRoutedUICommand _Unpin;
        private static readonly PNRoutedUICommand _Scramble;
        private static readonly PNRoutedUICommand _Unscramble;
        private static readonly PNRoutedUICommand _AddFavorites;
        private static readonly PNRoutedUICommand _RemoveFavorites;
        private static readonly PNRoutedUICommand _Run;
        private static readonly PNRoutedUICommand _EmptyBin;
        private static readonly PNRoutedUICommand _RestoreNote;
        private static readonly PNRoutedUICommand _Preview;
        private static readonly PNRoutedUICommand _PreviewFromMenu;
        private static readonly PNRoutedUICommand _PreviewSettings;
        private static readonly PNRoutedUICommand _UseCustColor;
        private static readonly PNRoutedUICommand _ChooseCustColor;
        private static readonly PNRoutedUICommand _PreviewRight;
        private static readonly PNRoutedUICommand _PreviewRightFromMenu;
        private static readonly PNRoutedUICommand _ShowGroups;
        private static readonly PNRoutedUICommand _ShowGroupsFromMenu;
        private static readonly PNRoutedUICommand _ColReset;
        private static readonly PNRoutedUICommand _HotkeysCP;
        private static readonly PNRoutedUICommand _MenusManagementCP;
        private static readonly PNRoutedUICommand _Preferences;
        private static readonly PNRoutedUICommand _Password;
        private static readonly PNRoutedUICommand _Search;
        private static readonly PNRoutedUICommand _QuickSearch;
        private static readonly PNRoutedUICommand _Help;
        private static readonly PNRoutedUICommand _Support;
        private static readonly PNRoutedUICommand _About;
        private static readonly PNRoutedUICommand _PwrdCreate;
        private static readonly PNRoutedUICommand _PwrdChange;
        private static readonly PNRoutedUICommand _PwrdRemove;
        private static readonly PNRoutedUICommand _SearchInNotes;
        private static readonly PNRoutedUICommand _SearchByTags;
        private static readonly PNRoutedUICommand _SearchByDates;
        private static readonly PNRoutedUICommand _IncBinInQSearch;
        private static readonly PNRoutedUICommand _ClearQSearch;
        private static readonly PNRoutedUICommand _GroupAdd;
        private static readonly PNRoutedUICommand _GroupAddSubgroup;
        private static readonly PNRoutedUICommand _GroupEdit;
        private static readonly PNRoutedUICommand _GroupRemove;
        private static readonly PNRoutedUICommand _GroupShow;
        private static readonly PNRoutedUICommand _GroupShowAll;
        private static readonly PNRoutedUICommand _GroupHide;
        private static readonly PNRoutedUICommand _GroupHideAll;
        private static readonly PNRoutedUICommand _GroupPassAdd;
        private static readonly PNRoutedUICommand _GroupPassRemove;

        public static PNRoutedUICommand NewNote
        {
            get { return _NewNote; }
        }

        public static PNRoutedUICommand LoadNote
        {
            get { return _LoadNote; }
        }

        public static PNRoutedUICommand NoteFromClipboard
        {
            get { return _NoteFromClipboard; }
        }

        public static PNRoutedUICommand DuplicateNote
        {
            get { return _DuplicateNote; }
        }

        public static PNRoutedUICommand SaveAsShortcut
        {
            get { return _SaveAsShortcut; }
        }

        public static PNRoutedUICommand GroupAdd
        {
            get { return _GroupAdd; }
        }

        public static PNRoutedUICommand GroupAddSubgroup
        {
            get { return _GroupAddSubgroup; }
        }

        public static PNRoutedUICommand GroupEdit
        {
            get { return _GroupEdit; }
        }

        public static PNRoutedUICommand GroupRemove
        {
            get { return _GroupRemove; }
        }

        public static PNRoutedUICommand GroupShow
        {
            get { return _GroupShow; }
        }

        public static PNRoutedUICommand GroupShowAll
        {
            get { return _GroupShowAll; }
        }

        public static PNRoutedUICommand GroupHide
        {
            get { return _GroupHide; }
        }

        public static PNRoutedUICommand GroupHideAll
        {
            get { return _GroupHideAll; }
        }

        public static PNRoutedUICommand GroupPassAdd
        {
            get { return _GroupPassAdd; }
        }

        public static PNRoutedUICommand GroupPassRemove
        {
            get { return _GroupPassRemove; }
        }

        public static PNRoutedUICommand Diary
        {
            get { return _Diary; }
        }

        public static PNRoutedUICommand Save
        {
            get { return _Save; }
        }

        public static PNRoutedUICommand SaveAs
        {
            get { return _SaveAs; }
        }

        public static PNRoutedUICommand SaveAsText
        {
            get { return _SaveAsText; }
        }

        public static PNRoutedUICommand RestoreFromBackup
        {
            get { return _RestoreFromBackup; }
        }

        public static PNRoutedUICommand Print
        {
            get { return _Print; }
        }

        public static PNRoutedUICommand Adjust
        {
            get { return _Adjust; }
        }

        public static PNRoutedUICommand AdjustAppearance
        {
            get { return _AdjustAppearance; }
        }

        public static PNRoutedUICommand AdjustSchedule
        {
            get { return _AdjustSchedule; }
        }

        public static PNRoutedUICommand Delete
        {
            get { return _Delete; }
        }

        public static PNRoutedUICommand SaveAll
        {
            get { return _SaveAll; }
        }

        public static PNRoutedUICommand BackupSync
        {
            get { return _BackupSync; }
        }

        public static PNRoutedUICommand Today
        {
            get { return _Today; }
        }

        public static PNRoutedUICommand BackupCreate
        {
            get { return _BackupCreate; }
        }

        public static PNRoutedUICommand BackupRestore
        {
            get { return _BackupRestore; }
        }

        public static PNRoutedUICommand SyncLocal
        {
            get { return _SyncLocal; }
        }

        public static PNRoutedUICommand ImportNotes
        {
            get { return _ImportNotes; }
        }

        public static PNRoutedUICommand ImportSettings
        {
            get { return _ImportSettings; }
        }

        public static PNRoutedUICommand ImportFonts
        {
            get { return _ImportFonts; }
        }

        public static PNRoutedUICommand ImportDictionaries
        {
            get { return _ImportDictionaries; }
        }

        public static PNRoutedUICommand Placement
        {
            get { return _Placement; }
        }

        public static PNRoutedUICommand DockAll
        {
            get { return _DockAll; }
        }

        public static PNRoutedUICommand DockAllNone
        {
            get { return _DockAllNone; }
        }

        public static PNRoutedUICommand DockAllLeft
        {
            get { return _DockAllLeft; }
        }

        public static PNRoutedUICommand DockAllTop
        {
            get { return _DockAllTop; }
        }

        public static PNRoutedUICommand DockAllRight
        {
            get { return _DockAllRight; }
        }

        public static PNRoutedUICommand DockAllBottom
        {
            get { return _DockAllBottom; }
        }

        public static PNRoutedUICommand Visibility
        {
            get { return _Visibility; }
        }

        public static PNRoutedUICommand ShowNote
        {
            get { return _ShowNote; }
        }

        public static PNRoutedUICommand ShowAll
        {
            get { return _ShowAll; }
        }

        public static PNRoutedUICommand HideNote
        {
            get { return _HideNote; }
        }

        public static PNRoutedUICommand HideAll
        {
            get { return _HideAll; }
        }

        public static PNRoutedUICommand AllToFront
        {
            get { return _AllToFront; }
        }

        public static PNRoutedUICommand SendAsText
        {
            get { return _SendAsText; }
        }

        public static PNRoutedUICommand SendAsAttachment
        {
            get { return _SendAsAttachment; }
        }

        public static PNRoutedUICommand SendAsZip
        {
            get { return _SendAsZip; }
        }

        public static PNRoutedUICommand SendNetwork
        {
            get { return _SendNetwork; }
        }

        public static PNRoutedUICommand ContactAdd
        {
            get { return _ContactAdd; }
        }

        public static PNRoutedUICommand ContactGroupAdd
        {
            get { return _ContactGroupAdd; }
        }

        public static PNRoutedUICommand ContactSelect
        {
            get { return _ContactSelect; }
        }

        public static PNRoutedUICommand ContactGroupSelect
        {
            get { return _ContactGroupSelect; }
        }

        public static PNRoutedUICommand Tags
        {
            get { return _Tags; }
        }

        public static PNRoutedUICommand TagsCurrent
        {
            get { return _TagsCurrent; }
        }

        public static PNRoutedUICommand TagsShowBy
        {
            get { return _TagsShowBy; }
        }

        public static PNRoutedUICommand TagsHideBy
        {
            get { return _TagsHideBy; }
        }

        public static PNRoutedUICommand Switches
        {
            get { return _Switches; }
        }

        public static PNRoutedUICommand OnTop
        {
            get { return _OnTop; }
        }

        public static PNRoutedUICommand HighPriority
        {
            get { return _HighPriority; }
        }

        public static PNRoutedUICommand ProtectionMode
        {
            get { return _ProtectionMode; }
        }

        public static PNRoutedUICommand SetNotePassword
        {
            get { return _SetNotePassword; }
        }

        public static PNRoutedUICommand RemoveNotePassword
        {
            get { return _RemoveNotePassword; }
        }

        public static PNRoutedUICommand MarkAsComplete
        {
            get { return _MarkAsComplete; }
        }

        public static PNRoutedUICommand RollUnroll
        {
            get { return _RollUnroll; }
        }

        public static PNRoutedUICommand Pin
        {
            get { return _Pin; }
        }

        public static PNRoutedUICommand Unpin
        {
            get { return _Unpin; }
        }

        public static PNRoutedUICommand Scramble
        {
            get { return _Scramble; }
        }

        public static PNRoutedUICommand Unscramble
        {
            get { return _Unscramble; }
        }

        public static PNRoutedUICommand AddFavorites
        {
            get { return _AddFavorites; }
        }

        public static PNRoutedUICommand Run
        {
            get { return _Run; }
        }

        public static PNRoutedUICommand EmptyBin
        {
            get { return _EmptyBin; }
        }

        public static PNRoutedUICommand RemoveFavorites
        {
            get { return _RemoveFavorites; }
        }

        public static PNRoutedUICommand RestoreNote
        {
            get { return _RestoreNote; }
        }

        public static PNRoutedUICommand Preview
        {
            get { return _Preview; }
        }

        public static PNRoutedUICommand PreviewSettings
        {
            get { return _PreviewSettings; }
        }

        public static PNRoutedUICommand UseCustColor
        {
            get { return _UseCustColor; }
        }

        public static PNRoutedUICommand ChooseCustColor
        {
            get { return _ChooseCustColor; }
        }

        public static PNRoutedUICommand PreviewRight
        {
            get { return _PreviewRight; }
        }

        public static PNRoutedUICommand ShowGroups
        {
            get { return _ShowGroups; }
        }

        public static PNRoutedUICommand ColReset
        {
            get { return _ColReset; }
        }

        public static PNRoutedUICommand HotkeysCP
        {
            get { return _HotkeysCP; }
        }

        public static PNRoutedUICommand MenusManagementCP
        {
            get { return _MenusManagementCP; }
        }

        public static PNRoutedUICommand Preferences
        {
            get { return _Preferences; }
        }

        public static PNRoutedUICommand Password
        {
            get { return _Password; }
        }

        public static PNRoutedUICommand Search
        {
            get { return _Search; }
        }

        public static PNRoutedUICommand QuickSearch
        {
            get { return _QuickSearch; }
        }

        public static PNRoutedUICommand Help
        {
            get { return _Help; }
        }

        public static PNRoutedUICommand Support
        {
            get { return _Support; }
        }

        public static PNRoutedUICommand About
        {
            get { return _About; }
        }

        public static PNRoutedUICommand PwrdCreate
        {
            get { return _PwrdCreate; }
        }

        public static PNRoutedUICommand PwrdChange
        {
            get { return _PwrdChange; }
        }

        public static PNRoutedUICommand PwrdRemove
        {
            get { return _PwrdRemove; }
        }

        public static PNRoutedUICommand SearchInNotes
        {
            get { return _SearchInNotes; }
        }

        public static PNRoutedUICommand SearchByTags
        {
            get { return _SearchByTags; }
        }

        public static PNRoutedUICommand SearchByDates
        {
            get { return _SearchByDates; }
        }

        public static PNRoutedUICommand IncBinInQSearch
        {
            get { return _IncBinInQSearch; }
        }

        public static PNRoutedUICommand ClearQSearch
        {
            get { return _ClearQSearch; }
        }

        public static PNRoutedUICommand Centralize
        {
            get { return _Centralize; }
        }

        public static PNRoutedUICommand PreviewFromMenu
        {
            get { return _PreviewFromMenu; }
        }

        public static PNRoutedUICommand ShowGroupsFromMenu
        {
            get { return _ShowGroupsFromMenu; }
        }

        public static PNRoutedUICommand PreviewRightFromMenu
        {
            get { return _PreviewRightFromMenu; }
        }
    }

    public class PNRoutedUICommand : RoutedUICommand, INotifyPropertyChanged
    {
        public PNRoutedUICommand(string text, string name, Type ownerType)
            : base(text, name, ownerType)
        {
        }

        public PNRoutedUICommand(string text, string name, Type ownerType, InputGestureCollection inputGestures)
            : base(text, name, ownerType, inputGestures)
        {
        }

        public string Section { get; set; }

        internal CommandType Type { get; set; }

        public new string Text
        {
            get { return base.Text; }
            set
            {
                if (value == base.Text) return;
                base.Text = value;
                OnPropertyChanged("Text");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
