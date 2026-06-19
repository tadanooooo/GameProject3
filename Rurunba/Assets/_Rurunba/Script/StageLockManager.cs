using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMesh Proを使うための宣言

public class StageLockManager : MonoBehaviour
{
    [Header("エリア2の解放設定")]
    [Tooltip("エリア2（11~20）の解放に必要なエリア1の星の数")]
    public int requiredStarsForArea2 = 20;
    [Tooltip("エリア2へ進む右矢印ボタン")]
    public Button nextAreaArrowButton2;
    [Tooltip("UI表示用（TextMesh Pro）例: × 12/15")]
    public TextMeshProUGUI totalStarsText2;

    [Header("エリア3の解放設定")]
    [Tooltip("エリア3（21~30）の解放に必要なエリア2の星の数")]
    public int requiredStarsForArea3 = 40;
    [Tooltip("エリア3へ進む右矢印ボタン")]
    public Button nextAreaArrowButton3;
    [Tooltip("UI表示用（TextMesh Pro）例: × 5/15")]
    public TextMeshProUGUI totalStarsText3;

    void Start()
    {
        // ステージセレクトに入った瞬間に、念のため最新のトータル星数を裏で集計し直す
        StageSaveManager.UpdateAndSaveTotalStars();

        // エリア2の解放処理
        int totalStarsArea1 = CalculateStars(1, 10); // 1〜10ステージの星を合計

        if (totalStarsText2 != null)
        {
            totalStarsText2.text = $"× {totalStarsArea1}/{requiredStarsForArea2}";
        }

        if (nextAreaArrowButton2 != null)
        {
            nextAreaArrowButton2.interactable = (totalStarsArea1 >= requiredStarsForArea2);
        }

        // エリア3の解放処理
        int totalStarsArea2 = CalculateStars(11, 20); // 11〜20ステージの星を合計

        if (totalStarsText3 != null)
        {
            totalStarsText3.text = $"× {totalStarsArea2}/{requiredStarsForArea3}";
        }

        if (nextAreaArrowButton3 != null)
        {
            nextAreaArrowButton3.interactable = (totalStarsArea2 >= requiredStarsForArea3);
        }
    }

    // 指定した範囲のステージの最高星数をすべて足し算する汎用関数
    int CalculateStars(int startStage, int endStage)
    {
        int sum = 0;
        for (int i = startStage; i <= endStage; i++)
        {
            // キーの名前を StageSaveManager の保存名 "_Stars" に統一
            int stageStars = PlayerPrefs.GetInt($"Stage_{i}_Stars", 0);
            sum += stageStars;
        }
        return sum;
    }

    // どちらのボタンのロック音にも対応できるように引数を追加
    public void PlayLockSE(Button clickedButton)
    {
        // クリックされたボタンが押せない状態（interactableがfalse）のときだけ実行
        if (clickedButton != null && !clickedButton.interactable)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(1); // 警告SE
            }
            Debug.Log($"{clickedButton.gameObject.name} はロック中なので警告音を鳴らしました");
        }
    }

    // 星のデータをリセットしてテストしたい時にUnity上で右クリックして実行できる便利機能
    [ContextMenu("Reset All Stars Data")]
    public void ResetAllStarsData()
    {
        for (int i = 1; i <= 30; i++) // 30ステージ分に拡張
        {
            // リセット対象のキーも "_Stars" に統一
            PlayerPrefs.DeleteKey($"Stage_{i}_Stars");
        }
        PlayerPrefs.DeleteKey("Total_Stars"); // トータル星数のデータもリセット
        PlayerPrefs.Save();
        Debug.Log("すべてのステージの星データをリセットしました");
    }
}