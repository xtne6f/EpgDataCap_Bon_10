#include "StdAfx.h"
#include "CheckRecFile.h"

#include "../../Common/PathUtil.h"

CCheckRecFile::CCheckRecFile(void)
{
}


CCheckRecFile::~CCheckRecFile(void)
{
}

void CCheckRecFile::SetCheckFolder(vector<wstring>* chkFolder)
{
	this->chkFolder = *chkFolder;
}

void CCheckRecFile::SetDeleteExt(vector<wstring>* delExt)
{
	this->delExt = *delExt;
}

void CCheckRecFile::CheckFreeSpace(map<DWORD, CReserveInfo*>* chkReserve, wstring defRecFolder)
{
	if( this->chkFolder.size() == 0 || chkReserve == NULL || defRecFolder.size() == 0 ){
		return;
	}

	map<wstring, ULONGLONG> checkMap;
	for( size_t i=0; i<this->chkFolder.size(); i++ ){
		checkMap.insert(pair<wstring, ULONGLONG>(this->chkFolder[i], 0));
	}

	LONGLONG now = GetNowI64Time();
	map<DWORD, CReserveInfo*>::iterator itrRes;
	for( itrRes = chkReserve->begin(); itrRes != chkReserve->end(); itrRes++ ){
		wstring chkFolder = defRecFolder;
		RESERVE_DATA data;
		itrRes->second->GetData(&data);
		if( data.recSetting.recMode == RECMODE_NO || data.recSetting.recMode == RECMODE_VIEW ){
			continue;
		}
		if( now + 2*60*60*I64_1SEC < ConvertI64Time(data.startTime) ){
			//2時間以上先
			continue;
		}
		if( ConvertI64Time(data.startTime) < now ){
			//録画中
			continue;
		}
		if( data.recSetting.recFolderList.size() > 0 ){
			chkFolder = data.recSetting.recFolderList[0].recFolder;
		}

		map<wstring, ULONGLONG>::iterator itr;
		itr = checkMap.find(chkFolder);
		if( itr != checkMap.end() ){
			DWORD bitrate = 0;
			_GetBitrate(data.originalNetworkID, data.transportStreamID, data.serviceID, &bitrate);
			itr->second += ((ULONGLONG)(bitrate/8)*1000) * data.durationSecond;
		}
	}

	map<wstring, ULONGLONG>::iterator itr;
	for( itr = checkMap.begin(); itr != checkMap.end(); itr++ ){
		if( itr->second > 0 ){
			ULARGE_INTEGER freeBytesAvailable;
			ULARGE_INTEGER totalNumberOfBytes;
			ULARGE_INTEGER totalNumberOfFreeBytes;
			if( _GetDiskFreeSpaceEx(itr->first.c_str(), &freeBytesAvailable, &totalNumberOfBytes, &totalNumberOfFreeBytes ) == TRUE ){
				ULONGLONG free = freeBytesAvailable.QuadPart;
				if( free > itr->second ){
					continue;
				}
				map<LONGLONG, TS_FILE_INFO> tsFileList;
				FindTsFileList(itr->first, &tsFileList);
				while( free < itr->second ){
					map<LONGLONG, TS_FILE_INFO>::iterator itr;
					itr = tsFileList.begin();
					if( itr != tsFileList.end() ){
						DeleteFile( itr->second.filePath.c_str() );

						_OutputDebugString(L"★Auto Delete : %s", itr->second.filePath.c_str());
						for( size_t i=0 ; i<this->delExt.size(); i++ ){
							wstring delFile = L"";
							wstring delFileName = L"";
							GetFileFolder(itr->second.filePath, delFile);
							GetFileTitle(itr->second.filePath, delFileName);
							delFile += L"\\";
							delFile += delFileName;
							delFile += this->delExt[i];

							DeleteFile( delFile.c_str() );
							_OutputDebugString(L"★Auto Delete : %s", delFile.c_str());
						}

						free += itr->second.fileSize;
						tsFileList.erase(itr);
					}else{
						break;
					}
				}
			}
		}
	}
}

