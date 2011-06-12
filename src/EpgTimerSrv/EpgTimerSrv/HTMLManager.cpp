#include "StdAfx.h"
#include "HTMLManager.h"
#include "../../Common/StringUtil.h"
#include "../../Common/TimeUtil.h"
#include "../../Common/PathUtil.h"

#define HTML_TOP "<HTML LANG=\"ja\">\r\n<HEAD>\r\n<META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html;charset=Shift_JIS\">\r\n<TITLE>EpgTimer</TITLE>\r\n</HEAD>\r\n<BODY>\r\n"
#define HTML_END "</BODY>\r\n</HTML>\r\n"

CHTMLManager::CHTMLManager(void)
{
}


CHTMLManager::~CHTMLManager(void)
{
}

void CHTMLManager::LoadRecSetData(WORD preset, REC_SETTING_DATA* recSetData)
{
	wstring iniPath = L"";
	GetModuleIniPath(iniPath);
	WCHAR iniBuff[512]=L"";

	wstring defName = L"";
	wstring defFolderName = L"";
	wstring defFolder1SegName = L"";
	if( preset == 0 ){
		defName = L"REC_DEF";
		defFolderName = L"REC_DEF_FOLDER";
		defFolder1SegName = L"REC_DEF_FOLDER_1SEG";
	}else{
		Format(defName, L"REC_DEF%d", preset);
		Format(defFolderName, L"REC_DEF_FOLDER%d", preset);
		Format(defFolder1SegName, L"REC_DEF_FOLDER_1SEG%d", preset);
	}

	recSetData->recMode = (BYTE)GetPrivateProfileInt(defName.c_str(), L"RecMode", 1, iniPath.c_str());
    recSetData->priority = (BYTE)GetPrivateProfileInt(defName.c_str(), L"Priority", 2, iniPath.c_str());
    recSetData->tuijyuuFlag = (BYTE)GetPrivateProfileInt(defName.c_str(), L"TuijyuuFlag", 1, iniPath.c_str());
    recSetData->serviceMode = (BYTE)GetPrivateProfileInt(defName.c_str(), L"ServiceMode", 0, iniPath.c_str());
    recSetData->pittariFlag = (BYTE)GetPrivateProfileInt(defName.c_str(), L"PittariFlag", 0, iniPath.c_str());

	ZeroMemory(iniBuff, sizeof(WCHAR)*512);
    GetPrivateProfileString(defName.c_str(), L"BatFilePath", L"", iniBuff, 512, iniPath.c_str());
    recSetData->batFilePath = iniBuff;

    int count = GetPrivateProfileInt(defFolderName.c_str(), L"Count", 0, iniPath.c_str());
    for (int i = 0; i < count; i++)
    {
		wstring fn;
		wstring wn;
		wstring rn;
		Format(fn, L"%d", i);
		Format(wn, L"WritePlugIn%d", i);
		Format(rn, L"RecNamePlugIn%d", i);
        REC_FILE_SET_INFO folderInfo;
        ZeroMemory(iniBuff, sizeof(WCHAR)*512);
		GetPrivateProfileString(defFolderName.c_str(), fn.c_str(), L"", iniBuff, 512, iniPath.c_str());
        folderInfo.recFolder = iniBuff;
        ZeroMemory(iniBuff, sizeof(WCHAR)*512);
		GetPrivateProfileString(defFolderName.c_str(), wn.c_str(), L"Write_Default.dll", iniBuff, 512, iniPath.c_str());
        folderInfo.writePlugIn = iniBuff;
        ZeroMemory(iniBuff, sizeof(WCHAR)*512);
		GetPrivateProfileString(defFolderName.c_str(), rn.c_str(), L"", iniBuff, 512, iniPath.c_str());
        folderInfo.recNamePlugIn = iniBuff;

        recSetData->recFolderList.push_back(folderInfo);
    }

    count = GetPrivateProfileInt(defFolder1SegName.c_str(), L"Count", 0, iniPath.c_str());
    for (int i = 0; i < count; i++)
    {
		wstring fn;
		wstring wn;
		wstring rn;
		Format(fn, L"%d", i);
		Format(wn, L"WritePlugIn%d", i);
		Format(rn, L"RecNamePlugIn%d", i);
        REC_FILE_SET_INFO folderInfo;
        ZeroMemory(iniBuff, sizeof(WCHAR)*512);
        GetPrivateProfileString(defFolder1SegName.c_str(), fn.c_str(), L"", iniBuff, 512, iniPath.c_str());
        folderInfo.recFolder = iniBuff;
        ZeroMemory(iniBuff, sizeof(WCHAR)*512);
        GetPrivateProfileString(defFolder1SegName.c_str(), wn.c_str(), L"Write_Default.dll", iniBuff, 512, iniPath.c_str());
        folderInfo.writePlugIn = iniBuff;
        ZeroMemory(iniBuff, sizeof(WCHAR)*512);
        GetPrivateProfileString(defFolder1SegName.c_str(), rn.c_str(), L"", iniBuff, 512, iniPath.c_str());
        folderInfo.recNamePlugIn = iniBuff;

        recSetData->partialRecFolder.push_back(folderInfo);
    }

    recSetData->suspendMode = (BYTE)GetPrivateProfileInt(defName.c_str(), L"SuspendMode", 0, iniPath.c_str());
    recSetData->rebootFlag = (BYTE)GetPrivateProfileInt(defName.c_str(), L"RebootFlag", 0, iniPath.c_str());
    recSetData->useMargineFlag = (BYTE)GetPrivateProfileInt(defName.c_str(), L"UseMargineFlag", 0, iniPath.c_str());
    recSetData->startMargine = GetPrivateProfileInt(defName.c_str(), L"StartMargine", 0, iniPath.c_str());
    recSetData->endMargine = GetPrivateProfileInt(defName.c_str(), L"EndMargine", 0, iniPath.c_str());
    recSetData->continueRecFlag = (BYTE)GetPrivateProfileInt(defName.c_str(), L"ContinueRec", 0, iniPath.c_str());
    recSetData->partialRecFlag = (BYTE)GetPrivateProfileInt(defName.c_str(), L"PartialRec", 0, iniPath.c_str());
    recSetData->tunerID = (DWORD)GetPrivateProfileInt(defName.c_str(), L"TunerID", 0, iniPath.c_str());

}

