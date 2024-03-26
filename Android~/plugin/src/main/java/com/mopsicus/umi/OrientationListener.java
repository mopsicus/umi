package com.mopsicus.umi;

import android.content.res.Configuration;
import android.util.Log;

import org.json.JSONException;
import org.json.JSONObject;

public class OrientationListener implements OrientationObserver {

    /**
     * Callback on change orientation
     *
     * @param orientation    Current orientation
     */
    @Override
    public void onOrientationChanged(int orientation) {
        String current = (orientation == Configuration.ORIENTATION_PORTRAIT) ? "PORTRAIT" : "LANDSCAPE";
        JSONObject json = new JSONObject();
        try {
            json.put("action", "ORIENTATION");
            json.put("orientation", current);
        } catch (JSONException e) {
            if (Plugin.bridge.isDebug) {
                Log.e("[UMI]", String.format("orientation change error: %s", e));
            }
        }
        Plugin.bridge.sendData(json.toString());
    }

}