void CCheckRecFile::CheckFreeSpaceLive(RESERVE_DATA* reserve, wstring recFolder)
{
	if( this->chkFolder.size() == 0 || reserve == NULL || recFolder.size() == 0 ){
		return;
	}

	map<wstring, ULONGLONG> checkMap;
	for( size_t i=0; i<this->chkFolder.size(); i++ ){
		checkMap.insert(pair<wstring, ULONGLONG>(this->chkFolder[i], 0));
	}

	LONGLONG now = GetNowI64Time();

	if( reserve->recSetting.recMode == RECMODE_NO || reserve->recSetting.recMode == RECMODE_VIEW ){
		return;
	}

	map<wstring, ULONGLONG>::iterator itr;
	itr = checkMap.find(recFolder);
	if( itr != checkMap.end() ){
		//500MB空いてるかチェック
		itr->second = 500*1024*1024;

		ULARGE_INTEGER freeBytesAvailable;
		ULARGE_INTEGER totalNumberOfBytes;
		ULARGE_INTEGER totalNumberOfFreeBytes;
		if( _GetDiskFreeSpaceEx(itr->first.c_str(), &freeBytesAvailable, &totalNumberOfBytes, &totalNumberOfFreeBytes ) == TRUE ){
			ULONGLONG free = freeBytesAvailable.QuadPart;
			if( free > itr->second ){
				return;
			}
			map<LONGLONG, TS_FILE_INFO> tsFileList;
			FindTsFileList(itr->first, &tsFileList);
			while( free < itr->second ){
				map<LONGLONG, TS_FILE_INFO>::iterator itr;
				itr = tsFileList.begin();
				if( itr != tsFileList.end() ){
					DeleteFile( itr->second.filePath.c_str() );

					_OutputDebugString(L"★Auto Delete : %s", itr->second.filePath.c_str());
					for( size_t i=0 ; i<this->delExt.size(); i++ ){
						wstring delFile = L"";
						wstring delFileName = L"";
						GetFileFolder(itr->second.filePath, delFile);
						GetFileTitle(itr->second.filePath, delFileName);
						delFile += L"\\";
						delFile += delFileName;
						delFile += this->delExt[i];

						DeleteFile( delFile.c_str() );
						_OutputDebugString(L"★Auto Delete : %s", delFile.c_str());
					}

					free += itr->second.fileSize;
					tsFileList.erase(itr);
				}else{
					break;
				}
			}
		}
	}
}

void CCheckRecFile::FindTsFileList(wstring findFolder, map<LONGLONG, TS_FILE_INFO>* findList)
{
	wstring searchKey = findFolder;
	searchKey += L"\\*.ts";

	WIN32_FIND_DATA findData;
	HANDLE find;

	//指定フォルダのファイル一覧取得
	find = FindFirstFile( searchKey.c_str(), &findData);
	if ( find == INVALID_HANDLE_VALUE ) {
		return ;
	}
	do{
		if( (findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) == 0 ){
			//本当に拡張子DLL?
			if( IsExt(findData.cFileName, L".ts") == TRUE ){
				TS_FILE_INFO item;

				Format(item.filePath, L"%s\\%s", findFolder.c_str(), findData.cFileName);

				HANDLE file = _CreateFile( item.filePath.c_str(), GENERIC_READ, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL );
				if( file != INVALID_HANDLE_VALUE ){
					FILETIME CreationTime;
					FILETIME LastAccessTime;
					FILETIME LastWriteTime;
					GetFileTime(file, &CreationTime, &LastAccessTime, &LastWriteTime);

					item.fileTime = ((LONGLONG)CreationTime.dwHighDateTime)<<32 | (LONGLONG)CreationTime.dwLowDateTime;

					DWORD sizeH=0;
					DWORD sizeL=0;
					sizeL = GetFileSize(file,&sizeH);

					item.fileSize = ((LONGLONG)sizeH)<<32 | (LONGLONG)sizeL;

					CloseHandle(file);

					findList->insert(pair<LONGLONG, TS_FILE_INFO>(item.fileTime, item));
				}
			}
		}
	}while(FindNextFile(find, &findData));

	FindClose(find);

}
