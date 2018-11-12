// ----------------------------------------------------------------------------
// The MIT License
// UnityMobileInput https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------

#import "Common.h"

@implementation Common

static NSString *object;
static NSString *receiver;

// Convert JSON string to NSDictionary
+ (NSDictionary *)jsonToDict:(NSString *)json {
    NSError *error;
    NSData *data = [json dataUsingEncoding:NSUTF8StringEncoding];
    return (error) ? NULL : [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:&error];
}

// Convert NSDictionary to JSON string
+ (NSString *)dictToJson:(NSDictionary *)dict {
    NSError *error;
    NSData *data = [NSJSONSerialization dataWithJSONObject:dict options:0 error:&error];
    return (error) ? NULL : [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
}

// Plugins initialize
+ (void)initialize:(NSString *)data {
    NSDictionary *params = [self jsonToDict:data];
    object = [params valueForKey:@"object"];
    receiver = [params valueForKey:@"receiver"];
}

// Send data in JSON format to Unity
+ (void)sendData:(NSString *)plugin data:(NSString *)data {
    NSMutableDictionary *dict = [[NSMutableDictionary alloc] init];
    [dict setValue:plugin forKey:@"name"];
    [dict setValue:data forKey:@"data"];
    NSString *result = [self dictToJson:dict];
    UnitySendMessage([object cStringUsingEncoding:NSUTF8StringEncoding], [receiver cStringUsingEncoding:NSUTF8StringEncoding], [result cStringUsingEncoding:NSUTF8StringEncoding]);
}

// Send error
+ (void)sendError:(NSString *)plugin code:(NSString *)code {
    [self sendError:plugin code:code data:NULL];
}

// Send error in JSON format to Unity
+ (void)sendError:(NSString *)plugin code:(NSString *)code data:(NSString *)data {
    NSDictionary *error = [NSDictionary dictionaryWithObjectsAndKeys:code, @"code", data, @"message", nil];
    NSMutableDictionary *dict = [[NSMutableDictionary alloc] init];
    [dict setValue:plugin forKey:@"name"];
    [dict setValue:error forKey:@"error"];
    NSString *result = [self dictToJson:dict];
    UnitySendMessage([object cStringUsingEncoding:NSUTF8StringEncoding], [receiver cStringUsingEncoding:NSUTF8StringEncoding], [result cStringUsingEncoding:NSUTF8StringEncoding]);
}

// Init plugins system
void pluginsInit(const char *data) {
    [Common initialize:[NSString stringWithUTF8String:data]];
}

@end


