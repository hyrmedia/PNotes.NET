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

#region Using

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using Domino;
using PNStaticFonts;
using Image = System.Drawing.Image;

#endregion

namespace PNotes.NET
{
    internal class EditControlSizeChangedEventArgs : EventArgs
    {
        internal Rectangle NewRectangle { get; private set; }

        internal EditControlSizeChangedEventArgs(Rectangle newEditRect)
        {
            NewRectangle = newEditRect;
        }
    }

    internal class ListBoxItemCheckChangedEventArgs : EventArgs
    {
        internal bool State { get; private set; }

        internal ListBoxItemCheckChangedEventArgs(bool state)
        {
            State = state;
        }
    }

    internal class TreeViewItemCheckChangedEventArgs : EventArgs
    {
        internal bool State { get; private set; }
        internal TreeView ParentTreeView { get; private set; }

        internal TreeViewItemCheckChangedEventArgs(bool state, TreeView parent)
        {
            State = state;
            ParentTreeView = parent;
        }
    }

    internal class BaloonClickedEventArgs : EventArgs
    {
        internal BaloonMode Mode { get; private set; }

        internal BaloonClickedEventArgs(BaloonMode mode)
        {
            Mode = mode;
        }
    }

    internal class NewVersionFoundEventArgs : EventArgs
    {
        internal string Version { get; private set; }

        internal NewVersionFoundEventArgs(string version)
        {
            Version = version;
        }
    }

    internal class ThemesUpdateFoundEventArgs : EventArgs
    {
        internal List<ThemesUpdate> ThemesList { get; private set; }

        internal ThemesUpdateFoundEventArgs(List<ThemesUpdate> listThemes)
        {
            ThemesList = listThemes;
        }
    }

    internal class PluginsUpdateFoundEventArgs : EventArgs
    {
        internal List<PluginsUpdate> PluginsList { get; private set; }
        internal PluginsUpdateFoundEventArgs(List<PluginsUpdate> listEx)
        {
            PluginsList = listEx;
        }
    }

    internal class CriticalUpdatesFoundEventArgs : EventArgs
    {
        internal bool Accepted { get; set; }
        internal string ProgramFileName { get; private set; }
        internal List<CriticalPluginUpdate> Plugins { get; private set; }
        internal CriticalUpdatesFoundEventArgs(string progFileName, List<CriticalPluginUpdate> plugins)
        {
            ProgramFileName = progFileName;
            Plugins = plugins;
        }
    }

    internal class PostSelectedEventArgs : EventArgs
    {
        internal string PostText { get; private set; }

        internal PostSelectedEventArgs(string text)
        {
            PostText = text;
        }
    }

    internal class CompSelectedEventArgs : EventArgs
    {
        private readonly string _CompName;

        internal CompSelectedEventArgs(string name)
        {
            _CompName = name;
        }

        internal string CompName
        {
            get { return _CompName; }
        }
    }

    internal class NotesSentEventArgs : EventArgs
    {
        private readonly List<PNote> _Notes;
        private readonly string _SentTo;

        internal NotesSentEventArgs(List<PNote> notes, string sentTo)
        {
            _Notes = notes;
            _SentTo = sentTo;
        }

        internal string SentTo
        {
            get { return _SentTo; }
        }

        internal List<PNote> Notes
        {
            get { return _Notes; }
        }
    }

    internal class PinnedWindowChangedEventArgs : EventArgs
    {
        private readonly string _PinClass;
        private readonly string _PinText;

        internal PinnedWindowChangedEventArgs(string pinClass, string pinText)
        {
            _PinClass = pinClass;
            _PinText = pinText;
        }

        internal string PinText
        {
            get { return _PinText; }
        }

        internal string PinClass
        {
            get { return _PinClass; }
        }
    }

    internal class ExchangeEnableChangedEventArgs : EventArgs
    {
        private readonly bool _Enabled;

        internal ExchangeEnableChangedEventArgs(bool enabled)
        {
            _Enabled = enabled;
        }

        internal bool Enabled
        {
            get { return _Enabled; }
        }
    }

    internal class ContactsSelectedEventArgs : EventArgs
    {
        private readonly List<PNContact> _Contacts = new List<PNContact>();

        internal List<PNContact> Contacts
        {
            get { return _Contacts; }
        }
    }

