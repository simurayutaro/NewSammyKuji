using System;
public enum CONTROLLER
{
    MAXBET = 0,
    ONEBET,
    OUT,
    LEVER,
    LEFTBUTTON,
    CENTERBUTTON,
    RIGHTBUTTON,
    SW1_SW6,
    SW2_SW5,
    SW3_SW4,
    PUSHBUTTON = 10,
    ARROW_LEFT,
    ARROW_RIGHT,
    ARROW_UP,
    ARROW_DOWN,
    NONE = 0xffff,
};

public enum VIDEO
{
    NONE,
    DEMO,
    LEVER_IN,
    LEVER_LP,
    CROON_CHA_IN,
    CROON_CHA_LP,
    CROON_CHA_BG,
    CROON_BG_BLUE,
    CROON_BG_GREEN,
    CROON_BG_RED,
    CROON_IN_BLUE,
    CROON_IN_GREEN,
    CROON_IN_RED,
    CUTIN_BLUE_1,
    CUTIN_BLUE_2,
    CUTIN_BLUE_3,
    CUTIN_BLUE_4,
    CUTIN_BLUE_5,
    CUTIN_BLUE_6,
    CUTIN_BLUE_7,
    CUTIN_BLUE_8,
    CUTIN_BLUE_9,
    CUTIN_BLUE_10,
    CUTIN_BLUE_11,
    CUTIN_BLUE_12,
    CUTIN_GREEN_1,
    CUTIN_GREEN_2,
    CUTIN_GREEN_3,
    CUTIN_GREEN_4,
    CUTIN_GREEN_5,
    CUTIN_GREEN_6,
    CUTIN_GREEN_7,
    CUTIN_GREEN_8,
    CUTIN_GREEN_9,
    CUTIN_GREEN_10,
    CUTIN_GREEN_11,
    CUTIN_GREEN_12,
    CUTIN_RED_1,
    CUTIN_RED_2,
    CUTIN_RED_3,
    CUTIN_RED_4,
    CUTIN_RED_5,
    CUTIN_RED_6,
    CUTIN_RED_7,
    CUTIN_RED_8,
    CUTIN_RED_9,
    CUTIN_RED_10,
    CUTIN_RED_11,
    CUTIN_RED_12,
    CROON_OUT_BLUE_1_1,
    CROON_OUT_BLUE_1_2,
    CROON_OUT_BLUE_1_3,
    CROON_OUT_BLUE_1_4,
    CROON_OUT_BLUE_1_5,
    CROON_OUT_BLUE_1_6,
    CROON_OUT_BLUE_1_7,
    CROON_OUT_BLUE_1_8,
    CROON_OUT_BLUE_1_9,
    CROON_OUT_BLUE_1_10,
    CROON_OUT_BLUE_2_1,
    CROON_OUT_BLUE_2_2,
    CROON_OUT_BLUE_2_3,
    CROON_OUT_BLUE_2_4,
    CROON_OUT_BLUE_2_5,
    CROON_OUT_BLUE_2_6,
    CROON_OUT_BLUE_2_7,
    CROON_OUT_BLUE_2_8,
    CROON_OUT_BLUE_2_9,
    CROON_OUT_BLUE_2_10,
    CROON_OUT_BLUE_3_1,
    CROON_OUT_BLUE_3_2,
    CROON_OUT_BLUE_3_3,
    CROON_OUT_BLUE_3_4,
    CROON_OUT_BLUE_3_5,
    CROON_OUT_BLUE_3_6,
    CROON_OUT_BLUE_3_7,
    CROON_OUT_BLUE_3_8,
    CROON_OUT_BLUE_3_9,
    CROON_OUT_BLUE_3_10,
    CROON_OUT_BLUE_4_1,
    CROON_OUT_BLUE_4_2,
    CROON_OUT_BLUE_4_3,
    CROON_OUT_BLUE_4_4,
    CROON_OUT_BLUE_4_5,
    CROON_OUT_BLUE_4_6,
    CROON_OUT_BLUE_4_7,
    CROON_OUT_BLUE_4_8,
    CROON_OUT_BLUE_4_9,
    CROON_OUT_BLUE_4_10,
    CROON_OUT_BLUE_5_1,
    CROON_OUT_BLUE_5_2,
    CROON_OUT_BLUE_5_3,
    CROON_OUT_BLUE_5_4,
    CROON_OUT_BLUE_5_5,
    CROON_OUT_BLUE_5_6,
    CROON_OUT_BLUE_5_7,
    CROON_OUT_BLUE_5_8,
    CROON_OUT_BLUE_5_9,
    CROON_OUT_BLUE_5_10,
    CROON_OUT_BLUE_6_1,
    CROON_OUT_BLUE_6_2,
    CROON_OUT_BLUE_6_3,
    CROON_OUT_BLUE_6_4,
    CROON_OUT_BLUE_6_5,
    CROON_OUT_BLUE_6_6,
    CROON_OUT_BLUE_6_7,
    CROON_OUT_BLUE_6_8,
    CROON_OUT_BLUE_6_9,
    CROON_OUT_BLUE_6_10,
    CROON_OUT_BLUE_7_1,
    CROON_OUT_BLUE_7_2,
    CROON_OUT_BLUE_7_3,
    CROON_OUT_BLUE_7_4,
    CROON_OUT_BLUE_7_5,
    CROON_OUT_BLUE_7_6,
    CROON_OUT_BLUE_7_7,
    CROON_OUT_BLUE_7_8,
    CROON_OUT_BLUE_7_9,
    CROON_OUT_BLUE_7_10,
    CROON_OUT_GREEN_1_1,
    CROON_OUT_GREEN_1_2,
    CROON_OUT_GREEN_1_3,
    CROON_OUT_GREEN_1_4,
    CROON_OUT_GREEN_1_5,
    CROON_OUT_GREEN_1_6,
    CROON_OUT_GREEN_1_7,
    CROON_OUT_GREEN_1_8,
    CROON_OUT_GREEN_1_9,
    CROON_OUT_GREEN_1_10,
    CROON_OUT_GREEN_2_1,
    CROON_OUT_GREEN_2_2,
    CROON_OUT_GREEN_2_3,
    CROON_OUT_GREEN_2_4,
    CROON_OUT_GREEN_2_5,
    CROON_OUT_GREEN_2_6,
    CROON_OUT_GREEN_2_7,
    CROON_OUT_GREEN_2_8,
    CROON_OUT_GREEN_2_9,
    CROON_OUT_GREEN_2_10,
    CROON_OUT_GREEN_3_1,
    CROON_OUT_GREEN_3_2,
    CROON_OUT_GREEN_3_3,
    CROON_OUT_GREEN_3_4,
    CROON_OUT_GREEN_3_5,
    CROON_OUT_GREEN_3_6,
    CROON_OUT_GREEN_3_7,
    CROON_OUT_GREEN_3_8,
    CROON_OUT_GREEN_3_9,
    CROON_OUT_GREEN_3_10,
    CROON_OUT_GREEN_4_1,
    CROON_OUT_GREEN_4_2,
    CROON_OUT_GREEN_4_3,
    CROON_OUT_GREEN_4_4,
    CROON_OUT_GREEN_4_5,
    CROON_OUT_GREEN_4_6,
    CROON_OUT_GREEN_4_7,
    CROON_OUT_GREEN_4_8,
    CROON_OUT_GREEN_4_9,
    CROON_OUT_GREEN_4_10,
    CROON_OUT_GREEN_5_1,
    CROON_OUT_GREEN_5_2,
    CROON_OUT_GREEN_5_3,
    CROON_OUT_GREEN_5_4,
    CROON_OUT_GREEN_5_5,
    CROON_OUT_GREEN_5_6,
    CROON_OUT_GREEN_5_7,
    CROON_OUT_GREEN_5_8,
    CROON_OUT_GREEN_5_9,
    CROON_OUT_GREEN_5_10,
    CROON_OUT_RED_1_1,
    CROON_OUT_RED_1_2,
    CROON_OUT_RED_1_3,
    CROON_OUT_RED_1_4,
    CROON_OUT_RED_1_5,
    CROON_OUT_RED_1_6,
    CROON_OUT_RED_1_7,
    CROON_OUT_RED_1_8,
    CROON_OUT_RED_1_9,
    CROON_OUT_RED_1_10,
    CROON_OUT_RED_2_1,
    CROON_OUT_RED_2_2,
    CROON_OUT_RED_2_3,
    CROON_OUT_RED_2_4,
    CROON_OUT_RED_2_5,
    CROON_OUT_RED_2_6,
    CROON_OUT_RED_2_7,
    CROON_OUT_RED_2_8,
    CROON_OUT_RED_2_9,
    CROON_OUT_RED_2_10,
    CROON_OUT_RED_3_1,
    CROON_OUT_RED_3_2,
    CROON_OUT_RED_3_3,
    CROON_OUT_RED_3_4,
    CROON_OUT_RED_3_5,
    CROON_OUT_RED_3_6,
    CROON_OUT_RED_3_7,
    CROON_OUT_RED_3_8,
    CROON_OUT_RED_3_9,
    CROON_OUT_RED_3_10,
    RESULT_BG,
    MAX
};

