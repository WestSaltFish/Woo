using TMPro;
using UnityEngine;

public class ClientDebugMessage : MonoBehaviour
{
    TMP_Text _debugText;
    RectTransform _debugTextRect;

    private void Awake()
    {
        _debugText = GetComponent<TMP_Text>();

        _debugTextRect = GetComponent<RectTransform>();

        Client.instance.RegistMessageEvent(OnReceiveMessage);
    }

    private void OnReceiveMessage(string msg)
    {
        _debugText.text += msg+"\n";

        AdjustTextHeight();
    }

    private void AdjustTextHeight()
    {
        _debugText.ForceMeshUpdate();

        float preferredHeight = _debugText.preferredHeight;

        _debugTextRect.sizeDelta = new Vector2(_debugTextRect.sizeDelta.x, preferredHeight);
    }
}
