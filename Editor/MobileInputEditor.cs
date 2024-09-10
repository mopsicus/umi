using UnityEditor;
using UnityEngine;

namespace UMI {

    /// <summary>
    /// Custom editor for MobileInput
    /// </summary>
    [CustomEditor(typeof(MobileInputField))]
    public class MobileInputEditor : Editor {

        /// <summary>
        /// Labels size
        /// </summary>
        const int LABEL_SIZE = 120;

        /// <summary>
        /// Offset size
        /// </summary>
        const int OFFSET = 20;

        /// <summary>
        /// Space between labels
        /// </summary>
        const int SPACE = 5;

        /// <summary>
        /// MobileInput target
        /// </summary>
        MobileInputField _target;

        /// <summary>
        /// Serialized target object
        /// </summary>
        SerializedObject _object;

        /// <summary>
        /// Return press event
        /// </summary>
        SerializedProperty _onReturnPressedEvent;

        /// <summary>
        /// Init data
        /// </summary>
        void OnEnable() {
            _target = (MobileInputField)target;
            _object = new SerializedObject(target);
            _onReturnPressedEvent = _object.FindProperty("OnReturnPressedEvent");
        }

        /// <summary>
        /// Draw inspector
        /// </summary>
        public override void OnInspectorGUI() {
            _object.Update();
            EditorGUI.BeginChangeCheck();
            GUILayout.Space(OFFSET);
            GUILayout.Label("Select type for Return button:");
            _target.ReturnKey = (MobileInputField.ReturnKeyType)GUILayout.Toolbar((int)_target.ReturnKey, new string[] { "Default", "Next", "Done", "Search" });
            GUILayout.Space(OFFSET);
            GUILayout.Label("Options:");
            GUILayout.Space(SPACE);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Background:");
            _target.BackgroundColor = EditorGUILayout.ColorField(_target.BackgroundColor);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Custom font name:", GUILayout.MaxWidth(LABEL_SIZE));
            _target.CustomFont = GUILayout.TextField(_target.CustomFont);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Keyboard language:", GUILayout.MaxWidth(LABEL_SIZE));
            _target.KeyboardLanguage = GUILayout.TextField(_target.KeyboardLanguage);
            GUILayout.EndHorizontal();            
            GUILayout.Space(SPACE);
            _target.IsManualHideControl = GUILayout.Toggle(_target.IsManualHideControl, " Manual hide control");
            GUILayout.Space(SPACE);
#if UNITY_IOS
            _target.IsWithDoneButton = GUILayout.Toggle(_target.IsWithDoneButton, " Show \"Done\" button");
            GUILayout.Space(SPACE);
            _target.IsWithClearButton = GUILayout.Toggle(_target.IsWithClearButton, " Show \"Clear\" button");
            GUILayout.Space(OFFSET);
#endif
            EditorGUILayout.PropertyField(_onReturnPressedEvent);
            if (EditorGUI.EndChangeCheck()) {
                _object.ApplyModifiedProperties();
            }
        }
    }
}