public enum CONTROLLER_SWITCH
{
    OFF = 0,
    ON
};
public enum TRACK
{
    NONE,
    SHITAPANEL,
    OBI,
    REEL,
    REEL_EFFECT,
    REEL_FLASH_L,
    REEL_FLASH_C,
    REEL_FLASH_R,
    TOKUSYO_1,
    TOKUSYO_2,
    TOKUSYO_3,
    TOKUSYO_4,
    TOKUSYO_5,
    TOKUSYO_6,
    TOKUSYO_7,
    TOKUSYO_8,
    TOKUSYO_9,
    TOKUSYO_10,
    ITEMS,
    FLASH,
    MAX
};

public enum REEL_LCR
{
    REEL_L = 0,
    REEL_C,
    REEL_R,
    REEL_ALL
};


public enum GAME_STATE
{
    STATE_IDLE,
    STATE_BET,
    STATE_LEVER,
    STATE_END
};
public enum REEL_STATE
{
    REEL_NONE,
    REEL_IDLE,
    REEL_RUN,
    REEL_STOP
};

public enum DATA_TYPE
{
    SOUND,
    MOVIE,
    RENDER_TEXTURE
};

public enum VIDEO_TYPE
{
    ONESHOT = 0,
    LOOP,
};
public enum VIDEO_CH
{
    CH_1 = 0,
    CH_2,
    CH_3,
    CH_4,
    CH_5,
    CH_MAX
}
public struct ANIM_TABLE
{
    public TRACK trackIndex;
    public string animName;
    public bool loopFlg;
    public SOUND_TYPE type;
    public SOUND soundIndex;
    public float volume;
    public VIDEO_CH videoCH;
    public int videoLoop;
    public VIDEO videoIndex;
};

