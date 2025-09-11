using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator2 : MonoBehaviour
{
    private bool EVflag;
    private float floar;
    // Start is called before the first frame update
    void Start()
    {
        floar = 1f;
        EVflag = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < 300f && floar == 1f && EVflag == true)
        {
            transform.Translate(0, 0.1f, 0);
            
        }
      

        if (transform.position.y > 139.66f && floar == 2f && EVflag == true)
        {
            transform.Translate(0, -0.1f, 0);
        }

      


    }
    private void OnTriggerEnter(Collider collision)//エレベーターの中に入ったら
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            floar = 1f;
            EVflag = true;
        }
        
       

    }
    private void OnTriggerExit(Collider collision)//エレベーターから出たら
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            // プレイヤーが降りたら必ず1Fに戻る
            floar = 2f; // 「今は2階にいる」と扱って
            EVflag = true; // 自動で下降開始
        }

    }
}