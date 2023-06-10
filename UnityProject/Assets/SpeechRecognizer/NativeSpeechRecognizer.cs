using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        IEnumerator Start()
        {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_IOS
            SpeechRecognizerMacOS.Instance.Initialize();
            SpeechRecognizerMacOS.Instance.RecognizedEvent += OnRecognized;
            SpeechRecognizerMacOS.Instance.PartialRecognizedEvent += OnPartialRecognized;
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
        }

        private void OnRecognized(string text)
        {
            RecognizedEvent?.Invoke(text);
        }

        private void OnPartialRecognized(string text)
        {
            PartialRecognizedEvent?.Invoke(text);
        }
    }
}