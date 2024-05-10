using UnityEngine;
using TMPro;
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
    public TMP_Text debugMeesage;

    public Vector3 Velocity { get => _currentVelocity; }
    public Vector3 Acceleration { get => _currentAcceleration; }
    public Vector3 Gravity { get => _currentGravity; }
    public Vector3 Rotation { get => _currentRotation; }
    public Quaternion Quaternion { get => _currenQuat; }


    private Vector3 _currentVelocity;
    private Vector3 _currentAcceleration;
    private Vector3 _currentGravity;
    private Vector3 _currentRotation;
    private Quaternion _currenQuat;

    // Start is called before the first frame update
    void Start()
    {
        Input.gyro.enabled = true;

        _currentAcceleration = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        _currentGravity = GetGravity();

        UpdateAcceleration();

        UpdateVelocity();

        _currentRotation = Input.gyro.attitude.eulerAngles;
        _currenQuat = Input.gyro.attitude;

        debugMeesage.text = $"Acc: {_currentAcceleration} " +
            $"\n\nAccMag: {_currentAcceleration.magnitude}" +
            $"\n\nVelocity: {_currentVelocity}" +
            $"\n\nGyrocopy: {_currentRotation}" +
            $"\n\nGravity: {-_currentGravity}";
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
            acc -= GetGravity();

        _currentAcceleration = Vector3.Lerp(_currentAcceleration, acc, 0.5f);
    }

    private void UpdateVelocity()
    {
        _currentVelocity += _currentAcceleration * Time.deltaTime;

        if (_currentAcceleration.magnitude < 0.01f)
            _currentVelocity = Vector3.zero;
    }

    private Vector3 GetGravity()
    {
        return Input.gyro.gravity;
    }

    public Vector3 GetEulerAngle()
    {
        return Input.gyro.attitude.eulerAngles;
    }
}
