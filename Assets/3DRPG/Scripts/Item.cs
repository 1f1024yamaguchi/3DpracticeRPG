using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class Item : MonoBehaviour
{

    public enum ItemType
    {
        //アイテムの種類定義
        SpeedUp,
        Attack_Power,
        Orb_of_Protecton
    }

    [SerializeField] private ItemType type;
    
    public void Initialize()
    {
        //アニメーションが終わるまでclliderを無効化する
        var colliderCache = GetComponent<Collider>();
        colliderCache.enabled = false;

        //出現アニメーション
        var transformCache = transform;
        var dropPosition = transform.localPosition + new Vector3(Random.Range(-1f,1f), 0, Random.Range(-1f,1f));

        transformCache.DOLocalMove(dropPosition, 0.5f);
        var defaultScale = transformCache.localScale;
        transformCache.localScale = Vector3.zero;
        transformCache.DOScale(defaultScale,0.5f).SetEase(Ease.OutBounce)
        .OnComplete(() =>
        {
            //アニメーションが終わったらcolliderを有効化する    
            colliderCache.enabled =true;
        });


    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        //TODOプレイヤーの所持品として追加する

        //オブジェクトを破壊する
        Destroy(gameObject);
    }
}


