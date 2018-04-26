// ----------------------------------------------------------------------------
// The MIT License
// LeopotamGroupLibrary https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------
using System;
using System.Collections;
using NiceJson;
using UnityEngine;
using UnityEngine.UI;

namespace Mopsicus.Plugins.MobileInput {

	/// <summary>
	/// Wrapper for Unity InputField
	/// Add this component on your InputField
	/// </summary>
	[RequireComponent (typeof (InputField))]
	public class MobileInput : MobileInputReceiver {

		/// <summary>
		/// Config structure
		/// </summary>
		private struct MobileInputConfig {
			public bool Multiline;
			public Color TextColor;
			public Color BackgroundColor;
			public string ContentType;
			public string Font;
			public float FontSize;
			public string Align;
			public string Placeholder;
			public Color PlaceholderColor;
			public int CharacterLimit;

		}

		/// <summary>
		/// Button type
		/// </summary>
		public enum ReturnKeyType {
			Default,
			Next,
			Done,
			Search
		}

		/// <summary>
		/// Input type
		/// </summary>
		public enum InputType {
			AutoCorrect,
			Password
		}

		/// <summary>
		/// Keyboard type
		/// </summary>
		public enum KeyboardType {
			ASCIICapable,
			NumbersAndPunctuation,
			URL,
			NumberPad,
			PhonePad,
			NamePhonePad,
			EmailAddress
		}

		/// <summary>
		/// "Done" button visible (for iOS)
		/// </summary>
		public bool IsWithDoneButton = true;
		/// <summary>
		/// button type
		/// </summary>
		public ReturnKeyType ReturnKey;
		/// <summary>
		/// input type
		/// </summary>
		public InputType Type;
		/// <summary>
		/// keyboard type
		/// </summary>
		public KeyboardType Keyboard;
		/// <summary>
		/// action when Return pressed
		/// </summary>
		public event Action ReturnPressed;
		/// <summary>
		/// event when Return pressed
		/// </summary>
		public UnityEngine.Events.UnityEvent OnReturnPressed;
		/// <summary>
		/// mobile input creation flag
		/// </summary>
		private bool _isMobileInputCreated = false;
		/// <summary>
		/// InputField object
		/// </summary>
		private InputField _inputObject;
		/// <summary>
		/// Text object from _inputObject
		/// </summary>
		private Text _inputObjectText;
		/// <summary>
		/// set focus on create
		/// </summary>
		private bool _isFocusOnCreate;
		/// <summary>
		/// set visible on create
		/// </summary>
		private bool _isVisibleOnCreate = true;
		/// <summary>
		/// last inputfield position
		/// </summary>
		private Rect _lastRect;
		/// <summary>
		/// config
		/// </summary>
		private MobileInputConfig _config;

		// event for plugin communication

		private const string CREATE = "CREATE_EDIT";
		private const string REMOVE = "REMOVE_EDIT";
		private const string SET_TEXT = "SET_TEXT";
		private const string SET_RECT = "SET_RECT";
		private const string SET_FOCUS = "SET_FOCUS";
		private const string SET_VISIBLE = "SET_VISIBLE";
		private const string TEXT_CHANGE = "TEXT_CHANGE";
		private const string TEXT_END_EDIT = "TEXT_END_EDIT";
		private const string ANDROID_KEY_DOWN = "ANDROID_KEY_DOWN";
		private const string RETURN_PRESSED = "RETURN_PRESSED";
		private const string READY = "READY";

		/// <summary>
		/// Constructor
		/// </summary>
		private void Awake () {
			_inputObject = this.GetComponent<InputField> ();
			if (_inputObject == null) {
#if DEBUG
				Debug.LogErrorFormat ("No InputField found {0} MobileInput Error", this.name);
#endif
				throw new MissingComponentException ();
			}
			_inputObjectText = _inputObject.textComponent;
		}

		/// <summary>
		/// Create mobile input on Start with coroutine
		/// </summary>
		protected override void Start () {
			base.Start ();
			StartCoroutine (InitialzieOnNextFrame ());
		}

		/// <summary>
		/// Show on enable
		/// </summary>
		private void OnEnable () {
			if (_isMobileInputCreated)
				this.SetVisible (true);
		}

