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

namespace EpgTimer
{
    /// <summary>
    /// EpgView.xaml の相互作用ロジック
    /// </summary>
    public partial class EpgNetworkView : UserControl
    {
        private Dictionary<UInt64, EventListInfo> eventDataList = new Dictionary<ulong,EventListInfo>();
        private ObservableNotifiableCollection<ProgramViewItem> programViewList = new ObservableNotifiableCollection<ProgramViewItem>();
        private Dictionary<UInt64, EventListInfo> enableList = new Dictionary<ulong, EventListInfo>();
        private DateTime startTime = new DateTime();
        private DateTime endTime = new DateTime();
        private DispatcherTimer nowViewTimer;
        private Line nowLine = null;
        private List<ReserveViewItem> reserveList = new List<ReserveViewItem>();
        private List<Rectangle> reserveBorder = new List<Rectangle>();
        private CtrlCmdUtil cmd = EpgTimerNW.NWConnect.Instance.cmd;

        public EpgNetworkView()
        {
            InitializeComponent();
            epgProgramView.PreviewMouseWheel += new MouseWheelEventHandler(epgProgramView_PreviewMouseWheel);
            epgProgramView.ScrollChanged += new ScrollChangedEventHandler(epgProgramView_ScrollChanged);
            epgProgramView.EventInfoDoubleClick += new EpgProgramView.EventInfoDoubleClickHandler(epgProgramView_EventInfoDoubleClick);
            epgDateView.TimeButtonClick += new RoutedEventHandler(epgDateView_TimeButtonClick);

            nowViewTimer = new DispatcherTimer(DispatcherPriority.Normal);
            nowViewTimer.Tick += new EventHandler(WaitReDrawNowLine);
        }

        public void ClearValue()
        {
            epgProgramView.epgViewPanel.ItemsSource = null;
            programViewList.Clear();
            eventDataList.Clear();
            enableList.Clear();
            startTime = new DateTime();
            endTime = new DateTime();
        }
        
        void epgProgramView_EventInfoDoubleClick(ProgramViewItem sender, Point cursorPos)
        {
            //予約枠内かチェック
            foreach (ReserveViewItem info in reserveList)
            {
                if (info.IsView == true)
                {
                    if (info.LeftPos <= cursorPos.X && cursorPos.X < info.LeftPos + info.Width)
                    {
                        if (info.TopPos <= cursorPos.Y && cursorPos.Y < info.TopPos + info.Height)
                        {
                            ChangeReserveWindow dlg = new ChangeReserveWindow();
                            dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
                            ReserveItem item = info.Info;
                            dlg.SetReserve(item.ReserveInfo);
                            if (sender != null)
                            {
                                dlg.SetEpgEventInfo(sender.EventInfo);
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
                                    deleteList.Add(item.ReserveInfo.ReserveID);
                                    cmd.SendDelReserve(deleteList);
                                }
                            }
                            return;
                        }
                    }
                }
            }
            if (sender != null)
            {
                //通常
                AddReserveWindow addDlg = new AddReserveWindow();
                addDlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
                addDlg.SetEpgEventInfo(sender.EventInfo);
                if (addDlg.ShowDialog() == true)
                {
                }
            }
        }

        public void SetReserveList(List<ReserveItem> reserveList)
        {
            this.reserveList.Clear();
            foreach (ReserveItem info in reserveList)
            {
                this.reserveList.Add(new ReserveViewItem(info, false));
            }
            ReDrawReserve();
        }