public enum SOUND_TYPE
{
    ONESHOT = 0,
    LOOP,
    LOOP_FADE,
};
public enum SOUND_CH
{
    CH_BGM = 0,
    CH_SUITABLE,
    CH_VO_SE,
    MAX,
}
public enum SOUND
{
    NONE,
    BGM_IDLE,
    BGM_LEVER,
    SE_TICKET,
    SE_LEVER_IN,
    SE_CROON_IN,
    SE_CROON_IN_CHANCE1,
    SE_CROON_IN_CHANCE2,
    SE_CROON_IN_CHANCE3,
    SE_CROON_IN_CHANCE4,
    SE_CROON_IN_CHANCE5,
    SE_REEL_DF,
    SE_REEL_CH,
    SE_CUTIN,
    SE_RESULT,
    MAX
};

public enum CALLBACK_TYPE
{
    NONE,
    ANIM_END,
    SOUND_END,
    EVENT,
    EVENT_ANIM_END,
    MOVIE_END,
    MOVIE_FRAME_REACHED,
    MOVIE_END_FRAME_REACHED
};
public enum GAME_MODE
{
    EASY,
    NORMAL,
    HARD
}
public enum GAME_LEVEL
{
    LEVEL1,
    LEVEL2,
    LEVEL3,
};
public enum EVALUATE
{
    MISS,
    SOSO,
    GOOD,
    EXCELLENT,
    BONUS
};
public enum RANK
{
    RANK_NONE,
    RANK_1ST,
    RANK_2ND,
    RANK_3RD,
    RANK_4TH,
    RANK_5TH,
    RANK_6TH,
    RANK_7TH,
    RANK_8TH,
    RANK_9TH,
    RANK_10TH,
}
public enum CONTROLLER_TYPE
{
    COLOR,
    VIBE_ONE,
    VIBE_REN,
};
public enum CONTROLLER_DEVICE
{
    NONE = 0,
    LEFT_R = 1 << 0,
    LEFT_G = 1 << 1,
    LEFT_B = 1 << 2,
    RIGHT_RGB = LEFT_R + LEFT_G + LEFT_B,
    CENTER_R = 1 << 3,
    CENTER_G = 1 << 4,
    CENTER_B = 1 << 5,
    CENTER_RGB = CENTER_B + CENTER_R + CENTER_G,
    RIGHT_R = 1 << 6,
    RIGHT_G = 1 << 7,
    RIGHT_B = 1 << 8,
    LEFT_RGB = RIGHT_R + RIGHT_G + RIGHT_B,
    PUSH = 1 << 11,
    BET_R = 1 << 13,
    BET_G = 1 << 14,
    BET_B = 1 << 15,
    BET_RGB = BET_B + BET_R + BET_G,
};
public enum TOKUSYO
{
    TOKUSYO_NONE = 0,
    TOKUSYO_7TH,
    TOKUSYO_6TH,
    TOKUSYO_5TH,
    TOKUSYO_4TH,
    TOKUSYO_3RD,
    TOKUSYO_2ND,
    TOKUSYO_1ST,
};
public enum ENSYUTU_TYPE
{
    TYPE_SAKIBARE,
    TYPE_BGCOLOR,
    TYPE_CROON_CATEGORY,
    TYPE_CROON_PATTERN,
    TYPE_DEME,
    TYPE_CUTINCOLOR,
    TYPE_CUTINTYPE,
};
public struct ENSYUTU_TABLE
{
    public ENSYUTU_SAKIBARE sakibare;
    public ENSYUTU_BGCOLOR bgcolor;
    public ENSYUTU_CROON_CATRGORY croonCat;
    public ENSYUTU_CROON_PATTERN croonPat;
    public ENSYUTU_DEME deme;
    public ENSYUTU_CUTIN_COLOR cutinColor;
    public ENSYUTU_CUTIN_TYPE cutinType;
};