    internal class ContactGroupChangedEventArgs : EventArgs
    {
        private readonly PNContactGroup _Group;
        private readonly AddEditMode _Mode;

        internal ContactGroupChangedEventArgs(PNContactGroup cg, AddEditMode mode)
        {
            _Group = cg;
            _Mode = mode;
            Accepted = true;
        }

        internal bool Accepted { get; set; }

        internal AddEditMode Mode
        {
            get { return _Mode; }
        }

        internal PNContactGroup Group
        {
            get { return _Group; }
        }
    }

    internal class ContactChangedEventArgs : EventArgs
    {
        private readonly PNContact _Contact;
        private readonly AddEditMode _Mode;

        internal ContactChangedEventArgs(PNContact contact, AddEditMode mode)
        {
            _Contact = contact;
            _Mode = mode;
            Accepted = true;
        }

        internal bool Accepted { get; set; }

        internal AddEditMode Mode
        {
            get { return _Mode; }
        }

        internal PNContact Contact
        {
            get { return _Contact; }
        }
    }

    internal class ContactsImportedEventArgs : EventArgs
    {
        internal IEnumerable<Tuple<string, string>> Contacts { get; private set; }

        internal ContactsImportedEventArgs(IEnumerable<Tuple<string, string>> contacts)
        {
            Contacts = contacts;
        }
    }

    internal class MailContactChangedEventArgs : EventArgs
    {
        private readonly PNMailContact _Contact;
        private readonly AddEditMode _Mode;

        internal MailContactChangedEventArgs(PNMailContact contact, AddEditMode mode)
        {
            _Contact = contact;
            _Mode = mode;
            Accepted = true;
        }

        internal bool Accepted { get; set; }

        internal AddEditMode Mode
        {
            get { return _Mode; }
        }

        internal PNMailContact Contact
        {
            get { return _Contact; }
        }
    }

    internal class SmtpChangedEventArgs : EventArgs
    {
        private readonly PNSmtpProfile _Profile;
        private readonly AddEditMode _Mode;

        internal SmtpChangedEventArgs(PNSmtpProfile profile, AddEditMode mode)
        {
            _Profile = profile;
            _Mode = mode;
            Accepted = true;
        }

        internal bool Accepted { get; set; }

        internal AddEditMode Mode
        {
            get { return _Mode; }
        }

        internal PNSmtpProfile Profile
        {
            get { return _Profile; }
        }
    }

    internal class LocalSyncCompleteEventArgs : EventArgs
    {
        private readonly LocalSyncResult _Result;

        internal LocalSyncCompleteEventArgs(LocalSyncResult result)
        {
            _Result = result;
        }

        internal LocalSyncResult Result
        {
            get { return _Result; }
        }
    }

    internal class SmilieSelectedEventArgs : EventArgs
    {
        private readonly Image _Image;

        internal SmilieSelectedEventArgs(Image image)
        {
            _Image = image;
        }

        internal Image Image
        {
            get { return _Image; }
        }
    }

    internal class NoteAppearanceAdjustedEventArgs : EventArgs
    {
        private readonly bool _CustomOpacity;
        private readonly bool _CustomSkin;
        private readonly bool _CustomSkinless;
        private readonly double _Opacity;
        private readonly PNSkinDetails _Skin;
        private readonly PNSkinlessDetails _Skinless;

        internal NoteAppearanceAdjustedEventArgs(bool custOpacity, bool custSkinless, bool custSkin, double opacity,
                                                 PNSkinlessDetails skinless, PNSkinDetails skin)
        {
            _CustomOpacity = custOpacity;
            _CustomSkinless = custSkinless;
            _CustomSkin = custSkin;
            _Opacity = opacity;
            _Skinless = skinless;
            _Skin = skin;
        }

        internal PNSkinDetails Skin
        {
            get { return _Skin; }
        }

        internal PNSkinlessDetails Skinless
        {
            get { return _Skinless; }
        }

        internal double Opacity
        {
            get { return _Opacity; }
        }

        internal bool CustomSkin
        {
            get { return _CustomSkin; }
        }

        internal bool CustomSkinless
        {
            get { return _CustomSkinless; }
        }

        internal bool CustomOpacity
        {
            get { return _CustomOpacity; }
        }
    }

