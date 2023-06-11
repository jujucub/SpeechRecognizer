using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif

namespace SpeechRecognizer
{
    public class NativeSpeechRecognizer : MonoBehaviour, IRecognizer
    {
        public event Action<SpeechRecognizerException> ErrorEvent;
        public event Action<string> RecognizedEvent;
        public event Action<string> PartialRecognizedEvent;

#if PLATFORM_ANDROID
        private AndroidJavaClass _speechToText;
#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private DictationRecognizer _dictationRecognizer;
#endif

        IEnumerator Start()
        {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_IOS
            SpeechRecognizerMacOS.Instance.Initialize();
            SpeechRecognizerMacOS.Instance.RecognizedEvent += OnRecognized;
            SpeechRecognizerMacOS.Instance.PartialRecognizedEvent += OnPartialRecognized;
#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            _dictationRecognizer = new DictationRecognizer();
            _dictationRecognizer.DictationResult += OnDictationResult;
            _dictationRecognizer.DictationHypothesis += OnDictationHypothesis;
            _dictationRecognizer.DictationError += OnDictationError;
#endif
            yield return RequestMicrophonePermission();

            StartListening();
        }

        IEnumerator RequestMicrophonePermission()
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        }

        void Update()
        {
        }

        void OnDestroy()
        {
            StopListening();
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_IOS
            SpeechRecognizerMacOS.Instance.Finish();
#endif
        }

        public void StartListening()
        {
#if !UNITY_EDITOR && PLATFORM_ANDROID
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (var activity = player.GetStatic <AndroidJavaObject>("currentActivity"))
                {
                    activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        Debug.Log("StartListening on UI thread");

                        _speechToText = new AndroidJavaClass("com.aitalk.speechtotext.SpeechToText");
                        _speechToText.CallStatic(
                            "StartListening",
                            activity,
                            gameObject.name,
                            "OnRecognized"
                        );
                    }));
                }
            }
#endif

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_IOS
            SpeechRecognizerMacOS.Instance.StartListening();
#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            _dictationRecognizer.Start();
#endif
        }

        public void StopListening()
        {
#if !UNITY_EDITOR && PLATFORM_ANDROID
            _speechToText.CallStatic("StopListening");
            _speechToText.Dispose();
            _speechToText = null;
#endif

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_IOS
            SpeechRecognizerMacOS.Instance.StopListening();
#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            _dictationRecognizer.Stop();
#endif
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private void OnDictationResult(string text, ConfidenceLevel confidenceLevel)
        {
            OnRecognized(text);
        }

        private void OnDictationHypothesis(string text)
        {
            OnPartialRecognized(text);
        }

        private void OnDictationError(string error, int hresult)
        {
            OnError(hresult, error);
        }
#endif

        private void OnRecognized(string text)
        {
            RecognizedEvent?.Invoke(text);
        }

        private void OnPartialRecognized(string text)
        {
            PartialRecognizedEvent?.Invoke(text);
        }

        private void OnError(long errorCode, string description)
        {
            ErrorEvent?.Invoke(new SpeechRecognizerException(errorCode, description));
        }
    }
}