void CHTMLManager::CreateRecSetForm(REC_SETTING_DATA* recSetData, vector<TUNER_RESERVE_INFO>* tunerList, string& htmlText)
{
	htmlText = "";
	htmlText+="録画モード:\r\n<select name=\"recMode\">\r\n";
	if( recSetData->recMode == RECMODE_ALL ){
		htmlText+="<option value=\"0\" selected>全サービス\r\n";
	}else{
		htmlText+="<option value=\"0\">全サービス\r\n";
	}
	if( recSetData->recMode == RECMODE_SERVICE ){
		htmlText+="<option value=\"1\" selected>指定サービスのみ\r\n";
	}else{
		htmlText+="<option value=\"1\">指定サービスのみ\r\n";
	}
	if( recSetData->recMode == RECMODE_ALL_NOB25 ){
		htmlText+="<option value=\"2\" selected>全サービス（デコード処理なし）\r\n";
	}else{
		htmlText+="<option value=\"2\">全サービス（デコード処理なし）\r\n";
	}
	if( recSetData->recMode == RECMODE_SERVICE_NOB25 ){
		htmlText+="<option value=\"3\" selected>指定サービスのみ（デコード処理なし）\r\n";
	}else{
		htmlText+="<option value=\"3\">指定サービスのみ（デコード処理なし）\r\n";
	}
	if( recSetData->recMode == RECMODE_VIEW ){
		htmlText+="<option value=\"4\" selected>視聴\r\n";
	}else{
		htmlText+="<option value=\"4\">視聴\r\n";
	}
	if( recSetData->recMode == RECMODE_NO ){
		htmlText+="<option value=\"5\" selected>無効\r\n";
	}else{
		htmlText+="<option value=\"5\">無効\r\n";
	}
	htmlText+="</select><BR>\r\n";

	htmlText+="追従:\r\n<select name=\"tuijyuuFlag\">\r\n";
	if( recSetData->tuijyuuFlag == 0 ){
		htmlText+="<option value=\"0\" selected>しない\r\n";
	}else{
		htmlText+="<option value=\"0\">しない\r\n";
	}
	if( recSetData->tuijyuuFlag == 1 ){
		htmlText+="<option value=\"1\" selected>する\r\n";
	}else{
		htmlText+="<option value=\"1\">する\r\n";
	}
	htmlText+="</select><BR>\r\n";

	htmlText+="優先度:\r\n<select name=\"priority\">\r\n";
	if( recSetData->priority == 1 ){
		htmlText+="<option value=\"1\" selected>1\r\n";
	}else{
		htmlText+="<option value=\"1\">1\r\n";
	}
	if( recSetData->priority == 2 ){
		htmlText+="<option value=\"2\" selected>2\r\n";
	}else{
		htmlText+="<option value=\"2\">2\r\n";
	}
	if( recSetData->priority == 3 ){
		htmlText+="<option value=\"3\" selected>3\r\n";
	}else{
		htmlText+="<option value=\"3\">3\r\n";
	}
	if( recSetData->priority == 4 ){
		htmlText+="<option value=\"4\" selected>4\r\n";
	}else{
		htmlText+="<option value=\"4\">4\r\n";
	}
	if( recSetData->priority == 5 ){
		htmlText+="<option value=\"5\" selected>5\r\n";
	}else{
		htmlText+="<option value=\"5\">5\r\n";
	}
	htmlText+="</select><BR>\r\n";

	htmlText+="ぴったり（？）録画:\r\n<select name=\"pittariFlag\">\r\n";
	if( recSetData->pittariFlag == 0 ){
		htmlText+="<option value=\"0\" selected>しない\r\n";
	}else{
		htmlText+="<option value=\"0\">しない\r\n";
	}
	if( recSetData->pittariFlag == 1 ){
		htmlText+="<option value=\"1\" selected>する\r\n";
	}else{
		htmlText+="<option value=\"1\">する\r\n";
	}
	htmlText+="</select><BR>\r\n";

	htmlText+="録画後動作:\r\n<select name=\"suspendMode\">\r\n";
	if( recSetData->suspendMode == 0 ){
		htmlText+="<option value=\"0\" selected>デフォルト設定を使用\r\n";
	}else{
		htmlText+="<option value=\"0\">デフォルト設定を使用\r\n";
	}
	if( recSetData->suspendMode == 1 ){
		htmlText+="<option value=\"1\" selected>スタンバイ\r\n";
	}else{
		htmlText+="<option value=\"1\">スタンバイ\r\n";
	}
	if( recSetData->suspendMode == 2 ){
		htmlText+="<option value=\"2\" selected>休止\r\n";
	}else{
		htmlText+="<option value=\"2\">休止\r\n";
	}
	if( recSetData->suspendMode == 3 ){
		htmlText+="<option value=\"3\" selected>シャットダウン\r\n";
	}else{
		htmlText+="<option value=\"3\">シャットダウン\r\n";
	}
	if( recSetData->suspendMode == 4 ){
		htmlText+="<option value=\"4\" selected>何もしない\r\n";
	}else{
		htmlText+="<option value=\"4\">何もしない\r\n";
	}
	htmlText+="</select>\r\n";

	if( recSetData->rebootFlag == 0 ){
		htmlText+=" <input type=checkbox name=\"rebootFlag\" value=\"1\">復帰後再起動する";
	}else{
		htmlText+=" <input type=checkbox name=\"rebootFlag\" value=\"1\" checked>復帰後再起動する";
	}
	htmlText+="<BR>\r\n";

	htmlText+="録画マージン:\r\n";
	if( recSetData->useMargineFlag == 0 ){
		htmlText+="<input type=checkbox name=\"useDefMargineFlag\" value=\"1\" checked>デフォルト設定で使用\r\n";
	}else{
		htmlText+="<input type=checkbox name=\"useDefMargineFlag\" value=\"1\">デフォルト設定で使用\r\n";
	}
	string buff;
	Format(buff, " 開始<input type=text name=\"startMargine\" value=\"%d\">\r\n", recSetData->startMargine);
	htmlText+=buff;
	Format(buff, " 終了<input type=text name=\"endMargine\" value=\"%d\">\r\n", recSetData->endMargine);
	htmlText+=buff;
	htmlText+="<BR>\r\n";

	htmlText+="指定サービス対象データ:\r\n";
	if( recSetData->serviceMode == 0 ){
		htmlText+="<input type=checkbox name=\"serviceMode\" value=\"0\" checked>デフォルト設定で使用 \r\n";
		htmlText+="<input type=checkbox name=\"serviceMode_1\" value=\"0\">字幕データを含める \r\n";
		htmlText+="<input type=checkbox name=\"serviceMode_2\" value=\"0\">データカルーセルを含める \r\n";
	}else{
		htmlText+="<input type=checkbox name=\"serviceMode\" value=\"0\">デフォルト設定で使用";
		if( (recSetData->serviceMode & 0x10)>0){
			htmlText+="<input type=checkbox name=\"serviceMode_1\" value=\"0\" checked>字幕データを含める \r\n";
		}else{
			htmlText+="<input type=checkbox name=\"serviceMode_1\" value=\"0\">字幕データを含める \r\n";
		}
		if( (recSetData->serviceMode & 0x20)>0){
			htmlText+="<input type=checkbox name=\"serviceMode_2\" value=\"0\" checked>データカルーセルを含める \r\n";
		}else{
			htmlText+="<input type=checkbox name=\"serviceMode_2\" value=\"0\">データカルーセルを含める \r\n";
		}
	}
	htmlText+="<BR>\r\n";

	htmlText+="連続録画動作:\r\n";
	if( recSetData->continueRecFlag == 0 ){
		htmlText+="<input type=checkbox name=\"continueRecFlag\" value=\"1\">後ろの予約を同一ファイルで出力する";
	}else{
		htmlText+="<input type=checkbox name=\"continueRecFlag\" value=\"1\" checked>後ろの予約を同一ファイルで出力する";
	}
	htmlText+="<BR>\r\n";

	htmlText+="使用チューナー強制指定:\r\n<select name=\"tunerID\">\r\n";
	if( recSetData->tunerID == 0 ){
		htmlText+="<option value=\"0\" selected>自動\r\n";
	}else{
		htmlText+="<option value=\"0\">自動\r\n";
	}
	for( size_t i=0; i<tunerList->size(); i++ ){
		if( (*tunerList)[i].tunerID != 0xFFFFFFFF ){
			string bon;
			WtoA((*tunerList)[i].tunerName, bon);
			if( recSetData->tunerID == (*tunerList)[i].tunerID){
				Format(buff, "<option value=\"%d\" selected>ID:%08X(%s)\r\n", (*tunerList)[i].tunerID, (*tunerList)[i].tunerID, bon.c_str());
			}else{
				Format(buff, "<option value=\"%d\">ID:%08X(%s)\r\n", (*tunerList)[i].tunerID, (*tunerList)[i].tunerID, bon.c_str());
			}
			htmlText+=buff;
		}
	}
	htmlText+="</select><BR>\r\n";

	htmlText+="録画後実行bat（プリセットによる変更のみ対応）:\r\n";
	string bat;
	WtoA(recSetData->batFilePath, bat);
	htmlText+=bat;
	htmlText+="<BR>\r\n";

	htmlText+="録画フォルダ、使用プラグイン（プリセットによる変更のみ対応）:\r\n";
	htmlText+="<TABLE BORDER=\"1\">\r\n";
	htmlText+="<TR><TD>フォルダ</TD><TD>出力PlugIn</TD><TD>ファイル名PlugIn</TD></TR>\r\n";
	string hiddenFolde="";
	Format(hiddenFolde, "<input type=hidden name=\"recFolderCount\" value=\"%d\">\r\n", recSetData->recFolderList.size());
	for( size_t i=0; i<recSetData->recFolderList.size(); i++ ){
		string item;
		string folder;
		string write;
		string name;
		WtoA(recSetData->recFolderList[i].recFolder, folder);
		WtoA(recSetData->recFolderList[i].writePlugIn, write);
		WtoA(recSetData->recFolderList[i].recNamePlugIn, name);
		Format(item, "<TR><TD>%s</TD><TD>%s</TD><TD>%s</TD></TR>\r\n", folder.c_str(), write.c_str(), name.c_str());
		htmlText+=item;
	}
	htmlText+="</TABLE>\r\n";
	htmlText+=hiddenFolde;

	htmlText+="部分受信サービス:<BR>\r\n";
	if( recSetData->partialRecFlag == 0 ){
		htmlText+="<input type=checkbox name=\"partialRecFlag\" value=\"1\">別ファイルに同時出力する";
	}else{
		htmlText+="<input type=checkbox name=\"partialRecFlag\" value=\"1\" checked>別ファイルに同時出力する";
	}
	htmlText+="<BR>\r\n";
	htmlText+="録画フォルダ、使用プラグイン（プリセットによる変更のみ対応）:\r\n";
	htmlText+="<TABLE BORDER=\"1\">\r\n";
	htmlText+="<TR><TD>フォルダ</TD><TD>出力PlugIn</TD><TD>ファイル名PlugIn</TD></TR>\r\n";
	Format(hiddenFolde, "<input type=hidden name=\"partialFolderCount\" value=\"%d\">\r\n", recSetData->partialRecFolder.size());
	for( size_t i=0; i<recSetData->partialRecFolder.size(); i++ ){
		string item;
		string folder;
		string write;
		string name;
		WtoA(recSetData->partialRecFolder[i].recFolder, folder);
		WtoA(recSetData->partialRecFolder[i].writePlugIn, write);
		WtoA(recSetData->partialRecFolder[i].recNamePlugIn, name);
		Format(item, "<TR><TD>%s</TD><TD>%s</TD><TD>%s</TD></TR>\r\n", folder.c_str(), write.c_str(), name.c_str());
		htmlText+=item;
	}
	htmlText+="</TABLE>\r\n";
	htmlText+=hiddenFolde;

	htmlText+="<BR>\r\n";
}

BOOL CHTMLManager::GetIndexPage(HTTP_STREAM* sendParam)
{
	if( sendParam == NULL ){
		return FALSE;
	}
	string html = HTML_TOP;
	html+="メニュー<BR><BR><A HREF=\"reserve.html\">予約一覧</A><BR>\r\n<A HREF=\"recinfo.html\">録画結果一覧</A><BR>\r\n<A HREF=\"epg.html\">番組表</A><BR>\r\n";
	html+=HTML_END;
	sendParam->dataSize = (DWORD)html.size();
	sendParam->data = new BYTE[sendParam->dataSize];
	memcpy(sendParam->data, html.c_str(), sendParam->dataSize);
	if( sendParam->dataSize > 0 ){
		Format(sendParam->httpHeader, "HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: %d\r\nConnection: close\r\n\r\n", sendParam->dataSize);
	}else{
		sendParam->httpHeader = "HTTP/1.0 400 Bad Request\r\nConnection: close\r\n\r\n";
	}
	return TRUE;
}

BOOL CHTMLManager::GetReservePage(vector<RESERVE_DATA*>* list, int pageIndex, HTTP_STREAM* sendParam)
{
	if( sendParam == NULL ){
		return FALSE;
	}

	wstring iniPath = L"";
	GetModuleIniPath(iniPath);
	int pageCount = GetPrivateProfileInt(L"HTTP", L"ReservePageCount", 30, iniPath.c_str());
	if( pageCount == 0 ){
		pageCount = 30;
	}

	string html = HTML_TOP;
	html+="予約一覧<BR><BR>\r\n";

	if(list == NULL ){
		html+="予約なし<BR>\r\n";
	}else{
		if( list->size() == 0 || (size_t)pageIndex*pageCount > list->size()){
			html+="予約なし<BR>\r\n";
		}else{
			//ページリンクの作成
			int pageNum = (int)list->size()/pageCount;
			if( (int)list->size()%pageCount > 0 ){
				pageNum++;
			}
			for( int i=0; i<pageNum; i++ ){
				string ref = "";
				if( i != pageIndex){
					Format(ref, "<A HREF=\"reserve.html?page=%d\">%d～</A> \r\n", i, i*pageCount+1);
				}else{
					Format(ref, "%d～ \r\n", i*pageCount+1);
				}
				html+=ref;
			}
			html+="<BR><BR>\r\n";

			//時間で予約ソート
			multimap<__int64, RESERVE_DATA*> sortMap;
			for( size_t i=0; i<list->size(); i++ ){
				__int64 stratTime = ConvertI64Time((*list)[i]->startTime);
				sortMap.insert(pair<__int64, RESERVE_DATA*>(stratTime, (*list)[i]));
			}
			//一覧の作成
			html+="<TABLE BORDER=\"1\">\r\n";

			multimap<__int64, RESERVE_DATA*>::iterator itr;
			itr = sortMap.begin();
			advance(itr, pageIndex*pageCount);
			for( int i=0; i<pageCount; i++){
				if( itr != sortMap.end() ){
					//html+="<TR><TD>\r\n";
					string tr = "<TR>";
					if( itr->second->recSetting.recMode == RECMODE_NO ){
						tr = "<TR BGCOLOR=#C8C8C8>";
					}else if(itr->second->overlapMode == 1 ){
						tr = "<TR BGCOLOR=#FFFF00>";
					}else if(itr->second->overlapMode == 2 ){
						tr = "<TR BGCOLOR=#FF0000>";
					}
					string item = "";
					wstring dateW = L"";
					string date = "";
					string station = "";
					string title = "";
					GetTimeString3(itr->second->startTime, itr->second->durationSecond, dateW);
					WtoA(dateW, date);
					WtoA(itr->second->stationName, station);
					WtoA(itr->second->title, title);
					Format(item, "%s<TD>%s</TD><TD>%s</TD><TD ALIGN=right ROWSPAN=\"2\"><A HREF=\"reserveinfo.html?id=%d\">詳細</A></TD></TR>\r\n", tr.c_str(), date.c_str(), station.c_str(), itr->second->reserveID);
					html+=item;
					Format(item, "%s<TD COLSPAN=\"2\">%s</TD></TR>\r\n", tr.c_str(), title.c_str());
					html+=item;
					//html+="</TABLE>\r\n</TD></TR>\r\n";
					itr++;
				}
			}
			html+="</TABLE>\r\n<BR><BR>\r\n";
		}
	}
	html+= "<A HREF=\"index.html\">メニューへ</A><BR>\r\n";

	html+=HTML_END;
	sendParam->dataSize = (DWORD)html.size();
	sendParam->data = new BYTE[sendParam->dataSize];
	memcpy(sendParam->data, html.c_str(), sendParam->dataSize);
	if( sendParam->dataSize > 0 ){
		Format(sendParam->httpHeader, "HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: %d\r\nConnection: close\r\n\r\n", sendParam->dataSize);
	}else{
		sendParam->httpHeader = "HTTP/1.0 400 Bad Request\r\nConnection: close\r\n\r\n";
	}
	return TRUE;
}

