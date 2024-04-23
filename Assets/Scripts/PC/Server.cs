using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour
{
    public static Server instance = null;

    // Server conection
    private UdpClient _server;

    private string _receiveString = null;

    private byte[] _receiveData = null;

    private IPEndPoint _remotePoint = new IPEndPoint(IPAddress.Any, 8888);
    
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
        Thread thread = new(StartServer);

        thread.Start();
    }

    void StartServer()
    { 
        _server = new(8888);
        Debug.Log("Server created!");

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
                Console.WriteLine("Error receiving data: {0}", ex.Message);
            }
        }

        if (_server != null) 
            _server.Close();
    }


    // Update is called once per frame
    void Update()
    {
           
    }

    private void OnDestroy()
    {
        if (_server != null) 
        {
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
