#include "StdAfx.h"
#include "SDTTable.h"

#include "../../../Common/EpgTimerUtil.h"
#include "../Descriptor/Descriptor.h"

CSDTTable::CSDTTable(void)
{
}

CSDTTable::~CSDTTable(void)
{
	Clear();
}

void CSDTTable::Clear()
{
	for( size_t i=0 ;i<serviceInfoList.size(); i++ ){
		SAFE_DELETE(serviceInfoList[i]);
	}
	serviceInfoList.clear();
}

BOOL CSDTTable::Decode( BYTE* data, DWORD dataSize, DWORD* decodeReadSize )
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
		_OutputDebugString( L"++CSDTTable:: section_syntax err" );
		return FALSE;
	}
	if( table_id != 0x42 && table_id != 0x46 ){
		//table_idがおかしい
		_OutputDebugString( L"++CSDTTable:: table_id err 0x%02X", table_id );
		return FALSE;
	}
	if( readSize+section_length > dataSize && section_length > 3){
		//サイズ異常
		_OutputDebugString( L"++CSDTTable:: size err %d > %d", readSize+section_length, dataSize );
		return FALSE;
	}
	//CRCチェック
	crc32 = ((DWORD)data[3+section_length-4])<<24 |
		((DWORD)data[3+section_length-3])<<16 |
		((DWORD)data[3+section_length-2])<<8 |
		data[3+section_length-1];
	if( crc32 != _Crc32(3+section_length-4, data) ){
		_OutputDebugString( L"++CSDTTable:: CRC err" );
		return FALSE;
	}

	if( section_length > 4 ){
		transport_stream_id = ((WORD)data[readSize])<<8 | data[readSize+1];
		version_number = (data[readSize+2]&0x3E)>>1;
		current_next_indicator = data[readSize+2]&0x01;
		section_number = data[readSize+3];
		last_section_number = data[readSize+4];
		original_network_id = ((WORD)data[readSize+5])<<8 | data[readSize+6];
		readSize += 8;
		while( readSize < (DWORD)section_length+3-4 ){
			SERVICE_INFO_DATA* item = new SERVICE_INFO_DATA;
			item->service_id = ((WORD)data[readSize])<<8 | data[readSize+1];
			item->EIT_user_defined_flags = (data[readSize+2]&0x1C)>>2;
			item->EIT_schedule_flag = (data[readSize+2]&0x02)>>1;
			item->EIT_present_following_flag = data[readSize+2]&0x01;
			item->running_status = (data[readSize+3]&0xE0)>>5;
			item->free_CA_mode = (data[readSize+3]&0x10)>>4;
			item->descriptors_loop_length = ((WORD)data[readSize+3]&0x0F)<<8 | data[readSize+4];
			readSize += 5;
			if( readSize+item->descriptors_loop_length <= (DWORD)section_length+3-4 && item->descriptors_loop_length > 0){
				CDescriptor descriptor;
				if( descriptor.Decode( data+readSize, item->descriptors_loop_length, &(item->descriptorList), NULL ) == FALSE ){
					_OutputDebugString( L"++CSDTTable:: descriptor2 err" );
					return FALSE;
				}
			}

			readSize+=item->descriptors_loop_length;

			serviceInfoList.push_back(item);
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

