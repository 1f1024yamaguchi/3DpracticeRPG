using UnityEngine;
using System.Collections;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyStatus : MobStatus
{
    private NavMeshAgent _agent;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if(_state == StateEnum.Attack)
        {
            _animator.SetFloat("MoveSpeed",0);
            if(_agent.isActiveAndEnabled) _agent.isStopped = true;
        }
        else
        {
            if(_agent.isActiveAndEnabled) _agent.isStopped = false;
            _animator.SetFloat("MoveSpeed" , _agent.velocity.magnitude);

        }
                //NavMeshAgentのvelocityで速度のベクトルが取得できる
        //_animator.SetFloat("MoveSpeed" , _agent.velocity.magnitude);
    }
    //親クラスのTakeDamageをオーバーライドして、ダメージを受けたときに揺れエフェクトを再生するようにする


    public override void Damage(int damage)
    {
        base.Damage(damage);
        
        if(TryGetComponent<DamageShakeDOTween>(out var shakeEffect))
        {
            shakeEffect.PlayShakeEffect();
        }
    }

    protected override void OnDie()
    {
        base.OnDie();
        StartCoroutine(DestroyCoroutine());
    }

    //倒されたときの消滅コルーチン
    private IEnumerator DestroyCoroutine()
    {
        //playerオブジェクトからLevelSystemコンポーネントを取得して経験値を加算する
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            LevelSystem lv = player.GetComponent<LevelSystem>();
            if (lv != null)
            {
                lv.AddExp(expPoint); // MobStatusで設定した経験値を送る
            }
        }
        yield return new WaitForSeconds(3);
        
        Destroy(gameObject);
    }


}
