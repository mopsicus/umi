#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>
#import <CoreText/CoreText.h>
#import "UnityForwardDecls.h"
#import <MobileCoreServices/UTCoreTypes.h>
#import "Bridge.h"

#define CREATE @"CREATE_EDIT"
#define REMOVE @"REMOVE_EDIT"
#define SET_TEXT @"SET_TEXT"
#define SET_CONTENT_TYPE @"SET_CONTENT_TYPE"
#define SET_TEXT_COLOR @"SET_TEXT_COLOR"
#define SET_PTEXT_COLOR @"SET_PTEXT_COLOR"
#define SET_BG_COLOR @"SET_BG_COLOR"
#define SET_READ_ONLY @"SET_READ_ONLY"
#define SET_LANGUAGE @"SET_LANGUAGE"
#define SET_RECT @"SET_RECT"
#define ON_FOCUS @"ON_FOCUS"
#define ON_UNFOCUS @"ON_UNFOCUS"
#define SET_FOCUS @"SET_FOCUS"
#define SET_VISIBLE @"SET_VISIBLE"
#define TEXT_CHANGE @"TEXT_CHANGE"
#define TEXT_END_EDIT @"TEXT_END_EDIT"
#define RETURN_PRESSED @"RETURN_PRESSED"
#define READY @"READY"
#define KEYBOARD_ACTION @"KEYBOARD"
#define ORIENTATION_ACTION @"ORIENTATION"

/// Link to main view controller
UIViewController *mainViewController = nil;

/// Dict with inputs
NSMutableDictionary *mobileInputList = nil;

/// Custom textfield with overridden input mode
@interface CustomTextField : UITextField

/// Language code
@property (nonatomic, strong) NSString *languageCode;
@end

/// Interface for placeholder
@interface PlaceholderTextView : UITextView

/// Placeholder text
@property(nonatomic, strong) NSString *placeholder;

/// Text color
@property(nonatomic, strong) UIColor *realTextColor UI_APPEARANCE_SELECTOR;

/// Placeholder text color
@property(nonatomic, strong) UIColor *placeholderColor UI_APPEARANCE_SELECTOR;

/// Language code
@property (nonatomic, strong) NSString *languageCode;
@end

/// MobileInput interface
@interface MobileInput : NSObject <UITextFieldDelegate, UITextViewDelegate> {
    
    /// Current input id
    int inputId;
    
    /// Input character limit
    int characterLimit;
    
    /// Flat to check multiline input
    BOOL isMultiline;
    
    /// Current edit component
    UIView *editView;
    
    /// Link to view controller
    UIViewController *viewController;
    
    /// Cached done button view
    UIToolbar *keyboardDoneButtonView;
    
    /// Cached done button item
    UIBarButtonItem *doneButton;
    
    /// Cached placeholder range
    NSRange placeholderRange;
}

/// Init MobileInput
/// - Parameter viewController: Current view controller
+ (void)init:(UIViewController *)viewController;

/// Process in messages from Unity app
/// - Parameters:
///   - inputId: Input ID
///   - data: JSON data
+ (void)processMessage:(int)inputId data:(NSString *)data;

/// Destroy component
+ (void)destroy;

/// Init with input ID
/// - Parameters:
///   - controller: Current view controller
///   - inputId: Input ID
- (id)initWith:(UIViewController *)controller andTag:(int)inputId;

/// Create native input field
/// - Parameter data: Options data
- (void)create:(NSDictionary *)data;

/// Action with current input field
/// - Parameter data: Action data
- (void)processData:(NSDictionary *)data;

/// Show/hide keyboard
/// - Parameter value: true/false
- (void)showKeyboard:(BOOL)value;

/// Get focus state
- (BOOL)isFocused;
@end

/// Custom textfield implemenation
@implementation CustomTextField

/// Set language for keyboard
/// - Parameter languageCode: ISO code
- (void)setLanguageCode:(NSString *)languageCode {
    _languageCode = languageCode;
    if ([self isFirstResponder]) {
        [self resignFirstResponder];
        [self becomeFirstResponder];
    }
}

/// Overridden input
- (UITextInputMode *)textInputMode {
    if (self.languageCode) {
        for (UITextInputMode *keyboard in UITextInputMode.activeInputModes) {
            if (keyboard.primaryLanguage) {
                NSLocale *locale = [[NSLocale alloc] initWithLocaleIdentifier:keyboard.primaryLanguage];
                if ([locale.languageCode isEqualToString:self.languageCode]) {
                    return keyboard;
                }
            }
        }
    }
    return [super textInputMode];
}

@end

/// Placeholder interface
@interface PlaceholderTextView ()

/// Text in placeholder
@property(unsafe_unretained, nonatomic, readonly) NSString *realText;

/// Notification when edit begins
/// - Parameter notification: Notification data
- (void)beginEditing:(NSNotification *)notification;

/// Notification when edit end
/// - Parameter notification: Notification data
- (void)endEditing:(NSNotification *)notification;
@end

