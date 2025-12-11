using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.IO;
using System;
using System.Text;
using System.Threading;
public class PachiSlot : MonoBehaviour
{
    Bone[] boneReel = new Bone[3];
    private AnimMgr animMgr;

    private int overrideBgColor = -1;
    private int overrideCroon = -1;
    private int overrideDeme = -1;
    private int overrideCutin = -1;
    private int overrideSakibare = -1;
    private int overrideCutinType = -1;
    private int overrideSakibareRandom = -1;

    private const float ROT = 0.8f;      // 1回転の速さ
    private const float POSITION_Y_BUTTOM = -2137.0f;
    private const float POSITION_Y_TOP = -89.0f;
    private const float REEL_INTRVAL = 102.4f;

    private GAME_STATE pachislotState = GAME_STATE.STATE_IDLE;
    private REEL_STATE[] reelState = new REEL_STATE[3];

    private float[] demePos;

    private Dictionary<ENSYUTU_DEME, float[]> zugaraPosList_L;
    private Dictionary<ENSYUTU_DEME, float[]> zugaraPosList_C;
    private Dictionary<ENSYUTU_DEME, float[]> zugaraPosList_R;

    private ANIM_TABLE[] animTbl;

    private Lottery lottery;
    private CUCMgr cucMgr;
    Dictionary<TOKUSYO, int> tokusyoValueList;
    private string[] filePath;
    string ENV = "PROD";
    string pcName;
    int lotteryTypeId = 26;
    int lotteryId = 1;
    const int LOT_MAX = 10;
    const int LOT_MIN = 1;
    bool initFlg = false;
    uint frameCnt = 0;
    const int FRAME_INTERVAL = 3;
    const int RETRY_MAX = 3;
    const int TIME_OUT = 5;

    private int lotnum = LOT_MIN;
    private REEL_LCR firstReelStopped = REEL_LCR.REEL_ALL;
    private REEL_LCR secondReelStopped = REEL_LCR.REEL_ALL;  // 2回目に押されたリールを記録
    private bool[] reelStopped = new bool[3]; // 各リールの停止状態を管理
    private bool[] reelSnapSEPlayed = new bool[3]; // 追加
    private bool isEndTriggered = false;  // OnEndが呼ばれたかを管理するフラグ
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    private string debugString;
    private bool autoFlg = false;
    private int waitFrame = 0;
    private const int WAIT_MIN = 60 * 60;
    private const int WAIT_MAX = 60 * 60 * 2;
#endif
    private SkeletonAnimation _skeletonAnimation;
    private SkeletonAnimation skeletonAnimation
    {
        get
        {
            if (_skeletonAnimation == null)
            {
                _skeletonAnimation = GetComponent<SkeletonAnimation>();
            }
            return _skeletonAnimation;
        }
    }

    // === 動的画像はめ込み機能用 ===
    private string dynamicImagesPath;
    private bool isDynamicImageEnabled = false;
    private Dictionary<string, string> slotImageMapping = new Dictionary<string, string>();

    // シーン管理用
    private GAME_STATE currentGameState = GAME_STATE.STATE_IDLE;
    private Dictionary<GAME_STATE, Dictionary<string, bool>> sceneSlotVisibility = new Dictionary<GAME_STATE, Dictionary<string, bool>>();

    // 設定可能なスロット名とファイル名のマッピング
    private readonly Dictionary<string, string> defaultSlotMapping = new Dictionary<string, string>()
    {
        {"slot_dynamic_bg", "background.png"},      // 背景画像用スロット
        {"slot_dynamic_effect", "effect.png"},     // エフェクト用スロット
        {"slot_dynamic_chara", "character.png"},   // キャラクター用スロット
        {"slot_dynamic_logo", "logo.png"},         // ロゴ用スロット
        {"slot_dynamic_item", "item.png"}          // アイテム用スロット
    };

    enum ANIM
    {
        SHITAPANEL,
        DEMO,
        OBI,
        LEVER_IN,
        LEVER_LP,
        CROON_CHA_IN,
        CROON_CHA_LP,
        CROON_CHA_BG,
        REEL,
        REEL_EFFECT,
        REEL_FLASH_L,
        REEL_FLASH_C,
        REEL_FLASH_R,
        BGCOLOR_MOVIE,
        CROON_IN,
        CROON_FLASH,
        CUTIN_MOVIE,
        CROON_OUT,
        PRIZES_IN,
        PRIZES_LP,
        PRIZES_SMALL_IN,
        PRIZES_SMALL_LP,
        PRIZES_ITEMS,
        RESULT_BG,
        RESULT_FLASH,

        SOUND_SE_TICKET,
        SOUND_BGM_LEVER,
        SOUND_SE_LEVER_IN,
        SOUND_SE_CROON_IN,
        SOUND_SE_CROON_IN_CHANCE1,
        SOUND_SE_CROON_IN_CHANCE2,
        SOUND_SE_CROON_IN_CHANCE3,
        SOUND_SE_CROON_IN_CHANCE4,
        SOUND_SE_CROON_IN_CHANCE5,
        SOUND_SE_REEL_DF,
        SOUND_SE_REEL_CH,
        SOUND_SE_CUTIN,
        SOUND_SE_RESULT,