BOOL CHTMLManager::GetReserveInfoPage(RESERVE_DATA* reserveData, wstring eventText, WORD preset, vector<TUNER_RESERVE_INFO>* tunerList, HTTP_STREAM* sendParam)
{
	if( sendParam == NULL ){
		return FALSE;
	}

	wstring iniPath = L"";
	GetModuleIniPath(iniPath);
	int pageCount = GetPrivateProfileInt(L"HTTP", L"ReservePageCount", 30, iniPath.c_str());
	if( pageCount == 0 ){
		pageCount = 30;
	}

	string html = HTML_TOP;
	html+="予約詳細<BR><BR>\r\n";

	if(reserveData == NULL ){
		html = "";
	}else{
		html+="<HR>番組情報<HR>\r\n";
		string eventInfo = "";
		WtoA(eventText, eventInfo);
		Replace(eventInfo, "\r\n","<BR>\r\n");
		html+=eventInfo;
		html+="<HR>録画設定<HR>\r\n";

		string buff = "";
		//プリセット
		Format(buff, "<form method=\"POST\" action=\"reserveinfo.html?id=%d\">\r\n", reserveData->reserveID);
		html+=buff;
		html+="プリセット:\r\n<select name=\"preset\">\r\n";
		html+="<option value=\"0\">デフォルト\r\n";

		wstring iniPath = L"";
		GetModuleIniPath(iniPath);
		WCHAR iniBuff[512]=L"";
		GetPrivateProfileString(L"SET", L"PresetID", L"", iniBuff, 512, iniPath.c_str());
		wstring parseBuff = iniBuff;
		vector<DWORD> idList;
		do{
			wstring presetID =L"";
			Separate(parseBuff, L",", presetID, parseBuff);
			idList.push_back((DWORD)_wtoi(presetID.c_str()));
		}while(parseBuff.size()>0);
		for( size_t i=0; i<idList.size(); i++ ){
			wstring appName = L"";
			Format(appName, L"REC_DEF%d", idList[i]);
			ZeroMemory(iniBuff, sizeof(WCHAR)*512);
			GetPrivateProfileString(appName.c_str(), L"SetName", L"", iniBuff, 512, iniPath.c_str());
			string item = "";
			string name = "";
			WtoA(iniBuff, name);
			if( idList[i]==preset ){
				Format(item, "<option value=\"%d\" selected>%s\r\n", idList[i], name.c_str());
			}else{
				Format(item, "<option value=\"%d\">%s\r\n", idList[i], name.c_str());
			}
			html+=item;
		}

		if( preset == 0xFFFF ){
			html+="<option value=\"65535\" selected>予約時\r\n";
		}else{
			html+="<option value=\"65535\">予約時\r\n";
		}
		html+="</select>\r\n<input type=submit value=\"load\">\r\n</form>\r\n";

		//録画設定
		Format(buff, "<form method=\"POST\" action=\"reservechg.html?id=%d\">\r\n", reserveData->reserveID);
		html+=buff;
		Format(buff, "<input type=hidden name=\"presetID\" value=\"%d\">\r\n", preset);
		html+=buff;

		string paramText = "";
		if( preset != 0xFFFF ){
			REC_SETTING_DATA recSetData;
			LoadRecSetData(preset, &recSetData);
			CreateRecSetForm(&recSetData, tunerList, paramText);
		}else{
			CreateRecSetForm(&reserveData->recSetting, tunerList, paramText);
		}
		html+=paramText;

		html+="<input type=submit value=\"変更\">\r\n</form>\r\n";

		//削除
		Format(buff, "<form method=\"POST\" action=\"reservedel.html?id=%d\">\r\n", reserveData->reserveID);
		html+=buff;
		html+="<input type=submit value=\"削除\">\r\n</form>\r\n";
	}
	html+= "<A HREF=\"reserve.html\">予約一覧へ</A><BR>\r\n";

	html+=HTML_END;
	sendParam->dataSize = (DWORD)html.size();
	sendParam->data = new BYTE[sendParam->dataSize];
	memcpy(sendParam->data, html.c_str(), sendParam->dataSize);
	if( sendParam->dataSize > 0 ){
		Format(sendParam->httpHeader, "HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: %d\r\nConnection: close\r\n\r\n", sendParam->dataSize);
	}else{
		sendParam->httpHeader = "HTTP/1.0 400 Bad Request\r\nConnection: close\r\n\r\n";
	}
	return TRUE;
}

BOOL CHTMLManager::GetReserveParam(RESERVE_DATA* reserveData, HTTP_STREAM* recvParam)
{
	string param = "";
	param.append((char*)recvParam->data, 0, recvParam->dataSize);
	map<string,string> paramMap;
	while(param.size()>0){
		string buff;
		Separate(param, "&", buff, param);
		if(buff.size()>0){
			string key;
			string val;
			Separate(buff, "=", key, val);
			paramMap.insert(pair<string,string>(key, val));
		}
	}
	map<string,string>::iterator itr;
	itr = paramMap.find("presetID");
	if( itr == paramMap.end() ){
		return FALSE;
	}
	WORD presetID = (WORD)atoi(itr->second.c_str());
	if( presetID != 0xFFFF ){
		reserveData->recSetting.recFolderList.clear();
		reserveData->recSetting.partialRecFolder.clear();
		LoadRecSetData(presetID, &reserveData->recSetting);
	}

	itr = paramMap.find("recMode");
	if( itr == paramMap.end() ){
		return FALSE;
	}
	reserveData->recSetting.recMode = (BYTE)atoi(itr->second.c_str());

	if(reserveData->eventID != 0xFFFF){
		itr = paramMap.find("tuijyuuFlag");
		if( itr == paramMap.end() ){
			return FALSE;
		}
		reserveData->recSetting.tuijyuuFlag = (BYTE)atoi(itr->second.c_str());
	}else{
		reserveData->recSetting.tuijyuuFlag = 0;
	}

	itr = paramMap.find("priority");
	if( itr == paramMap.end() ){
		return FALSE;
	}
	reserveData->recSetting.priority = (BYTE)atoi(itr->second.c_str());

	if(reserveData->eventID != 0xFFFF){
		itr = paramMap.find("pittariFlag");
		if( itr == paramMap.end() ){
			return FALSE;
		}
		reserveData->recSetting.pittariFlag = (BYTE)atoi(itr->second.c_str());
	}else{
		reserveData->recSetting.pittariFlag = 0;
	}

	itr = paramMap.find("suspendMode");
	if( itr == paramMap.end() ){
		return FALSE;
	}
	reserveData->recSetting.suspendMode = (BYTE)atoi(itr->second.c_str());
	
	if( reserveData->recSetting.suspendMode == 1 || reserveData->recSetting.suspendMode == 2 ){
		itr = paramMap.find("rebootFlag");
		if( itr == paramMap.end() ){
			reserveData->recSetting.rebootFlag = 0;
		}else{
			reserveData->recSetting.rebootFlag = 1;
		}
	}else{
		reserveData->recSetting.rebootFlag = 0;
	}

	itr = paramMap.find("useDefMargineFlag");
	if( itr == paramMap.end() ){
		reserveData->recSetting.useMargineFlag = 1;
		itr = paramMap.find("startMargine");
		if( itr == paramMap.end() ){
			return FALSE;
		}
		reserveData->recSetting.startMargine = atoi(itr->second.c_str());
		itr = paramMap.find("endMargine");
		if( itr == paramMap.end() ){
			return FALSE;
		}
		reserveData->recSetting.endMargine = atoi(itr->second.c_str());
	}else{
		reserveData->recSetting.useMargineFlag = 0;
		reserveData->recSetting.startMargine = 0;
		reserveData->recSetting.endMargine = 0;
	}

	itr = paramMap.find("serviceMode");
	if( itr != paramMap.end() ){
		reserveData->recSetting.serviceMode = 0;
	}else{
		reserveData->recSetting.serviceMode = 1;
		itr = paramMap.find("serviceMode_1");
		if( itr != paramMap.end() ){
			reserveData->recSetting.serviceMode |= 0x10;
		}
		itr = paramMap.find("serviceMode_2");
		if( itr != paramMap.end() ){
			reserveData->recSetting.serviceMode |= 0x20;
		}
	}

	itr = paramMap.find("continueRecFlag");
	if( itr == paramMap.end() ){
		reserveData->recSetting.continueRecFlag = 0;
	}else{
		reserveData->recSetting.continueRecFlag = 1;
	}
	
	itr = paramMap.find("tunerID");
	if( itr == paramMap.end() ){
		return FALSE;
	}
	reserveData->recSetting.tunerID = (DWORD)atoi(itr->second.c_str());

	itr = paramMap.find("partialRecFlag");
	if( itr == paramMap.end() ){
		reserveData->recSetting.partialRecFlag = 0;
	}else{
		reserveData->recSetting.partialRecFlag = 1;
	}

	return TRUE;
}

