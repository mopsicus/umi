using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using NiceJson;
using UnityEngine.Events;

namespace UMI {

    /// <summary>
    /// Content type
    /// </summary>
    public enum InputContentType {
        Standard,
        Autocorrected,
        IntegerNumber,
        DecimalNumber,
        Alphanumeric,
        Name,
        EmailAddress,
        Password,
        Pin
    }

    /// <summary>
    /// Wrapper for Unity TMP_InputField
    /// Add this component on your TMP_InputField
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class MobileInputField : MobileInputReceiver {

        /// <summary>
        /// Config structure
        /// </summary>
        struct MobileInputConfig {
            public bool Multiline;
            public Color TextColor;
            public Color BackgroundColor;
            public bool ChangeCaret;
            public Color CaretColor;
            public Color HighlightColor;
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
        /// Native input initialization state
        /// </summary>
        enum InitializationState {
            NotStarted,
            Scheduled,
            WaitingForReady,
            Ready,
            Destroyed
        }

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        /// <summary>
        /// InputField create event
        /// </summary>
        const string CREATE = "CREATE_EDIT";

        /// <summary>
        /// Set focus to InputField
        /// </summary>
        const string SET_FOCUS = "SET_FOCUS";
#endif

        /// <summary>
        /// WaitForEndOfFrame
        /// </summary>
        static readonly WaitForEndOfFrame WaitForEndOfFrame = null;

        /// <summary>
        /// InputField remove event
        /// </summary>
        const string REMOVE = "REMOVE_EDIT";

        /// <summary>
        /// Set text to InputField
        /// </summary>
        const string SET_TEXT = "SET_TEXT";

        /// <summary>
        /// Set input content type
        /// </summary>
        const string SET_CONTENT_TYPE = "SET_CONTENT_TYPE";

        /// <summary>
        /// Set text color
        /// </summary>
        const string SET_TEXT_COLOR = "SET_TEXT_COLOR";

        /// <summary>
        /// Set placeholder text color
        /// </summary>
        const string SET_PTEXT_COLOR = "SET_PTEXT_COLOR";

        /// <summary>
        /// Set background color
        /// </summary>
        const string SET_BG_COLOR = "SET_BG_COLOR";

        /// <summary>
        /// Set read only state
        /// </summary>
        const string SET_READ_ONLY = "SET_READ_ONLY";

        /// <summary>
        /// Set new Rect, position, size
        /// </summary>
        const string SET_RECT = "SET_RECT";

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
        /// Set language to keyboard
        /// </summary>
        const string SET_LANGUAGE = "SET_LANGUAGE";

        /// <summary>
        /// Set caret position
        /// </summary>
        const string SET_CARET = "SET_CARET";

        /// <summary>
        /// Event when text changing in InputField
        /// </summary>
        const string TEXT_CHANGE = "TEXT_CHANGE";

        /// <summary>
        /// Event when text end changing in InputField
        /// </summary>
        const string TEXT_END_EDIT = "TEXT_END_EDIT";

#if UNITY_ANDROID && !UNITY_EDITOR
        /// <summary>
        /// Event for Android
        /// </summary>
        const string ANDROID_KEY_DOWN = "ANDROID_KEY_DOWN";
#endif

        /// <summary>
        /// Event when Return key pressed
        /// </summary>
        const string RETURN_PRESSED = "RETURN_PRESSED";

        /// <summary>
        /// Ready event
        /// </summary>
        const string READY = "READY";

        /// <summary>
        /// Custom font name
        /// </summary>
        public string CustomFont = "default";

        /// <summary>
        /// Custom keyboard language, ISO code
        /// </summary>
        public string KeyboardLanguage = "default";

        /// <summary>
        /// Background color
        /// </summary>
        public Color BackgroundColor = Color.white;

        /// <summary>
        /// Hide and deselect input manually
        /// </summary>
        public bool IsManualHideControl = false;

        /// <summary>
        /// "Done" button visible (for iOS)
        /// </summary>
        public bool IsWithDoneButton = false;

        /// <summary>
        /// "(x)" button visible (for iOS)
        /// </summary>
        public bool IsWithClearButton = false;

        /// <summary>
        /// Type for return button
        /// </summary>
        public ReturnKeyType ReturnKey = ReturnKeyType.Default;

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
        public UnityEvent OnReturnPressedEvent = null;

        /// <summary>
        /// Native input initialization state
        /// </summary>
        InitializationState _initializationState = InitializationState.NotStarted;

        /// <summary>
        /// Currently scheduled initialization coroutine
        /// </summary>
        Coroutine _initializationCoroutine = null;

        /// <summary>
        /// InputField object
        /// </summary>
        TMP_InputField _inputObject = null;

        /// <summary>
        /// Text object from _inputObject
        /// </summary>
        TMP_Text _inputObjectText = null;

        /// <summary>
        /// Set focus on create flag
        /// </summary>
        bool _isFocusOnCreate = false;

        /// <summary>
        /// Last visibility requested through the public API
        /// </summary>
        bool _requestedVisible = true;

        /// <summary>
        /// Last field position cache
        /// </summary>
        Rect _lastRect = new Rect();

        /// <summary>
        /// Current config
        /// </summary>
        MobileInputConfig _config = new MobileInputConfig();

        /// <summary>
        /// Cache for string before create
        /// </summary>
        string _textOnCreate = null;

        /// <summary>
        /// Change , to . for float
        /// </summary>
        CultureInfo _cultureInfo = CultureInfo.InvariantCulture;

        /// <summary>
        /// Constructor
        /// </summary>
        void Awake() {
            _inputObject = GetComponent<TMP_InputField>();
            if ((object)_inputObject == null) {
#if UMI_DEBUG
                Debug.LogError($"[UMI] no found InputField for {this.name} MobileInput");
#endif
                throw new MissingComponentException();
            }
            _inputObjectText = _inputObject.textComponent;
            MobileInput.OnApplicationVisibilityChange += OnApplicationVisibilityChanged;
        }

        /// <summary>
        /// Create mobile input on Start with coroutine
        /// </summary>
        protected override void Start() {
            base.Start();
            RequestInitialization();
        }

        /// <summary>
        /// Show native on enable
        /// </summary>
        void OnEnable() {
            if (_initializationState == InitializationState.Ready) {
                SetRectNative(this._inputObjectText.rectTransform);
                ApplyNativeVisibility();
                ApplyFocusOnCreate();
            } else if (_initializationState == InitializationState.NotStarted) {
                RequestInitialization();
            }
        }

        /// <summary>
        /// Hide native on disable
        /// </summary>
        void OnDisable() {
            if (_initializationState == InitializationState.Scheduled) {
                CancelScheduledInitialization();
            } else if (_initializationState == InitializationState.Ready) {
                SetFocusNative(false);
                ApplyNativeVisibility();
            } else if (_initializationState == InitializationState.WaitingForReady) {
                ApplyNativeVisibility();
            }
        }

        /// <summary>
        /// Destructor
        /// </summary>
        protected override void OnDestroy() {
            MobileInput.OnApplicationVisibilityChange -= OnApplicationVisibilityChanged;
            CancelScheduledInitialization();
            RemoveNative();
            base.OnDestroy();
        }

        /// <summary>
        /// Reapply logical visibility after app focus/pause changes
        /// </summary>
        void OnApplicationVisibilityChanged() {
            ApplyNativeVisibility();
            ApplyFocusOnCreate();
        }

        /// <summary>
        /// Current InputField for external access
        /// </summary>
        public TMP_InputField InputField {
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
                return (_inputObject == null) ? string.Empty : _inputObject.text;
            }
            set {
                if (_initializationState != InitializationState.Ready) {
                    _textOnCreate = value;
                    return;
                }
                _inputObject.text = value;
                SetTextNative(value);
            }
        }

        /// <summary>
        /// Initialization
        /// </summary>
        void RequestInitialization() {
            if (_initializationState != InitializationState.NotStarted || !isActiveAndEnabled) {
                return;
            }
            _initializationState = InitializationState.Scheduled;
            _initializationCoroutine = StartCoroutine(InitProcess());
        }

        /// <summary>
        /// Cancel initialization before CREATE has been sent
        /// </summary>
        void CancelScheduledInitialization() {
            if (_initializationState != InitializationState.Scheduled) {
                return;
            }
            if (_initializationCoroutine != null) {
                StopCoroutine(_initializationCoroutine);
            }
            _initializationCoroutine = null;
            _initializationState = InitializationState.NotStarted;
        }

        /// <summary>
        /// Initialization
        /// </summary>
        IEnumerator InitProcess() {
            yield return WaitForEndOfFrame;

            _initializationCoroutine = null;
            if (_initializationState != InitializationState.Scheduled) {
                yield break;
            }
            if (!isActiveAndEnabled) {
                _initializationState = InitializationState.NotStarted;
                yield break;
            }
            if (!IsRegistered) {
                _initializationCoroutine = StartCoroutine(WaitForRegistration());
                yield break;
            }
            CreateAfterRegistration();
        }

        /// <summary>
        /// Wait until base.Start has registered the receiver before CREATE
        /// </summary>
        IEnumerator WaitForRegistration() {
            while (_initializationState == InitializationState.Scheduled && isActiveAndEnabled && !IsRegistered) {
                yield return null;
            }
            _initializationCoroutine = null;
            if (_initializationState != InitializationState.Scheduled) {
                yield break;
            }
            if (!isActiveAndEnabled) {
                _initializationState = InitializationState.NotStarted;
                yield break;
            }
            CreateAfterRegistration();
        }

        /// <summary>
        /// Prepare and send CREATE exactly once
        /// </summary>
        void CreateAfterRegistration() {
            PrepareNativeEdit();
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            var initialVisibility = ShouldShowNative();
            _initializationState = InitializationState.WaitingForReady;
            Visible = initialVisibility;
            CreateNativeEdit(initialVisibility);
            var data = this._inputObjectText.text.Trim();
            if (data.Length == 1) {
                var category = Char.GetUnicodeCategory(data[0]);
                if (category != UnicodeCategory.Format) {
                    this.SetTextNative(this._inputObjectText.text);
                }
            }
            _inputObject.placeholder.gameObject.SetActive(false);
            _inputObject.enabled = false;
            _inputObjectText.enabled = false;
#else
            _initializationState = InitializationState.Ready;
            ApplyNativeVisibility(true);
            ApplyFocusOnCreate();
#endif
        }

        /// <summary>
        /// Check position on each frame
        /// If changed - send to plugin
        /// It's need when app rotate on input field chage position
        /// </summary>
        void Update() {
#if UNITY_ANDROID && !UNITY_EDITOR
            UpdateForceKeyeventForAndroid();
#endif
            if (_inputObject != null && _initializationState == InitializationState.Ready) {
#if !UNITY_EDITOR
                var touchCount = Input.touchCount;
                if (touchCount > 0) {
                    var inputRect = this._inputObjectText.rectTransform.rect;
                    for (var i = 0; i < touchCount; i++) {
                        if (!inputRect.Contains(Input.touches[i].position)) {
#if UMI_DEBUG
                            Debug.Log($"[UMI] manual hide control: {IsManualHideControl}");
#endif                             
                            if (!IsManualHideControl) {
                                Hide();
                            }
                            return;
                        }
                    }
                }
#endif
                SetRectNative(_inputObjectText.rectTransform);
            }
        }

        /// <summary>
        /// Prepare config
        /// </summary>
        void PrepareNativeEdit() {
            var placeHolder = _inputObject.placeholder.GetComponent<TextMeshProUGUI>();
            _config.Placeholder = placeHolder.text;
            _config.PlaceholderColor = placeHolder.color;
            _config.CaretColor = _inputObject.caretColor;
            _config.HighlightColor = _inputObject.selectionColor;
            _config.ChangeCaret = _inputObject.customCaretColor;
            _config.CharacterLimit = _inputObject.characterLimit;
            var rect = GetScreenRectFromRectTransform(_inputObjectText.rectTransform);
            var ratio = rect.height / _inputObjectText.rectTransform.rect.height;
            _config.FontSize = ((float)_inputObjectText.fontSize) * ratio;
            _config.TextColor = _inputObjectText.color;
            _config.Align = _inputObjectText.alignment.ToString();
            _config.ContentType = _inputObject.contentType.ToString();
            _config.BackgroundColor = BackgroundColor;
            _config.Multiline = _inputObject.lineType != TMP_InputField.LineType.SingleLine;
            _config.KeyboardType = _inputObject.keyboardType.ToString();
            _config.InputType = _inputObject.inputType.ToString();
        }

        /// <summary>
        /// Get bounds and calc for current screen size
        /// </summary>
        /// <param name="rect">RectTranform object</param>
        /// <returns>Rect</returns>
        Rect GetScreenRectFromRectTransform(RectTransform rect) {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            var xMin = float.PositiveInfinity;
            var xMax = float.NegativeInfinity;
            var yMin = float.PositiveInfinity;
            var yMax = float.NegativeInfinity;
            for (var i = 0; i < 4; i++) {
                Vector3 coords;
                var canvas = rect.GetComponentInParent<Canvas>();
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                    coords = corners[i];
                } else if (canvas.renderMode == RenderMode.ScreenSpaceCamera) {
                    coords = canvas.worldCamera.WorldToScreenPoint (corners[i]);
                } else {
                    coords = RectTransformUtility.WorldToScreenPoint(Camera.main, corners[i]);
                }
                if (coords.x < xMin) {
                    xMin = coords.x;
                }
                if (coords.x > xMax) {
                    xMax = coords.x;
                }
                if (coords.y < yMin) {
                    yMin = coords.y;
                }
                if (coords.y > yMax) {
                    yMax = coords.y;
                }
            }
            var result = new Rect(xMin, Screen.height - yMax, xMax - xMin, yMax - yMin);
            return result;
        }

