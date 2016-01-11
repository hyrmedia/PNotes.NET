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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Xml.Linq;
using Microsoft.Win32;

namespace PNotes.NET
{
    internal class PNOffice
    {
        private const string OUTLOOK_APP = "Outlook.Application";
        private const string EXCEL_APP = "Excel.Application";
        private const string WORD_APP = "Word.Application";
        private const string ONENOTE_APP = "OneNote.Application";

        private const string OUTLOOK_EXE = "outlook.exe";
        private const string EXCEL_EXE = "excel.exe";
        private const string WORD_EXE = "winword.exe";
        private const string ONENOTE_EXE = "onenote.exe";

        private static string getOfficeComponentPath(OfficeApp app)
        {
            try
            {
                const string regKey = @"Software\Microsoft\Windows\CurrentVersion\App Paths";
                const string regKey64 = @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\App Paths";

                var exeName = "";
                switch (app)
                {
                    case OfficeApp.Outlook:
                        exeName = OUTLOOK_EXE;
                        break;
                    case OfficeApp.OneNote:
                        exeName = ONENOTE_EXE;
                        break;
                    case OfficeApp.Excel:
                        exeName = EXCEL_EXE;
                        break;
                    case OfficeApp.Word:
                        exeName = WORD_EXE;
                        break;
                }
                //try current user
                using (var key = Registry.CurrentUser)
                {
                    using (var subKey = key.OpenSubKey(regKey + "\\" + exeName, false))
                    {
                        if (subKey != null && subKey.ValueCount > 0)
                        {
                            return subKey.GetValue("").ToString();
                        }
                    }
                }
                //try local machine
                using (var key = Registry.LocalMachine)
                {
                    using (var subKey = key.OpenSubKey(regKey + "\\" + exeName, false))
                    {
                        if (subKey != null && subKey.ValueCount > 0)
                        {
                            return subKey.GetValue("").ToString();
                        }
                    }
                }
                //try local machine for x64
                using (var key = Registry.LocalMachine)
                {
                    using (var subKey = key.OpenSubKey(regKey64 + "\\" + exeName, false))
                    {
                        if (subKey != null && subKey.ValueCount > 0)
                        {
                            return subKey.GetValue("").ToString();
                        }
                    }
                }
                return "";
            }
            catch (NullReferenceException nex)
            {
                PNStatic.LogException(nex, false);
                return "";
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        internal static int GetOfficeAppVersion(OfficeApp app)
        {
            try
            {
                var filePath = getOfficeComponentPath(app);
                return filePath == "" ? 0 : PNStatic.GetFileMajorVersion(filePath);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return 0;
            }
        }

        internal static bool IsAppInstalled(OfficeApp app)
        {
            try
            {
                var keyName = "";
                switch (app)
                {
                    case OfficeApp.Outlook:
                        keyName = OUTLOOK_APP;
                        break;
                    case OfficeApp.Excel:
                        keyName = EXCEL_APP;
                        break;
                    case OfficeApp.Word:
                        keyName = WORD_APP;
                        break;
                }
                var key = Registry.ClassesRoot;
                var subKey = key.OpenSubKey(keyName);
                return subKey != null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal static List<Tuple<string, string>> GetOutlookContacts()
        {
            dynamic outlook = null;
            var list = new List<Tuple<string, string>>();
            try
            {
                try
                {
                    // if Outlook already runs
                    outlook = Marshal.GetActiveObject(OUTLOOK_APP);
                }
                catch
                {
                    // get reference to IDispatch interface
                    var outlookType = Type.GetTypeFromProgID(OUTLOOK_APP);
                    // run Outlook
                    outlook = Activator.CreateInstance(outlookType);
                }

                var folderContacts = outlook.Session.GetDefaultFolder(10); //OlDefaultFolders.olFolderContacts
                var outlookItems = folderContacts.Items;
                for (var i = 0; i < outlookItems.Count; i++)
                {
                    var contact = outlookItems[i + 1];
                    list.Add(Tuple.Create(contact.FullName, contact.Email1Address));
                }
                return list;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return list;
            }
            finally
            {
                if (outlook != null)
                {
                    Marshal.ReleaseComObject(outlook);
                    GC.GetTotalMemory(true);
                }
            }
        }

        internal static bool ExportToOutlookNote(string text)
        {
            dynamic outlook = null;
            try
            {
                try
                {
                    // if Outlook already runs
                    outlook = Marshal.GetActiveObject("Outlook.Application");
                }
                catch
                {
                    PNMessageBox.Show(PNLang.Instance.GetMessageText("outlook_not_running", "MS Outlook application is not running. Please, start the application and try again."), PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return false;
                }

                var note = outlook.CreateItem(5);
                note.Body = text;
                note.Save();
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
            finally
            {
                if (outlook != null)
                {
                    Marshal.ReleaseComObject(outlook);
                    GC.GetTotalMemory(true);
                }
            }
        }
    }
}