        TEST_HYOUJI,

    };
    void Awake()
    {
        GameObject gameObject = GameObject.Find("PachiSlot");

        _skeletonAnimation = GetComponent<SkeletonAnimation>();
        var skl = GetComponent<SkeletonAnimation>();

        animMgr = AnimMgr.GetInstance();
        animMgr.Initialize(skl);
    }
    // Start is called before the first frame update
    void Start()
    {
        //ここでそれぞれのムービーサイズを決める
        for (VIDEO_CH ch = VIDEO_CH.CH_1; ch < VIDEO_CH.CH_MAX; ch++)
        {
            animMgr.CreateVideo(ch, 0f, 1812f, 1080f, 864f);
        }
        //ボーンを取得
        //REEL_LCR.REEL_LのREEL_LCRは列挙型（defineにある）
        boneReel[(int)REEL_LCR.REEL_L] = GetComponent<SkeletonAnimation>().skeleton.FindBone("Reel_L");
        boneReel[(int)REEL_LCR.REEL_C] = GetComponent<SkeletonAnimation>().skeleton.FindBone("Reel_C");
        boneReel[(int)REEL_LCR.REEL_R] = GetComponent<SkeletonAnimation>().skeleton.FindBone("Reel_R");

        demePos = new float[]
        {
            POSITION_Y_TOP - REEL_INTRVAL * 0f,
            POSITION_Y_TOP - REEL_INTRVAL * 1f,
            POSITION_Y_TOP - REEL_INTRVAL * 2f,
            POSITION_Y_TOP - REEL_INTRVAL * 3f,
            POSITION_Y_TOP - REEL_INTRVAL * 4f,
            POSITION_Y_TOP - REEL_INTRVAL * 5f,
            POSITION_Y_TOP - REEL_INTRVAL * 6f,
            POSITION_Y_TOP - REEL_INTRVAL * 7f,
            POSITION_Y_TOP - REEL_INTRVAL * 8f,
            POSITION_Y_TOP - REEL_INTRVAL * 9f,
            POSITION_Y_TOP - REEL_INTRVAL * 10f,
            POSITION_Y_TOP - REEL_INTRVAL * 11f,
            POSITION_Y_TOP - REEL_INTRVAL * 12f,
            POSITION_Y_TOP - REEL_INTRVAL * 13f,
            POSITION_Y_TOP - REEL_INTRVAL * 14f,
            POSITION_Y_TOP - REEL_INTRVAL * 15f,
            POSITION_Y_TOP - REEL_INTRVAL * 16f,
            POSITION_Y_TOP - REEL_INTRVAL * 17f,
            POSITION_Y_TOP - REEL_INTRVAL * 18f,
            POSITION_Y_TOP - REEL_INTRVAL * 19f,
        };
        //左1st用出目の設定
        zugaraPosList_L = new Dictionary<ENSYUTU_DEME, float[]>
        {
            { ENSYUTU_DEME.LINE3_1,         new float[]{demePos[9]  ,demePos[9],demePos[9] }},  //OK
            { ENSYUTU_DEME.LINE2_1,         new float[]{demePos[9]  ,demePos[9],demePos[8] }}, //OK
            { ENSYUTU_DEME.LINE2_2,         new float[]{demePos[9]  ,demePos[9],demePos[10] }}, //OK
            { ENSYUTU_DEME.LINE2_3,         new float[]{demePos[10]  ,demePos[9],demePos[8] }}, //OK
            { ENSYUTU_DEME.LINE2_4,         new float[]{demePos[8]  ,demePos[9],demePos[10] }}, //OK
            { ENSYUTU_DEME.LINE1_1,        new float[]{demePos[7]  ,demePos[8],demePos[7] }}, //OK
            { ENSYUTU_DEME.LINE1_2,        new float[]{demePos[11]  ,demePos[10],demePos[11] }}, //OK
            { ENSYUTU_DEME.LINE1_3,        new float[]{demePos[7]  ,demePos[9],demePos[11] }}, //OK
            { ENSYUTU_DEME.LINE1_4,        new float[]{demePos[11]  ,demePos[9],demePos[7] }}, //OK
            { ENSYUTU_DEME.LINE1_5,        new float[]{demePos[8]  ,demePos[9],demePos[11] }}, //OK
            { ENSYUTU_DEME.LINE1_6,        new float[]{demePos[10]  ,demePos[9],demePos[7] }}, //OK
        };

        zugaraPosList_C = new Dictionary<ENSYUTU_DEME, float[]>
        {
            { ENSYUTU_DEME.LINE3_1,         new float[]{demePos[9]  ,demePos[9],demePos[9] }},  //OK
            { ENSYUTU_DEME.LINE2_1,         new float[]{demePos[9]  ,demePos[9],demePos[8] }}, //OK
            { ENSYUTU_DEME.LINE2_2,         new float[]{demePos[9]  ,demePos[9],demePos[10] }}, //OK
            { ENSYUTU_DEME.LINE2_3,         new float[]{demePos[10]  ,demePos[9],demePos[8] }}, //OK
            { ENSYUTU_DEME.LINE2_4,         new float[]{demePos[8]  ,demePos[9],demePos[10] }}, //OK
            { ENSYUTU_DEME.LINE1_1,        new float[]{demePos[7]  ,demePos[8],demePos[7] }}, //OK
            { ENSYUTU_DEME.LINE1_2,        new float[]{demePos[11]  ,demePos[10],demePos[11] }}, //OK
            { ENSYUTU_DEME.LINE1_3,        new float[]{demePos[7]  ,demePos[9],demePos[11] }}, //OK
            { ENSYUTU_DEME.LINE1_4,        new float[]{demePos[11]  ,demePos[9],demePos[7] }}, //OK
            { ENSYUTU_DEME.LINE1_5,        new float[]{demePos[8]  ,demePos[9],demePos[11] }}, //OK
            { ENSYUTU_DEME.LINE1_6,        new float[]{demePos[10]  ,demePos[9],demePos[7] }}, //OK

        };
        zugaraPosList_R = new Dictionary<ENSYUTU_DEME, float[]>
        {
            { ENSYUTU_DEME.LINE3_1,         new float[]{demePos[9]  ,demePos[9],demePos[9] }},  //OK
            { ENSYUTU_DEME.LINE2_1,         new float[]{demePos[9]  ,demePos[9],demePos[8] }}, //OK
            { ENSYUTU_DEME.LINE2_2,         new float[]{demePos[9]  ,demePos[9],demePos[10] }}, //OK
            { ENSYUTU_DEME.LINE2_3,         new float[]{demePos[10]  ,demePos[9],demePos[8] }}, //OK
            { ENSYUTU_DEME.LINE2_4,         new float[]{demePos[8]  ,demePos[9],demePos[10] }}, //OK
            { ENSYUTU_DEME.LINE1_1,        new float[]{demePos[7]  ,demePos[8],demePos[7] }}, //OK
            { ENSYUTU_DEME.LINE1_2,        new float[]{demePos[11]  ,demePos[10],demePos[11] }}, //OK
            { ENSYUTU_DEME.LINE1_3,        new float[]{demePos[7]  ,demePos[9],demePos[11] }}, //OK
            { ENSYUTU_DEME.LINE1_4,        new float[]{demePos[11]  ,demePos[9],demePos[7] }}, //OK
            { ENSYUTU_DEME.LINE1_5,        new float[]{demePos[8]  ,demePos[9],demePos[11] }}, //OK
            { ENSYUTU_DEME.LINE1_6,        new float[]{demePos[10]  ,demePos[9],demePos[7] }}, //OK

        };
        //リールの初期化
        SetReelState(REEL_LCR.REEL_ALL, REEL_STATE.REEL_IDLE);

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
        animTbl = new ANIM_TABLE[]
        {
            new ANIM_TABLE{trackIndex = TRACK.SHITAPANEL, animName = "Panel/Shitapanel/Shitapanel", loopFlg = true, soundIndex = SOUND.NONE, type = SOUND_TYPE.LOOP,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.NONE, animName = "", loopFlg = false, soundIndex = SOUND.BGM_IDLE, type = SOUND_TYPE.LOOP,     volume = 0.5f, videoIndex = VIDEO.DEMO,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.OBI, animName = "Full/Belt/Announce", loopFlg = true, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.NONE, animName = "", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.LEVER_IN,videoLoop = 0,videoCH = VIDEO_CH.CH_2},
            new ANIM_TABLE{trackIndex = TRACK.NONE, animName = "", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.LEVER_LP,videoLoop = -1,videoCH = VIDEO_CH.CH_3},
            new ANIM_TABLE{trackIndex = TRACK.NONE, animName = "", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.CROON_CHA_IN,videoLoop = 0,videoCH = VIDEO_CH.CH_2},
            new ANIM_TABLE{trackIndex = TRACK.NONE, animName = "", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.CROON_CHA_LP,videoLoop = -1,videoCH = VIDEO_CH.CH_2},
            new ANIM_TABLE{trackIndex = TRACK.NONE, animName = "", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.CROON_CHA_BG,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.REEL, animName = "Kyotai/Normal", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.REEL_EFFECT, animName = "Reel/Reel_Fire", loopFlg = true, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.REEL_FLASH_L, animName = "Reel/Reel_Flash_L", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.REEL_FLASH_C, animName = "Reel/Reel_Flash_C", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.REEL_FLASH_R, animName = "Reel/Reel_Flash_R", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.NONE, animName = "", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.CROON_BG_BLUE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.NONE, animName = "", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.CROON_IN_BLUE,videoLoop = 0,videoCH = VIDEO_CH.CH_2},
            new ANIM_TABLE{trackIndex = TRACK.NONE, animName = "Full/RESULT_PRIZES/Croon_Flash_In", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = 0,videoCH = VIDEO_CH.CH_2},
            new ANIM_TABLE{trackIndex = TRACK.NONE, animName = "", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.CUTIN_BLUE_1,videoLoop = 0,videoCH = VIDEO_CH.CH_4},
            new ANIM_TABLE{trackIndex = TRACK.NONE, animName = "", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.CUTIN_BLUE_1,videoLoop = 0,videoCH = VIDEO_CH.CH_3},
            new ANIM_TABLE{trackIndex = TRACK.NONE, animName = "Full/RESULT_PRIZES/Result_Prizes_In_*TokusyoValue*", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = 0,videoCH = VIDEO_CH.CH_3},
            new ANIM_TABLE{trackIndex = TRACK.NONE, animName = "Full/RESULT_PRIZES/Result_Prizes_Lp_*TokusyoValue*", loopFlg = true, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = 0,videoCH = VIDEO_CH.CH_3},
            new ANIM_TABLE{trackIndex = TRACK.NONE, animName = "Full/RESULT_PRIZES/Result_Prizes_small_*No*_In_*TokusyoValue*", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = 0,videoCH = VIDEO_CH.CH_3},
            new ANIM_TABLE{trackIndex = TRACK.NONE, animName = "Full/RESULT_PRIZES/Result_Prizes_small_*No*_Lp_*TokusyoValue*", loopFlg = true, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = 0,videoCH = VIDEO_CH.CH_3},
            new ANIM_TABLE{trackIndex = TRACK.ITEMS, animName = "Full/RESULT_PRIZES/Prizes_Items_In_*TokusyoValue*", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = 0,videoCH = VIDEO_CH.CH_3},
            new ANIM_TABLE{trackIndex = TRACK.ITEMS, animName = "", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.RESULT_BG,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.FLASH, animName = "Full/RESULT_PRIZES/Prizes_Flash_In", loopFlg = false, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.FLASH, animName = "", loopFlg = false, soundIndex = SOUND.SE_TICKET, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.FLASH, animName = "", loopFlg = false, soundIndex = SOUND.BGM_LEVER, type = SOUND_TYPE.LOOP,     volume = 0.5f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.FLASH, animName = "", loopFlg = false, soundIndex = SOUND.SE_LEVER_IN, type = SOUND_TYPE.ONESHOT,     volume = 0.8f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.FLASH, animName = "", loopFlg = false, soundIndex = SOUND.SE_CROON_IN, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.FLASH, animName = "", loopFlg = false, soundIndex = SOUND.SE_CROON_IN_CHANCE1, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.FLASH, animName = "", loopFlg = false, soundIndex = SOUND.SE_CROON_IN_CHANCE2, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.FLASH, animName = "", loopFlg = false, soundIndex = SOUND.SE_CROON_IN_CHANCE3, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.FLASH, animName = "", loopFlg = false, soundIndex = SOUND.SE_CROON_IN_CHANCE4, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.FLASH, animName = "", loopFlg = false, soundIndex = SOUND.SE_CROON_IN_CHANCE5, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.FLASH, animName = "", loopFlg = false, soundIndex = SOUND.SE_REEL_DF, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.FLASH, animName = "", loopFlg = false, soundIndex = SOUND.SE_REEL_CH, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.FLASH, animName = "", loopFlg = false, soundIndex = SOUND.SE_CUTIN, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.FLASH, animName = "", loopFlg = false, soundIndex = SOUND.SE_RESULT, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_1},
            new ANIM_TABLE{trackIndex = TRACK.FLASH, animName = "test_hyouji", loopFlg = true, soundIndex = SOUND.NONE, type = SOUND_TYPE.ONESHOT,     volume = 1.0f, videoIndex = VIDEO.NONE,videoLoop = -1,videoCH = VIDEO_CH.CH_3},

        };
        cucMgr = CUCMgr.GetInstance();
        cucMgr.SetObject(this);

        lottery = Lottery.GetInstance();
        SetFilePath();

        pcName = Environment.MachineName;

        // 動的画像はめ込み機能の初期化（Init_Idleより前に実行）
        InitializeDynamicImageSystem();

        Init_Idle();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        debugString = "";
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (initFlg)
        {
            frameCnt++;
            cucMgr.Update();
            //REEL_LCR lcrのlcrはREEL_LCR列挙型の変数　　0.1.2の3回ループする
            for (REEL_LCR lcr = 0; lcr < REEL_LCR.REEL_ALL; lcr++)
            {
                //現在処理してるリール位置、reelState[(int)lcr]はreelStateという配列
                UpdateReel(lcr, reelState[(int)lcr]);
            }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (autoFlg)
            {
                // ここで使いたいボタン一覧を配列にまとめる
                CONTROLLER[] buttons = new[]
                {
                    CONTROLLER.MAXBET,
                    CONTROLLER.OUT,
                    CONTROLLER.LEVER,
                    CONTROLLER.LEFTBUTTON,
                    CONTROLLER.CENTERBUTTON,
                    CONTROLLER.RIGHTBUTTON,
                    CONTROLLER.ARROW_LEFT,
                    CONTROLLER.ARROW_RIGHT,
                    CONTROLLER.ARROW_UP,
                    CONTROLLER.ARROW_DOWN
                };

                // 配列の中からランダムに１つ選ぶ
                int idx = UnityEngine.Random.Range(0, buttons.Length);
                CONTROLLER selected = buttons[idx];

                // 選ばれたボタンを押す
                InputCommand((int)selected, (int)CONTROLLER_SWITCH.ON);
                InputCommand((int)selected, (int)CONTROLLER_SWITCH.OFF);
            }
#endif
        }
        else
        {
            initFlg = animMgr.GetInitFlg();
        }
        animMgr.UpdateVideo();

    }
    public void InputCommand(int id, int sw)
    {
        //入力間が特定のフレーム未満なら弾く
        if (frameCnt - FRAME_INTERVAL < 0) return;

        bool inputFlg = sw > 0 ? true : false;

        if (inputFlg) frameCnt = 0;

        switch (id)
        {
            //この入力は使用しない
            case (int)CONTROLLER.MAXBET:
                OnBet(inputFlg);
                break;
            case (int)CONTROLLER.ONEBET:
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                if (inputFlg)
                {
                    autoFlg = !autoFlg;
                }
#endif
                break;
            case (int)CONTROLLER.OUT:
                OnOut(inputFlg);
                break;
            case (int)CONTROLLER.LEVER:
                OnLever(inputFlg);
                break;
            case (int)CONTROLLER.LEFTBUTTON:
                OnStop(inputFlg, REEL_LCR.REEL_L);
                break;
            case (int)CONTROLLER.CENTERBUTTON:
                OnStop(inputFlg, REEL_LCR.REEL_C);
                break;
            case (int)CONTROLLER.RIGHTBUTTON:
                OnStop(inputFlg, REEL_LCR.REEL_R);
                break;
            case (int)CONTROLLER.PUSHBUTTON:
                break;
            case (int)CONTROLLER.ARROW_LEFT:
            case (int)CONTROLLER.ARROW_RIGHT:
            case (int)CONTROLLER.ARROW_UP:
            case (int)CONTROLLER.ARROW_DOWN:
                OnArrow(inputFlg, (CONTROLLER)id);
                break;
        }
    }
    private void UpdateReel(REEL_LCR lcr, REEL_STATE state)
    {
        float[] deme;

        if (state == REEL_STATE.REEL_RUN)
        {
            RotateReel(lcr);
        }
        else if (state == REEL_STATE.REEL_STOP || state == REEL_STATE.REEL_IDLE)
        {
            // // 通常の処理、最初に押されたリールに応じて出目を設定
            // switch (firstReelStopped)
            // {
            //     case REEL_LCR.REEL_L:
            //         zugaraPosList_L.TryGetValue((ENSYUTU_DEME)lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_DEME), out deme);
            //         AdjustReelPosition(lcr, deme);
            //         break;
            //     case REEL_LCR.REEL_C:
            //         zugaraPosList_C.TryGetValue((ENSYUTU_DEME)lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_DEME), out deme);
            //         AdjustReelPosition(lcr, deme);
            //         break;
            //     case REEL_LCR.REEL_R:
            //         zugaraPosList_R.TryGetValue((ENSYUTU_DEME)lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_DEME), out deme);
            //         AdjustReelPosition(lcr, deme);
            //         break;
            // }

            // overrideDeme がセットされていればそちらを優先
            // 1) overrideDeme があれば優先
            // 2) なければ bgcolor に応じて 11 パターンの出目を選択
            //    bgcolor=0 → 0
            //    bgcolor=1 → 1～4 のいずれか
            //    bgcolor=2 → 5～10 のいずれか

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            // int demeValue = (overrideDeme >= 0)
            //     ? overrideDeme
            //     : lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_DEME);
#endif

            int demeValue = lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_DEME);


            // 最初に停止したリールに応じて位置調整
            switch (firstReelStopped)
            {
                case REEL_LCR.REEL_L:
                    zugaraPosList_L.TryGetValue(
                        (ENSYUTU_DEME)demeValue,
                        out deme
                    );
                    AdjustReelPosition(lcr, deme);
                    break;
                case REEL_LCR.REEL_C:
                    zugaraPosList_C.TryGetValue(
                        (ENSYUTU_DEME)demeValue,
                        out deme
                    );
                    AdjustReelPosition(lcr, deme);
                    break;
                case REEL_LCR.REEL_R:
                    zugaraPosList_R.TryGetValue(
                        (ENSYUTU_DEME)demeValue,
                        out deme
                    );
                    AdjustReelPosition(lcr, deme);
                    break;
            }
        }
        if (ChkStopAll() && !isEndTriggered)  // isEndTriggered フラグで OnEnd の連続呼び出しを防止
        {
            isEndTriggered = true;  // OnEndを呼び出したことを記録
            OnEnd();
        }
    }
    private void AdjustReelPosition(REEL_LCR lcr, float[] deme)
    {
        // …以下元の処理…
        // 押されたときのリールの位置???
        float PushPosY = boneReel[(int)lcr].Y;

        // リールを下方向に動かす
        RotateReel(lcr);

        // リールが目標位置を通り過ぎないようにする
        if (boneReel[(int)lcr].Y < deme[(int)lcr] && PushPosY >= deme[(int)lcr])
        {
            boneReel[(int)lcr].Y = deme[(int)lcr];  // 目標位置に到達したらぴったり止める
            reelStopped[(int)lcr] = true;
            // 追加：止まった瞬間に一度だけSE
            if (!reelSnapSEPlayed[(int)lcr])
            {
                // リール位置に応じて再生アニメを切り替え
                switch (lcr)
                {
                    case REEL_LCR.REEL_L:
                        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.REEL_FLASH_L]);
                        break;

                    case REEL_LCR.REEL_C:
                        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.REEL_FLASH_C]);
                        break;

                    case REEL_LCR.REEL_R:
                        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.REEL_FLASH_R]);
                        break;
                }

                reelSnapSEPlayed[(int)lcr] = true;
            }
        }
    }
    // リールの回転処理を共通化
    private void RotateReel(REEL_LCR lcr)
    {
        float velocity = Mathf.Abs(POSITION_Y_TOP - POSITION_Y_BUTTOM) / (60.0f * ROT);
        boneReel[(int)lcr].Y -= velocity;

        if (boneReel[(int)lcr].Y <= POSITION_Y_BUTTOM)
        {
            boneReel[(int)lcr].Y = POSITION_Y_TOP;
        }
    }
    public void SetReelState(REEL_LCR lcr, REEL_STATE state)
    {
        if (state == REEL_STATE.REEL_STOP)
        {
            // lcr が L, C, R のそれぞれに対して異なる条件を処理
            switch (lcr)
            {
                case REEL_LCR.REEL_L:
                    cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.LEFT_B, CONTROLLER_SWITCH.OFF);
                    cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.LEFT_R, CONTROLLER_SWITCH.ON);
                    break;
                case REEL_LCR.REEL_C:
                    cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.CENTER_B, CONTROLLER_SWITCH.OFF);
                    cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.CENTER_R, CONTROLLER_SWITCH.ON);
                    break;
                case REEL_LCR.REEL_R:
                    cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.RIGHT_B, CONTROLLER_SWITCH.OFF);
                    cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.RIGHT_R, CONTROLLER_SWITCH.ON);
                    break;
            }
        }
        if (lcr == REEL_LCR.REEL_ALL)
        {
            reelState[(int)REEL_LCR.REEL_L] = state;
            reelState[(int)REEL_LCR.REEL_C] = state;
            reelState[(int)REEL_LCR.REEL_R] = state;
        }
        else
        {
            reelState[(int)lcr] = state;
        }
    }
    public REEL_STATE GetReelState(REEL_LCR lcr)
    {
        return reelState[(int)lcr];
    }
    public REEL_STATE[] GetReelStateAll()
    {
        return reelState;
    }
    public bool ChkStopAll()
    {
        if (reelStopped[(int)REEL_LCR.REEL_L] &&
            reelStopped[(int)REEL_LCR.REEL_C] &&
            reelStopped[(int)REEL_LCR.REEL_R] &&
            GetReelState(REEL_LCR.REEL_L) == REEL_STATE.REEL_IDLE &&
            GetReelState(REEL_LCR.REEL_C) == REEL_STATE.REEL_IDLE &&
            GetReelState(REEL_LCR.REEL_R) == REEL_STATE.REEL_IDLE
            )
        {
            return true;
        }
        return false;
    }
    public GAME_STATE GetGameState()
    {
        return pachislotState;
    }
    public void SetGameState(GAME_STATE state)
    {
        GAME_STATE previousState = pachislotState;
        pachislotState = state;

        // ゲーム状態が変わった場合、動的画像の表示を更新
        if (previousState != state)
        {
            currentGameState = state;
            ApplySceneSlotVisibility(state);
        }
    }
    public void OnBet(bool flg)
    {
        if (GetGameState() == GAME_STATE.STATE_IDLE)
        {
            lottery.SetLotNum(lotnum);
            Init_LeverStandby();
            SetGameState(GAME_STATE.STATE_BET);
        }
    }
    public void OnLever(bool flg)
    {
        if (GetGameState() < GAME_STATE.STATE_BET) return;

        if (flg)
        {
            if (GetGameState() == GAME_STATE.STATE_BET)
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                SetDebugString("OnLever");
#endif
                ClearAnim();
                cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.RIGHT_R, CONTROLLER_SWITCH.OFF);
                cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.CENTER_R, CONTROLLER_SWITCH.OFF);
                cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.LEFT_R, CONTROLLER_SWITCH.OFF);
                cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.RIGHT_B, CONTROLLER_SWITCH.ON);
                cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.CENTER_B, CONTROLLER_SWITCH.ON);
                cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.LEFT_B, CONTROLLER_SWITCH.ON);
                animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.CROON_CHA_IN], new CallbackInfo
                {
                    Type = CALLBACK_TYPE.MOVIE_END,
                    Callback = obj => EndVideo(obj)
                });
                animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.CROON_CHA_BG]);

                animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.REEL]);
                animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.REEL_EFFECT]);



                SetGameState(GAME_STATE.STATE_LEVER);
                //抽選
                StartCoroutine(API_Request());
            }
        }
    }
    public void OnStop(bool flg, REEL_LCR lcr)
    {
        if (GetGameState() < GAME_STATE.STATE_BET) return;

        if (flg)
        {
            if (GetReelState(lcr) == REEL_STATE.REEL_RUN)
            {
                // animMgr.SetAnim(animTbl[(int)ANIM.SOUND_STOP]);
                SetReelState(lcr, REEL_STATE.REEL_STOP);

                ENSYUTU_DEME demeDetail = (ENSYUTU_DEME)lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_DEME);

                // 最初に押されたリールを記録
                if (firstReelStopped == REEL_LCR.REEL_ALL)  // 1回目のリール
                {
                    firstReelStopped = lcr;  // 最初に押されたリールを記録

                    if (firstReelStopped == REEL_LCR.REEL_L)
                    {
                        if (demeDetail == ENSYUTU_DEME.LINE3_1 || demeDetail == ENSYUTU_DEME.LINE2_1 || demeDetail == ENSYUTU_DEME.LINE2_2)
                        {
                            animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_CH]);
                        }
                        else
                        {
                            animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_DF]);
                        }
                    }
                    if (firstReelStopped == REEL_LCR.REEL_C)
                    {
                        if (demeDetail == ENSYUTU_DEME.LINE1_1 || demeDetail == ENSYUTU_DEME.LINE1_2)
                        {
                            animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_DF]);
                        }
                        else
                        {
                            animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_CH]);
                        }
                    }
                    if (firstReelStopped == REEL_LCR.REEL_R)
                    {
                        if (demeDetail == ENSYUTU_DEME.LINE3_1)
                        {
                            animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_CH]);
                        }
                        else
                        {
                            animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_DF]);
                        }
                    }

                }
                else if (secondReelStopped == REEL_LCR.REEL_ALL && lcr != firstReelStopped)  // 2回目のリール
                {
                    secondReelStopped = lcr;  // 2回目に押されたリールを記録

                    if (firstReelStopped == REEL_LCR.REEL_L)
                    {
                        if (secondReelStopped == REEL_LCR.REEL_R)
                        {
                            if (demeDetail == ENSYUTU_DEME.LINE3_1)
                            {
                                animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_CH]);
                            }
                            else
                            {
                                animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_DF]);
                            }
                        }
                        else
                        {
                            animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_DF]);
                        }
                    }
                    else if (firstReelStopped == REEL_LCR.REEL_C)
                    {
                        if (secondReelStopped == REEL_LCR.REEL_L)
                        {
                            if (demeDetail == ENSYUTU_DEME.LINE3_1 || demeDetail == ENSYUTU_DEME.LINE2_1 || demeDetail == ENSYUTU_DEME.LINE2_2)
                            {
                                animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_CH]);
                            }
                            else
                            {
                                animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_DF]);
                            }

                        }
                        else if (secondReelStopped == REEL_LCR.REEL_R)
                        {
                            if (demeDetail == ENSYUTU_DEME.LINE3_1)
                            {
                                animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_CH]);
                            }
                            else
                            {
                                animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_DF]);
                            }
                        }
                    }
                    else if (firstReelStopped == REEL_LCR.REEL_R)
                    {
                        if (secondReelStopped == REEL_LCR.REEL_C)
                        {
                            animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_DF]);
                        }
                        else if (secondReelStopped == REEL_LCR.REEL_L)
                        {
                            if (demeDetail == ENSYUTU_DEME.LINE3_1 || demeDetail == ENSYUTU_DEME.LINE2_1 || demeDetail == ENSYUTU_DEME.LINE2_2)
                            {
                                animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_CH]);
                            }
                            else
                            {
                                animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_DF]);
                            }
                        }
                    }
                }
                else if (lcr != firstReelStopped && lcr != secondReelStopped)  // 3回目のリール
                {
                    if (demeDetail == ENSYUTU_DEME.LINE3_1)
                    {
                        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_CH]);
                    }
                    else
                    {
                        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_REEL_DF]);
                    }

                }
            }
        }
        else
        {
            //離したときにアイドルに移行
            if (GetReelState(lcr) == REEL_STATE.REEL_STOP)
            {
                SetReelState(lcr, REEL_STATE.REEL_IDLE);
            }
        }

    }

    public void OnOut(bool flg)
    {
        if (GetGameState() != GAME_STATE.STATE_END && GetGameState() != GAME_STATE.STATE_BET) return;
        Init_Idle();
    }
    public void OnArrow(bool flg, CONTROLLER type)
    {
        if (GetGameState() != GAME_STATE.STATE_IDLE) return;

        if (flg)
        {
            if (type == CONTROLLER.ARROW_DOWN)
            {
                if (--lotnum < LOT_MIN) lotnum = LOT_MAX;
                animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_TICKET]);
            }
            else if (type == CONTROLLER.ARROW_UP)
            {
                if (++lotnum > LOT_MAX) lotnum = LOT_MIN;
                animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_TICKET]);
            }

            // チケット表示を更新
            UpdateTicketDisplay();
        }
    }
    //ここで揃ったか揃わないの結果をもらい映し出す映像を変える
    public void OnEnd()
    {
        // 二重呼び出し防止
        // if (isEndTriggered) return;
        // isEndTriggered = true;
        ClearAnim();


        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.CROON_FLASH]);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        // 1) 背景色の抽選結果を取得
        // int bgColorValue = overrideBgColor >= 0
        //     ? overrideBgColor
        //     : lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_BGCOLOR);
