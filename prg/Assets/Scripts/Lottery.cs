using System;
using System.Linq;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
public class Lottery
{
    const int MAX_LOT_NUM = 10;
    const int MAX_ENSYUTU_NUM = 15;
    const int LOOP_MAX = 1000;

    int RAND_TOKUSYO_1ST;
    int RAND_TOKUSYO_2ND;
    int RAND_TOKUSYO_3RD;
    int RAND_TOKUSYO_4TH;
    int RAND_TOKUSYO_5TH;
    int RAND_TOKUSYO_6TH;
    int RAND_TOKUSYO_7TH;
    int RAND_MAX;

    int RAND_TOKUSYO_1ST_RANGE;
    int RAND_TOKUSYO_2ND_RANGE;
    int RAND_TOKUSYO_3RD_RANGE;
    int RAND_TOKUSYO_4TH_RANGE;
    int RAND_TOKUSYO_5TH_RANGE;
    int RAND_TOKUSYO_6TH_RANGE;
    int RAND_TOKUSYO_7TH_RANGE;

    int RAND_TOKUSYO_MAX;
    int RAND_TOKUSYO_MIN;
    int RAND_ENSYUTU_MAX;
    int RAND_ENSYUTU_MIN;

    //インスタンス
    private static Lottery instance;

    //抽選番号
    private TOKUSYO[] lotNum;

    //演出抽選置値(本来はサーバーから取得する)
    private int[,] sakibare;
    private int[,] bgcolor;
    private int[,] croonCat;
    private int[,] croonPat;
    private int[,] deme;
    private int[,] cutinColor;
    private int[,] cutinType;

    Dictionary<TOKUSYO, int> tokusyoValueList;

    //演出リスト
    private ENSYUTU_TABLE ensyutuTbl;

    //演出番号
    private static int ensyutuNum = 0;

    // 3択チャレンジ
    private int challengeNum;

