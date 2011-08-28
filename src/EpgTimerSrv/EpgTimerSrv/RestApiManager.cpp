#include "StdAfx.h"
#include "RestApiManager.h"


CRestApiManager::CRestApiManager(void)
{
}


CRestApiManager::~CRestApiManager(void)
{
}

void CRestApiManager::CheckXMLChar(wstring& text)
{
	if( text.find(L"&") != string::npos ){
		Replace(text, L"&", L"&amp;");
	}
	if( text.find(L"<") != string::npos ){
		Replace(text, L"<", L"&lt;");
	}
	if( text.find(L">") != string::npos ){
		Replace(text, L">", L"&gt;");
	}
}

DWORD CRestApiManager::AnalyzeCmd(string verb, string url, string param, HTTP_STREAM* sendParam, CEpgDBManager* epgDB)
{
	DWORD ret = ERR_FALSE;
	string urlParam = "";
	if( url.find("?") != string::npos ){
		Separate(url, "?", url, urlParam);
	}
	if( urlParam.size() > 0 && param.size() > 0){
		urlParam += "&";
		urlParam += param;
	}else if( urlParam.size() == 0 && param.size() > 0){
		urlParam = param;
	}

	OutputDebugStringA(urlParam.c_str());

	if(url.find("/api/EnumService") == 0 ){
		ret = GetEnumService(urlParam, sendParam, epgDB);
	}else
	if(url.find("/api/EnumEventInfo") == 0 ){
		ret = GetEnumEventInfo(urlParam, sendParam, epgDB);
	}

	return ret;
}

DWORD CRestApiManager::GetEnumService(string param, HTTP_STREAM* sendParam, CEpgDBManager* epgDB)
{
	DWORD ret = NO_ERR;

	vector<EPGDB_SERVICE_INFO> list;
	wstring xml = L"";
	string utf8 = "";
	if( epgDB->GetServiceList(&list) == TRUE ){
		wstring buff = L"";
		xml += L"<?xml version=\"1.0\" encoding=\"UTF-8\" ?><entry>";
		Format(buff, L"<total>%d</total><index>0</index><count>%d</count>", list.size(), list.size());
		xml += buff;
		xml += L"<items>";
		wstring serviceinfo = L"";
		for( size_t i=0; i<list.size(); i++ ){
			serviceinfo += L"<serviceinfo>";
			Format(buff, L"<ONID>%d</ONID>", list[i].ONID);
			serviceinfo += buff;
			Format(buff, L"<TSID>%d</TSID>", list[i].TSID);
			serviceinfo += buff;
			Format(buff, L"<SID>%d</SID>", list[i].SID);
			serviceinfo += buff;
			Format(buff, L"<service_type>%d</service_type>", list[i].service_type);
			serviceinfo += buff;
			Format(buff, L"<partialReceptionFlag>%d</partialReceptionFlag>", list[i].partialReceptionFlag);
			serviceinfo += buff;
			Format(buff, L"<service_provider_name>%s</service_provider_name>", list[i].service_provider_name.c_str());
			serviceinfo += buff;
			Format(buff, L"<service_name>%s</service_name>", list[i].service_name.c_str());
			serviceinfo += buff;
			Format(buff, L"<network_name>%s</network_name>", list[i].network_name.c_str());
			serviceinfo += buff;
			Format(buff, L"<ts_name>%s</ts_name>", list[i].ts_name.c_str());
			serviceinfo += buff;
			Format(buff, L"<remote_control_key_id>%d</remote_control_key_id>", list[i].remote_control_key_id);
			serviceinfo += buff;
			serviceinfo += L"</serviceinfo>";
		}
		xml += serviceinfo;
		xml += L"</items>";
		xml += L"</entry>";
	}else{
		xml += L"<?xml version=\"1.0\" encoding=\"UTF-8\" ?><entry>";
		xml += L"<err>EPGデータを読み込み中、または存在しません</err>";
		xml += L"</entry>";
	}

	WtoUTF8(xml, utf8);

	sendParam->dataSize = (DWORD)utf8.size();
	sendParam->data = new BYTE[sendParam->dataSize];
	memcpy(sendParam->data, utf8.c_str(), sendParam->dataSize);
	if( sendParam->dataSize > 0 ){
		Format(sendParam->httpHeader, "HTTP/1.0 200 OK\r\nContent-Type: text/xml\r\nContent-Length: %d\r\nConnection: close\r\n\r\n", sendParam->dataSize);
	}else{
		sendParam->httpHeader = "HTTP/1.0 400 Bad Request\r\nConnection: close\r\n\r\n";
	}

	return ret;
}

