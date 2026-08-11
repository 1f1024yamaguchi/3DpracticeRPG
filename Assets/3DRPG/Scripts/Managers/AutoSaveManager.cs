using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pogostuck方式オートセーブマネージャー。
///
/// 設計方針（TOP_ROAD_roadmap.md 準拠）:
///   - 固定チェックポイントは置かず、「今どこにいるか」を一定間隔で同じキーに上書き保存し続ける
///   - 落下時にセーブシステムは何もしない。落ちた後の位置が次の保存周期でそのまま上書きされる
///   - 起動時にセーブがあればその座標へ配置して再開、無ければシーンの初期配置のまま
///
/// 保存先は PlayerPrefs（JSON文字列1件）。
///   - Windows/Steam: レジストリ
///   - WebGL/unityroom: IndexedDB
///   - Mac/Linux/モバイル: plist/xml
/// と全プラットフォームでUnityが吸収してくれるため、プラットフォーム分岐は不要。
///
/// WebGL対策:
///   - OnApplicationQuit はWebGLでは呼ばれないことがあるため、
///     フォーカス喪失/ポーズ時にも必ずディスクへ確定書き込み(Flush)する
///   - PlayerPrefs.Save() はWebGLでIndexedDB同期が走るため、
///     毎回ではなく flushInterval 間隔に間引いて負荷を抑える
///
/// 使い方: Mainシーンの空GameObjectにアタッチするだけ（参照は自動検索）。
/// </summary>
public class AutoSaveManager : MonoBehaviour
{
    // キー名を変えれば旧データと縁を切れる（データ構造を変えたら版数を上げる）
    private const string SaveKey = "TOPROAD_AUTOSAVE_V2";
    // 「初めから」で完全初期化するために削除する旧キー群
    private const string LegacySaveKeyV1 = "TOPROAD_AUTOSAVE_V1";
    private const string OwnedItemsKey = "OWNED_ITEMS_DATA";
    private static readonly string[] LegacyLevelKeys = { "PlayerLevel", "PlayerExp", "NextExp", "SkillPoints" };

    [Serializable]
    private class SaveData
    {
        public int version = 2;
        public string sceneName;
        public float posX, posY, posZ;
        public float rotY;
        public float elapsedTime;
        public float damagePercent; // スマブラ形式の被ダメージ％

        // 進行度（LevelSystem）
        public int level = 1;
        public int exp = 0;
        public int expToNext = 50;
        public int skillPoints = 0;

        // ステータス割り振り結果
        public int baseAttackPower = 2;
        public float maxLife = 100f;
        public float moveSpeed = 3f;
        public float jumpPower = 3f;
    }

    [Header("参照（未設定ならシーンから自動検索）")]
    [SerializeField] private Re_PlayerController player;
    [SerializeField] private TimerController timerController;

    [Header("保存設定")]
    [Tooltip("現在地を上書き保存する間隔（秒）")]
    [SerializeField] private float saveInterval = 0.5f;

    [Tooltip("この距離以上動いたら間隔を待たずに即保存（0で無効）")]
    [SerializeField] private float saveDistanceThreshold = 2f;

    [Tooltip("PlayerPrefsをディスクへ確定書き込みする間隔（秒）。WebGLのIndexedDB同期負荷対策")]
    [SerializeField] private float flushInterval = 5f;

    private PlayerStatus _playerStatus;
    private LevelSystem _levelSystem;
    private CharacterController _characterController;
    private Transform _playerTransform;

    private float _saveTimer;
    private float _flushTimer;
    private Vector3 _lastSavedPos;
    private bool _dirty;        // SetString済みだが未Flushのデータがあるか
    private bool _initialized;

    // ------------------------------------------------------------
    // 外部API（タイトル画面の「はじめから」などから呼ぶ）
    // ------------------------------------------------------------

    /// <summary>セーブデータが存在するか（タイトルの「つづきから」表示判定用）</summary>
    public static bool HasSave() => PlayerPrefs.HasKey(SaveKey);

