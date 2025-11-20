/* RAPIC5 Unity Plugin */
/* Copyright 2013-2018 AXELL CORPORATION */

using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Runtime.InteropServices;

public class RM5Decoder
{
	/* フレーム情報構造体 */

	[StructLayout(LayoutKind.Sequential)]
	public class rm5FrameInfo
	{
		public UInt32 width;       /* 水平画素数 */
		public UInt32 height;      /* ライン数 */
		public UInt32 stream_type; /* ストリームタイプ　RM5_STREAM_TYPE_*定数 */
		public UInt32 frame_type;  /* フレームタイプ RM5_FRAME_TYPE_*定数 */
		public UInt32 frame_size;  /* フレーム圧縮データのサイズ */
		public UInt32 frame_index; /* GOP内のフレーム番号（0から始まる） */
		public UInt32 frame_addr;  /* GOP内のフレーム開始アドレス（バイト単位） */
		public UInt32 gop_index;   /* 所属するGOP番号 */
		public UInt32 gop_addr;    /* ストリームにおけるGOPの開始アドレス（バイト単位） */
		public UInt32 final_frame; /* 最終フレームかどうか */
	}

	/* ムービー情報構造体 */
	
	[StructLayout(LayoutKind.Sequential)]
	public class rm5MovieInfo
	{
		public UInt32 width;           /* 水平画素数 */
		public UInt32 height;          /* ライン数 */
		public UInt32 stream_type;     /* ストリームタイプ　RM5_STREAM_TYPE_*定数 */
		public UInt32 total_frames;    /* 保有フレーム数 */
		public UInt32 total_gops;      /* 保有GOP数 */
		public UInt32 fps_numerator;   /* 分数表現したフレームレート(フレーム毎秒)の分子 */
		public UInt32 fps_denominator; /* 分数表現したフレームレート(フレーム毎秒)の分母 */
		public UInt32 flags;           /* RM5DEC_FLAG定数の論理和 */
	}

	/* ファイルアクセスコールバック構造体 */

	public delegate IntPtr rm5CallbackOpen(IntPtr args);
	public delegate Int32 rm5CallbackSeek(IntPtr fp, Int64 offset);
	public delegate Int64 rm5CallbackTell(IntPtr fp);
	public delegate Int64 rm5CallbackSize(IntPtr fp);
	public delegate Int32 rm5CallbackRead(IntPtr dest, Int64 size, IntPtr fp);
	public delegate Int32 rm5CallbackClose(IntPtr fp);

	[StructLayout(LayoutKind.Sequential)]
	public struct rm5DecoderCallback
	{
	  public rm5CallbackOpen  fopen;     /* ユーザ定義fopen関数 */
	  public rm5CallbackSeek  fseek;     /* ユーザ定義fseek関数 */
	  public rm5CallbackTell  ftell;     /* ユーザ定義ftell関数 */
	  public rm5CallbackRead  fread;     /* ユーザ定義fread関数 */
	  public rm5CallbackSize  fsize;     /* ユーザ定義fsize関数 */
	  public rm5CallbackClose fclose;    /* ユーザ定義fclose関数 */
	}

	public const Int32 RM5DEC_CALLBACK_VERSION           = 1;

	/* 内部バッファ取得用構造体 */

	[StructLayout(LayoutKind.Sequential)]
	public class rm5InnerBuffer {
	  public IntPtr y;                /* 輝度のバッファ(有効範囲はu10bit)(uint16) */
	  public IntPtr u;                /* 色差(青)のバッファ(有効範囲はu10bit)(uint16) */
	  public IntPtr v;                /* 色差(赤)のバッファ(有効範囲はu10bit)(uint16) */
	  public IntPtr alpha;            /* アルファチャネルのバッファ (uint8) */
	  public IntPtr index;            /* パレットインデックスのバッファ (uint) */
	  public IntPtr palette;          /* BGRAパレットのバッファ、RGBQUAD形式(uint)  */
	  public uint yuv_stride;         /* YUVの次のラインへのサイズ(short単位) */
	  public uint alpha_stride;       /* アルファの次のラインへのサイズ(unsigned char単位) */
	  public uint index_stride;       /* パレットインデックスの次のラインへのサイズ(unsigned char単位) */
	}

	/* エラーコード */
	
