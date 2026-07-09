using UnityEngine;
using UnityEngine.Video;

namespace UI.MultiMedia
{
    public enum MediaType
    {
        None,
        Image,
        Video
    }

    [System.Serializable]
    public class MediaPageData
    {
        [TextArea(3, 10)]
        public string description;

        public MediaType mediaType;
        public Sprite imageSprite;
        public VideoClip videoClip;
        public string videoUrl; // URL指定も可能にしておく
    }
}
