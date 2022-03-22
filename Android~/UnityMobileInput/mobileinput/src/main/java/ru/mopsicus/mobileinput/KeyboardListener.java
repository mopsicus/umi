// ----------------------------------------------------------------------------
// The MIT License
// UnityMobileInput https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------

package ru.mopsicus.mobileinput;

import org.json.JSONException;
import org.json.JSONObject;

import ru.mopsicus.common.Common;

public class KeyboardListener implements KeyboardObserver {

    private boolean previousKeyboardShowState = false;

    /**
     * Previous keyboard height to indicate if the keyboard height changed
     */
    private int previousKeyboardHeight = -1;

    private Common common = new Common();

    @Override
    public void onKeyboardHeight(float height, int keyboardHeight, int orientation) {
        boolean isShow = (keyboardHeight > 0);
        JSONObject json = new JSONObject();
        try {
            json.put("msg", Plugin.KEYBOARD_ACTION);
            json.put("show", isShow);
            json.put("height", height);
        } catch (JSONException e) {

        }

        if (previousKeyboardShowState != isShow || previousKeyboardHeight != keyboardHeight) {
            previousKeyboardShowState = isShow;
            previousKeyboardHeight = keyboardHeight;
            common.sendData(Plugin.name, json.toString());
        }
    }

}

