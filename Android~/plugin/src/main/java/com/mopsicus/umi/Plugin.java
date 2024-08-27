package com.mopsicus.umi;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.provider.Settings;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;
import android.widget.RelativeLayout;
import android.widget.RelativeLayout.LayoutParams;

import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;


public class Plugin {

    /**
     * Main Unity activity
     */
    @SuppressLint("StaticFieldLeak")
    public static Activity activity;

    /**
     * Parent layout for MobileInputs
     */
    @SuppressLint("StaticFieldLeak")
    public static RelativeLayout layout;

    /**
     * View for create layout
     */
    @SuppressLint("StaticFieldLeak")
    private static ViewGroup group;

    /**
     * Keyboard provider for receive show/hide events
     */
    @SuppressLint("StaticFieldLeak")
    private static KeyboardProvider keyboardProvider;

    /**
     * Keyboard lister class to send to Unity
     */
    private static KeyboardListener keyboardListener;

    /**
     * Orientation lister class to send to Unity
     */
    private static OrientationListener orientationListener;

    /**
     * Bridge for send to Unity
     */
    public static Bridge bridge;

    /**
     * Get view recursive
     *
     * @param view View to start search
     * @return Last view
     */
    private static View getLeafView(View view) {
        if (view instanceof ViewGroup) {
            ViewGroup viewGroup = (ViewGroup) view;
            for (int i = 0; i < viewGroup.getChildCount(); ++i) {
                View result = getLeafView(viewGroup.getChildAt(i));
                if (result != null) {
                    return result;
                }
            }
            return null;
        } else {
            return view;
        }
    }

    /**
     * Init plugin, create layout for MobileInputs
     *
     * @param data JSON data
     */
    @SuppressWarnings("unused")
    public static void init(final String data) {
        bridge = new Bridge();
        try {
            JSONObject json = new JSONObject(data);
            String object = json.getString("object");
            String receiver = json.getString("receiver");
            boolean isDebug = json.getBoolean("debug");
            bridge.initialize(object, receiver, isDebug);
        } catch (JSONException e) {
            Log.e("[UMI]", String.format("init error: %s", e));
        }
        activity = UnityPlayer.currentActivity;
        activity.runOnUiThread(() -> {
            if (layout != null) {
                group.removeView(layout);
            }
            ViewGroup rootView = activity.findViewById(android.R.id.content);
            View topMostView = getLeafView(rootView);
            group = (ViewGroup) topMostView.getParent();
            layout = new RelativeLayout(activity);
            LayoutParams params = new LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.MATCH_PARENT);
            group.addView(layout, params);
            orientationListener = new OrientationListener();
            keyboardListener = new KeyboardListener();
            keyboardProvider = new KeyboardProvider(activity, group, keyboardListener, orientationListener);
        });
    }

    /**
     * Get height of navigation bar
     *
     * @return int of value
     */
    @SuppressWarnings("unused")
    public static int getBarHeight() {
        return keyboardProvider.getNavBarHeight();
    }

    /**
     * Return type of screen navigation
     * 0 : Navigation is displaying with 3 buttons
     * 1 : Navigation is displaying with 2 button(Android P navigation mode)
     * 2 : Full screen gesture(Gesture on android Q)
     *
     * @return int of type
     */
    @SuppressWarnings("unused")
    public static int getBarType() {
        return keyboardProvider.getNavBarNavigationType();
    }

    /**
     * Destroy plugin, remove layout
     */
    @SuppressWarnings("unused")
    public static void destroy() {
        activity.runOnUiThread(() -> {
            if (layout != null) {
                group.removeView(layout);
            }
            keyboardProvider.disable();
            keyboardProvider = null;
            keyboardListener = null;
            orientationListener = null;
        });
    }

    /**
     * Send data to MobileInput
     *
     * @param id   Input id
     * @param data Data to process
     */
    @SuppressWarnings("unused")
    public static void execute(final int id, final String data) {
        activity.runOnUiThread(() -> MobileInput.processMessage(id, data));
    }

    /**
     * Check device rotate locking
     */
    @SuppressWarnings("unused")
    public static boolean checkIsRotateLocked() {
        int val = Settings.System.getInt(UnityPlayer.currentActivity.getContentResolver(), Settings.System.ACCELEROMETER_ROTATION, 0);
        return (val != 1);
    }

}
