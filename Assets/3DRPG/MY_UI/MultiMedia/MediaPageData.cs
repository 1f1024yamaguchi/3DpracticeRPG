using UnityEngine;
using UnityEngine.Video;

namespace UI.MultiMedia
{
    /// <summary>
    /// ページに表示するメディアの種類。
    /// None = テキストのみ / Image = 静止画 / Video = 動画
    /// </summary>
    public enum MediaType
    {
        None,
        Image,
        Video
    }

    /// <summary>
    /// カルーセルの「1ページ分」のデータ。
    /// AutoMenuGenerator の mediaPages や MultiMediaGenerator の pages に
    /// リストとして設定し、MultiMediaPresenter が表示します。
    /// mediaType に応じて imageSprite / videoClip / videoUrl のいずれかを使用します。
    /// </summary>
    [System.Serializable]
    public class MediaPageData
    {
        [TextArea(3, 10)]
        public string description;   // ページの説明文

        public MediaType mediaType;  // 表示するメディアの種類
        public Sprite imageSprite;   // mediaType=Image の時に表示する画像
        public VideoClip videoClip;  // mediaType=Video の時に再生する動画クリップ
        public string videoUrl;      // URL指定も可能にしておく（videoClipが未設定の時に使用）
    }
}
