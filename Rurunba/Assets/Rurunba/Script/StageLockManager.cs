using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMesh Proを使うための宣言

public class StageLockManager : MonoBehaviour
{
    [Header("エリア2（11~20）の解放に必要な星の数")]
    public int requiredStarsForArea2 = 15;

    [Header("次のエリア（ページ）へ進む右矢印ボタン")]
    public Button nextAreaArrowButton;

    [Header("UI表示用（TextMesh Pro）")]
    public TextMeshProUGUI totalStarsText;

    void Start()
    {
        // 1~10ステージの星の合計を計算
        int totalStarsArea1 = CalculateArea1Stars();

        // UIに合計数を表示（現在の星: ○ / 15）
        if (totalStarsText != null)
        {
            totalStarsText.text = $"× {totalStarsArea1}/{requiredStarsForArea2}";
        }

        // 星の数が足りているかチェック
        bool isArea2Unlocked = totalStarsArea1 >= requiredStarsForArea2;

        // 右矢印ボタンの「有効/無効」を切り替える
        if (nextAreaArrowButton != null)
        {
            nextAreaArrowButton.interactable = isArea2Unlocked; // 足りていれば押せる、足りなければグレーアウト
        }
    }

    // 1~10ステージの最高星数をすべて足し算する関数
    int CalculateArea1Stars()
    {
        int sum = 0;

        for (int i = 1; i <= 10; i++)
        {
            // 各ステージの最高星数をセーブデータから読み込む（データがなければ0）
            int stageStars = PlayerPrefs.GetInt($"Stage_{i}_MaxStars", 0);
            sum += stageStars;
        }

        return sum;
    }

    // 星のデータをリセットしてテストしたい時にUnity上で右クリックして実行できる便利機能
    [ContextMenu("Reset All Stars Data")]
    public void ResetAllStarsData()
    {
        for (int i = 1; i <= 20; i++)
        {
            PlayerPrefs.DeleteKey($"Stage_{i}_MaxStars");
        }
        Debug.Log("すべてのステージの星データをリセットしました");
    }
    public void PlayLockSE()
    {
        // ボタンが押せない状態（interactableがfalse）のときだけ実行
        if (nextAreaArrowButton != null && !nextAreaArrowButton.interactable)
        {
            // 押せない時の警告SEを鳴らす
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(1);
            }
            Debug.Log("ロック中なので警告音を鳴らしました");
        }
    }
}