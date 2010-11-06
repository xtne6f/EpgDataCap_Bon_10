using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using CtrlCmdCLI.Def;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;


namespace EpgTimer
{
    /// <summary>
    /// EpgProgramView.xaml の相互作用ロジック
    /// </summary>
    public partial class EpgProgramView : UserControl
    {
        public delegate void EventInfoDoubleClickHandler(ProgramViewItem sender, Point cursorPos);

        public event ScrollChangedEventHandler ScrollChanged = null;
        public event EventInfoDoubleClickHandler EventInfoDoubleClick = null;
        private Point lastDownMousePos;
        private double lastDownHOffset;
        private double lastDownVOffset;
        private DispatcherTimer toolTipTimer;
        private DispatcherTimer toolTipOffTimer;
        private Popup toolTip = new Popup();
        private Point lastPopupPos;
        private ProgramViewItem lastPopupInfo;
        private bool isDrag = false;

        public EpgProgramView()
        {
            InitializeComponent();

            toolTipTimer = new DispatcherTimer(DispatcherPriority.Normal);
            toolTipTimer.Tick += new EventHandler(toolTipTimer_Tick);
            toolTipTimer.Interval = TimeSpan.FromMilliseconds(500);
            toolTipOffTimer = new DispatcherTimer(DispatcherPriority.Normal);
            toolTipOffTimer.Tick += new EventHandler(toolTipOffTimer_Tick);
            toolTipOffTimer.Interval = TimeSpan.FromSeconds(15);

            toolTip.Placement = PlacementMode.MousePoint;
            toolTip.PopupAnimation = PopupAnimation.Fade;
            toolTip.PlacementTarget = epgViewPanel;
            toolTip.AllowsTransparency = true;
            toolTip.MouseLeftButtonDown += new MouseButtonEventHandler(toolTip_MouseLeftButtonDown);
            toolTip.PreviewMouseWheel += new MouseWheelEventHandler(toolTip_PreviewMouseWheel);
        }

        public void Clear()
        {
            toolTipTimer.Stop();
            toolTipOffTimer.Stop();
            toolTip.IsOpen = false;
            epgViewPanel.ReleaseMouseCapture();
            isDrag = false;
        }

        void toolTip_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            toolTipTimer.Stop();
            toolTipOffTimer.Stop();
            toolTip.IsOpen = false;

            RaiseEvent(e);
        }

