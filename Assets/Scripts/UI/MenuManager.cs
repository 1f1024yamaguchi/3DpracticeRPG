using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Video;

namespace UI
{
    public class MenuManager : MonoBehaviour
    {
        public enum TransitionType { Fade, Slide, Instant, Cascade }

        [Header("Settings")]
        [SerializeField] private TMPro.TextMeshProUGUI descriptionText;
        [SerializeField] private VideoPlayer previewVideoPlayer;
        [SerializeField] private GameObject previewVideoUI; // 動画を表示するUI（RawImage等）
        [SerializeField] private GameObject firstMenu;

        [Header("Animation Settings")]
        [SerializeField] private TransitionType transitionType = TransitionType.Slide;
        [SerializeField] private float transitionDuration = 0.2f;
        [SerializeField] private float slideDistance = 1920f;

        private readonly Stack<MenuState> _menuStack = new Stack<MenuState>();
        private GameObject _previewMenu;
        private GameObject _lastSelected;
        private readonly Dictionary<GameObject, Coroutine> _activeTransitions = new Dictionary<GameObject, Coroutine>();

        // 操作中のみTrueにするフラグ
        public bool CanPlayNavigationSound { get; private set; } = false;

        private struct MenuState
        {
            public GameObject MenuObject;
            public GameObject PreviousFocus;
        }

        private void Awake()
        {
            CanPlayNavigationSound = false;
        }

        private void Start()
        {
            if (firstMenu != null) OpenMenu(firstMenu, false);
        }

        public void OpenMenu(GameObject newMenu, bool playSound)
        {
            if (newMenu == null) return;

            // 遷移開始時は音を禁止
            CanPlayNavigationSound = false;

            GameObject currentFocus = EventSystem.current?.currentSelectedGameObject;

            if (_menuStack.Count > 0)
            {
                var current = _menuStack.Peek();
                
                // 二重登録（同じメニューを連続で開く）を防止
                if (current.MenuObject == newMenu) return;

                if (current.MenuObject != null)
                {
                    MarkRetainedFocus(currentFocus, retain: true);
                    SetFocusEffect(current.MenuObject, focused: false);
                    
                    // 現在のメニューを左に退場させる
                    TransitionOut(current.MenuObject, -1f);
                }
            }

            _menuStack.Push(new MenuState { MenuObject = newMenu, PreviousFocus = currentFocus });

            newMenu.SetActive(true);
            SetFocusEffect(newMenu, focused: true);
            
            // 新しいメニューを右から入場させる
            TransitionIn(newMenu, 1f);
            
            FocusFirst(newMenu);

            // フォーカス確定後に音を許可
            StartCoroutine(EnableSoundRoutine());
        }

        public void CloseMenu()
        {
            if (_menuStack.Count == 0) return;

            CanPlayNavigationSound = false;

            var closed = _menuStack.Pop();
            if (closed.MenuObject != null)
            {
                // 閉じるメニューを右に退場させる
                TransitionOut(closed.MenuObject, 1f, deactivateAfter: true);
            }

            MarkRetainedFocus(closed.PreviousFocus, retain: false);
            ClearDescription();
            ClearPreviewVideo();

            if (_menuStack.Count > 0)
            {
                var parent = _menuStack.Peek();
                if (parent.MenuObject != null)
                {
                    // 親メニューを左から入場させる
                    parent.MenuObject.SetActive(true);
                    TransitionIn(parent.MenuObject, -1f);
                    
                    SetFocusEffect(parent.MenuObject, focused: true);
                }
                RestoreFocus(closed.PreviousFocus, parent.MenuObject);
                StartCoroutine(EnableSoundRoutine());
            }
            else
            {
                EventSystem.current?.SetSelectedGameObject(null);
                CanPlayNavigationSound = false;
            }
        }

        private void TransitionIn(GameObject menuObj, float directionX)
        {
            if (transitionType == TransitionType.Instant || transitionType == TransitionType.Cascade)
            {
                var cg = menuObj.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
                var rt = menuObj.GetComponent<RectTransform>();
                if (transitionType == TransitionType.Instant && rt != null) rt.anchoredPosition = Vector2.zero;
                return;
            }

            if (_activeTransitions.TryGetValue(menuObj, out Coroutine existing) && existing != null)
            {
                StopCoroutine(existing);
            }

            _activeTransitions[menuObj] = StartCoroutine(TransitionCoroutine(menuObj, true, directionX, false));
        }

