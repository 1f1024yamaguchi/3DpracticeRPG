using UnityEditor;
using UnityEngine;
using UI;

namespace UI.Editor
{
    // AutoMenuGenerator の Inspector にメニュー生成・クリアボタンを追加する
    [CustomEditor(typeof(AutoMenuGenerator))]
    public class AutoMenuGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var generator = (AutoMenuGenerator)target;

            GUILayout.Space(12);
            EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "メニュー項目は Editor で手動生成します。\n" +
                "「メニューを生成」後にシーンを保存してください。",
                MessageType.Info);

            GUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("メニューを生成", GUILayout.Height(28)))
                {
                    generator.GenerateMenu();
                    MarkSceneDirty(generator);
                }

                if (GUILayout.Button("クリア", GUILayout.Height(28), GUILayout.Width(70)))
                {
                    generator.ClearMenu();
                    MarkSceneDirty(generator);
                }
            }
        }

        private static void MarkSceneDirty(Component target)
        {
            if (!Application.isPlaying)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(target.gameObject.scene);
        }
    }
}
