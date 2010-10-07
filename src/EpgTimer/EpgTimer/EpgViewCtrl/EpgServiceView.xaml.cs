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
        CtrlCmdUtil cmd = new CtrlCmdUtil();

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

                    UInt64 key = ((UInt64)serviceInfo.ONID) << 32 |
                        ((UInt64)serviceInfo.TSID) << 16 |
                        ((UInt64)serviceInfo.SID);
                    TvTestChChgInfo chInfo = new TvTestChChgInfo();
                    if (EpgTimerDef.Instance.CtrlCmd.SendGetChgChTVTest(key, ref chInfo) == 1)
                    {
                        bool send = false;
                        foreach (Process p in Process.GetProcesses())
                        {
                            if (String.Compare(p.ProcessName, "tvtest", true) == 0)
                            {
                                cmd.SetPipeSetting("Global\\TvTest_Ctrl_BonConnect_" + p.Id.ToString(), "\\\\.\\pipe\\TvTest_Ctrl_BonPipe_" + p.Id.ToString());
                                cmd.SetConnectTimeOut(1000);
                                String val = "";
                                if (cmd.SendViewGetBonDrivere(ref val) == 1)
                                {
                                    if (String.Compare(val, chInfo.bonDriver, true) != 0)
                                    {
                                        if (cmd.SendViewSetBonDrivere(chInfo.bonDriver) == 1)
                                        {
                                            cmd.SendViewSetCh(chInfo.chInfo);
                                        }
                                    }
                                    else
                                    {
                                        cmd.SendViewSetCh(chInfo.chInfo);
                                    }

                                    if (p.MainWindowHandle != IntPtr.Zero)
                                    {
                                        WakeupWindow(p.MainWindowHandle);
                                    }

                                    send = true;
                                    break;
                                }
                            }
                        }
                        if (send == false)
                        {
                            try
                            {
                                System.Diagnostics.Process process;
                                String cmdLine = "/d " + chInfo.bonDriver + " /chspace " + chInfo.chInfo.space.ToString();
                                if (Settings.Instance.TvTestCmd.Length > 0)
                                {
                                    cmdLine += " " + Settings.Instance.TvTestCmd;
                                }
                                process = System.Diagnostics.Process.Start(Settings.Instance.TvTestExe, cmdLine);
                                System.Threading.Thread.Sleep(1000);

                                cmd.SetPipeSetting("Global\\TvTest_Ctrl_BonConnect_" + process.Id.ToString(), "\\\\.\\pipe\\TvTest_Ctrl_BonPipe_" + process.Id.ToString());
                                cmd.SetConnectTimeOut(1000);
                                int count = 0;
                                while (count < 10)
                                {
                                    if (cmd.SendViewSetCh(chInfo.chInfo) == 1)
                                    {
                                        break;
                                    }
                                    System.Threading.Thread.Sleep(100);
                                    count++;
                                }
                            }
                            catch
                            {
                            }
                        }
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
