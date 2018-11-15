// ----------------------------------------------------------------------------
// The MIT License
// UnityMobileInput https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018 Mopsicus <mail@mopsicus.ru>
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
            _target.ReturnKey = (MobileInputField.ReturnKeyType) GUILayout.Toolbar ((int) _target.ReturnKey, new string[] { "Default", "Next", "Done", "Search" });
            GUILayout.Space (OFFSET);
#if UNITY_IOS
            GUILayout.Label ("Options:");
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