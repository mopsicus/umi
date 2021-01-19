// ----------------------------------------------------------------------------
// The MIT License
// UnityMobileInput https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018-2020 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using NiceJson;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace Mopsicus.Plugins {

    /// <summary>
    /// Base class for InputField
    /// </summary>
    public abstract class MobileInputReceiver : MonoBehaviour {

        /// <summary>
        /// Current input id
        /// </summary>
        private int _id;

        /// <summary>
        /// Init input and register interface
        /// </summary>
        protected virtual void Start () {
            _id = MobileInput.Register (this);
        }

        /// <summary>
        /// Action on destroy
        /// </summary>
        protected virtual void OnDestroy () {
            MobileInput.RemoveReceiver (_id);
        }

        /// <summary>
        /// Send data to plugin
        /// </summary>
        /// <param name="data">Data</param>
        protected void Execute (JsonObject data) {
            MobileInput.Execute (_id, data);
        }

        /// <summary>
        /// Send data to plugin manually
        /// </summary>
        /// <param name="data">Data</param>
        public abstract void Send (JsonObject data);

        /// <summary>
        /// Hide input
        /// </summary>
        public abstract void Hide ();
    }

    /// <summary>
    /// Mobile native input plugin
    /// </summary>
    public class MobileInput : MonoBehaviour, IPlugin {

        /// <summary>
        /// Event name for keyboard show/hide
        /// </summary>
        const string KEYBOARD_ACTION = "KEYBOARD_ACTION";

        /// <summary>
        /// Key name for settings save
        /// </summary>
        const string INIT_KEY = "mobileinput_inited";

        /// <summary>
        /// Delegate for show/hide keyboard action
        /// </summary>
        public delegate void ShowDelegate (bool isShow, int height);

        /// <summary>
        /// Handler for ShowDelegate
        /// </summary>
        public static ShowDelegate OnShowKeyboard = delegate { };

        /// <summary>
        /// Mobile fields dictionary
        /// </summary>
        private Dictionary<int, MobileInputReceiver> _inputs = new Dictionary<int, MobileInputReceiver> ();

        /// <summary>
        /// Current instance
        /// </summary>
        private static MobileInput _instance;

        /// <summary>
        /// Cache data for hidden app state
        /// </summary>
        private JsonObject _data;

        /// <summary>
        /// Cache error for hidden app state
        /// </summary>
        private JsonObject _error;

        /// <summary>
        /// MobileInput counter
        /// </summary>
        private int _counter = 0;

#if UNITY_IOS
        /// <summary>
        /// Send data to plugin input
        /// </summary>
        [DllImport ("__Internal")]
        private static extern void inputExecute (int id, string json);

        /// <summary>
        /// Init MobileInput plugin
        /// </summary>
        [DllImport ("__Internal")]
        private static extern void inputInit ();

        /// <summary>
        /// Destroy MobileInput plugin
        /// </summary>
        [DllImport ("__Internal")]
        private static extern void inputDestroy ();
#endif

        /// <summary>
        /// Constructor
        /// </summary>
        private void Awake () {
            if ((object) _instance == null) {
                _instance = GetComponent<MobileInput> ();
                Init ();
            }
        }

        /// <summary>
        /// Plugin name
        /// </summary>
        public string Name {
            get {
                return GetType ().Name.ToLower ();
            }
        }

        /// <summary>
        /// Current instance for external access
        /// </summary>
        public static MobileInput Plugin {
            get {
                return _instance;
            }
        }

        /// <summary>
        /// Callback on data
        /// </summary>
        public void OnData (JsonObject data) {
            Debug.Log (string.Format ("{0} plugin OnData: {1}", GetType ().Name, data.ToJsonPrettyPrintString ()));
            _data = data;
            try {
                JsonObject response = (JsonObject) JsonNode.ParseJsonString (data["data"]);
                string code = response["msg"];
                switch (code) {
                    case KEYBOARD_ACTION:
                        bool isShow = response["show"];
                        int height = 0;
                        height = response["height"];
                        OnShowKeyboard (isShow, height);
                        break;
                    default:
                        int id = response["id"];
                        if (_inputs.ContainsKey (id)) {
                            GetReceiver (id).Send (response);
                        }
                        break;
                }
                _data = null;
            } catch (Exception e) {
                Debug.LogError (string.Format ("{0} plugin OnData error: {1}", GetType ().Name, e.Message));
            }
        }

        /// <summary>
        /// Callback on error
        /// </summary>
        public void OnError (JsonObject data) {
            Debug.LogError (string.Format ("{0} plugin OnError: {0}", GetType ().Name, data.ToJsonPrettyPrintString ()));
            _error = data;
            try {
                _error = null;
            } catch (Exception e) {
                Debug.LogError (string.Format ("{0} plugin OnError error: {1}", GetType ().Name, e.Message));
            }
        }

        /// <summary>
        /// Init and save new MobileInput
        /// </summary>
        /// <param name="receiver">Receiver</param>
        /// <returns>Id</returns>
        public static int Register (MobileInputReceiver receiver) {
            int index = _instance._counter;
            _instance._counter++;
            _instance._inputs[index] = receiver;
            return index;
        }

        /// <summary>
        /// Remove MobileInput
        /// </summary>
        /// <param name="id">Input id</param>
        public static void RemoveReceiver (int id) {
            _instance._inputs.Remove (id);
        }

        /// <summary>
        /// Get MobileInput by index
        /// </summary>
        /// <param name="id">Input id</param>
        /// <returns>Receiver</returns>
        public static MobileInputReceiver GetReceiver (int id) {
            return _instance._inputs[id];
        }

        /// <summary>
        /// Send data to plugin
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="data">json</param>
        public static void Execute (int id, JsonObject data) {
            data["id"] = id;
            string json = data.ToJsonString ();
#if UNITY_EDITOR
            Debug.Log ("MobileInput execute " + json);
#elif UNITY_ANDROID
            using (AndroidJavaClass plugin = new AndroidJavaClass (string.Format (Plugins.ANDROID_CLASS_MASK, _instance.Name))) {
                plugin.CallStatic ("execute", id, json);
            }
#elif UNITY_IOS
            inputExecute (id, json);
#endif
        }

        /// <summary>
        /// Init plugin
        /// </summary>
        public static void Init () {
            int state = PlayerPrefs.GetInt (INIT_KEY, 0);
            if (state == 0) {
                string path = Application.streamingAssetsPath;
                if (Directory.Exists (path)) {
                    string[] files = Directory.GetFiles (path, "*.ttf");
                    foreach (string filePath in files) {
                        PrepareFontsAssets (Path.GetFileName (filePath));
                    }
                }
                PlayerPrefs.SetInt (INIT_KEY, 1);
                PlayerPrefs.Save ();
            }
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
        /// Handler to check data on focus change
        /// </summary>
        private void OnApplicationPause (bool pauseStatus) {
            if (!pauseStatus) {
                if (_data != null) {
                    OnData (_data);
                } else if (_error != null) {
                    OnError (_error);
                }
            }
        }

        /// <summary>
        /// Copy files from StreamingAssets to device path
        /// </summary>
        /// <param name="fileName">File name</param>
        static void PrepareFontsAssets (string fileName) {
            string folder = Application.dataPath;
            string filepath = string.Format ("{0}/{1}", Application.persistentDataPath, fileName);
#if UNITY_EDITOR
            string data = string.Format ("{0}/{1}", Application.streamingAssetsPath, fileName);
            if (File.Exists (filepath)) {
                File.Delete (filepath);
            }
            File.Copy (data, filepath);
#elif UNITY_ANDROID
            using (UnityWebRequest www = UnityWebRequest.Get (string.Format ("jar:file://{0}!/assets/{1}", folder, fileName))) {
                www.SendWebRequest ();
                while (!www.isDone) { }
                File.WriteAllBytes (filepath, www.downloadHandler.data);
            }
#elif UNITY_IOS
            string data = string.Format ("{0}/Raw/{1}", folder, fileName);
            if (File.Exists (filepath)) {
                File.Delete (filepath);
            }
            File.Copy (data, filepath);
#endif
        }

        /// <summary>
        /// Handler on app focus
        /// </summary>
        void OnApplicationFocus (bool focusStatus) {
            if (!focusStatus) {
                foreach (var item in _instance._inputs.Values) {
                    item.Hide ();
                }
            }
        }

    }

}