using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UI;

/// <summary>
/// タイトル画面のセーブ関連フロー制御。
///
/// 役割:
///   ・「初めから」: セーブがあれば確認ダイアログ → はいで DeleteSave してから本編へ。
///                    セーブが無ければ確認不要でそのまま本編へ。
///   ・「続きから」: セーブがあればそのまま本編へ（AutoSaveManager が自動ロード）。
///                    セーブが無ければグレーアウトして選択不可。
///   ・確認ダイアログ「はい/いいえ」の配線。
///
/// 設計:
///   既存の GenericMenuItem は Inspector の OnSubmit で LoadSceneFromEditor("Main") を
///   直接呼んでいる。ここでは Start() 時に対象項目を labelText で探し、
///   OnSubmitEvent をこのクラスのメソッドへ差し替える（＝コード側で確実に制御）。
///   参照を Inspector で明示的に割り当てた場合はそちらを優先する。
///
/// 使い方:
///   タイトルシーンの任意の常駐 GameObject（Menu_mather 等）にアタッチするだけ。
///   確認ダイアログを使う場合は confirmNewGameMenu（はい/いいえ を持つメニュー）を割り当てる。
/// </summary>
public class TitleMenuController : MonoBehaviour
{
    [Header("遷移先シーン")]
    [SerializeField] private string gameSceneName = "Main";

    [Header("項目ラベル（この名前で自動検出します）")]
    [SerializeField] private string newGameLabel = "初めから";
    [SerializeField] private string continueLabel = "続きから";
    [SerializeField] private string confirmYesLabel = "はい";
    [SerializeField] private string confirmNoLabel = "いいえ";

    [Header("参照（未設定ならラベルで自動検出）")]
    [SerializeField] private GenericMenuItem newGameItem;
    [SerializeField] private GenericMenuItem continueItem;
    [SerializeField] private MenuManager menuManager;

    [Tooltip("セーブがある状態で『初めから』を押した時に開く『はい/いいえ』確認メニュー。未設定なら確認なしで新規開始します。")]
    [SerializeField] private GameObject confirmNewGameMenu;
    [SerializeField] private GenericMenuItem confirmYesItem;
    [SerializeField] private GenericMenuItem confirmNoItem;

    private void Start()
    {
        AutoResolveReferences();
        WireButtons();
        RefreshContinueAvailability();
    }

    /// <summary>参照が未設定なら labelText で自動的に拾う。</summary>
    private void AutoResolveReferences()
    {
        if (menuManager == null) menuManager = FindFirstObjectByType<MenuManager>();

        bool needScan = newGameItem == null || continueItem == null || confirmYesItem == null || confirmNoItem == null;
        if (needScan)
        {
            var items = FindObjectsByType<GenericMenuItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var it in items)
            {
                if (it == null) continue;
                if (newGameItem == null && it.labelText == newGameLabel) newGameItem = it;
                else if (continueItem == null && it.labelText == continueLabel) continueItem = it;
                else if (confirmYesItem == null && it.labelText == confirmYesLabel) confirmYesItem = it;
                else if (confirmNoItem == null && it.labelText == confirmNoLabel) confirmNoItem = it;
            }
        }

        // 確認メニュー本体が未設定なら「はい」項目の親メニューから推定
        if (confirmNewGameMenu == null && confirmYesItem != null)
        {
            var gen = confirmYesItem.GetComponentInParent<AutoMenuGenerator>(true);
            if (gen != null) confirmNewGameMenu = gen.gameObject;
        }
    }

    /// <summary>各項目の OnSubmit をこのクラスのメソッドへ差し替える。</summary>
    private void WireButtons()
    {
        Rewire(newGameItem, OnNewGamePressed);
        Rewire(continueItem, OnContinuePressed);
        Rewire(confirmYesItem, OnConfirmNewGameYes);
        Rewire(confirmNoItem, OnConfirmNewGameNo);
    }

    private static void Rewire(GenericMenuItem item, UnityAction action)
    {
        if (item == null) return;
        item.OnSubmitEvent = new UnityEvent();
        item.OnSubmitEvent.AddListener(action);
        item.targetSubMenu = null; // サブメニュー遷移は自前で制御する
    }

    /// <summary>セーブの有無で「続きから」の可否を更新（無ければグレーアウト）。</summary>
    public void RefreshContinueAvailability()
    {
        if (continueItem == null) return;
        bool hasSave = AutoSaveManager.HasSave();
        continueItem.interactable = hasSave; // 非interactableで自動的にグレー表示＆選択不可
        continueItem.isPermitted = hasSave;
    }

    // ── 各項目の処理 ────────────────────────────────────────────

    public void OnNewGamePressed()
    {
        if (AutoSaveManager.HasSave() && confirmNewGameMenu != null && menuManager != null)
        {
            // セーブがある → 上書き確認ダイアログを開く
            menuManager.OpenMenu(confirmNewGameMenu, true);
        }
        else
        {
            // セーブが無い（または確認メニュー未設定）→ そのまま新規開始
            StartNewGame();
        }
    }

    public void OnContinuePressed()
    {
        if (!AutoSaveManager.HasSave()) return; // 保険（グレーアウト中は本来呼ばれない）
        LoadGameScene();
    }

    public void OnConfirmNewGameYes()
    {
        StartNewGame();
    }

    public void OnConfirmNewGameNo()
    {
        if (menuManager != null) menuManager.CloseMenu();
    }

    // ── 共通処理 ────────────────────────────────────────────────

    private void StartNewGame()
    {
        AutoSaveManager.DeleteSave();
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        if (LoadingScreenManager.Instance != null)
            LoadingScreenManager.Instance.LoadScene(gameSceneName, true);
        else
            SceneManager.LoadScene(gameSceneName);
    }
}
