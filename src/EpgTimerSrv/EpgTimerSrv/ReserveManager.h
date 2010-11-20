#pragma once

#include "../../Common/Util.h"
#include "../../Common/EpgTimerUtil.h"
#include "../../Common/PathUtil.h"
#include "../../Common/StringUtil.h"
#include "../../Common/ParseReserveText.h"
#include "../../Common/ParseRecInfoText.h"
#include "../../Common/ParseChText5.h"

#include "ReserveInfo.h"
#include "TunerManager.h"
#include "BatManager.h"

class CReserveManager
{
public:
	CReserveManager(void);
	~CReserveManager(void);

	void SetRegistGUI(map<DWORD, DWORD> registGUIMap);
	void SetRegistTCP(map<wstring, REGIST_TCP_INFO> registTCPMap);

	void ReloadSetting();

	//予約情報の読み込みを行う
	//戻り値：
	// TRUE（成功）、FALSE（失敗）
	BOOL ReloadReserveData();

	//予約情報を追加で読み込む
	//戻り値：
	// TRUE（成功）、FALSE（失敗）
	BOOL AddLoadReserveData();

	//予約情報を取得する
	//戻り値：
	// TRUE（成功）、FALSE（失敗）
	//引数：
	// reserveList		[OUT]予約情報一覧（呼び出し元で解放する必要あり）
	BOOL GetReserveDataAll(
		vector<RESERVE_DATA*>* reserveList
		);

	//チューナー毎の予約情報を取得する
	//戻り値：
	// TRUE（成功）、FALSE（失敗）
	//引数：
	// reserveList		[OUT]予約情報一覧
	BOOL GetTunerReserveAll(
		vector<TUNER_RESERVE_INFO>* list
		);

	//予約情報を取得する
	//戻り値：
	// TRUE（成功）、FALSE（失敗）
	//引数：
	// id				[IN]予約ID
	// reserveData		[OUT]予約情報
	BOOL GetReserveData(
		DWORD id,
		RESERVE_DATA* reserveData
		);

	//予約情報を追加する
	//戻り値：
	// TRUE（成功）、FALSE（失敗）
	//引数：
	// reserveList		[IN]予約情報
	BOOL AddReserveData(
		vector<RESERVE_DATA>* reserveList
		);

	//予約情報を変更する
	//戻り値：
	// TRUE（成功）、FALSE（失敗）
	//引数：
	// reserveList		[IN]予約情報
	BOOL ChgReserveData(
		vector<RESERVE_DATA>* reserveList
		);

	//予約情報を削除する
	//戻り値：
	// TRUE（成功）、FALSE（失敗）
	//引数：
	// reserveList		[IN]予約IDリスト
	BOOL DelReserveData(
		vector<DWORD>* reserveList
		);

	//予約の振り分けを行う
	void ReloadBankMap(BOOL notify);
	
	//録画済み情報一覧を取得する
	//戻り値：
	// TRUE（成功）、FALSE（失敗）
	//引数：
	// infoList			[OUT]録画済み情報一覧
	BOOL GetRecFileInfoAll(
		vector<REC_FILE_INFO>* infoList
		);

	//録画済み情報を削除する
	//戻り値：
	// TRUE（成功）、FALSE（失敗）
	//引数：
	// idList			[IN]IDリスト
	BOOL DelRecFileInfo(
		vector<DWORD>* idList
		);


	BOOL IsEnableSuspend(
		BYTE* suspendMode,
		BYTE* rebootFlag
		);

	BOOL IsEnableReloadEPG(
		);

	BOOL IsSuspendOK();

	BOOL GetSleepReturnTime(
		LONGLONG* returnTime
		);

	BOOL StartEpgCap();
	void StopEpgCap();

	BOOL IsFindReserve(
		WORD ONID,
		WORD TSID,
		WORD SID,
		WORD eventID
		);

	BOOL IsFindReserve(
		WORD ONID,
		WORD TSID,
		WORD SID,
		LONGLONG startTime,
		DWORD durationSec
		);

	void SendNotifyUpdate();
	void SendNotifyEpgReload();

	BOOL GetTVTestChgCh(
		LONGLONG chID,
		TVTEST_CH_CHG_INFO* chInfo
		);

	BOOL SetNWTVCh(
		SET_CH_INFO* chInfo
		);

	BOOL CloseNWTV(
		);

	void SetNWTVMode(
		DWORD mode
		);

protected:
	HANDLE lockEvent;

	HANDLE bankCheckThread;
	HANDLE bankCheckStopEvent;


	map<DWORD, DWORD> registGUIMap;
	map<wstring, REGIST_TCP_INFO> registTCPMap;
	HANDLE lockNotify;
	HANDLE notifyThread;
	HANDLE notifyStopEvent;
	WORD notifyStatus;
	HANDLE notifyStatusThread;
	HANDLE notifyStatusStopEvent;

	HANDLE notifyEpgReloadThread;
	HANDLE notifyEpgReloadStopEvent;

