using UnityEngine;

public class HandleMyMsgClient : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Client.instance.RegistHandleEvent(NetworkMessageType.MyMessage, OnReveiveMessage);
    }

    private void OnReveiveMessage(NetworkMessage data)
    {
        var message = data as MyMessage;

        Debug.Log($"Server receive message with succecful {message.successful}");
    }

    private void OnDestroy()
    {
        Client.instance.UnRegistHandleEvent(NetworkMessageType.MyMessage);
    }
}
