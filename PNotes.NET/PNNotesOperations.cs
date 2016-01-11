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

using PNEncryption;
using PNRichEdit;
using SQLiteWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Application = System.Windows.Forms.Application;
using Cursors = System.Windows.Input.Cursors;
using DataFormats = System.Windows.DataFormats;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Point = System.Windows.Point;
using PointConverter = System.Windows.PointConverter;
using RichTextBox = System.Windows.Forms.RichTextBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using Size = System.Windows.Size;
using SizeConverter = System.Windows.SizeConverter;

namespace PNotes.NET
{
    internal static class PNNotesOperations
    {
        private static void groupShowHide(PNGroup group, bool show)
        {
            try
            {
                ShowHideSpecificGroup(group.ID, show);
                foreach (var g in group.Subgroups)
                {
                    groupShowHide(g, show);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void RefreshHiddenMenus()
        {
            try
            {
                var hiddensNote = PNStatic.HiddenMenus.Where(hm => hm.Type == MenuType.Note).ToArray();
                var hiddensEdit = PNStatic.HiddenMenus.Where(hm => hm.Type == MenuType.Edit).ToArray();
                foreach (var note in PNStatic.Notes.Where(n => n.Visible && n.Dialog != null))
                {
                    PNStatic.HideMenus(note.Dialog.NoteMenu, hiddensNote);
                    PNStatic.HideMenus(note.Dialog.EditMenu, hiddensEdit);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplyTransparency(double opacity)
        {
            try
            {
                foreach (var note in PNStatic.Notes)
                {
                    note.Opacity = opacity;
                    if (note.FromDB)
                    {
                        SaveNoteOpacity(note);
                    }
                    if (note.Visible)
                    {
                        note.Dialog.Opacity = opacity;
                    }

                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplyButtonsSize(ToolStripButtonSize bs)
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Visible);
                foreach (var n in notes)
                {
                    n.Dialog.SetFooterButtonSize(bs);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplyHideToolbar()
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Visible);
                foreach (var n in notes)
                {
                    n.Dialog.HideToolbar();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplyDeleteButtonVisibility(bool visibility)
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Visible);
                foreach (var n in notes)
                {
                    n.Dialog.ApplyDeleteButtonVisibility(visibility);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplyUseAlternative(bool value)
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Visible);
                foreach (var n in notes)
                {
                    n.Dialog.ApplyUseAlternative(value);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplySpellColor()
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Visible);
                foreach (var n in notes)
                {
                    n.Dialog.InvalidateVisual();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplyMarginsWidth(short marginSize)
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Visible);
                foreach (var n in notes)
                {
                    n.Dialog.ApplyMarginsWidth(marginSize);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplyAutoHeight()
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Visible && n.DockStatus == DockStatus.None && !n.Rolled);
                foreach (var n in notes)
                {
                    n.Dialog.ApplyAutoHeight();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplyShowScrollBars(System.Windows.Forms.RichTextBoxScrollBars value)
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Visible);
                foreach (var n in notes)
                {
                    n.Dialog.ApplyShowScrollBars(value);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplyKeepVisibleOnShowDesktop(bool exclude)
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Visible);
                foreach (var n in notes)
                {
                    PNStatic.ToggleAeroPeek(new WindowInteropHelper(n.Dialog).Handle, exclude);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplyHideButtonVisibility(bool visibility)
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Visible);
                foreach (var n in notes)
                {
                    n.Dialog.ApplyHideButtonVisibility(visibility);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplyPanelButtonVisibility(bool visibility)
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Visible);
                foreach (var n in notes)
                {
                    n.Dialog.ApplyPanelButtonVisibility(visibility);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplySwitch(NoteBooleanTypes sw, bool value)
        {
            try
            {
                IEnumerable<PNote> notes;
                switch (sw)
                {
                    case NoteBooleanTypes.Complete:
                        notes = value ? PNStatic.Notes.Where(n => !n.Completed) : PNStatic.Notes.Where(n => n.Completed);
                        foreach (var n in notes)
                        {
                            if (n.Visible)
                            {
                                n.Dialog.ApplySwitch(sw);
                            }
                            else
                            {
                                n.Completed = value;
                            }
                        }
                        break;
                    case NoteBooleanTypes.Priority:
                        notes = value ? PNStatic.Notes.Where(n => !n.Priority) : PNStatic.Notes.Where(n => n.Priority);
                        foreach (var n in notes)
                        {
                            if (n.Visible)
                            {
                                n.Dialog.ApplySwitch(sw);
                            }
                            else
                            {
                                n.Priority = value;
                            }
                        }
                        break;
                    case NoteBooleanTypes.Protection:
                        notes = value ? PNStatic.Notes.Where(n => !n.Protected) : PNStatic.Notes.Where(n => n.Protected);
                        foreach (var n in notes)
                        {
                            if (n.Visible)
                            {
                                n.Dialog.ApplySwitch(sw);
                            }
                            else
                            {
                                n.Protected = value;
                            }
                        }
                        break;
                    case NoteBooleanTypes.Roll:
                        notes = value ? PNStatic.Notes.Where(n => !n.Rolled) : PNStatic.Notes.Where(n => n.Rolled);
                        foreach (var n in notes)
                        {
                            if (n.Visible)
                            {
                                n.Dialog.ApplySwitch(sw);
                            }
                            else
                            {
                                n.Rolled = value;
                            }
                        }
                        break;
                    case NoteBooleanTypes.Topmost:
                        notes = value ? PNStatic.Notes.Where(n => !n.Topmost) : PNStatic.Notes.Where(n => n.Topmost);
                        foreach (var n in notes)
                        {
                            if (n.Visible)
                            {
                                n.Dialog.ApplySwitch(sw);
                            }
                            else
                            {
                                n.Topmost = value;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static bool LoginToGroup(PNGroup group, ref List<int> loggedGroups)
        {
            try
            {
                var result = true;
                if (group.ParentID != -1)
                {
                    var parent = PNStatic.Groups.GetGroupByID(group.ParentID);
                    if (parent != null)
                    {
                        result &= LoginToGroup(parent, ref loggedGroups);
                    }
                }
                if (!loggedGroups.Contains(group.ID))
                {
                    if (group.PasswordString.Trim().Length > 0)
                    {
                        var text = " [" + PNLang.Instance.GetCaptionText("group", "Group") + " \"" + group.Name + "\"]";
                        var pwrdDelete = new WndPasswordDelete(PasswordDlgMode.LoginGroup, text, group.PasswordString)
                        {
                            Topmost = true,
                            Owner = PNStatic.FormMain
                        };
                        var showDialog = pwrdDelete.ShowDialog();
                        if (showDialog == null || !showDialog.Value)
                        {
                            return false;
                        }
                        loggedGroups.Add(@group.ID);
                    }
                    else
                    {
                        loggedGroups.Add(group.ID);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal static bool LoginToNote(PNote note)
        {
            try
            {
                var text = " [" + PNLang.Instance.GetCaptionText("note", "Note") + " \"" + note.Name + "\"]";
                var pwrdDelete = new WndPasswordDelete(PasswordDlgMode.LoginNote, text, note.PasswordString)
                {
                    Topmost = true,
                    Owner = PNStatic.FormMain
                };
                var showDialog = pwrdDelete.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private static bool isNotePresentedInCustomSettings(PNote note)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("SELECT COUNT(NOTE_ID) AS COUNT_ID FROM CUSTOM_NOTES_SETTINGS WHERE NOTE_ID = '");
                sb.Append(note.ID);
                sb.Append("'");
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    var o = oData.GetScalar(sb.ToString());
                    if (o != null && !PNData.IsDBNull(o) && Convert.ToInt32(o) > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal static void RemoveCustomNotesSettings(string id)
        {
            try
            {
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    var sb = new StringBuilder("DELETE FROM CUSTOM_NOTES_SETTINGS WHERE NOTE_ID = '");
                    sb.Append(id);
                    sb.Append("'");
                    oData.Execute(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveNoteSkin(PNote note)
        {
            try
            {
                var sb = new StringBuilder();
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    if (isNotePresentedInCustomSettings(note))
                    {
                        sb.Append("UPDATE CUSTOM_NOTES_SETTINGS SET SKIN_NAME = ");
                        if (note.Skin != null)
                        {
                            sb.Append("'");
                            sb.Append(note.Skin.SkinName);
                            sb.Append("'");
                        }
                        else
                        {
                            sb.Append("NULL");
                        }
                        sb.Append(" WHERE NOTE_ID = '");
                        sb.Append(note.ID);
                        sb.Append("'");
                    }
                    else
                    {
                        sb.Append("INSERT INTO CUSTOM_NOTES_SETTINGS (NOTE_ID, SKIN_NAME) VALUES(");
                        if (note.Skin != null)
                        {
                            sb.Append("'");
                            sb.Append(note.ID);
                            sb.Append("', '");
                            sb.Append(note.Skin.SkinName);
                            sb.Append("')");
                        }
                        else
                        {
                            sb.Append("'");
                            sb.Append(note.ID);
                            sb.Append("', NULL)");
                        }
                    }
                    oData.Execute(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveNoteSkinless(PNote note)
        {
            try
            {
                var cr = new ColorConverter();
                var lfc = new WPFFontConverter();
                var sb = new StringBuilder();
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    if (isNotePresentedInCustomSettings(note))
                    {
                        sb.Append("UPDATE CUSTOM_NOTES_SETTINGS SET BACK_COLOR = ");
                        if (note.Skinless != null)
                        {
                            sb.Append("'");
                            sb.Append(cr.ConvertToString(null, PNStatic.CultureInvariant, note.Skinless.BackColor));
                            sb.Append("'");
                        }
                        else
                        {
                            sb.Append("NULL");
                        }
                        sb.Append(", CAPTION_FONT_COLOR = ");
                        if (note.Skinless != null)
                        {
                            sb.Append("'");
                            sb.Append(cr.ConvertToString(null, PNStatic.CultureInvariant, note.Skinless.CaptionColor));
                            sb.Append("'");
                        }
                        else
                        {
                            sb.Append("NULL");
                        }
                        sb.Append(", CAPTION_FONT = ");
                        if (note.Skinless != null)
                        {
                            sb.Append("'");
                            sb.Append(lfc.ConvertToString(note.Skinless.CaptionFont));
                            sb.Append("'");
                        }
                        else
                        {
                            sb.Append("NULL");
                        }
                        sb.Append(" WHERE NOTE_ID = '");
                        sb.Append(note.ID);
                        sb.Append("'");
                    }
                    else
                    {
                        sb.Append("INSERT INTO CUSTOM_NOTES_SETTINGS (NOTE_ID, BACK_COLOR, CAPTION_FONT_COLOR, CAPTION_FONT) VALUES(");
                        if (note.Skinless != null)
                        {
                            sb.Append("'");
                            sb.Append(note.ID);
                            sb.Append("', '");
                            sb.Append(cr.ConvertToString(null, PNStatic.CultureInvariant, note.Skinless.BackColor));
                            sb.Append("', '");
                            sb.Append(cr.ConvertToString(null, PNStatic.CultureInvariant, note.Skinless.CaptionColor));
                            sb.Append("', '");
                            sb.Append(lfc.ConvertToString(note.Skinless.CaptionFont));
                            sb.Append("')");
                        }
                        else
                        {
                            sb.Append("'");
                            sb.Append(note.ID);
                            sb.Append("', NULL, NULL, NULL)");
                        }
                    }
                    oData.Execute(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveNoteOpacity(PNote note)
        {
            try
            {
                var sb = new StringBuilder();
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    sb.Append("UPDATE NOTES SET OPACITY = ");
                    sb.Append(note.Opacity.ToString(PNStatic.CultureInvariant));
                    sb.Append(" WHERE ID = '");
                    sb.Append(note.ID);
                    sb.Append("'");
                    oData.Execute(sb.ToString());

                    sb = new StringBuilder();
                    if (isNotePresentedInCustomSettings(note))
                    {
                        sb.Append("UPDATE CUSTOM_NOTES_SETTINGS SET CUSTOM_OPACITY = ");
                        sb.Append(Convert.ToInt32(note.CustomOpacity));
                        sb.Append(" WHERE NOTE_ID = '");
                        sb.Append(note.ID);
                        sb.Append("'");
                    }
                    else
                    {
                        sb.Append("INSERT INTO CUSTOM_NOTES_SETTINGS (NOTE_ID, CUSTOM_OPACITY) VALUES('");
                        sb.Append(note.ID);
                        sb.Append("', ");
                        sb.Append(Convert.ToInt32(note.CustomOpacity));
                        sb.Append(")");
                    }
                    oData.Execute(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveNoteSize(PNote note, Size size, System.Drawing.Size editSize)
        {
            try
            {
                note.NoteSize = size;
                note.EditSize = editSize;
                if (note.FromDB)
                {
                    var sc = new SizeConverter();
                    var scd = new System.Drawing.SizeConverter();
                    var sb = new StringBuilder();
                    sb.Append("UPDATE NOTES SET SIZE = '");
                    sb.Append(sc.ConvertToString(null, PNStatic.CultureInvariant, note.NoteSize));
                    sb.Append("', EDIT_SIZE = '");
                    sb.Append(scd.ConvertToString(null, PNStatic.CultureInvariant, note.EditSize));
                    sb.Append("' WHERE ID = '");
                    sb.Append(note.ID);
                    sb.Append("'");
                    using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                    {
                        oData.Execute(sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveNoteThumbnail(PNote note)
        {
            try
            {
                if (note.FromDB)
                {
                    var sb = new StringBuilder();
                    sb.Append("UPDATE NOTES SET THUMBNAIL = ");
                    sb.Append(Convert.ToInt32(note.Thumbnail));
                    sb.Append(" WHERE ID = '");
                    sb.Append(note.ID);
                    sb.Append("'");
                    using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                    {
                        oData.Execute(sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveNotePin(PNote note)
        {
            try
            {
                if (note.FromDB)
                {
                    var sb = new StringBuilder();
                    sb.Append("UPDATE NOTES SET PINNED = ");
                    sb.Append(Convert.ToInt32(note.Pinned));
                    sb.Append(", PIN_CLASS = '");
                    sb.Append(note.PinClass);
                    sb.Append("', PIN_TEXT = '");
                    sb.Append(note.PinText);
                    sb.Append("' WHERE ID = '");
                    sb.Append(note.ID);
                    sb.Append("'");
                    using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                    {
                        oData.Execute(sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveNoteLocation(PNote note, Point location)
        {
            try
            {
                note.NoteLocation = location;
                var factors = getRelationalFactors(note);
                note.XFactor = factors[0];
                note.YFactor = factors[1];

                if (note.FromDB)
                {
                    var pc = new PointConverter();
                    var sb = new StringBuilder();
                    sb.Append("UPDATE NOTES SET LOCATION = '");
                    sb.Append(pc.ConvertToString(null, PNStatic.CultureInvariant, note.NoteLocation));
                    sb.Append("', REL_X = ");
                    sb.Append(factors[0].ToString(PNStatic.CultureInvariant));
                    sb.Append(", REL_Y = ");
                    sb.Append(factors[1].ToString(PNStatic.CultureInvariant));
                    sb.Append(" WHERE ID = '");
                    sb.Append(note.ID);
                    sb.Append("'");
                    using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                    {
                        oData.Execute(sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static double[] getRelationalFactors(PNote note)
        {
            var factors = new[] { 0.0, 0.0 };
            try
            {
                var size = PNStatic.AllScreensSize();
                factors[0] = size.Width > 0 ? note.NoteLocation.X / size.Width : 0;
                factors[1] = size.Height > 0 ? note.NoteLocation.Y / size.Height : 0;
                return factors;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return factors;
            }
        }

        internal static bool ApplyBooleanChange(PNote note, NoteBooleanTypes type, bool state, object stateObject)
        {
            try
            {
                switch (type)
                {
                    case NoteBooleanTypes.Roll:
                        note.Rolled = state;
                        return SaveBooleanChange(note, state, "ROLLED");
                    case NoteBooleanTypes.Topmost:
                        note.Topmost = state;
                        return SaveBooleanChange(note, state, "TOPMOST");
                    case NoteBooleanTypes.Priority:
                        note.Priority = state;
                        return SaveBooleanChange(note, state, "PRIORITY");
                    case NoteBooleanTypes.Protection:
                        note.Protected = state;
                        return SaveBooleanChange(note, state, "PROTECTED");
                    case NoteBooleanTypes.Scrambled:
                        note.Scrambled = state;
                        return SaveBooleanChange(note, state, "SCRAMBLED");
                    case NoteBooleanTypes.Visible:
                        note.Visible = state;
                        return SaveBooleanChange(note, state, "VISIBLE");
                    case NoteBooleanTypes.Pin:
                        note.Pinned = state;
                        return SaveBooleanChange(note, state, "PINNED");
                    case NoteBooleanTypes.Password:
                        if (state)
                        {
                            note.PasswordString = (string)stateObject;
                        }
                        else
                        {
                            note.PasswordString = "";
                        }
                        return SavePasswordChange(note);
                    case NoteBooleanTypes.Favorite:
                        note.Favorite = state;
                        return SaveBooleanChange(note, state, "FAVORITE");
                    case NoteBooleanTypes.Complete:
                        note.Completed = state;
                        return SaveBooleanChange(note, state, "COMPLETED");
                    case NoteBooleanTypes.Change:
                        note.Changed = state;
                        if (!note.Changed)
                        {
                            //note has been saved
                            var sa = stateObject as SaveAsNoteNameSetEventArgs;
                            if (sa != null)
                            {
                                note.Name = sa.Name;
                                note.GroupID = sa.GroupID;
                            }
                            return !note.FromDB ? SaveNewNote(note) : SaveExistingNote(note);
                        }
                        return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private static void showSpecificNote(PNote note)
        {
            try
            {
                if (!note.Visible)
                {
                    if (note.Completed && PNStatic.Settings.Behavior.HideCompleted)
                    {
                        //don't show note
                        return;
                    }
                    if (note.PasswordString.Trim().Length > 0)
                    {
                        if (!LoginToNote(note))
                        {
                            return;
                        }
                    }
                    if (ApplyBooleanChange(note, NoteBooleanTypes.Visible, true, null))
                    {
                        note.Dialog = new WndNote(note, note.ID, NewNoteMode.Identificator);
                        note.Dialog.Show();
                    }
                }
                else
                {
                    note.Dialog.SendWindowToForeground();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static bool LogIntoNoteOrGroup(PNote note)
        {
            try
            {
                var group = PNStatic.Groups.GetGroupByID(note.GroupID);
                var loggedGroups = new List<int>();
                if (group != null)
                {
                    if (!LoginToGroup(group, ref loggedGroups))
                    {
                        return false;
                    }
                }
                if (note.PasswordString.Trim().Length > 0)
                {
                    if (!LoginToNote(note))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal static ShowHideResult ShowHideSpecificNote(PNote note, bool show)
        {
            try
            {
                if (show)
                {
                    if (!note.Visible)
                    {
                        if (note.Completed && PNStatic.Settings.Behavior.HideCompleted)
                        {
                            var message = PNLang.Instance.GetMessageText("completed_hidden",
                                "In order to show notes marked as 'Completed' uncheck appropriate check box at 'Behavior' page of Preferences dialog");
                            PNMessageBox.Show(message, PNStrings.PROG_NAME);
                            //don't show note
                            return ShowHideResult.NotSuccess;
                        }
                        if (!LogIntoNoteOrGroup(note))
                            return ShowHideResult.NotSuccess;
                        if (ApplyBooleanChange(note, NoteBooleanTypes.Visible, true, null))
                        {
                            note.Dialog = new WndNote(note, note.ID, NewNoteMode.Identificator);
                            note.Dialog.Show();
                        }
                    }
                    else
                    {
                        note.Dialog.SendWindowToForeground();
                        if (note.Thumbnail)
                        {
                            PNStatic.FormPanel.RemoveThumbnail(note);
                        }
                    }
                }
                else
                {
                    if (note.Visible)
                    {
                        note.Dialog.ApplyHideNote(note);
                    }
                }
                return ShowHideResult.Success;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return ShowHideResult.NotSuccess;
            }
        }

        internal static void DeleteAllNotesShortcuts()
        {
            try
            {
                var desktop = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
                var shortcuts = desktop.GetFiles("*.lnk");
                var linksToDelete = new List<string>();
                var linksToSave = new List<string>();
                foreach (var f in shortcuts)
                {
                    using (var link = new PNShellLink())
                    {
                        link.Load(f.FullName);
                        if (
                            !string.Equals(link.Target, System.Windows.Forms.Application.ExecutablePath,
                                StringComparison.OrdinalIgnoreCase)) continue;
                        var args = link.Arguments;
                        if (!args.StartsWith("-i")) continue;
                        var arr = args.Split(' ');
                        if (arr.Length < 2) continue;
                        linksToDelete.Add(f.FullName);
                        linksToSave.Add(arr[1]);
                    }
                }
                foreach (var s in linksToDelete)
                {
                    File.Delete(s);
                }
                PNData.SaveNotesWithShortcuts(linksToSave);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void DeleteNoteShortcut(string id)
        {
            try
            {
                var arguments = "-i " + id;
                var desktop = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
                var shortcuts = desktop.GetFiles("*.lnk");
                var fileToDelete = "";
                foreach (var f in shortcuts)
                {
                    using (var link = new PNShellLink())
                    {
                        link.Load(f.FullName);
                        if (
                            !string.Equals(link.Target, System.Windows.Forms.Application.ExecutablePath,
                                StringComparison.OrdinalIgnoreCase)) continue;
                        if (link.Arguments != arguments) continue;
                        fileToDelete = f.FullName;
                        break;
                    }
                }
                if (fileToDelete.Length > 0)
                {
                    File.Delete(fileToDelete);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveAsShortcut(PNote note)
        {
            try
            {
                // create shortcut
                var counter = 1;
                var arguments = "-i " + note.ID;
                var pntargets = new List<string>();
                var desktop = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
                var shortcuts = desktop.GetFiles("*.lnk");
                //get all desktop shortcuts first
                foreach (var f in shortcuts)
                {
                    using (var link = new PNShellLink())
                    {
                        link.Load(f.FullName);
                        if (string.Equals(link.Target, System.Windows.Forms.Application.ExecutablePath, StringComparison.OrdinalIgnoreCase))
                        {
                            //collect shortcuts to PNotes.NET
                            pntargets.Add(f.FullName);
                        }
                    }
                }
                var shortcutName = note.Name;
                while (true)
                {
                    if (pntargets.Any(s =>
                    {
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(s);
                        return fileNameWithoutExtension != null && string.Equals(fileNameWithoutExtension, shortcutName, StringComparison.OrdinalIgnoreCase);
                    }))
                    {
                        counter++;
                        shortcutName = note.Name + "(" + counter + ")";
                        continue;
                    }
                    break;
                }
                foreach (var s in pntargets)
                {
                    using (var link = new PNShellLink())
                    {
                        link.Load(s);
                        if (link.Arguments == arguments)
                        {
                            //exit if there is shortcut to the current note
                            return;
                        }
                    }
                }
                var shortcutFile = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" +
                                   shortcutName + ".lnk";
                if (File.Exists(shortcutFile)) return;
                using (var link = new PNShellLink())
                {
                    link.ShortcutFile = shortcutFile;
                    link.Target = System.Windows.Forms.Application.ExecutablePath;
                    link.WorkingDirectory = System.Windows.Forms.Application.StartupPath;
                    link.IconPath = System.Windows.Forms.Application.ExecutablePath;
                    link.IconIndex = 1;
                    link.Arguments = arguments;
                    link.Save();
                }
                if (PNStatic.Settings.GeneralSettings.CloseOnShortcut)
                {
                    ShowHideSpecificNote(note, false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ShowHideSpecificGroup(int groupID, bool show)
        {
            try
            {
                var loggedGroups = new List<int>();
                if (show)
                {
                    var gr = PNStatic.Groups.GetGroupByID(groupID);
                    if (gr == null || !LoginToGroup(gr, ref loggedGroups)) return;
                    var notes = PNStatic.Notes.Where(n => n.GroupID == groupID);
                    foreach (var note in notes)
                    {
                        showSpecificNote(note);
                    }
                }
                else
                {
                    var notes = PNStatic.Notes.Where(n => n.GroupID == groupID);
                    foreach (var note in notes)
                    {
                        ShowHideSpecificNote(note, false);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ShowHideAllGroups(bool show)
        {
            try
            {
                Application.DoEvents();
                Mouse.OverrideCursor = Cursors.Wait;
                if (PNStatic.FormCP != null)
                {
                    PNInterop.LockWindowUpdate(PNStatic.FormCP.Handle);
                }
                foreach (var group in PNStatic.Groups)
                {
                    if (group.ID != (int)SpecialGroups.RecycleBin)
                    {
                        groupShowHide(group, show);
                    }
                }
                ShowHideSpecificGroup((int)SpecialGroups.Diary, show);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                PNInterop.LockWindowUpdate(IntPtr.Zero);
                Mouse.OverrideCursor = null;
            }
        }

        internal static bool SaveBooleanChange(PNote note, bool value, string field)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("UPDATE NOTES SET ");
                sb.Append(field);
                sb.Append(" = ");
                sb.Append(Convert.ToInt32(value));
                sb.Append(" WHERE ID = '");
                sb.Append(note.ID);
                sb.Append("'");
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    oData.Execute(sb.ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal static void ChangeNoteLookOnGroupChange(WndNote wnd, PNGroup group)
        {
            try
            {
                if (!PNStatic.Settings.GeneralSettings.UseSkins)
                {
                    wnd.Background =
                        new SolidColorBrush(group.Skinless.BackColor);
                    wnd.PHeader.SetPNFont(group.Skinless.CaptionFont);
                    wnd.Foreground =
                        new SolidColorBrush(group.Skinless.CaptionColor);
                }
                else
                {
                    var note = PNStatic.Notes.Note(wnd.Handle);
                    if (note != null && note.Skin == null)
                    {
                        PNSkinsOperations.ApplyNoteSkin(wnd, note);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveGroupChange(PNote note)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("UPDATE NOTES SET GROUP_ID = ");
                sb.Append(note.GroupID);
                sb.Append(", PREV_GROUP_ID = ");
                sb.Append(note.PrevGroupID);
                sb.Append(" WHERE ID = '");
                sb.Append(note.ID);
                sb.Append("'");
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    oData.Execute(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static bool SavePasswordChange(PNote note)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("UPDATE NOTES SET PASSWORD_STRING = '");
                sb.Append(note.PasswordString.Replace("'", "''"));
                sb.Append("' WHERE ID = '");
                sb.Append(note.ID);
                sb.Append("'");
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    oData.Execute(sb.ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal static void LoadNoteCustomProperties(PNote note, DataRow r)
        {
            try
            {
                if (!PNData.IsDBNull(r["BACK_COLOR"]) || !PNData.IsDBNull(r["CAPTION_FONT_COLOR"]) || !PNData.IsDBNull(r["CAPTION_FONT"]))
                {
                    var drawingColorConverter = new System.Drawing.ColorConverter();
                    var wfc = new WPFFontConverter();
                    var mediaColorConverter = new ColorConverter();

                    note.Skinless = new PNSkinlessDetails();
                    if (!PNData.IsDBNull(r["BACK_COLOR"]))
                    {
                        try
                        {
                            var clr = mediaColorConverter.ConvertFromString(null, PNStatic.CultureInvariant,
                                                    (string)r["BACK_COLOR"]);
                            if (clr != null)
                            {
                                note.Skinless.BackColor = (Color)clr;
                            }
                        }
                        catch (FormatException)
                        {
                            //possible FormatException after synchronization with old database
                            var clr = drawingColorConverter.ConvertFromString(null, PNStatic.CultureInvariant,
                                                    (string)r["BACK_COLOR"]);
                            if (clr != null)
                            {
                                var drawingColor = (System.Drawing.Color)clr;
                                note.Skinless.BackColor = Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G,
                                    drawingColor.B);
                                var sb = new StringBuilder("UPDATE CUSTOM_NOTES_SETTINGS SET BACK_COLOR = '");
                                sb.Append(mediaColorConverter.ConvertToString(null, PNStatic.CultureInvariant, note.Skinless.BackColor));
                                sb.Append("' WHERE NOTE_ID = '");
                                sb.Append(r["NOTE_ID"]);
                                sb.Append("'");
                                PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                            }
                        }
                    }
                    if (!PNData.IsDBNull(r["CAPTION_FONT_COLOR"]))
                    {
                        try
                        {
                            var clr = mediaColorConverter.ConvertFromString(null, PNStatic.CultureInvariant,
                                                    (string)r["CAPTION_FONT_COLOR"]);
                            if (clr != null)
                            {
                                note.Skinless.CaptionColor = (Color)clr;
                            }
                        }
                        catch (FormatException)
                        {
                            //possible FormatException after synchronization with old database
                            var clr = drawingColorConverter.ConvertFromString(null, PNStatic.CultureInvariant,
                                                    (string)r["CAPTION_FONT_COLOR"]);
                            if (clr != null)
                            {
                                var drawingColor = (System.Drawing.Color)clr;
                                note.Skinless.CaptionColor = Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G,
                                    drawingColor.B);
                                var sb = new StringBuilder("UPDATE CUSTOM_NOTES_SETTINGS SET CAPTION_FONT_COLOR = '");
                                sb.Append(mediaColorConverter.ConvertToString(null, PNStatic.CultureInvariant, note.Skinless.CaptionColor));
                                sb.Append("' WHERE NOTE_ID = '");
                                sb.Append(r["NOTE_ID"]);
                                sb.Append("'");
                                PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                            }
                        }
                    }
                    if (!PNData.IsDBNull(r["CAPTION_FONT"]))
                    {
                        var fontString = (string)r["CAPTION_FONT"];
                        //try
                        //{
                        var fonts = new InstalledFontCollection();
                        var arr = fontString.Split(',');
                        if (fontString.Any(ch => ch == '^'))
                        {
                            //old format font string
                            var lfc = new PNStaticFonts.LogFontConverter();
                            var lf = lfc.ConvertFromString((string)r["CAPTION_FONT"]);
                            note.Skinless.CaptionFont = PNStatic.FromLogFont(lf);
                            var sb = new StringBuilder("UPDATE CUSTOM_NOTES_SETTINGS SET CAPTION_FONT = '");
                            sb.Append(wfc.ConvertToString(null, PNStatic.CultureInvariant, note.Skinless.CaptionFont));
                            sb.Append("' WHERE NOTE_ID = '");
                            sb.Append(r["NOTE_ID"]);
                            sb.Append("'");
                            PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                        }
                        else if (fonts.Families.Any(ff => ff.Name == arr[0]))
                        {
                            note.Skinless.CaptionFont = (PNFont)wfc.ConvertFromString((string)r["CAPTION_FONT"]);
                        }
                        else
                        {
                            //possible note existing font name
                            arr[0] = PNStrings.DEF_CAPTION_FONT;
                            fontString = string.Join(",", arr);
                            note.Skinless.CaptionFont = (PNFont)wfc.ConvertFromString(fontString);
                            var sb = new StringBuilder("UPDATE CUSTOM_NOTES_SETTINGS SET CAPTION_FONT = '");
                            sb.Append(fontString);
                            sb.Append("' WHERE NOTE_ID = '");
                            sb.Append(r["NOTE_ID"]);
                            sb.Append("'");
                            PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                        }
                        //}
                        //catch (IndexOutOfRangeException)
                        //{
                        //    //possible IndexOutOfRangeException after synchronization with old database
                        //    var lfc = new PNStaticFonts.LogFontConverter();
                        //    var lf = lfc.ConvertFromString((string)r["CAPTION_FONT"]);
                        //    note.Skinless.CaptionFont = PNStatic.FromLogFont(lf);
                        //    var sb = new StringBuilder("UPDATE CUSTOM_NOTES_SETTINGS SET CAPTION_FONT = '");
                        //    sb.Append(wfc.ConvertToString(null, PNStatic.CultureInvariant, note.Skinless.CaptionFont));
                        //    sb.Append("' WHERE NOTE_ID = '");
                        //    sb.Append(r["NOTE_ID"]);
                        //    sb.Append("'");
                        //    PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                        //}
                    }
                }
                if (!PNData.IsDBNull(r["SKIN_NAME"]))
                {
                    note.Skin = new PNSkinDetails { SkinName = (string)r["SKIN_NAME"] };
                    var path = Path.Combine(PNPaths.Instance.SkinsDir, note.Skin.SkinName);
                    path += ".pnskn";
                    if (File.Exists(path))
                    {
                        PNSkinsOperations.LoadSkin(path, note.Skin);
                    }
                }
                if (!PNData.IsDBNull(r["CUSTOM_OPACITY"]))
                {
                    note.CustomOpacity = (bool)r["CUSTOM_OPACITY"];
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveAllNotes(bool showQuestion)
        {
            try
            {
                var notes = PNStatic.Notes.Where(n => n.Changed && n.Visible);
                foreach (var note in notes)
                {
                    note.Dialog.ApplySave(note, showQuestion);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static bool ResetNotesTags(IEnumerable<string> newTags)
        {
            try
            {
                var removedTags = PNStatic.Tags.Except(newTags).ToList();
                if (removedTags.Count > 0)
                {
                    var inList = removedTags.ToCommaSeparatedSQLString();
                    var sqlList = new List<string> { "DELETE FROM NOTES_TAGS WHERE TAG IN(" + inList + ")" };
                    if (!PNData.ExecuteTransactionForList(sqlList, PNData.ConnectionString))
                    {
                        return false;
                    }
                    foreach (var note in PNStatic.Notes)
                    {
                        note.Tags.RemoveAll(removedTags.Contains);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal static bool LoadNoteTags(PNote note)
        {
            try
            {
                var sqlQuery = "SELECT TAG FROM NOTES_TAGS WHERE NOTE_ID = '" + note.ID + "'";
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    using (var t = oData.FillDataTable(sqlQuery))
                    {
                        foreach (DataRow r in t.Rows)
                        {
                            note.Tags.Add((string)r["TAG"]);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal static bool LoadLinkedNotes(PNote note)
        {
            try
            {
                var sqlQuery = "SELECT LINK_ID FROM LINKED_NOTES WHERE NOTE_ID = '" + note.ID + "'";
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    using (var t = oData.FillDataTable(sqlQuery))
                    {
                        foreach (DataRow r in t.Rows)
                        {
                            note.LinkedNotes.Add((string)r["LINK_ID"]);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal static void SaveNoteTags(PNote note)
        {
            try
            {
                var sqlList = new List<string> { "DELETE FROM NOTES_TAGS WHERE NOTE_ID = '" + note.ID + "'" };
                sqlList.AddRange(
                    note.Tags.Select(
                        s =>
                        "INSERT INTO NOTES_TAGS (NOTE_ID, TAG) VALUES('" + note.ID + "','" + s.Replace("'", "''") + "')"));
                PNData.ExecuteTransactionForList(sqlList, PNData.ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveLinkedNotes(PNote note)
        {
            try
            {
                var sqlList = new List<string> { "DELETE FROM LINKED_NOTES WHERE NOTE_ID = '" + note.ID + "'" };
                sqlList.AddRange(
                    note.LinkedNotes.Select(
                        s =>
                        "INSERT INTO LINKED_NOTES (NOTE_ID, LINK_ID) VALUES('" + note.ID + "','" + s.Replace("'", "''") +
                        "')"));
                PNData.ExecuteTransactionForList(sqlList, PNData.ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static bool loadNoteSchedule(PNote note)
        {
            try
            {
                var sqlQuery = "SELECT * FROM NOTES_SCHEDULE WHERE NOTE_ID = '" + note.ID + "'";
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    using (var t = oData.FillDataTable(sqlQuery))
                    {
                        if (t.Rows.Count > 0)
                        {
                            var mc = new MonthDayConverter();
                            var ac = new AlarmAfterValuesConverter();
                            var dw = new DaysOfWeekConverter();

                            var r = t.Rows[0];
                            note.Schedule.Type = (ScheduleType)Convert.ToInt32(r["SCHEDULE_TYPE"]);
                            note.Schedule.AlarmDate = DateTime.Parse((string)r["ALARM_DATE"], PNStatic.CultureInvariant);
                            note.Schedule.StartDate = DateTime.Parse((string)r["START_DATE"], PNStatic.CultureInvariant);
                            note.Schedule.LastRun = DateTime.Parse((string)r["LAST_RUN"], PNStatic.CultureInvariant);
                            note.Schedule.Sound = (string)r["SOUND"];
                            note.Schedule.StopAfter = Convert.ToInt32(r["STOP_AFTER"]);
                            note.Schedule.Track = Convert.ToBoolean(r["TRACK"]);
                            note.Schedule.RepeatCount = Convert.ToInt32(r["REPEAT_COUNT"]);
                            note.Schedule.SoundInLoop = Convert.ToBoolean(r["SOUND_IN_LOOP"]);
                            note.Schedule.UseTts = Convert.ToBoolean(r["USE_TTS"]);
                            note.Schedule.StartFrom = (ScheduleStart)Convert.ToInt32(r["START_FROM"]);
                            note.Schedule.MonthDay = (MonthDay)mc.ConvertFromString((string)r["MONTH_DAY"]);
                            note.Schedule.AlarmAfter = (AlarmAfterValues)ac.ConvertFromString((string)r["ALARM_AFTER"]);
                            note.Schedule.Weekdays = (List<DayOfWeek>)dw.ConvertFromString((string)r["WEEKDAYS"]);
                            if (!DBNull.Value.Equals(r["PROG_TO_RUN"]))
                                note.Schedule.ProgramToRunOnAlert = Convert.ToString(r["PROG_TO_RUN"]);
                            if (!DBNull.Value.Equals(r["CLOSE_ON_NOTIFICATION"]))
                                note.Schedule.CloseOnNotification = Convert.ToBoolean(r["CLOSE_ON_NOTIFICATION"]);
                            if (!PNData.IsDBNull(r["MULTI_ALERTS"]))
                            {
                                var arr1 = ((string)r["MULTI_ALERTS"]).Split(new[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var ma in arr1.Select(s => s.Split('|')).Select(arr2 => new MultiAlert
                                {
                                    Raised = Convert.ToBoolean(arr2[0]),
                                    Date = DateTime.Parse(arr2[1], PNStatic.CultureInvariant)
                                }))
                                {
                                    note.Schedule.MultiAlerts.Add(ma);
                                }
                            }
                            if (!PNData.IsDBNull(r["TIME_ZONE"]))
                            {
                                note.Schedule.TimeZone = TimeZoneInfo.FromSerializedString((string)r["TIME_ZONE"]);
                            }
                            if (note.Schedule.Type != ScheduleType.None)
                            {
                                note.Timer.Start();
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal static bool LoadNoteProperties(PNote note, DataRow r)
        {
            try
            {
                var sc = new SizeConverter();
                var scd = new System.Drawing.SizeConverter();
                var pc = new PointConverter();

                note.FromDB = true;
                note.ID = (string)r["ID"];
                note.Name = (string)r["NAME"];
                note.GroupID = (int)r["GROUP_ID"];
                note.PrevGroupID = (int)r["PREV_GROUP_ID"];
                note.Opacity = (double)r["OPACITY"];
                note.Visible = (bool)r["VISIBLE"];
                note.Favorite = (bool)r["FAVORITE"];
                note.Protected = (bool)r["PROTECTED"];
                note.Completed = (bool)r["COMPLETED"];
                note.Priority = (bool)r["PRIORITY"];
                note.PasswordString = (string)r["PASSWORD_STRING"];
                note.Pinned = (bool)r["PINNED"];
                note.Topmost = (bool)r["TOPMOST"];
                note.Rolled = (bool)r["ROLLED"];
                note.DockStatus = (DockStatus)r["DOCK_STATUS"];
                note.DockOrder = (int)r["DOCK_ORDER"];
                note.SentReceived = (SendReceiveStatus)r["SEND_RECEIVE_STATUS"];
                note.DateCreated = DateTime.Parse((string)r["DATE_CREATED"], PNStatic.CultureInvariant);
                note.DateSaved = DateTime.Parse((string)r["DATE_SAVED"], PNStatic.CultureInvariant);
                note.DateSent = DateTime.Parse((string)r["DATE_SENT"], PNStatic.CultureInvariant);
                note.DateReceived = DateTime.Parse((string)r["DATE_RECEIVED"], PNStatic.CultureInvariant);
                note.DateDeleted = DateTime.Parse((string)r["DATE_DELETED"], PNStatic.CultureInvariant);
                var convertFromString = sc.ConvertFromString(null, PNStatic.CultureInvariant, (string)r["SIZE"]);
                if (convertFromString != null)
                    note.NoteSize = (Size)convertFromString;
                convertFromString = pc.ConvertFromString(null, PNStatic.CultureInvariant, (string)r["LOCATION"]);
                if (convertFromString != null)
                    note.NoteLocation = (Point)convertFromString;
                convertFromString = scd.ConvertFromString(null, PNStatic.CultureInvariant, (string)r["EDIT_SIZE"]);
                if (convertFromString != null)
                    note.EditSize = (System.Drawing.Size)convertFromString;
                note.XFactor = (double)r["REL_X"];
                note.YFactor = (double)r["REL_Y"];
                note.SentTo = (string)r["SENT_TO"];
                note.ReceivedFrom = (string)r["RECEIVED_FROM"];
                note.ReceivedIp = !PNData.IsDBNull(r["RECEIVED_IP"]) ? (string) r["RECEIVED_IP"] : "";
                note.PinClass = (string)r["PIN_CLASS"];
                note.PinText = (string)r["PIN_TEXT"];
                note.Scrambled = !PNData.IsDBNull(r["SCRAMBLED"]) && (bool)r["SCRAMBLED"];
                note.Thumbnail = !PNData.IsDBNull(r["THUMBNAIL"]) && (bool)r["THUMBNAIL"];

                return loadNoteSchedule(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal static bool SaveExistingNote(PNote note)
        {
            try
            {
                var sb = new StringBuilder();
                note.DateSaved = DateTime.Now;

                sb.Append("UPDATE NOTES SET NAME = '");
                sb.Append(note.Name.Replace("'", "''"));
                sb.Append("', GROUP_ID = ");
                sb.Append(note.GroupID);
                sb.Append(", PREV_GROUP_ID = ");
                sb.Append(note.PrevGroupID);
                sb.Append(", DATE_SAVED = '");
                sb.Append(note.DateSaved.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("' WHERE ID = '");
                sb.Append(note.ID);
                sb.Append("'");
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    oData.Execute(sb.ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal static bool SaveNewNote(PNote note)
        {
            try
            {
                var sb = new StringBuilder();
                note.DateSaved = DateTime.Now;

                sb.Append("INSERT INTO NOTES (ID, NAME, GROUP_ID, PREV_GROUP_ID, OPACITY, VISIBLE, FAVORITE, PROTECTED, COMPLETED, PRIORITY, PASSWORD_STRING, PINNED, TOPMOST, ROLLED, DOCK_STATUS, DOCK_ORDER, SEND_RECEIVE_STATUS, DATE_CREATED, DATE_SAVED, DATE_SENT, DATE_RECEIVED, DATE_DELETED, SIZE, LOCATION, EDIT_SIZE, REL_X, REL_Y, SENT_TO, RECEIVED_FROM, PIN_CLASS, PIN_TEXT, SCRAMBLED, RECEIVED_IP) VALUES('");
                sb.Append(note.ID);
                sb.Append("', '");
                sb.Append(note.Name.Replace("'", "''"));
                sb.Append("', ");
                sb.Append(note.GroupID);
                sb.Append(", ");
                sb.Append(note.GroupID);   //as prev png
                sb.Append(", ");
                sb.Append(note.Opacity.ToString(PNStatic.CultureInvariant));
                sb.Append(", ");
                sb.Append(Convert.ToInt32(note.Visible));
                sb.Append(", ");
                sb.Append(Convert.ToInt32(note.Favorite));
                sb.Append(", ");
                sb.Append(Convert.ToInt32(note.Protected));
                sb.Append(", ");
                sb.Append(Convert.ToInt32(note.Completed));
                sb.Append(", ");
                sb.Append(Convert.ToInt32(note.Priority));
                sb.Append(", '");
                sb.Append(note.PasswordString.Replace("'", "''"));
                sb.Append("', ");
                sb.Append(Convert.ToInt32(note.Pinned));
                sb.Append(", ");
                sb.Append(Convert.ToInt32(note.Topmost));
                sb.Append(", ");
                sb.Append(Convert.ToInt32(note.Rolled));
                sb.Append(", ");
                sb.Append(Convert.ToInt32(note.DockStatus));
                sb.Append(", ");
                sb.Append(Convert.ToInt32(note.DockOrder));
                sb.Append(", ");
                sb.Append(Convert.ToInt32(note.SentReceived));
                sb.Append(", '");
                sb.Append(note.DateCreated.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("', '");
                sb.Append(note.DateSaved.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("', '");
                sb.Append(note.DateSent.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("', '");
                sb.Append(note.DateReceived.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("', '");
                sb.Append(note.DateDeleted.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("', '");
                var sc = new SizeConverter();
                sb.Append(sc.ConvertToString(null, PNStatic.CultureInvariant, note.NoteSize));
                sb.Append("', '");
                var pc = new PointConverter();
                sb.Append(pc.ConvertToString(null, PNStatic.CultureInvariant, note.NoteLocation));
                sb.Append("', '");
                var scd = new System.Drawing.SizeConverter();
                sb.Append(scd.ConvertToString(null, PNStatic.CultureInvariant, note.EditSize));
                sb.Append("', ");
                var factors = getRelationalFactors(note);
                sb.Append(factors[0].ToString(PNStatic.CultureInvariant));
                sb.Append(", ");
                sb.Append(factors[1].ToString(PNStatic.CultureInvariant));
                sb.Append(", '");
                sb.Append(note.SentTo.Replace("'", "''"));
                sb.Append("', '");
                sb.Append(note.ReceivedFrom.Replace("'", "''"));
                sb.Append("', '");
                sb.Append(note.PinClass);
                sb.Append("', '");
                sb.Append(note.PinText);
                sb.Append("', ");
                sb.Append(Convert.ToInt32(note.Scrambled));
                sb.Append(", '");
                sb.Append(note.ReceivedIp);
                sb.Append("'");
                sb.Append(")");
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    oData.Execute(sb.ToString());
                }
                note.FromDB = true;
                if (note.Skinless != null)
                {
                    SaveNoteSkinless(note);
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal static NoteDeleteType DeletionWarning(bool leftShiftDown, int notesCount, PNote note)
        {
            try
            {
                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                var dlgResult = MessageBoxResult.Yes;
                NoteDeleteType type;
                var sb = new StringBuilder();

                if (leftShiftDown || (note != null && !note.FromDB))
                {
                    type = NoteDeleteType.Complete;
                    if (PNStatic.Settings.GeneralSettings.ConfirmBeforeDeletion)
                    {
                        if (notesCount == 1)
                        {
                            sb.Append(PNLang.Instance.GetMessageText("delete_completely", "Do you want to completely delete this note?"));
                            if (note != null)
                            {
                                sb.AppendLine();
                                sb.AppendLine();
                                sb.Append(PNLang.Instance.GetMessageText("delete_note", "Note:"));
                                sb.Append(" ");
                                sb.Append(note.Name);
                                sb.AppendLine();
                                sb.Append(PNLang.Instance.GetMessageText("delete_created", "Created:"));
                                sb.Append(" ");
                                sb.Append(note.DateCreated.ToString(PNStatic.Settings.GeneralSettings.DateFormat, ci));
                            }
                        }
                        else
                        {
                            sb.Append(
                                PNLang.Instance.GetMessageText("delete_completely_many",
                                                               "Do you want to completely delete these " +
                                                               PNStrings.PLACEHOLDER1 + " notes?")
                                      .Replace(PNStrings.PLACEHOLDER1, notesCount.ToString(CultureInfo.InvariantCulture)));
                        }
                        dlgResult = PNMessageBox.Show(sb.ToString(), PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    }
                }
                else
                {
                    type = NoteDeleteType.Bin;
                    if (PNStatic.Settings.GeneralSettings.ConfirmBeforeDeletion)
                    {
                        if (notesCount == 1)
                        {
                            sb.Append(PNLang.Instance.GetMessageText("delete_to_bin", "Do you want to send this note to Recycle Bin?"));
                            if (note != null)
                            {
                                sb.AppendLine();
                                sb.AppendLine();
                                sb.Append(PNLang.Instance.GetMessageText("delete_note", "Note:"));
                                sb.Append(" ");
                                sb.Append(note.Name);
                                sb.AppendLine();
                                sb.Append(PNLang.Instance.GetMessageText("delete_created", "Created:"));
                                sb.Append(" ");
                                sb.Append(note.DateCreated.ToString(PNStatic.Settings.GeneralSettings.DateFormat, ci));
                            }
                        }
                        else
                        {
                            sb.Append(
                                PNLang.Instance.GetMessageText("delete_to_bin_many",
                                                               "Do you want to send these " + PNStrings.PLACEHOLDER1 +
                                                               " notes to Recycle Bin?")
                                      .Replace(PNStrings.PLACEHOLDER1, notesCount.ToString(CultureInfo.InvariantCulture)));
                        }
                        dlgResult = PNMessageBox.Show(sb.ToString(), PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    }
                }

                if (dlgResult != MessageBoxResult.Yes)
                {
                    return NoteDeleteType.None;
                }

                return type;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return NoteDeleteType.None;
            }
        }

        internal static bool DeleteNote(NoteDeleteType type, PNote note)
        {
            try
            {
                var result = false;

                if (note.DockStatus != DockStatus.None)
                {
                    PreUndockNote(note);
                }

                RemoveDeletedNoteFromLists(note);

                var id = note.ID;

                switch (type)
                {
                    case NoteDeleteType.Bin:
                        note.GroupID = (int)SpecialGroups.RecycleBin;
                        note.DateDeleted = DateTime.Now;
                        if (note.Visible && note.Dialog.InAlarm)
                        {
                            note.Dialog.InAlarm = false;
                        }
                        note.Visible = false;
                        note.Dialog = null;
                        note.Schedule = new PNNoteSchedule();
                        result = SaveNoteDeletedState(note, true);
                        break;
                    case NoteDeleteType.Complete:
                        var groupID = note.GroupID;
                        result = SaveNoteDeletedState(note, false);
                        DeleteNoteCompletely(note, CompleteDeletionSource.SingleNote);
                        PNStatic.FormMain.RaiseDeletedCompletelyEvent(id, groupID);
                        break;
                }
                if (note.Thumbnail && PNStatic.Settings.Behavior.ShowNotesPanel)
                {
                    PNStatic.FormPanel.RemoveThumbnail(note);
                }
                DeleteNoteShortcut(id);
                return result;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal static void SaveNoteDeletedDate(PNote note)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("UPDATE NOTES SET DATE_DELETED = '");
                sb.Append(note.DateDeleted.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("' WHERE ID = '");
                sb.Append(note.ID);
                sb.Append("'; ");
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    oData.Execute(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static bool SaveNoteDeletedState(PNote note, bool toBin)
        {
            try
            {
                var sb = new StringBuilder();
                if (toBin)
                {
                    sb.Append("UPDATE NOTES SET VISIBLE = 0, GROUP_ID = ");
                    sb.Append(note.GroupID);
                    sb.Append(", PREV_GROUP_ID = ");
                    sb.Append(note.PrevGroupID);
                    sb.Append(", DATE_DELETED = '");
                    sb.Append(note.DateDeleted.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                    sb.Append("' WHERE ID = '");
                    sb.Append(note.ID);
                    sb.Append("'; ");
                }
                else
                {
                    sb.Append("DELETE FROM NOTES WHERE ID = '");
                    sb.Append(note.ID);
                    sb.Append("'; ");
                    //all other tables should be here

                }
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    oData.Execute(sb.ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal static void DeleteNoteCompletely(PNote note, CompleteDeletionSource src)
        {
            try
            {
                var path = Path.Combine(PNPaths.Instance.DataDir, note.ID);
                path += PNStrings.NOTE_EXTENSION;
                if (File.Exists(path))
                {
                    File.Delete(path);
                    PNStatic.LogThis("Complete deletion of note " + path + "; Source: " + src);
                }
                //note.RaiseDeleteCompletelyEvent();
                note.Dialog = null;
                note.Dispose();
                PNStatic.Notes.Remove(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void RemoveDeletedNoteFromLists(PNote note)
        {
            try
            {
                foreach (var n in PNStatic.Notes)
                {
                    n.LinkedNotes.RemoveAll(l => l == note.ID);
                }
                //clean tables
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    var sb = new StringBuilder();
                    sb.Append("DELETE FROM LINKED_NOTES WHERE LINK_ID = '");
                    sb.Append(note.ID);
                    sb.Append("';");
                    oData.Execute(sb.ToString());
                }
                //remove all backup copies
                var di = new DirectoryInfo(PNPaths.Instance.BackupDir);
                var fis = di.GetFiles(note.ID + "*" + PNStrings.NOTE_BACK_EXTENSION);
                foreach (var fi in fis)
                {
                    fi.Delete();
                }
                fis = di.GetFiles(note.ID + "*" + PNStrings.NOTE_AUTO_BACK_EXTENSION);
                foreach (var fi in fis)
                {
                    fi.Delete();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveNoteOnSend(PNote note)
        {
            try
            {
                var sb = new StringBuilder();
                note.DateSaved = DateTime.Now;

                sb.Append("UPDATE NOTES SET DATE_SENT = '");
                sb.Append(note.DateSaved.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("', SEND_RECEIVE_STATUS = ");
                sb.Append(Convert.ToInt32(note.SentReceived));
                sb.Append(", SENT_TO = '");
                sb.Append(note.SentTo.Replace("'", "''"));
                sb.Append("' WHERE ID = '");
                sb.Append(note.ID);
                sb.Append("'");
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    oData.Execute(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveNoteDockStatus(PNote note)
        {
            try
            {
                var sb = new StringBuilder();
                note.DateSaved = DateTime.Now;

                sb.Append("UPDATE NOTES SET DOCK_STATUS = ");
                sb.Append(Convert.ToInt32(note.DockStatus));
                sb.Append(", DOCK_ORDER = ");
                sb.Append(Convert.ToInt32(note.DockOrder));
                sb.Append(" WHERE ID = '");
                sb.Append(note.ID);
                sb.Append("'");
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    oData.Execute(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void DeleteNoteSchedule(PNote note)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("DELETE FROM NOTES_SCHEDULE WHERE NOTE_ID = '");
                sb.Append(note.ID);
                sb.Append("'");
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    oData.Execute(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveNoteSchedule(PNote note)
        {
            try
            {
                var ac = new AlarmAfterValuesConverter();
                var mc = new MonthDayConverter();
                var dc = new DaysOfWeekConverter();
                var sb = new StringBuilder();
                DeleteNoteSchedule(note);
                sb.Append("INSERT INTO NOTES_SCHEDULE (NOTE_ID,SCHEDULE_TYPE,ALARM_DATE,START_DATE,LAST_RUN,SOUND,STOP_AFTER,TRACK,REPEAT_COUNT,SOUND_IN_LOOP,USE_TTS,START_FROM,MONTH_DAY,ALARM_AFTER,WEEKDAYS,PROG_TO_RUN,CLOSE_ON_NOTIFICATION,MULTI_ALERTS,TIME_ZONE) VALUES('");
                sb.Append(note.ID);
                sb.Append("',");
                sb.Append(Convert.ToInt32(note.Schedule.Type));
                sb.Append(",'");
                sb.Append(note.Schedule.AlarmDate.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("','");
                sb.Append(note.Schedule.StartDate.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("','");
                sb.Append(note.Schedule.LastRun.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("','");
                sb.Append(note.Schedule.Sound);
                sb.Append("',");
                sb.Append(note.Schedule.StopAfter);
                sb.Append(",");
                sb.Append(Convert.ToInt32(note.Schedule.Track));
                sb.Append(",");
                sb.Append(note.Schedule.RepeatCount);
                sb.Append(",");
                sb.Append(Convert.ToInt32(note.Schedule.SoundInLoop));
                sb.Append(",");
                sb.Append(Convert.ToInt32(note.Schedule.UseTts));
                sb.Append(",");
                sb.Append(Convert.ToInt32(note.Schedule.StartFrom));
                sb.Append(",'");
                sb.Append(mc.ConvertToString(note.Schedule.MonthDay));
                sb.Append("','");
                sb.Append(ac.ConvertToString(note.Schedule.AlarmAfter));
                sb.Append("','");
                sb.Append(dc.ConvertToString(note.Schedule.Weekdays));
                sb.Append("','");
                sb.Append(note.Schedule.ProgramToRunOnAlert);
                sb.Append("',");
                sb.Append(Convert.ToInt32(note.Schedule.CloseOnNotification));
                //MULTI_ALERTS
                if (note.Schedule.MultiAlerts.Any())
                {
                    var temp = new StringBuilder();
                    foreach (var ma in note.Schedule.MultiAlerts)
                    {
                        temp.Append(ma.Raised);
                        temp.Append("|");
                        temp.Append(ma.Date.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                        temp.Append("^");
                    }
                    sb.Append(",'");
                    sb.Append(temp);
                    sb.Append("'");
                }
                else
                {
                    sb.Append(",NULL");
                }
                //TIME_ZONE
                if (note.Schedule.TimeZone != null)
                {
                    sb.Append(",'");
                    sb.Append(note.Schedule.TimeZone.ToSerializedString());
                    sb.Append("'");
                }
                else
                {
                    sb.Append(",NULL");
                }
                sb.Append(")");
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    oData.Execute(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplyThumbnails()
        {
            try
            {
                foreach (var note in PNStatic.Notes.Where(note => note.Visible && !note.Thumbnail))
                {
                    note.Dialog.SetThumbnail();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplyDocking(DockStatus status)
        {
            try
            {
                foreach (var note in PNStatic.Notes.Where(note => note.Visible && note.DockStatus != status))
                {
                    if (status == DockStatus.None)
                    {
                        note.Dialog.UndockNote(note);
                    }
                    else
                    {
                        note.Dialog.DockNote(note, status, false);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void AdjustNoteSchedule(PNote note, Window owner)
        {
            try
            {
                var dlgSchedule = new WndAdjustSchedule(note) { Owner = owner };
                dlgSchedule.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static string GetNoteImage(PNote note)
        {
            try
            {

                if (note.GroupID == (int)SpecialGroups.RecycleBin)
                {
                    return "lsdel";
                    //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsdel.png"));
                }
                if (note.Visible)
                {
                    if (note.FromDB)
                    {
                        if (note.Schedule.Type != ScheduleType.None && note.Changed)
                        {
                            return "lsvord_change_sch";
                            //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvord_change_sch.png"));
                        }
                        if (note.Schedule.Type != ScheduleType.None)
                        {
                            return "lsvord_sch";
                            //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvord_sch.png"));
                        }
                        return note.Changed
                            ? "lsvord_change"
                            //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvord_change.png"))
                            : "lsvord";
                        //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvord.png"));
                    }
                    if (note.Schedule.Type != ScheduleType.None && note.Changed)
                    {
                        return "lsvnew_change_sch";
                        //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvnew_change_sch.png"));
                    }
                    if (note.Schedule.Type != ScheduleType.None)
                    {
                        return "lsvnew_sch";
                        //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvnew_sch.png"));
                    }
                    return note.Changed
                        ? "lsvnew_change"
                        : "lsvnew";
                    //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvnew_change.png"))

                    //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvnew.png"));
                }
                if (note.FromDB)
                {
                    if (note.Schedule.Type != ScheduleType.None && note.Changed)
                    {
                        return "lshord_change_sch";
                        //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lshord_change_sch.png"));
                    }
                    if (note.Schedule.Type != ScheduleType.None)
                    {
                        return "lshord_sch";
                        //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lshord_sch.png"));
                    }
                    return note.Changed
                        ? "lshord_change"
                        //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lshord_change.png"))
                        : "lshord";
                    //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lshord.png"));
                }
                if (note.Schedule.Type != ScheduleType.None && note.Changed)
                {
                    return "lshnew_change_sch";
                    //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lshnew_change_sch.png"));
                }
                if (note.Schedule.Type != ScheduleType.None)
                {
                    return "lshnew_sch";
                    //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lshnew_sch.png"));
                }
                return note.Changed
                    ? "lshnew_change"
                    //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lshnew_change.png"))
                    : "lshnew";
                //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lshnew.png"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "lsvord";
                //return new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "lsvord.png"));
            }
        }

        internal static int GetNoteIcon(PNote note)
        {
            try
            {
                if (note.GroupID == (int)SpecialGroups.RecycleBin)
                {
                    return 16;
                }
                if (note.Visible)
                {
                    if (note.FromDB)
                    {
                        if (note.Schedule.Type != ScheduleType.None && note.Changed)
                        {
                            return 3;
                        }
                        if (note.Schedule.Type != ScheduleType.None)
                        {
                            return 2;
                        }
                        if (note.Changed)
                        {
                            return 1;
                        }
                        return 0;
                    }
                    if (note.Schedule.Type != ScheduleType.None && note.Changed)
                    {
                        return 7;
                    }
                    if (note.Schedule.Type != ScheduleType.None)
                    {
                        return 6;
                    }
                    if (note.Changed)
                    {
                        return 5;
                    }
                    return 4;
                }
                if (note.FromDB)
                {
                    if (note.Schedule.Type != ScheduleType.None && note.Changed)
                    {
                        return 11;
                    }
                    if (note.Schedule.Type != ScheduleType.None)
                    {
                        return 10;
                    }
                    if (note.Changed)
                    {
                        return 9;
                    }
                    return 8;
                }
                if (note.Schedule.Type != ScheduleType.None && note.Changed)
                {
                    return 15;
                }
                if (note.Schedule.Type != ScheduleType.None)
                {
                    return 14;
                }
                if (note.Changed)
                {
                    return 13;
                }
                return 12;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return 0;
            }
        }

        internal static void LoadBackupCopy(PNote note, string fileBackUp)
        {
            try
            {
                var path = Path.Combine(PNPaths.Instance.BackupDir, fileBackUp);
                if (File.Exists(path))
                {
                    if (!note.Visible)
                    {
                        if (ShowHideSpecificNote(note, true) != ShowHideResult.Success)
                            return;
                    }
                    LoadNoteFile(note.Dialog.Edit, path);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void LoadBackupCopy(PNote note, Window owner)
        {
            try
            {
                var di = new DirectoryInfo(PNPaths.Instance.BackupDir);
                var fis = di.GetFiles(note.ID + "*" + PNStrings.NOTE_BACK_EXTENSION);
                var lenght = fis.Length;
                if (lenght == 0)
                {
                    var message = PNLang.Instance.GetMessageText("no_backup_copies", "There are no backup copies for current note");
                    PNMessageBox.Show(message, PNStrings.PROG_NAME);
                    return;
                }
                var ofn = new OpenFileDialog
                {
                    Title = PNLang.Instance.GetCaptionText("restore_note", "Restore note") + @" [" + note.Name + @"]"
                };
                var filter = PNLang.Instance.GetCaptionText("restore_filter", "Backup copies of notes");
                filter += "(" + note.ID + "*" + PNStrings.NOTE_BACK_EXTENSION + ")|" + note.ID + "*" + PNStrings.NOTE_BACK_EXTENSION;
                ofn.Filter = filter;
                ofn.RestoreDirectory = true;
                ofn.InitialDirectory = PNPaths.Instance.BackupDir;
                if (!ofn.ShowDialog(owner).Value) return;
                LoadBackupFile(note, ofn.FileName);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void LoadBackupFile(PNote note, string fileName)
        {
            try
            {
                if (!note.Visible)
                {
                    if (ShowHideSpecificNote(note, true) != ShowHideResult.Success)
                        return;
                }
                LoadNoteFile(note.Dialog.Edit, fileName);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveNoteAsTextFile(PNote note, Window owner)
        {
            try
            {
                if (PNStatic.Settings.Protection.PromptForPassword)
                    if (!LogIntoNoteOrGroup(note))
                        return;
                var sfd = new SaveFileDialog
                {
                    Title =
                        PNLang.Instance.GetCaptionText("save_as_text", "Save note as text file") + @" [" + note.Name +
                        @"]",
                    Filter = PNLang.Instance.GetCaptionText("save_as_filter", "Text files (*.txt)|*.txt"),
                    RestoreDirectory = true,
                    AddExtension = true
                };
                if (sfd.ShowDialog(owner).Value)
                {
                    using (var sw = new StreamWriter(sfd.FileName))
                    {
                        if (note.Visible)
                        {
                            sw.Write(note.Dialog.Edit.Text);
                        }
                        else
                        {
                            var rtb = new PNRichEditBox();
                            var path = Path.Combine(PNPaths.Instance.DataDir, note.ID) + PNStrings.NOTE_EXTENSION;
                            LoadNoteFile(rtb, path);
                            sw.Write(rtb.Text);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SendNotesAsAttachments(List<string> files)
        {
            try
            {
                var profile = PNStatic.SmtpProfiles.FirstOrDefault(c => c.Active);
                if (profile == null)
                {
                    var mailMessage = new MapiMailMessage();
                    foreach (var f in files)
                    {
                        mailMessage.Files.Add(f);
                    }
                    mailMessage.Subject = PNLang.Instance.GetMessageText("mail_subject_attachment", "Sent from PNotes.");
                    mailMessage.ShowDialog();
                }
                else
                {
                    var dlgSend = new WndSendSmtp(profile, null, null, files);
                    dlgSend.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SendNoteAsText(PNote note, string recipients = "", string linkToRemove = "")
        {
            try
            {
                var text = "";
                if (note.Visible)
                {
                    text = note.Dialog.Edit.Text;
                }
                else
                {
                    var path = Path.Combine(PNPaths.Instance.DataDir, note.ID + PNStrings.NOTE_EXTENSION);
                    if (File.Exists(path))
                    {
                        var rtb = new RichTextBox();
                        LoadNoteFile(rtb, path);
                        text = rtb.Text;
                    }
                }

                //remove possible link
                if (!string.IsNullOrEmpty(linkToRemove))
                {
                    text = text.Replace(linkToRemove, "");
                }

                var profile = PNStatic.SmtpProfiles.FirstOrDefault(c => c.Active);
                if (profile == null)
                {
                    text = text.Replace("\n", "%0D%0A");
                    text = text.Replace("\r", "%0D%0A");
                    text = text.Replace("\"", "%22");
                    text = text.Replace("&", "%26");
                    var mailMessage = new MapiMailMessage
                    {
                        Subject = PNLang.Instance.GetMessageText("mail_subject_text", "Sent from PNotes. Note name:") +
                                  @" " + note.Name,
                        Body = text
                    };
                    if(!string.IsNullOrEmpty(recipients))
                    {
                        var recips = recipients.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var r in recips)
                        {
                            mailMessage.Recipients.Add(r);
                        }
                    }
                    mailMessage.ShowDialog();
                }
                else
                {
                    var dlgSend = new WndSendSmtp(profile, note.Name, recipients, text, null);
                    dlgSend.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void CreateOrShowTodayDiary()
        {
            try
            {
                var note = PNStatic.Notes.FirstOrDefault(n => n.GroupID == (int)SpecialGroups.Diary && n.DateCreated.Date == DateTime.Today);
                if (note != null)
                {
                    ShowHideSpecificNote(note, true);
                }
                else
                {
                    note =
                        PNStatic.Notes.FirstOrDefault(
                            n =>
                            n.GroupID == (int)SpecialGroups.RecycleBin && n.PrevGroupID == (int)SpecialGroups.Diary &&
                            n.DateCreated.Date == DateTime.Today);
                    if (note != null)
                    {
                        note.GroupID = PNStatic.Groups.Any(g => g.ID == note.PrevGroupID) ? note.PrevGroupID : 0;
                        SaveGroupChange(note);
                        ShowHideSpecificNote(note, true);
                        if (PNStatic.FormCP != null)
                        {
                            PNStatic.FormCP.DiaryRestored();
                        }
                    }
                    else
                    {
                        PNStatic.FormMain.ApplyAction(MainDialogAction.NewNoteInGroup, (int)SpecialGroups.Diary);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveNoteFile(RichTextBox edit, string path)
        {
            try
            {
                edit.SaveFile(path, RichTextBoxStreamType.RichText);
                if (PNStatic.Settings.Protection.PasswordString.Length > 0 && PNStatic.Settings.Protection.StoreAsEncrypted)
                {
                    using (var pne = new PNEncryptor(PNStatic.Settings.Protection.PasswordString))
                    {
                        pne.EncryptTextFile(path);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void LoadNoteFile(System.Windows.Controls.RichTextBox edit, string originalPath)
        {
            try
            {
                if (PNStatic.Settings.Protection.PasswordString.Length == 0 || !PNStatic.Settings.Protection.StoreAsEncrypted)
                {
                    var textRange = new TextRange(edit.Document.ContentStart, edit.Document.ContentEnd);
                    using (var fileStream = new FileStream(originalPath, FileMode.OpenOrCreate))
                    {
                        textRange.Load(fileStream, DataFormats.Rtf);
                    }
                }
                else
                {
                    if (originalPath != null)
                    {
                        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(originalPath));
                        File.Copy(originalPath, tempPath, true);
                        using (var pne = new PNEncryptor(PNStatic.Settings.Protection.PasswordString))
                        {
                            pne.DecryptTextFile(tempPath);
                        }
                        var textRange = new TextRange(edit.Document.ContentStart, edit.Document.ContentEnd);
                        using (var fileStream = new FileStream(tempPath, FileMode.OpenOrCreate))
                        {
                            textRange.Load(fileStream, DataFormats.Rtf);
                        }
                        File.Delete(tempPath);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void LoadNoteFile(RichTextBox edit, string originalPath)
        {
            try
            {
                if (PNStatic.Settings.Protection.PasswordString.Length == 0 || !PNStatic.Settings.Protection.StoreAsEncrypted)
                {
                    edit.LoadFile(originalPath, RichTextBoxStreamType.RichText);
                }
                else
                {
                    if (originalPath != null)
                    {
                        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(originalPath));
                        File.Copy(originalPath, tempPath, true);
                        using (var pne = new PNEncryptor(PNStatic.Settings.Protection.PasswordString))
                        {
                            pne.DecryptTextFile(tempPath);
                        }
                        edit.LoadFile(tempPath, RichTextBoxStreamType.RichText);
                        File.Delete(tempPath);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void EncryptAllNotes(string passwordString)
        {
            try
            {
                var files = new DirectoryInfo(PNPaths.Instance.DataDir).GetFiles("*" + PNStrings.NOTE_EXTENSION);
                using (var pne = new PNEncryptor(passwordString))
                {
                    foreach (var f in files)
                    {
                        pne.EncryptTextFile(f.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void DecryptAllNotes(string passwordString)
        {
            try
            {
                var files = new DirectoryInfo(PNPaths.Instance.DataDir).GetFiles("*" + PNStrings.NOTE_EXTENSION);
                using (var pne = new PNEncryptor(passwordString))
                {
                    foreach (var f in files)
                    {
                        pne.DecryptTextFile(f.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void PreUndockNote(PNote note)
        {
            try
            {
                var prevStatus = note.DockStatus;
                note.DockStatus = DockStatus.None;
                note.DockOrder = -1;
                ShiftPreviousDock(note, prevStatus, true);

                if (note.FromDB)
                {
                    SaveNoteDockStatus(note);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void RelocateAllNotesOnScreenPlug()
        {
            try
            {
                var size = PNStatic.AllScreensSize();
                var rect = PNStatic.AllScreensBounds();
                if (PNStatic.Settings.Behavior.RelationalPositioning)
                {
                    foreach (var note in PNStatic.Notes.Where(n => n.Visible && n.Dialog != null && n.DockStatus == DockStatus.None && !rect.Contains(n.Dialog.GetLocation())))
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
                        SaveNoteLocation(note, note.Dialog.GetLocation());
                    }
                }
                else
                {
                    foreach (var note in PNStatic.Notes.Where(n => n.Visible && n.Dialog != null && n.DockStatus == DockStatus.None && !rect.Contains(n.Dialog.GetLocation())))
                    {
                        note.Dialog.SetLocation(new Point((size.Width - note.Dialog.Width) / 2,
                            (size.Height - note.Dialog.Height) / 2));
                        SaveNoteLocation(note, note.Dialog.GetLocation());
                    }
                }

                //re-dock all docked notes
                RedockNotes();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void RedockNotes()
        {
            try
            {
                var wa = PNStatic.AllScreensBounds();
                foreach (var note in PNStatic.Notes.Where(n => n.Visible && n.Dialog != null && n.DockStatus != DockStatus.None))
                {
                    var multiplier = PNStatic.DockedNotes[note.DockStatus].Count(n => n.DockOrder < note.DockOrder);
                    int w;
                    int h;
                    if (!PNStatic.Settings.GeneralSettings.UseSkins)
                    {
                        w = PNStatic.Settings.GeneralSettings.DockWidth;
                        h = PNStatic.Settings.GeneralSettings.DockHeight;
                    }
                    else
                    {
                        w = PNStatic.Docking.Skin.BitmapSkin.Width;
                        h = PNStatic.Docking.Skin.BitmapSkin.Height;
                    }
                    switch (note.DockStatus)
                    {
                        case DockStatus.Left:
                            note.Dialog.SetLocation(new Point(wa.Left, wa.Top + multiplier * h));
                            break;
                        case DockStatus.Top:
                            note.Dialog.SetLocation(new Point(wa.Left + multiplier * w, wa.Top));
                            break;
                        case DockStatus.Right:
                            note.Dialog.SetLocation(new Point(wa.Right - w, wa.Top + multiplier * h));
                            break;
                        case DockStatus.Bottom:
                            note.Dialog.SetLocation(new Point(wa.Left + multiplier * w, wa.Bottom - h));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void CentralizeNotes(IEnumerable<PNote> notes)
        {
            try
            {
                var size = PNStatic.AllScreensSize();
                foreach (var note in notes)
                {
                    if (note.Visible && note.Dialog != null)
                    {
                        note.Dialog.SetLocation(new Point((size.Width - note.Dialog.Width) / 2,
                            (size.Height - note.Dialog.Height) / 2));
                        SaveNoteLocation(note, note.Dialog.GetLocation());
                    }
                    else
                    {
                        note.NoteLocation = new Point((size.Width - note.NoteSize.Width) / 2,
                            (size.Height - note.NoteSize.Height) / 2);
                        SaveNoteLocation(note, note.NoteLocation);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ChangeDockSize()
        {
            try
            {
                var index = 0;
                double top = 0, offset = 0;

                var docked = PNStatic.DockedNotes[DockStatus.Left].OrderBy(n => n.DockOrder);
                foreach (var n in docked)
                {
                    n.Dialog.SetSize(PNStatic.Settings.GeneralSettings.DockWidth,
                        PNStatic.Settings.GeneralSettings.DockHeight);
                    if (index > 0)
                    {
                        n.Dialog.Top = top;
                    }
                    top = n.Dialog.Top + n.Dialog.Height;
                    index++;
                }
                index = 0;
                top = 0;
                docked = PNStatic.DockedNotes[DockStatus.Right].OrderBy(n => n.DockOrder);
                foreach (var n in docked)
                {
                    if (index == 0)
                        offset = n.Dialog.Width - PNStatic.Settings.GeneralSettings.DockWidth;
                    n.Dialog.SetSize(PNStatic.Settings.GeneralSettings.DockWidth,
                        PNStatic.Settings.GeneralSettings.DockHeight);
                    n.Dialog.SetLocation(index > 0
                        ? new Point(n.Dialog.Left + offset, top)
                        : new Point(n.Dialog.Left + offset, n.Dialog.Top));
                    top = n.Dialog.Top + n.Dialog.Height;
                    index++;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ShiftPreviousDock(PNote note, DockStatus prevStatus, bool reorder)
        {
            try
            {
                int w, h;

                if (!PNStatic.Settings.GeneralSettings.UseSkins)
                {
                    w = PNStatic.Settings.GeneralSettings.DockWidth;
                    h = PNStatic.Settings.GeneralSettings.DockHeight;
                }
                else
                {
                    w = PNStatic.Docking.Skin.BitmapSkin.Width;
                    h = PNStatic.Docking.Skin.BitmapSkin.Height;
                }
                IEnumerable<PNote> notes;

                if (prevStatus != DockStatus.None)
                {
                    PNStatic.DockedNotes[prevStatus].Remove(note);
                }

                switch (prevStatus)
                {
                    case DockStatus.Left:
                    case DockStatus.Right:
                        notes = PNStatic.Notes.Where(n => n.DockStatus == prevStatus && n.Dialog.Top > note.Dialog.Top);
                        foreach (var n in notes)
                        {
                            n.Dialog.Top -= h;
                            if (reorder)
                            {
                                n.DockOrder -= 1;
                                SaveNoteDockStatus(n);
                            }
                        }
                        break;
                    case DockStatus.Top:
                    case DockStatus.Bottom:
                        notes = PNStatic.Notes.Where(n => n.DockStatus == prevStatus && n.Dialog.Left > note.Dialog.Left);
                        foreach (var n in notes)
                        {
                            n.Dialog.Left -= w;
                            if (reorder)
                            {
                                n.DockOrder -= 1;
                                SaveNoteDockStatus(n);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ShiftDockLeft(DockArrow arrow)
        {
            try
            {
                List<PNote> notes = null;
                if (arrow == DockArrow.TopRight)
                {
                    notes = PNStatic.DockedNotes[DockStatus.Top];
                }
                else if (arrow == DockArrow.BottomRight)
                {
                    notes = PNStatic.DockedNotes[DockStatus.Bottom];
                }
                if (notes != null)
                {
                    foreach (var n in notes)
                    {
                        n.Dialog.Left -= n.Dialog.Width;
                    }
                }
                var dar = PNStatic.DockArrows[arrow];
                SetDockArrowVisibility(arrow, dar);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ShiftDockRight(DockArrow arrow)
        {
            try
            {
                List<PNote> notes = null;
                if (arrow == DockArrow.TopLeft)
                {
                    notes = PNStatic.DockedNotes[DockStatus.Top];
                }
                else if (arrow == DockArrow.BottomLeft)
                {
                    notes = PNStatic.DockedNotes[DockStatus.Bottom];
                }
                if (notes != null)
                {
                    foreach (var n in notes)
                    {
                        n.Dialog.Left += n.Dialog.Width;
                    }
                }
                var dar = PNStatic.DockArrows[arrow];
                SetDockArrowVisibility(arrow, dar);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ShiftDockUp(DockArrow arrow)
        {
            try
            {
                List<PNote> notes = null;
                if (arrow == DockArrow.LeftDown)
                {
                    notes = PNStatic.DockedNotes[DockStatus.Left];
                }
                else if (arrow == DockArrow.RightDown)
                {
                    notes = PNStatic.DockedNotes[DockStatus.Right];
                }
                if (notes != null)
                {
                    foreach (var n in notes)
                    {
                        n.Dialog.Top -= n.Dialog.Height;
                    }
                }
                var dar = PNStatic.DockArrows[arrow];
                SetDockArrowVisibility(arrow, dar);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ShiftDockDown(DockArrow arrow)
        {
            try
            {
                List<PNote> notes = null;

                if (arrow == DockArrow.LeftUp)
                {
                    notes = PNStatic.DockedNotes[DockStatus.Left];
                }
                else if (arrow == DockArrow.RightUp)
                {
                    notes = PNStatic.DockedNotes[DockStatus.Right];
                }
                if (notes != null)
                {
                    foreach (var n in notes)
                    {
                        n.Dialog.Top += n.Dialog.Height;
                    }
                }
                var dar = PNStatic.DockArrows[arrow];
                SetDockArrowVisibility(arrow, dar);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SetDockArrowVisibility(DockArrow arrow, WndArrow dar)
        {
            try
            {
                var wa = Screen.GetWorkingArea(new System.Drawing.Point(0, 0));
                switch (arrow)
                {
                    case DockArrow.LeftUp:
                        if (!PNStatic.DockedNotes[DockStatus.Left].Any(n => n.Dialog.Top < wa.Top))
                        {
                            dar.Hide();
                        }
                        break;
                    case DockArrow.LeftDown:
                        if (!PNStatic.DockedNotes[DockStatus.Left].Any(n => (n.Dialog.Top + n.Dialog.Height) > wa.Bottom))
                        {
                            dar.Hide();
                        }
                        break;
                    case DockArrow.TopLeft:
                        if (!PNStatic.DockedNotes[DockStatus.Top].Any(n => n.Dialog.Left < wa.Left))
                        {
                            dar.Hide();
                        }
                        break;
                    case DockArrow.TopRight:
                        if (!PNStatic.DockedNotes[DockStatus.Top].Any(n => (n.Dialog.Left + n.Dialog.Width) > wa.Right))
                        {
                            dar.Hide();
                        }
                        break;
                    case DockArrow.RightUp:
                        if (!PNStatic.DockedNotes[DockStatus.Right].Any(n => n.Dialog.Top < wa.Top))
                        {
                            dar.Hide();
                        }
                        break;
                    case DockArrow.RightDown:
                        if (!PNStatic.DockedNotes[DockStatus.Right].Any(n => (n.Dialog.Top + n.Dialog.Height) > wa.Bottom))
                        {
                            dar.Hide();
                        }
                        break;
                    case DockArrow.BottomLeft:
                        if (!PNStatic.DockedNotes[DockStatus.Bottom].Any(n => n.Dialog.Left < wa.Left))
                        {
                            dar.Hide();
                        }
                        break;
                    case DockArrow.BottomRight:
                        if (!PNStatic.DockedNotes[DockStatus.Bottom].Any(n => (n.Dialog.Left + n.Dialog.Width) > wa.Right))
                        {
                            dar.Hide();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static Color GetItemBackColor(string id)
        {
            var color = SystemColors.WindowColor;
            try
            {
                var note = PNStatic.Notes.Note(id);
                if (note != null)
                {
                    if (note.Skinless != null)
                    {
                        color = note.Skinless.BackColor;
                    }
                    else
                    {
                        var groupID = note.GroupID != (int)SpecialGroups.RecycleBin ? note.GroupID : note.PrevGroupID;
                        var group = PNStatic.Groups.GetGroupByID(groupID);
                        if (group != null)
                        {
                            color = group.Skinless.BackColor;
                        }
                    }
                }
                return color;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return color;
            }
        }
    }
}
