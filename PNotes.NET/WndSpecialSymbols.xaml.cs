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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSpecialSymbols.xaml
    /// </summary>
    public partial class WndSpecialSymbols
    {
        public WndSpecialSymbols()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal event EventHandler<SpecialSymbolSelectedEventArgs> SpecialSymbolSelected;

        private const string MATH_SYMBOLS =
            "∀∁∂∃∄∅∆∇∈∉∊∋∌∍∎∏∐∑−±∓∔÷∕∖∗∘∙√∛∜∝∞∟∠∡∢∣∤∥∦∧∨∩∪∫∬∭∮∯∰∱∲∳∴∵∶∷∸∹∺∻∼∽∾∿≀≁≂≃≄≅≆≇≈≉≊≋≌≍≎≏≐≑≒≓≔≕≖≗≘≙≚≛≜≝≞≟≠≡≢≣≤≥≦≧≨≩≪≫≬≭≮≯≰≱≲≳≴≵≶≷≸≹≺≻≼≽≾≿⊀⊁⊂⊃⊄⊅⊆⊇⊈⊉⊊⊋⊌⊍⊎⊏⊐⊑⊒⊓⊔⊕⊖⊗⊘⊙⊚⊛⊜⊝⊞⊟⊠⊡⊢⊣⊤⊥⊦⊧⊨⊩⊪⊫⊬⊭⊮⊯⊰⊱⊲⊳⊴⊵⊶⊷⊸⊹⊺⊻⊼⊽⊾⊿⋀⋁⋂⋃⋄⋅⋆⋇⋈⋉⋊⋋⋌⋍⋎⋏⋐⋑⋒⋓⋔⋕⋖⋗⋘⋙⋚⋛⋜⋝⋞⋟⋠⋡⋢⋣⋤⋥⋦⋧⋨⋩⋪⋫⋬⋭⋮⋯⋰⋱";

        private const string ARROW_SYMBOLS =
            "←↑→↓↔↕↖↗↘↙↚↛↜↝↞↟↠↡↢↣↤↥↦↧↨↩↪↫↬↭↮↯↰↱↲↳↴↵↶↷↸↹↺↻↼↽↾↿⇀⇁⇂⇃⇄⇅⇆⇇⇈⇉⇊⇋⇌⇍⇎⇏⇐⇑⇒⇓⇔⇕⇖⇗⇘⇙⇚⇛⇜⇝⇞⇟⇠⇡⇢⇣⇤⇥⇦⇧⇨⇩⇪";

        private const string GEOMETRIC_SHAPES =
            "■□▢▣▤▥▦▧▨▩▪▫▬▭▮▯▰▱▲△▴▵▶▷▸▹►▻▼▽▾▿◀◁◂◃◄◅◆◇◈◉◊○◌◍◎●◐◑◒◓◔◕◖◗◘◙◚◛◜◝◞◟◠◡◢◣◤◥◦◧◨◩◪◫◬◭◮";

        private const string MISC_SYMBOLS = "€£¥₪©®℗™℠§Ω℧µ℮℅℆‰‱℀℁℃℉⁂※₠₡№☺☻☼♀♂♠♣♥♦♪♫♭♮♯";

        private FontFamily _FontFamily;
        private void DlgSpecialSymbols_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;
                fillCombo();
                PNLang.Instance.ApplyControlLanguage(this);
                var fontLuc =
                    PNStatic.PrivateFonts.Families.FirstOrDefault(
                        f => f.Name.ToUpper().StartsWith("LUCIDA SANS UNICODE"));
                _FontFamily = fontLuc != null ? new FontFamily(fontLuc.Name) : new FontFamily("Lucida Sans Unicode");
                createButtons();
                cboSymbols.SelectedIndex = PNStatic.SymbolsIndex;
                cboSymbols.Focus();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void fillCombo()
        {
            try
            {
                cboSymbols.Items.Add("Miscellaneous");
                cboSymbols.Items.Add("Arrows");
                cboSymbols.Items.Add("Mathematical operators");
                cboSymbols.Items.Add("Geometric shapes");
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createButtons()
        {
            try
            {

                foreach (var block in MISC_SYMBOLS.Select(t => new TextBlock
                {
                    Text = new string(t, 1),
                    FontFamily = _FontFamily,
                    FontStyle = FontStyles.Normal,
                    FontSize=20,
                    Margin = new Thickness(1),
                    MinHeight = 24,
                    MinWidth = 24,
                    Cursor = Cursors.Hand
                }))
                {
                    block.MouseLeftButtonDown += block_Click;
                    block.MouseEnter += block_MouseEnter;
                    block.MouseLeave += block_MouseLeave;
                    pnlMisc.Children.Add(block);
                }
                foreach (var block in ARROW_SYMBOLS.Select(t => new TextBlock
                {
                    Text = new string(t, 1),
                    FontFamily = _FontFamily,
                    FontStyle = FontStyles.Normal,
                    FontSize = 20,
                    Margin = new Thickness(1),
                    MinHeight = 24,
                    MinWidth = 24,
                    Cursor = Cursors.Hand
                }))
                {
                    block.MouseLeftButtonDown += block_Click;
                    block.MouseEnter += block_MouseEnter;
                    block.MouseLeave += block_MouseLeave;
                    pnlArrows.Children.Add(block);
                }
                foreach (var block in MATH_SYMBOLS.Select(t => new TextBlock
                {
                    Text = new string(t, 1),
                    FontFamily = _FontFamily,
                    FontStyle = FontStyles.Normal,
                    FontSize = 20,
                    Margin = new Thickness(1),
                    MinHeight = 24,
                    MinWidth = 24,
                    Cursor = Cursors.Hand
                }))
                {
                    block.MouseLeftButtonDown += block_Click;
                    block.MouseEnter += block_MouseEnter;
                    block.MouseLeave += block_MouseLeave;
                    pnlMath.Children.Add(block);
                }
                foreach (var block in GEOMETRIC_SHAPES.Select(t => new TextBlock
                {
                    Text = new string(t, 1),
                    FontFamily = _FontFamily,
                    FontStyle = FontStyles.Normal,
                    FontSize = 20,
                    Margin = new Thickness(1),
                    MinHeight = 24,
                    MinWidth = 24,
                    Cursor = Cursors.Hand
                }))
                {
                    block.MouseLeftButtonDown += block_Click;
                    block.MouseEnter += block_MouseEnter;
                    block.MouseLeave += block_MouseLeave;
                    pnlGeometric.Children.Add(block);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        void block_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                var t = sender as TextBlock;
                if (t == null) return;
                t.FontWeight = FontWeights.Normal;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        void block_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                var t = sender as TextBlock;
                if (t == null) return;
                t.FontWeight = FontWeights.ExtraBold;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        void block_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var t = sender as TextBlock;
                if (t == null) return;
                if (SpecialSymbolSelected == null) return;
                SpecialSymbolSelected(this, new SpecialSymbolSelectedEventArgs(t.Text));
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboSymbols_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                pnlMisc.Visibility = pnlArrows.Visibility = pnlMath.Visibility = pnlGeometric.Visibility = Visibility.Collapsed;
                if (cboSymbols.SelectedIndex == -1) return;
                switch (cboSymbols.SelectedIndex)
                {
                    case 0:
                        pnlMisc.Visibility = Visibility.Visible;
                        break;
                    case 1:
                        pnlArrows.Visibility = Visibility.Visible;
                        break;
                    case 2:
                        pnlMath.Visibility = Visibility.Visible;
                        break;
                    case 3:
                        pnlGeometric.Visibility = Visibility.Visible;
                        break;
                }
                PNStatic.SymbolsIndex = cboSymbols.SelectedIndex;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgSpecialSymbols_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
            }
        }
    }
}