#endif

        int bgColorValue = lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_BGCOLOR);

        // 2) 背景色ムービーを再生
        //    animTbl[(int)ANIM.BGCOLOR_MOVIE] の動画IDだけ書き換えて使う
        ANIM_TABLE tblBg = animTbl[(int)ANIM.BGCOLOR_MOVIE];
        tblBg.videoIndex = VIDEO.CROON_BG_BLUE + bgColorValue;  // VIDEO.BGCOLOR_0 からオフセット
        animMgr.SetAnim(skeletonAnimation, tblBg);

        // 3) クルーンムービーを再生（同じ bgColorValue を使う）
        // CROON_IN_BLUE/GREEN/RED をオフセットで選択
        ANIM_TABLE tblCroon = animTbl[(int)ANIM.CROON_IN];
        tblCroon.videoIndex = VIDEO.CROON_IN_BLUE + bgColorValue;

        animMgr.SetAnim(
            skeletonAnimation,
            tblCroon,
            new CallbackInfo
            {
                Type = CALLBACK_TYPE.MOVIE_END,
                Callback = obj => EndVideo(obj)
            }
        );

    }

    //重要、画面上に表示するものの初期状態を設定
    public void Init_Idle()
    {
        lotnum = LOT_MIN;
        lottery.ResetLot();
        LoadConfig();
        ClearAnim();

        animMgr.StopAudioAll();

        // チケット表示を更新（動的画像対応）
        UpdateTicketDisplay();

        // 背景ムービー再生
        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.DEMO]);
        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.OBI]);
        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SHITAPANEL]);
        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.TEST_HYOUJI]);






        //IDLE状態でのデバイスの点灯
        cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.BET_R, CONTROLLER_SWITCH.ON);
        cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.RIGHT_R, CONTROLLER_SWITCH.ON);
        cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.CENTER_R, CONTROLLER_SWITCH.ON);
        cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.LEFT_R, CONTROLLER_SWITCH.ON);


        SetGameState(GAME_STATE.STATE_IDLE);
    }
    public void Init_LeverStandby()
    {
        ClearAnim();
        ResetReel();
        animMgr.StopAudioAll();

        //音の再生
        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_BGM_LEVER]);
        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_LEVER_IN]);

        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.LEVER_IN],
        new CallbackInfo
        {
            Type = CALLBACK_TYPE.MOVIE_END,
            Callback = obj => EndVideo(obj)
        });

        //各状態を初期化
        SetReelState(REEL_LCR.REEL_ALL, REEL_STATE.REEL_IDLE);
        firstReelStopped = REEL_LCR.REEL_ALL;   // 初期状態に戻す
        secondReelStopped = REEL_LCR.REEL_ALL;  // 初期状態に戻す
        for (int i = 0; i < reelStopped.Length; i++)
        {
            reelStopped[i] = false;  // 各リールの停止状態を false にリセット
            reelSnapSEPlayed[i] = false; // 追加

        }
        isEndTriggered = false;
        cucMgr.ControllerRequest(CONTROLLER_TYPE.COLOR, CONTROLLER_DEVICE.BET_R, CONTROLLER_SWITCH.OFF);

    }
    public void ClearAnim()
    {
        // Spine アニメをクリア（OBI トラックは残す）
        var state = skeletonAnimation.AnimationState;
        // TRACK.MAX は Define.cs の enum TRACK.MAX
        for (int track = 0; track < (int)TRACK.MAX; track++)
        {
            // OBI は (int)TRACK.OBI == 1
            // OBI と REEL_EFFECT は消さない
            if (track == (int)TRACK.OBI
                || track == (int)TRACK.REEL_EFFECT || track == (int)TRACK.REEL_FLASH_L
                || track == (int)TRACK.REEL_FLASH_C || track == (int)TRACK.REEL_FLASH_R || track == (int)TRACK.SHITAPANEL) continue;
            state.ClearTrack(track);
        }

        // 止めたくないサウンドを列挙（Enum→文字列で取得）
        var skipNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Enum.GetName(typeof(SOUND), SOUND.BGM_LEVER),
            Enum.GetName(typeof(SOUND), SOUND.SE_REEL_DF),  // 例: 他に止めたくないもの
            Enum.GetName(typeof(SOUND), SOUND.SE_REEL_CH),
        };
        // サウンド停止処理：skipNames に入っているクリップ名はそのまま流す
        foreach (var src in GameObject.FindObjectsOfType<AudioSource>())
        {
            // クリップ未設定は無視
            if (src.clip == null) continue;
            // クリップ名が skipNames に含まれていればスキップ
            if (skipNames.Contains(src.clip.name)) continue;
            // それ以外は停止＆クリア
            if (src.isPlaying) src.Stop();
            src.clip = null;
        }

        // 動画を全部止める ← ここを追加
        animMgr.StopVideoAll();

        // 4) Attachment（全スロット）をクリア（"obi_belt" スロットだけはスキップ）
        var skeleton = skeletonAnimation.skeleton;
        // 残したいスロット名を列挙（大文字小文字を無視）
        var keepSlots = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Belt",
            "reel_c_1",
            "reel_c_2",
            "ree_c_2",
            "reel_l_1",
            "reel_l_2",
            "reel_r_1",
            "reel_r_2",
            "Reel_Shadow",
            "Reel_Light",
            "Reel_Inner",
            "Reel_Fire",
            "reelMask",
            "Reel_other_Mask",
            "reel_lamp_mask_1",
            "reel_lamp_mask_2",
            "Reel_BG",
            // 動的画像スロットも保持
            "slot_dynamic_bg",
            "slot_dynamic_effect",
            "slot_dynamic_chara",
            "slot_dynamic_logo",
            "slot_dynamic_item",
        };
        foreach (var slot in skeleton.Slots)
        {
            // スロット名が keepSlots に含まれるならクリアをスキップ
            if (keepSlots.Contains(slot.Data.Name))
                continue;
            slot.Attachment = null;
        }
    }
    //帯とクルーンBGM以外消える
    public void ClearAnimAll()
    {
        // Spine アニメをクリア（OBI トラックは残す）
        var state = skeletonAnimation.AnimationState;
        // TRACK.MAX は Define.cs の enum TRACK.MAX
        for (int track = 0; track < (int)TRACK.MAX; track++)
        {
            // OBI は (int)TRACK.OBI == 1
            if (track == (int)TRACK.OBI || track == (int)TRACK.SHITAPANEL) continue;
            state.ClearTrack(track);
        }
        // サウンドを停止（ただし SOUND.BGM_LEVER のみ停止しない）
        // ※ Enum.GetName で SOUND.BGM_LEVER の名前を取得
        string leverName = Enum.GetName(typeof(SOUND), SOUND.BGM_LEVER);
        foreach (var src in GameObject.FindObjectsOfType<AudioSource>())
        {
            // クリップがセットされていない音源は無視
            if (src.clip == null) continue;
            // BGM用チャンネルかつクリップ名がレバーチャネルならスキップ
            if (src.gameObject.name == "AudioSourceObject_BGM"
                && src.clip.name.Equals(leverName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            // それ以外は停止してクリップをクリア
            if (src.isPlaying) src.Stop();
            src.clip = null;
        }

        // 動画を全部止める ← ここを追加
        animMgr.StopVideoAll();
        // 4) Attachment（全スロット）をクリア（"obi_belt" スロットだけはスキップ）
        var skeleton = skeletonAnimation.skeleton;
        // 残したいスロット名を列挙（大文字小文字を無視）
        var keepSlots = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Belt",
        };
        foreach (var slot in skeleton.Slots)
        {
            // スロット名が keepSlots に含まれるならクリアをスキップ
            if (keepSlots.Contains(slot.Data.Name))
                continue;
            slot.Attachment = null;
        }
    }
    // public void ClearAnimAll()
    // {
    //     // Spine アニメを全部止める
    //     animMgr.StopAnimAll(skeletonAnimation);
    //     // サウンドを全部止める
    //     animMgr.StopAudioAll();
    //     // 動画を全部止める ← ここを追加
    //     animMgr.StopVideoAll();
    //     // Attachment（全スロット）をクリア
    //     animMgr.ClearAttachmentAll(skeletonAnimation);
    // }
    public void ResetReel()
    {
        boneReel[(int)REEL_LCR.REEL_L].Y = -89.0f;
        boneReel[(int)REEL_LCR.REEL_C].Y = -89.0f;
        boneReel[(int)REEL_LCR.REEL_R].Y = -89.0f;
    }

    public void EndAnim(TrackEntry te)
    {
        ANIM_TABLE tbl;

        string animName = te.Animation.Name;

        tokusyoValueList.TryGetValue(lottery.GetTokusyo(0), out int value);

        animName = animName.Replace("_In", "_Lp");
        // int index = GetAnimIndex(animName);
        int prizesInValue = (int)ANIM.PRIZES_IN + 1;
        tbl = ChangeAnimTbl(animTbl[prizesInValue], 0, value);
        animMgr.SetAnim(skeletonAnimation, tbl);


        // for (int i = 0; i < lottery.GetLotNum(); i++)
        // {
        //     if (i == 0)
        //     {
        //         tokusyoValueList.TryGetValue(lottery.GetTokusyo(0), out int value);

        //         animName = animName.Replace("_In", "_Lp");
        //         // int index = GetAnimIndex(animName);
        //         int prizesInValue = (int)ANIM.PRIZES_IN + 1;
        //         tbl = ChangeAnimTbl(animTbl[prizesInValue], 0, value);
        //         animMgr.SetAnim(skeletonAnimation, tbl);

        //         return;
        //     }
        //     else
        //     {
        //         //後々復活
        //         // tokusyoValueList.TryGetValue(lottery.GetTokusyo(i), out int value);

        //         // animName = animName.Replace("_In", "_Lp");
        //         // // int index = GetAnimIndex(animName);
        //         // int prizesInValue = (int)ANIM.PRIZES_SMALL_IN + 1;
        //         // tbl = ChangeAnimTbl(animTbl[prizesInValue], i, value);
        //         // animMgr.SetAnim(skeletonAnimation, tbl);
        //     }
        // }

    }
    public int GetAnimIndex(string name)
    {
        ANIM_TABLE tbl;
        for (int i = 0; i < animTbl.Length; i++)
        {
            if (animTbl[i].animName == name)
            {
                return i;
            }
            else
            {
                tbl = ChangeAnim(animTbl[i], 0, 0);
                if (tbl.animName == name)
                {
                    return i;
                }
            }
        }
        return -1;
    }
    public ANIM_TABLE ChangeAnimTbl(ANIM_TABLE tbl, int tokusyoNo, int tokusyoValue)
    {
        ANIM_TABLE animtbl = tbl;

        animtbl = ChangeAnim(animtbl, tokusyoNo, tokusyoValue);
        // animtbl = ChangeSound(animtbl);
        animtbl = ChangeTrack(animtbl, tokusyoNo);

        return animtbl;
    }
    public ANIM_TABLE ChangeAnim(ANIM_TABLE tbl, int tokusyoNo, int tokusyoValue)
    {
        ANIM_TABLE animtbl = tbl;

        // animtbl.animName = animtbl.animName.Replace("*CharName*", charaMgr.GetCharaName());
        // animtbl.animName = animtbl.animName.Replace("*ImageNo*", lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_CUTIN).ToString());
        // animtbl.animName = animtbl.animName.Replace("*TokusyoNum*", (lottery.GetLotNum() - 1).ToString());
        animtbl.animName = animtbl.animName.Replace("*No*", tokusyoNo.ToString());
        animtbl.animName = animtbl.animName.Replace("*TokusyoValue*", tokusyoValue.ToString());
        // animtbl.animName = animtbl.animName.Replace("*CharaGrp*", ((int)charaMgr.GetCharaGrp() + 1).ToString());

        return animtbl;
    }

    public ANIM_TABLE ChangeTrack(ANIM_TABLE tbl, int index)
    {
        ANIM_TABLE animtbl = tbl;

        if (animtbl.animName.Contains("Result_Prizes"))
        {
            animtbl.trackIndex = TRACK.TOKUSYO_1 + index;
        }
        else if (animtbl.animName.Contains("Prizes_Items"))
        {
            animtbl.trackIndex = TRACK.ITEMS;
        }
        else
        {
            animtbl.trackIndex = TRACK.TOKUSYO_1 + index;
        }
        return animtbl;
    }
    public void TokusyoAnim()
    {
        // 特賞演出を開始する
        ClearAnimAll();

        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.RESULT_BG]);
        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.RESULT_FLASH]);
        animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_RESULT]);



        // まず大演出
        tokusyoValueList.TryGetValue(lottery.GetTokusyo(0), out int bigValue);
        var bigTbl = ChangeAnimTbl(animTbl[(int)ANIM.PRIZES_IN], 0, bigValue);
        animMgr.SetAnim(
            skeletonAnimation,
            bigTbl,
            new CallbackInfo
            {
                Type = CALLBACK_TYPE.ANIM_END,
                Callback = obj =>
                {
                    EndAnim((TrackEntry)obj);
                    PlaySmallAnims();
                }
            }
        );
    }
    private void PlaySmallAnims()
    {
        tokusyoValueList.TryGetValue(lottery.GetTokusyo(0), out int Value);
        var items = ChangeAnimTbl(animTbl[(int)ANIM.PRIZES_ITEMS], 0, Value);
        animMgr.SetAnim(skeletonAnimation, items);

        for (int i = 1; i < lottery.GetLotNum(); i++)
        {
            tokusyoValueList.TryGetValue(lottery.GetTokusyo(i), out int smallValue);
            var smallTbl = ChangeAnimTbl(animTbl[(int)ANIM.PRIZES_SMALL_IN], i, smallValue);

            animMgr.SetAnim(skeletonAnimation, smallTbl);

        }
        SetGameState(GAME_STATE.STATE_END);

    }
    // public void SpineEvent(TrackEntry trackEntry, Spine.Event e)
    // {
    //     int imgNo = lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_CUTIN);
    //     if (e.Data.Name == "Event_Push")
    //     {
    //         SetPushAnim(ENSYUTU_TYPE.TYPE_PUSHAF);

    //         //フリーズはここで音量を戻す
    //         if (imgNo > (int)ENSYUTU_CUTIN.CUTIN_8)
    //         {
    //             animMgr.SetSoundVolume((int)SOUND_CH.CH_BGM, 5.0f, 0.5f);
    //         }
    //         animMgr.DeleteAnimEvent(this);
    //     }
    // }
    public void ReachedFrameVideo(VideoClip crip)
    {
    }

    public void EndVideo(object obj)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR

        // 0) 背景色の抽選結果を再取得
        // int bgColorValue = overrideBgColor >= 0
        //     ? overrideBgColor
        //     : lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_BGCOLOR);
