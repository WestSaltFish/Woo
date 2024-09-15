using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using TMPro;
using System.Net.NetworkInformation;
using System.Linq;

public class Server : MonoBehaviour
{
    // Singleton instance
    public static Server instance = null;

    // Server conection
    [SerializeField, Disable] private bool _connected = false;
    [SerializeField] private int _port = 8888;
    [SerializeField] private int _maxRetries = 3;
    private UdpClient _server;
    private readonly object _lock = new();

    // Server data
    [SerializeField] private MobileSensorFlag _sensorEnable;
    private readonly Dictionary<uint, User> _users = new();
    private readonly ConcurrentQueue<MessageHandler> _tasks = new();
    private readonly Dictionary<NetworkMessageType, Action<NetworkMessage>> _actionHandles = new();

    public Action<MobileSensorData> onUpdateSensorData;

    // to generate user uid
    private uint _genUID = 0;

    public TMP_Text debugText;

    #region Unity events
    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        RegistHandleEvent(NetworkMessageType.JoinServer, HandleJoinServer);
        RegistHandleEvent(NetworkMessageType.LeaveServer, HandleLeaveServer);
        RegistHandleEvent(NetworkMessageType.MobileSensorEnable, HandleSensorEnable);
        RegistHandleEvent(NetworkMessageType.MobileSensorData, HandleSensorData);

