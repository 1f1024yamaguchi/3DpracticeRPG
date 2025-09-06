using System.Collections.Generic;
using UnityEngine;

public class VehiclePath : MonoBehaviour
{
    [SerializeField] private List<Transform> waypoints; // 経路点リスト
    [SerializeField] private float speed = 5f;          // 移動スピード
    private int currentIndex = 0;                       // 現在の目的地

    void Update()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        // 次の目的地
        Transform target = waypoints[currentIndex];

        // ターゲット方向へ移動
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // 進行方向を向かせる
        Vector3 direction = (target.position - transform.position).normalized;
        if (direction.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // 目的地に到着したら次のポイントへ
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentIndex++;
            if (currentIndex >= waypoints.Count)
            {
                currentIndex = 0; // ループさせる（ルートを一周）
            }
        }
    }
}