/// Placeholder implimentation
@implementation PlaceholderTextView

/// Synthesize real color
@synthesize realTextColor;

/// Synthesize item
@synthesize placeholder;

/// Synthesize color
@synthesize placeholderColor;


/// Init placeholder
/// - Parameter frame: Placeholder sizes and bounds
- (id)initWithFrame:(CGRect)frame {
    if ((self = [super initWithFrame:frame])) {
        [self awakeFromNib];
    }
    return self;
}

/// Constructor
- (void)awakeFromNib {
    [super awakeFromNib];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(beginEditing:) name:UITextViewTextDidBeginEditingNotification object:self];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(endEditing:) name:UITextViewTextDidEndEditingNotification object:self];
    self.realTextColor = self.textColor;
    self.placeholderColor = [UIColor lightGrayColor];
}

/// Set text to placeholder
/// - Parameter textPlaceholder: Text
- (void)setPlaceholder:(NSString *)textPlaceholder {
    if ([self.realText isEqualToString:placeholder] && ![self isFirstResponder]) {
        self.text = textPlaceholder;
    }
    if (textPlaceholder != placeholder) {
        placeholder = textPlaceholder;
    }
    [self endEditing:nil];
}

/// Set color to placeholder text
/// - Parameter colorPlaceholder: Color
- (void)setPlaceholderColor:(UIColor *)colorPlaceholder {
    placeholderColor = colorPlaceholder;
    if ([super.text isEqualToString:self.placeholder]) {
        self.textColor = self.placeholderColor;
    }
}

/// Get placeholder text
- (NSString *)text {
    NSString *text = [super text];
    return ([text isEqualToString:self.placeholder]) ? @"" : text;
}

/// Set text to item
/// - Parameter text: Text to set
- (void)setText:(NSString *)text {
    if (([text isEqualToString:@""] || text == nil) && ![self isFirstResponder]) {
        super.text = self.placeholder;
    } else {
        super.text = text;
    }
    if ([text isEqualToString:self.placeholder] || text == nil) {
        self.textColor = self.placeholderColor;
    } else {
        self.textColor = self.realTextColor;
    }
}

/// Get real text
- (NSString *)realText {
    return [super text];
}

/// Edit method
/// - Parameter notification: Notification data
- (void)beginEditing:(NSNotification *)notification {
    if ([self.realText isEqualToString:self.placeholder]) {
        super.text = nil;
        self.textColor = self.realTextColor;
    }
}

/// Edit end method
/// - Parameter notification: Notification data
- (void)endEditing:(NSNotification *)notification {
    [[UIApplication sharedApplication] sendAction:@selector(resignFirstResponder) to:nil from:nil forEvent:nil];
    if ([self.realText isEqualToString:@""] || self.realText == nil) {
        super.text = self.placeholder;
        self.textColor = self.placeholderColor;
    }
}

/// Set text color
/// - Parameter textColor: Color
- (void)setTextColor:(UIColor *)textColor {
    if ([self.realText isEqualToString:self.placeholder]) {
        if ([textColor isEqual:self.placeholderColor]) {
            [super setTextColor:textColor];
        } else {
            self.realTextColor = textColor;
        }
    } else {
        self.realTextColor = textColor;
        [super setTextColor:textColor];
    }
}

/// Destructor
- (void)dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
}

/// Set language for keyboard
/// - Parameter languageCode: ISO code
- (void)setLanguageCode:(NSString *)languageCode {
    _languageCode = languageCode;
    if ([self isFirstResponder]) {
        [self resignFirstResponder];
        [self becomeFirstResponder];
    }
}

/// Overidden input
- (UITextInputMode *)textInputMode {
    if (self.languageCode) {
        for (UITextInputMode *keyboard in UITextInputMode.activeInputModes) {
            if (keyboard.primaryLanguage) {
                NSLocale *locale = [[NSLocale alloc] initWithLocaleIdentifier:keyboard.primaryLanguage];
                if ([locale.languageCode isEqualToString:self.languageCode]) {
                    return keyboard;
                }
            }
        }
    }
    return [super textInputMode];
}

@end


/// MobileInput implemenation
@implementation MobileInput

/// Init MobileInput
/// - Parameter viewController: Current view controller
+ (void)init:(UIViewController *)viewController {
    mainViewController = viewController;
    mobileInputList = [[NSMutableDictionary alloc] init];
    [[UIDevice currentDevice] beginGeneratingDeviceOrientationNotifications];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(orientationChanged:) name:UIDeviceOrientationDidChangeNotification object:[UIDevice currentDevice]];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(keyboardWillShow:) name:UIKeyboardWillShowNotification object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(keyboardWillHide:) name:UIKeyboardWillHideNotification object:nil];
}

