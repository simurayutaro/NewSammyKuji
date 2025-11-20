using UnityEngine;
using Spine;
using Spine.Unity;
using System.IO;
using System;

public class AnimMgr
{
    public static AnimMgr instance;
    private SkeletonAnimation uiAnimation;
    private Spine.AnimationState uiState;
    private SoundMgr soundMgr;
    private VideoMgr videoMgr;
    private bool initFlg = false;

    public void Initialize(SkeletonAnimation skl)
    {
        uiAnimation = skl;
        uiState = skl.state;
    }
    /// <summary>
    /// slotName の Slot に対して、attachmentName を貼る／外す
    /// </summary>
    public void SetAnim(string slotName, string attachmentName, bool flg)
    {
        if (uiAnimation == null) return;
        var slot = uiAnimation.skeleton.FindSlot(slotName);
        if (slot == null) return;
        slot.Attachment = flg
            ? uiAnimation.skeleton.GetAttachment(slotName, attachmentName)
            : null;
    }

    /// <summary>
    /// slotName1〜slotName5 の各 Slot に、数字 num の桁ごとの Attachment を貼る／外す
    /// </summary>
    public void SetAnim(string slotName, string attachmentName, bool flg, int num)
    {
        if (uiAnimation == null) return;

        // 各桁を配列に分解
        int[] digits = new int[5];
        digits[4] = num / 10000;
        digits[3] = (num % 10000) / 1000;
        digits[2] = (num % 1000) / 100;
        digits[1] = (num % 100) / 10;
        digits[0] = num % 10;

        // １〜５番目の slot を順に更新
        for (int i = 0; i < 5; i++)
        {
            string sName = slotName + (i + 1);
            string aName = attachmentName + digits[i];
            var slot = uiAnimation.skeleton.FindSlot(sName);
            if (slot == null) continue;

            if (!flg)
            {
                slot.Attachment = null;
            }
            else
            {
                // 先頭桁は必ず表示、後続桁は前の桁が非ゼロなら表示
                bool show = (digits[i] > 0)
                            || (i == 0)
                            || (i + 1 < 5 && digits[i + 1] > 0 && digits[i] == 0);
                slot.Attachment = show
                    ? uiAnimation.skeleton.GetAttachment(sName, aName)
                    : null;
            }
        }
    }

    public class SpineEventData
    {
        public Spine.TrackEntry Entry { get; set; }
        public Spine.Event Event { get; set; }
    }

    public static AnimMgr GetInstance()
    {
        if (instance == null)
        {
            instance = new AnimMgr();
        }
        return instance;
    }
    public AnimMgr()
    {
        soundMgr = SoundMgr.GetInstance();
        videoMgr = VideoMgr.GetInstance();
        initFlg = true;
    }

    public void StopAnim(SkeletonAnimation skl, int track)
    {
        if (skl == null) return;
        skl.AnimationState.ClearTrack(track);
    }
    public void StopAnimAll(SkeletonAnimation skl)
    {
        if (skl == null) return;
        skl.AnimationState.ClearTracks();
    }
    public void SetAnim(SkeletonAnimation skl, ANIM_TABLE animTbl)
    {
        if (skl == null || soundMgr == null) return;

        //アニメーション再生
        if (animTbl.animName != "")
        {
            skl.state.SetAnimation((int)animTbl.trackIndex, animTbl.animName, animTbl.loopFlg);
        }
        //サウンド再生
        if (animTbl.soundIndex != SOUND.NONE)
        {
            soundMgr.PlayAudio(animTbl.type, animTbl.soundIndex, animTbl.volume, null);
        }
        //ビデオ再生
        if (animTbl.videoIndex != VIDEO.NONE)
        {
            videoMgr.PlayVideo((int)animTbl.videoIndex, animTbl.videoLoop, animTbl.videoCH);
        }
    }

    //アニメーション終了のコールバックを受けたい場合にこっちを呼ぶ
    public void SetAnim(
        SkeletonAnimation skl,
        ANIM_TABLE animTbl,
        params CallbackInfo[] callbacks)
    {
        SetAnim(skl, animTbl, 1.0f, 0.0f, callbacks);
    }

    //アニメーション終了のコールバックを受けたい場合にこっちを呼ぶ（スピードとスタート位置も指定）
    public void SetAnim(
        SkeletonAnimation skl,
        ANIM_TABLE animTbl,
        float speed,
        float startTime,
        params CallbackInfo[] callbacks)
    {
        TrackEntry trackEntry;

        if (skl == null || soundMgr == null) return;

        //アニメーション再生
        if (animTbl.animName != "")
        {
            trackEntry = skl.state.SetAnimation((int)animTbl.trackIndex, animTbl.animName, animTbl.loopFlg);
            trackEntry.TimeScale = speed;
            trackEntry.TrackTime = startTime;

            //Spineコールバックの登録
            foreach (var cb in callbacks)
            {
                switch (cb.Type)
                {
                    case CALLBACK_TYPE.ANIM_END:
                        trackEntry.Complete += (entry) =>
                        {
                            cb.Callback?.Invoke(entry);
                        };
                        break;

                    case CALLBACK_TYPE.EVENT:
                        trackEntry.Event += (entry, e) =>
                        {
                            cb.Callback?.Invoke(new SpineEventData { Entry = entry, Event = e });
                        };
                        break;
                }
            }
        }
        //サウンド再生
        if (animTbl.soundIndex != SOUND.NONE)
        {
            soundMgr.PlayAudio(animTbl.type, animTbl.soundIndex, animTbl.volume, callbacks);
        }

        //ビデオ再生
        if (animTbl.videoIndex != VIDEO.NONE)
        {
            videoMgr.PlayVideo((int)animTbl.videoIndex, animTbl.videoLoop, animTbl.videoCH, callbacks);
        }
    }