    public static Lottery GetInstance()
    {
        if (instance == null)
        {
            instance = new Lottery();
        }
        return instance;
    }
    private Lottery()
    {

        tokusyoValueList = new Dictionary<TOKUSYO, int>
        {
            { TOKUSYO.TOKUSYO_1ST, 1 },
            { TOKUSYO.TOKUSYO_2ND, 2 },
            { TOKUSYO.TOKUSYO_3RD, 3 },
            { TOKUSYO.TOKUSYO_4TH, 4 },
            { TOKUSYO.TOKUSYO_5TH, 5 },
            { TOKUSYO.TOKUSYO_6TH, 6 },
            { TOKUSYO.TOKUSYO_7TH, 7 },
        };
        // デフォルト・チャンス
        sakibare = new int[,]
        {
            //7等
            {1000,0},
            //6等
            {1000,0},
            //5等
            {500,500},
            //4等
            {200,800},
            //3等
            {100,900},
            //2等
            {50,950},
            //1等
            {50,950},
        };
        //青・緑・赤
        bgcolor = new int[,]
        {
            //7等
            {1000,0,0},
            //6等
            {1000,0,0},
            //5等
            {500,500,0},
            //4等
            {200,800,0},
            //3等
            {50,100,850},
            //2等
            {50,50,900},
            //1等
            {50,50,900},
        };
        croonPat = new int[,]
        {
            { 100,100,100,100,100,100,100,100,100,100},    //青クルーンパターン(10種類)
            { 100,100,100,100,100,100,100,100,100,100},    //緑クルーンパターン(10種類)
            { 100,100,100,100,100,100,100,100,100,100},    //赤クルーンパターン(10種類)
        };
        //3ライン1/2ライン1/2ライン2/2ライン3/2ライン4/1ライン1/1ライン2/1ライン3/1ライン4/1ライン5/1ライン6
        //多分特賞値でライン数を決めるんだよね？とりあえず置値は適当
        deme = new int[,]
        {
            //7等
            {0,0,0,0,0,166,166,167,167,167,167},
            //6等
            {0,0,0,0,0,166,166,167,167,167,167},
            //5等
            {0,100,100,100,100,100,100,100,100,100,100},
            //4等
            {0,100,100,100,100,100,100,100,100,100,100},
            //3等
            {150,100,100,100,100,100,75,75,75,75,75},
            //2等
            {300,100,100,100,100,50,50,50,50,50,50},
            //1等
            {450,100,100,100,100,25,25,25,25,25,25},
        };
        //カットイン色(青/緑/赤)
        cutinColor = new int[,]
        {
            //7等
            {800,200,0},
            //6等
            {700,300,0},
            //5等
            {500,450,50},
            //4等
            {200,300,500},
            //3等
            {100,200,700},
            //2等
            {50,100,850},
            //1等
            {50,100,850},
        };
        // カットインの種別(12種類)
        cutinType = new int[,]
        {
            //青
            {84,84,84,84,83,83,83,83,83,83,83,83},
            //緑
            {84,84,84,84,83,83,83,83,83,83,83,83},
            //赤
            {84,84,84,84,83,83,83,83,83,83,83,83},
        };
        ensyutuTbl = new ENSYUTU_TABLE();

        RAND_TOKUSYO_1ST = 5;
        RAND_TOKUSYO_2ND = 13;
        RAND_TOKUSYO_3RD = 21;
        RAND_TOKUSYO_4TH = 90;
        RAND_TOKUSYO_5TH = 299;
        RAND_TOKUSYO_6TH = 0;
        RAND_TOKUSYO_7TH = 0;
        RAND_MAX = 0xffff;

        RAND_TOKUSYO_1ST_RANGE = RAND_TOKUSYO_1ST;
        RAND_TOKUSYO_2ND_RANGE = RAND_TOKUSYO_2ND + RAND_TOKUSYO_1ST_RANGE;
        RAND_TOKUSYO_3RD_RANGE = RAND_TOKUSYO_3RD + RAND_TOKUSYO_2ND_RANGE;
        RAND_TOKUSYO_4TH_RANGE = RAND_TOKUSYO_4TH + RAND_TOKUSYO_3RD_RANGE;
        RAND_TOKUSYO_5TH_RANGE = RAND_TOKUSYO_5TH + RAND_TOKUSYO_4TH_RANGE;
        RAND_TOKUSYO_6TH_RANGE = RAND_TOKUSYO_6TH + RAND_TOKUSYO_5TH_RANGE;   // 新規
        RAND_TOKUSYO_7TH_RANGE = RAND_TOKUSYO_7TH + RAND_TOKUSYO_6TH_RANGE;   // 新規

        RAND_TOKUSYO_MAX = RAND_TOKUSYO_1ST + RAND_TOKUSYO_2ND + RAND_TOKUSYO_3RD + RAND_TOKUSYO_4TH + RAND_TOKUSYO_5TH + RAND_TOKUSYO_6TH + RAND_TOKUSYO_7TH;
        RAND_TOKUSYO_MIN = 0;
        RAND_ENSYUTU_MAX = 1000;
        RAND_ENSYUTU_MIN = 0;


    }
    public void SetLotNum(int num)
    {
        lotNum = new TOKUSYO[num];
    }
    public int GetLotNum()
    {
        if (lotNum == null) return 0;
        else return lotNum.Length;
    }
    public void LotTokusyo()
    {
        int rand = 0;
        int loopCnt = 0;
        bool result = false;

        if (lotNum == null) return;

        for (int i = 0; i < lotNum.Length; i++)
        {
            do
            {
                rand = UnityEngine.Random.Range(RAND_TOKUSYO_MIN, RAND_TOKUSYO_MAX);

                if (rand >= RAND_TOKUSYO_MIN && rand < RAND_TOKUSYO_1ST_RANGE && lotNum.Count(t => t == TOKUSYO.TOKUSYO_1ST) > 0)
                {
                    result = true;
                    if (++loopCnt > LOOP_MAX)
                    {
                        result = false;
                        rand = RAND_MAX;
                        break;
                    }
                }
                else result = false;

            } while (result);

            if (rand >= RAND_TOKUSYO_MIN && rand < RAND_TOKUSYO_1ST_RANGE)
            {
                lotNum[i] = TOKUSYO.TOKUSYO_1ST;
                SetTokusyoValue(TOKUSYO.TOKUSYO_1ST, --RAND_TOKUSYO_1ST);
            }
            else if (rand >= (int)RAND_TOKUSYO_1ST_RANGE && rand < RAND_TOKUSYO_2ND_RANGE)
            {
                lotNum[i] = TOKUSYO.TOKUSYO_2ND;
                SetTokusyoValue(TOKUSYO.TOKUSYO_2ND, --RAND_TOKUSYO_2ND);
            }
            else if (rand >= RAND_TOKUSYO_2ND_RANGE && rand < RAND_TOKUSYO_3RD_RANGE)
            {
                lotNum[i] = TOKUSYO.TOKUSYO_3RD;
                SetTokusyoValue(TOKUSYO.TOKUSYO_3RD, --RAND_TOKUSYO_3RD);
            }
            else if (rand >= RAND_TOKUSYO_3RD_RANGE && rand < RAND_TOKUSYO_4TH_RANGE)
            {
                lotNum[i] = TOKUSYO.TOKUSYO_4TH;
                SetTokusyoValue(TOKUSYO.TOKUSYO_4TH, --RAND_TOKUSYO_4TH);
            }
            else if (rand >= RAND_TOKUSYO_4TH_RANGE && rand < RAND_TOKUSYO_5TH_RANGE)
            {
                lotNum[i] = TOKUSYO.TOKUSYO_5TH;
                SetTokusyoValue(TOKUSYO.TOKUSYO_5TH, --RAND_TOKUSYO_5TH);
            }
            else if (rand >= RAND_TOKUSYO_5TH_RANGE && rand < RAND_TOKUSYO_6TH_RANGE)
            {
                lotNum[i] = TOKUSYO.TOKUSYO_6TH;
                SetTokusyoValue(TOKUSYO.TOKUSYO_6TH, --RAND_TOKUSYO_6TH);
            }
            else if (rand >= RAND_TOKUSYO_6TH_RANGE && rand < RAND_TOKUSYO_7TH_RANGE)
            {
                lotNum[i] = TOKUSYO.TOKUSYO_7TH;
                SetTokusyoValue(TOKUSYO.TOKUSYO_7TH, --RAND_TOKUSYO_7TH);
            }
            else  // rand ≥ RAND_TOKUSYO_7TH_RANGE
            {
                lotNum[i] = TOKUSYO.TOKUSYO_7TH;  // ← 元々の5等用
                SetTokusyoValue(TOKUSYO.TOKUSYO_7TH, --RAND_TOKUSYO_7TH);
            }
        }

        lotNum = lotNum.OrderByDescending(x => x).ToArray();
#if UNITY_EDITOR
        for (int i = 0; i < lotNum.Length; i++)
        {
            Debug.Log("lotNum[" + i + "]:" + lotNum[i]);
        }
#endif
    }
    public int LotEnsyutuDetail(ENSYUTU_TYPE type)
    {
        TOKUSYO tokusyo = GetTokuyoMax();
        int[,] tbl = null;

        switch (type)
        {
            case ENSYUTU_TYPE.TYPE_SAKIBARE:
                tbl = sakibare;
                return LotBase(tbl, (int)tokusyo - 1);

            case ENSYUTU_TYPE.TYPE_BGCOLOR:
                tbl = bgcolor;
                return LotBase(tbl, (int)tokusyo - 1);

            case ENSYUTU_TYPE.TYPE_CROON_CATEGORY:
                int ordinal = tokusyoValueList[tokusyo];
                int rankIndex = ordinal - 1;

                int colorOffset = ensyutuTbl.bgcolor == ENSYUTU_BGCOLOR.BLUE ? (int)CROON_HOLE.BLUE         // 青: Blue_1…Blue_7
                                 : ensyutuTbl.bgcolor == ENSYUTU_BGCOLOR.GREEN ? (int)CROON_HOLE.GREEN      // 緑: Green_1…Green_5
                                 : (int)CROON_HOLE.RED;                                                     // 赤: Red_1…Red_3

                //カテゴリ番号合成
                return colorOffset + rankIndex;

            case ENSYUTU_TYPE.TYPE_CROON_PATTERN:
                tbl = croonPat;
                return LotBase(tbl, (int)ensyutuTbl.bgcolor);

            case ENSYUTU_TYPE.TYPE_DEME:
                // 0) 背景色を取得 (0=青, 1=緑, 2=赤)
                int bgColorValue = (int)ensyutuTbl.bgcolor;

                // 1) bgColor に応じて出目を決定
                //    ・bgcolor=0 → 5～10 のいずれか
                //    ・bgcolor=1 → 1～4 のいずれか
                //    ・bgcolor=2 → 0
                int demeValue;
                if (bgColorValue == 0)
                {
                    demeValue = UnityEngine.Random.Range(5, 11);

                }
                else if (bgColorValue == 1)
                {
                    demeValue = UnityEngine.Random.Range(1, 5);
                }
                else
                {
                    demeValue = 0;
                }

                return demeValue;

            case ENSYUTU_TYPE.TYPE_CUTINCOLOR:
                tbl = cutinColor;
                return LotBase(tbl, (int)tokusyo - 1);

            case ENSYUTU_TYPE.TYPE_CUTINTYPE:
                tbl = cutinType;
                return LotBase(tbl, (int)ensyutuTbl.cutinColor);

            default:
                return -1;
        }
    }
    private int LotBase(int[,] tbl, int value)
    {
        int selectedValue = -1;
        int rand = UnityEngine.Random.Range(RAND_ENSYUTU_MIN, RAND_ENSYUTU_MAX);

        for (int i = 0; i < tbl.GetLength(1); i++)
        {
            rand -= tbl[value, i];
            if (rand < 0)
            {
                selectedValue = i;
                break;
            }
        }
        return selectedValue;
    }
    public void LotEnsyutu()
    {
        if (lotNum == null) return;

        ensyutuTbl.sakibare = (ENSYUTU_SAKIBARE)LotEnsyutuDetail(ENSYUTU_TYPE.TYPE_SAKIBARE);

        ensyutuTbl.bgcolor = (ENSYUTU_BGCOLOR)LotEnsyutuDetail(ENSYUTU_TYPE.TYPE_BGCOLOR);

        ensyutuTbl.croonCat = (ENSYUTU_CROON_CATRGORY)LotEnsyutuDetail(ENSYUTU_TYPE.TYPE_CROON_CATEGORY);

        ensyutuTbl.croonPat = (ENSYUTU_CROON_PATTERN)LotEnsyutuDetail(ENSYUTU_TYPE.TYPE_CROON_PATTERN);

        ensyutuTbl.deme = (ENSYUTU_DEME)LotEnsyutuDetail(ENSYUTU_TYPE.TYPE_DEME);

        ensyutuTbl.cutinColor = (ENSYUTU_CUTIN_COLOR)LotEnsyutuDetail(ENSYUTU_TYPE.TYPE_CUTINCOLOR);

        ensyutuTbl.cutinType = (ENSYUTU_CUTIN_TYPE)LotEnsyutuDetail(ENSYUTU_TYPE.TYPE_CUTINTYPE);
    }
    public int GetRand()
    {
        return UnityEngine.Random.Range(RAND_TOKUSYO_MIN, RAND_TOKUSYO_MAX);
    }

