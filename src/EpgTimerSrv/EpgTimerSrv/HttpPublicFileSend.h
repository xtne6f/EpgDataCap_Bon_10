#pragma once

#include "../../Common/CommonDef.h"

class CHttpPublicFileSend
{
public:
	CHttpPublicFileSend(void);
	~CHttpPublicFileSend(void);

	void SetPublicFolder(wstring folderPath);

	int HttpRequest(string method, string uri, map<string, string>* headerList, SOCKET clientSock, HANDLE stopEvent);

protected:
	wstring folderPath;
};

