using System;
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

    private void Start()
    {
        ConnecteToServer();
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

    private void ConnecteToServer()
    {
        _serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _serverPort);

        _client = new UdpClient();

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
                string receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                IPEndPoint remoteEndPoint = result.RemoteEndPoint;

                Debug.Log($"Receive from {remoteEndPoint} | Message: {receivedMessage}");
            }
            catch (Exception ex)
            {
                if (!_connected)
                {
                    Debug.Log("Client already closed!");
                }
                else
                {
                    Debug.Log($"Error receiving data: {ex.Message}.");
                }

                //CloseClientUDP();

                break;
            }
        }
    }

    private void OnDestroy()
    {
        CloseClientUDP();
    }

    private void Update()
    {
        // Send message to server
        if (Input.GetKeyDown(KeyCode.S))
        {
            TestSendMessage();
        }
    }

    private void CloseClientUDP()
    {
        _connected = false;

        if (_client != null)
        {
            _client.Close();
            _client = null;
            Debug.Log("Server close!");
        }
    }
}