    public TOKUSYO GetTokusyo(int index)
    {
        if (lotNum != null) return lotNum[index];
        else return TOKUSYO.TOKUSYO_NONE;
    }
    public TOKUSYO GetTokuyoMax()
    {
        if (lotNum != null)
        {
            return lotNum.Max();
        }
        else
            return TOKUSYO.TOKUSYO_NONE;
    }
    public int GetLotSize()
    {
        if (lotNum != null) return lotNum.Length;
        else return 0;
    }
    public TOKUSYO GetTokusyoMin()
    {
        if (lotNum != null) return lotNum.Min();
        else return TOKUSYO.TOKUSYO_NONE;
    }
    public int GetTokusyoNum(TOKUSYO tokusyo)
    {
        int num = 0;
        for (int i = 0; i < lotNum.Length; i++)
        {
            if (lotNum[i] == tokusyo) num++;
        }
        return num;
    }
    public int GetEnsyutuDetail(ENSYUTU_TYPE type)
    {
        int result;

        switch (type)
        {
            case ENSYUTU_TYPE.TYPE_SAKIBARE:
                result = (int)ensyutuTbl.sakibare;
                break;

            case ENSYUTU_TYPE.TYPE_BGCOLOR:
                result = (int)ensyutuTbl.bgcolor;
                break;

            case ENSYUTU_TYPE.TYPE_CROON_CATEGORY:
                result = (int)ensyutuTbl.croonCat;
                break;

            case ENSYUTU_TYPE.TYPE_CROON_PATTERN:
                result = (int)ensyutuTbl.croonPat;
                break;

            case ENSYUTU_TYPE.TYPE_DEME:
                result = (int)ensyutuTbl.deme;
                break;

            case ENSYUTU_TYPE.TYPE_CUTINCOLOR:
                result = (int)ensyutuTbl.cutinColor;
                break;

            case ENSYUTU_TYPE.TYPE_CUTINTYPE:
                result = (int)ensyutuTbl.cutinType;
                break;

            default:
                return -1;
        }

        return result;
    }
    public int GetEnsyutuNum()
    {
        return ensyutuNum;
    }
    public void ResetLot()
    {
        lotNum = null;
        ensyutuNum = 0;
        challengeNum = 0;
    }
    public int GetLotNumMax()
    {
        return MAX_LOT_NUM;
    }
    public int GetEnsyutuMax()
    {
        return MAX_ENSYUTU_NUM;
    }

