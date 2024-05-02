using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    // Singleton instance
    public static Server instance = null;

    // Server conection
    [SerializeField] private int _port = 8888;
    private UdpClient _server;
    private readonly object _lock = new();

    // Server data
    [SerializeField, Disable] private bool _connected = false;
    private readonly Dictionary<uint, User> _users = new();
    private readonly ConcurrentQueue<MessageHandler> _tasks = new();
    private readonly Dictionary<NetworkMessageType, Action<NetworkMessage>> _actionHandles = new();

    // to generate user uid
    private uint _genUID = 0;


    #region Unity events
    public void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;
    }

    void Start()
    {
        _actionHandles.Add(NetworkMessageType.JoinServer, HandleJoinServer);
        _actionHandles.Add(NetworkMessageType.LeaveServer, HandleLeaveServer);

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

    #endregion


    #region Network
    private void StartServerAsync()
    {
        if(!_connected)
        {
            _server = new(_port);
            _connected = true;

            Debug.Log("Server Start!");

            ListenForClientAsync();
        }
    }
   
    async private void ListenForClientAsync()
    {
        if (_connected)
        {
            try
            {
                // Receive result async
                UdpReceiveResult result = await _server.ReceiveAsync();

                // Start to listen immediatly
                ListenForClientAsync();

                // get message
                NetworkMessage message = NetworkPackage.GetDataFromBytes(result.Buffer, result.Buffer.Length);

                // Get sender ip
                message.endPoint = result.RemoteEndPoint;

                _tasks.Enqueue(new(message, _actionHandles[message.type]));
            }
            catch (Exception ex)
            {
                if (!_connected)
                {
                    Debug.LogWarning("Server already closed!");
                }

                Debug.LogWarning($"Error: {ex.Message}");

                CloseServerUDP();
            }
        }
    }

    public async void SendMessageToClient(NetworkMessage message)
    {
        Debug.Log($"Server: send messages [{message.type}] to client ({message.endPoint})");

        NetworkPackage package = new(message.type, message.GetBytes());

        byte[] data = package.GetBytes();

        await _server.SendAsync(data, data.Length, message.endPoint);

        Debug.Log($"Message send to {message.endPoint} sucessful");
    }

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
    #endregion

    #region Utils
    private uint GetNextUID()
    {
        return ++_genUID;
    }
    #endregion


    #region Message handlers
    private void HandleJoinServer(NetworkMessage data)
    {
        var message = data as JoinServer;

        if (!_users.ContainsKey(message.ownerUID))
        {
            User user = new(message.name, message.endPoint, GetNextUID());

            message.ownerUID = user.uid;

            _users.Add(user.uid, user);

            message.successful = true;
            
            Debug.Log($"User with endPoint {message.endPoint} join successfull");
        }
        else
        {
            message.successful = false;

            message.errorCode = NetworkErrorCode.ClientAlreadyInTheServer;

            Debug.LogWarning($"Server error : {message.errorCode}");
        }

        SendMessageToClient(message);
    }
    
    private void HandleLeaveServer(NetworkMessage data)
    {
        var message = data as LeaveServer;

        if (_users.ContainsKey(message.ownerUID))
        {
            _users.Remove(message.ownerUID);

            message.successful = true;

            Debug.Log($"User with endPoint {message.endPoint} leave successfull");
        }
        else
        {
            message.successful = false;

            message.errorCode = NetworkErrorCode.ClientAlreadyLeaveTheServer;

            Debug.LogWarning($"Server error : {message.errorCode}");
        }

        SendMessageToClient(message);
    }
    #endregion

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
