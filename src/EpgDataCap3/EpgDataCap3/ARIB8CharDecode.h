#pragma once

// MFCで使う時用
/*#ifdef _DEBUG
#undef new
#endif
#include <string>
#include <vector>
#include <map>
#ifdef _DEBUG
#define new DEBUG_NEW
#endif
*/
#include <string>
#include <vector>
#include <map>
using namespace std;

#include "ColorDef.h"

//文字符号集合
//Gセット
#define MF_JIS_KANJI1 0x39 //JIS互換漢字1面
#define MF_JIS_KANJI2 0x3A //JIS互換漢字2面
#define MF_KIGOU 0x3B //追加記号
#define MF_ASCII 0x4A //英数
#define MF_HIRA  0x30 //平仮名
#define MF_KANA  0x31 //片仮名
#define MF_KANJI 0x42 //漢字
#define MF_MOSAIC_A 0x32 //モザイクA
#define MF_MOSAIC_B 0x33 //モザイクB
#define MF_MOSAIC_C 0x34 //モザイクC
#define MF_MOSAIC_D 0x35 //モザイクD
#define MF_PROP_ASCII 0x36 //プロポーショナル英数
#define MF_PROP_HIRA  0x37 //プロポーショナル平仮名
#define MF_PROP_KANA  0x38 //プロポーショナル片仮名
#define MF_JISX_KANA 0x49 //JIX X0201片仮名
//DRCS
#define MF_DRCS_0 0x40 //DRCS-0
#define MF_DRCS_1 0x41 //DRCS-1
#define MF_DRCS_2 0x42 //DRCS-2
#define MF_DRCS_3 0x43 //DRCS-3
#define MF_DRCS_4 0x44 //DRCS-4
#define MF_DRCS_5 0x45 //DRCS-5
#define MF_DRCS_6 0x46 //DRCS-6
#define MF_DRCS_7 0x47 //DRCS-7
#define MF_DRCS_8 0x48 //DRCS-8
#define MF_DRCS_9 0x49 //DRCS-9
#define MF_DRCS_10 0x4A //DRCS-10
#define MF_DRCS_11 0x4B //DRCS-11
#define MF_DRCS_12 0x4C //DRCS-12
#define MF_DRCS_13 0x4D //DRCS-13
#define MF_DRCS_14 0x4E //DRCS-14
#define MF_DRCS_15 0x4F //DRCS-15
#define MF_MACRO 0x70 //マクロ

//符号集合の分類
#define MF_MODE_G 1 //Gセット
#define MF_MODE_DRCS 2 //DRCS
#define MF_MODE_OTHER 3 //その他

