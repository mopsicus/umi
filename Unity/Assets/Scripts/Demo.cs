using Mopsicus.Plugins;
using UnityEngine;
using UnityEngine.UI;

public class Demo : MonoBehaviour {

	public MobileInputField InputText;

	void Start () {
		MobileInput.OnPrepareKeyboard += OnPrepareKeyboard;
		MobileInput.OnShowKeyboard += OnShowKeyboard;
	}

	public void OnReturn () {
		Debug.Log ("OnReturn action");
	}

	public void OnEdit (string text) {
		Debug.LogFormat ("OnEdit action. Text: {0}", text);
	}

	public void OnEndEdit (string text) {
		Debug.LogFormat ("OnEdit action. Text: {0}", text);
	}

	public void SetTextData () {
		InputText.Text = "Text by script";
	}

	void OnPrepareKeyboard () {
		Debug.LogFormat ("Keyboad will show");
	}

	void OnShowKeyboard (bool isShow, int height) {
		Debug.LogFormat ("Keyboad action, show = {0}, height = {1}", isShow, height);
	}

}