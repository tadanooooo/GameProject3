using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMesh Proを使うための宣言

public class StageLockManager : MonoBehaviour
{
    [Header("この画面での表示設定")]
    [Tooltip("この画面のUIに表示したい目標の星の数（セレクト1画面なら20、セレクト2画面なら40）")]
    public int displayMaxGoalStars = 20;

    [Header("UI表示用テキスト（1〜30通算の星の合計をここに表示します）")]
    [Tooltip("エリア画面にある、通算の星の合計を表示したいTMPをアタッチしてください")]
    public TextMeshProUGUI globalTotalStarsText;

    [Header("エリア2（11~20）の解放設定")]
    [Tooltip("エリア2の解放に必要な、ゲーム全体の合計星数")]
    public int requiredStarsForArea2 = 20;
    [Tooltip("エリア2へ進む右矢印ボタン")]
    public Button nextAreaArrowButton2;
    [Tooltip("エリア2解放条件のUI表示用（例: × 12/20）※不要なら空でOK")]
    public TextMeshProUGUI conditionTextArea2;

    [Header("エリア3（21~30）の解放設定")]
    [Tooltip("エリア3の解放に必要な、ゲーム全体の合計星数")]
    public int requiredStarsForArea3 = 40;
    [Tooltip("エリア3へ進む右矢印ボタン")]
    public Button nextAreaArrowButton3;
    [Tooltip("エリア3解放条件のUI表示用（例: × 25/40）※不要なら空でOK")]
    public TextMeshProUGUI conditionTextArea3;

    void Start()
    {
        // ステージセレクトに入った瞬間に、念のため最新のトータル星数を裏で集計し直す
        StageSaveManager.UpdateAndSaveTotalStars();

        // ゲーム全体（1〜30ステージ）のすべての星の合計数を取得する
        int allTotalStars = CalculateStars(1, 30);

        // インスペクターで設定した画面ごとの目標数（20や40）が分母
        if (globalTotalStarsText != null)
        {
            globalTotalStarsText.text = $" {allTotalStars} / {displayMaxGoalStars}";
        }

        // --- エリア2の解放・表示処理 ---
        if (conditionTextArea2 != null)
        {
            conditionTextArea2.text = $"× {allTotalStars}/{requiredStarsForArea2}";
        }

        if (nextAreaArrowButton2 != null)
        {
            nextAreaArrowButton2.interactable = (allTotalStars >= requiredStarsForArea2);
        }

        // --- エリア3の解放・表示処理 ---
        if (conditionTextArea3 != null)
        {
            conditionTextArea3.text = $"× {allTotalStars}/{requiredStarsForArea3}";
        }

        if (nextAreaArrowButton3 != null)
        {
            nextAreaArrowButton3.interactable = (allTotalStars >= requiredStarsForArea3);
        }
    }

    // 指定した範囲のステージの最高星数をすべて足し算する汎用関数
    int CalculateStars(int startStage, int endStage)
    {
        int sum = 0;
        for (int i = startStage; i <= endStage; i++)
        {
            int stageStars = PlayerPrefs.GetInt($"Stage_{i}_Stars", 0);
            sum += stageStars;
        }
        return sum;
    }

    // どちらのボタンのロック音にも対応できるように引数を追加
    public void PlayLockSE(Button clickedButton)
    {
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
        for (int i = 1; i <= 30; i++)
        {
            PlayerPrefs.DeleteKey($"Stage_{i}_Stars");
        }
        PlayerPrefs.DeleteKey("Total_Stars");
        PlayerPrefs.Save();
        Debug.Log("すべてのステージの星データをリセットしました");
    }
}