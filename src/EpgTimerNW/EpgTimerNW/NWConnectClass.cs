using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CtrlCmdCLI;
using CtrlCmdCLI.Def;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace EpgTimerNW
{
    public enum CtrlCmd : uint
    {
        //タイマーGUI（EpgTimer_Bon.exe）用
        CMD_TIMER_GUI_SHOW_DLG = 101, //ダイアログを前面に表示
        CMD_TIMER_GUI_UPDATE_RESERVE = 102, //予約一覧の情報が更新された
        CMD_TIMER_GUI_UPDATE_EPGDATA = 103, //EPGデータの再読み込みが完了した
        CMD_TIMER_GUI_VIEW_EXECUTE = 110, //Viewアプリ（EpgDataCap_Bon.exe）を起動
        CMD_TIMER_GUI_QUERY_SUSPEND = 120, //スタンバイ、休止、シャットダウンに入っていいかの確認をユーザーに行う（入っていいならCMD_EPG_SRV_SUSPENDを送る）
        CMD_TIMER_GUI_QUERY_REBOOT = 121, //PC再起動に入っていいかの確認をユーザーに行う（入っていいならCMD_EPG_SRV_REBOOTを送る）
        CMD_TIMER_GUI_SRV_STATUS_CHG = 130 //サーバーのステータス変更通知（1:通常、2:EPGデータ取得開始、3:予約録画開始）
    }

    public enum ErrCode : uint
    {
        CMD_SUCCESS = 1, //成功
        CMD_ERR = 0, //汎用エラー
        CMD_NEXT = 202, //Enumコマンド用、続きあり
        CMD_NON_SUPPORT = 203, //未サポートのコマンド
        CMD_ERR_INVALID_ARG = 204, //引数エラー
        CMD_ERR_CONNECT = 205, //サーバーにコネクトできなかった
        CMD_ERR_DISCONNECT = 206, //サーバーから切断された
        CMD_ERR_TIMEOUT = 207, //タイムアウト発生
        CMD_ERR_BUSY = 208, //ビジー状態で現在処理できない（EPGデータ読み込み中、録画中など）
        CMD_NO_RES = 250 //Post用でレスポンスの必要なし
    }
    
    public class CMD_STREAM
    {
        public uint uiParam;
        public uint uiSize;
        public byte[] bData;

        public CMD_STREAM()
        {
            uiParam = 0;
            uiSize = 0;
            bData = null;
        }
    }

    public delegate int CMD_CALLBACK_PROC(object pParam, CMD_STREAM pCmdParam, ref CMD_STREAM pResParam);

    class NWConnect
    {
        private CMD_CALLBACK_PROC cmdProc = null;
        private object cmdParam = null;

        private bool connectFlag;
        private UInt32 serverPort;
        private TcpListener server = null;

        private String connectedIP;

        public CtrlCmdUtil cmd;

        public bool IsConnected
        {
            get
            {
                return connectFlag;
            }
        }

        public String ConnectedIP
        {
            get
            {
                return connectedIP;
            }
        }

        private static NWConnect _instance;
        public static NWConnect Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NWConnect();
                return _instance;
            }
            set { _instance = value; }
        }

        public NWConnect()
        {
            connectFlag = false;
            cmd = EpgTimer.EpgTimerDef.Instance.CtrlCmd;
        }

        public static void SendMagicPacket(byte[] physicalAddress)
        {
            SendMagicPacket(IPAddress.Broadcast, physicalAddress);
        }

        private static void SendMagicPacket(IPAddress broad, byte[] physicalAddress)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            for (int i = 0; i < 6; i++)
            {
                writer.Write((byte)0xff);
            }
            for (int i = 0; i < 16; i++)
            {
                writer.Write(physicalAddress);
            }

            UdpClient client = new UdpClient();
            client.EnableBroadcast = true;
            client.Send(stream.ToArray(), (int)stream.Position, new IPEndPoint(broad, 0));
        }

        public bool ConnectServer(String srvIP, UInt32 srvPort, UInt32 waitPort, CMD_CALLBACK_PROC pfnCmdProc, object pParam)
        {
            if (srvIP.Length == 0)
            {
                return false;
            }
            connectFlag = false;

            cmdProc = pfnCmdProc;
            cmdParam = pParam;
            StartTCPServer(waitPort);

            cmd.SetSendMode(true);
            cmd.SetNWSetting(srvIP, srvPort);
            if (cmd.SendRegistTCP(waitPort) != 1)
            {
                return false;
            }
            else
            {
                connectFlag = true;
                connectedIP = srvIP;
                return true;
            }
        }

        private bool StartTCPServer(UInt32 port)
        {
            if (serverPort == port && server != null)
            {
                return true;
            }
            if (server != null)
            {
                server.Stop();
                server = null;
            }
            serverPort = port;
            server = new TcpListener(IPAddress.Any, (int)port);
            server.Start();
            server.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), server);

            return true;
        }

        private bool StopTCPServer()
        {
            if (server != null)
            {
                server.Stop();
                server = null;
            } 
            return true;
        }

        public void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;

            TcpClient client = listener.EndAcceptTcpClient(ar);
            client.ReceiveBufferSize = 1024 * 1024;

            NetworkStream stream = client.GetStream();

            CMD_STREAM stCmd = new CMD_STREAM();
            CMD_STREAM stRes = new CMD_STREAM();
            //コマンド受信
            if (cmdProc != null)
            {
                byte[] bHead = new byte[8];

                if (stream.Read(bHead, 0, bHead.Length) == 8)
                {
                    stCmd.uiParam = BitConverter.ToUInt32(bHead, 0);
                    stCmd.uiSize = BitConverter.ToUInt32(bHead, 4);
                    if (stCmd.uiSize > 0)
                    {
                        stCmd.bData = new Byte[stCmd.uiSize];
                    }
                    int readSize = 0;
                    while (readSize < stCmd.uiSize)
                    {
                        readSize += stream.Read(stCmd.bData, readSize, (int)stCmd.uiSize);
                    }
                    cmdProc.Invoke(cmdParam, stCmd, ref stRes);

                    Array.Copy(BitConverter.GetBytes(stRes.uiParam), 0, bHead, 0, sizeof(uint));
                    Array.Copy(BitConverter.GetBytes(stRes.uiSize), 0, bHead, 4, sizeof(uint));
                    stream.Write(bHead, 0, 8);
                    if (stRes.uiSize > 0)
                    {
                        stream.Write(stRes.bData, 0, (int)stRes.uiSize);
                    }
                }
            }
            else
            {
                stRes.uiSize = 0;
                stRes.uiParam = 1;
            }
            stream.Dispose();
            client.Client.Close();

            server.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), server);
        }
    
    }
}
