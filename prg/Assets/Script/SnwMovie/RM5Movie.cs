/* RAPIC5 Unity Plugin Movie Decode Class */
/* Copyright 2013-2018 AXELL CORPORATION */

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;   
using System.IO;
using System;

public class RM5Movie {
	private Texture2D tex;							/* テクスチャ */
	private byte[] m_stream;						/* RAPIC5のストリームデータ */
	private IntPtr m_decoder;						/* デコーダインスタンス */
	private RM5Decoder.rm5MovieInfo m_movie_info;	/* 動画情報 */

	private Color32[] image_color32;
	private GCHandle image_handle;
	private IntPtr rgbquad_image;

	public RM5Movie(){
		m_decoder=IntPtr.Zero;
		m_movie_info=new RM5Decoder.rm5MovieInfo();
		image_color32=null;
		rgbquad_image=IntPtr.Zero;
	}
	
	~RM5Movie(){
		Dispose();
	}

	/** 
	 *  ファイルから動画を読み込みます
	 *  引数：
	 *  　 code  - ストリームデータ
	 *  返値：
	 *   　成功時 - デコード結果の格納されるTexture2Dオブジェクト
	 *   　失敗時 - null
	 */

	public Texture2D OpenFile(string path){
		/* 既存のデコーダインスタンスを開放 */

		Dispose();
		
		/* メモリからデコーダインスタンスの作成 */

		int status=RM5Decoder.rm5CreateDecoder(ref m_decoder,path,RM5Decoder.RM5DEC_MULTITHREAD_AUTO);
		if(status!=RM5Decoder.RM5DEC_STATUS_SUCCESS){
			return null;
		}
		
		RM5Decoder.rm5GetMovieInfo(m_decoder,m_movie_info,RM5Decoder.RM5DEC_MOVIE_INFO_VERSION);

		/* テクスチャの確保 */

		AlocTexture();

		return tex;
	}

	/** 
	 *  バイト配列から動画を読み込みます
	 *  引数：
	 *  　 code  - ストリームデータ
	 *  返値：
	 *   　成功時 - デコード結果の格納されるTexture2Dオブジェクト
	 *   　失敗時 - null
	 */

	public Texture2D OpenMem(byte [] code){
		/* エラーチェック */
		if(code==null){
			return null;
		}

		/* 動画をリソースファイルから読み込み */

		m_stream = code;

		/* 既存のデコーダインスタンスを開放 */

		Dispose();
		
		/* メモリからデコーダインスタンスの作成 */

		int status=RM5Decoder.rm5CreateMemDecoder(ref m_decoder,m_stream,m_stream.Length,RM5Decoder.RM5DEC_MULTITHREAD_AUTO);
		if(status!=RM5Decoder.RM5DEC_STATUS_SUCCESS){
			return null;
		}
		
		RM5Decoder.rm5GetMovieInfo(m_decoder,m_movie_info,RM5Decoder.RM5DEC_MOVIE_INFO_VERSION);

		/* テクスチャの確保 */

		AlocTexture();

		return tex;
	}

	/** 
	 *  コールバックから動画を読み込みます
	 *  引数：
	 *  　 fopen_args  - コールバック引数
	 *    callback    - ファイルアクセスコールバック
	 *  返値：
	 *   　成功時 - デコード結果の格納されるTexture2Dオブジェクト
	 *   　失敗時 - null
	 */

	public Texture2D OpenEx(IntPtr fopen_args,RM5Decoder.rm5DecoderCallback callback){
		/* 既存のデコーダインスタンスを開放 */

		Dispose();
		
		/* コールバックからデコーダインスタンスの作成 */

		int status=RM5Decoder.rm5CreateDecoderEx(ref m_decoder,fopen_args,callback,RM5Decoder.RM5DEC_CALLBACK_VERSION,RM5Decoder.RM5DEC_MULTITHREAD_AUTO);
		if(status!=RM5Decoder.RM5DEC_STATUS_SUCCESS){
			return null;
		}
		
		RM5Decoder.rm5GetMovieInfo(m_decoder,m_movie_info,RM5Decoder.RM5DEC_MOVIE_INFO_VERSION);

		/* テクスチャの確保 */

		AlocTexture();

		return tex;
	}

	private void AlocTexture(){
		/* MipMapを作成すると速度が低下するため、MipMapを無効化 */

		tex = new Texture2D((int)m_movie_info.width,(int)m_movie_info.height,TextureFormat.ARGB32,false);

		/* デコード先のバッファを取得 */

		image_color32 = tex.GetPixels32();
		image_handle = GCHandle.Alloc(image_color32, GCHandleType.Pinned);
		rgbquad_image = image_handle.AddrOfPinnedObject();
	}

	/**
	 *  動画の1フレームをデコードします
	 *  引数：
	 *　   frame_no - デコードするフレーム番号
	 *  返値：
	 *   　成功時   - RM5DEC_STATUS_SUCCESS
	 *　   失敗時   - エラーコード
	 */

