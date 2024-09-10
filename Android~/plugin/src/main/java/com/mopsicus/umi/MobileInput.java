package com.mopsicus.umi;

import android.content.Context;
import android.content.res.Configuration;
import android.content.res.Resources;
import android.graphics.Color;
import android.graphics.PorterDuff;
import android.graphics.Rect;
import android.graphics.Typeface;
import android.graphics.drawable.BitmapDrawable;
import android.graphics.drawable.Drawable;
import android.graphics.drawable.InsetDrawable;
import android.os.Build;
import android.os.LocaleList;
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
import android.widget.RelativeLayout.LayoutParams;
import android.widget.TextView;

import org.json.JSONException;
import org.json.JSONObject;

import java.lang.reflect.Field;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Locale;

public class MobileInput {

    private static final String CREATE = "CREATE_EDIT";
    private static final String REMOVE = "REMOVE_EDIT";
    private static final String SET_TEXT = "SET_TEXT";
    private static final String SET_CONTENT_TYPE = "SET_CONTENT_TYPE";
    private static final String SET_TEXT_COLOR = "SET_TEXT_COLOR";
    private static final String SET_PTEXT_COLOR = "SET_PTEXT_COLOR";
    private static final String SET_BG_COLOR = "SET_BG_COLOR";
    private static final String SET_READ_ONLY = "SET_READ_ONLY";
    private static final String SET_RECT = "SET_RECT";
    private static final String SET_FOCUS = "SET_FOCUS";
    private static final String ON_FOCUS = "ON_FOCUS";
    private static final String ON_UNFOCUS = "ON_UNFOCUS";
    private static final String SET_VISIBLE = "SET_VISIBLE";
    private static final String SET_LANGUAGE = "SET_LANGUAGE";
    private static final String TEXT_CHANGE = "TEXT_CHANGE";
    private static final String TEXT_END_EDIT = "TEXT_END_EDIT";
    private static final String ANDROID_KEY_DOWN = "ANDROID_KEY_DOWN";
    private static final String RETURN_PRESSED = "RETURN_PRESSED";
    private static final String READY = "READY";

    /**
     * Edit component
     */
    private EditText edit;

    /**
     * Input id
     */
    private int id;

    /**
     * Input layout
     */
    private final RelativeLayout layout;

    /**
     * Character limit for input
     */
    private int characterLimit;

    /**
     * Current input type
     */
    private int editInputType = 0;

    /**
     * Cached caret color
     */
    private int caretColor = Color.GRAY;

    /**
     * Cached flag to change caret to custom color or not
     */
    private boolean isCaretChange = false;

    /**
     * List of inputs
     */
    private static SparseArray<MobileInput> mobileInputList = null;

    /**
     * Constructor
     *
     * @param parentLayout Layout to create inputs
     */
    private MobileInput(RelativeLayout parentLayout) {
        layout = parentLayout;
        edit = null;
    }

    /**
     * Handler to process all messages for MobileInput
     *
     * @param id   Input id
     * @param data Data to process
     */
    public static void processMessage(int id, final String data) {
        if (mobileInputList == null) {
            mobileInputList = new SparseArray<>();
        }
        try {
            JSONObject json = new JSONObject(data);
            String msg = json.getString("msg");
            if (msg.equals(CREATE)) {
                MobileInput input = new MobileInput(Plugin.layout);
                input.Create(id, json);
                mobileInputList.append(id, input);
            } else {
                MobileInput input = mobileInputList.get(id);
                if (input != null) {
                    input.processData(json);
                }
            }
        } catch (JSONException e) {
            Plugin.bridge.sendError("RECEIVE_ERROR", e.getMessage());
        }
    }

    /**
     * Convert color from JSON
     *
     * @param data JSON object
     * @return Color
     */
    int getColor(JSONObject data) {
        try {
            int color_r = (int) (255.0f * data.getDouble("color_r"));
            int color_g = (int) (255.0f * data.getDouble("color_g"));
            int color_b = (int) (255.0f * data.getDouble("color_b"));
            int color_a = (int) (255.0f * data.getDouble("color_a"));
            return Color.argb(color_a, color_r, color_g, color_b);
        } catch (Exception e) {
            if (Plugin.bridge.isDebug) {
                Log.e("[UMI]", String.format("get color error: %s", e));
            }
            return Color.BLACK;
        }
    }

