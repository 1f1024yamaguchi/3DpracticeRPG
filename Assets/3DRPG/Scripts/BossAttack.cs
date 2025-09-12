using UnityEngine;
using System.Collections;

// 複数の攻撃パターンを管理・実行するクラス
[RequireComponent(typeof(MobStatus))]
public class BossAttack : MonoBehaviour
{
    // 攻撃の種類を定義
    public enum AttackType
    {
        SpinAttack, // 回転切り
        BeamAttack, // ビーム
        SpecialAttack // スペシャルアタック
    }

    [Header("回転切り設定")]
    [SerializeField] private Collider _spinAttackCollider;
    [SerializeField] private float _spinAttackKnockback = 2.0f;

    [Header("ビーム攻撃設定")]
    [SerializeField] private GameObject _beamPrefab; // ビームのプレハブ
    [SerializeField] private Transform _beamLaunchPoint; // ビームの発射口
    [SerializeField] private float _beamAttackKnockback = 1.0f;

    [Header("スペシャルアタック設定")]
    [SerializeField] private GameObject _specialAttackAreaPrefab; // 攻撃範囲を示すエフェクトなど

    [Header("共通設定")]
    [SerializeField] private float _attackCooldown = 2.0f; // 攻撃後の待ち時間

    private MobStatus _status;
    private Animator _animator;

    private void Start()
    {
        _status = GetComponent<MobStatus>();
        _animator = GetComponentInChildren<Animator>();
        
        // 開始時は全ての当たり判定を無効にしておく
        _spinAttackCollider.enabled = false;
    }

    // BossControllerから命令を受けて攻撃を開始する
    public void ExecuteAttack(AttackType type)
    {
        // 攻撃可能状態でなければ何もしない
        if (!_status.IsAttackable) return;

        // 状態を攻撃中に変更
        _status.GoToAttackStateIfPossible();

        // 攻撃の種類に応じて処理を分岐
        switch (type)
        {
            case AttackType.SpinAttack:
                _animator.SetTrigger("SpinAttack");
                break;
            case AttackType.BeamAttack:
                _animator.SetTrigger("BeamAttack");
                break;
            case AttackType.SpecialAttack:
                _animator.SetTrigger("SpecialAttack");
                StartCoroutine(SpecialAttackSequence());
                break;
        }
    }

    // --- 各攻撃の具体的な処理（アニメーションイベントから呼び出す） ---

    // 回転切りの当たり判定を開始
    public void OnSpinAttackStart()
    {
        _spinAttackCollider.enabled = true;
    }

    // 回転切りの当たり判定を終了
    public void OnSpinAttackEnd()
    {
        _spinAttackCollider.enabled = false;
    }
    
    // ビームを発射
    public void OnBeamLaunch()
    {
        if (_beamPrefab != null && _beamLaunchPoint != null)
        {
            Instantiate(_beamPrefab, _beamLaunchPoint.position, _beamLaunchPoint.rotation);
        }
    }

    // スペシャルアタックのシーケンス（予兆→攻撃）
    private IEnumerator SpecialAttackSequence()
    {
        // 例：攻撃範囲の予兆を3秒間表示
        GameObject areaIndicator = Instantiate(_specialAttackAreaPrefab, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(3.0f);
        Destroy(areaIndicator);
        
        // ここで範囲内にいるプレイヤーに大ダメージを与える処理などを書く
        Debug.Log("スペシャルアタック発動！");
    }


    // 攻撃がプレイヤーにヒットした時の処理
    // このメソッドは、各攻撃の当たり判定用Colliderにアタッチした別スクリプトから呼び出すと管理しやすい
    public void OnHitAttack(Collider other, AttackType type)
    {
        var playerStatus = other.GetComponent<PlayerStatus>();
        if (playerStatus == null) return;
        
        int power = _status.AttackPower;
        float knockback = 0;

        // 攻撃タイプに応じてノックバックの大きさを変える
        switch(type)
        {
            case AttackType.SpinAttack:
                knockback = _spinAttackKnockback;
                break;
            case AttackType.BeamAttack:
                knockback = _beamAttackKnockback;
                break;
        }

        Vector3 direction = (other.transform.position - transform.position).normalized;
        playerStatus.Damage(power, direction, power * knockback);
    }


    // 全ての攻撃アニメーションの最後に呼ばれるイベント
    public void OnAttackFinished()
    {
        StartCoroutine(CooldownCoroutine());
    }

    // 攻撃後のクールダウン
    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(_attackCooldown);
        _status.GoToNormalStateIfPossible();
    }
}