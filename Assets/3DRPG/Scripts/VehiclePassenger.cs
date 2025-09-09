using UnityEngine;

public class VehiclePassenger : MonoBehaviour
{
    private CharacterController characterController;
    private VehiclePath currentVehicle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void LateUpdate()
    {
        if(currentVehicle != null)
        {
            Vector3 delta = currentVehicle.DeltaPosition;
            delta.y =0; //y成分を無視するのでジャンプの邪魔をしない

            if (delta != Vector3.zero) //乗り物が移動しているときだけmove
            {
                characterController.Move(delta);
            }
            
        }
    }

    public void RideVehicle(VehiclePath vehicle)
    {
        currentVehicle = vehicle;
    }

    public void LeaveVehicle()
    {
        currentVehicle = null;
    }


}
