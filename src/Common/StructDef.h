﻿#ifndef __STRUCT_DEF_H__
#define __STRUCT_DEF_H__

#include "Util.h"

//録画フォルダ情報
typedef struct _REC_FILE_SET_INFO{
	wstring recFolder;
	wstring writePlugIn;
	_REC_FILE_SET_INFO & operator= (const _REC_FILE_SET_INFO & o) {
		recFolder = o.recFolder;
		writePlugIn = o.writePlugIn;
		return *this;
	};
}REC_FILE_SET_INFO;

//録画設定情報
typedef struct _REC_SETTING_DATA{
	BYTE recMode;				//録画モード
	BYTE priority;				//優先度
	BYTE tuijyuuFlag;			//追従モード
	DWORD serviceMode;			//処理対象データモード
	BYTE pittariFlag;			//ぴったり？録画
	wstring batFilePath;		//録画後BATファイルパス
	vector<REC_FILE_SET_INFO> recFolderList;		//録画フォルダパス
	BYTE suspendMode;			//休止モード
	BYTE rebootFlag;			//録画後再起動する
	BYTE useMargineFlag;		//録画マージンを個別指定
	INT startMargine;			//録画開始時のマージン
	INT endMargine;				//録画終了時のマージン
	BYTE continueRecFlag;		//後続同一サービス時、同一ファイルで録画
	BYTE partialRecFlag;		//物理CHに部分受信サービスがある場合、同時録画するかどうか
	DWORD tunerID;				//強制的に使用Tunerを固定
	//=オペレーターの処理
	_REC_SETTING_DATA(void){
		recMode = 1;
		priority = 1;
		tuijyuuFlag = 1;
		serviceMode = 0;
		pittariFlag = FALSE;
		batFilePath = L"";
		suspendMode = 0;
		rebootFlag = FALSE;
		useMargineFlag = FALSE;
		startMargine = 10;
		endMargine = 5;
		continueRecFlag = 0;
		partialRecFlag = 0;
		tunerID = 0;
	};
	~_REC_SETTING_DATA(void){
	}
	_REC_SETTING_DATA & operator= (const _REC_SETTING_DATA & o) {
		recMode = o.recMode;
		priority = o.priority;
		tuijyuuFlag = o.tuijyuuFlag;
		serviceMode = o.serviceMode;
		pittariFlag = o.pittariFlag;
		batFilePath = o.batFilePath;
		recFolderList = o.recFolderList;
		suspendMode = o.suspendMode;
		rebootFlag = o.rebootFlag;
		useMargineFlag = o.useMargineFlag;
		startMargine = o.startMargine;
		endMargine = o.endMargine;
		continueRecFlag = o.continueRecFlag;
		partialRecFlag = o.partialRecFlag;
		tunerID = o.tunerID;
		return *this;
	};
} REC_SETTING_DATA;

//登録予約情報
typedef struct _RESERVE_DATA{
	wstring title;					//番組名
	SYSTEMTIME startTime;			//録画開始時間
	DWORD durationSecond;			//録画総時間
	wstring stationName;			//サービス名
	WORD originalNetworkID;			//ONID
	WORD transportStreamID;			//TSID
	WORD serviceID;					//SID
	WORD eventID;					//EventID
	wstring comment;				//コメント
	DWORD reserveID;				//同一番組判別用ID
	BYTE recWaitFlag;				//予約待機入った？
	BYTE overlapMode;				//かぶり状態 1:かぶってチューナー足りない予約あり 2:チューナー足りなくて予約できない
	wstring recFilePath;			//録画ファイルパス
	SYSTEMTIME startTimeEpg;		//予約時の開始時間
	REC_SETTING_DATA recSetting;	//録画設定
	DWORD reserveStatus;			//予約追加状態
	//=オペレーターの処理
	_RESERVE_DATA(void){
		title=L"";
		ZeroMemory(&startTime, sizeof(SYSTEMTIME));
		durationSecond = 0;
		stationName = L"";
		originalNetworkID = 0;
		transportStreamID = 0;
		serviceID = 0;
		eventID = 0;
		comment = L"";
		reserveID = 0;
		recWaitFlag = FALSE;
		overlapMode = 0;
		ZeroMemory(&startTimeEpg, sizeof(SYSTEMTIME));
		recFilePath = L"";
		reserveStatus = 0;
	};
	~_RESERVE_DATA(void){
	}
	_RESERVE_DATA & operator= (const _RESERVE_DATA & o) {
		title=o.title;
		startTime = o.startTime;
		durationSecond = o.durationSecond;
		stationName = o.stationName;
		originalNetworkID = o.originalNetworkID;
		transportStreamID = o.transportStreamID;
		serviceID = o.serviceID;
		eventID = o.eventID;
		comment = o.comment;
		reserveID = o.reserveID;
		recWaitFlag = o.recWaitFlag;
		overlapMode = o.overlapMode;
		recFilePath = o.recFilePath;
		startTimeEpg = o.startTimeEpg;
		recSetting  = o.recSetting;
		reserveStatus = o.reserveStatus;
		return *this;
	};
} RESERVE_DATA;