/// Process in messages from Unity app
/// - Parameters:
///   - inputId: Input ID
///   - data: JSON data
+ (void)processMessage:(int)inputId data:(NSString *)data {
    NSDictionary *message = [Bridge jsonToDict:data];
    NSString *msg = [message valueForKey:@"msg"];
    if ([msg isEqualToString:CREATE]) {
        MobileInput *input = [[MobileInput alloc] initWith:mainViewController andTag:inputId];
        [input create:message];
        [mobileInputList setObject:input forKey:[NSNumber numberWithInt:inputId]];
    } else {
        MobileInput *input = [mobileInputList objectForKey:[NSNumber numberWithInt:inputId]];
        if (input) {
            [input processData:message];
        }
    }
}

/// Destroy component
+ (void)destroy {
    NSArray *list = [mobileInputList allValues];
    for (MobileInput *input in list) {
        [input remove];
    }
    mobileInputList = nil;
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    [[UIDevice currentDevice] endGeneratingDeviceOrientationNotifications];
}

/// Callback when orientation changed
/// - Parameter notification: Notification data
+ (void)orientationChanged:(NSNotification *)notification {
    UIDevice *device = notification.object;
    NSString *current = nil;
    switch(device.orientation) {
        case UIDeviceOrientationPortrait:
            current = @"PORTRAIT";
            break;
        case UIDeviceOrientationPortraitUpsideDown:
            current = @"PORTRAIT_DOWN";
            break;
        case UIDeviceOrientationLandscapeLeft:
        case UIDeviceOrientationLandscapeRight:
            current = @"LANDSCAPE";
            break;
        case UIDeviceOrientationFaceUp:
            current = @"FACE_UP";
            break;
        case UIDeviceOrientationFaceDown:
            current = @"FACE_DOWN";
            break;
        case UIDeviceOrientationUnknown:
            current = @"UNKNOWN";
            break;
        default:
            current = @"LANDSCAPE";
            break;
    };
    NSMutableDictionary *data = [[NSMutableDictionary alloc] init];
    [data setValue:ORIENTATION_ACTION forKey:@"action"];
    [data setValue:current forKey:@"orientation"];
    NSString *result = [Bridge dictToJson:data];
    [Bridge sendData:result];
}

/// Callback when keyboard show
/// - Parameter notification: Notification data
+ (void)keyboardWillShow:(NSNotification *)notification {
    NSDictionary *keyboardInfo = [notification userInfo];
    NSValue *keyboardFrameBegin = [keyboardInfo valueForKey:UIKeyboardFrameEndUserInfoKey];
    CGRect rectKeyboardFrame = [keyboardFrameBegin CGRectValue];
    CGFloat height = rectKeyboardFrame.size.height;
    NSMutableDictionary *data = [[NSMutableDictionary alloc] init];
    [data setValue:KEYBOARD_ACTION forKey:@"action"];
    [data setValue:[NSNumber numberWithBool:YES] forKey:@"show"];
    [data setValue:[NSNumber numberWithFloat:height] forKey:@"height"];
    NSString *result = [Bridge dictToJson:data];
    [Bridge sendData:result];
}

/// Callback when keyboard hide
/// - Parameter notification: Notification data
+ (void)keyboardWillHide:(NSNotification *)notification {
    NSMutableDictionary *data = [[NSMutableDictionary alloc] init];
    [data setValue:KEYBOARD_ACTION forKey:@"action"];
    [data setValue:[NSNumber numberWithBool:NO] forKey:@"show"];
    [data setValue:[NSNumber numberWithFloat:0] forKey:@"height"];
    NSString *result = [Bridge dictToJson:data];
    [Bridge sendData:result];
}

/// Init with input ID
/// - Parameters:
///   - controller: Current view controller
///   - inputId: Input ID
- (id)initWith:(UIViewController *)controller andTag:(int)idInput {
    if (self = [super init]) {
        viewController = controller;
        inputId = idInput;
    }
    return self;
}

/// Get focus state
- (BOOL)isFocused {
    return editView.isFirstResponder;
}

/// Send data to Unity app
/// - Parameter data: Data from plugin
- (void)sendData:(NSMutableDictionary *)data {
    [data setValue:[NSNumber numberWithInt:inputId] forKey:@"id"];
    NSString *result = [Bridge dictToJson:data];
    [Bridge sendData:result];
}

/// Get color from dict
/// - Parameter data: Dict with data
- (UIColor *)getColor:(NSDictionary *)data {
    float color_r = [[data valueForKey:@"color_r"] floatValue];
    float color_g = [[data valueForKey:@"color_g"] floatValue];
    float color_b = [[data valueForKey:@"color_b"] floatValue];
    float color_a = [[data valueForKey:@"color_a"] floatValue];
    return [UIColor colorWithRed:color_r green:color_g blue:color_b alpha:color_a];
}