		/// <summary>
		/// Hide on disable
		/// </summary>
		private void OnDisable () {
			if (_isMobileInputCreated) {
				this.SetFocus (false);
				this.SetVisible (false);
			}
		}

		/// <summary>
		/// Destructor
		/// </summary>
		protected override void OnDestroy () {
			RemoveNative ();
			base.OnDestroy ();
		}

		/// <summary>
		/// Handler for app focus lost
		/// </summary>
		/// <param name="hasFocus"></param>
		private void OnApplicationFocus (bool hasFocus) {
			if (!_isMobileInputCreated || !this.Visible)
				return;
			this.SetVisible (hasFocus);
		}

		/// <summary>
		/// Current InputField for external access
		/// </summary>
		public InputField InputField {
			get {
				return _inputObject;
			}
		}

		/// <summary>
		/// Mobile input visible
		/// </summary>
		/// <returns>true | false</returns>
		public bool Visible {
			get;
			private set;
		}

		/// <summary>
		/// Mobile input text
		/// </summary>
		public string Text {
			get {
				return _inputObject.text;
			}
			set {
				_inputObject.text = value;
				SetTextNative (value);
			}
		}

		/// <summary>
		/// Initialization coroutine
		/// </summary>
		private IEnumerator InitialzieOnNextFrame () {
			yield return null;
			this.PrepareNativeEdit ();
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
			this.CreateNativeEdit ();
			this.SetTextNative (this._inputObjectText.text);
			_inputObject.placeholder.gameObject.SetActive (false);
			_inputObjectText.enabled = false;
			_inputObject.enabled = false;
#endif
		}

		/// <summary>
		/// Check position on each frame
		/// If changed - send to plugin
		/// It's need when app rotate on input field chage position
		/// </summary>
		private void Update () {
#if UNITY_ANDROID && !UNITY_EDITOR
			this.UpdateForceKeyeventForAndroid ();
#endif
			if (this._inputObject != null && _isMobileInputCreated) {
				SetRectNative (this._inputObjectText.rectTransform);
			}
		}

		/// <summary>
		/// Get bounds and calc for current screen size
		/// </summary>
		/// <param name="rect">recttranform</param>
		/// <returns>rect</returns>
		Rect GetScreenRectFromRectTransform (RectTransform rect) {
			Vector3[] corners = new Vector3[4];
			rect.GetWorldCorners (corners);
			float xMin = float.PositiveInfinity;
			float xMax = float.NegativeInfinity;
			float yMin = float.PositiveInfinity;
			float yMax = float.NegativeInfinity;
			for (int i = 0; i < 4; i++) {
				Vector3 screenCoord;
				if (rect.GetCanvas().GetComponent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay)
				{
					screenCoord = corners[i];
				} else
				{
					screenCoord = RectTransformUtility.WorldToScreenPoint(Camera.main, corners[i]);
				}
				if (screenCoord.x < xMin)
					xMin = screenCoord.x;
				if (screenCoord.x > xMax)
					xMax = screenCoord.x;
				if (screenCoord.y < yMin)
					yMin = screenCoord.y;
				if (screenCoord.y > yMax)
					yMax = screenCoord.y;
			}
			Rect result = new Rect (xMin, Screen.height - yMax, xMax - xMin, yMax - yMin);
			return result;
		}

		/// <summary>
		/// Prepare config
		/// </summary>
		private void PrepareNativeEdit () {
			Text placeHolder = _inputObject.placeholder.GetComponent<Text> ();
			_config.Font = "Arial";
			_config.Placeholder = placeHolder.text;
			_config.PlaceholderColor = placeHolder.color;
			_config.CharacterLimit = _inputObject.characterLimit;
			Rect rect = GetScreenRectFromRectTransform (this._inputObjectText.rectTransform);
			float ratio = rect.height / _inputObjectText.rectTransform.rect.height;
			_config.FontSize = ((float) _inputObjectText.fontSize) * ratio;
			_config.TextColor = _inputObjectText.color;
			_config.Align = _inputObjectText.alignment.ToString ();
			_config.ContentType = _inputObject.contentType.ToString ();
			_config.BackgroundColor = _inputObject.colors.normalColor;
			_config.Multiline = (_inputObject.lineType == InputField.LineType.SingleLine) ? false : true;
		}