    public void SetTokusyoValue(TOKUSYO tokusyo, int num)
    {
        if (num < 0) num = 0;
        switch (tokusyo)
        {
            case TOKUSYO.TOKUSYO_1ST:
                RAND_TOKUSYO_1ST = num;
                break;
            case TOKUSYO.TOKUSYO_2ND:
                RAND_TOKUSYO_2ND = num;
                break;
            case TOKUSYO.TOKUSYO_3RD:
                RAND_TOKUSYO_3RD = num;
                break;
            case TOKUSYO.TOKUSYO_4TH:
                RAND_TOKUSYO_4TH = num;
                break;
            case TOKUSYO.TOKUSYO_5TH:
                RAND_TOKUSYO_5TH = num;
                break;
            case TOKUSYO.TOKUSYO_6TH:
                RAND_TOKUSYO_6TH = num;
                break;
            case TOKUSYO.TOKUSYO_7TH:
                RAND_TOKUSYO_7TH = num;
                break;
        }
        RAND_TOKUSYO_1ST_RANGE = RAND_TOKUSYO_1ST;
        RAND_TOKUSYO_2ND_RANGE = RAND_TOKUSYO_2ND + RAND_TOKUSYO_1ST_RANGE;
        RAND_TOKUSYO_3RD_RANGE = RAND_TOKUSYO_3RD + RAND_TOKUSYO_2ND_RANGE;
        RAND_TOKUSYO_4TH_RANGE = RAND_TOKUSYO_4TH + RAND_TOKUSYO_3RD_RANGE;
        RAND_TOKUSYO_5TH_RANGE = RAND_TOKUSYO_5TH + RAND_TOKUSYO_4TH_RANGE;
        RAND_TOKUSYO_6TH_RANGE = RAND_TOKUSYO_6TH + RAND_TOKUSYO_5TH_RANGE;
        RAND_TOKUSYO_7TH_RANGE = RAND_TOKUSYO_7TH + RAND_TOKUSYO_6TH_RANGE;

        RAND_TOKUSYO_MAX = RAND_TOKUSYO_1ST + RAND_TOKUSYO_2ND + RAND_TOKUSYO_3RD + RAND_TOKUSYO_4TH + RAND_TOKUSYO_5TH + RAND_TOKUSYO_6TH + RAND_TOKUSYO_7TH;
    }
    public int GetTokusyoValue(TOKUSYO tokusyo)
    {
        switch (tokusyo)
        {
            case TOKUSYO.TOKUSYO_1ST:
                return RAND_TOKUSYO_1ST;
            case TOKUSYO.TOKUSYO_2ND:
                return RAND_TOKUSYO_2ND;
            case TOKUSYO.TOKUSYO_3RD:
                return RAND_TOKUSYO_3RD;
            case TOKUSYO.TOKUSYO_4TH:
                return RAND_TOKUSYO_4TH;
            case TOKUSYO.TOKUSYO_5TH:
                return RAND_TOKUSYO_5TH;
            case TOKUSYO.TOKUSYO_6TH:
                return RAND_TOKUSYO_6TH;
            case TOKUSYO.TOKUSYO_7TH:
                return RAND_TOKUSYO_7TH;
            default:
                return RAND_TOKUSYO_5TH;
        }
    }
    ///////////////////////////////////////////////////////////////////////////////////////
    //デバッグ用の設定
    ///////////////////////////////////////////////////////////////////////////////////////

    public void SetTokusyoNum(int num)
    {
        lotNum = new TOKUSYO[num];
    }
    public void SetTokusyo(int index, TOKUSYO tokusyo)
    {
        lotNum[index] = tokusyo;
    }
    public void SetEnsyutuNum(int num)
    {
        ensyutuNum = num;
    }
    public void SetChallengeNum(int num)
    {
        challengeNum = num;
    }
}