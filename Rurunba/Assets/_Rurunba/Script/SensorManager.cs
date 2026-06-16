using UnityEngine;
using UnityEngine.InputSystem;
using Gyro = UnityEngine.InputSystem.Gyroscope;

public class GyroManager : MonoBehaviour
{
    void Start()
    {
        // 重力センサー（傾き用）
        if (GravitySensor.current != null)
            InputSystem.EnableDevice(GravitySensor.current);

        // 加速度センサー（衝撃検知や、重力センサーが動かない時の予備）
        if (Accelerometer.current != null)
            InputSystem.EnableDevice(Accelerometer.current);

        // ジャイロスコープ（回転速度の計測）
        if (UnityEngine.InputSystem.Gyroscope.current != null)
            InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
    }
}