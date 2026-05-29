using UnityEngine;
using TMPro; // TextMesh Proを使うために必要

public class TextBlinker : MonoBehaviour
{
    [Header("点滅させる間隔（秒）")]
    public float blinkInterval = 1.0f;

    private TextMeshProUGUI targetText;
    private float timer;

    void Start()
    {
        // 自分がついているオブジェクトからTextMesh Proコンポーネントを自動で取得
        targetText = GetComponent<TextMeshProUGUI>();

        if (targetText == null)
        {
            Debug.LogError("TextBlinker: TextMeshProUGUIコンポーネントが見つかりません。文字オブジェクトに貼り付けてください！");
        }
    }

    void Update()
    {
        if (targetText == null) return;

        // 時間をカウント
        timer += Time.deltaTime;

        // 設定した時間（1秒）が経つたびに実行
        if (timer >= blinkInterval)
        {
            // 文字が表示されていれば非表示に、消えていれば表示にする（反転処理）
            targetText.enabled = !targetText.enabled;

            // タイマーをリセット
            timer = 0f;
        }
    }
}