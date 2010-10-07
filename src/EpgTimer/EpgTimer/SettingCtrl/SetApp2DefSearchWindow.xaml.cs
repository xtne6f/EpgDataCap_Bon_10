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

namespace EpgTimer
{
    /// <summary>
    /// SetApp2DefSearchWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SetApp2DefSearchWindow : Window
    {
        public SearchKeyInfo searchSet = new SearchKeyInfo();
        public SetApp2DefSearchWindow()
        {
            InitializeComponent();

            searchKeyView.SettingGUIMode(true);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            searchKeyView.SetDefSearchKey(searchSet);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            searchKeyView.GetSearchKey(ref searchSet);
        }
    }
}
