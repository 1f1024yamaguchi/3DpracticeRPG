using UnityEngine;
using System.Collections;

//攻撃制御クラス
[RequireComponent(typeof(MobStatus))]

public class MobAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 0.5f; //攻撃後のクールダウン（秒）
    [SerializeField] private Collider attackCollider;

    private MobStatus _status;

    private void Start()
    {
        _status = GetComponent<MobStatus>();
        if (_status == null)
        {
            Debug.LogError("MobStatusコンポーネントがアタッチされていません！", this);
        }
    }

    //攻撃可能な状態であれば攻撃を行う
    public void AttackIfPossible()
    {
        if (_status == null)
        {
            Debug.LogError("MobStatus が null です！ MobStatus が正しくアタッチされているか確認してください。", this);
            return;
        }
        if (!_status.IsAttackable) return;
        //ステータスと衝突したオブジェクトで攻撃可否を判断する

        _status.GoToAttackStateIfPossible();
    }

    //攻撃対象が攻撃範囲に入った時に呼ばれる    
    public void OnAttackRangeEnter(Collider collider)
    {
        AttackIfPossible();
    }

    //攻撃開始時に呼ばれる
    public void OnAttackStart()
    {
        attackCollider.enabled = true;

    }

    //attackColliderが攻撃対象にHitしたときに呼ばれる
    public void OnHitAttack(Collider collider)
    {
        var targetMob = collider.GetComponent<MobStatus>();
        if (null == targetMob) return;

        //プレイヤーにダメージを与える
        targetMob.Damage(1);
    }
    
    //攻撃終了時に呼ばれる
    public void OnAttackFinished()
    {
        attackCollider.enabled= false;
        StartCoroutine(CooldownCoroutine());
    }

    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        _status.GoToNormalStateIfPossible();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
