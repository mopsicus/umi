package com.mopsicus.umi;


import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.res.Configuration;
import android.content.res.Resources;
import android.graphics.Rect;
import android.graphics.drawable.ColorDrawable;
import android.util.DisplayMetrics;
import android.view.Display;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.PopupWindow;

public class KeyboardProvider extends PopupWindow {

    /**
     * Observer keyboard to send data
     */
    private final KeyboardObserver keyboardObserver;

    /**
     * Observer orientation to send data
     */
    private final OrientationObserver orientationObserver;

    /**
     * Popup view for process
     */
    private final View popupView;

    /**
     * Main activity
     */
    private final Activity activity;

    /**
     * Max height for landscape
     */
    private int heightLandscapeMax;

    /**
     * Max height for portrait
     */
    private int heightPortraitMax;

    /**
     * Height of navigation bar
     */
    private final int navBarHeight;

    /**
     * Cached last orientation
     */
    private int lastOrientation;

    /**
     * Cached last height
     */
    private int lastHeight;

    /**
     * Constructor
     *
     * @param activity            Main Unity activity
     * @param parent              Parent view to create popup fake
     * @param keyboardListener    Listener class to get callback
     * @param orientationListener Listener class to get callback
     */
    public KeyboardProvider(Activity activity, ViewGroup parent, KeyboardObserver keyboardListener, OrientationObserver orientationListener) {
        super(activity);
        this.keyboardObserver = keyboardListener;
        this.orientationObserver = orientationListener;
        this.activity = activity;
        Resources resources = this.activity.getResources();
        String packageName = this.activity.getPackageName();
        @SuppressLint("DiscouragedApi") int id = resources.getIdentifier("popup", "layout", packageName);
        LayoutInflater inflater = (LayoutInflater) activity.getSystemService(Activity.LAYOUT_INFLATER_SERVICE);
        this.popupView = inflater.inflate(id, null, false);
        setContentView(popupView);
        setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_ADJUST_RESIZE | WindowManager.LayoutParams.SOFT_INPUT_STATE_ALWAYS_VISIBLE);
        setInputMethodMode(PopupWindow.INPUT_METHOD_NEEDED);
        setWidth(0);
        setHeight(WindowManager.LayoutParams.MATCH_PARENT);
        setBackgroundDrawable(new ColorDrawable(0));
        showAtLocation(parent, Gravity.NO_GRAVITY, 0, 0);
        navBarHeight = getNavigationBarHeight();
        lastOrientation = getScreenOrientation();
        popupView.getViewTreeObserver().addOnGlobalLayoutListener(this::handleOnGlobalLayout);
    }

    /**
     * Close fake popup
     */
    public void disable() {
        dismiss();
    }

    /**
     * Return screen orientation
     *
     * @return Orientation 0 or 1
     */
    private int getScreenOrientation() {
        return activity.getResources().getConfiguration().orientation;
    }

    /**
     * Get height of navigation bar
     */
    private int getNavigationBarHeight() {
        return 0;
/*
        On all modern devices works correctly
        With Unity full screen mode app
        if (!checkSoftKeys() || getNavBarNavigationType() == 2) {
            return 0;
        }
        Resources resources = activity.getResources();
        int resourceId = resources.getIdentifier("navigation_bar_height", "dimen", "android");
        if (resourceId > 0) {
            return resources.getDimensionPixelSize(resourceId);
        }
        return 0;
*/
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
    private int getNavBarNavigationType() {
        Resources resources = activity.getResources();
        @SuppressLint("DiscouragedApi") int resourceId = resources.getIdentifier("config_navBarInteractionMode", "integer", "android");
        if (resourceId > 0) {
            return resources.getInteger(resourceId);
        }
        return 0;
    }

    /**
     * Check soft keys on screen
     */
    @SuppressWarnings("unused")
    private boolean checkSoftKeys() {
        Display display = activity.getWindowManager().getDefaultDisplay();
        DisplayMetrics realDisplayMetrics = new DisplayMetrics();
        display.getRealMetrics(realDisplayMetrics);
        int realHeight = realDisplayMetrics.heightPixels;
        int realWidth = realDisplayMetrics.widthPixels;
        DisplayMetrics displayMetrics = new DisplayMetrics();
        display.getMetrics(displayMetrics);
        int displayHeight = displayMetrics.heightPixels;
        int displayWidth = displayMetrics.widthPixels;
        return (realWidth - displayWidth) > 0 || (realHeight - displayHeight) > 0;
    }

    /**
     * Handler to get keyboard height
     */
    private void handleOnGlobalLayout() {
        Rect rect = new Rect();
        popupView.getWindowVisibleDisplayFrame(rect);
        int orientation = getScreenOrientation();
        int keyboardHeight = 0;
        switch (orientation) {
            case Configuration.ORIENTATION_PORTRAIT:
                if (rect.bottom > heightPortraitMax) {
                    heightPortraitMax = rect.bottom;
                }
                keyboardHeight = heightPortraitMax - rect.bottom;
                break;
            case Configuration.ORIENTATION_LANDSCAPE:
                if (rect.bottom > heightLandscapeMax) {
                    heightLandscapeMax = rect.bottom;
                }
                keyboardHeight = heightLandscapeMax - rect.bottom;
                break;
            default:
                break;
        }
        if (keyboardHeight > 0) {
            keyboardHeight += navBarHeight;
        }
        int height = (orientation == Configuration.ORIENTATION_PORTRAIT) ? heightPortraitMax : heightLandscapeMax;
        if (lastHeight != keyboardHeight) {
            lastHeight = keyboardHeight;
            notifyKeyboardHeight(height, keyboardHeight, orientation);
        }
        if (lastOrientation != orientation) {
            lastOrientation = orientation;
            notifyOrientationChanged(orientation);
        }
    }

    /**
     * Send data observer
     *
     * @param height         Screen height
     * @param keyboardHeight Real height
     * @param orientation    Current orientation
     */
    private void notifyKeyboardHeight(int height, int keyboardHeight, int orientation) {
        if (keyboardObserver != null) {
            keyboardObserver.onKeyboardHeight(height, keyboardHeight, orientation);
        }
    }

    /**
     * Send data to observer
     *
     * @param orientation Current orientation
     */
    private void notifyOrientationChanged(int orientation) {
        if (orientationObserver != null) {
            orientationObserver.onOrientationChanged(orientation);
        }
    }
}
