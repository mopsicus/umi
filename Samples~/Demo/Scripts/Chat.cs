using UMI;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Chat-like demo
/// </summary>
public class Chat : MonoBehaviour {

    /// <summary>
    /// Max lines count for InputBox
    /// </summary>
    const int MAX_LINES_COUNT = 5;

    /// <summary>
    /// Min input height
    /// </summary>
    const float MIN_INPUT_HEIGHT = 144f;

    /// <summary>
    /// Offset for lower position
    /// </summary>
    const float CONTAINER_BOTTOM_OFFSET = 160f;

    /// <summary>
    /// Min item height
    /// </summary>
    const int MIN_ITEM_HEIGHT = 128;

    /// <summary>
    /// Max count to calc lines in input
    /// </summary>
    const int MAX_LINE_CHARACTERS_COUNT = 33;

    /// <summary>
    /// Line height to add when calc item height with text
    /// </summary>
    const float LINE_HEIGHT = 48f;

    /// <summary>
    /// Link to multi input
    /// </summary>
    [SerializeField]
    MobileInputField MultiInput = null;

    /// <summary>
    /// Link to list container
    /// </summary>
    [SerializeField]
    RectTransform ListRect = null;

    /// <summary>
    /// Link to input container
    /// </summary>
    [SerializeField]
    RectTransform InputRect = null;

    /// <summary>
    /// Link to canvas
    /// </summary>
    [SerializeField]
    RectTransform CanvasRect = null;

    /// <summary>
    /// Chat item prefab
    /// </summary>
    [SerializeField]
    GameObject ChatItem = null;

    /// <summary>
    /// Link to content for chat items
    /// </summary>
    [SerializeField]
    RectTransform Content = null;

    /// <summary>
    /// Cached keyboard height
    /// </summary>
    float _currentHeight = 0;

    /// <summary>
    /// Flag to add random message to both sides
    /// </summary>
    bool _isMyMessage = true;

    /// <summary>
    /// Constructor
    /// </summary>
    void Awake() {
        MobileInput.OnKeyboardAction += OnKeyboardAction;
    }

    /// <summary>
    /// Destructor
    /// </summary>
    void OnDestroy() {
        MobileInput.OnKeyboardAction -= OnKeyboardAction;
    }

    /// <summary>
    /// Callback on keyboard show/hide
    /// </summary>
    /// <param name="isShow">State</param>
    /// <param name="height">Keyboard height</param>
    void OnKeyboardAction(bool isShow, int height) {
#if UMI_DEBUG
        Debug.Log($"Scale: {MobileInput.GetScreenScale()}");
#endif         
        var ratio = (float)Screen.height / CanvasRect.sizeDelta.y / MobileInput.GetScreenScale();
        var keyboardHeight = height / ratio;
#if UNITY_IOS
        var scale = MobileInput.GetScreenScale();
        if (scale >= 3f) {
            keyboardHeight *= 2.8f / scale;
        }
#endif
#if UMI_DEBUG
        Debug.Log($"Keyboard height: {keyboardHeight}");
        Debug.Log($"Ratio: {ratio}");
#endif 
        MoveInputBox(keyboardHeight);
        SetInputBoxLines(CalculateLines(MultiInput.Text.Trim()));
    }

    /// <summary>
    /// Add random chat item
    /// </summary>
    /// <param name="message">Message text</param>
    void AddChatItem(string message) {
        var item = Instantiate(ChatItem, Content);
        var controller = item.GetComponent<ChatItem>();
        controller.SetData(message, _isMyMessage);
        _isMyMessage = !_isMyMessage;
    }

    /// <summary>
    /// Send message
    /// </summary>
    public void Send() {
        AddChatItem(MultiInput.Text);
        MultiInput.Text = "";
        HideKeyboard();
        SetInputBoxLines(CalculateLines(""));
    }

    /// <summary>
    /// Callback when text changed
    /// </summary>
    public void OnMessageInputChanged(string value) {
        SetInputBoxLines(CalculateLines(value));
    }

    /// <summary>
    /// Calc how many lines in input box
    /// </summary>
    /// <param name="text">Message text</param>
    /// <returns>Lines count</returns>
    int CalculateLines(string text) {
        var count = 0;
        var lines = text.Split('\n');
        count = lines.Length;
        for (var i = 0; i < lines.Length; i++) {
            var size = lines[i].Length / MAX_LINE_CHARACTERS_COUNT;
            count += size;
        }
#if UMI_DEBUG
        Debug.Log($"Lines: {count}");
#endif        
        return count;
    }

    /// <summary>
    /// Click on area to hide keyboard
    /// </summary>
    public void HideKeyboard() {
        OnKeyboardAction(false, 0);
        MultiInput.SetFocus(false);
    }

    /// <summary>
    /// Set height for input box
    /// </summary>
    /// <param name="lines">Lines to show</param>
    public void SetInputBoxLines(int lines) {
        if (lines > MAX_LINES_COUNT) {
            lines = MAX_LINES_COUNT;
        }
        lines /= 2;
        var height = _currentHeight + MIN_INPUT_HEIGHT + LINE_HEIGHT * lines;
        InputRect.offsetMax = new Vector2(0f, height);
        InputRect.offsetMin = new Vector2(0f, _currentHeight);
        var min = ListRect.offsetMin;
        height = _currentHeight + CONTAINER_BOTTOM_OFFSET + LINE_HEIGHT * lines;
        var offset = (height > 0) ? height : CONTAINER_BOTTOM_OFFSET;
        ListRect.offsetMin = new Vector2(min.x, offset);
    }

    /// <summary>
    /// Move input box up/down on keyboard show/hide
    /// </summary>
    /// <param name="height">Value to move</param>
    public void MoveInputBox(float height) {
        _currentHeight = height;
        InputRect.offsetMax = new Vector2(0f, _currentHeight + InputRect.sizeDelta.y);
        InputRect.offsetMin = new Vector2(0f, _currentHeight);
        var min = ListRect.offsetMin;
        var offset = (_currentHeight > 0) ? InputRect.sizeDelta.y + _currentHeight : CONTAINER_BOTTOM_OFFSET;
        ListRect.offsetMin = new Vector2(min.x, offset);
    }

    /// <summary>
    /// Load previous demo scene
    /// </summary>
    public void ShowDemo() {
        SceneManager.LoadScene("Demo");
    }
}