typedef struct _REC_FILE_INFO{
	DWORD id;					//ID
	wstring recFilePath;		//録画ファイルパス
	wstring title;				//番組名
	SYSTEMTIME startTime;		//開始時間
	DWORD durationSecond;		//録画時間
	wstring serviceName;		//サービス名
	WORD originalNetworkID;		//ONID
	WORD transportStreamID;		//TSID
	WORD serviceID;				//SID
	WORD eventID;				//EventID
	__int64 drops;				//ドロップ数
	__int64 scrambles;			//スクランブル数
	DWORD recStatus;			//録画結果のステータス
	SYSTEMTIME startTimeEpg;	//予約時の開始時間
	wstring comment;			//コメント
	wstring programInfo;		//.program.txtファイルの内容
	wstring errInfo;			//.errファイルの内容
	//=オペレーターの処理
	_REC_FILE_INFO(void){
		id = 0;
		recFilePath = L"";
		title = L"";
		ZeroMemory(&startTime, sizeof(SYSTEMTIME));
		durationSecond = 0;
		serviceName = L"";
		originalNetworkID = 0;
		transportStreamID = 0;
		serviceID = 0;
		eventID = 0;
		drops = 0;
		scrambles = 0;
		recStatus = 0;
		ZeroMemory(&startTimeEpg, sizeof(SYSTEMTIME));
		comment = L"";
		programInfo = L"";
		errInfo = L"";
	};
	~_REC_FILE_INFO(void){
	}
	_REC_FILE_INFO & operator= (const _REC_FILE_INFO & o) {
		id = o.id;
		recFilePath=o.recFilePath;
		title = o.title;
		startTime = o.startTime;
		durationSecond = o.durationSecond;
		serviceName = o.serviceName;
		originalNetworkID = o.originalNetworkID;
		transportStreamID = o.transportStreamID;
		serviceID = o.serviceID;
		eventID = o.eventID;
		drops = o.drops;
		scrambles = o.scrambles;
		recStatus = o.recStatus;
		startTimeEpg = o.startTimeEpg;
		comment = o.comment;
		programInfo = o.programInfo;
		errInfo = o.errInfo;
		return *this;
	};
	_REC_FILE_INFO & operator= (const _RESERVE_DATA & o) {
		id = 0;
		recFilePath=o.recFilePath;
		title = o.title;
		startTime = o.startTime;
		durationSecond = o.durationSecond;
		serviceName = o.stationName;
		originalNetworkID = o.originalNetworkID;
		transportStreamID = o.transportStreamID;
		serviceID = o.serviceID;
		eventID = o.eventID;
		drops = 0;
		scrambles = 0;
		recStatus = 0;
		startTimeEpg = o.startTimeEpg;
		comment = o.comment;
		programInfo = L"";
		errInfo = L"";
		return *this;
	};
} REC_FILE_INFO;

typedef struct _TUNER_RESERVE_INFO{
	DWORD tunerID;
	wstring tunerName;
	vector<DWORD> reserveList;
	_TUNER_RESERVE_INFO & operator= (const _TUNER_RESERVE_INFO & o) {
		tunerID = o.tunerID;
		tunerName=o.tunerName;
		reserveList = o.reserveList;
		return *this;
	};
} TUNER_RESERVE_INFO;

//チューナー毎サービス情報
typedef struct _CH_DATA4{
	int space;						//チューナー空間
	int ch;							//物理チャンネル
	WORD originalNetworkID;			//ONID
	WORD transportStreamID;			//TSID
	WORD serviceID;					//サービスID
	WORD serviceType;				//サービスタイプ
	BOOL partialFlag;				//部分受信サービス（ワンセグ）かどうか
	BOOL useViewFlag;				//一覧表示に使用するかどうか
	wstring serviceName;			//サービス名
	wstring chName;					//チャンネル名
	wstring networkName;			//ts_name or network_name
	BYTE remoconID;					//リモコンID
	//=オペレーターの処理
	_CH_DATA4(void){
		space = 0;
		ch = 0;
		originalNetworkID = 0;
		transportStreamID = 0;
		serviceID = 0;
		serviceType = 0;
		partialFlag = FALSE;
		useViewFlag = TRUE;
		serviceName = L"";
		chName = L"";
		networkName = L"";
		remoconID = 0;
	};
	~_CH_DATA4(void){
	}
	_CH_DATA4 & operator= (const _CH_DATA4 & o) {
		space = o.space;
		ch = o.ch;
		originalNetworkID = o.originalNetworkID;
		transportStreamID = o.transportStreamID;
		serviceID = o.serviceID;
		serviceType = o.serviceType;
		partialFlag = o.partialFlag;
		useViewFlag = o.useViewFlag;
		serviceName = o.serviceName;
		chName = o.chName;
		networkName = o.networkName;
		remoconID = o.remoconID;
		return *this;
	}
} CH_DATA4;

