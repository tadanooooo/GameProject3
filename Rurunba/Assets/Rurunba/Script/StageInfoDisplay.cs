using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 🌟 これを追加することで Image 型が使えるようになります

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

    [Header("全ステージの設定一覧")]
    public StageData[] allStages;

    [Header("表示・非表示を切り替えるパネル本体")]
    public GameObject infoPanel;

    [Header("書き換えるUIテキスト")]
    public TextMeshProUGUI stageTitleText;
    public TextMeshProUGUI targetTimeText;
    public TextMeshProUGUI bestTimeText;

    [Header("書き換える星の画像")]
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
            if (bestTime < 0) bestTimeText.text = "BestTime: None";
            else bestTimeText.text = "BestTime: " + bestTime.ToString("F2") + "s";
        }

        if (star1 != null) star1.gameObject.SetActive(bestStars >= 1);
        if (star2 != null) star2.gameObject.SetActive(bestStars >= 2);
        if (star3 != null) star3.gameObject.SetActive(bestStars >= 3);
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