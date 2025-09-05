using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Y_move : MonoBehaviour
{
    public float speed = 50.0f;
    private Vector3 targetpos;
    
    // Start is called before the first frame update
    void Start()
    {
        targetpos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(targetpos.x ,targetpos.y+Mathf.Sin(Time.time) * speed,targetpos.z);
    }
}
