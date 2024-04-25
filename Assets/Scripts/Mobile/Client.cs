using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEditor.VersionControl;
using UnityEngine;

public class Client : MonoBehaviour
{
    public uint uid = 0;

    private UdpClient _client;

    private string _name;

    private int _serverPort = 8888;

    private IPEndPoint _serverEndPoint;

    private void Start()
    {
        _serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _serverPort);

        _client = new UdpClient();

        Thread receiveThread = new Thread(ReceiveMessages);

        receiveThread.Start();
    }

    public void SetServerPort(int port)
    {
        _serverPort = port;
    }

    public void ConnecteToServer()
    {
        // Convert the message to a byte array
        byte[] data = Encoding.ASCII.GetBytes("hi");

        // Send the data to the server
        _client.Send(data, data.Length, _serverEndPoint);
    }

    private void ReceiveMessages()
    {
        while (true)
        {
            try
            {
                byte[] receiveBuffer = _client.Receive(ref _serverEndPoint);

                if (receiveBuffer.Length > 0)
                {
                    string message = Encoding.ASCII.GetString(receiveBuffer, 0, receiveBuffer.Length);
                    Debug.Log("Received message from server: " + message);
                }
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
            }

            break;
        }
    }
    private void OnApplicationQuit()
    {
        if (_client != null)
        {
            _client.Close();
            _client = null;
        }
    }

    private void Update()
    {
        // Send message to server
        if (Input.GetKeyDown(KeyCode.S))
        {
            ConnecteToServer();
        }
    }
}
