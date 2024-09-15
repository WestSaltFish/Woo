using UnityEngine;

public class HandleMyMsgServer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Server.instance.RegistHandleEvent(NetworkMessageType.MyMessage, OnReveiveMessage);
    }

    private void OnReveiveMessage(NetworkMessage data)
    {
        var message = data as MyMessage;

        Debug.Log($"Message receive from user {message.ownerUID}: {message.message}");

        message.successful = true;

        Server.instance.SendMessageToClient(message);
    }

    private void OnDestroy()
    {
        Server.instance.UnRegistHandleEvent(NetworkMessageType.MyMessage);
    }
}