    internal class NoteDeletedCompletelyEventArgs : EventArgs
    {
        private readonly int _GroupID;
        private readonly string _ID;

        internal NoteDeletedCompletelyEventArgs(string id, int groupID)
        {
            _ID = id;
            _GroupID = groupID;
        }

        internal int GroupID
        {
            get { return _GroupID; }
        }

        internal string ID
        {
            get { return _ID; }
        }
    }

    internal class NewNoteCreatedEventArgs : EventArgs
    {
        private readonly PNote _Note;

        internal NewNoteCreatedEventArgs(PNote note)
        {
            _Note = note;
        }

        internal PNote Note
        {
            get { return _Note; }
        }
    }

    internal class SpellCheckingStatusChangedEventArgs : EventArgs
    {
        private readonly bool _Status;

        internal SpellCheckingStatusChangedEventArgs(bool newStatus)
        {
            _Status = newStatus;
        }

        internal bool Status
        {
            get { return _Status; }
        }
    }

    internal class FontSelectedEventArgs : EventArgs
    {
        private readonly LOGFONT _LogFont;

        internal FontSelectedEventArgs(LOGFONT lf)
        {
            _LogFont = lf;
        }

        internal LOGFONT LogFont
        {
            get { return _LogFont; }
        }
    }

    internal class PasswordChangedEventArgs : EventArgs
    {
        private readonly string _NewPassword;

        internal PasswordChangedEventArgs(string newPwrd)
        {
            _NewPassword = newPwrd;
        }

        internal string NewPassword
        {
            get { return _NewPassword; }
        }
    }

    internal class GroupChangedEventArgs : EventArgs
    {
        internal GroupChangedEventArgs(PNGroup group, AddEditMode mode, PNTreeItem treeItem)
        {
            Group = group;
            Mode = mode;
            TreeItem = treeItem;
        }

        internal AddEditMode Mode { get; private set; }

        internal PNGroup Group { get; private set; }

        internal PNTreeItem TreeItem { get; private set; }
    }

    internal class GroupPropertyChangedEventArgs : EventArgs
    {
        private readonly object _NewStateObject;
        private readonly GroupChangeType _Type;

        internal GroupPropertyChangedEventArgs(object newStateObject, GroupChangeType type)
        {
            _NewStateObject = newStateObject;
            _Type = type;
        }

        internal GroupChangeType Type
        {
            get { return _Type; }
        }

        internal object NewStateObject
        {
            get { return _NewStateObject; }
        }
    }

    internal class NoteSendReceiveStatusChangedEventArgs : EventArgs
    {
        private readonly SendReceiveStatus _NewStatus;
        private readonly SendReceiveStatus _OldStatus;

        internal NoteSendReceiveStatusChangedEventArgs(SendReceiveStatus newStatus, SendReceiveStatus oldStatus)
        {
            _NewStatus = newStatus;
            _OldStatus = oldStatus;
        }

        internal SendReceiveStatus OldStatus
        {
            get { return _OldStatus; }
        }

        internal SendReceiveStatus NewStatus
        {
            get { return _NewStatus; }
        }
    }

    internal class NoteDockStatusChangedEventArgs : EventArgs
    {
        private readonly DockStatus _NewStatus;
        private readonly DockStatus _OldStatus;

        internal NoteDockStatusChangedEventArgs(DockStatus newStatus, DockStatus oldStatus)
        {
            _NewStatus = newStatus;
            _OldStatus = oldStatus;
        }

        internal DockStatus OldStatus
        {
            get { return _OldStatus; }
        }

        internal DockStatus NewStatus
        {
            get { return _NewStatus; }
        }
    }

    internal class NoteDateChangedEventArgs : EventArgs
    {
        private readonly DateTime _NewDate;
        private readonly DateTime _OldDate;
        private readonly NoteDateType _Type;

        internal NoteDateChangedEventArgs(DateTime newDate, DateTime oldDate, NoteDateType type)
        {
            _NewDate = newDate;
            _OldDate = oldDate;
            _Type = type;
        }

        internal DateTime NewDate
        {
            get { return _NewDate; }
        }

        internal DateTime OldDate
        {
            get { return _OldDate; }
        }

        internal NoteDateType Type
        {
            get { return _Type; }
        }
    }

