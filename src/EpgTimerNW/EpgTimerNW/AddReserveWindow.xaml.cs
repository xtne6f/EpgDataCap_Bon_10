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
using CtrlCmdCLI;
using CtrlCmdCLI.Def;

namespace EpgTimer
{
    /// <summary>
    /// AddReserveWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AddReserveWindow : Window
    {
        private EpgEventInfo epgEventInfo;
        private RecSettingData recSet = new RecSettingData();
        private CtrlCmdUtil cmd = EpgTimerNW.NWConnect.Instance.cmd;
        private ChSet5Item serviceInfo = null;

        public AddReserveWindow()
        {
            InitializeComponent();

            try
            {
                StringBuilder buff = new StringBuilder(512);
                recSet.RecMode = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "RecMode", 1, SettingPath.TimerSrvIniPath);
                recSet.Priority = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "Priority", 2, SettingPath.TimerSrvIniPath);
                recSet.TuijyuuFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "TuijyuuFlag", 1, SettingPath.TimerSrvIniPath);
                recSet.ServiceMode = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "ServiceMode", 0, SettingPath.TimerSrvIniPath);
                recSet.PittariFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "PittariFlag", 0, SettingPath.TimerSrvIniPath);

                buff.Clear();
                IniFileHandler.GetPrivateProfileString("REC_DEF", "BatFilePath", "", buff, 512, SettingPath.TimerSrvIniPath);
                recSet.BatFilePath = buff.ToString();

                int count = IniFileHandler.GetPrivateProfileInt("REC_DEF_FOLDER", "Count", 0, SettingPath.TimerSrvIniPath);
                for (int i = 0; i < count; i++)
                {
                    RecFileSetInfo folderInfo = new RecFileSetInfo();
                    buff.Clear();
                    IniFileHandler.GetPrivateProfileString("REC_DEF_FOLDER", i.ToString(), "", buff, 512, SettingPath.TimerSrvIniPath);
                    folderInfo.RecFolder = buff.ToString();
                    IniFileHandler.GetPrivateProfileString("REC_DEF_FOLDER", "WritePlugIn" + i.ToString(), "Write_Default.dll", buff, 512, SettingPath.TimerSrvIniPath);
                    folderInfo.WritePlugIn = buff.ToString();

                    recSet.RecFolderList.Add(folderInfo);
                }


                recSet.SuspendMode = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "SuspendMode", 0, SettingPath.TimerSrvIniPath);
                recSet.RebootFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "RebootFlag", 0, SettingPath.TimerSrvIniPath);
                recSet.UseMargineFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "UseMargineFlag", 0, SettingPath.TimerSrvIniPath);
                recSet.StartMargine = IniFileHandler.GetPrivateProfileInt("REC_DEF", "StartMargine", 0, SettingPath.TimerSrvIniPath);
                recSet.EndMargine = IniFileHandler.GetPrivateProfileInt("REC_DEF", "EndMargine", 0, SettingPath.TimerSrvIniPath);
            }
            catch
            {
            }
            recSettingView.SetDefRecSetting(recSet);
        }

        private void button_add_reserve_Click(object sender, RoutedEventArgs e)
        {
            recSettingView.GetRecSetting(ref recSet);

            ReserveData item = new ReserveData();
            item.RecSetting = recSet;

            item.Title = epgEventInfo.ShortInfo.event_name;
            if (epgEventInfo.StartTimeFlag == 0)
            {
                MessageBox.Show("開始時間未定のため予約できません");
                DialogResult = false;
            }
            else
            {
                item.StartTime = epgEventInfo.start_time;
                item.StartTimeEpg = epgEventInfo.start_time;
            }
            if (epgEventInfo.DurationFlag == 0)
            {
                item.DurationSecond = 10 * 60;
            }
            else
            {
                item.DurationSecond = epgEventInfo.durationSec;
            }
            item.StationName = serviceInfo.ServiceName;
            item.OriginalNetworkID = epgEventInfo.original_network_id;
            item.TransportStreamID = epgEventInfo.transport_stream_id;
            item.ServiceID = epgEventInfo.service_id;
            item.EventID = epgEventInfo.event_id;

            List<ReserveData> addList = new List<ReserveData>();
            addList.Add(item);

            uint err = cmd.SendAddReserve(addList);
            if (err == 205)
            {
                MessageBox.Show("サーバーに接続できませんでした");
            } 
            DialogResult = true;
        }

        public void SetEpgEventInfo(EpgEventInfo eventInfo)
        {
            epgEventInfo = eventInfo;

            string basicInfo = "";
            string extInfo = "";
            if (eventInfo != null)
            {
                UInt64 key = ((UInt64)eventInfo.original_network_id) << 32 |
                    ((UInt64)eventInfo.transport_stream_id) << 16 |
                    ((UInt64)eventInfo.service_id);
                
                if (ChSet5.Instance.ChList.ContainsKey(key) == true)
                {
                    serviceInfo = ChSet5.Instance.ChList[key];
                    basicInfo += serviceInfo.ServiceName + "\r\n";
                }

                if (eventInfo.StartTimeFlag == 1)
                {
                    basicInfo += eventInfo.start_time.ToString("yyyy/MM/dd(ddd) HH:mm:ss ～ ");
                }
                else
                {
                    basicInfo += "未定 ～ ";
                }
                if (eventInfo.DurationFlag == 1)
                {
                    DateTime endTime = eventInfo.start_time + TimeSpan.FromSeconds(eventInfo.durationSec);
                    basicInfo += endTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss") + "\r\n";
                }
                else
                {
                    basicInfo += "未定\r\n";
                }

                if (eventInfo.ShortInfo != null)
                {
                    basicInfo += eventInfo.ShortInfo.event_name + "\r\n\r\n";
                    extInfo += eventInfo.ShortInfo.text_char + "\r\n\r\n";
                }

                if (eventInfo.ExtInfo != null)
                {
                    extInfo += eventInfo.ExtInfo.text_char + "\r\n\r\n";
                }

                //ジャンル
                extInfo += "ジャンル :\r\n";
                if(eventInfo.ContentInfo != null)
                {
                    foreach(EpgContentData info in eventInfo.ContentInfo.nibbleList)
                    {
                        int nibble1 = info.content_nibble_level_1;
                        int nibble2 = info.content_nibble_level_2;
                        UInt16 contentKey1 = (UInt16)(nibble1 << 8 | 0xFF);
                        UInt16 contentKey2 = (UInt16)(nibble1 << 8 | nibble2);
                        if (EpgTimerDef.Instance.ContentKindDictionary.ContainsKey(contentKey1) == true)
                        {
                            extInfo += EpgTimerDef.Instance.ContentKindDictionary[contentKey1];
                        }
                        if (EpgTimerDef.Instance.ContentKindDictionary.ContainsKey(contentKey2) == true)
                        {
                            extInfo += " - " + EpgTimerDef.Instance.ContentKindDictionary[contentKey2];
                        }
                        extInfo += "\r\n";
                    }
                }
                extInfo += "\r\n";

                //映像
                extInfo += "映像 :";
                if (eventInfo.ComponentInfo != null)
                {
                    int streamContent = eventInfo.ComponentInfo.stream_content;
                    int componentType = eventInfo.ComponentInfo.component_type;
                    UInt16 componentKey = (UInt16)(streamContent << 8 | componentType);
                    if (EpgTimerDef.Instance.ComponentKindDictionary.ContainsKey(componentKey) == true)
                    {
                        extInfo += EpgTimerDef.Instance.ComponentKindDictionary[componentKey];
                    }
                    if (eventInfo.ComponentInfo.text_char.Length > 0)
                    {
                        extInfo += "\r\n";
                        extInfo += eventInfo.ComponentInfo.text_char;
                    }
                }
                extInfo += "\r\n";

                //音声
                extInfo += "音声 :\r\n";
                if (eventInfo.AudioInfo != null)
                {
                    foreach (EpgAudioComponentInfoData info in eventInfo.AudioInfo.componentList)
                    {
                        int streamContent = info.stream_content;
                        int componentType = info.component_type;
                        UInt16 componentKey = (UInt16)(streamContent << 8 | componentType);
                        if (EpgTimerDef.Instance.ComponentKindDictionary.ContainsKey(componentKey) == true)
                        {
                            extInfo += EpgTimerDef.Instance.ComponentKindDictionary[componentKey];
                        }
                        if (info.text_char.Length > 0)
                        {
                            extInfo += "\r\n";
                            extInfo += info.text_char;
                        }
                        extInfo += "\r\n";
                        extInfo += "サンプリングレート :";
                        switch (info.sampling_rate)
                        {
                            case 1:
                                extInfo += "16kHz";
                                break;
                            case 2:
                                extInfo += "22.05kHz";
                                break;
                            case 3:
                                extInfo += "24kHz";
                                break;
                            case 5:
                                extInfo += "32kHz";
                                break;
                            case 6:
                                extInfo += "44.1kHz";
                                break;
                            case 7:
                                extInfo += "48kHz";
                                break;
                            default:
                                break;
                        }
                        extInfo += "\r\n";
                    }
                }
                extInfo += "\r\n";

                //スクランブル
                if (!(0x7880 <= eventInfo.original_network_id && eventInfo.original_network_id <= 0x7FE8))
                {
                    if (eventInfo.FreeCAFlag == 0)
                    {
                        extInfo += "無料放送\r\n";
                    }
                    else
                    {
                        extInfo += "有料放送\r\n";
                    }
                    extInfo += "\r\n";
                }

                extInfo += "OriginalNetworkID : " + eventInfo.original_network_id.ToString() + " (0x" + eventInfo.original_network_id.ToString("X4") + ")\r\n";
                extInfo += "TransportStreamID : " + eventInfo.transport_stream_id.ToString() + " (0x" + eventInfo.transport_stream_id.ToString("X4") + ")\r\n";
                extInfo += "ServiceID : " + eventInfo.service_id.ToString() + " (0x" + eventInfo.service_id.ToString("X4") + ")\r\n";
                extInfo += "EventID : " + eventInfo.event_id.ToString() + " (0x" + eventInfo.event_id.ToString("X4") + ")\r\n";

            }
            textBox_info.Text = basicInfo;
            textBox_descInfo.Text = extInfo;
        }

    }
}
