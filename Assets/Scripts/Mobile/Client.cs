using System.Net;
using System.Net.Sockets;
using UnityEditor.PackageManager;
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

        // Create a UdpClient object bound to any available port
        _client = new UdpClient();
    }

    public void SetServerPort(uint port)
    {
        _serverPort = port;
    }

    public void ConnecteToServer()
    {
        
    }

    private void Update()
    {
        // Send message to server
    }
}
