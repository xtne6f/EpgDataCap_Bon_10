#pragma once

#include "../../Common/StructDef.h"
#include "EpgDBManager.h"

class CRestApiManager
{
public:
	CRestApiManager(void);
	~CRestApiManager(void);

	DWORD AnalyzeCmd(string verb, string url, string param, HTTP_STREAM* sendParam, CEpgDBManager* epgDB);

private:
	void CheckXMLChar(wstring& text);

	DWORD GetEnumService(string param, HTTP_STREAM* sendParam, CEpgDBManager* epgDB);
	DWORD GetEnumEventInfo(string param, HTTP_STREAM* sendParam, CEpgDBManager* epgDB);
};
