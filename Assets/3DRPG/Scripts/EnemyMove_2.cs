using UnityEngine;
using UnityEngine.AI; // NavMeshAgentを使うために必要

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStatus))]

public class EnemyMove_2 : MonoBehaviour
{
    // --- 敵の状態を定義 ---
    private enum State
    {
        Wandering, // 徘徊中
        Chasing,   // 追跡中
    }

    // --- Inspectorで設定する項目 ---
    [Header("索敵設定")]
    [SerializeField] private LayerMask raycastLayermask; // 視線判定に使うレイヤーマスク

    [Header("徘徊設定")]
    [SerializeField] private float wanderRadius = 10f; // 徘徊する半径
    [SerializeField] private float wanderWaitTime = 5f;  // 徘徊先で待つ時間

    // --- 内部で使う変数 ---
    private NavMeshAgent _agent;
    private EnemyStatus _status;
    private State _currentState = State.Wandering; // 初期状態は「徘徊」
    private float _wanderWaitTimer; // 待機時間用のタイマー
    private Transform _playerTarget; // プレイヤーのTransform

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _status = GetComponent<EnemyStatus>();

        // 待機タイマーを初期化
        _wanderWaitTimer = wanderWaitTime;
    }

    private void Update()
    {
        // もし移動不可能な状態（攻撃中など）なら、エージェントを停止して処理を抜ける
        if (!_status.IsMovable)
        {
            _agent.isStopped = true;
            return;
        }
        _agent.isStopped = false;

        // 現在の状態に応じて処理を切り替える
        switch (_currentState)
        {
            // --- 徘徊中の処理 ---
            case State.Wandering:
                // エージェントが目的地にほぼ到着したら
                if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
                {
                    // 待機タイマーを進める
                    _wanderWaitTimer -= Time.deltaTime;

                    // 待機時間が終わったら、次の目的地を探す
                    if (_wanderWaitTimer <= 0)
                    {
                        SetRandomDestination();
                    }
                }
                break;

            // --- 追跡中の処理 ---
            case State.Chasing:
                // プレイヤーを追いかけ続ける
                if (_playerTarget != null)
                {
                    _agent.destination = _playerTarget.position;
                }
                break;
        }
    }

    // ランダムな目的地を設定するメソッド
    private void SetRandomDestination()
    {
        // wanderRadiusの半径内のランダムな位置を計算
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        // NavMesh上でランダムな位置に最も近い地点を探す
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, wanderRadius, NavMesh.AllAreas))
        {
            // 見つかった地点を目的地に設定
            _agent.SetDestination(navHit.position);
        }

        // 次の待機時間をリセット
        _wanderWaitTimer = wanderWaitTime;
    }

    // --- 外部（CollisionDetectorなど）から呼ばれるメソッド ---

    // プレイヤーを発見した時に呼ばれる
    public void OnDetectObject(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            // 状態を「追跡」に切り替える
            _playerTarget = collider.transform;
            _currentState = State.Chasing;
        }
    }

    // プレイヤーを見失った時に呼ばれる
    public void OnLostObject(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            // 状態を「徘徊」に切り替える
            _playerTarget = null;
            _currentState = State.Wandering;
        }
    }
}