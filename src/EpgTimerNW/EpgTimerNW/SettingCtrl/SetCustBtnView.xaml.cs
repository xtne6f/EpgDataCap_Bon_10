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
    /// SetCustBtnView.xaml の相互作用ロジック
    /// </summary>
    public partial class SetCustBtnView : UserControl
    {
        public SetCustBtnView()
        {
            InitializeComponent();

            try
            {
                textBox_name1.Text = Settings.Instance.Cust1BtnName;
                textBox_exe1.Text = Settings.Instance.Cust1BtnCmd;
                textBox_opt1.Text = Settings.Instance.Cust1BtnCmdOpt;

                textBox_name2.Text = Settings.Instance.Cust2BtnName;
                textBox_exe2.Text = Settings.Instance.Cust2BtnCmd;
                textBox_opt2.Text = Settings.Instance.Cust2BtnCmdOpt;
            }
            catch
            {
            }
        }

        private void button_exe1_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".exe";
            dlg.Filter = "exe Files (.exe)|*.exe;|all Files(*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                textBox_exe1.Text = dlg.FileName;
            }
        }

        private void button_exe2_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".exe";
            dlg.Filter = "exe Files (.exe)|*.exe;|all Files(*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                textBox_exe2.Text = dlg.FileName;
            }
        }
        
        public void SaveSetting()
        {
            Settings.Instance.Cust1BtnName = textBox_name1.Text;
            Settings.Instance.Cust1BtnCmd = textBox_exe1.Text;
            Settings.Instance.Cust1BtnCmdOpt = textBox_opt1.Text;

            Settings.Instance.Cust2BtnName = textBox_name2.Text;
            Settings.Instance.Cust2BtnCmd = textBox_exe2.Text;
            Settings.Instance.Cust2BtnCmdOpt = textBox_opt2.Text;

        }
    }
}