BOOL CHTMLManager::GetReserveChgPage(HTTP_STREAM* sendParam, BOOL err)
{
	if( sendParam == NULL ){
		return FALSE;
	}
	string html = HTML_TOP;
	if( err == FALSE ){
		html+="予約を変更しました<BR><BR>\r\n<A HREF=\"reserve.html\">予約一覧</A>\r\n";
	}else{
		html+="入力値不正<BR><BR>\r\n<A HREF=\"reserve.html\">予約一覧</A>\r\n";
	}
	html+=HTML_END;
	sendParam->dataSize = (DWORD)html.size();
	sendParam->data = new BYTE[sendParam->dataSize];
	memcpy(sendParam->data, html.c_str(), sendParam->dataSize);
	if( sendParam->dataSize > 0 ){
		Format(sendParam->httpHeader, "HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: %d\r\nConnection: close\r\n\r\n", sendParam->dataSize);
	}else{
		sendParam->httpHeader = "HTTP/1.0 400 Bad Request\r\nConnection: close\r\n\r\n";
	}
	return TRUE;
}

BOOL CHTMLManager::GetReserveDelPage(HTTP_STREAM* sendParam, BOOL err)
{
	if( sendParam == NULL ){
		return FALSE;
	}
	string html = HTML_TOP;
	if( err == FALSE ){
		html+="予約を削除しました<BR><BR>\r\n<A HREF=\"reserve.html\">予約一覧</A>\r\n";
	}else{
		html+="入力値不正<BR><BR>\r\n<A HREF=\"reserve.html\">予約一覧</A>\r\n";
	}
	html+=HTML_END;
	sendParam->dataSize = (DWORD)html.size();
	sendParam->data = new BYTE[sendParam->dataSize];
	memcpy(sendParam->data, html.c_str(), sendParam->dataSize);
	if( sendParam->dataSize > 0 ){
		Format(sendParam->httpHeader, "HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: %d\r\nConnection: close\r\n\r\n", sendParam->dataSize);
	}else{
		sendParam->httpHeader = "HTTP/1.0 400 Bad Request\r\nConnection: close\r\n\r\n";
	}
	return TRUE;
}

BOOL CHTMLManager::GetRecInfoPage(vector<REC_FILE_INFO>* list, int pageIndex, HTTP_STREAM* sendParam)
{
	if( sendParam == NULL ){
		return FALSE;
	}

	wstring iniPath = L"";
	GetModuleIniPath(iniPath);
	int pageCount = GetPrivateProfileInt(L"HTTP", L"RecInfoPageCount", 30, iniPath.c_str());
	if( pageCount == 0 ){
		pageCount = 30;
	}

	string html = HTML_TOP;
	html+="録画済み一覧<BR><BR>\r\n";

	if(list == NULL ){
		html+="録画済みなし<BR>\r\n";
	}else{
		if( list->size() == 0 || (size_t)pageIndex*pageCount > list->size()){
			html+="録画済みなし<BR>\r\n";
		}else{
			//ページリンクの作成
			int pageNum = (int)list->size()/pageCount;
			if( (int)list->size()%pageCount > 0 ){
				pageNum++;
			}
			for( int i=0; i<pageNum; i++ ){
				string ref = "";
				if( i != pageIndex){
					Format(ref, "<A HREF=\"recinfo.html?page=%d\">%d～</A> \r\n", i, i*pageCount+1);
				}else{
					Format(ref, "%d～ \r\n", i*pageCount+1);
				}
				html+=ref;
			}
			html+="<BR><BR>\r\n";

			//時間でソート
			multimap<__int64, REC_FILE_INFO, std::greater<__int64>> sortMap;
			for( size_t i=0; i<list->size(); i++ ){
				__int64 stratTime = ConvertI64Time((*list)[i].startTime);
				sortMap.insert(pair<__int64, REC_FILE_INFO>(stratTime, (*list)[i]));
			}
			//一覧の作成
			html+="<TABLE BORDER=\"1\">\r\n";

			multimap<__int64, REC_FILE_INFO, std::greater<__int64>>::iterator itr;
			itr = sortMap.begin();
			advance(itr, pageIndex*pageCount);
			for( int i=0; i<pageCount; i++){
				if( itr != sortMap.end() ){
					//html+="<TR><TD>\r\n";
					string tr = "<TR>";
					if( itr->second.drops>0 ){
						tr = "<TR BGCOLOR=#FF0000>";
					}else if(itr->second.scrambles>0 ){
						tr = "<TR BGCOLOR=#FFFF00>";
					}
					string item = "";
					wstring dateW = L"";
					string date = "";
					string station = "";
					string title = "";
					string comment = "";
					GetTimeString3(itr->second.startTime, itr->second.durationSecond, dateW);
					WtoA(dateW, date);
					WtoA(itr->second.serviceName, station);
					WtoA(itr->second.title, title);
					WtoA(itr->second.comment, comment);
					Format(item, "%s<TD>%s</TD><TD>%s</TD><TD ALIGN=right ROWSPAN=\"3\"><A HREF=\"recinfodesc.html?id=%d\">詳細</A></TD></TR>\r\n", tr.c_str(), date.c_str(), station.c_str(), itr->second.id);
					html+=item;
					Format(item, "%s<TD COLSPAN=\"2\">%s</TD></TR>\r\n", tr.c_str(), title.c_str());
					html+=item;
					Format(item, "%s<TD COLSPAN=\"2\">%s</TD></TR>\r\n", tr.c_str(), comment.c_str());
					html+=item;
					//html+="</TABLE>\r\n</TD></TR>\r\n";
					itr++;
				}
			}
			html+="</TABLE>\r\n<BR><BR>\r\n";
		}
	}
	html+= "<A HREF=\"index.html\">メニューへ</A><BR>\r\n";

	html+=HTML_END;
	sendParam->dataSize = (DWORD)html.size();
	sendParam->data = new BYTE[sendParam->dataSize];
	memcpy(sendParam->data, html.c_str(), sendParam->dataSize);
	if( sendParam->dataSize > 0 ){
		Format(sendParam->httpHeader, "HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: %d\r\nConnection: close\r\n\r\n", sendParam->dataSize);
	}else{
		sendParam->httpHeader = "HTTP/1.0 400 Bad Request\r\nConnection: close\r\n\r\n";
	}
	return TRUE;
}

BOOL CHTMLManager::GetRecInfoDescPage(REC_FILE_INFO* recinfoData, HTTP_STREAM* sendParam)
{
	if( sendParam == NULL ){
		return FALSE;
	}

	string html = HTML_TOP;
	html+="録画済み詳細<BR><BR>\r\n";

	if(recinfoData == NULL ){
		html = "";
	}else{
		string buff = "";

		wstring dateW = L"";
		string date = "";
		string station = "";
		string title = "";
		GetTimeString3(recinfoData->startTime, recinfoData->durationSecond, dateW);
		WtoA(dateW, date);
		WtoA(recinfoData->serviceName, station);
		WtoA(recinfoData->title, title);

		Format(buff, "%s %s<BR>\r\n%s<BR><BR>\r\n", date.c_str(), station.c_str(), title.c_str());
		html+=buff;

		WtoA(recinfoData->comment, buff);
		html+="結果：";
		html+=buff;
		html+="<BR>\r\n";
		Format(buff, "ドロップ：%I64d<BR>\r\nスクランブル：%I64d<BR>\r\n", recinfoData->drops, recinfoData->scrambles);
		html+=buff;
		html+="<BR>\r\n";

		html+="<HR>番組情報<HR>\r\n";
		WtoA(recinfoData->programInfo, buff);
		Replace(buff, "\r\n","<BR>\r\n");
		html+=buff;
		html+="<HR>エラーログ<HR>\r\n<PRE>\r\n";
		WtoA(recinfoData->errInfo, buff);
		html+=buff;
		html+="</PRE>\r\n";

		//削除
		Format(buff, "<form method=\"POST\" action=\"recinfodel.html?id=%d\">\r\n", recinfoData->id);
		html+=buff;
		html+="<input type=submit value=\"削除\">\r\n</form>\r\n";
	}
	html+= "<A HREF=\"recinfo.html\">録画済み一覧へ</A><BR>\r\n";

	html+=HTML_END;
	sendParam->dataSize = (DWORD)html.size();
	sendParam->data = new BYTE[sendParam->dataSize];
	memcpy(sendParam->data, html.c_str(), sendParam->dataSize);
	if( sendParam->dataSize > 0 ){
		Format(sendParam->httpHeader, "HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: %d\r\nConnection: close\r\n\r\n", sendParam->dataSize);
	}else{
		sendParam->httpHeader = "HTTP/1.0 400 Bad Request\r\nConnection: close\r\n\r\n";
	}
	return TRUE;
}

BOOL CHTMLManager::GetRecInfoDelPage(HTTP_STREAM* sendParam, BOOL err)
{
	if( sendParam == NULL ){
		return FALSE;
	}
	string html = HTML_TOP;
	if( err == FALSE ){
		html+="録画情報を削除しました<BR><BR>\r\n<A HREF=\"recinfo.html\">録画済み一覧</A>\r\n";
	}else{
		html+="入力値不正<BR><BR>\r\n<A HREF=\"recinfo.html\">録画済み一覧</A>\r\n";
	}
	html+=HTML_END;
	sendParam->dataSize = (DWORD)html.size();
	sendParam->data = new BYTE[sendParam->dataSize];
	memcpy(sendParam->data, html.c_str(), sendParam->dataSize);
	if( sendParam->dataSize > 0 ){
		Format(sendParam->httpHeader, "HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: %d\r\nConnection: close\r\n\r\n", sendParam->dataSize);
	}else{
		sendParam->httpHeader = "HTTP/1.0 400 Bad Request\r\nConnection: close\r\n\r\n";
	}
	return TRUE;
}

