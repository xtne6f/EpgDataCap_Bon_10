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
    /// SetEpgServiceView.xaml の相互作用ロジック
    /// </summary>
    public partial class SetEpgServiceView : UserControl
    {
        private CtrlCmdUtil cmd = EpgTimerDef.Instance.CtrlCmd;
        public SetEpgServiceView()
        {
            InitializeComponent();

            List<ChSet5Item> tereList = new List<ChSet5Item>();
            List<ChSet5Item> bsList = new List<ChSet5Item>();
            List<ChSet5Item> csList = new List<ChSet5Item>();
            List<ChSet5Item> otherList = new List<ChSet5Item>();

            List<ChSet5Item> onTereList = new List<ChSet5Item>();
            List<ChSet5Item> onBsList = new List<ChSet5Item>();
            List<ChSet5Item> onCsList = new List<ChSet5Item>();
            List<ChSet5Item> onOtherList = new List<ChSet5Item>();
            try
            {
                textBox_mouse_scroll.Text = Settings.Instance.ScrollSize.ToString();
                textBox_minHeight.Text = Settings.Instance.MinHeight.ToString();
                textBox_service_width.Text = Settings.Instance.ServiceWidth.ToString();
                textBox_dragScroll.Text = Settings.Instance.DragScroll.ToString();

                onTereList = Settings.Instance.OnTereViewServiceList;
                onBsList = Settings.Instance.OnBsViewServiceList;
                onCsList = Settings.Instance.OnCsViewServiceList;
                onOtherList = Settings.Instance.OnOtherViewServiceList;

                Dictionary<UInt64, ChSet5Item> noViewList = new Dictionary<ulong, ChSet5Item>();
                foreach (ChSet5Item info in Settings.Instance.OffViewServiceList)
                {
                    noViewList.Add(info.Key, info);
                }

                foreach (ChSet5Item info in ChSet5.Instance.ChList.Values)
                {
                    if (info.ONID == 4)
                    {
                        bsList.Add(info);
                        if (noViewList.ContainsKey(info.Key) == false)
                        {
                            bool find = false;
                            foreach (ChSet5Item item in onBsList)
                            {
                                if (item.Key == info.Key)
                                {
                                    find = true;
                                    break;
                                }
                            }
                            if (find == false)
                            {
                                onBsList.Add(info);
                            }
                        }
                    }
                    else if (info.ONID == 6 || info.ONID == 7)
                    {
                        csList.Add(info);
                        if (noViewList.ContainsKey(info.Key) == false)
                        {
                            bool find = false;
                            foreach (ChSet5Item item in onCsList)
                            {
                                if (item.Key == info.Key)
                                {
                                    find = true;
                                    break;
                                }
                            }
                            if (find == false)
                            {
                                onCsList.Add(info);
                            }
                        }
                    }
                    else if (0x7880 <= info.ONID && info.ONID <= 0x7FE8)
                    {
                        tereList.Add(info);
                        if (noViewList.ContainsKey(info.Key) == false)
                        {
                            bool find = false;
                            foreach (ChSet5Item item in onTereList)
                            {
                                if (item.Key == info.Key)
                                {
                                    find = true;
                                    break;
                                }
                            }
                            if (find == false)
                            {
                                onTereList.Add(info);
                            }
                        }
                    }
                    else
                    {
                        otherList.Add(info);
                        if (noViewList.ContainsKey(info.Key) == false)
                        {
                            bool find = false;
                            foreach (ChSet5Item item in onOtherList)
                            {
                                if (item.Key == info.Key)
                                {
                                    find = true;
                                    break;
                                }
                            }
                            if (find == false)
                            {
                                onOtherList.Add(info);
                            }
                        }
                    }
                }


                List<EpgServiceInfo> epgServiceList = new List<EpgServiceInfo>();
                cmd.SendEnumService(ref epgServiceList);
                foreach (EpgServiceInfo info in epgServiceList)
                {
                    UInt64 key = ((UInt64)info.ONID) << 32 | ((UInt64)info.TSID) << 16 | ((UInt64)info.SID);
                    if (ChSet5.Instance.ChList.ContainsKey(key) == false)
                    {
                        //チャンネルスキャンにないサービス
                        ChSet5Item chItem = new ChSet5Item();
                        chItem.ONID = info.ONID;
                        chItem.TSID = info.TSID;
                        chItem.SID = info.SID;
                        chItem.ServiceType = info.service_type;
                        chItem.PartialFlag = info.partialReceptionFlag;
                        chItem.ServiceName = info.service_name;
                        chItem.NetworkName = info.network_name;

                        if (info.ONID == 4)
                        {
                            bsList.Add(chItem);
                            if (noViewList.ContainsKey(chItem.Key) == false)
                            {
                                bool find = false;
                                foreach (ChSet5Item item in onBsList)
                                {
                                    if (item.Key == chItem.Key)
                                    {
                                        find = true;
                                        break;
                                    }
                                }
                                if (find == false)
                                {
                                    onBsList.Add(chItem);
                                }
                            }
                        }
                        else if (info.ONID == 6 || info.ONID == 7)
                        {
                            csList.Add(chItem);
                            if (noViewList.ContainsKey(chItem.Key) == false)
                            {
                                bool find = false;
                                foreach (ChSet5Item item in onCsList)
                                {
                                    if (item.Key == chItem.Key)
                                    {
                                        find = true;
                                        break;
                                    }
                                }
                                if (find == false)
                                {
                                    onCsList.Add(chItem);
                                }
                            }
                        }
                        else if (0x7880 <= info.ONID && info.ONID <= 0x7FE8)
                        {
                            tereList.Add(chItem);
                            if (noViewList.ContainsKey(chItem.Key) == false)
                            {
                                bool find = false;
                                foreach (ChSet5Item item in onTereList)
                                {
                                    if (item.Key == chItem.Key)
                                    {
                                        find = true;
                                        break;
                                    }
                                }
                                if (find == false)
                                {
                                    onTereList.Add(chItem);
                                }
                            }
                        }
                        else
                        {
                            otherList.Add(chItem);
                            if (noViewList.ContainsKey(chItem.Key) == false)
                            {
                                bool find = false;
                                foreach (ChSet5Item item in onOtherList)
                                {
                                    if (item.Key == chItem.Key)
                                    {
                                        find = true;
                                        break;
                                    }
                                }
                                if (find == false)
                                {
                                    onOtherList.Add(chItem);
                                }
                            }
                        }
                    }
                }

                setEpgServiceSelect_tere.SetService(onTereList, tereList);
                setEpgServiceSelect_bs.SetService(onBsList, bsList);
                setEpgServiceSelect_cs.SetService(onCsList, csList);
                setEpgServiceSelect_other.SetService(onOtherList, otherList);


            }
            catch
            {
            }
        }

        public void SaveSetting()
        {
            List<ChSet5Item> onTereList = new List<ChSet5Item>();
            List<ChSet5Item> onBsList = new List<ChSet5Item>();
            List<ChSet5Item> onCsList = new List<ChSet5Item>();
            List<ChSet5Item> onOtherList = new List<ChSet5Item>();

            List<ChSet5Item> offList = new List<ChSet5Item>();

            setEpgServiceSelect_tere.GetOnService(ref onTereList, ref offList);
            setEpgServiceSelect_bs.GetOnService(ref onBsList, ref offList);
            setEpgServiceSelect_cs.GetOnService(ref onCsList, ref offList);
            setEpgServiceSelect_other.GetOnService(ref onOtherList, ref offList);

            Settings.Instance.OnTereViewServiceList = onTereList;
            Settings.Instance.OnBsViewServiceList = onBsList;
            Settings.Instance.OnCsViewServiceList = onCsList;
            Settings.Instance.OnOtherViewServiceList = onOtherList;

            Settings.Instance.OffViewServiceList = offList;
            try
            {
                Settings.Instance.ScrollSize = Convert.ToDouble(textBox_mouse_scroll.Text);
                Settings.Instance.MinHeight = Convert.ToDouble(textBox_minHeight.Text);
                Settings.Instance.ServiceWidth = Convert.ToDouble(textBox_service_width.Text);
                Settings.Instance.DragScroll = Convert.ToDouble(textBox_dragScroll.Text);
            }
            catch
            {
            }
        }
    }
}
