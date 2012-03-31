#include "StdAfx.h"
#include "HttpPublicFileSend.h"

#include "../../Common/StringUtil.h"
#include "../../Common/PathUtil.h"

#include <shlwapi.h>
#pragma comment(lib, "shlwapi.lib")

CHttpPublicFileSend::CHttpPublicFileSend(void)
{
	this->folderPath = L"";
}


CHttpPublicFileSend::~CHttpPublicFileSend(void)
{
}

void CHttpPublicFileSend::SetPublicFolder(wstring folderPath)
{
	this->folderPath = folderPath;
}

int CHttpPublicFileSend::HttpRequest(string method, string uri, map<string, string>* headerList, SOCKET clientSock, HANDLE stopEvent)
{
	int ret = 200;

	if( this->folderPath.size() == 0 ){
		return 404;
	}

	string buff = "";
	string path = "";

	UrlDecode(uri.c_str(), uri.size(), buff);

	Separate(buff, "/file/", buff, path);
	Replace(path, "/", "\\");

	wstring wbuff = L"";
	UTF8toW(path, wbuff);

	wstring filePath = this->folderPath;
	ChkFolderPath(filePath);
	filePath += L"\\";
	filePath += wbuff;

	if( PathFileExists(filePath.c_str()) == FALSE ){
		return 404;
	}

	return ret;
}
