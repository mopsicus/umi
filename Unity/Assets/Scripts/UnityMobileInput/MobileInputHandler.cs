// ----------------------------------------------------------------------------
// The MIT License
// LeopotamGroupLibrary https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif
using NiceJson;
using UnityEngine;

namespace Mopsicus.Plugins.MobileInput {

    /// <summary>
    /// Base class for InputField
    /// </summary>
    public abstract class MobileInputReceiver : MonoBehaviour {
        private int inputId;

        protected virtual void Start () {
            inputId = MobileInputHandler.RegisterAndGetReceiverId (this);
        }

        protected virtual void OnDestroy () {
            MobileInputHandler.RemoveReceiver (inputId);
        }

        protected void Execute (JsonObject data) {
            MobileInputHandler.Execute (inputId, data);
        }

        public abstract void Direct (JsonObject data);

        public abstract void Hide ();
    }

    /// <summary>
    /// MobileInput plugin handler
    /// </summary>
    public class MobileInputHandler : MonoBehaviour, IPlugin {

        private const string KEYBOARD_PREPARE = "KEYBOARD_PREPARE";
        private const string KEYBOARD_ACTION = "KEYBOARD_ACTION";

        /// <summary>
        /// delegate for show/hide keyboard
        /// </summary>
        /// <param name="isShow">keyboard visible</param>
        /// <param name="height">keyboard height</param>
        public delegate void ShowDelegate (bool isShow, int height);
        /// <summary>
        /// delegate befor show keyboard
        /// </summary>
        public delegate void PrepareDelegate ();
        /// <summary>
        /// handler for ShowDelegate
        /// </summary>
        public ShowDelegate OnShowKeyboard = delegate { };
        /// <summary>
        /// handler for PrepareDelegate
        /// </summary>
        public PrepareDelegate OnPrepareKeyboard = delegate { };
        /// <summary>
        /// plugin instance
        /// </summary>
        private static MobileInputHandler _instance;
        /// <summary>
        /// mobilefields dictionary
        /// </summary>
        private Dictionary<int, MobileInputReceiver> _inputs = new Dictionary<int, MobileInputReceiver> ();
        /// <summary>
        /// data cache
        /// </summary>
        private JsonObject _data;
        /// <summary>
        /// error cache	
        /// </summary>
        private JsonObject _error;
        /// <summary>
        /// mobileinput counter
        /// </summary>
        private int _inputIdCounter = 0;

#if UNITY_IOS
        [DllImport ("__Internal")]
        private static extern void inputExecute (int inputId, string json);
        [DllImport ("__Internal")]
        private static extern void inputMode (int mode);
        [DllImport ("__Internal")]
        private static extern void inputDestroy ();
        [DllImport ("__Internal")]
        private static extern void inputInit ();
#endif

        /// <summary>
        /// Constructor
        /// </summary>
        private void Awake () {
            if (_instance == null)
                _instance = GetComponent<MobileInputHandler> ();
            Init ();
        }

        private void OnDestroy () {
            Destroy ();
        }

        #region MobileInputReceiver functions

        /// <summary>
        /// Init and save new MobileInput
        /// </summary>
        /// <param name="receiver">receiver</param>
        /// <returns>id field</returns>
        public static int RegisterAndGetReceiverId (MobileInputReceiver receiver) {
            int index = _instance._inputIdCounter;
            _instance._inputIdCounter++;
            _instance._inputs[index] = receiver;
            return index;
        }

        /// <summary>
        /// Remove MobileInput
        /// </summary>
        /// <param name="inputId">id</param>
        public static void RemoveReceiver (int inputId) {
            _instance._inputs.Remove (inputId);
        }

        /// <summary>
        /// Get MobileInput by index
        /// </summary>
        /// <param name="inputId">id</param>
        /// <returns>receiver</returns>
        public static MobileInputReceiver GetReceiver (int inputId) {
            return _instance._inputs[inputId];
        }

        #endregion        

        /// <summary>
        /// Plugin name
        /// </summary>
        public string Name {
            get {
                return "mobileinput";
            }
        }

        /// <summary>
        /// Current instance for external access
        /// </summary>
        /// <returns></returns>
        public static MobileInputHandler Plugin {
            get {
                return _instance;
            }
        }