//全チューナーで認識したサービス一覧
typedef struct _CH_DATA5{
	WORD originalNetworkID;			//ONID
	WORD transportStreamID;			//TSID
	WORD serviceID;					//サービスID
	WORD serviceType;				//サービスタイプ
	BOOL partialFlag;				//部分受信サービス（ワンセグ）かどうか
	wstring serviceName;			//サービス名
	wstring networkName;			//ts_name or network_name
	BOOL epgCapFlag;				//EPGデータ取得対象かどうか
	BOOL searchFlag;				//検索時のデフォルト検索対象サービスかどうか
	//=オペレーターの処理
	_CH_DATA5(void){
		originalNetworkID = 0;
		transportStreamID = 0;
		serviceID = 0;
		serviceType = 0;
		partialFlag = FALSE;
		serviceName = L"";
		networkName = L"";
		epgCapFlag = TRUE;
		searchFlag = TRUE;
	};
	~_CH_DATA5(void){
	}
	_CH_DATA5 & operator= (const _CH_DATA5 & o) {
		originalNetworkID = o.originalNetworkID;
		transportStreamID = o.transportStreamID;
		serviceID = o.serviceID;
		serviceType = o.serviceType;
		partialFlag = o.partialFlag;
		serviceName = o.serviceName;
		networkName = o.networkName;
		epgCapFlag = o.epgCapFlag;
		searchFlag = o.searchFlag;
		return *this;
	}
} CH_DATA5;

typedef struct _REGIST_TCP_INFO{
	wstring ip;
	DWORD port;
	_REGIST_TCP_INFO & operator= (const _REGIST_TCP_INFO & o) {
		ip = o.ip;
		port = o.port;
		return *this;
	}
}REGIST_TCP_INFO;

//コマンド送受信ストリーム
typedef struct _CMD_STREAM{
	DWORD param;	//送信時コマンド、受信時エラーコード
	DWORD dataSize;	//dataのサイズ（BYTE単位）
	BYTE* data;		//送受信するバイナリデータ
	//=オペレーターの処理
	_CMD_STREAM(void){
		param = 0;
		dataSize = 0;
		data = NULL;
	}
	~_CMD_STREAM(void){
		SAFE_DELETE_ARRAY(data);
	}
} CMD_STREAM;



//EPG基本情報
typedef struct _EPGDB_SHORT_EVENT_INFO{
	wstring event_name;			//イベント名
	wstring text_char;			//情報
	wstring search_event_name;	//検索使用時のイベント名
	wstring search_text_char;	//検索使用時の情報
	_EPGDB_SHORT_EVENT_INFO & operator= (const _EPGDB_SHORT_EVENT_INFO & o) {
		event_name = o.event_name;
		text_char = o.text_char;
		search_event_name = o.search_event_name;
		search_text_char = o.search_text_char;
		return *this;
	};
} EPGDB_SHORT_EVENT_INFO;

//EPG拡張情報
typedef struct _EPGDB_EXTENDED_EVENT_INFO{
	wstring text_char;			//詳細情報
	wstring search_text_char;	//検索使用時の情報
	_EPGDB_EXTENDED_EVENT_INFO & operator= (const _EPGDB_EXTENDED_EVENT_INFO & o) {
		text_char = o.text_char;
		search_text_char = o.search_text_char;
		return *this;
	};
} EPGDB_EXTENDED_EVENT_INFO;

//EPGジャンルデータ
typedef struct _EPGDB_CONTENT_DATA{
	BYTE content_nibble_level_1;
	BYTE content_nibble_level_2;
	BYTE user_nibble_1;
	BYTE user_nibble_2;
	_EPGDB_CONTENT_DATA & operator= (const _EPGDB_CONTENT_DATA & o) {
		content_nibble_level_1 = o.content_nibble_level_1;
		content_nibble_level_2 = o.content_nibble_level_2;
		user_nibble_1 = o.user_nibble_1;
		user_nibble_2 = o.user_nibble_2;
		return *this;
	};
}EPGDB_CONTENT_DATA;

//EPGジャンル情報
typedef struct _EPGDB_CONTENT_INFO{
	vector<EPGDB_CONTENT_DATA> nibbleList;
	_EPGDB_CONTENT_INFO & operator= (const _EPGDB_CONTENT_INFO & o) {
		nibbleList = o.nibbleList;
		return *this;
	};
} EPGDB_CONTEN_INFO;

//EPG映像情報
typedef struct _EPGDB_COMPONENT_INFO{
	BYTE stream_content;
	BYTE component_type;
	BYTE component_tag;
	wstring text_char;			//情報
	_EPGDB_COMPONENT_INFO & operator= (const _EPGDB_COMPONENT_INFO & o) {
		stream_content = o.stream_content;
		component_type = o.component_type;
		component_tag = o.component_tag;
		text_char = o.text_char;
		return *this;
	};
} EPGDB_COMPONENT_INFO;

//EPG音声情報データ
typedef struct _EPGDB_AUDIO_COMPONENT_INFO_DATA{
	BYTE stream_content;
	BYTE component_type;
	BYTE component_tag;
	BYTE stream_type;
	BYTE simulcast_group_tag;
	BYTE ES_multi_lingual_flag;
	BYTE main_component_flag;
	BYTE quality_indicator;
	BYTE sampling_rate;
	wstring text_char;			//詳細情報
	_EPGDB_AUDIO_COMPONENT_INFO_DATA & operator= (const _EPGDB_AUDIO_COMPONENT_INFO_DATA & o) {
		stream_content = o.stream_content;
		component_type = o.component_type;
		component_tag = o.component_tag;
		stream_type = o.stream_type;
		simulcast_group_tag = o.simulcast_group_tag;
		ES_multi_lingual_flag = o.ES_multi_lingual_flag;
		main_component_flag = o.main_component_flag;
		quality_indicator = o.quality_indicator;
		sampling_rate = o.sampling_rate;
		text_char = o.text_char;
		return *this;
	};
} EPGDB_AUDIO_COMPONENT_INFO_DATA;

