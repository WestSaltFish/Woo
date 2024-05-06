using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using TMPro;

 [RequireComponent(typeof(MobileSensor))]
public class Client : MonoBehaviour
{
    // User info
    [Header("User params")]
    public uint uid = 0;
    private string _name = "default";

    // Network params
    [Header("Network params")]
    [SerializeField, Disable] private bool _connected = false;
    [SerializeField, Disable] private string _serverIp = "169.254.72.24";
    [SerializeField] private int _serverPort = 8888;
    private IPEndPoint _serverEndPoint;
    private UdpClient _client = null;
    private readonly ConcurrentQueue<MessageHandler> _tasks = new();
    private readonly Dictionary<NetworkMessageType, Action<NetworkMessage>> _actionHandles = new();
    [SerializeField, Disable] private MobileSensorFlag _sensorFlag = MobileSensorFlag.None;
    private Dictionary<MobileSensorFlag, Vector3> _sensorValues = new();
    private MobileSensor _mobileSensor;

    public TMP_Text _debugText;

    #region Unity evets

    private void Start()
    {
        _mobileSensor = GetComponent<MobileSensor>();

        _actionHandles.Add(NetworkMessageType.JoinServer, HandleJoinServer);
        _actionHandles.Add(NetworkMessageType.LeaveServer, HandleLeaveServer);
        _actionHandles.Add(NetworkMessageType.MobileSensorEnable, HandleSensorEnable);
        _actionHandles.Add(NetworkMessageType.MobileSensorData, HandleSensorData);
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
            RequestConnectToServer();
        }
        else if(Input.GetKeyDown(KeyCode.D))
        {
            RequestLeaveFromServer();
        }

        RequestSendSensorData(); 
    }

    private void OnDestroy()
    {
        CloseClientUDP();
    }

    #endregion

    #region Network
    // Receivr message from server
    async private void ReceiveMessagesAsync()
    {
        if (_connected)
        {
            try
            {
                // Receive result async
                UdpReceiveResult result = await _client.ReceiveAsync();

                // Start to listen message immediatly
                ReceiveMessagesAsync();

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

                    CloseClientUDP();
                }
            }
        }
    }

    public void SetServerPort(int port)
    {
        _serverPort = port;
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
    #endregion

    #region Requests
    public async void RequestConnectToServer()
    {
        if(_client == null)
        {
            // Get Server Port
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(_serverIp), _serverPort);

            // Init client udp
            _client = new();

            try
            {
                _client.Connect(_serverEndPoint);

                byte[] data = NetworkMessageFactory.JoinServerMessage(uid, _name).GetBytes();

                int bytesSent = await _client.SendAsync(data, data.Length);

                Debug.Log($"Sent {bytesSent} bytes to server {_serverEndPoint}.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Connecting error : {ex.Message}.");
                _debugText.text = $"Connecting error : {ex.Message}.";

                CloseClientUDP();

                return;
            }

            // temporal
            _connected = true;

            ReceiveMessagesAsync();
        }
    }

    private async void RequestLeaveFromServer()
    {
        if (_client != null)
        {
            try
            {
                byte[] data = NetworkMessageFactory.LeaveServertMessage(uid).GetBytes();

                int bytesSent = await _client.SendAsync(data, data.Length);

                Debug.Log($"Sent {bytesSent} bytes to server {_serverEndPoint}.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Connecting error : {ex.Message}.");

                return;
            }
        }
    }

    private async void RequestSendSensorData()
    {
        if (_client != null && _sensorFlag != MobileSensorFlag.None) 
        {
            try
            {
                if ((_sensorFlag & MobileSensorFlag.Velocity) != 0)
                {
                    _sensorValues[MobileSensorFlag.Velocity] = _mobileSensor.Velocity;
                }
                if ((_sensorFlag & MobileSensorFlag.Acceleration) != 0)
                {
                    _sensorValues[MobileSensorFlag.Acceleration] = _mobileSensor.Acceleration;
                }
                if ((_sensorFlag & MobileSensorFlag.Rotation) != 0)
                {
                    _sensorValues[MobileSensorFlag.Rotation] = _mobileSensor.Rotation;
                }
                if ((_sensorFlag & MobileSensorFlag.Gravity) != 0)
                {
                    _sensorValues[MobileSensorFlag.Gravity] = _mobileSensor.Gravity;
                }

                byte[] data = NetworkMessageFactory.MobileSensorDataMessage(uid, _sensorValues).GetBytes();

                int bytesSent = await _client.SendAsync(data, data.Length);

                Debug.Log($"Sent {bytesSent} bytes to server {_serverEndPoint}.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Connecting error : {ex.Message}.");

                return;
            }
        }
    }

    #endregion

    #region Message handlers
    private void HandleJoinServer(NetworkMessage data)
    {
        var message = data as JoinServer;

        if (message.successful)
        {
            uid = message.ownerUID;
            Debug.Log("Join server successful");

            if (_debugText != null) 
                _debugText.text = "Join server successful";
        }
        else
        {
            Debug.Log($"User error: {message.errorCode}");

            if (_debugText != null)
                _debugText.text = $"User error: {message.errorCode}";
        }
    }

    private void HandleLeaveServer(NetworkMessage data)
    {
        var message = data as LeaveServer;

        if(message.successful || message.errorCode == NetworkErrorCode.ClientAlreadyLeaveTheServer || _client == null)
        {
            uid = 0;
            CloseClientUDP();
            Debug.Log("Leave server successful");
        }
        else
        {
            RequestLeaveFromServer();

            Debug.Log($"User error: {message.errorCode}");
        }
    }

    private void HandleSensorEnable(NetworkMessage data)
    {
        var message = data as MobileSensorEnable;

        _sensorFlag = message.enableFlag;

        _sensorValues.Clear();

        if ((_sensorFlag & MobileSensorFlag.Velocity) != 0)
        {
            _sensorValues.Add(MobileSensorFlag.Velocity, _mobileSensor.Velocity);
        }
        if ((_sensorFlag & MobileSensorFlag.Acceleration) != 0)
        {
            _sensorValues.Add(MobileSensorFlag.Acceleration, _mobileSensor.Acceleration);
        }
        if ((_sensorFlag & MobileSensorFlag.Rotation) != 0)
        {
            _sensorValues.Add(MobileSensorFlag.Rotation, _mobileSensor.Rotation);
        }
        if ((_sensorFlag & MobileSensorFlag.Gravity) != 0)
        {
            _sensorValues.Add(MobileSensorFlag.Gravity, _mobileSensor.Gravity);
        }
    }

    private void HandleSensorData(NetworkMessage data)
    {
        if (data.successful)
        {
            Debug.Log("Sensor data send to server successful");
        }
    }

    #endregion
}
