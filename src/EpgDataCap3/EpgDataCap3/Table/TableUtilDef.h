#ifndef __TABLE_UTIL_DEF_H__
#define __TABLE_UTIL_DEF_H__

#include "BATTable.h"
#include "CATTable.h"
#include "EITTable.h"
#include "NITTable.h"
#include "PATTable.h"
#include "PMTTable.h"
#include "SDTTable.h"
#include "TDTTable.h"
#include "TOTTable.h"
#include "CDTTable.h"
#include "SDTTTable.h"
#include "BITTable.h"
#include "SITTable.h"

typedef struct _TABLE_DATA{
	CBATTable* BATTable;
	CCATTable* CATTable;
	CEITTable* EITTable;
	CNITTable* NITTable;
	CPATTable* PATTable;
	CPMTTable* PMTTable;
	CSDTTable* SDTTable;
	CTDTTable* TDTTable;
	CTOTTable* TOTTable;
	CCDTTable* CDTTable;
	CSDTTTable* SDTTTable;
	CBITTable* BITTable;
	CSITTable* SITTable;

	_TABLE_DATA(void){
		BATTable = NULL;
		CATTable = NULL;
		EITTable = NULL;
		NITTable = NULL;
		PATTable = NULL;
		PMTTable = NULL;
		SDTTable = NULL;
		TDTTable = NULL;
		TOTTable = NULL;
		CDTTable = NULL;
		SDTTTable = NULL;
		BITTable = NULL;
		SITTable = NULL;
	};
	~_TABLE_DATA(void){
		SAFE_DELETE(BATTable);
		SAFE_DELETE(CATTable);
		SAFE_DELETE(EITTable);
		SAFE_DELETE(NITTable);
		SAFE_DELETE(PATTable);
		SAFE_DELETE(PMTTable);
		SAFE_DELETE(SDTTable);
		SAFE_DELETE(TDTTable);
		SAFE_DELETE(TOTTable);
		SAFE_DELETE(CDTTable);
		SAFE_DELETE(SDTTTable);
		SAFE_DELETE(BITTable);
		SAFE_DELETE(SITTable);
	};
}TABLE_DATA;

#endif