//EPG音声情報
typedef struct _EPGDB_AUDIO_COMPONENT_INFO{
	vector<EPGDB_AUDIO_COMPONENT_INFO_DATA> componentList;
	_EPGDB_AUDIO_COMPONENT_INFO & operator= (const _EPGDB_AUDIO_COMPONENT_INFO & o) {
		componentList = o.componentList;
		return *this;
	};
} EPGDB_AUDIO_COMPONENT_INFO;

//EPGイベントデータ
typedef struct _EPGDB_EVENT_DATA{
	WORD original_network_id;
	WORD transport_stream_id;
	WORD service_id;
	WORD event_id;
	_EPGDB_EVENT_DATA & operator= (const _EPGDB_EVENT_DATA & o) {
		original_network_id = o.original_network_id;
		transport_stream_id = o.transport_stream_id;
		service_id = o.service_id;
		event_id = o.event_id;
		return *this;
	};
}EPGDB_EVENT_DATA;

//EPGイベントグループ情報
typedef struct _EPGDB_EVENTGROUP_INFO{
	BYTE group_type;
	vector<EPGDB_EVENT_DATA> eventDataList;
	_EPGDB_EVENTGROUP_INFO & operator= (const _EPGDB_EVENTGROUP_INFO & o) {
		group_type = o.group_type;
		eventDataList = o.eventDataList;
		return *this;
	};
} EPGDB_EVENTGROUP_INFO;

typedef struct _EPGDB_EVENT_INFO{
	WORD original_network_id;
	WORD transport_stream_id;
	WORD service_id;
	WORD event_id;							//イベントID
	BYTE StartTimeFlag;						//start_timeの値が有効かどうか
	SYSTEMTIME start_time;					//開始時間
	BYTE DurationFlag;						//durationの値が有効かどうか
	DWORD durationSec;						//総時間（単位：秒）

	EPGDB_SHORT_EVENT_INFO* shortInfo;		//基本情報
	EPGDB_EXTENDED_EVENT_INFO* extInfo;		//拡張情報
	EPGDB_CONTEN_INFO* contentInfo;			//ジャンル情報
	EPGDB_COMPONENT_INFO* componentInfo;		//映像情報
	EPGDB_AUDIO_COMPONENT_INFO* audioInfo;	//音声情報
	EPGDB_EVENTGROUP_INFO* eventGroupInfo;	//イベントグループ情報
	EPGDB_EVENTGROUP_INFO* eventRelayInfo;	//イベントリレー情報

	_EPGDB_EVENT_INFO(void){
		shortInfo = NULL;
		extInfo = NULL;
		contentInfo = NULL;
		componentInfo = NULL;
		audioInfo = NULL;
		eventGroupInfo = NULL;
		eventRelayInfo = NULL;
	};
	~_EPGDB_EVENT_INFO(void){
		SAFE_DELETE(shortInfo);
		SAFE_DELETE(extInfo);
		SAFE_DELETE(contentInfo);
		SAFE_DELETE(componentInfo);
		SAFE_DELETE(audioInfo);
		SAFE_DELETE(eventGroupInfo);
		SAFE_DELETE(eventRelayInfo);
	};
}EPGDB_EVENT_INFO;

typedef struct _EPGDB_SERVICE_INFO{
	WORD ONID;
	WORD TSID;
	WORD SID;
	BYTE service_type;
	BYTE partialReceptionFlag;
	wstring service_provider_name;
	wstring service_name;
	wstring network_name;
	wstring ts_name;
	BYTE remote_control_key_id;
	_EPGDB_SERVICE_INFO(void){
		ONID = 0;
		TSID = 0;
		SID = 0;
		service_type = 0;
		partialReceptionFlag = 0;
		service_provider_name = L"";
		service_name = L"";
		network_name = L"";
		ts_name = L"";
		remote_control_key_id = 0;
	};
	~_EPGDB_SERVICE_INFO(void){
	};
	_EPGDB_SERVICE_INFO & operator= (const _EPGDB_SERVICE_INFO & o) {
		ONID = o.ONID;
		TSID = o.TSID;
		SID = o.SID;
		service_type = o.service_type;
		partialReceptionFlag = o.partialReceptionFlag;
		service_provider_name = o.service_provider_name;
		service_name = o.service_name;
		network_name = o.network_name;
		ts_name = o.ts_name;
		remote_control_key_id = o.remote_control_key_id;
		return *this;
	};
}EPGDB_SERVICE_INFO;

typedef struct _EPGDB_SERVICE_EVENT_INFO{
	EPGDB_SERVICE_INFO serviceInfo;
	vector<EPGDB_EVENT_INFO*> eventList;
	~_EPGDB_SERVICE_EVENT_INFO(void){
	};
	_EPGDB_SERVICE_EVENT_INFO & operator= (const _EPGDB_SERVICE_EVENT_INFO & o) {
		serviceInfo = o.serviceInfo;
		eventList = o.eventList;
		return *this;
	};
}EPGDB_SERVICE_EVENT_INFO;

