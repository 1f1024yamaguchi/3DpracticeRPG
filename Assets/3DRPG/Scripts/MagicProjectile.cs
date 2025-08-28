using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 20f; // 弾の速度
    [SerializeField] private float lifeTime = 3f; // 弾が爆発するまでの時間

    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 3f;    // 爆発の範囲
    [SerializeField] private int explosionDamage = 10;      // 爆発のダメージ
    [SerializeField] private float explosionPower = 20f;    // 爆発の吹っ飛ばす力
    [SerializeField] private GameObject explosionVFX;       // 爆発時に再生するエフェクトのプレハブ

    private bool _hasExploded = false; // 爆発が複数回起こるのを防ぐフラグ

    void Start()
    {
        // lifeTime秒後に、Explodeメソッドを呼び出す
        Invoke("Explode", lifeTime);
    }

    void Update()
    {
        // 自分の前方にまっすぐ進み続ける
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // 何かにトリガーが触れた時の処理
    private void OnTriggerEnter(Collider other)
    {
        // 爆発処理を呼び出す
        Explode();
    }

    // 爆発処理の本体
    public void Explode()
    {
        // すでに爆発済みなら、何もしない
        if (_hasExploded) return;
        _hasExploded = true;

        // 1. 爆発エフェクトを生成する
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }

        // 2. 自身の位置を中心に、指定した半径内の全てのColliderを探す
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        // 3. 見つかった全てのColliderに対して、ダメージと吹っ飛ばし処理を行う
        foreach (var hitCollider in colliders)
        {
            // MobStatusを持っているオブジェクト（プレイヤーや敵）を探す
            MobStatus mobStatus = hitCollider.GetComponent<MobStatus>();
            if (mobStatus != null)
            {
                // 4. 吹っ飛ばしの方向を計算（爆心地から相手へのベクトル）
                Vector3 knockbackDirection = (hitCollider.transform.position - transform.position).normalized;
                
                // 5. 吹っ飛ばし情報付きのDamageメソッドを呼び出す
                mobStatus.Damage(explosionDamage, knockbackDirection, explosionPower);
            }
        }

        // 6. 魔法弾自身を消滅させる
        Destroy(gameObject);
    }
}