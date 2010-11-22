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
using CtrlCmdCLI;
using CtrlCmdCLI.Def;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;

namespace EpgTimer
{
    /// <summary>
    /// TunerReserveView.xaml の相互作用ロジック
    /// </summary>
    public partial class TunerReserveView : UserControl
    {
        private Dictionary<UInt32, ReserveItem> reserveItemList = new Dictionary<uint, ReserveItem>();
        private ObservableNotifiableCollection<TunerReserveViewItem> tunerList = new ObservableNotifiableCollection<TunerReserveViewItem>();
        private List<TunerNameViewItem> tunerInfoList = new List<TunerNameViewItem>();
        private System.Collections.SortedList timeList = new System.Collections.SortedList();
        private Dictionary<double, List<TunerReserveViewItem>> viewItemPos = new Dictionary<double, List<TunerReserveViewItem>>();


        private Point lastDownMousePos;
        private double lastDownHOffset;
        private double lastDownVOffset;
        private DispatcherTimer toolTipTimer;
        private DispatcherTimer toolTipOffTimer;
        private Popup toolTip = new Popup();
        private Point lastPopupPos;
        private TunerReserveViewItem lastPopupInfo;
        private bool isDrag = false;

        private CtrlCmdUtil cmd = EpgTimerDef.Instance.CtrlCmd;

        public TunerReserveView()
        {
            InitializeComponent();

            scrollViewer.PreviewMouseWheel += new MouseWheelEventHandler(scrollViewer_PreviewMouseWheel);

            toolTipTimer = new DispatcherTimer(DispatcherPriority.Normal);
            toolTipTimer.Tick += new EventHandler(toolTipTimer_Tick);
            toolTipTimer.Interval = TimeSpan.FromMilliseconds(500);
            toolTipOffTimer = new DispatcherTimer(DispatcherPriority.Normal);
            toolTipOffTimer.Tick += new EventHandler(toolTipOffTimer_Tick);
            toolTipOffTimer.Interval = TimeSpan.FromSeconds(15);

            toolTip.Placement = PlacementMode.MousePoint;
            toolTip.PopupAnimation = PopupAnimation.Fade;
            toolTip.PlacementTarget = tunerReservePanel;
            toolTip.AllowsTransparency = true;
            toolTip.MouseLeftButtonDown += new MouseButtonEventHandler(toolTip_MouseLeftButtonDown);
            toolTip.PreviewMouseWheel += new MouseWheelEventHandler(toolTip_PreviewMouseWheel);
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

                ReserveInfoDoubleClick(lastPopupInfo.Info);
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
            if (tunerReservePanel.ItemsSource != null)
            {
                if (MainWindow.GetWindow(this).IsActive == false)
                {
                    return;
                }
                Point cursorPos2 = Mouse.GetPosition(scrollViewer);
                if (cursorPos2.X < 0 || cursorPos2.Y < 0 ||
                    scrollViewer.ViewportWidth < cursorPos2.X || scrollViewer.ViewportHeight < cursorPos2.Y)
                {
                    return;
                }
                Point cursorPos = Mouse.GetPosition(tunerReservePanel);
                foreach (TunerReserveViewItem info in tunerReservePanel.ItemsSource)
                {
                    if (info.LeftPos <= cursorPos.X && cursorPos.X < info.LeftPos + info.Width)
                    {
                        if (info.TopPos <= cursorPos.Y && cursorPos.Y < info.TopPos + info.Height)
                        {
                            TextBlock block = info.Info.ToolTipView;
                            if (block != null)
                            {
                                block.Background = Brushes.LightBlue;
                                block.Foreground = Brushes.Blue;
                                toolTip.Child = block;
                                toolTip.IsOpen = true;
                                toolTipOffTimer.Start();

                                lastPopupInfo = info;
                                lastPopupPos = cursorPos;
                            }
                        }
                    }
                }
            }
        }

        void scrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            toolTipTimer.Stop();
            toolTipOffTimer.Stop();
            toolTip.IsOpen = false; 
            
