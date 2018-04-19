package ru.mopsicus.mobileinput;

// ----------------------------------------------------------------------------
// The MIT License
// LeopotamGroupLibrary https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------

import android.graphics.Color;
import android.graphics.Rect;
import android.graphics.Typeface;
import android.text.Editable;
import android.text.InputType;
import android.text.TextWatcher;
import android.util.Log;
import android.util.SparseArray;
import android.util.TypedValue;
import android.view.Gravity;
import android.view.KeyEvent;
import android.view.View;
import android.view.inputmethod.EditorInfo;
import android.view.inputmethod.InputMethodManager;
import android.widget.EditText;
import android.widget.RelativeLayout;
import android.widget.TextView;

import org.json.JSONException;
import org.json.JSONObject;

public class MobileInput {

    private EditText edit;
    private final RelativeLayout layout;
    private int tag;
    private int characterLimit;

    private static SparseArray<MobileInput> mapMobileInput = null;
    private static final String CREATE = "CREATE_EDIT";
    private static final String REMOVE = "REMOVE_EDIT";
    private static final String SET_TEXT = "SET_TEXT";
    private static final String SET_RECT = "SET_RECT";
    private static final String SET_FOCUS = "SET_FOCUS";
    private static final String SET_VISIBLE = "SET_VISIBLE";
    private static final String TEXT_CHANGE = "TEXT_CHANGE";
    private static final String TEXT_END_EDIT = "TEXT_END_EDIT";
    private static final String ANDROID_KEY_DOWN = "ANDROID_KEY_DOWN";
    private static final String RETURN_PRESSED = "RETURN_PRESSED";
    private static final String KEYBOARD_PREPARE = "KEYBOARD_PREPARE";
    private static final String READY = "READY";

    private MobileInput(RelativeLayout mainLayout) {
        layout = mainLayout;
        edit = null;
    }

    public static void processMessage(int id, final String data) {
        if (mapMobileInput == null)
            mapMobileInput = new SparseArray<>();
        try {
            JSONObject jsonMsg = new JSONObject(data);
            String msg = jsonMsg.getString("msg");
            if (msg.equals(CREATE)) {
                MobileInput input = new MobileInput(Plugin.mainLayout);
                input.Create(id, jsonMsg);
                mapMobileInput.append(id, input);
            } else {
                MobileInput input =  mapMobileInput.get(id);
                if (input != null) {
                    input.processData(jsonMsg);
                }
            }
        } catch (JSONException e) {
            Bridge.sendError(Plugin.handler, "RECEIVE_ERROR", e.getMessage());
        }
    }

    private void processData(JSONObject data) {
        try {
            String msg = data.getString("msg");
            switch (msg) {
                case REMOVE:
                    this.Remove();
                    break;
                case SET_TEXT:
                    String text = data.getString("text");
                    this.SetText(text);
                    break;
                case SET_RECT:
                    this.SetRect(data);
                    break;
                case SET_FOCUS:
                    boolean isFocus = data.getBoolean("is_focus");
                    this.SetFocus(isFocus);
                    break;
                case SET_VISIBLE:
                    boolean isVisible = data.getBoolean("is_visible");
                    this.SetVisible(isVisible);
                    break;
                case ANDROID_KEY_DOWN:
                    String strKey = data.getString("key");
                    this.OnForceAndroidKeyDown(strKey);
                    break;
            }

        } catch (JSONException e) {
            Bridge.sendError(Plugin.handler, "PROCESS_ERROR", e.getMessage());
        }
    }

