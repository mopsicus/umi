using UnityEngine;
using UMI;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// Demo with runtime methods
/// </summary>
public class Demo : MonoBehaviour {

    /// <summary>
    /// Link to single input
    /// </summary>
    [SerializeField]
    MobileInputField SingleInput = null;

    /// <summary>
    /// Link to multi input
    /// </summary>
    [SerializeField]
    MobileInputField MultiInput = null;

    /// <summary>
    /// Cached read only flag
    /// </summary>
    bool _isReadOnly = false;

    /// <summary>
    /// Cached random
    /// </summary>
    readonly System.Random _random = new System.Random();

    /// <summary>
    /// Constructor
    /// </summary>
    void Awake() {
        MobileInput.Init();
        MobileInput.OnKeyboardAction += OnKeyboardAction;
        MobileInput.OnOrientationChange += OnOrientationChange;
    }

    /// <summary>
    /// Destructor
    /// </summary>
    void OnDestroy() {
        MobileInput.OnKeyboardAction -= OnKeyboardAction;
        MobileInput.OnOrientationChange -= OnOrientationChange;
    }

    /// <summary>
    /// Callback on change screen orientation
    /// </summary>
    void OnOrientationChange(HardwareOrientation orientation) {
        Debug.Log($"Orientation change = {orientation}");
    }

    /// <summary>
    /// Callback on keyboard show/hide
    /// </summary>
    void OnKeyboardAction(bool isShow, int height) {
        Debug.Log($"Keyboard action show is = {isShow}, height = {height}");
    }

    /// <summary>
    /// Callback on return button pressed
    /// </summary>
    public void OnReturn() {
        Debug.Log($"Return button pressed");
    }

    /// <summary>
    /// Generate random color
    /// </summary>
    Color GetRandomColor() {
        return new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
    }

    /// <summary>
    /// Switch read only mode to single input
    /// </summary>
    public void SwitchReadOnly() {
        _isReadOnly = !_isReadOnly;
        SingleInput.SetReadonly(_isReadOnly);
    }

    /// <summary>
    /// Set random content type to to single input
    /// </summary>
    public void SwitchContentType() {
        var length = Enum.GetNames(typeof(InputContentType)).Length;
        var type = (InputContentType)_random.Next(length);
        Debug.Log($"Content type: {type}");
        SingleInput.SetContentType(type);
    }

    /// <summary>
    /// Set random text color to single input
    /// </summary>
    public void SwitchTextColor() {
        SingleInput.SetTextColor(GetRandomColor());
    }

    /// <summary>
    /// Set random placeholder text color to single input
    /// </summary>
    public void SwitchPlacholderTextColor() {
        SingleInput.SetPlaceholderTextColor(GetRandomColor());
    }

    /// <summary>
    /// Set random background color to single input
    /// </summary>
    public void SwitchBackgrounColor() {
        SingleInput.SetBackgroundColor(GetRandomColor());
    }

    /// <summary>
    /// Set unfocus and hide keyboard
    /// </summary>
    public void HideKeyboard() {
        SingleInput.SetFocus(false);
        MultiInput.SetFocus(false);
    }

    /// <summary>
    /// Load next demo scene
    /// </summary>
    public void ShowChatDemo() {
        SceneManager.LoadScene("Chat");
    }
}
