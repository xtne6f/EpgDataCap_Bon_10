#include "StdAfx.h"
#include "TwitterManager.h"


CTwitterManager::CTwitterManager(void)
{
	this->twitterUtil.Initialize();
	this->lockEvent = _CreateEvent(FALSE, TRUE, NULL);
}


CTwitterManager::~CTwitterManager(void)
{
	if( this->lockEvent != NULL ){
		UnLock();
		CloseHandle(this->lockEvent);
		this->lockEvent = NULL;
	}
	this->twitterUtil.UnInitialize();
}
BOOL CTwitterManager::Lock(LPCWSTR log, DWORD timeOut)
{
	if( this->lockEvent == NULL ){
		return FALSE;
	}
	//if( log != NULL ){
	//	_OutputDebugString(L"◆%s",log);
	//}
	DWORD dwRet = WaitForSingleObject(this->lockEvent, timeOut);
	if( dwRet == WAIT_ABANDONED || 
		dwRet == WAIT_FAILED ||
		dwRet == WAIT_TIMEOUT){
			OutputDebugString(L"◆CTwitterManager::Lock FALSE");
			if( log != NULL ){
				OutputDebugString(log);
			}
		return FALSE;
	}
	return TRUE;
}

void CTwitterManager::UnLock(LPCWSTR log)
{
	if( this->lockEvent != NULL ){
		SetEvent(this->lockEvent);
	}
	if( log != NULL ){
		OutputDebugString(log);
	}
}

//Proxy使用を設定
//戻り値：
// エラーコード
//引数：
// useProxy			[IN]Proxy使うかどうか（TRUE:Proxy使う）
// proxyInfo		[IN]Proxy使う場合の設定情報
void CTwitterManager::SetProxy(
	BOOL useProxy,
	USE_PROXY_INFO* proxyInfo
	)
{
	if( Lock(L"SetProxy") == FALSE ) return;

	twitterUtil.SetProxy(useProxy, proxyInfo);

	UnLock();
}

void CTwitterManager::SendTweet(
	SEND_TWEET_MODE mode,
	void* param1,
	void* param2,
	void* param3
	)
{
	if( Lock(L"SendTweet") == FALSE ) return;

	wstring text;

	switch(mode){
	case TW_TEXT:
		{
			text = (WCHAR*)param1;
		}
		break;
	case TW_CHG_RESERVE_RELOADEPG:
		break;
	case TW_CHG_RESERVE_CHK_REC:
		break;
	default:
		break;
	}

	if( text.size() > 0 ){
		this->twitterUtil.SendTweet(TRUE, text.c_str());
	}

	UnLock();
}
