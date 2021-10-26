#import <Foundation/Foundation.h>
#import "NativeCallProxy.h"

@implementation FrameworkLibAPI

id<NativeCallsProtocol> api = NULL;
+(void) registerAPIforNativeCalls:(id<NativeCallsProtocol>) aApi
{
    api = aApi;
}

@end

extern "C" {

  // Functions listed here are available to Unity. When called,
  // they forward the call to the `api` delegate.
  //
  // You should also perform data transformation here, from
  // C data struct to Objective-C **if needed**.

  void
  sendUnityStateUpdate(const char* state)
  {
      const NSString* str = @(state);
      [api onUnityStateChange: str];
  }

}
