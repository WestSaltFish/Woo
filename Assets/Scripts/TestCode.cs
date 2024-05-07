using UnityEngine;

public class TestCode : MonoBehaviour
{
    private Vector3 offset = Vector3.zero;

    private Quaternion rot;

    private void Start()
    {
        Server.instance.onUpdateSensorData += OnSensorUpdate;

        transform.rotation = Quaternion.Euler(90f, 90f, 0f);

        rot = new Quaternion(0, 0, 1, 0);
    }

    private void OnSensorUpdate(MobileSensorData data)
    {
        //Vector3 vec = new(data.rot.y, data.rot.x, data.rot.z);

        //transform.eulerAngles = vec - offset;
        //transform.rotation
        Quaternion sensorRotation = new (data.quat.x, -data.quat.z, data.quat.y, data.quat.w);

        transform.rotation = sensorRotation * rot;
    }

    public void ResetTransform()
    {
        offset = transform.eulerAngles + offset;
    }
}