    /// <summary>セーブデータを削除する（「はじめから」を選んだ時に呼ぶこと）</summary>
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.DeleteKey(LegacySaveKeyV1);
        PlayerPrefs.DeleteKey(OwnedItemsKey);
        foreach (var k in LegacyLevelKeys) PlayerPrefs.DeleteKey(k);
        PlayerPrefs.Save();
        Debug.Log("[AutoSave] セーブデータを削除しました（位置・レベル・ステータス・所持アイテムを初期化）");
    }

    // ------------------------------------------------------------
    // 初期化・ロード
    // ------------------------------------------------------------

    private void Start()
    {
        StartCoroutine(InitializeCoroutine());
    }

    private IEnumerator InitializeCoroutine()
    {
        // プレイヤー側の Start()（Animator取得等）が終わるのを1フレーム待つ
        yield return null;

        if (player == null) player = FindFirstObjectByType<Re_PlayerController>();
        if (timerController == null) timerController = FindFirstObjectByType<TimerController>();

        if (player == null)
        {
            Debug.LogError("[AutoSave] Re_PlayerController がシーンに見つかりません。オートセーブは無効です。");
            yield break;
        }

        _playerTransform = player.transform;
        _playerStatus = player.GetComponent<PlayerStatus>();
        _levelSystem = player.GetComponent<LevelSystem>();
        _characterController = player.GetComponent<CharacterController>();
        _lastSavedPos = _playerTransform.position;

        TryLoad();
        _initialized = true;
    }

    private void TryLoad()
    {
        if (!HasSave()) return;

        SaveData data = null;
        try
        {
            data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[AutoSave] セーブデータの解析に失敗したため初期位置から開始します: " + e.Message);
        }
        if (data == null) return;

        // 進行度・ステータス割り振り・被ダメージ％はシーンに依存しないので常に復元する
        if (_levelSystem != null)
            _levelSystem.RestoreProgress(data.level, data.exp, data.expToNext, data.skillPoints);
        if (_playerStatus != null)
        {
            _playerStatus.RestoreStats(data.baseAttackPower, data.maxLife);
            _playerStatus.RestoreDamagePercent(data.damagePercent);
        }
        player.moveSpeed = data.moveSpeed;
        player.jumpPower = data.jumpPower;

        // 位置・回転・タイマーは「同じシーンのセーブ」の時だけ適用する
        if (data.sceneName != SceneManager.GetActiveScene().name)
        {
            Debug.Log($"[AutoSave] 位置は別シーン'{data.sceneName}'のセーブのため未適用。進行度・ステータスのみ復元しました");
            _lastSavedPos = _playerTransform.position;
            return;
        }

        // CharacterControllerは有効のままtransformを書き換えると内部位置とズレるため、
        // 必ず無効化 → テレポート → 再有効化 の順で行う
        bool ccWasEnabled = _characterController != null && _characterController.enabled;
        if (_characterController != null) _characterController.enabled = false;

        _playerTransform.position = new Vector3(data.posX, data.posY, data.posZ);
        _playerTransform.rotation = Quaternion.Euler(0f, data.rotY, 0f);

        if (_characterController != null) _characterController.enabled = ccWasEnabled;

        if (timerController != null) timerController.SetTime(data.elapsedTime);

        _lastSavedPos = _playerTransform.position;
        Debug.Log($"[AutoSave] ロード完了: pos=({data.posX:F1}, {data.posY:F1}, {data.posZ:F1}) " +
                  $"time={data.elapsedTime:F1}s percent={data.damagePercent:F0}% " +
                  $"Lv{data.level} SP{data.skillPoints} atk{data.baseAttackPower} hp{data.maxLife:F0} spd{data.moveSpeed:F1} jmp{data.jumpPower:F1}");
    }

    // ------------------------------------------------------------
    // 常時上書き保存ループ
    // ------------------------------------------------------------

    private void Update()
    {
        if (!_initialized || player == null) return;

        // 死亡中は保存しない（死亡地点＋高%で詰み保存になるのを防ぐ）。
        // 落下は死亡ではないので通常どおり保存され続ける＝Pogostuck仕様
        if (_playerStatus != null && _playerStatus.State == MobStatus.StateEnum.Die) return;

        _saveTimer += Time.unscaledDeltaTime;

        bool movedFar = saveDistanceThreshold > 0f &&
                        (_playerTransform.position - _lastSavedPos).sqrMagnitude >=
                        saveDistanceThreshold * saveDistanceThreshold;

        if (_saveTimer >= saveInterval || movedFar)
        {
            _saveTimer = 0f;
            WriteSave();
        }

        // ディスク確定書き込みは間引く
        if (_dirty)
        {
            _flushTimer += Time.unscaledDeltaTime;
            if (_flushTimer >= flushInterval) Flush();
        }
    }

    private void WriteSave()
    {
        var pos = _playerTransform.position;
        int lv = 1, exp = 0, next = 50, sp = 0;
        if (_levelSystem != null) _levelSystem.GetProgress(out lv, out exp, out next, out sp);

        var data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            posX = pos.x,
            posY = pos.y,
            posZ = pos.z,
            rotY = _playerTransform.eulerAngles.y,
            elapsedTime = timerController != null ? timerController.CurrentTime : 0f,
            damagePercent = _playerStatus != null ? _playerStatus.DamagePercent : 0f,

            level = lv,
            exp = exp,
            expToNext = next,
            skillPoints = sp,

            baseAttackPower = _playerStatus != null ? _playerStatus.BaseAttackPower : 2,
            maxLife = _playerStatus != null ? _playerStatus.MaxLife : 100f,
            moveSpeed = player.moveSpeed,
            jumpPower = player.jumpPower,
        };

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        _lastSavedPos = pos;
        _dirty = true;
    }

    private void Flush()
    {
        PlayerPrefs.Save();
        _dirty = false;
        _flushTimer = 0f;
    }

    // ------------------------------------------------------------
    // アプリ終了・中断時の取りこぼし防止
    // ------------------------------------------------------------

    // WebGLではタブを閉じてもOnApplicationQuitが呼ばれないことがあるため、
    // フォーカス喪失・ポーズの時点で必ず最新状態を書いてFlushしておく
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SaveNowAndFlush();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveNowAndFlush();
    }

    private void OnApplicationQuit()
    {
        SaveNowAndFlush();
    }

    private void OnDestroy()
    {
        if (_dirty) Flush();
    }

    private void SaveNowAndFlush()
    {
        if (!_initialized || player == null) return;
        if (_playerStatus != null && _playerStatus.State == MobStatus.StateEnum.Die) return;
        WriteSave();
        Flush();
    }
}
