using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Video;

namespace UI
{
    // ─────────────────────────────────────────────────────────────────────────
    // メニュー全体の「階層管理」を担うクラス。
    //
    // 役割:
    //   ・メニューをスタック（Stack）で管理し、開く/閉じる（戻る）を制御
    //   ・開閉時のアニメーション（Fade / Slide / Instant / Cascade）
    //   ・選択中項目の説明文・プレビュー動画・サブメニューのプレビュー表示
    //   ・キャンセル入力（Escape / パッドの東ボタン）で1つ前のメニューへ戻る
    //   ・フォーカス（EventSystemの選択状態）の維持と復元
    //
    // 使い方:
    //   Canvas 配下に配置し、firstMenu に最初に開くメニューを設定するだけで
    //   Start() で自動的にメニューが開きます。
    // ─────────────────────────────────────────────────────────────────────────
    public class MenuManager : MonoBehaviour
    {
        // メニュー開閉時のアニメーションの種類
        //   Fade    : 透明度でフェードイン/アウト
        //   Slide   : 左右にスライドして入退場
        //   Instant : アニメーションなしで即切り替え
        //   Cascade : 前のメニューを残したまま重ねて表示（階段状）
        public enum TransitionType { Fade, Slide, Instant, Cascade }

        [Header("Settings")]
        [SerializeField] private TMPro.TextMeshProUGUI descriptionText; // 項目の説明を表示するテキスト
        [SerializeField] private VideoPlayer previewVideoPlayer;        // プレビュー動画の再生プレイヤー
        [SerializeField] private GameObject previewVideoUI;             // 動画を表示するUI（RawImage等）
        [SerializeField] private GameObject firstMenu;                  // 起動時に最初に開くメニュー

        [Header("Animation Settings")]
        [SerializeField] private TransitionType transitionType = TransitionType.Slide;
        [SerializeField] private float transitionDuration = 0.2f; // アニメーションの長さ（秒）
        [SerializeField] private float slideDistance = 1920f;     // Slide時の移動距離（画面幅相当）

        // 開いているメニューの履歴。Peek()が現在のメニュー、Popで1つ前に戻る
        private readonly Stack<MenuState> _menuStack = new Stack<MenuState>();
        private GameObject _previewMenu;   // 現在プレビュー表示中のサブメニュー
        private GameObject _lastSelected;  // フォーカス消失時に復元するための最後の選択項目
        // メニューごとに実行中のアニメーションコルーチンを記録（重複実行の防止用）
        private readonly Dictionary<GameObject, Coroutine> _activeTransitions = new Dictionary<GameObject, Coroutine>();

        // 操作中のみTrueにするフラグ（メニュー遷移直後の誤発音を防ぐ）
        public bool CanPlayNavigationSound { get; private set; } = false;

        // スタックに積む1メニュー分の情報
        private struct MenuState
        {
            public GameObject MenuObject;    // メニュー本体
            public GameObject PreviousFocus; // このメニューを開く直前に選択されていた項目（戻る時に復元）
        }

        private void Awake()
        {
            CanPlayNavigationSound = false;
        }

        private void Start()
        {
            // 最初のメニューを開く（起動時は効果音なし）
            if (firstMenu != null) OpenMenu(firstMenu, false);
        }

        /// <summary>
        /// メニューを開き、スタックに積みます。
        /// 現在のメニューは左へ退場し、新しいメニューが右から入場します。
        /// </summary>
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
                    // 親メニューの選択項目を「フォーカス保持」状態にして見た目を残す
                    MarkRetainedFocus(currentFocus, retain: true);
                    // 親メニューを操作不能にする（CanvasGroupで制御）
                    SetFocusEffect(current.MenuObject, focused: false);

