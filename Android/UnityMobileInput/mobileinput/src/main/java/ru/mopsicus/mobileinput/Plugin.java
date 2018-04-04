package ru.mopsicus.mobileinput;

// ----------------------------------------------------------------------------
// The MIT License
// LeopotamGroupLibrary https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------

import android.app.Activity;
import android.graphics.Rect;
import android.view.View;
import android.view.ViewGroup;
import android.view.ViewTreeObserver;
import android.widget.RelativeLayout;

import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

public class Plugin {

    public static String handler = "mobileinput"; // plugin name, need for bridge

    public static Activity unityActivity;
    public static RelativeLayout mainLayout;
    private static ViewGroup topViewGroup;
    private static boolean previousState = true;
    private static String KEYBOARD_ACTION = "KEYBOARD_ACTION";

    private static View getLeafView(View view) {
        if (view instanceof ViewGroup) {
            ViewGroup vg = (ViewGroup)view;
            for (int i = 0; i < vg.getChildCount(); ++i) {
                View chview = vg.getChildAt(i);
                View result = getLeafView(chview);
                if (result != null)
                    return result;
            }
            return null;
        }
        else {
            return view;
        }
    }

    public static void init() {
        unityActivity = UnityPlayer.currentActivity;
        unityActivity.runOnUiThread(new Runnable() {
            public void run() {
                if (mainLayout != null) {
                    topViewGroup.removeView(mainLayout);
                }
                final ViewGroup rootView = (ViewGroup) unityActivity.findViewById (android.R.id.content);
                View topMostView = getLeafView(rootView);
                topViewGroup = (ViewGroup) topMostView.getParent();
                mainLayout = new RelativeLayout(unityActivity);
                RelativeLayout.LayoutParams rlp = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.MATCH_PARENT);
                topViewGroup.addView(mainLayout, rlp);
                rootView.getViewTreeObserver().addOnGlobalLayoutListener(new ViewTreeObserver.OnGlobalLayoutListener() {
                    @Override
                    public void onGlobalLayout() {
                        Rect r = new Rect();
                        rootView.getWindowVisibleDisplayFrame(r);
                        int screenHeight = rootView.getRootView().getHeight();
                        int keyboardHeight = screenHeight - r.bottom;
                        boolean show = (keyboardHeight > screenHeight * 0.15);
                        float height = (float) keyboardHeight / (float) screenHeight;
                        JSONObject json = new JSONObject();
                        try {
                            json.put("msg", KEYBOARD_ACTION);
                            json.put("show", show);
                            json.put("height", height);
                        } catch (JSONException e) {}
                        if (previousState != show) {
                            previousState = show;
                            Bridge.sendData(handler, json.toString());
                        }
                    }
                });
            }
        });
    }

    public static void destroy() {
        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            public void run() {
                topViewGroup.removeView(mainLayout);
            }
        });
    }

    public static void execute(final int id, final String data) {
        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            public void run() {
                MobileInput.processMessage(id, data);
            }
        });
    }

}