        StartServerAsync();
    }

    void Update()
    {
        if (_tasks.Count > 0)
        {
            if (_tasks.TryDequeue(out MessageHandler task))
            {
                task.Execute();
            }
        }
    }

    private void OnDestroy()
    {
        CloseServerUDP();
    }

    #endregion

    #region Network
    public void RegistHandleEvent(NetworkMessageType type, Action<NetworkMessage> callbackFunc)
    {
        if (!_actionHandles.ContainsKey(type))
        {
            _actionHandles.Add(type, callbackFunc);
        }
    }

    public void UnRegistHandleEvent(NetworkMessageType type)
    {
        _actionHandles.Remove(type);
    }

    public void StartServerAsync()
    {
        if (!_connected)
        {
            _server = new(0);

            _port = ((IPEndPoint)_server.Client.LocalEndPoint).Port;

            _connected = true;

            if (debugText != null)
            {
                debugText.text = GetLocalIPAddress() + " : " + _port;
            }

            Debug.Log($"Server Start with {GetLocalIPAddress()} : {_port}!");

            ListenForClientAsync();
        }
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

                // Get sender ip
                message.endPoint = result.RemoteEndPoint;

                _tasks.Enqueue(new(message, _actionHandles[message.type]));
            }
            catch (Exception ex)
            {
                if (!_connected)
                {
                    Debug.LogWarning("Server already closed!");

                    return;
                }

                Debug.LogWarning($"Error: {ex.Message}");

                CloseServerUDP();
            }
        }
    }

    /// <summary>
    /// Send message to all client
    /// </summary>
    /// <param name="package"></param>
    public void SendMessageToClients(NetworkPackage package)
    {
        Debug.Log($"Server: send messages [{package.type}] to all client");

        byte[] data = package.GetBytes();

        foreach (var user in _users)
        {
            SendMessageToClient(data, user.Value.userEndPoint);
        }
    }

    /// <summary>
    /// Send message to specific client using message storaged endpoint
    /// </summary>
    /// <param name="message"></param>
    public void SendMessageToClient(NetworkMessage message)
    {
        Debug.Log($"Server: send messages [{message.type}] to client ({message.endPoint})");

        NetworkPackage package = new(message.type, message.GetBytes());

        byte[] data = package.GetBytes();

        SendMessageToClient(data, message.endPoint);
    }

    /// <summary>
    /// Send message to specific client
    /// </summary>
    /// <param name="message"></param>
    public async void SendMessageToClient(byte[] data, IPEndPoint endPoint)
    {
        int retryCount = 0;

        while (retryCount < _maxRetries)
        {
            try
            {
                await _server.SendAsync(data, data.Length, endPoint);

                Debug.Log($"Message send to {endPoint} sucessful");

                return;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
            {
                // time out 
                Debug.LogWarning($"Connection timed out: {ex.Message}");
                retryCount++;
            }
            catch (Exception ex)
            {
                // Another error
                Debug.LogWarning($"Connection error : {ex.Message}.");
                return;
            }
        }

        Debug.LogWarning($"Server: Failed to send message after {_maxRetries} retries.");

        return;
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

    #region Requests

    public void RequestEnableSensor()
    {
        var msg = NetworkMessageFactory.MobileSensorEnableMessage(_sensorEnable);

        SendMessageToClients(msg);
    }

    #endregion

    #region Utils
    private uint GetNextUID()
    {
        return ++_genUID;
    }

    public string GetLocalIPAddress()
    {
        /*
        IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress ip in ipEntry.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }

        return "0.0.0.0";

        */

        string wifiIpAddress = "0.0.0.0";

        // Get all network interface
        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (NetworkInterface iface in interfaces)
        {
            // Only find wifi interface
            if (iface.OperationalStatus == OperationalStatus.Up && iface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
            {
                IPInterfaceProperties ipProperties = iface.GetIPProperties();

                // Get a unic ip adress
                UnicastIPAddressInformation ipAddress = ipProperties.UnicastAddresses.FirstOrDefault(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork);

                if (ipAddress != null)
                {
                    wifiIpAddress = ipAddress.Address.ToString();
                    break;
                }
            }
        }

        return wifiIpAddress; 
    }

    public User GetUser(uint uid)
    {
        if(_users.ContainsKey(uid))
        {
            return _users[uid];
        }
        else
        {
            Debug.Log($"user with uid: {uid} doesn't exist.");
            return null;
        }
    }
    #endregion

    #region Message handlers
    private void HandleJoinServer(NetworkMessage data)
    {
        var msg = data as JoinServer;

        if (!_users.ContainsKey(msg.ownerUID))
        {
            User user = new(msg.name, msg.endPoint, GetNextUID());

            msg.ownerUID = user.uid;

            _users.Add(user.uid, user);

            msg.successful = true;

            Debug.Log($"User with endPoint {msg.endPoint} join successfull");
        }
        else
        {
            msg.successful = false;

            msg.errorCode = NetworkErrorCode.ClientAlreadyInTheServer;

            Debug.LogWarning($"Server error : {msg.errorCode}");
        }

        SendMessageToClient(msg);
    }

    private void HandleLeaveServer(NetworkMessage data)
    {
        var msg = data as LeaveServer;

        if (_users.ContainsKey(msg.ownerUID))
        {
            _users.Remove(msg.ownerUID);

            msg.successful = true;

            Debug.Log($"User with endPoint {msg.endPoint} leave successfull");
        }
        else
        {
            msg.successful = false;

            msg.errorCode = NetworkErrorCode.ClientAlreadyLeaveTheServer;

            Debug.LogWarning($"Server error : {msg.errorCode}");
        }

        SendMessageToClient(msg);
    }

    private void HandleSensorEnable(NetworkMessage data)
    {
        // Nothing for now
    }

    private void HandleSensorData(NetworkMessage data)
    {
        var msg = data as MobileSensorData;

        onUpdateSensorData?.Invoke(msg);

        if (!_users.ContainsKey(msg.ownerUID))
        {
            Debug.Log($"User with endPoint {msg.endPoint} is not exist in the server");

            msg.successful = false;
        }
        else
        {
            var user = _users[msg.ownerUID];

            msg.successful = true;

            // Only update necesarry data
            try
            {
                if ((_sensorEnable & MobileSensorFlag.Velocity) != 0)
                {
                    user.senserData.Velocity = msg.vel;
                }
                if ((_sensorEnable & MobileSensorFlag.Acceleration) != 0)
                {
                    user.senserData.Acceleration = msg.acc;
                }
                if ((_sensorEnable & MobileSensorFlag.Rotation) != 0)
                {
                    user.senserData.Rotation = msg.rot;
                }
                if ((_sensorEnable & MobileSensorFlag.Gravity) != 0)
                {
                    user.senserData.Gravity = msg.grav;
                }
            }
            catch (Exception ex)
            {
                msg.successful = false;

                Debug.Log($"Update date error: {ex}");
            }

            if (msg.successful)
            {
                Debug.Log($"User with endPoint {msg.endPoint} has updated her sensor data");
            }
        }
    }
    #endregion
}