	CParseReserveText reserveText;
	map<DWORD, CReserveInfo*> reserveInfoMap; //キー　reserveID
	map<LONGLONG, DWORD> reserveInfoIDMap; //キー　ONID<<48|TSID<<32|SID<<16|EventID
	CParseRecInfoText recInfoText;

	CParseChText5 chUtil;

	CTunerManager tunerManager;
	CBatManager batManager;

	typedef struct _BANK_WORK_INFO{
		CReserveInfo* reserveInfo;
		LONGLONG startTime;
		LONGLONG endTime;
		BYTE priority;
		BOOL recWaitFlag;
		wstring sortKey;
		DWORD reserveID;
		DWORD chID;		//originalNetworkID<<16 | transportStreamID
		DWORD preTunerID;
		DWORD useTunerID;
		WORD ONID;
		WORD TSID;
		WORD SID;
	}BANK_WORK_INFO;
	typedef struct _BANK_INFO{
		DWORD tunerID;
		map<DWORD, BANK_WORK_INFO*> reserveList; //キー 予約ID
	}BANK_INFO;
	map<DWORD, BANK_INFO*> bankMap;
	map<DWORD, BANK_WORK_INFO*> NGReserveMap;

	int defStartMargine;
	int defEndMargine;

	BOOL backPriorityFlag;

	map<DWORD, CTunerBankCtrl*> tunerBankMap; //キー bonID<<16 | tunerID

	BYTE enableSetSuspendMode;
	BYTE enableSetRebootFlag;
	BYTE enableEpgReload;

	BOOL epgCapCheckFlag;


	BOOL BSOnly;
	BOOL CS1Only;
	BOOL CS2Only;
	LONGLONG ngCapMin;
	LONGLONG ngCapTunerMin;
	vector<DWORD> epgCapTimeList;
	int wakeTime;
	BYTE defSuspendMode;
	BYTE defRebootFlag;
	int batMargin;
	vector<wstring> noStandbyExeList;
	DWORD noStandbyTime;
	BOOL autoDel;
	vector<wstring> delExtList;
	vector<wstring> delFolderList;
	BOOL eventRelay;

	vector<wstring> tvtestUseBon;

	int duraChgMarginMin;
	int notFindTuijyuHour;
	int noEpgTuijyuMin;

	BOOL autoDelRecInfo;
	int autoDelRecInfoNum;
	BOOL timeSync;
	BOOL setTimeSync;

	DWORD NWTVPID;
	wstring recExePath;
	CSendCtrlCmd sendCtrlNWTV;
	BOOL NWTVUDP;
	BOOL NWTVTCP;
protected:
	//PublicAPI排他制御用
	BOOL Lock(LPCWSTR log = NULL, DWORD timeOut = 60*1000);
	void UnLock(LPCWSTR log = NULL);

	BOOL NotifyLock(LPCWSTR log = NULL, DWORD timeOut = 60*1000);
	void NotifyUnLock(LPCWSTR log = NULL);


	BOOL _AddReserveData(RESERVE_DATA* reserve);
	BOOL _ChgReserveData(RESERVE_DATA* reserve, BOOL chgTime);

	void _ReloadBankMap();
	void CheckOverTimeReserve();
	void CreateWorkData(CReserveInfo* reserveInfo, BANK_WORK_INFO* workInfo, BOOL backPriority, DWORD reserveCount, DWORD reserveNum);
	DWORD ChkInsertStatus(BANK_INFO* bank, BANK_WORK_INFO* inItem);
	DWORD ReChkInsertStatus(BANK_INFO* bank, BANK_WORK_INFO* inItem);
	DWORD ChkInsertNGStatus(BANK_INFO* bank, BANK_WORK_INFO* inItem);

	void _SendNotifyUpdate();
	static UINT WINAPI SendNotifyThread(LPVOID param);
	void SendNotifyStatus(WORD status);
	static UINT WINAPI SendNotifyStatusThread(LPVOID param);
	void _SendNotifyEpgReload();
	static UINT WINAPI SendNotifyEpgReloadThread(LPVOID param);

	BOOL _DelReserveData(
		vector<DWORD>* reserveList
	);

	static UINT WINAPI BankCheckThread(LPVOID param);
	void CheckEndReserve();
	void CheckErrReserve();
	void CheckBatWork();
	void CheckTuijyu();
	BOOL CheckEventRelay(EPGDB_EVENT_INFO* info, RESERVE_DATA* data, BOOL errEnd = FALSE);

	BOOL CheckChgEvent(EPGDB_EVENT_INFO* info, RESERVE_DATA* data, BYTE* chgMode = NULL);
	BOOL CheckNotFindChgEvent(RESERVE_DATA* data, CTunerBankCtrl* ctrl);
	BOOL ChgDurationChk(EPGDB_EVENT_INFO* info);

	void EnableSuspendWork(BYTE suspendMode, BYTE rebootFlag, BYTE epgReload);
	BOOL _IsSuspendOK(BOOL rebootFlag);
	BOOL IsFindNoSuspendExe();

	BOOL GetNextEpgcapTime(LONGLONG* capTime, LONGLONG chkMargineMin);

	BOOL _StartEpgCap();
	BOOL IsEpgCap();
};