		/// <summary>
		/// Text change callback
		/// </summary>
		/// <param name="text">new text</param>
		private void onTextChange (string text) {
			if (text == this._inputObject.text)
				return;
			this._inputObject.text = text;
			if (this._inputObject.onValueChanged != null) {
				this._inputObject.onValueChanged.Invoke (text);
			}
		}

		/// <summary>
		/// Text change end callback
		/// </summary>
		/// <param name="text">text</param>
		private void onTextEditEnd (string text) {
			this._inputObject.text = text;
			if (this._inputObject.onEndEdit != null) {
				this._inputObject.onEndEdit.Invoke (text);
			}
			SetFocus (false);
		}

		/// <summary>
		/// Sending data to plugin
		/// </summary>
		/// <param name="data">json</param>
		public override void Direct (JsonObject data) {
			MobileInputHandler.Plugin.StartCoroutine (PluginsMessageRoutine (data));
		}

		/// <summary>
		/// Remove focus, keyboard when app lose focus
		/// </summary>
		public override void Hide () {
			this.SetFocus (false);
		}

		/// <summary>
		/// Coroutine for send, so its not freeze main thread
		/// </summary>
		/// <param name="data">json</param>
		private IEnumerator PluginsMessageRoutine (JsonObject data) {
			yield return null;
			string msg = data["msg"];
			if (msg.Equals (TEXT_CHANGE)) {
				string text = data["text"];
				this.onTextChange (text);
			} else if (msg.Equals (READY)) {
				this.Ready ();
			} else if (msg.Equals (TEXT_END_EDIT)) {
				string text = data["text"];
				this.onTextEditEnd (text);
			} else if (msg.Equals (RETURN_PRESSED)) {
				if (ReturnPressed != null)
					ReturnPressed ();
				if (OnReturnPressed != null)
					OnReturnPressed.Invoke ();
			}
		}

		/// <summary>
		/// Create native input field
		/// </summary>
		private void CreateNativeEdit () {
			Rect rect = GetScreenRectFromRectTransform (this._inputObjectText.rectTransform);
			JsonObject data = new JsonObject ();
			data["msg"] = CREATE;
			data["x"] = rect.x / Screen.width;
			data["y"] = rect.y / Screen.height;
			data["width"] = rect.width / Screen.width;
			data["height"] = rect.height / Screen.height;
			data["character_limit"] = _config.CharacterLimit;
			data["text_color_r"] = _config.TextColor.r;
			data["text_color_g"] = _config.TextColor.g;
			data["text_color_b"] = _config.TextColor.b;
			data["text_color_a"] = _config.TextColor.a;
			data["back_color_r"] = _config.BackgroundColor.r;
			data["back_color_g"] = _config.BackgroundColor.g;
			data["back_color_b"] = _config.BackgroundColor.b;
			data["back_color_a"] = _config.BackgroundColor.a;
			data["font"] = _config.Font;
			data["font_size"] = _config.FontSize;
			data["content_type"] = _config.ContentType;
			data["align"] = _config.Align;
			data["with_done_button"] = this.IsWithDoneButton;
			data["placeholder"] = _config.Placeholder;
			data["placeholder_color_r"] = _config.PlaceholderColor.r;
			data["placeholder_color_g"] = _config.PlaceholderColor.g;
			data["placeholder_color_b"] = _config.PlaceholderColor.b;
			data["placeholder_color_a"] = _config.PlaceholderColor.a;
			data["multiline"] = _config.Multiline;
			switch (ReturnKey) {
				case ReturnKeyType.Next:
					data["return_key_type"] = "Next";
					break;
				case ReturnKeyType.Done:
					data["return_key_type"] = "Done";
					break;
				case ReturnKeyType.Search:
					data["return_key_type"] = "Search";
					break;
				default:
					data["return_key_type"] = "Default";
					break;
			}
			switch (Type) {
				case InputType.AutoCorrect:
					data["input_type"] = "AutoCorrect";
					break;
				case InputType.Password:
					data["input_type"] = "Password";
					break;
			}
			switch (Keyboard) {
				case KeyboardType.ASCIICapable:
					data["keyboard_type"] = "ASCIICapable";
					break;
				case KeyboardType.EmailAddress:
					data["keyboard_type"] = "EmailAddress";
					break;
				case KeyboardType.NamePhonePad:
					data["keyboard_type"] = "NamePhonePad";
					break;
				case KeyboardType.NumberPad:
					data["keyboard_type"] = "NumberPad";
					break;
				case KeyboardType.NumbersAndPunctuation:
					data["keyboard_type"] = "NumbersAndPunctuation";
					break;
				case KeyboardType.PhonePad:
					data["keyboard_type"] = "PhonePad";
					break;
				case KeyboardType.URL:
					data["keyboard_type"] = "URL";
					break;
			}
			this.Execute (data);
		}

