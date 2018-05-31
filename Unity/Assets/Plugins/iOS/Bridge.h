// ----------------------------------------------------------------------------
// The MIT License
// UnityMobileInput https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>

extern void UnitySendMessage(const char *obj, const char *method, const char *msg);
extern UIViewController *UnityGetGLViewController(); 
extern void pluginsInit (const char* data);

@interface Bridge : NSObject
+ (NSDictionary *)jsonToDict:(NSString *)json;
+ (NSString *)dictToJson:(NSDictionary *)dict;
+ (void)initialize:(NSString *)data;
+ (void)sendData:(NSString *)plugin data:(NSString *)data;
+ (void)sendError:(NSString *)plugin code:(NSString *)code;
+ (void)sendError:(NSString *)plugin code:(NSString *)code data:(NSString *)data;
@end
