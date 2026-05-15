using UnityEngine;
using UnityEngine.InputSystem;

public class GyroPlayer : MonoBehaviour
{
    [Header("走行設定")]
    public float moveSpeed = 15f;
    public float sensitivity = 5f;

    [Header("回転の速さ")]
    // この値を大きくする（15~20）とクイックに、小さく（2~5）するとゆっくり向きが変わります
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

        // 前回の調整に基づいた軸設定（適宜、前回の成功パターンに合わせて微調整してください）
        float moveZ = accel.y * sensitivity;
        float moveX = accel.x * sensitivity;

        Vector3 moveInput = new Vector3(moveX, 0, moveZ);

        // 移動の処理
        // 傾きが一定以上あるときだけ動かす
        if (moveInput.magnitude > 0.1f)
        {
            // 物理的に移動させる
            rb.linearVelocity = new Vector3(moveInput.x * moveSpeed, rb.linearVelocity.y, moveInput.z * moveSpeed);

            // 回転の処理（ここが「ゲームっぽさ」のキモ）
            // 「今移動している方向」を向くための計算
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);

            // 現在の向きから目標の向きへ、滑らかに回転させる
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

    void OnCollisionStay (Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // 無敵中なら何もしない
            if (!HpManager.Instance.CanTakeDamage())
                return;

            HpManager.Instance.TakeDamage(1);

            Knockback(collision);
        }
    }

    void Knockback(Collision collision)
    {
        isKnockback = true;
        knockbackTimer = knockbackTime;

        // 接触面の法線方向
        Vector3 normal = collision.contacts[0].normal;

        // 一旦停止
        rb.linearVelocity = Vector3.zero;

        // 壁から垂直に吹き飛ばす
        rb.AddForce(normal * knockbackPower, ForceMode.Impulse);
    }
}