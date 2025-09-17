using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class VehiclePath : MonoBehaviour
{
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float returnDelay = 3f;
    private int currentIndex = 0;
    private Vector3 lastPosition;
    public Vector3 DeltaPosition { get; private set; } // ← プレイヤーに伝える用

    private bool isMoving = false;
    private bool isReturning = false;

    void Start()
    {
        lastPosition = transform.position;

        if(waypoints != null && waypoints.Count >0)
        {
            transform.position = waypoints[0].position;
            transform.rotation = waypoints[0].rotation;
            currentIndex =1; //次の目標はwaypoint１
        }
    }

    void Update()
    {
        if(!isMoving) return; //動作onの時だけ動かす
        
        if (waypoints == null || waypoints.Count == 0) return;

        Transform target = waypoints[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        //回転（進行方向を向かわせる）
        Vector3 direction = (target.position - transform.position).normalized;
        if (direction.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
            

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentIndex++;
            if (currentIndex >= waypoints.Count)
            {
                StartCoroutine(ReturnToFirstWaypoint());
            }
            
        }

        //移動量の更新
        if (isMoving && !isReturning)
        {
            DeltaPosition = transform.position - lastPosition;
        }
        else
        {
            DeltaPosition = Vector3.zero;
        }
        
        lastPosition = transform.position;
    }

    private IEnumerator ReturnToFirstWaypoint()
    {
        isReturning = true;
        isMoving = false;
        yield return new WaitForSeconds(returnDelay);

        transform.position = waypoints[0].position;
        transform.rotation = waypoints[0].rotation;

        //リセット
        currentIndex =1;
        lastPosition = transform.position;

        isReturning = false;
        
    }

    public void StartMoving()
    {
        isMoving = true;
    }

    public void StopMoving()
    {
        isMoving = false;
    }

   
}