/// Action with current input field
/// - Parameter data: Action data
- (void)processData:(NSDictionary *)data {
    NSString *msg = [data valueForKey:@"msg"];
    if ([msg isEqualToString:REMOVE]) {
        [self remove];
    } else if ([msg isEqualToString:SET_TEXT]) {
        NSString *text = [data valueForKey:@"text"];
        [self setText:text];
    } else if ([msg isEqualToString:SET_TEXT_COLOR]) {
        UIColor *color = [self getColor:data];
        if (isMultiline) {
            [(PlaceholderTextView *)editView setTextColor:color];
        } else {
            [(UITextField *)editView setTextColor:color];
        }
    } else if ([msg isEqualToString:SET_PTEXT_COLOR]) {
        UIColor *color = [self getColor:data];
        if (isMultiline) {
            [(PlaceholderTextView *)editView setPlaceholderColor:color];
        } else {
            NSMutableAttributedString *attributes = [[NSMutableAttributedString alloc]initWithAttributedString:[(UITextField *)editView attributedPlaceholder]];
            [attributes addAttribute:NSForegroundColorAttributeName value:color range:placeholderRange];
            [(UITextField *)editView setAttributedPlaceholder: attributes];
        }
    } else if ([msg isEqualToString:SET_BG_COLOR]) {
        UIColor *color = [self getColor:data];
        if (isMultiline) {
            [(PlaceholderTextView *)editView setBackgroundColor:color];
        } else {
            [(UITextField *)editView setBackgroundColor:color];
        }
    } else if ([msg isEqualToString:SET_READ_ONLY]) {
        BOOL isValue = [[data valueForKey:@"value"] boolValue];
        if (isMultiline) {
            [(PlaceholderTextView *)editView setUserInteractionEnabled:!isValue];
        } else {
            [(UITextField *)editView setUserInteractionEnabled:!isValue];
        }
    } else if ([msg isEqualToString:SET_CONTENT_TYPE]) {
        NSString *type = [data valueForKey:@"type"];
        [self setContentType:type];
    } else if ([msg isEqualToString:SET_RECT]) {
        [self setRect:data];
    } else if ([msg isEqualToString:SET_FOCUS]) {
        BOOL isFocus = [[data valueForKey:@"is_focus"] boolValue];
        [self setFocus:isFocus];
    } else if ([msg isEqualToString:SET_VISIBLE]) {
        BOOL isVisible = [[data valueForKey:@"is_visible"] boolValue];
        [self setVisible:isVisible];
    } else if ([msg isEqualToString:SET_LANGUAGE]) {
        NSString *code = [data valueForKey:@"value"];
        if (isMultiline) {
            [(PlaceholderTextView *)editView setLanguageCode:code];
        } else {
            [(CustomTextField *)editView setLanguageCode:code];
        }
    }
}

/// Set size and position for input
/// - Parameter data: Data to set up
- (void)setRect:(NSDictionary *)data {
    float x = [[data valueForKey:@"x"] floatValue] * viewController.view.bounds.size.width;
    float y = [[data valueForKey:@"y"] floatValue] * viewController.view.bounds.size.height;
    float width = [[data valueForKey:@"width"] floatValue] * viewController.view.bounds.size.width;
    float height = [[data valueForKey:@"height"] floatValue] * viewController.view.bounds.size.height;
    x -= editView.superview.frame.origin.x;
    y -= editView.superview.frame.origin.y;
    editView.frame = CGRectMake(x, y, width, height);
}

/// Set content type for input
/// - Parameter contentType: Content type
- (void)setContentType:(NSString *)contentType {
    UIKeyboardType keyType = UIKeyboardTypeDefault;
    BOOL autoCorrection = NO;
    BOOL password = NO;
    if ([contentType isEqualToString:@"Autocorrected"]) {
        autoCorrection = YES;
    } else if ([contentType isEqualToString:@"IntegerNumber"]) {
        keyType = UIKeyboardTypeNumberPad;
    } else if ([contentType isEqualToString:@"DecimalNumber"]) {
        keyType = UIKeyboardTypeDecimalPad;
    } else if ([contentType isEqualToString:@"Alphanumeric"]) {
        keyType = UIKeyboardTypeAlphabet;
    } else if ([contentType isEqualToString:@"Name"]) {
        keyType = UIKeyboardTypeNamePhonePad;
    } else if ([contentType isEqualToString:@"EmailAddress"]) {
        keyType = UIKeyboardTypeEmailAddress;
    } else if ([contentType isEqualToString:@"Password"]) {
        password = YES;
    } else if ([contentType isEqualToString:@"Pin"]) {
        keyType = UIKeyboardTypePhonePad;
    }
    if (isMultiline) {
        [(PlaceholderTextView *)editView setSecureTextEntry:password];
        [(PlaceholderTextView *)editView setKeyboardType:keyType];
        [(PlaceholderTextView *)editView setAutocorrectionType:autoCorrection ? UITextAutocorrectionTypeYes : UITextAutocorrectionTypeNo];
    } else {
        [(UITextField *)editView setSecureTextEntry:password];
        [(UITextField *)editView setKeyboardType:keyType];
        [(UITextField *)editView setAutocorrectionType:autoCorrection ? UITextAutocorrectionTypeYes : UITextAutocorrectionTypeNo];
    }
}

