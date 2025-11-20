using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
public class VideoMgr
{
    public static VideoMgr instance;
    private SnwMovie snwMovie;
    private ResourceData resourceData;
    private Canvas canvas;
    private VideoInfo[] videoInfo;
    private bool initFlg = false;
    private Dictionary<VIDEO_CH, CallbackInfo[]> videoCallbacks;

    public static VideoMgr GetInstance()
    {
        if (instance == null)
        {
            instance = new VideoMgr();
        }
        return instance;
    }
    public VideoMgr()
    {
        snwMovie = new SnwMovie();
        videoCallbacks = new Dictionary<VIDEO_CH, CallbackInfo[]>();
        resourceData = ResourceData.GetInstance();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        videoInfo = new VideoInfo[(int)VIDEO_CH.CH_MAX];

        initFlg = true;
    }
    public bool CreateVideo(VIDEO_CH ch, float x, float y, float width,float height)
    {
        string objectName = "RawImageObject_" + ch.ToString();

        Transform existing = canvas.transform.Find(objectName);
        if (existing != null && existing.GetComponent<RawImage>() != null)
        {
            return false;
        }

        //CanvasにRawImageの追加
        GameObject rawImageObject = new GameObject(objectName);
        RawImage rawImage = rawImageObject.AddComponent<RawImage>();

        // Canvasの子オブジェクトとして設定
        rawImageObject.transform.SetParent(canvas.transform, false);

        //RawImageを非アクティブ化
        rawImage.enabled = false;

        // RawImageの位置とサイズを設定
        RectTransform rectTransform = rawImage.rectTransform;
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0f, 0f);
        rectTransform.anchoredPosition = new Vector2(x, y);
        rectTransform.sizeDelta = new Vector2(width, height);

        return true;
    }
    public void Update()
    {
        // 各チャンネルの動画を更新
        for (int i = 0; i < (int)VIDEO_CH.CH_MAX; i++)
        {
            snwMovie.snwMovieExecIdx(i);
        }

        // コピーしてループ（元の Dictionary を安全に変更するため）
        var keys = videoCallbacks.Keys.ToArray();

        foreach (var ch in keys)
        {
            if (!videoCallbacks.TryGetValue(ch, out var callbacks))
                continue;

            // 停止していたら MOVIE_END を実行
            if (snwMovie.snwMovieIsPlaying((int)ch) == 0)
            {
                // コールバック内で再登録された情報を壊さないようにする
                videoCallbacks.Remove(ch);

                foreach (var cb in callbacks)
                {
                    if (cb.Type == CALLBACK_TYPE.MOVIE_END)
                    {
                        cb.Callback?.Invoke(videoInfo[(int)ch]);
                    }
                }

                continue;
            }

            // フレーム到達系コールバック
            int currentFrame = snwMovie.snwMovieGetFrame((int)ch);
            foreach (var cb in callbacks)
            {
                if (cb.Type == CALLBACK_TYPE.MOVIE_FRAME_REACHED &&
                    cb.Frame.HasValue &&
                    currentFrame == cb.Frame.Value)
                {
                    cb.Callback?.Invoke(videoInfo[(int)ch]);
                    cb.Frame = null; // 一度だけ実行
                }
            }
        }
    }

    public void PlayVideo(
        int index,
        int loop,
        VIDEO_CH ch,
        params CallbackInfo[] callbacks)
    {

        if (ch >= VIDEO_CH.CH_MAX || ch < VIDEO_CH.CH_1) return;
        if (index >= (int)VIDEO.MAX || index <= (int)VIDEO.NONE) return;

        // ムービーオープン
        snwMovie.snwMovieOpen((int)ch, resourceData.GetFilePath(DATA_TYPE.MOVIE, index));
        RawImage rawImage = GameObject.Find("RawImageObject_" + ch.ToString()).GetComponent<RawImage>();
        rawImage.texture = snwMovie.snwMovieGetTexture((int)ch);
        rawImage.rectTransform.localScale = new Vector3(1, -1, 1); // Y軸反転
        rawImage.enabled = true;

        // ムービー再生  
        snwMovie.snwMovieStart((int)ch, loop, false);

        // ムービー情報保存
        videoInfo[(int)ch].ch = ch;
        videoInfo[(int)ch].index = (VIDEO)index;

        // コールバックの設定
        if (callbacks != null)
        {
            videoCallbacks[ch] = callbacks;
        }
    }
 
    public bool StopVideo(VIDEO_CH ch)
    {
        if (ch < VIDEO_CH.CH_1 || ch >= VIDEO_CH.CH_MAX) return false;

        string objectName = "RawImageObject_" + ch.ToString();

        Transform existing = canvas.transform.Find(objectName);
        if (existing != null && existing.GetComponent<RawImage>() != null)
        {
            snwMovie.snwMovieTermIdx((int)ch);
            RawImage rawImage = GameObject.Find(objectName).GetComponent<RawImage>();
            rawImage.enabled = false;
            return true;
        }
        return false;
    }
 
    public void StopVideoAll()
    {
        for (VIDEO_CH ch = VIDEO_CH.CH_1; ch < VIDEO_CH.CH_MAX; ch++)
        {
            string objectName = "RawImageObject_" + ch.ToString();

            Transform existing = canvas.transform.Find(objectName);
            if (existing != null && existing.GetComponent<RawImage>() != null)
            {
                snwMovie.snwMovieTermIdx((int)ch);
                RawImage rawImage = GameObject.Find(objectName).GetComponent<RawImage>();
                rawImage.enabled = false;
            }
        }
    }
    public int IsPlayVideo(int ch)
    {
        return snwMovie.snwMovieIsPlaying(ch);
    }

    public int GetVideoFrame(int ch)
    {
        return snwMovie.snwMovieGetFrame(ch);
    }
 
    public bool GetFlg()
    {
        return initFlg;
    }
    public void Dispose()
    {
        if (snwMovie != null)
        {
            snwMovie.snwMovieExit();
            snwMovie = null;
        }
    }
}