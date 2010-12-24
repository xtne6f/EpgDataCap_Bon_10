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
using System.Windows.Shapes;
using CtrlCmdCLI.Def;
using CtrlCmdCLI;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EpgTimer
{
    /// <summary>
    /// RecInfoDescWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class RecInfoDescWindow : Window
    {
        private RecFileInfo recInfo = null;
        private CtrlCmdUtil cmd = EpgTimerNW.NWConnect.Instance.cmd;

        public RecInfoDescWindow()
        {
            InitializeComponent();
        }

        public void SetRecInfo(RecFileInfo info)
        {
            recInfo = info;
            textBox_pgInfo.Text = info.ProgramInfo;
            textBox_errLog.Text = info.ErrInfo;
        }

        private void button_play_Click(object sender, RoutedEventArgs e)
        {
            if (recInfo != null)
            {
                if (recInfo.RecFilePath.Length > 0)
                {
                    try
                    {
                        //System.Diagnostics.Process.Start(recInfo.RecFilePath);
                        UInt32 ctrlID = 0;
                        if (cmd.SendNwPlayOpen(recInfo.RecFilePath, ref ctrlID) == 1)
                        {
/*                            String cmdLine = "";
                            String exePath = SettingPath.ModulePath + "\\fileplay.exe";
                            UInt32 ip = 0;
                            Int32 shift = 24;
                            foreach (string word in EpgTimerNW.NWConnect.Instance.ConnectedIP.Split('.'))
                            {
                                ip |= Convert.ToUInt32(word) << shift;
                                shift -= 8;
                            }

                            cmdLine += recInfo.RecFilePath + " /streaming /ctrlid " + ctrlID.ToString() + " /serverip " + ip.ToString() + " /serverport " + Settings.Instance.NWServerPort.ToString();
                            System.Diagnostics.Process.Start(exePath, cmdLine);*/

                            try
                            {
                                bool open = false;
                                CtrlCmdUtil tvTestCmd = new CtrlCmdUtil();
                                tvTestCmd.SetConnectTimeOut(15*1000);
                                foreach (Process p in Process.GetProcesses())
                                {
                                    if (String.Compare(p.ProcessName, "tvtest", true) == 0)
                                    {
                                        open = true;
                                        if (p.MainWindowHandle != IntPtr.Zero)
                                        {
                                            tvTestCmd.SetPipeSetting("Global\\TvTest_Ctrl_BonConnect_" + p.Id.ToString(), "\\\\.\\pipe\\TvTest_Ctrl_BonPipe_" + p.Id.ToString());
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
                                    tvTestCmd.SetPipeSetting("Global\\TvTest_Ctrl_BonConnect_" + process.Id.ToString(), "\\\\.\\pipe\\TvTest_Ctrl_BonPipe_" + process.Id.ToString());
                                }

                                UInt32 ip = 0;
                                Int32 shift = 24;
                                foreach (string word in EpgTimerNW.NWConnect.Instance.ConnectedIP.Split('.'))
                                {
                                    ip |= Convert.ToUInt32(word) << shift;
                                    shift -= 8;
                                }

                                TVTestStreamingInfo sendInfo = new TVTestStreamingInfo();
                                sendInfo.enableMode = 1;
                                sendInfo.ctrlID = ctrlID;
                                sendInfo.serverIP = ip;
                                sendInfo.serverPort = Settings.Instance.NWServerPort;
                                if (Settings.Instance.NwTvModeUDP == true){
                                    sendInfo.udpSend = 1;
                                }
                                if (Settings.Instance.NwTvModeTCP == true)
                                {
                                    sendInfo.tcpSend = 1;
                                }
                                if (tvTestCmd.SendViewSetStreamingInfo(sendInfo) != 1)
                                {
                                    System.Threading.Thread.Sleep(5*1000);
                                    tvTestCmd.SendViewSetStreamingInfo(sendInfo);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
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