/// Create native input field
/// - Parameter data: Options data
- (void)create:(NSDictionary *)data {
    NSString *placeholder = [data valueForKey:@"placeholder"];
    placeholderRange = [placeholder rangeOfString:placeholder];
    float fontSize = [[data valueForKey:@"font_size"] floatValue];
    float x = [[data valueForKey:@"x"] floatValue] * viewController.view.bounds.size.width;
    float y = [[data valueForKey:@"y"] floatValue] * viewController.view.bounds.size.height;
    float width = [[data valueForKey:@"width"] floatValue] * viewController.view.bounds.size.width;
    float height = [[data valueForKey:@"height"] floatValue] * viewController.view.bounds.size.height;
    characterLimit = [[data valueForKey:@"character_limit"] intValue];
    float textColor_r = [[data valueForKey:@"text_color_r"] floatValue];
    float textColor_g = [[data valueForKey:@"text_color_g"] floatValue];
    float textColor_b = [[data valueForKey:@"text_color_b"] floatValue];
    float textColor_a = [[data valueForKey:@"text_color_a"] floatValue];
    UIColor *textColor = [UIColor colorWithRed:textColor_r green:textColor_g blue:textColor_b alpha:textColor_a];
    float backColor_r = [[data valueForKey:@"back_color_r"] floatValue];
    float backColor_g = [[data valueForKey:@"back_color_g"] floatValue];
    float backColor_b = [[data valueForKey:@"back_color_b"] floatValue];
    float backColor_a = [[data valueForKey:@"back_color_a"] floatValue];
    UIColor *backgroundColor = [UIColor colorWithRed:backColor_r green:backColor_g blue:backColor_b alpha:backColor_a];
    float placeHolderColor_r = [[data valueForKey:@"placeholder_color_r"] floatValue];
    float placeHolderColor_g = [[data valueForKey:@"placeholder_color_g"] floatValue];
    float placeHolderColor_b = [[data valueForKey:@"placeholder_color_b"] floatValue];
    float placeHolderColor_a = [[data valueForKey:@"placeholder_color_a"] floatValue];
    UIColor *placeHolderColor = [UIColor colorWithRed:placeHolderColor_r green:placeHolderColor_g blue:placeHolderColor_b alpha:placeHolderColor_a];
    float caretColor_r = [[data valueForKey:@"caret_color_r"] floatValue];
    float caretColor_g = [[data valueForKey:@"caret_color_g"] floatValue];
    float caretColor_b = [[data valueForKey:@"caret_color_b"] floatValue];
    float caretColor_a = [[data valueForKey:@"caret_color_a"] floatValue];
    UIColor *caretColor = [UIColor colorWithRed:caretColor_r green:caretColor_g blue:caretColor_b alpha:caretColor_a];
    NSString *contentType = [data valueForKey:@"content_type"];
    NSString *alignment = [data valueForKey:@"align"];
    NSString *customFont = [data valueForKey:@"font"];
    BOOL withDoneButton = [[data valueForKey:@"with_done_button"] boolValue];
    BOOL withClearButton = [[data valueForKey:@"with_clear_button"] boolValue];
    isMultiline = [[data valueForKey:@"multiline"] boolValue];
    BOOL isChangeCaret = [[data valueForKey:@"caret_color"] boolValue];
    BOOL autoCorrection = NO;
    BOOL password = NO;
    NSString *inputType = [data valueForKey:@"input_type"];
    NSString *keyboardType = [data valueForKey:@"keyboard_type"];
    NSString *keyboardLanguage = [data valueForKey:@"keyboard_language"];
    UIKeyboardType keyType = UIKeyboardTypeDefault;
    if ([contentType isEqualToString:@"Autocorrected"]) {
        autoCorrection = YES;
    } else if ([contentType isEqualToString:@"IntegerNumber"]) {
        keyType = UIKeyboardTypeNumberPad;
    } else if ([contentType isEqualToString:@"DecimalNumber"]) {
        keyType = UIKeyboardTypeDecimalPad;
    } else if ([contentType isEqualToString:@"Alphanumeric"]) {
        keyType = UIKeyboardTypeAlphabet;
    } else if ([contentType isEqualToString:@"Name"]) {
        keyType = UIKeyboardTypeNamePhonePad;
    } else if ([contentType isEqualToString:@"EmailAddress"]) {
        keyType = UIKeyboardTypeEmailAddress;
    } else if ([contentType isEqualToString:@"Password"]) {
        password = YES;
    } else if ([contentType isEqualToString:@"Pin"]) {
        keyType = UIKeyboardTypePhonePad;
    } else if ([contentType isEqualToString:@"Custom"]) {
        if ([keyboardType isEqualToString:@"ASCIICapable"]) {
            keyType = UIKeyboardTypeASCIICapable;
        } else if ([keyboardType isEqualToString:@"NumbersAndPunctuation"]) {
            keyType = UIKeyboardTypeNumbersAndPunctuation;
        } else if ([keyboardType isEqualToString:@"URL"]) {
            keyType = UIKeyboardTypeURL;
        } else if ([keyboardType isEqualToString:@"NumberPad"]) {
            keyType = UIKeyboardTypeNumberPad;
        } else if ([keyboardType isEqualToString:@"PhonePad"]) {
            keyType = UIKeyboardTypePhonePad;
        } else if ([keyboardType isEqualToString:@"NamePhonePad"]) {
            keyType = UIKeyboardTypeNamePhonePad;
        } else if ([keyboardType isEqualToString:@"EmailAddress"]) {
            keyType = UIKeyboardTypeEmailAddress;
        } else if ([keyboardType isEqualToString:@"Social"]) {
            keyType = UIKeyboardTypeTwitter;
        } else if ([keyboardType isEqualToString:@"Search"]) {
            keyType = UIKeyboardTypeWebSearch;
        } else {
            keyType = UIKeyboardTypeDefault;
        }
        if ([inputType isEqualToString:@"Standard"]) {
        } else if ([inputType isEqualToString:@"AutoCorrect"]) {
            autoCorrection = YES;
        } else if ([inputType isEqualToString:@"Password"]) {
            password = YES;
        }
    }
    UIControlContentHorizontalAlignment halign = UIControlContentHorizontalAlignmentLeft;
    UIControlContentVerticalAlignment valign = UIControlContentVerticalAlignmentCenter;
    NSTextAlignment textAlign = NSTextAlignmentCenter;
    if ([alignment isEqualToString:@"TopLeft"]) {
        valign = UIControlContentVerticalAlignmentTop;
        halign = UIControlContentHorizontalAlignmentLeft;
        textAlign = NSTextAlignmentLeft;
    } else if ([alignment isEqualToString:@"Top"]) {
        valign = UIControlContentVerticalAlignmentTop;
        halign = UIControlContentHorizontalAlignmentCenter;
        textAlign = NSTextAlignmentCenter;
    } else if ([alignment isEqualToString:@"TopRight"]) {
        valign = UIControlContentVerticalAlignmentTop;
        halign = UIControlContentHorizontalAlignmentRight;
        textAlign = NSTextAlignmentRight;
    } else if ([alignment isEqualToString:@"Left"]) {
        valign = UIControlContentVerticalAlignmentCenter;
        halign = UIControlContentHorizontalAlignmentLeft;
        textAlign = NSTextAlignmentLeft;
    } else if ([alignment isEqualToString:@"Center"]) {
        valign = UIControlContentVerticalAlignmentCenter;
        halign = UIControlContentHorizontalAlignmentCenter;
        textAlign = NSTextAlignmentCenter;
    } else if ([alignment isEqualToString:@"Right"]) {
        valign = UIControlContentVerticalAlignmentCenter;
        halign = UIControlContentHorizontalAlignmentRight;
        textAlign = NSTextAlignmentRight;
    } else if ([alignment isEqualToString:@"BottomLeft"]) {
        valign = UIControlContentVerticalAlignmentBottom;
        halign = UIControlContentHorizontalAlignmentLeft;
        textAlign = NSTextAlignmentLeft;
    } else if ([alignment isEqualToString:@"Bottom"]) {
        valign = UIControlContentVerticalAlignmentBottom;
        halign = UIControlContentHorizontalAlignmentCenter;
        textAlign = NSTextAlignmentCenter;
    } else if ([alignment isEqualToString:@"BottomRight"]) {
        valign = UIControlContentVerticalAlignmentBottom;
        halign = UIControlContentHorizontalAlignmentRight;
        textAlign = NSTextAlignmentRight;
    }
    if (withDoneButton) {
        keyboardDoneButtonView = [[UIToolbar alloc] init];
        [keyboardDoneButtonView sizeToFit];
        doneButton = [[UIBarButtonItem alloc] initWithTitle:@"Done" style:UIBarButtonItemStyleDone target:self action:@selector(doneClicked:)];
        UIBarButtonItem *flexibleSpace = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:nil action:nil];
        [keyboardDoneButtonView setItems:[NSArray arrayWithObjects:flexibleSpace, flexibleSpace, doneButton, nil]];
    } else {
        keyboardDoneButtonView = nil;
    }
    UIReturnKeyType returnKeyType = UIReturnKeyDefault;
    NSString *returnKeyTypeString = [data valueForKey:@"return_key_type"];
    if ([returnKeyTypeString isEqualToString:@"Next"]) {
        returnKeyType = UIReturnKeyNext;
    } else if ([returnKeyTypeString isEqualToString:@"Done"]) {
        returnKeyType = UIReturnKeyDone;
    } else if ([returnKeyTypeString isEqualToString:@"Search"]) {
        returnKeyType = UIReturnKeySearch;
    } else if ([returnKeyTypeString isEqualToString:@"Send"]) {
        returnKeyType = UIReturnKeySend;
    }
    fontSize = fontSize / [UIScreen mainScreen].scale;
    UIFont *uiFont = [UIFont systemFontOfSize:fontSize];
    if (![customFont isEqualToString:@"default"]) {
        NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
        NSString *fontPath = [[paths objectAtIndex:0] stringByAppendingPathComponent:[NSString stringWithFormat:@"%@.ttf", customFont]];
        NSURL *url = [NSURL fileURLWithPath:fontPath];
        CGDataProviderRef fontDataProvider = CGDataProviderCreateWithURL((__bridge CFURLRef)url);
        CGFontRef newFont = CGFontCreateWithDataProvider(fontDataProvider);
        NSString *font = (__bridge NSString *)CGFontCopyPostScriptName(newFont);
        CGDataProviderRelease(fontDataProvider);
        CFErrorRef error;
        CTFontManagerRegisterGraphicsFont(newFont, &error);
        CGFontRelease(newFont);
        uiFont = [UIFont fontWithName:font size:fontSize];
    }
    if (isMultiline) {
        PlaceholderTextView *textView = [[PlaceholderTextView alloc] initWithFrame:CGRectMake(x, y, width, height)];
        textView.keyboardType = keyType;
        [textView setFont:uiFont];
        textView.scrollEnabled = TRUE;
        textView.delegate = self;
        textView.tag = inputId;
        textView.text = @"";
        textView.textColor = textColor;
        textView.backgroundColor = backgroundColor;
        textView.returnKeyType = returnKeyType;
        textView.textAlignment = textAlign;
        textView.autocorrectionType = autoCorrection ? UITextAutocorrectionTypeYes : UITextAutocorrectionTypeNo;
        textView.contentInset = UIEdgeInsetsMake(0.0f, 0.0f, 0.0f, 0.0f);
        textView.placeholder = placeholder;
        textView.placeholderColor = placeHolderColor;
        if (isChangeCaret) {
            textView.tintColor = caretColor;
        }
        if (![keyboardLanguage isEqualToString:@"default"]) {
            textView.languageCode = keyboardLanguage;
        }
        textView.delegate = self;
        if (keyType == UIKeyboardTypeEmailAddress) {
            textView.autocapitalizationType = UITextAutocapitalizationTypeNone;
        }
        [textView setSecureTextEntry:password];
        if (keyboardDoneButtonView != nil) {
            textView.inputAccessoryView = keyboardDoneButtonView;
        }
        editView = textView;
    } else {
        CustomTextField *textField = [[CustomTextField alloc] initWithFrame:CGRectMake(x, y, width, height)];
        textField.keyboardType = keyType;
        [textField setFont:uiFont];
        textField.delegate = self;
        textField.tag = inputId;
        textField.text = @"";
        textField.textColor = textColor;
        textField.backgroundColor = backgroundColor;
        if (isChangeCaret) {
            textField.tintColor = caretColor;
        }
        if (![keyboardLanguage isEqualToString:@"default"]) {
            textField.languageCode = keyboardLanguage;
        }
        textField.returnKeyType = returnKeyType;
        textField.autocorrectionType = autoCorrection ? UITextAutocorrectionTypeYes : UITextAutocorrectionTypeNo;
        textField.contentVerticalAlignment = valign;
        textField.contentHorizontalAlignment = halign;
        textField.textAlignment = textAlign;
        if (withClearButton) {
            textField.clearButtonMode = UITextFieldViewModeWhileEditing;
        }
        NSMutableParagraphStyle *setting = [[NSMutableParagraphStyle alloc] init];
        setting.alignment = textAlign;
        textField.attributedPlaceholder = [[NSAttributedString alloc] initWithString:placeholder attributes:@{NSForegroundColorAttributeName: placeHolderColor, NSParagraphStyleAttributeName : setting}];
        textField.delegate = self;
        if (keyType == UIKeyboardTypeEmailAddress) {
            textField.autocapitalizationType = UITextAutocapitalizationTypeNone;
        }
        [textField addTarget:self action:@selector(textFieldDidChange:) forControlEvents:UIControlEventEditingChanged];
        [textField addTarget:self action:@selector(textFieldActive:) forControlEvents:UIControlEventEditingDidBegin];
        [textField addTarget:self action:@selector(textFieldInActive:) forControlEvents:UIControlEventEditingDidEnd];
        [textField setSecureTextEntry:password];
        if (keyboardDoneButtonView != nil) {
            textField.inputAccessoryView = keyboardDoneButtonView;
        }
        editView = textField;
    }
    [mainViewController.view addSubview:editView];
    NSMutableDictionary *msg = [[NSMutableDictionary alloc] init];
    [msg setValue:READY forKey:@"msg"];
    [self sendData:msg];
}

