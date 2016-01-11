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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndPanel.xaml
    /// </summary>
    public partial class WndPanel
    {
        public WndPanel()
        {
            InitializeComponent();
            ThumbnailsPanel.ItemsSource = Thumbnails;
        }

        private const double SIZE_HORZ = 96.0;
        private const double SIZE_VERT = 80.0;

        public ThumbnailsCollection Thumbnails = new ThumbnailsCollection();

        private bool _Hidden = true;
        private StackPanel _Stack;
        private bool _PreventMove;
        private double _GrowSizeCoefficient;

        private readonly Timer _Timer = new Timer(10);
        private readonly Timer _DelayTimer = new Timer();

        internal void RemoveAllThumbnails()
        {
            try
            {
                for (var i = Thumbnails.Count - 1; i >= 0; i--)
                {
                    removeThumbnail(Thumbnails[i]);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void UpdateAutoHideImageBinding()
        {
            try
            {
                var tr = imgAutoHide.RenderTransform as RotateTransform;
                if (tr == null) return;
                var bindingExpression = BindingOperations.GetBindingExpression(tr, RotateTransform.AngleProperty);
                if (bindingExpression != null)
                    bindingExpression.UpdateTarget();
                bindingExpression = BindingOperations.GetBindingExpression(imgAutoHide, ToolTipProperty);
                if (bindingExpression != null)
                    bindingExpression.UpdateTarget();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void UpdateOrientationImageBinding()
        {
            try
            {
                var tr = imgOrientation.RenderTransform as RotateTransform;
                if (tr == null) return;
                var bindingExpression = BindingOperations.GetBindingExpression(tr, RotateTransform.AngleProperty);
                if (bindingExpression != null)
                    bindingExpression.UpdateTarget();
                bindingExpression = BindingOperations.GetBindingExpression(imgOrientation, ToolTipProperty);
                if (bindingExpression != null)
                    bindingExpression.UpdateTarget();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void SetPanelPlacement()
        {
            try
            {
                switch (PNStatic.Settings.Behavior.NotesPanelOrientation)
                {
                    case NotesPanelOrientation.Left:
                        PanelScroll.Orientation = Orientation.Vertical;
                        _Stack.Orientation = Orientation.Vertical;
                        //SizeToContent = SizeToContent.Width;
                        Width = SIZE_VERT * _GrowSizeCoefficient;

                        Height = SystemParameters.WorkArea.Height / 3 * 2;
                        Top = SystemParameters.WorkArea.Height / 6;//(SystemParameters.WorkArea.Height - Height) / 2;
                        Left = PNStatic.Settings.Behavior.PanelAutoHide ? -SIZE_VERT + 2 : 0;

                        BorderLeft.Visibility = Visibility.Visible;
                        BorderTop.Visibility = Visibility.Hidden;

                        GridButtons.HorizontalAlignment = HorizontalAlignment.Left;
                        GridButtons.Margin = new Thickness(25, 0, 0, 0);
                        Grid.SetColumn(imgOrientation, 1);
                        Grid.SetRow(imgOrientation, 0);

                        foreach (var th in Thumbnails)
                        {
                            th.VerticalAlignment = VerticalAlignment.Top;
                            th.HorizontalAlignment = HorizontalAlignment.Left;
                        }
                        break;
                    case NotesPanelOrientation.Top:

                        PanelScroll.Orientation = Orientation.Horizontal;
                        _Stack.Orientation = Orientation.Horizontal;
                        //SizeToContent = SizeToContent.Height;
                        Height = SIZE_HORZ * _GrowSizeCoefficient;
                        Width = SystemParameters.WorkArea.Width / 3 * 2;
                        Left = SystemParameters.WorkArea.Width / 6;//(SystemParameters.WorkArea.Width - Width) / 2;
                        Top = PNStatic.Settings.Behavior.PanelAutoHide ? -SIZE_HORZ + 2 : 0;

                        BorderTop.Visibility = Visibility.Visible;
                        BorderLeft.Visibility = Visibility.Hidden;

                        GridButtons.HorizontalAlignment = HorizontalAlignment.Right;
                        GridButtons.Margin = new Thickness(0, 25, 0, 0);
                        Grid.SetColumn(imgOrientation, 0);
                        Grid.SetRow(imgOrientation, 1);

                        foreach (var th in Thumbnails)
                        {
                            th.VerticalAlignment = VerticalAlignment.Top;
                            th.HorizontalAlignment = HorizontalAlignment.Left;
                        }
                        break;
                }
                _Stack.Visibility = Visibility.Collapsed;
                _Stack.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void SetThumbnailBrush(Brush brush, string id)
        {
            try
            {
                var thb = Thumbnails.FirstOrDefault(t => t.Id == id);
                if (thb == null) return;
                thb.ThumbnailBrush = brush;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal int RemoveThumbnail(PNote note, bool resetProperty = true)
        {
            try
            {
                var index = 0;
                var thb = Thumbnails.FirstOrDefault(t => t.Id == note.ID);
                if (thb != null)
                {
                    index = Thumbnails.IndexOf(thb);
                    thb.Click -= Thumbnail_Click;
                    thb.MouseEnter -= Thumbnail_MouseEnter;
                    thb.MouseLeave -= Thumbnail_MouseLeave;
                    thb.MouseDoubleClick -= Thumbnail_MouseDoubleClick;
                    Thumbnails.Remove(thb);
                }

                if (resetProperty)
                {
                    note.Thumbnail = false;
                    PNNotesOperations.SaveNoteThumbnail(note);
                }

                if (note.Dialog == null)
                    return index; 
                note.PlaceOnScreen();
                return index;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return 0;
            }
        }

        private void removeThumbnail(ThumbnailButton thb)
        {
            try
            {
                var note = PNStatic.Notes.FirstOrDefault(n => n.ID == thb.Id);
                thb.Click -= Thumbnail_Click;
                thb.MouseEnter -= Thumbnail_MouseEnter;
                thb.MouseLeave -= Thumbnail_MouseLeave;
                thb.MouseDoubleClick -= Thumbnail_MouseDoubleClick;
                Thumbnails.Remove(thb);
                if (note == null) return;
                note.Thumbnail = false;
                PNNotesOperations.SaveNoteThumbnail(note);
                if (note.Dialog == null) return;
                note.PlaceOnScreen();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                Thumbnails.CollectionChanged += Thumbnails_CollectionChanged;
                var coeff = TryFindResource("ThumbFullGrow");
                _GrowSizeCoefficient = coeff != null ? Convert.ToDouble(coeff) : 2.0;
                SetPanelPlacement();
                _Timer.Elapsed += _Timer_Elapsed;
                _DelayTimer.Elapsed += _DelayTimer_Elapsed;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        
        private delegate void TimerElapsedDelegate(object sender, ElapsedEventArgs e);
        private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    TimerElapsedDelegate d = _Timer_Elapsed;
                    Dispatcher.Invoke(d, sender, e);
                }
                else
                {
                    _Timer.Stop();
                    if (!hidePanel())
                        _Timer.Start();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void _DelayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    TimerElapsedDelegate d = _DelayTimer_Elapsed;
                    Dispatcher.Invoke(d, sender, e);
                }
                else
                {
                    _DelayTimer.Stop();
                    var pt = PointToScreen(Mouse.GetPosition(this));
                    if (new Rect(Left, Top, ActualWidth, ActualHeight).Contains(pt))
                        mouseEntered();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Thumbnails_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        //switch (PNStatic.Settings.Behavior.NotesPanelOrientation)
                        //{
                        //    case NotesPanelOrientation.Left:
                        //        foreach (var th in e.NewItems.OfType<ThumbnailButton>())
                        //        {
                        //            th.Click += Thumbnail_Click;
                        //            th.MouseEnter += Thumbnail_MouseEnter;
                        //            th.MouseLeave += Thumbnail_MouseLeave;
                        //            th.MouseDoubleClick += Thumbnail_MouseDoubleClick;
                        //            th.VerticalAlignment = VerticalAlignment.Top;
                        //            th.HorizontalAlignment = HorizontalAlignment.Left;
                        //        }
                        //        break;
                        //    case NotesPanelOrientation.Top:
                        //        foreach (var th in e.NewItems.OfType<ThumbnailButton>())
                        //        {
                        //            th.Click += Thumbnail_Click;
                        //            th.MouseEnter += Thumbnail_MouseEnter;
                        //            th.MouseLeave += Thumbnail_MouseLeave;
                        //            th.MouseDoubleClick += Thumbnail_MouseDoubleClick;
                        //            th.VerticalAlignment = VerticalAlignment.Top;
                        //            th.HorizontalAlignment = HorizontalAlignment.Left;
                        //        }
                        //        break;
                        //}
                        foreach (var th in e.NewItems.OfType<ThumbnailButton>())
                        {
                            th.Click += Thumbnail_Click;
                            th.MouseEnter += Thumbnail_MouseEnter;
                            th.MouseLeave += Thumbnail_MouseLeave;
                            th.MouseDoubleClick += Thumbnail_MouseDoubleClick;
                            th.VerticalAlignment = VerticalAlignment.Top;
                            th.HorizontalAlignment = HorizontalAlignment.Left;
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (Thumbnails.Count == 0)
                        {
                            switch (PNStatic.Settings.Behavior.NotesPanelOrientation)
                            {
                                case NotesPanelOrientation.Left:
                                    Width = SIZE_VERT;
                                    break;
                                case NotesPanelOrientation.Top:
                                    Height = SIZE_HORZ;
                                    break;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        void Thumbnail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (PNStatic.Settings.Behavior.PanelRemoveMode != PanelRemoveMode.DoubleClick) return;
                var thb = sender as ThumbnailButton;
                if (thb == null) return;
                removeThumbnail(thb);
                allButtonsToNormalSize();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Thumbnail_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (PNStatic.Settings.Behavior.PanelSwitchOffAnimation) return;
                var button = sender as ThumbnailButton;
                if (button == null) return;
                var stb = TryFindResource("NormalSizeGrow") as Storyboard;
                if (stb == null) return;
                if (button.Prev != null)
                {
                    button.Prev.BeginStoryboard(stb);
                }
                if (button.Next != null)
                {
                    button.Next.BeginStoryboard(stb);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Thumbnail_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (PNStatic.Settings.Behavior.PanelSwitchOffAnimation) return;
                var button = sender as ThumbnailButton;
                if (button == null) return;
                var stb = TryFindResource("HalfsSizeGrow") as Storyboard;
                if (stb == null) return;
                if (button.Prev != null)
                {
                    button.Prev.BeginStoryboard(stb);
                }
                if (button.Next != null)
                {
                    button.Next.BeginStoryboard(stb);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Thumbnail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNStatic.Settings.Behavior.PanelRemoveMode != PanelRemoveMode.SingleClick) return;
                var thb = sender as ThumbnailButton;
                if (thb == null) return;
                removeThumbnail(thb);
                allButtonsToNormalSize();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (_PreventMove) return;
                if (!PNStatic.Settings.Behavior.PanelAutoHide) return;
                if (!_Hidden) return;
                if (PNStatic.Settings.Behavior.PanelEnterDelay == 0)
                {
                    mouseEntered();
                }
                else
                {
                    _DelayTimer.Interval = PNStatic.Settings.Behavior.PanelEnterDelay;
                    _DelayTimer.Start();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mouseEntered()
        {
            try
            {
                var dba = new DoubleAnimation { Duration = new Duration(TimeSpan.FromMilliseconds(300)) };
                dba.Completed += Animation_Completed;
                switch (PNStatic.Settings.Behavior.NotesPanelOrientation)
                {
                    case NotesPanelOrientation.Left:
                        dba.To = 0;
                        BeginAnimation(LeftProperty, dba);
                        break;
                    case NotesPanelOrientation.Top:
                        dba.To = 0;
                        BeginAnimation(TopProperty, dba);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Animation_Completed(object sender, EventArgs e)
        {
            try
            {
                switch (PNStatic.Settings.Behavior.NotesPanelOrientation)
                {
                    case NotesPanelOrientation.Left:
                        BeginAnimation(LeftProperty, null);
                        break;
                    case NotesPanelOrientation.Top:
                        BeginAnimation(TopProperty, null);
                        break;
                }
                _Hidden = !_Hidden;
                if (!_Hidden)
                    _Timer.Start();
                else
                {
                    _Timer.Stop();
                    allButtonsToNormalSize();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            //try
            //{
            //    if (_PreventMove) return;
            //    if (!PNStatic.Settings.Behavior.PanelAutoHide) return;
            //    if (_Hidden) return;
            //    var dba = new DoubleAnimation { Duration = new Duration(TimeSpan.FromMilliseconds(300)) };
            //    dba.Completed += Animation_Completed;
            //    switch (PNStatic.Settings.Behavior.NotesPanelOrientation)
            //    {
            //        case NotesPanelOrientation.Left:
            //            dba.To = -SIZE_VERT + 2;
            //            BeginAnimation(LeftProperty, dba);
            //            break;
            //        case NotesPanelOrientation.Top:
            //            dba.To = -SIZE_HORZ + 2;
            //            BeginAnimation(TopProperty, dba);
            //            break;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    PNStatic.LogException(ex);
            //}
        }

        private bool hidePanel()
        {
            try
            {
                if (_PreventMove) return false;
                if (!PNStatic.Settings.Behavior.PanelAutoHide) return false;
                if (_Hidden) return false;
                var pt = PointToScreen(Mouse.GetPosition(this));
                if (new Rect(Left, Top, ActualWidth, ActualHeight).Contains(pt)) return false;

                var dba = new DoubleAnimation { Duration = new Duration(TimeSpan.FromMilliseconds(300)) };
                dba.Completed += Animation_Completed;
                switch (PNStatic.Settings.Behavior.NotesPanelOrientation)
                {
                    case NotesPanelOrientation.Left:
                        dba.To = -SIZE_VERT + 2;
                        BeginAnimation(LeftProperty, dba);
                        break;
                    case NotesPanelOrientation.Top:
                        dba.To = -SIZE_HORZ + 2;
                        BeginAnimation(TopProperty, dba);
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void allButtonsToNormalSize()
        {
            try
            {
                var stb = TryFindResource("NormalSizeGrow") as Storyboard;
                if (stb == null) return;
                foreach (var b in Thumbnails)
                    b.BeginStoryboard(stb);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void StackPanel_Initialized(object sender, EventArgs e)
        {
            _Stack = sender as StackPanel;
        }

        private void Pin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                PNStatic.Settings.Behavior.PanelAutoHide = !PNStatic.Settings.Behavior.PanelAutoHide;
                PNData.SaveBehaviorSettings();
                UpdateAutoHideImageBinding();
                if (PNStatic.FormSettings != null)
                {
                    PNStatic.FormSettings.PanelAutohideChanged();
                }
                if (PNStatic.Settings.Behavior.PanelAutoHide)
                {
                    _PreventMove = true;
                    switch (PNStatic.Settings.Behavior.NotesPanelOrientation)
                    {
                        case NotesPanelOrientation.Left:
                            Left = -SIZE_VERT + 2;
                            break;
                        case NotesPanelOrientation.Top:
                            Top = -SIZE_HORZ + 2;
                            break;
                    }
                    _PreventMove = false;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Orient_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                PNStatic.Settings.Behavior.NotesPanelOrientation = PNStatic.Settings.Behavior.NotesPanelOrientation ==
                                                                   NotesPanelOrientation.Left
                    ? NotesPanelOrientation.Top
                    : NotesPanelOrientation.Left;
                PNData.SaveBehaviorSettings();
                SetPanelPlacement();
                UpdateOrientationImageBinding();
                if (PNStatic.FormSettings == null) return;
                PNStatic.FormSettings.PanelOrientationChanged();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }

    public class ThumbnailsCollection : ObservableCollection<ThumbnailButton>
    {
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        if (Items.Count == 1) break;
                        var buttons = Items.ToArray();
                        if (e.NewStartingIndex == buttons.Length - 1)
                        {
                            buttons[buttons.Length - 2].Next = buttons[buttons.Length - 1];
                            buttons[buttons.Length - 1].Prev = buttons[buttons.Length - 2];
                        }
                        else
                        {
                            if (e.NewStartingIndex == 0)
                            {
                                buttons[1].Prev = buttons[0];
                                buttons[0].Next = buttons[1];
                            }
                            else
                            {
                                buttons[e.NewStartingIndex - 1].Next = buttons[e.NewStartingIndex];
                                buttons[e.NewStartingIndex].Prev = buttons[e.NewStartingIndex - 1];
                                buttons[e.NewStartingIndex].Next = buttons[e.NewStartingIndex + 1];
                                buttons[e.NewStartingIndex + 1].Prev = buttons[e.NewStartingIndex];
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        if (Items.Count == 0) break;
                        var buttons = Items.ToArray();
                        if (e.OldStartingIndex == 0)
                        {
                            buttons[0].Prev = null;
                        }
                        else if (e.OldStartingIndex == buttons.Length)
                        {
                            buttons[buttons.Length - 1].Next = null;
                        }
                        else
                        {
                            buttons[e.OldStartingIndex - 1].Next = buttons[e.OldStartingIndex];
                            buttons[e.OldStartingIndex].Prev = buttons[e.OldStartingIndex - 1];
                        }
                    }
                    break;
            }
            base.OnCollectionChanged(e);
        }
    }
}
