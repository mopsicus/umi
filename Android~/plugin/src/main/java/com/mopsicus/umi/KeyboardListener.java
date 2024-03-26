package com.mopsicus.umi;

import android.util.Log;

import org.json.JSONException;
import org.json.JSONObject;

public class KeyboardListener implements KeyboardObserver {

    /**
     * Callback on keyboard show/hide
     *
     * @param height         Screen height
     * @param keyboardHeight Real keyboard height
     * @param orientation    Current orientation
     */
    @Override
    public void onKeyboardHeight(int height, int keyboardHeight, int orientation) {
        boolean isShow = (keyboardHeight > 0);
        JSONObject json = new JSONObject();
        try {
            json.put("action", "KEYBOARD");
            json.put("show", isShow);
            json.put("height", keyboardHeight);
        } catch (JSONException e) {
            if (Plugin.bridge.isDebug) {
                Log.e("[UMI]", String.format("on keyboard height error: %s", e));
            }
        }
        Plugin.bridge.sendData(json.toString());
    }

}
