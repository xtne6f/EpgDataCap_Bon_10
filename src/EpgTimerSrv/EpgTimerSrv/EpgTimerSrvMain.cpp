#include "StdAfx.h"
#include "EpgTimerSrvMain.h"

#include "../../Common/CommonDef.h"
#include "../../Common/CtrlCmdDef.h"
#include "../../Common/CtrlCmdUtil.h"
#include "../../Common/CtrlCmdUtil2.h"
#include "../../Common/StringUtil.h"

#include <process.h>

CEpgTimerSrvMain::CEpgTimerSrvMain(void)
{
	this->lockEvent = _CreateEvent(FALSE, TRUE, NULL);

	this->stopEvent = _CreateEvent(TRUE, FALSE,NULL);

	this->reloadEpgChkFlag = FALSE;

	this->suspendMode = 0xFF;
	this->rebootFlag= 0xFF;
	this->sleepThread = NULL;

	this->suspending = FALSE;

	this->pipeServer = NULL;
	this->tcpServer = NULL;

	this->enableTCPSrv = FALSE;
	this->tcpPort = 4510;
	this->autoAddDays = 8;
	this->chkGroupEvent = TRUE;
	this->rebootDef = 0;

	this->awayMode = FALSE;

	OSVERSIONINFO osvi;
	ZeroMemory(&osvi, sizeof(OSVERSIONINFO));
	osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);
	GetVersionEx((OSVERSIONINFO*)&osvi);
	if( VER_PLATFORM_WIN32_NT==osvi.dwPlatformId ){
		if( osvi.dwMajorVersion >= 6 ){
			//Vista以降
			this->awayMode = TRUE;
		}
	}

}


CEpgTimerSrvMain::~CEpgTimerSrvMain(void)
{
	if( this->stopEvent != NULL ){
		CloseHandle(this->stopEvent);
	}

	if( this->tcpServer != NULL ){
		this->tcpServer->StopServer();
		SAFE_DELETE(this->tcpServer);
	}
	if( this->pipeServer != NULL ){
		this->pipeServer->StopServer();
		SAFE_DELETE(this->pipeServer);
	}

	if( this->lockEvent != NULL ){
		UnLock();
		CloseHandle(this->lockEvent);
		this->lockEvent = NULL;
	}
}

BOOL CEpgTimerSrvMain::Lock(LPCWSTR log, DWORD timeOut)
{
	if( this->lockEvent == NULL ){
		return FALSE;
	}
	if( log != NULL ){
		OutputDebugString(log);
	}
	DWORD dwRet = WaitForSingleObject(this->lockEvent, timeOut);
	if( dwRet == WAIT_ABANDONED || 
		dwRet == WAIT_FAILED ||
		dwRet == WAIT_TIMEOUT){
			OutputDebugString(L"◆CEpgTimerSrvMain::Lock FALSE");
		return FALSE;
	}
	return TRUE;
}

void CEpgTimerSrvMain::UnLock(LPCWSTR log)
{
	if( this->lockEvent != NULL ){
		SetEvent(this->lockEvent);
	}
	if( log != NULL ){
		OutputDebugString(log);
	}
}

//メインループ処理
//引数：
// serviceFlag			[IN]サービスとしての起動かどうか
void CEpgTimerSrvMain::StartMain(
	BOOL serviceFlag
	)
{
	ReloadSetting();

	this->reserveManager.ReloadReserveData();

	wstring epgAutoAddFilePath;
	GetSettingPath(epgAutoAddFilePath);
	epgAutoAddFilePath += L"\\";
	epgAutoAddFilePath += EPG_AUTO_ADD_TEXT_NAME;

	wstring manualAutoAddFilePath;
	GetSettingPath(manualAutoAddFilePath);
	manualAutoAddFilePath += L"\\";
	manualAutoAddFilePath += MANUAL_AUTO_ADD_TEXT_NAME;

	this->epgAutoAdd.ParseText(epgAutoAddFilePath.c_str());
	this->manualAutoAdd.ParseText(manualAutoAddFilePath.c_str());

	//Pipeサーバースタート
	if( this->pipeServer == NULL ){
		pipeServer = new CPipeServer;
	}
	pipeServer->StartServer(CMD2_EPG_SRV_EVENT_WAIT_CONNECT, CMD2_EPG_SRV_PIPE, CtrlCmdCallback, this, 0, GetCurrentProcessId());

	this->epgDB.ReloadEpgData();
	this->reserveManager.ReloadBankMap(FALSE);
	this->reloadEpgChkFlag = TRUE;

	CSendCtrlCmd sendCtrl;
	DWORD countChkSuspend = 11;
	
	while(1){
		if( WaitForSingleObject(this->stopEvent, 1*1000) == WAIT_OBJECT_0 ){
			break;
		}
		if( this->reloadEpgChkFlag == TRUE ){
			if( this->epgDB.IsLoadingData() == FALSE ){
				//リロード終わったので自動予約登録処理を行う
				if( Lock() == TRUE ){
					CheckTuijyu();
					AutoAddReserveEPG();
					AutoAddReserveProgram();
					this->reserveManager.ReloadBankMap(TRUE);
					UnLock();
				}
				this->reloadEpgChkFlag = FALSE;
				this->reserveManager.SendNotifyEpgReload();

				//リロードタイミングで予約始まったかもしれないのでチェック
				BOOL streamingChk = TRUE;
				if( this->ngFileStreaming == TRUE ){
					if( this->streamingManager.IsStreaming() == TRUE ){
						streamingChk = FALSE;
					}
				}

				if( this->reserveManager.IsSuspendOK() == TRUE && streamingChk == TRUE){
					if( this->suspendMode != 0xFF && this->rebootFlag != 0xFF ){
						//問い合わせ
						if( this->suspendMode != 0 && this->suspendMode != 4 ){
							if( QuerySleep(this->rebootFlag, this->suspendMode) == FALSE ){
								StartSleep(this->rebootFlag, this->suspendMode);
							}
						}
					}
				}
				countChkSuspend = 11;
				this->suspendMode = 0xFF;
				this->rebootFlag = 0xFF;
			}
		}
		//予約終了後の動作チェック
		if( this->reserveManager.IsEnableSuspend(&this->suspendMode, &this->rebootFlag ) == TRUE ){
			OutputDebugString(L"★IsEnableSuspend");
			this->reloadEpgChkFlag = TRUE;
			this->epgDB.ReloadEpgData();
		}else{
			if( this->reserveManager.IsEnableReloadEPG() == TRUE ){
				this->reloadEpgChkFlag = TRUE;
				this->epgDB.ReloadEpgData();
			}
		}

		if( countChkSuspend > 10 ){
			BOOL streamingChk = TRUE;
			if( this->ngFileStreaming == TRUE ){
				if( this->streamingManager.IsStreaming() == TRUE ){
					streamingChk = FALSE;
				}
			}
			if( this->reserveManager.IsSuspendOK() == FALSE || streamingChk == FALSE){
				DWORD esMode = ES_SYSTEM_REQUIRED|ES_CONTINUOUS;
				if( this->awayMode == TRUE ){
					esMode |= ES_AWAYMODE_REQUIRED;
				}
				SetThreadExecutionState(esMode);
			}else{
				SetThreadExecutionState(ES_CONTINUOUS);
			}
			countChkSuspend = 0;

			LONGLONG returnTime = 0;
			if( reserveManager.GetSleepReturnTime(&returnTime) == TRUE ){
				if( sleepUtil.SetReturnTime(returnTime, this->rebootFlagWork, this->wakeMargin) == TRUE ){
				}
			}
		}
		countChkSuspend++;
	}
	pipeServer->StopServer();
}

void CEpgTimerSrvMain::ReloadSetting()
{
	wstring iniPath = L"";
	GetModuleIniPath(iniPath);
	this->enableTCPSrv = GetPrivateProfileInt(L"SET", L"EnableTCPSrv", 0, iniPath.c_str());
	this->tcpPort = GetPrivateProfileInt(L"SET", L"TCPPort", 4510, iniPath.c_str());

	if( this->enableTCPSrv == FALSE){
		if( this->tcpServer != NULL ){
			this->tcpServer->StopServer();
			SAFE_DELETE(this->tcpServer);
		}
	}else{
		if( this->tcpServer == NULL ){
			this->tcpServer = new CTCPServer;
			this->tcpServer->StartServer(this->tcpPort, CtrlCmdCallback, this, 0, GetCurrentProcessId());
		}
	}

	this->wakeMargin = GetPrivateProfileInt(L"SET", L"WakeTime", 5, iniPath.c_str());
	this->autoAddDays = GetPrivateProfileInt(L"SET", L"AutoAddDays", 8, iniPath.c_str());
	this->chkGroupEvent = GetPrivateProfileInt(L"SET", L"ChkGroupEvent", 1, iniPath.c_str());
	this->rebootDef = (BYTE)GetPrivateProfileInt(L"SET", L"Reboot", 0, iniPath.c_str());
	this->ngFileStreaming = (BYTE)GetPrivateProfileInt(L"NO_SUSPEND", L"NoFileStreaming", 0, iniPath.c_str());
}

//メイン処理停止
void CEpgTimerSrvMain::StopMain()
{
	this->epgDB.CancelLoadData();
	if( this->stopEvent != NULL ){
		SetEvent(this->stopEvent);
	}
}

