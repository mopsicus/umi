using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NiceJson;
#if UNITY_ANDROID
using UnityEngine.Networking;
#elif UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace UMI
{

    /// <summary>
    /// Device orientation type
    /// </summary>
    public enum HardwareOrientation
    {
        LANDSCAPE = 0,
        PORTRAIT = 1,
        PORTRAIT_DOWN = 2,
        UNKNOWN = 3,
        FACE_UP = 4,
        FACE_DOWN = 5
    }

    /// <summary>
    /// Base class for InputField
    /// </summary>
    public abstract class MobileInputReceiver : MonoBehaviour
    {

        /// <summary>
        /// Current input id
        /// </summary>
        int _id = 0;

        /// <summary>
        /// Init input and register interface
        /// </summary>
        protected virtual void Start()
        {
#if !UNITY_EDITOR
            _id = MobileInput.Register(this);
#endif
        }

        /// <summary>
        /// Action on destroy
        /// </summary>
        protected virtual void OnDestroy()
        {
#if !UNITY_EDITOR
            MobileInput.RemoveReceiver(_id);
#endif
        }

        /// <summary>
        /// Send data to plugin
        /// </summary>
        /// <param name="data">Data</param>
        protected void Execute(JsonObject data)
        {
#if !UNITY_EDITOR
            MobileInput.Execute(_id, data);
#endif
        }

        /// <summary>
        /// Send data to plugin manually
        /// </summary>
        /// <param name="data">Data</param>
        public abstract void Send(JsonObject data);

        /// <summary>
        /// Hide input
        /// </summary>
        public abstract void Hide();
    }

    /// <summary>
    /// Mobile native input plugin
    /// </summary>
    public class MobileInput : MonoBehaviour
    {

        /// <summary>
        /// Name for object
        /// </summary>
        const string PLUGIN_NAME = "MobileInput";

        /// <summary>
        /// Method receiver name
        /// </summary>
        const string PLUGIN_RECEIVER = "OnDataReceive";

#if UNITY_ANDROID
        /// <summary>
        /// Android plugin package name
        /// </summary>
        const string PLUGIN_PACKAGE = "com.mopsicus.umi.Plugin";
#endif

        /// <summary>
        /// Flag for check font copied to device
        /// </summary>
        const string INIT_KEY = "mobileinput_init";

        /// <summary>
        /// Event name for keyboard show/hide
        /// </summary>
        const string KEYBOARD_ACTION = "KEYBOARD";

        /// <summary>
        /// Event name for orientation change
        /// </summary>
        const string ORIENTATION_ACTION = "ORIENTATION";

        /// <summary>
        /// Landscape
        /// </summary>
        const string LANDSCAPE = "LANDSCAPE";

        /// <summary>
        /// Portrait
        /// </summary>
        const string PORTRAIT = "PORTRAIT";

        /// <summary>
        /// Portrait down
        /// </summary>
        const string PORTRAIT_DOWN = "PORTRAIT_DOWN";

        /// <summary>
        /// Unknown orientation
        /// </summary>
        const string UNKNOWN = "UNKNOWN";

        /// <summary>
        /// iOS up screen
        /// </summary>
        const string FACE_UP = "FACE_UP";

        /// <summary>
        /// iOS down face
        /// </summary>
        const string FACE_DOWN = "FACE_DOWN";

        /// <summary>
        /// Callback for keyboard action
        /// </summary>
        public static Action<bool, int> OnKeyboardAction = delegate { };

        /// <summary>
        /// Callback on screen rotate
        /// </summary>
        public static Action<HardwareOrientation> OnOrientationChange = delegate { };

        /// <summary>
        /// Mobile fields dictionary
        /// </summary>
        readonly Dictionary<int, MobileInputReceiver> _inputs = new Dictionary<int, MobileInputReceiver>();

        /// <summary>
        /// Current instance
        /// </summary>
        static MobileInput _instance = null;

        /// <summary>
        /// Cache data for hidden app state
        /// </summary>
        JsonObject _data = null;

        /// <summary>
        /// MobileInput counter
        /// </summary>
        int _counter = 0;

        /// <summary>
        /// Flag to check init state
        /// </summary>
        static bool _isInited = false;

#if UNITY_IOS
        /// <summary>
        /// Send data to plugin input
        /// </summary>
        [DllImport("__Internal")]
        static extern void inputExecute(int id, string json);

        /// <summary>
        /// Init MobileInput plugin
        /// </summary>
        [DllImport("__Internal")]
        static extern void inputInit(string json);

        /// <summary>
        /// Destroy MobileInput plugin
        /// </summary>
        [DllImport("__Internal")]
        static extern void inputDestroy();

        /// <summary>
        /// Get scale factor
        /// </summary>
        [DllImport("__Internal")]
        static extern float scaleFactor();
#endif

        /// <summary>
        /// Constructor
        /// </summary>
        void Awake()
        {
            if ((object)_instance == null)
            {
                _instance = GetComponent<MobileInput>();
            }
        }

        /// <summary>
        /// Callback on data
        /// </summary>
        public void OnData(JsonObject data)
        {
#if UMI_DEBUG
            Debug.Log($"[UMI] received: {data.ToJsonPrettyPrintString()}");
#endif
            _data = data;
            try
            {
                var response = (JsonObject)JsonNode.ParseJsonString(data["data"]);
                if (response.ContainsKey("action"))
                {
                    string action = response["action"];
                    switch (action)
                    {
                        case KEYBOARD_ACTION:
                            bool isShow = response["show"];
                            int height = response["height"];
                            OnKeyboardAction(isShow, height);
                            return;
                        case ORIENTATION_ACTION:
#if UNITY_ANDROID
                            if (IsRotatationLocked()) {
                                return;
                            }
#endif
                            string orientation = response["orientation"];
                            switch (orientation)
                            {
                                case LANDSCAPE:
                                    OnOrientationChange(HardwareOrientation.LANDSCAPE);
                                    break;
                                case PORTRAIT:
                                    OnOrientationChange(HardwareOrientation.PORTRAIT);
                                    break;
                                case PORTRAIT_DOWN:
                                    OnOrientationChange(HardwareOrientation.PORTRAIT_DOWN);
                                    break;
                                case UNKNOWN:
                                    OnOrientationChange(HardwareOrientation.UNKNOWN);
                                    break;
#if UNITY_IOS
                                case FACE_UP:
                                    OnOrientationChange(HardwareOrientation.FACE_UP);
                                    break;
                                case FACE_DOWN:
                                    OnOrientationChange(HardwareOrientation.FACE_DOWN);
                                    break;
#endif
                                default:
                                    OnOrientationChange(HardwareOrientation.UNKNOWN);
                                    break;
                            }
                            return;
                        default:
                            return;
                    }
                }
                int id = response["id"];
                if (_inputs.ContainsKey(id))
                {
                    GetReceiver(id).Send(response);
                }
                _data = null;
            }
            catch (Exception e)
            {
#if UMI_DEBUG
                Debug.LogError($"[UMI] received error: {e}");
#endif
            }
        }

        /// <summary>
        /// Callback on error
        /// </summary>
        public void OnError(JsonObject data)
        {
#if UMI_DEBUG
            Debug.LogError($"[UMI] error: {data.ToJsonPrettyPrintString()}");
#endif
        }

        /// <summary>
        /// Handler to process data to plugin
        /// </summary>
        /// <param name="data">Raw data</param>
        void OnDataReceive(string data)
        {
#if UMI_DEBUG
            Debug.Log($"[UMI] raw data: {data}");
#endif
            try
            {
                var info = (JsonObject)JsonNode.ParseJsonString(data);
                if (info.ContainsKey("error"))
                {
                    OnError(info);
                }
                else
                {
                    OnData(info);
                }
            }
            catch (Exception e)
            {
#if UMI_DEBUG
                Debug.LogError($"[UMI] raw data error: data = {data}, error = {e}");
#endif
            }
        }

#if UNITY_ANDROID
        /// <summary>
        /// Check if screen rotation locked
        /// </summary>
        bool IsRotatationLocked() {
            using (var plugin = new AndroidJavaClass(PLUGIN_PACKAGE)) {
                return plugin.CallStatic<bool>("checkIsRotateLocked");
            }
        }
#endif

        /// <summary>
        /// Init and save new MobileInput
        /// </summary>
        /// <param name="receiver">Receiver</param>
        /// <returns>Id</returns>
        public static int Register(MobileInputReceiver receiver)
        {
            var index = _instance._counter;
            _instance._counter++;
            _instance._inputs[index] = receiver;
            return index;
        }

        /// <summary>
        /// Remove MobileInput
        /// </summary>
        /// <param name="id">Input id</param>
        public static void RemoveReceiver(int id)
        {
            _instance._inputs.Remove(id);
        }

        /// <summary>
        /// Get MobileInput by index
        /// </summary>
        /// <param name="id">Input id</param>
        /// <returns>Receiver</returns>
        public static MobileInputReceiver GetReceiver(int id)
        {
            return _instance._inputs[id];
        }

        /// <summary>
        /// Send data to plugin
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="data">json</param>
        public static void Execute(int id, JsonObject data)
        {
#if UMI_DEBUG
            Debug.Log($"[UMI] ({id}) execute: {data.ToJsonString()}");
#endif
            data["id"] = id;
            var json = data.ToJsonString();
#if UNITY_ANDROID
            using (var plugin = new AndroidJavaClass(PLUGIN_PACKAGE)) {
                plugin.CallStatic("execute", id, json);
            }
#elif UNITY_IOS
            inputExecute(id, json);
#endif
        }

        /// <summary>
        /// Init plugin
        /// </summary>
        public static void Init()
        {
            if (_isInited)
            {
#if UMI_DEBUG
                Debug.LogError($"[UMI] already inited");
#endif
                return;
            }
#if UMI_DEBUG
            Debug.Log($"[UMI] init");
#endif
            var state = PlayerPrefs.GetInt(INIT_KEY, 0);
            if (state == 0)
            {
                UpdateFonts();
                PlayerPrefs.SetInt(INIT_KEY, 1);
                PlayerPrefs.Save();
            }
            var instance = new GameObject();
            instance.name = PLUGIN_NAME;
            instance.AddComponent<MobileInput>();
            DontDestroyOnLoad(instance);
            var data = new JsonObject();
            data["object"] = PLUGIN_NAME;
            data["receiver"] = PLUGIN_RECEIVER;
            data["debug"] = false;
#if UMI_DEBUG
            data["debug"] = true;
#endif            
#if UNITY_ANDROID
            using (var plugin = new AndroidJavaClass(PLUGIN_PACKAGE)) {
                plugin.CallStatic("init", data.ToJsonString());
            }
#elif UNITY_IOS
            inputInit(data.ToJsonString());
#endif
        }

        /// <summary>
        /// Destructor
        /// </summary>
        public static void Destroy()
        {
#if UMI_DEBUG
            Debug.Log($"[UMI] destroy");
#endif
#if UNITY_ANDROID
            using (var plugin = new AndroidJavaClass(PLUGIN_PACKAGE)) {
                plugin.CallStatic("destroy");
            }
#elif UNITY_IOS
            inputDestroy();
#endif
        }

        /// <summary>
        /// Check screen scale factor (iOS)
        /// </summary>
        public static float GetScreenScale()
        {
#if UNITY_ANDROID
            return 1f;
#elif UNITY_IOS
            return scaleFactor();
#endif
        }

        /// <summary>
        /// Update fonts
        /// </summary>
        public static void UpdateFonts()
        {
            var path = Application.streamingAssetsPath;
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.ttf");
                foreach (var filePath in files)
                {
                    PrepareFontsAsset(Path.GetFileName(filePath));
                }
            }
        }

        /// <summary>
        /// Copy file from StreamingAssets to device path
        /// </summary>
        /// <param name="fileName">File name</param>
        static void PrepareFontsAsset(string fileName)
        {
#if UMI_DEBUG
            Debug.Log($"[UMI] prepare font asset: {fileName}");
#endif
            var folder = Application.dataPath;
            var filepath = $"{Application.persistentDataPath}/{fileName}";
#if UNITY_ANDROID
            using (var www = UnityWebRequest.Get($"jar:file://{folder}!/assets/{fileName}")) {
                www.SendWebRequest();
                while (!www.isDone) { }
                File.WriteAllBytes(filepath, www.downloadHandler.data);
            }
#elif UNITY_IOS
            var data = $"{folder}/Raw/{fileName}";
            if (File.Exists(filepath)) {
                File.Delete(filepath);
            }
            File.Copy(data, filepath);
#endif
        }

        /// <summary>
        /// Handler to check data on focus change
        /// </summary>
        void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                if (_data != null)
                {
                    OnData(_data);
                }
            }
        }
    }
}