    /**
     * Process command for MobileInput
     *
     * @param data Data from Unity
     */
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
                case SET_TEXT_COLOR:
                    edit.setTextColor(this.getColor(data));
                    break;
                case SET_PTEXT_COLOR:
                    edit.setHintTextColor(this.getColor(data));
                    break;
                case SET_BG_COLOR:
                    edit.setBackgroundColor(this.getColor(data));
                    break;
                case SET_READ_ONLY:
                    boolean value = data.getBoolean("value");
                    edit.setInputType(value ? EditorInfo.TYPE_NULL : editInputType);
                    edit.setLongClickable(!value);
                    edit.setClickable(!value);
                    edit.setCursorVisible(!value);
                    break;
                case SET_CONTENT_TYPE:
                    String type = data.getString("type");
                    this.setContentType(type);
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
                case SET_LANGUAGE:
                    String code = data.getString("value");
                    setKeyboardLanguage(code);
                    break;
                case ANDROID_KEY_DOWN:
                    String strKey = data.getString("key");
                    this.OnForceAndroidKeyDown(strKey);
                    break;
            }
        } catch (JSONException e) {
            Plugin.bridge.sendError("PROCESS_ERROR", e.getMessage());
        }
    }

    /**
     * Set content type for input
     *
     * @param value Content type from component property
     */
    void setContentType(String value) {
        switch (value) {
            case "Autocorrected":
                editInputType |= InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_CAP_SENTENCES | InputType.TYPE_TEXT_FLAG_AUTO_CORRECT;
                break;
            case "IntegerNumber":
                editInputType |= InputType.TYPE_CLASS_NUMBER;
                break;
            case "DecimalNumber":
                editInputType |= InputType.TYPE_CLASS_NUMBER | InputType.TYPE_NUMBER_FLAG_DECIMAL;
                break;
            case "Alphanumeric":
                editInputType |= InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_CAP_SENTENCES;
                break;
            case "Name":
                editInputType |= InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PERSON_NAME;
                break;
            case "EmailAddress":
                editInputType |= InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS;
                break;
            case "Password":
                editInputType |= InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD;
                break;
            case "Pin":
                editInputType |= InputType.TYPE_CLASS_PHONE;
                break;
            default:
                editInputType = InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_CAP_SENTENCES | InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS;
                break;
        }
        edit.setInputType(editInputType);
    }

    /**
     * Create new MobileInput
     *
     * @param id   Input id
     * @param data Config
     */
    private void Create(int id, JSONObject data) {
        this.id = id;
        try {
            String placeHolder = data.getString("placeholder");
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
            int caretColor_r = (int) (255.0f * data.getDouble("caret_color_r"));
            int caretColor_g = (int) (255.0f * data.getDouble("caret_color_g"));
            int caretColor_b = (int) (255.0f * data.getDouble("caret_color_b"));
            int caretColor_a = (int) (255.0f * data.getDouble("caret_color_a"));
            int highlightColor_r = (int) (255.0f * data.getDouble("highlight_color_r"));
            int highlightColor_g = (int) (255.0f * data.getDouble("highlight_color_g"));
            int highlightColor_b = (int) (255.0f * data.getDouble("highlight_color_b"));
            int highlightColor_a = (int) (255.0f * data.getDouble("highlight_color_a"));
            String contentType = data.getString("content_type");
            String inputType = data.optString("input_type");
            String keyboardType = data.optString("keyboard_type");
            String keyboardLanguage = data.optString("keyboard_language");
            String returnKeyType = data.getString("return_key_type");
            String alignment = data.getString("align");
            String customFont = data.getString("font");
            boolean multiline = data.getBoolean("multiline");
            caretColor = Color.argb(caretColor_a, caretColor_r, caretColor_g, caretColor_b);
            isCaretChange = data.getBoolean("caret_color");
            edit = new EditText(Plugin.activity.getApplicationContext());
            edit.setSingleLine(!multiline);
            edit.setId(this.id);
            edit.setText("");
            edit.setHint(placeHolder);
            Rect rect = new Rect((int) x, (int) y, (int) (x + width), (int) (y + height));
            LayoutParams params = new LayoutParams(rect.width(), rect.height());
            params.setMargins(rect.left, rect.top, 0, 0);
            edit.setLayoutParams(params);
            edit.setPadding(0, 0, 0, 0);
            editInputType = 0;
            this.setContentType(contentType);
            if (contentType.equals("Custom")) {
                switch (keyboardType) {
                    case "ASCIICapable":
                        editInputType = InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS;
                        break;
                    case "NumbersAndPunctuation":
                        editInputType = InputType.TYPE_CLASS_NUMBER | InputType.TYPE_NUMBER_FLAG_DECIMAL | InputType.TYPE_NUMBER_FLAG_SIGNED;
                        break;
                    case "URL":
                        editInputType = InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS | InputType.TYPE_TEXT_VARIATION_URI;
                        break;
                    case "NumberPad":
                        editInputType = InputType.TYPE_CLASS_NUMBER;
                        break;
                    case "PhonePad":
                        editInputType = InputType.TYPE_CLASS_PHONE;
                        break;
                    case "NamePhonePad":
                        editInputType = InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PERSON_NAME;
                        break;
                    case "EmailAddress":
                        editInputType = InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS;
                        break;
                    case "Social":
                        editInputType = InputType.TYPE_TEXT_VARIATION_URI | InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS;
                        break;
                    case "Search":
                        editInputType = InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS | InputType.TYPE_NUMBER_FLAG_DECIMAL | InputType.TYPE_NUMBER_FLAG_SIGNED;
                        break;
                    default:
                        editInputType = InputType.TYPE_CLASS_TEXT;
                        break;
                }
                switch (inputType) {
                    case "Standard":
                        break;
                    case "AutoCorrect":
                        editInputType |= InputType.TYPE_TEXT_FLAG_AUTO_CORRECT;
                        break;
                    case "Password":
                        if (!keyboardType.equals("NumbersAndPunctuation") && !keyboardType.equals("NumberPad") && !keyboardType.equals("PhonePad")) {
                            editInputType |= InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD;
                        } else {
                            editInputType |= InputType.TYPE_NUMBER_VARIATION_PASSWORD;
                        }
                        break;
                }
            }
            if (multiline) {
                editInputType |= InputType.TYPE_TEXT_FLAG_MULTI_LINE | InputType.TYPE_TEXT_FLAG_CAP_SENTENCES;
            }
            edit.setInputType(editInputType);
            int gravity = 0;
            switch (alignment) {
                case "TopLeft":
                    gravity = Gravity.TOP | Gravity.LEFT;
                    break;
                case "Top":
                    gravity = Gravity.TOP | Gravity.CENTER_HORIZONTAL;
                    break;
                case "TopRight":
                    gravity = Gravity.TOP | Gravity.RIGHT;
                    break;
                case "Left":
                    gravity = Gravity.CENTER_VERTICAL | Gravity.LEFT;
                    break;
                case "Center":
                    gravity = Gravity.CENTER_VERTICAL | Gravity.CENTER_HORIZONTAL;
                    break;
                case "Right":
                    gravity = Gravity.CENTER_VERTICAL | Gravity.RIGHT;
                    break;
                case "BottomLeft":
                    gravity = Gravity.BOTTOM | Gravity.LEFT;
                    break;
                case "Bottom":
                    gravity = Gravity.BOTTOM | Gravity.CENTER_HORIZONTAL;
                    break;
                case "BottomRight":
                    gravity = Gravity.BOTTOM | Gravity.RIGHT;
                    break;
            }
            int imeOptions = EditorInfo.IME_FLAG_NO_EXTRACT_UI;
            switch (returnKeyType) {
                case "Next":
                    imeOptions |= EditorInfo.IME_ACTION_NEXT;
                    break;
                case "Done":
                    imeOptions |= EditorInfo.IME_ACTION_DONE;
                    break;
                case "Search":
                    imeOptions |= EditorInfo.IME_ACTION_SEARCH;
                    break;
                case "Send":
                    imeOptions |= EditorInfo.IME_ACTION_SEND;
                    break;
            }
            edit.setImeOptions(imeOptions);
            edit.setGravity(gravity);
            edit.setTextSize(TypedValue.COMPLEX_UNIT_PX, (float) fontSize);
            edit.setTextColor(Color.argb(textColor_a, textColor_r, textColor_g, textColor_b));
            edit.setBackgroundColor(Color.argb(backColor_a, backColor_r, backColor_g, backColor_b));
            edit.setHintTextColor(Color.argb(placeHolderColor_a, placeHolderColor_r, placeHolderColor_g, placeHolderColor_b));
            edit.setHighlightColor(Color.argb(highlightColor_a, highlightColor_r, highlightColor_g, highlightColor_b));
            if (!customFont.equals("default")) {
                try {
                    Typeface face = Typeface.createFromAsset(Plugin.activity.getAssets(), String.format("%s.ttf", customFont));
                    edit.setTypeface(face);
                } catch (Exception e) {
                    edit.setTypeface(Typeface.SANS_SERIF);
                }
            } else {
                edit.setTypeface(Typeface.SANS_SERIF);
            }
            if (!keyboardLanguage.equals("default")) {
                setKeyboardLanguage(keyboardLanguage);
            }
            final MobileInput input = this;
            edit.setOnFocusChangeListener((v, isFocus) -> {
                if (!isFocus) {
                    JSONObject editData = new JSONObject();
                    try {
                        editData.put("msg", TEXT_END_EDIT);
                        editData.put("text", input.GetText());
                    } catch (JSONException e) {
                        if (Plugin.bridge.isDebug) {
                            Log.e("[UMI]", String.format("create focus error: %s", e));
                        }
                    }
                    sendData(editData);
                }
                SetFocus(isFocus);
                JSONObject focusData = new JSONObject();
                try {
                    focusData.put("msg", (isFocus) ? ON_FOCUS : ON_UNFOCUS);
                } catch (JSONException e) {
                    if (Plugin.bridge.isDebug) {
                        Log.e("[UMI]", String.format("create set focus error: %s", e));
                    }
                }
                sendData(focusData);
            });
            edit.addTextChangedListener(new TextWatcher() {
                public void afterTextChanged(Editable s) {
                    JSONObject data = new JSONObject();
                    if (characterLimit > 0 && s.length() >= characterLimit + 1) {
                        s.delete(s.length() - 1, s.length());
                        edit.setText(s);
                        edit.setSelection(s.length());
                    }
                    try {
                        data.put("msg", TEXT_CHANGE);
                        data.put("text", s.toString());
                    } catch (JSONException e) {
                        if (Plugin.bridge.isDebug) {
                            Log.e("[UMI]", String.format("create change error: %s", e));
                        }
                    }
                    sendData(data);
                }

                @Override
                public void beforeTextChanged(CharSequence s, int start, int count, int after) {
                    // Auto-generated method stub

                }

                @Override
                public void onTextChanged(CharSequence s, int start, int before, int count) {
                    // Auto-generated method stub
                }
            });
            edit.setOnEditorActionListener((v, actionId, event) -> {
                if ((actionId == EditorInfo.IME_ACTION_DONE) || (actionId == EditorInfo.IME_ACTION_NEXT) || (actionId == EditorInfo.IME_ACTION_SEARCH) || (actionId == EditorInfo.IME_ACTION_SEND)) {
                    JSONObject returnData = new JSONObject();
                    try {
                        returnData.put("msg", RETURN_PRESSED);
                    } catch (JSONException e) {
                        if (Plugin.bridge.isDebug) {
                            Log.e("[UMI]", String.format("create editor error: %s", e));
                        }
                    }
                    sendData(returnData);
                    return true;
                }
                return false;
            });

            layout.addView(edit);
            data = new JSONObject();
            try {
                data.put("msg", READY);
            } catch (JSONException e) {
                if (Plugin.bridge.isDebug) {
                    Log.e("[UMI]", String.format("create ready error: %s", e));
                }
            }
            sendData(data);
        } catch (JSONException e) {
            if (Plugin.bridge.isDebug) {
                Log.e("[UMI]", String.format("create error: %s", e));
            }
            Plugin.bridge.sendError("CREATE_ERROR", e.getMessage());
        }
    }

    /***
     * Set cursor/caret/handles color
     *
     * @param color Color value
     */
    void setCaretColor(int color) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            if (edit.getTextCursorDrawable() instanceof InsetDrawable) {
                InsetDrawable insetDrawable = (InsetDrawable) edit.getTextCursorDrawable();
                insetDrawable.setColorFilter(color, PorterDuff.Mode.SRC_ATOP);
                edit.setTextCursorDrawable(insetDrawable);
                if (Plugin.bridge.isDebug) {
                    Log.d("[UMI]", String.format("set caret cursor: %s", color));
                }
            }
            if (edit.getTextSelectHandle() instanceof BitmapDrawable) {
                BitmapDrawable insetDrawable = (BitmapDrawable) edit.getTextSelectHandle();
                insetDrawable.setColorFilter(color, PorterDuff.Mode.SRC_ATOP);
                edit.setTextSelectHandle(insetDrawable);
                if (Plugin.bridge.isDebug) {
                    Log.d("[UMI]", String.format("set caret handle: %s", color));
                }
            }
            if (edit.getTextSelectHandleRight() instanceof BitmapDrawable) {
                BitmapDrawable insetDrawable = (BitmapDrawable) edit.getTextSelectHandleRight();
                insetDrawable.setColorFilter(color, PorterDuff.Mode.SRC_ATOP);
                edit.setTextSelectHandleRight(insetDrawable);
                if (Plugin.bridge.isDebug) {
                    Log.d("[UMI]", String.format("set caret handle right: %s", color));
                }
            }
            if (edit.getTextSelectHandleLeft() instanceof BitmapDrawable) {
                BitmapDrawable insetDrawable = (BitmapDrawable) edit.getTextSelectHandleLeft();
                insetDrawable.setColorFilter(color, PorterDuff.Mode.SRC_ATOP);
                edit.setTextSelectHandleLeft(insetDrawable);
                if (Plugin.bridge.isDebug) {
                    Log.d("[UMI]", String.format("set caret handle left: %s", color));
                }
            }
        } else {
            try {
                Resources res = edit.getContext().getResources();
                Field field = TextView.class.getDeclaredField("mEditor");
                field.setAccessible(true);
                Object editor = field.get(edit);
                field = TextView.class.getDeclaredField("mCursorDrawableRes");
                field.setAccessible(true);
                int cursorDrawableRes = field.getInt(edit);
                Drawable cursorDrawable = res.getDrawable(cursorDrawableRes).mutate();
                cursorDrawable.setColorFilter(color, PorterDuff.Mode.SRC_IN);
                Drawable[] drawables = {cursorDrawable, cursorDrawable};
                field = editor.getClass().getDeclaredField("mCursorDrawable");
                field.setAccessible(true);
                field.set(editor, drawables);
                String[] resFieldNames = {"mTextSelectHandleLeftRes", "mTextSelectHandleRightRes", "mTextSelectHandleRes"};
                String[] drawableFieldNames = {"mSelectHandleLeft", "mSelectHandleRight", "mSelectHandleCenter"};
                for (int i = 0; i < resFieldNames.length; i++) {
                    String resFieldName = resFieldNames[i];
                    String drawableFieldName = drawableFieldNames[i];
                    field = TextView.class.getDeclaredField(resFieldName);
                    field.setAccessible(true);
                    int selectHandleRes = field.getInt(edit);
                    Drawable selectHandleDrawable = res.getDrawable(selectHandleRes).mutate();
                    selectHandleDrawable.setColorFilter(color, PorterDuff.Mode.SRC_IN);
                    field = editor.getClass().getDeclaredField(drawableFieldName);
                    field.setAccessible(true);
                    field.set(editor, selectHandleDrawable);
                }
            } catch (Exception e) {
                if (Plugin.bridge.isDebug) {
                    Log.e("[UMI]", String.format("set caret error: %s", e));
                }
            }
        }
    }

    /**
     * Update locale list
     * 
     * @param languageCode New language code
     * @return Updated LocaleList
     */
    private LocaleList UpdateLocaleList(String languageCode) {
        LocaleList locales = edit.getImeHintLocales();
        if (locales == null) {
            return new LocaleList(new Locale(languageCode));
        }
        ArrayList<String> list = new ArrayList<>(Arrays.asList(locales.toLanguageTags().split(",")));
        if (!list.contains(languageCode)) {
            list.add(languageCode);
        }
        ArrayList<Locale> updated = new ArrayList<>();
        for (int i = 0; i < list.size(); i++) {
            updated.add(new Locale(list.get(i)));
        }
        return new LocaleList(updated.toArray(new Locale[updated.size()]));
    }

    /**
     * Set keyboard language for input
     *
     * @param languageCode Language ISO code
     */
    private void setKeyboardLanguage(String languageCode) {
        if (Plugin.bridge.isDebug) {
            Log.d("[UMI]", String.format("set keyboard language: %s", languageCode));
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
            edit.setImeHintLocales(UpdateLocaleList(languageCode));
        } else {
            Locale locale = new Locale(languageCode);
            Configuration config = new Configuration();
            config.locale = locale;
            Plugin.activity.getResources().updateConfiguration(config, null);
        }
        InputMethodManager imm = (InputMethodManager) Plugin.activity.getSystemService(Context.INPUT_METHOD_SERVICE);
        imm.restartInput(edit);
    }

    /**
     * Remove MobileInput
     */
    private void Remove() {
        if (edit != null) {
            layout.removeView(edit);
        }
        edit = null;
    }

    /**
     * Set new text
     *
     * @param newText Text for input
     */
    private void SetText(String newText) {
        if (edit != null) {
            edit.setText(newText);
        }
    }

    /**
     * Get text from MobileInput
     *
     * @return Text in input
     */
    private String GetText() {
        if (edit != null) {
            return edit.getText().toString();
        } else {
            return "";
        }
    }

    /**
     * Get focused state
     *
     * @return Input focus state
     */
    private boolean isFocused() {
        if (edit != null) {
            return edit.isFocused();
        } else {
            return false;
        }
    }

    /**
     * Set or clear focus to MobileInput
     *
     * @param isFocus State for input
     */
    private void SetFocus(boolean isFocus) {
        if (edit == null) {
            return;
        }
        if (isFocus) {
            edit.requestFocus();
            if (isCaretChange) {
                setCaretColor(caretColor);
            } else {
                setCaretColor(Color.GRAY);
            }
        } else {
            edit.clearFocus();
        }
        if (!isFocus) {
            for (int i = 0; i < mobileInputList.size(); i++) {
                int key = mobileInputList.keyAt(i);
                MobileInput input = mobileInputList.get(key);
                if (input.isFocused()) {
                    return;
                }
            }
        }
        this.showKeyboard(isFocus);
    }

    /**
     * Set new position and size
     *
     * @param data Position/size data
     */
    private void SetRect(JSONObject data) {
        try {
            double x = data.getDouble("x") * (double) layout.getWidth();
            double y = data.getDouble("y") * (double) layout.getHeight();
            double width = data.getDouble("width") * (double) layout.getWidth();
            double height = data.getDouble("height") * (double) layout.getHeight();
            Rect rect = new Rect((int) x, (int) y, (int) (x + width), (int) (y + height));
            LayoutParams params = new LayoutParams(rect.width(), rect.height());
            params.setMargins(rect.left, rect.top, 0, 0);
            edit.setLayoutParams(params);
        } catch (Exception e) {
            if (Plugin.bridge.isDebug) {
                Log.e("[UMI]", String.format("set rect error: %s", e));
            }
        }
    }

    /**
     * Set visible to MobileEdit
     *
     * @param isVisible Visible flag
     */
    private void SetVisible(boolean isVisible) {
        if (edit == null) {
            return;
        }
        edit.setVisibility(isVisible ? View.VISIBLE : View.INVISIBLE);
        if (isVisible) {
            edit.bringToFront();
        }
        edit.setEnabled(isVisible);
    }

    /**
     * Handler to process Android buttons
     *
     * @param strKey Key name
     */
    private void OnForceAndroidKeyDown(String strKey) {
        if (!this.isFocused()) {
            return;
        }
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

    /**
     * Show/hide keyboard
     *
     * @param isShow Keyboard state
     */
    private void showKeyboard(boolean isShow) {
        InputMethodManager imm = (InputMethodManager) Plugin.activity.getSystemService(Context.INPUT_METHOD_SERVICE);
        View rootView = Plugin.activity.getWindow().getDecorView();
        if (isShow) {
            imm.showSoftInput(edit, InputMethodManager.SHOW_FORCED);
        } else {
            edit.clearFocus();
            rootView.clearFocus();
            imm.hideSoftInputFromWindow(edit.getWindowToken(), 0);
        }
    }

    /**
     * Wrapper to send data to Unity app
     *
     * @param data Input id
     */
    private void sendData(JSONObject data) {
        try {
            data.put("id", this.id);
        } catch (JSONException e) {
            if (Plugin.bridge.isDebug) {
                Log.e("[UMI]", String.format("send data error: %s", e));
            }
        }
        Plugin.bridge.sendData(data.toString());
    }

}
