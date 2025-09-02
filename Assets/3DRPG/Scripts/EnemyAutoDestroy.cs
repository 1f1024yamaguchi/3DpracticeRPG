using UnityEngine;

public class EnemyAutoDestroy : MonoBehaviour
{
    [SerializeField] private float destroyDistance = 30f;
    private Transform player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform; //プレイヤーをタグで取得
    }

    // Update is called once per frame
    void Update()
    {
        if(player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if(distance > destroyDistance)
        {
            Destroy(gameObject); //一定距離以上なら消す
        }
        
    }
}
