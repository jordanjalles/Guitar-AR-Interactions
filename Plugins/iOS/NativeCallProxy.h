#import <Foundation/Foundation.h>

typedef void (*TestDelegate)(const char* degrees);

@protocol NativeCallsProtocol
@required
- (void) sendMessageToMobileApp:(NSString*)message;
- (void) onUnityStateChange:(const NSString*)state;
- (void) onSetTestDelegate:(TestDelegate) delegate;
// other methods
@end

__attribute__ ((visibility("default")))
@interface FrameworkLibAPI : NSObject
+(void) registerAPIforNativeCalls:(id<NativeCallsProtocol>) aApi;

@end
