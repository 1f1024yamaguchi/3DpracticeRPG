using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SpecialDashSkill : MonoBehaviour
{
    [Header("変則急襲斬り: 突進設定")]
    [SerializeField] private float dashSpeed = 15f;           // 突進速度
    [SerializeField] private float initialUpwardForce = 5f;   // 最初の跳ね上がり力
    [SerializeField] private float dashDuration = 1.0f;       // 敵に当たらなかった場合の終了時間

    [Header("回転モデル設定")]
    [SerializeField] private Transform spinPivot;
    [SerializeField] private float spinSpeed = 1500f;

    // 依存コンポーネント
    private CharacterController _characterController;
    private Animator _animator;
    private PlayerStatus _playerStatus; // MobStatusの派生クラス

    // 内部状態
    private float _timer = 0f;
    private Vector3 _dashVelocity;

    // テスト用の独立したInput（後でPlayerInputに統合推奨）
    private InputAction _testFireAction;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _playerStatus = GetComponent<PlayerStatus>();

        // テスト用：Tキーを押したら発動するように設定
        _testFireAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/t");
        _testFireAction.performed += ctx => TryStartSpecialDash();
        _testFireAction.Enable();
    }

    void OnDestroy()
    {
        // Start() 前に破棄される場合があるため null ガード
        if (_testFireAction != null)
        {
            _testFireAction.Disable();
            _testFireAction.Dispose();
            _testFireAction = null;
        }
    }

    void Update()
    {
        // プレイヤーの現在の状態を取得（MobStatusの _state がprotectedなら、判定用プロパティを作るか、
        // 今回はシンプルに Update 内の実行フラグで管理します）
        
        // Timerが動いている＝突進中として処理
        if (_timer > 0f)
        {
            ExecuteDash();
        }
    }

    /// <summary>
    /// 発動条件を満たしていれば突進を開始する
    /// </summary>
    private void TryStartSpecialDash()
    {
        // Normal状態（動ける状態）の時のみ発動可能
        // ※PlayerStatus側に public bool IsNormalState => _state == StateEnum.Normal; のようなプロパティがあると綺麗です。
        // 今回は簡易的にメソッドで遷移を試みます。

        _playerStatus.GoToSpecialDashStateIfPossible();

        // 状態が切り替わったかを判定（厳密には上記でフラグチェック推奨）
        StartDash();
    }

    /// <summary>
    /// 突進の初期化処理
    /// </summary>
    private void StartDash()
    {
        _timer = dashDuration;

        // プレイヤーの正面方向へ突進
        Vector3 forwardDir = transform.forward;
        
        // 突進のベクトルを計算（前方に高速＋上方に少し跳ねる）
        _dashVelocity = forwardDir * dashSpeed;
        _dashVelocity.y = initialUpwardForce;

        // アニメーションを再生
        // Animatorに "SpecialAttackTrigger" というTriggerパラメータを追加しておいてください
        _animator.SetTrigger("SpecialAttackTrigger");

        Debug.Log("変則急襲斬り：突進フェーズ開始！");
    }

    /// <summary>
    /// 突進中の移動処理（毎フレーム呼ばれる）
    /// </summary>
    private void ExecuteDash()
    {
        _timer -= Time.deltaTime;

        // キャラクターを移動（重力は意図的に無視し、一直線に飛ばす）
        _characterController.Move(_dashVelocity * Time.deltaTime);

        spinPivot.Rotate(Vector3.right * spinSpeed * Time.deltaTime, Space.Self);

        // 敵に当たらず時間が切れたら終了
        if (_timer <= 0f)
        {
            EndDash();
        }
    }

    /// <summary>
    /// 突進の終了（通常状態へ戻る）
    /// </summary>
    private void EndDash()
    {
        _timer = 0f;
        _dashVelocity = Vector3.zero;
        spinPivot.localRotation = Quaternion.identity;

        // PlayerStatusを通常状態に戻す
        _playerStatus.GoToNormalStateIfPossible();
        
        Debug.Log("変則急襲斬り：不発終了");
    }
}