    internal class NoteGroupChangedEventArgs : EventArgs
    {
        private readonly int _NewGroup;
        private readonly int _OldGroup;

        internal NoteGroupChangedEventArgs(int newGroup, int oldGroup)
        {
            _NewGroup = newGroup;
            _OldGroup = oldGroup;
        }

        internal int NewGroup
        {
            get { return _NewGroup; }
        }

        internal int OldGroup
        {
            get { return _OldGroup; }
        }
    }

    internal class NoteNameChangedEventArgs : EventArgs
    {
        private readonly string _NewName;
        private readonly string _OldName;

        internal NoteNameChangedEventArgs(string oldName, string newName)
        {
            _NewName = newName;
            _OldName = oldName;
        }

        internal string NewName
        {
            get { return _NewName; }
        }

        internal string OldName
        {
            get { return _OldName; }
        }
    }

    internal class SaveAsNoteNameSetEventArgs : EventArgs
    {
        private readonly int _GroupID;
        private readonly string _Name;

        internal SaveAsNoteNameSetEventArgs(string name, int groupID)
        {
            _Name = name;
            _GroupID = groupID;
        }

        internal int GroupID
        {
            get { return _GroupID; }
        }

        internal string Name
        {
            get { return _Name; }
        }
    }

    internal class NoteDeletedEventArgs : EventArgs
    {
        private readonly NoteDeleteType _Type;

        internal NoteDeletedEventArgs(NoteDeleteType type)
        {
            _Type = type;
        }

        internal NoteDeleteType Type
        {
            get { return _Type; }
        }

        internal bool Processed { get; set; }
    }

    internal class NoteBooleanChangedEventArgs : EventArgs
    {
        private readonly bool _State;
        private readonly object _StateObject;
        private readonly NoteBooleanTypes _Type;

        internal NoteBooleanChangedEventArgs(bool state, NoteBooleanTypes type, object stateObject)
        {
            _State = state;
            _Type = type;
            _StateObject = stateObject;
        }

        internal bool Processed { get; set; }

        internal NoteBooleanTypes Type
        {
            get { return _Type; }
        }

        internal object StateObject
        {
            get { return _StateObject; }
        }

        internal bool State
        {
            get { return _State; }
        }
    }

    internal class NoteMovedEventArgs : EventArgs
    {
        private readonly Point _NoteLocation;

        internal NoteMovedEventArgs(Point location)
        {
            _NoteLocation = location;
        }

        internal Point NoteLocation
        {
            get { return _NoteLocation; }
        }
    }

    internal class NoteResizedEventArgs : EventArgs
    {
        private readonly Size _NoteSize;

        internal NoteResizedEventArgs(Size size)
        {
            _NoteSize = size;
        }

        internal Size NoteSize
        {
            get { return _NoteSize; }
        }
    }

    internal class TableReadyEventArgs : EventArgs
    {
        internal string TableRtf { get; private set; }

        internal TableReadyEventArgs(string tableRtf)
        {
            TableRtf = tableRtf;
        }
    }

    internal class SpecialSymbolSelectedEventArgs : EventArgs
    {
        internal string Symbol { get; private set; }

        internal SpecialSymbolSelectedEventArgs(string symbol)
        {
            Symbol = symbol;
        }
    }

    internal class MenusOrderChangedEventArgs : EventArgs
    {
        internal bool Main { get; set; }
        internal bool Note { get; set; }
        internal bool Edit { get; set; }
        internal bool ControlPanel { get; set; }
    }

    internal class MailRecipientsChosenEventArgs : EventArgs
    {
        internal IEnumerable<PNMailContact> Recipients { get; private set; }

        internal MailRecipientsChosenEventArgs(IEnumerable<PNMailContact> recipients)
        {
            Recipients = recipients;
        }
    }

    internal class CanvasSavedEventArgs : EventArgs
    {
        internal Image Image { get; private set; }

        internal CanvasSavedEventArgs(Image image)
        {
            Image = image;
        }
    }

    internal class LotusCredentialSetEventArgs : EventArgs
    {
        internal NotesView PeopleView { get; private set; }
        internal NotesView ContactsView { get; private set; }

        internal LotusCredentialSetEventArgs(NotesView peopleView, NotesView contactsView)
        {
            PeopleView = peopleView;
            ContactsView = contactsView;
        }
    }
}