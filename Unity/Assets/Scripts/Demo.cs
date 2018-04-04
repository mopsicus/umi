using UnityEngine;

public class Demo : MonoBehaviour {

	public void OnReturn () {
		Debug.Log ("OnReturn action");
	}

	public void OnEdit (string text) {
		Debug.LogFormat ("OnEdit action. Text: {0}", text);
	}

	public void OnEndEdit (string text) {
		Debug.LogFormat ("OnEdit action. Text: {0}", text);
	}

}