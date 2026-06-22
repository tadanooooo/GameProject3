using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("通常時の設定")]
    public Vector3 offset = new Vector3(0, 10.0f, -5.0f);
    public Vector3 fixedRotation = new Vector3(60f, 0f, 0f);
    public float smoothTime = 0.2f;

    [Header("壁接触（90度）時の設定")]
    public float heightWhenTopDown = 15.0f;
    public Vector3 wallRotation = new Vector3(90f, 0f, 0f);

    [Header("壁検知の設定")]
    public LayerMask wallLayer;

    [Header("進行方向の視界を広げる設定")]
    [Tooltip("プレイヤーの速度に合わせてカメラをどれくらい先回りさせるか")]
    public float shiftStrength = 0.3f;
    [Tooltip("先回りさせる限界の距離")]
    public float maxShiftDistance = 3.0f;

    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    // プレイヤーの速度を計るためのRigidbody
    private Rigidbody targetRb;

    void Start()
    {
        // ターゲットから自動的にRigidbodyを取得しておく
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody>();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 shiftOffset = Vector3.zero;
        if (targetRb != null)
        {
            // プレイヤーの現在の速度（XとZの移動）を取得
            Vector3 playerVelocity = new Vector3(targetRb.linearVelocity.x, 0, targetRb.linearVelocity.z);

            // 速度に強さを掛け算して、先回りする距離を計算
            shiftOffset = playerVelocity * shiftStrength;

            // 先回りしすぎないように限界値を設定
            shiftOffset = Vector3.ClampMagnitude(shiftOffset, maxShiftDistance);
        }

        // 本来カメラが配置されるべき理想の位置（desiredPosition）を計算
        Vector3 desiredPosition = target.position + offset + shiftOffset;

        // プレイヤーからカメラに向かってレイ（光線）を飛ばし、壁レイヤーを検知
        RaycastHit hit;
        Vector3 direction = desiredPosition - target.position;
        float distance = direction.magnitude;

        if (Physics.Raycast(target.position, direction.normalized, out hit, distance, wallLayer))
        {
            // 壁を検知した場合、カメラの位置を自機の真上に移動
            targetPosition = target.position + Vector3.up * heightWhenTopDown; // 真上
            targetRotation = Quaternion.Euler(wallRotation);                   // 角度90度
        }
        else
        {
            // 壁がない場合は、通常通りの斜め見下ろし視点
            targetPosition = desiredPosition;
            targetRotation = Quaternion.Euler(fixedRotation);
        }

        // 移動と回転を反映（LerpやSmoothDampで滑らかに）
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
    }
}