		/// <summary>
		/// New field successfully added
		/// </summary>
		void Ready () {
			_isMobileInputCreated = true;
			if (!_isVisibleOnCreate)
				SetVisible (false);
			if (_isFocusOnCreate)
				SetFocus (true);
		}

		/// <summary>
		/// Set text to field
		/// </summary>
		/// <param name="text">string</param>
		void SetTextNative (string text) {
			JsonObject data = new JsonObject ();
			data["msg"] = SET_TEXT;
			data["text"] = text;
			this.Execute (data);
		}

		/// <summary>
		/// Remove field
		/// </summary>
		private void RemoveNative () {
			JsonObject data = new JsonObject ();
			data["msg"] = REMOVE;
			this.Execute (data);
		}

		/// <summary>
		/// Set new size and position
		/// </summary>
		/// <param name="inputRect">RectTransform</param>
		public void SetRectNative (RectTransform inputRect) {
			Rect rect = GetScreenRectFromRectTransform (inputRect);
			if (_lastRect == rect) {
				return;
			}
			_lastRect = rect;
			JsonObject data = new JsonObject ();
			data["msg"] = SET_RECT;
			data["x"] = rect.x / Screen.width;
			data["y"] = rect.y / Screen.height;
			data["width"] = rect.width / Screen.width;
			data["height"] = rect.height / Screen.height;
			this.Execute (data);
		}

		/// <summary>
		/// Set focus on field
		/// </summary>
		/// <param name="isFocus">true | false</param>
		public void SetFocus (bool isFocus) {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
			if (!_isMobileInputCreated) {
				_isFocusOnCreate = isFocus;
				return;
			}
			JsonObject data = new JsonObject ();
			data["msg"] = SET_FOCUS;
			data["is_focus"] = isFocus;
			this.Execute (data);
#else
			if (gameObject.activeInHierarchy) {
				if (isFocus)
					_inputObject.ActivateInputField ();
				else
					_inputObject.DeactivateInputField ();
			} else
				_isFocusOnCreate = isFocus;
#endif

		}

		/// <summary>
		/// Set field visible
		/// </summary>
		/// <param name="isVisible">true | false</param>
		public void SetVisible (bool isVisible) {
			if (!_isMobileInputCreated) {
				_isVisibleOnCreate = isVisible;
				return;
			}
			JsonObject data = new JsonObject ();
			data["msg"] = SET_VISIBLE;
			data["is_visible"] = isVisible;
			this.Execute (data);
			this.Visible = isVisible;
		}

#if UNITY_ANDROID && !UNITY_EDITOR

		/// <summary>
		/// Send android button state
		/// </summary>
		/// <param name="key">код</param>
		private void ForceSendKeydownAndroid (string key) {
			JsonObject data = new JsonObject ();
			data["msg"] = ANDROID_KEY_DOWN;
			data["key"] = key;
			this.Execute (data);
		}

		/// <summary>
		/// Keyboard handler
		/// </summary>
		private void UpdateForceKeyeventForAndroid () {
			if (UnityEngine.Input.anyKeyDown) {
				if (UnityEngine.Input.GetKeyDown (KeyCode.Backspace)) {
					this.ForceSendKeydownAndroid ("backspace");
				} else {
					foreach (char c in UnityEngine.Input.inputString) {
						if (c == '\n') {
							this.ForceSendKeydownAndroid ("enter");
						} else {
							this.ForceSendKeydownAndroid (UnityEngine.Input.inputString);
						}
					}
				}
			}
		}
#endif
	}

}
