package com.mopsicus.umi;


import android.util.Log;

import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

public class Bridge {

    /**
     * GameObject on scene in Unity with handler
     */
    String object = "MobileInput";

    /**
     * Method name which will be process messages from plugin
     */
    String receiver = "OnDataReceive";

    /**
     * Flag to on/off debug log
     */
    boolean isDebug = false;

    /**
     * Send data in JSON format to Unity
     *
     * @param data   Data to send in Unity
     */
    @SuppressWarnings("unused")
    public void sendData(String data) {
        JSONObject info = new JSONObject();
        try {
            info.put("data", data);
        } catch (JSONException e) {
            if (isDebug) {
                Log.e("[UMI]", String.format("send data error: %s", e));
            }
        }
        UnityPlayer.UnitySendMessage(object, receiver, info.toString());
    }

    /**
     * Send error code without data
     *
     * @param code   Error code
     */
    @SuppressWarnings("unused")
    public void sendError(String code) {
        sendError(code, "");
    }

    /**
     * Send error in JSON format to Unity
     *
     * @param code   Error code
     * @param data   Error data
     */
    public void sendError(String code, String data) {
        JSONObject error = new JSONObject();
        JSONObject info = new JSONObject();
        try {
            error.put("code", code);
            error.put("message", data);
            info.put("error", error);
        } catch (JSONException e) {
            if (isDebug) {
                Log.e("[UMI]", String.format("send error error: %s", e));
            }
        }
        UnityPlayer.UnitySendMessage(object, receiver, info.toString());
    }

    /**
     * Init bridge
     *
     * @param object Object to send data
     * @param receiver Method name
     * @param isDebug On/off debug log
     */
    public void initialize(String object, String receiver, boolean isDebug) {
        this.object = object;
        this.receiver = receiver;
        this.isDebug = isDebug;
    }

}
