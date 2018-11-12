// ----------------------------------------------------------------------------
// The MIT License
// UnityMobileInput https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------

package ru.mopsicus.mobileinput;

import android.app.Activity;
import android.graphics.Rect;
import android.view.View;
import android.view.ViewGroup;
import android.view.ViewTreeObserver;
import android.widget.RelativeLayout;
import android.widget.RelativeLayout.LayoutParams;

import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

import ru.mopsicus.common.Common;

public class Plugin {

    public static String name = "mobileinput";

    public static Activity activity;
    public static RelativeLayout layout;
    public static Common common;
    private static ViewGroup group;
    private static boolean isPreviousState = true;
    private static String KEYBOARD_ACTION = "KEYBOARD_ACTION";

    // Get view recursive
    private static View getLeafView(View view) {
        if (view instanceof ViewGroup) {
            ViewGroup viewGroup = (ViewGroup)view;
            for (int i = 0; i < viewGroup.getChildCount(); ++i) {
                View result = getLeafView(viewGroup.getChildAt(i));
                if (result != null) {
                    return result;
                }
            }
            return null;
        }
        else {
            return view;
        }
    }

    // Init plugin, create layout for MobileInputs
    public static void init() {
        common = new Common();
        activity = UnityPlayer.currentActivity;
        activity.runOnUiThread(new Runnable() {
            public void run() {
                if (layout != null) {
                    group.removeView(layout);
                }
                final ViewGroup rootView = (ViewGroup) activity.findViewById (android.R.id.content);
                View topMostView = getLeafView(rootView);
                group = (ViewGroup) topMostView.getParent();
                layout = new RelativeLayout(activity);
                LayoutParams params = new LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.MATCH_PARENT);
                group.addView(layout, params);
                rootView.getViewTreeObserver().addOnGlobalLayoutListener(new ViewTreeObserver.OnGlobalLayoutListener() {
                    @Override
                    public void onGlobalLayout() {
                        Rect rect = new Rect();
                        rootView.getWindowVisibleDisplayFrame(rect);
                        int screenHeight = rootView.getRootView().getHeight();
                        int keyboardHeight = screenHeight - rect.bottom;
                        boolean isShow = (keyboardHeight > screenHeight * 0.15);
                        float height = (float) keyboardHeight / (float) screenHeight;
                        JSONObject json = new JSONObject();
                        try {
                            json.put("msg", KEYBOARD_ACTION);
                            json.put("show", isShow);
                            json.put("height", height);
                        } catch (JSONException e) {}
                        if (isPreviousState != isShow) {
                            isPreviousState = isShow;
                            common.sendData(name, json.toString());
                        }
                    }
                });
            }
        });
    }

    // Destroy plugin, remove layout
    public static void destroy() {
        activity.runOnUiThread(new Runnable() {
            public void run() {
                if (layout != null) {
                    group.removeView(layout);
                }
            }
        });
    }

    // Send data to MobileInput
    public static void execute(final int id, final String data) {
        activity.runOnUiThread(new Runnable() {
            public void run() {
                MobileInput.processMessage(id, data);
            }
        });
    }

}
