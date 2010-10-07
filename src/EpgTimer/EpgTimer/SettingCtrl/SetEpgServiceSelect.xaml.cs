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

namespace EpgTimer
{
    /// <summary>
    /// SetEpgServiceSelect.xaml の相互作用ロジック
    /// </summary>
    public partial class SetEpgServiceSelect : UserControl
    {
        private List<ChSet5Item> onServiceList = new List<ChSet5Item>();
        private Dictionary<UInt64, ViewItem> allServiceList = new Dictionary<UInt64, ViewItem>();
        public SetEpgServiceSelect()
        {
            InitializeComponent();
        }

        public void SetSortMode(bool remoconFlag)
        {
            if (remoconFlag == true)
            {
                button_sort.Content = "リモコンIDでソート";
            }
            else
            {
                button_sort.Content = "サービスIDでソート";
            }
        }

        public void SetService(List<ChSet5Item> onService, List<ChSet5Item> allService)
        {
            foreach (ChSet5Item info in allService)
            {
                ViewItem item = new ViewItem(info, false);
                allServiceList.Add(item.Key, item);
            }

            foreach (ChSet5Item info in onService)
            {
                ViewItem item = new ViewItem(info, true);
                if (allServiceList.ContainsKey(item.Key) == true)
                {
                    allServiceList[item.Key].ViewOn = true;
                }
                listBox_on.Items.Add(item);
            }
            ReloadOffList();
        }

        public void GetOnService(ref List<ChSet5Item> onService, ref List<ChSet5Item> offService)
        {
            foreach (ViewItem info in listBox_on.Items)
            {
                onService.Add(info.ServiceItem);
            }
            foreach (ViewItem info in allServiceList.Values)
            {
                if (info.ViewOn == false)
                {
                    offService.Add(info.ServiceItem);
                }
            }
        }

        private void ReloadOffList()
        {
            listBox_off.Items.Clear();
            foreach (ViewItem info in allServiceList.Values)
            {
                if (info.ViewOn == false)
                {
                    listBox_off.Items.Add(info);
                }
            }
        }

        private void button_off_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_on.SelectedItem != null)
            {
                ViewItem item = listBox_on.SelectedItem as ViewItem;
                if (allServiceList.ContainsKey(item.Key) == true)
                {
                    allServiceList[item.Key].ViewOn = false;
                }
                listBox_on.Items.Remove(item);
                ReloadOffList();
            }
        }

        private void button_on_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_off.SelectedItem != null)
            {
                ViewItem item = listBox_off.SelectedItem as ViewItem;
                if (allServiceList.ContainsKey(item.Key) == true)
                {
                    allServiceList[item.Key].ViewOn = true;
                }
                listBox_on.Items.Add(item);
                ReloadOffList();
            }
        }

        private void button_up_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_on.SelectedItem != null)
            {
                if (listBox_on.SelectedIndex >= 1)
                {
                    object temp = listBox_on.SelectedItem;
                    int index = listBox_on.SelectedIndex;
                    listBox_on.Items.RemoveAt(listBox_on.SelectedIndex);
                    listBox_on.Items.Insert(index - 1, temp);
                    listBox_on.SelectedIndex = index - 1;
                }
            }
        }

        private void button_down_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_on.SelectedItem != null)
            {
                if (listBox_on.SelectedIndex < listBox_on.Items.Count - 1)
                {
                    object temp = listBox_on.SelectedItem;
                    int index = listBox_on.SelectedIndex;
                    listBox_on.Items.RemoveAt(listBox_on.SelectedIndex);
                    listBox_on.Items.Insert(index + 1, temp);
                    listBox_on.SelectedIndex = index + 1;
                }
            }
        }

        private void button_all_off_Click(object sender, RoutedEventArgs e)
        {
            listBox_on.Items.Clear();
            foreach (ViewItem info in allServiceList.Values)
            {
                info.ViewOn = false;
            }
            ReloadOffList();
        }

        private void button_all_on_Click(object sender, RoutedEventArgs e)
        {
            listBox_on.Items.Clear();
            foreach (ViewItem info in allServiceList.Values)
            {
                info.ViewOn = true;
                listBox_on.Items.Add(info);
            }
            ReloadOffList();
        }

        private void button_sort_Click(object sender, RoutedEventArgs e)
        {
            SortedList<String, ViewItem> sort = new SortedList<string, ViewItem>();
            foreach (ViewItem item in listBox_on.Items)
            {
                String key;
                key = item.ServiceItem.RemoconID.ToString("X2") + item.ServiceItem.ONID.ToString("X4") + item.ServiceItem.SID.ToString("X4");
                sort.Add(key, item);
            }
            listBox_on.Items.Clear();
            foreach (ViewItem item in sort.Values)
            {
                listBox_on.Items.Add(item);
            }
        }
    }

    class ViewItem
    {
        public ViewItem(ChSet5Item item, bool viewOn)
        {
            ServiceItem = item;
        }

        public ChSet5Item ServiceItem
        {
            get;
            set;
        }

        public bool ViewOn
        {
            get;
            set;
        }

        public UInt64 Key
        {
            get
            {
                UInt64 key = ((UInt64)ServiceItem.ONID) << 32 | ((UInt64)ServiceItem.TSID) << 16 | (UInt64)ServiceItem.SID;
                return key;
            }
        }

        public override string ToString()
        {
            if (ServiceItem != null)
            {
                return ServiceItem.ServiceName;
            }
            else
            {
                return "";
            }
        }

        public String ToolTipView
        {
            get
            {
                String viewTip = "";

                if (ServiceItem != null)
                {
                    viewTip =
                        "service_name : " + ServiceItem.ServiceName + "\r\n" +
                        "service_type : " + ServiceItem.ServiceType.ToString() + "(0x" + ServiceItem.ServiceType.ToString("X2") + ")" + "\r\n" +
                        "original_network_id : " + ServiceItem.ONID.ToString() + "(0x" + ServiceItem.ONID.ToString("X4") + ")" + "\r\n" +
                        "transport_stream_id : " + ServiceItem.TSID.ToString() + "(0x" + ServiceItem.TSID.ToString("X4") + ")" + "\r\n" +
                        "service_id : " + ServiceItem.SID.ToString() + "(0x" + ServiceItem.SID.ToString("X4") + ")" + "\r\n" +
                        "partial_reception : " + ServiceItem.PartialFlag.ToString();
                }
                return viewTip;
            }
        }
    }
}
