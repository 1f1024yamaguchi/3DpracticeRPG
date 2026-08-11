using UnityEditor;
using UnityEngine;

namespace UI.MultiMedia
{
    // ─────────────────────────────────────────────────────────────────────────
    // MultiMediaGenerator の Inspector に「生成」「クリア」ボタンを追加する
    // Editor 拡張。ボタン押下後は SetDirty で変更をシーンに保存対象として
    // マークします。
    // ─────────────────────────────────────────────────────────────────────────
    [CustomEditor(typeof(MultiMediaGenerator))]
    public class MultiMediaGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MultiMediaGenerator generator = (MultiMediaGenerator)target;

            GUILayout.Space(10);
            if (GUILayout.Button("Generate Menu (メニューを生成)"))
            {
                generator.GenerateMenu();
                EditorUtility.SetDirty(generator);
            }

            if (GUILayout.Button("Clear Menu (メニューをクリア)"))
            {
                generator.ClearMenu();
                EditorUtility.SetDirty(generator);
            }
        }
    }
}
