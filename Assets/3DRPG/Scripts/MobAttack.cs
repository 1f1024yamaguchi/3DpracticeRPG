using UnityEngine;
using System.Collections;

//攻撃制御クラス
[RequireComponent(typeof(MobStatus))]

public class MobAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 1.0f; //攻撃後のクールダウン（秒）
    [SerializeField] private Collider attackCollider;
    [SerializeField] private float attackKnockbackPower = 1.5f; //この攻撃のノックバック倍率

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
        var playerStatus  = collider.GetComponent<PlayerStatus>();

        //相手がプレイヤーの場合
        if (playerStatus != null)
        {
            if (playerStatus.IsGuarding)
            {
                Debug.Log("攻撃をガードされた。ダメージ０");
                return;
            }

        


            //攻撃方向を計算(自分から相手へのベクトル)
            Vector3 attackDirection = (collider.transform.position - transform.position).normalized;
            attackDirection.y = 0; //水平方向の吹っ飛ばしにする

            //吹っ飛ばし効果のある、新しいdamageメソッドを呼び出す
            //mobstatusから基本攻撃力を取得
            int attackerPower = _status.AttackPower;

            //最終的な吹っ飛ぶ威力を計算
            // (基本攻撃力 × この技のノックバック倍率)
            float finalKnockbackPower = attackerPower * attackKnockbackPower;

            Debug.Log("★★★ Step 2: 吹っ飛ばし威力計算！ attackerPower=" + attackerPower + ", finalKnockbackPower=" + finalKnockbackPower);

            //ダメージと吹っ飛ぶ威力を相手に伝える
            playerStatus.Damage(attackerPower, attackDirection, finalKnockbackPower);

        }
        else
        {
            var targetMob = collider.GetComponent<MobStatus>();
            if (targetMob == null) return;
            targetMob.Damage(1);
        }


        
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
