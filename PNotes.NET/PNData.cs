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

using Ionic.Zip;
using PNStaticFonts;
using SQLiteWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ColorConverter = System.Windows.Media.ColorConverter;
using Point = System.Windows.Point;
using PointConverter = System.Windows.PointConverter;
using Size = System.Windows.Size;
using SizeConverter = System.Windows.SizeConverter;
using SystemColors = System.Windows.SystemColors;

namespace PNotes.NET
{
    internal class PNData
    {
        internal const int HK_START = 100;

        private static string _connectionString = "";
        internal static string SettingsConnectionString = "";

        internal static bool IsDBNull(object o)
        {
            return DBNull.Value.Equals(o);
        }

        internal static string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set { _connectionString = value; }
        }

        internal static void SaveExitFlag(int flag)
        {
            try
            {
                using (var oData = new SQLiteDataObject(SettingsConnectionString))
                {
                    var sqlQuery = "UPDATE CONFIG SET EXIT_FLAG = " + flag.ToString(PNStatic.CultureInvariant);
                    oData.Execute(sqlQuery);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void CompactDatabases()
        {
            try
            {
                using (var oData = new SQLiteDataObject(SettingsConnectionString))
                {
                    oData.CompactDatabase();
                }
                using (var oData = new SQLiteDataObject(ConnectionString))
                {
                    oData.CompactDatabase();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void LoadHiddenMenus()
        {
            try
            {
                PNStatic.HiddenMenus.Clear();
                using (var oData = new SQLiteDataObject(ConnectionString))
                {
                    using (var t = oData.FillDataTable("SELECT * FROM HIDDEN_MENUS"))
                    {
                        foreach (DataRow r in t.Rows)
                        {
                            PNStatic.HiddenMenus.Add(new PNHiddenMenu(Convert.ToString(r["MENU_NAME"]),
                                                                (MenuType)r["MENU_TYPE"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveNotesWithShortcuts(IEnumerable<string> ids)
        {
            try
            {
                var sb = new StringBuilder("UPDATE CONFIG SET NOTES_WITH_SHORTCUTS = '");
                if (ids != null)
                {
                    sb.Append(string.Join(",", ids));
                }
                sb.Append("'");
                using (var oData = new SQLiteDataObject(SettingsConnectionString))
                {
                    oData.Execute(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static IEnumerable<string> GetNotesWithShortcuts()
        {
            try
            {
                using (var oData = new SQLiteDataObject(SettingsConnectionString))
                {
                    var obj = oData.GetScalar("SELECT NOTES_WITH_SHORTCUTS FROM CONFIG");
                    if (obj != null && !IsDBNull(obj) && !string.IsNullOrEmpty(Convert.ToString(obj)))
                    {
                        return Convert.ToString(obj).Split(',');
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        internal static void LoadDBSettings()
        {
            try
            {
                var mediaConverter = new ColorConverter();
                var drawingConverter = new System.Drawing.ColorConverter();
                var sc = new SizeConverter();
                var pc = new PointConverter();
                var wpfc = new WPFFontConverter();
                //var fc = new FontConverter();
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                PNStatic.Settings = new PNSettings();
                SettingsConnectionString = SQLiteDataObject.CheckAndCreateDatabase(PNPaths.Instance.SettingsDBPath);
                using (var oData = new SQLiteDataObject(SettingsConnectionString))
                {
                    string sqlQuery;
                    //config
                    var pnc = PNStatic.Settings.Config;
                    if (!oData.TableExists("CONFIG"))
                    {
                        sqlQuery = "CREATE TABLE [CONFIG] ([LAST_PAGE] TEXT, [EXIT_FLAG] INT, [CP_LAST_GROUP] INT, [SKINNABLE] BOOLEAN, [CP_PVW_COLOR] TEXT, [CP_USE_CUST_PVW_COLOR] BOOLEAN, [CP_SIZE] TEXT, [CP_LOCATION] TEXT, [CONTROLS_STYLE] TEXT, [CP_PVW_RIGHT] BOOLEAN, [UI_FONT] TEXT, [PROGRAM_VERSION] TEXT, [CP_PVW_SHOW] BOOLEAN, [CP_GROUPS_SHOW] BOOLEAN, [NOTES_WITH_SHORTCUTS] TEXT, [SEARCH_NOTES_SETT] TEXT)";
                        oData.Execute(sqlQuery);
                        sqlQuery =
                            "INSERT INTO CONFIG VALUES(NULL, -1, NULL, NULL, NULL, NULL, '1000,600', NULL, NULL, NULL, NULL, '" +
                            v.ToString(3) + "', NULL, NULL, NULL, NULL)";
                        oData.Execute(sqlQuery);
                        PNSingleton.Instance.FontUser = new PNFont();
                        pnc.CPSize = new Size(1000, 600);
                    }
                    else
                    {
                        using (var t = oData.GetSchema("Columns"))
                        {
                            var rows = t.Select("COLUMN_NAME = 'PROGRAM_VERSION' AND TABLE_NAME = 'CONFIG'");
                            if (rows.Length == 0)
                            {
                                PNSingleton.Instance.PlatformChanged = true;
                                sqlQuery = "ALTER TABLE CONFIG ADD COLUMN [PROGRAM_VERSION] TEXT";
                                oData.Execute(sqlQuery);
                                //save previous edition files
                                PNStatic.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("back_prev",
                                    "Backing up files from previous edition...");
                                savePreviousFiles();
                            }
                            rows = t.Select("COLUMN_NAME = 'CP_PVW_RIGHT' AND TABLE_NAME = 'CONFIG'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE CONFIG ADD COLUMN [CP_PVW_RIGHT] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'CP_PVW_SHOW' AND TABLE_NAME = 'CONFIG'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE CONFIG ADD COLUMN [CP_PVW_SHOW] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'CP_GROUPS_SHOW' AND TABLE_NAME = 'CONFIG'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE CONFIG ADD COLUMN [CP_GROUPS_SHOW] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'UI_FONT' AND TABLE_NAME = 'CONFIG'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE CONFIG ADD COLUMN [UI_FONT] TEXT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'NOTES_WITH_SHORTCUTS' AND TABLE_NAME = 'CONFIG'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE CONFIG ADD COLUMN [NOTES_WITH_SHORTCUTS] TEXT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'SEARCH_NOTES_SETT' AND TABLE_NAME = 'CONFIG'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE CONFIG ADD COLUMN [SEARCH_NOTES_SETT] TEXT";
                                oData.Execute(sqlQuery);
                            }
                        }
                        //store version
                        sqlQuery = "UPDATE CONFIG SET PROGRAM_VERSION = '" + v.ToString(3) + "'";
                        oData.Execute(sqlQuery);
                        //upgrade colors an fonts
                        if (PNSingleton.Instance.PlatformChanged)
                        {
                            var obj = oData.GetScalar("SELECT CP_PVW_COLOR FROM CONFIG");
                            if (obj != null && !IsDBNull(obj))
                            {
                                obj = drawingConverter.ConvertFromString(null, PNStatic.CultureInvariant, (string)obj);
                                if (obj != null)
                                {
                                    var clrD = (System.Drawing.Color)obj;
                                    var clrM = Color.FromArgb(clrD.A, clrD.R, clrD.G, clrD.B);
                                    var clrText = mediaConverter.ConvertToString(clrM);
                                    oData.Execute("UPDATE CONFIG SET CP_PVW_COLOR = '" + clrText + "'");
                                }
                            }
                            obj = oData.GetScalar("SELECT UI_FONT FROM CONFIG");
                            if (obj != null && !IsDBNull(obj))
                            {
                                var pnFonf = PNStatic.FromDrawingFont((string)obj);
                                var fontText = wpfc.ConvertToString(pnFonf);
                                oData.Execute("UPDATE CONFIG SET UI_FONT = '" + fontText + "'");
                            }
                        }
                        sqlQuery = "SELECT * FROM CONFIG";
                        using (var t = oData.FillDataTable(sqlQuery))
                        {
                            if (t.Rows.Count > 0)
                            {
                                var r = t.Rows[0];
                                if (!IsDBNull(r["LAST_PAGE"]))
                                {
                                    pnc.LastPage = Convert.ToInt32(r["LAST_PAGE"]);
                                }

                                if (!IsDBNull(r["EXIT_FLAG"]))
                                {
                                    pnc.ExitFlag = (int)r["EXIT_FLAG"];
                                }
                                if (!IsDBNull(r["CP_LAST_GROUP"]))
                                {
                                    pnc.CPLastGroup = (int)r["CP_LAST_GROUP"];
                                }
                                if (!IsDBNull(r["SKINNABLE"]))
                                {
                                    pnc.Skinnable = Convert.ToBoolean(r["SKINNABLE"]);
                                }
                                if (!IsDBNull(r["CP_PVW_COLOR"]))
                                {
                                    var convertFromString = mediaConverter.ConvertFromString(null, PNStatic.CultureInvariant, (string)r["CP_PVW_COLOR"]);
                                    if (
                                        convertFromString != null)
                                        pnc.CPPvwColor = (Color)convertFromString;
                                }
                                if (!IsDBNull(r["CP_USE_CUST_PVW_COLOR"]))
                                {
                                    pnc.CPUseCustPvwColor = Convert.ToBoolean(r["CP_USE_CUST_PVW_COLOR"]);
                                }
                                if (!IsDBNull(r["CP_SIZE"]))
                                {
                                    var str = Convert.ToString(r["CP_LOCATION"]);
                                    if (!str.Contains('-'))
                                    {
                                        str = Convert.ToString(r["CP_SIZE"]);
                                        var convertFromString = sc.ConvertFromString(null, PNStatic.CultureInvariant,
                                            str);
                                        if (convertFromString != null)
                                            pnc.CPSize = (Size)convertFromString;
                                    }
                                }
                                if (!IsDBNull(r["CP_LOCATION"]))
                                {
                                    var convertFromString = pc.ConvertFromString(null, PNStatic.CultureInvariant, (string)r["CP_LOCATION"]);
                                    if (
                                        convertFromString != null)
                                        pnc.CPLocation = (Point)convertFromString;
                                }
                                if (!IsDBNull(r["CONTROLS_STYLE"]))
                                {
                                    pnc.ControlsStyle = (string)r["CONTROLS_STYLE"];
                                }
                                if (!IsDBNull(r["CP_PVW_RIGHT"]))
                                {
                                    pnc.CPPvwRight = Convert.ToBoolean(r["CP_PVW_RIGHT"]);
                                }
                                if (!IsDBNull(r["CP_PVW_SHOW"]))
                                {
                                    pnc.CPPvwShow = Convert.ToBoolean(r["CP_PVW_SHOW"]);
                                }
                                if (!IsDBNull(r["CP_GROUPS_SHOW"]))
                                {
                                    pnc.CPGroupsShow = Convert.ToBoolean(r["CP_GROUPS_SHOW"]);
                                }
                                if (!IsDBNull(r["UI_FONT"]))
                                {
                                    var temp = (string)(r["UI_FONT"]);
                                    if (temp != "")
                                    {
                                        PNSingleton.Instance.FontUser = (PNFont)wpfc.ConvertFromString(temp);
                                    }
                                    else
                                    {
                                        PNSingleton.Instance.FontUser = new PNFont();
                                    }
                                }
                                else
                                {
                                    PNSingleton.Instance.FontUser = new PNFont();
                                }
                                if (!IsDBNull(r["SEARCH_NOTES_SETT"]))
                                {
                                    var arr = Convert.ToString(r["SEARCH_NOTES_SETT"]).Split('|');
                                    pnc.SearchNotesSettings.WholewWord = Convert.ToBoolean(arr[0]);
                                    pnc.SearchNotesSettings.MatchCase = Convert.ToBoolean(arr[1]);
                                    pnc.SearchNotesSettings.IncludeHidden = Convert.ToBoolean(arr[2]);
                                    pnc.SearchNotesSettings.Criteria = Convert.ToInt32(arr[3]);
                                    pnc.SearchNotesSettings.Scope = Convert.ToInt32(arr[4]);
                                }
                                SaveExitFlag(-1);
                            }
                            else
                            {
                                sqlQuery =
                                    "INSERT INTO CONFIG VALUES(NULL, -1, NULL, NULL, NULL, NULL, '1000,600', NULL, NULL, NULL, NULL, '" +
                                    v.ToString(3) + "', NULL, NULL, NULL, NULL)";
                                oData.Execute(sqlQuery);
                                PNSingleton.Instance.FontUser = new PNFont();
                                pnc.CPSize = new Size(1000, 600);
                            }
                        }
                    }

                    //general setting
                    var pngeneral = PNStatic.Settings.GeneralSettings;
                    if (!oData.TableExists("GENERAL_SETTINGS"))
                    {
                        sqlQuery = "CREATE TABLE [GENERAL_SETTINGS] ([LANGUAGE] TEXT, [RUN_ON_START] BOOLEAN, [SHOW_CP_ON_START] BOOLEAN, [CHECK_NEW_VERSION_ON_START] BOOLEAN, [HIDE_TOOLBAR] BOOLEAN, [USE_CUSTOM_FONTS] BOOLEAN, [SHOW_SCROLLBAR] BOOLEAN, [HIDE_DELETE_BUTTON] BOOLEAN, [CHANGE_HIDE_TO_DELETE] BOOLEAN, [HIDE_HIDE_BUTTON] BOOLEAN, [BULLETS_INDENT] INT, [MARGIN_WIDTH] INT, [SAVE_ON_EXIT] BOOLEAN, [CONFIRM_SAVING] BOOLEAN, [CONFIRM_BEFORE_DELETION] BOOLEAN, [SAVE_WITHOUT_CONFIRM_ON_HIDE] BOOLEAN, [WARN_ON_AUTOMATICAL_DELETE] BOOLEAN, [AUTO_SAVE] BOOLEAN, [AUTO_SAVE_PERIOD] INT, [REMOVE_FROM_BIN_PERIOD] INT, [DATE_FORMAT] TEXT, [TIME_FORMAT] TEXT, [SKINLESS_WIDTH] INT, [SKINLESS_HEIGHT] INT, [SPELL_COLOR] TEXT, [USE_SKINS] BOOLEAN, [SPELL_MODE] INT, [SPELL_DICT] TEXT, [DOCK_WIDTH] INT, [DOCK_HEIGHT] INT, [SHOW_PRIORITY_ON_START] BOOLEAN, [BUTTONS_SIZE] INT, [AUTOMATIC_SMILIES] BOOLEAN, [SPACE_POINTS] INT, [RESTORE_AUTO] BOOLEAN, [PARAGRAPH_INDENT] INT, [AUTO_HEIGHT] BOOLEAN, [CRITICAL_ON_START] BOOLEAN, [CRITICAL_PERIODICALLY] BOOLEAN, [DELETE_SHORTCUTS_ON_EXIT] BOOLEAN, [RESTORE_SHORTCUTS_ON_START] BOOLEAN, [CLOSE_ON_SHORTCUT] BOOLEAN)";
                        oData.Execute(sqlQuery);
                        sqlQuery = "INSERT INTO GENERAL_SETTINGS VALUES(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
                        oData.Execute(sqlQuery);
                        //store default english language
                        var langPath = Path.Combine(PNPaths.Instance.LangDir, pngeneral.Language);
                        PNLang.Instance.LoadLanguage(langPath);
                    }
                    else
                    {
                        using (var t = oData.GetSchema("Columns"))
                        {
                            var rows = t.Select("COLUMN_NAME = 'BUTTONS_SIZE' AND TABLE_NAME = 'GENERAL_SETTINGS'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE GENERAL_SETTINGS ADD COLUMN [BUTTONS_SIZE] INT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'AUTOMATIC_SMILIES' AND TABLE_NAME = 'GENERAL_SETTINGS'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE GENERAL_SETTINGS ADD COLUMN [AUTOMATIC_SMILIES] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'SPACE_POINTS' AND TABLE_NAME = 'GENERAL_SETTINGS'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE GENERAL_SETTINGS ADD COLUMN [SPACE_POINTS] INT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'RESTORE_AUTO' AND TABLE_NAME = 'GENERAL_SETTINGS'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE GENERAL_SETTINGS ADD COLUMN [RESTORE_AUTO] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'PARAGRAPH_INDENT' AND TABLE_NAME = 'GENERAL_SETTINGS'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE GENERAL_SETTINGS ADD COLUMN [PARAGRAPH_INDENT] INT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'AUTO_HEIGHT' AND TABLE_NAME = 'GENERAL_SETTINGS'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE GENERAL_SETTINGS ADD COLUMN [AUTO_HEIGHT] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'CRITICAL_ON_START' AND TABLE_NAME = 'GENERAL_SETTINGS'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE GENERAL_SETTINGS ADD COLUMN [CRITICAL_ON_START] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'CRITICAL_PERIODICALLY' AND TABLE_NAME = 'GENERAL_SETTINGS'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE GENERAL_SETTINGS ADD COLUMN [CRITICAL_PERIODICALLY] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'DELETE_SHORTCUTS_ON_EXIT' AND TABLE_NAME = 'GENERAL_SETTINGS'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE GENERAL_SETTINGS ADD COLUMN [DELETE_SHORTCUTS_ON_EXIT] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'RESTORE_SHORTCUTS_ON_START' AND TABLE_NAME = 'GENERAL_SETTINGS'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE GENERAL_SETTINGS ADD COLUMN [RESTORE_SHORTCUTS_ON_START] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'CLOSE_ON_SHORTCUT' AND TABLE_NAME = 'GENERAL_SETTINGS'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE GENERAL_SETTINGS ADD COLUMN [CLOSE_ON_SHORTCUT] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                        }
                        sqlQuery = "SELECT * FROM GENERAL_SETTINGS";
                        using (var t = oData.FillDataTable(sqlQuery))
                        {
                            if (t.Rows.Count > 0)
                            {
                                var r = t.Rows[0];
                                if (!IsDBNull(r["LANGUAGE"]))
                                {
                                    pngeneral.Language = (string) r["LANGUAGE"];
                                }
                                var langPath = Path.Combine(PNPaths.Instance.LangDir, pngeneral.Language);
                                PNLang.Instance.LoadLanguage(langPath);
                                if (!IsDBNull(r["RUN_ON_START"]))
                                {
                                    pngeneral.RunOnStart = (bool)r["RUN_ON_START"];
                                }
                                if (!IsDBNull(r["HIDE_TOOLBAR"]))
                                {
                                    pngeneral.HideToolbar = (bool)r["HIDE_TOOLBAR"];
                                }
                                if (!IsDBNull(r["SHOW_CP_ON_START"]))
                                {
                                    pngeneral.ShowCPOnStart = (bool)r["SHOW_CP_ON_START"];
                                }
                                if (!IsDBNull(r["CHECK_NEW_VERSION_ON_START"]))
                                {
                                    pngeneral.CheckNewVersionOnStart = (bool)r["CHECK_NEW_VERSION_ON_START"];
                                }
                                if (!IsDBNull(r["USE_CUSTOM_FONTS"]))
                                {
                                    pngeneral.UseCustomFonts = (bool)r["USE_CUSTOM_FONTS"];
                                }
                                if (!IsDBNull(r["SHOW_SCROLLBAR"]))
                                {
                                        pngeneral.ShowScrollbar =
                                            (System.Windows.Forms.RichTextBoxScrollBars)
                                                Convert.ToInt32(r["SHOW_SCROLLBAR"]);
                                }
                                if (!IsDBNull(r["HIDE_DELETE_BUTTON"]))
                                {
                                    pngeneral.HideDeleteButton = (bool)r["HIDE_DELETE_BUTTON"];
                                }
                                if (!IsDBNull(r["CHANGE_HIDE_TO_DELETE"]))
                                {
                                    pngeneral.ChangeHideToDelete = (bool)r["CHANGE_HIDE_TO_DELETE"];
                                }
                                if (!IsDBNull(r["HIDE_HIDE_BUTTON"]))
                                {
                                    pngeneral.HideHideButton = (bool)r["HIDE_HIDE_BUTTON"];
                                }
                                if (!IsDBNull(r["BULLETS_INDENT"]))
                                {
                                    pngeneral.BulletsIndent = (short)(int)r["BULLETS_INDENT"];
                                }
                                if (!IsDBNull(r["MARGIN_WIDTH"]))
                                {
                                    pngeneral.MarginWidth = (short)(int)r["MARGIN_WIDTH"];
                                }
                                if (!IsDBNull(r["DATE_FORMAT"]))
                                {
                                    pngeneral.DateFormat =
                                        ((string)r["DATE_FORMAT"]).Replace("H", "")
                                            .Replace("h", "")
                                            .Replace("m", "")
                                            .Replace(":", "")
                                            .Trim();
                                    Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern = pngeneral.DateFormat;
                                }
                                if (!IsDBNull(r["TIME_FORMAT"]))
                                {
                                    pngeneral.TimeFormat = (string)r["TIME_FORMAT"];
                                    Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongTimePattern = pngeneral.TimeFormat;
                                }
                                if (!IsDBNull(r["SAVE_ON_EXIT"]))
                                {
                                    pngeneral.SaveOnExit = (bool)r["SAVE_ON_EXIT"];
                                }
                                if (!IsDBNull(r["CONFIRM_SAVING"]))
                                {
                                    pngeneral.ConfirmSaving = (bool)r["CONFIRM_SAVING"];
                                }
                                if (!IsDBNull(r["CONFIRM_BEFORE_DELETION"]))
                                {
                                    pngeneral.ConfirmBeforeDeletion = (bool)r["CONFIRM_BEFORE_DELETION"];
                                }
                                if (!IsDBNull(r["SAVE_WITHOUT_CONFIRM_ON_HIDE"]))
                                {
                                    pngeneral.SaveWithoutConfirmOnHide = (bool)r["SAVE_WITHOUT_CONFIRM_ON_HIDE"];
                                }
                                if (!IsDBNull(r["WARN_ON_AUTOMATICAL_DELETE"]))
                                {
                                    pngeneral.WarnOnAutomaticalDelete = (bool)r["WARN_ON_AUTOMATICAL_DELETE"];
                                }
                                if (!IsDBNull(r["REMOVE_FROM_BIN_PERIOD"]))
                                {
                                    pngeneral.RemoveFromBinPeriod = (int)r["REMOVE_FROM_BIN_PERIOD"];
                                }
                                if (!IsDBNull(r["AUTO_SAVE"]))
                                {
                                    pngeneral.Autosave = (bool)r["AUTO_SAVE"];
                                }
                                if (!IsDBNull(r["AUTO_SAVE_PERIOD"]))
                                {
                                    pngeneral.AutosavePeriod = (int)r["AUTO_SAVE_PERIOD"];
                                }
                                if (!IsDBNull(r["SKINLESS_WIDTH"]))
                                {
                                    pngeneral.Width = (int)r["SKINLESS_WIDTH"];
                                }
                                if (!IsDBNull(r["SKINLESS_HEIGHT"]))
                                {
                                    pngeneral.Height = (int)r["SKINLESS_HEIGHT"];
                                }
                                if (!IsDBNull(r["SPELL_COLOR"]))
                                {
                                    pngeneral.SpellColor = (System.Drawing.Color)
                                            drawingConverter.ConvertFromString(null, PNStatic.CultureInvariant,
                                                (string)r["SPELL_COLOR"]);
                                }
                                if (!IsDBNull(r["USE_SKINS"]))
                                {
                                    pngeneral.UseSkins = (bool)r["USE_SKINS"];
                                }
                                if (!IsDBNull(r["SPELL_MODE"]))
                                {
                                    pngeneral.SpellMode = (int)r["SPELL_MODE"];
                                }
                                if (!IsDBNull(r["SPELL_DICT"]))
                                {
                                    pngeneral.SpellDict = (string)r["SPELL_DICT"];
                                }
                                if (!IsDBNull(r["DOCK_WIDTH"]))
                                {
                                    pngeneral.DockWidth = (int)r["DOCK_WIDTH"];
                                }
                                if (!IsDBNull(r["DOCK_HEIGHT"]))
                                {
                                    pngeneral.DockHeight = (int)r["DOCK_HEIGHT"];
                                }
                                if (!IsDBNull(r["SHOW_PRIORITY_ON_START"]))
                                {
                                    pngeneral.ShowPriorityOnStart = (bool)r["SHOW_PRIORITY_ON_START"];
                                }
                                if (!IsDBNull(r["BUTTONS_SIZE"]))
                                {
                                    pngeneral.ButtonsSize = (ToolStripButtonSize)((int)r["BUTTONS_SIZE"]);
                                }
                                if (!IsDBNull(r["AUTOMATIC_SMILIES"]))
                                {
                                    pngeneral.AutomaticSmilies = (bool)r["AUTOMATIC_SMILIES"];
                                }
                                if (!IsDBNull(r["SPACE_POINTS"]))
                                {
                                    pngeneral.SpacePoints = (int)r["SPACE_POINTS"];
                                }
                                if (!IsDBNull(r["RESTORE_AUTO"]))
                                {
                                    pngeneral.RestoreAuto = (bool)r["RESTORE_AUTO"];
                                }
                                if (!IsDBNull(r["PARAGRAPH_INDENT"]))
                                {
                                    pngeneral.ParagraphIndent = (int)r["PARAGRAPH_INDENT"];
                                }
                                if (!IsDBNull(r["AUTO_HEIGHT"]))
                                {
                                    pngeneral.AutoHeight = (bool)r["AUTO_HEIGHT"];
                                }
                                if (!IsDBNull(r["CRITICAL_ON_START"]))
                                {
                                    pngeneral.CheckCriticalOnStart = (bool)r["CRITICAL_ON_START"];
                                }
                                if (!IsDBNull(r["CRITICAL_PERIODICALLY"]))
                                {
                                    pngeneral.CheckCriticalPeriodically = (bool)r["CRITICAL_PERIODICALLY"];
                                }
                                if (!IsDBNull(r["DELETE_SHORTCUTS_ON_EXIT"]))
                                {
                                    pngeneral.DeleteShortcutsOnExit = (bool)r["DELETE_SHORTCUTS_ON_EXIT"];
                                }
                                if (!IsDBNull(r["RESTORE_SHORTCUTS_ON_START"]))
                                {
                                    pngeneral.RestoreShortcutsOnStart = (bool)r["RESTORE_SHORTCUTS_ON_START"];
                                }
                                if (!IsDBNull(r["CLOSE_ON_SHORTCUT"]))
                                {
                                    pngeneral.CloseOnShortcut = (bool)r["CLOSE_ON_SHORTCUT"];
                                }
                            }
                            else
                            {
                                sqlQuery = "INSERT INTO GENERAL_SETTINGS VALUES(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
                                oData.Execute(sqlQuery);
                            }
                        }
                    }

                    PNStatic.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("load_program_settings", "Loading program settings");

                    //schedule
                    if (!oData.TableExists("SCHEDULE"))
                    {
                        sqlQuery = "CREATE TABLE [SCHEDULE] ([SOUND] TEXT, [DATE_FORMAT] TEXT, [TIME_FORMAT] TEXT, [VOICE] TEXT, [ALLOW_SOUND] BOOLEAN, [TRACK_OVERDUE] BOOLEAN, [VISUAL_NOTIFY] BOOLEAN, [CENTER_SCREEN] BOOLEAN, [VOICE_VOLUME] INT, [VOICE_SPEED] INT, [VOICE_PITCH] INT)";
                        oData.Execute(sqlQuery);
                        sqlQuery = "INSERT INTO SCHEDULE VALUES(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
                        oData.Execute(sqlQuery);
                    }
                    else
                    {
                        sqlQuery = "SELECT * FROM SCHEDULE";
                        using (var t = oData.FillDataTable(sqlQuery))
                        {
                            if (t.Rows.Count > 0)
                            {
                                var r = t.Rows[0];
                                var pnsc = PNStatic.Settings.Schedule;
                                if (!IsDBNull(r["SOUND"]))
                                {
                                    pnsc.Sound = (string)r["SOUND"];
                                }
                                if (!IsDBNull(r["VOICE"]))
                                {
                                    pnsc.Voice = (string)r["VOICE"];
                                }
                                if (!IsDBNull(r["ALLOW_SOUND"]))
                                {
                                    pnsc.AllowSoundAlert = (bool)r["ALLOW_SOUND"];
                                }
                                if (!IsDBNull(r["TRACK_OVERDUE"]))
                                {
                                    pnsc.TrackOverdue = (bool)r["TRACK_OVERDUE"];
                                }
                                if (!IsDBNull(r["VISUAL_NOTIFY"]))
                                {
                                    pnsc.VisualNotification = (bool)r["VISUAL_NOTIFY"];
                                }
                                if (!IsDBNull(r["CENTER_SCREEN"]))
                                {
                                    pnsc.CenterScreen = (bool)r["CENTER_SCREEN"];
                                }
                                if (!IsDBNull(r["VOICE_VOLUME"]))
                                {
                                    pnsc.VoiceVolume = (int)r["VOICE_VOLUME"];
                                }
                                if (!IsDBNull(r["VOICE_SPEED"]))
                                {
                                    pnsc.VoiceSpeed = (int)r["VOICE_SPEED"];
                                }
                                if (!IsDBNull(r["VOICE_PITCH"]))
                                {
                                    pnsc.VoicePitch = (int)r["VOICE_PITCH"];
                                }
                            }
                            else
                            {
                                sqlQuery = "INSERT INTO SCHEDULE VALUES(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
                                oData.Execute(sqlQuery);
                            }
                        }
                    }
                    //behavior
                    if (!oData.TableExists("BEHAVIOR"))
                    {
                        sqlQuery = "CREATE TABLE [BEHAVIOR] ([NEW_ALWAYS_ON_TOP] BOOLEAN, [RELATIONAL_POSITION] BOOLEAN, [HIDE_COMPLETED] BOOLEAN, [BIG_ICONS_ON_CP] BOOLEAN, [DO_NOT_SHOW_IN_LIST] BOOLEAN, [KEEP_VISIBLE_ON_SHOW_DESKTOP] BOOLEAN, [DBL_CLICK_ACTION] INT, [SINGLE_CLICK_ACTION] INT, [DEFAULT_NAMING] INT, [DEFAULT_NAME_LENGHT] INT, [CONTENT_COLUMN_LENGTH] INT, [HIDE_FLUENTLY] BOOLEAN, [PLAY_SOUND_ON_HIDE] BOOLEAN, [OPACITY] REAL, [RANDOM_COLOR] BOOLEAN, [INVERT_TEXT_COLOR] BOOLEAN, [ROLL_ON_DBLCLICK] BOOLEAN, [FIT_WHEN_ROLLED] BOOLEAN, [SHOW_SEPARATE_NOTES] BOOLEAN, [PIN_CLICK_ACTION] INT, [NOTE_START_POSITION] INT, [HIDE_MAIN_WINDOW] BOOLEAN, [THEME] TEXT, [PREVENT_RESIZING] BOOLEAN, [SHOW_PANEL] BOOLEAN, [PANEL_DOCK] INT, [PANEL_AUTO_HIDE] BOOLEAN, [PANEL_REMOVE_MODE] INT, [PANEL_SWITCH_OFF_ANIMATION] BOOLEAN, [PANEL_ENTER_DELAY] INT)";
                        oData.Execute(sqlQuery);
                        sqlQuery = "INSERT INTO BEHAVIOR VALUES(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
                        oData.Execute(sqlQuery);
                    }
                    else
                    {
                        using (var t = oData.GetSchema("Columns"))
                        {
                            var rows = t.Select("COLUMN_NAME = 'NOTE_START_POSITION' AND TABLE_NAME = 'BEHAVIOR'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE BEHAVIOR ADD COLUMN [NOTE_START_POSITION] INT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'HIDE_MAIN_WINDOW' AND TABLE_NAME = 'BEHAVIOR'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE BEHAVIOR ADD COLUMN [HIDE_MAIN_WINDOW] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'THEME' AND TABLE_NAME = 'BEHAVIOR'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE BEHAVIOR ADD COLUMN [THEME] TEXT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'PREVENT_RESIZING' AND TABLE_NAME = 'BEHAVIOR'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE BEHAVIOR ADD COLUMN [PREVENT_RESIZING] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'SHOW_PANEL' AND TABLE_NAME = 'BEHAVIOR'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE BEHAVIOR ADD COLUMN [SHOW_PANEL] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'PANEL_DOCK' AND TABLE_NAME = 'BEHAVIOR'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE BEHAVIOR ADD COLUMN [PANEL_DOCK] INT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'PANEL_AUTO_HIDE' AND TABLE_NAME = 'BEHAVIOR'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE BEHAVIOR ADD COLUMN [PANEL_AUTO_HIDE] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'PANEL_REMOVE_MODE' AND TABLE_NAME = 'BEHAVIOR'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE BEHAVIOR ADD COLUMN [PANEL_REMOVE_MODE] INT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'PANEL_SWITCH_OFF_ANIMATION' AND TABLE_NAME = 'BEHAVIOR'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE BEHAVIOR ADD COLUMN [PANEL_SWITCH_OFF_ANIMATION] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'PANEL_ENTER_DELAY' AND TABLE_NAME = 'BEHAVIOR'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE BEHAVIOR ADD COLUMN [PANEL_ENTER_DELAY] INT";
                                oData.Execute(sqlQuery);
                            }
                        }
                        sqlQuery = "SELECT * FROM BEHAVIOR";
                        using (var t = oData.FillDataTable(sqlQuery))
                        {
                            if (t.Rows.Count > 0)
                            {
                                var r = t.Rows[0];
                                var pnb = PNStatic.Settings.Behavior;
                                if (!IsDBNull(r["NEW_ALWAYS_ON_TOP"]))
                                {
                                    pnb.NewNoteAlwaysOnTop = (bool)r["NEW_ALWAYS_ON_TOP"];
                                }
                                if (!IsDBNull(r["RELATIONAL_POSITION"]))
                                {
                                    pnb.RelationalPositioning = (bool)r["RELATIONAL_POSITION"];
                                }
                                if (!IsDBNull(r["HIDE_COMPLETED"]))
                                {
                                    pnb.HideCompleted = (bool)r["HIDE_COMPLETED"];
                                }
                                if (!IsDBNull(r["BIG_ICONS_ON_CP"]))
                                {
                                    pnb.BigIconsOnCP = (bool)r["BIG_ICONS_ON_CP"];
                                }
                                if (!IsDBNull(r["DO_NOT_SHOW_IN_LIST"]))
                                {
                                    pnb.DoNotShowNotesInList = (bool)r["DO_NOT_SHOW_IN_LIST"];
                                }
                                if (!IsDBNull(r["KEEP_VISIBLE_ON_SHOW_DESKTOP"]))
                                {
                                    pnb.KeepVisibleOnShowDesktop = (bool)r["KEEP_VISIBLE_ON_SHOW_DESKTOP"];
                                }
                                if (!IsDBNull(r["DBL_CLICK_ACTION"]))
                                {
                                    var index = (int) r["DBL_CLICK_ACTION"];
                                    if (index < Enum.GetValues(typeof (TrayMouseAction)).Length)
                                        pnb.DoubleClickAction = (TrayMouseAction) index;
                                }
                                if (!IsDBNull(r["SINGLE_CLICK_ACTION"]))
                                {
                                    var index = (int)r["SINGLE_CLICK_ACTION"];
                                    if (index < Enum.GetValues(typeof (TrayMouseAction)).Length)
                                        pnb.SingleClickAction = (TrayMouseAction) index;
                                }
                                if (!IsDBNull(r["DEFAULT_NAMING"]))
                                {
                                    var index = (int)r["DEFAULT_NAMING"];
                                    if (index < Enum.GetValues(typeof(DefaultNaming)).Length)
                                        pnb.DefaultNaming = (DefaultNaming)index;
                                }
                                if (!IsDBNull(r["DEFAULT_NAME_LENGHT"]))
                                {
                                    pnb.DefaultNameLength = (int)r["DEFAULT_NAME_LENGHT"];
                                }
                                if (!IsDBNull(r["CONTENT_COLUMN_LENGTH"]))
                                {
                                    pnb.ContentColumnLength = (int)r["CONTENT_COLUMN_LENGTH"];
                                }
                                if (!IsDBNull(r["HIDE_FLUENTLY"]))
                                {
                                    pnb.HideFluently = (bool)r["HIDE_FLUENTLY"];
                                }
                                if (!IsDBNull(r["PLAY_SOUND_ON_HIDE"]))
                                {
                                    pnb.PlaySoundOnHide = (bool)r["PLAY_SOUND_ON_HIDE"];
                                }
                                if (!IsDBNull(r["OPACITY"]))
                                {
                                    pnb.Opacity = (double)r["OPACITY"];
                                }
                                if (!IsDBNull(r["RANDOM_COLOR"]))
                                {
                                    pnb.RandomBackColor = (bool)r["RANDOM_COLOR"];
                                }
                                if (!IsDBNull(r["INVERT_TEXT_COLOR"]))
                                {
                                    pnb.InvertTextColor = (bool)r["INVERT_TEXT_COLOR"];
                                }
                                if (!IsDBNull(r["ROLL_ON_DBLCLICK"]))
                                {
                                    pnb.RollOnDblClick = (bool)r["ROLL_ON_DBLCLICK"];
                                }
                                if (!IsDBNull(r["FIT_WHEN_ROLLED"]))
                                {
                                    pnb.FitWhenRolled = (bool)r["FIT_WHEN_ROLLED"];
                                }
                                if (!IsDBNull(r["SHOW_SEPARATE_NOTES"]))
                                {
                                    pnb.ShowSeparateNotes = (bool)r["SHOW_SEPARATE_NOTES"];
                                }
                                if (!IsDBNull(r["PIN_CLICK_ACTION"]))
                                {
                                    var index = (int)r["PIN_CLICK_ACTION"];
                                    if (index < Enum.GetValues(typeof(PinClickAction)).Length)
                                        pnb.PinClickAction = (PinClickAction)index;
                                }
                                if (!IsDBNull(r["NOTE_START_POSITION"]))
                                {
                                    var index = (int)r["NOTE_START_POSITION"];
                                    if (index < Enum.GetValues(typeof(NoteStartPosition)).Length)
                                        pnb.StartPosition = (NoteStartPosition)index;
                                }
                                if (!IsDBNull(r["HIDE_MAIN_WINDOW"]))
                                {
                                    pnb.HideMainWindow = (bool)r["HIDE_MAIN_WINDOW"];
                                }
                                if (!IsDBNull(r["THEME"]))
                                {
                                    pnb.Theme = (string) r["THEME"];
                                }
                                else
                                {
                                    pnb.Theme = PNStrings.DEF_THEME;
                                }
                                if (!IsDBNull(r["PREVENT_RESIZING"]))
                                {
                                    pnb.PreventAutomaticResizing = (bool)r["PREVENT_RESIZING"];
                                }
                                if (!IsDBNull(r["SHOW_PANEL"]))
                                {
                                    pnb.ShowNotesPanel = (bool)r["SHOW_PANEL"];
                                }
                                if (!IsDBNull(r["PANEL_DOCK"]))
                                {
                                    pnb.NotesPanelOrientation = (NotesPanelOrientation)r["PANEL_DOCK"];
                                }
                                if (!IsDBNull(r["PANEL_AUTO_HIDE"]))
                                {
                                    pnb.PanelAutoHide = (bool)r["PANEL_AUTO_HIDE"];
                                }
                                if (!IsDBNull(r["PANEL_REMOVE_MODE"]))
                                {
                                    pnb.PanelRemoveMode = (PanelRemoveMode)r["PANEL_REMOVE_MODE"];
                                }
                                if (!IsDBNull(r["PANEL_SWITCH_OFF_ANIMATION"]))
                                {
                                    pnb.PanelSwitchOffAnimation = (bool)r["PANEL_SWITCH_OFF_ANIMATION"];
                                }
                                if (!IsDBNull(r["PANEL_ENTER_DELAY"]))
                                {
                                    pnb.PanelEnterDelay = (int)r["PANEL_ENTER_DELAY"];
                                }
                            }
                            else
                            {
                                sqlQuery = "INSERT INTO BEHAVIOR VALUES(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
                                oData.Execute(sqlQuery);
                            }
                        }
                    }
                    //protection
                    if (!oData.TableExists("PROTECTION"))
                    {
                        sqlQuery = "CREATE TABLE [PROTECTION] ([STORE_AS_ENCRYPTED] BOOLEAN, [HIDE_TRAY_ICON] BOOLEAN, [BACKUP_BEFORE_SAVING] BOOLEAN, [SILENT_FULL_BACKUP] BOOLEAN, [BACKUP_DEEPNESS] INT, [DO_NOT_SHOW_CONTENT] BOOLEAN, [INCLUDE_BIN_IN_SYNC] BOOLEAN, [PASSWORD_STRING] TEXT, [FULL_BACKUP_DAYS] TEXT, [FULL_BACKUP_TIME] TEXT, [FULL_BACKUP_DATE] TEXT, [PROMPT_PASSWORD] BOOLEAN)";
                        oData.Execute(sqlQuery);
                        sqlQuery = "INSERT INTO PROTECTION VALUES(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
                        oData.Execute(sqlQuery);
                    }
                    else
                    {
                        using (var t = oData.GetSchema("Columns"))
                        {
                            var rows = t.Select("COLUMN_NAME = 'FULL_BACKUP_DAYS' AND TABLE_NAME = 'PROTECTION'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE PROTECTION ADD COLUMN [FULL_BACKUP_DAYS] TEXT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'FULL_BACKUP_TIME' AND TABLE_NAME = 'PROTECTION'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE PROTECTION ADD COLUMN [FULL_BACKUP_TIME] TEXT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'FULL_BACKUP_DATE' AND TABLE_NAME = 'PROTECTION'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE PROTECTION ADD COLUMN [FULL_BACKUP_DATE] TEXT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'PROMPT_PASSWORD' AND TABLE_NAME = 'PROTECTION'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE PROTECTION ADD COLUMN [PROMPT_PASSWORD] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                        }
                        sqlQuery = "SELECT * FROM PROTECTION";
                        using (var t = oData.FillDataTable(sqlQuery))
                        {
                            if (t.Rows.Count > 0)
                            {
                                var r = t.Rows[0];
                                var pnp = PNStatic.Settings.Protection;
                                if (!IsDBNull(r["STORE_AS_ENCRYPTED"]))
                                {
                                    pnp.StoreAsEncrypted = (bool)r["STORE_AS_ENCRYPTED"];
                                }
                                if (!IsDBNull(r["HIDE_TRAY_ICON"]))
                                {
                                    pnp.HideTrayIcon = (bool)r["HIDE_TRAY_ICON"];
                                }
                                if (!IsDBNull(r["BACKUP_BEFORE_SAVING"]))
                                {
                                    pnp.BackupBeforeSaving = (bool)r["BACKUP_BEFORE_SAVING"];
                                }
                                if (!IsDBNull(r["SILENT_FULL_BACKUP"]))
                                {
                                    pnp.SilentFullBackup = (bool)r["SILENT_FULL_BACKUP"];
                                }
                                if (!IsDBNull(r["BACKUP_DEEPNESS"]))
                                {
                                    pnp.BackupDeepness = (int)r["BACKUP_DEEPNESS"];
                                }
                                if (!IsDBNull(r["DO_NOT_SHOW_CONTENT"]))
                                {
                                    pnp.DontShowContent = (bool)r["DO_NOT_SHOW_CONTENT"];
                                }
                                if (!IsDBNull(r["INCLUDE_BIN_IN_SYNC"]))
                                {
                                    pnp.IncludeBinInSync = (bool)r["INCLUDE_BIN_IN_SYNC"];
                                }
                                if (!IsDBNull(r["PASSWORD_STRING"]))
                                {
                                    pnp.PasswordString = (string)r["PASSWORD_STRING"];
                                }
                                if (!IsDBNull(r["FULL_BACKUP_DAYS"]))
                                {
                                    var temp = Convert.ToString(r["FULL_BACKUP_DAYS"]);
                                    if (!string.IsNullOrEmpty(temp))
                                    {
                                        var days = temp.Split(',');
                                        foreach (var d in days)
                                        {
                                            pnp.FullBackupDays.Add((DayOfWeek)Convert.ToInt32(d));
                                        }
                                    }
                                }
                                if (!IsDBNull(r["FULL_BACKUP_TIME"]))
                                {
                                    pnp.FullBackupTime = DateTime.Parse((string)r["FULL_BACKUP_TIME"], PNStatic.CultureInvariant);
                                }
                                if (!IsDBNull(r["FULL_BACKUP_DATE"]))
                                {
                                    pnp.FullBackupDate = DateTime.Parse((string)r["FULL_BACKUP_DATE"], PNStatic.CultureInvariant);
                                }
                                if (!IsDBNull(r["PROMPT_PASSWORD"]))
                                {
                                    pnp.PromptForPassword = (bool)r["PROMPT_PASSWORD"];
                                }
                            }
                            else
                            {
                                sqlQuery = "INSERT INTO PROTECTION VALUES(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
                                oData.Execute(sqlQuery);
                            }
                        }
                    }
                    //diary
                    if (!oData.TableExists("DIARY"))
                    {
                        sqlQuery = "CREATE TABLE [DIARY] ([CUSTOM_SETTINGS] BOOLEAN, [ADD_WEEKDAY] BOOLEAN, [FULL_WEEKDAY_NAME] BOOLEAN, [WEEKDAY_AT_THE_END] BOOLEAN, [DO_NOT_SHOW_PREVIOUS] BOOLEAN, [ASC_ORDER] BOOLEAN, [NUMBER_OF_PAGES] INT, [DATE_FORMAT] TEXT)";
                        oData.Execute(sqlQuery);
                        sqlQuery = "INSERT INTO DIARY VALUES(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
                        oData.Execute(sqlQuery);
                    }
                    else
                    {
                        sqlQuery = "SELECT * FROM DIARY";
                        using (var t = oData.FillDataTable(sqlQuery))
                        {
                            if (t.Rows.Count > 0)
                            {
                                var r = t.Rows[0];
                                var pndr = PNStatic.Settings.Diary;
                                if (!IsDBNull(r["CUSTOM_SETTINGS"]))
                                {
                                    pndr.CustomSettings = (bool)r["CUSTOM_SETTINGS"];
                                }
                                if (!IsDBNull(r["ADD_WEEKDAY"]))
                                {
                                    pndr.AddWeekday = (bool)r["ADD_WEEKDAY"];
                                }
                                if (!IsDBNull(r["FULL_WEEKDAY_NAME"]))
                                {
                                    pndr.FullWeekdayName = (bool)r["FULL_WEEKDAY_NAME"];
                                }
                                if (!IsDBNull(r["WEEKDAY_AT_THE_END"]))
                                {
                                    pndr.WeekdayAtTheEnd = (bool)r["WEEKDAY_AT_THE_END"];
                                }
                                if (!IsDBNull(r["DO_NOT_SHOW_PREVIOUS"]))
                                {
                                    pndr.DoNotShowPrevious = (bool)r["DO_NOT_SHOW_PREVIOUS"];
                                }
                                if (!IsDBNull(r["ASC_ORDER"]))
                                {
                                    pndr.AscendingOrder = (bool)r["ASC_ORDER"];
                                }
                                if (!IsDBNull(r["NUMBER_OF_PAGES"]))
                                {
                                    pndr.NumberOfPages = (int)r["NUMBER_OF_PAGES"];
                                }
                                if (!IsDBNull(r["DATE_FORMAT"]))
                                {
                                    pndr.DateFormat = (string)r["DATE_FORMAT"];
                                }
                            }
                            else
                            {
                                sqlQuery = "INSERT INTO DIARY VALUES(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
                                oData.Execute(sqlQuery);
                            }
                        }
                    }
                    //network
                    if (!oData.TableExists("NETWORK"))
                    {
                        sqlQuery = "CREATE TABLE [NETWORK] ([INCLUDE_BIN_IN_SYNC] BOOLEAN, [SYNC_ON_START] BOOLEAN, [SAVE_BEFORE_SYNC] BOOLEAN, [ENABLE_EXCHANGE] BOOLEAN, [SAVE_BEFORE_SEND] BOOLEAN, [NO_NOTIFY_ON_ARRIVE] BOOLEAN, [SHOW_RECEIVED_ON_CLICK] BOOLEAN, [SHOW_INCOMING_ON_CLICK] BOOLEAN, [NO_SOUND_ON_ARRIVE] BOOLEAN, [NO_NOTIFY_ON_SEND] BOOLEAN, [SHOW_AFTER_ARRIVE] BOOLEAN, [HIDE_AFTER_SEND] BOOLEAN, [NO_CONTACTS_IN_CONTEXT_MENU] BOOLEAN, [EXCHANGE_PORT] INT, [POST_COUNT] INT, [ALLOW_PING] BOOLEAN, [RECEIVED_ON_TOP] BOOLEAN)";
                        oData.Execute(sqlQuery);
                        sqlQuery = "INSERT INTO NETWORK VALUES(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
                        oData.Execute(sqlQuery);
                    }
                    else
                    {
                        using (var t = oData.GetSchema("Columns"))
                        {
                            var rows = t.Select("COLUMN_NAME = 'POST_COUNT' AND TABLE_NAME = 'NETWORK'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NETWORK ADD COLUMN [POST_COUNT] INT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'ALLOW_PING' AND TABLE_NAME = 'NETWORK'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NETWORK ADD COLUMN [ALLOW_PING] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'RECEIVED_ON_TOP' AND TABLE_NAME = 'NETWORK'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NETWORK ADD COLUMN [RECEIVED_ON_TOP] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                        }
                        sqlQuery = "SELECT * FROM NETWORK";
                        using (var t = oData.FillDataTable(sqlQuery))
                        {
                            if (t.Rows.Count > 0)
                            {
                                var r = t.Rows[0];
                                var pnw = PNStatic.Settings.Network;
                                if (!IsDBNull(r["INCLUDE_BIN_IN_SYNC"]))
                                {
                                    pnw.IncludeBinInSync = (bool)r["INCLUDE_BIN_IN_SYNC"];
                                }
                                if (!IsDBNull(r["SYNC_ON_START"]))
                                {
                                    pnw.SyncOnStart = (bool)r["SYNC_ON_START"];
                                }
                                if (!IsDBNull(r["SAVE_BEFORE_SYNC"]))
                                {
                                    pnw.SaveBeforeSync = (bool)r["SAVE_BEFORE_SYNC"];
                                }
                                if (!IsDBNull(r["ENABLE_EXCHANGE"]))
                                {
                                    pnw.EnableExchange = (bool)r["ENABLE_EXCHANGE"];
                                }
                                if (!IsDBNull(r["SAVE_BEFORE_SEND"]))
                                {
                                    pnw.SaveBeforeSending = (bool)r["SAVE_BEFORE_SEND"];
                                }
                                if (!IsDBNull(r["NO_NOTIFY_ON_ARRIVE"]))
                                {
                                    pnw.NoNotificationOnArrive = (bool)r["NO_NOTIFY_ON_ARRIVE"];
                                }
                                if (!IsDBNull(r["SHOW_RECEIVED_ON_CLICK"]))
                                {
                                    pnw.ShowReceivedOnClick = (bool)r["SHOW_RECEIVED_ON_CLICK"];
                                }
                                if (!IsDBNull(r["SHOW_INCOMING_ON_CLICK"]))
                                {
                                    pnw.ShowIncomingOnClick = (bool)r["SHOW_INCOMING_ON_CLICK"];
                                }
                                if (!IsDBNull(r["NO_SOUND_ON_ARRIVE"]))
                                {
                                    pnw.NoSoundOnArrive = (bool)r["NO_SOUND_ON_ARRIVE"];
                                }
                                if (!IsDBNull(r["NO_NOTIFY_ON_SEND"]))
                                {
                                    pnw.NoNotificationOnSend = (bool)r["NO_NOTIFY_ON_SEND"];
                                }
                                if (!IsDBNull(r["SHOW_AFTER_ARRIVE"]))
                                {
                                    pnw.ShowAfterArrive = (bool)r["SHOW_AFTER_ARRIVE"];
                                }
                                if (!IsDBNull(r["HIDE_AFTER_SEND"]))
                                {
                                    pnw.HideAfterSending = (bool)r["HIDE_AFTER_SEND"];
                                }
                                if (!IsDBNull(r["NO_CONTACTS_IN_CONTEXT_MENU"]))
                                {
                                    pnw.NoContactsInContextMenu = (bool)r["NO_CONTACTS_IN_CONTEXT_MENU"];
                                }
                                if (!IsDBNull(r["EXCHANGE_PORT"]))
                                {
                                    pnw.ExchangePort = (int)r["EXCHANGE_PORT"];
                                }
                                if (!IsDBNull(r["POST_COUNT"]))
                                {
                                    pnw.PostCount = (int)r["POST_COUNT"];
                                }
                                if (!IsDBNull(r["ALLOW_PING"]))
                                {
                                    pnw.AllowPing = (bool)r["ALLOW_PING"];
                                }
                                if (!IsDBNull(r["RECEIVED_ON_TOP"]))
                                {
                                    pnw.ReceivedOnTop = (bool)r["RECEIVED_ON_TOP"];
                                }
                            }
                            else
                            {
                                sqlQuery = "INSERT INTO NETWORK VALUES(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";
                                oData.Execute(sqlQuery);
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

        internal static void AddNewTags(IEnumerable<string> tags)
        {
            try
            {
                using (var oData = new SQLiteDataObject(ConnectionString))
                {
                    foreach (var t in tags)
                    {
                        var sb = new StringBuilder("SELECT TAG FROM TAGS WHERE TAG = '");
                        sb.Append(t.Replace("'", "''"));
                        sb.Append("'");
                        var obj = oData.GetScalar(sb.ToString());
                        if (obj != null && !IsDBNull(obj)) continue;
                        sb = new StringBuilder("INSERT INTO TAGS VALUES('");
                        sb.Append(t.Replace("'", "''"));
                        sb.Append("')");
                        oData.Execute(sb.ToString());
                        PNStatic.Tags.Add(t);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        //TODO
        //internal static void SaveGridColumnSort(DataGridViewColumn column, GRDSort sort)
        //{
        //    try
        //    {
        //        string tableName = column.DataGridView.Name == "grdNotes" ? "GRD_NOTES_COLUMNS" : "GRD_BACK_COLUMNS";
        //        var sb = new StringBuilder();
        //        sb.Append("UPDATE ");
        //        sb.Append(tableName);
        //        sb.Append(" SET CN_SORT_ORDER = 0, CN_LAST_SORTED = 0; ");
        //        sb.Append("UPDATE ");
        //        sb.Append(tableName);
        //        sb.Append(" SET CN_SORT_ORDER = ");
        //        sb.Append(Convert.ToInt32(sort.SortOrder));
        //        sb.Append(", CN_LAST_SORTED = ");
        //        sb.Append(Convert.ToInt32(sort.LastSorted));
        //        sb.Append(" WHERE CN_KEY = '");
        //        sb.Append(sort.Key);
        //        sb.Append("'");
        //        ExecuteTransactionForStringBuilder(sb, SettingsConnectionString);
        //    }
        //    catch (Exception ex)
        //    {
        //        PNStatic.LogException(ex);
        //    }
        //}

        internal static void SaveGridColumnVisibility(FixedWidthColumn column, string gridName)
        {
            try
            {
                var tableName = gridName == "grdNotes" ? "GRD_NOTES_COLUMNS" : "GRD_BACK_COLUMNS";
                var sb = new StringBuilder();
                sb.Append("UPDATE ");
                sb.Append(tableName);
                sb.Append(" SET CN_VISIBILITY = ");
                sb.Append(column.Visibility == Visibility.Visible ? 1 : 0);
                sb.Append(" WHERE CN_KEY = '");
                sb.Append(PNGridViewHelper.GetColumnName(column));
                sb.Append("'");
                ExecuteTransactionForStringBuilder(sb, SettingsConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveGridColumnsWidth(IEnumerable<ColumnProps> cols, string gridName)
        {
            try
            {
                var tableName = gridName == "grdNotes" ? "GRD_NOTES_COLUMNS" : "GRD_BACK_COLUMNS";
                foreach (var col in cols)
                {
                    var sb = new StringBuilder();
                    sb.Append("UPDATE ");
                    sb.Append(tableName);
                    sb.Append(" SET CN_DISPLAY_WIDTH = ");
                    sb.Append(Convert.ToInt32(col.Width));
                    sb.Append(" WHERE CN_KEY = '");
                    sb.Append(col.Name);
                    sb.Append("'");
                    ExecuteTransactionForStringBuilder(sb, SettingsConnectionString);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveGridColumnsOrder(GridViewColumnCollection cols, string gridName)
        {
            try
            {
                var sqlList = new List<string>();
                var tableName = gridName == "grdNotes" ? "GRD_NOTES_COLUMNS" : "GRD_BACK_COLUMNS";
                for (var i = 0; i < cols.Count; i++)
                {
                    var sb = new StringBuilder();
                    sb.Append("UPDATE ");
                    sb.Append(tableName);
                    sb.Append(" SET CN_DISPLAY_INDEX = ");
                    sb.Append(i);
                    sb.Append(" WHERE CN_KEY = '");
                    sb.Append(PNGridViewHelper.GetColumnName(cols[i]));
                    sb.Append("'");
                    sqlList.Add(sb.ToString());
                }
                ExecuteTransactionForList(sqlList, SettingsConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ResetGridColumns()
        {
            try
            {
                var sqlList = new List<string>
                    {
                        "UPDATE GRD_NOTES_COLUMNS SET CN_VISIBILITY = 1, CN_DISPLAY_INDEX = CN_ORIGINAL_INDEX, CN_DISPLAY_WIDTH = CN_ORIGINAL_WIDTH",
                        "UPDATE GRD_BACK_COLUMNS SET CN_VISIBILITY = 1, CN_DISPLAY_INDEX = CN_ORIGINAL_INDEX, CN_DISPLAY_WIDTH = CN_ORIGINAL_WIDTH"
                    };
                ExecuteTransactionForList(sqlList, SettingsConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void LoadGridColumns(GridView grd, List<ColumnProps> cols, List<GRDSort> sortColumns, string gridName)
        {
            try
            {
                cols.Clear();
                var sqlList = new List<string>();
                var tableName = gridName == "grdNotes" ? "GRD_NOTES_COLUMNS" : "GRD_BACK_COLUMNS";
                using (var oData = new SQLiteDataObject(SettingsConnectionString))
                {
                    if (oData.TableExists(tableName))
                    {
                        // check for "encrypted" column
                        if (gridName == "grdNotes")
                        {
                            var obj =
                                oData.GetScalar("SELECT CN_KEY FROM GRD_NOTES_COLUMNS WHERE CN_KEY = 'Note_Encrypted'");
                            if (obj == null || IsDBNull(obj))
                            {
                                oData.Execute(
                                    "UPDATE GRD_NOTES_COLUMNS SET CN_DISPLAY_INDEX = CN_ORIGINAL_INDEX");
                                oData.Execute(
                                    "UPDATE GRD_NOTES_COLUMNS SET CN_ORIGINAL_INDEX = CN_ORIGINAL_INDEX + 1, CN_DISPLAY_INDEX = CN_DISPLAY_INDEX + 1 WHERE CN_ORIGINAL_INDEX >= 9");
                                oData.Execute(
                                    "INSERT INTO GRD_NOTES_COLUMNS VALUES('Note_Encrypted', 1, 9, 9, 32, 32, NULL, NULL)");
                            }
                            obj =
                                oData.GetScalar("SELECT CN_KEY FROM GRD_NOTES_COLUMNS WHERE CN_KEY = 'Note_PrevGroup'");
                            if (obj == null || IsDBNull(obj))
                            {
                                oData.Execute(
                                    "UPDATE GRD_NOTES_COLUMNS SET CN_DISPLAY_INDEX = CN_ORIGINAL_INDEX");
                                oData.Execute(
                                    "UPDATE GRD_NOTES_COLUMNS SET CN_ORIGINAL_INDEX = CN_ORIGINAL_INDEX + 1, CN_DISPLAY_INDEX = CN_DISPLAY_INDEX + 1 WHERE CN_ORIGINAL_INDEX >= 11");
                                oData.Execute(
                                    "INSERT INTO GRD_NOTES_COLUMNS VALUES('Note_Encrypted', 1, 11, 11, 32, 32, NULL, NULL)");
                            }
                        }
                        using (var t = oData.FillDataTable("SELECT * FROM " + tableName))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                var column =
                                    grd.Columns.Where(t1 => PNGridViewHelper.GetColumnName(t1) == (string)r["CN_KEY"])
                                        .Cast<FixedWidthColumn>()
                                        .FirstOrDefault();
                                if (column == null) continue;

                                if (!Convert.ToBoolean(r["CN_VISIBILITY"]))
                                {
                                    column.Visibility = Visibility.Hidden;
                                    column.Width = column.ActualWidth;
                                }
                                else
                                {
                                    column.FixedWidth = Convert.ToInt32(r["CN_DISPLAY_WIDTH"]);
                                    column.Width = column.FixedWidth;
                                }
                                column.DisplayIndex = Convert.ToInt32(r["CN_DISPLAY_INDEX"]);
                                column.OriginalIndex = Convert.ToInt32(r["CN_ORIGINAL_INDEX"]);

                                cols.Add(new ColumnProps
                                {
                                    Name = (string)r["CN_KEY"],
                                    Visibility = column.Visibility,
                                    Width = Convert.ToInt32(r["CN_DISPLAY_WIDTH"])
                                });

                                var sort = new GRDSort { Key = (string)r["CN_KEY"] };
                                if (!IsDBNull(r["CN_SORT_ORDER"]))
                                {
                                    sort.SortOrder = (ListSortDirection)Convert.ToInt32(r["CN_SORT_ORDER"]);
                                }
                                if (!IsDBNull(r["CN_LAST_SORTED"]))
                                {
                                    sort.LastSorted = Convert.ToBoolean(r["CN_LAST_SORTED"]);
                                }
                                sortColumns.Add(sort);
                            }
                        }
                    }
                    else
                    {
                        sqlList.Add(
                            "CREATE TABLE [" + tableName + "] ([CN_KEY] TEXT PRIMARY KEY NOT NULL UNIQUE, [CN_VISIBILITY] BOOLEAN NOT NULL, [CN_ORIGINAL_INDEX] INT NOT NULL, [CN_DISPLAY_INDEX] INT NOT NULL, [CN_ORIGINAL_WIDTH] INT NOT NULL, [CN_DISPLAY_WIDTH] INT NOT NULL, [CN_SORT_ORDER] INT, [CN_LAST_SORTED] BOOLEAN)");
                        for (var i = 0; i < grd.Columns.Count; i++)
                        {
                            var c = (FixedWidthColumn)grd.Columns[i];
                            var sb = new StringBuilder();
                            sb.Append(
                                "INSERT INTO " + tableName + " (CN_KEY, CN_VISIBILITY, CN_ORIGINAL_INDEX, CN_DISPLAY_INDEX, CN_ORIGINAL_WIDTH, CN_DISPLAY_WIDTH, CN_SORT_ORDER, CN_LAST_SORTED) VALUES(");
                            sb.Append("'");
                            sb.Append(PNGridViewHelper.GetColumnName(c));
                            sb.Append("', ");
                            sb.Append(1);
                            sb.Append(", ");
                            sb.Append(i);
                            sb.Append(", ");
                            sb.Append(i);
                            sb.Append(", ");
                            sb.Append(c.FixedWidth);
                            sb.Append(", ");
                            sb.Append(c.FixedWidth);
                            sb.Append(", NULL, NULL)");
                            sqlList.Add(sb.ToString());

                            cols.Add(new ColumnProps
                            {
                                Name = PNGridViewHelper.GetColumnName(c),
                                Visibility = Visibility.Visible,
                                Width = c.FixedWidth
                            });
                        }
                        ExecuteTransactionForList(sqlList, SettingsConnectionString);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveLastPage()
        {
            try
            {
                using (var oData = new SQLiteDataObject(SettingsConnectionString))
                {
                    var sqlQuery = "UPDATE CONFIG SET LAST_PAGE = " + PNStatic.Settings.Config.LastPage.ToString(PNStatic.CultureInvariant);
                    oData.Execute(sqlQuery);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveControlsStyle(bool isDeafault)
        {
            try
            {
                using (var oData = new SQLiteDataObject(SettingsConnectionString))
                {
                    var sqlQuery = !isDeafault
                                          ? "UPDATE CONFIG SET CONTROLS_STYLE = '" +
                                            PNStatic.Settings.Config.ControlsStyle +
                                            "'"
                                          : "UPDATE CONFIG SET CONTROLS_STYLE = NULL";
                    oData.Execute(sqlQuery);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void DeleteGroups(List<int> ids)
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var id in ids)
                {
                    sb.Append("DELETE FROM GROUPS WHERE GROUP_ID = ");
                    sb.Append(id);
                    sb.Append("; ");
                    sb.AppendLine();
                    sb.Append("DELETE FROM HOT_KEYS WHERE MENU_NAME = '");
                    sb.Append(id.ToString(PNStatic.CultureInvariant) + "_show';");
                    sb.AppendLine();
                    sb.Append("DELETE FROM HOT_KEYS WHERE MENU_NAME = '");
                    sb.Append(id.ToString(PNStatic.CultureInvariant) + "_hide';");
                }
                if (ExecuteTransactionForStringBuilder(sb, ConnectionString))
                {
                    foreach (var id in ids)
                    {
                        var prefix = id.ToString(PNStatic.CultureInvariant) + "_";
                        PNStatic.HotKeysGroups.RemoveAll(hk => hk.MenuName.StartsWith(prefix));
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveGroupNewParent(PNGroup group)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("UPDATE GROUPS SET PARENT_ID = ");
                sb.Append(group.ParentID);
                sb.Append(" WHERE GROUP_ID = ");
                sb.Append(group.ID);
                ExecuteTransactionForStringBuilder(sb, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveGroupPassword(PNGroup group)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("UPDATE GROUPS SET PASSWORD_STRING = '");
                sb.Append(group.PasswordString);
                sb.Append("' WHERE GROUP_ID = ");
                sb.Append(group.ID);
                ExecuteTransactionForStringBuilder(sb, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveGroupChanges(PNGroup group)
        {
            try
            {
                var c = new ColorConverter();
                var dcc = new System.Drawing.ColorConverter();
                var lfc = new LogFontConverter();
                var wfc = new WPFFontConverter();
                var sb = new StringBuilder();
                sb.Append("UPDATE GROUPS SET GROUP_NAME = '");
                sb.Append(group.Name.Replace("'", "''"));
                sb.Append("', PARENT_ID = ");
                sb.Append(group.ParentID);
                sb.Append(", ICON = ");
                if (group.Image != null)
                {
                    sb.Append("'");
                    if (!group.IsDefaultImage)
                    {
                        var base64String = Convert.ToBase64String(group.Image.ToBytes());
                        sb.Append(base64String);
                    }
                    else
                    {
                        sb.Append(group.ImageName);
                        sb.Append(".png");
                    }
                    //using (var ms = new MemoryStream(1))
                    //{
                    //    group.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    //    ms.Position = 0;
                    //    string base64String = Convert.ToBase64String(ms.ToArray());
                    //    sb.Append(base64String);
                    //}
                    sb.Append("',");
                }
                else
                {
                    sb.Append("NULL,");
                }
                sb.Append(" BACK_COLOR = '");
                sb.Append(c.ConvertToString(null, PNStatic.CultureInvariant, group.Skinless.BackColor));
                sb.Append("', CAPTION_FONT_COLOR = '");
                sb.Append(c.ConvertToString(null, PNStatic.CultureInvariant, group.Skinless.CaptionColor));
                sb.Append("', CAPTION_FONT = '");
                sb.Append(wfc.ConvertToString(group.Skinless.CaptionFont));
                sb.Append("', SKIN_NAME = '");
                sb.Append(group.Skin.SkinName.Replace("'", "''"));
                sb.Append("', FONT_COLOR = '");
                sb.Append(dcc.ConvertToString(null, PNStatic.CultureInvariant, group.FontColor));
                sb.Append("', FONT = '");
                sb.Append(lfc.ConvertToString(group.Font));
                sb.Append("', IS_DEFAULT_IMAGE = ");
                sb.Append(Convert.ToInt32(group.IsDefaultImage));
                sb.Append(" WHERE GROUP_ID = ");
                sb.Append(group.ID);
                ExecuteTransactionForStringBuilder(sb, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static bool IsMediaColor(string colorString)
        {
            try
            {
                var mediaColorConverter = new ColorConverter();
                mediaColorConverter.ConvertFromString(null, PNStatic.CultureInvariant, colorString);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return true;
            }
        }

        internal static void NormalizeGroupsTable(SQLiteDataObject oData, bool updateIcons = true)
        {
            try
            {
                var listSql = new List<string>();
                var lfc = new LogFontConverter();
                var wfc = new WPFFontConverter();
                var drawingColorConverter = new System.Drawing.ColorConverter();
                var mediaColorConverter = new ColorConverter();

                if (updateIcons)
                {
                    using (
                        var t =
                            oData.FillDataTable(
                                "SELECT GROUP_ID, ICON FROM GROUPS WHERE ICON NOT LIKE '%.png' OR ICON LIKE 'resource.%'")
                        )
                    {
                        foreach (DataRow r in t.Rows)
                        {
                            var sb = new StringBuilder("UPDATE GROUPS SET ");
                            var icon = Convert.ToString(r["ICON"]);
                            if (icon.StartsWith("resource."))
                            {
                                var imageName = icon.Substring("resource.".Length);
                                imageName = imageName + ".png";
                                sb.Append("ICON = '");
                                sb.Append(imageName);
                                sb.Append("'");
                            }
                            else
                            {
                                sb.Append("IS_DEFAULT_IMAGE = 0");
                            }
                            sb.Append(" WHERE GROUP_ID = ");
                            sb.Append(r["GROUP_ID"]);
                            listSql.Add(sb.ToString());
                        }
                    }
                }
                using (
                    var t =
                        oData.FillDataTable(
                            "SELECT GROUP_ID, CAPTION_FONT FROM GROUPS WHERE CAPTION_FONT LIKE '%^%'"))
                {
                    foreach (DataRow r in t.Rows)
                    {
                        var lf = lfc.ConvertFromString((string)r["CAPTION_FONT"]);
                        var fnt = PNStatic.FromLogFont(lf);
                        var sb = new StringBuilder("UPDATE GROUPS SET CAPTION_FONT = '");
                        sb.Append(wfc.ConvertToString(null, PNStatic.CultureInvariant, fnt));
                        sb.Append("' WHERE GROUP_ID = ");
                        sb.Append(r["GROUP_ID"]);
                        listSql.Add(sb.ToString());
                    }
                }
                using (var t = oData.FillDataTable("SELECT GROUP_ID, BACK_COLOR, CAPTION_FONT_COLOR FROM GROUPS"))
                {
                    var backColors = new List<TempColor>();
                    var capColors = new List<TempColor>();
                    foreach (DataRow r in t.Rows)
                    {
                        if (!IsDBNull(r["BACK_COLOR"]))
                            backColors.Add(new TempColor
                            {
                                Color = Convert.ToString(r["BACK_COLOR"]),
                                Id = Convert.ToString(r["GROUP_ID"])
                            });
                        if (!IsDBNull(r["CAPTION_FONT_COLOR"]))
                            capColors.Add(new TempColor
                            {
                                Color = Convert.ToString(r["CAPTION_FONT_COLOR"]),
                                Id = Convert.ToString(r["GROUP_ID"])
                            });
                    }
                    foreach (var cr in backColors.Where(c => !IsMediaColor(c.Color)))
                    {
                        var convertFromString = drawingColorConverter.ConvertFromString(null, PNStatic.CultureInvariant, cr.Color);
                        if (convertFromString == null) continue;
                        var clr = (System.Drawing.Color)convertFromString;
                        var color = Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
                        var sb = new StringBuilder("UPDATE GROUPS SET BACK_COLOR = '");
                        sb.Append(mediaColorConverter.ConvertToString(null, PNStatic.CultureInvariant, color));
                        sb.Append("' WHERE GROUP_ID = ");
                        sb.Append(cr.Id);
                        listSql.Add(sb.ToString());
                    }
                    foreach (var cr in capColors.Where(c => !IsMediaColor(c.Color)))
                    {
                        var convertFromString = drawingColorConverter.ConvertFromString(null, PNStatic.CultureInvariant, cr.Color);
                        if (convertFromString == null) continue;
                        var clr = (System.Drawing.Color)convertFromString;
                        var color = Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
                        var sb = new StringBuilder("UPDATE GROUPS SET CAPTION_FONT_COLOR = '");
                        sb.Append(mediaColorConverter.ConvertToString(null, PNStatic.CultureInvariant, color));
                        sb.Append("' WHERE GROUP_ID = ");
                        sb.Append(cr.Id);
                        listSql.Add(sb.ToString());
                    }
                }

                if (listSql.Count == 0) return;
                ExecuteTransactionForList(listSql, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void NormalizeCustomNotesTable(SQLiteDataObject oData)
        {
            try
            {
                var listSql = new List<string>();
                var lfc = new LogFontConverter();
                var wfc = new WPFFontConverter();
                var drawingColorConverter = new System.Drawing.ColorConverter();
                var mediaColorConverter = new ColorConverter();

                using (
                        var t =
                            oData.FillDataTable(
                                "SELECT NOTE_ID, CAPTION_FONT FROM CUSTOM_NOTES_SETTINGS WHERE CAPTION_FONT LIKE '%^%'"))
                {
                    foreach (DataRow r in t.Rows)
                    {
                        var lf = lfc.ConvertFromString((string)r["CAPTION_FONT"]);
                        var fnt = PNStatic.FromLogFont(lf);
                        var sb = new StringBuilder("UPDATE CUSTOM_NOTES_SETTINGS SET CAPTION_FONT = '");
                        sb.Append(wfc.ConvertToString(null, PNStatic.CultureInvariant, fnt));
                        sb.Append("' WHERE NOTE_ID = ");
                        sb.Append(r["NOTE_ID"]);
                        listSql.Add(sb.ToString());
                    }
                }
                using (var t = oData.FillDataTable("SELECT NOTE_ID, BACK_COLOR, CAPTION_FONT_COLOR FROM CUSTOM_NOTES_SETTINGS"))
                {
                    var backColors = new List<TempColor>();
                    var capColors = new List<TempColor>();
                    foreach (DataRow r in t.Rows)
                    {
                        if (!IsDBNull(r["BACK_COLOR"]))
                            backColors.Add(new TempColor
                            {
                                Color = Convert.ToString(r["BACK_COLOR"]),
                                Id = Convert.ToString(r["NOTE_ID"])
                            });
                        if (!IsDBNull(r["CAPTION_FONT_COLOR"]))
                            capColors.Add(new TempColor
                            {
                                Color = Convert.ToString(r["CAPTION_FONT_COLOR"]),
                                Id = Convert.ToString(r["NOTE_ID"])
                            });
                    }
                    foreach (var cr in backColors.Where(c => !IsMediaColor(c.Color)))
                    {
                        var convertFromString = drawingColorConverter.ConvertFromString(null, PNStatic.CultureInvariant, cr.Color);
                        if (convertFromString == null) continue;
                        var clr = (System.Drawing.Color)convertFromString;
                        var color = Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
                        var sb = new StringBuilder("UPDATE CUSTOM_NOTES_SETTINGS SET BACK_COLOR = '");
                        sb.Append(mediaColorConverter.ConvertToString(null, PNStatic.CultureInvariant, color));
                        sb.Append("' WHERE NOTE_ID = ");
                        sb.Append(cr.Id);
                        listSql.Add(sb.ToString());
                    }
                    foreach (var cr in capColors.Where(c => !IsMediaColor(c.Color)))
                    {
                        var convertFromString = drawingColorConverter.ConvertFromString(null, PNStatic.CultureInvariant, cr.Color);
                        if (convertFromString == null) continue;
                        var clr = (System.Drawing.Color)convertFromString;
                        var color = Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
                        var sb = new StringBuilder("UPDATE CUSTOM_NOTES_SETTINGS SET CAPTION_FONT_COLOR = '");
                        sb.Append(mediaColorConverter.ConvertToString(null, PNStatic.CultureInvariant, color));
                        sb.Append("' WHERE NOTE_ID = ");
                        sb.Append(cr.Id);
                        listSql.Add(sb.ToString());
                    }
                }

                if (listSql.Count == 0) return;
                ExecuteTransactionForList(listSql, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void UpdateTablesAfterSync()
        {
            try
            {
                using (var oData = new SQLiteDataObject(ConnectionString))
                {
                    NormalizeGroupsTable(oData);
                    NormalizeCustomNotesTable(oData);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void InsertNewGroup(PNGroup group)
        {
            try
            {
                var c = new ColorConverter();
                var drcc = new System.Drawing.ColorConverter();
                var wfc = new WPFFontConverter();
                var lfc = new LogFontConverter();
                var sb = new StringBuilder();
                sb.Append("INSERT INTO GROUPS (GROUP_ID, PARENT_ID, GROUP_NAME, ICON, BACK_COLOR, CAPTION_FONT_COLOR, CAPTION_FONT, SKIN_NAME, PASSWORD_STRING, FONT, FONT_COLOR, IS_DEFAULT_IMAGE) VALUES(");
                sb.Append(group.ID);
                sb.Append(",");
                sb.Append(group.ParentID);
                sb.Append(",'");
                sb.Append(group.Name.Replace("'", "''"));
                sb.Append("','");
                if (!group.IsDefaultImage)
                {
                    var base64String = Convert.ToBase64String(group.Image.ToBytes());
                    sb.Append(base64String);
                }
                else
                {
                    sb.Append(group.ImageName);
                    sb.Append(".png");
                }
                //using (var ms = new MemoryStream(1))
                //{
                //    group.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                //    ms.Position = 0;
                //    string base64String = Convert.ToBase64String(ms.ToArray());
                //    sb.Append(base64String);
                //}
                sb.Append("','");
                sb.Append(c.ConvertToString(null, PNStatic.CultureInvariant, group.Skinless.BackColor));
                sb.Append("','");
                sb.Append(c.ConvertToString(null, PNStatic.CultureInvariant, group.Skinless.CaptionColor));
                sb.Append("','");
                sb.Append(wfc.ConvertToString(group.Skinless.CaptionFont));
                sb.Append("','");
                sb.Append(group.Skin.SkinName);
                sb.Append("','");
                sb.Append(group.PasswordString);
                sb.Append("','");
                sb.Append(lfc.ConvertToString(group.Font));
                sb.Append("','");
                sb.Append(drcc.ConvertToString(null, PNStatic.CultureInvariant, group.FontColor));
                sb.Append("', ");
                sb.Append(Convert.ToInt32(group.IsDefaultImage));
                sb.Append("); ");
                if (ExecuteTransactionForStringBuilder(sb, ConnectionString))
                {
                    sb = new StringBuilder();
                    var id = HK_START;
                    using (var oData = new SQLiteDataObject(ConnectionString))
                    {
                        var o = oData.GetScalar("SELECT MAX(ID) FROM HOT_KEYS");
                        if (o != null && !DBNull.Value.Equals(o))
                        {
                            id = (int)(long)o + 1;
                        }
                    }
                    var prefix = group.ID + "_show";
                    sb.Append("INSERT INTO HOT_KEYS (HK_TYPE, MENU_NAME, ID, SHORTCUT) VALUES(");
                    sb.Append(((int)HotkeyType.Group).ToString(PNStatic.CultureInvariant));
                    sb.Append(",'");
                    sb.Append(prefix);
                    sb.Append("',");
                    sb.Append(id.ToString(PNStatic.CultureInvariant));
                    sb.Append(",'');");
                    if (ExecuteTransactionForStringBuilder(sb, ConnectionString))
                    {
                        PNStatic.HotKeysGroups.Add(new PNHotKey { MenuName = prefix, ID = id, Type = HotkeyType.Group });
                    }
                    sb = new StringBuilder();
                    id++;
                    prefix = group.ID + "_hide";
                    sb.Append("INSERT INTO HOT_KEYS (HK_TYPE, MENU_NAME, ID, SHORTCUT) VALUES(");
                    sb.Append(((int)HotkeyType.Group).ToString(PNStatic.CultureInvariant));
                    sb.Append(",'");
                    sb.Append(prefix);
                    sb.Append("',");
                    sb.Append(id.ToString(PNStatic.CultureInvariant));
                    sb.Append(",'');");
                    if (ExecuteTransactionForStringBuilder(sb, ConnectionString))
                    {
                        PNStatic.HotKeysGroups.Add(new PNHotKey { MenuName = prefix, ID = id, Type = HotkeyType.Group });
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void InsertDefaultGroup(int id, int parentID, string name, BitmapImage image, ImageFormat imageFormat)
        {
            try
            {
                var c = new ColorConverter();
                var wpfFontConverter = new WPFFontConverter();
                var sb = new StringBuilder();
                sb.Append("INSERT INTO GROUPS (GROUP_ID, PARENT_ID, GROUP_NAME, ICON, BACK_COLOR, CAPTION_FONT_COLOR, CAPTION_FONT, SKIN_NAME, PASSWORD_STRING, IS_DEFAULT_IMAGE) VALUES(");
                sb.Append(id);
                sb.Append(",");
                sb.Append(parentID);
                sb.Append(",'");
                sb.Append(name.Replace("'", "''"));
                sb.Append("','");
                var base64String = Convert.ToBase64String(image.ToBytes());
                sb.Append(base64String);
                //using (var ms = new MemoryStream(1))
                //{
                //    image.Save(ms, imageFormat);
                //    ms.Position = 0;
                //    string base64String = Convert.ToBase64String(ms.ToArray());
                //    sb.Append(base64String);
                //}
                sb.Append("','");
                sb.Append(c.ConvertToString(null, PNStatic.CultureInvariant, PNSkinlessDetails.DefColor));
                sb.Append("','");
                sb.Append(c.ConvertToString(null, PNStatic.CultureInvariant, SystemColors.ControlTextColor));
                sb.Append("','");
                var f = new PNFont { FontWeight = FontWeights.Bold };
                sb.Append(wpfFontConverter.ConvertToString(f));
                sb.Append("','");
                sb.Append(PNSkinDetails.NO_SKIN);
                sb.Append("','',1");
                sb.Append("); ");
                ExecuteTransactionForStringBuilder(sb, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static int GetNewGroupID()
        {
            using (var oData = new SQLiteDataObject(ConnectionString))
            {
                var o = oData.GetScalar("SELECT MAX(GROUP_ID) FROM GROUPS");
                if (o != null && !IsDBNull(o))
                {
                    return Convert.ToInt32(o) + 1;
                }
            }
            return 1;
        }

        internal static void SaveFontUi()
        {
            try
            {
                var fc = new WPFFontConverter();
                var fontString = fc.ConvertToString(PNSingleton.Instance.FontUser);
                var sb = new StringBuilder("UPDATE CONFIG SET UI_FONT = '");
                sb.Append(fontString);
                sb.Append("'");
                ExecuteTransactionForStringBuilder(sb, SettingsConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveSearchNotesSettings()
        {
            try
            {
                var pcf = PNStatic.Settings.Config;
                var sb = new StringBuilder("UPDATE CONFIG SET SEARCH_NOTES_SETT = '");
                sb.Append(pcf.SearchNotesSettings);
                sb.Append("'");
                ExecuteTransactionForStringBuilder(sb, SettingsConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveCPProperties()
        {
            try
            {
                var crc = new ColorConverter();
                var sb = new StringBuilder();
                var sc = new SizeConverter();
                var pc = new PointConverter();

                var pcf = PNStatic.Settings.Config;
                sb.Append("UPDATE CONFIG SET CP_LAST_GROUP = ");
                sb.Append(pcf.CPLastGroup);
                sb.Append(",CP_PVW_COLOR = '");
                sb.Append(crc.ConvertToString(null, PNStatic.CultureInvariant, pcf.CPPvwColor));
                sb.Append("',CP_USE_CUST_PVW_COLOR = ");
                sb.Append(Convert.ToInt32(pcf.CPUseCustPvwColor));
                sb.Append(",CP_SIZE = '");
                sb.Append(sc.ConvertToString(null, PNStatic.CultureInvariant, pcf.CPSize));
                sb.Append("',CP_LOCATION = '");
                sb.Append(pc.ConvertToString(null, PNStatic.CultureInvariant, pcf.CPLocation));
                sb.Append("',CP_PVW_RIGHT = ");
                sb.Append(Convert.ToInt32(pcf.CPPvwRight));
                sb.Append(",CP_PVW_SHOW = ");
                sb.Append(Convert.ToInt32(pcf.CPPvwShow));
                sb.Append(",CP_GROUPS_SHOW = ");
                sb.Append(Convert.ToInt32(pcf.CPGroupsShow));
                ExecuteTransactionForStringBuilder(sb, SettingsConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SavePassword()
        {
            try
            {
                var sb = new StringBuilder();
                var pnp = PNStatic.Settings.Protection;
                sb.Append("UPDATE PROTECTION SET PASSWORD_STRING = '");
                sb.Append(pnp.PasswordString.Replace("'", "''"));
                sb.Append("'");
                ExecuteTransactionForStringBuilder(sb, SettingsConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveProtectionSettings()
        {
            try
            {
                var sb = new StringBuilder();
                var sbDays = new StringBuilder();
                var pnp = PNStatic.Settings.Protection;
                sb.Append("UPDATE PROTECTION SET STORE_AS_ENCRYPTED = ");
                sb.Append(Convert.ToInt32(pnp.StoreAsEncrypted));
                sb.Append(", HIDE_TRAY_ICON = ");
                sb.Append(Convert.ToInt32(pnp.HideTrayIcon));
                sb.Append(", BACKUP_BEFORE_SAVING = ");
                sb.Append(Convert.ToInt32(pnp.BackupBeforeSaving));
                sb.Append(", SILENT_FULL_BACKUP = ");
                sb.Append(Convert.ToInt32(pnp.SilentFullBackup));
                sb.Append(", BACKUP_DEEPNESS = ");
                sb.Append(pnp.BackupDeepness);
                sb.Append(", DO_NOT_SHOW_CONTENT = ");
                sb.Append(Convert.ToInt32(pnp.DontShowContent));
                sb.Append(", INCLUDE_BIN_IN_SYNC = ");
                sb.Append(Convert.ToInt32(pnp.IncludeBinInSync));
                sb.Append(", PASSWORD_STRING = '");
                sb.Append(pnp.PasswordString.Replace("'", "''"));
                sb.Append("', FULL_BACKUP_DAYS = '");
                foreach (var d in pnp.FullBackupDays)
                {
                    sbDays.Append(Convert.ToInt32(d));
                    sbDays.Append(",");
                }
                if (sbDays.Length > 0)
                {
                    sbDays.Length -= 1;
                    sb.Append(sbDays);
                }
                sb.Append("', FULL_BACKUP_TIME = ");
                if (pnp.FullBackupTime != DateTime.MinValue)
                {
                    sb.Append("'");
                    sb.Append(pnp.FullBackupTime.ToString("HH:mm", PNStatic.CultureInvariant));
                    sb.Append("'");
                }
                else
                {
                    sb.Append("NULL");
                }
                sb.Append(", FULL_BACKUP_DATE = ");
                if (pnp.FullBackupDate != DateTime.MinValue)
                {
                    sb.Append("'");
                    sb.Append(pnp.FullBackupDate.ToString("dd MMM yyyy HH:mm", PNStatic.CultureInvariant));
                    sb.Append("'");
                }
                else
                {
                    sb.Append("NULL");
                }
                sb.Append(", PROMPT_PASSWORD = ");
                sb.Append(Convert.ToInt32(pnp.PromptForPassword));
                ExecuteTransactionForStringBuilder(sb, SettingsConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveNetworkSettings()
        {
            try
            {
                var sb = new StringBuilder();
                var pnw = PNStatic.Settings.Network;
                sb.Append("UPDATE NETWORK SET INCLUDE_BIN_IN_SYNC = ");
                sb.Append(Convert.ToInt32(pnw.IncludeBinInSync));
                sb.Append(", SYNC_ON_START = ");
                sb.Append(Convert.ToInt32(pnw.SyncOnStart));
                sb.Append(", SAVE_BEFORE_SYNC = ");
                sb.Append(Convert.ToInt32(pnw.SaveBeforeSync));
                sb.Append(", ENABLE_EXCHANGE = ");
                sb.Append(Convert.ToInt32(pnw.EnableExchange));
                sb.Append(", SAVE_BEFORE_SEND = ");
                sb.Append(Convert.ToInt32(pnw.SaveBeforeSending));
                sb.Append(", NO_NOTIFY_ON_ARRIVE = ");
                sb.Append(Convert.ToInt32(pnw.NoNotificationOnArrive));
                sb.Append(", SHOW_RECEIVED_ON_CLICK = ");
                sb.Append(Convert.ToInt32(pnw.ShowReceivedOnClick));
                sb.Append(", SHOW_INCOMING_ON_CLICK = ");
                sb.Append(Convert.ToInt32(pnw.ShowIncomingOnClick));
                sb.Append(", NO_SOUND_ON_ARRIVE = ");
                sb.Append(Convert.ToInt32(pnw.NoSoundOnArrive));
                sb.Append(", NO_NOTIFY_ON_SEND = ");
                sb.Append(Convert.ToInt32(pnw.NoNotificationOnSend));
                sb.Append(", SHOW_AFTER_ARRIVE = ");
                sb.Append(Convert.ToInt32(pnw.ShowAfterArrive));
                sb.Append(", HIDE_AFTER_SEND = ");
                sb.Append(Convert.ToInt32(pnw.HideAfterSending));
                sb.Append(", NO_CONTACTS_IN_CONTEXT_MENU = ");
                sb.Append(Convert.ToInt32(pnw.NoContactsInContextMenu));
                sb.Append(", EXCHANGE_PORT = ");
                sb.Append(pnw.ExchangePort);
                sb.Append(", POST_COUNT = ");
                sb.Append(pnw.PostCount);
                sb.Append(", ALLOW_PING = ");
                sb.Append(Convert.ToInt32(pnw.AllowPing));
                sb.Append(", RECEIVED_ON_TOP = ");
                sb.Append(Convert.ToInt32(pnw.ReceivedOnTop));
                ExecuteTransactionForStringBuilder(sb, SettingsConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveBehaviorSettings()
        {
            try
            {
                var sb = new StringBuilder();
                var pnb = PNStatic.Settings.Behavior;
                sb.Append("UPDATE BEHAVIOR SET NEW_ALWAYS_ON_TOP = ");
                sb.Append(Convert.ToInt32(pnb.NewNoteAlwaysOnTop));
                sb.Append(", RELATIONAL_POSITION = ");
                sb.Append(Convert.ToInt32(pnb.RelationalPositioning));
                sb.Append(", HIDE_COMPLETED = ");
                sb.Append(Convert.ToInt32(pnb.HideCompleted));
                sb.Append(", BIG_ICONS_ON_CP = ");
                sb.Append(Convert.ToInt32(pnb.BigIconsOnCP));
                sb.Append(", DO_NOT_SHOW_IN_LIST = ");
                sb.Append(Convert.ToInt32(pnb.DoNotShowNotesInList));
                sb.Append(", KEEP_VISIBLE_ON_SHOW_DESKTOP = ");
                sb.Append(Convert.ToInt32(pnb.KeepVisibleOnShowDesktop));
                sb.Append(", DBL_CLICK_ACTION = ");
                sb.Append((int)pnb.DoubleClickAction);
                sb.Append(", SINGLE_CLICK_ACTION = ");
                sb.Append((int)pnb.SingleClickAction);
                sb.Append(", DEFAULT_NAMING = ");
                sb.Append((int)pnb.DefaultNaming);
                sb.Append(", DEFAULT_NAME_LENGHT = ");
                sb.Append(pnb.DefaultNameLength);
                sb.Append(", CONTENT_COLUMN_LENGTH = ");
                sb.Append(pnb.ContentColumnLength);
                sb.Append(", HIDE_FLUENTLY = ");
                sb.Append(Convert.ToInt32(pnb.HideFluently));
                sb.Append(", PLAY_SOUND_ON_HIDE = ");
                sb.Append(Convert.ToInt32(pnb.PlaySoundOnHide));
                sb.Append(", OPACITY = ");
                sb.Append(pnb.Opacity.ToString(PNStatic.CultureInvariant));
                sb.Append(", RANDOM_COLOR = ");
                sb.Append(Convert.ToInt32(pnb.RandomBackColor));
                sb.Append(", INVERT_TEXT_COLOR = ");
                sb.Append(Convert.ToInt32(pnb.InvertTextColor));
                sb.Append(", ROLL_ON_DBLCLICK = ");
                sb.Append(Convert.ToInt32(pnb.RollOnDblClick));
                sb.Append(", FIT_WHEN_ROLLED = ");
                sb.Append(Convert.ToInt32(pnb.FitWhenRolled));
                sb.Append(", SHOW_SEPARATE_NOTES = ");
                sb.Append(Convert.ToInt32(pnb.ShowSeparateNotes));
                sb.Append(", PIN_CLICK_ACTION = ");
                sb.Append(Convert.ToInt32(pnb.PinClickAction));
                sb.Append(", NOTE_START_POSITION = ");
                sb.Append(Convert.ToInt32(pnb.StartPosition));
                sb.Append(", HIDE_MAIN_WINDOW = ");
                sb.Append(Convert.ToInt32(pnb.HideMainWindow));
                sb.Append(", THEME = ");
                if (pnb.Theme != PNStrings.DEF_THEME)
                {
                    sb.Append("'");
                    sb.Append(pnb.Theme);
                    sb.Append("'");
                }
                else
                {
                    sb.Append("NULL");
                }
                sb.Append(", PREVENT_RESIZING = ");
                sb.Append(Convert.ToInt32(pnb.PreventAutomaticResizing));
                sb.Append(", SHOW_PANEL = ");
                sb.Append(Convert.ToInt32(pnb.ShowNotesPanel));
                sb.Append(", PANEL_DOCK = ");
                sb.Append(Convert.ToInt32(pnb.NotesPanelOrientation));
                sb.Append(", PANEL_AUTO_HIDE = ");
                sb.Append(Convert.ToInt32(pnb.PanelAutoHide));
                sb.Append(", PANEL_REMOVE_MODE = ");
                sb.Append(Convert.ToInt32(pnb.PanelRemoveMode));
                sb.Append(", PANEL_SWITCH_OFF_ANIMATION = ");
                sb.Append(Convert.ToInt32(pnb.PanelSwitchOffAnimation));
                sb.Append(", PANEL_ENTER_DELAY = ");
                sb.Append(Convert.ToInt32(pnb.PanelEnterDelay));

                ExecuteTransactionForStringBuilder(sb, SettingsConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveScheduleSettings()
        {
            try
            {
                var sb = new StringBuilder();
                var pnsc = PNStatic.Settings.Schedule;
                sb.Append("UPDATE SCHEDULE SET SOUND = '");
                sb.Append(pnsc.Sound.Replace("'", "''"));
                sb.Append("', VOICE = '");
                sb.Append(pnsc.Voice.Replace("'", "''"));
                sb.Append("', ALLOW_SOUND = ");
                sb.Append(Convert.ToInt32(pnsc.AllowSoundAlert));
                sb.Append(", TRACK_OVERDUE = ");
                sb.Append(Convert.ToInt32(pnsc.TrackOverdue));
                sb.Append(", VISUAL_NOTIFY = ");
                sb.Append(Convert.ToInt32(pnsc.VisualNotification));
                sb.Append(", CENTER_SCREEN = ");
                sb.Append(Convert.ToInt32(pnsc.CenterScreen));
                sb.Append(", VOICE_VOLUME = ");
                sb.Append(pnsc.VoiceVolume);
                sb.Append(", VOICE_SPEED = ");
                sb.Append(pnsc.VoiceSpeed);
                sb.Append(", VOICE_PITCH = ");
                sb.Append(pnsc.VoicePitch);
                ExecuteTransactionForStringBuilder(sb, SettingsConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveSpellSettings()
        {
            try
            {
                var sb = new StringBuilder();
                var pngeneral = PNStatic.Settings.GeneralSettings;
                sb.Append("UPDATE GENERAL_SETTINGS SET ");
                sb.Append("SPELL_MODE = ");
                sb.Append(pngeneral.SpellMode);
                sb.Append(", SPELL_DICT = '");
                sb.Append(pngeneral.SpellDict);
                sb.Append("'");
                ExecuteTransactionForStringBuilder(sb, SettingsConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveHiddenMenus()
        {
            try
            {
                var sqlList = new List<string> { "DELETE FROM HIDDEN_MENUS" };
                foreach (var m in PNStatic.HiddenMenus)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO HIDDEN_MENUS VALUES('");
                    sb.Append(m.Name);
                    sb.Append("', ");
                    sb.Append(Convert.ToInt32(m.Type));
                    sb.Append(")");
                    sqlList.Add(sb.ToString());
                }
                ExecuteTransactionForList(sqlList, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void UpdateContact(PNContact cn)
        {
            try
            {
                var sb = new StringBuilder("UPDATE CONTACTS SET GROUP_ID = ");
                sb.Append(cn.GroupID);
                sb.Append(", CONTACT_NAME = '");
                sb.Append(cn.Name);
                sb.Append("', COMP_NAME = '");
                sb.Append(cn.ComputerName);
                sb.Append("', IP_ADDRESS = '");
                sb.Append(cn.IpAddress);
                sb.Append("', USE_COMP_NAME = ");
                sb.Append(Convert.ToInt32(cn.UseComputerName));
                sb.Append(" WHERE ID = ");
                sb.Append(cn.ID);
                ExecuteTransactionForStringBuilder(sb, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveContacts()
        {
            try
            {
                var sqlList = new List<string> { "DELETE FROM CONTACTS" };
                foreach (var cn in PNStatic.Contacts)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO CONTACTS VALUES(");
                    sb.Append(cn.ID);
                    sb.Append(",");
                    sb.Append(cn.GroupID);
                    sb.Append(",'");
                    sb.Append(cn.Name.Replace("'", "''"));
                    sb.Append("','");
                    sb.Append(cn.ComputerName.Replace("'", "''"));
                    sb.Append("','");
                    sb.Append(cn.IpAddress);
                    sb.Append("','");
                    sb.Append(Convert.ToInt32(cn.UseComputerName));
                    sb.Append("')");
                    sqlList.Add(sb.ToString());
                }
                ExecuteTransactionForList(sqlList, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveSocialPlugins()
        {
            try
            {
                var sqlList = new List<string> { "DELETE FROM POST_PLUGINS" };
                foreach (var p in PNStatic.PostPlugins)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO POST_PLUGINS VALUES('");
                    sb.Append(p.Replace("'", "''"));
                    sb.Append("')");
                    sqlList.Add(sb.ToString());
                }
                ExecuteTransactionForList(sqlList, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveSyncPlugins()
        {
            try
            {
                var sqlList = new List<string> { "DELETE FROM SYNC_PLUGINS" };
                foreach (var p in PNStatic.SyncPlugins)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO SYNC_PLUGINS VALUES('");
                    sb.Append(p.Replace("'", "''"));
                    sb.Append("')");
                    sqlList.Add(sb.ToString());
                }
                ExecuteTransactionForList(sqlList, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveTags()
        {
            try
            {
                var sqlList = new List<string> { "DELETE FROM TAGS" };
                foreach (var s in PNStatic.Tags)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO TAGS VALUES('");
                    sb.Append(s.Replace("'", "''"));
                    sb.Append("')");
                    sqlList.Add(sb.ToString());
                }
                ExecuteTransactionForList(sqlList, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveMailContacts()
        {
            try
            {
                var sqlList = new List<string> { "DELETE FROM MAIL_CONTACTS" };
                foreach (var c in PNStatic.MailContacts)
                {
                    var sb = new StringBuilder("INSERT INTO MAIL_CONTACTS VALUES(");
                    sb.Append(c.Id);
                    sb.Append(", '");
                    sb.Append(c.DisplayName);
                    sb.Append("', '");
                    sb.Append(c.Address);
                    sb.Append("')");
                    sqlList.Add(sb.ToString());
                }
                ExecuteTransactionForList(sqlList, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveSmtpClients()
        {
            try
            {
                var sqlList = new List<string> { "DELETE FROM SMTP_PROFILES" };
                foreach (var c in PNStatic.SmtpProfiles)
                {
                    var sb = new StringBuilder("INSERT INTO SMTP_PROFILES VALUES(");
                    sb.Append(c.Id);
                    sb.Append(", ");
                    sb.Append(Convert.ToInt32(c.Active));
                    sb.Append(", '");
                    sb.Append(c.HostName);
                    sb.Append("', '");
                    sb.Append(c.DisplayName);
                    sb.Append("', '");
                    sb.Append(c.SenderAddress);
                    sb.Append("', '");
                    sb.Append(c.Password);
                    sb.Append("', ");
                    sb.Append(c.Port);
                    sb.Append(")");
                    sqlList.Add(sb.ToString());
                }
                ExecuteTransactionForList(sqlList, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveSearchProviders()
        {
            try
            {
                var sqlList = new List<string> { "DELETE FROM SEARCH_PROVIDERS" };
                foreach (var s in PNStatic.SearchProviders)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO SEARCH_PROVIDERS VALUES('");
                    sb.Append(s.Name.Replace("'", "''"));
                    sb.Append("','");
                    sb.Append(s.QueryString.Replace("'", "''"));
                    sb.Append("')");
                    sqlList.Add(sb.ToString());
                }
                ExecuteTransactionForList(sqlList, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveExternals()
        {
            try
            {
                var sqlList = new List<string> { "DELETE FROM EXTERNALS" };
                foreach (var e in PNStatic.Externals)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO EXTERNALS VALUES('");
                    sb.Append(e.Name.Replace("'", "''"));
                    sb.Append("','");
                    sb.Append(e.Program.Replace("'", "''"));
                    sb.Append("','");
                    sb.Append(e.CommandLine.Replace("'", "''"));
                    sb.Append("')");
                    sqlList.Add(sb.ToString());
                }
                ExecuteTransactionForList(sqlList, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveSyncComps()
        {
            try
            {
                var sqlList = new List<string> { "DELETE FROM SYNC_COMPS" };
                foreach (var sc in PNStatic.SyncComps)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO SYNC_COMPS VALUES('");
                    sb.Append(sc.CompName.Replace("'", "''"));
                    sb.Append("','");
                    sb.Append(sc.DataDir.Replace("'", "''"));
                    sb.Append("','");
                    sb.Append(sc.DBDir.Replace("'", "''"));
                    sb.Append("','");
                    sb.Append(sc.UseDataDir.ToString());
                    sb.Append("')");
                    sqlList.Add(sb.ToString());
                }
                ExecuteTransactionForList(sqlList, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveContactGroups()
        {
            try
            {
                var sqlList = new List<string> { "DELETE FROM CONTACT_GROUPS" };
                foreach (var cg in PNStatic.ContactGroups)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO CONTACT_GROUPS VALUES(");
                    sb.Append(cg.ID);
                    sb.Append(",'");
                    sb.Append(cg.Name.Replace("'", "''"));
                    sb.Append("')");
                    sqlList.Add(sb.ToString());
                }
                ExecuteTransactionForList(sqlList, ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveGeneralSettings()
        {
            try
            {
                var sb = new StringBuilder();
                var cc = new System.Drawing.ColorConverter();

                var pngeneral = PNStatic.Settings.GeneralSettings;
                sb.Append("UPDATE GENERAL_SETTINGS SET ");
                sb.Append("LANGUAGE = '");
                sb.Append(pngeneral.Language);
                sb.Append("', RUN_ON_START = ");
                sb.Append(Convert.ToInt32(pngeneral.RunOnStart));
                sb.Append(", SHOW_CP_ON_START = ");
                sb.Append(Convert.ToInt32(pngeneral.ShowCPOnStart));
                sb.Append(", CHECK_NEW_VERSION_ON_START = ");
                sb.Append(Convert.ToInt32(pngeneral.CheckNewVersionOnStart));
                sb.Append(", HIDE_TOOLBAR = ");
                sb.Append(Convert.ToInt32(pngeneral.HideToolbar));
                sb.Append(", USE_CUSTOM_FONTS = ");
                sb.Append(Convert.ToInt32(pngeneral.UseCustomFonts));
                sb.Append(", SHOW_SCROLLBAR = ");
                sb.Append(Convert.ToInt32(pngeneral.ShowScrollbar));
                sb.Append(", HIDE_DELETE_BUTTON = ");
                sb.Append(Convert.ToInt32(pngeneral.HideDeleteButton));
                sb.Append(", CHANGE_HIDE_TO_DELETE = ");
                sb.Append(Convert.ToInt32(pngeneral.ChangeHideToDelete));
                sb.Append(", HIDE_HIDE_BUTTON = ");
                sb.Append(Convert.ToInt32(pngeneral.HideHideButton));
                sb.Append(", BULLETS_INDENT = ");
                sb.Append(pngeneral.BulletsIndent);
                sb.Append(", MARGIN_WIDTH = ");
                sb.Append(pngeneral.MarginWidth);
                sb.Append(", SAVE_ON_EXIT = ");
                sb.Append(Convert.ToInt32(pngeneral.SaveOnExit));
                sb.Append(", CONFIRM_SAVING = ");
                sb.Append(Convert.ToInt32(pngeneral.ConfirmSaving));
                sb.Append(", CONFIRM_BEFORE_DELETION = ");
                sb.Append(Convert.ToInt32(pngeneral.ConfirmBeforeDeletion));
                sb.Append(", SAVE_WITHOUT_CONFIRM_ON_HIDE = ");
                sb.Append(Convert.ToInt32(pngeneral.SaveWithoutConfirmOnHide));
                sb.Append(", WARN_ON_AUTOMATICAL_DELETE = ");
                sb.Append(Convert.ToInt32(pngeneral.WarnOnAutomaticalDelete));
                sb.Append(", AUTO_SAVE = ");
                sb.Append(Convert.ToInt32(pngeneral.Autosave));
                sb.Append(", AUTO_SAVE_PERIOD = ");
                sb.Append(pngeneral.AutosavePeriod);
                sb.Append(", REMOVE_FROM_BIN_PERIOD = ");
                sb.Append(pngeneral.RemoveFromBinPeriod);
                sb.Append(", DATE_FORMAT = '");
                sb.Append(pngeneral.DateFormat.Replace("'", "''"));
                sb.Append("', TIME_FORMAT = '");
                sb.Append(pngeneral.TimeFormat.Replace("'", "''"));
                sb.Append("', SKINLESS_WIDTH = ");
                sb.Append(pngeneral.Width);
                sb.Append(", SKINLESS_HEIGHT = ");
                sb.Append(pngeneral.Height);
                sb.Append(", SPELL_COLOR = '");
                sb.Append(cc.ConvertToString(null, PNStatic.CultureInvariant, pngeneral.SpellColor));
                sb.Append("', USE_SKINS = ");
                sb.Append(Convert.ToInt32(pngeneral.UseSkins));
                sb.Append(", DOCK_WIDTH = ");
                sb.Append(pngeneral.DockWidth);
                sb.Append(", DOCK_HEIGHT = ");
                sb.Append(pngeneral.DockHeight);
                sb.Append(", SHOW_PRIORITY_ON_START = ");
                sb.Append(Convert.ToInt32(pngeneral.ShowPriorityOnStart));
                sb.Append(", BUTTONS_SIZE = ");
                sb.Append(Convert.ToInt32(pngeneral.ButtonsSize));
                sb.Append(", AUTOMATIC_SMILIES = ");
                sb.Append(Convert.ToInt32(pngeneral.AutomaticSmilies));
                sb.Append(", SPACE_POINTS = ");
                sb.Append(pngeneral.SpacePoints);
                sb.Append(", RESTORE_AUTO = ");
                sb.Append(Convert.ToInt32(pngeneral.RestoreAuto));
                sb.Append(", PARAGRAPH_INDENT = ");
                sb.Append(pngeneral.ParagraphIndent);
                sb.Append(", AUTO_HEIGHT = ");
                sb.Append(Convert.ToInt32(pngeneral.AutoHeight));
                sb.Append(", CRITICAL_ON_START = ");
                sb.Append(Convert.ToInt32(pngeneral.CheckCriticalOnStart));
                sb.Append(", CRITICAL_PERIODICALLY = ");
                sb.Append(Convert.ToInt32(pngeneral.CheckCriticalPeriodically));
                sb.Append(", DELETE_SHORTCUTS_ON_EXIT = ");
                sb.Append(Convert.ToInt32(pngeneral.DeleteShortcutsOnExit));
                sb.Append(", RESTORE_SHORTCUTS_ON_START = ");
                sb.Append(Convert.ToInt32(pngeneral.RestoreShortcutsOnStart));
                sb.Append(", CLOSE_ON_SHORTCUT = ");
                sb.Append(Convert.ToInt32(pngeneral.CloseOnShortcut));

                ExecuteTransactionForStringBuilder(sb, SettingsConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void SaveDiarySettings()
        {
            try
            {
                var sb = new StringBuilder();
                var pndr = PNStatic.Settings.Diary;
                sb.Append("UPDATE DIARY SET CUSTOM_SETTINGS = ");
                sb.Append(Convert.ToInt32(pndr.CustomSettings));
                sb.Append(", ADD_WEEKDAY = ");
                sb.Append(Convert.ToInt32(pndr.AddWeekday));
                sb.Append(", FULL_WEEKDAY_NAME = ");
                sb.Append(Convert.ToInt32(pndr.FullWeekdayName));
                sb.Append(", WEEKDAY_AT_THE_END = ");
                sb.Append(Convert.ToInt32(pndr.WeekdayAtTheEnd));
                sb.Append(", DO_NOT_SHOW_PREVIOUS = ");
                sb.Append(Convert.ToInt32(pndr.DoNotShowPrevious));
                sb.Append(", ASC_ORDER = ");
                sb.Append(Convert.ToInt32(pndr.AscendingOrder));
                sb.Append(", NUMBER_OF_PAGES = ");
                sb.Append(pndr.NumberOfPages);
                sb.Append(", DATE_FORMAT = '");
                sb.Append(pndr.DateFormat.Replace("'", "''"));
                sb.Append("'");
                ExecuteTransactionForStringBuilder(sb, SettingsConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static bool ExecuteTransactionForStringBuilder(StringBuilder sb, string connectionString)
        {
            try
            {
                using (var oData = new SQLiteDataObject(connectionString))
                {
                    if (oData.BeginTransaction())
                    {
                        try
                        {
                            oData.ExecuteInTransaction(sb.ToString());
                            oData.CommitTransaction();
                        }
                        catch (Exception ex)
                        {
                            oData.RollbackTransaction();
                            PNStatic.LogException(ex);
                            return false;
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

        internal static bool ExecuteTransactionForList(List<string> sqlList, string connectionString)
        {
            try
            {
                using (var oData = new SQLiteDataObject(connectionString))
                {
                    if (oData.BeginTransaction())
                    {
                        try
                        {
                            foreach (var s in sqlList)
                            {
                                oData.ExecuteInTransaction(s);
                            }
                            oData.CommitTransaction();
                        }
                        catch (Exception ex)
                        {
                            oData.RollbackTransaction();
                            PNStatic.LogException(ex);
                            return false;
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

        private static void savePreviousFiles()
        {
            try
            {
                if (File.Exists(PNStrings.OLD_EDITION_ARCHIVE))
                    File.Delete(PNStrings.OLD_EDITION_ARCHIVE);
                var zipFile = new ZipFile(PNStrings.OLD_EDITION_ARCHIVE);
                var di = new DirectoryInfo(PNPaths.Instance.DataDir);
                var files = di.GetFiles().Select(f => f.FullName);
                zipFile.AddFiles(files, false, Path.GetFileName(PNPaths.Instance.DataDir));
                zipFile.AddItem(PNPaths.Instance.SettingsDBPath, "");
                zipFile.Save();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