static char AsciiTable[][3]={
	"！","”","＃","＄","％","＆","’",
	"（","）","＊","＋","，","－","．","／",
	"０","１","２","３","４","５","６","７",
	"８","９","：","；","＜","＝","＞","？",
	"＠","Ａ","Ｂ","Ｃ","Ｄ","Ｅ","Ｆ","Ｇ",
	"Ｈ","Ｉ","Ｊ","Ｋ","Ｌ","Ｍ","Ｎ","Ｏ",
	"Ｐ","Ｑ","Ｒ","Ｓ","Ｔ","Ｕ","Ｖ","Ｗ",
	"Ｘ","Ｙ","Ｚ","［","￥","］","＾","＿",
	"‘","ａ","ｂ","ｃ","ｄ","ｅ","ｆ","ｇ",
	"ｈ","ｉ","ｊ","ｋ","ｌ","ｍ","ｎ","ｏ",
	"ｐ","ｑ","ｒ","ｓ","ｔ","ｕ","ｖ","ｗ",
	"ｘ","ｙ","ｚ","｛","｜","｝","￣"
};
static char HiraTable[][3]={
	"ぁ","あ","ぃ","い","ぅ","う","ぇ",
	"え","ぉ","お","か","が","き","ぎ","く",
	"ぐ","け","げ","こ","ご","さ","ざ","し",
	"じ","す","ず","せ","ぜ","そ","ぞ","た",
	"だ","ち","ぢ","っ","つ","づ","て","で",
	"と","ど","な","に","ぬ","ね","の","は",
	"ば","ぱ","ひ","び","ぴ","ふ","ぶ","ぷ",
	"へ","べ","ぺ","ほ","ぼ","ぽ","ま","み",
	"む","め","も","ゃ","や","ゅ","ゆ","ょ",
	"よ","ら","り","る","れ","ろ","ゎ","わ",
	"ゐ","ゑ","を","ん","　","　","　","ゝ",
	"ゞ","ー","。","「","」","、","・"
};
static char KanaTable[][3]={
	"ァ","ア","ィ","イ","ゥ","ウ","ェ",
	"エ","ォ","オ","カ","ガ","キ","ギ","ク",
	"グ","ケ","ゲ","コ","ゴ","サ","ザ","シ",
	"ジ","ス","ズ","セ","ゼ","ソ","ゾ","タ",
	"ダ","チ","ヂ","ッ","ツ","ヅ","テ","デ",
	"ト","ド","ナ","ニ","ヌ","ネ","ノ","ハ",
	"バ","パ","ヒ","ビ","ピ","フ","ブ","プ",
	"ヘ","ベ","ペ","ホ","ボ","ポ","マ","ミ",
	"ム","メ","モ","ャ","ヤ","ュ","ユ","ョ",
	"ヨ","ラ","リ","ル","レ","ロ","ヮ","ワ",
	"ヰ","ヱ","ヲ","ン","ヴ","ヵ","ヶ","ヽ",
	"ヾ","ー","。","「","」","、","・"
};

typedef struct _GAIJI_TABLE{
	unsigned short usARIB8;
	string strChar;
} GAIJI_TABLE;