void CEpgTimerSrvMain::StartSleep(BYTE rebootFlag, BYTE suspendMode)
{
	if( this->sleepThread != NULL ){
		if( ::WaitForSingleObject(this->sleepThread, 0) == WAIT_OBJECT_0 ){
			CloseHandle(this->sleepThread);
			this->sleepThread = NULL;
		}
	}
	if( this->sleepThread == NULL ){
		this->rebootFlagWork = rebootFlag;
		this->suspendModeWork = suspendMode;
		this->sleepThread = (HANDLE)_beginthreadex(NULL, 0, SleepThread, (LPVOID)this, CREATE_SUSPENDED, NULL);
		SetThreadPriority( this->sleepThread, THREAD_PRIORITY_NORMAL );
		ResumeThread(this->sleepThread);
	}
}

UINT WINAPI CEpgTimerSrvMain::SleepThread(void* param)
{
	CEpgTimerSrvMain* sys = (CEpgTimerSrvMain*)param;
	sys->suspending = TRUE;

	if( sys->rebootFlagWork == 1 && sys->suspendModeWork == 0xFF ){
		sys->StartReboot();
		return 0;
	}

	SetThreadExecutionState(ES_CONTINUOUS);

	LONGLONG returnTime = 0;
	if( sys->reserveManager.GetSleepReturnTime(&returnTime) == TRUE ){
		SYSTEMTIME retTime;
		ConvertSystemTime(returnTime, &retTime);
		wstring strTime;
		GetTimeString(retTime, strTime);
		_OutputDebugString(L"ReturnTime: %s", strTime.c_str());
		if( sys->sleepUtil.SetReturnTime(returnTime, sys->rebootFlagWork, sys->wakeMargin) == TRUE ){
			//ストリーミングを終了する
			sys->streamingManager.CloseAllFile();

			if( sys->suspendModeWork == 1 ){
				sys->sleepUtil.SetStandby(TRUE);
				if( sys->rebootFlagWork == 1 ){
					if( sys->QueryReboot(1) == FALSE ){
						sys->StartReboot();
					}
				}
			}else if( sys->suspendModeWork == 2 ){
				sys->sleepUtil.SetStandby(FALSE);
				if( sys->rebootFlagWork == 1 ){
					if( sys->QueryReboot(1) == FALSE ){
						sys->StartReboot();
					}
				}
			}else if( sys->suspendModeWork == 3 ){
				TOKEN_PRIVILEGES TokenPri;
				HANDLE hToken;

				if ( OpenProcessToken(GetCurrentProcess(),(TOKEN_ADJUST_PRIVILEGES|TOKEN_QUERY),&hToken) ){
					LookupPrivilegeValue(NULL,SE_SHUTDOWN_NAME,&TokenPri.Privileges[0].Luid);

					TokenPri.PrivilegeCount = 1;
					TokenPri.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
					AdjustTokenPrivileges( hToken, FALSE, &TokenPri, 0, NULL, NULL );
					if ( GetLastError() == ERROR_SUCCESS ){
						ExitWindowsEx(EWX_POWEROFF,0);
					}
				}
			}
		}
	}else{
		//ストリーミングを終了する
		sys->streamingManager.CloseAllFile();

		if( sys->suspendModeWork == 1 ){
			sys->sleepUtil.SetStandby(TRUE);
			if( sys->rebootFlagWork == 1 ){
				if( sys->QueryReboot(1) == FALSE ){
					sys->StartReboot();
				}
			}
		}else if( sys->suspendModeWork == 2 ){
			sys->sleepUtil.SetStandby(FALSE);
			if( sys->rebootFlagWork == 1 ){
				if( sys->QueryReboot(1) == FALSE ){
					sys->StartReboot();
				}
			}
		}else if( sys->suspendModeWork == 3 ){
			TOKEN_PRIVILEGES TokenPri;
			HANDLE hToken;

			if ( OpenProcessToken(GetCurrentProcess(),(TOKEN_ADJUST_PRIVILEGES|TOKEN_QUERY),&hToken) ){
				LookupPrivilegeValue(NULL,SE_SHUTDOWN_NAME,&TokenPri.Privileges[0].Luid);

				TokenPri.PrivilegeCount = 1;
				TokenPri.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
				AdjustTokenPrivileges( hToken, FALSE, &TokenPri, 0, NULL, NULL );
				if ( GetLastError() == ERROR_SUCCESS ){
					ExitWindowsEx(EWX_POWEROFF,0);
				}
			}
		}
	}

	sys->suspending = FALSE;

	return 0;
}

void CEpgTimerSrvMain::StartReboot()
{
	TOKEN_PRIVILEGES TokenPri;
	HANDLE hToken;
	if ( OpenProcessToken(GetCurrentProcess(),(TOKEN_ADJUST_PRIVILEGES|TOKEN_QUERY),&hToken) ){
		LookupPrivilegeValue(NULL,SE_SHUTDOWN_NAME,&TokenPri.Privileges[0].Luid);

		TokenPri.PrivilegeCount = 1;
		TokenPri.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
		AdjustTokenPrivileges( hToken, FALSE, &TokenPri, 0, NULL, NULL );
		if ( GetLastError() == ERROR_SUCCESS ){
			ExitWindowsEx(EWX_REBOOT,0);
		}
	}
}

BOOL CEpgTimerSrvMain::QuerySleep(BYTE rebootFlag, BYTE suspendMode)
{
	CSendCtrlCmd sendCtrl;
	BOOL ret = FALSE;

	map<DWORD,DWORD>::iterator itrReg;
	vector<DWORD> errID;
	for( itrReg = this->registGUI.begin(); itrReg != this->registGUI.end(); itrReg++){
		if( _FindOpenExeProcess(itrReg->first) == TRUE ){
			wstring pipe;
			wstring waitEvent;
			Format(pipe, L"%s%d", CMD2_GUI_CTRL_PIPE, itrReg->first);
			Format(waitEvent, L"%s%d", CMD2_GUI_CTRL_WAIT_CONNECT, itrReg->first);

			sendCtrl.SetPipeSetting(waitEvent, pipe);
			sendCtrl.SetConnectTimeOut(5*1000);
			if( sendCtrl.SendGUIQuerySuspend(rebootFlag, suspendMode) != CMD_SUCCESS ){
				errID.push_back(itrReg->first);
			}else{
				ret = TRUE;
				break;
			}
		}else{
			errID.push_back(itrReg->first);
		}
	}
	for( size_t i=0; i<errID.size(); i++ ){
		itrReg = this->registGUI.find(errID[i]);
		if( itrReg != this->registGUI.end() ){
			this->registGUI.erase(itrReg);
		}
	}

	return ret;
}

BOOL CEpgTimerSrvMain::QueryReboot(BYTE rebootFlag)
{
	CSendCtrlCmd sendCtrl;
	BOOL ret = FALSE;

	map<DWORD,DWORD>::iterator itrReg;
	vector<DWORD> errID;
	for( itrReg = this->registGUI.begin(); itrReg != this->registGUI.end(); itrReg++){
		if( _FindOpenExeProcess(itrReg->first) == TRUE ){
			wstring pipe;
			wstring waitEvent;
			Format(pipe, L"%s%d", CMD2_GUI_CTRL_PIPE, itrReg->first);
			Format(waitEvent, L"%s%d", CMD2_GUI_CTRL_WAIT_CONNECT, itrReg->first);

			sendCtrl.SetPipeSetting(waitEvent, pipe);
			sendCtrl.SetConnectTimeOut(5*1000);
			if( sendCtrl.SendGUIQueryReboot(rebootFlag) != CMD_SUCCESS ){
				errID.push_back(itrReg->first);
			}else{
				ret = TRUE;
				break;
			}
		}else{
			errID.push_back(itrReg->first);
		}
	}
	for( size_t i=0; i<errID.size(); i++ ){
		itrReg = this->registGUI.find(errID[i]);
		if( itrReg != this->registGUI.end() ){
			this->registGUI.erase(itrReg);
		}
	}

	return ret;
}

//休止／スタンバイ移行処理中かどうか
//戻り値：
// TRUE（移行中）、FALSE
BOOL CEpgTimerSrvMain::IsSuspending()
{
	BOOL ret = FALSE;
	ret = this->suspending;
	return ret;
}

//休止／スタンバイに移行して構わない状況かどうか
//戻り値：
// TRUE（構わない）、FALSE（移行しては駄目）
BOOL CEpgTimerSrvMain::ChkSuspend()
{
	BOOL ret = FALSE;
	BOOL streamingChk = TRUE;
	if( this->ngFileStreaming == TRUE ){
		if( this->streamingManager.IsStreaming() == TRUE ){
			streamingChk = FALSE;
		}
	}
	if( streamingChk == TRUE ){
		ret = this->reserveManager.IsSuspendOK();
	}
	return ret;
}