    public void SetBonePos(SkeletonAnimation skl, string boneName, Vector2 pos)
    {
        if (skl == null) return;

        Bone bone = skl.skeleton.FindBone(boneName);
        if (bone != null)
        {
            bone.X = pos.x;
            bone.Y = pos.y;
        }
    }

    public TrackEntry GetTrackEntry(SkeletonAnimation skl, int track)
    {
        if (skl == null) return null;

        TrackEntry trackEntry = skl.state.GetCurrent(track);

        if (trackEntry != null)
        {
            return trackEntry;
        }
        return null;
    }

    public void PauseAnim(SkeletonAnimation skl, int track)
    {
        if (skl == null) return;

        TrackEntry trackEntry = skl.state.GetCurrent(track);

        if (trackEntry != null)
        {
            trackEntry.TimeScale = 0.0f;
        }
    }

    public bool IsPlayingAnim(SkeletonAnimation skl, int track)
    {
        if (skl == null) return false;

        TrackEntry trackEntry = skl.state.GetCurrent(track);

        if (trackEntry != null)
        {
            return !trackEntry.IsComplete;
        }
        return false;
    }

    public float GetSpeedAnim(SkeletonAnimation skl, int track)
    {
        if (skl == null) return 0.0f;

        TrackEntry trackEntry = skl.state.GetCurrent(track);

        if (trackEntry != null)
        {
            return trackEntry.TimeScale;
        }

        return 0.0f;
    }
    public void SetSpeedAnim(SkeletonAnimation skl, int track, float speed)
    {
        if (skl == null) return;

        TrackEntry trackEntry = skl.state.GetCurrent(track);

        if (trackEntry != null)
        {
            trackEntry.TimeScale = speed;
        }
    }
    public string GetAnim(SkeletonAnimation skl, int track)
    {
        if (skl == null) return "";

        TrackEntry trackEntry = skl.state.GetCurrent(track);

        if (trackEntry != null)
        {
            // トラックで再生されているアニメーションの名前を取得
            return trackEntry.Animation.Name;
        }
        return "None";
    }

    public float GetAnimTime(SkeletonAnimation skl, int track)
    {
        if (skl == null) return 0.0f;
        TrackEntry trackEntry = skl.state.GetCurrent(track);
        if (trackEntry != null)
        {
            return trackEntry.TrackTime;
        }
        return 0.0f;
    }

    public void StopAudio(int ch)
    {
        if (soundMgr == null) return;
        soundMgr.StopAudio(ch);
    }
    public void StopAudioAll()
    {
        if (soundMgr == null) return;
        soundMgr.StopAudioAll();
    }
    public void StopVideo(VIDEO_CH ch)
    {
        if (videoMgr != null)
            videoMgr.StopVideo(ch);
    }
    /// <summary>
    /// 動画をすべて停止する
    /// </summary>
    public void StopVideoAll()
    {
        if (videoMgr != null)
            videoMgr.StopVideoAll();
    }
    public void SetPngToAttachment(
        SkeletonAnimation skl,
        string path,
        string slotName,
        bool isResionCopy = false,
        string originalAttachmentName = "",
        bool isSpritesShader = true)
    {
        if (skl == null || soundMgr == null) return;

        //スロット名からスロットを取得
        Slot slot = skl.Skeleton.FindSlot(slotName);

        if (slot != null)
        {
            //ファイル名をアタッチメント名として使用
            string attachmentName = Path.GetFileNameWithoutExtension(path);

            //既にアタッチメントとして登録されていたら終了
            // if (slot.Attachment != null && slot.Attachment.Name == attachmentName)
            // {
            //     return;
            // }

            //pngデータからTexture2Dを作成
            byte[] imageData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            texture.Apply();

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            //ファイル名をアタッチメント名として使用
            texture.name = attachmentName;

            // Textureを元にMaterialを作成 Spine/Skeletonシェーダーだと白い枠線が出てしまうので、このシェーダーを利用
            Material material;
            if (isSpritesShader)
            {
                material = new Material(Shader.Find("Sprites/Default"));
            }
            else
            {
                material = new Material(Shader.Find("Spine/Skeleton"));
                material.SetInt("_StraightAlphaInput", 1);
                material.EnableKeyword("_STRAIGHT_ALPHA_INPUT");
            }
            material.mainTexture = texture;

            // Spine用のAtlasRegionを作成
            AtlasRegion region = CreateAtlasRegion(texture, material);

            // 新しいRegionAttachmentを作成
            RegionAttachment attachment = CreateRegionAttachment(region, texture);

            // 属性をコピー
            if (isResionCopy)
            {
                CopyOriginalRegion(skl, attachment, slotName, originalAttachmentName);
            }

            // Skeletonのスロットに適用
            slot.Attachment = attachment;

            Skeleton.Physics physics = new Skeleton.Physics();
            skl.Skeleton.UpdateWorldTransform(physics);
        }
    }
    private AtlasRegion CreateAtlasRegion(Texture2D texture, Material material)
    {
        AtlasRegion region = new AtlasRegion();
        region.name = texture.name;

        AtlasPage page = new AtlasPage();
        page.rendererObject = material;
        page.width = texture.width;
        page.height = texture.height;
        region.page = page;

        // UV設定
        float uvShrink = 0f;//0.01f;

        region.u = 0f + uvShrink;
        region.v = 1f - uvShrink;
        region.u2 = 1f - uvShrink;
        region.v2 = 0f + uvShrink;

        // サイズ関連
        region.width = texture.width;
        region.height = texture.height;
        region.originalWidth = texture.width;
        region.originalHeight = texture.height;
        region.offsetX = 0f;
        region.offsetY = 0f;
        region.packedWidth = texture.width;
        region.packedHeight = texture.height;
        region.degrees = 0; // 回転なし

        return region;
    }
    private RegionAttachment CreateRegionAttachment(AtlasRegion region, Texture2D texture)
    {
        RegionAttachment attachment = new RegionAttachment(texture.name);

        attachment.Region = region;

        attachment.Width = region.originalWidth;
        attachment.Height = region.originalHeight;
        attachment.X = 0f;
        attachment.Y = 0f;
        attachment.ScaleX = 1f;
        attachment.ScaleY = 1f;
        attachment.Rotation = 0f;

        // UV・Offsetの計算
        attachment.UpdateRegion();

        return attachment;
    }