	public int Decode(int frame_no){
		/* フレームのデコード */

		int status=RM5Decoder.rm5Decode(m_decoder,frame_no);
		if(status!=RM5Decoder.RM5DEC_STATUS_SUCCESS){
			return status;
		}
		return RM5Decoder.RM5DEC_STATUS_SUCCESS;
	}

	/**
	 *  デコードした画像を取得します
	 *  引数:
	 *     なし
	 *  返値：
	 *   　成功時   - RM5DEC_STATUS_SUCCESS
	 *　   失敗時   - エラーコード
	 */
	public int GetImage(){
		/* バッファにデコード */

		int image_buf_size=(int)(m_movie_info.width*m_movie_info.height*4);
		int image_stride=(int)(m_movie_info.width*4);
		int status=RM5Decoder.rm5GetImage(m_decoder,rgbquad_image,image_buf_size,RM5Decoder.RM5DEC_IMAGE_RGBA,image_stride);


		if(status!=RM5Decoder.RM5DEC_STATUS_SUCCESS){
			return status;
		}

		/* デコードした画像をテクスチャに転送 */

		tex.SetPixels32( image_color32 );
		tex.Apply();

		return RM5Decoder.RM5DEC_STATUS_SUCCESS;
	}

	/**
	 *  デコードしたYUV画像を取得します
	 *  引数:
	 *     なし
	 *  返値：
	 *   　成功時   - RM5DEC_STATUS_SUCCESS
	 *　   失敗時   - エラーコード
	 */
	public int GetImageYuv(){
		/* 内部バッファの取得 */

		RM5Decoder.rm5InnerBuffer buffer=new RM5Decoder.rm5InnerBuffer();
		int status=RM5Decoder.rm5GetInnerBuffer(m_decoder,buffer);

		if(status!=RM5Decoder.RM5DEC_STATUS_SUCCESS){
			return status;
		}

		/* デコードした画像をテクスチャに転送 */

		byte[] alphaArray = new byte[buffer.alpha_stride*m_movie_info.height];
		if(buffer.alpha!=IntPtr.Zero){
			System.Runtime.InteropServices.Marshal.Copy(buffer.alpha, alphaArray, 0, (int)(buffer.alpha_stride*m_movie_info.height));
		}

		byte[] yArray = new byte[buffer.yuv_stride*m_movie_info.height*2];
		byte[] uArray = new byte[buffer.yuv_stride*m_movie_info.height*2];
		byte[] vArray = new byte[buffer.yuv_stride*m_movie_info.height*2];

		if(buffer.y!=IntPtr.Zero && buffer.u!=IntPtr.Zero && buffer.v!=IntPtr.Zero){
			System.Runtime.InteropServices.Marshal.Copy(buffer.y, yArray, 0, (int)(buffer.yuv_stride*m_movie_info.height*2));
			System.Runtime.InteropServices.Marshal.Copy(buffer.u, uArray, 0, (int)(buffer.yuv_stride*m_movie_info.height*2));
			System.Runtime.InteropServices.Marshal.Copy(buffer.v, vArray, 0, (int)(buffer.yuv_stride*m_movie_info.height*2));
		}
		
		for(int y=0;y<m_movie_info.height;y++){
			for(int x=0;x<m_movie_info.width;x++){
				uint adr_yuv=(uint)((buffer.yuv_stride*y)+x);
				uint adr_a=(uint)((buffer.alpha_stride*y)+x);

				byte yuv_y=(byte)(((yArray[adr_yuv*2+1]<<8)+yArray[adr_yuv*2+0])>>2);
				byte yuv_u=(byte)(((uArray[adr_yuv*2+1]<<8)+uArray[adr_yuv*2+0])>>2);
				byte yuv_v=(byte)(((vArray[adr_yuv*2+1]<<8)+vArray[adr_yuv*2+0])>>2);

				byte alpha=255;
				if(buffer.alpha!=IntPtr.Zero){
					alpha=(byte)(alphaArray[adr_a]);
				}

				image_color32[m_movie_info.width*y+x]=new Color32(yuv_y,yuv_u,yuv_v,alpha);
			}
		}

		tex.SetPixels32( image_color32 );
		tex.Apply();

		return RM5Decoder.RM5DEC_STATUS_SUCCESS;
	}

	/**
	 *  動画のフレーム数を取得します
	 *  返値：
	 *   　動画のフレーム数
	 */

	public int GetTotalFrames(){
		return (int)m_movie_info.total_frames;
	}

	/**
	 *  動画のフレームレートを取得します
	 *  返値：
	 *   　動画のフレームレート
	 */

	public float GetFrameRate(){
		if(m_movie_info.fps_denominator==0){
			return 1.0f;
		}
		return m_movie_info.fps_numerator/m_movie_info.fps_denominator;
	}

	/**
	 *  インスタンスを開放します
	 */

	public void Dispose(){
		if (image_handle.IsAllocated){
			image_handle.Free();
		}
		
		image_color32=null;
		rgbquad_image=IntPtr.Zero;

		if(m_decoder!=IntPtr.Zero){
			RM5Decoder.rm5DestroyDecoder(ref m_decoder);
			m_decoder=IntPtr.Zero;
		}
	}
}