DWORD CRestApiManager::GetEnumEventInfo(string param, HTTP_STREAM* sendParam, CEpgDBManager* epgDB)
{
	DWORD ret = NO_ERR;

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
	WORD ONID = 0xFFFF;
	itr = paramMap.find("ONID");
	if( itr != paramMap.end() ){
		ONID = (WORD)atoi(itr->second.c_str());
	}
	WORD TSID = 0xFFFF;
	itr = paramMap.find("TSID");
	if( itr != paramMap.end() ){
		TSID = (WORD)atoi(itr->second.c_str());
	}
	WORD SID = 0xFFFF;
	itr = paramMap.find("SID");
	if( itr != paramMap.end() ){
		SID = (WORD)atoi(itr->second.c_str());
	}
	WORD basicOnly = 1;
	itr = paramMap.find("basic");
	if( itr != paramMap.end() ){
		basicOnly = (WORD)atoi(itr->second.c_str());
	}
	DWORD index = 0;
	itr = paramMap.find("index");
	if( itr != paramMap.end() ){
		index = (DWORD)atoi(itr->second.c_str());
	}
	DWORD count = 200;
	itr = paramMap.find("count");
	if( itr != paramMap.end() ){
		count = (DWORD)atoi(itr->second.c_str());
	}

	vector<EPGDB_SERVICE_EVENT_INFO*> list;
	wstring xml = L"";
	string utf8 = "";
	if( epgDB->EnumEventAll(&list) == TRUE ){
		wstring buff = L"";
		DWORD total = 0;
		DWORD findCount = 0;

		xml += L"<?xml version=\"1.0\" encoding=\"UTF-8\" ?><entry>";

		wstring serviceinfo = L"";
		serviceinfo += L"<items>";
		for( size_t i=0; i<list.size(); i++ ){
			for( size_t j=0; j<list[i]->eventList.size(); j++){
				EPGDB_EVENT_INFO* eventInfo = list[i]->eventList[j];
				if( eventInfo->original_network_id != ONID && ONID != 0xFFFF ){
					continue;
				}
				if( eventInfo->transport_stream_id != TSID && TSID != 0xFFFF ){
					continue;
				}
				if( eventInfo->service_id != SID && SID != 0xFFFF ){
					continue;
				}
				if( total < index ){
					total++;
					continue;
				}
				if( index + count <= total ){
					total++;
					continue;
				}
				total++;
				findCount++;

				serviceinfo += L"<eventinfo>";
				Format(buff, L"<ONID>%d</ONID>", eventInfo->original_network_id);
				serviceinfo += buff;
				Format(buff, L"<TSID>%d</TSID>", eventInfo->transport_stream_id);
				serviceinfo += buff;
				Format(buff, L"<SID>%d</SID>", eventInfo->service_id);
				serviceinfo += buff;
				Format(buff, L"<eventID>%d</eventID>", eventInfo->event_id);
				serviceinfo += buff;
				if( eventInfo->StartTimeFlag == 1 ){
					Format(buff, L"<startDate>%d/%d/%d</startDate>", eventInfo->start_time.wYear, eventInfo->start_time.wMonth, eventInfo->start_time.wDay);
					serviceinfo += buff;
					Format(buff, L"<startTime>%d:%d:%d</startTime>", eventInfo->start_time.wHour, eventInfo->start_time.wMinute, eventInfo->start_time.wSecond);
					serviceinfo += buff;
					Format(buff, L"<startDayOfWeek>%d</startDayOfWeek>", eventInfo->start_time.wDayOfWeek);
					serviceinfo += buff;
				}
				if( eventInfo->DurationFlag == 1 ){
					Format(buff, L"<duration>%d</duration>", eventInfo->durationSec);
					serviceinfo += buff;
				}
				if( eventInfo->shortInfo != NULL ){
					wstring chk = eventInfo->shortInfo->event_name;
					CheckXMLChar(chk);
					Format(buff, L"<event_name>%s</event_name>", chk.c_str());
					serviceinfo += buff;

					chk = eventInfo->shortInfo->text_char;
					CheckXMLChar(chk);
					Format(buff, L"<event_text>%s</event_text>", chk.c_str());
					serviceinfo += buff;
				}
				if( eventInfo->contentInfo != NULL ){
					serviceinfo += L"";
					for( size_t k=0; k<eventInfo->contentInfo->nibbleList.size(); k++){
						wstring nibble = L"";
						Format(nibble,L"<contentInfo><nibble1>%d</nibble1><nibble2>%d</nibble2></contentInfo>", 
							eventInfo->contentInfo->nibbleList[k].content_nibble_level_1,
							eventInfo->contentInfo->nibbleList[k].content_nibble_level_2);
						serviceinfo += nibble;
					}
				}
				if( eventInfo->eventGroupInfo != NULL ){
					for( size_t k=0; k<eventInfo->eventGroupInfo->eventDataList.size(); k++){
						wstring group = L"";
						Format(group,L"<groupInfo><ONID>%d</ONID><TSID>%d</TSID><SID>%d</SID><eventID>%d</eventID></groupInfo>", 
							eventInfo->eventGroupInfo->eventDataList[k].original_network_id,
							eventInfo->eventGroupInfo->eventDataList[k].transport_stream_id,
							eventInfo->eventGroupInfo->eventDataList[k].service_id,
							eventInfo->eventGroupInfo->eventDataList[k].event_id
							);
						serviceinfo += group;
					}
				}

				Format(buff, L"<freeCAFlag>%d</freeCAFlag>", eventInfo->freeCAFlag);
				serviceinfo += buff;

				if( basicOnly == 0 ){
					if( eventInfo->extInfo != NULL ){
						wstring chk = eventInfo->extInfo->text_char;
						CheckXMLChar(chk);
						Format(buff, L"<event_ext_text>%s</event_ext_text>", chk.c_str());
						serviceinfo += buff;
					}
					if( eventInfo->componentInfo != NULL ){
						Format(buff, L"<videoInfo><stream_content>%d</stream_content><component_type>%d</component_type><component_tag>%d</component_tag><text>%s</text></videoInfo>", 
							eventInfo->componentInfo->stream_content,
							eventInfo->componentInfo->component_type,
							eventInfo->componentInfo->component_tag,
							eventInfo->componentInfo->text_char.c_str()
							);
						serviceinfo += buff;
					}
					if( eventInfo->audioInfo != NULL ){
						for( size_t k=0; k<eventInfo->audioInfo->componentList.size(); k++ ){
							Format(buff, L"<audioInfo><stream_content>%d</stream_content><component_type>%d</component_type><component_tag>%d</component_tag><stream_type>%d</stream_type><simulcast_group_tag>%d</simulcast_group_tag><ES_multi_lingual_flag>%d</ES_multi_lingual_flag><main_component_flag>%d</main_component_flag><quality_indicator>%d</quality_indicator><sampling_rate>%d</sampling_rate><text>%s</text></audioInfo>", 
								eventInfo->audioInfo->componentList[k].stream_content,
								eventInfo->audioInfo->componentList[k].component_type,
								eventInfo->audioInfo->componentList[k].component_tag,
								eventInfo->audioInfo->componentList[k].stream_type,
								eventInfo->audioInfo->componentList[k].simulcast_group_tag,
								eventInfo->audioInfo->componentList[k].ES_multi_lingual_flag,
								eventInfo->audioInfo->componentList[k].main_component_flag,
								eventInfo->audioInfo->componentList[k].quality_indicator,
								eventInfo->audioInfo->componentList[k].sampling_rate,
								eventInfo->audioInfo->componentList[k].text_char.c_str()
								);
							serviceinfo += buff;
						}
					}
					if( eventInfo->eventRelayInfo != NULL ){
						for( size_t k=0; k<eventInfo->eventRelayInfo->eventDataList.size(); k++){
							wstring group = L"";
							Format(group,L"<relayInfo><ONID>%d</ONID><TSID>%d</TSID><SID>%d</SID><eventID>%d</eventID></relayInfo>", 
								eventInfo->eventRelayInfo->eventDataList[k].original_network_id,
								eventInfo->eventRelayInfo->eventDataList[k].transport_stream_id,
								eventInfo->eventRelayInfo->eventDataList[k].service_id,
								eventInfo->eventRelayInfo->eventDataList[k].event_id
								);
							serviceinfo += group;
						}
					}
				}
				serviceinfo += L"</eventinfo>";

			}
			list[i]->eventList.clear();
			SAFE_DELETE(list[i]);
		}
		serviceinfo += L"</items>";

		Format(buff, L"<total>%d</total><index>%d</index><count>%d</count>", total, index, findCount);
		xml += buff;
		xml += serviceinfo;
		xml += L"</entry>";
	}else{
		xml += L"<?xml version=\"1.0\" encoding=\"UTF-8\" ?><entry>";
		xml += L"<err>EPGデータを読み込み中、または存在しません</err>";
		xml += L"</entry>";
	}

	WtoUTF8(xml, utf8);

	sendParam->dataSize = (DWORD)utf8.size();
	sendParam->data = new BYTE[sendParam->dataSize];
	memcpy(sendParam->data, utf8.c_str(), sendParam->dataSize);
	if( sendParam->dataSize > 0 ){
		Format(sendParam->httpHeader, "HTTP/1.0 200 OK\r\nContent-Type: text/xml\r\nContent-Length: %d\r\nConnection: close\r\n\r\n", sendParam->dataSize);
	}else{
		sendParam->httpHeader = "HTTP/1.0 400 Bad Request\r\nConnection: close\r\n\r\n";
	}

	return ret;
}
