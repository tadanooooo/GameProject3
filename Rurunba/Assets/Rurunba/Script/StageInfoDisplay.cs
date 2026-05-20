using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageInfoDisplay : MonoBehaviour
{
    public static StageInfoDisplay instance;

    [System.Serializable]
    public struct StageData
    {
        public int stageNumber;
        public float targetTime;
        public string sceneName;
    }

    [Header("全ステージ設定一覧")]
    public StageData[] allStages;

    [Header("切り替えるパネル")]
    public GameObject infoPanel;

    [Header("UIテキスト")]
    public TextMeshProUGUI stageTitleText;
    public TextMeshProUGUI targetTimeText;
    public TextMeshProUGUI bestTimeText;

    [Header("星画像")]
    public Image star1;
    public Image star2;
    public Image star3;

    private string currentSelectedSceneName;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        if (infoPanel != null) infoPanel.SetActive(false);
    }

    public void DisplayStageInfo(int stageNumber)
    {
        if (infoPanel != null) infoPanel.SetActive(true);

        if (stageTitleText != null) stageTitleText.text = "STAGE " + stageNumber;

        float targetTime = 0f;
        currentSelectedSceneName = "";

        foreach (var stage in allStages)
        {
            if (stage.stageNumber == stageNumber)
            {
                targetTime = stage.targetTime;
                currentSelectedSceneName = stage.sceneName;
                break;
            }
        }
        if (targetTimeText != null) targetTimeText.text = "TargetTime: " + targetTime.ToString("F2") + "s";

        int bestStars = StageSaveManager.LoadStars(stageNumber);
        float bestTime = StageSaveManager.LoadBestTime(stageNumber);

        if (bestTimeText != null)
        {
            if (bestTime >= 9999f)
            {
                bestTimeText.text = "BestTime: None";
            }
            else
            {
                bestTimeText.text = "BestTime: " + bestTime.ToString("F2") + "s";
            }
        }

        // 星の数に応じて表示する画像を切り替え
        switch (bestStars)
        {
            case 0: // まだクリアしていない時
                if (star1 != null) star1.gameObject.SetActive(false);
                if (star2 != null) star2.gameObject.SetActive(false);
                if (star3 != null) star3.gameObject.SetActive(false);
                break;
            case 1: // 星1つの画像だけを表示
                if (star1 != null) star1.gameObject.SetActive(true);
                if (star2 != null) star2.gameObject.SetActive(false);
                if (star3 != null) star3.gameObject.SetActive(false);
                break;
            case 2: // 星2つの画像だけを表示
                if (star1 != null) star1.gameObject.SetActive(false);
                if (star2 != null) star2.gameObject.SetActive(true);
                if (star3 != null) star3.gameObject.SetActive(false);
                break;
            case 3: // 星3つの画像だけを表示
                if (star1 != null) star1.gameObject.SetActive(false);
                if (star2 != null) star2.gameObject.SetActive(false);
                if (star3 != null) star3.gameObject.SetActive(true);
                break;
        }
    }

    public void ClickStartStage()
    {
        if (!string.IsNullOrEmpty(currentSelectedSceneName))
        {
            SceneManager.LoadScene(currentSelectedSceneName);
        }
        else
        {
            Debug.LogError("シーン名が設定されていません");
        }
    }

    public void ClickClosePanel()
    {
        if (infoPanel != null) infoPanel.SetActive(false);
    }
}