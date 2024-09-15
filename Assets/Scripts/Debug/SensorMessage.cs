using TMPro;
using UnityEngine;

public class SensorMessage : MonoBehaviour
{
    public TMP_Text message;
    public MobileSensor sensor;

    // Update is called once per frame
    void Update()
    {
        message.text = $"Acc: {sensor.Acceleration} " +
    $"\n\nAccMag: {sensor.Acceleration.magnitude}" +
    $"\n\nVelocity: {sensor.Velocity}" +
    $"\n\nGyrocopy: {sensor.Rotation}" +
    $"\n\nGravity: {-sensor.Gravity}";
    }
}