	public const Int32 RM5DEC_STATUS_SUCCESS			 = 0;   /* 処理は成功した */
	public const Int32 RM5DEC_STATUS_INVALID_ARGUMENT	 = 1;   /* 不正な引数を指定した */
	public const Int32 RM5DEC_STATUS_INVALID_STATE		 = 2;   /* 現在の設定ではこの関数を呼ぶことはできない */
	public const Int32 RM5DEC_STATUS_INVALID_VERSION	 = 3;   /* 指定したバージョンのファイルか構造体に対応していない */
	public const Int32 RM5DEC_STATUS_FAILED_FILE_API	 = 4;   /* ファイルの読み込みに失敗した */
	public const Int32 RM5DEC_STATUS_FAILED_LOAD_LIBRARY = 5;   /* デコードに必要なDLLが見つからない */
	public const Int32 RM5DEC_STATUS_BROKEN				 = 6;   /* 壊れたRM5ファイルが渡された */
	public const Int32 RM5DEC_STATUS_MEMORY_INSUFFICIENT = 7;   /* メモリが不足している  */
	public const Int32 RM5DEC_STATUS_THREAD_ERROR		 = 8;   /* スレッドAPIでエラーが発生した */
	public const Int32 RM5DEC_STATUS_OTHER_ERROR		 = 128; /* その他のエラーが発生した */

	/* デコード画像フォーマット */
	
	public const Int32 RM5DEC_IMAGE_RGBQUAD          = 0; /* 1ピクセル32ビット B,G,R,A順(Windows/Mac) R,G,B,A順(Android/iOS) */
	public const Int32 RM5DEC_IMAGE_RGBQUAD_PREMULTA = 4; /* 1ピクセル32ビット B,G,R,A順(Windows/Mac) R,G,B,A順(Android/iOS) */
                                                          /* RGB値にAが乗算されたビットプレーンが返る */
	public const Int32 RM5DEC_IMAGE_RGBA             = 6; /* 1ピクセル32ビット R,G,B,A順(Windows/Mac/Android/iOS) */

	public const Int32 RM5DEC_ALPHA                  = 0; /* 1ピクセル8ビット ALPHA値 */

	/* マルチスレッドモード */
	
	public const Int32 RM5DEC_MULTITHREAD_AUTO = 0;
	
	/* デコードストリームフォーマット */

	public const Int32 RM5DEC_STREAM_TYPE_YUV444     = 0x01; /* YUV444専用ストリーム */
	public const Int32 RM5DEC_STREAM_TYPE_YUV422     = 0x02; /* YUV422専用ストリーム */
	public const Int32 RM5DEC_STREAM_TYPE_INDEX	     = 0x03; /* パレットインデックスストリーム */
	public const Int32 RM5DEC_STREAM_TYPE_LOSSLESS   = 0x04; /* 32bpp無歪画像ストリーム */
	
	public const Int32 RM5DEC_STREAM_TYPE_IMAGE_MASK = 0x0f; /* イメージ情報を取得するためのマスク */
	public const Int32 RM5DEC_STREAM_TYPE_ALPHA_MASK = 0x10; /* α情報を取得するためのマスク */
	
	/* バージョン情報 */
	
	public const Int32 RM5DEC_MOVIE_INFO_VERSION = 3; /* 構造体バージョン */
	public const Int32 RM5DEC_FRAME_INFO_VERSION = 3; /* 構造体バージョン */

	/* API定義 */
	
	#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_PS4 || UNITY_EDITOR || UNITY_ANDROID
		const String LIBRARY_NAME="rm5decoder";
	#else
		const String LIBRARY_NAME="__Internal";
	#endif
	
/**
 *  RM5デコーダオブジェクトを作成／初期化します。(標準ファイルAPI)
 *  引数:
 *    decoder       - デコーダオブジェクトポインタの格納先へのポインタ
 *    strm_filename - デコードするRM5ムービーファイル名(A:MBCS/W:UTF16)
 *    num_thread    - RM5DEC_MULTITHREAD_AUTOもしくはデコードスレッド数
 *  返値:
 *    成功した場合、RM5DEC_STATUS_SUCCESSを返す。そうでなければ、それ以外のRM5DEC_STATUS_xxx定数を返す。
 *  解説:
 *    デコード対象はRM5ファイル。ファイルは標準APIを使用して読み込みます。
 */