        /// <summary>
        /// Text change callback
        /// </summary>
        /// <param name="text">new text</param>
        void OnTextChange(string text) {
            if (text == _inputObject.text) {
                return;
            }
            _inputObject.text = text;
            _inputObject.onValueChanged?.Invoke(text);
        }

        /// <summary>
        /// Text change end callback
        /// </summary>
        /// <param name="text">text</param>
        void OnTextEditEnd(string text) {
            _inputObject.text = text;
            _inputObject.onEndEdit?.Invoke(text);
            SetFocus(false);
        }

        /// <summary>
        /// Sending data to plugin
        /// </summary>
        /// <param name="data">JSON</param>
        public override void Send(JsonObject data) {
            string msg = data["msg"];
            if (msg.Equals(READY)) {
                Ready();
                return;
            }
            if (!isActiveAndEnabled) {
                ProcessData(data);
                return;
            }
            StartCoroutine(SendDataProcess(data));
        }

        /// <summary>
        /// Remove focus, keyboard when app lose focus
        /// </summary>
        public override void Hide() {
            if (_inputObject != null && _initializationState == InitializationState.Ready) {
                SetFocus(false);
            }
        }

        /// <summary>
        /// Coroutine for send, so its not freeze main thread
        /// </summary>
        /// <param name="data">JSON</param>
        IEnumerator SendDataProcess(JsonObject data) {
            yield return WaitForEndOfFrame;
            ProcessData(data);
        }