BOOL CHTMLManager::GetEpgPage(CEpgDBManager* epgDB, vector<RESERVE_DATA*>* reserveList, string url, HTTP_STREAM* sendParam)
{
	if( sendParam == NULL || epgDB == NULL){
		return FALSE;
	}
	string param = "";
	Separate(url, "?", url, param);
	map<string,string> paramMap;
	while(param.size()>0){
		string buff;
		Separate(param, "&", buff, param);
		if(buff.size()>0){
			string key;
			string val;
			Separate(buff, "=", key, val);
			paramMap.insert(pair<string,string>(key, val));
		}
	}
	map<string,string>::iterator itr;
	int tab = 0;
	itr = paramMap.find("tab");
	if( itr != paramMap.end() ){
		tab = atoi(itr->second.c_str());
	}
	int page = 0;
	itr = paramMap.find("page");
	if( itr != paramMap.end() ){
		page = atoi(itr->second.c_str());
	}
	int date = 0;
	itr = paramMap.find("date");
	if( itr != paramMap.end() ){
		date = atoi(itr->second.c_str());
	}

	wstring iniPath = L"";
	GetModuleIniPath(iniPath);
	BOOL custPage = (BOOL)GetPrivateProfileInt(L"HTTP", L"HttpCustEpg", 0, iniPath.c_str());

	string htmlText = "";
	if( custPage == FALSE ){
		if( CreateDefEpgPage(epgDB, reserveList, tab, page, date, htmlText) == FALSE ){
			GetEpgErrPage(sendParam);
			return TRUE;
		}
	}else{
		if( CreateCustEpgPage(epgDB, reserveList, tab, page, date, htmlText) == FALSE ){
			GetEpgErrPage(sendParam);
			return TRUE;
		}
	}

	string html = HTML_TOP;
	html+=htmlText;
	html+=HTML_END;
	sendParam->dataSize = (DWORD)html.size();
	sendParam->data = new BYTE[sendParam->dataSize];
	memcpy(sendParam->data, html.c_str(), sendParam->dataSize);
	if( sendParam->dataSize > 0 ){
		Format(sendParam->httpHeader, "HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: %d\r\nConnection: close\r\n\r\n", sendParam->dataSize);
	}else{
		sendParam->httpHeader = "HTTP/1.0 400 Bad Request\r\nConnection: close\r\n\r\n";
	}
	return TRUE;
}

BOOL CHTMLManager::GetEpgErrPage(HTTP_STREAM* sendParam)
{
	if( sendParam == NULL ){
		return FALSE;
	}
	string html = HTML_TOP;
	html+="EPGデータが読み込み中、または存在しません。<BR><BR>\r\n<A HREF=\"index.html\">メニュー</A>\r\n";
	html+=HTML_END;
	sendParam->dataSize = (DWORD)html.size();
	sendParam->data = new BYTE[sendParam->dataSize];
	memcpy(sendParam->data, html.c_str(), sendParam->dataSize);
	if( sendParam->dataSize > 0 ){
		Format(sendParam->httpHeader, "HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: %d\r\nConnection: close\r\n\r\n", sendParam->dataSize);
	}else{
		sendParam->httpHeader = "HTTP/1.0 400 Bad Request\r\nConnection: close\r\n\r\n";
	}
	return TRUE;
}

BOOL CHTMLManager::CreateDefEpgPage(CEpgDBManager* epgDB, vector<RESERVE_DATA*>* reserveList, int tab, int page, int date, string& htmlText)
{
	map<LONGLONG, RESERVE_DATA*> reserveMap;
	for( size_t i=0; i<(*reserveList).size(); i++ ){
		if( (*reserveList)[i]->eventID != 0xFFFF ){
			LONGLONG key = _Create64Key2(
				(*reserveList)[i]->originalNetworkID, 
				(*reserveList)[i]->transportStreamID, 
				(*reserveList)[i]->serviceID, 
				(*reserveList)[i]->eventID
				);
			reserveMap.insert(pair<LONGLONG, RESERVE_DATA*>(key, (*reserveList)[i]));
		}
	}

	string buff;
	string tabPageLink;
	Format(tabPageLink, "<A HREF=\"epg.html?tab=0&page=%d&date=%d\">BS</A> <A HREF=\"epg.html?tab=1&page=%d&date=%d\">CS</A> <A HREF=\"epg.html?tab=2&page=%d&date=%d\">地デジ</A> <A HREF=\"epg.html?tab=3&page=%d&date=%d\">その他</A><BR><BR>\r\n",
		0, date,
		0, date,
		0, date,
		0, date
		);
	htmlText+=tabPageLink;

	wstring iniPath = L"";
	GetModuleIniPath(iniPath);
	int pageColumn = GetPrivateProfileInt(L"HTTP", L"HttpEpgPageColumn", 6, iniPath.c_str());
	int minPx = GetPrivateProfileInt(L"HTTP", L"HttpEpg1minPx", 2, iniPath.c_str());
	map<int,string> colorList;
	for( int i=0; i<20; i++ ){
		wstring key;
		Format(key, L"BackColor%d", i);
		WCHAR stringBuff[256]=L"";
		GetPrivateProfileString(L"HTTP", key.c_str(), L"FFFFFF", stringBuff, 255, iniPath.c_str());
		WtoA(stringBuff, buff);

		colorList.insert(pair<int,string>(i, buff));
	}

	vector<EPGDB_SERVICE_INFO> serviceList;
	epgDB->GetServiceList(&serviceList);

	int findCount = 0;
	vector<EPGDB_SERVICE_INFO> useServiceList;
	for( size_t i=0; i<serviceList.size(); i++ ){
		if( serviceList[i].ONID == 0x0004 ){
			if(tab == 0){
				if( serviceList[i].service_type == 0x01 || serviceList[i].service_type == 0xA5 ){
					if( page*pageColumn<=findCount && findCount <(page+1)*pageColumn ){
						useServiceList.push_back(serviceList[i]);
					}
					findCount++;
				}
			}
		}else if( serviceList[i].ONID == 0x0006 || serviceList[i].ONID == 0x0007){
			if( tab == 1 ){
				if( serviceList[i].service_type == 0x01 || serviceList[i].service_type == 0xA5 ){
					if( page*pageColumn<=findCount && findCount <(page+1)*pageColumn ){
						useServiceList.push_back(serviceList[i]);
					}
					findCount++;
				}
			}
		}else if( 0x7880 <= serviceList[i].ONID && serviceList[i].ONID <= 0x7FE8){
			if( tab == 2 ){
				if( serviceList[i].service_type == 0x01 || serviceList[i].service_type == 0xA5 ){
					if( page*pageColumn<=findCount && findCount <(page+1)*pageColumn ){
						useServiceList.push_back(serviceList[i]);
					}
					findCount++;
				}
			}
		}else{
			if( tab == 3 ){
				if( serviceList[i].service_type == 0x01 || serviceList[i].service_type == 0xA5 ){
					if( page*pageColumn<=findCount && findCount <(page+1)*pageColumn ){
						useServiceList.push_back(serviceList[i]);
					}
					findCount++;
				}
			}
		}
	}

	//ページリンクの作成
	int pageNum = findCount/pageColumn;
	if( findCount%pageColumn > 0 ){
		pageNum++;
	}
	string pageLinkHtml = "";
	for( int i=0; i<pageNum; i++ ){
		string ref = "";
		if( i==page ){
			Format(ref, "%d \r\n", i);
		}else{
			Format(ref, "<A HREF=\"epg.html?tab=%d&page=%d&date=%d\">%d</A> \r\n", tab, i, date, i);
		}
		pageLinkHtml+=ref;
	}
	htmlText+=pageLinkHtml;
	htmlText+="<BR><BR>\r\n";

	if( useServiceList.size() == 0 ){
		htmlText+="<A HREF=\"index.html\">メニュー</A>\r\n";
		return TRUE;
	}

	//日時のチェック
	LONGLONG startTime = 0;
	LONGLONG endTime = 0;
	for( size_t i=0; i<useServiceList.size(); i++ ){
		vector<EPGDB_EVENT_INFO*> eventList;
		LONGLONG key = _Create64Key(useServiceList[i].ONID, useServiceList[i].TSID, useServiceList[i].SID); 
		epgDB->EnumEventInfo(key, &eventList);
		for(size_t j=0; j<eventList.size(); j++){
			LONGLONG stime = ConvertI64Time(eventList[j]->start_time);
			LONGLONG etime = GetSumTime(eventList[j]->start_time, eventList[j]->durationSec);
			if( startTime == 0 ){
				startTime = stime;
			}else if(stime<startTime){
				startTime = stime;
			}
			if( endTime == 0 ){
				endTime = etime;
			}else if(endTime<etime){
				endTime = etime;
			}
		}
	}
	if( startTime == 0 || endTime == 0 ){
		htmlText+="<A HREF=\"index.html\">メニュー</A>\r\n";
		return TRUE;
	}
	startTime -= 4*60*60*I64_1SEC;
	endTime -= 4*60*60*I64_1SEC;
	int dateCount=0;
	string dateLinkHtml = "";
	for(__int64 i=startTime; i<=endTime; i+=24*60*60*I64_1SEC){
		SYSTEMTIME dayTime;
		ConvertSystemTime( i, &dayTime );
		wstring week;
		GetDayOfWeekString(dayTime, week);
		string weekA;
		WtoA(week, weekA);
		if( dateCount == date ){
			Format(buff, "%d/%d%s \r\n", dayTime.wMonth, dayTime.wDay, weekA.c_str());
		}else{
			Format(buff, "<A HREF=\"epg.html?tab=%d&page=%d&date=%d\">%d/%d%s</A> \r\n", tab, page, dateCount, dayTime.wMonth, dayTime.wDay, weekA.c_str());
		}
		dateLinkHtml+=buff;
		dateCount++;
	}
	htmlText+=dateLinkHtml;
	htmlText+="<BR><BR>\r\n";

	//番組表本体
	htmlText+="<TABLE cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n";
	htmlText+="<TR><TD height=\"20px\" width=\"30px\" nowrap></TD>\r\n";
	//サービス
	for( size_t i=0; i<useServiceList.size(); i++ ){
		string service;
		WtoA(useServiceList[i].service_name, service);
		Format(buff, "<TD height=\"20px\" width=\"150px\" nowrap>%s</TD>\r\n", service.c_str());
		htmlText+=buff;
	}
	htmlText+="</TR>\r\n";

	//時間
	htmlText+="<TR><TD valign=\"top\" nowrap>";
	htmlText+="<TABLE cellpadding=\"0\" cellspacing=\"0\" border=\"1\">\r\n";
	for( int i=4; i<28; i++ ){
		Format(buff, "<TR><TD height=\"%dpx\" width=\"28px\" nowrap>%d</TD></TR>\r\n", minPx*60, i);
		htmlText+=buff;
	}
	htmlText+="</TABLE>\r\n";
	htmlText+="</TD>\r\n";

	//番組
	SYSTEMTIME chkTime;
	ConvertSystemTime( startTime, &chkTime );
	chkTime.wHour = 4;
	chkTime.wMinute = 0;
	chkTime.wSecond = 0;
	chkTime.wMilliseconds = 0;
	LONGLONG chkStartTime = ConvertI64Time(chkTime) + (date*24*60*60*I64_1SEC);
	LONGLONG chkEndTime = chkStartTime + 24*60*60*I64_1SEC;
	for( size_t i=0; i<useServiceList.size(); i++ ){
		//必要な物抽出
		vector<EPGDB_EVENT_INFO*> eventList;
		LONGLONG key = _Create64Key(useServiceList[i].ONID, useServiceList[i].TSID, useServiceList[i].SID); 
		epgDB->EnumEventInfo(key, &eventList);
		map<LONGLONG, EPGDB_EVENT_INFO*> sortMap;
		for(size_t j=0; j<eventList.size(); j++){
			LONGLONG chk = ConvertI64Time(eventList[j]->start_time);
			if( chkStartTime <= chk && chk < chkEndTime ){
				sortMap.insert(pair<LONGLONG, EPGDB_EVENT_INFO*>(chk, eventList[j]));
			}
		}
		//HTML作成
		htmlText+="<TD valign=\"top\" rowspan=\"24\">\r\n";
		htmlText+="<TABLE cellpadding=\"0\" cellspacing=\"0\" border=\"1\" style=\"table-layout:fixed;\">\r\n";
		map<LONGLONG, EPGDB_EVENT_INFO*>::iterator itr;
		LONGLONG lastEnd = chkStartTime;
		for( itr = sortMap.begin(); itr != sortMap.end(); itr++ ){
			LONGLONG evStartTime = ConvertI64Time(itr->second->start_time);
			LONGLONG evEndTime = GetSumTime(itr->second->start_time, itr->second->durationSec);
			if( lastEnd < evStartTime ){
				//空白挿入
				int duration = (int)((evStartTime-lastEnd)/(60*I64_1SEC));
				Format(buff,"<tr><td valign=\"top\" height=\"%dpx\" width=\"148px\" bgcolor=#FFFFFF><font size=\"2\"><DIV style=\"height:%dpx; overflow:auto;\"> </DIV></font></td></tr>\r\n", duration*minPx, duration*minPx);
				htmlText+=buff;
				lastEnd = evEndTime;
			}
			//イベント挿入
			int duration = itr->second->durationSec/60;
			if( chkEndTime < evEndTime){
				duration = (int)((chkEndTime-evStartTime)/(60*I64_1SEC));
			}
			string title;
			string color = "FFFFFF";
			if( itr->second->shortInfo != NULL ){
				WtoA(itr->second->shortInfo->event_name, title);
			}
			if( itr->second->contentInfo != NULL ){
				if( itr->second->contentInfo->nibbleList.size() > 0 ){
					map<int,string>::iterator itrColor;
					itrColor = colorList.find(itr->second->contentInfo->nibbleList[0].content_nibble_level_1);
					if( itrColor != colorList.end() ){
						color = itrColor->second;
					}
				}
			}
			map<LONGLONG, RESERVE_DATA*>::iterator itrReserve;
			LONGLONG key = _Create64Key2(
				itr->second->original_network_id,
				itr->second->transport_stream_id,
				itr->second->service_id,
				itr->second->event_id
				);
			itrReserve = reserveMap.find(key);
			if( itrReserve != reserveMap.end() ){
				Format(buff,"<tr><td valign=\"top\" height=\"%dpx\" width=\"148px\" bgcolor=#%s><font size=\"2\"><DIV style=\"height:%dpx; overflow:auto;\"><font color=#FF8000><B>予</B></font> %02d <a href=\"epginfo.html?onid=%d&tsid=%d&sid=%d&evid=%d&preset=65535\">%s</a></DIV></font></td></tr>\r\n",
					duration*minPx,
					color.c_str(),
					duration*minPx,
					itr->second->start_time.wMinute,
					itr->second->original_network_id,
					itr->second->transport_stream_id,
					itr->second->service_id,
					itr->second->event_id,
					title.c_str()
					);
			}else{
				Format(buff,"<tr><td valign=\"top\" height=\"%dpx\" width=\"148px\" bgcolor=#%s><font size=\"2\"><DIV style=\"height:%dpx; overflow:auto;\">%02d <a href=\"epginfo.html?onid=%d&tsid=%d&sid=%d&evid=%d&preset=0\">%s</a></DIV></font></td></tr>\r\n",
					duration*minPx,
					color.c_str(),
					duration*minPx,
					itr->second->start_time.wMinute,
					itr->second->original_network_id,
					itr->second->transport_stream_id,
					itr->second->service_id,
					itr->second->event_id,
					title.c_str()
					);
			}
			htmlText+=buff;
			lastEnd = evEndTime;
		}
		htmlText+="</TABLE>\r\n";
		htmlText+="</TD>\r\n";
	}

	htmlText+="</TR>\r\n";
	htmlText+="</TABLE><BR>\r\n";

	htmlText+=dateLinkHtml;
	htmlText+="<BR><BR>\r\n";
	htmlText+=pageLinkHtml;
	htmlText+="<BR><BR>\r\n";
	htmlText+=tabPageLink;

	htmlText+="<A HREF=\"index.html\">メニュー</A>\r\n";
	return TRUE;
}

