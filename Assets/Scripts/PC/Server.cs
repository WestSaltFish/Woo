using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Server : MonoBehaviour
{
    public static Server instance = null;

    // Server conection
    private UdpClient _server;

    private string _receiveString = null;

    private byte[] _receiveData = null;

    private IPEndPoint _remotePoint = new (IPAddress.Any, 8888);

    private readonly object _lock = new();

    // Server data
    private bool _connected = false;

    private Dictionary<uint, Client> _clients = new();

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

    async private void StartServerAsync()
    {
        _server = new(8888);
        _connected = true;

        while (_connected)
        {
            try
            {
                // Receive result async
                UdpReceiveResult result = await _server.ReceiveAsync();

                // get message
                string receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                IPEndPoint remoteEndPoint = result.RemoteEndPoint;

                Debug.Log($"Receive from {remoteEndPoint} | Message: {receivedMessage}");
            }
            catch (Exception ex)
            {
                if (!_connected)
                {
                    Debug.Log("Server already closed!");
                    break;
                }

                Debug.Log($"Error: {ex.Message}");

                if (_server != null)
                    _server.Close();

                _connected = false;

                break;
            }
        }
    }

    void StartServer()
    { 
        _server = new(8888);
        _connected = true;

        Debug.Log("Server created!");

        while(_connected)
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

                if (_server != null)
                    _server.Close();

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
        if (_server != null) 
        {
            _connected = false;
            _server.Close();
            _server = null;
            Debug.Log("Server closed!");
        }      
    }

    uint getNextUID()
    {
        return _UID++;
    }
}