        public void ReDrawReserve()
        {
            foreach (Rectangle info in reserveBorder)
            {
                epgProgramView.canvas.Children.Remove(info);
            }
            foreach (ReserveViewItem info in reserveList)
            {
                info.IsView = false;
            }

            for (int i = 0; i < enableList.Count; i++)
            {
                EventListInfo eventInfo = enableList.Values.ElementAt(i);
                foreach (ReserveViewItem info in reserveList)
                {
                    if (eventInfo.ServiceKey == info.ChKey)
                    {
                        Rectangle rect = new Rectangle();
                        if (info.Info.ReserveInfo.EventID != 0xFFFF)
                        {
                            //EPG予約なので幅求めるために検索
                            foreach (ProgramViewItem pgInfo in programViewList)
                            {
                                if (pgInfo.EventInfo.original_network_id == info.Info.ReserveInfo.OriginalNetworkID &&
                                    pgInfo.EventInfo.transport_stream_id == info.Info.ReserveInfo.TransportStreamID &&
                                    pgInfo.EventInfo.service_id == info.Info.ReserveInfo.ServiceID &&
                                    pgInfo.EventInfo.event_id == info.Info.ReserveInfo.EventID)
                                {
                                    info.Width = pgInfo.Width;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            //プログラム予約なのでデフォルト幅固定
                            info.Width = Settings.Instance.ServiceWidth;
                            //開始時間がかぶる予約を探してみる
                            foreach (ProgramViewItem pgInfo in programViewList)
                            {
                                if (pgInfo.EventInfo.original_network_id == info.Info.ReserveInfo.OriginalNetworkID &&
                                    pgInfo.EventInfo.transport_stream_id == info.Info.ReserveInfo.TransportStreamID &&
                                    pgInfo.EventInfo.service_id == info.Info.ReserveInfo.ServiceID)
                                {
                                    DateTime endTime = pgInfo.EventInfo.start_time.AddSeconds(pgInfo.EventInfo.durationSec);
                                    if (pgInfo.EventInfo.start_time <= info.Info.ReserveInfo.StartTime &&
                                        info.Info.ReserveInfo.StartTime < endTime)
                                    {
                                        info.Width = pgInfo.Width;
                                        break;
                                    }
                                }
                            }
                        }
                        info.Height = (info.Info.ReserveInfo.DurationSecond * Settings.Instance.MinHeight) / 60;

                        rect.Opacity = 0.5;
                        string colorName = "Lime";
                        try
                        {
                            if (info.Info.ReserveInfo.RecSetting.RecMode == 5)
                            {
                                colorName = Settings.Instance.ReserveRectColorNo;
                            }
                            else if (info.Info.ReserveInfo.OverlapMode == 2)
                            {
                                colorName = Settings.Instance.ReserveRectColorNoTuner;
                            }
                            else
                            {
                                colorName = Settings.Instance.ReserveRectColorNormal;
                            }
                        }
                        catch
                        {
                        }
                        if (Settings.Instance.ReserveRectBackground == false)
                        {
                            rect.Fill = System.Windows.Media.Brushes.Transparent;
                            rect.StrokeThickness = 3;

                            rect.Stroke = ColorDef.Instance.ColorTable[colorName];
                        }
                        else
                        {
                            rect.Fill = ColorDef.Instance.ColorTable[colorName];
                        }

                        info.LeftPos = Settings.Instance.ServiceWidth * i;
                        info.TopPos = (info.Info.ReserveInfo.StartTime - startTime).TotalMinutes * Settings.Instance.MinHeight;
                        info.IsView = true;

                        rect.Width = info.Width;
                        rect.Height = info.Height;

                        rect.MouseLeftButtonDown += new MouseButtonEventHandler(rect_MouseLeftButtonDown);

                        Canvas.SetLeft(rect, info.LeftPos);
                        Canvas.SetTop(rect, info.TopPos);
                        Canvas.SetZIndex(rect, 10);
                        epgProgramView.canvas.Children.Add(rect);

                        reserveBorder.Add(rect);
                    }
                }
            }

        }

        void rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            epgProgramView.epgViewPanel.RaiseEvent(e);
        }

        private void WaitReDrawNowLine(object sender, EventArgs e)
        {
            ReDrawNowLine();
        }

        private void ReDrawNowLine()
        {
            try
            {
                nowViewTimer.Stop();
                DateTime nowTime = DateTime.Now;
                if (nowTime < startTime)
                {
                    if (nowLine != null)
                    {
                        epgProgramView.canvas.Children.Remove(nowLine);
                    }
                    nowLine = null;
                    return;
                }
                if (nowLine == null)
                {
                    nowLine = new Line();
                    Canvas.SetZIndex(nowLine, 20);
                    nowLine.Stroke = new SolidColorBrush(Colors.Red);
                    nowLine.StrokeThickness = Settings.Instance.MinHeight * 2;
                    nowLine.Opacity = 0.5;
                    epgProgramView.canvas.Children.Add(nowLine);
                }
                TimeSpan hightTime = nowTime - startTime;
                double posY = hightTime.TotalMinutes * Settings.Instance.MinHeight;

                if (posY > epgProgramView.canvas.Height)
                {
                    if (nowLine != null)
                    {
                        epgProgramView.canvas.Children.Remove(nowLine);
                    }
                    nowLine = null;
                    return;
                }

                nowLine.X1 = 0;
                nowLine.Y1 = posY;
                nowLine.X2 = epgProgramView.canvas.Width;
                nowLine.Y2 = posY;

                nowViewTimer.Interval = TimeSpan.FromSeconds(60 - nowTime.Second);
                nowViewTimer.Start();
            }
            catch
            {
            }
        }

        void epgDateView_TimeButtonClick(object sender, RoutedEventArgs e)
        {
            Button timeButton = sender as Button;
            DateTime time = (DateTime)timeButton.DataContext;
            if (time < startTime)
            {
                epgProgramView.scrollViewer.ScrollToVerticalOffset(0);
            }
            else
            {
                TimeSpan timeHeight = time - startTime;
                epgProgramView.scrollViewer.ScrollToVerticalOffset(timeHeight.TotalMinutes * Settings.Instance.MinHeight);
            }
        }

        void epgProgramView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender.GetType() == typeof(EpgProgramView))
            {
                epgTimeView.scrollViewer.ScrollToVerticalOffset(epgProgramView.scrollViewer.VerticalOffset);
                epgServiceView.scrollViewer.ScrollToHorizontalOffset(epgProgramView.scrollViewer.HorizontalOffset);
            }
        }

        void epgProgramView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            if (sender.GetType() == typeof(EpgProgramView))
            {
                EpgProgramView view = sender as EpgProgramView;
                /*if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (e.Delta < 0)
                    {
                        //下方向
                        Settings.Instance.MinHeight += 1;
                        Settings.Instance.ServiceWidth += 10;
                        DrawProgramView();
                        ReDrawReserve();
                    }
                    else
                    {
                        //上方向
                        if (Settings.Instance.MinHeight > 1 && Settings.Instance.ServiceWidth > 10)
                        {
                            Settings.Instance.MinHeight -= 1;
                            Settings.Instance.ServiceWidth -= 10;
                            DrawProgramView();
                            ReDrawReserve();
                        }
                    }
                }
                else
                {*/
                    if (e.Delta < 0)
                    {
                        //下方向
                        view.scrollViewer.ScrollToVerticalOffset(view.scrollViewer.VerticalOffset + Settings.Instance.ScrollSize);
                    }
                    else
                    {
                        //上方向
                        if (view.scrollViewer.VerticalOffset < Settings.Instance.ScrollSize)
                        {
                            view.scrollViewer.ScrollToVerticalOffset(0);
                        }
                        else
                        {
                            view.scrollViewer.ScrollToVerticalOffset(view.scrollViewer.VerticalOffset - Settings.Instance.ScrollSize);
                        }
                    }
                //}
            }
        }

        public void AddProgramList(EpgServiceInfo serviceInfo, List<EpgEventInfo> eventList)
        {
            UInt64 key = ((UInt64)serviceInfo.ONID) << 32 | ((UInt64)serviceInfo.TSID) << 16 | (UInt64)serviceInfo.SID;
            if (eventDataList.ContainsKey(key) == false)
            {
                EventListInfo item = new EventListInfo(serviceInfo, eventList);
                eventDataList.Add(key, item);
            }
        }

        public DateTime StartTime
        {
            get { return startTime; }
        }

        public DateTime EndTime
        {
            get { return endTime; }
        }

        public void ClearProgram()
        {
            eventDataList.Clear();
        }

        private void EnableService(ref DateTime start, ref DateTime end, ref Dictionary<UInt64, EventListInfo> enableList)
        {
            Dictionary<UInt64, ChSet5Item> noViewList = new Dictionary<ulong, ChSet5Item>();
            foreach (ChSet5Item info in Settings.Instance.OffViewServiceList)
            {
                noViewList.Add(info.Key, info);
            }

            foreach (ChSet5Item info in Settings.Instance.OnTereViewServiceList)
            {
                if( eventDataList.ContainsKey(info.Key) == true )
                {
                    enableList.Add(info.Key, eventDataList[info.Key]);
                    CheckTime(eventDataList[info.Key].StartTime, eventDataList[info.Key].EndTime, ref start, ref end);
                }
            }
            foreach (ChSet5Item info in Settings.Instance.OnBsViewServiceList)
            {
                if (eventDataList.ContainsKey(info.Key) == true)
                {
                    enableList.Add(info.Key, eventDataList[info.Key]);
                    CheckTime(eventDataList[info.Key].StartTime, eventDataList[info.Key].EndTime, ref start, ref end);
                }
            }
            foreach (ChSet5Item info in Settings.Instance.OnCsViewServiceList)
            {
                if (eventDataList.ContainsKey(info.Key) == true)
                {
                    enableList.Add(info.Key, eventDataList[info.Key]);
                    CheckTime(eventDataList[info.Key].StartTime, eventDataList[info.Key].EndTime, ref start, ref end);
                }
            }
            foreach (ChSet5Item info in Settings.Instance.OnOtherViewServiceList)
            {
                if (eventDataList.ContainsKey(info.Key) == true)
                {
                    enableList.Add(info.Key, eventDataList[info.Key]);
                    CheckTime(eventDataList[info.Key].StartTime, eventDataList[info.Key].EndTime, ref start, ref end);
                }
            }

            foreach (EventListInfo info in eventDataList.Values)
            {
                if (noViewList.ContainsKey(info.ServiceKey) == false)
                {
                    if (enableList.ContainsKey(info.ServiceKey) == false)
                    {
                        enableList.Add(info.ServiceKey, eventDataList[info.ServiceKey]);
                        CheckTime(eventDataList[info.ServiceKey].StartTime, eventDataList[info.ServiceKey].EndTime, ref start, ref end);
                    }
                }
            }

        }

        private void CheckTime(DateTime newStart, DateTime newEnd, ref DateTime currentStart, ref DateTime currentEnd)
        {
            if (currentStart.TimeOfDay == TimeSpan.Zero)
            {
                currentStart = newStart;
                currentStart = new DateTime(currentStart.Year, currentStart.Month, currentStart.Day, currentStart.Hour, 0, 0);
            }
            else
            {
                if (newStart.TimeOfDay != TimeSpan.Zero)
                {
                    if (currentStart > newStart)
                    {
                        currentStart = newStart;
                        currentStart = new DateTime(currentStart.Year, currentStart.Month, currentStart.Day, currentStart.Hour, 0, 0);
                    }
                }
            }

            if (currentEnd.TimeOfDay == TimeSpan.Zero)
            {
                currentEnd = newEnd;
                if (currentEnd.Minute != 0)
                {
                    currentEnd = new DateTime(currentEnd.Year, currentEnd.Month, currentEnd.Day, currentEnd.Hour, 0, 0).AddHours(1);
                }
            }
            else
            {
                if (newEnd.TimeOfDay != TimeSpan.Zero)
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

        public void DrawProgramView()
        {
            epgProgramView.epgViewPanel.ItemsSource = null;
            programViewList.Clear();
            enableList.Clear();

            EnableService(ref startTime, ref endTime, ref enableList);
            TimeSpan timeHeight = endTime - startTime;

            epgProgramView.canvas.Height = timeHeight.TotalMinutes * Settings.Instance.MinHeight;
            epgProgramView.canvas.Width = enableList.Count * Settings.Instance.ServiceWidth;
            epgProgramView.epgViewPanel.Height = timeHeight.TotalMinutes * Settings.Instance.MinHeight;
            epgProgramView.epgViewPanel.Width = enableList.Count * Settings.Instance.ServiceWidth;

            for (int i = 0; i < enableList.Count; i++)
            {
                EventListInfo info = enableList.Values.ElementAt(i);
                foreach (EpgEventInfo eventInfo in info.EventList.Values)
                {
                    int widthSpan = 1;
                    if (eventInfo.EventGroupInfo != null)
                    {
                        bool spanFlag = false;
                        foreach (EpgEventData data in eventInfo.EventGroupInfo.eventDataList)
                        {
                            if (info.ServiceInfo.ONID == data.original_network_id &&
                                info.ServiceInfo.TSID == data.transport_stream_id &&
                                info.ServiceInfo.SID == data.service_id)
                            {
                                spanFlag = true;
                                break;
                            }
                        }
                        if (spanFlag == false)
                        {
                            continue;
                        }
                        else
                        {
                            int count = 1;
                            while (i + count < enableList.Count)
                            {
                                EventListInfo nextInfo = enableList.Values.ElementAt(i + count);
                                bool findNext = false;
                                foreach (EpgEventData data in eventInfo.EventGroupInfo.eventDataList)
                                {
                                    if (nextInfo.ServiceInfo.ONID == data.original_network_id &&
                                        nextInfo.ServiceInfo.TSID == data.transport_stream_id &&
                                        nextInfo.ServiceInfo.SID == data.service_id)
                                    {
                                        widthSpan++;
                                        findNext = true;
                                    }
                                }
                                if (findNext == false)
                                {
                                    break;
                                }
                                count++;
                            }
                        }

                    }

                    ProgramViewItem viewItem = new ProgramViewItem(eventInfo);
                    viewItem.Height = (eventInfo.durationSec * Settings.Instance.MinHeight) / 60;
                    viewItem.Width = Settings.Instance.ServiceWidth * widthSpan;
                    viewItem.LeftPos = Settings.Instance.ServiceWidth * i;
                    viewItem.TopPos = (eventInfo.start_time - startTime).TotalMinutes * Settings.Instance.MinHeight;
                    programViewList.Add(viewItem);
                }
            }
            epgProgramView.epgViewPanel.ItemsSource = programViewList;

            epgTimeView.SetTime(StartTime, EndTime);
            epgDateView.SetTime(StartTime, EndTime);
            epgServiceView.SetService(enableList);

            ReDrawNowLine();
        }

        private void button_now_Click(object sender, RoutedEventArgs e)
        {
            MoveNowTime();
        }

        public void MoveNowTime()
        {
            DateTime time = DateTime.Now;
            if (time < startTime)
            {
                epgProgramView.scrollViewer.ScrollToVerticalOffset(0);
            }
            else
            {
                try
                {
                    TimeSpan timeHeight = time - startTime;
                    double pos = timeHeight.TotalMinutes * Settings.Instance.MinHeight - 100;
                    if (pos < 0)
                    {
                        pos = 0;
                    }
                    epgProgramView.scrollViewer.ScrollToVerticalOffset(pos);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }
    }

    public class EventListInfo
    {
        public EventListInfo()
        {
        }
        public EventListInfo(EpgServiceInfo serviceInfo, List<EpgEventInfo> eventList)
        {
            ServiceInfo = serviceInfo;
            EventList = new Dictionary<ushort, EpgEventInfo>();
            foreach (EpgEventInfo info in eventList)
            {
                if (info.StartTimeFlag == 1)
                {
                    if (StartTime.TimeOfDay == TimeSpan.Zero)
                    {
                        StartTime = info.start_time;
                    }
                    else
                    {
                        if (StartTime > info.start_time)
                        {
                            StartTime = info.start_time;
                        }
                    }
                    if (info.DurationFlag == 1)
                    {
                        if (EndTime.TimeOfDay == TimeSpan.Zero)
                        {
                            EndTime = info.start_time.AddSeconds(info.durationSec);
                        }
                        else
                        {
                            if (EndTime < info.start_time.AddSeconds(info.durationSec))
                            {
                                EndTime = info.start_time.AddSeconds(info.durationSec);
                            }
                        }
                    }
                }
                EventList.Add(info.event_id, info);
            }
        }

        public DateTime StartTime
        {
            get;
            set;
        }
        public DateTime EndTime
        {
            get;
            set;
        }

        public EpgServiceInfo ServiceInfo
        {
            get;
            set;
        }

        public UInt64 ServiceKey
        {
            get
            {
                UInt64 key = ((UInt64)ServiceInfo.ONID)<<32 | ((UInt64)ServiceInfo.TSID)<<16 | (UInt64)ServiceInfo.SID;
                return key;
            }
        }

        public Dictionary<UInt16, EpgEventInfo> EventList
        {
            get;
            set;
        }
    }

    public class ReserveViewItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public ReserveViewItem()
        {
        }
        
        public ReserveViewItem(ReserveItem item, bool view)
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

    public class ProgramViewItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private double width = 10;
        private double height = 10;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public ProgramViewItem()
        {
        }
        public ProgramViewItem(EpgEventInfo info)
        {
            EventInfo = info;
        }

        public void SendUpdateColorNotify()
        {
            NotifyPropertyChanged("ContentColor");
        }

        public EpgEventInfo EventInfo
        {
            get;
            set;
        }

        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                NotifyPropertyChanged("Width");
            }
        }

        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                NotifyPropertyChanged("Height");
            }
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

        public String ProgramInfo
        {
            get
            {
                String text = "";
                if (EventInfo.StartTimeFlag == 0)
                {
                    text += "未定 ";
                }
                else
                {
                    text += EventInfo.start_time.Minute.ToString("d02") + " ";
                }
                if (EventInfo != null)
                {
                    if (EventInfo.ShortInfo != null)
                    {
                        text += EventInfo.ShortInfo.event_name + "\r\n\r\n" + EventInfo.ShortInfo.text_char;
                    }
                }
                return text;
            }
        }

        public SolidColorBrush ContentColor
        {
            get
            {
                SolidColorBrush color = Brushes.White;
                if (EventInfo != null)
                {
                    if (EventInfo.ContentInfo != null)
                    {
                        if (EventInfo.ContentInfo.nibbleList.Count > 0)
                        {
                            try
                            {
                                string colorName = "White";
                                foreach (EpgContentData info in EventInfo.ContentInfo.nibbleList)
                                {
                                    if (info.content_nibble_level_1 <= 0x0B || info.content_nibble_level_1 == 0x0F)
                                    {
                                        colorName = Settings.Instance.ContentColorList[info.content_nibble_level_1];
                                        break;
                                    }
                                }
                                color = ColorDef.Instance.ColorTable[colorName];
                            }
                            catch
                            {
                            }
                        }
                        else
                        {
                            string colorName = Settings.Instance.ContentColorList[0x10];
                            color = ColorDef.Instance.ColorTable[colorName];
                        }
                    }
                    else
                    {
                        string colorName = Settings.Instance.ContentColorList[0x10];
                        color = ColorDef.Instance.ColorTable[colorName];
                    }
                }
                return color;
            }
        }
    }
}