public enum ENSYUTU_SAKIBARE
{
    DEFAULT,
    CHANCE,
};

public enum ENSYUTU_BGCOLOR
{
    BLUE,
    GREEN,
    RED,
};
public enum CROON_HOLE
{
    BLUE = 0,
    GREEN = 7,
    RED = 12
};
public enum ENSYUTU_CROON_CATRGORY
{
    Blue_1,
    Blue_2,
    Blue_3,
    Blue_4,
    Blue_5,
    Blue_6,
    Blue_7,
    Green_1,
    Green_2,
    Green_3,
    Green_4,
    Green_5,
    Red_1,
    Red_2,
    Red_3,
};
public enum ENSYUTU_CROON_PATTERN
{
    PAT_1,
    PAT_2,
    PAT_3,
    PAT_4,
    PAT_5,
    PAT_6,
    PAT_7,
    PAT_8,
    PAT_9,
    PAT_10,
    PAT_MAX
}
public enum ENSYUTU_DEME
{
    LINE3_1,
    LINE2_1,
    LINE2_2,
    LINE2_3,
    LINE2_4,
    LINE1_1,
    LINE1_2,
    LINE1_3,
    LINE1_4,
    LINE1_5,
    LINE1_6,
};
public enum ENSYUTU_CUTIN_COLOR
{
    BLUE,
    GREEN,
    RED
};
public enum ENSYUTU_CUTIN_TYPE
{
    CUTIN_1,
    CUTIN_2,
    CUTIN_3,
    CUTIN_4,
    CUTIN_5,
    CUTIN_6,
    CUTIN_7,
    CUTIN_8,
    CUTIN_9,
    CUTIN_10,
    CUTIN_11,
    CUTIN_12,
    CUTIN_MAX
};
public enum ENSYUTU_PUSH
{
    NONE,
    NORMAL,
    RED,
    DEKA,
    NORMAL_VIB,
    RED_VIB,
    DEKA_VIB,
};


public enum PROCESS_CONTROL
{
    NONE,
    GETHANDLE,
    MINIMIZE,
    KILL,
};
public enum CHARA_GRP
{
    HRK_AOI_KRM,
    MGR_SMR,
    TSL_NIN
};
public enum FILE_PATH
{
    CONFIG,
};
public class CallbackInfo
{
    public CALLBACK_TYPE Type;                // 種別（ANIM_END, SOUND_END など）
    public Action<object> Callback;           // 実行関数
    public int? Frame;                        // 指定フレーム（必要な場合のみ）
};
public struct VideoInfo
{
    public VIDEO index;
    public VIDEO_CH ch;
};
public struct SoundInfo
{
    public SOUND index;
    public SOUND_TYPE type;
    public float volume;
};



