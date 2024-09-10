# UMI documentation

UMI aka UnityMobileInput is a hack for Unity that allows you to use native input fields on Android and iOS. Waiting for the days when it becomes redundant :)

## Introduction

UMI consists of three main parts: native plugins for Android and iOS and the Unity interface to manage them. Thus, if you need to add or change something, you'll have to do it in three places. All source code is open and commented. 

## Unity

To initialize the UMI, call `MobileInput.Init()`. Do this before using `MobileInputField` on any scene.

```csharp
using UnityEngine;
using UMI;

/// <summary>
/// It's a starter script 
/// </summary>
[DefaultExecutionOrder(-100)]
public class Bootstrap : MonoBehaviour {

    void Start() {
        MobileInput.Init();
        ...
    }

}
```

### MobileInput.cs

This script manages the interaction between the Unity app and native part. It contains references and identifiers of all MobileInputField scripts and some additional functions.

`Init()` – init UMI and prepare for work

`Destroy()` – destroy plugin and finish work

`GetScreenScale()` – get current screen scale, relevant for iOS

`UpdateFonts()` – update fonts in app folder when changing fonts list

`IsRotationLocked()` – check is screen rotation locked (Android)

`GetBarType()` – get navigation bar type (Android)

`GetBarHeight()` – get navigation bar height, if exist (Android)

`Action<bool, int> OnKeyboardAction` and `Action<HardwareOrientation> OnOrientationChange` – two events that you can subscribe to and detect keyboard appearance and orientation changes

```csharp
using UnityEngine;
using UMI;

public class ChatScreen : MonoBehaviour {

    void Awake() {
        MobileInput.OnKeyboardAction += OnKeyboardAction;
        MobileInput.OnOrientationChange += OnOrientationChange;
    }

    void OnDestroy() {
        MobileInput.OnKeyboardAction -= OnKeyboardAction;
        MobileInput.OnOrientationChange -= OnOrientationChange;        
    }

    void OnOrientationChange(HardwareOrientation orientation) {
        // raise when the screen orientation is changed
    }

    void OnKeyboardAction(bool isShow, int height) {
        // raise when the keyboard is displayed or hidden, and when the keyboard height is changed
    }
    
}
```

Be sure to unsubscribe from them to prevent exceptions!

### MobileInputField.cs

This is the basic script for using UMI. Add this script to a game object with `TMP Input field` and convert it to a native input field.

`InputField` – return TMP_InputField

`Visible` – return field visibility

`Text` – return and set field text 

`SetTextColor(Color color)` – set field text color

`SetPlaceholderTextColor(Color color)` – set placeholder text color

`SetBackgroundColor(Color color)` – set background color

`SetReadonly(bool value)` – set read only mode

`SetFocus(bool isFocus)` – set or unset focus to field

`SetVisible(bool isVisible)` – switch field visibility

`SetLanguage(string value)` – change keyboard language

`SetRect(RectTransform inputRect)` – set new field size and position, this is useful if you want to move or resize the input field manually, at other times this is done automatically using the game object parameters

`SetContentType(InputContentType type)` – set content type to field

`InputContentType` is an enum containing the standard field types for the mobile industry:

```csharp
public enum InputContentType {
    Standard,
    Autocorrected,
    IntegerNumber,
    DecimalNumber,
    Alphanumeric,
    Name,
    EmailAddress,
    Password,
    Pin
}
```
> [!NOTE] 
> Some of them don't work with multiline input fields

Many options from `TMP Input Field` will be applied to the native field. You can customize them as well.

- text color
- placeholder text
- placeholder text color
- cursor/caret color
- text selection color (Android, for iOS uses cursor/caret color)
- character limit
- font size
- text align
- content type
- input type
- keyboard type
- multiline option

### Debug

You can enable debug logging by adding `UMI_DEBUG` to the scripting define symbols in the Unity project settings.

It will show all commands sent and received from the native part.

## Android

Android part is a compiled AAR library. All sources are available in [Android~](../Android~/) folder. We are using Android Studio and Java to develop this part of plugin.

Current `compileSdk` is **34**, `minSdk` is **24**.

### Recompile

If you need to recompile AAR library, for example, change `compileSdk` or `minSdk`, you can do it by running configuration `:plugin [assemble]`. It will recompile lib, rename it and save to `!Releases` folder.

### Manifest

Patch manifest and make sure your `AndroidManifest.xml` has the following setting:

```xml
<activity android:name="com.unity3d.player.UnityPlayerActivity" android:label="@string/app_name" android:windowSoftInputMode="adjustNothing">
    ...
    <meta-data android:name="unityplayer.ForwardNativeEventsToDalvik" android:value="true" />
    ...
</activity>
```

The `adjustNothing` option has been added to prevent the screen from shifting up when the keyboard is displayed. 

> [!NOTE] 
> Activity name may vary

## iOS

iOS part is just one `MobileInput.mm` file with all classes and C wrapper. We are using Xcode and Objective-C to develop this part of plugin.

Different native input types are used for multiline and singleline options: `UITextView` and `UITextField`.

## Keyboard height explanation

When you get the keyboard height from `OnKeyboardAction`, you get the "true" height. To convert it to Unity UI height, you need to divide it by the screen aspect ratio. Screen aspect ratio is the ratio of the height of the screen to the height of the UI canvas and to the screen scale (for iOS). 

For Android builds, you can use the `GetBarType()` and `GetBarHeight()` methods to determine the screen device parameters for more flexible control.

See [demo app](../Samples~/Demo/) with chat screen.



