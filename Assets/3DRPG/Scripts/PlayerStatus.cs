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


    private LevelSystem _levelSystem; //レベル管理への参照

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
        _levelSystem = GetComponent<LevelSystem>();
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

    public void UpgradeAttack()
    {
        if (_levelSystem.ConsumeSkillPoint())
        {

            _originalAttackPower += 2; //攻撃力を直接強化
            attackPower = _originalAttackPower; //現在の攻撃力も更新
            Debug.Log("攻撃力が強化されました！現在の攻撃力: " + attackPower);
        }
    }

    public void DowngradeAttack()
    {
        if (attackPower > 9) //攻撃力が10より大きい場合のみ弱体化
        {
            _originalAttackPower -= 2; //攻撃力を直接弱体化（最低1まで）
            attackPower = _originalAttackPower; //現在の攻撃力も更新
            _levelSystem.AddSkillPoints(1); //スキルポイントを返却
            Debug.Log("攻撃力が弱体化されました！現在の攻撃力: " + attackPower);
        }
    }

    public void UpgradeSpeed()
    {
        if(_levelSystem.ConsumeSkillPoint())
        {
            _playerController.AddBaseSpeed(0.2f); //PlayerControllerの基本速度を0.2上げる
        }
    }

    public void DowngradeSpeed()
    {
        if (_playerController.moveSpeed > 2.5f) //基本速度が2.5fより大きい場合のみ弱体化
        {
            _playerController.RemoveBaseSpeed(0.4f); //PlayerControllerの基本速度を0.2下げる
            _levelSystem.AddSkillPoints(1); //スキルポイントを返却
        }
    }



    public void UpgradeHP()
    {
        if(_levelSystem.ConsumeSkillPoint())
        {
            lifeMax += 15; //HPの最大値を15上げる
            Heal(15); //HPを増えた分回復
            Debug.Log("HPが強化されました！現在の最大HP: " + lifeMax);
        }
    }

    public void DowngradeHP()
    {
        if (lifeMax > 100) //最大HPが100より大きい場合のみ弱体化
        {
            lifeMax -= 15; //HPの最大値を15下げる
            Heal(-15); //HPを減らす（弱体化分）
            if (_life > lifeMax) _life = lifeMax; //現在のHPが最大HPを超えないようにする
            _levelSystem.AddSkillPoints(1); //スキルポイントを返却
            Debug.Log("HPが弱体化されました！現在の最大HP: " + lifeMax);
        }

    }

    public void UpgradeJump()
    {
        if(_levelSystem.ConsumeSkillPoint())
        {
            _playerController.AddBaseJump(0.4f); //PlayerControllerのジャンプ力を0.2上げる
        }
    }

    public void DowngradeJump()
    {
        if (_playerController.jumpPower > 4.5f) //ジャンプ力が4.5より大きい場合のみ弱体化
        {
            _playerController.RemoveBaseJump(0.2f); //PlayerControllerのジャンプ力を0.2下げる
            _levelSystem.AddSkillPoints(1); //スキルポイントを返却
        }

    }

    
}