        /// <summary>
        /// Process a native callback on Unity's main thread
        /// </summary>
        void ProcessData(JsonObject data) {
            string msg = data["msg"];
            if (msg.Equals(TEXT_CHANGE)) {
                string text = data["text"];
                OnTextChange(text);
            } else if (msg.Equals(ON_FOCUS)) {
                OnFocusChanged(true);
            } else if (msg.Equals(ON_UNFOCUS)) {
                OnFocusChanged(false);
            } else if (msg.Equals(TEXT_END_EDIT)) {
                string text = data["text"];
                OnTextEditEnd(text);
            } else if (msg.Equals(RETURN_PRESSED)) {
                OnReturnPressed();
                OnReturnPressedEvent?.Invoke();
            }
        }

        /// <summary>
        /// Convert float value to InvariantCulture string
        /// </summary>
        /// <param name="value">float value</param>
        string InvariantCultureString(float value) {
            return value.ToString("G", _cultureInfo);
        }

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        /// <summary>
        /// Create native input field
        /// </summary>
        void CreateNativeEdit(bool isVisible) {
            var rect = GetScreenRectFromRectTransform(_inputObjectText.rectTransform);
            var data = new JsonObject();
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
            data["with_done_button"] = IsWithDoneButton;
            data["with_clear_button"] = IsWithClearButton;
            data["font"] = CustomFont;
            data["placeholder"] = _config.Placeholder;
            data["placeholder_color_r"] = InvariantCultureString(_config.PlaceholderColor.r);
            data["placeholder_color_g"] = InvariantCultureString(_config.PlaceholderColor.g);
            data["placeholder_color_b"] = InvariantCultureString(_config.PlaceholderColor.b);
            data["placeholder_color_a"] = InvariantCultureString(_config.PlaceholderColor.a);
            data["caret_color_r"] = InvariantCultureString(_config.CaretColor.r);
            data["caret_color_g"] = InvariantCultureString(_config.CaretColor.g);
            data["caret_color_b"] = InvariantCultureString(_config.CaretColor.b);
            data["caret_color_a"] = InvariantCultureString(_config.CaretColor.a);              
            data["highlight_color_r"] = InvariantCultureString(_config.HighlightColor.r);
            data["highlight_color_g"] = InvariantCultureString(_config.HighlightColor.g);
            data["highlight_color_b"] = InvariantCultureString(_config.HighlightColor.b);
            data["highlight_color_a"] = InvariantCultureString(_config.HighlightColor.a);  
            data["caret_color"] = _config.ChangeCaret;
            data["multiline"] = _config.Multiline;
            data["input_type"] = _config.InputType;
            data["keyboard_type"] = _config.KeyboardType;
            data["keyboard_language"] = KeyboardLanguage;
            data["is_visible"] = isVisible;
            data["return_key_type"] = ReturnKey switch {
                ReturnKeyType.Next => (JsonNode)"Next",
                ReturnKeyType.Done => (JsonNode)"Done",
                ReturnKeyType.Search => (JsonNode)"Search",
                ReturnKeyType.Send => (JsonNode)"Send",
                _ => (JsonNode)"Default",
            };           
            Execute(data);
        }
#endif

