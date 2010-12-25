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

using CtrlCmdCLI;
using CtrlCmdCLI.Def;

namespace EpgTimer
{
    /// <summary>
    /// SetTwitterView.xaml の相互作用ロジック
    /// </summary>
    public partial class SetTwitterView : UserControl
    {
        TwitterPlugInUtil util = new TwitterPlugInUtil();

        public SetTwitterView()
        {
            InitializeComponent();

            util.Initialize();

            if (IniFileHandler.GetPrivateProfileInt("TWITTER", "use", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_useTwitter.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("TWITTER", "useProxy", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_useProxy.IsChecked = true;
            }

            StringBuilder buff = new StringBuilder(1024);
            buff.Clear();
            IniFileHandler.GetPrivateProfileString("TWITTER", "ProxyServer", "", buff, 1024, SettingPath.TimerSrvIniPath);
            textBox_proxy_addr.Text = buff.ToString();
            buff.Clear();
            IniFileHandler.GetPrivateProfileString("TWITTER", "ProxyID", "", buff, 1024, SettingPath.TimerSrvIniPath);
            textBox_proxy_id.Text = buff.ToString();
            buff.Clear();
            IniFileHandler.GetPrivateProfileString("TWITTER", "ProxyPWD", "", buff, 1024, SettingPath.TimerSrvIniPath);
            textBox_proxy_pwd.Password = buff.ToString();

            buff.Clear();
            IniFileHandler.GetPrivateProfileString("TWITTER_MACRO", "MACRO1", "●EPG読み込み：$ServiceNameOLD$ $SDYYYYOLD$/$SDMOLD$/$SDDOLD$ $STHOLD$:$STMOLD$ ～ $ETHOLD$:$ETMOLD$ EventID:0x$EID16OLD$ → $SDYYYYNEW$/$SDMNEW$/$SDDNEW$ $STHNEW$:$STMNEW$ ～ $ETHNEW$:$ETMNEW$ EventID:0x$EID16NEW$ $TitleNEW$", buff, 1024, SettingPath.TimerSrvIniPath);
            textBox_tweet1.Text = buff.ToString();
            buff.Clear();
            IniFileHandler.GetPrivateProfileString("TWITTER_MACRO", "MACRO2", "●追従：$ServiceNameOLD$ $SDYYYYOLD$/$SDMOLD$/$SDDOLD$ $STHOLD$:$STMOLD$ ～ $ETHOLD$:$ETMOLD$ → $SEYMDHMNEW$ $TitleNEW$", buff, 1024, SettingPath.TimerSrvIniPath);
            textBox_tweet2.Text = buff.ToString();
            buff.Clear();
            IniFileHandler.GetPrivateProfileString("TWITTER_MACRO", "MACRO3", "●予約追加：$ServiceNameNEW$ $SDYYYYNEW$/$SDMNEW$/$SDDNEW$ $STHNEW$:$STMNEW$ ～ $ETHNEW$:$ETMNEW$ $TitleNEW$", buff, 1024, SettingPath.TimerSrvIniPath);
            textBox_tweet3.Text = buff.ToString();
            buff.Clear();
            IniFileHandler.GetPrivateProfileString("TWITTER_MACRO", "MACRO4", "●録画開始：$ServiceNameNEW$ $SDYYYYNEW$/$SDMNEW$/$SDDNEW$ $STHNEW$:$STMNEW$ ～ $ETHNEW$:$ETMNEW$ $TitleNEW$", buff, 1024, SettingPath.TimerSrvIniPath);
            textBox_tweet4.Text = buff.ToString();
            buff.Clear();
            IniFileHandler.GetPrivateProfileString("TWITTER_MACRO", "MACRO5", "●録画終了：$ServiceNameNEW$ $SDYYYYNEW$/$SDMNEW$/$SDDNEW$ $STHNEW$:$STMNEW$ ～ $ETHNEW$:$ETMNEW$ $Result$ Drops:$Drops$ Scrambles:$Scrambles$ $TitleNEW$", buff, 1024, SettingPath.TimerSrvIniPath);
            textBox_tweet5.Text = buff.ToString();

            if (IniFileHandler.GetPrivateProfileInt("TWITTER_MACRO", "Use1", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_tweet1.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("TWITTER_MACRO", "Use2", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_tweet2.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("TWITTER_MACRO", "Use3", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_tweet3.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("TWITTER_MACRO", "Use4", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_tweet4.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("TWITTER_MACRO", "Use5", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_tweet5.IsChecked = true;
            }
        }

        public void SaveSetting()
        {
            if (checkBox_useTwitter.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("TWITTER", "use", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("TWITTER", "use", "0", SettingPath.TimerSrvIniPath);
            }
            if (checkBox_useProxy.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("TWITTER", "useProxy", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("TWITTER", "useProxy", "0", SettingPath.TimerSrvIniPath);
            }

            IniFileHandler.WritePrivateProfileString("TWITTER", "ProxyServer", textBox_proxy_addr.Text, SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("TWITTER", "ProxyID", textBox_proxy_id.Text, SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("TWITTER", "ProxyPWD", textBox_proxy_pwd.Password, SettingPath.TimerSrvIniPath);

            IniFileHandler.WritePrivateProfileString("TWITTER_MACRO", "MACRO1", textBox_tweet1.Text, SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("TWITTER_MACRO", "MACRO2", textBox_tweet2.Text, SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("TWITTER_MACRO", "MACRO3", textBox_tweet3.Text, SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("TWITTER_MACRO", "MACRO4", textBox_tweet4.Text, SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("TWITTER_MACRO", "MACRO5", textBox_tweet5.Text, SettingPath.TimerSrvIniPath);

            if (checkBox_tweet1.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("TWITTER_MACRO", "Use1", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("TWITTER_MACRO", "Use1", "0", SettingPath.TimerSrvIniPath);
            }
            if (checkBox_tweet2.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("TWITTER_MACRO", "Use2", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("TWITTER_MACRO", "Use2", "0", SettingPath.TimerSrvIniPath);
            }
            if (checkBox_tweet3.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("TWITTER_MACRO", "Use3", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("TWITTER_MACRO", "Use3", "0", SettingPath.TimerSrvIniPath);
            }
            if (checkBox_tweet4.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("TWITTER_MACRO", "Use4", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("TWITTER_MACRO", "Use4", "0", SettingPath.TimerSrvIniPath);
            }
            if (checkBox_tweet5.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("TWITTER_MACRO", "Use5", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("TWITTER_MACRO", "Use5", "0", SettingPath.TimerSrvIniPath);
            }

        }

        private void button_getIESet_Click(object sender, RoutedEventArgs e)
        {
            CurrentUserIEProxyConfig ieConfig = new CurrentUserIEProxyConfig();
            textBox_proxy_addr.Text = "";
            if (util.GetIEProxyConfig(ref ieConfig) == true)
            {
                if (ieConfig.proxy.Length == 0)
                {
                    ProxyConfig proxyConfig = new ProxyConfig();
                    if (ieConfig.autoDetect == 1)
                    {
                        util.GetProxyAutoDetect(ref proxyConfig);
                        if (proxyConfig.proxy.Length > 0)
                        {
                            textBox_proxy_addr.Text = proxyConfig.proxy;
                        }
                    }
                    if (proxyConfig.proxy.Length == 0 && ieConfig.autoConfigUrl.Length > 0)
                    {
                        util.GetProxyAutoScript(ieConfig.autoConfigUrl, ref proxyConfig);
                        if (proxyConfig.proxy.Length > 0)
                        {
                            textBox_proxy_addr.Text = proxyConfig.proxy;
                        }
                    }
                }
                else
                {
                    textBox_proxy_addr.Text = ieConfig.proxy;
                }
            }
        }

        private void button_getUrl_Click(object sender, RoutedEventArgs e)
        {
            UseProxyInfo proxyInfo = new UseProxyInfo();
            if (checkBox_useProxy.IsChecked == true)
            {
                proxyInfo.serverName = textBox_proxy_addr.Text;
                proxyInfo.userName = textBox_proxy_id.Text;
                proxyInfo.password = textBox_proxy_pwd.Password;
                util.SetProxy(true, proxyInfo);
            }
            else
            {
                util.SetProxy(false, proxyInfo);
            }
            String url = "";
            if (util.GetAuthorizationUrl(ref url) == true)
            {
                textBox_url.Text = url;
            }
            else
            {
                MessageBox.Show("取得できませんでした。ネットワークの状態を確認してください");
            }
        }

        private void button_number_Click(object sender, RoutedEventArgs e)
        {
            if (util.SetAuthorizationPWD(textBox_number.Text) == true)
            {
                MessageBox.Show("認証が完了しました。");
            }
            else
            {
                MessageBox.Show("認証できませんでした。ネットワークの状態を確認してください。");
            }
        }

        private void button_test_Click(object sender, RoutedEventArgs e)
        {
            UseProxyInfo proxyInfo = new UseProxyInfo();
            if (checkBox_useProxy.IsChecked == true)
            {
                proxyInfo.serverName = textBox_proxy_addr.Text;
                proxyInfo.userName = textBox_proxy_id.Text;
                proxyInfo.password = textBox_proxy_pwd.Password;
                util.SetProxy(true, proxyInfo);
            }
            else
            {
                util.SetProxy(false, proxyInfo);
            }
            if (util.SendTweet(false, textBox_test.Text) == true)
            {
                MessageBox.Show("送信しました。送信内容を確認してください。");
            }
            else
            {
                MessageBox.Show("送信できませんでした。ネットワークの状態を確認してください。");
            }
        }
    }
}
