using System.Net;

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
}
