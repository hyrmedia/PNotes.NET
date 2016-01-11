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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndImportSettings.xaml
    /// </summary>
    public partial class WndImportSettings
    {
        public WndImportSettings()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
            _Loaded = true;
        }

        private readonly bool _Loaded;

        private void DlgImportSettings_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtIniPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableOk();
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
                var wasChange = false;

                Cursor = Cursors.Wait;
                lblProgress.Text = PNLang.Instance.GetCaptionText("retrievivg_data", "Retrieving data");
                lblProgress.Visibility = elpProgress.Visibility = Visibility.Visible;

                string iniPath = txtIniPath.Text.Trim();
                if (chkImpSounds.IsChecked != null && chkImpSounds.IsChecked.Value)
                {
                    wasChange |= importSounds(iniPath);
                }
                if (chkImpTags.IsChecked != null && chkImpTags.IsChecked.Value)
                {
                    wasChange |= importTags(iniPath);
                }
                if (chkImpExtPrograms.IsChecked != null && chkImpExtPrograms.IsChecked.Value)
                {
                    wasChange |= importExternals(iniPath);
                }
                if (chkImpSearchEngines.IsChecked != null && chkImpSearchEngines.IsChecked.Value)
                {
                    wasChange |= importSearchProviders(iniPath);
                }
                if (chkImpContacts.IsChecked != null && chkImpContacts.IsChecked.Value)
                {
                    wasChange |= importContacts(iniPath);
                }
                DialogResult = wasChange;
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
            if (!_Loaded) return;
            if (chkImpContacts.IsChecked == null || chkImpExtPrograms.IsChecked == null ||
                chkImpSearchEngines.IsChecked == null || chkImpSounds.IsChecked == null || chkImpTags.IsChecked == null)
            {
                cmdOK.IsEnabled = false;
                return;
            }
            cmdOK.IsEnabled = (chkImpContacts.IsChecked.Value || chkImpExtPrograms.IsChecked.Value ||
                               chkImpSearchEngines.IsChecked.Value || chkImpSounds.IsChecked.Value ||
                               chkImpTags.IsChecked.Value) &&
                              txtIniPath.Text.Trim().Length > 0;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            enableOk();
        }

        private bool importContacts(string iniPath)
        {
            try
            {
                var result = false;
                var size = 1024;
                var buffer = new string(' ', size);
                while (PNInterop.GetPrivateProfileString("contacts", null, null, buffer, size, iniPath) == size - 2)
                {
                    // loop until sufficient buffer size
                    size *= 2;
                    buffer = new string(' ', size);
                }
                var names = buffer.Split('\0');
                var keys = names.Where(n => n.Trim().Length > 0);
                var sqlList = new List<string>();
                var contacts = new List<PNContact>();
                int tempID = 0;
                if (PNStatic.Contacts.Count > 0)
                {
                    tempID = PNStatic.Contacts.Max(c => c.ID) + 1;
                }
                foreach (var key in keys)
                {
                    var cont = new PCONTPROP();
                    cont = PNInterop.ReadINIStructure(iniPath, "contacts", key, cont);
                    if (cont.name != null && cont.name.Trim().Length > 0)
                    {
                        var pnc = new PNContact
                        {
                            ComputerName = cont.host,
                            GroupID = cont.group,
                            Name = cont.name,
                            UseComputerName = cont.usename
                        };
                        if (cont.address != 0)
                        {
                            pnc.IpAddress = buildAddressString(cont.address);
                        }
                        var temp = PNStatic.Contacts.FirstOrDefault(c => c.Name == pnc.Name);
                        pnc.ID = temp != null ? temp.ID : tempID++;
                        contacts.Add(pnc);
                    }
                }
                // get contact froups
                size = 1024;
                buffer = new string(' ', size);
                while (PNInterop.GetPrivateProfileString("cont_groups", null, null, buffer, size, iniPath) == size - 2)
                {
                    // loop until sufficient buffer size
                    size *= 2;
                    buffer = new string(' ', size);
                }
                names = buffer.Split('\0');
                keys = names.Where(n => n.Trim().Length > 0);
                var groups = new List<PNContactGroup>();
                tempID = 0;
                if (PNStatic.ContactGroups.Count > 0)
                {
                    tempID = PNStatic.ContactGroups.Max(g => g.ID) + 1;
                }
                foreach (var key in keys)
                {
                    var grp = new PCONTGROUP();
                    grp = PNInterop.ReadINIStructure(iniPath, "cont_groups", key, grp);
                    if (grp.name == null || grp.name.Trim().Length <= 0) continue;
                    var pg = new PNContactGroup { Name = grp.name };
                    var temp = PNStatic.ContactGroups.FirstOrDefault(g => g.Name == grp.name);
                    pg.ID = temp != null ? temp.ID : tempID++;
                    groups.Add(pg);
                }
                // build sql
                foreach (var pg in groups)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT OR REPLACE INTO CONTACT_GROUPS (ID, GROUP_NAME) VALUES(");
                    sb.Append(pg.ID);
                    sb.Append(", '");
                    sb.Append(pg.Name);
                    sb.Append("')");
                    sqlList.Add(sb.ToString());
                }
                foreach (var pc in contacts)
                {
                    var sb = new StringBuilder();
                    sb.Append(
                        "INSERT OR REPLACE INTO CONTACTS (ID, GROUP_ID, CONTACT_NAME, COMP_NAME, IP_ADDRESS, USE_COMP_NAME) VALUES(");
                    sb.Append(pc.ID);
                    sb.Append(", ");
                    sb.Append(pc.GroupID);
                    sb.Append(", '");
                    sb.Append(pc.Name);
                    sb.Append("', '");
                    sb.Append(pc.ComputerName);
                    sb.Append("', '");
                    sb.Append(pc.IpAddress);
                    sb.Append("', ");
                    sb.Append(Convert.ToInt32(pc.UseComputerName));
                    sb.Append(")");
                    sqlList.Add(sb.ToString());
                }
                if (sqlList.Count > 0 && PNData.ExecuteTransactionForList(sqlList, PNData.ConnectionString))
                {
                    result = true;
                    foreach (PNContactGroup pg in groups)
                    {
                        PNStatic.ContactGroups.RemoveAll(g => g.Name == pg.Name);
                        PNStatic.ContactGroups.Add(pg);
                    }
                    foreach (PNContact pc in contacts)
                    {
                        PNStatic.Contacts.RemoveAll(c => c.Name == pc.Name);
                        PNStatic.Contacts.Add(pc);
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

        private bool importSearchProviders(string iniPath)
        {
            try
            {
                bool result = false;
                int size = 1024;
                var buffer = new string(' ', size);
                while (PNInterop.GetPrivateProfileString("search_engines", null, null, buffer, size, iniPath) ==
                       size - 2)
                {
                    // loop until sufficient buffer size
                    size *= 2;
                    buffer = new string(' ', size);
                }
                var names = buffer.Split('\0');
                var keys = names.Where(n => n.Trim().Length > 0);
                var sqlList = new List<string>();
                var providers = new List<PNSearchProvider>();
                foreach (var key in keys)
                {
                    var query = new StringBuilder(1024);
                    while (
                        PNInterop.GetPrivateProfileStringByBuilder("search_engines", key, "", query, query.Capacity,
                                                                   iniPath) == query.Capacity - 1)
                    {
                        query.Capacity *= 2;
                    }
                    providers.Add(new PNSearchProvider { Name = key, QueryString = query.ToString() });
                    var sb = new StringBuilder();
                    sb.Append("INSERT OR REPLACE INTO SEARCH_PROVIDERS (SP_NAME, SP_QUERY) VALUES(");
                    sb.Append("'");
                    sb.Append(key);
                    sb.Append("', ");
                    sb.Append("'");
                    sb.Append(query);
                    sb.Append("'");
                    sb.Append(")");
                    sqlList.Add(sb.ToString());
                }
                if (sqlList.Count > 0 && PNData.ExecuteTransactionForList(sqlList, PNData.ConnectionString))
                {
                    result = true;
                    foreach (var prv in providers)
                    {
                        PNStatic.SearchProviders.RemoveAll(pr => pr.Name == prv.Name);
                        PNStatic.SearchProviders.Add(prv);
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

        private bool importExternals(string iniPath)
        {
            try
            {
                var result = false;
                var size = 1024;
                var buffer = new string(' ', size);
                while (PNInterop.GetPrivateProfileString("external_programs", null, null, buffer, size, iniPath) ==
                       size - 2)
                {
                    // loop until sufficient buffer size
                    size *= 2;
                    buffer = new string(' ', size);
                }
                var names = buffer.Split('\0');
                var keys = names.Where(n => n.Trim().Length > 0);
                var listSql = new List<string>();
                var externals = new List<PNExternal>();
                foreach (string key in keys)
                {
                    var prog = new StringBuilder(1024);
                    while (
                        PNInterop.GetPrivateProfileStringByBuilder("external_programs", key, "", prog, prog.Capacity,
                                                                   iniPath) == prog.Capacity - 1)
                    {
                        prog.Capacity *= 2;
                    }
                    var data = prog.ToString().Split((char)1);
                    var ext = new PNExternal { Name = key, Program = data[0] };
                    var sb = new StringBuilder();
                    sb.Append("INSERT OR REPLACE INTO EXTERNALS (EXT_NAME, PROGRAM, COMMAND_LINE) VALUES(");
                    sb.Append("'");
                    sb.Append(key);
                    sb.Append("', ");
                    sb.Append("'");
                    sb.Append(data[0]);
                    sb.Append("', ");
                    sb.Append("'");
                    if (data.Length == 2)
                    {
                        sb.Append(data[1]);
                        ext.CommandLine = data[1];
                    }
                    sb.Append("'");
                    sb.Append(")");
                    listSql.Add(sb.ToString());
                    externals.Add(ext);
                }
                if (listSql.Count > 0 && PNData.ExecuteTransactionForList(listSql, PNData.ConnectionString))
                {
                    result = true;
                    foreach (PNExternal ext in externals)
                    {
                        PNStatic.Externals.RemoveAll(ex => ex.Name == ext.Name);
                        PNStatic.Externals.Add(ext);
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

        private bool importTags(string iniPath)
        {
            try
            {
                var result = false;
                var tags = new StringBuilder(1024);
                while (
                    PNInterop.GetPrivateProfileStringByBuilder("tags_pre", "tags", "", tags, tags.Capacity, iniPath) ==
                    tags.Capacity - 1)
                {
                    tags.Capacity *= 2;
                }
                if (tags.Length > 0)
                {
                    var arr = tags.ToString().Split(',');
                    var listSql = new List<string>();
                    foreach (var tag in arr)
                    {
                        var sb = new StringBuilder();
                        sb.Append("INSERT OR REPLACE INTO TAGS (TAG) VALUES('");
                        sb.Append(tag);
                        sb.Append("')");
                        listSql.Add(sb.ToString());
                    }
                    if (PNData.ExecuteTransactionForList(listSql, PNData.ConnectionString))
                    {
                        result = true;
                        foreach (string tag in arr)
                        {
                            if (!PNStatic.Tags.Contains(tag))
                            {
                                PNStatic.Tags.Add(tag);
                            }
                        }
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

        private bool importSounds(string iniPath)
        {
            try
            {
                var result = false;
                if (iniPath != null)
                {
                    var dirName = Path.GetDirectoryName(iniPath);
                    if (string.IsNullOrEmpty(dirName)) return false;
                    var soundsDir = Path.Combine(dirName, "sound");
                    if (Directory.Exists(soundsDir))
                    {
                        var files = new DirectoryInfo(soundsDir).GetFiles("*.wav");
                        foreach (var fi in files)
                        {
                            var filePath = Path.Combine(PNPaths.Instance.SoundsDir, fi.Name);
                            File.Copy(fi.FullName, filePath, true);
                            result = true;
                        }
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

        private string buildAddressString(int x)
        {
            return FIRST_IPADDRESS(x) + "." + SECOND_IPADDRESS(x) + "." + THIRD_IPADDRESS(x) + "." + FOURTH_IPADDRESS(x);
        }

        private string FIRST_IPADDRESS(int x)
        {
            return ((x >> 24) & 0xFF).ToString(PNStatic.CultureInvariant);
        }

        private string SECOND_IPADDRESS(int x)
        {
            return ((x >> 16) & 0xFF).ToString(PNStatic.CultureInvariant);
        }

        private string THIRD_IPADDRESS(int x)
        {
            return ((x >> 8) & 0xFF).ToString(PNStatic.CultureInvariant);
        }

        private string FOURTH_IPADDRESS(int x)
        {
            return (x & 0xFF).ToString(PNStatic.CultureInvariant);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        private struct PCONTGROUP
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string name;
            public int id;
            public int next;
            public int prev;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        private struct PCONTPROP
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string name;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string host;
            public int address;
            public bool usename;
            public bool send;
            public int group;
        }
    }
}
