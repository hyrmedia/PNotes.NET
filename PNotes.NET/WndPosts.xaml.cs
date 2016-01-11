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

using PluginsCore;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndPosts.xaml
    /// </summary>
    public partial class WndPosts
    {
        internal event EventHandler<PostSelectedEventArgs> PostSelected;

        public WndPosts()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private readonly string _Plugin;
        private readonly string _NoteName;
        private readonly IEnumerable<PostDetails> _Posts;

 
        internal WndPosts(IEnumerable<PostDetails> posts, string plugin, string noteName)
            : this()
        {
            _Plugin = plugin;
            _NoteName = noteName;
            _Posts = posts;
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = grdPosts.SelectedItem as PostDetails;
                if (item == null) return;
                if (PostSelected != null)
                {
                    PostSelected(this, new PostSelectedEventArgs(item.PostText));
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgPosts_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                Title = Title.Replace(PNStrings.PLACEHOLDER1, @"[" + _Plugin + @"]").Replace(PNStrings.PLACEHOLDER2, _NoteName);
                grdPosts.ItemsSource = _Posts;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdPosts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cmdOK.IsEnabled = grdPosts.SelectedItems.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdPosts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton != MouseButton.Left) return;
                var item = grdPosts.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdPosts)) as PostDetails;
                if (item == null) return;
                cmdOK.PerformClick();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
