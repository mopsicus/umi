package ru.mopsicus.mobileinput;

// ----------------------------------------------------------------------------
// The MIT License
// LeopotamGroupLibrary https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------

import com.unity3d.player.UnityPlayer;
import org.json.JSONException;
import org.json.JSONObject;

public class Bridge {

    static String object = "Plugins";
    static String receiver = "OnDataReceive";

    // Send data in JSON format to Unity
    public static void sendData(String plugin, String data) {
        JSONObject info = new JSONObject();
        try {
            info.put("name", plugin);
            info.put("data", data);
        } catch (JSONException e) {
            e.printStackTrace();
        }
        UnityPlayer.UnitySendMessage(object, receiver, info.toString());
    }

    // Send error
    public static void sendError(String plugin, String code) {
        sendError(plugin, code, "");
    }

    // Send error in JSON format to Unity
    public static void sendError(String plugin, String code, String data) {
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