        private void TransitionOut(GameObject menuObj, float directionX, bool deactivateAfter = false)
        {
            if (transitionType == TransitionType.Cascade)
            {
                // Cascade（階段状）の場合は、前のメニューを透明にせずそのまま残す
                var cg = menuObj.GetComponent<CanvasGroup>();
                if (cg != null && deactivateAfter) cg.alpha = 0f; // 完全に閉じる時だけ透明にする
                if (deactivateAfter) menuObj.SetActive(false);
                return;
            }

            if (transitionType == TransitionType.Instant)
            {
                var cg = menuObj.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 0f;
                if (deactivateAfter) menuObj.SetActive(false);
                return;
            }

            if (_activeTransitions.TryGetValue(menuObj, out Coroutine existing) && existing != null)
            {
                StopCoroutine(existing);
            }

            _activeTransitions[menuObj] = StartCoroutine(TransitionCoroutine(menuObj, false, directionX, deactivateAfter));
        }

        private IEnumerator TransitionCoroutine(GameObject menuObj, bool isEntering, float directionX, bool deactivateAfter)
        {
            var cg = menuObj.GetComponent<CanvasGroup>();
            if (cg == null) cg = menuObj.AddComponent<CanvasGroup>();
            var rt = menuObj.GetComponent<RectTransform>();

            float elapsedTime = 0f;

            // 初期状態のセット
            // X座標は常に画面中央(0)を基準とし、Y座標のみ現在のInspectorの値を維持する
            Vector2 originalPos = rt != null ? new Vector2(0f, rt.anchoredPosition.y) : Vector2.zero;
            Vector2 startPos = originalPos;
            Vector2 endPos = originalPos;

            float startAlpha = isEntering ? 0f : 1f;
            float endAlpha = isEntering ? 1f : 0f;

            //Vector2 startPos = Vector2.zero; //初期位置設定を変えたのでコメントアウト（後から消してもよい）
            //Vector2 endPos = Vector2.zero;   //初期位置設定を変えたのでコメントアウト（後から消してもよい）

            if (transitionType == TransitionType.Slide && rt != null)
            {
                cg.alpha = 1f; // Fadeの透明状態をリセット
                float offset = slideDistance * directionX;
                startPos = isEntering ? new Vector2(offset, originalPos.y) : originalPos;
                endPos = isEntering ? originalPos : new Vector2(offset, originalPos.y);
                rt.anchoredPosition = startPos;
            }

            if (transitionType == TransitionType.Fade)
            {
                cg.alpha = startAlpha;
            }

            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / transitionDuration);
                
                // イーズアウト（滑らかに減速）
                float easeT = 1f - Mathf.Pow(1f - t, 3f);

                if (transitionType == TransitionType.Fade)
                {
                    cg.alpha = Mathf.Lerp(startAlpha, endAlpha, easeT);
                }

                if (transitionType == TransitionType.Slide && rt != null)
                {
                    rt.anchoredPosition = Vector2.Lerp(startPos, endPos, easeT);
                }

                yield return null;
            }

            if(transitionType == TransitionType.Fade)
            {
                cg.alpha = endAlpha;
            }
            
            if (transitionType == TransitionType.Slide && rt != null)
            {
                rt.anchoredPosition = endPos;
            }

            if (deactivateAfter)
            {
                menuObj.SetActive(false);
            }
            
