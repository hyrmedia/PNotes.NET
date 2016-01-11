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

using System.Windows;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndDeleteContactsGroup.xaml
    /// </summary>
    public partial class WndDeleteContactsGroup
    {
        public WndDeleteContactsGroup()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal DeleteContactsGroupBehavior DeleteBehavior { get; private set; }

        private void DlgDeleteContactsGroup_Loaded(object sender, RoutedEventArgs e)
        {
            PNLang.Instance.ApplyControlLanguage(this);
            optContsToNone.Content = optContsToNone.Content + " '" +
                                     PNLang.Instance.GetCaptionText("no_cont_group", PNStrings.NO_GROUP) + "'";
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            DeleteBehavior = optContsToNone.IsChecked != null && optContsToNone.IsChecked.Value
                ? DeleteContactsGroupBehavior.Move
                : DeleteContactsGroupBehavior.DeleteAll;
            DialogResult = true;
        }
    }
}
