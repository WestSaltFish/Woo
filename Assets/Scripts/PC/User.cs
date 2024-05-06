using System.Net;
using System.Numerics;

public struct UserSensorData
{
    public UnityEngine.Vector3 Velocity { get; set; }
    public UnityEngine.Vector3 Acceleration { get; set; }
    public UnityEngine.Vector3 Rotation { get; set; }
    public UnityEngine.Vector3 Gravity { get; set; }
}

public class User
{
    public User(string name, IPEndPoint endPoint, uint uid)
    {
        userName = name;
        userEndPoint = endPoint;
        this.uid = uid;
    }

    public string userName = "Default";
    public IPEndPoint userEndPoint;
    public uint uid = 0;

    public UserSensorData senserData;
}
