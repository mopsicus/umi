// ----------------------------------------------------------------------------
// The MIT License
// UnityMobileInput https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018-2020 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Mopsicus.Plugins {

    /// <summary>
    /// Custom editor for MobileInput
    /// </summary>
    [CustomEditor (typeof (MobileInputField))]
    public class MobileInputEditor : Editor {

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
        private MobileInputField _target;

        /// <summary>
        /// Serialized target object
        /// </summary>
        private SerializedObject _object;

        /// <summary>
        /// Set custom font
        /// </summary>
        private SerializedProperty _customFont;

        /// <summary>
        /// Is manual hide control
        /// </summary>
        private SerializedProperty _isManualHideControl;

        /// <summary>
        /// Return press event
        /// </summary>
        private SerializedProperty _onReturnPressedEvent;

#if UNITY_IOS
        /// <summary>
        /// Visible Done button
        /// </summary>
        private SerializedProperty _isWithDoneButton;

        /// <summary>
        /// Visible Clear buttonƒ
        /// </summary>
        private SerializedProperty _isWithClearButton;
#endif

        /// <summary>
        /// Init data
        /// </summary>
        private void OnEnable () {
            _target = (MobileInputField) target;
            _object = new SerializedObject (target);
            _customFont = _object.FindProperty ("CustomFont");
            _isManualHideControl = _object.FindProperty ("IsManualHideControl");
            _onReturnPressedEvent = _object.FindProperty ("OnReturnPressedEvent");
#if UNITY_IOS
            _isWithDoneButton = _object.FindProperty ("IsWithDoneButton");
            _isWithClearButton = _object.FindProperty ("IsWithClearButton");
#endif
        }

        /// <summary>
        /// Draw inspector
        /// </summary>
        public override void OnInspectorGUI () {
            _object.Update ();
            EditorGUI.BeginChangeCheck ();
            GUILayout.Space (OFFSET);
            GUILayout.Label ("Select type for Return button:");
            _target.ReturnKey = (MobileInputField.ReturnKeyType) GUILayout.Toolbar ((int) _target.ReturnKey, new string[] { "Default", "Next", "Done", "Search", "Send" });
            GUILayout.Space (OFFSET);
            GUILayout.Label ("Options:");
            GUILayout.Space (SPACE);
            GUILayout.BeginHorizontal ();
            GUILayout.Label ("Custom font name:", GUILayout.MaxWidth (120));
            _target.CustomFont = GUILayout.TextField (_target.CustomFont);
            GUILayout.EndHorizontal ();
            GUILayout.Space (SPACE);
            _target.IsManualHideControl = GUILayout.Toggle (_target.IsManualHideControl, " Manual hide control");
            GUILayout.Space (SPACE);
#if UNITY_IOS
            _target.IsWithDoneButton = GUILayout.Toggle (_target.IsWithDoneButton, " Show \"Done\" button");
            GUILayout.Space (SPACE);
            _target.IsWithClearButton = GUILayout.Toggle (_target.IsWithClearButton, " Show \"Clear\" button");
            GUILayout.Space (OFFSET);
#endif
            EditorGUILayout.PropertyField (_onReturnPressedEvent);
            if (EditorGUI.EndChangeCheck ()) {
                _object.ApplyModifiedProperties ();
            }
        }

    }
}