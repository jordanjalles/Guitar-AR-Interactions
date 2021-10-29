@protocol NativeCallsProtocol
@required
- (void) onUnityStateChange:(const NSString*) state;
// other methods
@end

__attribute__ ((visibility("default")))
@interface FrameworkLibAPI : NSObject
+(void) registerAPIforNativeCalls:(id<NativeCallsProtocol>) aApi;

@end
