using UnityEngine;
using UnityEditor;

/// <summary>
/// Player の前方5m先に test_cube を生成し、Stage の子として配置するエディタ拡張。
/// メニュー Tools > 3DRPG > Place Test Cube から実行する。
/// </summary>
public static class PlaceTestCube
{
    [MenuItem("Tools/3DRPG/Place Test Cube")]
    private static void Place()
    {
        // Player をタグで取得
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            EditorUtility.DisplayDialog("Place Test Cube", "\"Player\" タグのオブジェクトが見つかりません。", "OK");
            return;
        }

        // Stage を名前で取得
        GameObject stage = GameObject.Find("Stage");
        if (stage == null)
        {
            EditorUtility.DisplayDialog("Place Test Cube", "\"Stage\" オブジェクトが見つかりません。", "OK");
            return;
        }

        // 既存の test_cube があれば作り直し
        Transform existing = stage.transform.Find("test_cube");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        // 前方5m先の位置を計算
        Vector3 position = player.transform.position + player.transform.forward * 5f;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "test_cube";
        Undo.RegisterCreatedObjectUndo(cube, "Create test_cube");

        cube.transform.SetParent(stage.transform, true);
        cube.transform.position = position;
        cube.transform.rotation = player.transform.rotation;

        Selection.activeGameObject = cube;
        EditorGUIUtility.PingObject(cube);

        // シーンを変更済みとしてマーク
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(cube.scene);

        Debug.Log($"test_cube を {position} に配置しました（Stage の子）。");
    }
}
