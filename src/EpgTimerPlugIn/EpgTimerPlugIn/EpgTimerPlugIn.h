#pragma once

#define TVTEST_PLUGIN_CLASS_IMPLEMENT	// クラスとして実装
#include "TVTestPlugin.h"

#include "../../Common/PipeServer.h"
#include "../../Common/ErrDef.h"
#include "../../Common/CtrlCmdDef.h"
#include "../../Common/CtrlCmdUtil.h"


class CEpgTimerPlugIn : public TVTest::CTVTestPlugin
{
private:
	CPipeServer pipeServer;

private:
	static LRESULT CALLBACK EventCallback(UINT Event,LPARAM lParam1,LPARAM lParam2,void *pClientData);
	static int CALLBACK CtrlCmdCallback(void* param, CMD_STREAM* cmdParam, CMD_STREAM* resParam);

public:
	CEpgTimerPlugIn();
	virtual bool GetPluginInfo(TVTest::PluginInfo *pInfo);
	virtual bool Initialize();
	virtual bool Finalize();

	void InitializePlugin();
};
