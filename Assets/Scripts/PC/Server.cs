using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class MessageHandler
{
    public MessageHandler(NetworkMessage message)
    {
        _message = message;
    }

    public void Execute()
    {
        //_action.Invoke(_message);
    }

    private readonly NetworkMessage _message;
}

public class Server : MonoBehaviour
{
    public static Server instance = null;

    // Server conection
    private UdpClient _server;

    private byte[] _receiveData = null;

    //private IPEndPoint _remotePoint = new (IPAddress.Any, 8888);

    private readonly object _lock = new();

    // Server data
    private bool _connected = false;

    private readonly Dictionary<uint, User> _users = new();

    private readonly ConcurrentQueue<MessageHandler> _tasks = new();

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
        StartServerAsync();

        //Thread thread = new(StartServer);

        //thread.Start();
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

                // get user endPoint
                //IPEndPoint remoteEndPoint = result.RemoteEndPoint;

                //Debug.Log($"Receive from {remoteEndPoint} | Message: {receivedMessage}");

                _tasks.Enqueue(new(message));

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
    void Update()
    {
           
    }

    private void OnDestroy()
    {
        CloseServerUDP();
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

    uint getNextUID()
    {
        return _UID++;
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
