﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CtrlCmdCLI;
using CtrlCmdCLI.Def;

namespace EpgTimer
{
    public class EpgTimerDef
    {
        public Dictionary<UInt16, ContentKindInfo> ContentKindDictionary
        {
            get;
            set;
        }
        public Dictionary<UInt16, ComponentKindInfo> ComponentKindDictionary
        {
            get;
            set;
        }
        public Dictionary<byte, DayOfWeekInfo> DayOfWeekDictionary
        {
            get;
            set;
        }
        public Dictionary<UInt16, UInt16> HourDictionary
        {
            get;
            set;
        }
        public Dictionary<UInt16, UInt16> MinDictionary
        {
            get;
            set;
        }
        public Dictionary<byte, RecModeInfo> RecModeDictionary
        {
            get;
            set;
        }
        public Dictionary<byte, YesNoInfo> YesNoDictionary
        {
            get;
            set;
        }
        public Dictionary<byte, PriorityInfo> PriorityDictionary
        {
            get;
            set;
        }
        public CtrlCmdUtil CtrlCmd
        {
            get;
            set;
        }

        private static EpgTimerDef _instance;
        public static EpgTimerDef Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EpgTimerDef();
                return _instance;
            }
            set { _instance = value; }
        }

        public EpgTimerDef()
        {
            if (CtrlCmd == null)
            {
                CtrlCmd = new CtrlCmdUtil();
            }
            if (ContentKindDictionary == null)
            {
                ContentKindDictionary = new Dictionary<UInt16, ContentKindInfo>();
                ContentKindDictionary.Add(0x00FF, new ContentKindInfo("ニュース／報道", "", 0x00, 0xFF));
                ContentKindDictionary.Add(0x0000, new ContentKindInfo("ニュース／報道", "定時・総合", 0x00, 0x00));
                ContentKindDictionary.Add(0x0001, new ContentKindInfo("ニュース／報道", "天気", 0x00, 0x01));
                ContentKindDictionary.Add(0x0002, new ContentKindInfo("ニュース／報道", "特集・ドキュメント", 0x00, 0x02));
                ContentKindDictionary.Add(0x0003, new ContentKindInfo("ニュース／報道", "政治・国会", 0x00, 0x03));
                ContentKindDictionary.Add(0x0004, new ContentKindInfo("ニュース／報道", "経済・市況", 0x00, 0x04));
                ContentKindDictionary.Add(0x0005, new ContentKindInfo("ニュース／報道", "海外・国際", 0x00, 0x05));
                ContentKindDictionary.Add(0x0006, new ContentKindInfo("ニュース／報道", "解説", 0x00, 0x06));
                ContentKindDictionary.Add(0x0007, new ContentKindInfo("ニュース／報道", "討論・会談", 0x00, 0x07));
                ContentKindDictionary.Add(0x0008, new ContentKindInfo("ニュース／報道", "報道特番", 0x00, 0x08));
                ContentKindDictionary.Add(0x0009, new ContentKindInfo("ニュース／報道", "ローカル・地域", 0x00, 0x09));
                ContentKindDictionary.Add(0x000A, new ContentKindInfo("ニュース／報道", "交通", 0x00, 0x0A));
                ContentKindDictionary.Add(0x000F, new ContentKindInfo("ニュース／報道", "その他", 0x00, 0x0F));

                ContentKindDictionary.Add(0x01FF, new ContentKindInfo("スポーツ", "", 0x01, 0xFF));
                ContentKindDictionary.Add(0x0100, new ContentKindInfo("スポーツ", "スポーツニュース", 0x01, 0x00));
                ContentKindDictionary.Add(0x0101, new ContentKindInfo("スポーツ", "野球", 0x01, 0x01));
                ContentKindDictionary.Add(0x0102, new ContentKindInfo("スポーツ", "サッカー", 0x01, 0x02));
                ContentKindDictionary.Add(0x0103, new ContentKindInfo("スポーツ", "ゴルフ", 0x01, 0x03));
                ContentKindDictionary.Add(0x0104, new ContentKindInfo("スポーツ", "その他の球技", 0x01, 0x04));
                ContentKindDictionary.Add(0x0105, new ContentKindInfo("スポーツ", "相撲・格闘技", 0x01, 0x05));
                ContentKindDictionary.Add(0x0106, new ContentKindInfo("スポーツ", "オリンピック・国際大会", 0x01, 0x06));
                ContentKindDictionary.Add(0x0107, new ContentKindInfo("スポーツ", "マラソン・陸上・水泳", 0x01, 0x07));
                ContentKindDictionary.Add(0x0108, new ContentKindInfo("スポーツ", "マリン・ウィンタースポーツ", 0x01, 0x08));
                ContentKindDictionary.Add(0x0109, new ContentKindInfo("スポーツ", "競馬・公営競技", 0x01, 0x09));
                ContentKindDictionary.Add(0x010F, new ContentKindInfo("スポーツ", "その他", 0x01, 0x0F));

                ContentKindDictionary.Add(0x02FF, new ContentKindInfo("情報／ワイドショー", "", 0x02, 0xFF));
                ContentKindDictionary.Add(0x0200, new ContentKindInfo("情報／ワイドショー", "芸能・ワイドショー", 0x02, 0x00));
                ContentKindDictionary.Add(0x0201, new ContentKindInfo("情報／ワイドショー", "ファッション", 0x02, 0x01));
                ContentKindDictionary.Add(0x0202, new ContentKindInfo("情報／ワイドショー", "暮らし・住まい", 0x02, 0x02));
                ContentKindDictionary.Add(0x0203, new ContentKindInfo("情報／ワイドショー", "健康・医療", 0x02, 0x03));
                ContentKindDictionary.Add(0x0204, new ContentKindInfo("情報／ワイドショー", "ショッピング・通販", 0x02, 0x04));
                ContentKindDictionary.Add(0x0205, new ContentKindInfo("情報／ワイドショー", "グルメ・料理", 0x02, 0x05));
                ContentKindDictionary.Add(0x0206, new ContentKindInfo("情報／ワイドショー", "イベント", 0x02, 0x06));
                ContentKindDictionary.Add(0x0207, new ContentKindInfo("情報／ワイドショー", "番組紹介・お知らせ", 0x02, 0x07));
                ContentKindDictionary.Add(0x020F, new ContentKindInfo("情報／ワイドショー", "その他", 0x02, 0x0F));
                
                ContentKindDictionary.Add(0x03FF, new ContentKindInfo("ドラマ", "", 0x03, 0xFF));
                ContentKindDictionary.Add(0x0300, new ContentKindInfo("ドラマ", "国内ドラマ", 0x03, 0x00));
                ContentKindDictionary.Add(0x0301, new ContentKindInfo("ドラマ", "海外ドラマ", 0x03, 0x01));
                ContentKindDictionary.Add(0x0302, new ContentKindInfo("ドラマ", "時代劇", 0x03, 0x02));
                ContentKindDictionary.Add(0x030F, new ContentKindInfo("ドラマ", "その他", 0x03, 0x0F));

                ContentKindDictionary.Add(0x04FF, new ContentKindInfo("音楽", "", 0x04, 0xFF));
                ContentKindDictionary.Add(0x0400, new ContentKindInfo("音楽", "国内ロック・ポップス", 0x04, 0x00));
                ContentKindDictionary.Add(0x0401, new ContentKindInfo("音楽", "海外ロック・ポップス", 0x04, 0x01));
                ContentKindDictionary.Add(0x0402, new ContentKindInfo("音楽", "クラシック・オペラ", 0x04, 0x02));
                ContentKindDictionary.Add(0x0403, new ContentKindInfo("音楽", "ジャズ・フュージョン", 0x04, 0x03));
                ContentKindDictionary.Add(0x0404, new ContentKindInfo("音楽", "歌謡曲・演歌", 0x04, 0x04));
                ContentKindDictionary.Add(0x0405, new ContentKindInfo("音楽", "ライブ・コンサート", 0x04, 0x05));
                ContentKindDictionary.Add(0x0406, new ContentKindInfo("音楽", "ランキング・リクエスト", 0x04, 0x06));
                ContentKindDictionary.Add(0x0407, new ContentKindInfo("音楽", "カラオケ・のど自慢", 0x04, 0x07));
                ContentKindDictionary.Add(0x0408, new ContentKindInfo("音楽", "民謡・邦楽", 0x04, 0x08));
                ContentKindDictionary.Add(0x0409, new ContentKindInfo("音楽", "童謡・キッズ", 0x04, 0x09));
                ContentKindDictionary.Add(0x040A, new ContentKindInfo("音楽", "民族音楽・ワールドミュージック", 0x04, 0x0A));
                ContentKindDictionary.Add(0x040F, new ContentKindInfo("音楽", "その他", 0x04, 0x0F));

                ContentKindDictionary.Add(0x05FF, new ContentKindInfo("バラエティ", "", 0x05, 0xFF));
                ContentKindDictionary.Add(0x0500, new ContentKindInfo("バラエティ", "クイズ", 0x05, 0x00));
                ContentKindDictionary.Add(0x0501, new ContentKindInfo("バラエティ", "ゲーム", 0x05, 0x01));
                ContentKindDictionary.Add(0x0502, new ContentKindInfo("バラエティ", "トークバラエティ", 0x05, 0x02));
                ContentKindDictionary.Add(0x0503, new ContentKindInfo("バラエティ", "お笑い・コメディ", 0x05, 0x03));
                ContentKindDictionary.Add(0x0504, new ContentKindInfo("バラエティ", "音楽バラエティ", 0x05, 0x04));
                ContentKindDictionary.Add(0x0505, new ContentKindInfo("バラエティ", "旅バラエティ", 0x05, 0x05));
                ContentKindDictionary.Add(0x0506, new ContentKindInfo("バラエティ", "料理バラエティ", 0x05, 0x06));
                ContentKindDictionary.Add(0x050F, new ContentKindInfo("バラエティ", "その他", 0x05, 0x0F));

                ContentKindDictionary.Add(0x06FF, new ContentKindInfo("映画", "", 0x06, 0xFF));
                ContentKindDictionary.Add(0x0600, new ContentKindInfo("映画", "洋画", 0x06, 0x00));
                ContentKindDictionary.Add(0x0601, new ContentKindInfo("映画", "邦画", 0x06, 0x01));
                ContentKindDictionary.Add(0x0602, new ContentKindInfo("映画", "アニメ", 0x06, 0x02));
                ContentKindDictionary.Add(0x060F, new ContentKindInfo("映画", "その他", 0x06, 0x0F));

                ContentKindDictionary.Add(0x07FF, new ContentKindInfo("アニメ／特撮", "", 0x07, 0xFF));
                ContentKindDictionary.Add(0x0700, new ContentKindInfo("アニメ／特撮", "国内アニメ", 0x07, 0x00));
                ContentKindDictionary.Add(0x0701, new ContentKindInfo("アニメ／特撮", "海外アニメ", 0x07, 0x01));
                ContentKindDictionary.Add(0x0702, new ContentKindInfo("アニメ／特撮", "特撮", 0x07, 0x02));
                ContentKindDictionary.Add(0x070F, new ContentKindInfo("アニメ／特撮", "その他", 0x07, 0x0F));

                ContentKindDictionary.Add(0x08FF, new ContentKindInfo("ドキュメンタリー／教養", "", 0x08, 0xFF));
                ContentKindDictionary.Add(0x0800, new ContentKindInfo("ドキュメンタリー／教養", "社会・時事", 0x08, 0x00));
                ContentKindDictionary.Add(0x0801, new ContentKindInfo("ドキュメンタリー／教養", "歴史・紀行", 0x08, 0x01));
                ContentKindDictionary.Add(0x0802, new ContentKindInfo("ドキュメンタリー／教養", "自然・動物・環境", 0x08, 0x02));
                ContentKindDictionary.Add(0x0803, new ContentKindInfo("ドキュメンタリー／教養", "宇宙・科学・医学", 0x08, 0x03));
                ContentKindDictionary.Add(0x0804, new ContentKindInfo("ドキュメンタリー／教養", "カルチャー・伝統文化", 0x08, 0x04));
                ContentKindDictionary.Add(0x0805, new ContentKindInfo("ドキュメンタリー／教養", "文学・文芸", 0x08, 0x05));
                ContentKindDictionary.Add(0x0806, new ContentKindInfo("ドキュメンタリー／教養", "スポーツ", 0x08, 0x06));
                ContentKindDictionary.Add(0x0807, new ContentKindInfo("ドキュメンタリー／教養", "ドキュメンタリー全般", 0x08, 0x07));
                ContentKindDictionary.Add(0x0808, new ContentKindInfo("ドキュメンタリー／教養", "インタビュー・討論", 0x08, 0x08));
                ContentKindDictionary.Add(0x080F, new ContentKindInfo("ドキュメンタリー／教養", "その他", 0x08, 0x0F));

                ContentKindDictionary.Add(0x09FF, new ContentKindInfo("劇場／公演", "", 0x09, 0xFF));
                ContentKindDictionary.Add(0x0900, new ContentKindInfo("劇場／公演", "現代劇・新劇", 0x09, 0x00));
                ContentKindDictionary.Add(0x0901, new ContentKindInfo("劇場／公演", "ミュージカル", 0x09, 0x01));
                ContentKindDictionary.Add(0x0902, new ContentKindInfo("劇場／公演", "ダンス・バレエ", 0x09, 0x02));
                ContentKindDictionary.Add(0x0903, new ContentKindInfo("劇場／公演", "落語・演芸", 0x09, 0x03));
                ContentKindDictionary.Add(0x0904, new ContentKindInfo("劇場／公演", "歌舞伎・古典", 0x09, 0x04));
                ContentKindDictionary.Add(0x090F, new ContentKindInfo("劇場／公演", "その他", 0x09, 0x0F));

                ContentKindDictionary.Add(0x0AFF, new ContentKindInfo("趣味／教育", "", 0x0A, 0xFF));
                ContentKindDictionary.Add(0x0A00, new ContentKindInfo("趣味／教育", "旅・釣り・アウトドア", 0x0A, 0x00));
                ContentKindDictionary.Add(0x0A01, new ContentKindInfo("趣味／教育", "園芸・ペット・手芸", 0x0A, 0x01));
                ContentKindDictionary.Add(0x0A02, new ContentKindInfo("趣味／教育", "音楽・美術・工芸", 0x0A, 0x02));
                ContentKindDictionary.Add(0x0A03, new ContentKindInfo("趣味／教育", "囲碁・将棋", 0x0A, 0x03));
                ContentKindDictionary.Add(0x0A04, new ContentKindInfo("趣味／教育", "麻雀・パチンコ", 0x0A, 0x04));
                ContentKindDictionary.Add(0x0A05, new ContentKindInfo("趣味／教育", "車・オートバイ", 0x0A, 0x05));
                ContentKindDictionary.Add(0x0A06, new ContentKindInfo("趣味／教育", "コンピュータ・ＴＶゲーム", 0x0A, 0x06));
                ContentKindDictionary.Add(0x0A07, new ContentKindInfo("趣味／教育", "会話・語学", 0x0A, 0x07));
                ContentKindDictionary.Add(0x0A08, new ContentKindInfo("趣味／教育", "幼児・小学生", 0x0A, 0x08));
                ContentKindDictionary.Add(0x0A09, new ContentKindInfo("趣味／教育", "中学生・高校生", 0x0A, 0x09));
                ContentKindDictionary.Add(0x0A0A, new ContentKindInfo("趣味／教育", "大学生・受験", 0x0A, 0x0A));
                ContentKindDictionary.Add(0x0A0B, new ContentKindInfo("趣味／教育", "生涯教育・資格", 0x0A, 0x0B));
                ContentKindDictionary.Add(0x0A0C, new ContentKindInfo("趣味／教育", "教育問題", 0x0A, 0x0C));
                ContentKindDictionary.Add(0x0A0F, new ContentKindInfo("趣味／教育", "その他", 0x0A, 0x0F));

                ContentKindDictionary.Add(0x0BFF, new ContentKindInfo("福祉", "", 0x0B, 0xFF));
                ContentKindDictionary.Add(0x0B00, new ContentKindInfo("福祉", "高齢者", 0x0B, 0x00));
                ContentKindDictionary.Add(0x0B01, new ContentKindInfo("福祉", "障害者", 0x0B, 0x01));
                ContentKindDictionary.Add(0x0B02, new ContentKindInfo("福祉", "社会福祉", 0x0B, 0x02));
                ContentKindDictionary.Add(0x0B03, new ContentKindInfo("福祉", "ボランティア", 0x0B, 0x03));
                ContentKindDictionary.Add(0x0B04, new ContentKindInfo("福祉", "手話", 0x0B, 0x04));
                ContentKindDictionary.Add(0x0B05, new ContentKindInfo("福祉", "文字（字幕）", 0x0B, 0x05));
                ContentKindDictionary.Add(0x0B06, new ContentKindInfo("福祉", "音声解説", 0x0B, 0x06));
                ContentKindDictionary.Add(0x0B0F, new ContentKindInfo("福祉", "その他", 0x0B, 0x0F));

                ContentKindDictionary.Add(0x0FFF, new ContentKindInfo("その他", "", 0x0F, 0xFF));
                ContentKindDictionary.Add(0xFFFF, new ContentKindInfo("なし", "", 0xFF, 0xFF));
            }
            if (ComponentKindDictionary == null)
            {
                ComponentKindDictionary = new Dictionary<UInt16, ComponentKindInfo>();
                ComponentKindDictionary.Add(0x0101, new ComponentKindInfo(0x01, 0x01, "480i(525i)、アスペクト比4:3"));
                ComponentKindDictionary.Add(0x0102, new ComponentKindInfo(0x01, 0x02, "480i(525i)、アスペクト比16:9 パンベクトルあり"));
                ComponentKindDictionary.Add(0x0103, new ComponentKindInfo(0x01, 0x03, "480i(525i)、アスペクト比16:9 パンベクトルなし"));
                ComponentKindDictionary.Add(0x0104, new ComponentKindInfo(0x01, 0x04, "480i(525i)、アスペクト比 > 16:9"));
                ComponentKindDictionary.Add(0x0191, new ComponentKindInfo(0x01, 0x91, "2160p、アスペクト比4:3"));
                ComponentKindDictionary.Add(0x0192, new ComponentKindInfo(0x01, 0x92, "2160p、アスペクト比16:9 パンベクトルあり"));
                ComponentKindDictionary.Add(0x0193, new ComponentKindInfo(0x01, 0x93, "2160p、アスペクト比16:9 パンベクトルなし"));
                ComponentKindDictionary.Add(0x0194, new ComponentKindInfo(0x01, 0x94, "2160p、アスペクト比 > 16:9"));
                ComponentKindDictionary.Add(0x01A1, new ComponentKindInfo(0x01, 0xA1, "480p(525p)、アスペクト比4:3"));
                ComponentKindDictionary.Add(0x01A2, new ComponentKindInfo(0x01, 0xA2, "480p(525p)、アスペクト比16:9 パンベクトルあり"));
                ComponentKindDictionary.Add(0x01A3, new ComponentKindInfo(0x01, 0xA3, "480p(525p)、アスペクト比16:9 パンベクトルなし"));
                ComponentKindDictionary.Add(0x01A4, new ComponentKindInfo(0x01, 0xA4, "480p(525p)、アスペクト比 > 16:9"));
                ComponentKindDictionary.Add(0x01B1, new ComponentKindInfo(0x01, 0xB1, "1080i(1125i)、アスペクト比4:3"));
                ComponentKindDictionary.Add(0x01B2, new ComponentKindInfo(0x01, 0xB2, "1080i(1125i)、アスペクト比16:9 パンベクトルあり"));
                ComponentKindDictionary.Add(0x01B3, new ComponentKindInfo(0x01, 0xB3, "1080i(1125i)、アスペクト比16:9 パンベクトルなし"));
                ComponentKindDictionary.Add(0x01B4, new ComponentKindInfo(0x01, 0xB4, "1080i(1125i)、アスペクト比 > 16:9"));
                ComponentKindDictionary.Add(0x01C1, new ComponentKindInfo(0x01, 0xC1, "720p(750p)、アスペクト比4:3"));
                ComponentKindDictionary.Add(0x01C2, new ComponentKindInfo(0x01, 0xC2, "720p(750p)、アスペクト比16:9 パンベクトルあり"));
                ComponentKindDictionary.Add(0x01C3, new ComponentKindInfo(0x01, 0xC3, "720p(750p)、アスペクト比16:9 パンベクトルなし"));
                ComponentKindDictionary.Add(0x01C4, new ComponentKindInfo(0x01, 0xC4, "720p(750p)、アスペクト比 > 16:9"));
                ComponentKindDictionary.Add(0x01D1, new ComponentKindInfo(0x01, 0xD1, "240p アスペクト比4:3"));
                ComponentKindDictionary.Add(0x01D2, new ComponentKindInfo(0x01, 0xD2, "240p アスペクト比16:9 パンベクトルあり"));
                ComponentKindDictionary.Add(0x01D3, new ComponentKindInfo(0x01, 0xD3, "240p アスペクト比16:9 パンベクトルなし"));
                ComponentKindDictionary.Add(0x01D4, new ComponentKindInfo(0x01, 0xD4, "240p アスペクト比 > 16:9"));
                ComponentKindDictionary.Add(0x01E1, new ComponentKindInfo(0x01, 0xE1, "1080p(1125p)、アスペクト比4:3"));
                ComponentKindDictionary.Add(0x01E2, new ComponentKindInfo(0x01, 0xE2, "1080p(1125p)、アスペクト比16:9 パンベクトルあり"));
                ComponentKindDictionary.Add(0x01E3, new ComponentKindInfo(0x01, 0xE3, "1080p(1125p)、アスペクト比16:9 パンベクトルなし"));
                ComponentKindDictionary.Add(0x01E4, new ComponentKindInfo(0x01, 0xE4, "1080p(1125p)、アスペクト比 > 16:9"));
                ComponentKindDictionary.Add(0x0201, new ComponentKindInfo(0x02, 0x01, "1/0モード（シングルモノ）"));
                ComponentKindDictionary.Add(0x0202, new ComponentKindInfo(0x02, 0x02, "1/0＋1/0モード（デュアルモノ）"));
                ComponentKindDictionary.Add(0x0203, new ComponentKindInfo(0x02, 0x03, "2/0モード（ステレオ）"));
                ComponentKindDictionary.Add(0x0204, new ComponentKindInfo(0x02, 0x04, "2/1モード"));
                ComponentKindDictionary.Add(0x0205, new ComponentKindInfo(0x02, 0x05, "3/0モード"));
                ComponentKindDictionary.Add(0x0206, new ComponentKindInfo(0x02, 0x06, "2/2モード"));
                ComponentKindDictionary.Add(0x0207, new ComponentKindInfo(0x02, 0x07, "3/1モード"));
                ComponentKindDictionary.Add(0x0208, new ComponentKindInfo(0x02, 0x08, "3/2モード"));
                ComponentKindDictionary.Add(0x0209, new ComponentKindInfo(0x02, 0x09, "3/2＋LFEモード（3/2.1モード）"));
                ComponentKindDictionary.Add(0x020A, new ComponentKindInfo(0x02, 0x0A, "3/3.1モード"));
                ComponentKindDictionary.Add(0x020B, new ComponentKindInfo(0x02, 0x0B, "2/0/0-2/0/2-0.1モード"));
                ComponentKindDictionary.Add(0x020C, new ComponentKindInfo(0x02, 0x0C, "5/2.1モード"));
                ComponentKindDictionary.Add(0x020D, new ComponentKindInfo(0x02, 0x0D, "3/2/2.1モード"));
                ComponentKindDictionary.Add(0x020E, new ComponentKindInfo(0x02, 0x0E, "2/0/0-3/0/2-0.1モード"));
                ComponentKindDictionary.Add(0x020F, new ComponentKindInfo(0x02, 0x0F, "0/2/0-3/0/2-0.1モード"));
                ComponentKindDictionary.Add(0x0210, new ComponentKindInfo(0x02, 0x10, "2/0/0-3/2/3-0.2モード"));
                ComponentKindDictionary.Add(0x0211, new ComponentKindInfo(0x02, 0x11, "3/3/3-5/2/3-3/0/0.2モード"));
                ComponentKindDictionary.Add(0x0240, new ComponentKindInfo(0x02, 0x40, "視覚障害者用音声解説"));
                ComponentKindDictionary.Add(0x0241, new ComponentKindInfo(0x02, 0x41, "聴覚障害者用音声"));
                ComponentKindDictionary.Add(0x0501, new ComponentKindInfo(0x05, 0x01, "H.264|MPEG-4 AVC、480i(525i)、アスペクト比4:3"));
                ComponentKindDictionary.Add(0x0502, new ComponentKindInfo(0x05, 0x02, "H.264|MPEG-4 AVC、480i(525i)、アスペクト比16:9 パンベクトルあり"));
                ComponentKindDictionary.Add(0x0503, new ComponentKindInfo(0x05, 0x03, "H.264|MPEG-4 AVC、480i(525i)、アスペクト比16:9 パンベクトルなし"));
                ComponentKindDictionary.Add(0x0504, new ComponentKindInfo(0x05, 0x04, "H.264|MPEG-4 AVC、480i(525i)、アスペクト比 > 16:9"));
                ComponentKindDictionary.Add(0x0591, new ComponentKindInfo(0x05, 0x91, "H.264|MPEG-4 AVC、2160p、アスペクト比4:3"));
                ComponentKindDictionary.Add(0x0592, new ComponentKindInfo(0x05, 0x92, "H.264|MPEG-4 AVC、2160p、アスペクト比16:9 パンベクトルあり"));
                ComponentKindDictionary.Add(0x0593, new ComponentKindInfo(0x05, 0x93, "H.264|MPEG-4 AVC、2160p、アスペクト比16:9 パンベクトルなし"));
                ComponentKindDictionary.Add(0x0594, new ComponentKindInfo(0x05, 0x94, "H.264|MPEG-4 AVC、2160p、アスペクト比 > 16:9"));
                ComponentKindDictionary.Add(0x05A1, new ComponentKindInfo(0x05, 0xA1, "H.264|MPEG-4 AVC、480p(525p)、アスペクト比4:3"));
                ComponentKindDictionary.Add(0x05A2, new ComponentKindInfo(0x05, 0xA2, "H.264|MPEG-4 AVC、480p(525p)、アスペクト比16:9 パンベクトルあり"));
                ComponentKindDictionary.Add(0x05A3, new ComponentKindInfo(0x05, 0xA3, "H.264|MPEG-4 AVC、480p(525p)、アスペクト比16:9 パンベクトルなし"));
                ComponentKindDictionary.Add(0x05A4, new ComponentKindInfo(0x05, 0xA4, "H.264|MPEG-4 AVC、480p(525p)、アスペクト比 > 16:9"));
                ComponentKindDictionary.Add(0x05B1, new ComponentKindInfo(0x05, 0xB1, "H.264|MPEG-4 AVC、1080i(1125i)、アスペクト比4:3"));
                ComponentKindDictionary.Add(0x05B2, new ComponentKindInfo(0x05, 0xB2, "H.264|MPEG-4 AVC、1080i(1125i)、アスペクト比16:9 パンベクトルあり"));
                ComponentKindDictionary.Add(0x05B3, new ComponentKindInfo(0x05, 0xB3, "H.264|MPEG-4 AVC、1080i(1125i)、アスペクト比16:9 パンベクトルなし"));
                ComponentKindDictionary.Add(0x05B4, new ComponentKindInfo(0x05, 0xB4, "H.264|MPEG-4 AVC、1080i(1125i)、アスペクト比 > 16:9"));
                ComponentKindDictionary.Add(0x05C1, new ComponentKindInfo(0x05, 0xC1, "H.264|MPEG-4 AVC、720p(750p)、アスペクト比4:3"));
                ComponentKindDictionary.Add(0x05C2, new ComponentKindInfo(0x05, 0xC2, "H.264|MPEG-4 AVC、720p(750p)、アスペクト比16:9 パンベクトルあり"));
                ComponentKindDictionary.Add(0x05C3, new ComponentKindInfo(0x05, 0xC3, "H.264|MPEG-4 AVC、720p(750p)、アスペクト比16:9 パンベクトルなし"));
                ComponentKindDictionary.Add(0x05C4, new ComponentKindInfo(0x05, 0xC4, "H.264|MPEG-4 AVC、720p(750p)、アスペクト比 > 16:9"));
                ComponentKindDictionary.Add(0x05D1, new ComponentKindInfo(0x05, 0xD1, "H.264|MPEG-4 AVC、240p アスペクト比4:3"));
                ComponentKindDictionary.Add(0x05D2, new ComponentKindInfo(0x05, 0xD2, "H.264|MPEG-4 AVC、240p アスペクト比16:9 パンベクトルあり"));
                ComponentKindDictionary.Add(0x05D3, new ComponentKindInfo(0x05, 0xD3, "H.264|MPEG-4 AVC、240p アスペクト比16:9 パンベクトルなし"));
                ComponentKindDictionary.Add(0x05D4, new ComponentKindInfo(0x05, 0xD4, "H.264|MPEG-4 AVC、240p アスペクト比 > 16:9"));
                ComponentKindDictionary.Add(0x05E1, new ComponentKindInfo(0x05, 0xE1, "H.264|MPEG-4 AVC、1080p(1125p)、アスペクト比4:3"));
                ComponentKindDictionary.Add(0x05E2, new ComponentKindInfo(0x05, 0xE2, "H.264|MPEG-4 AVC、1080p(1125p)、アスペクト比16:9 パンベクトルあり"));
                ComponentKindDictionary.Add(0x05E3, new ComponentKindInfo(0x05, 0xE3, "H.264|MPEG-4 AVC、1080p(1125p)、アスペクト比16:9 パンベクトルなし"));
                ComponentKindDictionary.Add(0x05E4, new ComponentKindInfo(0x05, 0xE4, "H.264|MPEG-4 AVC、1080p(1125p)、アスペクト比 > 16:9"));
            }
            if (DayOfWeekDictionary == null)
            {
                DayOfWeekDictionary = new Dictionary<byte, DayOfWeekInfo>();
                DayOfWeekDictionary.Add(0x00, new DayOfWeekInfo("日", 0x00));
                DayOfWeekDictionary.Add(0x01, new DayOfWeekInfo("月", 0x01));
                DayOfWeekDictionary.Add(0x02, new DayOfWeekInfo("火", 0x02));
                DayOfWeekDictionary.Add(0x03, new DayOfWeekInfo("水", 0x03));
                DayOfWeekDictionary.Add(0x04, new DayOfWeekInfo("木", 0x04));
                DayOfWeekDictionary.Add(0x05, new DayOfWeekInfo("金", 0x05));
                DayOfWeekDictionary.Add(0x06, new DayOfWeekInfo("土", 0x06));
            }
            if( HourDictionary == null ){
                HourDictionary = new Dictionary<UInt16,UInt16>();
                for( UInt16 i=0; i<=23; i++ ){
                    HourDictionary.Add(i,i);
                }
            }
            if( MinDictionary == null ){
                MinDictionary = new Dictionary<UInt16,UInt16>();
                for( UInt16 i=0; i<=59; i++ ){
                    MinDictionary.Add(i,i);
                }
            }
            if (RecModeDictionary == null)
            {
                RecModeDictionary = new Dictionary<byte, RecModeInfo>();
                RecModeDictionary.Add(0x00, new RecModeInfo("全サービス", 0x00));
                RecModeDictionary.Add(0x01, new RecModeInfo("指定サービス", 0x01));
                RecModeDictionary.Add(0x02, new RecModeInfo("全サービス（デコード処理なし）", 0x02));
                RecModeDictionary.Add(0x03, new RecModeInfo("指定サービス（デコード処理なし）", 0x03));
                RecModeDictionary.Add(0x04, new RecModeInfo("視聴", 0x04));
                RecModeDictionary.Add(0x05, new RecModeInfo("無効", 0x05));
            }
            if (YesNoDictionary == null)
            {
                YesNoDictionary = new Dictionary<byte, YesNoInfo>();
                YesNoDictionary.Add(0x00, new YesNoInfo("しない", 0x00));
                YesNoDictionary.Add(0x01, new YesNoInfo("する", 0x01));
            }
            if (PriorityDictionary == null)
            {
                PriorityDictionary = new Dictionary<byte, PriorityInfo>();
                PriorityDictionary.Add(0x01, new PriorityInfo("1", 0x01));
                PriorityDictionary.Add(0x02, new PriorityInfo("2", 0x02));
                PriorityDictionary.Add(0x03, new PriorityInfo("3", 0x03));
                PriorityDictionary.Add(0x04, new PriorityInfo("4", 0x04));
                PriorityDictionary.Add(0x05, new PriorityInfo("5", 0x05));
            }
        }
    }

    public class ContentKindInfo
    {
        public ContentKindInfo()
        {
        }
        public ContentKindInfo(String contentName, String subName, Byte nibble1, Byte nibble2)
        {
            this.ContentName = contentName;
            this.SubName = subName;
            this.Nibble1 = nibble1;
            this.Nibble2 = nibble2;
        }
        public String ContentName
        {
            get;
            set;
        }
        public String SubName
        {
            get;
            set;
        }
        public Byte Nibble1
        {
            get;
            set;
        }
        public Byte Nibble2
        {
            get;
            set;
        }
        public override string ToString()
        {
            if (Nibble2 == 0xFF)
            {
                return ContentName;
            }
            else
            {
                return "  " + SubName;
            }
        }
        public String ToolTipView
        {
            get
            {
                String viewTip = "";
                if (Nibble2 == 0xFF)
                {
                    viewTip = ContentName + "\r\n" +
                        "id : " + Nibble1.ToString() + "(0x" + Nibble1.ToString("X2") + ")";
                }
                else
                {
                    viewTip = ContentName + " : " + SubName + "\r\n" +
                        "id : " + Nibble1.ToString() + "(0x" + Nibble1.ToString("X2") + ")" + "\r\n" +
                        "sub_id : " + Nibble2.ToString() + "(0x" + Nibble2.ToString("X2") + ")";
                }
                return viewTip;
            }
        }
    }

    public class ComponentKindInfo
    {
        public ComponentKindInfo()
        {
        }
        public ComponentKindInfo(Byte streamContent, Byte componentType, String componentName)
        {
            StreamContent = streamContent;
            ComponentType = componentType;
            ComponentName = componentName;
        }
        public Byte StreamContent
        {
            get;
            set;
        }
        public Byte ComponentType
        {
            get;
            set;
        }
        public String ComponentName
        {
            get;
            set;
        }
        public override string ToString()
        {
            return ComponentName;
        }
    }

    public class DayOfWeekInfo
    {
        public DayOfWeekInfo()
        {
        }
        public DayOfWeekInfo(String displayName, Byte value)
        {
            this.DisplayName = displayName;
            this.Value = value;
        }
        public String DisplayName
        {
            get;
            set;
        }
        public Byte Value
        {
            get;
            set;
        }
        public override string ToString()
        {
            return DisplayName;
        }
    }

    public class RecModeInfo
    {
        public RecModeInfo()
        {
        }
        public RecModeInfo(String displayName, Byte value)
        {
            this.DisplayName = displayName;
            this.Value = value;
        }
        public String DisplayName
        {
            get;
            set;
        }
        public Byte Value
        {
            get;
            set;
        }
        public override string ToString()
        {
            return DisplayName;
        }
    }

    public class YesNoInfo
    {
        public YesNoInfo()
        {
        }
        public YesNoInfo(String displayName, Byte value)
        {
            this.DisplayName = displayName;
            this.Value = value;
        }
        public String DisplayName
        {
            get;
            set;
        }
        public Byte Value
        {
            get;
            set;
        }
        public override string ToString()
        {
            return DisplayName;
        }
    }

    public class PriorityInfo
    {
        public PriorityInfo()
        {
        }
        public PriorityInfo(String displayName, Byte value)
        {
            this.DisplayName = displayName;
            this.Value = value;
        }
        public String DisplayName
        {
            get;
            set;
        }
        public Byte Value
        {
            get;
            set;
        }
        public override string ToString()
        {
            return DisplayName;
        }
    }

    public class DateItem
    {
        public DateItem()
        {
        }
        public EpgSearchDateInfo DateInfo
        {
            get;
            set;
        }
        public String ViewText
        {
            get;
            set;
        }
        public override string ToString()
        {
            return ViewText;
        }
    }

    public class IEPGStationInfo
    {
        public String StationName
        {
            get;
            set;
        }
        public UInt64 Key
        {
            get;
            set;
        }
        public override string ToString()
        {
            return StationName;
        }
    }
}
