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
using System.IO;
using System.Windows.Forms;

namespace PNotes.NET
{
    internal sealed class PNPaths
    {
        private static readonly Lazy<PNPaths> lazy = new Lazy<PNPaths>(() => new PNPaths());

        private PNPaths()
        {
        }

        internal static PNPaths Instance { get { return lazy.Value; } }

        private string _LangDir = Application.StartupPath + @"\lang";
        private string _SkinsDir = Application.StartupPath + @"\skins";
        private string _SoundsDir = Application.StartupPath + @"\sounds";
        private string _SettingsDir = Application.StartupPath;
        private string _DataDir = Application.StartupPath + @"\data";
        private string _BackupDir = Application.StartupPath + @"\backup";
        private string _FontsDir = Application.StartupPath + @"\fonts";
        private string _DictDir = Application.StartupPath + @"\dictionaries";
        private string _PluginsDir = Application.StartupPath + @"\plugins";
        private string _DBPath = Application.StartupPath + @"\data\" + PNStrings.DB_FILE;
        private string _SettingsDBPath = Application.StartupPath + @"\" + PNStrings.SETTINGS_FILE;
        private readonly string _TempDir = Path.Combine(Path.GetTempPath(), "pnotestemp");
        private string _ThemesDir = Application.StartupPath + @"\themes";

        internal string PluginsDir
        {
            get { return Instance._PluginsDir; }
            set { Instance._PluginsDir = value; }
        }
        internal string TempDir
        {
            get { return Instance._TempDir; }
        }
        internal string SettingsDBPath
        {
            get { return Instance._SettingsDBPath; }
            set { Instance._SettingsDBPath = value; }
        }
        internal string DBPath
        {
            get { return Instance._DBPath; }
            set { Instance._DBPath = value; }
        }
        internal string DictDir
        {
            get { return Instance._DictDir; }
            set { Instance._DictDir = value; }
        }
        internal string FontsDir
        {
            get { return Instance._FontsDir; }
            set { Instance._FontsDir = value; }
        }
        internal string BackupDir
        {
            get { return Instance._BackupDir; }
            set { Instance._BackupDir = value; }
        }
        internal string DataDir
        {
            get { return Instance._DataDir; }
            set { Instance._DataDir = value; _DBPath = value + @"\" + PNStrings.DB_FILE; }
        }
        internal string SettingsDir
        {
            get { return Instance._SettingsDir; }
            set { Instance._SettingsDir = value; _SettingsDBPath = _SettingsDir + @"\" + PNStrings.SETTINGS_FILE; }
        }
        internal string SoundsDir
        {
            get { return Instance._SoundsDir; }
            set { Instance._SoundsDir = value; }
        }
        internal string SkinsDir
        {
            get { return Instance._SkinsDir; }
            set { Instance._SkinsDir = value; }
        }
        internal string LangDir
        {
            get { return Instance._LangDir; }
            set { Instance._LangDir = value; }
        }

        internal string ThemesDir
        {
            get { return Instance._ThemesDir; }
            set { Instance._ThemesDir = value; }
        }
    }
}
