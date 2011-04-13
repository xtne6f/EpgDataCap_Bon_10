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
    /// RecInfoView.xaml の相互作用ロジック
    /// </summary>
    public partial class RecInfoView : UserControl
    {
        private List<RecInfoItem> resultList = new List<RecInfoItem>();

        private GridViewColumn _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;
        private CtrlCmdUtil cmd = CommonManager.Instance.CtrlCmd;

        private bool ReloadInfo = true;

        public RecInfoView()
        {
            InitializeComponent();

            try
            {
                if (Settings.Instance.RecInfoColumnWidth0 != 0)
                {
                    gridView_recinfo.Columns[0].Width = Settings.Instance.RecInfoColumnWidth0;
                }
                if (Settings.Instance.RecInfoColumnWidth1 != 0)
                {
                    gridView_recinfo.Columns[1].Width = Settings.Instance.RecInfoColumnWidth1;
                }
                if (Settings.Instance.RecInfoColumnWidth2 != 0)
                {
                    gridView_recinfo.Columns[2].Width = Settings.Instance.RecInfoColumnWidth2;
                }
                if (Settings.Instance.RecInfoColumnWidth3 != 0)
                {
                    gridView_recinfo.Columns[3].Width = Settings.Instance.RecInfoColumnWidth3;
                }
                if (Settings.Instance.RecInfoColumnWidth4 != 0)
                {
                    gridView_recinfo.Columns[4].Width = Settings.Instance.RecInfoColumnWidth4;
                }
                if (Settings.Instance.RecInfoColumnWidth5 != 0)
                {
                    gridView_recinfo.Columns[5].Width = Settings.Instance.RecInfoColumnWidth5;
                }
                if (Settings.Instance.RecInfoColumnWidth6 != 0)
                {
                    gridView_recinfo.Columns[6].Width = Settings.Instance.RecInfoColumnWidth6;
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
                Settings.Instance.RecInfoColumnWidth0 = gridView_recinfo.Columns[0].Width;
                Settings.Instance.RecInfoColumnWidth1 = gridView_recinfo.Columns[1].Width;
                Settings.Instance.RecInfoColumnWidth2 = gridView_recinfo.Columns[2].Width;
                Settings.Instance.RecInfoColumnWidth3 = gridView_recinfo.Columns[3].Width;
                Settings.Instance.RecInfoColumnWidth4 = gridView_recinfo.Columns[4].Width;
                Settings.Instance.RecInfoColumnWidth5 = gridView_recinfo.Columns[5].Width;
                Settings.Instance.RecInfoColumnWidth6 = gridView_recinfo.Columns[6].Width;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void button_del_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listView_recinfo.SelectedItems.Count > 0)
                {
                    List<UInt32> IDList = new List<uint>();
                    foreach (RecInfoItem info in listView_recinfo.SelectedItems)
                    {
                        IDList.Add(info.RecInfo.ID);
                    }
                    cmd.SendDelRecInfo(IDList);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            } 
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            try
            {
                ICollectionView dataView = CollectionViewSource.GetDefaultView(listView_recinfo.DataContext);

                dataView.SortDescriptions.Clear();

                SortDescription sd = new SortDescription(sortBy, direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();

                Settings.Instance.RecInfoColumnHead = sortBy;
                Settings.Instance.RecInfoSortDirection = direction;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }


        public bool ReloadInfoData()
        {
            try
            {
                ICollectionView dataView = CollectionViewSource.GetDefaultView(listView_recinfo.DataContext);
                if (dataView != null)
                {
                    dataView.SortDescriptions.Clear();
                    dataView.Refresh();
                }
                listView_recinfo.DataContext = null;
                resultList.Clear();

                ErrCode err = CommonManager.Instance.DB.ReloadrecFileInfo();
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

                foreach (RecFileInfo info in CommonManager.Instance.DB.RecFileInfo.Values)
                {
                    RecInfoItem item = new RecInfoItem(info);
                    resultList.Add(item);
                }

                listView_recinfo.DataContext = resultList;
                if (_lastHeaderClicked != null)
                {
                    string header = ((Binding)_lastHeaderClicked.DisplayMemberBinding).Path.Path;
                    Sort(header, _lastDirection);
                }
                else
                {
                    bool sort = false;
                    foreach (GridViewColumn info in gridView_recinfo.Columns)
                    {
                        string header = ((Binding)info.DisplayMemberBinding).Path.Path;
                        if (String.Compare(header, Settings.Instance.RecInfoColumnHead, true) == 0)
                        {
                            Sort(header, Settings.Instance.RecInfoSortDirection);

                            if (Settings.Instance.RecInfoSortDirection == ListSortDirection.Ascending)
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
                            _lastDirection = Settings.Instance.RecInfoSortDirection;
                            sort = true;
                            break;
                        }
                    }
                    if (gridView_recinfo.Columns.Count > 0 && sort == false)
                    {
                        string header = ((Binding)gridView_recinfo.Columns[0].DisplayMemberBinding).Path.Path;
                        Sort(header, _lastDirection);
                        gridView_recinfo.Columns[0].HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                        _lastHeaderClicked = gridView_recinfo.Columns[0];
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

        /// <summary>
        /// リストの更新通知
        /// </summary>
        public void UpdateInfo()
        {
            ReloadInfo = true;
            if (this.IsVisible == true)
            {
                if (ReloadInfoData() == true)
                {
                    ReloadInfo = false;
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ReloadInfo == true && this.IsVisible == true)
            {
                if (ReloadInfoData() == true)
                {
                    ReloadInfo = false;
                }
            }
        }

        private void listView_recinfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listView_recinfo.SelectedItem != null)
            {
                RecInfoItem info = listView_recinfo.SelectedItem as RecInfoItem;
                RecInfoDescWindow dlg = new RecInfoDescWindow();
                dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
                dlg.SetRecInfo(info.RecInfo);
                dlg.ShowDialog();
            }
        }

        private void button_play_Click(object sender, RoutedEventArgs e)
        {
            if (listView_recinfo.SelectedItem != null)
            {
                RecInfoItem info = listView_recinfo.SelectedItem as RecInfoItem;
                if (info.RecInfo.RecFilePath.Length > 0)
                {
                    try
                    {
                        CommonManager.Instance.FilePlay(info.RecInfo.RecFilePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void autoadd_Click(object sender, RoutedEventArgs e)
        {
            if (listView_recinfo.SelectedItem != null)
            {
                SearchWindow dlg = new SearchWindow();
                dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
                dlg.SetViewMode(1);

                EpgSearchKeyInfo key = new EpgSearchKeyInfo();

                RecInfoItem item = listView_recinfo.SelectedItem as RecInfoItem;

                key.andKey = item.RecInfo.Title;
                Int64 sidKey = ((Int64)item.RecInfo.OriginalNetworkID) << 32 | ((Int64)item.RecInfo.TransportStreamID) << 16 | ((Int64)item.RecInfo.ServiceID);
                key.serviceList.Add(sidKey);

                dlg.SetSearchDefKey(key);
                dlg.ShowDialog();
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ReloadInfo == true && this.IsVisible == true)
            {
                if (ReloadInfoData() == true)
                {
                    ReloadInfo = false;
                }
            }
        }
    }
}
