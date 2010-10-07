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
using System.ComponentModel;

using CtrlCmdCLI.Def;

namespace EpgTimer
{
    /// <summary>
    /// EpgProgramViewItem.xaml の相互作用ロジック
    /// </summary>
    public partial class EpgProgramViewItem : UserControl
    {
        public EpgProgramViewItem()
        {
            InitializeComponent();
        }

        public void SetProgramInfo(ProgramViewItem info)
        {
            this.DataContext = info;
            textBlock.DataContext = info;
        }
    }

    public class ProgramViewItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private Int32 width = 10;
        private Int32 height = 10;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public ProgramViewItem()
        {
        }
        public ProgramViewItem(EpgEventInfo info)
        {
            EventInfo = info;
        }

        public void SendUpdateColorNotify()
        {
            NotifyPropertyChanged("ContentColor");
        }

        public EpgEventInfo EventInfo
        {
            get;
            set;
        }

        public Int32 Width
        {
            get { return width; }
            set
            {
                width = value;
                NotifyPropertyChanged("Width");
            }
        }

        public Int32 Height
        {
            get { return height; }
            set
            {
                height = value;
                NotifyPropertyChanged("Height");
            }
        }

        public Int32 LeftPos
        {
            get;
            set;
        }

        public Int32 TopPos
        {
            get;
            set;
        }
        
        public String ProgramInfo
        {
            get
            {
                String text = "";
                if (EventInfo != null)
                {
                    if (EventInfo.ShortInfo != null)
                    {
                        text += EventInfo.ShortInfo.event_name;
                    }
                }
                return text;
            }
        }

        public SolidColorBrush ContentColor
        {
            get
            {
                SolidColorBrush color = ColorDef.Instance.ColorTable["White"];
                if (EventInfo != null)
                {
                    if (EventInfo.ContentInfo != null)
                    {
                        if (EventInfo.ContentInfo.nibbleList.Count > 0)
                        {
                            try
                            {
                                string colorName = Settings.Instance.ContentColorList[EventInfo.ContentInfo.nibbleList[0].content_nibble_level_1];
                                color = ColorDef.Instance.ColorTable[colorName];
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                return color;
            }
        }
    }
}
