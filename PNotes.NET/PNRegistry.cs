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
using System.Linq;
using System.Security;
using System.Windows.Forms;
using Microsoft.Win32;

namespace PNotes.NET
{
    internal class PNRegistry
    {
        internal static void CleanRegRunMRU()
        {
            try
            {
                var fileName = Path.GetFileName(Application.ExecutablePath);
                var progName = fileName.ToUpper();
                using (var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\RunMRU", true))
                {
                    if (key == null)
                        return;
                    var values = key.GetValueNames();
                    try
                    {
                        foreach (var s in values)
                        {
                            var value = key.GetValue(s);
                            if (!(value is string)) continue;
                            var v = Convert.ToString(value);
                            if (!string.IsNullOrEmpty(v) && v.ToUpper().Contains(progName))
                            {
                                key.DeleteValue(s, false);
                            }
                        }
                    }
                    catch (SecurityException)
                    {
                    }
                }
            }
            catch (SecurityException)
            {
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void CleanRegMUICache()
        {
            try
            {
                var fileName = Path.GetFileName(Application.ExecutablePath);
                var progName = fileName.ToUpper();
                using (var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\ShellNoRoam\\MUICache", true))
                {
                    if (key == null)
                        return;
                    var values = key.GetValueNames();
                    try
                    {
                        foreach (var s in values.Where(s => s.ToUpper().Contains(progName)))
                        {
                            key.DeleteValue(s, false);
                        }
                    }
                    catch (SecurityException)
                    {
                    }
                }
            }
            catch (SecurityException)
            {
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void CleanRegOpenSaveMRU()
        {
            try
            {
                var fileName = Path.GetFileName(Application.ExecutablePath);
                var progName = fileName.ToUpper();
                using (var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\ComDlg32\\OpenSaveMRU", true))
                {
                    if (key == null)
                        return;
                    var subkeys = key.GetSubKeyNames();
                    try
                    {
                        foreach (var s in subkeys)
                        {
                            try
                            {
                                using (var sk = key.OpenSubKey(s, true))
                                {
                                    if (sk == null) continue;
                                    var values = sk.GetValueNames();
                                    foreach (var vs in values)
                                    {
                                        var value = sk.GetValue(vs);
                                        if (!(value is string)) continue;
                                        var v = Convert.ToString(value);
                                        if (!string.IsNullOrEmpty(v) && v.ToUpper().Contains(progName))
                                        {
                                            sk.DeleteValue(vs, false);
                                        }
                                    }
                                }
                            }
                            catch (SecurityException)
                            {
                            }
                        }
                    }
                    catch (SecurityException)
                    {
                    }
                }
            }
            catch (SecurityException)
            {
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void CleanRegOpenWithList()
        {
            try
            {
                var fileName = Path.GetFileName(Application.ExecutablePath);
                var progName = fileName.ToUpper();
                using (var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts", true))
                {
                    if (key == null)
                        return;
                    var subkeys = key.GetSubKeyNames();
                    foreach (var s in subkeys)
                    {
                        using (var sk = key.OpenSubKey(s, true))
                        {
                            if (sk == null)
                                continue;
                            var subsubkeys = sk.GetSubKeyNames();
                            foreach (var subs in subsubkeys)
                            {
                                try
                                {
                                    using (var ssk = sk.OpenSubKey(subs, true))
                                    {
                                        if (ssk == null)
                                            continue;
                                        var values = ssk.GetValueNames();
                                        foreach (var vs in values)
                                        {
                                            var value = ssk.GetValue(vs);
                                            if (!(value is string)) continue;
                                            var v = Convert.ToString(value);
                                            if (!string.IsNullOrEmpty(v) && v.ToUpper().Contains(progName))
                                            {
                                                ssk.DeleteValue(vs, false);
                                            }
                                        }
                                    }
                                }
                                catch (SecurityException)
                                {
                                }
                            }
                        }
                    }
                }
            }
            catch (SecurityException)
            {
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
