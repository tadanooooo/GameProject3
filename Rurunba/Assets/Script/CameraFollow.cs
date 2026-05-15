using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("通常時の設定")]
    public Vector3 offset = new Vector3(0, 10.0f, -5.0f);
    public Vector3 fixedRotation = new Vector3(60f, 0f, 0f);
    public float smoothTime = 0.2f;

    [Header("壁接触（90度）時の設定")]
    // 90度になった時の高さを個別に設定
    public float heightWhenTopDown = 15.0f;
    public Vector3 wallRotation = new Vector3(90f, 0f, 0f);

    [Header("壁検知の設定")]
    public LayerMask wallLayer;

    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void LateUpdate()
    {
        if (target == null) return;

        //  本来行きたい理想の位置を計算
        Vector3 desiredPosition = target.position + offset;

        // レイキャストで壁があるかチェック
        RaycastHit hit;
        Vector3 direction = desiredPosition - target.position;
        float distance = direction.magnitude;

        if (Physics.Raycast(target.position, direction.normalized, out hit, distance, wallLayer))
        {
            // 壁あり
            // 後ろに下がれないので、位置を「ターゲットの真上（指定の高さ）」にする
            targetPosition = target.position + Vector3.up * heightWhenTopDown;
            targetRotation = Quaternion.Euler(wallRotation);
        }
        else
        {
            // 壁なし通常通り
            targetPosition = desiredPosition;
            targetRotation = Quaternion.Euler(fixedRotation);
        }

        // 移動と回転を反映（LerpやSmoothDampで滑らかに）
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
    }
}