using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;           // 追いかける対象（ロボット）

    [Header("位置の設定")]
    // ここで高さを調整（例: Y=10, Z=-5など）
    public Vector3 offset = new Vector3(0, 10.0f, -5.0f);
    public float smoothTime = 0.2f;

    [Header("角度の設定")]
    // 見下ろし角度（例: X=60 など）
    public Vector3 fixedRotation = new Vector3(90f, 0f, 0f);

    private Vector3 currentVelocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        //  位置の計算
        // 「target.TransformPoint」を使わずに、「ターゲットの位置 + オフセット」にすることで
        // ターゲットの回転に影響されない「絶対的な上空」の位置を作ります
        Vector3 targetPosition = target.position + offset;

        // 滑らかに移動
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        //  角度の固定
        // プレイヤーが回転しても、カメラの角度は常に fixedRotation のまま
        transform.rotation = Quaternion.Euler(fixedRotation);
    }
}