typedef struct _EPGDB_SEARCH_DATE_INFO{
	BYTE startDayOfWeek;
	WORD startHour;
	WORD startMin;
	BYTE endDayOfWeek;
	WORD endHour;
	WORD endMin;
	_EPGDB_SEARCH_DATE_INFO & operator= (const _EPGDB_SEARCH_DATE_INFO & o) {
		startDayOfWeek = o.startDayOfWeek;
		startHour = o.startHour;
		startMin = o.startMin;
		endDayOfWeek = o.endDayOfWeek;
		endHour = o.endHour;
		endMin = o.endMin;
		return *this;
	};
} EPGDB_SEARCH_DATE_INFO;

//検索条件
typedef struct _EPGDB_SEARCH_KEY_INFO{
	wstring andKey;
	wstring notKey;
	BOOL regExpFlag;
	BOOL titleOnlyFlag;
	vector<EPGDB_CONTENT_DATA> contentList;
	vector<EPGDB_SEARCH_DATE_INFO> dateList;
	vector<__int64> serviceList;
	vector<WORD> videoList;
	vector<WORD> audioList;
	BYTE aimaiFlag;
	//=オペレーターの処理
	_EPGDB_SEARCH_KEY_INFO(void){
		andKey = L"";
		notKey = L"";
		regExpFlag = FALSE;
		titleOnlyFlag = FALSE;
		aimaiFlag = 0;
	};
	_EPGDB_SEARCH_KEY_INFO & operator= (const _EPGDB_SEARCH_KEY_INFO & o) {
		andKey = o.andKey;
		notKey = o.notKey;
		regExpFlag = o.regExpFlag;
		titleOnlyFlag = o.titleOnlyFlag;
		contentList = o.contentList;
		dateList = o.dateList;
		serviceList = o.serviceList;
		videoList = o.videoList;
		audioList = o.audioList;
		aimaiFlag = o.aimaiFlag;
		return *this;
	};
}EPGDB_SEARCH_KEY_INFO;

//自動予約登録情報
typedef struct _EPG_AUTO_ADD_DATA{
	DWORD dataID;
	EPGDB_SEARCH_KEY_INFO searchInfo;	//検索キー
	REC_SETTING_DATA recSetting;	//録画設定
	_EPG_AUTO_ADD_DATA & operator= (const _EPG_AUTO_ADD_DATA & o) {
		dataID = o.dataID;
		searchInfo = o.searchInfo;
		recSetting = o.recSetting;
		return *this;
	};
} EPG_AUTO_ADD_DATA;

typedef struct _MANUAL_AUTO_ADD_DATA{
	DWORD dataID;
	BYTE dayOfWeekFlag;				//対象曜日
	DWORD startTime;				//録画開始時間（00:00を0として秒単位）
	DWORD durationSecond;			//録画総時間
	wstring title;					//番組名
	wstring stationName;			//サービス名
	WORD originalNetworkID;			//ONID
	WORD transportStreamID;			//TSID
	WORD serviceID;					//SID
	REC_SETTING_DATA recSetting;	//録画設定
	_MANUAL_AUTO_ADD_DATA & operator= (const _MANUAL_AUTO_ADD_DATA & o) {
		dataID = o.dataID;
		dayOfWeekFlag = o.dayOfWeekFlag;
		startTime = o.startTime;
		durationSecond = o.durationSecond;
		title = o.title;
		stationName = o.stationName;
		originalNetworkID = o.originalNetworkID;
		transportStreamID = o.transportStreamID;
		serviceID = o.serviceID;
		recSetting = o.recSetting;
		return *this;
	};
} MANUAL_AUTO_ADD_DATA;

//コマンド送信用
//チャンネル変更情報
typedef struct _SET_CH_INFO{
	BOOL useSID;//wONIDとwTSIDとwSIDの値が使用できるかどうか
	WORD ONID;
	WORD TSID;
	WORD SID;
	BOOL useBonCh;//dwSpaceとdwChの値が使用できるかどうか
	DWORD space;
	DWORD ch;
	//=オペレーターの処理
	_SET_CH_INFO & operator= (const _SET_CH_INFO & o) {
		useSID = o.useSID;
		ONID = o.ONID;
		TSID = o.TSID;
		SID = o.SID;
		useBonCh = o.useBonCh;
		space = o.space;
		ch = o.ch;
		return *this;
	};
}SET_CH_INFO;

typedef struct _SET_CTRL_MODE{
	DWORD ctrlID;
	WORD SID;
	BYTE enableScramble;
	BYTE enableCaption;
	BYTE enableData;
	//=オペレーターの処理
	_SET_CTRL_MODE & operator= (const _SET_CTRL_MODE & o) {
		ctrlID = o.ctrlID;
		SID = o.SID;
		enableScramble = o.enableScramble;
		enableCaption = o.enableCaption;
		enableData = o.enableData;
		return *this;
	};
} SET_CTRL_MODE;