BOOL CHTMLManager::CreateCustEpgPage(CEpgDBManager* epgDB, vector<RESERVE_DATA*>* reserveList, int tab, int page, int date, string& htmlText)
{
	map<LONGLONG, RESERVE_DATA*> reserveMap;
	for( size_t i=0; i<(*reserveList).size(); i++ ){
		if( (*reserveList)[i]->eventID != 0xFFFF ){
			LONGLONG key = _Create64Key2(
				(*reserveList)[i]->originalNetworkID, 
				(*reserveList)[i]->transportStreamID, 
				(*reserveList)[i]->serviceID, 
				(*reserveList)[i]->eventID
				);
			reserveMap.insert(pair<LONGLONG, RESERVE_DATA*>(key, (*reserveList)[i]));
		}
	}

	string buff;
	wstring iniPath = L"";
	GetModuleIniPath(iniPath);
	
	int pageColumn = GetPrivateProfileInt(L"HTTP", L"HttpEpgPageColumn", 6, iniPath.c_str());
	int minPx = GetPrivateProfileInt(L"HTTP", L"HttpEpg1minPx", 2, iniPath.c_str());
	map<int,string> colorList;
	for( int i=0; i<20; i++ ){
		wstring key;
		Format(key, L"BackColor%d", i);
		WCHAR stringBuff[256]=L"";
		GetPrivateProfileString(L"HTTP", key.c_str(), L"FFFFFF", stringBuff, 255, iniPath.c_str());
		WtoA(stringBuff, buff);

		colorList.insert(pair<int,string>(i, buff));
	}

	vector<LONGLONG> viewServiceList;
	int viewCount = 0;
	//タブ情報
	string tabPageLink;
	int tabNum = GetPrivateProfileInt(L"HTTP", L"HttpCustCount", 0, iniPath.c_str());
	if( tabNum == 0 ){
		htmlText+="<A HREF=\"index.html\">メニュー</A>\r\n";
		return TRUE;
	}
	for( int i=0; i<tabNum; i++ ){
		wstring key;
		Format(key, L"HTTP_CUST%d", i);
		WCHAR wBuff[512]=L"";
		GetPrivateProfileString(key.c_str(), L"Name", L"", wBuff, 511, iniPath.c_str());
		string name;
		WtoA(wBuff, name);
		Format(buff, "<A HREF=\"epg.html?tab=%d&page=0&date=%d\">%s</A> ", 
			i, date, name.c_str());
		tabPageLink += buff;

		//表示サービス
		if( i== tab ){
			viewCount = GetPrivateProfileInt(key.c_str(), L"ViewServiceCount", 0, iniPath.c_str());
			for( int j=0; j<viewCount; j++ ){
				if( page*pageColumn<=j && j <(page+1)*pageColumn ){
					wstring key2;
					Format(key2, L"ViewService%d", j);
					WCHAR wBuff[512]=L"";
					GetPrivateProfileString(key.c_str(), key2.c_str(), L"", wBuff, 511, iniPath.c_str());
					__int64 i64Ch=_wtoi64(wBuff);

					viewServiceList.push_back(i64Ch);
				}
			}
		}
	}
	tabPageLink += "<BR><BR>\r\n";
	htmlText+=tabPageLink;


	//ページリンクの作成
	int pageNum = viewCount/pageColumn;
	if( viewCount%pageColumn > 0 ){
		pageNum++;
	}
	string pageLinkHtml = "";
	for( int i=0; i<pageNum; i++ ){
		string ref = "";
		if( i==page ){
			Format(ref, "%d \r\n", i);
		}else{
			Format(ref, "<A HREF=\"epg.html?tab=%d&page=%d&date=%d\">%d</A> \r\n", tab, i, date, i);
		}
		pageLinkHtml+=ref;
	}
	htmlText+=pageLinkHtml;
	htmlText+="<BR><BR>\r\n";

	if( viewServiceList.size() == 0 ){
		htmlText+="<A HREF=\"index.html\">メニュー</A>\r\n";
		return TRUE;
	}

	//日時のチェック
	LONGLONG startTime = 0;
	LONGLONG endTime = 0;
	for( size_t i=0; i<viewServiceList.size(); i++ ){
		vector<EPGDB_EVENT_INFO*> eventList;
		epgDB->EnumEventInfo(viewServiceList[i], &eventList);
		for(size_t j=0; j<eventList.size(); j++){
			LONGLONG stime = ConvertI64Time(eventList[j]->start_time);
			LONGLONG etime = GetSumTime(eventList[j]->start_time, eventList[j]->durationSec);
			if( startTime == 0 ){
				startTime = stime;
			}else if(stime<startTime){
				startTime = stime;
			}
			if( endTime == 0 ){
				endTime = etime;
			}else if(endTime<etime){
				endTime = etime;
			}
		}
	}
	if( startTime == 0 || endTime == 0 ){
		htmlText+="<A HREF=\"index.html\">メニュー</A>\r\n";
		return TRUE;
	}
	startTime -= 4*60*60*I64_1SEC;
	endTime -= 4*60*60*I64_1SEC;
	int dateCount=0;
	string dateLinkHtml = "";
	for(__int64 i=startTime; i<=endTime; i+=24*60*60*I64_1SEC){
		SYSTEMTIME dayTime;
		ConvertSystemTime( i, &dayTime );
		wstring week;
		GetDayOfWeekString(dayTime, week);
		string weekA;
		WtoA(week, weekA);
		if( dateCount == date ){
			Format(buff, "%d/%d%s \r\n", dayTime.wMonth, dayTime.wDay, weekA.c_str());
		}else{
			Format(buff, "<A HREF=\"epg.html?tab=%d&page=%d&date=%d\">%d/%d%s</A> \r\n", tab, page, dateCount, dayTime.wMonth, dayTime.wDay, weekA.c_str());
		}
		dateLinkHtml+=buff;
		dateCount++;
	}
	htmlText+=dateLinkHtml;
	htmlText+="<BR><BR>\r\n";

	//番組表本体
	htmlText+="<TABLE cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n";
	htmlText+="<TR><TD height=\"20px\" width=\"30px\" nowrap></TD>\r\n";
	//サービス
	for( size_t i=0; i<viewServiceList.size(); i++ ){
		string service;
		wstring serviceName;
		WORD onid = (WORD)(viewServiceList[i]>>32);
		WORD tsid = (WORD)((viewServiceList[i]>>16)&0x000000000000FFFF);
		WORD sid = (WORD)(viewServiceList[i]&0x000000000000FFFF);
		epgDB->SearchServiceName(onid, tsid, sid, serviceName);
		WtoA(serviceName, service);
		Format(buff, "<TD height=\"20px\" width=\"150px\" nowrap>%s</TD>\r\n", service.c_str());
		htmlText+=buff;
	}
	htmlText+="</TR>\r\n";

	//時間
	htmlText+="<TR><TD valign=\"top\" nowrap>";
	htmlText+="<TABLE cellpadding=\"0\" cellspacing=\"0\" border=\"1\">\r\n";
	for( int i=4; i<28; i++ ){
		Format(buff, "<TR><TD height=\"%dpx\" width=\"28px\" nowrap>%d</TD></TR>\r\n", minPx*60, i);
		htmlText+=buff;
	}
	htmlText+="</TABLE>\r\n";
	htmlText+="</TD>\r\n";

	//番組
	SYSTEMTIME chkTime;
	ConvertSystemTime( startTime, &chkTime );
	chkTime.wHour = 4;
	chkTime.wMinute = 0;
	chkTime.wSecond = 0;
	chkTime.wMilliseconds = 0;
	LONGLONG chkStartTime = ConvertI64Time(chkTime) + (date*24*60*60*I64_1SEC);
	LONGLONG chkEndTime = chkStartTime + 24*60*60*I64_1SEC;
	for( size_t i=0; i<viewServiceList.size(); i++ ){
		//必要な物抽出
		vector<EPGDB_EVENT_INFO*> eventList;
		epgDB->EnumEventInfo(viewServiceList[i], &eventList);
		map<LONGLONG, EPGDB_EVENT_INFO*> sortMap;
		for(size_t j=0; j<eventList.size(); j++){
			LONGLONG chk = ConvertI64Time(eventList[j]->start_time);
			if( chkStartTime <= chk && chk < chkEndTime ){
				sortMap.insert(pair<LONGLONG, EPGDB_EVENT_INFO*>(chk, eventList[j]));
			}
		}
		//HTML作成
		htmlText+="<TD valign=\"top\" rowspan=\"24\">\r\n";
		htmlText+="<TABLE cellpadding=\"0\" cellspacing=\"0\" border=\"1\" style=\"table-layout:fixed;\">\r\n";
		map<LONGLONG, EPGDB_EVENT_INFO*>::iterator itr;
		LONGLONG lastEnd = chkStartTime;
		for( itr = sortMap.begin(); itr != sortMap.end(); itr++ ){
			LONGLONG evStartTime = ConvertI64Time(itr->second->start_time);
			LONGLONG evEndTime = GetSumTime(itr->second->start_time, itr->second->durationSec);
			if( lastEnd < evStartTime ){
				//空白挿入
				int duration = (int)((evStartTime-lastEnd)/(60*I64_1SEC));
				Format(buff,"<tr><td valign=\"top\" height=\"%dpx\" width=\"148px\" bgcolor=#FFFFFF><font size=\"2\"><DIV style=\"height:%dpx; overflow:auto;\"> </DIV></font></td></tr>\r\n", duration*minPx, duration*minPx);
				htmlText+=buff;
				lastEnd = evEndTime;
			}
			//イベント挿入
			int duration = itr->second->durationSec/60;
			if( chkEndTime < evEndTime){
				duration = (int)((chkEndTime-evStartTime)/(60*I64_1SEC));
			}
			string title;
			string color = "FFFFFF";
			if( itr->second->shortInfo != NULL ){
				WtoA(itr->second->shortInfo->event_name, title);
			}
			if( itr->second->contentInfo != NULL ){
				if( itr->second->contentInfo->nibbleList.size() > 0 ){
					map<int,string>::iterator itrColor;
					itrColor = colorList.find(itr->second->contentInfo->nibbleList[0].content_nibble_level_1);
					if( itrColor != colorList.end() ){
						color = itrColor->second;
					}
				}
			}
			map<LONGLONG, RESERVE_DATA*>::iterator itrReserve;
			LONGLONG key = _Create64Key2(
				itr->second->original_network_id,
				itr->second->transport_stream_id,
				itr->second->service_id,
				itr->second->event_id
				);
			itrReserve = reserveMap.find(key);
			if( itrReserve != reserveMap.end() ){
				Format(buff,"<tr><td valign=\"top\" height=\"%dpx\" width=\"148px\" bgcolor=#%s><font size=\"2\"><DIV style=\"height:%dpx; overflow:auto;\"><font color=#FF8000><B>予</B></font> %02d <a href=\"epginfo.html?onid=%d&tsid=%d&sid=%d&evid=%d&preset=65535\">%s</a></DIV></font></td></tr>\r\n",
					duration*minPx,
					color.c_str(),
					duration*minPx,
					itr->second->start_time.wMinute,
					itr->second->original_network_id,
					itr->second->transport_stream_id,
					itr->second->service_id,
					itr->second->event_id,
					title.c_str()
					);
			}else{
				Format(buff,"<tr><td valign=\"top\" height=\"%dpx\" width=\"148px\" bgcolor=#%s><font size=\"2\"><DIV style=\"height:%dpx; overflow:auto;\">%02d <a href=\"epginfo.html?onid=%d&tsid=%d&sid=%d&evid=%d&preset=0\">%s</a></DIV></font></td></tr>\r\n",
					duration*minPx,
					color.c_str(),
					duration*minPx,
					itr->second->start_time.wMinute,
					itr->second->original_network_id,
					itr->second->transport_stream_id,
					itr->second->service_id,
					itr->second->event_id,
					title.c_str()
					);
			}
			htmlText+=buff;
			lastEnd = evEndTime;
		}
		htmlText+="</TABLE>\r\n";
		htmlText+="</TD>\r\n";
	}

	htmlText+="</TR>\r\n";
	htmlText+="</TABLE><BR>\r\n";

	htmlText+=dateLinkHtml;
	htmlText+="<BR><BR>\r\n";
	htmlText+=pageLinkHtml;
	htmlText+="<BR><BR>\r\n";
	htmlText+=tabPageLink;

	htmlText+="<A HREF=\"index.html\">メニュー</A>\r\n";
	return TRUE;
}

