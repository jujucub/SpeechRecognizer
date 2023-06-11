//
//  SpeechRecognizer.h
//  SpeechRecognizer
//
//  Created by Hisaki Sato on 2023/06/03.
//

#import <Foundation/Foundation.h>
#import <Speech/Speech.h>

typedef void (*RecognizedCallback)(const char *message);
typedef void (*ErrorCallback)(long errorCode, const char* message);

@interface SpeechRecognizer : NSObject<SFSpeechRecognizerDelegate>

@property (nonatomic, strong) SFSpeechRecognizer *speechRecognizer;
@property (nonatomic, strong) SFSpeechAudioBufferRecognitionRequest *recognitionRequest;
@property (nonatomic, strong) SFSpeechRecognitionTask *recognitionTask;
@property (nonatomic, strong) AVAudioEngine *audioEngine;
@property RecognizedCallback recognizedCallback;
@property RecognizedCallback partialRecognizedCallback;
@property ErrorCallback errorCallback;
@property NSString* locale;

- (void)startListening;
- (void)stopListening;

@end

extern "C" {
    void RegisterRecognizedCallback(RecognizedCallback callback);
    void RegisterPartialRecognizedCallback(RecognizedCallback callback);
    void RegisterErrorCallback(ErrorCallback callback);
    void Initialize(const char* language);
    void StartListening();
    void StopListening();
    void Finish();
}
