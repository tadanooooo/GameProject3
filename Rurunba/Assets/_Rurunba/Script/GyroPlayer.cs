using UnityEngine;
using UnityEngine.InputSystem;

public class GyroPlayer : MonoBehaviour
{
    [Header("走行設定")]
    public float moveSpeed = 15f;
    public float sensitivity = 5f;

    [Header("カーペット設定")]
    [Tooltip("カーペットの上に乗ったときの減速倍率（0.5なら速度半分、0.3なら速度30%）")]
    public float carpetSpeedMultiplier = 0.5f;
    private bool isOnCarpet = false;

    [Header("障害物（継続ダメージ）設定")]
    [Tooltip("障害物に触れている時にダメージを与える間隔（秒）。")]
    public float damageInterval = 0.5f;
    [Tooltip("1回あたりに受けるダメージ量（小数対応）")]
    public float obstacleDamage = 0.2f;
    private float damageTimer = 0f;

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
        if (rb != null && rb.isKinematic) return;

        // 継続ダメージのタイマーを進める
        if (damageTimer > 0)
        {
            damageTimer -= Time.deltaTime;
        }

        // 本編(TimeManager) または チュートリアル(TutorialManager) のタイマーが動いていない時は完全に移動を遮断
        bool shouldStop = false;
        if (TimeManager.instance != null && !TimeManager.instance.isTimerRunning) shouldStop = true;
        if (TutorialManager.instance != null && !TutorialManager.instance.isTimerRunning) shouldStop = true;

        if (shouldStop)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
            return; // ジャイロの検知や移動処理をすべてスキップ！
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
            float currentSpeed = isOnCarpet ? (moveSpeed * carpetSpeedMultiplier) : moveSpeed;
            rb.linearVelocity = new Vector3(moveInput.x * currentSpeed, rb.linearVelocity.y, moveInput.z * currentSpeed);

            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSmoothSpeed * Time.deltaTime
            );
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (HpManager.Instance == null) return;
            if (!HpManager.Instance.CanTakeDamage()) return;

            HpManager.Instance.TakeDamage(1f, collision);
            Knockback(collision);

            // チュートリアルマネージャーへダメージ（壁衝突）を通知
            if (TutorialManager.instance != null)
            {
                TutorialManager.instance.NotifyDamage();
            }
        }
        else if (collision.gameObject.CompareTag("Carpet"))
        {
            isOnCarpet = true;
            Debug.Log("カーペットに乗ったため減速します");
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (HpManager.Instance == null) return;
            if (HpManager.Instance.IsDead()) return;

            if (damageTimer <= 0f)
            {
                HpManager.Instance.TakeDamage(obstacleDamage, collision, 0f);
                damageTimer = damageInterval;
                Debug.Log("障害物に接触中：継続ダメージを与えました");

                // チュートリアル中にObstacleタグの障害物でダメージを受けた場合もカウントを進める
                if (TutorialManager.instance != null)
                {
                    TutorialManager.instance.NotifyDamage();
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Carpet"))
        {
            isOnCarpet = false;
            Debug.Log("カーペットから降りたため通常速度に戻ります");
        }
    }

    void Knockback(Collision collision)
    {
        if (collision.contacts.Length == 0) return;

        isKnockback = true;
        knockbackTimer = knockbackTime;

        Vector3 normal = collision.contacts[0].normal;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(normal * knockbackPower, ForceMode.Impulse);
        }
    }
}