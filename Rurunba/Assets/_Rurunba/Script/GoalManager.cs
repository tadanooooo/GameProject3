using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GoalManager : MonoBehaviour
{
    public static GoalManager instance;

    [Header("現在のステージ番号")]
    public int stageNumber = 1;

    [Header("UIパネル設定")]
    public GameObject goalTextObject;
    public GameObject clearPanel;

    [Header("ボタン設定")]
    public GameObject retryButton;
    public GameObject nextStageButton;
    public GameObject stageSelectButton;

    [Header("星画像")]
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    [Header("TimeAttack")]
    public float targetTime = 30.0f;

    [Header("移動先シーン名")]
    public string stageSelectSceneName = "1_StageSelectScene";

    public string nextStageSceneName = "";

    private bool isCleared = false;

    private float savedGoalTime = 0f;

    private void Awake()
    {
        instance = this;
    }

    public void StartGoal()
    {
        if (isCleared) return;

        isCleared = true;

        if (TimeManager.instance != null)
        {
            savedGoalTime = TimeManager.instance.GetTime();
        }

        StartCoroutine(GoalSequence());
    }

    IEnumerator GoalSequence()
    {
        if (TimeManager.instance != null)
        {
            TimeManager.instance.StopTimer();
        }

        Debug.Log("ゴールタイム : " + savedGoalTime);

        Rigidbody rb = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (goalTextObject != null)
        {
            goalTextObject.SetActive(true);
        }

        yield return new WaitForSeconds(3.0f);

        if (goalTextObject != null)
        {
            goalTextObject.SetActive(false);
        }

        ExecuteGameClear();
    }

    void ExecuteGameClear()
    {
        if (clearPanel != null) clearPanel.SetActive(true);

        if (retryButton != null) retryButton.SetActive(true);

        if (nextStageButton != null) nextStageButton.SetActive(true);

        if (stageSelectButton != null) stageSelectButton.SetActive(true);

        int earnedStars = CalculateStars(savedGoalTime);

        StageSaveManager.SaveStars(stageNumber, earnedStars);

        StageSaveManager.SaveBestTime(stageNumber, savedGoalTime);
    }

    int CalculateStars(float finalTime)
    {
        if (star1 != null) star1.SetActive(false);

        if (star2 != null) star2.SetActive(false);

        if (star3 != null) star3.SetActive(false);

        int starCount = 1;

        bool allCollected = false;

        if (GameManager.instance != null &&
            GameManager.instance.IsAllTrashCollected())
        {
            allCollected = true;
            starCount = 2;
        }

        if (allCollected && finalTime <= targetTime)
        {
            starCount = 3;
        }

        switch (starCount)
        {
            case 1:
                if (star1 != null) star1.SetActive(true);
                break;

            case 2:
                if (star2 != null) star2.SetActive(true);
                break;

            case 3:
                if (star3 != null) star3.SetActive(true);
                break;
        }

        return starCount;
    }

    public void ClickRetry()
    {
        string currentSceneName =
            SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(currentSceneName);
    }

    public void ClickBackToSelect()
    {
        if (!string.IsNullOrEmpty(stageSelectSceneName))
        {
            SceneManager.LoadScene(stageSelectSceneName);
        }
    }

    public void ClickGoToNext()
    {
        if (!string.IsNullOrEmpty(nextStageSceneName))
        {
            SceneManager.LoadScene(nextStageSceneName);
        }
    }
}