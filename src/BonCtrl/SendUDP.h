#pragma once

#include <winsock2.h>
#include <ws2tcpip.h>
#include "BonCtrlDef.h"
#include "../Common/StringUtil.h"

#define SAFE_DELETE(p)       { if(p) { delete (p);     (p)=NULL; } }
#define SAFE_DELETE_ARRAY(p) { if(p) { delete[] (p);   (p)=NULL; } }

#ifdef _DEBUG
#undef new
#endif
#include <vector>
#include <map>
#ifdef _DEBUG
#define new DEBUG_NEW
#endif
using namespace std;

class CSendUDP
{
public:
	CSendUDP(void);
	~CSendUDP(void);

	BOOL StartUpload( vector<NW_SEND_INFO>* List );
	void SendData(BYTE* pbBuff, DWORD dwSize);
	BOOL CloseUpload();

protected:
	static UINT WINAPI SendThread(LPVOID pParam);

protected:
	typedef struct _SOCKET_DATA{
		SOCKET sock;
		struct sockaddr_in addr;
	}SOCKET_DATA;
	vector<SOCKET_DATA> SockList;

	wstring m_strIniPath;

	vector<TS_DATA*> m_TSBuff;
	HANDLE m_hSendThread;
	HANDLE m_hSendStopEvent;

	HANDLE m_hCriticalEvent;

	UINT m_uiWait;
	UINT m_uiSendSize;

};
