using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Spown_enemy : MonoBehaviour
{
   [SerializeField] private PlayerStatus playerStatus;
   [SerializeField] private GameObject enemyPrefab;
   [SerializeField] private GameObject[] obstacles; //障害物の配列
    void Start()
    {
        StartCoroutine(SpawnLoop()); //Coroutineを開始する
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            //距離10のベクトル
            var distanceVector = new Vector3(10,0);
            //プレイヤーの位置をベースにした敵の出現位置
            //Y軸に対して上記ベクトルをランダムに0～360度回転させている。

            var spawnPositionFromPlayer = Quaternion.Euler(0, Random.Range(0,360f), 0) * distanceVector;

            //敵を出現させたい位置を決定する
            var spawnPosition = playerStatus.transform.position + spawnPositionFromPlayer;


            //指定座標から一番近いNavMeshの座標を探す
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(spawnPosition, out navMeshHit, 10, NavMesh.AllAreas))
            {
                //enemyPrefabを複製, NavMeshAgentは必ずNavMesh上に配置する
                Instantiate(enemyPrefab, navMeshHit.position, Quaternion. identity);


            }

            //１０秒待つ
            yield return new WaitForSeconds(10);

            if(playerStatus.Life <= 0)
            {
                //プレイヤーが倒れたらループを抜ける
                break;
            }
        }
    }
}
