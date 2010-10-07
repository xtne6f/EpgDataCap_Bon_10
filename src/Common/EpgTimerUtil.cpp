#include "stdafx.h"
#include "EpgTimerUtil.h"

#include "PathUtil.h"
#include "StringUtil.h"
#include "TimeUtil.h"

#include <process.h>
#include <tlhelp32.h> 
#include <shlwapi.h>

LONGLONG _Create64Key( WORD OriginalNetworkID, WORD TransportStreamID, WORD ServiceID )
{
	LONGLONG Key = 
		(((LONGLONG)(OriginalNetworkID&0x0000FFFF))<<32) |
		(((LONGLONG)(TransportStreamID&0x0000FFFF))<<16) |
		((LONGLONG)(ServiceID&0x0000FFFF));
	return Key;
}

LONGLONG _Create64Key2( WORD OriginalNetworkID, WORD TransportStreamID, WORD ServiceID, WORD EventID )
{
	LONGLONG Key = 
		(((LONGLONG)(OriginalNetworkID & 0x0000FFFF))<<48) |
		(((LONGLONG)(TransportStreamID & 0x0000FFFF))<<32) |
		(((LONGLONG)(ServiceID & 0x0000FFFF))<<16) |
		((LONGLONG)(EventID & 0x0000FFFF));
	return Key;
}

