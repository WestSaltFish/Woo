using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

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

public class Server : MonoBehaviour
{
    public static Server instance = null;

    // Server conection
    private UdpClient _server;

    //private IPEndPoint _remotePoint = new (IPAddress.Any, 8888);

    private readonly object _lock = new();

    // Server data
    private bool _connected = false;

    private readonly Dictionary<uint, User> _users = new();

    private readonly ConcurrentQueue<MessageHandler> _tasks = new();

    private readonly Dictionary<NetworkMessageType, Action<NetworkMessage>> _actionHandles = new();

    [HideInInspector]
    private uint _UID = 0;

    public void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _actionHandles.Add(NetworkMessageType.JoinServer, HandleJoinServer);

        StartServerAsync();
    }

    void Update()
    {
        if (_tasks.Count > 0)
        {
            if (_tasks.TryDequeue(out MessageHandler task))
                task.Execute();
        }
    }

    private void OnDestroy()
    {
        CloseServerUDP();
    }

    private void StartServerAsync()
    {
        _server = new(8888);
        _connected = true;

        Debug.Log("Server Start!");

        ListenForClientAsync();
    }

    async private void ListenForClientAsync()
    {
        while (_connected)
        {
            try
            {
                // Receive result async
                UdpReceiveResult result = await _server.ReceiveAsync();

                // get message
                NetworkMessage message = NetworkPackage.GetDataFromBytes(result.Buffer, result.Buffer.Length);

                message.endPoint = result.RemoteEndPoint;

                //Debug.Log($"Receive from {remoteEndPoint} | Message: {receivedMessage}");

                _tasks.Enqueue(new(message, _actionHandles[message.type]));

                // Convert the message to a byte array
                //byte[] data = Encoding.ASCII.GetBytes("Hello" + remoteEndPoint);

                // Send the data to the server
                //_server?.Send(data, data.Length, remoteEndPoint);
            }
            catch (Exception ex)
            {
                if (!_connected)
                {
                    Debug.LogWarning("Server already closed!");
                    break;
                }

                Debug.LogWarning($"Error: {ex.Message}");

                _server?.Close();

                _connected = false;

                break;
            }
        }
    }

    // Update is called once per frame


    private void CloseServerUDP()
    {
        _connected = false;

        if (_server != null)
        {
            _connected = false;
            _server.Close();
            _server = null;
            Debug.Log("Server close!");
        }
    }

    private uint GetNextUID()
    {
        return ++_UID;
    }

    public async void SendMessageToClient(User user, NetworkMessage message)
    {
        Debug.Log($"Server: send messages [{message.type}] to client ({user.userName})");

        NetworkPackage package = new(message.type, message.GetBytes());

        byte[] data = package.GetBytes();

        await _server.SendAsync(data, data.Length, user.userEndPoint);

        Debug.Log($"Message send to {user.userEndPoint} sucessful");
    }

    private void HandleJoinServer(NetworkMessage data)
    {
        var message = data as JoinServer;

        if (!_users.ContainsKey(message.ownerUid))
        {
            User user = new(message.name, message.endPoint, GetNextUID());

            message.ownerUid = user.uid;

            _users.Add(user.uid, user);

            Debug.Log($"User with endPoint {message.endPoint} join successfull");

            message.successful = true;
        }
        else
        {
            message.successful = false;

            message.errorCode = NetworkErrorCode.ClientAlreadyInTheServer;

            Debug.LogWarning($"Server error : {message.errorCode}");
        }

        SendMessageToClient(_users[message.ownerUid], message);
    }

    // obsolete
    /*
    void StartServer()
    {
        _server = new(8888);
        _connected = true;

        Debug.Log("Server created!");

        while (_connected)
        {
            try
            {
                _receiveData = _server.Receive(ref _remotePoint); // stuck the program
                Debug.Log("Message received!");
                _receiveString = Encoding.Default.GetString(_receiveData);
                Debug.Log(_receiveString);
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == 10004) // Socket error: An operation was aborted due to a signal from the user.
                {
                    Console.WriteLine("Receive operation was canceled.");
                }
                else
                {
                    Console.WriteLine("Error receiving data: {0}.", ex.Message);
                }

                _server?.Close();

                _connected = false;

                break;
            }
        }
    }
    */
}
