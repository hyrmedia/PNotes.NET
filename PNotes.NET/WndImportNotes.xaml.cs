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

using Microsoft.Win32;
using PNStaticFonts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndImportNotes.xaml
    /// </summary>
    public partial class WndImportNotes
    {
        public WndImportNotes()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
            _Loaded = true;
        }

        private const int NB_HIGH_PRIORITY = 1;
        private const int NB_PROTECTED = 2;
        private const int NB_NOT_TRACK = 3;
        private const int NB_COMPLETED = 4;
        private const int SP_SOUND_IN_LOOP = 0;
        private const int SP_USE_TTS = 2;
        private const int START_PROG = 200;
        //private const int GROUP_RECYCLE = -2;
        private const int GROUP_INCOMING = -6;
        private const int F_SKIN = 2;
        private const int F_C_FONT = 16;
        private const int F_C_COLOR = 32;
        private const int F_B_COLOR = 64;

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct RECTINT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Unicode)]
        private struct NOTE_DATA
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szName;
            public SYSTEMTIME stChanged;
            public bool onTop;
            public int idGroup;
            public int dockData;
            public bool visible;
            public RECTINT rcp;
            public bool prevOnTop;
            public bool rolled;
            public int res1;
            public int idPrevGroup;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Unicode)]
        private struct SCHEDULE_TYPE
        {
            public short scType;
            public SYSTEMTIME scDate;
            public SYSTEMTIME scStart;
            public SYSTEMTIME scLastRun;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szSound;
            public int parameters;
            public short stopLoop;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct NOTE_REL_POSITION
        {
            public double left;
            public double top;
            public int width;
            public int height;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct ADDITIONAL_NAPP
        {
            public byte transValue;
            public byte addResByte;
            public int addRes1;
            public int addRes2;
        }

        private const int LF_FACESIZE = 32;
        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        private struct LOGFONTW
        {
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LF_FACESIZE)]
            public string lfFaceName;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        private struct NOTE_APPEARANCE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szSkin;
            public LOGFONTW lfFont;
            public int crFont;
            public LOGFONTW lfCaption;
            public int crCaption;
            public int crWindow;
            public bool fFontSet;
            public int nPrivate;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        private struct NOTE_PIN
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PNInterop.MAX_PATH)]
            public string text;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PNInterop.MAX_PATH)]
            public string className;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        struct HK_TYPE
        {
            public int id;
            public int identifier;
            public uint fsModifiers;
            public uint vk;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
            public string szKey;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        struct PNGROUP
        {
            public int id;
            public int parent;
            public int image;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szName;
            public IntPtr next;
            public HK_TYPE hotKeyShow;
            public HK_TYPE hotKeyHide;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szLock;
            public int crWindow;
            public int crCaption;
            public int crFont;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szSkin;
            public bool customCRWindow;
            public bool customCRCaption;
            public bool customCRFont;
            public bool customSkin;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        private struct SEND_REC_STATUS
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string sentTo;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string recFrom;
            public SYSTEMTIME lastSent;
            public SYSTEMTIME lastRec;
        }

        private readonly Dictionary<DockStatus, int> m_DockedNotes = new Dictionary<DockStatus, int>();
        private readonly bool _Loaded;

        private void DlgImportNotes_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                lblWarning.Text = PNLang.Instance.GetCaptionText("import_warning_1", "Before you proceed:");
                lblWarning.Text += '\n' + PNLang.Instance.GetCaptionText("import_warning_2", "1.   Decrypt all encrypted notes in old version of PNotes");
                lblWarning.Text += '\n' + PNLang.Instance.GetCaptionText("import_warning_3", "2.   Remove password protection from all protected notes in old version of PNotes");
                lblWarning.Text += '\n' + @"                ";
                m_DockedNotes.Add(DockStatus.Left, 0);
                m_DockedNotes.Add(DockStatus.Top, 0);
                m_DockedNotes.Add(DockStatus.Right, 0);
                m_DockedNotes.Add(DockStatus.Bottom, 0);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableOk();
        }

        private void cmdDataDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fbd = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = PNLang.Instance.GetCaptionText("choose_dir", "Choose directory")
                };
                if (txtDataDir.Text.Trim().Length > 0)
                    fbd.SelectedPath = txtDataDir.Text.Trim();
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtDataDir.Text = fbd.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDBDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fbd = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = PNLang.Instance.GetCaptionText("choose_dir", "Choose directory")
                };
                if (txtDBDir.Text.Trim().Length > 0)
                    fbd.SelectedPath = txtDBDir.Text.Trim();
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtDBDir.Text = fbd.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void chkUseDataDir_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                enableOk();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdIniPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    Title = PNLang.Instance.GetCaptionText("ini_file_text", "PNotes initialization file"),
                    Filter = PNLang.Instance.GetCaptionText("ini_file_filter",
                        "PNotes initialization file (notes.ini)|notes.ini")
                };
                if (ofd.ShowDialog(this).Value)
                {
                    txtIniPath.Text = ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkImportGroups.IsChecked != null && chkImportGroups.IsChecked.Value)
                {
                    if (string.IsNullOrEmpty(txtIniPath.Text))
                    {
                        PNMessageBox.Show(
                            PNLang.Instance.GetMessageText("ini_file_path_required",
                                                           "Initialization file (notes.ini) path is required"),
                            PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                }
                Cursor = Cursors.Wait;
                lblProgress.Text = PNLang.Instance.GetCaptionText("retrievivg_data", "Retrieving data");
                lblProgress.Visibility = elpProgress.Visibility = Visibility.Visible;

                var dict = new Dictionary<int, int>();
                if (chkImportGroups.IsChecked != null && chkImportGroups.IsChecked.Value)
                {
                    dict = importGroups();
                    if (dict == null) return;
                }
                importNotes(dict);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void enableOk()
        {
            try
            {
                if (chkUseDataDir.IsChecked != null && chkUseDataDir.IsChecked.Value)
                {
                    cmdOK.IsEnabled = txtDataDir.Text.Trim().Length > 0;
                }
                else
                {
                    cmdOK.IsEnabled = txtDataDir.Text.Trim().Length > 0 && txtDBDir.Text.Trim().Length > 0;
                }
                if (chkImportGroups.IsChecked != null && chkImportGroups.IsChecked.Value)
                {
                    cmdOK.IsEnabled &= txtIniPath.Text.Trim().Length > 0;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private short loWord(int dword)
        {
            return (short)(dword & 0xFFFF);
        }

        private void importNotes(Dictionary<int, int> groupIds)
        {
            try
            {
                var listID = new List<string>();
                var size = 1024;

                var dbPath =
                    Path.Combine(
                        chkUseDataDir.IsChecked != null && chkUseDataDir.IsChecked.Value
                            ? txtDataDir.Text
                            : txtDBDir.Text, "notes.pnid");
                // get all notes ids
                var buffer = new string(' ', size);
                while (PNInterop.GetPrivateProfileString(null, null, null, buffer, size, dbPath) == size - 2)
                {
                    // loop until sufficient buffer size
                    size *= 2;
                    buffer = new string(' ', size);
                }
                // section names are delimeted by '\0' character with additional '\0' character at the end
                var names = buffer.ToString(PNStatic.CultureInvariant).Split('\0');
                var sections = names.Where(n => n.Trim().Length > 0);
                // interate through all notes ids
                foreach (var id in sections)
                {
                    // get names of all keys under the specified id
                    size = 1024;
                    buffer = new string(' ', size);
                    while (PNInterop.GetPrivateProfileString(id, null, null, buffer, size, dbPath) == size - 2)
                    {
                        // loop until sufficient buffer size
                        size *= 2;
                        buffer = new string(' ', size);
                    }
                    // key names are delimeted by '\0' character with additional '\0' character at the end
                    names = buffer.ToString(CultureInfo.InvariantCulture).Split('\0');
                    var keys = names.Where(n => n.Trim().Length > 0);
                    var enumerable = keys as string[] ?? keys.ToArray();
                    if (enumerable.Contains("data"))
                    {
                        // only if there is 'data' key
                        var data = new NOTE_DATA();
                        var schedule = new SCHEDULE_TYPE();
                        var relPos = new NOTE_REL_POSITION();
                        var addApp = new ADDITIONAL_NAPP();
                        var creation = new SYSTEMTIME();
                        var deletion = new SYSTEMTIME();
                        var appearance = new NOTE_APPEARANCE();
                        var pin = new NOTE_PIN();
                        var sendrec = new SEND_REC_STATUS();
                        var password = new StringBuilder(256);
                        var tags = new StringBuilder(1024);
                        var links = new StringBuilder(1024);
                        // iterate through all keys
                        foreach (var key in enumerable)
                        {
                            switch (key)
                            {
                                case "data":
                                    data = PNInterop.ReadINIStructure(dbPath, id, key, data);
                                    if (string.IsNullOrEmpty(data.szName))
                                    {
                                        // no note's name found - continue to iterate notes ids
                                        goto _sections_loop;
                                    }
                                    if (data.idGroup == -2 && (chkNoRecycle.IsChecked != null && chkNoRecycle.IsChecked.Value))
                                    {
                                        // do not get note from Recycle bin - continue to iterate notes ids
                                        goto _sections_loop;
                                    }
                                    break;
                                case "schedule":
                                    schedule = PNInterop.ReadINIStructure(dbPath, id, key, schedule);
                                    break;
                                case "rel_position":
                                    relPos = PNInterop.ReadINIStructure(dbPath, id, key, relPos);
                                    break;
                                case "add_appearance":
                                    addApp = PNInterop.ReadINIStructure(dbPath, id, key, addApp);
                                    break;
                                case "creation":
                                    creation = PNInterop.ReadINIStructure(dbPath, id, key, creation);
                                    break;
                                case "appearance":
                                    appearance = PNInterop.ReadINIStructure(dbPath, id, key, appearance);
                                    break;
                                case "pin":
                                    pin = PNInterop.ReadINIStructure(dbPath, id, key, pin);
                                    break;
                                case "send_rec":
                                    sendrec = PNInterop.ReadINIStructure(dbPath, id, key, sendrec);
                                    break;
                                case "deletion":
                                    deletion = PNInterop.ReadINIStructure(dbPath, id, key, deletion);
                                    break;
                                case "lock":
                                    PNInterop.GetPrivateProfileStringByBuilder(id, key, "", password, 256, dbPath);
                                    if (password.Length > 0)
                                    {
                                        // no import for password protected notes - continue to iterate notes ids
                                        goto _sections_loop;
                                    }
                                    break;
                                case "tags":
                                    while (PNInterop.GetPrivateProfileStringByBuilder(id, key, "", tags, tags.Capacity, dbPath) == tags.Capacity - 1)
                                    {
                                        tags.Capacity *= 2;
                                    }
                                    break;
                                case "links":
                                    while (PNInterop.GetPrivateProfileStringByBuilder(id, key, "", links, links.Capacity, dbPath) == tags.Capacity - 1)
                                    {
                                        links.Capacity *= 2;
                                    }
                                    break;
                            }
                        }
                        if (addNewNote(id, data, schedule, relPos, addApp, creation, deletion, appearance, pin, sendrec,
                            tags, links, groupIds,
                            chkKeepInvisible.IsChecked ?? false))
                        {
                            listID.Add(id);
                        }
                    }
                _sections_loop:
                    System.Windows.Forms.Application.DoEvents();
                }
                if (listID.Count > 0)
                {
                    var rtb = new System.Windows.Forms.RichTextBox();
                    var ids = listID.ToArray();
                    foreach (string id in ids)
                    {
                        var src = Path.Combine(txtDataDir.Text, id + ".pnote");
                        if (!File.Exists(src))
                        {
                            listID.Remove(id);
                            continue;
                        }
                        try
                        {
                            rtb.LoadFile(src, System.Windows.Forms.RichTextBoxStreamType.RichText);
                        }
                        catch (ArgumentException aex)
                        {
                            if (aex.Message.Contains("File format is not valid"))
                            {
                                listID.Remove(id);
                                continue;
                            }
                        }
                        var dest = Path.Combine(PNPaths.Instance.DataDir, id + PNStrings.NOTE_EXTENSION);
                        File.Copy(src, dest, true);
                    }

                    //show notes if appropriate check box is unchecked
                    PNStatic.FormMain.LoadNotesByList(listID, !(chkKeepInvisible.IsChecked ?? false));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private Dictionary<int, int> importGroups()
        {
            try
            {
                var dict = new Dictionary<int, int>();
                var size = 1024;

                string iniPath = txtIniPath.Text.Trim();
                // get all notes ids
                string buffer = new string(' ', size);
                while (PNInterop.GetPrivateProfileString(null, null, null, buffer, size, iniPath) == size - 2)
                {
                    // loop until sufficient buffer size
                    size *= 2;
                    buffer = new string(' ', size);
                }
                // section names are delimeted by '\0' character with additional '\0' character at the end
                var names = buffer.ToString(CultureInfo.InvariantCulture).Split('\0');
                var groupSection = names.FirstOrDefault(s => s == "groups");
                if (groupSection == null) return null;
                // get ids of all groups under section
                size = 1024;
                buffer = new string(' ', size);
                while (PNInterop.GetPrivateProfileString(groupSection, null, null, buffer, size, iniPath) == size - 2)
                {
                    // loop until sufficient buffer size
                    size *= 2;
                    buffer = new string(' ', size);
                }
                names =
                    buffer.ToString(CultureInfo.InvariantCulture)
                          .Split('\0')
                          .Where(s => s.Trim().Length > 0 && s.Trim() != "0")
                          .ToArray();
                var png = new PNGROUP();
                var crc = new ColorConverter();
                var crd = new System.Drawing.ColorConverter();
                foreach (var id in names)
                {
                    int structSize = Marshal.SizeOf(typeof(PNGROUP));
                    int intSize = Marshal.SizeOf(typeof(int));
                    int boolSize = Marshal.SizeOf(typeof(bool));
                    int hkSize = Marshal.SizeOf(typeof(HK_TYPE));
                    png = PNInterop.ReadINIStructure(iniPath, "groups", id, png, structSize);
                    if (png.Equals(default(PNGROUP)))
                    {
                        structSize -= (intSize * 3 + boolSize * 4 + 2 * 64);
                        png = PNInterop.ReadINIStructure(iniPath, "groups", id, png, structSize);
                        if (png.Equals(default(PNGROUP)))
                        {
                            structSize -= 2 * 256;
                            png = PNInterop.ReadINIStructure(iniPath, "groups", id, png, structSize);
                            if (png.Equals(default(PNGROUP)))
                            {
                                structSize -= hkSize * 2;
                                png = PNInterop.ReadINIStructure(iniPath, "groups", id, png, structSize);
                                if (!png.Equals(default(PNGROUP)))
                                {
                                    png.customCRCaption = png.customCRFont = png.customCRWindow = png.customSkin = false;
                                    png.szSkin = "";
                                    png.szLock = "";
                                    png.hotKeyHide = png.hotKeyShow = default(HK_TYPE);
                                }
                            }
                            else
                            {
                                png.customCRCaption = png.customCRFont = png.customCRWindow = png.customSkin = false;
                                png.szSkin = "";
                                png.szLock = "";
                            }
                        }
                        else
                        {
                            png.customCRCaption = png.customCRFont = png.customCRWindow = png.customSkin = false;
                            png.szSkin = "";
                        }
                    }
                    if (png.Equals(default(PNGROUP))) return null;

                    var pg = new PNGroup();
                    var idNew = PNData.GetNewGroupID();
                    var idOld = png.id;
                    pg.Name = png.szName;
                    pg.ID = idNew;
                    pg.ParentID = -1;
                    if (png.customCRWindow)
                    {
                        var fromString = crc.ConvertFromString(null, PNStatic.CultureInvariant, PNStatic.FromIntToColorString(png.crWindow));
                        if (fromString != null)
                            pg.Skinless.BackColor = (Color)fromString;
                    }
                    if (png.customCRCaption)
                    {
                        var fromString = crc.ConvertFromString(null, PNStatic.CultureInvariant, PNStatic.FromIntToColorString(png.crCaption));
                        if (fromString != null)
                            pg.Skinless.CaptionColor = (Color)fromString;
                    }
                    if (png.customCRFont)
                    {
                        var fromString = crd.ConvertFromString(null, PNStatic.CultureInvariant, PNStatic.FromIntToDrawinfColorString(png.crFont));
                        if (fromString != null)
                            pg.FontColor = (System.Drawing.Color)fromString;
                    }
                    if (png.customSkin)
                    {
                        pg.Skin.SkinName = png.szSkin;
                        var path = Path.Combine(PNPaths.Instance.SkinsDir, pg.Skin.SkinName + PNStrings.SKIN_EXTENSION);
                        if (File.Exists(path))
                        {
                            PNSkinsOperations.LoadSkin(path, pg.Skin);
                        }
                    }
                    pg.Image = TryFindResource("gr") as BitmapImage;
                    pg.IsDefaultImage = true;
                    var parent = PNStatic.Groups.GetGroupByID((int)SpecialGroups.AllGroups);
                    if (parent == null) continue;
                    parent.Subgroups.Add(pg);
                    PNData.InsertNewGroup(pg);
                    dict.Add(idOld, idNew);
                }
                return dict;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private bool addNewNote(string id, NOTE_DATA data, SCHEDULE_TYPE schedule, NOTE_REL_POSITION relPos,
            ADDITIONAL_NAPP addApp, SYSTEMTIME creation, SYSTEMTIME deletion, NOTE_APPEARANCE appearance, NOTE_PIN pin,
            SEND_REC_STATUS sendrec, StringBuilder tags, StringBuilder links, Dictionary<int, int> groupIds, bool hide)
        {
            try
            {
                var sqlList = new List<string>();
                var sb = new StringBuilder();
                var afv = new AlarmAfterValues();
                var md = new MonthDay();
                var dw = new List<DayOfWeek>();
                var sc = new SizeConverter();
                var pc = new PointConverter();
                var ac = new AlarmAfterValuesConverter();
                var mdc = new MonthDayConverter();
                var dwc = new DaysOfWeekConverter();
                var lfc = new LogFontConverter();

                if (PNStatic.Notes.Any(n => n.ID == id))
                {
                    string message = PNLang.Instance.GetMessageText("id_exists",
                        "The note with the same ID already exists.");
                    message += '\n';
                    message += PNLang.Instance.GetMessageText("continue_anyway", "Continue anyway?");
                    if (
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                        MessageBoxResult.No)
                    {
                        return false;
                    }
                }
                sqlList.Add("DELETE FROM NOTES WHERE ID = '" + id + "'");
                sb.Append(
                    "INSERT INTO NOTES (ID, NAME, GROUP_ID, PREV_GROUP_ID, OPACITY, VISIBLE, FAVORITE, PROTECTED, COMPLETED, PRIORITY, PASSWORD_STRING, PINNED, TOPMOST, ROLLED, DOCK_STATUS, DOCK_ORDER, SEND_RECEIVE_STATUS, DATE_CREATED, DATE_SAVED, DATE_SENT, DATE_RECEIVED, DATE_DELETED, SIZE, LOCATION, EDIT_SIZE, REL_X, REL_Y, SENT_TO, RECEIVED_FROM, PIN_CLASS, PIN_TEXT, RECEIVED_IP) VALUES(");
                // ID
                sb.Append("'");
                sb.Append(id);
                sb.Append("', ");
                // NAME
                sb.Append("'");
                sb.Append(data.szName.Replace("'", "''"));
                sb.Append("', ");
                // GROUP_ID
                if (groupIds.Keys.Contains(data.idGroup))
                {
                    sb.Append(groupIds[data.idGroup]);
                }
                else
                {
                    if (PNStatic.Groups.Any(g => g.ID == data.idGroup))
                    {
                        if (data.idGroup == GROUP_INCOMING)
                            sb.Append((int)SpecialGroups.Incoming);
                        else
                            sb.Append(data.idGroup);
                    }
                    else
                    {
                        sb.Append("0");
                    }
                }
                sb.Append(", ");
                // PREV_GROUP_ID
                sb.Append("0");
                sb.Append(", ");
                // OPACITY
                sb.Append(addApp.transValue != 0 ? (addApp.transValue / 255.0).ToString(PNStatic.CultureInvariant) : "1.0");
                sb.Append(", ");
                // VISIBLE
                sb.Append(!hide ? Convert.ToInt32(data.visible) : 0);
                sb.Append(", ");
                // FAVORITE
                sb.Append("0, ");
                // PROTECTED
                sb.Append(Convert.ToInt32(PNStatic.IsBitSet(data.res1, NB_PROTECTED)));
                sb.Append(", ");
                // COMPLETED
                sb.Append(Convert.ToInt32(PNStatic.IsBitSet(data.res1, NB_COMPLETED)));
                sb.Append(", ");
                // PRIORITY
                sb.Append(Convert.ToInt32(PNStatic.IsBitSet(data.res1, NB_HIGH_PRIORITY)));
                sb.Append(", ");
                // PASSWORD_STRING
                sb.Append("'',");
                // PINNED
                sb.Append(
                    Convert.ToInt32((pin.className != null && pin.text != null && pin.className != "" && pin.text != "")));
                sb.Append(", ");
                // TOPMOST
                sb.Append(Convert.ToInt32(data.onTop));
                sb.Append(", ");
                // ROLLED
                sb.Append(Convert.ToInt32(data.rolled));
                sb.Append(", ");
                // DOCK_STATUS
                int ddata = loWord(data.dockData);
                if (ddata < (int)DockStatus.None || ddata > (int)DockStatus.Bottom)
                    sb.Append((int)DockStatus.None);
                else
                    sb.Append(ddata);
                sb.Append(", ");
                // DOCK_ORDER
                var dorder = -1;
                switch ((DockStatus)ddata)
                {
                    case DockStatus.Left:
                    case DockStatus.Top:
                    case DockStatus.Right:
                    case DockStatus.Bottom:
                        if (PNStatic.DockedNotes[(DockStatus)ddata].Count > 0)
                        {
                            dorder = PNStatic.DockedNotes[(DockStatus)ddata].Max(n => n.DockOrder) + 1;
                            dorder += m_DockedNotes[(DockStatus)ddata];
                            m_DockedNotes[(DockStatus)ddata]++;
                        }
                        else
                            dorder = 0;
                        break;
                }
                sb.Append(dorder);
                sb.Append(", ");
                // SEND_RECEIVE_STATUS
                if (sendrec.sentTo != null && sendrec.recFrom != null && sendrec.sentTo != "" && sendrec.recFrom != "")
                    sb.Append((int)(SendReceiveStatus.Both));
                else if (!string.IsNullOrEmpty(sendrec.sentTo))
                    sb.Append((int)SendReceiveStatus.Sent);
                else if (!string.IsNullOrEmpty(sendrec.recFrom))
                    sb.Append((int)SendReceiveStatus.Received);
                else
                    sb.Append("0");
                sb.Append(", ");
                // DATE_CREATED
                DateTime date = new DateTime(creation.wYear != 0 ? creation.wYear : 1,
                    creation.wMonth != 0 ? creation.wMonth : 1, creation.wDay != 0 ? creation.wDay : 1, creation.wHour,
                    creation.wMinute, creation.wSecond);
                sb.Append("'");
                sb.Append(date.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("', ");
                // DATE_SAVED
                date = new DateTime(data.stChanged.wYear != 0 ? data.stChanged.wYear : 1,
                    data.stChanged.wMonth != 0 ? data.stChanged.wMonth : 1,
                    data.stChanged.wDay != 0 ? data.stChanged.wDay : 1, data.stChanged.wHour, data.stChanged.wMinute,
                    data.stChanged.wSecond);
                sb.Append("'");
                sb.Append(date.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("', ");
                // DATE_SENT
                date = new DateTime(sendrec.lastSent.wYear != 0 ? sendrec.lastSent.wYear : 1,
                    sendrec.lastSent.wMonth != 0 ? sendrec.lastSent.wMonth : 1,
                    sendrec.lastSent.wDay != 0 ? sendrec.lastSent.wDay : 1, sendrec.lastSent.wHour,
                    sendrec.lastSent.wMinute, sendrec.lastSent.wSecond);
                sb.Append("'");
                sb.Append(date.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("', ");
                // DATE_RECEIVED
                date = new DateTime(sendrec.lastRec.wYear != 0 ? sendrec.lastRec.wYear : 1,
                    sendrec.lastRec.wMonth != 0 ? sendrec.lastRec.wMonth : 1,
                    sendrec.lastRec.wDay != 0 ? sendrec.lastRec.wDay : 1, sendrec.lastRec.wHour, sendrec.lastRec.wMinute,
                    sendrec.lastRec.wSecond);
                sb.Append("'");
                sb.Append(date.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("', ");
                // DATE_DELETED
                date = new DateTime(deletion.wYear != 0 ? deletion.wYear : 1, deletion.wMonth != 0 ? deletion.wMonth : 1,
                    deletion.wDay != 0 ? deletion.wDay : 1, deletion.wHour, deletion.wMinute, deletion.wSecond);
                sb.Append("'");
                sb.Append(date.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant).Replace("'", "''"));
                sb.Append("', ");
                // SIZE
                Size size = new Size(data.rcp.right - data.rcp.left, data.rcp.bottom - data.rcp.top);
                sb.Append("'");
                sb.Append(sc.ConvertToString(null, PNStatic.CultureInvariant, size));
                sb.Append("', ");
                // LOCATION
                Point location = new Point(data.rcp.left, data.rcp.top);
                sb.Append("'");
                sb.Append(pc.ConvertToString(null, PNStatic.CultureInvariant, location));
                sb.Append("', ");
                // EDIT_SIZE - just copy note size, there is no way to get the edit size of note
                sb.Append("'");
                sb.Append(sc.ConvertToString(null, PNStatic.CultureInvariant, size));
                sb.Append("', ");
                // REL_X
                sb.Append(Math.Abs(relPos.left - 0) > double.Epsilon
                    ? relPos.left.ToString(PNStatic.CultureInvariant)
                    : "1.0");
                sb.Append(", ");
                // REL_Y
                sb.Append(Math.Abs(relPos.top - 0) > double.Epsilon
                    ? relPos.top.ToString(PNStatic.CultureInvariant)
                    : "1.0");
                sb.Append(", ");
                // SENT_TO
                sb.Append("'");
                sb.Append(!string.IsNullOrEmpty(sendrec.sentTo) ? sendrec.sentTo.Replace("'", "''") : "");
                sb.Append("', ");
                // RECEIVED_FROM
                sb.Append("'");
                sb.Append(!string.IsNullOrEmpty(sendrec.recFrom) ? sendrec.recFrom.Replace("'", "''") : "");
                sb.Append("', ");
                // PIN_CLASS
                sb.Append("'");
                sb.Append(!string.IsNullOrEmpty(pin.className) ? pin.className.Replace("'", "''") : "");
                sb.Append("', ");
                // PIN_TEXT
                sb.Append("'");
                sb.Append(!string.IsNullOrEmpty(pin.text) ? pin.text.Replace("'", "''") : "");
                //RECEIVED_IP
                sb.Append("', ''");
                
                sb.Append(")");
                sqlList.Add(sb.ToString());

                // schedule
                // SCHEDULE_TYPE
                if (schedule.scType > 0)
                {
                    ScheduleType sctype;
                    if (schedule.scType > START_PROG)
                    {
                        sctype = (ScheduleType)(schedule.scType - START_PROG);
                    }
                    else
                    {
                        sctype = (ScheduleType)schedule.scType;
                    }
                    var doInsert = true;
                    if (sctype == ScheduleType.RepeatEvery || sctype == ScheduleType.After)
                    {
                        if (schedule.scDate.wYear == 0 &&
                            schedule.scDate.wMonth == 0 &&
                            schedule.scDate.wDayOfWeek == 0 &&
                            schedule.scDate.wDay == 0 &&
                            schedule.scDate.wHour == 0 &&
                            schedule.scDate.wMinute == 0 &&
                            schedule.scDate.wSecond == 0)
                        {
                            doInsert = false;
                        }
                    }
                    if (doInsert)
                    {
                        sb = new StringBuilder();
                        sb.Append(
                            "INSERT INTO NOTES_SCHEDULE (NOTE_ID, SCHEDULE_TYPE, ALARM_DATE, START_DATE, LAST_RUN, SOUND, STOP_AFTER, TRACK, REPEAT_COUNT, SOUND_IN_LOOP, USE_TTS, START_FROM, MONTH_DAY, ALARM_AFTER, WEEKDAYS, PROG_TO_RUN, CLOSE_ON_NOTIFICATION, MULTI_ALERTS, TIME_ZONE) VALUES(");
                        // NOTE_ID
                        sb.Append("'");
                        sb.Append(id);
                        sb.Append("', ");
                        sb.Append((int)sctype);
                        sb.Append(", ");
                        // ALARM_DATE
                        date = new DateTime(schedule.scDate.wYear != 0 ? schedule.scDate.wYear : 1,
                            schedule.scDate.wMonth != 0 ? schedule.scDate.wMonth : 1,
                            schedule.scDate.wDay != 0 ? schedule.scDate.wDay : 1, schedule.scDate.wHour,
                            schedule.scDate.wMinute, schedule.scDate.wSecond);
                        sb.Append("'");
                        sb.Append(date.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant)
                            .Replace("'", "''"));
                        sb.Append("', ");
                        // START_DATE
                        date = new DateTime(schedule.scStart.wYear != 0 ? schedule.scStart.wYear : 1,
                            schedule.scStart.wMonth != 0 ? schedule.scStart.wMonth : 1,
                            schedule.scStart.wDay != 0 ? schedule.scStart.wDay : 1, schedule.scStart.wHour,
                            schedule.scStart.wMinute, schedule.scStart.wSecond);
                        sb.Append("'");
                        sb.Append(date.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant)
                            .Replace("'", "''"));
                        sb.Append("', ");
                        // LAST_RUN
                        date = new DateTime(schedule.scLastRun.wYear != 0 ? schedule.scLastRun.wYear : 1,
                            schedule.scLastRun.wMonth != 0 ? schedule.scLastRun.wMonth : 1,
                            schedule.scLastRun.wDay != 0 ? schedule.scLastRun.wDay : 1, schedule.scLastRun.wHour,
                            schedule.scLastRun.wMinute, schedule.scLastRun.wSecond);
                        sb.Append("'");
                        sb.Append(date.ToString(PNStrings.DATE_TIME_FORMAT, PNStatic.CultureInvariant)
                            .Replace("'", "''"));
                        sb.Append("', ");
                        // SOUND
                        sb.Append("'");
                        sb.Append(schedule.szSound != null && schedule.szSound.Trim().Length > 0
                            ? schedule.szSound.Replace("'", "''")
                            : PNSchedule.DEF_SOUND);
                        sb.Append("', ");
                        // STOP_AFTER
                        sb.Append(schedule.stopLoop);
                        sb.Append(", ");
                        // TRACK
                        sb.Append(Convert.ToInt32(!PNStatic.IsBitSet(data.res1, NB_NOT_TRACK)));
                        sb.Append(", ");
                        // REPEAT_COUNT - no way to get repeat count
                        sb.Append(0);
                        sb.Append(", ");
                        // SOUND_IN_LOOP
                        sb.Append(Convert.ToInt32(PNStatic.IsBitSet(schedule.parameters, SP_SOUND_IN_LOOP)));
                        sb.Append(", ");
                        // USE_TTS
                        sb.Append(Convert.ToInt32(PNStatic.IsBitSet(schedule.parameters, SP_USE_TTS)));
                        sb.Append(", ");
                        // START_FROM
                        if (schedule.scType > START_PROG)
                            sb.Append((int)ScheduleStart.ProgramStart);
                        else
                            sb.Append((int)ScheduleStart.ExactTime);
                        sb.Append(", ");
                        // MONTH_DAY
                        if (sctype == ScheduleType.MonthlyDayOfWeek)
                        {
                            md.WeekDay = (DayOfWeek)schedule.scDate.wDayOfWeek;
                            md.OrdinalNumber = (DayOrdinal)schedule.scDate.wMilliseconds;
                        }
                        sb.Append("'");
                        var convertToString = mdc.ConvertToString(md);
                        if (convertToString != null) sb.Append(convertToString.Replace("'", "''"));
                        sb.Append("', ");
                        // ALARM_AFTER
                        if (sctype == ScheduleType.RepeatEvery || sctype == ScheduleType.After)
                        {
                            afv.Years = schedule.scDate.wYear;
                            afv.Months = schedule.scDate.wMonth;
                            afv.Weeks = schedule.scDate.wDayOfWeek;
                            afv.Days = schedule.scDate.wDay;
                            afv.Hours = schedule.scDate.wHour;
                            afv.Minutes = schedule.scDate.wMinute;
                            afv.Seconds = schedule.scDate.wSecond;
                        }
                        sb.Append("'");
                        sb.Append(ac.ConvertToString(afv).Replace("'", "''"));
                        sb.Append("', ");
                        // WEEKDAYS
                        if (sctype == ScheduleType.Weekly)
                        {
                            if (PNStatic.IsBitSet(schedule.scDate.wDayOfWeek, 1))
                                dw.Add(DayOfWeek.Sunday);
                            if (PNStatic.IsBitSet(schedule.scDate.wDayOfWeek, 2))
                                dw.Add(DayOfWeek.Monday);
                            if (PNStatic.IsBitSet(schedule.scDate.wDayOfWeek, 3))
                                dw.Add(DayOfWeek.Tuesday);
                            if (PNStatic.IsBitSet(schedule.scDate.wDayOfWeek, 4))
                                dw.Add(DayOfWeek.Wednesday);
                            if (PNStatic.IsBitSet(schedule.scDate.wDayOfWeek, 5))
                                dw.Add(DayOfWeek.Thursday);
                            if (PNStatic.IsBitSet(schedule.scDate.wDayOfWeek, 6))
                                dw.Add(DayOfWeek.Friday);
                            if (PNStatic.IsBitSet(schedule.scDate.wDayOfWeek, 7))
                                dw.Add(DayOfWeek.Saturday);
                        }
                        sb.Append("'");
                        sb.Append(dwc.ConvertToString(dw));
                        sb.Append("','");
                        //PROG_TO_RUN
                        sb.Append("',");
                        //CLOSE_ON_NOTIFICATION
                        sb.Append(0);
                        //MULTI_ALERTS
                        sb.Append(", NULL");
                        //TIME_ZONE
                        sb.Append(", NULL");
                        sb.Append(")");
                        sqlList.Add(sb.ToString());
                    }
                }
                // custom note settings
                sb = new StringBuilder();
                sb.Append(
                    "INSERT INTO CUSTOM_NOTES_SETTINGS (NOTE_ID, BACK_COLOR, CAPTION_FONT_COLOR, CAPTION_FONT, SKIN_NAME, CUSTOM_OPACITY) VALUES(");
                // NOTE_ID
                sb.Append("'");
                sb.Append(id);
                sb.Append("', ");
                // BACK_COLOR
                if (appearance.crWindow != 0 && (appearance.nPrivate & F_B_COLOR) == F_B_COLOR)
                {
                    sb.Append("'");
                    var colorString = PNStatic.FromIntToColorString(appearance.crWindow);
                    sb.Append(colorString);
                    sb.Append("', ");
                }
                else
                {
                    sb.Append("NULL, ");
                }
                // CAPTION_FONT_COLOR
                if (appearance.crCaption != 0 && (appearance.nPrivate & F_C_COLOR) == F_C_COLOR)
                {
                    sb.Append("'");
                    var colorString = PNStatic.FromIntToColorString(appearance.crCaption);
                    sb.Append(colorString);
                    sb.Append("', ");
                }
                else
                {
                    sb.Append("NULL, ");
                }
                // CAPTION_FONT
                if (appearance.lfCaption.lfFaceName != null && appearance.lfCaption.lfFaceName.Trim().Length > 0 &&
                    (appearance.nPrivate & F_C_FONT) == F_C_FONT)
                {
                    var lf = new LOGFONT
                    {
                        lfCharSet = appearance.lfCaption.lfCharSet,
                        lfClipPrecision = appearance.lfCaption.lfClipPrecision,
                        lfEscapement = appearance.lfCaption.lfEscapement,
                        lfFaceName = appearance.lfCaption.lfFaceName.Trim(),
                        lfHeight = appearance.lfCaption.lfHeight,
                        lfItalic = appearance.lfCaption.lfItalic,
                        lfOrientation = appearance.lfCaption.lfOrientation,
                        lfOutPrecision = appearance.lfCaption.lfOutPrecision,
                        lfPitchAndFamily = appearance.lfCaption.lfPitchAndFamily,
                        lfQuality = appearance.lfCaption.lfQuality,
                        lfStrikeOut = appearance.lfCaption.lfStrikeOut,
                        lfUnderline = appearance.lfCaption.lfUnderline,
                        lfWeight = appearance.lfCaption.lfWeight,
                        lfWidth = appearance.lfCaption.lfWidth
                    };
                    sb.Append("'");
                    sb.Append(lfc.ConvertToString(lf));
                    sb.Append("', ");
                }
                else
                {
                    sb.Append("NULL, ");
                }
                // SKIN_NAME
                if (appearance.szSkin != null && appearance.szSkin.Trim().Length > 0 &&
                    (appearance.nPrivate & F_SKIN) == F_SKIN)
                {
                    sb.Append("'");
                    sb.Append(appearance.szSkin.Trim().Replace("'", "''"));
                    sb.Append("', ");
                }
                else
                {
                    sb.Append("NULL, ");
                }
                // CUSTOM_OPACITY
                sb.Append(addApp.transValue != 0 ? 1 : 0);
                sb.Append(")");
                sqlList.Add(sb.ToString());

                // linked notes
                if (links.Length > 0)
                {
                    var arr = links.ToString().Split('|');
                    foreach (var s in arr)
                    {
                        if (s.Trim().Length > 0)
                        {
                            sb = new StringBuilder();
                            sb.Append("INSERT INTO LINKED_NOTES (NOTE_ID, LINK_ID) VALUES(");
                            // NOTE_ID
                            sb.Append("'");
                            sb.Append(id);
                            sb.Append("', ");
                            // LINK_ID
                            sb.Append("'");
                            sb.Append(s);
                            sb.Append("'");
                            sb.Append(")");
                            sqlList.Add(sb.ToString());
                        }
                    }
                }

                // tags
                if (tags.Length > 0)
                {
                    var arr = tags.ToString().Split(',');
                    foreach (var s in arr)
                    {
                        if (s.Trim().Length > 0)
                        {
                            sb = new StringBuilder();
                            sb.Append("INSERT INTO NOTES_TAGS (NOTE_ID, TAG) VALUES(");
                            // NOTE_ID
                            sb.Append("'");
                            sb.Append(id);
                            sb.Append("', ");
                            // TAG
                            sb.Append("'");
                            sb.Append(s.Replace("'", "''"));
                            sb.Append("'");
                            sb.Append(") ");
                            sqlList.Add(sb.ToString());
                        }
                    }
                }

                if (!removeNote(id))
                    return false;
                if (!PNData.ExecuteTransactionForList(sqlList, PNData.ConnectionString))
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool removeNote(string id)
        {
            try
            {
                var note = PNStatic.Notes.FirstOrDefault(n => n.ID == id);
                if (note != null)
                {
                    WndNote dlg = null;
                    if (note.Visible)
                    {
                        dlg = note.Dialog;
                    }
                    if (PNNotesOperations.DeleteNote(NoteDeleteType.Complete, note))
                    {
                        if (dlg != null)
                        {
                            dlg.Close();
                        }
                    }
                    else
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
    }
}