static GAIJI_TABLE GaijiTable[]={
	{0x7A4D, "10."},
	{0x7A4E, "11."},
	{0x7A4F, "12."},
	{0x7A50, "[HV]"}, //90区48点
	{0x7A51, "[SD]"},
	{0x7A52, "[Ｐ]"},
	{0x7A53, "[Ｗ]"},
	{0x7A54, "[MV]"},
	{0x7A55, "[手]"},
	{0x7A56, "[字]"},
	{0x7A57, "[双]"},
	{0x7A58, "[デ]"},
	{0x7A59, "[Ｓ]"},
	{0x7A5A, "[二]"},
	{0x7A5B, "[多]"},
	{0x7A5C, "[解]"},
	{0x7A5D, "[SS]"},
	{0x7A5E, "[Ｂ]"},
	{0x7A5F, "[Ｎ]"},//
	{0x7A60, "■"},//90区64点
	{0x7A61, "●"},
	{0x7A62, "[天]"},
	{0x7A63, "[交]"},
	{0x7A64, "[映]"},
	{0x7A65, "[無]"},
	{0x7A66, "[料]"},
	{0x7A67, "[・]"},
	{0x7A68, "[前]"},
	{0x7A69, "[後]"},
	{0x7A6A, "[再]"},
	{0x7A6B, "[新]"},
	{0x7A6C, "[初]"},
	{0x7A6D, "[終]"},
	{0x7A6E, "[生]"},
	{0x7A6F, "[販]"},
	{0x7A70, "[声]"},//90区80点
	{0x7A71, "[吹]"},
	{0x7A72, "[PPV]"},
	{0x7A73, "(秘)"},
	{0x7A74, "ほか"},
	//91区は飛ばす
	{0x7C21, "→"},//92区1点
	{0x7C22, "←"},
	{0x7C23, "↑"},
	{0x7C24, "↓"},
	{0x7C25, "・"},
	{0x7C26, "・"},
	{0x7C27, "年"},
	{0x7C28, "月"},
	{0x7C29, "日"},
	{0x7C2A, "円"},
	{0x7C2B, "m^2"},
	{0x7C2C, "m^3"},
	{0x7C2D, "cm"},
	{0x7C2E, "cm^2"},
	{0x7C2F, "cm^3"},
	{0x7C30, "０."},//92区16点
	{0x7C31, "１."},
	{0x7C32, "２."},
	{0x7C33, "３."},
	{0x7C34, "４."},
	{0x7C35, "５."},
	{0x7C36, "６."},
	{0x7C37, "７."},
	{0x7C38, "８."},
	{0x7C39, "９."},
	{0x7C3A, "氏"},
	{0x7C3B, "副"},
	{0x7C3C, "元"},
	{0x7C3D, "故"},
	{0x7C3E, "前"},
	{0x7C3F, "後"},
	{0x7C40, "０,"},//92区32点
	{0x7C41, "１,"},
	{0x7C42, "２,"},
	{0x7C43, "３,"},
	{0x7C44, "４,"},
	{0x7C45, "５,"},
	{0x7C46, "６,"},
	{0x7C47, "７,"},
	{0x7C48, "８,"},
	{0x7C49, "９,"},
	{0x7C4A, "[社]"},
	{0x7C4B, "[財]"},
	{0x7C4C, "[有]"},
	{0x7C4D, "[株]"},
	{0x7C4E, "[代]"},
	{0x7C4F, "(問)"},
	{0x7C50, "・"},//92区48点
	{0x7C51, "・"},
	{0x7C52, "・"},
	{0x7C53, "・"},
	{0x7C54, "・"},
	{0x7C55, "・"},
	{0x7C56, "・"},
	{0x7C57, "(CD)"},
	{0x7C58, "(vn)"},
	{0x7C59, "(ob)"},
	{0x7C5A, "(cb)"},
	{0x7C5B, "(ce"},
	{0x7C5C, "mb)"},
	{0x7C5D, "(hp)"},
	{0x7C5E, "(br)"},
	{0x7C5F, "(ｐ)"},
	{0x7C60, "(ｓ)"},//92区64点
	{0x7C61, "(ms)"},
	{0x7C62, "(ｔ)"},
	{0x7C63, "(bs)"},
	{0x7C64, "(ｂ)"},
	{0x7C65, "(tb)"},
	{0x7C66, "(tp)"},
	{0x7C67, "(ds)"},
	{0x7C68, "(ag)"},
	{0x7C69, "(eg)"},
	{0x7C6A, "(vo)"},
	{0x7C6B, "(fl)"},
	{0x7C6C, "(ke"},
	{0x7C6D, "y)"},
	{0x7C6E, "(sa"},
	{0x7C6F, "x)"},
	{0x7C70, "(sy"},//92区80点
	{0x7C71, "n)"},
	{0x7C72, "(or"},
	{0x7C73, "g)"},
	{0x7C74, "(pe"},
	{0x7C75, "r)"},
	{0x7C76, "(Ｒ)"},
	{0x7C77, "(Ｃ)"},
	{0x7C78, "(箏)"},
	{0x7C79, "ＤＪ"},
	{0x7C7A, "[演]"},
	{0x7C7B, "Fax"},
	{0x7D21, "(月)"},//93区1点
	{0x7D22, "(火)"},
	{0x7D23, "(水)"},
	{0x7D24, "(木)"},
	{0x7D25, "(金)"},
	{0x7D26, "(土)"},
	{0x7D27, "(日)"},
	{0x7D28, "(祝)"},
	{0x7D29, "㍾"},
	{0x7D2A, "㍽"},
	{0x7D2B, "㍼"},
	{0x7D2C, "㍻"},
	{0x7D2D, "No."},
	{0x7D2E, "Tel"},
	{0x7D2F, "(〒)"},
	{0x7D30, "()()"},//93区16点
	{0x7D31, "[本]"},
	{0x7D32, "[三]"},
	{0x7D33, "[二]"},
	{0x7D34, "[安]"},
	{0x7D35, "[点]"},
	{0x7D36, "[打]"},
	{0x7D37, "[盗]"},
	{0x7D38, "[勝]"},
	{0x7D39, "[敗]"},
	{0x7D3A, "[Ｓ]"},
	{0x7D3B, "[投]"},
	{0x7D3C, "[捕]"},
	{0x7D3D, "[一]"},
	{0x7D3E, "[二]"},
	{0x7D3F, "[三]"},
	{0x7D40, "[遊]"},//93区32点
	{0x7D41, "[左]"},
	{0x7D42, "[中]"},
	{0x7D43, "[右]"},
	{0x7D44, "[指]"},
	{0x7D45, "[走]"},
	{0x7D46, "[打]"},
	{0x7D47, "l"},
	{0x7D48, "kg"},
	{0x7D49, "Hz"},
	{0x7D4A, "ha"},
	{0x7D4B, "km"},
	{0x7D4C, "km^2"},
	{0x7D4D, "hPa"},
	{0x7D4E, "・"},
	{0x7D4F, "・"},
	{0x7D50, "1/2"},//93区48点
	{0x7D51, "0/3"},
	{0x7D52, "1/3"},
	{0x7D53, "2/3"},
	{0x7D54, "3/3"},
	{0x7D55, "1/4"},
	{0x7D56, "3/4"},
	{0x7D57, "1/5"},
	{0x7D58, "2/5"},
	{0x7D59, "3/5"},
	{0x7D5A, "4/5"},
	{0x7D5B, "1/6"},
	{0x7D5C, "1/7"},
	{0x7D5D, "1/8"},
	{0x7D5E, "1/9"},
	{0x7D5F, "1/10"},
	{0x7D6E, "!!"},//93区78点
	{0x7D6F, "!?"},
	{0x7E21, "Ⅰ"},//94区1点
	{0x7E22, "Ⅱ"},
	{0x7E23, "Ⅲ"},
	{0x7E24, "Ⅳ"},
	{0x7E25, "Ⅴ"},
	{0x7E26, "Ⅵ"},
	{0x7E27, "Ⅶ"},
	{0x7E28, "Ⅷ"},
	{0x7E29, "Ⅸ"},
	{0x7E2A, "Ⅹ"},
	{0x7E2B, "XI"},
	{0x7E2C, "XII"},
	{0x7E2D, "⑰"},
	{0x7E2E, "⑱"},
	{0x7E2F, "⑲"},
	{0x7E30, "⑳"},//94区16点
	{0x7E31, "(１)"},
	{0x7E32, "(２)"},
	{0x7E33, "(３)"},
	{0x7E34, "(４)"},
	{0x7E35, "(５)"},
	{0x7E36, "(６)"},
	{0x7E37, "(７)"},
	{0x7E38, "(８)"},
	{0x7E39, "(９)"},
	{0x7E3A, "(10)"},
	{0x7E3B, "(11)"},
	{0x7E3C, "(12)"},
	{0x7E3D, "(21)"},
	{0x7E3E, "(22)"},
	{0x7E3F, "(23)"},
	{0x7E40, "(24)"},//94区32点
	{0x7E41, "(Ａ)"},
	{0x7E42, "(Ｂ)"},
	{0x7E43, "(Ｃ)"},
	{0x7E44, "(Ｄ)"},
	{0x7E45, "(Ｅ)"},
	{0x7E46, "(Ｆ)"},
	{0x7E47, "(Ｇ)"},
	{0x7E48, "(Ｈ)"},
	{0x7E49, "(Ｉ)"},
	{0x7E4A, "(Ｊ)"},
	{0x7E4B, "(Ｋ)"},
	{0x7E4C, "(Ｌ)"},
	{0x7E4D, "(Ｍ)"},
	{0x7E4E, "(Ｎ)"},
	{0x7E4F, "(Ｏ)"},
	{0x7E50, "(Ｐ)"},//94区48点
	{0x7E51, "(Ｑ)"},
	{0x7E52, "(Ｒ)"},
	{0x7E53, "(Ｓ)"},
	{0x7E54, "(Ｔ)"},
	{0x7E55, "(Ｕ)"},
	{0x7E56, "(Ｖ)"},
	{0x7E57, "(Ｗ)"},
	{0x7E58, "(Ｘ)"},
	{0x7E59, "(Ｙ)"},
	{0x7E5A, "(Ｚ)"},
	{0x7E5B, "(25)"},
	{0x7E5C, "(26)"},
	{0x7E5D, "(27)"},
	{0x7E5E, "(28)"},
	{0x7E5F, "(29)"},
	{0x7E60, "(30)"},//94区64点
	{0x7E61, "①"},
	{0x7E62, "②"},
	{0x7E63, "③"},
	{0x7E64, "④"},
	{0x7E65, "⑤"},
	{0x7E66, "⑥"},
	{0x7E67, "⑦"},
	{0x7E68, "⑧"},
	{0x7E69, "⑨"},
	{0x7E6A, "⑩"},
	{0x7E6B, "⑪"},
	{0x7E6C, "⑫"},
	{0x7E6D, "⑬"},
	{0x7E6E, "⑭"},
	{0x7E6F, "⑮"},
	{0x7E70, "⑯"},//94区80点
	{0x7E71, "(１)"},
	{0x7E72, "(２)"},
	{0x7E73, "(３)"},
	{0x7E74, "(４)"},
	{0x7E75, "(５)"},
	{0x7E76, "(６)"},
	{0x7E77, "(７)"},
	{0x7E78, "(８)"},
	{0x7E79, "(９)"},
	{0x7E7A, "(10)"},
	{0x7E7B, "(11)"},
	{0x7E7C, "(12)"},
	{0x7E7D, "(31)"}
};

