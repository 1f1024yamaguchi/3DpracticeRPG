using UnityEngine;
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform platform; //動かす床
    [SerializeField] private float moveHeight = 3f; //上に動く高さ
    [SerializeField] private float moveSpeed = 2f; //動くスピード
    [SerializeField] private float stayTime = 0.3f; //


    private Vector3 startPos; //元の位置
    private Vector3 targetPos; //上昇後の位置
    private bool playerOn = false; //猶予込み判定
    
    private float exitTimer=0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    void Start()
    {
        startPos = platform.position;
        targetPos = startPos + Vector3.up * moveHeight;
    }
    // Update is called once per frame
    void Update()
    {
        Debug.Log(playerOn);
        //プレイヤーが載っていない時はタイマーを減らす
        if(!playerOn && exitTimer > 0f)
        {
            exitTimer -= Time.deltaTime;
            if(exitTimer <= 0f)
            {
                playerOn = false; //本当に下りた判定
            }
            else
            {
                playerOn = true; //猶予がある
            }
        }
        
        if(playerOn)
        {
            platform.position = Vector3.MoveTowards(platform.position, targetPos, moveSpeed * Time.deltaTime);
        }
        else//プレイヤーがいなければ元の場所に
        {
            platform.position = Vector3.MoveTowards(platform.position, startPos, moveSpeed * Time.deltaTime);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit: " + collision.gameObject.name); 
        if(collision.gameObject.CompareTag("Player"))
        {
            playerOn = true;
            exitTimer = 0f;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            
            exitTimer = stayTime; //一定時間はまだ乗っている扱い
        }
    }
}
