using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DecalShadowControl : MonoBehaviour
{
    private DecalProjector projector;
    public float baseSize = 1.0f; // 基本のサイズ
    public float maxDistance = 10.0f; // 最大距離 
    [SerializeField] private float UpY = 1.5f; // Rayの飛ばす位置を少し高くする変数

    //滑らかさを調整する変数
    [SerializeField] private float smoothTime = 1.0f; // 滑らかさの時間

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        projector = GetComponent<DecalProjector>();
        
    }

    // Update is called once per frame
    void LateUpdate()
    {

        Ray ray = new Ray(transform.parent.position + transform.parent.up * UpY, Vector3.down);

        if(Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            // 1. 位置を地面に固定（子要素なのでローカル座標を調整）
            // hit.pointを使って、投影機の位置が常に地面に張り付くようにする
            //transform.position = hit.point + Vector3.up * 0.5f;

            // 2. 距離(hit.distance)に応じて見た目を変える
            float distRatio = hit.distance / maxDistance; // 0(地面) ～ 1(最大高度)
            Debug.Log($"Distance: {hit.distance}, Ratio: {distRatio}");

            float currentSize = Mathf.Lerp(baseSize, baseSize * 2.0f, distRatio); // 距離に応じてサイズを変える

            //projector.size = new Vector3(currentSize, currentSize, projector.size.z);

            float smoothedSize = Mathf.Lerp(projector.size.x, currentSize, Time.deltaTime * smoothTime); // 滑らかにサイズを変える

            // 3. サイズを更新
            projector.size = new Vector3(smoothedSize, smoothedSize, projector.size.z);


        }
        
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red);
    }
}
