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
using System.Security;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows;
using PNStaticFonts;

namespace PNotes.NET
{
    internal class PNInterop
    {
        #region Custom messages
        internal const int WM_USER = 0x0400;
        internal const int WPM_BASE = WM_USER + 11180;
        internal const int WPM_CLOSE_PROG = WPM_BASE + 1;
        internal const int WPM_CLOSE_SILENT_SAVE = WPM_BASE + 2;
        internal const int WPM_CLOSE_SILENT_WO_SAVE = WPM_BASE + 3;
        internal const int WPM_NEW_NOTE = WPM_BASE + 4;
        internal const int WPM_NEW_NOTE_FROM_CB = WPM_BASE + 5;
        internal const int WPM_NEW_DIARY = WPM_BASE + 6;
        internal const int WPM_BACKUP = WPM_BASE + 7;
        internal const int WPM_NEW_NOTE_WITH_TEXT = WPM_BASE + 8;
        internal const int WPM_RELOAD_NOTES = WPM_BASE + 9;
        internal const int WPM_START_FROM_ANOTHER_INSTANCE = WPM_BASE + 10;
        #endregion

        #region INI files API
        internal const int MAX_PATH = 260;

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringW", CharSet = CharSet.Unicode)]
        internal static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, string lpReturnedString, int nSize, string lpFileName);
        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringW", CharSet = CharSet.Unicode)]
        internal static extern int GetPrivateProfileStringByBuilder(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStructW", CharSet = CharSet.Unicode)]
        internal static extern bool GetPrivateProfileStruct(string lpszSection, string lpsKey, IntPtr lpStruct, int uSizeStruct, string szFile);

        #endregion

        #region API for drag and resize
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool LockWindowUpdate(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int SendMessage(IntPtr hwnd, uint msg, int wParam, int lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        internal static extern int SendMessageCopyData(IntPtr hwnd, uint msg, IntPtr wParam, ref COPYDATASTRUCT lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SystemParametersInfo")]
        private static extern bool GetFullDrag(uint uiAction, uint uiParam, ref bool pvParam, uint fWinIni);
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SystemParametersInfo")]
        private static extern bool SetFullDrag(uint uiAction, bool uiParam, int pvParam, uint fWinIni);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        internal static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        internal static extern bool ClientToScreen(IntPtr hWnd, ref POINTINT lpPoint);

        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCAPTION = 0x2;
        private const int SPI_SETDRAGFULLWINDOWS = 37;
        private const int SPI_GETDRAGFULLWINDOWS = 38;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTBOTTOM = 15;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        #endregion

        #region API for windows enumeration
        internal delegate bool EnumWindowsProcDelegate(IntPtr hwnd, int lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool EnumWindows(EnumWindowsProcDelegate lpEnumFunc, int lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int count);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetClassName(IntPtr hwnd, StringBuilder text, int count);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetWindowTextLength(IntPtr hwnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool IsWindowVisible(IntPtr hwnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr FindWindow(string className, string windowName);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetWindowPlacement(IntPtr hwnd, [MarshalAs(UnmanagedType.Struct)]ref WINDOWPLACEMENT wp);
        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        internal static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTINT
        {
            public int X;
            public int Y;

            public POINTINT(int x, int y)
            {
                X = x;
                Y = y;
            }
            public POINTINT(Point pt)
            {
                X = Convert.ToInt32(pt.X);
                Y = Convert.ToInt32(pt.Y);
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            internal long x;
            internal long y;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            internal long left;
            internal long top;
            internal long right;
            internal long bottom;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPLACEMENT
        {
            internal uint length;
            internal uint flags;
            internal uint showCmd;
            internal POINT ptMinPosition;
            internal POINT ptMaxPosition;
            internal RECT rcNormalPosition;
        }

        internal const uint SW_SHOWMINIMIZED = 2;

        #endregion

        #region Misc API

        [Flags]
        internal enum DwmWindowAttribute : uint
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_LAST
        }


        internal const int WM_DEVICECHANGE = 0x0219;
        internal const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        internal const int DBT_DEVICEARRIVAL = 0x8000;
        internal const int WM_CLOSE = 0x0010;
        internal const int WM_ACTIVATE = 0x0006;
        internal const int WM_ACTIVATEAPP = 0x001C;
        internal const int GWL_EXSTYLE = -20;
        private const int LOGPIXELSY = 90;

        internal enum ActiveFlags : short
        {
            WA_INACTIVE = 0,
            WA_ACTIVE = 1,
            WA_CLICKACTIVE = 2
        }

        internal const int WS_EX_TOOLWINDOW = 0x00000080;
        internal const int WS_EX_NOACTIVATE = 0x08000000;

        internal const int FR_PRIVATE = 0x10;

        //Used for WM_COPYDATA for string messages
        internal const int WM_COPYDATA = 0x4A;
        internal struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern bool AddFontResourceEx(string lpszFilename, int fl, int pdv);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        private static extern bool RemoveFontResourceEx(string lpszFilename, int fl, int pdv);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern bool DeleteObject(IntPtr hObject);
        [DllImport("dwmapi.dll", PreserveSig = true)]
        internal static extern int DwmSetWindowAttribute(IntPtr hwnd,
                                                       DwmWindowAttribute dwmAttribute,
                                                       IntPtr pvAttribute,
                                                       uint cbAttribute);
        #endregion

        internal static T ReadINIStructure<T>(string fileName, string sectionName, string keyName, T type, int structSize)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocCoTaskMem(structSize);
                if (ptr != IntPtr.Zero)
                {
                    if (GetPrivateProfileStruct(sectionName, keyName, ptr, structSize, fileName))
                    {
                        return (T)Marshal.PtrToStructure(ptr, typeof(T));
                    }
                }
                return default(T);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return default(T);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(ptr);
            }
        }

        internal static T ReadINIStructure<T>(string fileName, string sectionName, string keyName, T type)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(T)));
                if (ptr != IntPtr.Zero)
                {
                    if (GetPrivateProfileStruct(sectionName, keyName, ptr, Marshal.SizeOf(typeof(T)), fileName))
                    {
                        return (T)Marshal.PtrToStructure(ptr, typeof(T));
                    }
                }
                return default(T);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return default(T);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(ptr);
            }
        }

        internal static void ResizeWindowRight(IntPtr handle)
        {
            var fullDrag = false;
            GetFullDrag(SPI_GETDRAGFULLWINDOWS, 0, ref fullDrag, 0);
            SetFullDrag(SPI_SETDRAGFULLWINDOWS, true, 0, 0);
            ReleaseCapture();
            SendMessage(handle, WM_NCLBUTTONDOWN, HTRIGHT, 0);
            SetFullDrag(SPI_SETDRAGFULLWINDOWS, fullDrag, 0, 0);
        }

        internal static void ResizeWindowBottomRight(IntPtr handle)
        {
            var fullDrag = false;
            GetFullDrag(SPI_GETDRAGFULLWINDOWS, 0, ref fullDrag, 0);
            SetFullDrag(SPI_SETDRAGFULLWINDOWS, true, 0, 0);
            ReleaseCapture();
            SendMessage(handle, WM_NCLBUTTONDOWN, HTBOTTOMRIGHT, 0);
            SetFullDrag(SPI_SETDRAGFULLWINDOWS, fullDrag, 0, 0);
        }

        internal static void DragWindow(IntPtr handle)
        {
            bool fullDrag = false;
            GetFullDrag(SPI_GETDRAGFULLWINDOWS, 0, ref fullDrag, 0);
            SetFullDrag(SPI_SETDRAGFULLWINDOWS, true, 0, 0);
            ReleaseCapture();
            SendMessage(handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            SetFullDrag(SPI_SETDRAGFULLWINDOWS, fullDrag, 0, 0);
        }

        internal static void ResizeWindowByBorder(PNBorderDragDirection direction, IntPtr handle)
        {
            bool fullDrag = false;
            GetFullDrag(SPI_GETDRAGFULLWINDOWS, 0, ref fullDrag, 0);
            SetFullDrag(SPI_SETDRAGFULLWINDOWS, true, 0, 0);
            ReleaseCapture();
            switch (direction)
            {
                case PNBorderDragDirection.WestNorth:
                    SendMessage(handle, WM_NCLBUTTONDOWN, HTTOPLEFT, 0);
                    break;
                case PNBorderDragDirection.Left:
                    SendMessage(handle, WM_NCLBUTTONDOWN, HTLEFT, 0);
                    break;
                case PNBorderDragDirection.WestSouth:
                    SendMessage(handle, WM_NCLBUTTONDOWN, HTBOTTOMLEFT, 0);
                    break;
                case PNBorderDragDirection.EastSouth:
                    SendMessage(handle, WM_NCLBUTTONDOWN, HTBOTTOMRIGHT, 0);
                    break;
                case PNBorderDragDirection.Up:
                    SendMessage(handle, WM_NCLBUTTONDOWN, HTTOP, 0);
                    break;
                case PNBorderDragDirection.EastNorth:
                    SendMessage(handle, WM_NCLBUTTONDOWN, HTTOPRIGHT, 0);
                    break;
                case PNBorderDragDirection.Right:
                    SendMessage(handle, WM_NCLBUTTONDOWN, HTRIGHT, 0);
                    break;
                case PNBorderDragDirection.Down:
                    SendMessage(handle, WM_NCLBUTTONDOWN, HTBOTTOM, 0);
                    break;
            }
            SetFullDrag(SPI_SETDRAGFULLWINDOWS, fullDrag, 0, 0);
        }

        internal static void BringWindowToTop(IntPtr handle)
        {
            SetForegroundWindow(handle);
        }

        internal static void AddCustomFonts()
        {
            try
            {
                string[] exts = { ".fon", ".fnt", ".ttf", ".ttc", ".fot", ".otf" };
                var di = new DirectoryInfo(PNPaths.Instance.FontsDir);
                FileInfo[] fonts = di.GetFiles();
                foreach (FileInfo f in fonts)
                {
                    if (exts.Contains(f.Extension))
                    {
                        if (AddFontResourceEx(f.FullName, FR_PRIVATE, 0))
                        {
                            PNStatic.CustomFonts.Add(f.FullName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void RemoveCustomFonts()
        {
            try
            {
                foreach (string f in PNStatic.CustomFonts)
                {
                    RemoveFontResourceEx(f, FR_PRIVATE, 0);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static short LOWORD(int l)
        {
            return (short)(l & 0xffff);
        }

        internal static short HIWORD(int l)
        {
            return (short)(l >> 16);
        }

        internal static int ConvertToLogfontHeight(int height)
        {
            IntPtr hdc = Fonts.GetDC(IntPtr.Zero);
            try
            {
                var result = -Fonts.MulDiv(height, Fonts.GetDeviceCaps(hdc, (int)VariableParams.LOGPIXELSY), 72);// -(height * Fonts.GetDeviceCaps(hdc, LOGPIXELSY)) / 96;
                return result;
                }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return 0;
            }
            finally
            {
                if (!Equals(hdc, IntPtr.Zero))
                    Fonts.ReleaseDC(IntPtr.Zero, hdc);
            }
        }

        internal static int ConvertFromLogfontHeight(int lfHeight)
        {
            IntPtr hdc = Fonts.GetDC(IntPtr.Zero);
            try
            {
                return -(lfHeight * 96) / Fonts.GetDeviceCaps(hdc, LOGPIXELSY);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return 0;
            }
            finally
            {
                if (!Equals(hdc, IntPtr.Zero))
                    Fonts.ReleaseDC(IntPtr.Zero, hdc);
            }
        }

        #region Computer names
        // Based on code by Sacha Barber
        // http://www.codeproject.com/Articles/16113/Retreiving-a-listNames-of-network-computer-names-using

        /// <summary>
        /// Netapi32.dll : The NetServerEnum function lists all servers
        /// of the specified type that are
        /// visible in a domain. For example, an 
        /// application can call NetServerEnum
        /// to listNames all domain controllers only
        /// or all SQL servers only.
        /// You can combine bit masks to listNames
        /// several types. For example, a value 
        /// of 0x00000003  combines the bit
        /// masks for SV_TYPE_WORKSTATION 
        /// (0x00000001) and SV_TYPE_SERVER (0x00000002)
        /// </summary>
        [DllImport("Netapi32", CharSet = CharSet.Auto, SetLastError = true), SuppressUnmanagedCodeSecurity]
        private static extern int NetServerEnum(
            string serverName, // must be null
            int dwLevel,
            ref IntPtr pBuf,
            int dwPrefMaxLen,
            out int dwEntriesRead,
            out int dwTotalEntries,
            int dwServerType,
            string domain, // null for login domain
            out int dwResumeHandle
            );

        /// <summary>
        /// Netapi32.dll : The NetApiBufferFree function frees 
        /// the memory that the NetApiBufferAllocate function allocates. 
        /// Call NetApiBufferFree to free
        /// the memory that other network 
        /// management functions return.
        /// </summary>
        /// <param name="pBuf"></param>
        /// <returns></returns>
        [DllImport("Netapi32", SetLastError = true), SuppressUnmanagedCodeSecurity]
        private static extern int NetApiBufferFree(IntPtr pBuf);

        //create a _SERVER_INFO_100 STRUCTURE
        [StructLayout(LayoutKind.Sequential)]
        private struct SERVER_INFO_100
        {
            internal int sv100_platform_id;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string sv100_name;
        }

        /// <summary>
        /// Uses the DllImport : NetServerEnum
        /// with all its required parameters
        /// (see http://msdn.microsoft.com/library/default.asp?
        ///      url=/library/en-us/netmgmt/netmgmt/netserverenum.asp
        /// for full details or method signature) to
        /// retrieve a listNames of domain SV_TYPE_WORKSTATION
        /// and SV_TYPE_SERVER PC's
        /// </summary>
        /// <returns>List of strings that represents
        /// all the SV_TYPE_WORKSTATION and SV_TYPE_SERVER
        /// PC's in the Domain</returns>
        internal static List<string> GetNetworkComputers()
        {
            //local fields
            var networkComputers = new List<string>();
            const int MAX_PREFERRED_LENGTH = -1;
            const int SV_TYPE_WORKSTATION = 1;
            const int SV_TYPE_SERVER = 2;
            IntPtr buffer = IntPtr.Zero;
            int sizeofInfo = Marshal.SizeOf(typeof(SERVER_INFO_100));


            try
            {
                //call the DllImport : NetServerEnum 
                //with all its required parameters
                //see http://msdn.microsoft.com/library/
                //default.asp?url=/library/en-us/netmgmt/netmgmt/netserverenum.asp
                //for full details of method signature
                int entriesRead;
                int totalEntries;
                int resHandle;
                int ret = NetServerEnum(null, 100, ref buffer,
                                        MAX_PREFERRED_LENGTH,
                                        out entriesRead,
                                        out totalEntries, SV_TYPE_WORKSTATION |
                                                          SV_TYPE_SERVER, null, out
                                                                                    resHandle);
                //if the returned with a NERR_Success 
                //(C++ term), =0 for C#
                if (ret == 0)
                {
                    //loop through all SV_TYPE_WORKSTATION 
                    //and SV_TYPE_SERVER PC's
                    for (int i = 0; i < totalEntries; i++)
                    {
                        //get pointer to, Pointer to the 
                        //buffer that received the data from
                        //the call to NetServerEnum. 
                        //Must ensure to use correct size of 
                        //STRUCTURE to ensure correct 
                        //location in memory is pointed to
                        var tmpBuffer = new IntPtr((int)buffer +
                                                      (i * sizeofInfo));
                        //Have now got a pointer to the listNames 
                        //of SV_TYPE_WORKSTATION and 
                        //SV_TYPE_SERVER PC's, which is unmanaged memory
                        //Needs to Marshal data from an 
                        //unmanaged block of memory to a 
                        //managed object, again using 
                        //STRUCTURE to ensure the correct data
                        //is marshalled 
                        var svrInfo = (SERVER_INFO_100)
                                                  Marshal.PtrToStructure(tmpBuffer,
                                                                         typeof(SERVER_INFO_100));

                        //add the PC names to the ArrayList
                        networkComputers.Add(svrInfo.sv100_name);
                    }
                }
                //return entries found
                return networkComputers;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return new List<string>();
            }
            finally
            {
                NetApiBufferFree(buffer);
            }
        }
        #endregion
    }
}
