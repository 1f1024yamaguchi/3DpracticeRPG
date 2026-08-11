using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using TMPro;

namespace UI
{
    // ─────────────────────────────────────────────────────────────────────────
    // メニューの「1項目」を表す万能コンポーネント。
    // Unity標準の Selectable を継承しているため、EventSystem による
    // 上下移動・決定・マウスクリックをそのまま利用できます。
    //
    // itemType によって挙動が変わります:
    //   Button   : 決定で OnSubmitEvent を実行（サブメニューがあれば開く）
    //   Slider   : 左右キーで数値を min〜max の範囲で増減
    //   Toggle   : 決定で ON/OFF を切り替え
    //   Selector : 左右キーで selectorOptions の文字列を切り替え
    //   Carousel : 決定で CarouselMenuController を初期化してページ閲覧モードへ
    //
    // 通常は AutoMenuGenerator がこのコンポーネント付き Prefab を複製し、
    // 各フィールドへ値を流し込む形で使用します。
    // ─────────────────────────────────────────────────────────────────────────
    [RequireComponent(typeof(RectTransform))]
    public class GenericMenuItem : Selectable, ISubmitHandler, IPointerClickHandler, ISelectHandler
    {
        // 項目の種類。種類ごとに入力の解釈と表示が変わる
        public enum ItemType { Button, Slider, Toggle, Selector, Carousel }

        [Header("Item Config")]
        public ItemType itemType = ItemType.Button;
        public string labelText = "New Item";                       // 項目名（左側に表示するテキスト）
        [TextArea(2, 4)] public string descriptionText = "";        // 選択中に説明欄へ表示する文章
        public string commandInputText = "";                        // 操作説明（例：↓↘→ + P）
        public VideoClip previewVideo;                              // 選択中に再生するプレビュー動画
        public GameObject targetSubMenu;                            // 決定で開くサブメニュー
        [Tooltip("選択時にサブメニューをプレビュー表示するかどうか。チェックを外すと決定を押すまでサブメニューが表示されません。")]
        public bool showSubMenuAsPreview = true;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _labelTMPro;        // 項目名の表示先
        [SerializeField] private TextMeshProUGUI _valueTMPro;        // 値（数値/ON/OFF/選択肢）の表示先
        [SerializeField] private TextMeshProUGUI _commandInputTMPro; // 操作説明の表示先
        public GameObject cursorObject;                              // 選択中に表示するカーソル

        [Header("Events")]
        public UnityEvent OnSubmitEvent;            // 決定時に実行されるイベント
        public UnityEvent<int> OnValueChangedEvent; // 値変更時に実行されるイベント（新しい値が渡る）

        [Header("Values")]
        [Tooltip("PlayerPrefsにデータがない時に使われる初期値")]
        public int initialValue = 0;
        public int currentValue = 0;   // 現在の値（Slider:数値 / Toggle:0or1 / Selector:選択index）
        public int minValue = 0;       // Slider の最小値
        public int maxValue = 10;      // Slider の最大値
        public string[] selectorOptions; // Selector で表示する選択肢の文字列

        [Tooltip("If set, will load/save value from SystemSettingsManager using this key")]
        public string playerPrefsKey;

        // false の場合、決定操作を受け付けない（選択は可能なまま）
        public bool isPermitted = true;

        [Header("Carousel Data")]
        // Carousel タイプの時に閲覧させるページデータ一覧
        public System.Collections.Generic.List<UI.MultiMedia.MediaPageData> mediaPages;

        [Header("Input Settings")]
        // 左右長押しで値が変わりすぎないようにする連続入力の間隔（秒）
        [SerializeField] private float inputCooldown = 0.2f;
        private float _lastInputTime; // 最後に左右入力を受け付けた時刻

        [Header("Disabled Visuals")]
        public Color disabledTextColor = new Color(0.5f, 0.5f, 0.5f, 1f);         // 非アクティブ時の文字色
        public Color disabledSelectedTextColor = new Color(0.8f, 0.8f, 0.8f, 1f); // 非アクティブだがフォーカス保持中の文字色
        public Color normalTextColor = Color.white;                                // 通常時の文字色

