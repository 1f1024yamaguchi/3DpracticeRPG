using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[RequireComponent(typeof(Rigidbody))] // Rigidbodyを必須にする
public class Y_move : MonoBehaviour
{
    private Rigidbody rb;

    public float amplitude = 5.0f; //振れ幅
    public float speed =2.0f; //振動の速さ
    private Vector3 startpos;
    
    
    // Start is called before the first frame update
    void Start()
    {
        startpos = transform.position;
        rb = GetComponent<Rigidbody>();

        rb.interpolation = RigidbodyInterpolation.Interpolate; // Rigidbodyの補間を有効にする、動きを滑らかにする

        rb.isKinematic = true; // Rigidbodyをキネマティックに設定する、物理演算の影響を受けないようにする
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float newY = startpos.y + Mathf.Sin(Time.time * speed) * amplitude;
        Vector3 nextPosition = new Vector3(startpos.x, newY, startpos.z);

        rb.MovePosition(nextPosition); // Rigidbodyを使って位置を更新する
    }
}