BOOL CHTMLManager::GetEpgInfoPage(CEpgDBManager* epgDB, vector<RESERVE_DATA*>* reserveList, vector<TUNER_RESERVE_INFO>* tunerList, string param, HTTP_STREAM* sendParam)
{
	if( sendParam == NULL || epgDB == NULL){
		return FALSE;
	}
	OutputDebugStringA(param.c_str());
	map<string,string> paramMap;
	while(param.size()>0){
		string buff;
		Separate(param, "&", buff, param);
		if(buff.size()>0){
			string key;
			string val;
			Separate(buff, "=", key, val);
			paramMap.insert(pair<string,string>(key, val));
		}
	}
	map<string,string>::iterator itr;
	WORD onid = 0;
	itr = paramMap.find("onid");
	if( itr != paramMap.end() ){
		onid = (WORD)atoi(itr->second.c_str());
	}
	WORD tsid = 0;
	itr = paramMap.find("tsid");
	if( itr != paramMap.end() ){
		tsid = (WORD)atoi(itr->second.c_str());
	}
	WORD sid = 0;
	itr = paramMap.find("sid");
	if( itr != paramMap.end() ){
		sid = (WORD)atoi(itr->second.c_str());
	}
	WORD evid = 0;
	itr = paramMap.find("evid");
	if( itr != paramMap.end() ){
		evid = (WORD)atoi(itr->second.c_str());
	}
	WORD preset = 0;
	itr = paramMap.find("preset");
	if( itr != paramMap.end() ){
		preset = (WORD)atoi(itr->second.c_str());
	}

	RESERVE_DATA* reserveInfo = NULL;
	for( size_t i=0; i<(*reserveList).size(); i++ ){
		if( (*reserveList)[i]->originalNetworkID == onid,
			(*reserveList)[i]->transportStreamID == tsid, 
			(*reserveList)[i]->serviceID == sid,
			(*reserveList)[i]->eventID == evid
			){
				reserveInfo = (*reserveList)[i];
				break;
		}
	}

	EPGDB_EVENT_INFO* eventInfo;
	if( epgDB->SearchEpg(onid, tsid, sid, evid, &eventInfo) == FALSE ){
		return FALSE;
	}
	wstring serviceName;
	epgDB->SearchServiceName(onid, tsid, sid, serviceName);
	wstring eventText;
	_ConvertEpgInfoText2(eventInfo, eventText, serviceName);
	string buff;
	WtoA(eventText, buff);

	string html = HTML_TOP;
	html+="番組詳細<BR><BR>\r\n";

	html+="<HR>番組情報<HR>\r\n";
	Replace(buff, "\r\n","<BR>\r\n");
	html+=buff;
	html+="<HR>録画設定<HR>\r\n";

	//プリセット
	Format(buff, "<form method=\"POST\" action=\"epginfo.html\">\r\n");
	html+=buff;
	Format(buff, "<input type=hidden name=\"onid\" value=\"%d\">\r\n", onid);
	html+=buff;
	Format(buff, "<input type=hidden name=\"tsid\" value=\"%d\">\r\n", tsid);
	html+=buff;
	Format(buff, "<input type=hidden name=\"sid\" value=\"%d\">\r\n", sid);
	html+=buff;
	Format(buff, "<input type=hidden name=\"evid\" value=\"%d\">\r\n", evid);
	html+=buff;
	html+="プリセット:\r\n<select name=\"preset\">\r\n";
	html+="<option value=\"0\">デフォルト\r\n";

	wstring iniPath = L"";
	GetModuleIniPath(iniPath);
	WCHAR iniBuff[512]=L"";
	GetPrivateProfileString(L"SET", L"PresetID", L"", iniBuff, 512, iniPath.c_str());
	wstring parseBuff = iniBuff;
	vector<DWORD> idList;
	do{
		wstring presetID =L"";
		Separate(parseBuff, L",", presetID, parseBuff);
		idList.push_back((DWORD)_wtoi(presetID.c_str()));
	}while(parseBuff.size()>0);
	for( size_t i=0; i<idList.size(); i++ ){
		wstring appName = L"";
		Format(appName, L"REC_DEF%d", idList[i]);
		ZeroMemory(iniBuff, sizeof(WCHAR)*512);
		GetPrivateProfileString(appName.c_str(), L"SetName", L"", iniBuff, 512, iniPath.c_str());
		string item = "";
		string name = "";
		WtoA(iniBuff, name);
		if( idList[i]==preset ){
			Format(item, "<option value=\"%d\" selected>%s\r\n", idList[i], name.c_str());
		}else{
			Format(item, "<option value=\"%d\">%s\r\n", idList[i], name.c_str());
		}
		html+=item;
	}

	if( reserveInfo != NULL ){
		if( preset == 0xFFFF ){
			html+="<option value=\"65535\" selected>予約時\r\n";
		}else{
			html+="<option value=\"65535\">予約時\r\n";
		}
	}
	html+="</select>\r\n<input type=submit value=\"load\">\r\n</form>\r\n";

	//録画設定
	if( reserveInfo == NULL ){
		Format(buff, "<form method=\"POST\" action=\"reserveadd.html?onid=%d&tsid=%d&sid=%d&evid=%d\">\r\n", onid, tsid, sid, evid);
		html+=buff;
		Format(buff, "<input type=hidden name=\"presetID\" value=\"%d\">\r\n", preset);
		html+=buff;

		string paramText = "";
		if( preset != 0xFFFF ){
			REC_SETTING_DATA recSetData;
			LoadRecSetData(preset, &recSetData);
			CreateRecSetForm(&recSetData, tunerList, paramText);
		}else{
			CreateRecSetForm(&reserveInfo->recSetting, tunerList, paramText);
		}
		html+=paramText;
		Format(buff, "<input type=hidden name=\"onid\" value=\"%d\">\r\n", onid);
		html+=buff;
		Format(buff, "<input type=hidden name=\"tsid\" value=\"%d\">\r\n", tsid);
		html+=buff;
		Format(buff, "<input type=hidden name=\"sid\" value=\"%d\">\r\n", sid);
		html+=buff;
		Format(buff, "<input type=hidden name=\"evid\" value=\"%d\">\r\n", evid);
		html+=buff;
		html+="<input type=submit value=\"追加\">\r\n</form>\r\n";
	}else{
		Format(buff, "<form method=\"POST\" action=\"reservechg.html?id=%d\">\r\n", reserveInfo->reserveID);
		html+=buff;
		Format(buff, "<input type=hidden name=\"presetID\" value=\"%d\">\r\n", preset);
		html+=buff;

		string paramText = "";
		if( preset != 0xFFFF ){
			REC_SETTING_DATA recSetData;
			LoadRecSetData(preset, &recSetData);
			CreateRecSetForm(&recSetData, tunerList, paramText);
		}else{
			CreateRecSetForm(&reserveInfo->recSetting, tunerList, paramText);
		}
		html+=paramText;

		html+="<input type=submit value=\"変更\">\r\n</form>\r\n";

		//削除
		Format(buff, "<form method=\"POST\" action=\"reservedel.html?id=%d\">\r\n", reserveInfo->reserveID);
		html+=buff;
		html+="<input type=submit value=\"削除\">\r\n</form>\r\n";
	}

	html+= "<A HREF=\"epg.html\">番組表へ</A><BR>\r\n";

	html+=HTML_END;
	sendParam->dataSize = (DWORD)html.size();
	sendParam->data = new BYTE[sendParam->dataSize];
	memcpy(sendParam->data, html.c_str(), sendParam->dataSize);
	if( sendParam->dataSize > 0 ){
		Format(sendParam->httpHeader, "HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: %d\r\nConnection: close\r\n\r\n", sendParam->dataSize);
	}else{
		sendParam->httpHeader = "HTTP/1.0 400 Bad Request\r\nConnection: close\r\n\r\n";
	}
	return TRUE;
}