BOOL CEpgTimerSrvMain::CheckTuijyu()
{
	BOOL ret = FALSE;

	wstring iniAppPath = L"";
	GetModuleIniPath(iniAppPath);

	BOOL chgTitle = GetPrivateProfileInt(L"SET", L"ResAutoChgTitle", 1, iniAppPath.c_str());
	BOOL chkTime = GetPrivateProfileInt(L"SET", L"ResAutoChkTime", 1, iniAppPath.c_str());

	vector<RESERVE_DATA*> reserveList;
	vector<RESERVE_DATA> chgList;
	this->reserveManager.GetReserveDataAll(&reserveList);
	for( size_t i=0; i<reserveList.size(); i++ ){
		if( reserveList[i]->recSetting.recMode == RECMODE_NO ){
			continue;
		}
		if( reserveList[i]->eventID == 0xFFFF ){
			continue;
		}
		if( reserveList[i]->recSetting.tuijyuuFlag == 0 ){
			continue;
		}
		if( reserveList[i]->reserveStatus != ADD_RESERVE_NORMAL ){
			continue;
		}

		RESERVE_DATA oldData = *(reserveList[i]);
		EPGDB_EVENT_INFO* info;
		if( this->epgDB.SearchEpg(
			reserveList[i]->originalNetworkID,
			reserveList[i]->transportStreamID,
			reserveList[i]->serviceID,
			reserveList[i]->eventID,
			&info
			) == TRUE){

				BOOL chgRes = FALSE;
				if( info->StartTimeFlag == 1 ){
					if( ConvertI64Time(reserveList[i]->startTime) != ConvertI64Time(info->start_time) ){
						reserveList[i]->startTime = info->start_time;
						chgRes = TRUE;
					}
				}
				if( info->DurationFlag == 1 ){
					if( reserveList[i]->durationSecond != info->durationSec ){
						reserveList[i]->durationSecond = info->durationSec;
						chgRes = TRUE;
					}
				}
				if( chgTitle == TRUE ){
					if( info->shortInfo != NULL ){
						if( CompareNoCase(reserveList[i]->title, info->shortInfo->event_name) != 0 ){
							reserveList[i]->title = info->shortInfo->event_name;
							chgRes = TRUE;
						}
					}
				}
				if( chgRes == TRUE ){
					chgList.push_back(*(reserveList[i]));
					this->reserveManager.SendTweet(TW_CHG_RESERVE_RELOADEPG, &oldData, reserveList[i], NULL);
				}
		}else{
			//IDで見つからなかったので時間で検索してみる
			if( this->epgDB.SearchEpg(
				reserveList[i]->originalNetworkID,
				reserveList[i]->transportStreamID,
				reserveList[i]->serviceID,
				ConvertI64Time(reserveList[i]->startTime),
				reserveList[i]->durationSecond,
				&info
				) == TRUE){

					reserveList[i]->eventID = info->event_id;

					if( chkTime == FALSE ){
						//番組名も同じか確認
						if( info->shortInfo != NULL ){
							if( CompareNoCase(reserveList[i]->title, info->shortInfo->event_name) == 0 ){
								chgList.push_back(*(reserveList[i]));

								this->reserveManager.SendTweet(TW_CHG_RESERVE_RELOADEPG, &oldData, reserveList[i], NULL);
							}
						}
					}else{
						//時間のみで判断
						if( chgTitle == TRUE ){
							if( info->shortInfo != NULL ){
								if( CompareNoCase(reserveList[i]->title, info->shortInfo->event_name) != 0 ){
									reserveList[i]->title = info->shortInfo->event_name;
								}
							}
						}
						chgList.push_back(*(reserveList[i]));

						this->reserveManager.SendTweet(TW_CHG_RESERVE_RELOADEPG, &oldData, reserveList[i], NULL);

					}
			}
		}
	}
	if( chgList.size() > 0 ){
		this->reserveManager.ChgReserveData(&chgList);
		ret = TRUE;
	}
	for( size_t i=0; i<reserveList.size(); i++ ){
		SAFE_DELETE(reserveList[i]);
	}

	return ret;
}


BOOL CEpgTimerSrvMain::AutoAddReserveEPG()
{
	BOOL ret = TRUE;

	map<ULONGLONG, RESERVE_DATA*> addMap;
	map<ULONGLONG, RESERVE_DATA*>::iterator itrAdd;

	LONGLONG nowTime = GetNowI64Time();

	map<DWORD, EPG_AUTO_ADD_DATA*>::iterator itrKey;
	for( itrKey = this->epgAutoAdd.dataIDMap.begin(); itrKey != this->epgAutoAdd.dataIDMap.end(); itrKey++ ){
		vector<EPGDB_SEARCH_KEY_INFO> searchKey;
		vector<EPGDB_EVENT_INFO*> result;

		searchKey.push_back(itrKey->second->searchInfo);

		this->epgDB.SearchEpg(&searchKey, &result);
		for( size_t i=0; i<result.size(); i++ ){
			if( result[i]->StartTimeFlag == 0 || result[i]->DurationFlag == 0 ){
				//時間未定なので対象外
				continue;
			}
			if( ConvertI64Time(result[i]->start_time) < nowTime ){
				//開始時間過ぎているので対象外
				continue;
			}
			if( nowTime + ((LONGLONG)this->autoAddDays)*24*60*60*I64_1SEC < ConvertI64Time(result[i]->start_time)){
				//対象期間外
				continue;
			}

			if(this->reserveManager.IsFindReserve(
				result[i]->original_network_id,
				result[i]->transport_stream_id,
				result[i]->service_id,
				result[i]->event_id
				) == FALSE ){
					ULONGLONG eventKey = _Create64Key2(
						result[i]->original_network_id,
						result[i]->transport_stream_id,
						result[i]->service_id,
						result[i]->event_id
						);

					itrAdd = addMap.find(eventKey);
					if( itrAdd == addMap.end() ){
						//まだ存在しないので追加対象
						if(result[i]->eventGroupInfo != NULL && this->chkGroupEvent == TRUE){
							//イベントグループのチェックをする
							BOOL findGroup = FALSE;
							for(size_t j=0; j<result[i]->eventGroupInfo->eventDataList.size(); j++ ){
								EPGDB_EVENT_DATA groupData = result[i]->eventGroupInfo->eventDataList[j];
								if(this->reserveManager.IsFindReserve(
									groupData.original_network_id,
									groupData.transport_stream_id,
									groupData.service_id,
									groupData.event_id
									) == TRUE ){
										findGroup = TRUE;
										break;
								}
					
								ULONGLONG eventKey = _Create64Key2(
									groupData.original_network_id,
									groupData.transport_stream_id,
									groupData.service_id,
									groupData.event_id
									);

								itrAdd = addMap.find(eventKey);
								if( itrAdd != addMap.end() ){
									findGroup = TRUE;
									break;
								}
							}
							if( findGroup == TRUE ){
								continue;
							}
						}
						//まだ存在しないので追加対象
						RESERVE_DATA* addItem = new RESERVE_DATA;
						if( result[i]->shortInfo != NULL ){
							addItem->title = result[i]->shortInfo->event_name;
						}
						addItem->startTime = result[i]->start_time;
						addItem->startTimeEpg = result[i]->start_time;
						addItem->durationSecond = result[i]->durationSec;
						this->epgDB.SearchServiceName(
							result[i]->original_network_id,
							result[i]->transport_stream_id,
							result[i]->service_id,
							addItem->stationName
							);
						addItem->originalNetworkID = result[i]->original_network_id;
						addItem->transportStreamID = result[i]->transport_stream_id;
						addItem->serviceID = result[i]->service_id;
						addItem->eventID = result[i]->event_id;

						addItem->recSetting = itrKey->second->recSetting;

						addMap.insert(pair<ULONGLONG, RESERVE_DATA*>(eventKey, addItem));
					}else{
						//無効ならそれを優先
						if( itrKey->second->recSetting.recMode == RECMODE_NO ){
							itrAdd->second->recSetting.recMode = RECMODE_NO;
						}
					}
			}
		}
	}

	vector<RESERVE_DATA> setList;
	for( itrAdd = addMap.begin(); itrAdd != addMap.end(); itrAdd++ ){
		setList.push_back(*(itrAdd->second));
		SAFE_DELETE(itrAdd->second);
	}
	addMap.clear();
	if( setList.size() > 0 ){
		this->reserveManager.AddReserveData(&setList, TRUE);
		setList.clear();
	}

	return ret;
}

BOOL CEpgTimerSrvMain::AutoAddReserveProgram()
{
	BOOL ret = TRUE;

	vector<RESERVE_DATA> setList;
	vector<RESERVE_DATA*> reserveList;

	SYSTEMTIME nowTime;
	GetLocalTime(&nowTime);
	LONGLONG now = GetNowI64Time();

	SYSTEMTIME baseTime = nowTime;
	baseTime.wHour = 0;
	baseTime.wMinute = 0;
	baseTime.wSecond = 0;
	baseTime.wMilliseconds = 0;

	LONGLONG baseStartTime = ConvertI64Time(baseTime);

	this->reserveManager.GetReserveDataAll(&reserveList);

	map<DWORD, MANUAL_AUTO_ADD_DATA*>::iterator itr;
	for( itr = this->manualAutoAdd.dataIDMap.begin(); itr != this->manualAutoAdd.dataIDMap.end(); itr++){
		BYTE weekChkFlag = (BYTE)(1<<nowTime.wDayOfWeek);
		for( BYTE i=0; i<8; i++ ){
			if( (itr->second->dayOfWeekFlag & weekChkFlag) != 0 ){
				LONGLONG startTime = baseStartTime + ((LONGLONG)itr->second->startTime) * I64_1SEC + (((LONGLONG)i) * 24*60*60*I64_1SEC);

				if( startTime > now ){
					//時間的に予約追加候補
					BOOL find = FALSE;
					for( size_t j=0; j<reserveList.size(); j++ ){
						//同一時間の予約がすでにあるかチェック
						if( reserveList[j]->eventID != 0xFFFF ){
							continue;
						}
						if( reserveList[j]->originalNetworkID != itr->second->originalNetworkID ||
							reserveList[j]->transportStreamID != itr->second->transportStreamID ||
							reserveList[j]->serviceID != itr->second->serviceID 
							){
							continue;
						}
						if( ConvertI64Time(reserveList[j]->startTime) == startTime &&
							reserveList[j]->durationSecond == itr->second->durationSecond
							){
							find = TRUE;
							break;
						}
					}
					if( find == FALSE ){
						//見つからなかったので予約追加
						RESERVE_DATA item;
						item.title = itr->second->title;
						ConvertSystemTime(startTime, &item.startTime); 
						item.startTimeEpg = item.startTime;
						item.durationSecond = itr->second->durationSecond;
						item.stationName = itr->second->stationName;
						item.originalNetworkID = itr->second->originalNetworkID;
						item.transportStreamID = itr->second->transportStreamID;
						item.serviceID = itr->second->serviceID;
						item.eventID = 0xFFFF;
						item.recSetting = itr->second->recSetting;

						setList.push_back(item);
					}
				}
			}

			weekChkFlag = weekChkFlag<<1;
			if( weekChkFlag == 0x80){
				weekChkFlag = 1;
			}
		}
	}

	if( setList.size() > 0 ){
		this->reserveManager.AddReserveData(&setList);
	}

	return ret;
}

