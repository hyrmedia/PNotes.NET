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
using System.IO;
using System.IO.Packaging;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndArchName.xaml
    /// </summary>
    public partial class WndArchName
    {
        public WndArchName()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        public WndArchName(List<string> files)
            : this()
        {
            _Files = files;
        }

        readonly List<string> _Files;

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(PNPaths.Instance.TempDir))
                {
                    Directory.Delete(PNPaths.Instance.TempDir, true);
                }
                Directory.CreateDirectory(PNPaths.Instance.TempDir);
                var zipPath = Path.Combine(PNPaths.Instance.TempDir, txtArchName.Text.Trim() + ".zip");
                using (var package = Package.Open(zipPath, FileMode.OpenOrCreate))
                {
                    foreach (string f in _Files)
                    {
                        var fileName = Path.GetFileName(f);
                        if (fileName == null) continue;
                        var partUriFile = PackUriHelper.CreatePartUri(new Uri(fileName, UriKind.Relative));
                        var packagePartFile = package.CreatePart(partUriFile, MediaTypeNames.Text.RichText, CompressionOption.Normal);
                        if (packagePartFile == null) continue;
                        package.CreateRelationship(partUriFile, TargetMode.Internal, fileName);
                        using (var fileStream = new FileStream(f, FileMode.Open, FileAccess.Read))
                        {
                            PNStatic.CopyStream(fileStream, packagePartFile.GetStream());
                        }
                    }
                }
                var archives = new List<string> { zipPath };
                PNNotesOperations.SendNotesAsAttachments(archives);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgArchName_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    PNLang.Instance.ApplyControlLanguage(this);
                    Title = lblArchName.Text;
                    txtArchName.Focus();
                }
                catch (Exception ex)
                {
                    PNStatic.LogException(ex);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtArchName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                cmdOK.IsEnabled = txtArchName.Text.Trim().Length > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