            e.Handled = true;
            if (sender.GetType() == typeof(ScrollViewer))
            {
                if (e.Delta < 0)
                {
                    //下方向
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + Settings.Instance.ScrollSize);
                }
                else
                {
                    //上方向
                    if (scrollViewer.VerticalOffset < Settings.Instance.ScrollSize)
                    {
                        scrollViewer.ScrollToVerticalOffset(0);
                    }
                    else
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - Settings.Instance.ScrollSize);
                    }
                }
            }
        }

        public void SetReserveInfo(List<TunerReserveInfo> tunerInfo, List<ReserveItem> reserveList)
        {
            reserveItemList.Clear();
            tunerList.Clear();
            tunerInfoList.Clear();
            timeList.Clear();
            viewItemPos.Clear();
            tunerReservePanel.ItemsSource = null;

            foreach (ReserveItem info in reserveList)
            {
                reserveItemList.Add(info.ReserveInfo.ReserveID, info);
                if (info.ReserveInfo.RecSetting.RecMode != 5)
                {
                    DateTime newStartTime = new DateTime(info.ReserveInfo.StartTime.Year, info.ReserveInfo.StartTime.Month, info.ReserveInfo.StartTime.Day, info.ReserveInfo.StartTime.Hour, 0, 0);
                    DateTime newEndTime = info.ReserveInfo.StartTime.AddSeconds(info.ReserveInfo.DurationSecond);
                    try
                    {
                        while (newStartTime <= newEndTime)
                        {
                            if (timeList.ContainsKey(newStartTime) == false)
                            {
                                timeList.Add(newStartTime, new TimePosInfo(newStartTime, 0));
                            }
                            newStartTime = newStartTime.AddHours(1);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            double timePos = 0;
            foreach (TimePosInfo info in timeList.Values)
            {
                info.TopPos = timePos;
                timePos += 120;
            }

            double nameLeft = 0;

            foreach (TunerReserveInfo info in tunerInfo)
            {
                double nameWidthCount = 1;
                foreach (UInt32 id in info.reserveList)
                {
                    if (reserveItemList.ContainsKey(id) == true)
                    {
                        TunerReserveViewItem item = new TunerReserveViewItem(reserveItemList[id], true);

                        DateTime posTimeKey = new DateTime(item.Info.ReserveInfo.StartTime.Year, item.Info.ReserveInfo.StartTime.Month, item.Info.ReserveInfo.StartTime.Day, item.Info.ReserveInfo.StartTime.Hour, 0, 0);
                        TimePosInfo timeInfo = timeList[posTimeKey] as TimePosInfo;
                        item.LeftPos = nameLeft;
                        item.TopPos = timeInfo.TopPos + (item.Info.ReserveInfo.StartTime - timeInfo.Time).TotalMinutes * 2;
                        item.Height = (item.Info.ReserveInfo.DurationSecond * 2) / 60;
                        item.Width = 150;

                        double count = 1;
                        while (IsViewLeftPosNG(item) == true)
                        {
                            item.LeftPos += 150;
                            count++;
                            if (nameWidthCount < count)
                            {
                                nameWidthCount = count;
                            }
                        }

                        tunerList.Add(item);
                        if( viewItemPos.ContainsKey(item.LeftPos) == false ){
                            List<TunerReserveViewItem> posList = new List<TunerReserveViewItem>();
                            viewItemPos.Add(item.LeftPos, posList);
                        }
                        viewItemPos[item.LeftPos].Add(item);
                    }
                }
                TunerNameViewItem tunerItem = new TunerNameViewItem(info, 150 * nameWidthCount);
                tunerInfoList.Add(tunerItem);

                nameLeft += 150 * nameWidthCount;
            }
            tunerReserveTimeView.SetTime(timeList);
            tunerReserveNameView.SetTunerInfo(tunerInfoList);

            tunerReservePanel.Width = nameLeft;
            tunerReservePanel.Height = timeList.Values.Count * 120;

            tunerReservePanel.ItemsSource = tunerList;
        }

        private bool IsViewLeftPosNG(TunerReserveViewItem item)
        {
            if (viewItemPos.ContainsKey(item.LeftPos) == true)
            {
                foreach (TunerReserveViewItem chk in viewItemPos[item.LeftPos])
                {
                    if (chk.LeftPos == item.LeftPos)
                    {
                        if (chk.TopPos <= item.TopPos && item.TopPos < chk.TopPos + chk.Height)
                        {
                            return true;
                        }
                        if (chk.TopPos < item.TopPos + item.Height && item.TopPos + item.Height < chk.TopPos + chk.Height)
                        {
                            return true;
                        }
                        if (item.TopPos <= chk.TopPos && chk.TopPos < item.TopPos + item.Height)
                        {
                            return true;
                        }
                        if (item.TopPos < chk.TopPos + chk.Height && chk.TopPos + chk.Height < item.TopPos + item.Height)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void CheckTime(DateTime newStart, DateTime newEnd, ref DateTime currentStart, ref DateTime currentEnd)
        {
            if (currentStart.Ticks == TimeSpan.Zero.Ticks)
            {
                currentStart = newStart;
                currentStart = new DateTime(currentStart.Year, currentStart.Month, currentStart.Day, currentStart.Hour, 0, 0);
            }
            else
            {
                if (newStart.Ticks != TimeSpan.Zero.Ticks)
                {
                    if (currentStart > newStart)
                    {
                        currentStart = newStart;
                        currentStart = new DateTime(currentStart.Year, currentStart.Month, currentStart.Day, currentStart.Hour, 0, 0);
                    }
                }
            }

            if (currentEnd.Ticks == TimeSpan.Zero.Ticks)
            {
                currentEnd = newEnd;
                if (currentEnd.Minute != 0)
                {
                    currentEnd = new DateTime(currentEnd.Year, currentEnd.Month, currentEnd.Day, currentEnd.Hour, 0, 0).AddHours(1);
                }
            }
            else
            {
                if (newEnd.Ticks != TimeSpan.Zero.Ticks)
                {
                    if (currentEnd < newEnd)
                    {
                        currentEnd = newEnd;
                        if (currentEnd.Minute != 0)
                        {
                            currentEnd = new DateTime(currentEnd.Year, currentEnd.Month, currentEnd.Day, currentEnd.Hour, 0, 0).AddHours(1);
                        }
                    }
                }
            }
        }

        private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender.GetType() == typeof(ScrollViewer))
            {
                scrollViewer.ScrollToHorizontalOffset(Math.Floor(scrollViewer.HorizontalOffset));
                scrollViewer.ScrollToVerticalOffset(Math.Floor(scrollViewer.VerticalOffset)); 
                tunerReserveTimeView.scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset);
                tunerReserveNameView.scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset);
            }

        }

        private void tunerReservePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(TunerReservePanel))
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
                    Point CursorPos = Mouse.GetPosition(tunerReservePanel);
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
                                tunerReservePanel.CaptureMouse();
                                isDrag = true;
                            }

                        }

                        toolTipTimer.Start();
                        lastPopupPos = CursorPos;
                    }
                }
            }
        }

        private void tunerReservePanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            toolTipTimer.Stop();
            toolTipOffTimer.Stop();
            toolTip.IsOpen = false;

            lastDownMousePos = Mouse.GetPosition(null);
            lastDownHOffset = scrollViewer.HorizontalOffset;
            lastDownVOffset = scrollViewer.VerticalOffset;
            tunerReservePanel.CaptureMouse();
            isDrag = true;

            if (e.ClickCount == 2)
            {
                Point cursorPos = Mouse.GetPosition(tunerReservePanel);
                if (tunerReservePanel.ItemsSource != null)
                {
                    foreach (TunerReserveViewItem info in tunerReservePanel.ItemsSource)
                    {
                        if (info.LeftPos <= cursorPos.X && cursorPos.X < info.LeftPos + info.Width)
                        {
                            if (info.TopPos <= cursorPos.Y && cursorPos.Y < info.TopPos + info.Height)
                            {
                                ReserveInfoDoubleClick(info.Info);
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void tunerReservePanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tunerReservePanel.ReleaseMouseCapture();
            isDrag = false;
        }

        private void ReserveInfoDoubleClick(ReserveItem info)
        {
            if (info != null)
            {
                ChangeReserveWindow dlg = new ChangeReserveWindow();
                dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
                dlg.SetReserve(info.ReserveInfo);
                if (info.ReserveInfo.EventID != 0xFFFF)
                {
                    EpgEventInfo eventInfo = new EpgEventInfo();
                    UInt64 pgID = ((UInt64)info.ReserveInfo.OriginalNetworkID) << 48 |
                        ((UInt64)info.ReserveInfo.TransportStreamID) << 32 |
                        ((UInt64)info.ReserveInfo.ServiceID) << 16 |
                        (UInt64)info.ReserveInfo.EventID;
                    if (cmd.SendGetPgInfo(pgID, ref eventInfo) == 1)
                    {
                        dlg.SetEpgEventInfo(eventInfo);
                    }
                }
                if (dlg.ShowDialog() == true)
                {
                    if (dlg.DeleteEnd == false)
                    {
                        List<ReserveData> addList = new List<ReserveData>();
                        addList.Add(dlg.setInfo);
                        cmd.SendChgReserve(addList);
                    }
                    else
                    {
                        List<UInt32> deleteList = new List<uint>();
                        deleteList.Add(info.ReserveInfo.ReserveID);
                        cmd.SendDelReserve(deleteList);
                    }
                }
            }
        }

    }

    public class TimePosInfo
    {
        public TimePosInfo(DateTime time, double pos)
        {
            Time = time;
            TopPos = pos;
        }

        public DateTime Time
        {
            get;
            set;
        }
        public double TopPos
        {
            get;
            set;
        }
    }

    public class TunerReserveViewItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public TunerReserveViewItem()
        {
        }

        public TunerReserveViewItem(ReserveItem item, bool view)
        {
            Info = item;
            IsView = view;
        }

        public bool IsView
        {
            get;
            set;
        }

        public UInt64 ChKey
        {
            get
            {
                UInt64 key = 0;
                if (Info != null)
                {
                    key = ((UInt64)Info.ReserveInfo.OriginalNetworkID) << 32 | ((UInt64)Info.ReserveInfo.TransportStreamID) << 16 | (UInt64)Info.ReserveInfo.ServiceID;
                }
                return key;
            }
        }
        public ReserveItem Info
        {
            get;
            set;
        }
        public String ReserveInfo
        {
            get
            {
                String view = "";
                if (Info != null)
                {
                    view += Info.ReserveInfo.StartTime.Minute.ToString("d02") + " ";
                    view += Info.ServiceName + "\r\n";
                    view += Info.EventName;
                }
                return view;
            }
        }

        public double Width
        {
            get;
            set;
        }

        public double Height
        {
            get;
            set;
        }

        public double LeftPos
        {
            get;
            set;
        }

        public double TopPos
        {
            get;
            set;
        }
    }

}
