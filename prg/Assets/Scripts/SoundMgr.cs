using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SoundMgr
{
    public static SoundMgr instance;
    private List<AudioSource> audioSources;
    private ResourceData resourceData;
    private AudioClip[] audioClip;
    private Dictionary<SOUND_CH, CallbackInfo[]> soundCallbacks;

    public static SoundMgr GetInstance()
    {
        if (instance == null)
        {
            instance = new SoundMgr();
        }
        return instance;
    }
    public SoundMgr()
    {
        // BGM用AudioSourceの生成
        audioSources = new List<AudioSource>();
        GameObject audioObject = new GameObject("AudioSourceObject_BGM");
        AudioSource bgm = audioObject.AddComponent<AudioSource>();
        audioSources.Add(bgm);

        //リソースデータを取得
        resourceData = ResourceData.GetInstance();
        audioClip = new AudioClip[(int)SOUND.MAX];

        for (SOUND i = SOUND.NONE + 1;i<SOUND.MAX;i++)
        {
            audioClip[(int)i] = Resources.Load<AudioClip>(resourceData.GetFilePath(DATA_TYPE.SOUND, (int)i));
        }
    }

    public void PlayAudio(
        SOUND_TYPE type,
        SOUND flsno,
        float volume,
        params CallbackInfo[] callbacks)
    {
        if (!NullCheck()) return;

        AudioClip crip = audioClip[(int)flsno];
        if (type == SOUND_TYPE.LOOP)
        {
            //リクエストされたBGMが既に鳴っていたら鳴らさない
            if (audioSources[0].clip == null || audioSources[0].clip.name != crip.name)
            {
                audioSources[0].clip = crip;
                audioSources[0].loop = type == SOUND_TYPE.LOOP;
                audioSources[0].volume = volume;
                audioSources[0].Play();
            }
        }
        else
        {
            int ch = GetCh();

            audioSources[ch].clip = crip;
            audioSources[ch].loop = type == SOUND_TYPE.LOOP;
            audioSources[ch].volume = volume;
            audioSources[ch].Play();

        }
    }

    private IEnumerator WaitSound(AudioSource source, object obj)
    {
        if (!NullCheck()) yield break;

        while (source.isPlaying)
        {
            yield return null; // 次のフレームまで待機
        }
    }
    public void StopAudioAll()
    {
        if (!NullCheck()) return;

        for (int i = 0; i < audioSources.Count; i++)
        {
            if (audioSources[i].isPlaying == true) audioSources[i].Stop();
        }
    }
    public void StopAudio(int ch)
    {
        if (!NullCheck()) return;
        if (audioSources[ch].isPlaying == true) audioSources[ch].Stop();
    }

    public int GetCh()
    {
        for (int i = 1; i < audioSources.Count; i++)
        {
            if (!audioSources[i].isPlaying)
            {
                ClearAudioSources(i + 1);
                return i;
            }
        }
        // 空きがない場合は新しく追加
        GameObject audioObject = new GameObject("AudioSourceObject_SE" + audioSources.Count);
        AudioSource newSource = audioObject.AddComponent<AudioSource>();
        audioSources.Add(newSource);

        return audioSources.Count - 1;
    }

    private bool NullCheck()
    {
        if (resourceData == null || audioSources == null) return false;
        else return true;
    }
    private void ClearAudioSources(int startIndex)
    {
        for (int i = audioSources.Count - 1; i >= startIndex; i--)
        {
            if (!audioSources[i].isPlaying)
            {
                GameObject.Destroy(audioSources[i].gameObject);
                audioSources.RemoveAt(i);
            }
        }
    }
    public void Dispose()
    {

    }
}