static GAIJI_TABLE GaijiTbl2[]={
	{0x7521, "〓"},
	{0x7522, "〓"},
	{0x7523, "〓"},
	{0x7524, "〓"},
	{0x7525, "侚"},
	{0x7526, "俉"},
	{0x7527, "〓"},
	{0x7528, "〓"},
	{0x7529, "〓"},
	{0x752A, "〓"}, //10
	{0x752B, "匇"},
	{0x752C, "〓"},
	{0x752D, "〓"},
	{0x752E, "詹"},
	{0x752F, "〓"},
	{0x7530, "〓"},
	{0x7531, "〓"},
	{0x7532, "〓"},
	{0x7533, "咩"},
	{0x7534, "〓"}, //20
	{0x7535, "〓"},
	{0x7536, "〓"},
	{0x7537, "〓"},
	{0x7538, "〓"},
	{0x7539, "〓"},
	{0x753A, "塚"},
	{0x753B, "〓"},
	{0x753C, "〓"},
	{0x753D, "〓"},
	{0x753E, "〓"}, //30
	{0x753F, "寬"},
	{0x7540, "﨑"},
	{0x7541, "〓"},
	{0x7542, "〓"},
	{0x7543, "弴"},
	{0x7544, "彅"},
	{0x7545, "德"},
	{0x7546, "〓"},
	{0x7547, "〓"},
	{0x7548, "愰"}, //40
	{0x7549, "昤"},
	{0x754A, "〓"},
	{0x754B, "曙"},
	{0x754C, "曺"},
	{0x754D, "曻"},
	{0x754E, "〓"},
	{0x754F, "〓"},
	{0x7550, "〓"},
	{0x7551, "〓"},
	{0x7552, "〓"}, //50
	{0x7553, "〓"},
	{0x7554, "櫛"},
	{0x7555, "〓"},
	{0x7556, "〓"},
	{0x7557, "〓"},
	{0x7558, "〓"},
	{0x7559, "〓"},
	{0x755A, "〓"},
	{0x755B, "〓"},
	{0x755C, "〓"}, //60
	{0x755D, "〓"},
	{0x755E, "〓"},
	{0x755F, "〓"},
	{0x7560, "〓"},
	{0x7561, "〓"},
	{0x7562, "〓"},
	{0x7563, "〓"},
	{0x7564, "〓"},
	{0x7565, "煇"},
	{0x7566, "燁"}, //70
	{0x7567, "〓"},
	{0x7568, "〓"},
	{0x7569, "〓"},
	{0x756A, "珉"},
	{0x756B, "珖"},
	{0x756C, "〓"},
	{0x756D, "〓"},
	{0x756E, "〓"},
	{0x756F, "琦"},
	{0x7570, "琪"}, //80
	{0x7571, "〓"},
	{0x7572, "〓"},
	{0x7573, "〓"},
	{0x7574, "〓"},
	{0x7575, "〓"},
	{0x7576, "〓"},
	{0x7577, "〓"},
	{0x7578, "〓"},
	{0x7579, "〓"},
	{0x757A, "〓"}, //90
	{0x757B, "祇"},
	{0x757C, "禮"},
	{0x757D, "〓"},
	{0x757E, "〓"},
	{0x7621, "〓"},
	{0x7622, "〓"},
	{0x7623, "〓"},
	{0x7624, "〓"},
	{0x7625, "〓"},
	{0x7626, "〓"}, //100
	{0x7627, "〓"},
	{0x7628, "羡"},
	{0x7629, "〓"},
	{0x762A, "〓"},
	{0x762B, "〓"},
	{0x762C, "〓"},
	{0x762D, "葛"},
	{0x762E, "蓜"},
	{0x762F, "蓬"},
	{0x7630, "蕙"}, //110
	{0x7631, "〓"},
	{0x7632, "蝕"},
	{0x7633, "〓"},
	{0x7634, "〓"},
	{0x7635, "裵"},
	{0x7636, "角"},
	{0x7637, "諶"},
	{0x7638, "〓"},
	{0x7639, "辻"},
	{0x763A, "〓"}, //120
	{0x763B, "〓"},
	{0x763C, "鄧"},
	{0x763D, "鄭"},
	{0x763E, "〓"},
	{0x763F, "〓"},
	{0x7640, "銈"},
	{0x7641, "錡"},
	{0x7642, "鍈"},
	{0x7643, "閒"},
	{0x7644, "〓"}, //130
	{0x7645, "餃"},
	{0x7646, "〓"},
	{0x7647, "髙"},
	{0x7648, "鯖"},
	{0x7649, "〓"},
	{0x764A, "〓"},
	{0x764B, "〓"}
};

