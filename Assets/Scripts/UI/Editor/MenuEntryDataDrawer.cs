using UnityEditor;
using UnityEngine;
using UI;

namespace UI.Editor
{
    [CustomPropertyDrawer(typeof(MenuEntryData))]
    public class MenuEntryDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 背景を描画するために少しマージンを取る
            Rect fieldRect = new Rect(position.x, position.y + 2, position.width, EditorGUIUtility.singleLineHeight);

            // 折りたたみ（Foldout）
            property.isExpanded = EditorGUI.Foldout(fieldRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                SerializedProperty itemName = property.FindPropertyRelative("itemName");
                SerializedProperty type = property.FindPropertyRelative("type");
                SerializedProperty description = property.FindPropertyRelative("description");
                SerializedProperty targetSubMenu = property.FindPropertyRelative("targetSubMenu");

                SerializedProperty initialValue = property.FindPropertyRelative("initialValue");
                SerializedProperty minValue = property.FindPropertyRelative("minValue");
                SerializedProperty maxValue = property.FindPropertyRelative("maxValue");
                SerializedProperty selectorOptions = property.FindPropertyRelative("selectorOptions");
                SerializedProperty playerPrefsKey = property.FindPropertyRelative("playerPrefsKey");
                SerializedProperty mediaPages = property.FindPropertyRelative("mediaPages");

                SerializedProperty onSubmit = property.FindPropertyRelative("OnSubmit");
                SerializedProperty onValueChanged = property.FindPropertyRelative("OnValueChanged");
                SerializedProperty isPermitted = property.FindPropertyRelative("isPermitted");

                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(fieldRect, itemName);

                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(fieldRect, type);

                float descHeight = EditorGUI.GetPropertyHeight(description);
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                fieldRect.height = descHeight;
                EditorGUI.PropertyField(fieldRect, description);
                fieldRect.height = EditorGUIUtility.singleLineHeight;
                fieldRect.y += descHeight - EditorGUIUtility.singleLineHeight; // adjust y for the next item

                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(fieldRect, targetSubMenu);

                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(fieldRect, isPermitted);

                GenericMenuItem.ItemType t = (GenericMenuItem.ItemType)type.enumValueIndex;

                if (t != GenericMenuItem.ItemType.Button)
                {
                    fieldRect.y += EditorGUIUtility.singleLineHeight + (EditorGUIUtility.standardVerticalSpacing * 3);
                    EditorGUI.LabelField(fieldRect, "Values (" + t.ToString() + " Config)", EditorStyles.boldLabel);
                    fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(fieldRect, playerPrefsKey);

                    if (t == GenericMenuItem.ItemType.Slider)
                    {
                        fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(fieldRect, initialValue, new GUIContent("Initial Value"));
                        fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(fieldRect, minValue);
                        fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(fieldRect, maxValue);
                    }
                    else if (t == GenericMenuItem.ItemType.Selector)
                    {
                        fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(fieldRect, initialValue, new GUIContent("Initial Selection Index"));
                        fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        float optionsHeight = EditorGUI.GetPropertyHeight(selectorOptions);
                        fieldRect.height = optionsHeight;
                        EditorGUI.PropertyField(fieldRect, selectorOptions, true);
                        fieldRect.height = EditorGUIUtility.singleLineHeight;
                        fieldRect.y += optionsHeight - EditorGUIUtility.singleLineHeight;
                    }
                    else if (t == GenericMenuItem.ItemType.Toggle)
                    {
                        fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(fieldRect, initialValue, new GUIContent("Initial Value (0=OFF, 1=ON)"));
                    }
                    else if (t == GenericMenuItem.ItemType.Carousel)
                    {
                        fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        float pagesHeight = EditorGUI.GetPropertyHeight(mediaPages, true);
                        fieldRect.height = pagesHeight;
                        EditorGUI.PropertyField(fieldRect, mediaPages, true);
                        fieldRect.height = EditorGUIUtility.singleLineHeight;
                        fieldRect.y += pagesHeight - EditorGUIUtility.singleLineHeight;
                    }
                }

                fieldRect.y += EditorGUIUtility.singleLineHeight + (EditorGUIUtility.standardVerticalSpacing * 3);
                EditorGUI.LabelField(fieldRect, "Events", EditorStyles.boldLabel);
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                float submitHeight = EditorGUI.GetPropertyHeight(onSubmit);
                fieldRect.height = submitHeight;
                EditorGUI.PropertyField(fieldRect, onSubmit);
                fieldRect.height = EditorGUIUtility.singleLineHeight;
                fieldRect.y += submitHeight - EditorGUIUtility.singleLineHeight;

                if (t != GenericMenuItem.ItemType.Button)
                {
                    fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    float valueChangedHeight = EditorGUI.GetPropertyHeight(onValueChanged);
                    fieldRect.height = valueChangedHeight;
                    EditorGUI.PropertyField(fieldRect, onValueChanged);
                    fieldRect.height = EditorGUIUtility.singleLineHeight;
                    fieldRect.y += valueChangedHeight - EditorGUIUtility.singleLineHeight;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight + 4;

            SerializedProperty type = property.FindPropertyRelative("type");
            GenericMenuItem.ItemType t = (GenericMenuItem.ItemType)type.enumValueIndex;

            float height = EditorGUIUtility.singleLineHeight + 4; // Foldout offset
            height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2; // itemName, type
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("description")) + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // targetSubMenu
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // isPermitted

            if (t != GenericMenuItem.ItemType.Button)
            {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Header
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                if (t == GenericMenuItem.ItemType.Slider)
                {
                    height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3;
                }
                else if (t == GenericMenuItem.ItemType.Selector)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("selectorOptions")) + EditorGUIUtility.standardVerticalSpacing;
                }
                else if (t == GenericMenuItem.ItemType.Toggle)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                else if (t == GenericMenuItem.ItemType.Carousel)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("mediaPages"), true) + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Events header
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OnSubmit")) + EditorGUIUtility.standardVerticalSpacing;

            if (t != GenericMenuItem.ItemType.Button)
            {
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OnValueChanged")) + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }
    }
}
