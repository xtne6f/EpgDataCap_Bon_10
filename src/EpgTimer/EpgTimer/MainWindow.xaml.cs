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
using System.Windows.Threading;
using Common;
using CtrlCmdCLI;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EpgTimer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Threading.Mutex mutex;
        private TaskTrayClass taskTray = null;
        private bool serviceMode = false;
        private Dictionary<string, Button> buttonList = new Dictionary<string,Button>();
        private PipeServer pipeServer = null;

        private CtrlCmdUtil cmd = EpgTimerDef.Instance.CtrlCmd;
        private string pipeName = "\\\\.\\pipe\\EpgTimerGUI_Ctrl_BonPipe_";
        private string pipeEventName = "Global\\EpgTimerGUI_Ctrl_BonConnect_";
        private bool closeFlag = false;
        private bool initExe = false;
        public MainWindow()
        {
            ChSet5.LoadFile();
            Settings.LoadFromXmlFile();

            mutex = new System.Threading.Mutex(false, "Global\\EpgTimer_Bon2");
            if (!mutex.WaitOne(0, false))
            {
                CheckCmdLine();

                mutex.Close();
                mutex = null;

                closeFlag = true;
                Close();
                return;
            }

            bool startExe = false;
            try
            {
                if (ServiceCtrlClass.ServiceIsInstalled("EpgTimer Service") == true)
                {
                    if (ServiceCtrlClass.IsStarted("EpgTimer Service") == false)
                    {
                        bool check = false;
                        for( int i=0; i<15; i++ ){
                            if (ServiceCtrlClass.StartService("EpgTimer Service") == true)
                            {
                                check = true;
                            }
                            System.Threading.Thread.Sleep(1000);
                            if (ServiceCtrlClass.IsStarted("EpgTimer Service") == true)
                            {
                                check = true;
                            }
                        }
                        if (check == false)
                        {
                            MessageBox.Show("サービスの開始に失敗しました。\r\nVista以降のOSでは、管理者権限で起動されている必要があります。");
                            closeFlag = true;
                            Close();
                            return;
                        }
                        else
                        {
                            serviceMode = true;
                            startExe = true;
                        }
                    }
                    else
                    {
                        serviceMode = true;
                        startExe = true;
                    }
                }
            }
            catch
            {
                serviceMode = false;
            }

            try
            {
                if (serviceMode == false)
                {
                    String moduleFolder = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                    String exePath = moduleFolder + "\\EpgTimerSrv.exe";
                    System.Diagnostics.Process process = System.Diagnostics.Process.Start(exePath);
                    startExe = true;
                }
            }
            catch
            {
                startExe = false;
            }
            
            if (startExe == false)
            {
                MessageBox.Show("EpgTimerSrv.exeの起動ができませんでした");
                closeFlag = true;
                Close();
                return;
            }
            
            InitializeComponent();

            initExe = true;

            try
            {
                if (Settings.Instance.WakeMin == true)
                {
                    this.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            catch
            {
            }

            //タスクトレイの表示
            taskTray = new TaskTrayClass(this);
            taskTray.Icon = Properties.Resources.TaskIconBlue;
            taskTray.Visible = true;
            taskTray.ContextMenuClick += new EventHandler(taskTray_ContextMenuClick);

            //上のボタン
            Button settingButton = new Button();
            settingButton.MinWidth = 75;
            settingButton.Margin = new Thickness(2,2,2,15);
            settingButton.Click += new RoutedEventHandler(settingButton_Click);
            settingButton.Content = "設定";
            buttonList.Add("設定", settingButton);

            Button searchButton = new Button();
            searchButton.MinWidth = 75;
            searchButton.Margin = new Thickness(2, 2, 2, 15);
            searchButton.Click += new RoutedEventHandler(searchButton_Click);
            searchButton.Content = "検索";
            buttonList.Add("検索", searchButton);

            Button closeButton = new Button();
            closeButton.MinWidth = 75;
            closeButton.Margin = new Thickness(2, 2, 2, 15);
            closeButton.Click += new RoutedEventHandler(closeButton_Click);
            closeButton.Content = "終了";
            buttonList.Add("終了", closeButton);

            Button stanbyButton = new Button();
            stanbyButton.MinWidth = 75;
            stanbyButton.Margin = new Thickness(2, 2, 2, 15);
            stanbyButton.Click += new RoutedEventHandler(standbyButton_Click);
            stanbyButton.Content = "スタンバイ";
            buttonList.Add("スタンバイ", stanbyButton);

            Button suspendButton = new Button();
            suspendButton.MinWidth = 75;
            suspendButton.Margin = new Thickness(2, 2, 2, 15);
            suspendButton.Click += new RoutedEventHandler(suspendButton_Click);
            suspendButton.Content = "休止";
            buttonList.Add("休止", suspendButton);

            Button epgCapButton = new Button();
            epgCapButton.MinWidth = 75;
            epgCapButton.Margin = new Thickness(2, 2, 2, 15);
            epgCapButton.Click += new RoutedEventHandler(epgCapButton_Click);
            epgCapButton.Content = "EPG取得";
            buttonList.Add("EPG取得", epgCapButton);

            Button epgReloadButton = new Button();
            epgReloadButton.MinWidth = 75;
            epgReloadButton.Margin = new Thickness(2, 2, 2, 15);
            epgReloadButton.Click += new RoutedEventHandler(epgReloadButton_Click);
            epgReloadButton.Content = "EPG再読み込み";
            buttonList.Add("EPG再読み込み", epgReloadButton);

            Button custum1Button = new Button();
            custum1Button.MinWidth = 75;
            custum1Button.Margin = new Thickness(2, 2, 2, 15);
            custum1Button.Click += new RoutedEventHandler(custum1Button_Click);
            custum1Button.Content = "カスタム１";
            buttonList.Add("カスタム１", custum1Button);

            Button custum2Button = new Button();
            custum2Button.MinWidth = 75;
            custum2Button.Margin = new Thickness(2, 2, 2, 15);
            custum2Button.Click += new RoutedEventHandler(custum2Button_Click);
            custum2Button.Content = "カスタム２";
            buttonList.Add("カスタム２", custum2Button);

            Button nwTVEndButton = new Button();
            nwTVEndButton.MinWidth = 75;
            nwTVEndButton.Margin = new Thickness(2, 2, 2, 15);
            nwTVEndButton.Click += new RoutedEventHandler(nwTVEndButton_Click);
            nwTVEndButton.Content = "NetworkTV終了";
            buttonList.Add("NetworkTV終了", nwTVEndButton);

            ResetButtonView();

            ResetTaskMenu();

            //ウインドウ位置の復元
            if (Settings.Instance.MainWndTop != 0)
            {
                this.Top = Settings.Instance.MainWndTop;
            }
            if (Settings.Instance.MainWndLeft != 0)
            {
                this.Left = Settings.Instance.MainWndLeft;
            }
            if (Settings.Instance.MainWndWidth != 0)
            {
                this.Width = Settings.Instance.MainWndWidth;
            }
            if (Settings.Instance.MainWndHeight != 0)
            {
                this.Height = Settings.Instance.MainWndHeight;
            }
            this.WindowState = Settings.Instance.LastWindowState;

            pipeServer = new PipeServer();
            pipeName += System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
            pipeEventName += System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
            pipeServer.StartServer(pipeEventName, pipeName, OutsideCmdCallback, this);
            try
            {
                List<ReserveItem> reserveList = new List<ReserveItem>();
                reserveView.ReloadReserve();
                reserveView.GetReserveList(ref reserveList);
                epgView.SetReserveList(reserveList);
                List<CtrlCmdCLI.Def.TunerReserveInfo> tunerReserveList = new List<CtrlCmdCLI.Def.TunerReserveInfo>();
                cmd.SendEnumTunerReserve(ref tunerReserveList);
                tunerReserveView.SetReserveInfo(tunerReserveList, reserveList);

                CtrlCmdCLI.Def.ReserveData item = new CtrlCmdCLI.Def.ReserveData();
                if (reserveView.GetNextReserve(ref item) == true)
                {
                    String timeView = item.StartTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss ～ ");
                    DateTime endTime = item.StartTime + TimeSpan.FromSeconds(item.DurationSecond);
                    timeView += endTime.ToString("HH:mm:ss");
                    taskTray.Text = "次の予約：" + item.StationName + " " + timeView + " " + item.Title;
                }
                else
                {
                    taskTray.Text = "次の予約なし";
                }
                cmd.SendRegistGUI((uint)System.Diagnostics.Process.GetCurrentProcess().Id);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }

        }

        private void CheckCmdLine()
        {
            foreach (string arg in Environment.GetCommandLineArgs())
            {
                String ext = System.IO.Path.GetExtension(arg);
                if (string.Compare(ext, ".exe", true) == 0)
                {
                    //何もしない
                }
                else if (string.Compare(ext, ".eaa", true) == 0)
                {
                    //自動予約登録条件追加
                    EAAFileClass eaaFile = new EAAFileClass();
                    if (eaaFile.LoadEAAFile(arg) == true)
                    {
                        List<CtrlCmdCLI.Def.EpgAutoAddData> val = new List<CtrlCmdCLI.Def.EpgAutoAddData>();
                        val.Add(eaaFile.AddKey);
                        cmd.SendAddEpgAutoAdd(val);
                    }
                    else
                    {
                        MessageBox.Show("解析に失敗しました。");
                    }
                }
                else if (string.Compare(ext, ".tvpid", true) == 0 || string.Compare(ext, ".tvpio", true) == 0)
                {
                    //iEPG追加
                    IEPGFileClass iepgFile = new IEPGFileClass();
                    if (iepgFile.LoadTVPIDFile(arg) == true)
                    {
                        List<CtrlCmdCLI.Def.ReserveData> val = new List<CtrlCmdCLI.Def.ReserveData>();
                        val.Add(iepgFile.AddInfo);
                        cmd.SendAddReserve(val);
                    }
                    else
                    {
                        MessageBox.Show("解析に失敗しました。デジタル用Version 2のiEPGの必要があります。");
                    }
                }
                else if (string.Compare(ext, ".tvpi", true) == 0)
                {
                    //iEPG追加
                    IEPGFileClass iepgFile = new IEPGFileClass();
                    if (iepgFile.LoadTVPIFile(arg) == true)
                    {
                        List<CtrlCmdCLI.Def.ReserveData> val = new List<CtrlCmdCLI.Def.ReserveData>();
                        val.Add(iepgFile.AddInfo);
                        cmd.SendAddReserve(val);
                    }
                    else
                    {
                        MessageBox.Show("解析に失敗しました。放送局名がサービスに関連づけされていない可能性があります。");
                    }
                }
            }
        }

        void epgReloadButton_Click(object sender, RoutedEventArgs e)
        {
            EpgReloadCmd();
        }

        void EpgReloadCmd()
        {
            if (cmd.SendReloadEpg() != 1)
            {
                MessageBox.Show("EPG再読み込みを行える状態ではありません。\r\n（EPGデータ読み込み中。など）");
            }
        }

        void epgCapButton_Click(object sender, RoutedEventArgs e)
        {
            EpgCapCmd();
        }

        void EpgCapCmd()
        {
            if (cmd.SendEpgCapNow() != 1)
            {
                MessageBox.Show("EPG取得を行える状態ではありません。\r\n（もうすぐ予約が始まる。EPGデータ読み込み中。など）");
            }
        }

        void suspendButton_Click(object sender, RoutedEventArgs e)
        {
            SuspendCmd();
        }

        void SuspendCmd()
        {
            if (cmd.SendChkSuspend() != 1)
            {
                MessageBox.Show("休止に移行できる状態ではありません。\r\n（もうすぐ予約が始まる。または抑制条件のexeが起動している。など）");
            }
            else
            {
                if (IniFileHandler.GetPrivateProfileInt("SET", "Reboot", 0, SettingPath.TimerSrvIniPath) == 1)
                {
                    cmd.SendSuspend(0x0102);
                }
                else
                {
                    cmd.SendSuspend(2);
                }
            }
        }

        void standbyButton_Click(object sender, RoutedEventArgs e)
        {
            StandbyCmd();
        }

        void StandbyCmd()
        {
            if (cmd.SendChkSuspend() != 1)
            {
                MessageBox.Show("スタンバイに移行できる状態ではありません。\r\n（もうすぐ予約が始まる。または抑制条件のexeが起動している。など）");
            }
            else
            {
                if (IniFileHandler.GetPrivateProfileInt("SET", "Reboot", 0, SettingPath.TimerSrvIniPath) == 1)
                {
                    cmd.SendSuspend(0x0101);
                }
                else
                {
                    cmd.SendSuspend(1);
                }
            }
        }

        void closeButton_Click(object sender, RoutedEventArgs e)
        {
            CloseCmd();
        }

        void CloseCmd()
        {
            closeFlag = true;
            Close();
        }

        void searchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchCmd();
        }

        void SearchCmd()
        {
            SearchWindow search = new SearchWindow();
            search.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
            search.ShowDialog();
        }

        void settingButton_Click(object sender, RoutedEventArgs e)
        {
            SettingCmd();
        }

        void SettingCmd()
        {
            SettingWindow setting = new SettingWindow();
            setting.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
            if (setting.ShowDialog() == true)
            {
                if (setting.ServiceStop == false)
                {
                    epgView.ReDrawEpgView();
                    cmd.SendReloadSetting();
                    ResetButtonView();
                    ResetTaskMenu();
                }
            }
            if (setting.ServiceStop == true)
            {
                MessageBox.Show("サービスの状態を変更したため終了します。");
                initExe = false;
                closeFlag = true;
                Close();
                return;
            }
            ChSet5.LoadFile();
        }

        void custum1Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Settings.Instance.Cust1BtnCmd, Settings.Instance.Cust1BtnCmdOpt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void custum2Button_Click(object sender, RoutedEventArgs e)
        {
            try{
                System.Diagnostics.Process.Start(Settings.Instance.Cust2BtnCmd, Settings.Instance.Cust2BtnCmdOpt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void nwTVEndButton_Click(object sender, RoutedEventArgs e)
        {
            EpgTimerDef.Instance.CtrlCmd.SendNwTVClose();
            CtrlCmdUtil cmdTvTest = new CtrlCmdUtil();
            foreach (Process p in Process.GetProcesses())
            {
                if (String.Compare(p.ProcessName, "tvtest", true) == 0)
                {
                    cmdTvTest.SetPipeSetting("Global\\TvTest_Ctrl_BonConnect_" + p.Id.ToString(), "\\\\.\\pipe\\TvTest_Ctrl_BonPipe_" + p.Id.ToString());
                    cmdTvTest.SetConnectTimeOut(1000);
                    String val = "";
                    if (cmdTvTest.SendViewGetBonDrivere(ref val) == 1)
                    {
                        if (String.Compare(val, "BonDriver_UDP.dll", true) == 0)
                        {
                            cmdTvTest.SendViewAppClose();
                            break;
                        }
                        else if (String.Compare(val, "BonDriver_TCP.dll", true) == 0)
                        {
                            cmdTvTest.SendViewAppClose();
                            break;
                        }
                    }
                }
            }

        }
        
        /// <summary>
        /// タスクトレイの右クリックメニューが押された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void taskTray_ContextMenuClick(object sender, EventArgs e)
        {
            String tag = sender.ToString();
            if (String.Compare("設定", tag) == 0)
            {
                SettingCmd();
            }
            else if (String.Compare("終了", tag) == 0)
            {
                CloseCmd();
            }
            else if (String.Compare("スタンバイ", tag) == 0)
            {
                StandbyCmd();
            }
            else if (String.Compare("休止", tag) == 0)
            {
                SuspendCmd();
            }
            else if (String.Compare("EPG取得", tag) == 0)
            {
                EpgCapCmd();
            }
        }

        /// <summary>
        /// ウインドウの状態が変更された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Visibility = System.Windows.Visibility.Hidden;
            }
            if (this.WindowState == WindowState.Normal || this.WindowState == WindowState.Maximized)
            {
                taskTray.LastViewState = this.WindowState;
                Settings.Instance.LastWindowState = this.WindowState;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                if (this.Visibility == System.Windows.Visibility.Visible && this.Width > 0 && this.Height > 0)
                {
                    Settings.Instance.MainWndWidth = this.Width;
                    Settings.Instance.MainWndHeight = this.Height;
                }
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                if (this.Visibility == System.Windows.Visibility.Visible && this.Top > 0 && this.Left > 0)
                {
                    Settings.Instance.MainWndTop = this.Top;
                    Settings.Instance.MainWndLeft = this.Left;
                }
            }
        }

        private void ResetTaskMenu()
        {
            List<Object> addList = new List<object>();
            foreach (String info in Settings.Instance.TaskMenuList)
            {
                if (String.Compare(info, "（セパレータ）") == 0)
                {
                    addList.Add("");
                }
                else
                {
                    addList.Add(info);
                }
            }
            taskTray.SetContextMenu(addList);
        }

        private void ResetButtonView()
        {
            stackPanel_button.Children.Clear();
            foreach (string info in Settings.Instance.ViewButtonList)
            {
                if (String.Compare(info, "（空白）") == 0)
                {
                    Label space = new Label();
                    space.Width = 15;
                    stackPanel_button.Children.Add(space);
                }
                else
                {
                    if (buttonList.ContainsKey(info) == true)
                    {
                        if (String.Compare(info, "カスタム１") == 0)
                        {
                            buttonList[info].Content = Settings.Instance.Cust1BtnName;
                        }
                        if (String.Compare(info, "カスタム２") == 0)
                        {
                            buttonList[info].Content = Settings.Instance.Cust2BtnName;
                        }
                        stackPanel_button.Children.Add(buttonList[info]);
                    }
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Settings.Instance.CloseMin == true && closeFlag == false)
            {
                e.Cancel = true;
                WindowState = System.Windows.WindowState.Minimized;
            }
            else
            {
                if (initExe == true)
                {
                    reserveView.SaveSize();
                    recInfoView.SaveSize();
                    cmd.SetConnectTimeOut(3000);
                    cmd.SendUnRegistGUI((uint)System.Diagnostics.Process.GetCurrentProcess().Id);
                    Settings.SaveToXmlFile();
                }
                pipeServer.StopServer();

                if (mutex != null)
                {
                    if (serviceMode == false && initExe == true)
                    {
                        cmd.SendClose();
                    }
                    mutex.ReleaseMutex();
                    mutex.Close();
                }
            }
        }

        private int OutsideCmdCallback(object pParam, CMD_STREAM pCmdParam, ref CMD_STREAM pResParam)
        {
            System.Diagnostics.Trace.WriteLine((CtrlCmd)pCmdParam.uiParam);
            switch ((CtrlCmd)pCmdParam.uiParam)
            {
                case CtrlCmd.CMD_TIMER_GUI_SHOW_DLG:
                    {
                        pResParam.uiParam = (uint)ErrCode.CMD_SUCCESS;
                        this.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
                case CtrlCmd.CMD_TIMER_GUI_UPDATE_RESERVE:
                    {
                        pResParam.uiParam = (uint)ErrCode.CMD_SUCCESS;
                        if (Dispatcher.CheckAccess() == true)
                        {
                            reserveView.ReloadReserve();
                            List<ReserveItem> reserveList = new List<ReserveItem>();
                            reserveView.GetReserveList(ref reserveList);
                            epgView.SetReserveList(reserveList);
                            recInfoView.ReloadRecInfo();
                            autoAddView.ReloadData();

                            List<CtrlCmdCLI.Def.TunerReserveInfo> tunerReserveList = new List<CtrlCmdCLI.Def.TunerReserveInfo>();
                            cmd.SendEnumTunerReserve(ref tunerReserveList);
                            tunerReserveView.SetReserveInfo(tunerReserveList, reserveList);

                            CtrlCmdCLI.Def.ReserveData item = new CtrlCmdCLI.Def.ReserveData();
                            if (reserveView.GetNextReserve(ref item) == true)
                            {
                                String timeView = item.StartTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss ～ ");
                                DateTime endTime = item.StartTime + TimeSpan.FromSeconds(item.DurationSecond);
                                timeView += endTime.ToString("HH:mm:ss");
                                taskTray.Text = "次の予約：" + item.StationName + " " + timeView + " " + item.Title;
                            }
                            else
                            {
                                taskTray.Text = "次の予約なし";
                            }
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                reserveView.ReloadReserve();
                                List<ReserveItem> reserveList = new List<ReserveItem>();
                                reserveView.GetReserveList(ref reserveList);
                                epgView.SetReserveList(reserveList);
                                recInfoView.ReloadRecInfo();
                                autoAddView.ReloadData();

                                List<CtrlCmdCLI.Def.TunerReserveInfo> tunerReserveList = new List<CtrlCmdCLI.Def.TunerReserveInfo>();
                                cmd.SendEnumTunerReserve(ref tunerReserveList);
                                tunerReserveView.SetReserveInfo(tunerReserveList, reserveList);

                                CtrlCmdCLI.Def.ReserveData item = new CtrlCmdCLI.Def.ReserveData();
                                if (reserveView.GetNextReserve(ref item) == true)
                                {
                                    String timeView = item.StartTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss ～ ");
                                    DateTime endTime = item.StartTime + TimeSpan.FromSeconds(item.DurationSecond);
                                    timeView += endTime.ToString("HH:mm:ss");
                                    taskTray.Text = "次の予約：" + item.StationName + " " + timeView + " " + item.Title;
                                }
                                else
                                {
                                    taskTray.Text = "次の予約なし";
                                }
                            }));
                        }
                    }
                    break;
                case CtrlCmd.CMD_TIMER_GUI_UPDATE_EPGDATA:
                    {
                        pResParam.uiParam = (uint)ErrCode.CMD_SUCCESS;
                        if (Dispatcher.CheckAccess() == true)
                        {
                            epgView.ReloadEpgData();
                            epgView.ReDrawReserve();
                            GC.Collect();
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                epgView.ReloadEpgData();
                                epgView.ReDrawReserve();
                                GC.Collect();
                            }));
                        }
                    }
                    break;
                case CtrlCmd.CMD_TIMER_GUI_VIEW_EXECUTE:
                    {
                        pResParam.uiParam = (uint)ErrCode.CMD_SUCCESS;
                        String exeCmd = "";
                        ReadStreamData(ref exeCmd, pCmdParam);
                        try
                        {
                            string[] cmd = exeCmd.Split('\"');
                            System.Diagnostics.Process process;
                            if (cmd.Length >= 3)
                            {
                                process = System.Diagnostics.Process.Start(cmd[1], cmd[2]);
                            }
                            else if (cmd.Length >= 2)
                            {
                                process = System.Diagnostics.Process.Start(cmd[1]);
                            }
                            else
                            {
                                process = System.Diagnostics.Process.Start(cmd[0]);
                            }
                            CreateStreamData(process.Id, ref pResParam);
                        }
                        catch
                        {
                        }
                    }
                    break;
                case CtrlCmd.CMD_TIMER_GUI_QUERY_SUSPEND:
                    {
                        pResParam.uiParam = (uint)ErrCode.CMD_SUCCESS;

                        UInt16 param = 0;
                        ReadStreamData(ref param, pCmdParam);

                        Byte reboot = (Byte)((param & 0xFF00) >> 8);
                        Byte suspendMode = (Byte)(param & 0x00FF);

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            SuspendCheckWindow dlg = new SuspendCheckWindow();
                            dlg.SetMode(0, suspendMode);
                            if (dlg.ShowDialog() != true)
                            {
                                cmd.SendSuspend(param);
                            }
                        }));
                    }
                    break;
                case CtrlCmd.CMD_TIMER_GUI_QUERY_REBOOT:
                    {
                        pResParam.uiParam = (uint)ErrCode.CMD_SUCCESS;

                        UInt16 param = 0;
                        ReadStreamData(ref param, pCmdParam);

                        Byte reboot = (Byte)((param & 0xFF00) >> 8);
                        Byte suspendMode = (Byte)(param & 0x00FF);

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            SuspendCheckWindow dlg = new SuspendCheckWindow();
                            dlg.SetMode(reboot, suspendMode);
                            if (dlg.ShowDialog() != true)
                            {
                                cmd.SendReboot();
                            }
                        }));
                    }
                    break;
                case CtrlCmd.CMD_TIMER_GUI_SRV_STATUS_CHG:
                    {
                        pResParam.uiParam = (uint)ErrCode.CMD_SUCCESS;
                        UInt16 status = 0;
                        ReadStreamData(ref status, pCmdParam);

                        if (Dispatcher.CheckAccess() == true)
                        {
                            if (status == 1)
                            {
                                taskTray.Icon = Properties.Resources.TaskIconRed;
                            }
                            else if (status == 2)
                            {
                                taskTray.Icon = Properties.Resources.TaskIconGreen;
                            }
                            else
                            {
                                taskTray.Icon = Properties.Resources.TaskIconBlue;
                            }
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                if (status == 1)
                                {
                                    taskTray.Icon = Properties.Resources.TaskIconRed;
                                }
                                else if (status == 2)
                                {
                                    taskTray.Icon = Properties.Resources.TaskIconGreen;
                                }
                                else
                                {
                                    taskTray.Icon = Properties.Resources.TaskIconBlue;
                                }
                            }));
                        }
                    }
                    break;
                default:
                    pResParam.uiParam = (uint)ErrCode.CMD_NON_SUPPORT;
                    break;
            }
            return 0;
        }

        public static bool ReadStreamData(ref String value, CMD_STREAM cmd)
        {
            int iPos = 0;
            int iStrSize = 0;
            Encoding uniEnc = Encoding.GetEncoding("unicode");

            if (cmd.uiSize != cmd.bData.Length)
            {
                return false;
            }

            iStrSize = BitConverter.ToInt32(cmd.bData, iPos);
            iPos += sizeof(int);
            iStrSize -= sizeof(int) + 2;
            
            value = uniEnc.GetString(cmd.bData, iPos, iStrSize);
            iPos += iStrSize;

            return true;
        }

        public static bool ReadStreamData(ref UInt16 value, CMD_STREAM cmd)
        {
            int iPos = 0;

            if (cmd.uiSize != cmd.bData.Length)
            {
                return false;
            }

            value = BitConverter.ToUInt16(cmd.bData, iPos);
            iPos += sizeof(uint);

            return true;
        }

        public static bool CreateStreamData(int value, ref CMD_STREAM cmd)
        {
            cmd.uiSize = sizeof(uint);
            cmd.bData = new byte[cmd.uiSize];

            int iPos = 0;

            Array.Copy(BitConverter.GetBytes(value), 0, cmd.bData, iPos, sizeof(uint));
            iPos += sizeof(uint);

            return true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void Window_PreviewDrop(object sender, DragEventArgs e)
        {
            string[] filePath = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            foreach (string path in filePath)
            {
                String ext = System.IO.Path.GetExtension(path);
                if (string.Compare(ext, ".eaa", true) == 0)
                {
                    //自動予約登録条件追加
                    EAAFileClass eaaFile = new EAAFileClass();
                    if (eaaFile.LoadEAAFile(path) == true)
                    {
                        List<CtrlCmdCLI.Def.EpgAutoAddData> val = new List<CtrlCmdCLI.Def.EpgAutoAddData>();
                        val.Add(eaaFile.AddKey);
                        cmd.SendAddEpgAutoAdd(val);
                    }
                    else
                    {
                        MessageBox.Show("解析に失敗しました。");
                    }
                }
                else if (string.Compare(ext, ".tvpid", true) == 0 || string.Compare(ext, ".tvpio", true) == 0)
                {
                    //iEPG追加
                    IEPGFileClass iepgFile = new IEPGFileClass();
                    if (iepgFile.LoadTVPIDFile(path) == true)
                    {
                        List<CtrlCmdCLI.Def.ReserveData> val = new List<CtrlCmdCLI.Def.ReserveData>();
                        val.Add(iepgFile.AddInfo);
                        cmd.SendAddReserve(val);
                    }
                    else
                    {
                        MessageBox.Show("解析に失敗しました。デジタル用Version 2のiEPGの必要があります。");
                    }
                }
                else if (string.Compare(ext, ".tvpi", true) == 0 )
                {
                    //iEPG追加
                    IEPGFileClass iepgFile = new IEPGFileClass();
                    if (iepgFile.LoadTVPIFile(path) == true)
                    {
                        List<CtrlCmdCLI.Def.ReserveData> val = new List<CtrlCmdCLI.Def.ReserveData>();
                        val.Add(iepgFile.AddInfo);
                        cmd.SendAddReserve(val);
                    }
                    else
                    {
                        MessageBox.Show("解析に失敗しました。放送局名がサービスに関連づけされていない可能性があります。");
                    }
                }
            }
        }


    }
}