#define CRCPOLY1 0x04C11DB7UL
typedef unsigned char byte;
static const unsigned long crctable[256] = {
	0x00000000, 0x04C11DB7, 0x09823B6E, 0x0D4326D9,	0x130476DC, 0x17C56B6B, 0x1A864DB2, 0x1E475005,
	0x2608EDB8, 0x22C9F00F, 0x2F8AD6D6, 0x2B4BCB61, 0x350C9B64, 0x31CD86D3, 0x3C8EA00A, 0x384FBDBD,
	0x4C11DB70, 0x48D0C6C7, 0x4593E01E, 0x4152FDA9,	0x5F15ADAC, 0x5BD4B01B, 0x569796C2, 0x52568B75, 
	0x6A1936C8, 0x6ED82B7F, 0x639B0DA6, 0x675A1011,	0x791D4014, 0x7DDC5DA3, 0x709F7B7A, 0x745E66CD,
	0x9823B6E0, 0x9CE2AB57, 0x91A18D8E, 0x95609039,	0x8B27C03C, 0x8FE6DD8B, 0x82A5FB52, 0x8664E6E5,
	0xBE2B5B58, 0xBAEA46EF, 0xB7A96036, 0xB3687D81,	0xAD2F2D84, 0xA9EE3033, 0xA4AD16EA, 0xA06C0B5D,
	0xD4326D90, 0xD0F37027, 0xDDB056FE, 0xD9714B49,	0xC7361B4C, 0xC3F706FB, 0xCEB42022, 0xCA753D95,
	0xF23A8028, 0xF6FB9D9F, 0xFBB8BB46, 0xFF79A6F1,	0xE13EF6F4, 0xE5FFEB43, 0xE8BCCD9A, 0xEC7DD02D,
	0x34867077, 0x30476DC0, 0x3D044B19, 0x39C556AE,	0x278206AB, 0x23431B1C, 0x2E003DC5, 0x2AC12072, 
	0x128E9DCF, 0x164F8078, 0x1B0CA6A1, 0x1FCDBB16,	0x018AEB13, 0x054BF6A4, 0x0808D07D, 0x0CC9CDCA,
	0x7897AB07, 0x7C56B6B0, 0x71159069, 0x75D48DDE,	0x6B93DDDB, 0x6F52C06C, 0x6211E6B5, 0x66D0FB02,
	0x5E9F46BF, 0x5A5E5B08, 0x571D7DD1, 0x53DC6066,	0x4D9B3063, 0x495A2DD4, 0x44190B0D, 0x40D816BA,
	0xACA5C697, 0xA864DB20, 0xA527FDF9, 0xA1E6E04E,	0xBFA1B04B, 0xBB60ADFC, 0xB6238B25, 0xB2E29692,
	0x8AAD2B2F, 0x8E6C3698, 0x832F1041, 0x87EE0DF6,	0x99A95DF3, 0x9D684044, 0x902B669D, 0x94EA7B2A,
	0xE0B41DE7, 0xE4750050, 0xE9362689, 0xEDF73B3E,	0xF3B06B3B, 0xF771768C, 0xFA325055, 0xFEF34DE2, 
	0xC6BCF05F, 0xC27DEDE8, 0xCF3ECB31, 0xCBFFD686,	0xD5B88683, 0xD1799B34, 0xDC3ABDED, 0xD8FBA05A,
	0x690CE0EE, 0x6DCDFD59, 0x608EDB80, 0x644FC637,	0x7A089632, 0x7EC98B85, 0x738AAD5C, 0x774BB0EB,
	0x4F040D56, 0x4BC510E1, 0x46863638, 0x42472B8F,	0x5C007B8A, 0x58C1663D, 0x558240E4, 0x51435D53, 
	0x251D3B9E, 0x21DC2629, 0x2C9F00F0, 0x285E1D47,	0x36194D42, 0x32D850F5, 0x3F9B762C, 0x3B5A6B9B,
	0x0315D626, 0x07D4CB91, 0x0A97ED48, 0x0E56F0FF,	0x1011A0FA, 0x14D0BD4D, 0x19939B94, 0x1D528623,
	0xF12F560E, 0xF5EE4BB9, 0xF8AD6D60, 0xFC6C70D7,	0xE22B20D2, 0xE6EA3D65, 0xEBA91BBC, 0xEF68060B, 
	0xD727BBB6, 0xD3E6A601, 0xDEA580D8, 0xDA649D6F,	0xC423CD6A, 0xC0E2D0DD, 0xCDA1F604, 0xC960EBB3,
	0xBD3E8D7E, 0xB9FF90C9, 0xB4BCB610, 0xB07DABA7,	0xAE3AFBA2, 0xAAFBE615, 0xA7B8C0CC, 0xA379DD7B,
	0x9B3660C6, 0x9FF77D71, 0x92B45BA8, 0x9675461F,	0x8832161A, 0x8CF30BAD, 0x81B02D74, 0x857130C3, 
	0x5D8A9099, 0x594B8D2E, 0x5408ABF7, 0x50C9B640,	0x4E8EE645, 0x4A4FFBF2, 0x470CDD2B, 0x43CDC09C,
	0x7B827D21, 0x7F436096, 0x7200464F, 0x76C15BF8,	0x68860BFD, 0x6C47164A, 0x61043093, 0x65C52D24,
	0x119B4BE9, 0x155A565E, 0x18197087, 0x1CD86D30,	0x029F3D35, 0x065E2082, 0x0B1D065B, 0x0FDC1BEC,
	0x3793A651, 0x3352BBE6, 0x3E119D3F, 0x3AD08088,	0x2497D08D, 0x2056CD3A, 0x2D15EBE3, 0x29D4F654,
	0xC5A92679, 0xC1683BCE, 0xCC2B1D17, 0xC8EA00A0,	0xD6AD50A5, 0xD26C4D12, 0xDF2F6BCB, 0xDBEE767C,
	0xE3A1CBC1, 0xE760D676, 0xEA23F0AF, 0xEEE2ED18,	0xF0A5BD1D, 0xF464A0AA, 0xF9278673, 0xFDE69BC4, 
	0x89B8FD09, 0x8D79E0BE, 0x803AC667, 0x84FBDBD0,	0x9ABC8BD5, 0x9E7D9662, 0x933EB0BB, 0x97FFAD0C,
	0xAFB010B1, 0xAB710D06, 0xA6322BDF, 0xA2F33668,	0xBCB4666D, 0xB8757BDA, 0xB5365D03, 0xB1F740B4
};

unsigned long _Crc32(int n, BYTE c[])
{
	unsigned long r;

	r = 0xFFFFFFFFUL;
	while (--n >= 0)
		r = (r << CHAR_BIT) ^ crctable[(byte)(r >> (32 - CHAR_BIT)) ^ *c++];
	return r & 0xFFFFFFFFUL;
}

