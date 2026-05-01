using UnityEngine;

public class DamageTextHandler : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab; //ダメージテキスト
    
    // 直接canvas指定。
    //[SerializeField] private Transform canvasTransform; 

    // Inspectorでセットする必要はありません（プログラムが自動で見つけます）
    private Transform _canvasTransform;
    
    private MobStatus _mobStatus; //MobStatusへの参照

    void Awake()
    {
        _mobStatus = GetComponent<MobStatus>(); //mobstatysへの参照を取得

        //シーン内のcanvasを自動で見つける。
        _canvasTransform = GameObject.Find("Canvas_damage").transform;
    
    }

    void OnEnable()
    {
        _mobStatus.OnDamageTaken += SpawnDamageText; //ダメージを受けたときにSpawnDamageTextを呼び出す
         
    }

    void OnDisable()
    {
        _mobStatus.OnDamageTaken -= SpawnDamageText; //イベントの購読を解除
    }

    // Update is called once per frame
    void SpawnDamageText(int amount, Vector3 worldPos)
    {
        if (damageTextPrefab != null && _canvasTransform != null)
        {
            GameObject obj = Instantiate(damageTextPrefab, _canvasTransform); //ダメージテキストのプレハブを生成
            //obj.GetComponent<DamageText>().Setup(amount, worldPos);

            DamageText dt = obj.GetComponent<DamageText>();
            if (dt != null)
            {
                dt.Setup(amount, worldPos);
            }
        }

    }
}
