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
    /// EpgTimeView.xaml の相互作用ロジック
    /// </summary>
    public partial class EpgTimeView : UserControl
    {
        public EpgTimeView()
        {
            InitializeComponent();
        }

        public void SetTime(DateTime startTime, DateTime endTime)
        {
            stackPanel_time.Children.Clear();
            DateTime itemTime = startTime;
            while (itemTime < endTime)
            {
                TextBlock item = new TextBlock();
                double height = Settings.Instance.MinHeight;
                if (height < 1)
                {
                    height = 0.5;
                }
                item.Height = (60 * height) - 4;
                if (itemTime.Hour % 3 == 0)
                {
                    if (height < 1)
                    {
                        item.Text = itemTime.ToString("M/d\r\nH");
                    }
                    else if (height < 1.5)
                    {
                        item.Text = itemTime.ToString("M/d\r\n(ddd)\r\nH");
                    }
                    else
                    {
                        item.Text = itemTime.ToString("M/d\r\n(ddd)\r\n\r\nH");
                    }
                }
                else
                {
                    if (height < 1)
                    {
                        item.Text = itemTime.Hour.ToString();
                    }
                    else if (height < 1.5)
                    {
                        item.Text = itemTime.ToString("\r\nH");
                    }
                    else
                    {
                        item.Text = itemTime.ToString("\r\n\r\n\r\nH");
                    }
                    
                }
                if (itemTime.DayOfWeek == DayOfWeek.Saturday)
                {
                    item.Foreground = Brushes.Blue;
                }
                else if (itemTime.DayOfWeek == DayOfWeek.Sunday)
                {
                    item.Foreground = Brushes.Red;
                }
                item.Margin = new Thickness(2, 2, 2, 2);
                item.Background = Brushes.AliceBlue;
                item.TextAlignment = TextAlignment.Center;
                item.FontSize = 12;
                itemTime = itemTime.AddHours(1);
                stackPanel_time.Children.Add(item);
            }
        }

        private void scrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }
    }
}
