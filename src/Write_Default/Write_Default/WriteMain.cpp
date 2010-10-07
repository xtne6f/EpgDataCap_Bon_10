#include "StdAfx.h"
#include "WriteMain.h"


CWriteMain::CWriteMain(void)
{
	this->file = NULL;
}


CWriteMain::~CWriteMain(void)
{
	if( this->file != NULL ){
		_StopSave();
	}
}

//ファイル保存を開始する
//戻り値：
// TRUE（成功）、FALSE（失敗）
//引数：
// fileName				[IN]保存ファイルフルパス（必要に応じて拡張子変えたりなど行う）
// overWriteFlag		[IN]同一ファイル名存在時に上書きするかどうか（TRUE：する、FALSE：しない）
// createSize			[IN]入力予想容量（188バイトTSでの容量。即時録画時など総時間未定の場合は0。延長などの可能性もあるので目安程度）
BOOL CWriteMain::_StartSave(
	LPCWSTR fileName,
	BOOL overWriteFlag,
	ULONGLONG createSize
	)
{
	this->savePath = L"";

	wstring recFilePath = fileName;
	if( overWriteFlag == TRUE ){
		this->file = _CreateFile( recFilePath.c_str(), GENERIC_WRITE, FILE_SHARE_READ, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL );
	}else{
		this->file = _CreateFile( recFilePath.c_str(), GENERIC_WRITE, FILE_SHARE_READ, CREATE_NEW, FILE_ATTRIBUTE_NORMAL, NULL );
		if( this->file == INVALID_HANDLE_VALUE ){
			if( GetNextFileName(fileName, recFilePath) == TRUE ){
				this->file = _CreateFile( recFilePath.c_str(), GENERIC_WRITE, FILE_SHARE_READ, CREATE_NEW, FILE_ATTRIBUTE_NORMAL, NULL );
			}
		}
	}
	if( this->file == INVALID_HANDLE_VALUE ){
		this->file = NULL;
		return FALSE;
	}

	//ディスクに容量を確保
	if( createSize > 0 ){
		LARGE_INTEGER stPos;
		stPos.QuadPart = createSize;
		SetFilePointerEx( this->file, stPos, NULL, FILE_BEGIN );
		SetEndOfFile( this->file );
		CloseHandle( this->file );
		this->file = _CreateFile( recFilePath.c_str(), GENERIC_WRITE, FILE_SHARE_READ, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL );
		SetFilePointer( this->file, 0, NULL, FILE_BEGIN );
	}

	this->savePath = recFilePath;

	return TRUE;
}

//ファイル保存を終了する
//戻り値：
// TRUE（成功）、FALSE（失敗）
BOOL CWriteMain::_StopSave(
	)
{
	if( this->file != NULL ){
		SetEndOfFile(this->file);
		CloseHandle(this->file);
		this->file = NULL;
	}
	return TRUE;
}

//実際に保存しているファイルパスを取得する（再生やバッチ処理に利用される）
//filePathがNULL時は必要なサイズをfilePathSizeで返す
//通常filePathSize=512で呼び出し
//戻り値：
// TRUE（成功）、FALSE（失敗）
//引数：
// filePath				[OUT]保存ファイルフルパス
// filePathSize			[IN/OUT]filePathのサイズ(WCHAR単位)
BOOL CWriteMain::_GetSaveFilePath(
	WCHAR* filePath,
	DWORD* filePathSize
	)
{
	if( filePath == NULL ){
		if( filePathSize == NULL ){
			return FALSE;
		}else{
			*filePathSize = (DWORD)this->savePath.size()+1;
		}
	}else{
		if( filePathSize == NULL ){
			return FALSE;
		}else{
			if( *filePathSize < (DWORD)this->savePath.size()+1 ){
				*filePathSize = (DWORD)this->savePath.size()+1;
				return FALSE;
			}else{
				wcscpy_s(filePath, *filePathSize, this->savePath.c_str());
			}
		}
	}
	return TRUE;
}

//保存用TSデータを送る
//空き容量不足などで書き出し失敗した場合、writeSizeの値を元に
//再度保存処理するときの送信開始地点を決める
//戻り値：
// TRUE（成功）、FALSE（失敗）
//引数：
// data					[IN]TSデータ
// size					[IN]dataのサイズ
// writeSize			[OUT]保存に利用したサイズ
BOOL CWriteMain::_AddTSBuff(
	BYTE* data,
	DWORD size,
	DWORD* writeSize
	)
{
	BOOL ret = FALSE;
	if( this->file != NULL && data != NULL && size > 0 ){
		ret = WriteFile(this->file, data, size, writeSize, NULL);
	}
	return ret;
}

BOOL CWriteMain::GetNextFileName(wstring filePath, wstring& newPath)
{
	WCHAR szPath[_MAX_PATH];
	WCHAR szDrive[_MAX_DRIVE];
	WCHAR szDir[_MAX_DIR];
	WCHAR szFname[_MAX_FNAME];
	WCHAR szExt[_MAX_EXT];
	_wsplitpath_s( filePath.c_str(), szDrive, _MAX_DRIVE, szDir, _MAX_DIR, szFname, _MAX_FNAME, szExt, _MAX_EXT );
	_wmakepath_s(  szPath, _MAX_PATH, szDrive, szDir, NULL, NULL );

	BOOL findFlag = FALSE;
	for( int i=1; i<1000; i++ ){
		WIN32_FIND_DATA findData;
		HANDLE find;

		wstring name;
		Format(name, L"%s%s-(%d)%s", szPath, szFname, i, szExt);
		newPath = name;
		
		find = FindFirstFile( newPath.c_str(), &findData);
		if ( find == INVALID_HANDLE_VALUE ) {
			findFlag = TRUE;
			break;
		}
		FindClose(find);
	}
	return findFlag;
}
