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
    ///
    /// 呼び出し元は CarouselMenuController / MultiMediaContent で、
    /// このクラス自体は入力を一切扱わず「表示専門」です（Presenterパターン）。
    /// 動画は Prepare() → prepareCompleted → Play() の順で再生することで、
    /// ロード中に前の映像（残像）が見えるのを防ぎ、フェードインさせています。
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
        [SerializeField] private float videoFadeDuration = 0.5f;

        private string currentMediaId; // 現在のロード対象を記録
        private Coroutine videoFadeCoroutine;

        /// <summary>
        /// 1ページ分のデータを画面に表示します。
        /// itemName はタイトル欄、data.description は説明欄に反映され、
        /// mediaType に応じて画像または動画の表示に切り替わります。
        /// </summary>
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

        /// <summary>
        /// メディア表示の切り替え本体。
        /// いったん画像・動画を両方非表示/停止してから、必要なものだけ表示します。
        /// </summary>
        private void UpdateMediaDisplay(MediaPageData data)
        {
            if (videoFadeCoroutine != null)
            {
                StopCoroutine(videoFadeCoroutine);
                videoFadeCoroutine = null;
            }

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
                
                if (videoFadeCoroutine != null)
                {
                    StopCoroutine(videoFadeCoroutine);
                }
                videoFadeCoroutine = StartCoroutine(FadeVideoIn());
            }
            vp.Play();
        }

        /// <summary>
        /// 動画表示用 RawImage の透明度を 0→1 に上げてフェードインさせます。
        /// </summary>
        private IEnumerator FadeVideoIn()
        {
            float timer = 0f;
            Color color = videoRenderTexture.color;
            color.a = 0f;
            videoRenderTexture.color = color;

            while (timer < videoFadeDuration)
            {
                timer += Time.deltaTime;
                color.a = Mathf.Clamp01(timer / videoFadeDuration);
                videoRenderTexture.color = color;
                yield return null;
            }

            color.a = 1f;
            videoRenderTexture.color = color;
            videoFadeCoroutine = null;
        }

        /// <summary>
        /// テキスト・画像・動画の表示をすべて消去し、再生を停止します。
        /// </summary>
        public void ClearAll()
        {
            if (videoFadeCoroutine != null)
            {
                StopCoroutine(videoFadeCoroutine);
                videoFadeCoroutine = null;
            }

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
