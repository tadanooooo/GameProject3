using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

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

        if (stageTitleText != null) stageTitleText.text = stageNumber.ToString();

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
        if (targetTimeText != null) targetTimeText.text = targetTime.ToString("F2") + "s";

        int bestStars = StageSaveManager.LoadStars(stageNumber);
        float bestTime = StageSaveManager.LoadBestTime(stageNumber);

        if (bestTimeText != null)
        {
            if (bestTime >= 9999f)
            {
                bestTimeText.text = "データなし";
            }
            else
            {
                bestTimeText.text = bestTime.ToString("F2") + "s";
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
        // 押された瞬間にまずSEを鳴らす
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }

        // 0.5秒待つコルーチンをスタート
        StartCoroutine(StartStageSequence());
    }
    private IEnumerator StartStageSequence()
    {
        // ここで0.5秒（0.5f）待機する
        yield return new WaitForSeconds(0.5f);

        if (!string.IsNullOrEmpty(currentSelectedSceneName))
        {
            // 待ったあとにシーンを読み込む
            SceneManager.LoadScene(currentSelectedSceneName);
        }
        else
        {
            Debug.LogError("シーン名が設定されていません");
        }
    }

    public void ClickClosePanel()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }
        if (infoPanel != null) infoPanel.SetActive(false);
    }
}