    private void Create(int id, JSONObject data) {
        this.tag = id;
        try {
            String placeHolder = data.getString("placeholder");
            String font = data.getString("font");
            double fontSize = data.getDouble("font_size");
            double x = data.getDouble("x") * (double) layout.getWidth();
            double y = data.getDouble("y") * (double) layout.getHeight();
            double width = data.getDouble("width") * (double) layout.getWidth();
            double height = data.getDouble("height") * (double) layout.getHeight();
            characterLimit = data.getInt("character_limit");
            int textColor_r = (int) (255.0f * data.getDouble("text_color_r"));
            int textColor_g = (int) (255.0f * data.getDouble("text_color_g"));
            int textColor_b = (int) (255.0f * data.getDouble("text_color_b"));
            int textColor_a = (int) (255.0f * data.getDouble("text_color_a"));
            int backColor_r = (int) (255.0f * data.getDouble("back_color_r"));
            int backColor_g = (int) (255.0f * data.getDouble("back_color_g"));
            int backColor_b = (int) (255.0f * data.getDouble("back_color_b"));
            int backColor_a = (int) (255.0f * data.getDouble("back_color_a"));
            int placeHolderColor_r = (int) (255.0f * data.getDouble("placeholder_color_r"));
            int placeHolderColor_g = (int) (255.0f * data.getDouble("placeholder_color_g"));
            int placeHolderColor_b = (int) (255.0f * data.getDouble("placeholder_color_b"));
            int placeHolderColor_a = (int) (255.0f * data.getDouble("placeholder_color_a"));
            String contentType = data.getString("content_type");
            String inputType = data.optString("input_type");
            String keyboardType = data.optString("keyboard_type");
            String returnKeyType = data.getString("return_key_type");
            String alignment = data.getString("align");
            boolean multiline = data.getBoolean("multiline");
            edit = new EditText(Plugin.unityActivity.getApplicationContext());
            edit.setSingleLine(!multiline);
            edit.setId(0);
            edit.setText("");
            edit.setHint(placeHolder);
            Rect rect = new Rect((int) x, (int) y, (int) (x + width), (int) (y + height));
            RelativeLayout.LayoutParams params = new RelativeLayout.LayoutParams(rect.width(), rect.height());
            params.setMargins(rect.left, rect.top, 0, 0);
            edit.setLayoutParams(params);
            edit.setPadding(0, 0, 0, 0);
            int editInputType = 0;
            switch (contentType) {
                case "Standard" : editInputType |= InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_CAP_SENTENCES; break; // This is default behaviour
                case "Autocorrected" : editInputType |= InputType.TYPE_CLASS_TEXT  | InputType.TYPE_TEXT_FLAG_CAP_SENTENCES | InputType.TYPE_TEXT_FLAG_AUTO_CORRECT; break;
                case "IntegerNumber" : editInputType |= InputType.TYPE_CLASS_NUMBER; break;
                case "DecimalNumber" : editInputType |= InputType.TYPE_CLASS_NUMBER | InputType.TYPE_NUMBER_FLAG_DECIMAL; break;
                case "Alphanumeric" : editInputType |= InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_CAP_SENTENCES; break; // This is default behaviour
                case "Name" : editInputType |= InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PERSON_NAME; break;
                case "EmailAddress" : editInputType |= InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS; break;
                case "Password" : editInputType |= InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD; break;
                case "Pin" : editInputType |= InputType.TYPE_CLASS_PHONE; break;
                case "Custom" : // We need more details
                    switch (keyboardType) {
                        case "ASCIICapable" : editInputType = InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS; break;
                        case "NumbersAndPunctuation" : editInputType = InputType.TYPE_CLASS_NUMBER | InputType.TYPE_NUMBER_FLAG_DECIMAL |InputType.TYPE_NUMBER_FLAG_SIGNED; break;
                        case "URL" : editInputType = InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS | InputType.TYPE_TEXT_VARIATION_URI; break;
                        case "NumberPad" : editInputType = InputType.TYPE_CLASS_NUMBER;  break;
                        case "PhonePad" : editInputType = InputType.TYPE_CLASS_PHONE;  break;
                        case "NamePhonePad" : editInputType = InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PERSON_NAME; break;
                        case "EmailAddress" : editInputType = InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS; break;
                        default :  editInputType = InputType.TYPE_CLASS_TEXT; break;
                    }
                    if (multiline) editInputType  |=  InputType.TYPE_TEXT_FLAG_MULTI_LINE | InputType.TYPE_TEXT_FLAG_CAP_SENTENCES;
                    switch (inputType) {
                        case "AutoCorrect" : editInputType |= InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_AUTO_CORRECT; break;
                        case "Password" : editInputType |= InputType.TYPE_CLASS_TEXT | InputType.TYPE_NUMBER_VARIATION_PASSWORD | InputType.TYPE_TEXT_VARIATION_PASSWORD ; break;
                    }
                    break;
                default : editInputType |= InputType.TYPE_CLASS_TEXT;  // No action
                    break;
            }
            edit.setInputType(editInputType);
            int gravity = 0;
            switch (alignment) {
                case "UpperLeft":
                    gravity = Gravity.TOP | Gravity.LEFT;
                    break;
                case "UpperCenter":
                    gravity = Gravity.TOP | Gravity.CENTER_HORIZONTAL;
                    break;
                case "UpperRight":
                    gravity = Gravity.TOP | Gravity.RIGHT;
                    break;
                case "MiddleLeft":
                    gravity = Gravity.CENTER_VERTICAL | Gravity.LEFT;
                    break;
                case "MiddleCenter":
                    gravity = Gravity.CENTER_VERTICAL | Gravity.CENTER_HORIZONTAL;
                    break;
                case "MiddleRight":
                    gravity = Gravity.CENTER_VERTICAL | Gravity.RIGHT;
                    break;
                case "LowerLeft":
                    gravity = Gravity.BOTTOM | Gravity.LEFT;
                    break;
                case "LowerCenter":
                    gravity = Gravity.BOTTOM | Gravity.CENTER_HORIZONTAL;
                    break;
                case "LowerRight":
                    gravity = Gravity.BOTTOM | Gravity.RIGHT;
                    break;
            }
            int imeOptions = EditorInfo.IME_FLAG_NO_EXTRACT_UI;
            if (returnKeyType.equals("Next")) {
                imeOptions |= EditorInfo.IME_ACTION_NEXT;
            }
            else if (returnKeyType.equals("Done")) {
                imeOptions |= EditorInfo.IME_ACTION_DONE;
            }
            else if (returnKeyType.equals("Search")) {
                imeOptions |= EditorInfo.IME_ACTION_SEARCH;
            }
            edit.setImeOptions(imeOptions);
            edit.setGravity(gravity);
            edit.setTextSize(TypedValue.COMPLEX_UNIT_PX, (float) fontSize);
            edit.setTextColor(Color.argb(textColor_a, textColor_r, textColor_g, textColor_b));
            edit.setBackgroundColor(Color.argb(backColor_a, backColor_r, backColor_g, backColor_b));
            edit.setHintTextColor(Color.argb(placeHolderColor_a, placeHolderColor_r, placeHolderColor_g, placeHolderColor_b));
            if (font != null && !font.isEmpty()) {
                Typeface tf = Typeface.create(font, Typeface.NORMAL);
                edit.setTypeface(tf);
            }
            final MobileInput input = this;
            edit.setOnFocusChangeListener(new View.OnFocusChangeListener() {
                @Override
                public void onFocusChange(View v, boolean hasFocus) {
                    if (!hasFocus) {
                        JSONObject data = new JSONObject();
                        try {
                            data.put("msg", TEXT_END_EDIT);
                            data.put("text", input.GetText());
                        }
                        catch(JSONException e) {}
                        sendData(data);
                    }
                    SetFocus(hasFocus);
                }
            });
            edit.addTextChangedListener(new TextWatcher() {
                public void afterTextChanged(Editable s) {
                    JSONObject data = new JSONObject();
                    if(characterLimit > 0 && s.length() >= characterLimit+1) {
                        s.delete(s.length() - 1, s.length());
                        edit.setText(s);
                        edit.setSelection(s.length());
                    }
                    try {
                        data.put("msg", TEXT_CHANGE);
                        data.put("text", s.toString());
                    }
                    catch(JSONException e) {}
                    sendData(data);
                }

                @Override
                public void beforeTextChanged(CharSequence s, int start, int count, int after) {
                    // TODO Auto-generated method stub

                }

                @Override
                public void onTextChanged(CharSequence s, int start, int before, int count) {
                    // TODO Auto-generated method stub
                }
            });
            edit.setOnEditorActionListener(new TextView.OnEditorActionListener() {
                @Override
                public boolean onEditorAction(TextView v, int actionId, KeyEvent event) {
                    if ((actionId == EditorInfo.IME_ACTION_DONE) || (actionId == EditorInfo.IME_ACTION_NEXT) || (actionId == EditorInfo.IME_ACTION_SEARCH)) {
                        JSONObject data = new JSONObject();
                        try {
                            data.put("msg", RETURN_PRESSED);
                        }
                        catch(JSONException e) {}
                        sendData(data);
                        return true;
                    }
                    return false;
                }
            });

            layout.addView(edit);
            data = new JSONObject();
            try {
                data.put("msg", READY);
            }
            catch(JSONException e) {}
            sendData(data);
        } catch (JSONException e) {
            Bridge.sendError(Plugin.handler, "CREATE_ERROR", e.getMessage());
        }
    }

