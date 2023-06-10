
using UnityEngine;
using SpeechRecognizer;

public class SpeechRecognizerTest : MonoBehaviour
{
    void Start()
    {
        var recognizer = GetComponent<IRecognizer>();
        recognizer.RecognizedEvent += OnRecognized;
        recognizer.PartialRecognizedEvent += OnPartialRecognized;
        recognizer.ErrorEvent += OnError;
    }

    void OnRecognized(string message)
    {
        Debug.Log("OnRecognized: " + message);
    }

    void OnPartialRecognized(string message)
    {
        Debug.Log("OnPartialRecognized: " + message);
    }

    void OnError(SpeechRecognizerException exception)
    {
        Debug.LogError("OnError: " + exception.Message);
    }
}