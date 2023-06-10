
using System;

namespace SpeechRecognizer
{
    public class SpeechRecognizerException : Exception
    {
        long _errorCode;
        public long ErrorCode => _errorCode;
        public SpeechRecognizerException(long errorCode, string message) : base(message)
        {
            _errorCode = errorCode;
        }
    }

    public interface IRecognizer
    {
        event System.Action<SpeechRecognizerException> ErrorEvent;
        event System.Action<string> RecognizedEvent;
        event System.Action<string> PartialRecognizedEvent;
    }

    public interface IMicrophoneRecognizer : IRecognizer
    {
        void Recognize(byte[] audioData);
    }
}