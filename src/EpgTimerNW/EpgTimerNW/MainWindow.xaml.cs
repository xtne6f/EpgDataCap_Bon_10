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
using Common;
using EpgTimer;
using CtrlCmdCLI;
using CtrlCmdCLI.Def;

namespace EpgTimerNW
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Threading.Mutex mutex;
        private TaskTrayClass taskTray = null;
        private bool closeFlag = false;
        private Dictionary<string, Button> buttonList = new Dictionary<string, Button>();
        private CtrlCmdUtil cmd = EpgTimerNW.NWConnect.Instance.cmd;

        public MainWindow()
        {
            Settings.LoadFromXmlFile();
            NWConnect.Instance.cmd.SetSendMode(true);
            NWConnect.Instance.cmd.SetNWSetting(Settings.Instance.NWServerIP, Settings.Instance.NWServerPort);

            mutex = new System.Threading.Mutex(false, "Global\\EpgTimer_BonNW");
            if (!mutex.WaitOne(0, false))
            {
                CheckCmdLine();

                mutex.Close();
                mutex = null;

                closeFlag = true;
                Close();
                return;
            } 
            
            InitializeComponent();

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
            settingButton.Margin = new Thickness(2, 2, 2, 15);
            settingButton.Click += new RoutedEventHandler(settingButton_Click);
            settingButton.Content = "設定";
            buttonList.Add("設定", settingButton);

            Button connectButton = new Button();
            connectButton.MinWidth = 75;
            connectButton.Margin = new Thickness(2, 2, 2, 15);
            connectButton.Click += new RoutedEventHandler(connectButton_Click);
            connectButton.Content = "再接続";
            buttonList.Add("再接続", connectButton);

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

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResetButtonView();

            ResetTaskMenu();

            if (NWConnect.Instance.ConnectServer(Settings.Instance.NWServerIP, Settings.Instance.NWServerPort, Settings.Instance.NWWaitPort, OutsideCmdCallback, this) == false)
            {
                if (ConnectCmd(false) == false)
                {
                    return;
                }
            }
            try
            {
                byte[] binData;
                if (NWConnect.Instance.cmd.SendFileCopy("ChSet5.txt", out binData) == 1)
                {
                    string filePath = SettingPath.SettingFolderPath;
                    System.IO.Directory.CreateDirectory(filePath);
                    filePath += "\\ChSet5.txt";
                    using (System.IO.BinaryWriter w = new System.IO.BinaryWriter(System.IO.File.Create(filePath)))
                    {
                        w.Write(binData);
                        w.Close();
                    }
                    ChSet5.LoadFile();
                }
                reserveView.ReloadReserve();
                List<ReserveItem> reserveList = new List<ReserveItem>();
                reserveView.GetReserveList(ref reserveList);
                epgView.SetReserveList(reserveList);
                recInfoView.ReloadRecInfo();
                autoAddView.ReloadData();

                epgView.ReloadEpgData();
                epgView.ReDrawReserve();
                
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
                reserveView.SaveSize();
                recInfoView.SaveSize();
                if (NWConnect.Instance.IsConnected == true)
                {
                    if (NWConnect.Instance.cmd.SendUnRegistTCP(Settings.Instance.NWServerPort) == 205)
                    {
                        MessageBox.Show("サーバーに接続できませんでした");
                    }
                }
                Settings.SaveToXmlFile();

                if (mutex != null)
                {
                    mutex.ReleaseMutex();
                    mutex.Close();
                }
            }
        }

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
                        if (NWConnect.Instance.cmd.SendAddEpgAutoAdd(val) == 205)
                        {
                            MessageBox.Show("サーバーに接続できませんでした");
                        }
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
                        if (NWConnect.Instance.cmd.SendAddReserve(val) == 205)
                        {
                            MessageBox.Show("サーバーに接続できませんでした");
                        }
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
                    if (iepgFile.LoadTVPIFile(path) == true)
                    {
                        List<CtrlCmdCLI.Def.ReserveData> val = new List<CtrlCmdCLI.Def.ReserveData>();
                        val.Add(iepgFile.AddInfo);
                        if (NWConnect.Instance.cmd.SendAddReserve(val) == 205)
                        {
                            MessageBox.Show("サーバーに接続できませんでした");
                        }
                    }
                    else
                    {
                        MessageBox.Show("解析に失敗しました。放送局名がサービスに関連づけされていない可能性があります。");
                    }
                }
            }
        }

        void connectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectCmd(true);
        }

        bool ConnectCmd(bool reloadFlag)
        {
            ConnectWindow dlg = new ConnectWindow();
            dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
            if (dlg.ShowDialog() == true)
            {
                if (NWConnect.Instance.ConnectServer(Settings.Instance.NWServerIP, Settings.Instance.NWServerPort, Settings.Instance.NWWaitPort, OutsideCmdCallback, this) == false)
                {
                    MessageBox.Show("サーバーへの接続に失敗しました");
                }
                else
                {
                    if (reloadFlag == true)
                    {
                        byte[] binData;
                        if (NWConnect.Instance.cmd.SendFileCopy("ChSet5.txt", out binData) == 1)
                        {
                            string filePath = SettingPath.SettingFolderPath;
                            System.IO.Directory.CreateDirectory(filePath);
                            filePath += "\\ChSet5.txt";
                            using (System.IO.BinaryWriter w = new System.IO.BinaryWriter(System.IO.File.Create(filePath)))
                            {
                                w.Write(binData);
                                w.Close();
                            }
                            ChSet5.LoadFile();
                        }

                        reserveView.ReloadReserve();
                        List<ReserveItem> reserveList = new List<ReserveItem>();
                        reserveView.GetReserveList(ref reserveList);
                        epgView.SetReserveList(reserveList);
                        recInfoView.ReloadRecInfo();
                        autoAddView.ReloadData();

                        epgView.ReloadEpgData();
                        epgView.ReDrawReserve();

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

                    return true;
                }
            }
            return false;
        }

        void epgReloadButton_Click(object sender, RoutedEventArgs e)
        {
            EpgReloadCmd();
        }

        void EpgReloadCmd()
        {
            uint err = NWConnect.Instance.cmd.SendReloadEpg();
            if (err == 205)
            {
                MessageBox.Show("サーバーに接続できませんでした");
            }
            else if (err != 1)
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
            uint err = NWConnect.Instance.cmd.SendEpgCapNow();
            if (err == 205)
            {
                MessageBox.Show("サーバーに接続できませんでした");
            }
            else if (err != 1)
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
            /*if (cmd.SendChkSuspend() != 1)
            {
                MessageBox.Show("休止に移行できる状態ではありません。\r\n（もうすぐ予約が始まる。または抑制条件のexeが起動している。など）");
            }
            else
            {
                cmd.SendSuspend(2);
            }*/
        }

        void standbyButton_Click(object sender, RoutedEventArgs e)
        {
            StandbyCmd();
        }

        void StandbyCmd()
        {
            /*if (cmd.SendChkSuspend() != 1)
            {
                MessageBox.Show("スタンバイに移行できる状態ではありません。\r\n（もうすぐ予約が始まる。または抑制条件のexeが起動している。など）");
            }
            else
            {
                cmd.SendSuspend(1);
            }*/
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
                    //cmd.SendReloadSetting();
                    ResetButtonView();
                    ResetTaskMenu();
                }
            }
            ChSet5.LoadFile();
        }

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
                        stackPanel_button.Children.Add(buttonList[info]);
                    }
                }
            }
        }

        private int OutsideCmdCallback(object pParam, CMD_STREAM pCmdParam, ref CMD_STREAM pResParam)
        {
            MainWindow sys = (MainWindow)pParam;

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
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                epgView.ReloadEpgData();
                                epgView.ReDrawReserve();
                            }));
                        }
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
                        if (NWConnect.Instance.cmd.SendAddEpgAutoAdd(val) == 205)
                        {
                            MessageBox.Show("サーバーに接続できませんでした");
                        }
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
                        if (NWConnect.Instance.cmd.SendAddReserve(val) == 205)
                        {
                            MessageBox.Show("サーバーに接続できませんでした");
                        }
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
                        if (NWConnect.Instance.cmd.SendAddReserve(val) == 205)
                        {
                            MessageBox.Show("サーバーに接続できませんでした");
                        }
                    }
                    else
                    {
                        MessageBox.Show("解析に失敗しました。放送局名がサービスに関連づけされていない可能性があります。");
                    }
                }
            }
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
    }
}
