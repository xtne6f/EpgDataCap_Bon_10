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
    /// SearchKeyWordView.xaml の相互作用ロジック
    /// </summary>
    public partial class SearchKeyWordView : UserControl
    {
        public SearchKeyWordView()
        {
            InitializeComponent();
            try
            {
                foreach (String info in Settings.Instance.AndKeyList)
                {
                    comboBox_keyword.Items.Add(info);
                }
                foreach (String info in Settings.Instance.NotKeyList)
                {
                    comboBox_notKeyword.Items.Add(info);
                }
            }
            catch
            {
            }
        }

        public void SaveSearchLog()
        {
            try
            {
                bool find = false;
                if (comboBox_keyword.Text.Length > 0)
                {
                    foreach (String info in Settings.Instance.AndKeyList)
                    {
                        if (String.Compare(comboBox_keyword.Text, info, true) == 0)
                        {
                            find = true;
                            break;
                        }
                    }
                    if (find == false)
                    {
                        Settings.Instance.AndKeyList.Add(comboBox_keyword.Text);
                        comboBox_keyword.Items.Add(comboBox_keyword.Text);
                        if (Settings.Instance.AndKeyList.Count > 30)
                        {
                            Settings.Instance.AndKeyList.RemoveAt(0);
                        }
                    }
                }

                find = false;
                if (comboBox_notKeyword.Text.Length > 0)
                {
                    foreach (String info in Settings.Instance.NotKeyList)
                    {
                        if (String.Compare(comboBox_notKeyword.Text, info, true) == 0)
                        {
                            find = true;
                            break;
                        }
                    }
                    if (find == false)
                    {
                        Settings.Instance.NotKeyList.Add(comboBox_notKeyword.Text);
                        comboBox_notKeyword.Items.Add(comboBox_notKeyword.Text);
                        if (Settings.Instance.NotKeyList.Count > 30)
                        {
                            Settings.Instance.NotKeyList.RemoveAt(0);
                        }
                    }
                }
                Settings.SaveToXmlFile();

            }
            catch
            {
            }
        }

        private void button_and_Click(object sender, RoutedEventArgs e)
        {
            KeyWordWindow dlg = new KeyWordWindow();
            dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
            dlg.KeyWord = comboBox_keyword.Text;
            dlg.ShowDialog();
            comboBox_keyword.Text = dlg.KeyWord;
        }

        private void button_not_Click(object sender, RoutedEventArgs e)
        {
            KeyWordWindow dlg = new KeyWordWindow();
            dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
            dlg.KeyWord = comboBox_notKeyword.Text;
            dlg.ShowDialog();
            comboBox_notKeyword.Text = dlg.KeyWord;
        }
    }
}
