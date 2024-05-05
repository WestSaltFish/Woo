using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;

public enum NetworkMessageType
{
    None,
    JoinServer,
    LeaveServer,
    Heartbeat,
    CloseServer,
    Message,
    MobileRotation,
    MaxCount,
}

public enum NetworkErrorCode
{
    None,
    ClientAlreadyInTheServer,
    ClientAlreadyLeaveTheServer,
}

[Serializable]
public class NetworkPackage
{
    public NetworkPackage(NetworkMessageType type, byte[] data)
    {
        this.type = type;
        this.data = data;
    }

    public byte[] GetBytes()
    {
        return Encoding.ASCII.GetBytes(JsonUtility.ToJson(this));
    }

    public NetworkMessage GetData()
    {
        return _getDataActions[type].Invoke(data);
    }

    public static NetworkMessage GetDataFromBytes(byte[] data, int lenght)
    {
        var jsonString = Encoding.ASCII.GetString(data, 0, lenght);

        var networkPackage = JsonUtility.FromJson<NetworkPackage>(jsonString);

        return networkPackage.GetData();
    }

    public NetworkMessageType type;

    public byte[] data;

    private static readonly Dictionary<NetworkMessageType, Func<byte[],NetworkMessage>> _getDataActions = new()
    {
        { NetworkMessageType.Heartbeat, GetData<HearthBeat> },
        { NetworkMessageType.JoinServer, GetData<JoinServer> },
        { NetworkMessageType.LeaveServer, GetData<LeaveServer> },
        { NetworkMessageType.MobileRotation, GetData<MobileRotation> },
    };

    static public T GetData<T>(byte[] data)
    {
        return JsonUtility.FromJson<T>(Encoding.ASCII.GetString(data, 0, data.Length));
    }
}

[Serializable]
public class NetworkMessage
{
    protected NetworkMessage(NetworkMessageType type, uint ownerUid)
    {
        this.type = type;
        this.ownerUID = ownerUid;
    }

    virtual public byte[] GetBytes()
    {
        return Encoding.ASCII.GetBytes(JsonUtility.ToJson(this));
    }

    // 4 server & client
    public NetworkMessageType type = NetworkMessageType.None;

    public uint ownerUID = 0;

    public IPEndPoint endPoint = null;

    public NetworkErrorCode errorCode = NetworkErrorCode.None;

    // 4 client
    public bool successful = false;
}

public class NetworkMessageFactory
{
    static public NetworkPackage JoinServerMessage(uint uid, string userName)
    {
        JoinServer msg = new(uid, userName);

        return new NetworkPackage(NetworkMessageType.JoinServer, msg.GetBytes());
    }

    static public NetworkPackage HeartBeatMessage(uint uid)
    {
        HearthBeat msg = new(uid);

        return new NetworkPackage(NetworkMessageType.Heartbeat, msg.GetBytes());
    }

    static public NetworkPackage LeaveServertMessage(uint uid)
    {
        LeaveServer msg = new(uid);

        return new NetworkPackage(NetworkMessageType.LeaveServer, msg.GetBytes());
    }

    static public NetworkPackage MobileRotationMessage(uint uid, Vector3 rotation)
    {
        MobileRotation msg = new(uid, rotation);

        return new NetworkPackage(NetworkMessageType.MobileRotation, msg.GetBytes());
    }
}

// -----------------------------------------------
// ------------REQUEST IN THE SERVER--------------
// -----------------------------------------------

[Serializable]
public class HearthBeat : NetworkMessage
{
    public HearthBeat(uint userId) : base(NetworkMessageType.Heartbeat, userId) { }
}

[Serializable]
public class JoinServer : NetworkMessage
{
    public JoinServer(uint uid, string userName) : base(NetworkMessageType.JoinServer, 0)
    {
        ownerUID = uid;
        name = userName;
    }

    // 4 server
    public string name;
}

[Serializable]
public class LeaveServer : NetworkMessage
{
    public LeaveServer(uint userId, bool forceLeave = false) : base(NetworkMessageType.LeaveServer, userId)
    {
        successful = forceLeave;
    }
}

[Serializable]
public class Message : NetworkMessage
{
    public Message(uint userId, string message) : base(NetworkMessageType.Message, userId)
    {
        this.message = message;
    }

    // 4 server
    public string message;
}

public class MobileRotation : NetworkMessage
{
    public MobileRotation(uint userId, Vector3 rotation) : base(NetworkMessageType.Message, userId)
    {
        this.rotation = rotation;
    }

    // 4 server
    public Vector3 rotation;
}
