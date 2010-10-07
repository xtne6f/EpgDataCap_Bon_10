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

namespace EpgTimer
{
    /// <summary>
    /// EpgView.xaml の相互作用ロジック
    /// </summary>
    public partial class EpgView : UserControl
    {
        private CtrlCmdUtil cmd = EpgTimerDef.Instance.CtrlCmd;
        private Dictionary<UInt16, EpgNetworkView> networkViewList = new Dictionary<ushort, EpgNetworkView>();
        private List<ReserveItem> reserveList = null;
        public EpgView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (networkViewList.Count == 0)
            {
                if (ReloadEpgData() == true)
                {
                    label1.Visibility = System.Windows.Visibility.Hidden;
                    label2.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    label1.Visibility = System.Windows.Visibility.Visible;
                    label2.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                label1.Visibility = System.Windows.Visibility.Hidden;
                label2.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        public void SetReserveList(List<ReserveItem> reserveList)
        {
            if (networkViewList.Count != 0)
            {
                foreach (EpgNetworkView info in networkViewList.Values)
                {
                    info.SetReserveList(reserveList);
                }
            }
            else
            {
                this.reserveList = reserveList;
            }
        }

        public void ReDrawReserve()
        {
            foreach (EpgNetworkView info in networkViewList.Values)
            {
                info.ReDrawReserve();
            }
        }

        public void ReDrawEpgView()
        {
            foreach (EpgNetworkView info in networkViewList.Values)
            {
                info.DrawProgramView();
                info.ReDrawReserve();
            }
        }

        public bool ReloadEpgData()
        {
            foreach (EpgNetworkView info in networkViewList.Values)
            {
                info.ClearValue();
                info.epgProgramView.Clear();
            }
            int selectTab = tabControl.SelectedIndex;
//            networkViewList.Clear();
//            tabControl.Items.Clear();

            List<EpgServiceEventInfo> serviceEventList = new List<EpgServiceEventInfo>();
            uint err = cmd.SendEnumPgAll(ref serviceEventList);
            if (err == 1)
            {
                foreach (EpgServiceEventInfo info in serviceEventList)
                {
                    EpgNetworkView epgView = null;
                    if (0x7880 <= info.serviceInfo.ONID && info.serviceInfo.ONID <= 0x7FE8)
                    {
                        if (networkViewList.ContainsKey(0x7880) == false)
                        {
                            epgView = new EpgNetworkView();

                            TabItem tabItem = new TabItem();
                            tabItem.Header = "地デジ";
                            tabItem.Content = epgView;
                            tabControl.Items.Add(tabItem);
                            networkViewList.Add(0x7880, epgView);
                        }
                        else
                        {
                            epgView = networkViewList[0x7880];
                        }
                    }
                    else if (info.serviceInfo.ONID == 0x0004)
                    {
                        if (networkViewList.ContainsKey(0x0004) == false)
                        {
                            epgView = new EpgNetworkView();

                            TabItem tabItem = new TabItem();
                            tabItem.Header = "BS";
                            tabItem.Content = epgView;
                            tabControl.Items.Add(tabItem);
                            networkViewList.Add(0x0004, epgView);

                        }
                        else
                        {
                            epgView = networkViewList[0x0004];
                        }
                    }
                    else if (info.serviceInfo.ONID == 0x0006 || info.serviceInfo.ONID == 0x0007)
                    {
                        if (networkViewList.ContainsKey(0x0006) == false)
                        {
                            epgView = new EpgNetworkView();

                            TabItem tabItem = new TabItem();
                            tabItem.Header = "CS";
                            tabItem.Content = epgView;
                            tabControl.Items.Add(tabItem);
                            networkViewList.Add(0x0006, epgView);
                        }
                        else
                        {
                            epgView = networkViewList[0x0006];
                        }
                    }
                    else
                    {
                        if (networkViewList.ContainsKey(0xFFFF) == false)
                        {
                            epgView = new EpgNetworkView();

                            TabItem tabItem = new TabItem();
                            tabItem.Header = "その他";
                            tabItem.Content = epgView;
                            tabControl.Items.Add(tabItem);
                            networkViewList.Add(0xFFFF, epgView);
                        }
                        else
                        {
                            epgView = networkViewList[0xFFFF];
                        }
                    }

                    if (networkViewList.Count == 0)
                    {
                        label1.Visibility = System.Windows.Visibility.Visible;
                        label2.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        label1.Visibility = System.Windows.Visibility.Hidden;
                        label2.Visibility = System.Windows.Visibility.Hidden;
                    }

                    if (epgView != null)
                    {
                        epgView.AddProgramList(info.serviceInfo, info.eventList);
                    }
                }

                foreach (EpgNetworkView info in networkViewList.Values)
                {
                    info.DrawProgramView();
                    if (this.reserveList != null)
                    {
                        info.SetReserveList(this.reserveList);
                    }
                }
                this.reserveList = null;
            }

            /*
            List<EpgServiceInfo> serviceList = new List<EpgServiceInfo>();
            uint err = cmd.SendEnumService(ref serviceList);
            if (err == 1)
            {
                foreach (EpgServiceInfo info in serviceList)
                {
                    EpgNetworkView epgView = null;
                    if (0x7880 <= info.ONID && info.ONID <= 0x7FE8)
                    {
                        if (networkViewList.ContainsKey(0x7880) == false)
                        {
                            epgView = new EpgNetworkView();

                            TabItem tabItem = new TabItem();
                            tabItem.Header = "地デジ";
                            tabItem.Content = epgView;
                            tabControl.Items.Add(tabItem);
                            networkViewList.Add(0x7880, epgView);
                        }
                        else
                        {
                            epgView = networkViewList[0x7880];
                        }
                    }
                    else if (info.ONID == 0x0004)
                    {
                        if (networkViewList.ContainsKey(0x0004) == false)
                        {
                            epgView = new EpgNetworkView();

                            TabItem tabItem = new TabItem();
                            tabItem.Header = "BS";
                            tabItem.Content = epgView;
                            tabControl.Items.Add(tabItem);
                            networkViewList.Add(0x0004, epgView);

                        }
                        else
                        {
                            epgView = networkViewList[0x0004];
                        }
                    }
                    else if (info.ONID == 0x0006 || info.ONID == 0x0007)
                    {
                        if (networkViewList.ContainsKey(0x0006) == false)
                        {
                            epgView = new EpgNetworkView();

                            TabItem tabItem = new TabItem();
                            tabItem.Header = "CS";
                            tabItem.Content = epgView;
                            tabControl.Items.Add(tabItem);
                            networkViewList.Add(0x0006, epgView);
                        }
                        else
                        {
                            epgView = networkViewList[0x0006];
                        }
                    }
                    else
                    {
                        if (networkViewList.ContainsKey(0xFFFF) == false)
                        {
                            epgView = new EpgNetworkView();

                            TabItem tabItem = new TabItem();
                            tabItem.Header = "その他";
                            tabItem.Content = epgView;
                            tabControl.Items.Add(tabItem);
                            networkViewList.Add(0xFFFF, epgView);
                        }
                        else
                        {
                            epgView = networkViewList[0xFFFF];
                        }
                    }

                    if (networkViewList.Count == 0)
                    {
                        label1.Visibility = System.Windows.Visibility.Visible;
                        label2.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        label1.Visibility = System.Windows.Visibility.Hidden;
                        label2.Visibility = System.Windows.Visibility.Hidden;
                    }

                    if (epgView != null)
                    {
                        UInt64 key = ((UInt64)info.ONID) << 32 | ((UInt64)info.TSID) << 16 | (UInt64)info.SID;
                        List<EpgEventInfo> eventList = new List<EpgEventInfo>();
                        err = cmd.SendEnumPgInfo(key, ref eventList);
                        if (err == 1)
                        {
                            epgView.AddProgramList(info, eventList);
                        }
                    }
                }

                foreach (EpgNetworkView info in networkViewList.Values)
                {
                    info.DrawProgramView();
                    if (this.reserveList != null)
                    {
                        info.SetReserveList(this.reserveList);
                    }
                }
                this.reserveList = null;
            }
            else
            {
                return false;
            }
            */

            if (tabControl.Items.Count > 0 )
            {
                if (selectTab >= 0)
                {
                    tabControl.SelectedIndex = selectTab;
                    TabItem tabItem = tabControl.SelectedItem as TabItem;
                    EpgNetworkView view = tabItem.Content as EpgNetworkView;
                    view.MoveNowTime();
                }
                else
                {
                    tabControl.SelectedIndex = 0;
                    foreach (TabItem tabItem in tabControl.Items)
                    {
                        EpgNetworkView view = tabItem.Content as EpgNetworkView;
                        view.MoveNowTime();
                    }
                }
            }
            serviceEventList.Clear();
            return true;
        }
    }
}
