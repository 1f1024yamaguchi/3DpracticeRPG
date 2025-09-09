using UnityEngine;

public class VehicleRideTrigger : MonoBehaviour
{
    [SerializeField] private VehiclePath vehicle;



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var passenger = other.GetComponent<VehiclePassenger>();
            if(passenger != null)
            {
                passenger.RideVehicle(vehicle);
                vehicle.StartMoving(); //乗ったら船が動く
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var passenger = other.GetComponent<VehiclePassenger>();
            if(passenger != null)
            {
                passenger.LeaveVehicle();
                vehicle.StopMoving(); //降りたら止める
            }
        }
    }
}
