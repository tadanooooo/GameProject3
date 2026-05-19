using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GoalPoint : MonoBehaviour
{
    // インスペクターでステージ番設定
    [Header("現在のステージ番号")]
    public int stageNumber = 1;

    [Header("UIパネル設定")]
    public GameObject clearPanel;

    [Header("ボタン設定")]
    public GameObject retryButton;
    public GameObject nextStageButton;
    public GameObject stageSelectButton;

    [Header("星の画像（それぞれの単体画像）")]
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    [Header("TimeAttack")]
    public float targetTime = 30.0f;
    private float elapsedTime = 0f;

    private bool isCleared = false;

    [Header("移動先シーン名の設定")]
    [Tooltip("ステージ選択画面のシーン名")]
    public string stageSelectSceneName = "StageSelectScene";
    [Tooltip("次のステージのシーン名")]
    public string nextStageSceneName = "";

    void Update()
    {
        if (!isCleared)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCleared)
        {
            isCleared = true;
            ExecuteGameClear();
        }
    }

    void ExecuteGameClear()
    {
        if (TimeManager.instance != null)
        {
            TimeManager.instance.StopTimer();
        }

        Debug.Log("GameClear! TIME: " + elapsedTime);

        Rigidbody rb = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (clearPanel != null) clearPanel.SetActive(true);
        if (retryButton != null) retryButton.SetActive(true);
        if (nextStageButton != null) nextStageButton.SetActive(true);
        if (stageSelectButton != null) stageSelectButton.SetActive(true);

        // 星の数を計算して画面に表示し、同時にその数を取得する
        int earnedStars = CalculateStars();

        // 【ここがデータ保存の心臓部！】
        // クリアしたステージ番号、獲得した星の数、今回のクリアタイムをセーブする
        StageSaveManager.SaveStars(stageNumber, earnedStars);
        StageSaveManager.SaveBestTime(stageNumber, elapsedTime);
    }

    // 外部に星の数を返せるように、void から int に変更しました
    int CalculateStars()
    {
        float finalTime = elapsedTime; // 今回かかった時間

        if (star1 != null) star1.SetActive(false);
        if (star2 != null) star2.SetActive(false);
        if (star3 != null) star3.SetActive(false);

        int starCount = 1; // 最低でも星1つ

        bool allCollected = false;
        if (GameManager.instance != null && GameManager.instance.IsAllTrashCollected())
        {
            allCollected = true;
            starCount = 2; // ゴミ全回収で星2つ
        }

        if (allCollected && finalTime <= targetTime)
        {
            starCount = 3; // さらに目標タイムクリアで星3つ
        }

        switch (starCount)
        {
            case 1:
                if (star1 != null) star1.SetActive(true);
                break;
            case 2:
                if (star1 != null) star1.SetActive(true);
                if (star2 != null) star2.SetActive(true);
                break;
            case 3:
                if (star1 != null) star1.SetActive(true);
                if (star2 != null) star2.SetActive(true);
                if (star3 != null) star3.SetActive(true);
                break;
        }

        return starCount; // 計算した星の数をクリア処理に引き渡す
    }

    public void ClickRetry()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
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