using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class PlayerStatus : MobStatus
{
    //[SerializeField] private float knockbackPower = 100f; //吹っ飛ぶ強さ 不要になった
    private PlayerController _playerController; //PlayerControllerを保持
    private Coroutine _attackBuffCoroutine;
    private int _originalAttackPower; //元の攻撃力を保持する変数
    [SerializeField] private ParticleSystem attackBuffParticles;

    public bool IsGuardEffective { get; private set;}

    //実際にガードが有効かどうかのフラグ

    protected override void OnDie()
    {
        base.OnDie();
        StartCoroutine(GoToGameOverCoroutine());
    }
    
    private IEnumerator GoToGameOverCoroutine()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("GameOverScene");
    }
    
    


    



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start(); // MobStatus の Start() を実行
        _playerController = GetComponent<PlayerController>();
        _originalAttackPower = attackPower; //初期攻撃力を保存

           
        
        
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
    public void BuffAttackPower(int multiplier)
    {
        attackPower = _originalAttackPower * multiplier;
        Debug.Log("攻撃力アップ！ 現在の攻撃力: " + attackPower);
    }

    public void ResetAttackPower()
    {
        //攻撃力を元に戻す
        attackPower = _originalAttackPower;
        Debug.Log("攻撃力アップ効果終了。現在の攻撃力: " + attackPower);
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
