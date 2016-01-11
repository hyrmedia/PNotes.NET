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
using System.Windows;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for Footer.xaml
    /// </summary>
    public partial class SkinlessFooter : IFooter
    {
        internal event EventHandler<MarkButtonClickedEventArgs> MarkButtonClicked;

        public SkinlessFooter()
        {
            InitializeComponent();
        }

        private readonly List<MarkButton> _MarkButtons = new List<MarkButton>();

        private void MarkButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as MarkButton;
            if (button == null || MarkButtonClicked == null) return;
            MarkButtonClicked(this, new MarkButtonClickedEventArgs(button.ButtonType));
        }

        public void SetButtonSize(ToolStripButtonSize size)
        {
            Height = (int) size + 6;
            foreach (var b in _MarkButtons)
                b.ButtonSize = size;
        }

        public void SetMarkButtonVisibility(MarkType type, bool value)
        {
            var b = _MarkButtons.FirstOrDefault(m => m.ButtonType == type);
            var v = value ? Visibility.Visible : Visibility.Collapsed;
            if (b != null)
                b.Visibility = v;
        }

        public void SetMarkButtonTooltip(MarkType type, string toolTip)
        {
            var b = _MarkButtons.FirstOrDefault(m => m.ButtonType == type);
            if (b != null)
                b.ToolTip = toolTip;
        }

        public bool IsMarkButtonVisible(MarkType type)
        {
            var b = _MarkButtons.FirstOrDefault(m => m.ButtonType == type);
            if (b != null)
                return b.Visibility == Visibility.Visible;
            return false;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            _MarkButtons.Add(ScheduleButton);
            _MarkButtons.Add(ChangeButton);
            _MarkButtons.Add(ProtectedButton);
            _MarkButtons.Add(PriorityButton);
            _MarkButtons.Add(CompleteButton);
            _MarkButtons.Add(PasswordButton);
            _MarkButtons.Add(PinButton);
            _MarkButtons.Add(MailButton);
            _MarkButtons.Add(EncryptedButton);

            foreach (var b in _MarkButtons)
                b.Visibility = Visibility.Collapsed;
        }
    }

    internal class FormatButtonClickedEventArgs : EventArgs
    {
        internal FormatType Buttontype { get; private set; }

        internal FormatButtonClickedEventArgs(FormatType type)
        {
            Buttontype = type;
        }
    }

    internal class MarkButtonClickedEventArgs : EventArgs
    {
        internal MarkType Buttontype { get; private set; }

        internal MarkButtonClickedEventArgs(MarkType type)
        {
            Buttontype = type;
        }
    }
}
