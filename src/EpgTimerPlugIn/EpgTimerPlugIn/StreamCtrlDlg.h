#pragma once

#include "../../Common/SendCtrlCmd.h"
#include "../../Common/StringUtil.h"

class CStreamCtrlDlg
{
public:
	CStreamCtrlDlg(void);
	~CStreamCtrlDlg(void);

	void SetCtrlCmd(CSendCtrlCmd* ctrlCmd, DWORD ctrlID, BOOL chkUdp, BOOL chkTcp, BOOL play, BOOL timeShiftMode);
	DWORD CreateStreamCtrlDialog(HINSTANCE hInstance, HWND parentHWND);
	void CloseStreamCtrlDialog();

	void StopTimer();

	BOOL ShowCtrlDlg(DWORD cmdShow);
	HWND GetDlgHWND(){ return this->hwnd; }

	void StartFullScreenMouseChk();
	void StopFullScreenMouseChk();
protected:
	void SetNWModeSend();
	void EnumIP();
	void UpdateLog();

	static LRESULT CALLBACK DlgProc(HWND hDlgWnd, UINT msg, WPARAM wp, LPARAM lp);

protected:
	HWND hwnd;
	CSendCtrlCmd* cmd;
	DWORD ctrlID;
	BOOL iniTCP;
	BOOL iniUDP;
	BOOL timeShiftMode;

	NWPLAY_PLAY_INFO nwPlayInfo;
};

