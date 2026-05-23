using System;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UI.MultiMedia
{
    /// <summary>
    /// メディアの表示（Text, Image, Video）を管理するコンポーネント。
    /// 指定されたMediaPageDataに基づいてUIを更新する。
    /// </summary>
    public class MultiMediaPresenter : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image displayImage;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RawImage videoRenderTexture; // 動画表示用のRawImage

        [Header("Settings")]
        // [SerializeField] private bool clearOnNone = true;

        private string currentMediaId; // 現在のロード対象を記録

        public void DisplayPage(string itemName, MediaPageData data)
        {
            if (data == null)
            {
                ClearAll();
                return;
            }

            // ItemNameの更新
            if (itemNameText != null)
            {
                itemNameText.text = itemName;
            }

            // Descriptionの更新
            if (descriptionText != null)
            {
                descriptionText.text = data.description;
            }

            // メディアの表示切り替え
            UpdateMediaDisplay(data);
        }

        private void UpdateMediaDisplay(MediaPageData data)
        {

            // いったんすべて非表示/停止
            if (displayImage != null) displayImage.gameObject.SetActive(false);
            
            // 動画がロードされるまではRawImageを非表示にして残像を隠す
            if (videoRenderTexture != null) videoRenderTexture.gameObject.SetActive(false);
            
            if (videoPlayer != null) 
            {
                videoPlayer.Stop();
                // イベントの多重登録を防ぐために一旦解除
                videoPlayer.prepareCompleted -= OnVideoPrepared;
            }

            switch (data.mediaType)
            {
                case MediaType.Image:
                    if (displayImage != null && data.imageSprite != null)
                    {
                        displayImage.sprite = data.imageSprite;
                        displayImage.gameObject.SetActive(true);
                    }
                    break;

                case MediaType.Video:
                    if (videoPlayer != null && videoRenderTexture != null)
                    {
                        if (data.videoClip != null)
                        {
                            videoPlayer.source = VideoSource.VideoClip;
                            videoPlayer.clip = data.videoClip;
                            currentMediaId = data.videoClip.name; // 現在のロード対象を記録
                        }
                        else if (!string.IsNullOrEmpty(data.videoUrl))
                        {
                            videoPlayer.source = VideoSource.Url;
                            videoPlayer.url = data.videoUrl;
                            currentMediaId = data.videoUrl; // 現在のロード対象を記録
                        }
                        else
                        {
                            break;
                        }

                        // Play()の代わりにPrepare()を呼び、完了イベントを登録する
                        videoPlayer.prepareCompleted += OnVideoPrepared;
                        videoPlayer.Prepare();
                    }
                    break;

                case MediaType.None:
                default:
                    // 何もしない（既に非表示）
                    break;
            }
        }

        /// <summary>
        /// ロード準備完了時に呼ばれるコールバック
        /// </summary>
        private void OnVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnVideoPrepared; // イベント解除

            // 再生準備が完了したら、RawImageを表示して再生開始
            if (videoRenderTexture != null)
            {
                videoRenderTexture.gameObject.SetActive(true);
                Color color = videoRenderTexture.color;
                color.a = 1f;
                videoRenderTexture.color = color;
            }
            vp.Play();
        }

        public void ClearAll()
        {

            if (itemNameText != null) itemNameText.text = string.Empty;
            if (descriptionText != null) descriptionText.text = string.Empty;
            if (displayImage != null) displayImage.gameObject.SetActive(false);
            if (videoRenderTexture != null) videoRenderTexture.gameObject.SetActive(false);
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                videoPlayer.prepareCompleted -= OnVideoPrepared;
            }
        }
    }
}
