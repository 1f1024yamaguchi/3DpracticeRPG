using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStatus))]


public class EnemyMove : MonoBehaviour
{
    [SerializeField] private LayerMask raycastLayermask; //レイヤーマスク
    private RaycastHit[] _raycastHits = new RaycastHit[10];
    
    private NavMeshAgent _agent;
    private EnemyStatus _status;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>(); //NavMeshAgentを保存しておく
        _status = GetComponent<EnemyStatus>();
    }

    // CollisionDetectorのontriggerstayにセットし、衝突判定を受け取るメソッド
    public void OnDetectObject(Collider collider)
    {
        if (!_status.IsMovable)
        {
            _agent.isStopped = true;
            return;
        }
        //検知したオブジェクトに[Player]のタグがついていれば、そのオブジェクトを追いかける
        if (collider.CompareTag("Player"))
        {
            var positionDiff = collider.transform.position - transform.position; //自信とプレイヤーの座標差分を計算する
            var distance = positionDiff.magnitude; //プレイヤーとの距離を計算する
            var direction = positionDiff.normalized; //プレイヤーへの方向
            //_raycastHitsに、ヒットしたColliderや座標情報などが格納される。RaycastAllとRaycastNonAllocは同等の機能だが、RaycastNonAllocだとメモリにゴミが残らないのでこちらを推奨
            var hitCount = Physics.RaycastNonAlloc(transform.position, direction, _raycastHits, distance, raycastLayermask);
            Debug.Log("hitCount: " + hitCount);
            if (hitCount ==0)
            {
                //本作のプレイヤーはcharactercontrollerを使っていて、colliderは使っていないのでRaycastはヒットしない。つまり、ヒット数が0であればプレイヤーとの間に障害物はないということになる
                _agent.isStopped = false;
                _agent.destination = collider.transform.position;

            }
            else
            {
                //見失ったら停止する
                _agent.isStopped = true;
            }


        }
    }
    
}
