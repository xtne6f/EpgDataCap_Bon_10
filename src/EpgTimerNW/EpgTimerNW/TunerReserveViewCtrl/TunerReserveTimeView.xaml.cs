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

namespace EpgTimer
{
    /// <summary>
    /// TunerReserveTimeView.xaml の相互作用ロジック
    /// </summary>
    public partial class TunerReserveTimeView : UserControl
    {
        public TunerReserveTimeView()
        {
            InitializeComponent();
        }

        public void SetTime(System.Collections.SortedList timeList)
        {
            stackPanel_time.Children.Clear();
            foreach (TimePosInfo info in timeList.Values)
            {
                TextBlock item = new TextBlock();
                item.Height = (60 * 2) - 4;
                item.Text = info.Time.ToString("M/d\r\n(ddd)\r\n\r\nH");
                if (info.Time.DayOfWeek == DayOfWeek.Saturday)
                {
                    item.Foreground = Brushes.Blue;
                }
                else if (info.Time.DayOfWeek == DayOfWeek.Sunday)
                {
                    item.Foreground = Brushes.Red;
                }
                item.Margin = new Thickness(2, 2, 2, 2);
                item.Background = Brushes.AliceBlue;
                item.TextAlignment = TextAlignment.Center;
                item.FontSize = 12;
                stackPanel_time.Children.Add(item);
            }
        }
        
        private void scrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }
    }
}