/// Set text to input
/// - Parameter text: Text to set
- (void)setText:(NSString *)text {
    if ([editView isKindOfClass:[UITextField class]]) {
        [((UITextField *) editView) setText:text];
    } else if ([editView isKindOfClass:[UITextView class]]) {
        [((UITextView *) editView) setText:text];
    }
}

/// Handler for done button
/// - Parameter sender: Link to sender
- (IBAction) doneClicked:(id)sender {
    [self showKeyboard:false];
}

/// Remove input
- (void)remove {
    [editView resignFirstResponder];
    [editView removeFromSuperview];
    if (keyboardDoneButtonView != nil) {
        doneButton = nil;
        keyboardDoneButtonView = nil;
    }
}

/// Focus action with current input
/// - Parameter isFocus: true/false
- (void)setFocus:(BOOL)isFocus {
    if (isFocus) {
        [editView becomeFirstResponder];
    } else {
        [editView resignFirstResponder];
    }
}

/// Show/hide keyboard
/// - Parameter isShow: true/false
- (void)showKeyboard:(BOOL)isShow {
    [editView endEditing:(isShow ? YES : NO)];
}

/// Show/hide input
/// - Parameter isVisible: true/false
- (void)setVisible:(BOOL)isVisible {
    editView.hidden = (isVisible ? NO : YES);
}

