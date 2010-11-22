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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections;

using CtrlCmdCLI;
using CtrlCmdCLI.Def;

namespace EpgTimer
{
    /// <summary>
    /// ReserveView.xaml の相互作用ロジック
    /// </summary>
    public partial class ReserveView : UserControl
    {
        private CtrlCmdUtil cmd = EpgTimerDef.Instance.CtrlCmd;
        private List<ReserveItem> resultList = new List<ReserveItem>();

        GridViewColumn _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public ReserveView()
        {
            InitializeComponent();

            try
            {
                if (Settings.Instance.ResColumnWidth0 != 0)
                {
                    gridView_reserve.Columns[0].Width = Settings.Instance.ResColumnWidth0;
                }
                if (Settings.Instance.ResColumnWidth1 != 0)
                {
                    gridView_reserve.Columns[1].Width = Settings.Instance.ResColumnWidth1;
                }
                if (Settings.Instance.ResColumnWidth2 != 0)
                {
                    gridView_reserve.Columns[2].Width = Settings.Instance.ResColumnWidth2;
                }
                if (Settings.Instance.ResColumnWidth3 != 0)
                {
                    gridView_reserve.Columns[3].Width = Settings.Instance.ResColumnWidth3;
                }
                if (Settings.Instance.ResColumnWidth4 != 0)
                {
                    gridView_reserve.Columns[4].Width = Settings.Instance.ResColumnWidth4;
                }
                if (Settings.Instance.ResColumnWidth5 != 0)
                {
                    gridView_reserve.Columns[5].Width = Settings.Instance.ResColumnWidth5;
                }
                if (Settings.Instance.ResColumnWidth6 != 0)
                {
                    gridView_reserve.Columns[6].Width = Settings.Instance.ResColumnWidth6;
                }
            }
            catch
            {
            }
        }

        public void SaveSize()
        {
            try
            {
                Settings.Instance.ResColumnWidth0 = gridView_reserve.Columns[0].Width;
                Settings.Instance.ResColumnWidth1 = gridView_reserve.Columns[1].Width;
                Settings.Instance.ResColumnWidth2 = gridView_reserve.Columns[2].Width;
                Settings.Instance.ResColumnWidth3 = gridView_reserve.Columns[3].Width;
                Settings.Instance.ResColumnWidth4 = gridView_reserve.Columns[4].Width;
                Settings.Instance.ResColumnWidth5 = gridView_reserve.Columns[5].Width;
                Settings.Instance.ResColumnWidth6 = gridView_reserve.Columns[6].Width;
            }
            catch
            {
            }
        }

        public bool GetNextReserve(ref ReserveData item)
        {
            ReserveItem next = null;
            foreach (ReserveItem info in resultList)
            {
                if (info.ReserveInfo.RecSetting.RecMode != 5)
                {
                    if (next == null)
                    {
                        next = info;
                    }
                    else
                    {
                        if (next.ReserveInfo.StartTime > info.ReserveInfo.StartTime)
                        {
                            next = info;
                        }
                    }
                }
            }
            if (next == null)
            {
                return false;
            }
            else
            {
                item = next.ReserveInfo;
                return true;
            }
        }

        public void ReloadReserve()
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(listView_reserve.DataContext);
            if (dataView != null)
            {
                dataView.SortDescriptions.Clear();
                dataView.Refresh();
            }
            listView_reserve.DataContext = null;
            resultList.Clear();

            List<ReserveData> list = new List<ReserveData>();
            uint err = cmd.SendEnumReserve(ref list);
            foreach (ReserveData info in list)
            {
                ReserveItem item = new ReserveItem(info);
                resultList.Add(item);
            }

            listView_reserve.DataContext = resultList;
            if (_lastHeaderClicked != null)
            {
                string header = ((Binding)_lastHeaderClicked.DisplayMemberBinding).Path.Path;
                Sort(header, _lastDirection);
            }
            else
            {
                bool sort = false;
                foreach (GridViewColumn info in gridView_reserve.Columns)
                {
                    string header = ((Binding)info.DisplayMemberBinding).Path.Path;
                    if( String.Compare(header, Settings.Instance.ResColumnHead, true ) == 0 ){
                        Sort(header, Settings.Instance.ResSortDirection);

                        if (Settings.Instance.ResSortDirection == ListSortDirection.Ascending)
                        {
                            info.HeaderTemplate =
                              Resources["HeaderTemplateArrowUp"] as DataTemplate;
                        }
                        else
                        {
                            info.HeaderTemplate =
                              Resources["HeaderTemplateArrowDown"] as DataTemplate;
                        }

                        _lastHeaderClicked = info;
                        _lastDirection = Settings.Instance.ResSortDirection;

                        sort = true;
                        break;
                    }
                }
                if (gridView_reserve.Columns.Count > 0 && sort == false)
                {
                    string header = ((Binding)gridView_reserve.Columns[0].DisplayMemberBinding).Path.Path;
                    Sort(header, _lastDirection);
                    gridView_reserve.Columns[0].HeaderTemplate =
                      Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    _lastHeaderClicked = gridView_reserve.Columns[0];
                }
            }
        }

