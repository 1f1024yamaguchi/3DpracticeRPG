using UnityEngine;

public class WallClimb : MonoBehaviour
{
    public float climbSpeed =3f; //登る速度
    private Rigidbody rb;
    private bool isClimbing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isClimbing && Input.GetKey(KeyCode.W))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x , climbSpeed, rb.linearVelocity.z);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isClimbing = false;
            rb.useGravity = true; //
        }
    }
}
