using System;

public class MessageHandler
{
    public MessageHandler(NetworkMessage message, Action<NetworkMessage> action)
    {
        _message = message;
        _action = action;
    }

    public void Execute()
    {
        _action.Invoke(_message);
    }

    private readonly NetworkMessage _message;
    private readonly Action<NetworkMessage> _action;
}
