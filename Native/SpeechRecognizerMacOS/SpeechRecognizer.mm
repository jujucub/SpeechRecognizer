#import "SpeechRecognizer.h"

@implementation SpeechRecognizer

- (instancetype)init {
    self = [super init];
    if(self){
        return [self initWithLocale:@"ja_JP"];
    }
    return self;
}

- (instancetype)initWithLocale:(NSString*) locale {
    self = [super init];
    if(self){
        self.locale = locale;
        
        self.speechRecognizer = [[SFSpeechRecognizer alloc] initWithLocale:[[NSLocale alloc] initWithLocaleIdentifier:self.locale]];
        self.speechRecognizer.delegate = self;
        self.audioEngine = [[AVAudioEngine alloc] init];
        self.recognitionRequest = [[SFSpeechAudioBufferRecognitionRequest alloc] init];
    }
    return self;
}

- (void)startListening {
    
    AVAudioInputNode *inputNode = self.audioEngine.inputNode;
    self.recognitionRequest.shouldReportPartialResults = YES;
    if(self.speechRecognizer.supportsOnDeviceRecognition) {
        self.recognitionRequest.requiresOnDeviceRecognition = YES;
    }
        
    AVAudioFormat *format = [inputNode outputFormatForBus:0];
    [inputNode installTapOnBus:0 bufferSize:2048 format:format block:^(AVAudioPCMBuffer * _Nonnull buffer, AVAudioTime * _Nonnull when) {
            [self.recognitionRequest appendAudioPCMBuffer:buffer];
        }];
    
    [self.audioEngine prepare];
    [self.audioEngine startAndReturnError:nil];

    self.recognitionTask = [self.speechRecognizer recognitionTaskWithRequest:self.recognitionRequest resultHandler:^(SFSpeechRecognitionResult * _Nullable result, NSError * _Nullable error) {
        BOOL isFinal = NO;
        if (result != nil) {
            isFinal = result.isFinal;
            dispatch_async(dispatch_get_main_queue(), ^{
                const char *recognizedString = MakeStringCopy([result.bestTranscription.formattedString UTF8String]);
                
                if(result.speechRecognitionMetadata != nil &&
                   result.speechRecognitionMetadata.speechDuration != 0.0 &&
                   result.speechRecognitionMetadata.averagePauseDuration != 0.0){
                    if(self.recognizedCallback != nil){
                        self.recognizedCallback(recognizedString);
                    }
                } else {
                    if(self.partialRecognizedCallback != nil){
                        self.partialRecognizedCallback(recognizedString);
                    }
                }
            });
        }
        
        if (error != nil) {
            dispatch_async(dispatch_get_main_queue(), ^{
                const char* domain = MakeStringCopy([error.description UTF8String]);
                if(self.errorCallback != nil){
                    self.errorCallback(error.code, domain);
                }
            });
        }
        
        if(error != nil || isFinal) {
            [inputNode removeTapOnBus:0];
        }
    }];
}

- (void)stopListening {
    [self.audioEngine.inputNode removeTapOnBus:0];
    [self.audioEngine stop];
    [self.recognitionRequest endAudio];
    if (self.recognitionTask != nil) {
        [self.recognitionTask cancel];
        self.recognitionTask = nil;
    }
}

// Helper method to create C string copy
char* MakeStringCopy (const char* string)
{
    if (string == NULL)
        return NULL;
    
    // By default mono string marshaler creates .Net string for returned UTF-8 C string
    // and calls free for returned value, thus returned strings should be allocated on heap
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

@end

static SpeechRecognizer *speechRecognizer = nil;

extern "C" {
    void RegisterRecognizedCallback(RecognizedCallback callback) {
        speechRecognizer.recognizedCallback = callback;
    }

    void RegisterPartialRecognizedCallback(RecognizedCallback callback) {
        speechRecognizer.partialRecognizedCallback = callback;
    }

    void RegisterErrorCallback(ErrorCallback callback) {
        speechRecognizer.errorCallback = callback;
    }

    void Initialize(const char* locale) {
        speechRecognizer = [[SpeechRecognizer alloc] initWithLocale: [NSString stringWithCString: locale encoding:NSUTF8StringEncoding]];
    }
    
    void StartListening() {
        [speechRecognizer startListening];
    }
    
    void StopListening() {
        [speechRecognizer stopListening];
    }

    void Finish() {
        speechRecognizer.errorCallback = nil;
        speechRecognizer.recognizedCallback = nil;
        speechRecognizer.partialRecognizedCallback = nil;
        speechRecognizer = nil;
    }
}
