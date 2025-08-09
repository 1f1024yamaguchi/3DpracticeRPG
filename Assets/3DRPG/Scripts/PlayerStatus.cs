using UnityEngine;

public class PlayerStatus : MobStatus
{
    //[SerializeField] private float knockbackPower = 100f; //吹っ飛ぶ強さ 不要になった
    private PlayerController _playerController; //PlayerControllerを保持


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
}
