using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnwMoviePlayer
{
    private int ch;                 // 動画チャンネル番号
    private int loop;               // ループ再生
    private int loopFrame;          // ループした際に戻るフレーム数
    private int startFrame;         // 再生開始フレーム
    private RM5Movie m_movie;       // RM5 ムービーオブジェクト
    private double m_time;          // デコードフレームを決定するためのタイムカウンタ
    private Texture2D m_tex;        // RM5 ムービーオブジェクトが返したテクスチャ
    private int m_before_frame;     // 前回デコードしたフレーム番号
    private bool hold;              // 再生終了時に最終フレームをホールド

    enum RM5State
    {
        INIT,   //読込待ち
        READY,  //読込完了
        PLAYING,//再生中
        STOP,   //再生終了
        FAILED  //読込失敗
    };

    RM5State state = RM5State.INIT;

    public SnwMoviePlayer(int ch)
    {
        this.ch = ch;
        loop = 0;
        loopFrame = 0;
        startFrame = 0;
        m_movie = null;
        m_time = 0;
        m_tex = null;
        m_before_frame = -1;
        hold = false;
        state = RM5State.INIT;
    }

    ~SnwMoviePlayer()
    {
        Close();
    }

    public bool Open(string path)
    {
        // 動画の読込を開始
        return LoadMovie(path);
    }

    public void Start(int loop, int startFrame, bool hold)
    {
        this.startFrame = startFrame;
        this.loop = loop;
        this.hold = hold;

        //  再生を開始する
        if (state == RM5State.READY || state == RM5State.STOP)
        {
            state = RM5State.PLAYING;
            Decode();
        }
    }

    public void Exec()
    {
        // デコードを行う
        Decode();
    }

    public void Stop()
    {
        // 再生を停止する
        if (state == RM5State.PLAYING)
        {
            state = RM5State.STOP;
            m_time = 0.0f;
        }
    }

    public void Close()
    {
        if (m_movie != null)
        {
            //デコーダインスタンスを解放する
            m_movie.Dispose();
            m_movie = null;

            ////明示的にテクスチャを解放する
            //if (m_tex != null)
            //{
            //    Fill(m_tex);
            //    m_tex = null;
            //}
        }
        if (m_tex != null)
        {
            UnityEngine.Object.Destroy(m_tex);
            m_tex = null;
        }
        state = RM5State.INIT;
    }

    public int GetChannel()
    {
        return ch;
    }

    public void SetLoopPoint(int loopFrame)
    {
        this.loopFrame = loopFrame;
    }

    public bool IsPlaying()
    {
        // 動画再生中にtrueを返す
        return (state == RM5State.PLAYING);
    }

    public Texture2D GetTexture2D()
    {
        // 動画のフレームを取得する
        if (m_tex == null)
        {
            return null;
        }
        return m_tex;
    }

    public int GetFrame()
    {
        // 現在のフレームを返します
        if (m_movie == null)
        {
            return 0;
        }
        return m_before_frame < 0 ? 0 : m_before_frame;
    }

    public int GetTotalFrames()
    {
        // 動画の総フレーム数を取得する
        if (m_movie == null)
        {
            return 0;
        }
        return m_movie.GetTotalFrames();
    }

    private bool LoadMovie(string filePath)
    {
        // 読み込み済み
        if (state != RM5State.INIT)
        {
            return false;
        }

        if (filePath == "")
        {
            return false;
        }

        filePath = Application.streamingAssetsPath + "/" + filePath;

        return OpenMovie(filePath);
    }

    private bool OpenMovie(string filePath)
    {
        Close();
        m_movie = new RM5Movie();
        m_tex = m_movie.OpenFile(filePath); 

        // 動画ファイルを開くことに失敗した
        if (m_tex == null)
        {
            Debug.Log("RM5 file not found " + filePath);
            Close();
            state = RM5State.FAILED;
            return false;
        }

        // テクスチャをクリア
        Fill(m_tex);

        //読み込み完了
        state = RM5State.READY;
        m_before_frame = -1;
        return true;
    }

    // テクスチャを0フィルする
    private void Fill(Texture2D tex)
    {
        Color32 fillColor = new Color32(0, 0, 0, 0);
        Color32[] fillColorArray = tex.GetPixels32();
        for (int i = 0; i < fillColorArray.Length; i++)
        {
            fillColorArray[i] = fillColor;
        }
        tex.SetPixels32(fillColorArray);
        tex.Apply();
    }

    // デコードを行う
    private void Decode()
    {
        // デコード可能状態か判定
        if (m_movie == null)
        {
            return;
        }
        if (state != RM5State.PLAYING)
        {
            return;
        }

        // デコードフレームを決定する
        int offset = startFrame;

        int frame = (int)(m_time * m_movie.GetFrameRate()) + offset;
        m_time += Time.deltaTime;

        if (frame < 0)
        {
            return;
        }

        int frames = m_movie.GetTotalFrames() - offset;
        if (frame >= frames)
        {
            if (loop != 0)
            {
                if (loop > 0)
                {
                    loop--;
                }

                if (loopFrame > 0)
                {
                    startFrame = loopFrame;
                }
                // frame = (frame - offset) % frames + offset;
                m_time = 0.0f;
            }
            else
            {
                Stop();
                if (hold)
                {
                    frame = frames - 1;
                }
                else
                {
                    ClearTexture();
                    return;
                }
            }
        }

        // 前回と同じフレームはデコードを省略
        if (m_before_frame == frame)
        {
            return;
        }

        // デコードを行ってテクスチャを更新する
        m_movie.Decode(frame);
        m_movie.GetImage();
        m_before_frame = frame;
    }

    // テクスチャをクリアする
    private void ClearTexture()
    {
        if (m_tex != null)
        {
            Fill(m_tex);
            m_before_frame = -1;
        }
    }
}
