#import <Foundation/Foundation.h>

/// Send data in Unity app
/// - Parameters:
///   - obj: GameObject that will be receive messages
///   - method: Method name in script
///   - msg: Message
extern void UnitySendMessage(const char *obj, const char *method, const char *msg);

/// Main Unity controller
extern UIViewController *UnityGetGLViewController();

/// Bridge to communicate with Unity app
@interface Bridge : NSObject

/// Convert JSON string to NSDictionary
/// - Parameter json: JSON string
+ (NSDictionary *)jsonToDict:(NSString *)json;

/// Convert NSDictionary to JSON string
/// - Parameter dict: NSDictionary to convert
+ (NSString *)dictToJson:(NSDictionary *)dict;

/// Init plugin
/// - Parameter data: JSON string with receiver and method
+ (void)initialize:(NSString *)data;

/// Send data from plugin to Unity app
/// - Parameter data: JSON string
+ (void)sendData:(NSString *)data;

/// Send error to Unity app
/// - Parameters:
///   - code: Error code
///   - data: Error description
+ (void)sendError:(NSString *)code data:(NSString *)data;
@end