        private bool _isRetainedFocus;      // サブメニューを開いた際、親側の項目が「選択中のまま」であることを示すフラグ
        private MenuManager _menuManager;   // 親階層の MenuManager（説明文・プレビューの更新先）

        /// <summary>
        /// 参照の自動取得と親 MenuManager のキャッシュを行います。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            TryAutoFetchReferences();
            _menuManager = GetComponentInParent<MenuManager>(true);
        }

        /// <summary>
        /// 有効化時に保存値を読み込み、表示を最新化します。
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            LoadSettings();
            RefreshUI();
        }

        /// <summary>
        /// playerPrefsKey が設定されていれば保存済みの値を読み込みます。
        /// （SystemSettingsManager 連携は現在コメントアウト中）
        /// </summary>
        private void LoadSettings()
        {
            if (string.IsNullOrEmpty(playerPrefsKey)) return;

            // if (SystemSettingsManager.Instance != null)
            // {
            //     // currentValue ではなく initialValue をデフォルトとして渡す
            //     currentValue = SystemSettingsManager.Instance.GetSettingInt(playerPrefsKey, initialValue);
            // }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Inspector の値が変更された時に表示を即時反映します（Editor専用）。
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            RefreshUI();
            RefreshUIState(currentSelectionState);
        }
#endif

        /// <summary>
        /// Slider / Selector の左右入力を毎フレーム監視します。
        /// OnMove ではなくポーリングにすることで「押しっぱなしでの連続変更」を実現しています。
        /// </summary>
        private void Update()
        {
            // 自分が選択されていない、または値を持たないタイプなら何もしない
            if (!IsSelectedByEventSystem()) return;
            if (itemType != ItemType.Slider && itemType != ItemType.Selector) return;

            float h = GetHorizontalInput();
            if (Mathf.Abs(h) > 0.5f)
            {
                // クールダウンを挟んで値を増減（押しっぱなし対応）
                if (Time.unscaledTime - _lastInputTime > inputCooldown)
                {
                    if (h > 0) IncreaseValue();
                    else DecreaseValue();
                    _lastInputTime = Time.unscaledTime;
                }
            }
            else if (Mathf.Abs(h) < 0.1f)
            {
                // 入力が離されたらクールダウンをリセット（次の入力を即受け付ける）
                _lastInputTime = 0f;
            }
        }

        /// <summary>
        /// Selectable の状態遷移（Normal/Selected/Disabled等）に合わせて見た目を更新します。
        /// </summary>
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            RefreshUIState(state);
        }

        /// <summary>
        /// この項目が選択された時の処理。
        /// 説明文・プレビュー動画・サブメニューのプレビュー表示を MenuManager 経由で更新します。
        /// </summary>
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            // 選択時にもロード（他で変更された際の同期）
            LoadSettings();
            RefreshUI();

            if (_menuManager != null && _menuManager.CanPlayNavigationSound)
            {
                // if (SEManager.Instance != null) SEManager.Instance.PlaySE("Select");
            }

            if (_menuManager != null)
            {
                // 説明文の更新
                if (!string.IsNullOrEmpty(descriptionText))
                    _menuManager.UpdateDescription(descriptionText);

                // プレビュー動画の再生（nullなら停止）
                _menuManager.UpdatePreviewVideo(previewVideo);

                // サブメニューのプレビュー表示
                if (targetSubMenu != null && showSubMenuAsPreview)
                    _menuManager.ShowPreview(targetSubMenu);
            }

            // Carouselプレビュー処理：選択しただけで1ページ目を右側に表示する
            if (itemType == ItemType.Carousel && targetSubMenu != null && mediaPages != null && mediaPages.Count > 0 && showSubMenuAsPreview)
            {
                var carouselController = targetSubMenu.GetComponent<UI.MultiMedia.CarouselMenuController>();
                if (carouselController != null)
                {
                    carouselController.ShowPreview(labelText, mediaPages[0]);
                }
            }
        }

        /// <summary>
        /// 選択が外れた時の処理。説明文・プレビューをクリアします。
        /// ただしサブメニューへ潜った場合（_isRetainedFocus）はカルーセル表示を残します。
        /// </summary>
        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            _lastInputTime = 0f;
            if (_menuManager != null)
            {
                _menuManager.ClearDescription();
                _menuManager.ClearPreviewVideo();
                if (targetSubMenu != null && showSubMenuAsPreview)
                    _menuManager.HidePreview(targetSubMenu);
            }

            // Carouselプレビュークリア処理
            if (itemType == ItemType.Carousel && targetSubMenu != null && !_isRetainedFocus && showSubMenuAsPreview)
            {
                var carouselController = targetSubMenu.GetComponent<UI.MultiMedia.CarouselMenuController>();
                if (carouselController != null)
                {
                    carouselController.ClearPreview();
                }
            }
        }

        /// <summary>
        /// 決定（Submit）時の処理。タイプごとに分岐します。
        ///   Toggle   → 値を反転
        ///   Carousel → CarouselMenuController を初期化
        ///   共通     → OnSubmitEvent 実行 → サブメニューがあれば開く
        /// </summary>
        public void OnSubmit(BaseEventData eventData)
        {
            // 許可されていない項目は何もしない
            if (!isPermitted) return;

            // if (targetSubMenu == null && SEManager.Instance != null)
            // {
            //     SEManager.Instance.PlaySE("NameSet");
            // }

            if (itemType == ItemType.Toggle) ToggleValue();
            else if (itemType == ItemType.Carousel && targetSubMenu != null)
            {
                // カルーセルにページデータを渡して閲覧モードを開始
                var carouselController = targetSubMenu.GetComponent<UI.MultiMedia.CarouselMenuController>();
                if (carouselController != null)
                {
                    carouselController.Initialize(labelText, mediaPages);
                }
            }

            OnSubmitEvent?.Invoke();

            // サブメニューがあれば MenuManager のスタックに積んで開く
            if (targetSubMenu != null && _menuManager != null)
            {
                _menuManager.OpenMenu(targetSubMenu, true);
            }
        }

        /// <summary>
        /// マウス左クリックを決定操作として扱います。
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (!IsActive() || !IsInteractable() || !isPermitted) return;
            OnSubmit(null);
        }

        /// <summary>
        /// サブメニューを開いた際に MenuManager から呼ばれ、
        /// 「親メニュー側で選択されたままの見た目」を維持するかどうかを設定します。
        /// </summary>
        public void SetRetainedFocus(bool retained)
        {
            _isRetainedFocus = retained;
            DoStateTransition(currentSelectionState, true);
        }

        /// <summary>
        /// ラベル・操作説明・値テキストを現在の状態に合わせて再描画します。
        /// AutoMenuGenerator が値を流し込んだ後にも呼ばれます。
        /// </summary>
        public void RefreshUI()
        {
            if (_labelTMPro != null) _labelTMPro.text = labelText;
            if (_commandInputTMPro != null) _commandInputTMPro.text = commandInputText;
            if (_valueTMPro == null || _valueTMPro == _labelTMPro) return;

            // タイプごとに値テキストの表示内容を切り替える
            switch (itemType)
            {
                case ItemType.Slider: _valueTMPro.text = currentValue.ToString(); break;
                case ItemType.Selector:
                    _valueTMPro.text = (selectorOptions != null && currentValue >= 0 && currentValue < selectorOptions.Length)
                        ? selectorOptions[currentValue] : currentValue.ToString();
                    break;
                case ItemType.Toggle: _valueTMPro.text = currentValue == 1 ? "ON" : "OFF"; break;
                default: _valueTMPro.text = ""; break;
            }
        }

        /// <summary>
        /// Inspector で参照が未設定の場合、子の TextMeshProUGUI を自動で拾います。
        /// （1つ目=ラベル、2つ目=値テキスト とみなす）
        /// </summary>
        private void TryAutoFetchReferences()
        {
            if (_labelTMPro != null) return;
            var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            if (texts.Length > 0) _labelTMPro = texts[0];
            if (texts.Length > 1) _valueTMPro = texts[1];
        }

        // EventSystem 上で現在この項目が選択されているか
        private bool IsSelectedByEventSystem() => EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;

        /// <summary>
        /// 選択状態に応じて文字色とカーソルの表示/非表示を更新します。
        /// </summary>
        private void RefreshUIState(SelectionState state)
        {
            bool isDisabled = !IsInteractable() || state == SelectionState.Disabled;
            if (_labelTMPro != null)
            {
                // 無効時はグレー系、フォーカス保持中はやや明るいグレーにする
                Color c = isDisabled ? (_isRetainedFocus ? disabledSelectedTextColor : disabledTextColor) : normalTextColor;
                _labelTMPro.color = c;
                if (_commandInputTMPro != null) _commandInputTMPro.color = c;
                if (_valueTMPro != null && _valueTMPro != _labelTMPro) _valueTMPro.color = c;
            }
            if (cursorObject != null)
            {
                // 選択中（またはフォーカス保持中）だけカーソルを表示
                bool show = state == SelectionState.Selected || state == SelectionState.Pressed || state == SelectionState.Highlighted || (isDisabled && _isRetainedFocus);
#if UNITY_EDITOR
                if (!Application.isPlaying) show = true; // Editor上では位置確認のため常時表示
#endif
                if (cursorObject.activeSelf != show) cursorObject.SetActive(show);
            }
        }

        /// <summary>
        /// 値を1増やします（Selectorの場合は選択肢数-1が上限）。
        /// 変化があれば表示更新・イベント発火・保存を行います。
        /// </summary>
        private void IncreaseValue()
        {
            int max = (itemType == ItemType.Selector && selectorOptions != null && selectorOptions.Length > 0) ? selectorOptions.Length - 1 : maxValue;
            int old = currentValue;
            currentValue = Mathf.Min(currentValue + 1, max);
            if (old != currentValue)
            {
                RefreshUI();
                OnValueChangedEvent?.Invoke(currentValue);
                SaveSettings();
            }
        }

        /// <summary>
        /// 値を1減らします（Selectorの場合は0が下限）。
        /// </summary>
        private void DecreaseValue()
        {
            int min = (itemType == ItemType.Selector) ? 0 : minValue;
            int old = currentValue;
            currentValue = Mathf.Max(currentValue - 1, min);
            if (old != currentValue)
            {
                RefreshUI();
                OnValueChangedEvent?.Invoke(currentValue);
                SaveSettings();
            }
        }

        /// <summary>
        /// Toggle 用。0⇔1 を反転させます。
        /// </summary>
        private void ToggleValue()
        {
            currentValue = (currentValue == 0) ? 1 : 0;
            RefreshUI();
            OnValueChangedEvent?.Invoke(currentValue);
            SaveSettings();
        }

        /// <summary>
        /// playerPrefsKey が設定されていれば現在値を保存します。
        /// （SystemSettingsManager 連携は現在コメントアウト中）
        /// </summary>
        private void SaveSettings()
        {
            if (string.IsNullOrEmpty(playerPrefsKey)) return;
            // if (SystemSettingsManager.Instance != null)
            // {
            //     SystemSettingsManager.Instance.SaveSettingInt(playerPrefsKey, currentValue);
            // }
        }

        /// <summary>
        /// キーボード（←→/A/D）とゲームパッド（十字キー/左スティック）から
        /// 水平方向の入力値（-1〜1）を取得します。
        /// </summary>
        private float GetHorizontalInput()
        {
            float h = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) h = 1f;
                else if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) h = -1f;
            }
            if (Gamepad.current != null && h == 0f)
            {
                h = Gamepad.current.dpad.ReadValue().x;
                if (Mathf.Abs(h) < 0.1f) h = Gamepad.current.leftStick.ReadValue().x;
            }
            return h;
        }
    }
}
