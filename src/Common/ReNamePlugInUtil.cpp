#include "StdAfx.h"
#include "ReNamePlugInUtil.h"

#include "Util.h"
#include "ErrDef.h"

CReNamePlugInUtil::CReNamePlugInUtil(void)
{
	module = NULL;

	this->lockEvent = _CreateEvent(FALSE, TRUE, _T("Global\\EpgTimerSrv_ReName_Event") );
}

CReNamePlugInUtil::~CReNamePlugInUtil(void)
{
	UnInitialize();

	if( this->lockEvent != NULL ){
		UnLock();
		CloseHandle(this->lockEvent);
		this->lockEvent = NULL;
	}
}

BOOL CReNamePlugInUtil::Lock(LPCWSTR log, DWORD timeOut)
{
	if( this->lockEvent == NULL ){
		return FALSE;
	}
	if( log != NULL ){
		OutputDebugString(log);
	}
	DWORD dwRet = WaitForSingleObject(this->lockEvent, timeOut);
	if( dwRet == WAIT_ABANDONED || 
		dwRet == WAIT_FAILED){
		return FALSE;
	}
	return TRUE;
}

void CReNamePlugInUtil::UnLock(LPCWSTR log)
{
	if( this->lockEvent != NULL ){
		SetEvent(this->lockEvent);
	}
	if( log != NULL ){
		OutputDebugString(log);
	}
}

//DLLの初期化
//戻り値：
// エラーコード
//引数：
// loadDllFilePath		[IN]ロードするDLLパス
BOOL CReNamePlugInUtil::Initialize(
	LPCWSTR loadDllFilePath
	)
{
	if( module != NULL ){
		return FALSE;
	}
	if( Lock() == FALSE ) return FALSE;

	pfnGetPlugInNameRNP = NULL;
	pfnSettingRNP = NULL;
	pfnConvertRecNameRNP = NULL;


	BOOL ret = TRUE;

	module = ::LoadLibrary(loadDllFilePath);

	if( module == NULL ){
		OutputDebugString(L"dlのロードに失敗しました\r\n");
		UnLock();
		return FALSE;
	}

	pfnGetPlugInNameRNP = ( GetPlugInNameRNP ) ::GetProcAddress( module , "GetPlugInName");
	if( !pfnGetPlugInNameRNP ){
		OutputDebugString(L"GetPlugInNameの GetProcAddress に失敗\r\n");
		ret = FALSE;
		goto ERR_END;
	}
	pfnSettingRNP = ( SettingRNP ) ::GetProcAddress( module , "Setting");
	if( !pfnSettingRNP ){
		OutputDebugString(L"Settingの GetProcAddress に失敗\r\n");
		ret = FALSE;
		goto ERR_END;
	}
	pfnConvertRecNameRNP = ( ConvertRecNameRNP ) ::GetProcAddress( module , "ConvertRecName");
	if( !pfnConvertRecNameRNP ){
		OutputDebugString(L"ConvertRecNameの GetProcAddress に失敗\r\n");
		ret = FALSE;
		goto ERR_END;
	}



ERR_END:
	if( ret == FALSE ){
		::FreeLibrary( module );
		module=NULL;
	}
	UnLock();
	return ret;
}

//DLLの開放
//戻り値：
// エラーコード
void CReNamePlugInUtil::UnInitialize()
{
	if( module != NULL ){
		::FreeLibrary( module );
	}
	module = NULL;

	pfnGetPlugInNameRNP = NULL;
	pfnSettingRNP = NULL;
	pfnConvertRecNameRNP = NULL;

}

//PlugInの名前を取得する
//nameがNULL時は必要なサイズをnameSizeで返す
//通常nameSize=256で呼び出し
//戻り値
// TRUE（成功）、FALSE（失敗）
//引数：
// name						[OUT]名称
// nameSize					[IN/OUT]nameのサイズ(WCHAR単位)
BOOL CReNamePlugInUtil::GetPlugInName(
	WCHAR* name,
	DWORD* nameSize
	)
{
	if( module == NULL ){
		return ERR_NOT_INIT;
	}

	return pfnGetPlugInNameRNP(name, nameSize);
}

//PlugInで設定が必要な場合、設定用のダイアログなどを表示する
//引数：
// parentWnd				[IN]親ウインドウ
void CReNamePlugInUtil::Setting(
	HWND parentWnd
	)
{
	if( module == NULL ){
		return ;
	}

	return pfnSettingRNP(parentWnd);
}

//入力された予約情報を元に、録画時のファイル名を作成する（拡張子含む）
//recNameがNULL時は必要なサイズをrecNamesizeで返す
//通常recNamesize=256で呼び出し
//戻り値
// TRUE（成功）、FALSE（失敗）
//引数：
// info						[IN]予約情報
// recName					[OUT]名称
// recNamesize				[IN/OUT]nameのサイズ(WCHAR単位)
BOOL CReNamePlugInUtil::ConvertRecName(
	PLUGIN_RESERVE_INFO* info,
	WCHAR* recName,
	DWORD* recNamesize
	)
{
	if( module == NULL ){
		return ERR_NOT_INIT;
	}
	if( Lock() == FALSE ) return FALSE;

	BOOL ret = pfnConvertRecNameRNP(info, recName, recNamesize);

	UnLock();

	return ret;
}