static BYTE DefaultMacro0[]={
	0x1B,0x24,0x39,0x1B,0x29,0x4A,0x1B,0x2A,0x30,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};
static BYTE DefaultMacro1[]={
	0x1B,0x24,0x39,0x1B,0x29,0x31,0x1B,0x2A,0x30,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};
static BYTE DefaultMacro2[]={
	0x1B,0x24,0x39,0x1B,0x29,0x20,0x40,0x1B,0x2A,0x30,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};
static BYTE DefaultMacro3[]={
	0x1B,0x28,0x32,0x1B,0x29,0x34,0x1B,0x2A,0x35,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};
static BYTE DefaultMacro4[]={
	0x1B,0x28,0x32,0x1B,0x29,0x33,0x1B,0x2A,0x35,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};
static BYTE DefaultMacro5[]={
	0x1B,0x28,0x32,0x1B,0x29,0x20,0x40,0x1B,0x2A,0x35,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};
static BYTE DefaultMacro6[]={
	0x1B,0x28,0x20,0x41,0x1B,0x29,0x20,0x42,0x1B,0x2A,0x20,0x43,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};
static BYTE DefaultMacro7[]={
	0x1B,0x28,0x20,0x44,0x1B,0x29,0x20,0x45,0x1B,0x2A,0x20,0x46,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};
