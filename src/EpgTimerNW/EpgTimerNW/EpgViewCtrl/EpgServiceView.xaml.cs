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
    /// EpgServiceView.xaml の相互作用ロジック
    /// </summary>
    public partial class EpgServiceView : UserControl
    {
        public EpgServiceView()
        {
            InitializeComponent();
        }

        public void SetService(Dictionary<UInt64, EventListInfo> eventDataList)
        {
            stackPanel_service.Children.Clear();
            foreach (EventListInfo info in eventDataList.Values)
            {
                TextBlock item = new TextBlock();
                item.Text = info.ServiceInfo.service_name;
                if (info.ServiceInfo.remote_control_key_id != 0)
                {
                    item.Text += "\r\n" + info.ServiceInfo.remote_control_key_id.ToString();
                }
                else
                {
                    item.Text += "\r\n" + info.ServiceInfo.network_name + " " + info.ServiceInfo.SID.ToString();
                }
                item.Width = Settings.Instance.ServiceWidth-4;
                item.Margin = new Thickness(2, 2, 2, 2);
                item.Background = Brushes.AliceBlue;
                item.TextAlignment = TextAlignment.Center;
                item.FontSize = 12;
                stackPanel_service.Children.Add(item);
            }
        }
    }
}
