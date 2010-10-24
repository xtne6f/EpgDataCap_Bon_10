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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EpgTimer
{
    /// <summary>
    /// EpgServiceView.xaml の相互作用ロジック
    /// </summary>
    public partial class EpgServiceView : UserControl
    {
        private CtrlCmdUtil cmd = EpgTimerNW.NWConnect.Instance.cmd;

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
                item.MouseLeftButtonDown += new MouseButtonEventHandler(item_MouseLeftButtonDown);
                item.DataContext = info.ServiceInfo;
                stackPanel_service.Children.Add(item);
            }
        }

        void item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (sender.GetType() == typeof(TextBlock))
                {
                    TextBlock item = sender as TextBlock;
                    EpgServiceInfo serviceInfo = item.DataContext as EpgServiceInfo;
                    if (Settings.Instance.NwTvMode == true)
                    {
                        SetChInfo chInfo = new SetChInfo();
                        chInfo.useSID = 1;
                        chInfo.useBonCh = 0;
                        chInfo.ONID = serviceInfo.ONID;
                        chInfo.TSID = serviceInfo.TSID;
                        chInfo.SID = serviceInfo.SID;

                        UInt32 nwMode = 0;
                        if (Settings.Instance.NwTvModeUDP == true)
                        {
                            nwMode += 1;
                        }
                        if (Settings.Instance.NwTvModeTCP == true)
                        {
                            nwMode += 2;
                        }
                        if (cmd.SendNwTVMode(nwMode) == 1)
                        {
                            if (cmd.SendNwTVSetCh(chInfo) == 1)
                            {
                                try
                                {
                                    bool open = false;
                                    foreach (Process p in Process.GetProcesses())
                                    {
                                        if (String.Compare(p.ProcessName, "tvtest", true) == 0)
                                        {
                                            open = true;
                                            if (p.MainWindowHandle != IntPtr.Zero)
                                            {
                                                WakeupWindow(p.MainWindowHandle);
                                            }
                                        }
                                    }
                                    if (open == false)
                                    {
                                        System.Diagnostics.Process process;
                                        String cmdLine = "";
                                        cmdLine += Settings.Instance.TvTestCmd;
                                        if (cmdLine.IndexOf("/d") < 0)
                                        {
                                            if (Settings.Instance.TvTestCmd.Length > 0)
                                            {
                                                if (cmdLine.Length > 0)
                                                {
                                                    cmdLine += " ";
                                                }
                                            }
                                            if (Settings.Instance.NwTvModeUDP == true)
                                            {
                                                cmdLine = "/d BonDriver_UDP.dll";
                                            }
                                            else if (Settings.Instance.NwTvModeTCP)
                                            {
                                                cmdLine = "/d BonDriver_TCP.dll";
                                            }
                                        }
                                        process = System.Diagnostics.Process.Start(Settings.Instance.TvTestExe, cmdLine);

                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                    else
                    {
                        //ネットワークモード以外は非サポート
                    }
                }
            }
        }

        // 外部プロセスのウィンドウを起動する
        public static void WakeupWindow(IntPtr hWnd)
        {
            // メイン・ウィンドウが最小化されていれば元に戻す
            if (IsIconic(hWnd))
            {
                ShowWindowAsync(hWnd, SW_RESTORE);
            }

            // メイン・ウィンドウを最前面に表示する
            SetForegroundWindow(hWnd);
        }
        // 外部プロセスのメイン・ウィンドウを起動するためのWin32 API
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);
        // ShowWindowAsync関数のパラメータに渡す定義値
        private const int SW_RESTORE = 9;  // 画面を元の大きさに戻す
    }
}
