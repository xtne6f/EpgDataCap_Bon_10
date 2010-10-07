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
    /// SetApp2DefRecWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SetApp2DefRecWindow : Window
    {
        public RecSettingData recSet = new RecSettingData();
        public SetApp2DefRecWindow()
        {
            InitializeComponent();

            recSettingView.VisibleAddReserve(false);
            recSettingView.VisibleAddAutoAdd(false);

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            recSettingView.GetRecSetting(ref recSet);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            recSettingView.SetDefRecSetting(recSet);
        }
    }
}