        public void GetReserveList(ref List<ReserveItem> reserveList)
        {
            reserveList = this.resultList;
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked.Column != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    string header = ((Binding)headerClicked.Column.DisplayMemberBinding).Path.Path;
                    Sort(header, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked.Column)
                    {
                        _lastHeaderClicked.HeaderTemplate = null;
                    }


                    _lastHeaderClicked = headerClicked.Column;
                    _lastDirection = direction;
                }
            }

        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(listView_reserve.DataContext);

            dataView.SortDescriptions.Clear();

            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();

            Settings.Instance.ResColumnHead = sortBy;
            Settings.Instance.ResSortDirection = direction;
        }

        private void button_del_Click(object sender, RoutedEventArgs e)
        {
            if (listView_reserve.SelectedItems.Count > 0)
            {
                List<UInt32> reserveIDList = new List<uint>();
                foreach (ReserveItem info in listView_reserve.SelectedItems)
                {
                    reserveIDList.Add(info.ReserveInfo.ReserveID);
                }
                cmd.SendDelReserve(reserveIDList);
            }
        }

        private void button_no_Click(object sender, RoutedEventArgs e)
        {
            if (listView_reserve.SelectedItems.Count > 0)
            {
                List<ReserveData> reserveList = new List<ReserveData>();
                foreach (ReserveItem info in listView_reserve.SelectedItems)
                {
                    info.ReserveInfo.RecSetting.RecMode = 0x05;
                    reserveList.Add(info.ReserveInfo);
                }
                cmd.SendChgReserve(reserveList);
            }
        }

        private void button_change_Click(object sender, RoutedEventArgs e)
        {
            if (listView_reserve.SelectedItem != null)
            {
                ChangeReserveWindow dlg = new ChangeReserveWindow();
                dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
                ReserveItem item = listView_reserve.SelectedItem as ReserveItem;
                dlg.SetReserve(item.ReserveInfo);
                if (item.ReserveInfo.EventID != 0xFFFF)
                {
                    EpgEventInfo eventInfo = new EpgEventInfo();
                    UInt64 pgID = ((UInt64)item.ReserveInfo.OriginalNetworkID) << 48 |
                        ((UInt64)item.ReserveInfo.TransportStreamID) << 32 |
                        ((UInt64)item.ReserveInfo.ServiceID) << 16 |
                        (UInt64)item.ReserveInfo.EventID;
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
                        deleteList.Add(item.ReserveInfo.ReserveID);
                        cmd.SendDelReserve(deleteList);
                    }
                }
            }
        }

        private void button_add_manual_Click(object sender, RoutedEventArgs e)
        {
            ChangeReserveWindow dlg = new ChangeReserveWindow();
            dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
            dlg.AddReserveMode(true);
            if (dlg.ShowDialog() == true)
            {
                List<ReserveData> addList = new List<ReserveData>();
                addList.Add(dlg.setInfo);
                cmd.SendAddReserve(addList);
            }
        }

        private void listView_reserve_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listView_reserve.SelectedItem != null)
            {
                ChangeReserveWindow dlg = new ChangeReserveWindow();
                dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
                ReserveItem item = listView_reserve.SelectedItem as ReserveItem;
                dlg.SetReserve(item.ReserveInfo);
                if (item.ReserveInfo.EventID != 0xFFFF)
                {
                    EpgEventInfo eventInfo = new EpgEventInfo();
                    UInt64 pgID = ((UInt64)item.ReserveInfo.OriginalNetworkID) << 48 |
                        ((UInt64)item.ReserveInfo.TransportStreamID) << 32 |
                        ((UInt64)item.ReserveInfo.ServiceID) << 16 |
                        (UInt64)item.ReserveInfo.EventID;
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
                        deleteList.Add(item.ReserveInfo.ReserveID);
                        cmd.SendDelReserve(deleteList);
                    }
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (resultList.Count == 0)
            {
                ReloadReserve();
            }
        }

        private void recmode_Click(object sender, RoutedEventArgs e)
        {
            byte recMode = 0;
            if (sender == recmode_all)
            {
                recMode = 0;
            }
            else if (sender == recmode_only)
            {
                recMode = 1;
            }
            else if (sender == recmode_all_nodec)
            {
                recMode = 2;
            }
            else if (sender == recmode_only_nodec)
            {
                recMode = 3;
            }
            else if (sender == recmode_view)
            {
                recMode = 4;
            }
            else
            {
                return;
            }

            if (listView_reserve.SelectedItems.Count > 0)
            {
                List<ReserveData> reserveList = new List<ReserveData>();
                foreach (ReserveItem info in listView_reserve.SelectedItems)
                {
                    info.ReserveInfo.RecSetting.RecMode = recMode;
                    reserveList.Add(info.ReserveInfo);
                }
                cmd.SendChgReserve(reserveList);
            }
        }

        private void priority_Click(object sender, RoutedEventArgs e)
        {
            byte priority = 1;
            if (sender == priority_1)
            {
                priority = 1;
            }
            else if (sender == priority_2)
            {
                priority = 2;
            }
            else if (sender == priority_3)
            {
                priority = 3;
            }
            else if (sender == priority_4)
            {
                priority = 4;
            }
            else if (sender == priority_5)
            {
                priority = 5;
            }
            else
            {
                return;
            }

            if (listView_reserve.SelectedItems.Count > 0)
            {
                List<ReserveData> reserveList = new List<ReserveData>();
                foreach (ReserveItem info in listView_reserve.SelectedItems)
                {
                    info.ReserveInfo.RecSetting.Priority = priority;
                    reserveList.Add(info.ReserveInfo);
                }
                cmd.SendChgReserve(reserveList);
            }
        }

        private void autoadd_Click(object sender, RoutedEventArgs e)
        {
            if (listView_reserve.SelectedItem != null)
            {
                
                SearchWindow dlg = new SearchWindow();
                dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
                dlg.SetViewMode(1);

                SearchKeyInfo key = new SearchKeyInfo();

                ReserveItem item = listView_reserve.SelectedItem as ReserveItem;

                key.AndKey = item.ReserveInfo.Title;
                Int64 sidKey = ((Int64)item.ReserveInfo.OriginalNetworkID) << 32 | ((Int64)item.ReserveInfo.TransportStreamID) << 16 | ((Int64)item.ReserveInfo.ServiceID);
                key.ServiceList.Add(sidKey);
                try
                {
                    dlg.SetDefSearchKey(key);
                    dlg.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }

    public class ReserveItem
    {
        public ReserveItem(ReserveData item)
        {
            this.ReserveInfo = item;
        }
        public ReserveData ReserveInfo
        {
            get;
            set;
        }
        public String EventName
        {
            get
            {
                String view = "";
                if (ReserveInfo != null)
                {
                    view = ReserveInfo.Title;
                }
                return view;
            }
        }
        public String ServiceName
        {
            get
            {
                String view = "";
                if (ReserveInfo != null)
                {
                    view = ReserveInfo.StationName;
                }
                return view;
            }
        }
        public String StartTime
        {
            get
            {
                String view = "";
                if (ReserveInfo != null)
                {
                    view = ReserveInfo.StartTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss ～ ");
                    DateTime endTime = ReserveInfo.StartTime + TimeSpan.FromSeconds(ReserveInfo.DurationSecond);
                    view += endTime.ToString("HH:mm:ss");
                }
                return view;
            }
        }
        public String RecMode
        {
            get
            {
                String view = "";
                if (ReserveInfo != null)
                {
                    switch (ReserveInfo.RecSetting.RecMode)
                    {
                        case 0:
                            view = "全サービス";
                            break;
                        case 1:
                            view = "指定サービス";
                            break;
                        case 2:
                            view = "全サービス（デコード処理なし）";
                            break;
                        case 3:
                            view = "指定サービス（デコード処理なし）";
                            break;
                        case 4:
                            view = "視聴";
                            break;
                        case 5:
                            view = "無効";
                            break;
                        default:
                            break;
                    }
                }
                return view;
            }
        }
        public String Priority
        {
            get
            {
                String view = "";
                if (ReserveInfo != null)
                {
                    view = ReserveInfo.RecSetting.Priority.ToString();
                }
                return view;
            }
        }
        public String Tuijyu
        {
            get
            {
                String view = "";
                if (ReserveInfo != null)
                {
                    if (ReserveInfo.RecSetting.TuijyuuFlag == 0)
                    {
                        view = "しない";
                    }
                    else if (ReserveInfo.RecSetting.TuijyuuFlag == 1)
                    {
                        view = "する";
                    }
                }
                return view;
            }
        }
        public String Pittari
        {
            get
            {
                String view = "";
                if (ReserveInfo != null)
                {
                    if (ReserveInfo.RecSetting.PittariFlag == 0)
                    {
                        view = "しない";
                    }
                    else if (ReserveInfo.RecSetting.PittariFlag == 1)
                    {
                        view = "する";
                    }
                }
                return view;
            }
        }
        public SolidColorBrush BackColor
        {
            get
            {
                SolidColorBrush color = Brushes.White;
                if (ReserveInfo != null)
                {
                    if (ReserveInfo.RecSetting.RecMode == 5)
                    {
                        color = Brushes.DarkGray;
                    }
                    else if (ReserveInfo.OverlapMode == 2)
                    {
                        color = Brushes.Red;
                    }
                    else if (ReserveInfo.OverlapMode == 1)
                    {
                        color = Brushes.Yellow;
                    }
                }
                return color;
            }
        }
        public TextBlock ToolTipView
        {
            get
            {
                if (Settings.Instance.NoToolTip == true)
                {
                    return null;
                }
                String view = "";
                if (ReserveInfo != null)
                {
                    view = ReserveInfo.StartTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss ～ ");
                    DateTime endTime = ReserveInfo.StartTime + TimeSpan.FromSeconds(ReserveInfo.DurationSecond);
                    view += endTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss") + "\r\n";

                    view += ServiceName + "\r\n";
                    view += EventName + "\r\n\r\n";
                    view += "録画モード : " + RecMode + "\r\n";
                    view += "優先度 : " + Priority + "\r\n";
                    view += "追従 : " + Tuijyu + "\r\n";
                    //view += "ぴったり（？） : " + Pittari + "\r\n";
                    if ((ReserveInfo.RecSetting.ServiceMode & 0x01) == 0)
                    {
                        view += "指定サービス対象データ : デフォルト\r\n";
                    }
                    else
                    {
                        view += "指定サービス対象データ : ";
                        if ((ReserveInfo.RecSetting.ServiceMode & 0x10) > 0)
                        {
                            view += "字幕含む ";
                        }
                        if ((ReserveInfo.RecSetting.ServiceMode & 0x20) > 0)
                        {
                            view += "データカルーセル含む";
                        }
                        view += "\r\n";
                    }

                    view += "録画実行bat : " + ReserveInfo.RecSetting.BatFilePath + "\r\n";

                    if (ReserveInfo.RecSetting.RecFolderList.Count == 0)
                    {
                        view += "録画フォルダ : デフォルト\r\n";
                    }
                    else
                    {
                        view += "録画フォルダ : \r\n";
                        foreach (RecFileSetInfo info in ReserveInfo.RecSetting.RecFolderList)
                        {
                            view += info.RecFolder + " (WritePlugIn:" + info.WritePlugIn + ")\r\n";
                        }
                    }

                    if (ReserveInfo.RecSetting.UseMargineFlag == 0)
                    {
                        view += "録画マージン : デフォルト\r\n";
                    }
                    else
                    {
                        view += "録画マージン : 開始 " + ReserveInfo.RecSetting.StartMargine.ToString() + 
                            " 終了 " + ReserveInfo.RecSetting.EndMargine.ToString() + "\r\n";
                    }

                    if (ReserveInfo.RecSetting.SuspendMode == 0)
                    {
                        view += "録画後動作 : デフォルト\r\n";
                    }
                    else
                    {
                        view += "録画後動作 : ";
                        switch (ReserveInfo.RecSetting.SuspendMode)
                        {
                            case 1:
                                view += "スタンバイ";
                                break;
                            case 2:
                                view += "休止";
                                break;
                            case 3:
                                view += "シャットダウン";
                                break;
                            case 4:
                                view += "何もしない";
                                break;
                        }
                        if (ReserveInfo.RecSetting.RebootFlag == 1)
                        {
                            view += " 復帰後再起動する";
                        }
                        view += "\r\n";
                    }

                }
                view += "OriginalNetworkID : " + ReserveInfo.OriginalNetworkID.ToString() + " (0x" + ReserveInfo.OriginalNetworkID.ToString("X4") + ")\r\n";
                view += "TransportStreamID : " + ReserveInfo.TransportStreamID.ToString() + " (0x" + ReserveInfo.TransportStreamID.ToString("X4") + ")\r\n";
                view += "ServiceID : " + ReserveInfo.ServiceID.ToString() + " (0x" + ReserveInfo.ServiceID.ToString("X4") + ")\r\n";
                view += "EventID : " + ReserveInfo.EventID.ToString() + " (0x" + ReserveInfo.EventID.ToString("X4") + ")\r\n";


                TextBlock block = new TextBlock();
                block.Text = view;
                block.MaxWidth = 400;
                block.TextWrapping = TextWrapping.Wrap;
                return block;
            }
        }
    }


}