static BYTE DefaultMacro8[]={
	0x1B,0x28,0x20,0x47,0x1B,0x29,0x20,0x48,0x1B,0x2A,0x20,0x49,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};
static BYTE DefaultMacro9[]={
	0x1B,0x28,0x20,0x4A,0x1B,0x29,0x20,0x4B,0x1B,0x2A,0x20,0x4C,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};
static BYTE DefaultMacroA[]={
	0x1B,0x28,0x20,0x4D,0x1B,0x29,0x20,0x4E,0x1B,0x2A,0x20,0x4F,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};
static BYTE DefaultMacroB[]={
	0x1B,0x24,0x39,0x1B,0x29,0x20,0x42,0x1B,0x2A,0x30,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};
static BYTE DefaultMacroC[]={
	0x1B,0x24,0x39,0x1B,0x29,0x20,0x43,0x1B,0x2A,0x30,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};
static BYTE DefaultMacroD[]={
	0x1B,0x24,0x39,0x1B,0x29,0x20,0x44,0x1B,0x2A,0x30,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};
static BYTE DefaultMacroE[]={
	0x1B,0x28,0x31,0x1B,0x29,0x30,0x1B,0x2A,0x4A,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};
static BYTE DefaultMacroF[]={
	0x1B,0x28,0x4A,0x1B,0x29,0x32,0x1B,0x2A,0x20,0x41,0x1B,0x2B,0x20,0x70,0x0F,0x1B,0x7D
};