LONGLONG _GetRecSize( DWORD OriginalNetworkID, DWORD TransportStreamID, DWORD ServiceID, BOOL ServiceOnlyFlag, DWORD DurationSecond )
{
	LONGLONG RecSize = 0;
	if( OriginalNetworkID == 4 ){
		//BS
		if( ServiceOnlyFlag == TRUE ){
			if( ServiceID == 101 || ServiceID == 102 ){
				//BS1、BS2
				//9Mbpsで計算
				RecSize = ( 9 * 1024 * 1024 * ((LONGLONG)DurationSecond) ) / 8;
			}if( ServiceID == 910 ){
				//WNI・910は2Mbps
				RecSize = ( 2 * 1024 * 1024 * ((LONGLONG)DurationSecond) ) / 8;
			}else{
				//18Mbpsで計算
				RecSize = ( 18 * 1024 * 1024 * ((LONGLONG)DurationSecond) ) / 8;
			}
		}else{
			//20Mbpsで計算
			RecSize = ( 20 * 1024 * 1024 * ((LONGLONG)DurationSecond) ) / 8;
		}
	}else if( OriginalNetworkID == 6 ){
		//CS1
		if( ServiceOnlyFlag == TRUE ){
			switch( ServiceID ){
				case 239: //日本映画専門ｃｈＨＤ
				case 306: //フジテレビＮＥＸＴ
				case 800: //スカチャンＨＤ８００
				case 55: //ショップチャンネル
					//13Mbpsで計算
					RecSize = ( 13 * 1024 * 1024 * ((LONGLONG)DurationSecond) ) / 8;
					break;
				default:
					//5Mbpsで計算
					RecSize = ( 5 * 1024 * 1024 * ((LONGLONG)DurationSecond) ) / 8;
					break;
			}
		}else{
			//40Mbpsで計算
			RecSize = ( 40 * 1024 * 1024 * ((LONGLONG)DurationSecond) ) / 8;
		}
	}else if( OriginalNetworkID == 7 ){
		//CS2
		if( ServiceOnlyFlag == TRUE ){
			switch( ServiceID ){
				case 240: //ムービープラスＨＤ
				case 253: //ＪスポーツＰｌｕｓＨ
				case 314: //ＬａＬａ　ＨＤ
					//13Mbpsで計算
					RecSize = ( 13 * 1024 * 1024 * ((LONGLONG)DurationSecond) ) / 8;
					break;
				case 257: //日テレＧ＋
				case 262: //ゴルフネットワーク
				case 290: //ＳＫＹ・ＳＴＡＧＥ
				case 300: //日テレプラス
				case 307: //フジテレビＯＮＥ
				case 308: //フジテレビＴＷＯ
				case 333: //ＡＴ－Ｘ
				case 350: //日テレＮＥＷＳ２４
				case 354: //ＣＮＮｊ
					//8Mbpsで計算
					RecSize = ( 5 * 1024 * 1024 * ((LONGLONG)DurationSecond) ) / 8;
					break;
				default:
					//5Mbpsで計算
					RecSize = ( 5 * 1024 * 1024 * ((LONGLONG)DurationSecond) ) / 8;
					break;
			}
		}else{
			//40Mbpsで計算
			RecSize = ( 40 * 1024 * 1024 * ((LONGLONG)DurationSecond) ) / 8;
		}
	}else{
		//地デジ
		if( ServiceOnlyFlag == TRUE ){
			//15Mbpsで計算
			RecSize = ( 15 * 1024 * 1024 * ((LONGLONG)DurationSecond) ) / 8;
		}else{
			//18Mbpsで計算
			RecSize = ( 18 * 1024 * 1024 * ((LONGLONG)DurationSecond) ) / 8;
		}
	}
	return RecSize;
}

BOOL _FindOpenExeProcess(DWORD processID)
{
	HANDLE hSnapshot;
	PROCESSENTRY32 procent;

	BOOL bFind = FALSE;
	/* Toolhelpスナップショットを作成する */
	hSnapshot = CreateToolhelp32Snapshot( TH32CS_SNAPPROCESS,0 );
	if ( hSnapshot != (HANDLE)-1 ) {
		procent.dwSize = sizeof(PROCESSENTRY32);
		if ( Process32First( hSnapshot,&procent ) != FALSE ){
			do {
				if( procent.th32ProcessID == processID ){
					bFind = TRUE;
					break;
				}
			} while ( Process32Next( hSnapshot,&procent ) != FALSE );
		}
		CloseHandle( hSnapshot );
	}
	return bFind;
}

DWORD _BCDtoDWORD(BYTE* data, BYTE size, BYTE digit)
{
	if( data == NULL || (size<<1) < digit ){
		return 0;
	}
	DWORD value = 0;
	for( BYTE i=0; i<digit; i++ ){
		value = value*10;
		if( (i & 0x1) == 0 ){
			value += (data[i>>1]&0xF0)>>4;
		}else{
			value += (data[i>>1]&0x0F);
		}
	}
	return value;
}