typedef struct _SET_CTRL_REC_PARAM{
	DWORD ctrlID;
	wstring fileName;
	BYTE overWriteFlag;
	ULONGLONG createSize;
	vector<REC_FILE_SET_INFO> saveFolder;
	BYTE pittariFlag;
	WORD pittariONID;
	WORD pittariTSID;
	WORD pittariSID;
	WORD pittariEventID;
	//=オペレーターの処理
	_SET_CTRL_REC_PARAM & operator= (const _SET_CTRL_REC_PARAM & o) {
		ctrlID = o.ctrlID;
		fileName = o.fileName;
		overWriteFlag = o.overWriteFlag;
		createSize = o.createSize;
		saveFolder = o.saveFolder;
		pittariFlag = o.pittariFlag;
		pittariONID = o.pittariONID;
		pittariTSID = o.pittariTSID;
		pittariSID = o.pittariSID;
		pittariEventID = o.pittariEventID;
		return *this;
	};
} SET_CTRL_REC_PARAM;

typedef struct _SET_CTRL_REC_STOP_PARAM{
	DWORD ctrlID;
	BOOL saveErrLog;
	//=オペレーターの処理
	_SET_CTRL_REC_STOP_PARAM & operator= (const _SET_CTRL_REC_STOP_PARAM & o) {
		ctrlID = o.ctrlID;
		saveErrLog = o.saveErrLog;
		return *this;
	};
} SET_CTRL_REC_STOP_PARAM;

typedef struct _SET_CTRL_REC_STOP_RES_PARAM{
	wstring recFilePath;
	ULONGLONG drop;
	ULONGLONG scramble;
	BYTE subRecFlag;
	//=オペレーターの処理
	_SET_CTRL_REC_STOP_RES_PARAM & operator= (const _SET_CTRL_REC_STOP_RES_PARAM & o) {
		recFilePath = o.recFilePath;
		drop = o.drop;
		scramble = o.scramble;
		subRecFlag = o.subRecFlag;
		return *this;
	};
} SET_CTRL_REC_STOP_RES_PARAM;

typedef struct _SEARCH_EPG_INFO_PARAM{
	WORD ONID;
	WORD TSID;
	WORD SID;
	WORD eventID;
	BYTE pfOnlyFlag;
	//=オペレーターの処理
	_SEARCH_EPG_INFO_PARAM & operator= (const _SEARCH_EPG_INFO_PARAM & o) {
		ONID = o.ONID;
		TSID = o.TSID;
		SID = o.SID;
		eventID = o.eventID;
		pfOnlyFlag = o.pfOnlyFlag;
		return *this;
	};
} SEARCH_EPG_INFO_PARAM;

typedef struct _GET_EPG_PF_INFO_PARAM{
	WORD ONID;
	WORD TSID;
	WORD SID;
	BYTE pfNextFlag;
	//=オペレーターの処理
	_GET_EPG_PF_INFO_PARAM & operator= (const _GET_EPG_PF_INFO_PARAM & o) {
		ONID = o.ONID;
		TSID = o.TSID;
		SID = o.SID;
		pfNextFlag = o.pfNextFlag;
		return *this;
	};
} GET_EPG_PF_INFO_PARAM;

typedef struct _TVTEST_CH_CHG_INFO{
	wstring bonDriver;
	SET_CH_INFO chInfo;
	//=オペレーターの処理
	_TVTEST_CH_CHG_INFO & operator= (const _TVTEST_CH_CHG_INFO & o) {
		bonDriver = o.bonDriver;
		chInfo = o.chInfo;
		return *this;
	};
} TVTEST_CH_CHG_INFO;


////////////////////////////////////////////////////////////////////////////////////////////
//旧バージョンコマンド送信用
typedef struct _OLD_RESERVE_DATA{
	wstring strTitle;
	SYSTEMTIME StartTime;
	DWORD dwDurationSec;
	wstring strStationName;
	unsigned short usONID;
	unsigned short usTSID;
	unsigned short usServiceID;
	unsigned short usEventID;
	unsigned char ucPriority;
	unsigned char ucTuijyuu;
	wstring strComment;
	DWORD dwRecMode;
	BOOL bPittari;
	wstring strBatPath;
	DWORD dwReserveID; //同一番組判別用ID
	BOOL bSetWait; //予約待機入った？
	DWORD dwPiledUpMode; //かぶり状態 1:かぶってチューナー足りない予約あり 2:チューナー足りなくて予約できない
	wstring strRecFolder;
	WORD wSuspendMode;
	BOOL bReboot;
	wstring strRecFilePath;
	BOOL bUseMargine;
	int iStartMargine;
	int iEndMargine;
	DWORD dwServiceMode;
	//=オペレーターの処理
	_OLD_RESERVE_DATA & operator= (const _OLD_RESERVE_DATA & o) {
		strTitle=o.strTitle;
		StartTime = o.StartTime;
		dwDurationSec = o.dwDurationSec;
		strStationName = o.strStationName;
		usONID = o.usONID;
		usTSID = o.usTSID;
		usServiceID = o.usServiceID;
		usEventID = o.usEventID;
		ucPriority = o.ucPriority;
		ucTuijyuu = o.ucTuijyuu;
		strComment = o.strComment;
		dwRecMode = o.dwRecMode;
		bPittari = o.bPittari;
		strBatPath = o.strBatPath;
		dwReserveID = o.dwReserveID;
		bSetWait = o.bSetWait;
		dwPiledUpMode = o.dwPiledUpMode;
		strRecFolder = o.strRecFolder;
		wSuspendMode = o.wSuspendMode;
		bReboot = o.bReboot;
		strRecFilePath = o.strRecFilePath;
		bUseMargine = o.bUseMargine;
		iStartMargine = o.iStartMargine;
		iEndMargine = o.iEndMargine;
		dwServiceMode = o.dwServiceMode;
		return *this;
	};
	_OLD_RESERVE_DATA & operator= (const _RESERVE_DATA & o) {
		strTitle=o.title;
		StartTime = o.startTime;
		dwDurationSec = o.durationSecond;
		strStationName = o.stationName;
		usONID = o.originalNetworkID;
		usTSID = o.transportStreamID;
		usServiceID = o.serviceID;
		usEventID = o.eventID;
		ucPriority = o.recSetting.priority;
		ucTuijyuu = o.recSetting.tuijyuuFlag;
		strComment = o.comment;
		dwRecMode = o.recSetting.recMode;
		bPittari = o.recSetting.pittariFlag;
		strBatPath = o.recSetting.batFilePath;
		dwReserveID = o.reserveID;
		bSetWait = 0;
		dwPiledUpMode = o.overlapMode;
		if( o.recSetting.recFolderList.size() >0 ){
			strRecFolder = o.recSetting.recFolderList[0].recFolder;
		}else{
			strRecFolder = L"";
		}
		wSuspendMode = o.recSetting.suspendMode;
		bReboot = o.recSetting.rebootFlag;
		strRecFilePath = o.recFilePath;
		bUseMargine = o.recSetting.useMargineFlag;
		iStartMargine = o.recSetting.startMargine;
		iEndMargine = o.recSetting.endMargine;
		dwServiceMode = o.recSetting.serviceMode;
		return *this;
	};
} OLD_RESERVE_DATA;