#endif

        //bgColorを取得
        int bgColorValue = lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_BGCOLOR);
        VideoInfo info = (VideoInfo)obj;

        switch (info.index)
        {
            // —— 既存：レバーインのハンドリング ——————————
            case VIDEO.LEVER_IN:
                if (GetGameState() == GAME_STATE.STATE_BET)
                {
                    animMgr.SetAnim(
                        skeletonAnimation,
                        animTbl[(int)ANIM.LEVER_LP]
                    );
                }
                break;
            case VIDEO.CROON_CHA_IN:
                if (GetGameState() == GAME_STATE.STATE_LEVER)
                {
                    animMgr.SetAnim(
                        skeletonAnimation,
                        animTbl[(int)ANIM.CROON_CHA_LP]
                    );
                }
                break;
            // —— CROON_IN が終わったら CUTIN → 色＆12種ランダム —————
            case VIDEO.CROON_IN_BLUE:
            case VIDEO.CROON_IN_GREEN:
            case VIDEO.CROON_IN_RED:
                {

                    //////////////////////////////カットインを制御している/////////////////////////////////////////////////////////
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    // 色の抽選結果（0=青,1=緑,2=赤）
                    // int cutinColor = overrideCutin >= 0
                    // ? overrideCutin
                    // : lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_CUTINCOLOR);

                    // int cutinType = overrideCutinType >= 0
                    //     ? overrideCutinType
                    //     : lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_CUTINTYPE);
#endif

                    // カットインの色とタイプを取得
                    int cutinColor = lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_CUTINCOLOR);
                    int cutinType = lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_CUTINTYPE);

                    // 明示キャストしてセット
                    ANIM_TABLE tblCutin = animTbl[(int)ANIM.CUTIN_MOVIE];
                    tblCutin.videoIndex = (VIDEO)(tblCutin.videoIndex + cutinColor * (int)ENSYUTU_CUTIN_TYPE.CUTIN_MAX + cutinType);

                    animMgr.SetAnim(skeletonAnimation, tblCutin);
                    animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_CUTIN]);


                    //////////////////////////////クルーンのアウトを制御している/////////////////////////////////////////////////////////

                    int baseIdx = (int)VIDEO.CROON_OUT_BLUE_1_1 + lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_CROON_CATEGORY) * (int)ENSYUTU_CROON_PATTERN.PAT_MAX;
                    int subIdx = lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_CROON_PATTERN);
                    int targetIdx = baseIdx + subIdx;

                    ANIM_TABLE tblOut1 = animTbl[(int)ANIM.CROON_OUT];
                    tblOut1.videoIndex = (VIDEO)targetIdx;
                    animMgr.SetAnim(
                        skeletonAnimation,
                        tblOut1,
                        new CallbackInfo
                        {
                            Type = CALLBACK_TYPE.MOVIE_END,
                            Callback = _ => TokusyoAnim()
                        }
                    );
                    break;

                }

                // —— その他はクリア ————————————————————————
                // default:
                //     ClearAnim();
                //     break;
        }
    }
    public int ExtractValue(string line)
    {
        string[] parts = line.Split(':');
        if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int value))
        {
            return value;
        }
        return 0;
    }
    public void WriteConfig()
    {
        if (File.Exists(filePath[(int)FILE_PATH.CONFIG]))
        {
            string[] lines = File.ReadAllLines(filePath[(int)FILE_PATH.CONFIG]);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("1等:"))
                {
                    lines[i] = "1等:" + lottery.GetTokusyoValue(TOKUSYO.TOKUSYO_1ST);
                }
                else if (lines[i].StartsWith("2等:"))
                {
                    lines[i] = "2等:" + lottery.GetTokusyoValue(TOKUSYO.TOKUSYO_2ND);
                }
                else if (lines[i].StartsWith("3等:"))
                {
                    lines[i] = "3等:" + lottery.GetTokusyoValue(TOKUSYO.TOKUSYO_3RD);
                }
                else if (lines[i].StartsWith("4等:"))
                {
                    lines[i] = "4等:" + lottery.GetTokusyoValue(TOKUSYO.TOKUSYO_4TH);
                }
                else if (lines[i].StartsWith("5等:"))
                {
                    lines[i] = "5等:" + lottery.GetTokusyoValue(TOKUSYO.TOKUSYO_5TH);
                }
            }
            // 変更後のデータを再びファイルに書き込む
            File.WriteAllLines(filePath[(int)FILE_PATH.CONFIG], lines);
        }
    }
    public void LoadConfig()
    {
        int num = 0;

        if (File.Exists(filePath[(int)FILE_PATH.CONFIG]))
        {
            using (StreamReader sr = new StreamReader(filePath[(int)FILE_PATH.CONFIG]))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("1等:"))
                    {
                        num = ExtractValue(line);
                        lottery.SetTokusyoValue(TOKUSYO.TOKUSYO_1ST, num);
                    }
                    else if (line.StartsWith("2等:"))
                    {
                        num = ExtractValue(line);
                        lottery.SetTokusyoValue(TOKUSYO.TOKUSYO_2ND, num);
                    }
                    else if (line.StartsWith("3等:"))
                    {
                        num = ExtractValue(line);
                        lottery.SetTokusyoValue(TOKUSYO.TOKUSYO_3RD, num);
                    }
                    else if (line.StartsWith("4等:"))
                    {
                        num = ExtractValue(line);
                        lottery.SetTokusyoValue(TOKUSYO.TOKUSYO_4TH, num);
                    }
                    else if (line.StartsWith("5等:"))
                    {
                        num = ExtractValue(line);
                        lottery.SetTokusyoValue(TOKUSYO.TOKUSYO_5TH, num);
                    }
                    // ↓ 追加部分: 演出上書き値を読み込む
                    else if (line.StartsWith("sakibare:"))
                    {
                        overrideSakibare = ExtractValue(line);
                    }
                    else if (line.StartsWith("bgcolor:"))
                    {
                        overrideBgColor = ExtractValue(line);
                    }
                    else if (line.StartsWith("croon:"))
                    {
                        overrideCroon = ExtractValue(line);
                    }
                    else if (line.StartsWith("deme:"))
                    {
                        overrideDeme = ExtractValue(line);
                    }
                    else if (line.StartsWith("cutin:"))
                    {
                        overrideCutin = ExtractValue(line);
                    }
                    else if (line.StartsWith("cutintype:"))
                    {
                        overrideCutinType = ExtractValue(line);
                    }
                    else if (line.StartsWith("sakibarerandom:"))
                    {
                        overrideSakibareRandom = ExtractValue(line);
                    }
                }
            }
        }
        else
        {
            //ファイルが無かったら初期設定
            lottery.SetTokusyoValue(TOKUSYO.TOKUSYO_1ST, 5);
            lottery.SetTokusyoValue(TOKUSYO.TOKUSYO_2ND, 13);
            lottery.SetTokusyoValue(TOKUSYO.TOKUSYO_3RD, 20);
            lottery.SetTokusyoValue(TOKUSYO.TOKUSYO_4TH, 90);
            lottery.SetTokusyoValue(TOKUSYO.TOKUSYO_5TH, 299);
        }
    }
    public void SetFilePath()
    {
        //Editor実行時は絶対パスをセット
        if (Application.isEditor)
        {
            filePath = new string[]
            {
                "C:\\minpachi\\" + "config.ini"
            };
        }
        else
        {
            filePath = new string[]
            {
                ".\\" + "config.ini"
            };
        }
    }

    public string GetFilePath(FILE_PATH path)
    {
        return filePath[(int)path];
    }
    public IEnumerator API_Request()
    {
        string url = null;
        int retryCnt = 0;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        SetDebugString("API_Request");
#endif
        switch (ENV)
        {

            case "STG":
                url = Resources.Load<TextAsset>("api_stg").text.Trim();
                break;

            case "PROD":
                url = Resources.Load<TextAsset>("api_prod").text.Trim();
                break;

            default:
                //ローカル抽選
                lottery.LotTokusyo();
                lottery.LotEnsyutu();
                WriteConfig();
                break;
        }

        if (url != null)
        {
            while (retryCnt < RETRY_MAX)
            {
                string jsonData = $"{{\"drawCount\":\"{lotnum}\", \"pcName\":\"{pcName}\", \"lotteryTypeId\":\"{lotteryTypeId}\", \"lotteryId\":\"{lotteryId}\"}}";

                using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                    request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.timeout = TIME_OUT;

                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.ConnectionError ||
                        request.result == UnityWebRequest.Result.ProtocolError)
                    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        SetDebugString("通信エラー リトライ回数:" + retryCnt);
                        Debug.Log("通信エラー リトライ回数:" + retryCnt);

#endif
                        retryCnt++;
                        if (retryCnt >= RETRY_MAX)
                        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                            SetDebugString("通信エラー　リトライ上限");
                            Debug.Log("通信エラー　リトライ上限");

                            autoFlg = false;
#endif
                            ClearAnimAll();
                            Init_Idle();
                            yield break;
                        }
                    }
                    else
                    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        SetDebugString("通信成功");
