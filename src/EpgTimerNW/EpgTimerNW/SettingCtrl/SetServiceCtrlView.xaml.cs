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
    /// SetServiceCtrlView.xaml の相互作用ロジック
    /// </summary>
    public partial class SetServiceCtrlView : UserControl
    {
        public bool ServiceStop = false;
        public SetServiceCtrlView()
        {
            InitializeComponent();

            UpdateBtn();
        }

        private void UpdateBtn()
        {
            if (ServiceCtrlClass.ServiceIsInstalled("EpgTimer Service") == false)
            {
                button_inst.IsEnabled = true;
                button_uninst.IsEnabled = false;
                button_stop.IsEnabled = false;
            }
            else
            {
                button_inst.IsEnabled = false;
                button_uninst.IsEnabled = true;
                if (ServiceCtrlClass.IsStarted("EpgTimer Service") == true)
                {
                    button_stop.IsEnabled = true;
                }
                else
                {
                    button_stop.IsEnabled = false;
                }
            }
        }

        private void button_inst_Click(object sender, RoutedEventArgs e)
        {
            String exePath = SettingPath.ModulePath + "\\EpgTimerSrv.exe";
            if (ServiceCtrlClass.Install("EpgTimer Service", "EpgTimer Service", exePath) == false)
            {
                MessageBox.Show("インストールに失敗しました。\r\nVista以降のOSでは、管理者権限で起動されている必要があります。");
            }
            UpdateBtn();
        }

        private void button_uninst_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceCtrlClass.Uninstall("EpgTimer Service") == false)
            {
                MessageBox.Show("アンインストールに失敗しました。\r\nVista以降のOSでは、管理者権限で起動されている必要があります。");
            }
            else
            {
                ServiceStop = true;
            }
            UpdateBtn();
        }

        private void button_stop_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceCtrlClass.StopService("EpgTimer Service") == false)
            {
                MessageBox.Show("サービスの停止に失敗しました。\r\nVista以降のOSでは、管理者権限で起動されている必要があります。");
            }
            else
            {
                ServiceStop = true;
            }
            UpdateBtn();
        }
    }
}
