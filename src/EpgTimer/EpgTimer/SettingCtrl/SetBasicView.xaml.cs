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
using System.Reflection;
using System.Runtime.InteropServices;


namespace EpgTimer
{
    /// <summary>
    /// SetBasicView.xaml の相互作用ロジック
    /// </summary>
    public partial class SetBasicView : UserControl
    {
        public SetBasicView()
        {
            InitializeComponent();

            StringBuilder buff = new StringBuilder(512);
            buff.Clear();
            IniFileHandler.GetPrivateProfileString("SET", "DataSavePath", SettingPath.DefSettingFolderPath, buff, 512, SettingPath.CommonIniPath);
            textBox_setPath.Text = buff.ToString();

            string defRecExe = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + "\\EpgDataCap_Bon.exe";
            buff.Clear();
            IniFileHandler.GetPrivateProfileString("SET", "RecExePath", defRecExe, buff, 512, SettingPath.CommonIniPath);
            textBox_exe.Text = buff.ToString();

            int num = IniFileHandler.GetPrivateProfileInt("SET", "RecFolderNum", 0, SettingPath.CommonIniPath);
            if (num == 0)
            {
                listBox_recFolder.Items.Add(SettingPath.DefSettingFolderPath);
            }
            else
            {
                for (uint i = 0; i < num; i++)
                {
                    string key = "RecFolderPath" + i.ToString();
                    buff.Clear();
                    IniFileHandler.GetPrivateProfileString("SET", key, "", buff, 512, SettingPath.CommonIniPath);
                    if (buff.Length > 0)
                    {
                        listBox_recFolder.Items.Add(buff.ToString());
                    }
                }
            }
        }

        public void SaveSetting()
        {
            System.IO.Directory.CreateDirectory(textBox_setPath.Text);

            IniFileHandler.WritePrivateProfileString("SET", "DataSavePath", textBox_setPath.Text, SettingPath.CommonIniPath);
            IniFileHandler.WritePrivateProfileString("SET", "RecExePath", textBox_exe.Text, SettingPath.CommonIniPath);
            IniFileHandler.WritePrivateProfileString("SET", "RecFolderNum", listBox_recFolder.Items.Count.ToString(), SettingPath.CommonIniPath);
            for (int i = 0; i < listBox_recFolder.Items.Count; i++)
            {
                string key = "RecFolderPath" + i.ToString();
                string val = listBox_recFolder.Items[i] as string;
                IniFileHandler.WritePrivateProfileString("SET", key, val, SettingPath.CommonIniPath);
            }
        }

        private void button_setPath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "設定関係保存フォルダ";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox_setPath.Text = dlg.SelectedPath;
            }
        }

        private void button_exe_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".exe";
            dlg.Filter = "exe Files (.exe)|*.exe;|all Files(*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                textBox_exe.Text = dlg.FileName;
            }
        }

        private void button_rec_up_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_recFolder.SelectedItem != null)
            {
                if (listBox_recFolder.SelectedIndex >= 1)
                {
                    object temp = listBox_recFolder.SelectedItem;
                    int index = listBox_recFolder.SelectedIndex;
                    listBox_recFolder.Items.RemoveAt(listBox_recFolder.SelectedIndex);
                    listBox_recFolder.Items.Insert(index - 1, temp);
                    listBox_recFolder.SelectedIndex = index - 1;
                }
            }
        }

        private void button_rec_down_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_recFolder.SelectedItem != null)
            {
                if (listBox_recFolder.SelectedIndex < listBox_recFolder.Items.Count-1)
                {
                    object temp = listBox_recFolder.SelectedItem;
                    int index = listBox_recFolder.SelectedIndex;
                    listBox_recFolder.Items.RemoveAt(listBox_recFolder.SelectedIndex);
                    listBox_recFolder.Items.Insert(index + 1, temp);
                    listBox_recFolder.SelectedIndex = index + 1;
                }
            }

        }

        private void button_rec_del_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_recFolder.SelectedItem != null)
            {
                listBox_recFolder.Items.RemoveAt(listBox_recFolder.SelectedIndex);
            }
        }

        private void button_rec_open_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "録画フォルダ";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox_recFolder.Text = dlg.SelectedPath;
            }
        }

        private void button_rec_add_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(textBox_recFolder.Text) == false)
            {
                foreach (String info in listBox_recFolder.Items)
                {
                    if (String.Compare(textBox_recFolder.Text, info, true) == 0)
                    {
                        MessageBox.Show("すでに追加されています");
                        return;
                    }
                }
                listBox_recFolder.Items.Add(textBox_recFolder.Text);
            }
        }

        private void button_shortCut_Click(object sender, RoutedEventArgs e)
        {
            Assembly myAssembly = Assembly.GetEntryAssembly();
            string targetPath = myAssembly.Location;
            string shortcutPath = System.IO.Path.Combine(
                Environment.GetFolderPath(System.Environment.SpecialFolder.Startup),
                @"EpgTime.lnk");

            CreateShortCut(shortcutPath, targetPath, "");
        }

        /// <summary>
        /// ショートカットの作成
        /// </summary>
        /// <remarks>WSHを使用して、ショートカット(lnkファイル)を作成します。(遅延バインディング)</remarks>
        /// <param name="path">出力先のファイル名(*.lnk)</param>
        /// <param name="targetPath">対象のアセンブリ(*.exe)</param>
        /// <param name="description">説明</param>
        private void CreateShortCut(String path, String targetPath, String description)
        {
            //using System.Reflection;

            // WSHオブジェクトを作成し、CreateShortcutメソッドを実行する
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            object shell = Activator.CreateInstance(shellType);
            object shortCut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { path });

            Type shortcutType = shell.GetType();
            // TargetPathプロパティをセットする
            shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortCut, new object[] { targetPath });
            // Descriptionプロパティをセットする
            shortcutType.InvokeMember("Description", BindingFlags.SetProperty, null, shortCut, new object[] { description });
            // Saveメソッドを実行する
            shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortCut, null);

        }

    }
}
