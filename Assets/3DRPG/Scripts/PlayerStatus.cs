using UnityEngine;
using System.Collections;

public class PlayerStatus : MobStatus
{
    //[SerializeField] private float knockbackPower = 100f; //吹っ飛ぶ強さ 不要になった
    private PlayerController _playerController; //PlayerControllerを保持
    private Coroutine attackBuffCoroutine;
    private int originalAttack;

    public bool IsGuardEffective { get; private set;}
    //実際にガードが有効かどうかのフラグ
    



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start(); // MobStatus の Start() を実行
        _playerController = GetComponent<PlayerController>();
        
        
        
    }

    public override void Damage(int damage, Vector3 attackDirection, float baseKnockbackPower)
    {
        Debug.Log("★★★ PlayerStatus.Damageが呼ばれました！現在のステート: " + _state);
        if (_state == StateEnum.Die || _state == StateEnum.Guard) return;

        //状態をノックバックに変更
        _state = StateEnum.Knockback;
        _animator.SetTrigger("Knockback");

        base.Damage(damage);

        //まだ死んでいなければ吹っ飛ばし処理へ
        // 吹っ飛ばしの最終的な威力を計算して、PlayerControllerに命令
        if (_state != StateEnum.Die)
        {
            

            float finalKnockbackPower = baseKnockbackPower / knockbackResistance;
            _playerController.ApplyKnockback(attackDirection, finalKnockbackPower);
        }
    }

    // PlayerEffectManagerから呼び出されるメソッド
    public void ApplyAttackBuff(float duration, int multiplier)
    {
        if(attackBuffCoroutine != null)
        {
            StopCoroutine(attackBuffCoroutine);
            attackPower = originalAttack;
        }
        attackBuffCoroutine = StartCoroutine(AttackBuffCoroutine(duration, multiplier));
    
    }

    //一定時間だけ攻撃力を変更し、元に戻す
    private IEnumerator AttackBuffCoroutine(float duration, int multiplier)
    {
        int originalAttack = attackPower;

        //攻撃力を二倍にする
        attackPower *= multiplier;
        Debug.Log("現在の攻撃力" + attackPower);

        //指定した時間待つ
        yield return new WaitForSeconds(duration);

        //攻撃力を元に戻す
        attackPower = originalAttack;
        Debug.Log("攻撃力アップ終了" + attackPower);
        attackBuffCoroutine = null;
    }

    public void OnGuardStart()
    {
        IsGuardEffective = true;
    }

    public void OnGuardFinished()
    {
        IsGuardEffective = false;
    }

    
}
