using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Client : MonoBehaviour
{
    public uint uid = 0;

    private UdpClient _client;

    private string _name;

    private int _serverPort = 8888;

    private IPEndPoint _serverEndPoint;

    private bool _connected = false;

    private readonly ConcurrentQueue<MessageHandler> _tasks = new();

    private readonly Dictionary<NetworkMessageType, Action<NetworkMessage>> _actionHandles = new();

    private void Start()
    {
        _actionHandles.Add(NetworkMessageType.JoinServer, HandleJoinServer);

        ConnecteToServer();
    }



    private void Update()
    {
        if (_tasks.Count > 0)
        {
            if (_tasks.TryDequeue(out MessageHandler task))
                task.Execute();
        }

        // Send message to server
        if (Input.GetKeyDown(KeyCode.S))
        {
            TestSendMessage();
        }
    }

    private void OnDestroy()
    {
        CloseClientUDP();
    }

    public void SetServerPort(int port)
    {
        _serverPort = port;
    }

    void TestSendMessage()
    {
        // Convert the message to a byte array
        byte[] data = Encoding.ASCII.GetBytes("hi");

        // Send the data to the server
        _client?.Send(data, data.Length, _serverEndPoint);
    }

    private async void ConnecteToServer()
    {
        // Get Server Port
        _serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _serverPort);

        // Init client udp
        _client = new();

        try
        {
            _client.Connect(_serverEndPoint);

            byte[] data = NetworkMessageFactory.JoinServerMessage("Chen").GetBytes();

            int bytesSent = await _client.SendAsync(data, data.Length);

            Debug.Log($"Sent {bytesSent} bytes to {_serverEndPoint}.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Connecting error : {ex.Message}.");

            return;
        }

        // temporal
        _connected = true;

        ReceiveMessagesAsync();
    }

    async private void ReceiveMessagesAsync()
    {
        while (_connected)
        {
            try
            {
                // Receive result async
                UdpReceiveResult result = await _client.ReceiveAsync();

                // get message
                NetworkMessage message = NetworkPackage.GetDataFromBytes(result.Buffer, result.Buffer.Length);

                _tasks.Enqueue(new(message, _actionHandles[message.type]));
            }
            catch (Exception ex)
            {
                if (!_connected)
                {
                    Debug.LogWarning("Client already closed!");
                }
                else
                {
                    Debug.LogWarning($"Error receiving data: {ex.Message}.");
                }

                //CloseClientUDP();

                break;
            }
        }
    }

    private void CloseClientUDP()
    {
        _connected = false;

        if (_client != null)
        {
            _client.Close();
            _client = null;
            Debug.Log("Client close!");
        }
    }

    private void HandleJoinServer(NetworkMessage data)
    {
        var message = data as JoinServer;

        if (message.succesful)
        {
            Debug.Log("Join server succesful");
        }
    }
}
