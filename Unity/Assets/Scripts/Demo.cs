using Mopsicus.Plugins.MobileInput;
using UnityEngine;
using UnityEngine.UI;

public class Demo : MonoBehaviour {

	public InputField InputText;

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
		InputText.GetComponent<MobileInput> ().Text = "Text by script";
	}

}