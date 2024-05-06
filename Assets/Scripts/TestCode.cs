using UnityEngine;

public class TestCode : MonoBehaviour
{
    private void Start()
    {
        Server.instance.onUpdateSensorData += OnSensorUpdate;
    }

    private void OnSensorUpdate(MobileSensorData data)
    {
        transform.eulerAngles = data.mobileSensorData[MobileSensorFlag.Rotation];
    }
}
