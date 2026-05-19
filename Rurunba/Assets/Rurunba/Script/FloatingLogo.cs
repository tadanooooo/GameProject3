using UnityEngine;

public class FloatingLogo : MonoBehaviour
{
    [Header("揺れの設定")]
    public float amplitude = 20f; // 揺れる幅（ピクセル）
    public float speed = 2f;      // 揺れるスピード

    private Vector3 startPos;

    void Start()
    {
        // 最初の位置を覚えておく
        startPos = transform.localPosition;
    }

    void Update()
    {
        // サイン波を使って上下のズレを計算
        // Mathf.Sin(時間 * スピード) * 幅
        float newY = startPos.y + Mathf.Sin(Time.time * speed) * amplitude;

        // 計算した位置を適用
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
    }
}