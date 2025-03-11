using UnityEngine;

public class RoundLight : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Update()
    {
        transform.Rotate(new Vector3(0,-6) * Time.deltaTime);
    }


}
