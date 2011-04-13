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
        private bool RedrawReserve = true;
        private List<ReserveItem> reserveList = new List<ReserveItem>();

        string _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
        string _lastHeaderClicked2 = null;
        ListSortDirection _lastDirection2 = ListSortDirection.Ascending;

        private CtrlCmdUtil cmd = CommonManager.Instance.CtrlCmd;

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
                if (Settings.Instance.ResColumnWidth7 != 0)
                {
                    gridView_reserve.Columns[7].Width = Settings.Instance.ResColumnWidth7;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
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
                Settings.Instance.ResColumnWidth7 = gridView_reserve.Columns[7].Width;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (RedrawReserve == true && this.IsVisible == true)
            {
                if (ReDrawReserveData() == true)
                {
                    RedrawReserve = false;
                }
            }
        }

        /// <summary>
        /// 予約情報の更新通知
        /// </summary>
        public void UpdateReserveData()
        {
            RedrawReserve = true;
            if (this.IsVisible == true)
            {
                if (ReDrawReserveData() == true)
                {
                    RedrawReserve = false;
                }
            }
        }

        private bool ReDrawReserveData()
        {
            try
            {
                ErrCode err = CommonManager.Instance.DB.ReloadReserveInfo();
                if (err == ErrCode.CMD_ERR_CONNECT)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("サーバー または EpgTimerSrv に接続できませんでした。");
                    }), null); 
                    return false;
                }
                if (err == ErrCode.CMD_ERR_TIMEOUT)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("EpgTimerSrvとの接続にタイムアウトしました。");
                    }), null);
                    return false;
                }
                if (err != ErrCode.CMD_SUCCESS)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("情報の取得でエラーが発生しました。");
                    }), null);
                    return false;
                }

                ICollectionView dataView = CollectionViewSource.GetDefaultView(listView_reserve.DataContext);
                if (dataView != null)
                {
                    dataView.SortDescriptions.Clear();
                    dataView.Refresh();
                }
                listView_reserve.DataContext = null;
                reserveList.Clear();

                foreach (ReserveData info in CommonManager.Instance.DB.ReserveList.Values)
                {
                    ReserveItem item = new ReserveItem(info);
                    reserveList.Add(item);
                }

                listView_reserve.DataContext = reserveList;
                if (_lastHeaderClicked != null)
                {
                    //string header = ((Binding)_lastHeaderClicked.DisplayMemberBinding).Path.Path;
                    Sort(_lastHeaderClicked, _lastDirection);
                }
                else
                {
                    bool sort = false;
                    foreach (GridViewColumn info in gridView_reserve.Columns)
                    {
                        string header = ((Binding)info.DisplayMemberBinding).Path.Path;
                        if (String.Compare(header, Settings.Instance.ResColumnHead, true) == 0)
                        {
                            Sort(header, Settings.Instance.ResSortDirection);

                            _lastHeaderClicked = header;
                            _lastDirection = Settings.Instance.ResSortDirection;

                            sort = true;
                            break;
                        }
                    }
                    if (gridView_reserve.Columns.Count > 0 && sort == false)
                    {
                        string header = ((Binding)gridView_reserve.Columns[0].DisplayMemberBinding).Path.Path;
                        Sort(header, _lastDirection);
                        _lastHeaderClicked = header;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
                }), null);
                return false;
            }
            return true;
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            try
            {
                ICollectionView dataView = CollectionViewSource.GetDefaultView(listView_reserve.DataContext);

                dataView.SortDescriptions.Clear();

                SortDescription sd = new SortDescription(sortBy, direction);
                dataView.SortDescriptions.Add(sd);
                if (_lastHeaderClicked2 != null)
                {
                    if (String.Compare(sortBy, _lastHeaderClicked2) != 0)
                    {
                        SortDescription sd2 = new SortDescription(_lastHeaderClicked2, _lastDirection2);
                        dataView.SortDescriptions.Add(sd2);
                    }
                }
                dataView.Refresh();

                Settings.Instance.ResColumnHead = sortBy;
                Settings.Instance.ResSortDirection = direction;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    string header = ((Binding)headerClicked.Column.DisplayMemberBinding).Path.Path;
                    if (String.Compare( header, _lastHeaderClicked) != 0 )
                    {
                        direction = ListSortDirection.Ascending;
                        _lastHeaderClicked2 = _lastHeaderClicked;
                        _lastDirection2 = _lastDirection;
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

                    Sort(header, direction);

                    _lastHeaderClicked = header;
                    _lastDirection = direction;
                }
            }
        }

        private void listView_reserve_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ChangeReserve();
        }

        private void button_change_Click(object sender, RoutedEventArgs e)
        {
            ChangeReserve();
        }

        private void ChangeReserve()
        {
            if (listView_reserve.SelectedItem != null)
            {
                ReserveItem item = listView_reserve.SelectedItem as ReserveItem;
                ChgReserveWindow dlg = new ChgReserveWindow();
                dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
                dlg.SetReserveInfo(item.ReserveInfo);
                if (dlg.ShowDialog() == true)
                {
                }
            }
        }

        private void recmode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<ReserveData> list = new List<ReserveData>();
                foreach (ReserveItem item in listView_reserve.SelectedItems)
                {
                    ReserveData reserveInfo = item.ReserveInfo;

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
                    else if (sender == recmode_no)
                    {
                        recMode = 5;
                    }
                    else
                    {
                        return;
                    }
                    reserveInfo.RecSetting.RecMode = recMode;

                    list.Add(reserveInfo);
                }
                if (list.Count > 0)
                {
                    ErrCode err = (ErrCode)cmd.SendChgReserve(list);
                    if (err == ErrCode.CMD_ERR_CONNECT)
                    {
                        MessageBox.Show("サーバー または EpgTimerSrv に接続できませんでした。");
                    }
                    if (err == ErrCode.CMD_ERR_TIMEOUT)
                    {
                        MessageBox.Show("EpgTimerSrvとの接続にタイムアウトしました。");
                    }
                    if (err != ErrCode.CMD_SUCCESS)
                    {
                        MessageBox.Show("チューナー一覧の取得でエラーが発生しました。");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void button_no_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<ReserveData> list = new List<ReserveData>();
                foreach (ReserveItem item in listView_reserve.SelectedItems)
                {
                    ReserveData reserveInfo = item.ReserveInfo;

                    reserveInfo.RecSetting.RecMode = 5;

                    list.Add(reserveInfo);
                }
                if (list.Count > 0)
                {
                    ErrCode err = (ErrCode)cmd.SendChgReserve(list);
                    if (err == ErrCode.CMD_ERR_CONNECT)
                    {
                        MessageBox.Show("サーバー または EpgTimerSrv に接続できませんでした。");
                    }
                    if (err == ErrCode.CMD_ERR_TIMEOUT)
                    {
                        MessageBox.Show("EpgTimerSrvとの接続にタイムアウトしました。");
                    }
                    if (err != ErrCode.CMD_SUCCESS)
                    {
                        MessageBox.Show("チューナー一覧の取得でエラーが発生しました。");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void priority_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<ReserveData> list = new List<ReserveData>();
                foreach (ReserveItem item in listView_reserve.SelectedItems)
                {
                    ReserveData reserveInfo = item.ReserveInfo;

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
                    reserveInfo.RecSetting.Priority = priority;

                    list.Add(reserveInfo);
                }
                if (list.Count > 0)
                {
                    ErrCode err = (ErrCode)cmd.SendChgReserve(list);
                    if (err == ErrCode.CMD_ERR_CONNECT)
                    {
                        MessageBox.Show("サーバー または EpgTimerSrv に接続できませんでした。");
                    }
                    if (err == ErrCode.CMD_ERR_TIMEOUT)
                    {
                        MessageBox.Show("EpgTimerSrvとの接続にタイムアウトしました。");
                    }
                    if (err != ErrCode.CMD_SUCCESS)
                    {
                        MessageBox.Show("チューナー一覧の取得でエラーが発生しました。");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void autoadd_Click(object sender, RoutedEventArgs e)
        {
            if (listView_reserve.SelectedItem != null)
            {
                SearchWindow dlg = new SearchWindow();
                dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
                dlg.SetViewMode(1);

                EpgSearchKeyInfo key = new EpgSearchKeyInfo();

                ReserveItem item = listView_reserve.SelectedItem as ReserveItem;

                key.andKey = item.ReserveInfo.Title;
                Int64 sidKey = ((Int64)item.ReserveInfo.OriginalNetworkID) << 32 | ((Int64)item.ReserveInfo.TransportStreamID) << 16 | ((Int64)item.ReserveInfo.ServiceID);
                key.serviceList.Add(sidKey);

                dlg.SetSearchDefKey(key);
                dlg.ShowDialog();
            }
        }

        private void timeShiftPlay_Click(object sender, RoutedEventArgs e)
        {
            if (listView_reserve.SelectedItem != null)
            {
                ReserveItem info = listView_reserve.SelectedItem as ReserveItem;
                CommonManager.Instance.TVTestCtrl.StartTimeShift(info.ReserveInfo.ReserveID);
            }
        }

        private void button_del_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<UInt32> list = new List<UInt32>();
                foreach (ReserveItem item in listView_reserve.SelectedItems)
                {
                    ReserveData reserveInfo = item.ReserveInfo;

                    list.Add(reserveInfo.ReserveID);
                }
                if (list.Count > 0)
                {
                    ErrCode err = (ErrCode)cmd.SendDelReserve(list);
                    if (err == ErrCode.CMD_ERR_CONNECT)
                    {
                        MessageBox.Show("サーバー または EpgTimerSrv に接続できませんでした。");
                    }
                    if (err == ErrCode.CMD_ERR_TIMEOUT)
                    {
                        MessageBox.Show("EpgTimerSrvとの接続にタイムアウトしました。");
                    }
                    if (err != ErrCode.CMD_SUCCESS)
                    {
                        MessageBox.Show("チューナー一覧の取得でエラーが発生しました。");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void button_add_manual_Click(object sender, RoutedEventArgs e)
        {
            ChgReserveWindow dlg = new ChgReserveWindow();
            dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
            dlg.AddReserveMode(true);
            dlg.ShowDialog();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (RedrawReserve == true && this.IsVisible == true)
            {
                if (ReDrawReserveData() == true)
                {
                    RedrawReserve = false;
                }
            }
        }

    }
}
