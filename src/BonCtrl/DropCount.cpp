#include "StdAfx.h"
#include "DropCount.h"


CDropCount::CDropCount(void)
{
	this->drop = 0;
	this->scramble = 0;
	this->lastLogTime = 0;

	this->lastLogDrop = 0;
	this->lastLogScramble = 0;

	this->signalLv = 0;
}


CDropCount::~CDropCount(void)
{
}

void CDropCount::AddData(BYTE* data, DWORD size)
{
	if( data == NULL || size == 0 ){
		return ;
	}
	for( DWORD i=0; i<size; i+=188 ){
		CTSPacketUtil packet;
		if( packet.Set188TS(data+i, 188) == TRUE ){
			map<WORD, DROP_INFO>::iterator itr;
			itr = this->infoMap.find(packet.PID);
			if( itr == this->infoMap.end() ){
				DROP_INFO item;
				item.PID = packet.PID;
				item.lastCounter = packet.continuity_counter;
				item.total++;
				if( packet.transport_scrambling_control != 0 ){
					item.scramble++;
				}
				this->infoMap.insert(pair<WORD, DROP_INFO>(item.PID, item));
			}else{
				CheckCounter(&packet, &(itr->second));
			}
		}
	}
}

void CDropCount::Clear()
{
	this->infoMap.clear();
	this->drop = 0;
	this->scramble = 0;
	this->log.clear();
	this->lastLogTime = 0;

	this->lastLogDrop = 0;
	this->lastLogScramble = 0;
	this->signalLv = 0;
}

void CDropCount::SetSignal(float level)
{
	this->signalLv = level;
}

void CDropCount::GetCount(ULONGLONG* drop, ULONGLONG* scramble)
{
	if( drop != NULL ){
		*drop = this->drop;
	}
	if( scramble != NULL ){
		*scramble = this->scramble;
	}
}

ULONGLONG CDropCount::GetDropCount()
{
	return this->drop;
}

ULONGLONG CDropCount::GetScrambleCount()
{
	return this->scramble;
}

void CDropCount::CheckCounter(CTSPacketUtil* tsPacket, DROP_INFO* info)
{
	if( tsPacket == NULL || info == NULL ){
		return;
	}
	info->total++;

	if( tsPacket->PID == 0x1FFF){
		return;
	}
	if( tsPacket->transport_scrambling_control != 0 ){
		info->scramble++;
		this->scramble++;
	}

	if( tsPacket->adaptation_field_control == 0x00 || tsPacket->adaptation_field_control == 0x02 ){
		//ペイロードが存在しない場合は意味なし
		info->duplicateFlag = FALSE;
		if( tsPacket->adaptation_field_control == 0x02 || tsPacket->adaptation_field_control == 0x03 ){
			if( tsPacket->transport_scrambling_control == 0 ){
				if(tsPacket->discontinuity_indicator == 1){
					//不連続の判定が必要
					info->drop++;
					this->drop++;
					goto CHK_END;
				}else{
					goto CHK_END;
				}
			}
		}else{
			goto CHK_END;
		}
	}
	if( info->lastCounter == tsPacket->continuity_counter ){
		if( tsPacket->adaptation_field_control == 0x01 || tsPacket->adaptation_field_control == 0x03 ){
			if( tsPacket->transport_scrambling_control == 0 ){
				if( info->duplicateFlag == FALSE ){
					//重送？一応連続と判定
					info->duplicateFlag = TRUE;
					if( tsPacket->adaptation_field_control == 0x02 || tsPacket->adaptation_field_control == 0x03 ){
						if(tsPacket->discontinuity_indicator == 1){
							//不連続の判定が必要
							info->duplicateFlag = FALSE;
							info->drop++;
							this->drop++;
							goto CHK_END;
						}else{
							goto CHK_END;
						}
					}else{
						goto CHK_END;
					}
				}else{
					//前回重送と判断してるので不連続
					info->duplicateFlag = FALSE;
					info->drop++;
					this->drop++;
					goto CHK_END;
				}
			}else{
				goto CHK_END;
			}
		}
	}
	if( info->lastCounter+1 != tsPacket->continuity_counter ){
		if( info->lastCounter != 0x0F && tsPacket->continuity_counter != 0x00 ){
			//カウンターが飛んだので不連続
			ULONGLONG count = 0;
			if( tsPacket->continuity_counter <= info->lastCounter ){
				count = (tsPacket->continuity_counter+15) - info->lastCounter;
			}else{
				count = tsPacket->continuity_counter - info->lastCounter;
			}

			info->drop+= count;
			this->drop+= count;
			info->total+= count-1;
			goto CHK_END;
		}
	}

CHK_END:
	info->lastCounter = tsPacket->continuity_counter;
	if( this->lastLogTime + 5 < GetTimeCount() ){
		if( this->lastLogDrop != this->drop ||
			this->lastLogScramble != this->scramble
			){
				wstring log;
				SYSTEMTIME now;
				GetLocalTime(&now);
				Format(log, L"%04d/%02d/%02d %02d:%02d:%02d Drop:%I64d Scramble:%I64d Signal: %.02f",
					now.wYear,
					now.wMonth,
					now.wDay,
					now.wHour,
					now.wMinute,
					now.wSecond,
					this->drop,
					this->scramble,
					this->signalLv
					);
				this->log.push_back(log);
				this->lastLogDrop = this->drop;
				this->lastLogScramble = this->scramble;
		}
		this->lastLogTime = GetTimeCount();
	}
}

void CDropCount::SaveLog(wstring filePath)
{
	HANDLE file = _CreateFile2( filePath.c_str(), GENERIC_WRITE, FILE_SHARE_READ, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL );
	if( file != INVALID_HANDLE_VALUE ){
		DWORD write;

		string buff;
		for( size_t i=0; i<log.size(); i++ ){
			WtoA(log[i], buff);
			buff += "\r\n";
			WriteFile(file, buff.c_str(), (DWORD)buff.size(), &write, NULL );
		}

		buff = "\r\n";
		WriteFile(file, buff.c_str(), (DWORD)buff.size(), &write, NULL );

		map<WORD, DROP_INFO>::iterator itr;
		for( itr = this->infoMap.begin(); itr != this->infoMap.end(); itr++ ){
			
			Format(buff, "PID: 0x%04X  Total:%9I64d  Drop:%9I64d  Scramble: %9I64d\r\n",
				itr->first, itr->second.total, itr->second.drop, itr->second.scramble );
			WriteFile(file, buff.c_str(), (DWORD)buff.size(), &write, NULL );
		}

		CloseHandle(file);
	}
}

