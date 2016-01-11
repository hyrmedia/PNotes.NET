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

//this file was orinally found at vbAccelerator - http://www.vbaccelerator.com/home/NET/Code/Libraries/Shell_Projects/Creating_and_Modifying_Shortcuts/article.asp

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PNotes.NET
{
    internal class PNShellLink : IDisposable
    {
        #region IPersist Interface
        [ComImport]
        [Guid("0000010C-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPersist
        {
            [PreserveSig]
            //[helpstring("Returns the class identifier for the component object")]
            void GetClassID(out Guid pClassID);
        }
        #endregion

        #region IPersistFile Interface
        [ComImport]
        [Guid("0000010B-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPersistFile
        {
            // can't get this to go if I extend IPersist, so put it here:
            [PreserveSig]
            void GetClassID(out Guid pClassID);

            //[helpstring("Checks for changes since last file write")]		
            void IsDirty();

            //[helpstring("Opens the specified file and initializes the object from its contents")]		
            void Load(
                [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
                uint dwMode);

            //[helpstring("Saves the object into the specified file")]		
            void Save(
                [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
                [MarshalAs(UnmanagedType.Bool)] bool fRemember);

            //[helpstring("Notifies the object that save is completed")]		
            void SaveCompleted(
                [MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

            //[helpstring("Gets the current name of the file associated with the object")]		
            void GetCurFile(
                [MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
        }
        #endregion

        #region IShellLink Interface
        [ComImport]
        [Guid("000214EE-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkA
        {
            //[helpstring("Retrieves the path and filename of a shell link object")]
            void GetPath(
                [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile,
                int cchMaxPath,
                ref WIN32_FIND_DATAA pfd,
                uint fFlags);

            //[helpstring("Retrieves the listNames of shell link item identifiers")]
            void GetIDList(out IntPtr ppidl);

            //[helpstring("Sets the listNames of shell link item identifiers")]
            void SetIDList(IntPtr pidl);

            //[helpstring("Retrieves the shell link description string")]
            void GetDescription(
                [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile,
                int cchMaxName);

            //[helpstring("Sets the shell link description string")]
            void SetDescription(
                [MarshalAs(UnmanagedType.LPStr)] string pszName);

            //[helpstring("Retrieves the name of the shell link working directory")]
            void GetWorkingDirectory(
                [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszDir,
                int cchMaxPath);

            //[helpstring("Sets the name of the shell link working directory")]
            void SetWorkingDirectory(
                [MarshalAs(UnmanagedType.LPStr)] string pszDir);

            //[helpstring("Retrieves the shell link command-line arguments")]
            void GetArguments(
                [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs,
                int cchMaxPath);

            //[helpstring("Sets the shell link command-line arguments")]
            void SetArguments(
                [MarshalAs(UnmanagedType.LPStr)] string pszArgs);

            //[propget, helpstring("Retrieves or sets the shell link hot key")]
            void GetHotkey(out short pwHotkey);
            //[propput, helpstring("Retrieves or sets the shell link hot key")]
            void SetHotkey(short pwHotkey);

            //[propget, helpstring("Retrieves or sets the shell link show command")]
            void GetShowCmd(out uint piShowCmd);
            //[propput, helpstring("Retrieves or sets the shell link show command")]
            void SetShowCmd(uint piShowCmd);

            //[helpstring("Retrieves the location (path and index) of the shell link icon")]
            void GetIconLocation(
                [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath,
                int cchIconPath,
                out int piIcon);

            //[helpstring("Sets the location (path and index) of the shell link icon")]
            void SetIconLocation(
                [MarshalAs(UnmanagedType.LPStr)] string pszIconPath,
                int iIcon);

            //[helpstring("Sets the shell link relative path")]
            void SetRelativePath(
                [MarshalAs(UnmanagedType.LPStr)] string pszPathRel,
                uint dwReserved);

            //[helpstring("Resolves a shell link. The system searches for the shell link object and updates the shell link path and its listNames of identifiers (if necessary)")]
            void Resolve(
                IntPtr hWnd,
                uint fFlags);

            //[helpstring("Sets the shell link path and filename")]
            void SetPath(
                [MarshalAs(UnmanagedType.LPStr)] string pszFile);
        }

        [ComImport]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkW
        {
            //[helpstring("Retrieves the path and filename of a shell link object")]
            void GetPath(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
                int cchMaxPath,
                ref WIN32_FIND_DATAW pfd,
                uint fFlags);

            //[helpstring("Retrieves the listNames of shell link item identifiers")]
            void GetIDList(out IntPtr ppidl);

            //[helpstring("Sets the listNames of shell link item identifiers")]
            void SetIDList(IntPtr pidl);

            //[helpstring("Retrieves the shell link description string")]
            void GetDescription(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
                int cchMaxName);

            //[helpstring("Sets the shell link description string")]
            void SetDescription(
                [MarshalAs(UnmanagedType.LPWStr)] string pszName);

            //[helpstring("Retrieves the name of the shell link working directory")]
            void GetWorkingDirectory(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,
                int cchMaxPath);

            //[helpstring("Sets the name of the shell link working directory")]
            void SetWorkingDirectory(
                [MarshalAs(UnmanagedType.LPWStr)] string pszDir);

            //[helpstring("Retrieves the shell link command-line arguments")]
            void GetArguments(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs,
                int cchMaxPath);

            //[helpstring("Sets the shell link command-line arguments")]
            void SetArguments(
                [MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

            //[propget, helpstring("Retrieves or sets the shell link hot key")]
            void GetHotkey(out short pwHotkey);
            //[propput, helpstring("Retrieves or sets the shell link hot key")]
            void SetHotkey(short pwHotkey);

            //[propget, helpstring("Retrieves or sets the shell link show command")]
            void GetShowCmd(out uint piShowCmd);
            //[propput, helpstring("Retrieves or sets the shell link show command")]
            void SetShowCmd(uint piShowCmd);

            //[helpstring("Retrieves the location (path and index) of the shell link icon")]
            void GetIconLocation(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
                int cchIconPath,
                out int piIcon);

            //[helpstring("Sets the location (path and index) of the shell link icon")]
            void SetIconLocation(
                [MarshalAs(UnmanagedType.LPWStr)] string pszIconPath,
                int iIcon);

            //[helpstring("Sets the shell link relative path")]
            void SetRelativePath(
                [MarshalAs(UnmanagedType.LPWStr)] string pszPathRel,
                uint dwReserved);

            //[helpstring("Resolves a shell link. The system searches for the shell link object and updates the shell link path and its listNames of identifiers (if necessary)")]
            void Resolve(
                IntPtr hWnd,
                uint fFlags);

            //[helpstring("Sets the shell link path and filename")]
            void SetPath(
                [MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }
        #endregion

        #region ShellLinkCoClass
        [Guid("00021401-0000-0000-C000-000000000046")]
        [ClassInterface(ClassInterfaceType.None)]
        [ComImport]
        private class CShellLink { }
        #endregion

        #region IShellLink Private structs

        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0, CharSet = CharSet.Unicode)]
        private struct WIN32_FIND_DATAW
        {
            public uint dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0, CharSet = CharSet.Ansi)]
        private struct WIN32_FIND_DATAA
        {
            public uint dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0)]
        private struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }
        #endregion

        #region UnManaged Methods
        /*
        private class UnManagedMethods
        {
            [DllImport("Shell32", CharSet = CharSet.Auto)]
            internal extern static int ExtractIconEx(
                [MarshalAs(UnmanagedType.LPTStr)] 
				string lpszFile,
                int nIconIndex,
                IntPtr[] phIconLarge,
                IntPtr[] phIconSmall,
                int nIcons);

            [DllImport("user32")]
            internal extern static int DestroyIcon(IntPtr hIcon);
        }
*/
        #endregion

        private IShellLinkW _linkW;
        private IShellLinkA _linkA;
        private string _ShortcutFile = "";

        private const int MAX_PATH = 512;
        private const uint SLGP_UNCPRIORITY = 0x02;

        #region Constructors
        public PNShellLink()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                _linkW = (IShellLinkW) new CShellLink();
            }
            else
            {
                _linkA = (IShellLinkA)new CShellLink();
            }
        }
        #endregion

        #region IDisposable Members

        private bool disposedValue = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_linkW != null)
                    {
                        Marshal.ReleaseComObject(_linkW);
                        _linkW = null;
                    }
                    if (_linkA != null)
                    {
                        Marshal.ReleaseComObject(_linkA);
                        _linkA = null;
                    }
                }
            }
        }
        ~PNShellLink()
        {
            Dispose(false);
        }
        #endregion

        public string ShortcutFile
        {
            get { return _ShortcutFile; }
            set { _ShortcutFile = value; }
        }

        /*
                private Icon getIcon(bool large)
                {
                    // Get icon index and path:
                    int iconIndex;
                    StringBuilder iconPath = new StringBuilder(260, 260);
                    if (_linkA == null)
                    {
                        _linkW.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                    }
                    else
                    {
                        _linkA.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                    }
                    string iconFile = iconPath.ToString();

                    // Use ExtractIconEx to get the icon:
                    IntPtr[] hIconEx = new[] { IntPtr.Zero };
                    if (large)
                    {
                        var iconsCount = UnManagedMethods.ExtractIconEx(
                            iconFile,
                            iconIndex,
                            hIconEx,
                            null,
                            1);
                    }
                    else
                    {
                        var iconsCount = UnManagedMethods.ExtractIconEx(
                            iconFile,
                            iconIndex,
                            null,
                            hIconEx,
                            1);
                    }
                    // If success then return as a GDI+ object
                    Icon icon = null;
                    if (hIconEx[0] != IntPtr.Zero)
                    {
                        icon = Icon.FromHandle(hIconEx[0]);
                    }
                    return icon;
                }
        */

        /// <summary>
        /// Sets the path to the file containing the icon for this shortcut.
        /// </summary>
        public string IconPath
        {
            set
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                int iconIndex;
                if (_linkA == null)
                {
                    _linkW.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                else
                {
                    _linkA.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                if (_linkA == null)
                {
                    _linkW.SetIconLocation(value, iconIndex);
                }
                else
                {
                    _linkA.SetIconLocation(value, iconIndex);
                }
            }
        }

        /// <summary>
        /// Sets the index of this icon within the icon path's resources
        /// </summary>
        public int IconIndex
        {
            set
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                int iconIndex;
                if (_linkA == null)
                {
                    _linkW.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                else
                {
                    _linkA.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                if (_linkA == null)
                {
                    _linkW.SetIconLocation(iconPath.ToString(), value);
                }
                else
                {
                    _linkA.SetIconLocation(iconPath.ToString(), value);
                }
            }
        }

        /// <summary>
        /// Sets/sets the fully qualified path to the link's target
        /// </summary>
        public string Target
        {
            get
            {
                var sb = new StringBuilder(MAX_PATH, MAX_PATH);
                if (_linkW != null)
                {
                    var wd = new WIN32_FIND_DATAW();
                    _linkW.GetPath(sb, MAX_PATH, ref wd, SLGP_UNCPRIORITY);
                }
                else
                {
                    var wd = new WIN32_FIND_DATAA();
                    _linkA.GetPath(sb, MAX_PATH, ref wd, SLGP_UNCPRIORITY);
                }
                return sb.ToString();
            }
            set
            {
                if (_linkA == null)
                {
                    _linkW.SetPath(value);
                }
                else
                {
                    _linkA.SetPath(value);
                }
            }
        }

        /// <summary>
        /// Sets the Working Directory for the Link
        /// </summary>
        public string WorkingDirectory
        {
            set
            {
                if (_linkA == null)
                {
                    _linkW.SetWorkingDirectory(value);
                }
                else
                {
                    _linkA.SetWorkingDirectory(value);
                }
            }
        }

        /// <summary>
        /// Sets the description of the link
        /// </summary>
        public string Description
        {
            set
            {
                if (_linkA == null)
                {
                    _linkW.SetDescription(value);
                }
                else
                {
                    _linkA.SetDescription(value);
                }
            }
        }

        /// <summary>
        /// Sets the command line arguments of the link
        /// </summary>
        public string Arguments
        {
            get
            {
                var sb = new StringBuilder(MAX_PATH, MAX_PATH);
                if (_linkW != null)
                    _linkW.GetArguments(sb, MAX_PATH);
                else
                    _linkA.GetArguments(sb, MAX_PATH);
                return sb.ToString();
            }
            set
            {
                if (_linkA == null)
                {
                    _linkW.SetArguments(value);
                }
                else
                {
                    _linkA.SetArguments(value);
                }
            }
        }

        public void Save()
        {
            // SaveProp the object to disk
            if (_linkA == null)
            {
                ((IPersistFile)_linkW).Save(_ShortcutFile, true);
            }
            else
            {
                ((IPersistFile)_linkA).Save(_ShortcutFile, true);
            }
        }

        public void Load(string linkPath)
        {
            if (_linkA != null)
            {
                ((IPersistFile) _linkA).Load(linkPath, 0);
            }
            else
            {
                ((IPersistFile)_linkW).Load(linkPath, 0);
            }
        }
    }
}