BOOL _MJDtoYMD(DWORD mjd, WORD* y, WORD* m, WORD* d)
{
	if( y == NULL || m == NULL || d == NULL ){
		return FALSE;
	}
	
	int yy = (int)( ((double)mjd-15078.2)/365.25 );
	int mm = (int)( ((double)mjd-14956.1-(int)(yy*365.25))/30.6001 );
	*d = (WORD)( mjd-14956-(int)(yy*365.25)-(int)(mm*30.6001) );
	WORD k=0;
	if( mm == 14 || mm == 15 ){
		k=1;
	}

	*y = yy + k;
	*m = mm-1-k*12;

	return TRUE;
}

BOOL _MJDtoSYSTEMTIME(DWORD mjd, SYSTEMTIME* time)
{
	if( time == NULL ){
		return FALSE;
	}

	FILETIME fileTime;
	SYSTEMTIME mjdTime;
	LONGLONG oneDay = 24*60*60*(LONGLONG)10000000;

	ZeroMemory(&mjdTime, sizeof(SYSTEMTIME));
	mjdTime.wYear = 1858;
	mjdTime.wMonth = 11;
	mjdTime.wDay = 17;

	SystemTimeToFileTime(&mjdTime, &fileTime);
	LONGLONG tempTime = ((LONGLONG)fileTime.dwHighDateTime)<<32 | fileTime.dwLowDateTime;
	tempTime += (LONGLONG)mjd*oneDay;

	fileTime.dwLowDateTime = (DWORD)(tempTime&0x00000000FFFFFFFF);
	fileTime.dwHighDateTime = (DWORD)((tempTime&0xFFFFFFFF00000000)>>32);

	FileTimeToSystemTime(&fileTime, time);

	return TRUE;
}

BOOL _GetBitrate(WORD ONID, WORD TSID, WORD SID, DWORD* bitrate)
{
	wstring iniPath;
	GetModuleFolderPath(iniPath);

	iniPath += L"\\Bitrate.ini";

	wstring defKey = L"FFFFFFFFFFFF";
	wstring defNWKey = L"";
	Format(defNWKey, L"%04XFFFFFFFF", ONID);
	wstring defTSKey = L"";
	Format(defTSKey, L"%04X%04XFFFF", ONID, TSID);
	wstring key = L"";
	Format(key, L"%04X%04X%04X", ONID, TSID, SID);

	int defRate = GetPrivateProfileInt(L"BITRATE", defKey.c_str(), 0, iniPath.c_str());
	int defNWRate = GetPrivateProfileInt(L"BITRATE", defNWKey.c_str(), 0, iniPath.c_str());
	int defTSRate = GetPrivateProfileInt(L"BITRATE", defTSKey.c_str(), 0, iniPath.c_str());
	int rate = GetPrivateProfileInt(L"BITRATE", key.c_str(), 0, iniPath.c_str());

	if( rate != 0 ){
		*bitrate = (DWORD)rate;
		return TRUE;
	}
	if( defTSRate != 0 ){
		*bitrate = (DWORD)defTSRate;
		return TRUE;
	}
	if( defNWRate != 0 ){
		*bitrate = (DWORD)defNWRate;
		return TRUE;
	}
	if( defRate != 0 ){
		*bitrate = (DWORD)defRate;
		return TRUE;
	}
	*bitrate = 19*1024;
	return TRUE;
}

//EPG情報をTextに変換
void _ConvertEpgInfoText(EPGDB_EVENT_INFO* info, wstring& text)
{
	text = L"";
	if( info == NULL ){
		return ;
	}

	wstring time=L"未定";
	if( info->StartTimeFlag == TRUE && info->DurationFlag == TRUE ){
		GetTimeString3(info->start_time, info->durationSec, time);
	}else if( info->StartTimeFlag == TRUE && info->DurationFlag == FALSE ){
		GetTimeString4(info->start_time, time);
		time += L" ～ 未定";
	}
	text += time;
	text += L"\r\n";

	if(info->shortInfo != NULL ){
		text += info->shortInfo->event_name;
		text += L"\r\n\r\n";
		text += info->shortInfo->text_char;
		text += L"\r\n\r\n";
	}

	if(info->extInfo != NULL ){
		text += L"詳細情報\r\n";
		text += info->extInfo->text_char;
		text += L"\r\n\r\n";
	}

	wstring buff = L"";
	Format(buff, L"OriginalNetworkID:%d(0x%04X)\r\nTransportStreamID:%d(0x%04X)\r\nServiceID:%d(0x%04X)\r\nEventID:%d(0x%04X)\r\n",
		info->original_network_id, info->original_network_id,
		info->transport_stream_id, info->transport_stream_id,
		info->service_id, info->service_id,
		info->event_id, info->event_id);
	text += buff;
}

