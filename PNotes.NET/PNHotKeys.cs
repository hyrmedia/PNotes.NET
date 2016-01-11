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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PNotes.NET
{
    [Flags]
    internal enum HotkeyModifiers
    {
        MOD_NONE = 0x0000,
        MOD_ALT = 0x0001,
        MOD_CONTROL = 0x0002,
        MOD_SHIFT = 0x0004,
        MOD_WIN = 0x0008
    }

    internal enum HotkeyType
    {
        Main,
        Note,
        Edit,
        Group
    }

    internal static class HotkeysStatic
    {
        internal const int VK_OEM_1 = 0xBA;
        internal const int VK_OEM_PLUS = 0xBB;
        internal const int VK_OEM_COMMA = 0xBC;
        internal const int VK_OEM_MINUS = 0xBD;
        internal const int VK_OEM_PERIOD = 0xBE;
        internal const int VK_OEM_2 = 0xBF;
        internal const int VK_OEM_3 = 0xC0;
        internal const int VK_OEM_4 = 0xDB;
        internal const int VK_OEM_5 = 0xDC;
        internal const int VK_OEM_6 = 0xDD;
        internal const int VK_OEM_7 = 0xDE;
        internal const int VK_OEM_8 = 0xDF;
        internal const int VK_BACK = 0x08;
        internal const int VK_RETURN = 0x0D;
        internal const int VK_SHIFT = 0x10;
        internal const int VK_CONTROL = 0x11;
        internal const int VK_MENU = 0x12;
        internal const int VK_PAUSE = 0x13;
        internal const int VK_ESCAPE = 0x1B;
        internal const int VK_TAB = 0x09;
        internal const int VK_SPACE = 0x20;
        internal const int VK_PRIOR = 0x21;
        internal const int VK_NEXT = 0x22;
        internal const int VK_END = 0x23;
        internal const int VK_HOME = 0x24;
        internal const int VK_LEFT = 0x25;
        internal const int VK_UP = 0x26;
        internal const int VK_RIGHT = 0x27;
        internal const int VK_DOWN = 0x28;
        internal const int VK_INSERT = 0x2D;
        internal const int VK_DELETE = 0x2E;
        internal const int VK_SCROLL = 0x91;
        internal const int VK_0 = 0x30;
        internal const int VK_1 = 0x31;
        internal const int VK_2 = 0x32;
        internal const int VK_3 = 0x33;
        internal const int VK_4 = 0x34;
        internal const int VK_5 = 0x35;
        internal const int VK_6 = 0x36;
        internal const int VK_7 = 0x37;
        internal const int VK_8 = 0x38;
        internal const int VK_9 = 0x39;
        internal const int VK_A = 0x41;
        internal const int VK_B = 0x42;
        internal const int VK_C = 0x43;
        internal const int VK_D = 0x44;
        internal const int VK_E = 0x45;
        internal const int VK_F = 0x46;
        internal const int VK_G = 0x47;
        internal const int VK_H = 0x48;
        internal const int VK_I = 0x49;
        internal const int VK_J = 0x4A;
        internal const int VK_K = 0x4B;
        internal const int VK_L = 0x4C;
        internal const int VK_M = 0x4D;
        internal const int VK_N = 0x4E;
        internal const int VK_O = 0x4F;
        internal const int VK_P = 0x50;
        internal const int VK_Q = 0x51;
        internal const int VK_R = 0x52;
        internal const int VK_S = 0x53;
        internal const int VK_T = 0x54;
        internal const int VK_U = 0x55;
        internal const int VK_V = 0x56;
        internal const int VK_W = 0x57;
        internal const int VK_X = 0x58;
        internal const int VK_Y = 0x59;
        internal const int VK_Z = 0x5A;
        internal const int VK_LWIN = 0x5B;
        internal const int VK_RWIN = 0x5C;
        internal const int VK_NUMPAD0 = 0x60;
        internal const int VK_NUMPAD1 = 0x61;
        internal const int VK_NUMPAD2 = 0x62;
        internal const int VK_NUMPAD3 = 0x63;
        internal const int VK_NUMPAD4 = 0x64;
        internal const int VK_NUMPAD5 = 0x65;
        internal const int VK_NUMPAD6 = 0x66;
        internal const int VK_NUMPAD7 = 0x67;
        internal const int VK_NUMPAD8 = 0x68;
        internal const int VK_NUMPAD9 = 0x69;
        internal const int VK_MULTIPLY = 0x6A;
        internal const int VK_ADD = 0x6B;
        internal const int VK_SUBTRACT = 0x6D;
        internal const int VK_DIVIDE = 0x6F;
        internal const int VK_F1 = 0x70;
        internal const int VK_F2 = 0x71;
        internal const int VK_F3 = 0x72;
        internal const int VK_F4 = 0x73;
        internal const int VK_F5 = 0x74;
        internal const int VK_F6 = 0x75;
        internal const int VK_F7 = 0x76;
        internal const int VK_F8 = 0x77;
        internal const int VK_F9 = 0x78;
        internal const int VK_F10 = 0x79;
        internal const int VK_F11 = 0x7A;
        internal const int VK_F12 = 0x7B;
        internal const int VK_F13 = 0x7C;
        internal const int VK_F14 = 0x7D;
        internal const int VK_F15 = 0x7E;
        internal const int VK_F16 = 0x7F;
        internal const int VK_F17 = 0x80;
        internal const int VK_F18 = 0x81;
        internal const int VK_F19 = 0x82;
        internal const int VK_F20 = 0x83;
        internal const int VK_F21 = 0x84;
        internal const int VK_F22 = 0x85;
        internal const int VK_F23 = 0x86;
        internal const int VK_F24 = 0x87;
        internal const int VK_LSHIFT = 0xA0;
        internal const int VK_RSHIFT = 0xA1;
        internal const int VK_LCONTROL = 0xA2;
        internal const int VK_RCONTROL = 0xA3;
        internal const int VK_LMENU = 0xA4;
        internal const int VK_RMENU = 0xA5;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetKeyState(int nVirtKey);

        internal static bool RegisterHK(IntPtr hwnd, PNHotKey hk)
        {
            return RegisterHotKey(hwnd, hk.ID, (uint)hk.Modifiers, hk.VK);
        }

        internal static bool UnregisterHK(IntPtr hwnd, int id)
        {
            return UnregisterHotKey(hwnd, id);
        }

        internal static bool LeftShiftDown()
        {
            if ((GetKeyState(VK_LSHIFT) & 0x8000) == 0x8000)
                return true;
            return false;
        }

        internal static bool ShiftDown()
        {
            if ((GetKeyState(VK_SHIFT) & 0x8000) == 0x8000)
                return true;
            return false;
        }

        internal static bool ControlDown()
        {
            if ((GetKeyState(VK_CONTROL) & 0x8000) == 0x8000)
                return true;
            return false;
        }

        internal static Keys GetShortcut(PNHotKey hk)
        {
            var keys = Keys.None;
            if ((hk.Modifiers & HotkeyModifiers.MOD_CONTROL) > 0)
                keys |= Keys.Control;
            if ((hk.Modifiers & HotkeyModifiers.MOD_ALT) > 0)
                keys |= Keys.Menu;
            if ((hk.Modifiers & HotkeyModifiers.MOD_SHIFT) > 0)
                keys |= Keys.Shift;
            if ((hk.Modifiers & HotkeyModifiers.MOD_WIN) > 0)
                keys |= Keys.LWin;
            switch (hk.VK)
            {
                case VK_0:
                    keys |= Keys.D0;
                    break;
                case VK_1:
                    keys |= Keys.D1;
                    break;
                case VK_2:
                    keys |= Keys.D2;
                    break;
                case VK_3:
                    keys |= Keys.D3;
                    break;
                case VK_4:
                    keys |= Keys.D4;
                    break;
                case VK_5:
                    keys |= Keys.D5;
                    break;
                case VK_6:
                    keys |= Keys.D6;
                    break;
                case VK_7:
                    keys |= Keys.D7;
                    break;
                case VK_8:
                    keys |= Keys.D8;
                    break;
                case VK_9:
                    keys |= Keys.D9;
                    break;
                case VK_F1:
                    keys |= Keys.F1;
                    break;
                case VK_F2:
                    keys |= Keys.F2;
                    break;
                case VK_F3:
                    keys |= Keys.F3;
                    break;
                case VK_F4:
                    keys |= Keys.F4;
                    break;
                case VK_F5:
                    keys |= Keys.F5;
                    break;
                case VK_F6:
                    keys |= Keys.F6;
                    break;
                case VK_F7:
                    keys |= Keys.F7;
                    break;
                case VK_F8:
                    keys |= Keys.F8;
                    break;
                case VK_F9:
                    keys |= Keys.F9;
                    break;
                case VK_F10:
                    keys |= Keys.F10;
                    break;
                case VK_F11:
                    keys |= Keys.F11;
                    break;
                case VK_F12:
                    keys |= Keys.F12;
                    break;
                case VK_F13:
                    keys |= Keys.F13;
                    break;
                case VK_F14:
                    keys |= Keys.F14;
                    break;
                case VK_F15:
                    keys |= Keys.F15;
                    break;
                case VK_F16:
                    keys |= Keys.F16;
                    break;
                case VK_F17:
                    keys |= Keys.F17;
                    break;
                case VK_F18:
                    keys |= Keys.F18;
                    break;
                case VK_F19:
                    keys |= Keys.F19;
                    break;
                case VK_F20:
                    keys |= Keys.F20;
                    break;
                case VK_F21:
                    keys |= Keys.F21;
                    break;
                case VK_F22:
                    keys |= Keys.F22;
                    break;
                case VK_F23:
                    keys |= Keys.F23;
                    break;
                case VK_F24:
                    keys |= Keys.F24;
                    break;
                case VK_NUMPAD0:
                    keys |= Keys.NumPad0;
                    break;
                case VK_NUMPAD1:
                    keys |= Keys.NumPad1;
                    break;
                case VK_NUMPAD2:
                    keys |= Keys.NumPad2;
                    break;
                case VK_NUMPAD3:
                    keys |= Keys.NumPad3;
                    break;
                case VK_NUMPAD4:
                    keys |= Keys.NumPad4;
                    break;
                case VK_NUMPAD5:
                    keys |= Keys.NumPad5;
                    break;
                case VK_NUMPAD6:
                    keys |= Keys.NumPad6;
                    break;
                case VK_NUMPAD7:
                    keys |= Keys.NumPad7;
                    break;
                case VK_NUMPAD8:
                    keys |= Keys.NumPad8;
                    break;
                case VK_NUMPAD9:
                    keys |= Keys.NumPad9;
                    break;
                case VK_A:
                    keys |= Keys.A;
                    break;
                case VK_B:
                    keys |= Keys.B;
                    break;
                case VK_C:
                    keys |= Keys.C;
                    break;
                case VK_D:
                    keys |= Keys.D;
                    break;
                case VK_E:
                    keys |= Keys.E;
                    break;
                case VK_F:
                    keys |= Keys.F;
                    break;
                case VK_G:
                    keys |= Keys.G;
                    break;
                case VK_H:
                    keys |= Keys.H;
                    break;
                case VK_I:
                    keys |= Keys.I;
                    break;
                case VK_J:
                    keys |= Keys.J;
                    break;
                case VK_K:
                    keys |= Keys.K;
                    break;
                case VK_L:
                    keys |= Keys.L;
                    break;
                case VK_M:
                    keys |= Keys.M;
                    break;
                case VK_N:
                    keys |= Keys.N;
                    break;
                case VK_O:
                    keys |= Keys.O;
                    break;
                case VK_P:
                    keys |= Keys.P;
                    break;
                case VK_Q:
                    keys |= Keys.Q;
                    break;
                case VK_R:
                    keys |= Keys.R;
                    break;
                case VK_S:
                    keys |= Keys.S;
                    break;
                case VK_T:
                    keys |= Keys.T;
                    break;
                case VK_U:
                    keys |= Keys.U;
                    break;
                case VK_V:
                    keys |= Keys.V;
                    break;
                case VK_W:
                    keys |= Keys.W;
                    break;
                case VK_X:
                    keys |= Keys.X;
                    break;
                case VK_Y:
                    keys |= Keys.Y;
                    break;
                case VK_Z:
                    keys |= Keys.Z;
                    break;
                case VK_ADD:
                    keys |= Keys.Add;
                    break;
                case VK_BACK:
                    keys = Keys.Back;
                    break;
                case VK_DELETE:
                    keys = Keys.Delete;
                    break;
                case VK_DIVIDE:
                    keys = Keys.Divide;
                    break;
                case VK_DOWN:
                    keys = Keys.Down;
                    break;
                case VK_END:
                    keys = Keys.End;
                    break;
                case VK_ESCAPE:
                    keys = Keys.Escape;
                    break;
                case VK_HOME:
                    keys = Keys.Home;
                    break;
                case VK_INSERT:
                    keys = Keys.Insert;
                    break;
                case VK_LEFT:
                    keys = Keys.Left;
                    break;
                case VK_MULTIPLY:
                    keys = Keys.Multiply;
                    break;
                case VK_NEXT:
                    keys = Keys.Next;
                    break;
                case VK_OEM_1:
                    keys = Keys.Oem1;
                    break;
                case VK_OEM_2:
                    keys = Keys.Oem2;
                    break;
                case VK_OEM_3:
                    keys = Keys.Oem3;
                    break;
                case VK_OEM_4:
                    keys = Keys.Oem4;
                    break;
                case VK_OEM_5:
                    keys = Keys.Oem5;
                    break;
                case VK_OEM_6:
                    keys = Keys.Oem6;
                    break;
                case VK_OEM_7:
                    keys = Keys.Oem7;
                    break;
                case VK_OEM_8:
                    keys = Keys.Oem8;
                    break;
                case VK_OEM_COMMA:
                    keys = Keys.Oemcomma;
                    break;
                case VK_OEM_MINUS:
                    keys = Keys.OemMinus;
                    break;
                case VK_OEM_PERIOD:
                    keys = Keys.OemPeriod;
                    break;
                case VK_OEM_PLUS:
                    keys = Keys.Oemplus;
                    break;
                case VK_PAUSE:
                    keys = Keys.Pause;
                    break;
                case VK_PRIOR:
                    keys = Keys.Prior;
                    break;
                case VK_RETURN:
                    keys = Keys.Return;
                    break;
                case VK_RIGHT:
                    keys = Keys.Right;
                    break;
                case VK_SCROLL:
                    keys = Keys.Scroll;
                    break;
                case VK_SPACE:
                    keys = Keys.Space;
                    break;
                case VK_SUBTRACT:
                    keys = Keys.Subtract;
                    break;
                case VK_TAB:
                    keys = Keys.Tab;
                    break;
                case VK_UP:
                    keys = Keys.Up;
                    break;
            }
            return keys;
        }

        internal static HotkeyModifiers GetModifiers()
        {
            string dummy = "";
            return GetModifiers(ref dummy);
        }

        internal static HotkeyModifiers GetModifiers(ref string text)
        {
            var result = HotkeyModifiers.MOD_NONE;
            if ((GetKeyState(VK_CONTROL) & 0x8000) == 0x8000)
            {
                result |= HotkeyModifiers.MOD_CONTROL;
                text += "Ctrl+";
            }
            if ((GetKeyState(VK_SHIFT) & 0x8000) == 0x8000)
            {
                result |= HotkeyModifiers.MOD_SHIFT;
                text += "Shift+";
            }
            if ((GetKeyState(VK_MENU) & 0x8000) == 0x8000)
            {
                result |= HotkeyModifiers.MOD_ALT;
                text += "Alt+";
            }
            if ((GetKeyState(VK_LWIN) & 0x8000) == 0x8000 || (GetKeyState(VK_RWIN) & 0x8000) == 0x8000)
            {
                result |= HotkeyModifiers.MOD_WIN;
                text += "Win+";
            }
            return result;
        }

        internal static uint GetKey()
        {
            string dummy = "";
            return GetKey(ref dummy);
        }

        internal static uint GetKey(ref string text)
        {
            const uint key = 0;
            if ((GetKeyState(VK_A) & 0x8000) == 0x8000)
            {
                text = "A";
                return VK_A;
            }
            if ((GetKeyState(VK_B) & 0x8000) == 0x8000)
            {
                text = "B";
                return VK_B;
            }
            if ((GetKeyState(VK_C) & 0x8000) == 0x8000)
            {
                text = "C";
                return VK_C;
            }
            if ((GetKeyState(VK_D) & 0x8000) == 0x8000)
            {
                text = "D";
                return VK_D;
            }
            if ((GetKeyState(VK_E) & 0x8000) == 0x8000)
            {
                text = "E";
                return VK_E;
            }
            if ((GetKeyState(VK_F) & 0x8000) == 0x8000)
            {
                text = "F";
                return VK_F;
            }
            if ((GetKeyState(VK_G) & 0x8000) == 0x8000)
            {
                text = "G";
                return VK_G;
            }
            if ((GetKeyState(VK_H) & 0x8000) == 0x8000)
            {
                text = "H";
                return VK_H;
            }
            if ((GetKeyState(VK_I) & 0x8000) == 0x8000)
            {
                text = "I";
                return VK_I;
            }
            if ((GetKeyState(VK_J) & 0x8000) == 0x8000)
            {
                text = "J";
                return VK_J;
            }
            if ((GetKeyState(VK_K) & 0x8000) == 0x8000)
            {
                text = "K";
                return VK_K;
            }
            if ((GetKeyState(VK_L) & 0x8000) == 0x8000)
            {
                text = "L";
                return VK_L;
            }
            if ((GetKeyState(VK_M) & 0x8000) == 0x8000)
            {
                text = "M";
                return VK_M;
            }
            if ((GetKeyState(VK_N) & 0x8000) == 0x8000)
            {
                text = "N";
                return VK_N;
            }
            if ((GetKeyState(VK_O) & 0x8000) == 0x8000)
            {
                text = "O";
                return VK_O;
            }
            if ((GetKeyState(VK_P) & 0x8000) == 0x8000)
            {
                text = "P";
                return VK_P;
            }
            if ((GetKeyState(VK_Q) & 0x8000) == 0x8000)
            {
                text = "Q";
                return VK_Q;
            }
            if ((GetKeyState(VK_R) & 0x8000) == 0x8000)
            {
                text = "R";
                return VK_R;
            }
            if ((GetKeyState(VK_S) & 0x8000) == 0x8000)
            {
                text = "S";
                return VK_S;
            }
            if ((GetKeyState(VK_T) & 0x8000) == 0x8000)
            {
                text = "T";
                return VK_T;
            }
            if ((GetKeyState(VK_U) & 0x8000) == 0x8000)
            {
                text = "U";
                return VK_U;
            }
            if ((GetKeyState(VK_V) & 0x8000) == 0x8000)
            {
                text = "V";
                return VK_V;
            }
            if ((GetKeyState(VK_W) & 0x8000) == 0x8000)
            {
                text = "W";
                return VK_W;
            }
            if ((GetKeyState(VK_X) & 0x8000) == 0x8000)
            {
                text = "X";
                return VK_X;
            }
            if ((GetKeyState(VK_Y) & 0x8000) == 0x8000)
            {
                text = "Y";
                return VK_Y;
            }
            if ((GetKeyState(VK_Z) & 0x8000) == 0x8000)
            {
                text = "Z";
                return VK_Z;
            }
            if ((GetKeyState(VK_ESCAPE) & 0x8000) == 0x8000)
            {
                text = "Esc";
                return VK_ESCAPE;
            }
            if ((GetKeyState(VK_F1) & 0x8000) == 0x8000)
            {
                text = "F1";
                return VK_F1;
            }
            if ((GetKeyState(VK_F2) & 0x8000) == 0x8000)
            {
                text = "F2";
                return VK_F2;
            }
            if ((GetKeyState(VK_F3) & 0x8000) == 0x8000)
            {
                text = "F3";
                return VK_F3;
            }
            if ((GetKeyState(VK_F4) & 0x8000) == 0x8000)
            {
                text = "F4";
                return VK_F4;
            }
            if ((GetKeyState(VK_F5) & 0x8000) == 0x8000)
            {
                text = "F5";
                return VK_F5;
            }
            if ((GetKeyState(VK_F6) & 0x8000) == 0x8000)
            {
                text = "F6";
                return VK_F6;
            }
            if ((GetKeyState(VK_F7) & 0x8000) == 0x8000)
            {
                text = "F7";
                return VK_F7;
            }
            if ((GetKeyState(VK_F8) & 0x8000) == 0x8000)
            {
                text = "F8";
                return VK_F8;
            }
            if ((GetKeyState(VK_F9) & 0x8000) == 0x8000)
            {
                text = "F9";
                return VK_F9;
            }
            if ((GetKeyState(VK_F10) & 0x8000) == 0x8000)
            {
                text = "F10";
                return VK_F10;
            }
            if ((GetKeyState(VK_F11) & 0x8000) == 0x8000)
            {
                text = "F11";
                return VK_F11;
            }
            if ((GetKeyState(VK_F12) & 0x8000) == 0x8000)
            {
                text = "F12";
                return VK_F12;
            }
            if ((GetKeyState(VK_BACK) & 0x8000) == 0x8000)
            {
                text = "Backspace";
                return VK_BACK;
            }
            if ((GetKeyState(VK_INSERT) & 0x8000) == 0x8000)
            {
                text = "Ins";
                return VK_INSERT;
            }
            if ((GetKeyState(VK_HOME) & 0x8000) == 0x8000)
            {
                text = "Home";
                return VK_HOME;
            }
            if ((GetKeyState(VK_PRIOR) & 0x8000) == 0x8000)
            {
                text = "PgUp";
                return VK_PRIOR;
            }
            if ((GetKeyState(VK_NEXT) & 0x8000) == 0x8000)
            {
                text = "PgDn";
                return VK_NEXT;
            }
            if ((GetKeyState(VK_END) & 0x8000) == 0x8000)
            {
                text = "End";
                return VK_END;
            }
            if ((GetKeyState(VK_DELETE) & 0x8000) == 0x8000)
            {
                text = "Del";
                return VK_DELETE;
            }
            if ((GetKeyState(VK_SPACE) & 0x8000) == 0x8000)
            {
                text = "Space";
                return VK_SPACE;
            }
            if ((GetKeyState(VK_UP) & 0x8000) == 0x8000)
            {
                text = "Up";
                return VK_UP;
            }
            if ((GetKeyState(VK_DOWN) & 0x8000) == 0x8000)
            {
                text = "Down";
                return VK_DOWN;
            }
            if ((GetKeyState(VK_LEFT) & 0x8000) == 0x8000)
            {
                text = "Left";
                return VK_LEFT;
            }
            if ((GetKeyState(VK_RIGHT) & 0x8000) == 0x8000)
            {
                text = "Right";
                return VK_RIGHT;
            }
            if ((GetKeyState(VK_0) & 0x8000) == 0x8000)
            {
                text = "0";
                return VK_0;
            }
            if ((GetKeyState(VK_1) & 0x8000) == 0x8000)
            {
                text = "1";
                return VK_1;
            }
            if ((GetKeyState(VK_2) & 0x8000) == 0x8000)
            {
                text = "2";
                return VK_2;
            }
            if ((GetKeyState(VK_3) & 0x8000) == 0x8000)
            {
                text = "3";
                return VK_3;
            }
            if ((GetKeyState(VK_4) & 0x8000) == 0x8000)
            {
                text = "4";
                return VK_4;
            }
            if ((GetKeyState(VK_5) & 0x8000) == 0x8000)
            {
                text = "5";
                return VK_5;
            }
            if ((GetKeyState(VK_6) & 0x8000) == 0x8000)
            {
                text = "6";
                return VK_6;
            }
            if ((GetKeyState(VK_7) & 0x8000) == 0x8000)
            {
                text = "7";
                return VK_7;
            }
            if ((GetKeyState(VK_8) & 0x8000) == 0x8000)
            {
                text = "8";
                return VK_8;
            }
            if ((GetKeyState(VK_9) & 0x8000) == 0x8000)
            {
                text = "9";
                return VK_9;
            }
            if ((GetKeyState(VK_NUMPAD0) & 0x8000) == 0x8000)
            {
                text = "Num 0";
                return VK_NUMPAD0;
            }
            if ((GetKeyState(VK_NUMPAD1) & 0x8000) == 0x8000)
            {
                text = "Num 1";
                return VK_NUMPAD1;
            }
            if ((GetKeyState(VK_NUMPAD2) & 0x8000) == 0x8000)
            {
                text = "Num 2";
                return VK_NUMPAD2;
            }
            if ((GetKeyState(VK_NUMPAD3) & 0x8000) == 0x8000)
            {
                text = "Num 3";
                return VK_NUMPAD3;
            }
            if ((GetKeyState(VK_NUMPAD4) & 0x8000) == 0x8000)
            {
                text = "Num 4";
                return VK_NUMPAD4;
            }
            if ((GetKeyState(VK_NUMPAD5) & 0x8000) == 0x8000)
            {
                text = "Num 5";
                return VK_NUMPAD5;
            }
            if ((GetKeyState(VK_NUMPAD6) & 0x8000) == 0x8000)
            {
                text = "Num 6";
                return VK_NUMPAD6;
            }
            if ((GetKeyState(VK_NUMPAD7) & 0x8000) == 0x8000)
            {
                text = "Num 7";
                return VK_NUMPAD7;
            }
            if ((GetKeyState(VK_NUMPAD8) & 0x8000) == 0x8000)
            {
                text = "Num 8";
                return VK_NUMPAD8;
            }
            if ((GetKeyState(VK_NUMPAD9) & 0x8000) == 0x8000)
            {
                text = "Num 9";
                return VK_NUMPAD9;
            }
            if ((GetKeyState(VK_PAUSE) & 0x8000) == 0x8000)
            {
                text = "Pause";
                return VK_PAUSE;
            }
            if ((GetKeyState(VK_ADD) & 0x8000) == 0x8000)
            {
                text = "+";
                return VK_ADD;
            }
            if ((GetKeyState(VK_SUBTRACT) & 0x8000) == 0x8000)
            {
                text = "-";
                return VK_SUBTRACT;
            }
            if ((GetKeyState(VK_MULTIPLY) & 0x8000) == 0x8000)
            {
                text = "*";
                return VK_MULTIPLY;
            }
            if ((GetKeyState(VK_DIVIDE) & 0x8000) == 0x8000)
            {
                text = "/";
                return VK_DIVIDE;
            }
            if ((GetKeyState(VK_RETURN) & 0x8000) == 0x8000)
            {
                text = "Enter";
                return VK_RETURN;
            }
            return key;
        }
    }
}
