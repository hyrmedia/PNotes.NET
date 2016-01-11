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

using PNRichEdit;
using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndAddTable.xaml
    /// </summary>
    public partial class WndAddTable
    {
        internal event EventHandler<TableReadyEventArgs> TableReady;

        private class _Thickness
        {
            internal string Text { private get; set; }
            internal int Value { get; set; }
            public override string ToString()
            {
                return Text;
            }
        }

        private const string BORDERS =
            @"\clbrdrl\brdrwXXX0\brdrs\brdrcf1\clbrdrt\brdrwXXX0\brdrs\brdrcf1\clbrdrr\brdrwXXX0\brdrs\brdrcf1\clbrdrb\brdrwXXX0\brdrs\brdrcf1 ";

        private const string COLORS = @"{\colortbl ;\redRRR\greenGGG\blueBBB;}";

        private StringBuilder _Rtf;

        public WndAddTable()
        {
            InitializeComponent();
            initializeEdit();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private EditControl _EditControl;
        private PNRichEditBox _Edit;
        private bool _Loaded;

        private void initializeEdit()
        {
            try
            {
                _EditControl = new EditControl(brdHost);
                _Edit = _EditControl.EditBox;
                _Edit.ReadOnly = true;
                var brush = brdHost.Background as SolidColorBrush;
                if (brush == null) return;
                _EditControl.WinForm.BackColor = System.Drawing.Color.FromArgb(255, brush.Color.R, brush.Color.G,
                    brush.Color.B);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void prepareRtf(bool example)
        {
            try
            {
                _Rtf = new StringBuilder(@"{\rtf1\ansi\deff0");

                var rows = (int)updTableRows.Value;
                var cols = (int)updTableColumns.Value;
                var width = Convert.ToInt32(cboTableColumnWidth.SelectedItem);
                var ind = Convert.ToInt32(cboTableTextIndent.SelectedItem);
                var thickness = cboTableBorders.SelectedItem as _Thickness;
                if (thickness != null)
                {
                    var bw = thickness.Value;
                    var clr = System.Drawing.Color.FromArgb(crpTable.SelectedColor.A, crpTable.SelectedColor.R,
                        crpTable.SelectedColor.G, crpTable.SelectedColor.B);

                    _Rtf.Append(COLORS.Replace("RRR", clr.R.ToString(CultureInfo.InvariantCulture))
                        .Replace("GGG", clr.G.ToString(CultureInfo.InvariantCulture))
                        .Replace("BBB", clr.B.ToString(CultureInfo.InvariantCulture)));

                    for (var i = 0; i < rows; i++)
                    {
                        var w = width;
                        _Rtf.Append(@"{\trowd\trgaph");
                        _Rtf.Append(ind);
                        for (var j = 0; j < cols; j++)
                        {
                            _Rtf.Append(BORDERS.Replace("XXX", bw.ToString(CultureInfo.InvariantCulture)));
                            _Rtf.Append(@"\cellx");
                            _Rtf.Append(w);
                            w += width;
                        }
                        for (var j = 0; j < cols; j++)
                        {
                            _Rtf.Append(@"\intbl");
                            if (example)
                            {
                                _Rtf.Append(" text");
                            }
                            _Rtf.Append(@"\cell");
                        }
                        _Rtf.Append(@"\row}");
                    }
                }
                _Rtf.Append("}");
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyTableLook()
        {
            try
            {
                if (!_Loaded) return;
                if (cboTableColumnWidth.SelectedIndex == -1 || cboTableTextIndent.SelectedIndex == -1 || cboTableBorders.SelectedIndex == -1) return;
                prepareRtf(true);
                _Edit.SelectAll();
                _Edit.SelectedRtf = _Rtf.ToString();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgAddTable_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                cboTableColumnWidth.SelectedItem = 2000;
                cboTableTextIndent.SelectedItem = 144;
                cboTableBorders.Items.Add(new _Thickness { Text = "0", Value = 0 });
                cboTableBorders.Items.Add(new _Thickness { Text = "1", Value = 1 });
                cboTableBorders.Items.Add(new _Thickness { Text = "2", Value = 3 });
                cboTableBorders.Items.Add(new _Thickness { Text = "3", Value = 4 });
                cboTableBorders.Items.Add(new _Thickness { Text = "4", Value = 6 });
                cboTableBorders.Items.Add(new _Thickness { Text = "5", Value = 7 });
                cboTableBorders.Items.Add(new _Thickness { Text = "6", Value = 9 });
                cboTableBorders.Items.Add(new _Thickness { Text = "7", Value = 10 });
                cboTableBorders.SelectedIndex = 1;
                _Loaded = true;
                applyTableLook();
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
                if (TableReady != null)
                {
                    prepareRtf(false);
                    TableReady(this, new TableReadyEventArgs(_Rtf.ToString()));
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Upd_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            applyTableLook();
        }

        private void crpTable_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            applyTableLook();
        }

        private void Cbo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            applyTableLook();
        }
    }
}
