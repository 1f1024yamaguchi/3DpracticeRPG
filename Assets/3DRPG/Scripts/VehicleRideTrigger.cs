using UnityEngine;

public class VehicleRideTrigger : MonoBehaviour
{
    private VehiclePath vehiclePath;

    void Start()
    {
        // 同じオブジェクトにある VehiclePath を取得
        vehiclePath = GetComponent<VehiclePath>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            vehiclePath.StartMoving();

            // プレイヤーを子にして一緒に動かす
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            vehiclePath.StopMoving();

            // 子から外して降りる
            other.transform.SetParent(null);
        }
    }
}
