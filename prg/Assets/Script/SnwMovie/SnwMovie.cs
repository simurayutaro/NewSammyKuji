using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnwMovie
{
    private List<SnwMoviePlayer> moviePlayers; // 動画プレイヤーのリスト

    public SnwMovie()
    {
        moviePlayers = new List<SnwMoviePlayer>();
    }

    ~SnwMovie()
    {
        Dispose();
    }

    public void snwMovieExit()
    {
        Dispose();
    }

    //  Assets/StreamingASsets/[path]から動画を読み込みます
    public void snwMovieOpen(int ch, string path)
    {
        snwMovieTermIdx(ch); // 既存のプレイヤーを終了

        SnwMoviePlayer player = new SnwMoviePlayer(ch);
        if (player.Open(path))
        {
            moviePlayers.Add(player);
        }
        else
        {
            Debug.LogError("Failed to open movie file: " + path);
        }
    }

    // chのチャンネルの動画を終了します
    public void snwMovieTermIdx(int ch)
    {
        //foreach (SnwMoviePlayer player in moviePlayers)
        moviePlayers.RemoveAll(player =>
        {
            if (player.GetChannel() == ch)
            {
                player.Close();
                //moviePlayers.Remove(player);
                //break;
                return true;
            }
            return false;
        //}
        });
    }

    // chのチャンネルの動画を再生します
    // loop: 0: 1回, N: N回繰り返し, -1: 繰り返し
    // hold: true: 再生終了時に最終フレームをホールドする
    public void snwMovieStart(int ch, int loop, bool hold = false)
    {
        foreach (SnwMoviePlayer player in moviePlayers)
        {
            if (player.GetChannel() == ch)
            {
                player.Start(loop, 0, hold);
                break;
            }
        }
    }

    // chのチャンネルの動画を再生します
    // loop: 0: 1回, N: N回繰り返し, -1: 繰り返し
    // hold: true: 再生終了時に最終フレームをホールドする
    // startFrame: 再生開始フレーム
    public void snwMovieStartPos(int ch, int loop, int startFrame, bool hold = false)
    {
        foreach (SnwMoviePlayer player in moviePlayers)
        {
            if (player.GetChannel() == ch)
            {
                player.Start(loop, startFrame, hold);
                break;
            }
        }
    }

    // ループポイントを設定します
    public void snwMovieSetLoopPoint(int ch, int loopFrame)
    {
        foreach (SnwMoviePlayer player in moviePlayers)
        {
            if (player.GetChannel() == ch)
            {
                player.SetLoopPoint(loopFrame);
                break;
            }
        }
    }

    // 再生中かどうかを返します
    // 1: 再生中, 0: 停止中
    public int snwMovieIsPlaying(int ch)
    {
        foreach (SnwMoviePlayer player in moviePlayers)
        {
            if (player.GetChannel() == ch)
            {
                return player.IsPlaying() ? 1 : 0;
            }
        }
        return 0;
    }

    // 1フレーム進めます
    public void snwMovieExecIdx(int ch)
    {
        foreach (SnwMoviePlayer player in moviePlayers)
        {
            if (player.GetChannel() == ch)
            {
                player.Exec();
                break;
            }
        }
    }

    // RM5 ムービーオブジェクトが返したテクスチャを返します
    public Texture2D snwMovieGetTexture(int ch)
    {
        foreach (SnwMoviePlayer player in moviePlayers)
        {
            if (player.GetChannel() == ch)
            {
                return player.GetTexture2D();
            }
        }
        return null;
    }

    // 現在のフレームを返します
    public int snwMovieGetFrame(int ch)
    {
        foreach (SnwMoviePlayer player in moviePlayers)
        {
            if (player.GetChannel() == ch)
            {
                return player.GetFrame();
            }
        }
        return 0;
    }

    // 動画の総フレーム数を取得します
    public int snwMovieGetTotalFrames(int ch)
    {
        foreach (SnwMoviePlayer player in moviePlayers)
        {
            if (player.GetChannel() == ch)
            {
                return player.GetTotalFrames();
            }
        }
        return 0;
    }

    private void Dispose()
    {
        foreach (SnwMoviePlayer player in moviePlayers)
        {
            player.Close();
        }
        moviePlayers.Clear();
    }
}
