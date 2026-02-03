using UnityEngine;

public class WallTransparencyController : MonoBehaviour
{

    [SerializeField] private Transform lookTarget; // プレイヤーのTransformをインスペクターで設定
    [SerializeField] private float transparentAlpha = 0.4f; // 透明にする際のAlpha値
    [SerializeField] private float fadeSpeed = 5.0f; // 透けるときの速さ
    


    private GameObject _lastHitWall;   // 現在透けさせている（または戻している最中の）壁
    private float _targetAlpha = 1.0f; //目標とするForceAlphaの値
    private float _currentAlpha = 1.0f; //現在のForceAlphaの値
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame
    void Update()
    {
        
        if (lookTarget == null) return;
        
         //カメラからプレイヤーへの方向と距離を計算
        Vector3 origin = transform.position; //カメラ位置
        Vector3 direction = lookTarget.position - origin; //ターゲットへの方向
        // direction は lookTarget.position - transform.position で計算したもの
        Debug.DrawRay(transform.position, direction, Color.red);
        float distance = direction.magnitude; //ターゲットまでの距離

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, distance))
        {

            GameObject hitObj = hit.transform.gameObject;
            Debug.Log("レイが当たったオブジェクト: " + hitObj.name);

            if(hit.transform  == lookTarget)
            {
                _targetAlpha = 1.0f;
            }
            else
            {
                            // 前の壁と違うものに当たったら、前の壁を即座に不透明に戻す
                if(_lastHitWall != null && _lastHitWall != hit.transform.gameObject )
                {
                    ResetWallImmediately(_lastHitWall);
                    _currentAlpha = 1.0f; // アルファ計算をリセット
                }
                _lastHitWall = hit.transform.gameObject;
                _targetAlpha = transparentAlpha;

                }
        }
        else
        {
            _targetAlpha = 1.0f;
        }
        


        UpdateWallAlpha();
        
        
    }

    void UpdateWallAlpha()
    {
        if(_lastHitWall == null) return;
        

        _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, Time.deltaTime * fadeSpeed);

        // 【修正ポイント】親子・兄弟関係をすべて網羅するRenderer取得

        Renderer[]  renderers = GetAllRelevantRenderers(_lastHitWall);




        foreach(var renderer in renderers)
        {
            

            renderer.material.SetFloat("_ForceAlpha", _currentAlpha);

        }

        //完全に不透明に入ったら管理から外す
        if (_currentAlpha >= 0.99f && _targetAlpha == 1.0f)
        {
            ResetWallImmediately(_lastHitWall);
            _lastHitWall = null;
        } 
    }

    private Renderer[] GetAllRelevantRenderers(GameObject obj)
    {

        LODGroup lodGroup = obj.GetComponentInParent<LODGroup>();

        
        if(lodGroup != null)
        {
            // LODがある場合：そのグループ全体のメッシュを返す
            return lodGroup.GetComponentsInChildren<Renderer>();
        }

        Renderer[] renderes = obj.GetComponentsInChildren<Renderer>();

        if(renderes.Length ==0 && obj.transform.parent != null)
        {
            renderes = obj.transform.parent.GetComponentsInChildren<Renderer>();
        }
        return renderes;
    }

    // 壁が急に切り替わった時、前の壁を強制的に不透明にする
    void ResetWallImmediately(GameObject obj)
    {
        if(obj == null) return;
        Renderer[] renderers = GetAllRelevantRenderers(obj);


        foreach(var r in renderers)
        {
            r.material.SetFloat("_ForceAlpha", 1.0f);
        }
    }
}