//文字サイズ
typedef enum{
	STR_SMALL = 0, //SSZ
	STR_MEDIUM, //MSZ
	STR_NORMAL, //NSZ
	STR_MICRO, //SZX 0x60
	STR_HIGH_W, //SZX 0x41
	STR_WIDTH_W, //SZX 0x44
	STR_W, //SZX 0x45
	STR_SPECIAL_1, //SZX 0x6B
	STR_SPECIAL_2, //SZX 0x64
} STRING_SIZE;

typedef struct _CAPTION_CHAR_DATA{
	string strDecode;
	STRING_SIZE emCharSizeMode;

	CLUT_DAT stCharColor;
	CLUT_DAT stBackColor;
	CLUT_DAT stRasterColor;

	BOOL bUnderLine;
	BOOL bShadow;
	BOOL bBold;
	BOOL bItalic;
	BYTE bFlushMode;

	WORD wCharW;
	WORD wCharH;
	WORD wCharHInterval;
	WORD wCharVInterval;
	//=オペレーターの処理
	_CAPTION_CHAR_DATA & operator= (const _CAPTION_CHAR_DATA & o) {
		strDecode=o.strDecode;
		emCharSizeMode = o.emCharSizeMode;
		stCharColor = o.stCharColor;
		stBackColor = o.stBackColor;
		stRasterColor = o.stRasterColor;
		bUnderLine = o.bUnderLine;
		bShadow = o.bShadow;
		bBold = o.bBold;
		bItalic = o.bItalic;
		bFlushMode = o.bFlushMode;
		wCharW = o.wCharH;
		wCharHInterval = o.wCharHInterval;
		wCharVInterval = o.wCharVInterval;
		return *this;
	};
} CAPTION_CHAR_DATA;

typedef struct _CAPTION_DATA{
	BOOL bClear;
	WORD wSWFMode;
	WORD wClientX;
	WORD wClientY;
	WORD wClientW;
	WORD wClientH;
	WORD wPosX;
	WORD wPosY;
	vector<CAPTION_CHAR_DATA> CharList;
	DWORD dwWaitTime;
	//=オペレーターの処理
	_CAPTION_DATA & operator= (const _CAPTION_DATA & o) {
		bClear=o.bClear;
		wSWFMode = o.wSWFMode;
		wClientX = o.wClientX;
		wClientY = o.wClientY;
		wClientW = o.wClientW;
		wClientH = o.wClientH;
		wPosX = o.wPosX;
		wPosY = o.wPosY;
		CharList = o.CharList;
		dwWaitTime = o.dwWaitTime;
		return *this;
	};
} CAPTION_DATA;

