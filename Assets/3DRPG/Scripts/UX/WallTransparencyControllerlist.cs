using UnityEngine;
using System.Collections.Generic; // リストを使うために必要

public class WallTransparencyControllerlist : MonoBehaviour
{
    [SerializeField] private Transform lookTarget;
    [SerializeField] private float transparentAlpha = 0.4f;
    [SerializeField] private float fadeSpeed = 5.0f;
    [SerializeField] private LayerMask wallLayer; // 壁レイヤーを指定すると確実

    // 現在透過処理の対象になっている壁のリスト
    private List<TransparentWallData> _activeWalls = new List<TransparentWallData>();

    // 壁ごとの状態を管理するためのクラス
    private class TransparentWallData
    {
        public GameObject wall;
        public float currentAlpha = 1.0f;
        public float targetAlpha = 1.0f;
        public bool isHitThisFrame; // 今のフレームでレイが当たったか
    }

    void Update()
    {
        if (lookTarget == null) return;

        Vector3 origin = transform.position;
        Vector3 direction = lookTarget.position - origin;
        float distance = direction.magnitude;

        // 全ての壁を貫通して取得
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, wallLayer);

        // 1. 全ての登録済み壁の「今フレーム当たったフラグ」をリセット
        foreach (var data in _activeWalls) data.isHitThisFrame = false;

        // 2. レイに当たった壁をリストに登録・更新
        foreach (var hit in hits)
        {
            if (hit.transform == lookTarget) continue;

            GameObject obj = hit.transform.gameObject;
            var data = _activeWalls.Find(x => x.wall == obj);

            if (data == null)
            {
                // 新しくリストに追加
                data = new TransparentWallData { wall = obj, currentAlpha = 1.0f };
                _activeWalls.Add(data);
            }
            data.targetAlpha = transparentAlpha;
            data.isHitThisFrame = true;
        }

        // 3. 当たらなかった壁は目標値を1.0に戻す
        foreach (var data in _activeWalls)
        {
            if (!data.isHitThisFrame) data.targetAlpha = 1.0f;
        }

        // 4. リスト内の全オブジェクトのアルファ更新
        UpdateAllWalls();
    }

    void UpdateAllWalls()
    {
        for (int i = _activeWalls.Count - 1; i >= 0; i--)
        {
            var data = _activeWalls[i];
            data.currentAlpha = Mathf.Lerp(data.currentAlpha, data.targetAlpha, Time.deltaTime * fadeSpeed);

            // Renderer取得（LOD対応版のロジックを使用）
            Renderer[] renderers = GetAllRelevantRenderers(data.wall);
            foreach (var r in renderers)
            {
                r.material.SetFloat("_ForceAlpha", data.currentAlpha);
            }

            // 完全に不透明に戻ったらリストから削除
            if (data.currentAlpha >= 0.99f && data.targetAlpha == 1.0f)
            {
                _activeWalls.RemoveAt(i);
            }
        }
    }
    
    // (以前作成した GetAllRelevantRenderers メソッドをここに置く)

    private Renderer[] GetAllRelevantRenderers(GameObject obj)
    {
        LODGroup lodGroup = obj.GetComponentInParent<LODGroup>();
        
        if(lodGroup != null)
        {
            // LODがある場合：そのグループ全体のメッシュを返す
            return lodGroup.GetComponentsInChildren<Renderer>();
        }

        Renderer[] renderes = obj.GetComponentsInChildren<Renderer>();

        if(renderes.Length == 0 && obj.transform.parent != null)
        {
            renderes = obj.transform.parent.GetComponentsInChildren<Renderer>();
        }
        return renderes;
    }

}