#import "Bridge.h"

/// Bridge implementation to communicate with Unity app
@implementation Bridge

/// GameObject that will be receive messages
static NSString *object = @"MobileInput";

/// Method name in script
static NSString *receiver = @"OnDataReceive";

/// Flag to on/off debug log
static bool isDebug = NO;

/// Convert JSON string to NSDictionary
/// - Parameter json: JSON string
+ (NSDictionary *)jsonToDict:(NSString *)json {
    NSError *error;
    NSData *data = [json dataUsingEncoding:NSUTF8StringEncoding];
    return (error) ? NULL : [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:&error];
}

/// Convert NSDictionary to JSON string
/// - Parameter dict: NSDictionary to convert
+ (NSString *)dictToJson:(NSDictionary *)dict {
    NSError *error;
    NSData *data = [NSJSONSerialization dataWithJSONObject:dict options:0 error:&error];
    return (error) ? NULL : [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
}

/// Init plugin
/// - Parameter data: JSON string with receiver and method
+ (void)initialize:(NSString *)data {
    NSDictionary *params = [self jsonToDict:data];
    object = [params valueForKey:@"object"];
    receiver = [params valueForKey:@"receiver"];
    isDebug = [params valueForKey:@"debug"];
}

/// Send data from plugin to Unity app
/// - Parameter data: JSON string
+ (void)sendData:(NSString *)data {
    NSMutableDictionary *dict = [[NSMutableDictionary alloc] init];
    [dict setValue:data forKey:@"data"];
    NSString *result = [self dictToJson:dict];
    UnitySendMessage([object cStringUsingEncoding:NSUTF8StringEncoding], [receiver cStringUsingEncoding:NSUTF8StringEncoding], [result cStringUsingEncoding:NSUTF8StringEncoding]);
}

/// Send error to Unity app
/// - Parameters:
///   - code: Error code
///   - data: Error description
+ (void)sendError:(NSString *)code data:(NSString *)data  {
    NSDictionary *error = [NSDictionary dictionaryWithObjectsAndKeys:code, @"code", data, @"message", nil];
    NSMutableDictionary *dict = [[NSMutableDictionary alloc] init];
    [dict setValue:error forKey:@"error"];
    NSString *result = [self dictToJson:dict];
    UnitySendMessage([object cStringUsingEncoding:NSUTF8StringEncoding], [receiver cStringUsingEncoding:NSUTF8StringEncoding], [result cStringUsingEncoding:NSUTF8StringEncoding]);
}

@end