    private void Remove() {
        if (edit != null) {
            layout.removeView(edit);
        }
        edit = null;
    }

    private void SetText(String newText) {
        if (edit != null) {
            edit.setText(newText);
        }
    }
    private String GetText() {
        return edit.getText().toString();
    }

    private boolean isFocused() {
        return edit.isFocused();
    }

    private void SetFocus(boolean isFocus) {
        if (isFocus) {
            edit.requestFocus();
        } else {
            edit.clearFocus();
        }
        this.showKeyboard(isFocus);
    }

    private void SetRect(JSONObject data) {
        try {
            double x = data.getDouble("x") * (double) layout.getWidth();
            double y = data.getDouble("y") * (double) layout.getHeight();
            double width = data.getDouble("width") * (double) layout.getWidth();
            double height = data.getDouble("height") * (double) layout.getHeight();
            Rect rect = new Rect((int) x, (int) y, (int) (x + width), (int) (y + height));
            RelativeLayout.LayoutParams params = new RelativeLayout.LayoutParams(rect.width(), rect.height());
            params.setMargins(rect.left, rect.top, 0, 0);
            edit.setLayoutParams(params);
        } catch (JSONException e) {}
    }

    private void SetVisible(boolean bVisible) {
        edit.setEnabled(bVisible);
        edit.setVisibility(bVisible ? View.VISIBLE : View.INVISIBLE);
    }