            _activeTransitions.Remove(menuObj);
        }

        private IEnumerator EnableSoundRoutine()
        {
            yield return null; // 1フレーム待機で十分
            CanPlayNavigationSound = true;
        }

                private void Update()
        {
            if (_menuStack.Count > 0)
            {
                var top = _menuStack.Peek();
                if (top.MenuObject == null)
                {
                    _menuStack.Clear();
                    return;
                }
                MaintainSelection();
                HandleCancelInput();
            }
        }
        public void UpdateDescription(string text) { if (descriptionText != null) descriptionText.text = text; }
        public void ClearDescription() { if (descriptionText != null) descriptionText.text = string.Empty; }

        public void UpdatePreviewVideo(VideoClip clip)
        {
            if (previewVideoPlayer != null)
            {
                if (clip != null)
                {
                    if (previewVideoUI != null) previewVideoUI.SetActive(true);
                    previewVideoPlayer.clip = clip;
                    previewVideoPlayer.Play();
                }
                else
                {
                    if (previewVideoUI != null) previewVideoUI.SetActive(false);
                    previewVideoPlayer.Stop();
                    previewVideoPlayer.clip = null;
                }
            }
        }
        
        public void ClearPreviewVideo()
        {
            UpdatePreviewVideo(null);
        }

        public void ShowPreview(GameObject subMenu)
        {
            if (subMenu == null || _previewMenu == subMenu) return;
            HideCurrentPreview();
            _previewMenu = subMenu;
            if (!_previewMenu.activeSelf || !IsInStack(_previewMenu))
            {
                //もし、対象のメニューが閉じるアニメーション中なら強制停止する
                if(_activeTransitions.TryGetValue(_previewMenu, out Coroutine existing) && existing != null)
                {
                    StopCoroutine(existing);
                    _activeTransitions.Remove(_previewMenu);
                }

                //オブジェクトをアクティブにする
                _previewMenu.SetActive(true);
                SetFocusEffect(_previewMenu, focused: false);

                var cg = _previewMenu.GetComponent<CanvasGroup>();
                if(cg != null ) cg.alpha =1f; //透明度をリセット

                var rt = _previewMenu.GetComponent<RectTransform>();

                if(rt != null)
                {
                    rt.anchoredPosition = Vector2.zero; //位置をリセット
                }


            }
        }

        public void HidePreview(GameObject subMenu) { if (_previewMenu == subMenu && _previewMenu != null) HideCurrentPreview(); }

        private void MaintainSelection()
        {
            if (EventSystem.current == null) return;
            var current = EventSystem.current.currentSelectedGameObject;
            if (current != null) _lastSelected = current;
            else if (_lastSelected != null && _lastSelected.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(_lastSelected);
        }

        private void HandleCancelInput()
        {
            if (!IsCancelPressed()) return;
            if (_menuStack.Count == 0) return;
            var top = _menuStack.Peek();
            if (top.MenuObject == null) return;
            var amg = top.MenuObject.GetComponent<AutoMenuGenerator>();
            if (amg == null || amg.allowCancel) CloseMenu();
        }

        private bool IsCancelPressed()
        {
            // if (InputManager.Instance?.PlayerInput != null)
            // {
            //     var action = InputManager.Instance.PlayerInput.actions.FindAction("UI/Cancel") ?? InputManager.Instance.PlayerInput.actions.FindAction("Cancel");
            //     if (action != null && action.WasPressedThisFrame()) return true;
            // }
            return (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                   (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);
        }

        private void FocusFirst(GameObject menuObject)
        {
            if (EventSystem.current == null) return;

            var selectables = menuObject.GetComponentsInChildren<UnityEngine.UI.Selectable>(true);
            foreach (var s in selectables)
            {
                if (s.gameObject.activeInHierarchy && s.interactable)
                {
                    EventSystem.current.SetSelectedGameObject(s.gameObject);

                    if (s.TryGetComponent<GenericMenuItem>(out var item))
                    {
                        if (!string.IsNullOrEmpty(item.descriptionText))
                        {
                            UpdateDescription(item.descriptionText);
                        }

                        if (item.targetSubMenu != null)
                        {
                            ShowPreview(item.targetSubMenu);
                        }
                    }
                    return;
                }
            }
        }

        private void SetFocusEffect(GameObject menu, bool focused)
        {
            var cg = menu.GetComponent<CanvasGroup>();

            if (cg == null)
            {
                cg = menu.AddComponent<CanvasGroup>();
            }

            cg.interactable = focused;
            cg.blocksRaycasts = focused;
            cg.ignoreParentGroups = true;
        }

        private bool IsInStack(GameObject menu)
        {
            foreach (var state in _menuStack) if (state.MenuObject == menu) return true;
            return false;
        }

        private static void MarkRetainedFocus(GameObject target, bool retain)
        {
            if (target != null && target.TryGetComponent<GenericMenuItem>(out var item))
                item.SetRetainedFocus(retain);
        }

        private void RestoreFocus(GameObject previousFocus, GameObject fallbackMenu)
        {
            if (EventSystem.current == null) return;
            if (previousFocus != null && previousFocus.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(previousFocus);
            else if (fallbackMenu != null) FocusFirst(fallbackMenu);
        }

        private void HideCurrentPreview()
        {
            if (_previewMenu == null) return;
            if (!IsInStack(_previewMenu)) _previewMenu.SetActive(false);
            _previewMenu = null;
        }

        // InspectorのUnityEventから１つの引数でメニューを開くためのヘルパー関数
        public void OpenMenuSimple(GameObject newMenu)
        {
            OpenMenu(newMenu, true);  //音を鳴らす場合はtrue、鳴らさないならfalse
        }
    }
}

//