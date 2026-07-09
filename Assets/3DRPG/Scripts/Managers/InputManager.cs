using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers
{
    /// <summary>
    /// UI入力を抽象化するマネージャー。
    /// New Input System の InputActionAsset を直接参照し、
    /// PlayerInput コンポーネントが存在しないシーンでも動作する。
    /// 
    /// 使い方:
    ///   1. 空のGameObjectに本コンポーネントをアタッチ
    ///   2. Inspector で InputActionAsset (InputSystem_Actions) を設定
    ///   3. 各シーンに配置、または DontDestroyOnLoad で永続化
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Input Action Asset")]
        [Tooltip("InputSystem_Actions アセットをここにドラッグ")]
        [SerializeField] private InputActionAsset inputActionAsset;

        [Header("Action Map / Action Names")]
        [SerializeField] private string uiActionMapName = "UI";
        [SerializeField] private string cancelActionName = "Cancel";

        private InputAction _cancelAction;

        // ── 公開プロパティ（毎フレーム自動リセット） ─────────────────
        /// <summary>キャンセル／Bボタンが押された瞬間のフレームで true</summary>
        public bool ButtonBDown { get; private set; }

        // ── 拡張用: 必要に応じて追加 ──────────────────────────────
        // public bool ButtonADown { get; private set; }
        // public bool ShoulderLDown { get; private set; }
        // public bool ShoulderRDown { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetupActions();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SetupActions()
        {
            if (inputActionAsset == null)
            {
                Debug.LogError("[InputManager] InputActionAsset が設定されていません。");
                return;
            }

            var uiMap = inputActionAsset.FindActionMap(uiActionMapName, throwIfNotFound: false);
            if (uiMap == null)
            {
                Debug.LogError($"[InputManager] ActionMap '{uiActionMapName}' が見つかりません。");
                return;
            }

            _cancelAction = uiMap.FindAction(cancelActionName, throwIfNotFound: false);
            if (_cancelAction == null)
            {
                Debug.LogError($"[InputManager] Action '{cancelActionName}' が見つかりません。");
                return;
            }

            _cancelAction.performed += OnCancelPerformed;
            _cancelAction.Enable();
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            ButtonBDown = true;
        }

        private void LateUpdate()
        {
            // フレーム末でフラグをリセット（1フレームだけ true になる）
            ButtonBDown = false;
        }

        private void OnDestroy()
        {
            if (_cancelAction != null)
            {
                _cancelAction.performed -= OnCancelPerformed;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