typedef struct _OLD_SEARCH_KEY{
	wstring strAnd;
	wstring strNot;
	BOOL bTitle;
	int iJanru;
	int iSH;
	int iSM;
	int iEH;
	int iEM;
	BOOL bChkMon;
	BOOL bChkTue;
	BOOL bChkWed;
	BOOL bChkThu;
	BOOL bChkFri;
	BOOL bChkSat;
	BOOL bChkSun;
	vector<__int64> CHIDList; //ONID<<24 | TSID<<16 | SID
	//以下自動予約登録時関係のみ使用
	int iAutoAddID; //自動予約登録一覧の識別用キー
	int iPriority;
	int iTuijyuu;
	int iRecMode;
	int iPittari;
	wstring strBat;
	wstring strRecFolder;
	WORD wSuspendMode;
	BOOL bReboot;
	BOOL bUseMargine;
	int iStartMargine;
	int iEndMargine;
	DWORD dwServiceMode;

	BOOL bRegExp;
	wstring strPattern;
	//=オペレーターの処理
	_OLD_SEARCH_KEY & operator= (const _OLD_SEARCH_KEY & o) {
		strAnd = o.strAnd;
		strNot = o.strNot;
		bTitle = o.bTitle;
		iJanru = o.iJanru;
		iSH = o.iSH;
		iSM = o.iSM;
		iEH = o.iEH;
		iEM = o.iEM;
		bChkMon = o.bChkMon;
		bChkTue = o.bChkTue;
		bChkWed = o.bChkWed;
		bChkThu = o.bChkThu;
		bChkFri = o.bChkFri;
		bChkSat = o.bChkSat;
		bChkSun = o.bChkSun;
		CHIDList = o.CHIDList;
		iAutoAddID = o.iAutoAddID;
		iPriority = o.iPriority;
		iTuijyuu = o.iTuijyuu;
		iRecMode = o.iRecMode;
		iPittari = o.iPittari;
		strBat = o.strBat;
		strRecFolder = o.strRecFolder;
		wSuspendMode = o.wSuspendMode;
		bReboot = o.bReboot;
		bUseMargine = o.bUseMargine;
		iStartMargine = o.iStartMargine;
		iEndMargine = o.iEndMargine;
		dwServiceMode = o.dwServiceMode;

		bRegExp = o.bRegExp;
		strPattern = o.strPattern;
		return *this;
	};
} OLD_SEARCH_KEY;

typedef struct _OLD_EVENT_ID_INFO{
	DWORD dwOriginalNID;
	DWORD dwTSID;
	DWORD dwServiceID;
	DWORD dwEventID;
}OLD_EVENT_ID_INFO;

typedef struct _OLD_NIBBLE_DATA{
	unsigned char ucContentNibbleLv1; //content_nibble_level_1
	unsigned char ucContentNibbleLv2; //content_nibble_level_2
	unsigned char ucUserNibbleLv1; //user_nibble
	unsigned char ucUserNibbleLv2; //user_nibble
	_OLD_NIBBLE_DATA & operator= (const _OLD_NIBBLE_DATA & o) {
		ucContentNibbleLv1 = o.ucContentNibbleLv1;
		ucContentNibbleLv2 = o.ucContentNibbleLv2;
		ucUserNibbleLv1 = o.ucUserNibbleLv1;
		ucUserNibbleLv2 = o.ucUserNibbleLv2;
		return *this;
	};
}OLD_NIBBLE_DATA;

