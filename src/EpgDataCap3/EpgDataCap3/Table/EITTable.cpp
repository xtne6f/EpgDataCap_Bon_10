#include "StdAfx.h"
#include "EITTable.h"

#include "../../../Common/EpgTimerUtil.h"
#include "../Descriptor/Descriptor.h"

CEITTable::CEITTable(void)
{
}

CEITTable::~CEITTable(void)
{
	Clear();
}

void CEITTable::Clear()
{
	for( size_t i=0 ;i<eventInfoList.size(); i++ ){
		SAFE_DELETE(eventInfoList[i]);
	}
	eventInfoList.clear();
}

BOOL CEITTable::Decode( BYTE* data, DWORD dataSize, DWORD* decodeReadSize )
{
	if( data == NULL ){
		return FALSE;
	}
	Clear();

	//////////////////////////////////////////////////////
	//サイズのチェック
	//最低限table_idとsection_length+CRCのサイズは必須
	if( dataSize < 7 ){
		return FALSE;
	}
	//->サイズのチェック

	DWORD readSize = 0;
	//////////////////////////////////////////////////////
	//解析処理
	table_id = data[0];
	section_syntax_indicator = (data[1]&0x80)>>7;
	section_length = ((WORD)data[1]&0x0F)<<8 | data[2];
	readSize+=3;

	if( section_syntax_indicator != 1 ){
		//固定値がおかしい
		_OutputDebugString( L"++CEITTable:: section_syntax err" );
		return FALSE;
	}
	if( table_id < 0x4E || table_id > 0x6F ){
		//table_idがおかしい
		_OutputDebugString( L"++CEITTable:: table_id err 0x%02X", table_id );
		return FALSE;
	}
	if( readSize+section_length > dataSize && section_length > 3){
		//サイズ異常
		_OutputDebugString( L"++CEITTable:: size err %d > %d", readSize+section_length, dataSize );
		return FALSE;
	}
	//CRCチェック
	crc32 = ((DWORD)data[3+section_length-4])<<24 |
		((DWORD)data[3+section_length-3])<<16 |
		((DWORD)data[3+section_length-2])<<8 |
		data[3+section_length-1];
	if( crc32 != _Crc32(3+section_length-4, data) ){
		_OutputDebugString( L"++CEITTable:: CRC err" );
		return FALSE;
	}

	if( section_length > 4 ){
		service_id = ((WORD)data[readSize])<<8 | data[readSize+1];
		version_number = (data[readSize+2]&0x3E)>>1;
		current_next_indicator = data[readSize+2]&0x01;
		section_number = data[readSize+3];
		last_section_number = data[readSize+4];
		transport_stream_id = ((WORD)data[readSize+5])<<8 | data[readSize+6];
		original_network_id = ((WORD)data[readSize+7])<<8 | data[readSize+8];
		segment_last_section_number = data[readSize+9];
		last_table_id = data[readSize+10];
		readSize += 11;
		while( readSize < (DWORD)section_length+3-4 ){
			EVENT_INFO_DATA* item = new EVENT_INFO_DATA;
			item->event_id = ((WORD)data[readSize])<<8 | data[readSize+1];
			if( data[readSize+2] == 0xFF && data[readSize+3] == 0xFF && data[readSize+4] == 0xFF &&
				data[readSize+5] == 0xFF && data[readSize+6] == 0xFF )
			{
				item->StartTimeFlag = FALSE;
			}else{
				item->StartTimeFlag = TRUE;
				DWORD mjd = ((DWORD)data[readSize+2])<<8 | data[readSize+3];
				_MJDtoSYSTEMTIME(mjd, &(item->start_time));
				item->start_time.wHour = (WORD)_BCDtoDWORD(data+readSize+4, 1, 2);
				item->start_time.wMinute = (WORD)_BCDtoDWORD(data+readSize+5, 1, 2);
				item->start_time.wSecond = (WORD)_BCDtoDWORD(data+readSize+6, 1, 2);
			}
			readSize+=7;
			if( data[readSize] == 0xFF && data[readSize+1] == 0xFF && data[readSize+2] == 0xFF)
			{
				item->DurationFlag = FALSE;
			}else{
				item->DurationFlag = TRUE;
				item->durationHH = (WORD)_BCDtoDWORD(data+readSize, 1, 2);
				item->durationMM = (WORD)_BCDtoDWORD(data+readSize+1, 1, 2);
				item->durationSS = (WORD)_BCDtoDWORD(data+readSize+2, 1, 2);
			}
			readSize+=3;
			item->running_status = (data[readSize]&0xE0)>>5;
			item->free_CA_mode = (data[readSize]&0x10)>>4;
			item->descriptors_loop_length = ((WORD)data[readSize]&0x0F)<<8 | data[readSize+1];
			readSize += 2;
			if( readSize+item->descriptors_loop_length <= (DWORD)section_length+3-4 && item->descriptors_loop_length > 0){
				CDescriptor descriptor;
				if( descriptor.Decode( data+readSize, item->descriptors_loop_length, &(item->descriptorList), NULL ) == FALSE ){
					_OutputDebugString( L"++CEITTable:: descriptor2 err" );
					return FALSE;
				}
			}

			readSize+=item->descriptors_loop_length;

			eventInfoList.push_back(item);
		}
	}else{
		return FALSE;
	}
	//->解析処理

	if( decodeReadSize != NULL ){
		*decodeReadSize = 3+section_length;
	}

	return TRUE;
}