#endif
                        API_Response_Data result = JsonUtility.FromJson<API_Response_Data>(request.downloadHandler.text);

                        //各データをデシリアライズ
                        IntermediateResults intermediateResults = JsonUtility.FromJson<IntermediateResults>(result.intermediateResults);
                        Presentation presentation = JsonUtility.FromJson<Presentation>(result.presentation);
                        Revival revival = JsonUtility.FromJson<Revival>(result.revival);
                        FinalResults finalResults = JsonUtility.FromJson<FinalResults>(result.finalResults);

                        //抽選個数のセット
                        lottery.SetTokusyoNum(finalResults.results.Length);

                        //抽選値をセット
                        for (int i = 0; i < finalResults.results.Length; i++)
                        {
                            int rawValue = finalResults.results[i];  // サーバーから来た生の値

                            // Enum 定義に存在するか確認
                            if (Enum.IsDefined(typeof(TOKUSYO), rawValue))
                            {
                                TOKUSYO tokusyo = (TOKUSYO)rawValue;
                                Debug.Log($"[抽選 {i}] サーバー結果: raw={rawValue} → enum={tokusyo}");
                                lottery.SetTokusyo(i, tokusyo);
                            }

                        }

                        //演出はローカル抽選
                        lottery.LotEnsyutu();

                        break;
                    }
                }
            }
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        // サキバレ用サウンド（DEFAULT or CHANCE×5）を条件で再生
        // var sakibareDetail = overrideSakibare >= 0
        //     ? (ENSYUTU_SAKIBARE)overrideSakibare
        //     : (ENSYUTU_SAKIBARE)lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_SAKIBARE);
