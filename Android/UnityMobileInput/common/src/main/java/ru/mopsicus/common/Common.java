// ----------------------------------------------------------------------------
// The MIT License
// UnityMobileInput https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------

package ru.mopsicus.common;

import org.json.JSONException;
import org.json.JSONObject;
import com.unity3d.player.UnityPlayer;

public class Common {

    String object = "Plugins";
    String receiver = "OnDataReceive";

    // Send data in JSON format to Unity
    public void sendData(String plugin, String data) {
        JSONObject info = new JSONObject();
        try {
            info.put("name", plugin);
            info.put("data", data);
        } catch (JSONException e) {
            e.printStackTrace();
        }
        UnityPlayer.UnitySendMessage(object, receiver, info.toString());
    }

    // Send error code without data
    public void sendError(String plugin, String code) {
        sendError(plugin, code, "");
    }

    // Send error in JSON format to Unity
    public void sendError(String plugin, String code, String data) {
        JSONObject error = new JSONObject();
        JSONObject info = new JSONObject();
        try {
            error.put("code", code);
            error.put("message", data);
            info.put("name", plugin);
            info.put("error", error);
        } catch (JSONException e) {
            e.printStackTrace();
        }
        UnityPlayer.UnitySendMessage(object, receiver, info.toString());
    }

}
