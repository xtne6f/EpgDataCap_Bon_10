// EpgTimerPlugIn.cpp : DLL アプリケーション用にエクスポートされる関数を定義します。
//

#include "stdafx.h"
#include "EpgTimerPlugIn.h"

#include "../../Common/StringUtil.h"
#include "../../Common/PathUtil.h"

// プラグインクラスのインスタンスを生成する
TVTest::CTVTestPlugin *CreatePluginClass()
{
	return new CEpgTimerPlugIn;
}


CEpgTimerPlugIn::CEpgTimerPlugIn()
{
}


// プラグインの情報を返す
bool CEpgTimerPlugIn::GetPluginInfo(TVTest::PluginInfo *pInfo)
{
	pInfo->Type           = TVTest::PLUGIN_TYPE_NORMAL;
	pInfo->Flags          = 0;
	pInfo->pszPluginName  = L"EpgTimer PlugIn";
	pInfo->pszCopyright   = L"りょうちん Copyright (C) 2010";
	pInfo->pszDescription = L"EpgTimerSrvからの制御用";
	return true;
}


// 初期化処理
bool CEpgTimerPlugIn::Initialize()
{
	// イベントコールバック関数を登録
	m_pApp->SetEventCallback(EventCallback, this);

	return true;
}

void CEpgTimerPlugIn::InitializePlugin()
{
	wstring pipeName = L"";
	wstring eventName = L"";

	Format(pipeName, L"%s%d", CMD2_TVTEST_CTRL_PIPE, GetCurrentProcessId());
	Format(eventName, L"%s%d", CMD2_TVTEST_CTRL_WAIT_CONNECT, GetCurrentProcessId());

	OutputDebugString(pipeName.c_str());
	OutputDebugString(eventName.c_str());
	this->pipeServer.StartServer(eventName.c_str(), pipeName.c_str(), CtrlCmdCallback, this, 0, GetCurrentProcessId());

	return ;
}

// 終了処理
bool CEpgTimerPlugIn::Finalize()
{
	this->pipeServer.StopServer();
	return true;
}

// イベントコールバック関数
// 何かイベントが起きると呼ばれる
LRESULT CALLBACK CEpgTimerPlugIn::EventCallback(UINT Event,LPARAM lParam1,LPARAM lParam2,void *pClientData)
{
	CEpgTimerPlugIn *pThis=static_cast<CEpgTimerPlugIn*>(pClientData);
	switch (Event) {
	case TVTest::EVENT_PLUGINENABLE:
		if (lParam1!=0) {
			pThis->InitializePlugin();
			return TRUE;
		}else{
			pThis->pipeServer.StopServer();
			return TRUE;
		}
		break;
	default:
		break;
	}

	return 0;
}

int CALLBACK CEpgTimerPlugIn::CtrlCmdCallback(void* param, CMD_STREAM* cmdParam, CMD_STREAM* resParam)
{
	CEpgTimerPlugIn* sys = (CEpgTimerPlugIn*)param;

	resParam->dataSize = 0;
	resParam->param = CMD_ERR;

	switch( cmdParam->param ){
	case CMD2_VIEW_APP_SET_BONDRIVER:
		OutputDebugString(L"TvTest:CMD2_VIEW_APP_SET_BONDRIVER");
		{
			wstring val;
			if( ReadVALUE(&val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				if( sys->m_pApp->SetDriverName(val.c_str()) == TRUE ){
					resParam->param = CMD_SUCCESS;
				}
			}
		}
		break;
	case CMD2_VIEW_APP_GET_BONDRIVER:
		OutputDebugString(L"TvTest:CMD2_VIEW_APP_GET_BONDRIVER");
		{
			WCHAR buff[512] = L"";
			sys->m_pApp->GetDriverFullPathName(buff, 512);
			wstring bonName;
			GetFileName(buff, bonName );
			if( bonName.size() > 0 ){
				resParam->dataSize = GetVALUESize(bonName);
				resParam->data = new BYTE[resParam->dataSize];
				if( WriteVALUE(bonName, resParam->data, resParam->dataSize, NULL) == TRUE ){
					resParam->param = CMD_SUCCESS;
				}
			}
		}
		break;
	case CMD2_VIEW_APP_SET_CH:
		OutputDebugString(L"TvTest:CMD2_VIEW_APP_SET_CH");
		{
			SET_CH_INFO val;
			if( ReadVALUE(&val, cmdParam->data, cmdParam->dataSize, NULL ) == TRUE ){
				if( val.useSID == TRUE && val.useBonCh == TRUE ){
					int space = 0;
					int ch = 0;
					TVTest::ChannelInfo chInfo;
					while(1){
						if( sys->m_pApp->GetChannelInfo(space, ch, &chInfo) == false ){
							if( ch == 0 ){
								break;
							}else{
								space++;
								ch = 0;
							}
						}else{
							if( chInfo.Space == val.space &&
								chInfo.Channel == val.ch )
							{
								if( sys->m_pApp->SetChannel(space, ch, val.SID) == true ){
									resParam->param = CMD_SUCCESS;
								}
								break;
							}
							ch++;
						}
					}
				}
			}
		}
		break;
	case CMD2_VIEW_APP_CLOSE:
		OutputDebugString(L"TvTest:CMD2_VIEW_APP_CLOSE");
		{
			sys->m_pApp->Close(1);
		}
		break;
	default:
		_OutputDebugString(L"TvTest:err default cmd %d\r\n", cmdParam->param);
		resParam->param = CMD_NON_SUPPORT;
		break;
	}

	return 0;
}


