using UnityEngine;
using TMPro;

public class DebugWindow : MonoBehaviour
{
    [SerializeField] TMP_Text textMesh = null;

    void OnEnable()
    {
        // TODO only enable debug window if using Unity Editor - don't render in game. MAybe spawn the whole prefab into scene only if in Editor
        Application.logMessageReceived += LogMessage;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogMessage;
    }

    public void LogMessage(string message, string stackTrace, LogType type)
    {

        if(type.Equals(LogType.Warning) || type.Equals(LogType.Error))
        {
            //AppendToConsole(message);
        }

        if (type.Equals(LogType.Assert))
        {
            LiveShowInConsole(message);
        }
    }

    void LiveShowInConsole(string message)
    {
        if (!textMesh) return;

        textMesh.text = message;
        
    }

    public void LogCustomMessage(string message)
    {
        AppendToConsole(message);
    }

    public void AppendToConsole(string message)
    {
        if (!textMesh) return;

        if (textMesh.text.Length > 250)
        {
            textMesh.text =  message;
        }
        else
        {
            textMesh.text += "\n" + message;
        }
    }
}
