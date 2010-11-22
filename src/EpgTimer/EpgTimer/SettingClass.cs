﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace EpgTimer
{
    class IniFileHandler
    {
        [DllImport("KERNEL32.DLL")]
        public static extern uint
          GetPrivateProfileString(string lpAppName,
          string lpKeyName, string lpDefault,
          StringBuilder lpReturnedString, uint nSize,
          string lpFileName);

        [DllImport("KERNEL32.DLL",
            EntryPoint = "GetPrivateProfileStringA")]
        public static extern uint
          GetPrivateProfileStringByByteArray(string lpAppName,
          string lpKeyName, string lpDefault,
          byte[] lpReturnedString, uint nSize,
          string lpFileName);

        [DllImport("KERNEL32.DLL")]
        public static extern int
          GetPrivateProfileInt(string lpAppName,
          string lpKeyName, int nDefault, string lpFileName);

        [DllImport("KERNEL32.DLL")]
        public static extern uint WritePrivateProfileString(
          string lpAppName,
          string lpKeyName,
          string lpString,
          string lpFileName);
    }

    class SettingPath
    {
        public static string CommonIniPath
        {
            get
            {
                string iniPath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                iniPath += "\\Common.ini";
                return iniPath;
            }
        }
        public static string TimerSrvIniPath
        {
            get
            {
                string iniPath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                iniPath += "\\EpgTimerSrv.ini";
                return iniPath;
            }
        }
        public static string DefSettingFolderPath
        {
            get
            {
//                string defSetPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
//                defSetPath += "\\EpgTimerBon";
                string defSetPath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                defSetPath += "\\Setting";

                return defSetPath;
            }
        }
        public static string SettingFolderPath
        {
            get
            {
                string iniPath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                iniPath += "\\Common.ini";
                string defSetPath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                defSetPath += "\\Setting";
                StringBuilder buff = new StringBuilder(512);
                IniFileHandler.GetPrivateProfileString("SET", "DataSavePath", defSetPath, buff, 512, iniPath);
                return  buff.ToString();
            }
        }
        public static void CheckFolderPath(ref string folderPath)
        {
            if( folderPath.LastIndexOf("\\") == folderPath.Length-1 ){
                folderPath = folderPath.Remove(folderPath.Length - 1);
            }
        }
        public static string ModulePath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            }
        }
    }

    public class Settings
    {
        private bool searchKeyRegExp;
        private bool searchKeyTitleOnly;
        private List<ContentKindInfo> searchKeyContentList;
        private List<DateItem> searchKeyDateItemList;
        private List<string> contentColorList;
        private double minHeight;
        private double serviceWidth;
        private double scrollSize;
        private List<string> viewButtonList;
        private List<ChSet5Item> offViewServiceList;
        private List<ChSet5Item> onTereViewServiceList;
        private List<ChSet5Item> onBsViewServiceList;
        private List<ChSet5Item> onCsViewServiceList;
        private List<ChSet5Item> onOtherViewServiceList;
        private bool closeMin;
        private bool wakeMin;
        private string reserveRectColorNormal;
        private string reserveRectColorNo;
        private string reserveRectColorNoTuner;
        private bool reserveRectBackground;
        private System.Windows.WindowState lastWindowState;
        private double mainWndLeft;
        private double mainWndTop;
        private double mainWndWidth;
        private double mainWndHeight;
        private List<string> taskMenuList;
        private double resColumnWidth0;
        private double resColumnWidth1;
        private double resColumnWidth2;
        private double resColumnWidth3;
        private double resColumnWidth4;
        private double resColumnWidth5;
        private double resColumnWidth6;
        private double recInfoColumnWidth0;
        private double recInfoColumnWidth1;
        private double recInfoColumnWidth2;
        private double recInfoColumnWidth3;
        private double recInfoColumnWidth4;
        private double recInfoColumnWidth5;
        private string fontName;
        private double fontSize;
        private string nwServerIP;
        private UInt32 nwServerPort;
        private UInt32 nwWaitPort;
        private string nwMacAdd;      
        private List<string> andKeyList;
        private List<string> notKeyList;
        private string tvTestExe;
        private string tvTestCmd;
        private string resColumnHead;
        private ListSortDirection resSortDirection;
        private string recInfoColumnHead;
        private ListSortDirection recInfoSortDirection;
        private string cust1BtnName;
        private string cust1BtnCmd;
        private string cust1BtnCmdOpt;
        private string cust2BtnName;
        private string cust2BtnCmd;
        private string cust2BtnCmdOpt;
        private List<IEPGStationInfo> iEpgStationList;
        private bool searchKeyAimaiFlag;
        private double dragScroll;
        private bool nwTvMode;
        private bool nwTvModeUDP;
        private bool nwTvModeTCP;
        private bool epgToolTip;
        private string reserveRectColorWarning;
        private bool noToolTip;

        public bool SearchKeyRegExp
        {
            get { return searchKeyRegExp; }
            set { searchKeyRegExp = value; }
        }
        public bool SearchKeyTitleOnly
        {
            get { return searchKeyTitleOnly; }
            set { searchKeyTitleOnly = value; }
        }
        public List<ContentKindInfo> SearchKeyContentList
        {
            get { return searchKeyContentList; }
            set { searchKeyContentList = value; }
        }
        public List<DateItem> SearchKeyDateItemList
        {
            get { return searchKeyDateItemList; }
            set { searchKeyDateItemList = value; }
        }
        public List<string> ContentColorList
        {
            get { return contentColorList; }
            set { contentColorList = value; }
        }
        public double MinHeight
        {
            get { return minHeight; }
            set { minHeight = value; }
        }
        public double ServiceWidth
        {
            get { return serviceWidth; }
            set { serviceWidth = value; }
        }
        public double ScrollSize
        {
            get { return scrollSize; }
            set { scrollSize = value; }
        }
        public List<string> ViewButtonList
        {
            get { return viewButtonList; }
            set { viewButtonList = value; }
        }
        public List<ChSet5Item> OffViewServiceList
        {
            get { return offViewServiceList; }
            set { offViewServiceList = value; }
        }
        public List<ChSet5Item> OnTereViewServiceList
        {
            get { return onTereViewServiceList; }
            set { onTereViewServiceList = value; }
        }
        public List<ChSet5Item> OnBsViewServiceList
        {
            get { return onBsViewServiceList; }
            set { onBsViewServiceList = value; }
        }
        public List<ChSet5Item> OnCsViewServiceList
        {
            get { return onCsViewServiceList; }
            set { onCsViewServiceList = value; }
        }
        public List<ChSet5Item> OnOtherViewServiceList
        {
            get { return onOtherViewServiceList; }
            set { onOtherViewServiceList = value; }
        }
        public bool CloseMin
        {
            get { return closeMin; }
            set { closeMin = value; }
        }
        public bool WakeMin
        {
            get { return wakeMin; }
            set { wakeMin = value; }
        }
        public string ReserveRectColorNormal
        {
            get { return reserveRectColorNormal; }
            set { reserveRectColorNormal = value; }
        }
        public string ReserveRectColorNo
        {
            get { return reserveRectColorNo; }
            set { reserveRectColorNo = value; }
        }
        public string ReserveRectColorNoTuner
        {
            get { return reserveRectColorNoTuner; }
            set { reserveRectColorNoTuner = value; }
        }
        public bool ReserveRectBackground
        {
            get { return reserveRectBackground; }
            set { reserveRectBackground = value; }
        }
        public System.Windows.WindowState LastWindowState
        {
            get { return lastWindowState; }
            set { lastWindowState = value; }
        }
        public double MainWndLeft
        {
            get { return mainWndLeft; }
            set { mainWndLeft = value; }
        }
        public double MainWndTop        {
            get { return mainWndTop; }
            set { mainWndTop = value; }
        }
        public double MainWndWidth
        {
            get { return mainWndWidth; }
            set { mainWndWidth = value; }
        }
        public double MainWndHeight
        {
            get { return mainWndHeight; }
            set { mainWndHeight = value; }
        }
        public List<string> TaskMenuList
        {
            get { return taskMenuList; }
            set { taskMenuList = value; }
        }
        public double ResColumnWidth0
        {
            get { return resColumnWidth0; }
            set { resColumnWidth0 = value; }
        }
        public double ResColumnWidth1
        {
            get { return resColumnWidth1; }
            set { resColumnWidth1 = value; }
        }
        public double ResColumnWidth2
        {
            get { return resColumnWidth2; }
            set { resColumnWidth2 = value; }
        }
        public double ResColumnWidth3
        {
            get { return resColumnWidth3; }
            set { resColumnWidth3 = value; }
        }
        public double ResColumnWidth4
        {
            get { return resColumnWidth4; }
            set { resColumnWidth4 = value; }
        }
        public double ResColumnWidth5
        {
            get { return resColumnWidth5; }
            set { resColumnWidth5 = value; }
        }
        public double ResColumnWidth6
        {
            get { return resColumnWidth6; }
            set { resColumnWidth6 = value; }
        }
        public double RecInfoColumnWidth0
        {
            get { return recInfoColumnWidth0; }
            set { recInfoColumnWidth0 = value; }
        }
        public double RecInfoColumnWidth1
        {
            get { return recInfoColumnWidth1; }
            set { recInfoColumnWidth1 = value; }
        }
        public double RecInfoColumnWidth2
        {
            get { return recInfoColumnWidth2; }
            set { recInfoColumnWidth2 = value; }
        }
        public double RecInfoColumnWidth3
        {
            get { return recInfoColumnWidth3; }
            set { recInfoColumnWidth3 = value; }
        }
        public double RecInfoColumnWidth4
        {
            get { return recInfoColumnWidth4; }
            set { recInfoColumnWidth4 = value; }
        }
        public double RecInfoColumnWidth5
        {
            get { return recInfoColumnWidth5; }
            set { recInfoColumnWidth5 = value; }
        }
        public string FontName
        {
            get { return fontName; }
            set { fontName = value; }
        }
        public double FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }
        public string NWServerIP
        {
            get { return nwServerIP; }
            set { nwServerIP = value; }
        }
        public UInt32 NWServerPort
        {
            get { return nwServerPort; }
            set { nwServerPort = value; }
        }
        public UInt32 NWWaitPort
        {
            get { return nwWaitPort; }
            set { nwWaitPort = value; }
        }
        public string NWMacAdd
        {
            get { return nwMacAdd; }
            set { nwMacAdd = value; }
        }        
        public List<string> AndKeyList
        {
            get { return andKeyList; }
            set { andKeyList = value; }
        }
        public List<string> NotKeyList
        {
            get { return notKeyList; }
            set { notKeyList = value; }
        }
        public string TvTestExe
        {
            get { return tvTestExe; }
            set { tvTestExe = value; }
        }
        public string TvTestCmd
        {
            get { return tvTestCmd; }
            set { tvTestCmd = value; }
        }
        public string ResColumnHead
        {
            get { return resColumnHead; }
            set { resColumnHead = value; }
        }
        public ListSortDirection ResSortDirection
        {
            get { return resSortDirection; }
            set { resSortDirection = value; }
        }
        public string RecInfoColumnHead
        {
            get { return recInfoColumnHead; }
            set { recInfoColumnHead = value; }
        }
        public ListSortDirection RecInfoSortDirection
        {
            get { return recInfoSortDirection; }
            set { recInfoSortDirection = value; }
        }
        public string Cust1BtnName
        {
            get { return cust1BtnName; }
            set { cust1BtnName = value; }
        }
        public string Cust1BtnCmd
        {
            get { return cust1BtnCmd; }
            set { cust1BtnCmd = value; }
        }
        public string Cust1BtnCmdOpt
        {
            get { return cust1BtnCmdOpt; }
            set { cust1BtnCmdOpt = value; }
        }
        public string Cust2BtnName
        {
            get { return cust2BtnName; }
            set { cust2BtnName = value; }
        }
        public string Cust2BtnCmd
        {
            get { return cust2BtnCmd; }
            set { cust2BtnCmd = value; }
        }
        public string Cust2BtnCmdOpt
        {
            get { return cust2BtnCmdOpt; }
            set { cust2BtnCmdOpt = value; }
        }
        public List<IEPGStationInfo> IEpgStationList
        {
            get { return iEpgStationList; }
            set { iEpgStationList = value; }
        }
        public bool SearchKeyAimaiFlag
        {
            get { return searchKeyAimaiFlag; }
            set { searchKeyAimaiFlag = value; }
        }
        public double DragScroll
        {
            get { return dragScroll; }
            set { dragScroll = value; }
        }
        public bool NwTvMode
        {
            get { return nwTvMode; }
            set { nwTvMode = value; }
        }
        public bool NwTvModeUDP
        {
            get { return nwTvModeUDP; }
            set { nwTvModeUDP = value; }
        }
        public bool NwTvModeTCP
        {
            get { return nwTvModeTCP; }
            set { nwTvModeTCP = value; }
        }
        public bool EpgToolTip
        {
            get { return epgToolTip; }
            set { epgToolTip = value; }
        }
        public string ReserveRectColorWarning
        {
            get { return reserveRectColorWarning; }
            set { reserveRectColorWarning = value; }
        }
        public bool NoToolTip
        {
            get { return noToolTip; }
            set { noToolTip = value; }
        }
        
        public Settings()
        {
            searchKeyContentList = new List<ContentKindInfo>();
            searchKeyDateItemList = new List<DateItem>();
            contentColorList = new List<string>();
            minHeight = 2;
            serviceWidth = 150;
            scrollSize = 240;
            viewButtonList = new List<string>();
            offViewServiceList = new List<ChSet5Item>();
            onTereViewServiceList = new List<ChSet5Item>();
            onBsViewServiceList = new List<ChSet5Item>();
            onCsViewServiceList = new List<ChSet5Item>();
            onOtherViewServiceList = new List<ChSet5Item>();
            reserveRectColorNormal = "Lime";
            reserveRectColorNo = "Black";
            reserveRectColorNoTuner = "Red";
            reserveRectBackground = false;
            lastWindowState = System.Windows.WindowState.Normal;
            taskMenuList = new List<string>();
            fontName = "メイリオ";
            fontSize = 12;
            nwServerIP = "";
            nwServerPort = 4510;
            nwWaitPort = 4520;
            nwMacAdd = "";
            andKeyList = new List<string>();
            notKeyList = new List<string>();
            tvTestExe = "";
            tvTestCmd = "";
            resColumnHead = "";
            resSortDirection = ListSortDirection.Ascending;
            recInfoColumnHead = "";
            recInfoSortDirection = ListSortDirection.Ascending;
            cust1BtnName = "";
            cust1BtnCmd = "";
            cust1BtnCmdOpt = "";
            cust2BtnName = "";
            cust2BtnCmd = "";
            cust2BtnCmdOpt = "";
            iEpgStationList = new List<IEPGStationInfo>();
            dragScroll = 1.0;
            nwTvMode = false;
            nwTvModeUDP = false;
            nwTvModeTCP = false;
            epgToolTip = true;
            reserveRectColorWarning = "Yellow";
            noToolTip = false;
        }
        [NonSerialized()]
        private static Settings _instance;
        [System.Xml.Serialization.XmlIgnore]
        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Settings();
                return _instance;
            }
            set { _instance = value; }
        }

        public static void LoadFromXmlFile()
        {
            string path = GetSettingPath();

            try
            {
                FileStream fs = new FileStream(path,
                    FileMode.Open,
                    FileAccess.Read);
                System.Xml.Serialization.XmlSerializer xs =
                    new System.Xml.Serialization.XmlSerializer(
                        typeof(Settings));
                //読み込んで逆シリアル化する
                object obj = xs.Deserialize(fs);
                fs.Close();
                Instance = (Settings)obj;

            }
            catch
            {
            }
            finally
            {
                if (Instance.contentColorList.Count == 0)
                {
                    for (int i = 0; i < 0x11; i++)
                    {
                        Instance.contentColorList.Add("White");
                    }
                }
                else if (Instance.contentColorList.Count == 0x10)
                {
                    Instance.contentColorList.Add("White");
                }
                if (Instance.viewButtonList.Count == 0)
                {
                    Instance.viewButtonList.Add("設定");
                    Instance.viewButtonList.Add("（空白）");
                    Instance.viewButtonList.Add("検索");
                    Instance.viewButtonList.Add("（空白）");
                    Instance.viewButtonList.Add("スタンバイ");
                    Instance.viewButtonList.Add("休止");
                    Instance.viewButtonList.Add("（空白）");
                    Instance.viewButtonList.Add("EPG取得");
                    Instance.viewButtonList.Add("（空白）");
                    Instance.viewButtonList.Add("EPG再読み込み");
                    Instance.viewButtonList.Add("（空白）");
                    Instance.viewButtonList.Add("終了");
                }
                if (Instance.taskMenuList.Count == 0)
                {
                    Instance.taskMenuList.Add("設定");
                    Instance.taskMenuList.Add("（セパレータ）");
                    Instance.taskMenuList.Add("スタンバイ");
                    Instance.taskMenuList.Add("休止");
                    Instance.taskMenuList.Add("（セパレータ）");
                    Instance.taskMenuList.Add("終了");
                }
            }
        }

        public static void SaveToXmlFile()
        {
            string path = GetSettingPath();

            try
            {
                FileStream fs = new FileStream(path,
                    FileMode.Create,
                    FileAccess.Write);
                System.Xml.Serialization.XmlSerializer xs =
                    new System.Xml.Serialization.XmlSerializer(
                    typeof(Settings));
                //シリアル化して書き込む
                xs.Serialize(fs, Instance);
                fs.Close();
            }
            catch
            {
            }
        }

        private static string GetSettingPath()
        {
            Assembly myAssembly = Assembly.GetEntryAssembly();
            string path = myAssembly.Location + ".xml";

            return path;
        }

        public static void GetDefRecSetting(ref CtrlCmdCLI.Def.RecSettingData defKey)
        {
            StringBuilder buff = new StringBuilder(512);

            defKey.RecMode = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "RecMode", 1, SettingPath.TimerSrvIniPath);
            defKey.Priority = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "Priority", 2, SettingPath.TimerSrvIniPath);
            defKey.TuijyuuFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "TuijyuuFlag", 1, SettingPath.TimerSrvIniPath);
            defKey.ServiceMode = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "ServiceMode", 0, SettingPath.TimerSrvIniPath);
            defKey.PittariFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "PittariFlag", 0, SettingPath.TimerSrvIniPath);

            buff.Clear();
            IniFileHandler.GetPrivateProfileString("REC_DEF", "BatFilePath", "", buff, 512, SettingPath.TimerSrvIniPath);
            defKey.BatFilePath = buff.ToString();

            int count = IniFileHandler.GetPrivateProfileInt("REC_DEF_FOLDER", "Count", 0, SettingPath.TimerSrvIniPath);
            for (int i = 0; i < count; i++)
            {
                CtrlCmdCLI.Def.RecFileSetInfo folderInfo = new CtrlCmdCLI.Def.RecFileSetInfo();
                buff.Clear();
                IniFileHandler.GetPrivateProfileString("REC_DEF_FOLDER", i.ToString(), "", buff, 512, SettingPath.TimerSrvIniPath);
                folderInfo.RecFolder = buff.ToString();
                IniFileHandler.GetPrivateProfileString("REC_DEF_FOLDER", "WritePlugIn" + i.ToString(), "Write_Default.dll", buff, 512, SettingPath.TimerSrvIniPath);
                folderInfo.WritePlugIn = buff.ToString();

                defKey.RecFolderList.Add(folderInfo);
            }

            defKey.SuspendMode = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "SuspendMode", 0, SettingPath.TimerSrvIniPath);
            defKey.RebootFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "RebootFlag", 0, SettingPath.TimerSrvIniPath);
            defKey.UseMargineFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "UseMargineFlag", 0, SettingPath.TimerSrvIniPath);
            defKey.StartMargine = IniFileHandler.GetPrivateProfileInt("REC_DEF", "StartMargine", 0, SettingPath.TimerSrvIniPath);
            defKey.EndMargine = IniFileHandler.GetPrivateProfileInt("REC_DEF", "EndMargine", 0, SettingPath.TimerSrvIniPath);
        }
    }
}
