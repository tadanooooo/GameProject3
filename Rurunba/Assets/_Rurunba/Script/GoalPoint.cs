using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GoalPoint : MonoBehaviour
{
    [Header("現在のステージ番号")]
    public int stageNumber = 1;

    [Header("UIパネル設定")]
    public Image goalImage;
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

    private bool isCleared = false;
    private float savedGoalTime = 0f; // 固定するゴールタイム

    [Header("移動先シーン名")]
    [Tooltip("ステージ選択画面シーン名")]
    public string stageSelectSceneName = "1_StageSelectScene";
    [Tooltip("次のステージシーン名")]
    public string nextStageSceneName = "";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCleared)
        {
            isCleared = true;

            if (TimeManager.instance != null)
            {
                savedGoalTime = TimeManager.instance.GetTime();
            }
            else
            {
                // 万が一TimeManagerが見つからなかった時の保険
                savedGoalTime = 0f;
            }

            // 直接パネルを開かず、時間差演出のコルーチンをスタート
            StartCoroutine(GoalSequence());
        }
    }

    // ゴール時処理
    IEnumerator GoalSequence()
    {
        // 画面のタイマーのカウントを止める
        if (TimeManager.instance != null)
        {
            TimeManager.instance.StopTimer();
        }

        // ログで画面から取得したタイムが本当に一致しているか確認
        Debug.Log("画面から取得した確定ゴールタイム: " + savedGoalTime);

        // プレイヤーの動きを完全に止め、物理演算を無効
        Rigidbody rb = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero; // 回転も止める
            rb.isKinematic = true;
        }

        // GOALの文字
        if (goalImage != null) goalImage.gameObject.SetActive(true);

        // そのまま3.0秒待つ
        yield return new WaitForSeconds(3.0f);

        // GOAL文字消す
        if (goalImage != null) goalImage.gameObject.SetActive(false);

        // クリアリザルト画面
        ExecuteGameClear();
    }

    void ExecuteGameClear()
    {
        // パネルやボタンを表示
        if (clearPanel != null) clearPanel.SetActive(true);
        if (retryButton != null) retryButton.SetActive(true);
        if (nextStageButton != null) nextStageButton.SetActive(true);
        if (stageSelectButton != null) stageSelectButton.SetActive(true);

        // タイムで星を計算
        int earnedStars = CalculateStars(savedGoalTime);

        // セーブ処理はすべて StageSaveManager に一任します。
        // これにより自動で最新のトータル計算（_Stars）まで連動してセーブされます。
        StageSaveManager.SaveStars(stageNumber, earnedStars);
        StageSaveManager.SaveBestTime(stageNumber, savedGoalTime);

        Debug.Log($"【ゴールセーブ成功】ステージ {stageNumber} の星の数: {earnedStars} を保存し、トータル値を更新しました。");
    }

    // 引数で固定タイムを受け取るように変更
    int CalculateStars(float finalTime)
    {
        if (star1 != null) star1.SetActive(false);
        if (star2 != null) star2.SetActive(false);
        if (star3 != null) star3.SetActive(false);

        int starCount = 1;

        bool allCollected = false;
        if (GameManager.instance != null && GameManager.instance.IsAllTrashCollected())
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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void ClickBackToSelect()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }
        if (!string.IsNullOrEmpty(stageSelectSceneName))
        {
            SceneManager.LoadScene(stageSelectSceneName);
        }
    }

    public void ClickGoToNext()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }
        if (!string.IsNullOrEmpty(nextStageSceneName))
        {
            SceneManager.LoadScene(nextStageSceneName);
        }
    }
}