BOOL CHTMLManager::GetAddReserveData(CEpgDBManager* epgDB, RESERVE_DATA* reserveData, string param)
{
	if( epgDB == NULL){
		return FALSE;
	}
	OutputDebugStringA(param.c_str());
	map<string,string> paramMap;
	while(param.size()>0){
		string buff;
		Separate(param, "&", buff, param);
		if(buff.size()>0){
			string key;
			string val;
			Separate(buff, "=", key, val);
			paramMap.insert(pair<string,string>(key, val));
		}
	}
	map<string,string>::iterator itr;
	WORD onid = 0;
	itr = paramMap.find("onid");
	if( itr != paramMap.end() ){
		onid = (WORD)atoi(itr->second.c_str());
	}
	WORD tsid = 0;
	itr = paramMap.find("tsid");
	if( itr != paramMap.end() ){
		tsid = (WORD)atoi(itr->second.c_str());
	}
	WORD sid = 0;
	itr = paramMap.find("sid");
	if( itr != paramMap.end() ){
		sid = (WORD)atoi(itr->second.c_str());
	}
	WORD evid = 0;
	itr = paramMap.find("evid");
	if( itr != paramMap.end() ){
		evid = (WORD)atoi(itr->second.c_str());
	}

	EPGDB_EVENT_INFO* eventInfo;
	if( epgDB->SearchEpg(onid, tsid, sid, evid, &eventInfo) == FALSE ){
		return FALSE;
	}
	if( eventInfo->shortInfo != NULL ){
		reserveData->title = eventInfo->shortInfo->event_name;
	}
	reserveData->startTime = eventInfo->start_time;
	reserveData->startTimeEpg = eventInfo->start_time;
	reserveData->durationSecond = eventInfo->durationSec;
	epgDB->SearchServiceName(
		eventInfo->original_network_id,
		eventInfo->transport_stream_id,
		eventInfo->service_id,
		reserveData->stationName
		);
	reserveData->originalNetworkID = eventInfo->original_network_id;
	reserveData->transportStreamID = eventInfo->transport_stream_id;
	reserveData->serviceID = eventInfo->service_id;
	reserveData->eventID = eventInfo->event_id;

	itr = paramMap.find("presetID");
	if( itr == paramMap.end() ){
		return FALSE;
	}
	WORD presetID = (WORD)atoi(itr->second.c_str());
	reserveData->recSetting.recFolderList.clear();
	reserveData->recSetting.partialRecFolder.clear();
	LoadRecSetData(presetID, &reserveData->recSetting);

	itr = paramMap.find("recMode");
	if( itr == paramMap.end() ){
		return FALSE;
	}
	reserveData->recSetting.recMode = (BYTE)atoi(itr->second.c_str());

	if(reserveData->eventID != 0xFFFF){
		itr = paramMap.find("tuijyuuFlag");
		if( itr == paramMap.end() ){
			return FALSE;
		}
		reserveData->recSetting.tuijyuuFlag = (BYTE)atoi(itr->second.c_str());
	}else{
		reserveData->recSetting.tuijyuuFlag = 0;
	}

	itr = paramMap.find("priority");
	if( itr == paramMap.end() ){
		return FALSE;
	}
	reserveData->recSetting.priority = (BYTE)atoi(itr->second.c_str());

	if(reserveData->eventID != 0xFFFF){
		itr = paramMap.find("pittariFlag");
		if( itr == paramMap.end() ){
			return FALSE;
		}
		reserveData->recSetting.pittariFlag = (BYTE)atoi(itr->second.c_str());
	}else{
		reserveData->recSetting.pittariFlag = 0;
	}

	itr = paramMap.find("suspendMode");
	if( itr == paramMap.end() ){
		return FALSE;
	}
	reserveData->recSetting.suspendMode = (BYTE)atoi(itr->second.c_str());
	
	if( reserveData->recSetting.suspendMode == 1 || reserveData->recSetting.suspendMode == 2 ){
		itr = paramMap.find("rebootFlag");
		if( itr == paramMap.end() ){
			reserveData->recSetting.rebootFlag = 0;
		}else{
			reserveData->recSetting.rebootFlag = 1;
		}
	}else{
		reserveData->recSetting.rebootFlag = 0;
	}

	itr = paramMap.find("useDefMargineFlag");
	if( itr == paramMap.end() ){
		reserveData->recSetting.useMargineFlag = 1;
		itr = paramMap.find("startMargine");
		if( itr == paramMap.end() ){
			return FALSE;
		}
		reserveData->recSetting.startMargine = atoi(itr->second.c_str());
		itr = paramMap.find("endMargine");
		if( itr == paramMap.end() ){
			return FALSE;
		}
		reserveData->recSetting.endMargine = atoi(itr->second.c_str());
	}else{
		reserveData->recSetting.useMargineFlag = 0;
		reserveData->recSetting.startMargine = 0;
		reserveData->recSetting.endMargine = 0;
	}

	itr = paramMap.find("serviceMode");
	if( itr != paramMap.end() ){
		reserveData->recSetting.serviceMode = 0;
	}else{
		reserveData->recSetting.serviceMode = 1;
		itr = paramMap.find("serviceMode_1");
		if( itr != paramMap.end() ){
			reserveData->recSetting.serviceMode |= 0x10;
		}
		itr = paramMap.find("serviceMode_2");
		if( itr != paramMap.end() ){
			reserveData->recSetting.serviceMode |= 0x20;
		}
	}

	itr = paramMap.find("continueRecFlag");
	if( itr == paramMap.end() ){
		reserveData->recSetting.continueRecFlag = 0;
	}else{
		reserveData->recSetting.continueRecFlag = 1;
	}
	
	itr = paramMap.find("tunerID");
	if( itr == paramMap.end() ){
		return FALSE;
	}
	reserveData->recSetting.tunerID = (DWORD)atoi(itr->second.c_str());

	itr = paramMap.find("partialRecFlag");
	if( itr == paramMap.end() ){
		reserveData->recSetting.partialRecFlag = 0;
	}else{
		reserveData->recSetting.partialRecFlag = 1;
	}

	return TRUE;

}

BOOL CHTMLManager::GetReserveAddPage(HTTP_STREAM* sendParam, BOOL err)
{
	if( sendParam == NULL ){
		return FALSE;
	}
	string html = HTML_TOP;
	if( err == FALSE ){
		html+="予約を追加しました<BR><BR>\r\n<A HREF=\"epg.html\">番組表</A>\r\n";
	}else{
		html+="入力値不正<BR><BR>\r\n<A HREF=\"epg.html\">番組表</A>\r\n";
	}
	html+=HTML_END;
	sendParam->dataSize = (DWORD)html.size();
	sendParam->data = new BYTE[sendParam->dataSize];
	memcpy(sendParam->data, html.c_str(), sendParam->dataSize);
	if( sendParam->dataSize > 0 ){
		Format(sendParam->httpHeader, "HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: %d\r\nConnection: close\r\n\r\n", sendParam->dataSize);
	}else{
		sendParam->httpHeader = "HTTP/1.0 400 Bad Request\r\nConnection: close\r\n\r\n";
	}
	return TRUE;
}

