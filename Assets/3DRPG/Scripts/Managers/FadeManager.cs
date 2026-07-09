using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    /// <summary>
    /// フェード付きシーン遷移を提供するマネージャー。
    /// 既存の LoadingScreenManager が存在すればそれを利用し、
    /// 存在しなければ直接 SceneManager.LoadScene でフォールバックする。
    ///
    /// 使い方:
    ///   1. 空のGameObjectに本コンポーネントをアタッチ
    ///   2. LoadingScreenManager と同じシーン、または DontDestroyOnLoad で永続化
    /// </summary>
    public class FadeManager : MonoBehaviour
    {
        public static FadeManager Instance { get; private set; }

        [Header("Fallback Settings")]
        [Tooltip("LoadingScreenManager が存在しない場合に Tips を表示するか")]
        [SerializeField] private bool showTipsOnFallback = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// フェード演出付きでシーンを遷移する。
        /// LoadingScreenManager があればフェード+Tips付き、
        /// なければ直接ロード。
        /// </summary>
        /// <param name="sceneName">遷移先のシーン名</param>
        public void FadeToScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[FadeManager] シーン名が空です。遷移をキャンセルしました。");
                return;
            }

            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.LoadScene(sceneName, showTipsOnFallback);
            }
            else
            {
                Debug.LogWarning($"[FadeManager] LoadingScreenManager が見つかりません。直接シーン '{sceneName}' をロードします。");
                SceneManager.LoadScene(sceneName);
            }
        }

        /// <summary>
        /// Tips表示なしの高速フェード遷移。
        /// </summary>
        /// <param name="sceneName">遷移先のシーン名</param>
        public void FadeToSceneQuick(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[FadeManager] シーン名が空です。遷移をキャンセルしました。");
                return;
            }

            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.LoadScene(sceneName, false);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
