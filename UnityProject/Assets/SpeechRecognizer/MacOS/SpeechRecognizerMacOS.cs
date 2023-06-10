using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace SpeechRecognizer
{
    public class SpeechRecognizerMacOS
    {
        static class Native
        {
            [DllImport("SpeechRecognizer")]
            public static extern void RegisterRecognizedCallback(RecognizedCallback callback);
            [DllImport("SpeechRecognizer")]
            public static extern void RegisterPartialRecognizedCallback(RecognizedCallback callback);
            [DllImport("SpeechRecognizer")]
            public static extern void RegisterErrorCallback(ErrroCallback callback);

            [DllImport("SpeechRecognizer")]
            public static extern void Initialize(string locale);
            [DllImport("SpeechRecognizer")]
            public static extern void StartListening();
            [DllImport("SpeechRecognizer")]
            public static extern void StopListening();
            [DllImport("SpeechRecognizer")]
            public static extern void Finish();
        }

        private static SpeechRecognizerMacOS _instance;
        public static SpeechRecognizerMacOS Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SpeechRecognizerMacOS();
                }
                return _instance;
            }
        }

        public delegate void RecognizedCallback(string message);
        public delegate void ErrroCallback(long errorCode, string message);

        public event System.Action<string> RecognizedEvent;
        public event System.Action<string> PartialRecognizedEvent;
        public event System.Action<Exception> ErrorEvent;

        public void Initialize(string locale = "ja_JP")
        {
            Native.Initialize(locale);
            Native.RegisterRecognizedCallback(OnRecognized);
            Native.RegisterPartialRecognizedCallback(OnPartialRecognized);
            Native.RegisterErrorCallback(OnError);
        }

        public void StartListening()
        {
            Native.StartListening();
        }

        public void StopListening()
        {
            Native.StopListening();
        }

        public void Finish()
        {
            Native.Finish();
            RecognizedEvent = null;
            PartialRecognizedEvent = null;
            ErrorEvent = null;
        }

        [AOT.MonoPInvokeCallback(typeof(RecognizedCallback))]
        static void OnRecognized(string message)
        {
            Instance.RecognizedEvent?.Invoke(message);
        }

        [AOT.MonoPInvokeCallback(typeof(RecognizedCallback))]
        static void OnPartialRecognized(string message)
        {
            Instance.PartialRecognizedEvent?.Invoke(message);
        }

        [AOT.MonoPInvokeCallback(typeof(ErrroCallback))]
        static void OnError(long errorCode, string message)
        {
            Instance.ErrorEvent?.Invoke(new SpeechRecognizerException(errorCode, message));
        }
    }
}