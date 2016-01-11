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

namespace PNotes.NET
{
    internal class PNStrings
    {
        internal const string SHORTCUT_FILE = @"\PNotes.NET.lnk";
        internal const string PROG_NAME = "PNotes.NET";
        internal const string CHM_FILE = "PNotes.NET.chm";
        internal const string PDF_FILE = "PNotes.NET.pdf";
        internal const string OLD_EDITION_ARCHIVE = "PNOldEditionBackup.zip";
        internal const string PRE_RUN_FILE = "pnotesprerun.xml";
        internal const string ATT_FROM = "from";
        internal const string ATT_TO = "to";
        internal const string ATT_NAME = "name";
        internal const string ATT_DEL_DIR = "deleteDir";
        internal const string ATT_IS_CRITICAL = "isCritical";
        internal const string ELM_COPY_THEMES = "copy_themes";
        internal const string ELM_COPY_PLUGS = "copy_plugins";
        internal const string ELM_COPY_FILES = "copy_files";
        internal const string ELM_COPY = "copy";
        internal const string ELM_PRE_RUN = "pre_run";
        internal const string ELM_REMOVE = "remove";
        internal const string ELM_DIR = "dir";
        internal const string URL_DOWNLOAD_ROOT = "http://downloads.sourceforge.net/pnotes/";
        internal const string URL_HELP = "http://pnotes.sf.net/helpnet/index.html";
        internal const string URL_SKINS = "http://pnotes.sourceforge.net/index.php?page=9";
        internal const string URL_LANGS = "http://pnotes.sourceforge.net/index.php?page=3";
        internal const string URL_THEMES = "http://pnotes.sourceforge.net/index.php?page=11";
        internal const string URL_UPDATE = "http://pnotes.sourceforge.net/versionnet.txt";
        internal const string URL_PLUGINS_UPDATE = "http://pnotes.sourceforge.net/versionplugins.txt";
        internal const string URL_MAIN = "http://pnotes.sourceforge.net/index.php?page=1";
        internal const string URL_DOWNLOAD = "http://pnotes.sourceforge.net/index.php?page=5";
        internal const string URL_PLUGINS_DOWNLOAD = "http://pnotes.sourceforge.net/index.php?page=10";
        internal const string URL_DOWNLOAD_DIR = "http://downloads.sourceforge.net/pnotes/";
        internal const string URL_PAYPAL = "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=8YEWS9ZW3VFJS&lc=IL&item_name=PNotes (it is free and will remain free, but your donation wiil definitely help to make it better)&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted";
        internal const string URL_DICTIONARIES = "http://extensions.openoffice.org/";
        internal const string URL_DICT_FTP = "ftp://ftp.snt.utwente.nl/pub/software/openoffice/contrib/dictionaries";
        internal const string URL_CRITICAL_UPDATES = "http://pnotes.sourceforge.net/critical.xml";
        internal const string URL_THEMES_UPDATE = "http://pnotes.sourceforge.net/versionthemes.xml";
        internal const string MAIL_ADDRESS = "andrey.gruber@gmail.com";
        internal const string DEF_THEME = "Dark Gray (default)";
        internal const string TEMP_DB_FILE = "temp.db3";
        internal const string DB_FILE = "notes.db3";
        internal const string SETTINGS_FILE = "settings.db3";
        internal const string DEF_NOTE_NAME = "Untitled";
        internal const string NOTE_EXTENSION = ".pnote";
        internal const string NOTE_BACK_EXTENSION = ".pnxb";
        internal const string FULL_BACK_EXTENSION = ".pnback";
        internal const string NOTE_AUTO_BACK_EXTENSION = ".pnote~";
        internal const string SKIN_EXTENSION = ".pnskn";
        internal const string THEME_FILE_MASK = "*.pntheme";
        internal const string DEF_CAPTION_FONT = "Segoe UI";
        //internal const string RESOURCE_PREFIX = "pack://application:,,,/images/";
        //internal const string BIG_IMAGES_PREFIX = "pack://application:,,,/images/bigimages/";
        internal const string FOLDERS_PREFIX = "pack://application:,,,/images/folders/";
        internal const string SMILIES_PREFIX = "pack://application:,,,/images/smilies/";
        internal const string PNG_EXT = ".png";
        internal const string CRITICAL_UPDATE_LOG = "pncritical";
        internal const string DATE_TIME_FORMAT = "dd MMM yyyy HH:mm:ss";
        internal const string PLACEHOLDER1 = "%PLACEHOLDER1%";
        internal const string PLACEHOLDER2 = "%PLACEHOLDER2%";
        internal const string PLACEHOLDER3 = "%PLACEHOLDER3%";
        internal const string YEARS = "%YEARS%";
        internal const string MONTHS = "%MONTHS%";
        internal const string WEEKS = "%WEEKS%";
        internal const string DAYS = "%DAYS%";
        internal const string HOURS = "%HOURS%";
        internal const string MINUTES = "%MINUTES%";
        internal const string SECONDS = "%SECONDS%";
        internal const string TEMP_SYNC_DIR = "pnotestempsync";
        internal const string TEMP_THEMES_DIR = "pnotestempthemes";
        internal const string TEMP_DICT_DIR = "pnotestempdict";
        internal const string DATE_FORMAT_CHARS = "d\tDay of month as digits with no leading zero for single-digit days.\n" +
                                                    "dd\tDay of month as digits with leading zero for single-digit days.\n" +
                                                    "ddd\tDay of week as a three-letter abbreviation.\n" +
                                                    "dddd\tDay of week as its full name.\n" +
                                                    "M\tMonth as digits with no leading zero for single-digit months.\n" +
                                                    "MM\tMonth as digits with leading zero for single-digit months.\n" +
                                                    "MMM\tMonth as a three-letter abbreviation.\n" +
                                                    "MMMM\tMonth as its full name.\n" +
                                                    "y\tYear as last two digits, but with no leading zero for years less than 10.\n" +
                                                    "yy\tYear as last two digits, but with leading zero for years less than 10.\n" +
                                                    "yyyy\tYear represented by full four digits.";
        internal const string TIME_FORMAT_CHARS = "h\tHours with no leading zero for single-digit hours; 12-hour clock.\n" +
                                                    "hh\tHours with leading zero for single-digit hours; 12-hour clock.\n" +
                                                    "H\tHours with no leading zero for single-digit hours; 24-hour clock.\n" +
                                                    "HH\tHours with leading zero for single-digit hours; 24-hour clock.\n" +
                                                    "m\tMinutes with no leading zero for single-digit minutes.\n" +
                                                    "mm\tMinutes with leading zero for single-digit minutes.\n" +
                                                    "s\tSeconds with no leading zero for single-digit seconds.\n" +
                                                    "ss\tSeconds with leading zero for single-digit seconds.\n" +
                                                    "t\tOne character time-marker string, such as A or P.\n" +
                                                    "tt\tMulticharacter time-marker string, such as AM or PM.";
        internal const string CREATE_TRIGGERS = "CREATE TRIGGER IF NOT EXISTS [TRG_NOTES_DELETE] AFTER DELETE ON [NOTES] BEGIN DELETE FROM NOTES_TAGS WHERE NOTE_ID = OLD . ID; DELETE FROM CUSTOM_NOTES_SETTINGS WHERE NOTE_ID = OLD . ID; DELETE FROM NOTES_SCHEDULE WHERE NOTE_ID = OLD . ID; DELETE FROM LINKED_NOTES WHERE NOTE_ID = OLD . ID;  END;" +
            "CREATE TRIGGER IF NOT EXISTS [TRG_CUSTOM_NOTES_SETTINGS_UPDATE] AFTER UPDATE ON [CUSTOM_NOTES_SETTINGS] BEGIN UPDATE CUSTOM_NOTES_SETTINGS SET UPD_DATE = strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' ) WHERE NOTE_ID = OLD . NOTE_ID ; END;" +
            "CREATE TRIGGER IF NOT EXISTS [TRG_GROUPS_UPDATE] AFTER UPDATE ON [GROUPS] BEGIN UPDATE GROUPS SET UPD_DATE = strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' ) WHERE GROUP_ID = OLD . GROUP_ID ; END;" +
            "CREATE TRIGGER IF NOT EXISTS [TRG_LINKED_NOTES_UPDATE] AFTER UPDATE ON [LINKED_NOTES] BEGIN UPDATE LINKED_NOTES SET UPD_DATE = strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' ) WHERE NOTE_ID = OLD . NOTE_ID ; END;" +
            "CREATE TRIGGER IF NOT EXISTS [TRG_NOTES_SCHEDULE_UPDATE] AFTER UPDATE ON [NOTES_SCHEDULE] BEGIN UPDATE NOTES_SCHEDULE SET UPD_DATE = strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' ) WHERE NOTE_ID = OLD . NOTE_ID ; END;" +
            "CREATE TRIGGER IF NOT EXISTS [TRG_NOTES_TAGS_UPDATE] AFTER UPDATE ON [NOTES_TAGS] BEGIN UPDATE NOTES_TAGS SET UPD_DATE = strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' ) WHERE NOTE_ID = OLD . NOTE_ID ; END;" +
            "CREATE TRIGGER IF NOT EXISTS [TRG_NOTES_UPDATE] AFTER UPDATE ON [NOTES] BEGIN UPDATE NOTES SET UPD_DATE = strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' ) WHERE ID = OLD . ID ; END;";
        internal const string DROP_TRIGGERS = "DROP TRIGGER TRG_CUSTOM_NOTES_SETTINGS_UPDATE;" +
            "DROP TRIGGER TRG_GROUPS_UPDATE;" +
            "DROP TRIGGER TRG_LINKED_NOTES_UPDATE;" +
            "DROP TRIGGER TRG_NOTES_SCHEDULE_UPDATE;" +
            "DROP TRIGGER TRG_NOTES_TAGS_UPDATE;" +
            "DROP TRIGGER TRG_NOTES_UPDATE;";
        internal const string NONE = "(none)";
        internal const string NO_GROUP = "(No group)";
        internal const string END_OF_FILE = "<EOF>";
        internal const string END_OF_ADDRESS = "<EOA>";
        internal const string END_OF_TEXT = "<EOT>";
        internal const string END_OF_NOTE = "<EON>";
        internal const string END_OF_OPTIONS = "<EOO>";
        internal const string SUCCESS = "SUCCESS";
        internal const string NOSPLASH = "nosplash";
        internal const string DEFAULT_FONT_NAME = "Lucida Sans Unicode";

        //internal const string DEFAULT_UI_FONT = "Microsoft Sans Serif, 8.25pt";
        internal const string MENU_SEPARATOR_STRING = "-----------";
        internal const string MAIL_PATTERN =
            @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$";

        internal const char DEL_CHAR = '\a';

        internal static readonly string[] RestrictedHotkeys =
        {
            "Ctrl+S",
            "Ctrl+C",
            "Ctrl+X",
            "Ctrl+V",
            "Ctrl+O",
            "Ctrl+P",
            "Ctrl+F",
            "Ctrl+A",
            "Ctrl+Z",
            "Ctrl+Y", 
            "Ctrl+G", 
            "Ctrl+E", 
            "Ctrl+R", 
            "Ctrl+L", 
            "Ctrl+B", 
            "Ctrl+I", 
            "Ctrl+K", 
            "Ctrl+U", 
            "Ctrl+Del",
            "Shift+Del",
            "Ctrl+Backspace",
            "Shift+Ins",
            "F1", 
            "F3", 
            "F5"
        };
    }
}