    private void CopyOriginalRegion(SkeletonAnimation skl, RegionAttachment attachment, string slotName, string originalAttachmentName)
    {
        Attachment originalAttachment = skl.Skeleton.GetAttachment(slotName, originalAttachmentName);
        if (originalAttachment is RegionAttachment originalRegion && attachment is RegionAttachment newRegion)
        {
            newRegion.X = originalRegion.X;
            newRegion.Y = originalRegion.Y;
            newRegion.ScaleX = originalRegion.ScaleX;
            newRegion.ScaleY = originalRegion.ScaleY;
            newRegion.Rotation = originalRegion.Rotation;
            newRegion.Width = originalRegion.Width;
            newRegion.Height = originalRegion.Height;

            newRegion.A = originalRegion.A;
            newRegion.R = originalRegion.R;
            newRegion.G = originalRegion.G;
            newRegion.B = originalRegion.B;

            newRegion.UpdateRegion();
        }
    }

    public void AttachmentToggle(SkeletonAnimation skl, string slotName, string attachmentName, bool tgl)
    {
        if (skl == null) return;

        Slot slot = skl.skeleton.FindSlot(slotName);
        if (slot != null)
        {
            if (tgl)
            {
                slot.Attachment = skl.skeleton.GetAttachment(slotName, attachmentName);
            }
            else
            {
                slot.Attachment = null;
            }
        }
    }
    public void AttachmentToggleToTarget(SkeletonAnimation skl, string slotName, string attachmentName, bool tgl, string targetSlotName = null)
    {
        if (skl == null) return;

        Slot slot = skl.skeleton.FindSlot(slotName);
        if (slot != null)
        {
            if (tgl)
            {
                var target = targetSlotName != null ? targetSlotName : slotName;
                slot.Attachment = skl.skeleton.GetAttachment(target, attachmentName);
            }
            else
            {
                slot.Attachment = null;
            }
        }
    }
    public void ClearAttachmentAll(SkeletonAnimation skl)
    {
        if (skl == null) return;

        StopAnimAll(skl);

        foreach (var slot in skl.skeleton.Slots)
        {
            slot.Attachment = null;
        }
    }
    public Slot GetSlot(SkeletonAnimation skl, string slotName)
    {
        if (skl == null) return null;

        return skl.skeleton.FindSlot(slotName);
    }
    public Attachment GetAttachment(SkeletonAnimation skl, string slotName, string attachmentName)
    {
        if (skl == null) return null;

        Slot slot = GetSlot(skl, slotName);
        if (slot != null)
        {
            return skl.skeleton.GetAttachment(slotName, attachmentName);
        }

        return null;
    }
    public void UpdateVideo()
    {
        videoMgr.Update();
    }
    public bool GetInitFlg()
    {
        return initFlg;
    }
    public bool CreateVideo(VIDEO_CH ch, float x, float y, float width, float height)
    {
        return videoMgr.CreateVideo(ch, x, y, width, height);
    }
    public void Dispose()
    {
        if (videoMgr != null)
        {
            videoMgr.Dispose();
            videoMgr = null;
        }
        if (soundMgr != null)
        {
            soundMgr.Dispose();
            soundMgr = null;
        }
    }
}