	#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
	[DllImport(LIBRARY_NAME, EntryPoint = "rm5CreateDecoderW", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern int rm5CreateDecoder(ref IntPtr decoder, [MarshalAs(UnmanagedType.LPWStr)]string path,Int32 num_thread);
	#else
	[DllImport(LIBRARY_NAME, EntryPoint = "rm5CreateDecoderA", CharSet=CharSet.Ansi)]
	public static extern int rm5CreateDecoder(ref IntPtr decoder, [MarshalAs(UnmanagedType.LPStr)]string path,Int32 num_thread);
	#endif

/**
 *  RM5デコーダオブジェクトを作成／初期化します。(メモリ)
 *  引数:
 *    decoder    - デコーダオブジェクトポインタの格納先へのポインタ
 *    strm_buf   - RM5ムービーデータを格納しているメモリ領域へのポインタ
 *    strm_size  - strm_bufに格納されているRM5ムービーデータのバイトサイズ
 *    num_thread - RM5DEC_MULTITHREAD_AUTOもしくはデコードスレッド数
 *  返値:
 *    成功した場合、RM5DEC_STATUS_SUCCESSを返す。そうでなければ、それ以外のRM5DEC_STATUS_xxx定数を返す。
 *  解説:
 *    デコード対象はメモリ上のRM5ムービーデータ。
 */

	[DllImport(LIBRARY_NAME)]
	public static extern int rm5CreateMemDecoder(ref IntPtr decoder, byte[] buf,Int32 buf_size,Int32 num_thread);

/**
 *  RM5デコーダオブジェクトを作成／初期化します。(ユーザ定義ファイルアクセスコールバック)
 *  引数:
 *    decoder       - デコーダオブジェクトポインタの格納先へのポインタ
 *    fopen_args    - RM5DEC_USER_API_FOPENに通知される引数ポインタ
 *    callback      - ユーザ定義ファイルアクセスコールバック構造体
 *    version       - コールバック構造体のバージョン(RM5DEC_CALLBACK_VERSION)
 *    num_thread    - RM5DEC_MULTITHREAD_AUTOもしくはデコードスレッド数
 *  返値:
 *    成功した場合、RM5DEC_STATUS_SUCCESSを返す。そうでなければ、それ以外のRM5DEC_STATUS_xxx定数を返す。
 *  解説:
 *    デコード対象はRM5ファイル。ファイルはcallbackで定義したユーザ定義コールバック関数を使用して読み込みます。
 */

	[DllImport(LIBRARY_NAME)]
	public static extern int rm5CreateDecoderEx(ref IntPtr decoder, IntPtr fopen_args, rm5DecoderCallback callback, Int32 version, Int32 num_thread);

/**
 *  RM5デコーダオブジェクトを廃棄します。
 *  引数:
 *    decoder - デコーダオブジェクトポインタへのポインタ
 */

	[DllImport(LIBRARY_NAME)]
	public static extern void rm5DestroyDecoder(ref IntPtr decoder);

/**
 *  RM5ムービーのフレームをデコードします。
 *  引数:
 *    decoder     - デコーダオブジェクトポインタ
 *    index       - フレームインデックス
 *  返値:
 *    成功した場合、RM5DEC_STATUS_SUCCESSを返す。そうでなければ、それ以外のRM5DEC_STATUS_xxx定数を返す。
 *  解説：
 *    該当ムービを内部バッファにデコードします。
 *    この命令の後、以降の画像取得関数が有効になります。
 */

	[DllImport(LIBRARY_NAME)]
	public static extern int rm5Decode(IntPtr decoder, Int32 index);

/**
 *  RM5ムービーのデコードしたフレームの画像を取得します。
 *  引数:
 *    decoder     - デコーダオブジェクトポインタ
 *    image       - フレーム画像を取得するバッファへのポインタ
 *    buf_size    - imageのバッファバイト数
 *    image_type  - imageに格納するカラーフォーマット
 *    image_stride- imageバッファの水平ラインバイト数
 *  返値:
 *    成功した場合、RM5DEC_STATUS_SUCCESSを返す。そうでなければ、それ以外のRM5DEC_STATUS_xxx定数を返す。
 *  解説:
 *    image_typeを指定することで、様々な形式の画像データを取得することができる。
 *
 *    image_typeが取り得るカラーフォーマット値は以下の通り。
 *    <table>
 *    値                               説明
 *    ----------------                 ---------------
 *    RM5DEC_IMAGE_RGBQUAD             image にRGBQUAD形式（1ピクセルあたり4バイト）で格納される。
 *                                     どのSTREAM_TYPEでも指定可能である。
 *    RM5DEC_IMAGE_RGBQUAD_PREMULTA    image にRGBQUAD形式（1ピクセルあたり4バイト）で、RGB値にA値が乗算された形式で格納される。
 *                                     どのSTREAM_TYPEでも指定可能である。
 *    </table>
 */

	[DllImport(LIBRARY_NAME)]
	public static extern int rm5GetImage(IntPtr decoder, IntPtr image, Int32 buf_size, Int32 image_type, Int32 image_stride);

/**
 *  RM5ムービーのデコードしたフレームのアルファ画像を取得します。
 *  引数:
 *    decoder     - デコーダオブジェクトポインタ
 *    alpha       - アルファプレーンを取得するバッファへのポインタ
 *    buf_size    - alphaのバッファバイト数
 *    alpha_type  - alphaに格納するカラーフォーマット
 *    alpha_stride- alphaバッファの水平ラインバイト数
 *  返値:
 *    成功した場合、RM5DEC_STATUS_SUCCESSを返す。そうでなければ、それ以外のRM5DEC_STATUS_xxx定数を返す。
 *  解説:
 *    alpha_typeを指定することで、様々な形式の画像データを取得することができる。
 *    alpha_typeが取り得るカラーフォーマット値は以下の通り。
 *    <table>
 *    値                               説明
 *    ----------------                 ---------------
 *    RM5DEC_ALPHA                     alpha にアルファ形式（1ピクセルあたり1バイト）で格納される。
 *    </table>
 */

	[DllImport(LIBRARY_NAME)]
	public static extern int rm5GetAlpha(IntPtr decoder, byte[] alpha, Int32 buf_size, Int32 alpha_type, Int32 alpha_stride);

/**
 *  RM5ムービーの全体情報を取得します。
 *  引数:
 *    decoder - デコーダオブジェクトポインタ
 *    info    - ムービー情報を取得する rm5MovieInfo 構造体へのポインタ
 *    version - rm5MovieInfo構造体のバージョン(RM5DEC_MOVIE_INFO_VERSION)
 *  返値:
 *    成功した場合、RM5DEC_STATUS_SUCCESSを返す。そうでなければ、それ以外のRM5DEC_STATUS_xxx定数を返す。
 *  解説:
 *    versionに適切なバージョンがセットされていない場合、失敗する。
 */

	[DllImport(LIBRARY_NAME)]
	public static extern int rm5GetMovieInfo(IntPtr decoder, [In,Out] rm5MovieInfo info,Int32 version);

/**
 *  RM5ムービーのフレーム情報を取得します。
 *  引数:
 *    decoder - デコーダオブジェクトポインタ
 *    info    - フレーム情報を格納する rm5FrameInfo 構造体へのポインタ
 *    index   - フレームインデックス
 *    version - rm5FrameInfo構造体のバージョン(RM5DEC_FRAME_INFO_VERSION)
 *  返値:
 *    成功した場合、RM5DEC_STATUS_SUCCESSを返す。そうでなければ、それ以外のRM5DEC_STATUS_xxx定数を返す。
 */

	[DllImport(LIBRARY_NAME)]
	public static extern int rm5GetFrameInfo(IntPtr decoder,[In, Out] rm5FrameInfo info, Int32 index,Int32 version);

/**
 *  内部バッファを取得します。
 *  引数:
 *    decoder    - デコーダオブジェクトポインタ
 *    buffer     - 内部バッファ情報等を格納する構造体
 *  返値:
 *    成功した場合、RM5DEC_STATUS_SUCCESSを返す。そうでなければ、それ以外のRM5DEC_STATUS_xxx定数を返す。
 *  用途：
 *    色座標変換をスキップして高速化したい場合に使用する。
 *    通常は呼ぶ必要は無い。
 */

	[DllImport(LIBRARY_NAME)]
	public static extern int rm5GetInnerBuffer(IntPtr decoder, [In, Out] rm5InnerBuffer buffer);
}