class CARIB8CharDecode
{
public:
	CARIB8CharDecode(void);
	~CARIB8CharDecode(void);

	//PSI/SIを想定したSJISへの変換
	BOOL PSISI( const BYTE* pbSrc, DWORD dwSrcSize, string* strDec );
	//字幕を想定したSJISへの変換
	BOOL Caption( const BYTE* pbSrc, DWORD dwSrcSize, vector<CAPTION_DATA>* pCaptionList );

protected:
	typedef struct _MF_MODE{
		int iMF; //文字符号集合
		int iMode; //符号集合の分類
		int iByte; //読み込みバイト数
		//=オペレーターの処理
		_MF_MODE & operator= (const _MF_MODE & o) {
			iMF = o.iMF;
			iMode = o.iMode;
			iByte = o.iByte;
			return *this;
		}
	} MF_MODE;

	BOOL m_bPSI;

	MF_MODE m_G0;
	MF_MODE m_G1;
	MF_MODE m_G2;
	MF_MODE m_G3;
	MF_MODE* m_GL;
	MF_MODE* m_GR;

	//デコードした文字列
	string m_strDecode;
	//文字サイズ
	STRING_SIZE m_emStrSize;

	//CLUTのインデックス
	BYTE m_bCharColorIndex;
	BYTE m_bBackColorIndex;
	BYTE m_bRasterColorIndex;
	BYTE m_bDefPalette;

	BOOL m_bUnderLine;
	BOOL m_bShadow;
	BOOL m_bBold;
	BOOL m_bItalic;
	BYTE m_bFlushMode;

	//表示書式
	WORD m_wSWFMode;
	WORD m_wClientX;
	WORD m_wClientY;
	WORD m_wClientW;
	WORD m_wClientH;
	WORD m_wPosX;
	WORD m_wPosY;
	WORD m_wCharW;
	WORD m_wCharH;
	WORD m_wCharHInterval;
	WORD m_wCharVInterval;
	WORD m_wMaxChar;

	DWORD m_dwWaitTime;

	vector<CAPTION_DATA>* m_pCaptionList;
protected:
	void InitPSISI(void);
	void InitCaption(void);
	BOOL Analyze( const BYTE* pbSrc, DWORD dwSrcSize, DWORD* pdwReadSize );

	BOOL IsSmallCharMode(void);
	BOOL IsChgPos(void);
	void CreateCaptionData(CAPTION_DATA* pItem);
	void CreateCaptionCharData(CAPTION_CHAR_DATA* pItem);
	void CheckModify(void);

	//制御符号
	BOOL C0( const BYTE* pbSrc, DWORD* pdwReadSize );
	BOOL C1( const BYTE* pbSrc, DWORD* pdwReadSize );
	BOOL GL( const BYTE* pbSrc, DWORD* pdwReadSize );
	BOOL GR( const BYTE* pbSrc, DWORD* pdwReadSize );
	//シングルシフト
	BOOL SS2( const BYTE* pbSrc, DWORD* pdwReadSize );
	BOOL SS3( const BYTE* pbSrc, DWORD* pdwReadSize );
	//エスケープシーケンス
	BOOL ESC( const BYTE* pbSrc, DWORD* pdwReadSize );
	//２バイト文字変換
	BOOL ToSJIS( const BYTE bFirst, const BYTE bSecond );
	BOOL ToCustomFont( const BYTE bFirst, const BYTE bSecond );

	BOOL CSI( const BYTE* pbSrc, DWORD* pdwReadSize );

};
