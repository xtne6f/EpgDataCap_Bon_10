#pragma once

#include "../../Common/twitterUtil.h"

typedef enum{
	TW_TEXT	= 0,				//テキストそのまま
	TW_CHG_RESERVE_RELOADEPG,	//EPG再読み込み
	TW_CHG_RESERVE_CHK_REC,		//録画中の追従
}SEND_TWEET_MODE;

class CTwitterManager
{
public:
	CTwitterManager(void);
	~CTwitterManager(void);

	//Proxy使用を設定
	//戻り値：
	// エラーコード
	//引数：
	// useProxy			[IN]Proxy使うかどうか（TRUE:Proxy使う）
	// proxyInfo		[IN]Proxy使う場合の設定情報
	void SetProxy(
		BOOL useProxy,
		USE_PROXY_INFO* proxyInfo
		);


	void SendTweet(
		SEND_TWEET_MODE mode,
		void* param1,
		void* param2,
		void* param3
		);

protected:
	HANDLE lockEvent;
	CTwitterUtil twitterUtil;

protected:
	//PublicAPI排他制御用
	BOOL Lock(LPCWSTR log = NULL, DWORD timeOut = 60*1000);
	void UnLock(LPCWSTR log = NULL);

};