    private void OnForceAndroidKeyDown(String strKey) {
        if (!this.isFocused())
            return;
        int keyCode = -1;
        if (strKey.equalsIgnoreCase("backspace")) {
            keyCode = KeyEvent.KEYCODE_DEL;
        } else if (strKey.equalsIgnoreCase("enter")) {
            keyCode = KeyEvent.KEYCODE_ENTER;
        } else if (strKey.equals("0")) {
            keyCode = KeyEvent.KEYCODE_0;
        } else if (strKey.equals("1")) {
            keyCode = KeyEvent.KEYCODE_1;
        } else if (strKey.equals("2")) {
            keyCode = KeyEvent.KEYCODE_2;
        } else if (strKey.equals("3")) {
            keyCode = KeyEvent.KEYCODE_3;
        } else if (strKey.equals("4")) {
            keyCode = KeyEvent.KEYCODE_4;
        } else if (strKey.equals("5")) {
            keyCode = KeyEvent.KEYCODE_5;
        } else if (strKey.equals("6")) {
            keyCode = KeyEvent.KEYCODE_6;
        } else if (strKey.equals("7")) {
            keyCode = KeyEvent.KEYCODE_7;
        } else if (strKey.equals("8")) {
            keyCode = KeyEvent.KEYCODE_8;
        } else if (strKey.equals("9")) {
            keyCode = KeyEvent.KEYCODE_9;
        }
        if (keyCode > 0) {
            KeyEvent ke = new KeyEvent(KeyEvent.ACTION_DOWN, keyCode);
            edit.onKeyDown(keyCode, ke);
        }
    }

    private void showKeyboard(boolean isShow) {
        InputMethodManager imm = (InputMethodManager) Plugin.unityActivity.getSystemService(Plugin.unityActivity.INPUT_METHOD_SERVICE);
        View rootView = Plugin.unityActivity.getWindow().getDecorView();
        if (isShow) {
            final MobileInput input = this;
            JSONObject data = new JSONObject();
            try {
                data.put("msg", KEYBOARD_PREPARE);
            }
            catch(JSONException e) {}
            sendData(data);
            imm.showSoftInput(edit, InputMethodManager.SHOW_FORCED);
        } else {
            edit.clearFocus();
            rootView.clearFocus();
            imm.hideSoftInputFromWindow(edit.getWindowToken(), 0);
        }
    }

    private void sendData(JSONObject data) {
        try {
            data.put("id", this.tag);
        }
        catch(JSONException e) {}
        Bridge.sendData(Plugin.handler, data.toString());
    }

}
