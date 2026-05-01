using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{

    [SerializeField] private GameObject enemyPrefab; //出現させる敵のプレハブ
    [SerializeField] private float respawnDistance=20.0f; //プレイヤーがこれより離れたら復活
    [SerializeField] private float checkInterval=2.0f; //距離チェックの頻度

    private Vector3 _originPosition; //最初に置かれた位置を記憶
    private GameObject _spawnedEnemy; //現在出現している敵の参照
    private Transform _playerTransform; //プレイヤーのTransformへの参照

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _originPosition = transform.position;
    }

    void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform; //プレイヤーのTransformを取得
        Spawn(); //最初の敵を出現させる
        
    }

    // Update is called once per frame
    void Update()
    {

        if(_spawnedEnemy == null) //敵がいない場合
        {
            float distanceToPlayer = Vector3.Distance(_originPosition, _playerTransform.position);
            if(distanceToPlayer > respawnDistance) //プレイヤーが一定距離以上離れているか
            {
                Spawn(); //敵を出現させる
            }
        }
        
    }

    private void Spawn()
    {
        _spawnedEnemy = Instantiate(enemyPrefab, _originPosition, transform.rotation); //敵を出現させる
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_originPosition, Vector3.one); //出現位置の可視化
    }
}
