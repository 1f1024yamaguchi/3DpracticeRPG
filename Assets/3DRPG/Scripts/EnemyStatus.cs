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
        // FindGameObjectWithTag は子・孫オブジェクト(例: Tunic)を返すことがあるため
        // GetComponentInParent で親方向を辿って LevelSystem を取得する
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Debug.Log($"[EXP] player found: {player!= null}");
        Debug.Log($"[EXP] player name: {player?.name}, path: {(player != null ? GetFullPath(player.transform) : "N/A")}");
        Debug.Log($"[EXP] lv found: {player.GetComponent<LevelSystem>()!= null}");
        if (player != null)
        {
            //LevelSystem lv = player.GetComponent<LevelSystem>();
            LevelSystem lv = player.GetComponentInParent<LevelSystem>();
            if (lv != null)
            {
                Debug.Log($"[EXP] add {expPoint}");
                lv.AddExp(expPoint); // MobStatusで設定した経験値を送る
            }
            else
            {
                Debug.LogWarning($"[EXP] LevelSystem not found. player path: {GetFullPath(player.transform)}");
            }
        }
        yield return new WaitForSeconds(3);
        
        Destroy(gameObject);
    }

    private string GetFullPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }


}