        /// <summary>
        /// New field successfully added
        /// </summary>
        void Ready() {
            if (_initializationState != InitializationState.WaitingForReady) {
                return;
            }
            _initializationState = InitializationState.Ready;
            if (!string.IsNullOrEmpty(_textOnCreate)) {
                Text = _textOnCreate;
            }
            ApplyNativeVisibility(true);
            ApplyFocusOnCreate();
        }

        /// <summary>
        /// Set text color
        /// </summary>
        public void SetTextColor(Color color) {
            var data = new JsonObject();
            data["msg"] = SET_TEXT_COLOR;
            data["color_r"] = InvariantCultureString(color.r);
            data["color_g"] = InvariantCultureString(color.g);
            data["color_b"] = InvariantCultureString(color.b);
            data["color_a"] = InvariantCultureString(color.a);
            Execute(data);
        }

        /// <summary>
        /// Set text color for placeholder
        /// </summary>
        public void SetPlaceholderTextColor(Color color) {
            var data = new JsonObject();
            data["msg"] = SET_PTEXT_COLOR;
            data["color_r"] = InvariantCultureString(color.r);
            data["color_g"] = InvariantCultureString(color.g);
            data["color_b"] = InvariantCultureString(color.b);
            data["color_a"] = InvariantCultureString(color.a);
            Execute(data);
        }

