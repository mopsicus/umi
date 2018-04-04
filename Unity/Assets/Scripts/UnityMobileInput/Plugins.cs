// ----------------------------------------------------------------------------
// The MIT License
// LeopotamGroupLibrary https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------
using System;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif
using Mopsicus.Plugins.MobileInput;
using NiceJson;
using UnityEngine;

namespace Mopsicus.Plugins {

	/// <summary>
	/// plugin interface
	/// </summary>
	public interface IPlugin {
		/// <summary>
		/// plugin name
		/// </summary>
		string Name { get; }
		/// <summary>
		/// data receive callback
		/// </summary>
		/// <param name="data">json</param>
		void OnData (JsonObject data);
		/// <summary>
		/// error receive callback
		/// </summary>
		/// <param name="data">json</param>
		void OnError (JsonObject data);
	}

	/// <summary>
	/// Plugins controller
	/// Can manage other plugins by this system
	/// </summary>
	public class Plugins : MonoBehaviour {

#if UNITY_ANDROID
		public const string ANDROID_CLASS_MASK = "ru.mopsicus.{0}.Plugin";
#elif UNITY_IOS
		[DllImport ("__Internal")]
		private static extern void pluginsInit (string data);
#endif		

		/// <summary>
		/// plugins gameobject for communication (do not change!)
		/// </summary>
		const string _dataObject = "Plugins";
		/// <summary>
		/// method name for receive data from plugins (do not change!)
		/// </summary>
		const string _dataReceiver = "OnDataReceive";
		/// <summary>
		/// plugins list
		/// </summary>
		private IPlugin[] _plugins;

		private void Awake () {
			name = _dataObject;
			DontDestroyOnLoad (gameObject);
			InitPlugins ();
		}

		private void OnDestroy () {
			_plugins = new IPlugin[0];
		}

		/// <summary>
		/// Init plugins in app
		/// </summary>
		void InitPlugins () {
			gameObject.AddComponent<MobileInputHandler> ();
			// add your plugins			
			_plugins = GetComponents<IPlugin> ();
			JsonObject data = new JsonObject ();
			data["object"] = _dataObject;
			data["receiver"] = _dataReceiver;
#if UNITY_EDITOR
			Debug.Log ("Plugins init");
#elif UNITY_IOS
			pluginsInit (data.ToJsonString ());
#endif
		}

		/// <summary>
		/// Process data from plugins and send plugin
		/// </summary>
		/// <param name="data">json</param>
		void OnDataReceive (string data) {
#if DEBUG
			Debug.Log ("Plugins receive data: " + data);
#endif				
			try {
				JsonObject info = (JsonObject) JsonNode.ParseJsonString (data);
				foreach (IPlugin plugin in _plugins) {
					if (plugin.Name == info["name"]) {
						if (info.ContainsKey ("error"))
							plugin.OnError (info);
						else
							plugin.OnData (info);
						break;
					}
				}
			} catch (Exception e) {
#if DEBUG
				Debug.LogError ("Plugins receive error: " + e.Message + " stack: " + e.StackTrace);
#endif	
			}

		}

	}

}