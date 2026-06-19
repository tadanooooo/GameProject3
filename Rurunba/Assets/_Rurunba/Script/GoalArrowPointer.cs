using UnityEngine;

public class GoalArrowPointer : MonoBehaviour
{
    [Header("操作するUIの設定")]
    [Tooltip("MainUIの中に入れた、矢印の画像（RectTransform）をドラッグしてください")]
    public RectTransform arrowUI;

    [Header("ターゲット設定")]
    [Tooltip("ステージにあるゴールのTransform")]
    public Transform goalTarget;

    [Tooltip("プレイヤーのTransform")]
    public Transform playerTransform;

    [Header("カメラ設定")]
    [Tooltip("メインカメラ（Main Camera）をドラッグしてください")]
    public Camera mainCamera;

    void Start()
    {
        // カメラが未設定なら自動で取得
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        if (arrowUI == null || goalTarget == null || playerTransform == null || mainCamera == null) return;

        // 3D空間上での、プレイヤーから見たゴールの方向（ベクトル）を計算
        Vector3 direction3D = goalTarget.position - playerTransform.position;

        // カメラの向きを基準にして、画面内（2D）での方向に変換する
        // カメラから見た右と前の方向を取得
        Vector3 cameraRight = mainCamera.transform.right;
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0; // 水平な回転にするために垂直成分をカット
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // プレイヤーからゴールへの方向を、カメラ基準のX（横）とZ（縦）に分解
        float forwardDot = Vector3.Dot(direction3D, cameraForward);
        float rightDot = Vector3.Dot(direction3D, cameraRight);

        // 画面内での角度（ラジアン→角度）を計算
        // UnityのUIは上が基準（0度）になることが多いので、それに合わせて計算
        float angle = Mathf.Atan2(rightDot, forwardDot) * Mathf.Rad2Deg;

        // UIの矢印画像を、計算した角度だけ回転させる
        // Z軸を中心に回転させることで、画面内で時計の針のように回ります
        arrowUI.localRotation = Quaternion.Euler(0, 0, -angle);
    }
}