        /// <summary>
        /// Set background color
        /// </summary>
        public void SetBackgroundColor(Color color) {
            var data = new JsonObject();
            data["msg"] = SET_BG_COLOR;
            data["color_r"] = InvariantCultureString(color.r);
            data["color_g"] = InvariantCultureString(color.g);
            data["color_b"] = InvariantCultureString(color.b);
            data["color_a"] = InvariantCultureString(color.a);
            Execute(data);
        }

        /// <summary>
        /// Set content type to input
        /// </summary>
        /// <param name="type">Type</param>
        public void SetContentType(InputContentType type) {
            var data = new JsonObject();
            data["msg"] = SET_CONTENT_TYPE;
            data["type"] = type.ToString();
            Execute(data);
        }

        /// <summary>
        /// Switch readonly mode
        /// </summary>
        /// <param name="value">On/off</param>
        public void SetReadonly(bool value) {
            var data = new JsonObject();
            data["msg"] = SET_READ_ONLY;
            data["value"] = value;
            Execute(data);
        }

        /// <summary>
        /// Set language to keyboard
        /// </summary>
        /// <param name="value">ISO language code</param>
        public void SetLanguage(string value) {
            var data = new JsonObject();
            data["msg"] = SET_LANGUAGE;
            data["value"] = value;
            Execute(data);
        }

        /// <summary>
        /// Set caret position
        /// </summary>
        /// <param name="value">Position in string</param>
        public void SetCaret(int value) {
            var data = new JsonObject();
            data["msg"] = SET_CARET;
            data["value"] = value;
            Execute(data);
        }

        /// <summary>
        /// Set text to field
        /// </summary>
        /// <param name="text">New text</param>
        void SetTextNative(string text) {
            var data = new JsonObject();
            data["msg"] = SET_TEXT;
            data["text"] = text;
            Execute(data);
        }

        /// <summary>
        /// Remove field
        /// </summary>
        void RemoveNative() {
            var removeRequired = _initializationState == InitializationState.WaitingForReady ||
                _initializationState == InitializationState.Ready;
            _initializationState = InitializationState.Destroyed;
            Visible = false;
            if (removeRequired) {
                var data = new JsonObject();
                data["msg"] = REMOVE;
                Execute(data);
            }
        }

