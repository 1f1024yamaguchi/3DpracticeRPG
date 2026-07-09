using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using TMPro;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class GenericMenuItem : Selectable, ISubmitHandler, IPointerClickHandler, ISelectHandler
    {
        public enum ItemType { Button, Slider, Toggle, Selector, Carousel }

        [Header("Item Config")]
        public ItemType itemType = ItemType.Button;
        public string labelText = "New Item";
        [TextArea(2, 4)] public string descriptionText = "";
        public string commandInputText = "";
        public VideoClip previewVideo;
        public GameObject targetSubMenu;
        [Tooltip("選択時にサブメニューをプレビュー表示するかどうか。チェックを外すと決定を押すまでサブメニューが表示されません。")]
        public bool showSubMenuAsPreview = true;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _labelTMPro;
        [SerializeField] private TextMeshProUGUI _valueTMPro;
        [SerializeField] private TextMeshProUGUI _commandInputTMPro;
        public GameObject cursorObject;

        [Header("Events")]
        public UnityEvent OnSubmitEvent;
        public UnityEvent<int> OnValueChangedEvent;

        [Header("Values")]
        [Tooltip("PlayerPrefsにデータがない時に使われる初期値")]
        public int initialValue = 0;
        public int currentValue = 0;
        public int minValue = 0;
        public int maxValue = 10;
        public string[] selectorOptions;

        [Tooltip("If set, will load/save value from SystemSettingsManager using this key")]
        public string playerPrefsKey;

        public bool isPermitted = true;

        [Header("Carousel Data")]
        public System.Collections.Generic.List<UI.MultiMedia.MediaPageData> mediaPages;

        [Header("Input Settings")]
        [SerializeField] private float inputCooldown = 0.2f;
        private float _lastInputTime;

        [Header("Disabled Visuals")]
        public Color disabledTextColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public Color disabledSelectedTextColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        public Color normalTextColor = Color.white;

        private bool _isRetainedFocus;
        private MenuManager _menuManager;

        protected override void Awake()
        {
            base.Awake();
            TryAutoFetchReferences();
            _menuManager = GetComponentInParent<MenuManager>(true);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            LoadSettings();
            RefreshUI();
        }

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
        protected override void OnValidate()
        {
            base.OnValidate();
            RefreshUI();
            RefreshUIState(currentSelectionState);
        }
#endif

        private void Update()
        {
            if (!IsSelectedByEventSystem()) return;
            if (itemType != ItemType.Slider && itemType != ItemType.Selector) return;

            float h = GetHorizontalInput();
            if (Mathf.Abs(h) > 0.5f)
            {
                if (Time.unscaledTime - _lastInputTime > inputCooldown)
                {
                    if (h > 0) IncreaseValue();
                    else DecreaseValue();
                    _lastInputTime = Time.unscaledTime;
                }
            }
            else if (Mathf.Abs(h) < 0.1f)
            {
                _lastInputTime = 0f;
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            RefreshUIState(state);
        }

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
                if (!string.IsNullOrEmpty(descriptionText))
                    _menuManager.UpdateDescription(descriptionText);
                
                _menuManager.UpdatePreviewVideo(previewVideo);

                if (targetSubMenu != null && showSubMenuAsPreview)
                    _menuManager.ShowPreview(targetSubMenu);
            }

            // Carouselプレビュー処理
            if (itemType == ItemType.Carousel && targetSubMenu != null && mediaPages != null && mediaPages.Count > 0 && showSubMenuAsPreview)
            {
                var carouselController = targetSubMenu.GetComponent<UI.MultiMedia.CarouselMenuController>();
                if (carouselController != null)
                {
                    carouselController.ShowPreview(labelText, mediaPages[0]);
                }
            }
        }

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

        public void OnSubmit(BaseEventData eventData)
        {
            if (!isPermitted) return;

            // if (targetSubMenu == null && SEManager.Instance != null)
            // {
            //     SEManager.Instance.PlaySE("NameSet");
            // }

            if (itemType == ItemType.Toggle) ToggleValue();
            else if (itemType == ItemType.Carousel && targetSubMenu != null)
            {
                var carouselController = targetSubMenu.GetComponent<UI.MultiMedia.CarouselMenuController>();
                if (carouselController != null)
                {
                    carouselController.Initialize(labelText, mediaPages);
                }
            }

            OnSubmitEvent?.Invoke();

            if (targetSubMenu != null && _menuManager != null)
            {
                _menuManager.OpenMenu(targetSubMenu, true);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (!IsActive() || !IsInteractable() || !isPermitted) return;
            OnSubmit(null);
        }

        public void SetRetainedFocus(bool retained)
        {
            _isRetainedFocus = retained;
            DoStateTransition(currentSelectionState, true);
        }

        public void RefreshUI()
        {
            if (_labelTMPro != null) _labelTMPro.text = labelText;
            if (_commandInputTMPro != null) _commandInputTMPro.text = commandInputText;
            if (_valueTMPro == null || _valueTMPro == _labelTMPro) return;

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

        private void TryAutoFetchReferences()
        {
            if (_labelTMPro != null) return;
            var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            if (texts.Length > 0) _labelTMPro = texts[0];
            if (texts.Length > 1) _valueTMPro = texts[1];
        }

        private bool IsSelectedByEventSystem() => EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;

        private void RefreshUIState(SelectionState state)
        {
            bool isDisabled = !IsInteractable() || state == SelectionState.Disabled;
            if (_labelTMPro != null)
            {
                Color c = isDisabled ? (_isRetainedFocus ? disabledSelectedTextColor : disabledTextColor) : normalTextColor;
                _labelTMPro.color = c;
                if (_commandInputTMPro != null) _commandInputTMPro.color = c;
                if (_valueTMPro != null && _valueTMPro != _labelTMPro) _valueTMPro.color = c;
            }
            if (cursorObject != null)
            {
                bool show = state == SelectionState.Selected || state == SelectionState.Pressed || state == SelectionState.Highlighted || (isDisabled && _isRetainedFocus);
#if UNITY_EDITOR
                if (!Application.isPlaying) show = true;
#endif
                if (cursorObject.activeSelf != show) cursorObject.SetActive(show);
            }
        }

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

        private void ToggleValue()
        {
            currentValue = (currentValue == 0) ? 1 : 0;
            RefreshUI();
            OnValueChangedEvent?.Invoke(currentValue);
            SaveSettings();
        }

        private void SaveSettings()
        {
            if (string.IsNullOrEmpty(playerPrefsKey)) return;
            // if (SystemSettingsManager.Instance != null)
            // {
            //     SystemSettingsManager.Instance.SaveSettingInt(playerPrefsKey, currentValue);
            // }
        }

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