/// Callback on input text change
/// - Parameter text: Changing text
- (void)onTextChange:(NSString *)text {
    NSMutableDictionary *msg = [[NSMutableDictionary alloc] init];
    [msg setValue:TEXT_CHANGE forKey:@"msg"];
    [msg setValue:text forKey:@"text"];
    [self sendData:msg];
}

/// Callback on text edit end
/// - Parameter text: Text on end
- (void)onTextEditEnd:(NSString *)text {
    NSMutableDictionary *msg = [[NSMutableDictionary alloc] init];
    [msg setValue:TEXT_END_EDIT forKey:@"msg"];
    [msg setValue:text forKey:@"text"];
    [self sendData:msg];
}

/// Callback on text changed
/// - Parameter textView: Textview instance
- (void)textViewDidChange:(UITextView *)textView {
    [self onTextChange:textView.text];
}

/// Callback on begin editing
/// - Parameter textView: Textview instance
- (void)textViewDidBeginEditing:(UITextView *)textView {
    if (isMultiline) {
        NSMutableDictionary *msg = [[NSMutableDictionary alloc] init];
        [msg setValue:ON_FOCUS forKey:@"msg"];
        [self sendData:msg];
    }
}

/// Callback on end editing
/// - Parameter textView: Textview instance
- (void)textViewDidEndEditing:(UITextView *)textView {
    if (isMultiline) {
        NSMutableDictionary *msg = [[NSMutableDictionary alloc] init];
        [msg setValue:ON_UNFOCUS forKey:@"msg"];
        [self sendData:msg];
    }
    [self onTextEditEnd:textView.text];
}

