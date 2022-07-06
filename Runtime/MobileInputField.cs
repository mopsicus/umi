// ----------------------------------------------------------------------------
// The MIT License
// UnityMobileInput https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018-2020 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using NiceJson;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Mopsicus.Plugins {

    /// <summary>
    /// Wrapper for Unity InputField
    /// Add this component on your InputField
    /// </summary>
    [RequireComponent (typeof (InputField))]
    public class MobileInputField : MobileInputReceiver {

        /// <summary>
        /// Config structure
        /// </summary>
        private struct MobileInputConfig {
            public bool Multiline;
            public Color TextColor;
            public Color BackgroundColor;
            public string ContentType;
            public string InputType;
            public string KeyboardType;
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
            Search,
            Send
        }

        /// <summary>
        /// Custom font name
        /// </summary>
        public string CustomFont = "default";        

        /// <summary>
        /// Hide and deselect input manually
        /// </summary>
        public bool IsManualHideControl = false;

        /// <summary>
        /// "Done" button visible (for iOS)
        /// </summary>
        public bool IsWithDoneButton = true;

        /// <summary>
        /// "(x)" button visible (for iOS)
        /// </summary>
        public bool IsWithClearButton = true;

        /// <summary>
        /// Type for return button
        /// </summary>
        public ReturnKeyType ReturnKey;

        /// <summary>
        /// Action when Return pressed, for subscribe
        /// </summary>
        public Action OnReturnPressed = delegate { };

        /// <summary>
        /// Action when Focus changed
        /// </summary>
        public Action<bool> OnFocusChanged = delegate { };

        /// <summary>
        /// Event when Return pressed, for Unity inspector
        /// </summary>
        public UnityEvent OnReturnPressedEvent;

        /// <summary>
        /// Mobile input creation flag
        /// </summary>
        private bool _isMobileInputCreated = false;
	
        /// <summary>
        /// Mobile input first creation flag
        /// </summary>
	private bool _isFirstCreation = true;

        /// <summary>
        /// InputField object
        /// </summary>
        private InputField _inputObject;

        /// <summary>
        /// Text object from _inputObject
        /// </summary>
        private Text _inputObjectText;

        /// <summary>
        /// Set focus on create flag
        /// </summary>
        private bool _isFocusOnCreate;

        /// <summary>
        /// Set visible on create flag
        /// </summary>
        private bool _isVisibleOnCreate = true;

        /// <summary>
        /// Last field position cache
        /// </summary>
        private Rect _lastRect;

        /// <summary>
        /// Current config
        /// </summary>
        private MobileInputConfig _config;

        /// <summary>
        /// InputField create event
        /// </summary>
        const string CREATE = "CREATE_EDIT";

        /// <summary>
        /// InputField remove event
        /// </summary>
        const string REMOVE = "REMOVE_EDIT";

        /// <summary>
        /// Set text to InputField
        /// </summary>
        const string SET_TEXT = "SET_TEXT";

        /// <summary>
        /// Set new Rect, position, size
        /// </summary>
        const string SET_RECT = "SET_RECT";

        /// <summary>
        /// Set focus to InputField
        /// </summary>
        const string SET_FOCUS = "SET_FOCUS";

        /// <summary>
        /// Event when InputField is focused
        /// </summary>
        const string ON_FOCUS = "ON_FOCUS";

        /// <summary>
        /// Event when InputField is unfocused
        /// </summary>
        const string ON_UNFOCUS = "ON_UNFOCUS";

        /// <summary>
        /// Set visible to InputField
        /// </summary>
        const string SET_VISIBLE = "SET_VISIBLE";

        /// <summary>
        /// Event when text changing in InputField
        /// </summary>
        const string TEXT_CHANGE = "TEXT_CHANGE";

        /// <summary>
        /// Event when text end changing in InputField
        /// </summary>
        const string TEXT_END_EDIT = "TEXT_END_EDIT";

        /// <summary>
        /// Event for Android
        /// </summary>
        const string ANDROID_KEY_DOWN = "ANDROID_KEY_DOWN";

        /// <summary>
        /// Event when Return key pressed
        /// </summary>
        const string RETURN_PRESSED = "RETURN_PRESSED";

        /// <summary>
        /// Ready event
        /// </summary>
        const string READY = "READY";

        /// <summary>
        /// Constructor
        /// </summary>
        private void Awake () {
            _inputObject = this.GetComponent<InputField> ();
            if ((object) _inputObject == null) {
                Debug.LogError (string.Format ("No found InputField for {0} MobileInput", this.name));
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
	    _isFirstCreation = false;
        }

        /// <summary>
        /// Show native on enable
        /// </summary>
        private void OnEnable () {
            if (_isMobileInputCreated) {
                this.SetVisible (true);
            } else if (!_isFirstCreation) {
            	StartCoroutine (InitialzieOnNextFrame ());
	    }
        }

        /// <summary>
        /// Hide native on disable
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
        private void OnApplicationFocus (bool hasFocus) {
            if (!_isMobileInputCreated || !this.Visible) {
                return;
            }
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
        /// MobileInput visible
        /// </summary>
        public bool Visible {
            get;
            private set;
        }

        /// <summary>
        /// MobileInput text
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
            _inputObject.enabled = false;
            _inputObjectText.enabled = false;
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
            if (_isMobileInputCreated && this._inputObject != null) {
#if !UNITY_EDITOR
                int touchCount = Input.touchCount;
                if (touchCount > 0) {
                    Rect inputRect = this._inputObjectText.rectTransform.rect;
                    for (int i = 0; i < touchCount; i++) {
                        if (!inputRect.Contains (Input.GetTouch(i).position)) {
                            if (!IsManualHideControl) {
                                Hide ();
                            }
                            return;
                        }
                    }
                }
#endif
                SetRectNative (this._inputObjectText.rectTransform);
            }
        }

		/// <summary>
		/// Get sizes and convert to current screen size
		/// </summary>
		/// <param name="rect">RectTranform from Gameobject</param>
		/// <returns>Rect</returns>
		public static Rect GetScreenRectFromRectTransform (RectTransform rect) {
			Vector3[] corners = new Vector3[4];
			rect.GetWorldCorners (corners);
			float xMin = float.PositiveInfinity;
			float xMax = float.NegativeInfinity;
			float yMin = float.PositiveInfinity;
			float yMax = float.NegativeInfinity;
			for (int i = 0; i < 4; i++) {
				Vector3 screenCoord;
				Canvas canvas = rect.GetComponentInParent<Canvas> ();
				if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
					screenCoord = corners[i];
				} else if (canvas.renderMode == RenderMode.ScreenSpaceCamera) {
					screenCoord = canvas.worldCamera.WorldToScreenPoint (corners[i]);
				} else {
					screenCoord = RectTransformUtility.WorldToScreenPoint (Camera.main, corners[i]);
				}
				if (screenCoord.x < xMin) {
					xMin = screenCoord.x;
				}
				if (screenCoord.x > xMax) {
					xMax = screenCoord.x;
				}
				if (screenCoord.y < yMin) {
					yMin = screenCoord.y;
				}
				if (screenCoord.y > yMax) {
					yMax = screenCoord.y;
				}
			}
			Rect result = new Rect (xMin, Screen.height - yMax, xMax - xMin, yMax - yMin);
			return result;
		}        

        /// <summary>
        /// Prepare config
        /// </summary>
        private void PrepareNativeEdit () {
            Text placeHolder = _inputObject.placeholder.GetComponent<Text> ();
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
            _config.KeyboardType = _inputObject.keyboardType.ToString ();
            _config.InputType = _inputObject.inputType.ToString ();
        }

        /// <summary>
        /// Text change callback
        /// </summary>
        /// <param name="text">new text</param>
        private void OnTextChange (string text) {
            if (text == this._inputObject.text) {
                return;
            }
            this._inputObject.text = text;
            if (this._inputObject.onValueChanged != null) {
                this._inputObject.onValueChanged.Invoke (text);
            }
        }

        /// <summary>
        /// Text change end callback
        /// </summary>
        /// <param name="text">text</param>
        private void OnTextEditEnd (string text) {
            this._inputObject.text = text;
            if (this._inputObject.onEndEdit != null) {
                this._inputObject.onEndEdit.Invoke (text);
            }
            SetFocus (false);
        }

        /// <summary>
        /// Sending data to plugin
        /// </summary>
        /// <param name="data">JSON</param>
        public override void Send (JsonObject data) {
            MobileInput.Plugin.StartCoroutine (PluginsMessageRoutine (data));
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
        /// <param name="data">JSON</param>
        private IEnumerator PluginsMessageRoutine (JsonObject data) {
            yield return null;
            string msg = data["msg"];
            if (msg.Equals (TEXT_CHANGE)) {
                string text = data["text"];
                this.OnTextChange (text);
            } else if (msg.Equals (READY)) {
                this.Ready ();
            } else if (msg.Equals (ON_FOCUS)) {
                OnFocusChanged (true);
            } else if (msg.Equals (ON_UNFOCUS)) {
                OnFocusChanged (false);
            } else if (msg.Equals (TEXT_END_EDIT)) {
                string text = data["text"];
                this.OnTextEditEnd (text);
            } else if (msg.Equals (RETURN_PRESSED)) {
                OnReturnPressed ();
                if (OnReturnPressedEvent != null) {
                    OnReturnPressedEvent.Invoke ();
                }
            }
        }

	/// <summary>
        /// Convert float value to InvariantCulture string
        /// </summary>
        /// <param name="value">float value</param>
        /// <returns></returns>
        private string InvariantCultureString (float value){
            return value.ToString ("G", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Create native input field
        /// </summary>
        private void CreateNativeEdit () {
            Rect rect = GetScreenRectFromRectTransform (this._inputObjectText.rectTransform);
            JsonObject data = new JsonObject ();
            data["msg"] = CREATE;
            data["x"] = InvariantCultureString(rect.x / Screen.width);
            data["y"] = InvariantCultureString(rect.y / Screen.height);
            data["width"] = InvariantCultureString(rect.width / Screen.width);
            data["height"] = InvariantCultureString(rect.height / Screen.height);
            data["character_limit"] = _config.CharacterLimit;
            data["text_color_r"] = InvariantCultureString(_config.TextColor.r);
            data["text_color_g"] = InvariantCultureString(_config.TextColor.g);
            data["text_color_b"] = InvariantCultureString(_config.TextColor.b);
            data["text_color_a"] = InvariantCultureString(_config.TextColor.a);
            data["back_color_r"] = InvariantCultureString(_config.BackgroundColor.r);
            data["back_color_g"] = InvariantCultureString(_config.BackgroundColor.g);
            data["back_color_b"] = InvariantCultureString(_config.BackgroundColor.b);
            data["back_color_a"] = InvariantCultureString(_config.BackgroundColor.a);
            data["font_size"] = InvariantCultureString(_config.FontSize);
            data["content_type"] = _config.ContentType;
            data["align"] = _config.Align;
            data["with_done_button"] = this.IsWithDoneButton;
            data["with_clear_button"] = this.IsWithClearButton;
            data["placeholder"] = _config.Placeholder;
            data["placeholder_color_r"] = InvariantCultureString(_config.PlaceholderColor.r);
            data["placeholder_color_g"] = InvariantCultureString(_config.PlaceholderColor.g);
            data["placeholder_color_b"] = InvariantCultureString(_config.PlaceholderColor.b);
            data["placeholder_color_a"] = InvariantCultureString(_config.PlaceholderColor.a);
            data["multiline"] = _config.Multiline;
            data["font"] = this.CustomFont;
            data["input_type"] = _config.InputType;
            data["keyboard_type"] = _config.KeyboardType;
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
                case ReturnKeyType.Send:
                    data["return_key_type"] = "Send";
                    break;
                default:
                    data["return_key_type"] = "Default";
                    break;
            }

            this.Execute (data);
        }

        /// <summary>
        /// New field successfully added
        /// </summary>
        void Ready () {
            _isMobileInputCreated = true;
            if (!_isVisibleOnCreate) {
                SetVisible (false);
            }
            if (_isFocusOnCreate) {
                SetFocus (true);
            }
        }

        /// <summary>
        /// Set text to field
        /// </summary>
        /// <param name="text">New text</param>
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
	    _isMobileInputCreated = false;
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
            data["x"] = InvariantCultureString(rect.x / Screen.width);
            data["y"] = InvariantCultureString(rect.y / Screen.height);
            data["width"] = InvariantCultureString(rect.width / Screen.width);
            data["height"] = InvariantCultureString(rect.height / Screen.height);
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
                if (isFocus) {
                    _inputObject.ActivateInputField ();
                } else {
                    _inputObject.DeactivateInputField ();
                }
            } else {
                _isFocusOnCreate = isFocus;
            }
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
        /// <param name="key">Code</param>
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
                            this.ForceSendKeydownAndroid (Input.inputString);
                        }
                    }
                }
            }
        }
#endif

    }

}
