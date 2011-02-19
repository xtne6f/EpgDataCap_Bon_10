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

namespace EpgTimer
{
    /// <summary>
    /// ChangeReserveWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ChangeReserveWindow : Window
    {
        private EpgEventInfo epgEventInfo;
        private ChSet5Item serviceInfo = null;
        public bool DeleteEnd = false;

        public ReserveData setInfo = new ReserveData();
        public ChangeReserveWindow()
        {
            InitializeComponent();

            tabItem_pgInfo.Visibility = System.Windows.Visibility.Hidden;
            try
            {
                comboBox_service.ItemsSource = ChSet5.Instance.ChList.Values;
                comboBox_service.SelectedIndex = 0;
            }
            catch
            {
            }

        }

        public void AddReserveMode(bool addMode)
        {
            if (addMode == true)
            {
                checkBox_program.IsChecked = true;

                this.Title = "プログラム予約追加";
                checkBox_program.Visibility = System.Windows.Visibility.Hidden;

                DateTime startTime = DateTime.Now.AddMinutes(1);
                textBox_start_yyyy.Text = startTime.Year.ToString();
                textBox_start_mm.Text = startTime.Month.ToString();
                textBox_start_dd.Text = startTime.Day.ToString();
                textBox_start_HH.Text = startTime.Hour.ToString();
                textBox_start_MM.Text = startTime.Minute.ToString();
                textBox_start_SS.Text = "0";

                DateTime endTime = startTime.AddMinutes(30);
                textBox_end_yyyy.Text = endTime.Year.ToString();
                textBox_end_mm.Text = endTime.Month.ToString();
                textBox_end_dd.Text = endTime.Day.ToString();
                textBox_end_HH.Text = endTime.Hour.ToString();
                textBox_end_MM.Text = endTime.Minute.ToString();
                textBox_end_SS.Text = "0";

                button_change.Content = "予約";

                button_delete.Visibility = System.Windows.Visibility.Hidden;

                UpdateGUIMode(true);
            }
            else
            {
                checkBox_program.IsChecked = false;

                this.Title = "予約変更";
                checkBox_program.Visibility = System.Windows.Visibility.Visible;

                button_change.Content = "変更";

                button_delete.Visibility = System.Windows.Visibility.Visible;

                UpdateGUIMode(false);
            }
        }

        public void SetReserve(ReserveData info)
        {
            if (info.EventID == 0xFFFF)
            {
                UpdateGUIMode(true);
                checkBox_program.IsChecked = true;
                checkBox_program.IsEnabled = false;
            }
            else
            {
                UpdateGUIMode(false);
                checkBox_program.IsChecked = false;
                checkBox_program.IsEnabled = true;
            }

            textBox_title.Text = info.Title;
            foreach (ChSet5Item data in comboBox_service.Items)
            {
                if (String.Compare(data.ServiceName, info.StationName) == 0)
                {
                    comboBox_service.SelectedItem = data;
                    break;
                }
            }

            textBox_start_yyyy.Text = info.StartTime.Year.ToString();
            textBox_start_mm.Text = info.StartTime.Month.ToString();
            textBox_start_dd.Text = info.StartTime.Day.ToString();
            textBox_start_HH.Text = info.StartTime.Hour.ToString();
            textBox_start_MM.Text = info.StartTime.Minute.ToString();
            textBox_start_SS.Text = info.StartTime.Second.ToString();

            DateTime endTime = info.StartTime.AddSeconds(info.DurationSecond);
            textBox_end_yyyy.Text = endTime.Year.ToString();
            textBox_end_mm.Text = endTime.Month.ToString();
            textBox_end_dd.Text = endTime.Day.ToString();
            textBox_end_HH.Text = endTime.Hour.ToString();
            textBox_end_MM.Text = endTime.Minute.ToString();
            textBox_end_SS.Text = endTime.Second.ToString();

            recSettingView.SetDefRecSetting(info.RecSetting);

            setInfo = info;
        }

        private void UpdateGUIMode(bool program)
        {
            if (program == true)
            {
                recSettingView.comboBox_tuijyu.IsEnabled = false;
                recSettingView.comboBox_pittari.IsEnabled = false;
                comboBox_service.IsEnabled = true;
                textBox_start_yyyy.IsEnabled = true;
                textBox_start_mm.IsEnabled = true;
                textBox_start_dd.IsEnabled = true;
                textBox_start_HH.IsEnabled = true;
                textBox_start_MM.IsEnabled = true;
                textBox_start_SS.IsEnabled = true;
                textBox_end_yyyy.IsEnabled = true;
                textBox_end_mm.IsEnabled = true;
                textBox_end_dd.IsEnabled = true;
                textBox_end_HH.IsEnabled = true;
                textBox_end_MM.IsEnabled = true;
                textBox_end_SS.IsEnabled = true;
            }
            else
            {
                recSettingView.comboBox_tuijyu.IsEnabled = true;
                recSettingView.comboBox_pittari.IsEnabled = true;
                comboBox_service.IsEnabled = false;
                textBox_start_yyyy.IsEnabled = false;
                textBox_start_mm.IsEnabled = false;
                textBox_start_dd.IsEnabled = false;
                textBox_start_HH.IsEnabled = false;
                textBox_start_MM.IsEnabled = false;
                textBox_start_SS.IsEnabled = false;
                textBox_end_yyyy.IsEnabled = false;
                textBox_end_mm.IsEnabled = false;
                textBox_end_dd.IsEnabled = false;
                textBox_end_HH.IsEnabled = false;
                textBox_end_MM.IsEnabled = false;
                textBox_end_SS.IsEnabled = false;
            }
        }

        private void checkBox_program_Click(object sender, RoutedEventArgs e)
        {
            UpdateGUIMode((bool)checkBox_program.IsChecked);
        }

        private void button_change_Click(object sender, RoutedEventArgs e)
        {
            RecSettingData recSet = new RecSettingData();
            recSettingView.GetRecSetting(ref recSet);
            setInfo.RecSetting = recSet;

            setInfo.Title = textBox_title.Text;
            if (checkBox_program.IsChecked == true)
            {
                try
                {
                    ChSet5Item service = comboBox_service.SelectedItem as ChSet5Item;
                    setInfo.StationName = service.ServiceName;
                    setInfo.OriginalNetworkID = service.ONID;
                    setInfo.TransportStreamID = service.TSID;
                    setInfo.ServiceID = service.SID;
                    setInfo.EventID = 0xFFFF;
                    DateTime startTime = new DateTime(
                        Convert.ToInt32(textBox_start_yyyy.Text),
                        Convert.ToInt32(textBox_start_mm.Text),
                        Convert.ToInt32(textBox_start_dd.Text),
                        Convert.ToInt32(textBox_start_HH.Text),
                        Convert.ToInt32(textBox_start_MM.Text),
                        Convert.ToInt32(textBox_start_SS.Text),
                        0,
                        DateTimeKind.Utc
                        );
                    DateTime endTime = new DateTime(
                        Convert.ToInt32(textBox_end_yyyy.Text),
                        Convert.ToInt32(textBox_end_mm.Text),
                        Convert.ToInt32(textBox_end_dd.Text),
                        Convert.ToInt32(textBox_end_HH.Text),
                        Convert.ToInt32(textBox_end_MM.Text),
                        Convert.ToInt32(textBox_end_SS.Text),
                        0,
                        DateTimeKind.Utc
                        );
                    if (startTime > endTime)
                    {
                        MessageBox.Show("終了日時が開始日時より前です");
                        return;
                    }
                    setInfo.StartTime = startTime;
                    setInfo.StartTimeEpg = startTime;
                    TimeSpan duration = endTime - startTime;
                    setInfo.DurationSecond = (uint)duration.TotalSeconds;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                setInfo.RecSetting.TuijyuuFlag = 0;
                setInfo.RecSetting.PittariFlag = 0;
            }

            DeleteEnd = false;
            DialogResult = true;
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            DeleteEnd = false;
            DialogResult = false;
        }


        private void button_dlete_Click(object sender, RoutedEventArgs e)
        {
            DeleteEnd = true;
            DialogResult = true;
        }
        
        public void SetEpgEventInfo(EpgEventInfo eventInfo)
        {
            tabItem_pgInfo.Visibility = System.Windows.Visibility.Visible;

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
                if (eventInfo.ContentInfo != null)
                {
                    foreach (EpgContentData info in eventInfo.ContentInfo.nibbleList)
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
            textBox_pgInfo.Text = basicInfo;
            textBox_pgInfo.Text += extInfo;
        }
    }
}
