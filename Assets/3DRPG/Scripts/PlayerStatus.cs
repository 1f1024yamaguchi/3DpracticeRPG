using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class PlayerStatus : MobStatus
{
    //[SerializeField] private float knockbackPower = 100f; //吹っ飛ぶ強さ 不要になった
    private Re_PlayerController _playerController; //PlayerControllerを保持
    private Coroutine _attackBuffCoroutine;
    private int _originalAttackPower; //元の攻撃力を保持する変数


    private LevelSystem _levelSystem; //レベル管理への参照

    [SerializeField] private ParticleSystem attackBuffParticles;

    public float DamagePercent { get; private set; } = 0f;

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
        _playerController = GetComponent<Re_PlayerController>();
        _levelSystem = GetComponent<LevelSystem>();
        _originalAttackPower = attackPower; //初期攻撃力を保存

        // MobStatusのStartで送られたHP初期化イベントを上書きする
        TriggerLifeChanged(DamagePercent, 999f); 
    }

    public override void Damage(int damage)
    {
        if (_state == StateEnum.Die || _state == StateEnum.Guard) return;

        //状態をノックバックに変更
        _state = StateEnum.Knockback;
        _animator.SetTrigger("Knockback");

        int finaldamage = (int)Math.Round(damage * UnityEngine.Random.Range(0.85f, 1.15f), MidpointRounding.AwayFromZero);
        DamagePercent += finaldamage;

        TriggerDamageTaken(finaldamage, transform.position);
        TriggerLifeChanged(DamagePercent, 999f); // Max is dummy for UI
    }

    public override void Damage(int damage, Vector3 attackDirection, float baseKnockbackPower)
    {
        Debug.Log("★★★ PlayerStatus.Damageが呼ばれました！現在のステート: " + _state);
        if (_state == StateEnum.Die || _state == StateEnum.Guard) return;

        //状態をノックバックに変更
        _state = StateEnum.Knockback;
        _animator.SetTrigger("Knockback");

        int finaldamage = (int)Math.Round(damage * UnityEngine.Random.Range(0.85f, 1.15f), MidpointRounding.AwayFromZero);
        DamagePercent += finaldamage;
        

        TriggerDamageTaken(finaldamage, transform.position);
        TriggerLifeChanged(DamagePercent, 999f);

        // 吹っ飛ばしの最終的な威力を計算して、PlayerControllerに命令
        //「1 + (DamagePercent / 100)」という緩やかな計算式
        float knockbackMultiplier = 1f + (DamagePercent / 100f);
        float finalKnockbackPower = (baseKnockbackPower / knockbackResistance) * knockbackMultiplier;
        
        _playerController.ApplyKnockback(attackDirection, finalKnockbackPower);
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



    public void HealPercent(int amount)
    {
        if (_state == StateEnum.Die) return;

        DamagePercent = Mathf.Max(0, DamagePercent - amount);
        TriggerLifeChanged(DamagePercent, 999f);
        Debug.Log("HP回復アイテム使用（％ダウン）！現在の％: " + DamagePercent);
    }

    public void UpgradeHP()
    {
        if(_levelSystem.ConsumeSkillPoint())
        {
            lifeMax += 15; //HPの最大値を15上げる
            // Heal(15); スマブラ方式では回復不要か、あるいは％を下げる？
            // ％を下げるように変更
            DamagePercent = Mathf.Max(0, DamagePercent - 15);
            TriggerLifeChanged(DamagePercent, 999f);
            Debug.Log("HPが強化（％ダウン）されました！現在の％: " + DamagePercent);
        }
    }

    public void DowngradeHP()
    {
        if (lifeMax > 100) //最大HPが100より大きい場合のみ弱体化
        {
            lifeMax -= 15; //HPの最大値を15下げる
            DamagePercent += 15; //ペナルティとして％アップ
            TriggerLifeChanged(DamagePercent, 999f);
            _levelSystem.AddSkillPoints(1); //スキルポイントを返却
            Debug.Log("HPが弱体化されました！現在の％: " + DamagePercent);
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