#endif

        // サキバレ用サウンド（DEFAULT or CHANCE×5）を条件で再生
        var sakibareDetail = (ENSYUTU_SAKIBARE)lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_SAKIBARE);

        if (sakibareDetail == ENSYUTU_SAKIBARE.DEFAULT)
        {
            // デフォルトのクロインSE
            animMgr.SetAnim(skeletonAnimation, animTbl[(int)ANIM.SOUND_SE_CROON_IN]);
        }
        else
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR

            // CHANCE 5種からランダム再生
            //         int sakibareRandom = overrideSakibareRandom >= 0
            // ? overrideSakibareRandom
            // : UnityEngine.Random.Range(1, 6);  // 1～5
#endif

            int sakibareRandom = UnityEngine.Random.Range(1, 6);  // 1～5

            animMgr.SetAnim(
                skeletonAnimation,
                animTbl[(int)ANIM.SOUND_SE_CROON_IN_CHANCE1 + (sakibareRandom - 1)]
            );
        }


        //リール回転開始
        SetReelState(REEL_LCR.REEL_ALL, REEL_STATE.REEL_RUN);
    }

    void OnGUI()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        DebugInfoView();  // OnGUIの内部で呼び出す
#endif
    }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public void DebugInfoView()
    {
        // GUIスタイルの設定
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 60;
        guiStyle.normal.textColor = Color.white;

        // 黒背景
        // GUI.color = new Color(0, 0, 0, 1.0f);
        // GUI.Box(new Rect(0, 0, 400, 600), GUIContent.none);

        GUI.color = Color.white;

        // デバッグ情報表示（各種演出情報）
        // GUI.Label(new Rect(10, 10 + STR_INTERVAL * (++i), 400, 10), "BGCOLOR: " + lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_BGCOLOR).ToString(), guiStyle);
        // GUI.Label(new Rect(10, 10 + STR_INTERVAL * (++i), 400, 10), "ZUGARA: " + lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_CROON).ToString(), guiStyle);
        // GUI.Label(new Rect(10, 10 + STR_INTERVAL * (++i), 400, 10), "DEME: " + lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_DEME).ToString(), guiStyle);
        // GUI.Label(new Rect(10, 10 + STR_INTERVAL * (++i), 400, 10), "CUTIN: " + lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_CUTIN).ToString(), guiStyle);

        // GUI.Label(new Rect(10, 10 + STR_INTERVAL * (++i), 400, 10), "WAITFRAME:"+ waitFrame, guiStyle);
        // GUI.Label(new Rect(10, 10 + STR_INTERVAL * (++i), 400, 10), "FRAMECNT:" + frameCnt, guiStyle);
        // GUI.Label(new Rect(10, 10 + STR_INTERVAL * (++i), 400, 10), debugString, guiStyle);
        // GUI.Label(new Rect(10, 10 + STR_INTERVAL * (++i), 400, 10), "SAKIBARE: " + lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_SAKIBARE).ToString(), guiStyle);
        // GUI.Label(new Rect(10, 10 + STR_INTERVAL * (++i), 400, 10), "BGCOLOR: " + lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_BGCOLOR).ToString(), guiStyle);
        // GUI.Label(new Rect(10, 10 + STR_INTERVAL * (++i), 400, 10), "CUTINCOLOR: " + lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_CUTINCOLOR).ToString(), guiStyle);
        // GUI.Label(new Rect(10, 10 + STR_INTERVAL * (++i), 400, 10), "CUTINTYPE: " + lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_CUTINTYPE).ToString(), guiStyle);
        // GUI.Label(new Rect(10, 10 + STR_INTERVAL * (++i), 400, 10), "DEME: " + lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_DEME).ToString(), guiStyle);
        // GUI.Label(new Rect(10, 10 + STR_INTERVAL * (++i), 400, 10), "CROONCATEGORY: " + lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_CROON_CATEGORY).ToString(), guiStyle);
        // GUI.Label(new Rect(10, 10 + STR_INTERVAL * (++i), 400, 10), "CROONPAT: " + lottery.GetEnsyutuDetail(ENSYUTU_TYPE.TYPE_CROON_PATTERN).ToString(), guiStyle);
    }
    private void SetDebugString(string str)
    {
        debugString = str;
    }

    // === 動的画像はめ込み機能の実装 ===

    /// <summary>
    /// 動的画像システムの初期化
    /// </summary>
    private void InitializeDynamicImageSystem()
    {
        // StreamingAssetsフォルダ内の動的画像用フォルダパスを設定
        dynamicImagesPath = Path.Combine(Application.streamingAssetsPath, "DynamicImages");

        // フォルダが存在しない場合は作成
        if (!Directory.Exists(dynamicImagesPath))
        {
            Directory.CreateDirectory(dynamicImagesPath);
        }

        // デフォルトのスロット-ファイル名マッピングをコピー
        foreach (var mapping in defaultSlotMapping)
        {
            slotImageMapping[mapping.Key] = mapping.Value;
        }

        // 既存の画像ファイルをチェックして初期読み込み
        LoadExistingImages();

        isDynamicImageEnabled = true;

        // デフォルトのシーン設定を初期化
        InitializeDefaultSceneSettings();

        // 現在のゲーム状態を取得して適用
        currentGameState = GetGameState();
        ApplySceneSlotVisibility(currentGameState);
    }

    /// <summary>
    /// 既存の画像ファイルを読み込み
    /// </summary>
    private void LoadExistingImages()
    {
        if (!Directory.Exists(dynamicImagesPath)) return;

        string[] pngFiles = Directory.GetFiles(dynamicImagesPath, "*.png");

        foreach (string filePath in pngFiles)
        {
            string fileName = Path.GetFileName(filePath);
            string slotName = GetSlotNameFromFileName(fileName);

            if (!string.IsNullOrEmpty(slotName))
            {
                LoadImageToSlot(filePath, slotName);
            }
        }
    }

    /// <summary>
    /// ファイル名からスロット名を取得
    /// </summary>
    private string GetSlotNameFromFileName(string fileName)
    {
        foreach (var mapping in slotImageMapping)
        {
            if (mapping.Value.Equals(fileName, StringComparison.OrdinalIgnoreCase))
            {
                return mapping.Key;
            }
        }
        return null;
    }

    /// <summary>
    /// 画像をスロットに読み込み
    /// </summary>
    private void LoadImageToSlot(string filePath, string slotName)
    {
        if (!File.Exists(filePath)) return;
        if (skeletonAnimation == null) return;

        // AnimMgrの既存メソッドを使用して画像をはめ込み
        animMgr.SetPngToAttachment(skeletonAnimation, filePath, slotName, false, "", true);
    }

    /// <summary>
    /// チケット表示を更新（動的画像対応）
    /// </summary>
    private void UpdateTicketDisplay()
    {
        // 動的画像ファイル名（例: ticket_1.png, ticket_2.png, ...）
        string fileName = $"ticket_{lotnum}.png";
        string filePath = Path.Combine(dynamicImagesPath, fileName);

        // 動的画像が有効で、ファイルが存在する場合は動的画像を使用
        if (isDynamicImageEnabled && File.Exists(filePath))
        {
            if (skeletonAnimation == null) return;

            // "ticket" スロットに対して、動的画像をアタッチメントとして設定
            // アタッチメント名は lotnum の文字列（"1", "2", ..., "10"）
            animMgr.SetPngToAttachment(skeletonAnimation, filePath, "ticket", false, lotnum.ToString(), true);
        }
        else
        {
            // フォールバック: 従来のSpineアタッチメントを使用
            animMgr.SetAnim("ticket", lotnum.ToString(), true);
        }
    }

    /// <summary>
    /// 動的画像システムの有効/無効切り替え
    /// </summary>
    public void SetDynamicImageEnabled(bool enabled)
    {
        isDynamicImageEnabled = enabled;
    }

    /// <summary>
    /// 特定スロットの画像を表示/非表示切り替え
    /// </summary>
    public void SetSlotVisible(string slotName, bool visible)
    {
        if (skeletonAnimation != null)
        {
            Slot slot = skeletonAnimation.Skeleton.FindSlot(slotName);
            if (slot != null)
            {
                if (visible)
                {
                    // 対応する画像ファイルがあるかチェックして読み込み
                    string fileName = slotImageMapping.ContainsKey(slotName) ? slotImageMapping[slotName] : null;
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        string filePath = Path.Combine(dynamicImagesPath, fileName);
                        if (File.Exists(filePath))
                        {
                            LoadImageToSlot(filePath, slotName);
                        }
                    }
                }
                else
                {
                    // アタッチメントをnullにして非表示
                    slot.Attachment = null;
                }
            }
        }
    }

    // === シーン管理システム ===

    /// <summary>
    /// 特定のゲーム状態での各スロットの表示設定を行う
    /// </summary>
    public void SetSceneSlotVisibility(GAME_STATE gameState, string slotName, bool visible)
    {
        if (!sceneSlotVisibility.ContainsKey(gameState))
        {
            sceneSlotVisibility[gameState] = new Dictionary<string, bool>();
        }

        sceneSlotVisibility[gameState][slotName] = visible;

        // 現在のゲーム状態の場合は即座に反映
        if (currentGameState == gameState)
        {
            SetSlotVisible(slotName, visible);
        }

    }

    /// <summary>
    /// ゲーム状態に基づいて動的画像の表示を適用
    /// </summary>
    private void ApplySceneSlotVisibility(GAME_STATE gameState)
    {
        if (sceneSlotVisibility.ContainsKey(gameState))
        {
            foreach (var slotVisibility in sceneSlotVisibility[gameState])
            {
                SetSlotVisible(slotVisibility.Key, slotVisibility.Value);
            }
        }
    }

    /// <summary>
    /// デフォルトのシーン表示設定を初期化
    /// </summary>
    private void InitializeDefaultSceneSettings()
    {
        // STATE_IDLE（待機状態）での表示設定
        SetSceneSlotVisibility(GAME_STATE.STATE_IDLE, "slot_dynamic_bg", true);      // 背景表示
        SetSceneSlotVisibility(GAME_STATE.STATE_IDLE, "slot_dynamic_effect", false); // エフェクト非表示
        SetSceneSlotVisibility(GAME_STATE.STATE_IDLE, "slot_dynamic_chara", true);   // キャラクター表示
        SetSceneSlotVisibility(GAME_STATE.STATE_IDLE, "slot_dynamic_logo", true);    // ロゴ表示
        SetSceneSlotVisibility(GAME_STATE.STATE_IDLE, "slot_dynamic_item", false);   // アイテム非表示

        // STATE_BET（ベット状態）での表示設定
        SetSceneSlotVisibility(GAME_STATE.STATE_BET, "slot_dynamic_bg", false);       // 背景表示
        SetSceneSlotVisibility(GAME_STATE.STATE_BET, "slot_dynamic_effect", false);  // エフェクト非表示
        SetSceneSlotVisibility(GAME_STATE.STATE_BET, "slot_dynamic_chara", true);    // キャラクター表示
        SetSceneSlotVisibility(GAME_STATE.STATE_BET, "slot_dynamic_logo", false);    // ロゴ非表示
        SetSceneSlotVisibility(GAME_STATE.STATE_BET, "slot_dynamic_item", true);     // アイテム表示

        // STATE_LEVER（レバー状態）での表示設定
        SetSceneSlotVisibility(GAME_STATE.STATE_LEVER, "slot_dynamic_bg", false);     // 背景表示
        SetSceneSlotVisibility(GAME_STATE.STATE_LEVER, "slot_dynamic_effect", true); // エフェクト表示
        SetSceneSlotVisibility(GAME_STATE.STATE_LEVER, "slot_dynamic_chara", true);  // キャラクター表示
        SetSceneSlotVisibility(GAME_STATE.STATE_LEVER, "slot_dynamic_logo", false);  // ロゴ非表示
        SetSceneSlotVisibility(GAME_STATE.STATE_LEVER, "slot_dynamic_item", false);  // アイテム非表示

        // STATE_END（終了状態）での表示設定
        SetSceneSlotVisibility(GAME_STATE.STATE_END, "slot_dynamic_bg", false);      // 背景非表示
        SetSceneSlotVisibility(GAME_STATE.STATE_END, "slot_dynamic_effect", true);   // エフェクト表示
        SetSceneSlotVisibility(GAME_STATE.STATE_END, "slot_dynamic_chara", false);   // キャラクター非表示
        SetSceneSlotVisibility(GAME_STATE.STATE_END, "slot_dynamic_logo", false);    // ロゴ非表示
        SetSceneSlotVisibility(GAME_STATE.STATE_END, "slot_dynamic_item", true);     // アイテム表示
    }

    private void OnDestroy()
    {
        // クリーンアップ
        if (animMgr != null)
        {
            animMgr.Dispose();
        }
    }
    private void OnApplicationQuit()
    {
        OnDestroy();
    }
#endif
}