int CALLBACK CEpgTimerSrvMain::CtrlCmdCallback(void* param, CMD_STREAM* cmdParam, CMD_STREAM* resParam)
{
	CEpgTimerSrvMain* sys = (CEpgTimerSrvMain*)param;

	resParam->dataSize = 0;
	resParam->param = CMD_ERR;


	switch( cmdParam->param ){
	case CMD2_EPG_SRV_ADDLOAD_RESERVE:
		if( sys->Lock() == TRUE ){
			if( sys->reserveManager.AddLoadReserveData() == TRUE ){
				resParam->param = CMD_SUCCESS;
			}
			sys->UnLock();
		}else{
			resParam->param = CMD_ERR_BUSY;
		}
		break;
	case CMD2_EPG_SRV_RELOAD_EPG:
		if( sys->epgDB.IsLoadingData() == TRUE ){
			resParam->param = CMD_ERR_BUSY;
		}else{
			if( sys->Lock() == TRUE ){
				if( sys->epgDB.ReloadEpgData() == TRUE ){
					sys->reloadEpgChkFlag = TRUE;
					resParam->param = CMD_SUCCESS;
				}
				sys->UnLock();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}
		}
		break;
	case CMD2_EPG_SRV_RELOAD_SETTING:
		{
			resParam->param = CMD_SUCCESS;
			sys->ReloadSetting();
			sys->reserveManager.ReloadSetting();
			sys->reserveManager.ReloadBankMap(TRUE);
		}
		break;
	case CMD2_EPG_SRV_CLOSE:
		{
			sys->StopMain();
			resParam->param = CMD_SUCCESS;
		}
		break;
	case CMD2_EPG_SRV_REGIST_GUI:
		{
			DWORD processID = 0;
			if( ReadVALUE( &processID, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				resParam->param = CMD_SUCCESS;
				sys->registGUI.insert(pair<DWORD,DWORD>(processID,processID));
				sys->reserveManager.SetRegistGUI(sys->registGUI);
			}
		}
		break;
	case CMD2_EPG_SRV_UNREGIST_GUI:
		{
			DWORD processID = 0;
			if( ReadVALUE( &processID, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				resParam->param = CMD_SUCCESS;
				map<DWORD,DWORD>::iterator itr;
				itr = sys->registGUI.find(processID);
				if( itr != sys->registGUI.end() ){
					sys->registGUI.erase(itr);
				}
				sys->reserveManager.SetRegistGUI(sys->registGUI);
			}
		}
		break;
	case CMD2_EPG_SRV_REGIST_GUI_TCP:
		{
			REGIST_TCP_INFO val;
			if( ReadVALUE( &val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				resParam->param = CMD_SUCCESS;
				wstring key = L"";
				Format(key, L"%s:%d", val.ip.c_str(), val.port);

				sys->registTCP.insert(pair<wstring,REGIST_TCP_INFO>(key,val));
				sys->reserveManager.SetRegistTCP(sys->registTCP);
			}
		}
		break;
	case CMD2_EPG_SRV_UNREGIST_GUI_TCP:
		{
			REGIST_TCP_INFO val;
			if( ReadVALUE( &val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				resParam->param = CMD_SUCCESS;
				wstring key = L"";
				Format(key, L"%s:%d", val.ip.c_str(), val.port);

				map<wstring,REGIST_TCP_INFO>::iterator itr;
				itr = sys->registTCP.find(key);
				if( itr != sys->registTCP.end() ){
					sys->registTCP.erase(itr);
				}
				sys->reserveManager.SetRegistTCP(sys->registTCP);
			}
		}
		break;

	case CMD2_EPG_SRV_ENUM_RESERVE:
		{
			OutputDebugString(L"CMD2_EPG_SRV_ENUM_RESERVE");
			if( sys->Lock() == TRUE ){
				vector<RESERVE_DATA*> list;
				if(sys->reserveManager.GetReserveDataAll(&list) == TRUE ){
					resParam->param = CMD_SUCCESS;
					resParam->dataSize = GetVALUESize(&list);
					resParam->data = new BYTE[resParam->dataSize];
					if( WriteVALUE(&list, resParam->data, resParam->dataSize, NULL) == FALSE ){
						_OutputDebugString(L"err Write res CMD2_EPG_SRV_ENUM_RESERVE\r\n");
						resParam->dataSize = 0;
						resParam->param = CMD_ERR;
					}
					for( size_t i=0; i<list.size(); i++ ){
						SAFE_DELETE(list[i]);
					}
					list.clear();
				}
				sys->UnLock();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}
		}
		break;
	case CMD2_EPG_SRV_GET_RESERVE:
		{
			OutputDebugString(L"CMD2_EPG_SRV_GET_RESERVE");
			if( sys->Lock() == TRUE ){
				DWORD reserveID = 0;
				if( ReadVALUE( &reserveID, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
					RESERVE_DATA info;
					if(sys->reserveManager.GetReserveData(reserveID, &info) == TRUE ){
						resParam->param = CMD_SUCCESS;
						resParam->dataSize = GetVALUESize(&info);
						resParam->data = new BYTE[resParam->dataSize];
						if( WriteVALUE(&info, resParam->data, resParam->dataSize, NULL) == FALSE ){
							_OutputDebugString(L"err Write res CMD2_EPG_SRV_GET_RESERVE\r\n");
							resParam->dataSize = 0;
							resParam->param = CMD_ERR;
						}
					}
				}
				sys->UnLock();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}
		}
		break;
	case CMD2_EPG_SRV_ADD_RESERVE:
		{
			if( sys->Lock() == TRUE ){
				vector<RESERVE_DATA> list;
				if( ReadVALUE( &list, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
					if(sys->reserveManager.AddReserveData(&list) == TRUE ){
						resParam->param = CMD_SUCCESS;
					}
				}
				sys->UnLock();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}
		}
		break;
	case CMD2_EPG_SRV_DEL_RESERVE:
		{
			if( sys->Lock() == TRUE ){
				vector<DWORD> list;
				if( ReadVALUE( &list, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
					if(sys->reserveManager.DelReserveData(&list) == TRUE ){
						resParam->param = CMD_SUCCESS;
					}
				}
				sys->UnLock();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}
		}
		break;
	case CMD2_EPG_SRV_CHG_RESERVE:
		{
			if( sys->Lock() == TRUE ){
				vector<RESERVE_DATA> list;
				if( ReadVALUE( &list, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
					if(sys->reserveManager.ChgReserveData(&list) == TRUE ){
						resParam->param = CMD_SUCCESS;
					}
				}
				sys->UnLock();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}
		}
		break;
	case CMD2_EPG_SRV_ENUM_RECINFO:
		{
			OutputDebugString(L"CMD2_EPG_SRV_ENUM_RECINFO");
			if( sys->Lock() == TRUE ){
				vector<REC_FILE_INFO> list;
				if(sys->reserveManager.GetRecFileInfoAll(&list) == TRUE ){
					resParam->param = CMD_SUCCESS;
					resParam->dataSize = GetVALUESize(&list);
					resParam->data = new BYTE[resParam->dataSize];
					if( WriteVALUE(&list, resParam->data, resParam->dataSize, NULL) == FALSE ){
						_OutputDebugString(L"err Write res CMD2_EPG_SRV_ENUM_RECINFO\r\n");
						resParam->dataSize = 0;
						resParam->param = CMD_ERR;
					}
				}
				sys->UnLock();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}
		}
		break;
	case CMD2_EPG_SRV_DEL_RECINFO:
		{
			if( sys->Lock() == TRUE ){
				vector<DWORD> list;
				if( ReadVALUE( &list, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
					if(sys->reserveManager.DelRecFileInfo(&list) == TRUE ){
						resParam->param = CMD_SUCCESS;
					}
				}
				sys->UnLock();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}
		}
		break;
	case CMD2_EPG_SRV_ENUM_SERVICE:
		{
			OutputDebugString(L"CMD2_EPG_SRV_ENUM_SERVICE");
			if( sys->epgDB.IsLoadingData() == TRUE ){
				resParam->param = CMD_ERR_BUSY;
			}else{
				if( sys->Lock() == TRUE ){
					vector<EPGDB_SERVICE_INFO> list;
					if( sys->epgDB.GetServiceList(&list) == TRUE ){
						resParam->param = CMD_SUCCESS;
						resParam->dataSize = GetVALUESize(&list);
						resParam->data = new BYTE[resParam->dataSize];
						if( WriteVALUE(&list, resParam->data, resParam->dataSize, NULL) == FALSE ){
							_OutputDebugString(L"err Write res CMD2_EPG_SRV_ENUM_SERVICE\r\n");
							resParam->dataSize = 0;
							resParam->param = CMD_ERR;
						}
					}
					sys->UnLock();
				}else{
					resParam->param = CMD_ERR_BUSY;
				}
			}
		}
		break;
	case CMD2_EPG_SRV_ENUM_PG_INFO:
		{
			OutputDebugString(L"CMD2_EPG_SRV_ENUM_PG_INFO");
			if( sys->epgDB.IsLoadingData() == TRUE ){
				resParam->param = CMD_ERR_BUSY;
			}else{
				if( sys->Lock() == TRUE ){
					resParam->param = CMD_ERR;
					vector<EPGDB_EVENT_INFO*> val;
					LONGLONG serviceKey = 0;

					if( ReadVALUE(&serviceKey, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
						if( sys->epgDB.EnumEventInfo(serviceKey, &val) == TRUE){
							resParam->param = CMD_SUCCESS;
							resParam->dataSize = GetVALUESize(&val);
							resParam->data = new BYTE[resParam->dataSize];
							if( WriteVALUE(&val, resParam->data, resParam->dataSize, NULL) == FALSE ){
								_OutputDebugString(L"err Write res CMD2_EPG_SRV_ENUM_PG_INFO\r\n");
								resParam->dataSize = 0;
								resParam->param = CMD_ERR;
							}
						}
					}
					sys->UnLock();
				}else{
					resParam->param = CMD_ERR_BUSY;
				}
			}
		}
		break;
	case CMD2_EPG_SRV_SEARCH_PG:
		{
			OutputDebugString(L"CMD2_EPG_SRV_SEARCH_PG");
			if( sys->epgDB.IsLoadingData() == TRUE ){
				resParam->param = CMD_ERR_BUSY;
			}else{
				if( sys->Lock() == TRUE ){
					vector<EPGDB_SEARCH_KEY_INFO> key;
					vector<EPGDB_EVENT_INFO*> val;

					if( ReadVALUE( &key, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
						if( sys->epgDB.SearchEpg(&key, &val) == TRUE ){
							resParam->param = CMD_SUCCESS;
							resParam->dataSize = GetVALUESize(&val);
							resParam->data = new BYTE[resParam->dataSize];
							if( WriteVALUE(&val, resParam->data, resParam->dataSize, NULL) == FALSE ){
								_OutputDebugString(L"err Write res CMD2_EPG_SRV_SEARCH_PG\r\n");
								resParam->dataSize = 0;
								resParam->param = CMD_ERR;
							}
						}
					}
					sys->UnLock();
				}else{
					resParam->param = CMD_ERR_BUSY;
				}
			}
		}
		break;
	case CMD2_EPG_SRV_GET_PG_INFO:
		{
			OutputDebugString(L"CMD2_EPG_SRV_GET_PG_INFO");
			if( sys->epgDB.IsLoadingData() == TRUE ){
				resParam->param = CMD_ERR_BUSY;
			}else{
				if( sys->Lock() == TRUE ){
					ULONGLONG key;
					EPGDB_EVENT_INFO* val;

					if( ReadVALUE( &key, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
						WORD ONID = (WORD)(key>>48);
						WORD TSID = (WORD)((key&0x0000FFFF00000000)>>32);
						WORD SID = (WORD)((key&0x00000000FFFF0000)>>16);
						WORD eventID = (WORD)(key&0x000000000000FFFF);
						if( sys->epgDB.SearchEpg(ONID, TSID, SID, eventID, &val) == TRUE ){
							resParam->param = CMD_SUCCESS;
							resParam->dataSize = GetVALUESize(val);
							resParam->data = new BYTE[resParam->dataSize];
							if( WriteVALUE(val, resParam->data, resParam->dataSize, NULL) == FALSE ){
								_OutputDebugString(L"err Write res CMD2_EPG_SRV_GET_PG_INFO\r\n");
								resParam->dataSize = 0;
								resParam->param = CMD_ERR;
							}
						}
					}
					sys->UnLock();
				}else{
					resParam->param = CMD_ERR_BUSY;
				}
			}
		}
		break;
	case CMD2_EPG_SRV_CHK_SUSPEND:
		{
			BOOL streamingChk = TRUE;
			if( sys->ngFileStreaming == TRUE ){
				if( sys->streamingManager.IsStreaming() == TRUE ){
					streamingChk = FALSE;
				}
			}
			if( sys->reserveManager.IsSuspendOK() == TRUE && streamingChk == TRUE){
				resParam->param = CMD_SUCCESS;
			}
		}
		break;
	case CMD2_EPG_SRV_SUSPEND:
		{
			WORD val = 0;
			if( ReadVALUE( &val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				BYTE reboot = val>>8;
				if( reboot == 0xFF ){
					reboot = sys->rebootDef;
				}
				BYTE suspendMode = val&0x00FF;

				BOOL streamingChk = TRUE;
				if( sys->ngFileStreaming == TRUE ){
					if( sys->streamingManager.IsStreaming() == TRUE ){
						streamingChk = FALSE;
					}
				}
				if( sys->reserveManager.IsSuspendOK() == TRUE && streamingChk == TRUE){
					if( sys->Lock() == TRUE ){
						sys->StartSleep(reboot, suspendMode);
						sys->UnLock();
					}
					resParam->param = CMD_SUCCESS;
				}
			}
		}
		break;
	case CMD2_EPG_SRV_REBOOT:
		{
			resParam->param = CMD_SUCCESS;
			sys->StartSleep(1, 0xFF);
		}
		break;
	case CMD2_EPG_SRV_EPG_CAP_NOW:
		{
			if( sys->epgDB.IsLoadingData() == TRUE ){
				resParam->param = CMD_ERR_BUSY;
			}else{
				if( sys->reserveManager.StartEpgCap() == TRUE ){
					resParam->param = CMD_SUCCESS;
				}
			}
		}
		break;
	case CMD2_EPG_SRV_ENUM_AUTO_ADD:
		{
			OutputDebugString(L"CMD2_EPG_SRV_ENUM_AUTO_ADD");
			if( sys->Lock() == TRUE ){
				vector<EPG_AUTO_ADD_DATA> val;
				map<DWORD, EPG_AUTO_ADD_DATA*>::iterator itr;
				for( itr = sys->epgAutoAdd.dataIDMap.begin(); itr != sys->epgAutoAdd.dataIDMap.end(); itr++ ){
					val.push_back(*(itr->second));
				}
				if( val.size() > 0 ){
					resParam->param = CMD_SUCCESS;
					resParam->dataSize = GetVALUESize(&val);
					resParam->data = new BYTE[resParam->dataSize];
					if( WriteVALUE(&val, resParam->data, resParam->dataSize, NULL) == FALSE ){
						_OutputDebugString(L"err Write res CMD2_EPG_SRV_ENUM_AUTO_ADD\r\n");
						resParam->dataSize = 0;
						resParam->param = CMD_ERR;
					}
				}

				sys->UnLock();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}
		}
		break;
	case CMD2_EPG_SRV_ADD_AUTO_ADD:
		{
			if( sys->Lock() == TRUE ){
				vector<EPG_AUTO_ADD_DATA> val;
				if( ReadVALUE( &val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
					for( size_t i=0; i<val.size(); i++ ){
						sys->epgAutoAdd.AddData(&val[i]);
					}

					wstring savePath = L"";
					GetSettingPath(savePath);
					savePath += L"\\";
					savePath += EPG_AUTO_ADD_TEXT_NAME;

					sys->epgAutoAdd.SaveText(savePath.c_str());

					resParam->param = CMD_SUCCESS;

					sys->AutoAddReserveEPG();
				}

				sys->UnLock();

				sys->reserveManager.SendNotifyUpdate();

			}else{
				resParam->param = CMD_ERR_BUSY;
			}

		}
		break;
	case CMD2_EPG_SRV_DEL_AUTO_ADD:
		{
			if( sys->Lock() == TRUE ){
				vector<DWORD> val;
				if( ReadVALUE( &val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
					for( size_t i=0; i<val.size(); i++ ){
						sys->epgAutoAdd.DelData(val[i]);
					}

					wstring savePath = L"";
					GetSettingPath(savePath);
					savePath += L"\\";
					savePath += EPG_AUTO_ADD_TEXT_NAME;

					sys->epgAutoAdd.SaveText(savePath.c_str());

					resParam->param = CMD_SUCCESS;
				}

				sys->UnLock();

				sys->reserveManager.SendNotifyUpdate();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}

		}
		break;
	case CMD2_EPG_SRV_CHG_AUTO_ADD:
		{
			if( sys->Lock() == TRUE ){
				vector<EPG_AUTO_ADD_DATA> val;
				if( ReadVALUE( &val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
					for( size_t i=0; i<val.size(); i++ ){
						sys->epgAutoAdd.ChgData(&val[i]);
					}

					wstring savePath = L"";
					GetSettingPath(savePath);
					savePath += L"\\";
					savePath += EPG_AUTO_ADD_TEXT_NAME;

					sys->epgAutoAdd.SaveText(savePath.c_str());

					resParam->param = CMD_SUCCESS;

					sys->AutoAddReserveEPG();
				}

				sys->UnLock();

				sys->reserveManager.SendNotifyUpdate();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}

		}
		break;
	case CMD2_EPG_SRV_ENUM_MANU_ADD:
		{
			OutputDebugString(L"CMD2_EPG_SRV_ENUM_MANU_ADD");
			if( sys->Lock() == TRUE ){
				vector<MANUAL_AUTO_ADD_DATA> val;
				map<DWORD, MANUAL_AUTO_ADD_DATA*>::iterator itr;
				for( itr = sys->manualAutoAdd.dataIDMap.begin(); itr != sys->manualAutoAdd.dataIDMap.end(); itr++ ){
					val.push_back(*(itr->second));
				}
				if( val.size() > 0 ){
					resParam->param = CMD_SUCCESS;
					resParam->dataSize = GetVALUESize(&val);
					resParam->data = new BYTE[resParam->dataSize];
					if( WriteVALUE(&val, resParam->data, resParam->dataSize, NULL) == FALSE ){
						_OutputDebugString(L"err Write res CMD2_EPG_SRV_ENUM_MANU_ADD\r\n");
						resParam->dataSize = 0;
						resParam->param = CMD_ERR;
					}
				}

				sys->UnLock();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}

		}
		break;
	case CMD2_EPG_SRV_ADD_MANU_ADD:
		{
			if( sys->Lock() == TRUE ){
				vector<MANUAL_AUTO_ADD_DATA> val;
				if( ReadVALUE( &val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
					for( size_t i=0; i<val.size(); i++ ){
						sys->manualAutoAdd.AddData(&val[i]);
					}

					wstring savePath = L"";
					GetSettingPath(savePath);
					savePath += L"\\";
					savePath += MANUAL_AUTO_ADD_TEXT_NAME;

					sys->manualAutoAdd.SaveText(savePath.c_str());

					resParam->param = CMD_SUCCESS;

					sys->AutoAddReserveProgram();
				}

				sys->UnLock();

				sys->reserveManager.SendNotifyUpdate();

			}else{
				resParam->param = CMD_ERR_BUSY;
			}

		}
		break;
	case CMD2_EPG_SRV_DEL_MANU_ADD:
		{
			if( sys->Lock() == TRUE ){
				vector<DWORD> val;
				if( ReadVALUE( &val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
					for( size_t i=0; i<val.size(); i++ ){
						sys->manualAutoAdd.DelData(val[i]);
					}

					wstring savePath = L"";
					GetSettingPath(savePath);
					savePath += L"\\";
					savePath += MANUAL_AUTO_ADD_TEXT_NAME;

					sys->manualAutoAdd.SaveText(savePath.c_str());

					resParam->param = CMD_SUCCESS;
				}

				sys->UnLock();

				sys->reserveManager.SendNotifyUpdate();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}

		}
		break;
	case CMD2_EPG_SRV_CHG_MANU_ADD:
		{
			if( sys->Lock() == TRUE ){
				vector<MANUAL_AUTO_ADD_DATA> val;
				if( ReadVALUE( &val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
					for( size_t i=0; i<val.size(); i++ ){
						sys->manualAutoAdd.ChgData(&val[i]);
					}

					wstring savePath = L"";
					GetSettingPath(savePath);
					savePath += L"\\";
					savePath += MANUAL_AUTO_ADD_TEXT_NAME;

					sys->manualAutoAdd.SaveText(savePath.c_str());

					resParam->param = CMD_SUCCESS;

					sys->AutoAddReserveProgram();
				}

				sys->UnLock();

				sys->reserveManager.SendNotifyUpdate();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}

		}
		break;
	case CMD2_EPG_SRV_ENUM_TUNER_RESERVE:
		{
			OutputDebugString(L"CMD2_EPG_SRV_ENUM_TUNER_RESERVE");
			vector<TUNER_RESERVE_INFO> list;
			if( sys->Lock() == TRUE ){
				if(sys->reserveManager.GetTunerReserveAll(&list) == TRUE ){
					resParam->param = CMD_SUCCESS;
					resParam->dataSize = GetVALUESize(&list);
					resParam->data = new BYTE[resParam->dataSize];
					if( WriteVALUE(&list, resParam->data, resParam->dataSize, NULL) == FALSE ){
						_OutputDebugString(L"err Write res CMD2_EPG_SRV_ENUM_TUNER_RESERVE\r\n");
						resParam->dataSize = 0;
						resParam->param = CMD_ERR;
					}
				}
				sys->UnLock();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}
		}
		break;
	case CMD2_EPG_SRV_FILE_COPY:
		{
			wstring val;
			if( ReadVALUE( &val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				if( CompareNoCase(val, L"ChSet5.txt") == 0){
					wstring path = L"";
					GetSettingPath(path);
					path += L"\\ChSet5.txt";

					HANDLE file = _CreateFile(path.c_str(), GENERIC_READ, FILE_SHARE_READ|FILE_SHARE_WRITE, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL );
					if( file != INVALID_HANDLE_VALUE){
						DWORD dwFileSize = GetFileSize( file, NULL );
						if( dwFileSize > 0 ){
							resParam->dataSize = dwFileSize;
							resParam->data = new BYTE[resParam->dataSize];

							DWORD dwRead=0;
							ReadFile( file, resParam->data, resParam->dataSize, &dwRead, NULL );
						}
						CloseHandle(file);
						resParam->param = CMD_SUCCESS;
					}
				}
			}
		}
		break;
	case CMD2_EPG_SRV_ENUM_PG_ALL:
		{
			OutputDebugString(L"CMD2_EPG_SRV_ENUM_PG_ALL");
			if( sys->epgDB.IsLoadingData() == TRUE ){
				resParam->param = CMD_ERR_BUSY;
			}else{
				if( sys->Lock() == TRUE ){
					resParam->param = CMD_ERR;
					vector<EPGDB_SERVICE_EVENT_INFO*> val;

					if( sys->epgDB.EnumEventAll(&val) == TRUE){
						resParam->param = CMD_SUCCESS;
						resParam->dataSize = GetVALUESize(&val);
						resParam->data = new BYTE[resParam->dataSize];
						if( WriteVALUE(&val, resParam->data, resParam->dataSize, NULL) == FALSE ){
							_OutputDebugString(L"err Write res CMD2_EPG_SRV_ENUM_PG_ALL\r\n");
							resParam->dataSize = 0;
							resParam->param = CMD_ERR;
						}
						for( size_t i=0;i<val.size(); i++ ){
							SAFE_DELETE(val[i]);
						}
					}
					sys->UnLock();
				}else{
					resParam->param = CMD_ERR_BUSY;
				}
			}
		}
		break;
	case CMD2_EPG_SRV_ENUM_PLUGIN:
		{
			OutputDebugString(L"CMD2_EPG_SRV_ENUM_PLUGIN");
			WORD mode = 0;
			if( ReadVALUE( &mode, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				if( mode == 1 || mode == 2 ){
					wstring path = L"";
					GetModuleFolderPath(path);

					wstring searchKey = path;

					if( mode == 1 ){
						searchKey += L"\\RecName\\RecName*.dll";
					}else if( mode == 2 ){
						searchKey += L"\\Write\\Write*.dll";
					}

					WIN32_FIND_DATA findData;
					HANDLE find;

					//指定フォルダのファイル一覧取得
					find = FindFirstFile( searchKey.c_str(), &findData);
					if ( find != INVALID_HANDLE_VALUE ) {
						vector<wstring> fileList;
						do{
							if( (findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) == 0 ){
								//本当に拡張子DLL?
								if( IsExt(findData.cFileName, L".dll") == TRUE ){
									fileList.push_back(findData.cFileName);
								}
							}
						}while(FindNextFile(find, &findData));

						FindClose(find);

						resParam->param = CMD_SUCCESS;
						resParam->dataSize = GetVALUESize(&fileList);
						resParam->data = new BYTE[resParam->dataSize];
						if( WriteVALUE(&fileList, resParam->data, resParam->dataSize, NULL) == FALSE ){
							_OutputDebugString(L"err Write res CMD2_EPG_SRV_ENUM_PLUGIN\r\n");
							resParam->dataSize = 0;
							resParam->param = CMD_ERR;
						}
					}

				}
			}
		}
		break;
	case CMD2_EPG_SRV_GET_CHG_CH_TVTEST:
		{
			LONGLONG key = 0;

			if( ReadVALUE(&key, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				TVTEST_CH_CHG_INFO chInfo;
				if( sys->reserveManager.GetTVTestChgCh(key, &chInfo) == TRUE ){
					resParam->param = CMD_SUCCESS;
					resParam->dataSize = GetVALUESize(&chInfo);
					resParam->data = new BYTE[resParam->dataSize];
					if( WriteVALUE(&chInfo, resParam->data, resParam->dataSize, NULL) == FALSE ){
						_OutputDebugString(L"err Write res CMD2_EPG_SRV_GET_CHG_CH_TVTEST\r\n");
						resParam->dataSize = 0;
						resParam->param = CMD_ERR;
					}
				}
			}
		}
		break;
	case CMD2_EPG_SRV_NWTV_SET_CH:
		{
			SET_CH_INFO val;
			if( ReadVALUE(&val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				if( sys->reserveManager.SetNWTVCh(&val) == TRUE ){
					resParam->param = CMD_SUCCESS;
				}
			}
		}
		break;
	case CMD2_EPG_SRV_NWTV_CLOSE:
		{
			if( sys->reserveManager.CloseNWTV() == TRUE ){
				resParam->param = CMD_SUCCESS;
			}
		}
		break;
	case CMD2_EPG_SRV_NWTV_MODE:
		{
			DWORD val;
			if( ReadVALUE(&val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				sys->reserveManager.SetNWTVMode(val);
				resParam->param = CMD_SUCCESS;
			}
		}
		break;
	case CMD2_EPG_SRV_NWPLAY_OPEN:
		{
			wstring val;
			if( ReadVALUE(&val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				DWORD id=0;
				if( sys->streamingManager.OpenFile(val.c_str(), &id) == TRUE ){
					resParam->param = CMD_SUCCESS;
					resParam->dataSize = GetVALUESize(id);
					resParam->data = new BYTE[resParam->dataSize];
					if( WriteVALUE(id, resParam->data, resParam->dataSize, NULL) == FALSE ){
						_OutputDebugString(L"err Write res CMD2_EPG_SRV_NWPLAY_OPEN\r\n");
						resParam->dataSize = 0;
						resParam->param = CMD_ERR;
					}
				}
			}
		}
		break;
	case CMD2_EPG_SRV_NWPLAY_CLOSE:
		{
			DWORD val;
			if( ReadVALUE(&val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				if( sys->streamingManager.CloseFile(val) == TRUE ){
					resParam->param = CMD_SUCCESS;
				}
			}
		}
		break;
	case CMD2_EPG_SRV_NWPLAY_PLAY:
		{
			DWORD val;
			if( ReadVALUE(&val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				if( sys->streamingManager.StartSend(val) == TRUE ){
					resParam->param = CMD_SUCCESS;
				}
			}
		}
		break;
	case CMD2_EPG_SRV_NWPLAY_STOP:
		{
			DWORD val;
			if( ReadVALUE(&val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				if( sys->streamingManager.StopSend(val) == TRUE ){
					resParam->param = CMD_SUCCESS;
				}
			}
		}
		break;
	case CMD2_EPG_SRV_NWPLAY_GET_POS:
		{
			NWPLAY_POS_CMD val;
			if( ReadVALUE(&val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				if( sys->streamingManager.GetPos(&val) == TRUE ){
					resParam->param = CMD_SUCCESS;
					resParam->dataSize = GetVALUESize(&val);
					resParam->data = new BYTE[resParam->dataSize];
					if( WriteVALUE(&val, resParam->data, resParam->dataSize, NULL) == FALSE ){
						_OutputDebugString(L"err Write res CMD2_EPG_SRV_NWPLAY_GET_POS\r\n");
						resParam->dataSize = 0;
						resParam->param = CMD_ERR;
					}
				}
			}
		}
		break;
	case CMD2_EPG_SRV_NWPLAY_SET_POS:
		{
			NWPLAY_POS_CMD val;
			if( ReadVALUE(&val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				if( sys->streamingManager.SetPos(&val) == TRUE ){
					resParam->param = CMD_SUCCESS;
				}
			}
		}
		break;
	case CMD2_EPG_SRV_NWPLAY_SET_IP:
		{
			NWPLAY_PLAY_INFO val;
			if( ReadVALUE(&val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				if( sys->streamingManager.SetIP(&val) == TRUE ){
					resParam->param = CMD_SUCCESS;
					resParam->dataSize = GetVALUESize(&val);
					resParam->data = new BYTE[resParam->dataSize];
					if( WriteVALUE(&val, resParam->data, resParam->dataSize, NULL) == FALSE ){
						_OutputDebugString(L"err Write res CMD2_EPG_SRV_NWPLAY_SET_IP\r\n");
						resParam->dataSize = 0;
						resParam->param = CMD_ERR;
					}
				}
			}
		}
		break;
	case CMD2_EPG_SRV_NWPLAY_TF_OPEN:
		{
			DWORD val;
			if( ReadVALUE(&val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				NWPLAY_TIMESHIFT_INFO resVal;
				DWORD ctrlID = 0;
				DWORD processID = 0;
				if( sys->reserveManager.GetRecFilePath(val, resVal.filePath, &ctrlID, &processID) == TRUE ){
					if( sys->streamingManager.OpenTimeShift(resVal.filePath.c_str(), processID, ctrlID, &resVal.ctrlID) == TRUE ){
						resParam->param = CMD_SUCCESS;
						resParam->dataSize = GetVALUESize(&resVal);
						resParam->data = new BYTE[resParam->dataSize];
						if( WriteVALUE(&resVal, resParam->data, resParam->dataSize, NULL) == FALSE ){
							_OutputDebugString(L"err Write res CMD2_EPG_SRV_NWPLAY_TF_OPEN\r\n");
							resParam->dataSize = 0;
							resParam->param = CMD_ERR;
						}
					}
				}
			}
		}
		break;
	////////////////////////////////////////////////////////////
	//CMD_VER対応コマンド
	case CMD2_EPG_SRV_ENUM_RESERVE2:
		{
			OutputDebugString(L"CMD2_EPG_SRV_ENUM_RESERVE2");
			if( sys->Lock() == TRUE ){
				vector<RESERVE_DATA*> list;
				if(sys->reserveManager.GetReserveDataAll(&list) == TRUE ){
					WORD ver = (WORD)CMD_VER;

					if( ReadVALUE2(ver, &ver, cmdParam->data, cmdParam->dataSize, NULL) == TRUE ){
						DWORD writeSize = 0;
						resParam->param = CMD_SUCCESS;
						resParam->dataSize = GetVALUESize2(ver, &list)+GetVALUESize2(ver, ver);
						resParam->data = new BYTE[resParam->dataSize];
						if( WriteVALUE2(ver, ver, resParam->data, resParam->dataSize, &writeSize) == FALSE ){
							_OutputDebugString(L"err Write res CMD2_EPG_SRV_ENUM_RESERVE2\r\n");
							resParam->dataSize = 0;
							resParam->param = CMD_ERR;
						}else
						if( WriteVALUE2(ver, &list, resParam->data+writeSize, resParam->dataSize-writeSize, NULL) == FALSE ){
							_OutputDebugString(L"err Write res CMD2_EPG_SRV_ENUM_RESERVE2\r\n");
							resParam->dataSize = 0;
							resParam->param = CMD_ERR;
						}
						for( size_t i=0; i<list.size(); i++ ){
							SAFE_DELETE(list[i]);
						}
						list.clear();
					}
				}
				sys->UnLock();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}
		}
		break;
	case CMD2_EPG_SRV_GET_RESERVE2:
		{
			OutputDebugString(L"CMD2_EPG_SRV_GET_RESERVE2");
			if( sys->Lock() == TRUE ){
				WORD ver = (WORD)CMD_VER;
				DWORD readSize = 0;
				if( ReadVALUE2(ver, &ver, cmdParam->data, cmdParam->dataSize, &readSize) == TRUE ){

					DWORD reserveID = 0;
					if( ReadVALUE2(ver, &reserveID, cmdParam->data+readSize, cmdParam->dataSize-readSize, NULL ) == TRUE ){
						RESERVE_DATA info;
						if(sys->reserveManager.GetReserveData(reserveID, &info) == TRUE ){
							DWORD writeSize = 0;
							resParam->param = CMD_SUCCESS;
							resParam->dataSize = GetVALUESize2(ver, &info)+GetVALUESize2(ver, ver);
							resParam->data = new BYTE[resParam->dataSize];
							if( WriteVALUE2(ver, ver, resParam->data, resParam->dataSize, &writeSize) == FALSE ){
								_OutputDebugString(L"err Write res CMD2_EPG_SRV_GET_RESERVE2\r\n");
								resParam->dataSize = 0;
								resParam->param = CMD_ERR;
							}else
							if( WriteVALUE2(ver, &info, resParam->data+writeSize, resParam->dataSize-writeSize, NULL) == FALSE ){
								_OutputDebugString(L"err Write res CMD2_EPG_SRV_GET_RESERVE2\r\n");
								resParam->dataSize = 0;
								resParam->param = CMD_ERR;
							}
						}
					}
				}
				sys->UnLock();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}
		}
		break;
	case CMD2_EPG_SRV_ADD_RESERVE2:
		{
			OutputDebugString(L"CMD2_EPG_SRV_ADD_RESERVE2");
			if( sys->Lock() == TRUE ){
				WORD ver = (WORD)CMD_VER;
				DWORD readSize = 0;
				if( ReadVALUE2(ver, &ver, cmdParam->data, cmdParam->dataSize, &readSize) == TRUE ){

					vector<RESERVE_DATA> list;
					if( ReadVALUE2(ver, &list, cmdParam->data+readSize, cmdParam->dataSize-readSize, NULL ) == TRUE ){
						if(sys->reserveManager.AddReserveData(&list) == TRUE ){
							DWORD writeSize = 0;
							resParam->param = CMD_SUCCESS;
							resParam->dataSize = GetVALUESize2(ver, ver);
							resParam->data = new BYTE[resParam->dataSize];
							if( WriteVALUE2(ver, ver, resParam->data, resParam->dataSize, &writeSize) == FALSE ){
								_OutputDebugString(L"err Write res CMD2_EPG_SRV_ADD_RESERVE2\r\n");
								resParam->dataSize = 0;
								resParam->param = CMD_ERR;
							}
						}
					}
				}
				sys->UnLock();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}
		}
		break;
	case CMD2_EPG_SRV_CHG_RESERVE2:
		{
			OutputDebugString(L"CMD2_EPG_SRV_CHG_RESERVE2");
			if( sys->Lock() == TRUE ){
				WORD ver = (WORD)CMD_VER;
				DWORD readSize = 0;
				if( ReadVALUE2(ver, &ver, cmdParam->data, cmdParam->dataSize, &readSize) == TRUE ){
					vector<RESERVE_DATA> list;
					if( ReadVALUE2(ver, &list, cmdParam->data+readSize, cmdParam->dataSize-readSize, NULL ) == TRUE ){
						if(sys->reserveManager.ChgReserveData(&list) == TRUE ){
							DWORD writeSize = 0;
							resParam->param = CMD_SUCCESS;
							resParam->dataSize = GetVALUESize2(ver, ver);
							resParam->data = new BYTE[resParam->dataSize];
							if( WriteVALUE2(ver, ver, resParam->data, resParam->dataSize, &writeSize) == FALSE ){
								_OutputDebugString(L"err Write res CMD2_EPG_SRV_CHG_RESERVE2\r\n");
								resParam->dataSize = 0;
								resParam->param = CMD_ERR;
							}
						}
					}
				}
				sys->UnLock();
			}else{
				resParam->param = CMD_ERR_BUSY;
			}
		}
		break;
	////////////////////////////////////////////////////////////
	//旧バージョン互換コマンド
	case CMD_EPG_SRV_GET_RESERVE_INFO:
		{
			resParam->param = OLD_CMD_ERR;
			if( sys->Lock() == TRUE ){
				DWORD reserveID = 0;
				if( ReadVALUE(&reserveID, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
					RESERVE_DATA info;
					if(sys->reserveManager.GetReserveData(reserveID, &info) == TRUE ){
						OLD_RESERVE_DATA oldInfo;
						oldInfo = info;
						CreateReserveDataStream(&oldInfo, resParam);
						resParam->param = OLD_CMD_SUCCESS;
					}
				}
				sys->UnLock();
			}
		}
		break;
	case CMD_EPG_SRV_ADD_RESERVE:
		{
			resParam->param = OLD_CMD_ERR;
			if( sys->Lock() == TRUE ){
				OLD_RESERVE_DATA oldItem;
				if( CopyReserveData(&oldItem, cmdParam) == TRUE){
					RESERVE_DATA item;
					CopyOldNew(&oldItem, &item);

					vector<RESERVE_DATA> list;
					list.push_back(item);
					if(sys->reserveManager.AddReserveData(&list) == TRUE ){
						resParam->param = OLD_CMD_SUCCESS;
					}
				}
				sys->UnLock();
			}
		}
		break;
	case CMD_EPG_SRV_DEL_RESERVE:
		{
			resParam->param = OLD_CMD_ERR;
			if( sys->Lock() == TRUE ){
				OLD_RESERVE_DATA oldItem;
				if( CopyReserveData(&oldItem, cmdParam) == TRUE){
					vector<DWORD> list;
					list.push_back(oldItem.dwReserveID);
					if(sys->reserveManager.DelReserveData(&list) == TRUE ){
						resParam->param = OLD_CMD_SUCCESS;
					}
				}
				sys->UnLock();
			}
		}
		break;
	case CMD_EPG_SRV_CHG_RESERVE:
		{
			resParam->param = OLD_CMD_ERR;
			if( sys->Lock() == TRUE ){
				OLD_RESERVE_DATA oldItem;
				if( CopyReserveData(&oldItem, cmdParam) == TRUE){
					RESERVE_DATA item;
					CopyOldNew(&oldItem, &item);

					vector<RESERVE_DATA> list;
					list.push_back(item);
					if(sys->reserveManager.ChgReserveData(&list) == TRUE ){
						resParam->param = OLD_CMD_SUCCESS;
					}
				}
				sys->UnLock();
			}
		}

		break;
	case CMD_EPG_SRV_ADD_AUTO_ADD:
		{
			resParam->param = OLD_CMD_ERR;
			if( sys->Lock() == TRUE ){
				OLD_SEARCH_KEY oldItem;
				if( CopySearchKeyData(&oldItem, cmdParam) == TRUE){
					EPG_AUTO_ADD_DATA item;
					CopyOldNew(&oldItem, &item);

					if( sys->Lock() == TRUE ){
						sys->epgAutoAdd.AddData(&item);

						wstring savePath = L"";
						GetSettingPath(savePath);
						savePath += L"\\";
						savePath += EPG_AUTO_ADD_TEXT_NAME;

						sys->epgAutoAdd.SaveText(savePath.c_str());

						resParam->param = OLD_CMD_SUCCESS;

						sys->AutoAddReserveEPG();
						sys->UnLock();
						sys->reserveManager.SendNotifyUpdate();
					}
				}
				sys->UnLock();
			}
		}
		break;
	case CMD_EPG_SRV_DEL_AUTO_ADD:
		{
			resParam->param = OLD_CMD_ERR;
			if( sys->Lock() == TRUE ){
				OLD_SEARCH_KEY oldItem;
				if( CopySearchKeyData(&oldItem, cmdParam) == TRUE){
					if( sys->Lock() == TRUE ){
						sys->epgAutoAdd.DelData((DWORD)oldItem.iAutoAddID);

						wstring savePath = L"";
						GetSettingPath(savePath);
						savePath += L"\\";
						savePath += EPG_AUTO_ADD_TEXT_NAME;

						sys->epgAutoAdd.SaveText(savePath.c_str());

						resParam->param = OLD_CMD_SUCCESS;
						sys->UnLock();
						sys->reserveManager.SendNotifyUpdate();
					}
				}
				sys->UnLock();
			}
		}
		break;
	case CMD_EPG_SRV_CHG_AUTO_ADD:
		{
			resParam->param = OLD_CMD_ERR;
			if( sys->Lock() == TRUE ){
				OLD_SEARCH_KEY oldItem;
				if( CopySearchKeyData(&oldItem, cmdParam) == TRUE){
					EPG_AUTO_ADD_DATA item;
					CopyOldNew(&oldItem, &item);

					if( sys->Lock() == TRUE ){
						sys->epgAutoAdd.ChgData(&item);

						wstring savePath = L"";
						GetSettingPath(savePath);
						savePath += L"\\";
						savePath += EPG_AUTO_ADD_TEXT_NAME;

						sys->epgAutoAdd.SaveText(savePath.c_str());

						resParam->param = OLD_CMD_SUCCESS;

						sys->AutoAddReserveEPG();
						sys->UnLock();
						sys->reserveManager.SendNotifyUpdate();
					}
				}
				sys->UnLock();
			}
		}
		break;
	case CMD_EPG_SRV_SEARCH_PG_FIRST:
		{
			sys->oldSearchList.clear();
			resParam->param = OLD_CMD_ERR;
			if( sys->epgDB.IsLoadingData() == TRUE ){
				resParam->param = CMD_ERR_BUSY;
			}else{
				if( sys->Lock() == TRUE ){
					OLD_SEARCH_KEY oldItem;
					if( CopySearchKeyData(&oldItem, cmdParam) == TRUE){
						EPGDB_SEARCH_KEY_INFO item;
						CopyOldNew(&oldItem, &item);

						vector<EPGDB_SEARCH_KEY_INFO> key;
						vector<EPGDB_EVENT_INFO*> val;
						key.push_back(item);
						if( sys->epgDB.SearchEpg(&key, &val) == TRUE ){
							for( size_t i=0; i<val.size(); i++ ){
								OLD_EVENT_INFO_DATA3 add;
								add = *val[i];
								sys->oldSearchList.push_back(add);
							}
							if( sys->oldSearchList.size() == 0 ){
								resParam->param = OLD_CMD_ERR;
							}else{
								if( sys->oldSearchList.size() == 1 ){
									resParam->param = OLD_CMD_SUCCESS;
								}else{
									resParam->param = OLD_CMD_NEXT;
								}
								CreateEventInfoData3Stream(&sys->oldSearchList[0], resParam);
								sys->oldSearchList.erase(sys->oldSearchList.begin());
								vector<OLD_EVENT_INFO_DATA3>(sys->oldSearchList).swap(sys->oldSearchList);
							}
						}
					}
					sys->UnLock();
				}
			}
		}
		break;
	case CMD_EPG_SRV_SEARCH_PG_NEXT:
		{
			resParam->param = OLD_CMD_ERR;
			if( sys->oldSearchList.size() == 0 ){
				resParam->param = OLD_CMD_ERR;
			}else{
				if( sys->Lock() == TRUE ){
					if( sys->oldSearchList.size() == 1 ){
						resParam->param = OLD_CMD_SUCCESS;
					}else{
						resParam->param = OLD_CMD_NEXT;
					}
					CreateEventInfoData3Stream(&sys->oldSearchList[0], resParam);
					sys->oldSearchList.erase(sys->oldSearchList.begin());
					sys->UnLock();
				}
			}
		}
		break;
	default:
		_OutputDebugString(L"err default cmd %d\r\n", cmdParam->param);
		resParam->param = CMD_NON_SUPPORT;
		break;
	}

	return 0;
}