using UnityEngine;
using UnityEngine.InputSystem;

public class GyroPlayer : MonoBehaviour
{
    [Header("走行設定")]
    public float moveSpeed = 15f;
    public float sensitivity = 5f;

    [Header("回転の速さ")]
    public float turnSmoothSpeed = 10f;

    [Header("ノックバック")]
    public float knockbackPower = 10f;
    public float knockbackTime = 0.3f;

    private bool isKnockback = false;
    private float knockbackTimer = 0f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (Accelerometer.current != null)
            InputSystem.EnableDevice(Accelerometer.current);
    }

    void Update()
    {
        // TimeManagerが存在していて、かつ、まだタイマーが走っていない（カウントダウン中）のとき
        if (TimeManager.instance != null && !TimeManager.instance.isTimerRunning)
        {
            if (rb != null)
            {
                // 元々止まっていた時と同じように、速度を完全にゼロにして固定します
                rb.linearVelocity = Vector3.zero;
            }
            return; // ここから下のジャイロの移動・回転計算をすべて無視して処理を抜けます
        }

        // ノックバック中
        if (isKnockback)
        {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0)
            {
                isKnockback = false;
            }

            return;
        }

        if (Accelerometer.current == null) return;

        Vector3 accel = Accelerometer.current.acceleration.ReadValue();

        float moveZ = accel.y * sensitivity;
        float moveX = accel.x * sensitivity;

        Vector3 moveInput = new Vector3(moveX, 0, moveZ);

        // 移動の処理
        if (moveInput.magnitude > 0.1f)
        {
            // 物理的に移動させる
            rb.linearVelocity = new Vector3(moveInput.x * moveSpeed, rb.linearVelocity.y, moveInput.z * moveSpeed);

            // 回転の処理
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSmoothSpeed * Time.deltaTime
            );
        }
        else
        {
            // 傾きが戻ったら止まる
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (!HpManager.Instance.CanTakeDamage())
                return;

            //AudioManager.Instance.PlaySE(1);

            HpManager.Instance.TakeDamage(1);

            Knockback(collision);
        }
    }

    void Knockback(Collision collision)
    {
        isKnockback = true;
        knockbackTimer = knockbackTime;

        Vector3 normal = collision.contacts[0].normal;

        rb.linearVelocity = Vector3.zero;

        rb.AddForce(normal * knockbackPower, ForceMode.Impulse);
    }
}