        void toolTip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                toolTipTimer.Stop();
                toolTipOffTimer.Stop();
                toolTip.IsOpen = false;
                if (EventInfoDoubleClick != null)
                {
                    EventInfoDoubleClick(lastPopupInfo, lastPopupPos);
                }
            }
        }

        void toolTipOffTimer_Tick(object sender, EventArgs e)
        {
            toolTipOffTimer.Stop();
            toolTip.IsOpen = false;
        }

        void toolTipTimer_Tick(object sender, EventArgs e)
        {
            toolTipTimer.Stop();
            if (epgViewPanel.ItemsSource != null)
            {
                if (EpgTimerNW.MainWindow.GetWindow(this).IsActive == false)
                {
                    return;
                }
                Point cursorPos2 = Mouse.GetPosition(scrollViewer);
                if (cursorPos2.X < 0 || cursorPos2.Y < 0 ||
                    scrollViewer.ViewportWidth < cursorPos2.X || scrollViewer.ViewportHeight < cursorPos2.Y)
                {
                    return;
                }
                Point cursorPos = Mouse.GetPosition(epgViewPanel);
                foreach (ProgramViewItem info in epgViewPanel.ItemsSource)
                {
                    if (info.LeftPos <= cursorPos.X && cursorPos.X < info.LeftPos + info.Width)
                    {
                        if (info.TopPos <= cursorPos.Y && cursorPos.Y < info.TopPos + info.Height)
                        {
                            String viewTip = "";

                            if (info != null)
                            {
                                if (info.EventInfo.StartTimeFlag == 1)
                                {
                                    viewTip += info.EventInfo.start_time.ToString("yyyy/MM/dd(ddd) HH:mm:ss ～ ");
                                }
                                else
                                {
                                    viewTip += "未定 ～ ";
                                }
                                if (info.EventInfo.DurationFlag == 1)
                                {
                                    DateTime endTime = info.EventInfo.start_time + TimeSpan.FromSeconds(info.EventInfo.durationSec);
                                    viewTip += endTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss") + "\r\n";
                                }
                                else
                                {
                                    viewTip += "未定\r\n";
                                }

                                if (info.EventInfo.ShortInfo != null)
                                {
                                    viewTip += info.EventInfo.ShortInfo.event_name + "\r\n\r\n";
                                    viewTip += info.EventInfo.ShortInfo.text_char + "\r\n\r\n";
                                }
                                if (info.EventInfo.ExtInfo != null)
                                {
                                    viewTip += info.EventInfo.ExtInfo.text_char + "\r\n\r\n";
                                }
                            }

                            TextBlock block = new TextBlock();
                            block.Text = viewTip;
                            block.MaxWidth = 400;
                            block.TextWrapping = TextWrapping.Wrap;

                            block.Background = Brushes.LightBlue;
                            block.Foreground = Brushes.Blue;
                            toolTip.Child = block;
                            if (Settings.Instance.EpgToolTip == true)
                            {
                                toolTip.IsOpen = true;
                                toolTipOffTimer.Start();
                            }

                            lastPopupInfo = info;
                            lastPopupPos = cursorPos;
                        }
                    }
                }
            }
        }

        private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (ScrollChanged != null)
            {
                scrollViewer.ScrollToHorizontalOffset(Math.Floor(scrollViewer.HorizontalOffset));
                scrollViewer.ScrollToVerticalOffset(Math.Floor(scrollViewer.VerticalOffset)); 
                ScrollChanged(this, e);
            }
        }

        private void epgViewPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(EpgViewPanel))
            {
                if (e.LeftButton == MouseButtonState.Pressed && isDrag == true)
                {
                    toolTipTimer.Stop();
                    toolTipOffTimer.Stop();
                    toolTip.IsOpen = false;

                    Point CursorPos = Mouse.GetPosition(null); 
                    double MoveX = lastDownMousePos.X - CursorPos.X;
                    double MoveY = lastDownMousePos.Y - CursorPos.Y;

                    double OffsetH = 0;
                    double OffsetV = 0;
                    MoveX *= Settings.Instance.DragScroll;
                    MoveY *= Settings.Instance.DragScroll;
                    OffsetH = lastDownHOffset + MoveX;
                    OffsetV = lastDownVOffset + MoveY;
                    if (OffsetH < 0)
                    {
                        OffsetH = 0;
                    }
                    if (OffsetV < 0)
                    {
                        OffsetV = 0;
                    }

                    scrollViewer.ScrollToHorizontalOffset(Math.Floor(OffsetH));
                    scrollViewer.ScrollToVerticalOffset(Math.Floor(OffsetV));
                }
                else
                {
                    Point CursorPos = Mouse.GetPosition(epgViewPanel);
                    if (lastPopupPos != CursorPos)
                    {
                        toolTipTimer.Stop();
                        toolTipOffTimer.Stop();
                        if (toolTip.IsOpen == true)
                        {
                            toolTip.IsOpen = false;
                            lastDownMousePos = Mouse.GetPosition(null);
                            lastDownHOffset = scrollViewer.HorizontalOffset;
                            lastDownVOffset = scrollViewer.VerticalOffset;
                            if (e.LeftButton == MouseButtonState.Pressed)
                            {
                                epgViewPanel.CaptureMouse();
                                isDrag = true;
                            }

                        }

                        toolTipTimer.Start();
                        lastPopupPos = CursorPos;
                    }
                }
            }
        }

        private void epgViewPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            toolTipTimer.Stop();
            toolTipOffTimer.Stop();
            toolTip.IsOpen = false;

            lastDownMousePos = Mouse.GetPosition(null);
            lastDownHOffset = scrollViewer.HorizontalOffset;
            lastDownVOffset = scrollViewer.VerticalOffset;
            epgViewPanel.CaptureMouse();
            isDrag = true;

            if (e.ClickCount == 2)
            {
                bool findPG = false;
                Point cursorPos = Mouse.GetPosition(epgViewPanel);
                if (epgViewPanel.ItemsSource != null)
                {
                    foreach (ProgramViewItem info in epgViewPanel.ItemsSource)
                    {
                        if (info.LeftPos <= cursorPos.X && cursorPos.X < info.LeftPos + info.Width)
                        {
                            if (info.TopPos <= cursorPos.Y && cursorPos.Y < info.TopPos + info.Height)
                            {
                                if (EventInfoDoubleClick != null)
                                {
                                    EventInfoDoubleClick(info, cursorPos);
                                    findPG = true;
                                }
                            }
                        }
                    }
                }
                if (findPG == false)
                {
                    EventInfoDoubleClick(null, cursorPos);
                }
            }
        }

        private void epgViewPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            epgViewPanel.ReleaseMouseCapture();
            isDrag = false;
        }
    }

}
