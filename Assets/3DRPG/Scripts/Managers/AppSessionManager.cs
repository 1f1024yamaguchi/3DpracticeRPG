using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    /// <summary>
    /// アプリケーションセッション全体の状態を管理するマネージャー。
    /// シーン遷移履歴を自動的に追跡し、「前のシーンに戻る」機能を提供する。
    ///
    /// 使い方:
    ///   1. 空のGameObjectに本コンポーネントをアタッチ
    ///   2. DontDestroyOnLoad で自動永続化される
    /// </summary>
    public class AppSessionManager : MonoBehaviour
    {
        public static AppSessionManager Instance { get; private set; }

        /// <summary>直前のシーン名。履歴がない場合は空文字列。</summary>
        public string previousSceneName { get; private set; } = "";

        // シーン履歴スタック（将来的に多段階「戻る」に対応可能）
        private readonly Stack<string> _sceneHistory = new Stack<string>();

        // 現在のシーン名（遷移検出用）
        private string _currentSceneName;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // 初期シーン名を記録
                _currentSceneName = SceneManager.GetActiveScene().name;

                // シーン遷移イベントを購読
                SceneManager.activeSceneChanged += OnActiveSceneChanged;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            // oldScene.name が有効な場合のみ履歴に追加
            // (初回ロード時は oldScene.name が空の場合がある)
            if (!string.IsNullOrEmpty(_currentSceneName))
            {
                _sceneHistory.Push(_currentSceneName);
                previousSceneName = _currentSceneName;
            }

            _currentSceneName = newScene.name;
        }

        /// <summary>
        /// 履歴スタックから1つ戻る。戻り先のシーン名を返す。
        /// 履歴がない場合は空文字列を返す。
        /// </summary>
        public string PopPreviousScene()
        {
            if (_sceneHistory.Count > 0)
            {
                string scene = _sceneHistory.Pop();
                previousSceneName = _sceneHistory.Count > 0 ? _sceneHistory.Peek() : "";
                return scene;
            }
            return "";
        }

        /// <summary>
        /// 履歴をすべてクリアする。
        /// タイトル画面に戻った際などに使用。
        /// </summary>
        public void ClearHistory()
        {
            _sceneHistory.Clear();
            previousSceneName = "";
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