typedef struct _OLD_EVENT_INFO_DATA3{
	WORD wOriginalNID;
	WORD wTSID;
	WORD wServiceID;
	WORD wEventID;
	wstring strEventName;
	wstring strEventText;
	wstring strEventExtText;
	SYSTEMTIME stStartTime;
	DWORD dwDurationSec;
	unsigned char ucComponentType;
	wstring strComponentTypeText;
	unsigned char ucAudioComponentType;
	unsigned char ucESMultiLangFlag;
	unsigned char ucMainComponentFlag;
	unsigned char ucSamplingRate;
	wstring strAudioComponentTypeText;
	vector<OLD_NIBBLE_DATA> NibbleList;
	vector<OLD_EVENT_ID_INFO> EventRelayList;

	wstring strSearchTitle;
	wstring strSearchInfo;
	vector<OLD_EVENT_ID_INFO> EventGroupList;

	_OLD_EVENT_INFO_DATA3 & operator= (const _OLD_EVENT_INFO_DATA3 & o) {
		wOriginalNID = o.wOriginalNID;
		wTSID = o.wTSID;
		wServiceID = o.wServiceID;
		wEventID = o.wEventID;
		strEventName = o.strEventName;
		strEventText = o.strEventText;
		strEventExtText = o.strEventExtText;
		stStartTime = o.stStartTime;
		dwDurationSec = o.dwDurationSec;
		ucComponentType = o.ucComponentType;
		strComponentTypeText = o.strComponentTypeText;
		ucAudioComponentType = o.ucAudioComponentType;
		ucESMultiLangFlag = o.ucESMultiLangFlag;
		ucMainComponentFlag = o.ucMainComponentFlag;
		ucSamplingRate = o.ucSamplingRate;
		strAudioComponentTypeText = o.strAudioComponentTypeText;
		NibbleList = o.NibbleList;
		EventRelayList = o.EventRelayList;

		strSearchTitle = o.strSearchTitle;
		strSearchInfo = o.strSearchInfo;
		EventGroupList = o.EventGroupList;
		return *this;
	};
	_OLD_EVENT_INFO_DATA3 & operator= (const _EPGDB_EVENT_INFO & o) {
		wOriginalNID = o.original_network_id;
		wTSID = o.transport_stream_id;
		wServiceID = o.service_id;
		wEventID = o.event_id;
		if( o.shortInfo != NULL ){
			strEventName = o.shortInfo->event_name;
		}else{
			strEventName = L"";
		}
		if( o.shortInfo != NULL ){
			strEventText = o.shortInfo->text_char;
		}else{
			strEventText = L"";
		}
		if( o.extInfo != NULL ){
			strEventText = o.extInfo->text_char;
		}else{
			strEventText = L"";
		}
		stStartTime = o.start_time;
		dwDurationSec = o.durationSec;
		if( o.componentInfo != NULL ){
			ucComponentType = o.componentInfo->component_type;
			strComponentTypeText = o.componentInfo->text_char;
		}else{
			ucComponentType = 0;
			strComponentTypeText = L"";
		}
		if( o.audioInfo != NULL ){
			if( o.audioInfo->componentList.size() > 0 ){
				ucAudioComponentType = o.audioInfo->componentList[0].component_type;
				ucESMultiLangFlag = o.audioInfo->componentList[0].ES_multi_lingual_flag;
				ucMainComponentFlag = o.audioInfo->componentList[0].main_component_flag;
				ucSamplingRate = o.audioInfo->componentList[0].sampling_rate;
				strAudioComponentTypeText = o.audioInfo->componentList[0].text_char;
			}
		}
		if( o.contentInfo != NULL ){
			NibbleList.clear();
			for( size_t i=0; i<o.contentInfo->nibbleList.size(); i++ ){
				OLD_NIBBLE_DATA item;
				item.ucContentNibbleLv1 = o.contentInfo->nibbleList[i].content_nibble_level_1;
				item.ucContentNibbleLv2 = o.contentInfo->nibbleList[i].content_nibble_level_2;
				item.ucUserNibbleLv1 = o.contentInfo->nibbleList[i].user_nibble_1;
				item.ucUserNibbleLv2 = o.contentInfo->nibbleList[i].user_nibble_2;
				NibbleList.push_back(item);
			}
		}
		if( o.eventGroupInfo != NULL ){
			EventGroupList.clear();
			for( size_t i=0; i<o.eventGroupInfo->eventDataList.size(); i++ ){
				OLD_EVENT_ID_INFO item;
				item.dwOriginalNID = o.eventGroupInfo->eventDataList[i].original_network_id;
				item.dwTSID = o.eventGroupInfo->eventDataList[i].transport_stream_id;
				item.dwServiceID = o.eventGroupInfo->eventDataList[i].service_id;
				item.dwEventID = o.eventGroupInfo->eventDataList[i].event_id;
				EventGroupList.push_back(item);
			}
		}
		if( o.eventRelayInfo != NULL ){
			EventRelayList.clear();
			for( size_t i=0; i<o.eventRelayInfo->eventDataList.size(); i++ ){
				OLD_EVENT_ID_INFO item;
				item.dwOriginalNID = o.eventRelayInfo->eventDataList[i].original_network_id;
				item.dwTSID = o.eventRelayInfo->eventDataList[i].transport_stream_id;
				item.dwServiceID = o.eventRelayInfo->eventDataList[i].service_id;
				item.dwEventID = o.eventRelayInfo->eventDataList[i].event_id;
				EventRelayList.push_back(item);
			}
		}

		return *this;
	};
}OLD_EVENT_INFO_DATA3;

#endif