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
        private CtrlCmdUtil cmd = EpgTimerDef.Instance.CtrlCmd;
        private List<RecInfoItem> resultList = new List<RecInfoItem>();

        private GridViewColumn _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

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
            }
            catch
            {
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
            }
            catch
            {
            }
        }

        public void ReloadRecInfo()
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(listView_recinfo.DataContext);
            if (dataView != null)
            {
                dataView.SortDescriptions.Clear();
                dataView.Refresh();
            }
            listView_recinfo.DataContext = null;
            resultList.Clear();

            List<RecFileInfo> list = new List<RecFileInfo>();
            uint err = cmd.SendEnumRecInfo(ref list);
            foreach (RecFileInfo info in list)
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

        private void button_del_Click(object sender, RoutedEventArgs e)
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

        private void button_play_Click(object sender, RoutedEventArgs e)
        {
            if (listView_recinfo.SelectedItem != null)
            {
                RecInfoItem info = listView_recinfo.SelectedItem as RecInfoItem;
                if (info.RecInfo.RecFilePath.Length > 0)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(info.RecInfo.RecFilePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
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
            ICollectionView dataView = CollectionViewSource.GetDefaultView(listView_recinfo.DataContext);

            dataView.SortDescriptions.Clear();

            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();

            Settings.Instance.RecInfoColumnHead = sortBy;
            Settings.Instance.RecInfoSortDirection = direction;
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (resultList.Count == 0)
            {
                ReloadRecInfo();
            }
        }

        private void autoadd_Click(object sender, RoutedEventArgs e)
        {
            if (listView_recinfo.SelectedItem != null)
            {

                SearchWindow dlg = new SearchWindow();
                dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
                dlg.SetViewMode(1);

                SearchKeyInfo key = new SearchKeyInfo();

                RecInfoItem item = listView_recinfo.SelectedItem as RecInfoItem;

                key.AndKey = item.RecInfo.Title;
                Int64 sidKey = ((Int64)item.RecInfo.OriginalNetworkID) << 32 | ((Int64)item.RecInfo.TransportStreamID) << 16 | ((Int64)item.RecInfo.ServiceID);
                key.ServiceList.Add(sidKey);

                dlg.SetDefSearchKey(key);
                dlg.ShowDialog();
            }
        }
    }

    class RecInfoItem
    {
        public RecInfoItem(RecFileInfo item)
        {
            this.RecInfo = item;
        }
        public RecFileInfo RecInfo
        {
            get;
            set;
        }
        public String EventName
        {
            get
            {
                String view = "";
                if (RecInfo != null)
                {
                    view = RecInfo.Title;
                }
                return view;
            }
        }
        public String ServiceName
        {
            get
            {
                String view = "";
                if (RecInfo != null)
                {
                    view = RecInfo.ServiceName;
                }
                return view;
            }
        }
        public String StartTime
        {
            get
            {
                String view = "";
                if (RecInfo != null)
                {
                    view = RecInfo.StartTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss ～ ");
                    DateTime endTime = RecInfo.StartTime + TimeSpan.FromSeconds(RecInfo.DurationSecond);
                    view += endTime.ToString("HH:mm:ss");                
                }
                return view;
            }
        }
        public String Drops
        {
            get
            {
                String view = "";
                if (RecInfo != null)
                {
                    view = RecInfo.Drops.ToString();
                }
                return view;
            }
        }
        public String Scrambles
        {
            get
            {
                String view = "";
                if (RecInfo != null)
                {
                    view = RecInfo.Scrambles.ToString();
                }
                return view;
            }
        }
        public String Result
        {
            get
            {
                String view = "";
                if (RecInfo != null)
                {
                    view = RecInfo.Comment;
                }
                return view;
            }
        }
        public SolidColorBrush BackColor
        {
            get
            {
                SolidColorBrush color = Brushes.White;
                if (RecInfo != null)
                {
                    if (RecInfo.Scrambles > 0)
                    {
                        color = Brushes.Yellow;
                    }
                    if (RecInfo.Drops > 0)
                    {
                        color = Brushes.Red;
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
                if (RecInfo != null)
                {
                    view = RecInfo.StartTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss ～ ");
                    DateTime endTime = RecInfo.StartTime + TimeSpan.FromSeconds(RecInfo.DurationSecond);
                    view += endTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss") + "\r\n";

                    view += ServiceName + "\r\n";
                    view += EventName + "\r\n\r\n";
                    view += "OriginalNetworkID : " + RecInfo.OriginalNetworkID.ToString() + " (0x" + RecInfo.OriginalNetworkID.ToString("X4") + ")\r\n";
                    view += "TransportStreamID : " + RecInfo.TransportStreamID.ToString() + " (0x" + RecInfo.TransportStreamID.ToString("X4") + ")\r\n";
                    view += "ServiceID : " + RecInfo.ServiceID.ToString() + " (0x" + RecInfo.ServiceID.ToString("X4") + ")\r\n";
                    view += "EventID : " + RecInfo.EventID.ToString() + " (0x" + RecInfo.EventID.ToString("X4") + ")\r\n";
                    view += "Drops : " + RecInfo.Drops.ToString() + "\r\n";
                    view += "Scrambles : " + RecInfo.Scrambles.ToString() + "\r\n";
                }


                TextBlock block = new TextBlock();
                block.Text = view;
                block.MaxWidth = 400;
                block.TextWrapping = TextWrapping.Wrap;
                return block;
            }
        }
    }
}