                    // 現在のメニューを左に退場させる
                    TransitionOut(current.MenuObject, -1f);
                }
            }

            // 戻り先のフォーカスと一緒にスタックへ積む
            _menuStack.Push(new MenuState { MenuObject = newMenu, PreviousFocus = currentFocus });

            newMenu.SetActive(true);
            SetFocusEffect(newMenu, focused: true);

            // 新しいメニューを右から入場させる
            TransitionIn(newMenu, 1f);

            // 新メニュー内の最初の項目にフォーカスを合わせる
            FocusFirst(newMenu);

            // フォーカス確定後に音を許可
            StartCoroutine(EnableSoundRoutine());
        }

        /// <summary>
        /// 現在のメニューを閉じてスタックから降ろし、1つ前のメニューへ戻ります。
        /// フォーカスは開く前に選択していた項目へ復元されます。
        /// </summary>
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

            // 親メニュー側の「フォーカス保持」表示を解除
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
                // すべてのメニューを閉じた場合は選択を解除
                EventSystem.current?.SetSelectedGameObject(null);
                CanPlayNavigationSound = false;
            }
        }

        /// <summary>
        /// メニューの「入場」アニメーションを開始します。
        /// directionX: 1=右から入場、-1=左から入場（Slide時のみ意味を持つ）
        /// </summary>
        private void TransitionIn(GameObject menuObj, float directionX)
        {
            // Instant / Cascade はアニメーションなしで即表示
            if (transitionType == TransitionType.Instant || transitionType == TransitionType.Cascade)
            {
                var cg = menuObj.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
                var rt = menuObj.GetComponent<RectTransform>();
                if (transitionType == TransitionType.Instant && rt != null) rt.anchoredPosition = Vector2.zero;
                return;
            }

            // 同じメニューのアニメーションが動いていたら止めてから開始
            if (_activeTransitions.TryGetValue(menuObj, out Coroutine existing) && existing != null)
            {
                StopCoroutine(existing);
            }

            _activeTransitions[menuObj] = StartCoroutine(TransitionCoroutine(menuObj, true, directionX, false));
        }

        /// <summary>
        /// メニューの「退場」アニメーションを開始します。
        /// deactivateAfter: 完了後に SetActive(false) するかどうか。
        /// </summary>
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

        /// <summary>
        /// Fade / Slide の実体となるアニメーションコルーチン。
        /// isEntering: true=入場、false=退場。イーズアウト補間で滑らかに動かします。
        /// </summary>
        private IEnumerator TransitionCoroutine(GameObject menuObj, bool isEntering, float directionX, bool deactivateAfter)
        {
            // CanvasGroup がなければ自動追加（透明度制御に必要）
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
                // 入場: 画面外→中央 / 退場: 中央→画面外
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
                // Time.timeScale の影響を受けないよう unscaledDeltaTime を使用
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

            // 最終状態を確実にセット（補間の誤差対策）
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

        /// <summary>
        /// 1フレーム待ってからナビゲーション音を許可します。
        /// （メニューを開いた直後の自動フォーカスで音が鳴るのを防ぐ）
        /// </summary>
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
                // メニューが破棄されていたらスタックをリセット
                if (top.MenuObject == null)
                {
                    _menuStack.Clear();
                    return;
                }
                MaintainSelection();   // フォーカス消失の自動復元
                HandleCancelInput();   // キャンセル（戻る）入力の監視
            }
        }

        // ── 説明文・プレビュー動画（GenericMenuItem から呼ばれる） ──────────

        public void UpdateDescription(string text) { if (descriptionText != null) descriptionText.text = text; }
        public void ClearDescription() { if (descriptionText != null) descriptionText.text = string.Empty; }

        /// <summary>
        /// プレビュー動画を再生します。clip が null の場合は停止して非表示にします。
        /// </summary>
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

        /// <summary>
        /// サブメニューを「操作せず表示だけ」のプレビュー状態で表示します。
        /// 項目を選択中に、決定前の中身を見せるために使います。
        /// </summary>
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
                SetFocusEffect(_previewMenu, focused: false); // プレビューなので操作は不可

                var cg = _previewMenu.GetComponent<CanvasGroup>();
                if(cg != null ) cg.alpha =1f; //透明度をリセット

                var rt = _previewMenu.GetComponent<RectTransform>();

                if(rt != null)
                {
                    rt.anchoredPosition = Vector2.zero; //位置をリセット
                }


            }
        }

        /// <summary>
        /// 指定のサブメニューがプレビュー中であれば非表示にします。
        /// </summary>
        public void HidePreview(GameObject subMenu) { if (_previewMenu == subMenu && _previewMenu != null) HideCurrentPreview(); }

        /// <summary>
        /// EventSystem の選択が外れた（null になった）場合に、
        /// 最後に選択していた項目へフォーカスを戻します。
        /// マウスクリックで選択が外れても操作不能にならないための保険です。
        /// </summary>
        private void MaintainSelection()
        {
            if (EventSystem.current == null) return;
            var current = EventSystem.current.currentSelectedGameObject;
            if (current != null) _lastSelected = current;
            else if (_lastSelected != null && _lastSelected.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(_lastSelected);
        }

        /// <summary>
        /// キャンセル入力を検知したらメニューを閉じます。
        /// AutoMenuGenerator.allowCancel が false のメニューでは閉じません。
        /// </summary>
        private void HandleCancelInput()
        {
            if (!IsCancelPressed()) return;
            if (_menuStack.Count == 0) return;
            var top = _menuStack.Peek();
            if (top.MenuObject == null) return;
            var amg = top.MenuObject.GetComponent<AutoMenuGenerator>();
            if (amg == null || amg.allowCancel) CloseMenu();
        }

        /// <summary>
        /// キャンセル操作（Escapeキー / パッドの東ボタン=B/○）が押されたかどうか。
        /// </summary>
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

        /// <summary>
        /// メニュー内の最初の操作可能な Selectable にフォーカスを合わせ、
        /// その項目の説明文・サブメニュープレビューも即時反映します。
        /// </summary>
        private void FocusFirst(GameObject menuObject)
        {
            if (EventSystem.current == null) return;

            var selectables = menuObject.GetComponentsInChildren<UnityEngine.UI.Selectable>(true);
            foreach (var s in selectables)
            {
                if (s.gameObject.activeInHierarchy && s.interactable)
                {
                    EventSystem.current.SetSelectedGameObject(s.gameObject);

                    // フォーカス直後は OnSelect が説明文を更新しないケースがあるため手動反映
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

        /// <summary>
        /// CanvasGroup を使ってメニューの操作可否を切り替えます。
        /// focused=false のメニューは表示されていても入力を受け付けません。
        /// </summary>
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

        // 指定メニューが現在スタック（開いている階層）に含まれるか
        private bool IsInStack(GameObject menu)
        {
            foreach (var state in _menuStack) if (state.MenuObject == menu) return true;
            return false;
        }

        /// <summary>
        /// 対象が GenericMenuItem であれば「フォーカス保持」表示を設定/解除します。
        /// </summary>
        private static void MarkRetainedFocus(GameObject target, bool retain)
        {
            if (target != null && target.TryGetComponent<GenericMenuItem>(out var item))
                item.SetRetainedFocus(retain);
        }

        /// <summary>
        /// メニューを閉じた後、以前選択していた項目にフォーカスを戻します。
        /// 復元できない場合は親メニューの先頭項目にフォーカスします。
        /// </summary>
        private void RestoreFocus(GameObject previousFocus, GameObject fallbackMenu)
        {
            if (EventSystem.current == null) return;
            if (previousFocus != null && previousFocus.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(previousFocus);
            else if (fallbackMenu != null) FocusFirst(fallbackMenu);
        }

        /// <summary>
        /// 現在のプレビューを閉じます（正式に開かれているメニューは消しません）。
        /// </summary>
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
