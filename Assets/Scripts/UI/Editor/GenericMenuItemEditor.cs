using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UI; // namespace of GenericMenuItem

namespace UI.Editor
{
    [CustomEditor(typeof(GenericMenuItem))]
    [CanEditMultipleObjects]
    public class GenericMenuItemEditor : SelectableEditor
    {
        private SerializedProperty itemType;
        private SerializedProperty labelText;
        private SerializedProperty descriptionText;
        private SerializedProperty commandInputText;
        private SerializedProperty previewVideo;
        private SerializedProperty targetSubMenu;
        private SerializedProperty showSubMenuAsPreview;
        
        private SerializedProperty disabledTextColor;
        private SerializedProperty disabledSelectedTextColor;
        private SerializedProperty normalTextColor;

        private SerializedProperty cursorObject;
        private SerializedProperty _labelTMPro;
        private SerializedProperty _valueTMPro;
        private SerializedProperty _commandInputTMPro;
        private SerializedProperty OnSubmitEvent;
        private SerializedProperty OnValueChangedEvent;
        private SerializedProperty currentValue;
        private SerializedProperty minValue;
        private SerializedProperty maxValue;
        private SerializedProperty selectorOptions;
        private SerializedProperty inputCooldown;
        private SerializedProperty isPermitted;
        private SerializedProperty playerPrefsKey;
        private SerializedProperty mediaPages;

        protected override void OnEnable()
        {
            base.OnEnable(); // SelectableEditor requirements

            itemType = serializedObject.FindProperty("itemType");
            labelText = serializedObject.FindProperty("labelText");
            descriptionText = serializedObject.FindProperty("descriptionText");
            commandInputText = serializedObject.FindProperty("commandInputText");
            previewVideo = serializedObject.FindProperty("previewVideo");
            targetSubMenu = serializedObject.FindProperty("targetSubMenu");
            showSubMenuAsPreview = serializedObject.FindProperty("showSubMenuAsPreview");
            
            disabledTextColor = serializedObject.FindProperty("disabledTextColor");
            disabledSelectedTextColor = serializedObject.FindProperty("disabledSelectedTextColor");
            normalTextColor = serializedObject.FindProperty("normalTextColor");

            cursorObject = serializedObject.FindProperty("cursorObject");
            _labelTMPro = serializedObject.FindProperty("_labelTMPro");
            _valueTMPro = serializedObject.FindProperty("_valueTMPro");
            _commandInputTMPro = serializedObject.FindProperty("_commandInputTMPro");
            OnSubmitEvent = serializedObject.FindProperty("OnSubmitEvent");
            OnValueChangedEvent = serializedObject.FindProperty("OnValueChangedEvent");
            currentValue = serializedObject.FindProperty("currentValue");
            minValue = serializedObject.FindProperty("minValue");
            maxValue = serializedObject.FindProperty("maxValue");
            selectorOptions = serializedObject.FindProperty("selectorOptions");
            inputCooldown = serializedObject.FindProperty("inputCooldown");
            isPermitted   = serializedObject.FindProperty("isPermitted");
            playerPrefsKey = serializedObject.FindProperty("playerPrefsKey");
            mediaPages = serializedObject.FindProperty("mediaPages");
        }

        public override void OnInspectorGUI()
        {
            // Selectable properties (Interactable, Transition, Navigation)
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(itemType);
            EditorGUILayout.PropertyField(labelText);
            EditorGUILayout.PropertyField(descriptionText);
            EditorGUILayout.PropertyField(commandInputText);
            EditorGUILayout.PropertyField(previewVideo);
            EditorGUILayout.PropertyField(targetSubMenu);
            EditorGUILayout.PropertyField(showSubMenuAsPreview);
            EditorGUILayout.PropertyField(isPermitted);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Disabled Visuals", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(disabledTextColor);
            EditorGUILayout.PropertyField(disabledSelectedTextColor);
            EditorGUILayout.PropertyField(normalTextColor);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(cursorObject);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_labelTMPro);
            EditorGUILayout.PropertyField(_commandInputTMPro);
            
            GenericMenuItem.ItemType type = (GenericMenuItem.ItemType)itemType.enumValueIndex;
            
            if (type != GenericMenuItem.ItemType.Button)
            {
                EditorGUILayout.PropertyField(_valueTMPro);
            }

            EditorGUILayout.PropertyField(OnSubmitEvent);
            if (type != GenericMenuItem.ItemType.Button)
            {
                EditorGUILayout.PropertyField(OnValueChangedEvent);
            }

            if (type != GenericMenuItem.ItemType.Button)
            {
                EditorGUILayout.PropertyField(playerPrefsKey);
                
                if (type == GenericMenuItem.ItemType.Slider)
                {
                    EditorGUILayout.PropertyField(currentValue);
                    EditorGUILayout.PropertyField(minValue);
                    EditorGUILayout.PropertyField(maxValue);
                }
                else if (type == GenericMenuItem.ItemType.Selector)
                {
                    EditorGUILayout.PropertyField(currentValue);
                    EditorGUILayout.PropertyField(selectorOptions);
                }
                else if (type == GenericMenuItem.ItemType.Toggle)
                {
                    EditorGUILayout.PropertyField(currentValue);
                }
                else if (type == GenericMenuItem.ItemType.Carousel)
                {
                    EditorGUILayout.PropertyField(mediaPages, true);
                }
            }

            if (type == GenericMenuItem.ItemType.Slider || type == GenericMenuItem.ItemType.Selector)
            {
                EditorGUILayout.PropertyField(inputCooldown);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
