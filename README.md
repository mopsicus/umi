<a href="./README.md">![Static Badge](https://img.shields.io/badge/english-118027)</a>
<a href="./README.ru.md">![Static Badge](https://img.shields.io/badge/русский-0390fc)</a>
<p align="center">
    <picture>
        <source media="(prefers-color-scheme: dark)" srcset="Media/logo-umi-dark.png">
        <source media="(prefers-color-scheme: light)" srcset="Media/logo-umi.png">
        <img alt="UMI (UnityMobileInput)" height="192" width="512" src="Media/logo-umi.png">
    </picture>
</p>
<h3 align="center">UMI</h3>
<h4 align="center">Unity plugin for Android and iOS, allows to use features of mobile native input fields</h4>
<p align="center">
    <a href="#quick-start">Quick start</a> · <a href="/Documentation~/index.md">Documentation</a> · <a href="https://github.com/mopsicus/umi/issues">Report Bug</a>
</p>

# 💬 Overview

This package allows you to use native input fields on Android and iOS. It works with TMP InputField component and uses some of its options.

### Problem

The current implementation of Unity's input field doesn't allow you to use selection, copy/paste, emoji, and other built-in features on mobile. And no one knows when that will happen or if it will happen at all.

### Solution

This plugin creates native input fields above the canvas UI with all platform features. Since this is a hack and the input fields are placed above the UI, you will have to control their visibility and position yourself. The plugin provides some useful options to make this more convenient.

# ✨ Features

- native input field and keyboard on iOS and Android
- select return button type: `Default`, `Next`, `Done`, `Search`, `Send`
- control return button callback
- control `Done` and `Clear` buttons (iOS)
- change some options in runtime
- hiding additional mobile input box (Android)
- detect keyboard show/hide (with height)
- detect screen orientation
- custom fonts support

# 🚀 Usage

### Installation

Get it from [releases page](https://github.com/mopsicus/umi/releases) or add the line to `Packages/manifest.json` and module will be installed directly from Git url:

```
"com.mopsicus.umi": "https://github.com/mopsicus/umi.git",
```

### Quick start

See the samples section to get a [demo app](./Samples~/Demo). This demo will show you how to initiate and use UMI in your app, how to create a chat-like app, and how to use a custom font. 

_Tested in Unity 2020.3.x, Android (API >= 24) and iOS._

### Initialization

Before creating the first input field, UMI must be initiated. It should create a special game object on the scene with a controller that will interact with native plugins.

To do this, add `UMI` to the `uses` section and call the init method, for example, in the `Awake` method in your app's entry point.

```csharp
using UnityEngine;
using UMI;

public class Bootstrap : MonoBehaviour {

    void Awake() {
        MobileInput.Init();
    }
    
}
```

> [!NOTE] 
> Make sure you do this before creating all input fields, otherwise UMI will raise an exception.

### Setup

To begin using UMI in your project, add `MobileInputField` script to game object with `TMP Input field`.

In the inspector, you can edit several options that will be applied to the native input field:

- text color
- placeholder text
- placeholder text color
- character limit
- font size
- text align
- content type
- input type
- keyboard type
- multiline option

From UMI, you can edit these additional options:

- background color
- return button type
- return button callback
- custom font
- manual hide option
- done & clear buttons option

### Keyboard and orientation callbacks

If you need to detect the appearance or hiding of the keyboard, you must subscribe to events and add a handler to your code.

```csharp
using UnityEngine;
using UMI;

public class Bootstrap : MonoBehaviour {

    void Awake() {
        MobileInput.Init();
        MobileInput.OnKeyboardAction += OnKeyboardAction;
        MobileInput.OnOrientationChange += OnOrientationChange;
    }

    void OnOrientationChange(HardwareOrientation orientation) {
        // raise when the screen orientation is changed
    }

    void OnKeyboardAction(bool isShow, int height) {
        // raise when the keyboard is displayed or hidden, and when the keyboard height is changed
    }
    
}
```

With `OnKeyboardAction` you can control UI elements, such as moving the input field as in chat apps. See the [demo app](./Samples~/Demo).

### Runtime methods

- `SetTextColor` – change text color
- `SetPlaceholderColor` – change placeholder text color
- `SetBackgroundColor` – change background color
- `SetContentType` – change input field content type
- `SetReadonly` – change readonly mode

### How to use custom fonts

1. Copy TTF fonts to `StreamingAssets` folder
2. Input font name in property instead `default`
3. Profit

When you first initialize, UMI will copy the fonts to a special app folder for your use. If you change the font(s) on the next update, you'll have to call `MobileInput.UpdateFonts()` to update the app folder with the fonts.

### iOS

The iOS plugin is simple, with only 3 files. If you want to know how it works under the hood - look for the `MobileInput.mm` file in the Plugins folder of the package and read the [docs](Documentation~/index.md).

### Android

Android plugin is a compiled AAR library. All sources are available in [Android~](./Android~/) folder. You can edit the android part and recompile the library to suit your needs.

Make sure your `AndroidManifest.xml` has the following setting:

```xml
<activity android:name="com.unity3d.player.UnityPlayerActivity" android:label="@string/app_name" android:windowSoftInputMode="adjustNothing">
    ...
    <meta-data android:name="unityplayer.ForwardNativeEventsToDalvik" android:value="true" />
    ...
</activity>
```

The `adjustNothing` option has been added to prevent the screen from shifting up when the keyboard is displayed.

### Unity

The Unity part contains a wrapper to call the native methods of the input field and an editor script to customize the parameters in inspector.

At initialization, the plugin creates a non-destructible game object for sending and receiving commands from the native part. When the `MobileInputField` script is used, the plugin registers it and creates a native field.

Data passed between the Unity app and native plugins is in JSON format. UMI uses [NiceJson](https://github.com/AngelQuirogaM/NiceJson) library for this purpose with some modifications.

# 🏗️ Contributing

We invite you to contribute and help improve UMI. Please see [contributing document](./CONTRIBUTING.md). 🤗

You also can contribute to the UMI project by:

- Helping other users 
- Monitoring the issue queue
- Sharing it to your socials
- Referring it in your projects

### Environment setup

For a better experience, you can set up an environment for local development. Since UMI is developed with VS Code, all settings are provided for it.

1. Use `Monokai Pro` or `eppz!` theme
2. Use `FiraCode` font
3. Install extensions:
    - C#
    - C# Dev Kit
    - Unity
4. Enable `Inlay Hints` in C# extension
5. Install `Visual Studio Editor` package in Unity
6. Put `.editorconfig` in root project directory
7. Be cool

# 🤝 Support

You can support the project by using any of the ways below:

* Bitcoin (BTC): 1VccPXdHeiUofzEj4hPfvVbdnzoKkX8TJ
* USDT (TRC20): TMHacMp461jHH2SHJQn8VkzCPNEMrFno7m
* TON: UQDVp346KxR6XxFeYc3ksZ_jOuYjztg7b4lEs6ulEWYmJb0f
* Visa, Mastercard via [Boosty](https://boosty.to/mopsicus/donate)
* MIR via [CloudTips](https://pay.cloudtips.ru/p/9f507669)
* [Paypal](https://www.paypal.me/mopsicus)

# ✉️ Contact

Before you ask a question, it is best to search for existing [issues](https://github.com/mopsicus/umi/issues) that might help you. Anyway, you can ask any questions and send suggestions by [email](mailto:mail@mopsicus.ru) or [Telegram](https://t.me/mopsicus).

# 🔑 License

UMI is licensed under the [MIT License](./LICENSE.md). Use it for free and be happy. 🎉