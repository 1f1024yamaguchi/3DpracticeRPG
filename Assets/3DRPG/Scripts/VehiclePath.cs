using System.Collections.Generic;
using UnityEngine;

public class VehiclePath : MonoBehaviour
{
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private float speed = 5f;
    private int currentIndex = 0;
    private bool isMoving = false;

    void Update()
    {
        if (!isMoving) return;
        if (waypoints == null || waypoints.Count == 0) return;

        Transform target = waypoints[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        Vector3 direction = (target.position - transform.position).normalized;
        if (direction.magnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(direction);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentIndex++;
            if (currentIndex >= waypoints.Count)
                currentIndex = 0; // ループ
        }
    }

    public void StartMoving() => isMoving = true;
    public void StopMoving() => isMoving = false;
}