/// Callback on return pressed
/// - Parameter textField: Textfield instance
- (BOOL)textFieldShouldReturn:(UITextField *)textField {
    if (![editView isFirstResponder]) {
        return YES;
    }
    NSMutableDictionary *msg = [[NSMutableDictionary alloc] init];
    [msg setValue:RETURN_PRESSED forKey:@"msg"];
    [self sendData:msg];
    return YES;
}

/// Callback when textfield changing to control limit
/// - Parameters:
///   - textField: Textfield instance
///   - range: Range characters
///   - string: Replacement string
- (BOOL)textField:(UITextField *)textField shouldChangeCharactersInRange:(NSRange)range replacementString:(NSString *)string {
    if (range.length + range.location > textField.text.length) {
        return NO;
    }
    NSUInteger newLength = [textField.text length] + [string length] - range.length;
    if (characterLimit > 0) {
        return newLength <= characterLimit;
    } else {
        return YES;
    }
}

/// Callback on input focused
/// - Parameter textField: Textfield instance
- (void)textFieldActive:(UITextField *)textField {
    NSMutableDictionary *msg = [[NSMutableDictionary alloc] init];
    [msg setValue:ON_FOCUS forKey:@"msg"];
    [self sendData:msg];
}

/// Callback on input unfocused
/// - Parameter theTextField: Textfield instance
- (void)textFieldInActive:(UITextField *)textField {
    NSMutableDictionary *msg = [[NSMutableDictionary alloc] init];
    [msg setValue:ON_UNFOCUS forKey:@"msg"];
    [self sendData:msg];
}

/// Callback on text did change
/// - Parameter textField: Textfield instance
- (void)textFieldDidChange:(UITextField *)textField {
    [self onTextChange:textField.text];
}

@end

/// C wrapper
extern "C" {


/// Pass data from Unity app to native input
/// - Parameters:
///   - inputId: Input ID
///   - data: Options data
void inputExecute(int inputId, const char *data) {
    [MobileInput processMessage:inputId data:[NSString stringWithUTF8String:data]];
}

/// Destoy component
void inputDestroy() {
    [MobileInput destroy];
}

/// Init plugin with receiver name and methid
/// - Parameter data: Options data
void inputInit(const char *data) {
    [Bridge initialize:[NSString stringWithUTF8String:data]];
    [MobileInput init:UnityGetGLViewController()];
}

/// Get current screen scale factor
float scaleFactor() {
    return [[UIScreen mainScreen] nativeScale];
}

}