        /// <summary>
        /// Set new size and position
        /// </summary>
        /// <param name="inputRect">RectTransform</param>
        public void SetRectNative(RectTransform inputRect) {
            var rect = GetScreenRectFromRectTransform(inputRect);
            if (_inputObject == null || _initializationState != InitializationState.Ready || _lastRect == rect) {
                return;
            }
            _lastRect = rect;
            var data = new JsonObject();
            data["msg"] = SET_RECT;
            data["x"] = InvariantCultureString(rect.x / Screen.width);
            data["y"] = InvariantCultureString(rect.y / Screen.height);
            data["width"] = InvariantCultureString(rect.width / Screen.width);
            data["height"] = InvariantCultureString(rect.height / Screen.height);
            Execute(data);
        }

        /// <summary>
        /// Set focus on field
        /// </summary>
        /// <param name="isFocus">true | false</param>
        public void SetFocus(bool isFocus) {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (_initializationState != InitializationState.Ready || (isFocus && !ShouldShowNative())) {
                _isFocusOnCreate = isFocus;
                return;
            }
            _isFocusOnCreate = false;
            SetFocusNative(isFocus);
#else
            if (gameObject.activeInHierarchy) {
                if (isFocus) {
                    _inputObject.ActivateInputField();
                } else {
                    _inputObject.DeactivateInputField();
                }
                _isFocusOnCreate = false;
            } else {
                _isFocusOnCreate = isFocus;
            }
#endif
        }

        /// <summary>
        /// Apply focus directly without changing a deferred focus request
        /// </summary>
        void SetFocusNative(bool isFocus) {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (_initializationState != InitializationState.Ready) {
                return;
            }
            var data = new JsonObject();
            data["msg"] = SET_FOCUS;
            data["is_focus"] = isFocus;
            Execute(data);
#else
            if (gameObject.activeInHierarchy) {
                if (isFocus) {
                    _inputObject.ActivateInputField();
                } else {
                    _inputObject.DeactivateInputField();
                }
            }
#endif
        }

        /// <summary>
        /// Apply deferred focus once the native input can safely receive it
        /// </summary>
        void ApplyFocusOnCreate() {
            if (!_isFocusOnCreate || _initializationState != InitializationState.Ready || !ShouldShowNative()) {
                return;
            }
            _isFocusOnCreate = false;
            SetFocusNative(true);
        }

        /// <summary>
        /// Check the effective visibility without changing the public request
        /// </summary>
        bool ShouldShowNative() {
            return _requestedVisible &&
                isActiveAndEnabled &&
                MobileInput.CanShowInputs &&
                _initializationState != InitializationState.Destroyed;
        }

        /// <summary>
        /// Apply the effective visibility to a native input that has been created
        /// </summary>
        void ApplyNativeVisibility(bool force = false) {
            if (_initializationState != InitializationState.WaitingForReady &&
                _initializationState != InitializationState.Ready) {
                return;
            }
            var isVisible = ShouldShowNative();
            if (!force && Visible == isVisible) {
                return;
            }
            var data = new JsonObject();
            data["msg"] = SET_VISIBLE;
            data["is_visible"] = isVisible;
            Execute(data);
            Visible = isVisible;
            _lastRect = new Rect();
        }

        /// <summary>
        /// Set requested field visibility
        /// </summary>
        /// <param name="isVisible">true | false</param>
        public void SetVisible(bool isVisible) {
            _requestedVisible = isVisible;
            ApplyNativeVisibility();
            ApplyFocusOnCreate();
        }

#if UNITY_ANDROID && !UNITY_EDITOR

        /// <summary>
        /// Send android button state
        /// </summary>
        /// <param name="key">Code</param>
        private void ForceSendKeydownAndroid(string key) {
            var data = new JsonObject();
            data["msg"] = ANDROID_KEY_DOWN;
            data["key"] = key;
            Execute(data);
        }

        /// <summary>
        /// Keyboard handler
        /// </summary>
        private void UpdateForceKeyeventForAndroid() {
            if (Input.anyKeyDown) {
                if (Input.GetKeyDown(KeyCode.Backspace)) {
                    ForceSendKeydownAndroid("backspace");
                } else {
                    foreach (var c in Input.inputString) {
                        if (c == '\n') {
                            ForceSendKeydownAndroid("enter");
                        } else {
                            ForceSendKeydownAndroid(Input.inputString);
                        }
                    }
                }
            }
        }
#endif

    }
}
