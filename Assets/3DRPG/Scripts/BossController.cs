using UnityEngine;

// ボスのAI。行動パターンを決定する
[RequireComponent(typeof(BossAttack), typeof(MobStatus))]
public class BossController : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform; // プレイヤーのTransform
    [SerializeField] private float _spinAttackRange = 3.0f; // 回転切りを出す距離
    [SerializeField] private float _beamAttackRange = 10.0f; // ビームを出す距離
    
    private BossAttack _bossAttack;
    private MobStatus _status;
    private bool _isSpecialAttackPhase = false; // SPアタック段階に入ったか

    void Start()
    {
        _bossAttack = GetComponent<BossAttack>();
        _status = GetComponent<MobStatus>();
        // ゲーム開始時にプレイヤーを見つけておく
        if (_playerTransform == null)
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        // ボスが行動可能状態（Normal）でなければ何もしない
        if (_status.State != MobStatus.StateEnum.Normal)
        {
            return;
        }
        
        // プレイヤーがいない場合は何もしない
        if (_playerTransform == null)
        {
            return;
        }

        // --- 行動決定ロジック ---

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        
        // HPが半分以下になったら一度だけスペシャルアタックを発動
        if (!_isSpecialAttackPhase && _status.Life <= _status.LifeMax / 2)
        {
            _isSpecialAttackPhase = true;
            _bossAttack.ExecuteAttack(BossAttack.AttackType.SpecialAttack);
            return; // スペシャルアタック中は他の行動をしない
        }

        // プレイヤーが回転切りの範囲内にいる場合
        if (distanceToPlayer <= _spinAttackRange)
        {
            // プレイヤーの方向を向く
            transform.LookAt(_playerTransform);
            _bossAttack.ExecuteAttack(BossAttack.AttackType.SpinAttack);
        }
        // プレイヤーがビームの範囲内にいる場合
        else if (distanceToPlayer <= _beamAttackRange)
        {
            // プレイヤーの方向を向く
            transform.LookAt(_playerTransform);
            _bossAttack.ExecuteAttack(BossAttack.AttackType.BeamAttack);
        }
        else
        {
            // 範囲外ならプレイヤーを追いかける（NavMeshAgentなどを使うのがおすすめ）
            // ここでは簡易的に何もしない
        }
    }
}