        /// <summary>
        /// Data receive callback
        /// </summary>
        /// <param name="data">json</param>
        public void OnData (JsonObject data) {
#if DEBUG
            Debug.Log ("MobileInput OnData " + data.ToJsonPrettyPrintString ());
#endif			
            _data = data;
            try {
                JsonObject response = (JsonObject) JsonNode.ParseJsonString (data["data"]);
                string code = response["msg"];
                switch (code) {
                    case KEYBOARD_PREPARE:
                        OnPrepareKeyboard ();
                        break;
                    case KEYBOARD_ACTION:
                        bool isShow = response["show"];
                        int height = 0;
#if UNITY_ANDROID
                        height = (int) (response["height"] * (float) Screen.height);
#elif UNITY_IOS
                        height = response["height"];
#endif
                        OnShowKeyboard (isShow, height);
                        break;
                    default:
                        int inputId = response["id"];
                        if (_inputs.ContainsKey (inputId)) {
                            GetReceiver (inputId).Direct (response);
                        }
                        break;
                }
                _data = null;
            } catch (Exception e) {
#if DEBUG
                Debug.LogError ("MobileInput OnData error:" + e.Message);
#endif	
            }
        }

        /// <summary>
        /// Error receive callback
        /// </summary>
        /// <param name="data">json</param>
        public void OnError (JsonObject data) {
#if DEBUG
            Debug.LogError ("MobileInput OnError " + data.ToJsonPrettyPrintString ());
#endif		
            _error = data;
            try {
                _error = null;
            } catch (Exception e) {
#if DEBUG
                Debug.LogError ("MobileInput OnError error:" + e.Message);
#endif	
            }
        }

        /// <summary>
        /// Send data to plugin
        /// </summary>
        /// <param name="inputId">id</param>
        /// <param name="data">json</param>
        public static void Execute (int inputId, JsonObject data) {
            data["id"] = inputId;
            string json = data.ToJsonString ();
#if UNITY_EDITOR
            Debug.Log ("MobileInput execute " + json);
#elif UNITY_ANDROID
            using (AndroidJavaClass plugin = new AndroidJavaClass (string.Format (Plugins.ANDROID_CLASS_MASK, _instance.Name))) {
                plugin.CallStatic ("execute", inputId, json);
            }
#elif UNITY_IOS
            inputExecute (inputId, json);
#endif
        }

        /// <summary>
        /// Swith mode for iOS
        /// </summary>
        /// <param name="mode">0 or 1</param>
        public static void SwitchMode (int mode) {
#if UNITY_EDITOR
            Debug.Log ("MobileInput mode " + mode);
#elif UNITY_ANDROID
#elif UNITY_IOS
            inputMode (mode);
#endif        
        }

        /// <summary>
        /// Init plugin
        /// </summary>
        public static void Init () {
#if UNITY_EDITOR
#elif UNITY_ANDROID
            using (AndroidJavaClass plugin = new AndroidJavaClass (string.Format (Plugins.ANDROID_CLASS_MASK, _instance.Name))) {
                plugin.CallStatic ("init");
            }
#elif UNITY_IOS
            inputInit ();
#endif
        }

        /// <summary>
        /// Destructor
        /// </summary>
        public static void Destroy () {
#if UNITY_EDITOR
#elif UNITY_ANDROID
            using (AndroidJavaClass plugin = new AndroidJavaClass (string.Format (Plugins.ANDROID_CLASS_MASK, _instance.Name))) {
                plugin.CallStatic ("destroy");
            }
#elif UNITY_IOS
            inputDestroy ();
#endif
        }

        /// <summary>
        /// Handler on app pause
        /// </summary>
        private void OnApplicationPause (bool pauseStatus) {
            if (!pauseStatus) {
                DisableFocus ();
                if (_data != null)
                    OnData (_data);
                else if (_error != null)
                    OnError (_error);
            }
        }

        /// <summary>
        /// Handler on app focus
        /// </summary>
        void OnApplicationFocus (bool focusStatus) {
            if (!focusStatus) {
                DisableFocus ();
            }
        }

        /// <summary>
        /// Disable focus for all input to hide keyboard
        /// </summary>
        void DisableFocus () {
            foreach (var item in _instance._inputs.Values) {
                item.Hide ();
            }
        }

    }

}