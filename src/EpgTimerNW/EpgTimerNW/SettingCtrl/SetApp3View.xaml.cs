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
    /// SetApp3View.xaml の相互作用ロジック
    /// </summary>
    public partial class SetApp3View : UserControl
    {
        List<ViewMenuItem> buttonItem = new List<ViewMenuItem>();
        List<ViewMenuItem> taskItem = new List<ViewMenuItem>();

        public SetApp3View()
        {
            InitializeComponent();

            buttonItem.Add(new ViewMenuItem("（空白）", false));
            buttonItem.Add(new ViewMenuItem("設定", false));
            buttonItem.Add(new ViewMenuItem("再接続", false));
            buttonItem.Add(new ViewMenuItem("検索", false));
            buttonItem.Add(new ViewMenuItem("スタンバイ", false));
            buttonItem.Add(new ViewMenuItem("休止", false)); 
            buttonItem.Add(new ViewMenuItem("EPG取得", false));
            buttonItem.Add(new ViewMenuItem("EPG再読み込み", false));
            buttonItem.Add(new ViewMenuItem("終了", false));
            buttonItem.Add(new ViewMenuItem("NetworkTV終了", false));
            buttonItem.Add(new ViewMenuItem("カスタム１", false));
            buttonItem.Add(new ViewMenuItem("カスタム２", false));

            taskItem.Add(new ViewMenuItem("（セパレータ）", false));
            taskItem.Add(new ViewMenuItem("設定", false));
            taskItem.Add(new ViewMenuItem("再接続", false));
            taskItem.Add(new ViewMenuItem("EPG取得", false));
            taskItem.Add(new ViewMenuItem("終了", false));

            try
            {
                foreach (String info in Settings.Instance.ViewButtonList)
                {
                    //.NET的に同一文字列のStringを入れると選択動作がおかしくなるみたいなので毎回作成しておく
                    listBox_viewBtn.Items.Add(new ViewMenuItem(info, true));
                    if (String.Compare(info, "（空白）") != 0)
                    {
                        foreach (ViewMenuItem item in buttonItem)
                        {
                            if (String.Compare(info, item.MenuName) == 0)
                            {
                                item.IsSelected = true;
                                break;
                            }
                        }
                    }
                }
                foreach (String info in Settings.Instance.TaskMenuList)
                {
                    //.NET的に同一文字列のStringを入れると選択動作がおかしくなるみたいなので毎回作成しておく
                    listBox_viewTask.Items.Add(new ViewMenuItem(info, true));
                    if (String.Compare(info, "（セパレータ）") != 0)
                    {
                        foreach (ViewMenuItem item in taskItem)
                        {
                            if (String.Compare(info, item.MenuName) == 0)
                            {
                                item.IsSelected = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            ReLoadButtonItem();
            ReLoadTaskItem();
        }

        public void SaveSetting()
        {
            Settings.Instance.ViewButtonList.Clear();
            foreach (ViewMenuItem info in listBox_viewBtn.Items)
            {
                Settings.Instance.ViewButtonList.Add(info.MenuName);
            }

            Settings.Instance.TaskMenuList.Clear();
            foreach (ViewMenuItem info in listBox_viewTask.Items)
            {
                Settings.Instance.TaskMenuList.Add(info.MenuName);
            }
        }

        private void ReLoadButtonItem()
        {
            listBox_itemBtn.Items.Clear();
            foreach (ViewMenuItem info in buttonItem)
            {
                if (info.IsSelected == false)
                {
                    listBox_itemBtn.Items.Add(info);
                }
            }
        }
        
        private void ReLoadTaskItem()
        {
            listBox_itemTask.Items.Clear();
            foreach (ViewMenuItem info in taskItem)
            {
                if (info.IsSelected == false)
                {
                    listBox_itemTask.Items.Add(info);
                }
            }
        }

        private void button_btnUp_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_viewBtn.SelectedItem != null)
            {
                if (listBox_viewBtn.SelectedIndex >= 1)
                {
                    object temp = listBox_viewBtn.SelectedItem;
                    int index = listBox_viewBtn.SelectedIndex;
                    listBox_viewBtn.Items.RemoveAt(listBox_viewBtn.SelectedIndex);
                    listBox_viewBtn.Items.Insert(index - 1, temp);
                    listBox_viewBtn.SelectedIndex = index - 1;
                }
            }
        }

        private void button_btnDown_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_viewBtn.SelectedItem != null)
            {
                if (listBox_viewBtn.SelectedIndex < listBox_viewBtn.Items.Count - 1)
                {
                    object temp = listBox_viewBtn.SelectedItem;
                    int index = listBox_viewBtn.SelectedIndex;
                    listBox_viewBtn.Items.RemoveAt(listBox_viewBtn.SelectedIndex);
                    listBox_viewBtn.Items.Insert(index + 1, temp);
                    listBox_viewBtn.SelectedIndex = index + 1;
                }
            }
        }

        private void button_btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_viewBtn.SelectedItem != null)
            {
                ViewMenuItem info = listBox_viewBtn.SelectedItem as ViewMenuItem;
                if (String.Compare(info.MenuName, "設定") == 0)
                {
                    MessageBox.Show("設定は非表示にすることができません");
                    return;
                }
                if (String.Compare(info.MenuName, "（空白）") != 0)
                {
                    foreach (ViewMenuItem item in buttonItem)
                    {
                        if (String.Compare(info.MenuName, item.MenuName) == 0)
                        {
                            item.IsSelected = false;
                            break;
                        }
                    }
                }
                listBox_viewBtn.Items.Remove(info);
                ReLoadButtonItem();
            }
        }

        private void button_btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_itemBtn.SelectedItem != null)
            {
                ViewMenuItem info = listBox_itemBtn.SelectedItem as ViewMenuItem;
                if (String.Compare(info.MenuName, "（空白）") != 0)
                {
                    info.IsSelected = true;
                }
                listBox_viewBtn.Items.Add(new ViewMenuItem(info.MenuName, true));
                ReLoadButtonItem();
            }

        }

        private void button_taskUp_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_viewTask.SelectedItem != null)
            {
                if (listBox_viewTask.SelectedIndex >= 1)
                {
                    object temp = listBox_viewTask.SelectedItem;
                    int index = listBox_viewTask.SelectedIndex;
                    listBox_viewTask.Items.RemoveAt(listBox_viewTask.SelectedIndex);
                    listBox_viewTask.Items.Insert(index - 1, temp);
                    listBox_viewTask.SelectedIndex = index - 1;
                }
            }
        }

        private void button_taskDown_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_viewTask.SelectedItem != null)
            {
                if (listBox_viewTask.SelectedIndex < listBox_viewTask.Items.Count - 1)
                {
                    object temp = listBox_viewTask.SelectedItem;
                    int index = listBox_viewTask.SelectedIndex;
                    listBox_viewTask.Items.RemoveAt(listBox_viewTask.SelectedIndex);
                    listBox_viewTask.Items.Insert(index + 1, temp);
                    listBox_viewTask.SelectedIndex = index + 1;
                }
            }
        }

        private void button_taskDel_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_viewTask.SelectedItem != null)
            {
                ViewMenuItem info = listBox_viewTask.SelectedItem as ViewMenuItem;
                if (String.Compare(info.MenuName, "（セパレータ）") != 0)
                {
                    foreach (ViewMenuItem item in taskItem)
                    {
                        if (String.Compare(info.MenuName, item.MenuName) == 0)
                        {
                            item.IsSelected = false;
                            break;
                        }
                    }
                }
                listBox_viewTask.Items.Remove(info);
                ReLoadTaskItem();
            }
        }

        private void button_taskAdd_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_itemTask.SelectedItem != null)
            {
                ViewMenuItem info = listBox_itemTask.SelectedItem as ViewMenuItem;
                if (String.Compare(info.MenuName, "（セパレータ）") != 0)
                {
                    info.IsSelected = true;
                }
                listBox_viewTask.Items.Add(new ViewMenuItem(info.MenuName, true));
                ReLoadTaskItem();
            }
        }

    }

    class ViewMenuItem
    {
        public ViewMenuItem(String name, bool select)
        {
            MenuName = name;
            IsSelected = select;
        }

        public bool IsSelected
        {
            get;
            set;
        }
        public String MenuName
        { 
            get;
            set;
        }
        public override string ToString()
        {
            return MenuName;
        }
    }
}
