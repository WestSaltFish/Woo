using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;

public enum NetworkMessageType
{
    Null,
    JoinServer,
    LeaveServer,
    Heartbeat,
    CloseServer,
    MaxCount,
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

    private delegate NetworkMessage GetDataAction(byte[] data);

    static private Dictionary<NetworkMessageType, GetDataAction> _getDataActions = new()
    {
        { NetworkMessageType.Heartbeat, HearthBeat.GetData },
        { NetworkMessageType.JoinServer, JoinServer.GetData },
        { NetworkMessageType.LeaveServer, LeaveServer.GetData },
    };
}

[Serializable]
public class NetworkMessage
{
    protected NetworkMessage(NetworkMessageType type, uint messageOwnerId)
    {
        this.type = type;
        this.messageOwnerId = messageOwnerId;
    }

    virtual public byte[] GetBytes()
    {
        return Encoding.ASCII.GetBytes(JsonUtility.ToJson(this));
    }

    // 4 server & client
    public NetworkMessageType type = NetworkMessageType.Null;

    public uint messageOwnerId = 0;

    // 4 client
    public bool succesful = false;

    public EndPoint endPoint = null;

    public EndPoint testEndPoint = null;
}

[Serializable]
public class HearthBeat : NetworkMessage
{
    public HearthBeat(uint userId) : base(NetworkMessageType.Heartbeat, userId) { }

    public override byte[] GetBytes()
    {
        return Encoding.ASCII.GetBytes(JsonUtility.ToJson(this));
    }

    static public HearthBeat GetData(byte[] data)
    {
        return JsonUtility.FromJson<HearthBeat>(Encoding.ASCII.GetString(data, 0, data.Length));
    }
}

// -----------------------------------------------
// ------------REQUEST IN THE SERVER--------------
// -----------------------------------------------

[Serializable]
public class JoinServer : NetworkMessage
{
    public JoinServer(string userName) : base(NetworkMessageType.JoinServer, 0)
    {
        name = userName;
    }

    static public JoinServer GetData(byte[] data)
    {
        return JsonUtility.FromJson<JoinServer>(Encoding.ASCII.GetString(data, 0, data.Length));
    }

    // 4 server
    public string name;

    // 4 clients
    public List<uint> roomsId = null;
    public List<int> currentPlayers = null;
    public List<int> maxPlayers = null;
}

[Serializable]
public class LeaveServer : NetworkMessage
{
    public LeaveServer(uint userId, bool forceLeave = false) : base(NetworkMessageType.LeaveServer, userId)
    {
        succesful = forceLeave;
    }

    static public LeaveServer GetData(byte[] data)
    {
        return JsonUtility.FromJson<LeaveServer>(Encoding.ASCII.GetString(data, 0, data.Length));
    }
}
