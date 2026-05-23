using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UI
{
    // ─────────────────────────────────────────────────────────────────────────
    // メニューの各項目を定義するデータクラス。
    // Inspectorの List<MenuEntryData> に内容を記入し、Editorボタンで生成します。
    // ─────────────────────────────────────────────────────────────────────────
    [System.Serializable]
    public class MenuEntryData
    {
        // [Header("Basic Settings")]
        public string itemName = "New Item";
        public GenericMenuItem.ItemType type = GenericMenuItem.ItemType.Button;

        [Tooltip("If set, will load/save value from PlayerPrefs using this key")]
        public string playerPrefsKey;

        [TextArea(2, 3)]
        public string description;

        [Tooltip("ボタン操作の説明（例：↓↘→ + P）")]
        public string commandInputText;

        [Tooltip("この項目を選択中に再生するプレビュー動画")]
        public VideoClip previewVideo;

        [Tooltip("この項目を選択中にプレビュー表示させる、または開く子メニュー")]
        public GameObject targetSubMenu;

        // ── Slider / Selector / Toggle 用 ──────────────────────────
        public int initialValue = 0;
        public int minValue = 0;
        public int maxValue = 10;
        public string[] selectorOptions;

        [Tooltip("False の場合、決定キーを押しても OnSubmit を実行しません（項目は選択可能なまま）")]
        public bool isPermitted = true;

        // ── イベント ────────────────────────────────────────────────
        // [Header("Events")]
        public UnityEvent OnSubmit;
        public UnityEvent<int> OnValueChanged;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // メニューを「Editorで手動生成」するコンポーネント。
    // Runtime での動的生成は行わないため Start/Awake での処理はありません。
    //
    // 使い方:
    //   1. menuItems を Inspector で設定する
    //   2. Inspectorボタン「メニューを生成」を押してシーンに焼き付ける
    //   3. シーンを保存すれば以降 Runtime でも追加処理は不要
    // ─────────────────────────────────────────────────────────────────────────
    public class AutoMenuGenerator : MonoBehaviour
    {
        [Header("Prefab & Container")]
        [Tooltip("GenericMenuItem が付与された雛形 Prefab")]
        public GenericMenuItem menuItemPrefab;

        [Tooltip("生成した項目を並べる親オブジェクト")]
        public Transform container;

        [Header("Menu Definition")]
        [Tooltip("メニューの選択肢を上から順番に設定します（Editor生成用データ）")]
        public List<MenuEntryData> menuItems = new List<MenuEntryData>();

        [Header("Settings")]
        [Tooltip("このメニューが開かれている時、キャンセル操作（戻る）を許可するかどうか")]
        public bool allowCancel = true;

        // 生成済み項目のリスト。[HideInInspector] にしておくことで
        // Inspector を汚さず、シリアライズによる参照も保持できる。
        [HideInInspector, SerializeField]
        private List<GenericMenuItem> _generatedItems = new List<GenericMenuItem>();

        // ─────────────────────────────────────────────────────────────
        // Editor / ContextMenu から呼ぶ生成・クリア
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 生成済みのメニュー項目をすべて削除します。
        /// Playモード・Editorモード両対応。
        /// </summary>
        [ContextMenu("メニューをクリア")]
        public void ClearMenu()
        {
            foreach (var item in _generatedItems)
            {
                if (item == null || item.gameObject == null) continue;

                if (Application.isPlaying)
                    Destroy(item.gameObject);
                else
                    DestroyImmediate(item.gameObject);
            }
            _generatedItems.Clear();
        }

        /// <summary>
        /// menuItems の定義に従ってメニュー項目を生成し、ナビゲーションリンクを設定します。
        /// 既存の項目がある場合は先にクリアします。
        /// </summary>
        [ContextMenu("メニューを生成")]
        public void GenerateMenu()
        {
            ClearMenu();

            if (menuItemPrefab == null || container == null)
            {
                Debug.LogError("[AutoMenuGenerator] Prefab または Container が未設定です。");
                return;
            }

            foreach (var data in menuItems)
            {
                var newItem = Instantiate(menuItemPrefab, container);
                newItem.gameObject.SetActive(true);

                // ── 基本設定 ──────────────────────────────────────────
                newItem.labelText = data.itemName;
                newItem.itemType = data.type;
                newItem.descriptionText = data.description;
                newItem.commandInputText = data.commandInputText;
                newItem.previewVideo = data.previewVideo;
                newItem.targetSubMenu = data.targetSubMenu;
                newItem.playerPrefsKey = data.playerPrefsKey;

                // ── 値設定（Slider / Selector / Toggle） ──────────────
                newItem.currentValue = data.initialValue;
                newItem.minValue = data.minValue;
                newItem.maxValue = data.maxValue;
                newItem.selectorOptions = data.selectorOptions;

                // ── イベント ──────────────────────────────────────────
                newItem.OnSubmitEvent = data.OnSubmit;
                newItem.OnValueChangedEvent = data.OnValueChanged;

                // ── 許可フラグ ────────────────────────────────────────
                newItem.isPermitted = data.isPermitted;

                newItem.RefreshUI();
                _generatedItems.Add(newItem);
            }

            LinkNavigation();
        }

        /// <summary>
        /// 生成された項目間で循環する上下ナビゲーションリンクを設定します。
        /// </summary>
        private void LinkNavigation()
        {
            int count = _generatedItems.Count;
            for (int i = 0; i < count; i++)
            {
                var current = _generatedItems[i];
                var nav = current.navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = _generatedItems[(i - 1 + count) % count];
                nav.selectOnDown = _generatedItems[(i + 1) % count];
                current.navigation = nav;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // シーン遷移ヘルパー（UnityEvent から呼び出し用）
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Inspector のイベント欄からシーン遷移を呼び出すためのヘルパーです。
        /// </summary>
        public void CallFadeToScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        public void RestartThisScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
