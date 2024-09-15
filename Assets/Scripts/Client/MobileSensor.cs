using UnityEngine;
using System;

// https://stackoverflow.com/questions/8447/what-does-the-flags-enum-attribute-mean-in-c
[Flags]
public enum MobileSensorFlag
{
    None = 0,               // 00000000
    Velocity = 1 << 0,      // 00000001
    Acceleration = 1 << 1,  // 00000010
    Gravity = 1 << 2,       // 00000100
    Rotation = 1 << 3,       // 00001000
}

public class MobileSensor : MonoBehaviour
{
    public Vector3 Velocity { get => _currentVelocity; }
    public Vector3 Acceleration { get => _currentAcceleration; }
    public Vector3 Gravity { get => Input.gyro.gravity; }
    public Vector3 Rotation { get => Input.gyro.attitude.eulerAngles; }
    public Quaternion Quaternion { get => Input.gyro.attitude; }

    private Vector3 _currentVelocity;
    private Vector3 _currentAcceleration;

    // Start is called before the first frame update
    void Start()
    {
        Input.gyro.enabled = true;

        _currentAcceleration = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAcceleration();

        UpdateVelocity();
    }

    private void UpdateAcceleration(bool withGravity = false)
    {
        Vector3 acc = Vector3.zero;
        float period = 0.0f;

        foreach (AccelerationEvent evnt in Input.accelerationEvents)
        {
            acc += evnt.acceleration * evnt.deltaTime;
            period += evnt.deltaTime;
        }
        if (period > 0)
        {
            acc *= 1.0f / period;
        }

        if (!withGravity)
        {
            acc -= Gravity;
        }

        _currentAcceleration = Vector3.Lerp(_currentAcceleration, acc, 0.5f);
    }

    private void UpdateVelocity()
    {
        _currentVelocity += _currentAcceleration * Time.deltaTime;

        if (_currentAcceleration.magnitude < 0.01f)
        {
            _currentVelocity = Vector3.zero;
        }
    }
}
