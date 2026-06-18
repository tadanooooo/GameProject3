using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI; // ボタンのコンポーネント（Button）を制御するために追加

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

    // 星の数による次のステージ制限設定
    [Header("次ステージの解放制限設定")]
    [Tooltip("制限をかけたい現在のステージ番号（例: 10ステージクリア時に判定なら 10）")]
    public int lockBorderStageNumber = 10;
    [Tooltip("進むために必要な累計の獲得星数")]
    public int requiredTotalStars = 15;
    [Tooltip("合計何ステージ分の星をチェックするか（例: 10ステージ分なら 10）")]
    public int totalStagesToCheck = 10;

    [Header("星画像（それぞれ1〜3枚の星が描かれた単一の画像オブジェクト）")]
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    [Header("スコア・ベスト更新UI設定")]
    [Tooltip("今回のリザルトタイムを表示するTextMeshProテキストオブジェクト")]
    public TextMeshProUGUI scoreTimeText;
    [Tooltip("ベストタイムを更新した時に表示するベスト更新！のオブジェクト（テキストや画像など）")]
    public GameObject newRecordObject;

    [Header("スクリプト制御・スタンプ演出の設定")]
    [Tooltip("リザルトパネルが開いてから、星が降ってくるまでの溜め時間（秒）")]
    public float startDelay = 0.5f;
    [Tooltip("出現した瞬間の最初の大きさの倍率（3.5倍なら3.5）")]
    public float startScaleMultiplier = 3.5f;
    [Tooltip("ドカンと元のサイズに縮むまでにかかる時間（秒）")]
    public float shrinkDuration = 0.15f;
    [Tooltip("スタンプが叩きつけられた後、少し弾む（バウンドする）演出を入れるか")]
    public bool useBounceEffect = true;
    [Tooltip("星が押されてから、スコアが降ってくるまでの時間差（秒）")]
    public float nextStampDelay = 0.4f;

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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(2);
        }
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
        if (star1 != null) star1.SetActive(false);
        if (star2 != null) star2.SetActive(false);
        if (star3 != null) star3.SetActive(false);
        if (scoreTimeText != null) scoreTimeText.gameObject.SetActive(false);
        if (newRecordObject != null) newRecordObject.SetActive(false);

        if (clearPanel != null) clearPanel.SetActive(true);
        if (retryButton != null) retryButton.SetActive(true);
        if (nextStageButton != null) nextStageButton.SetActive(true);
        if (stageSelectButton != null) stageSelectButton.SetActive(true);

        int earnedStars = CalculateStarsCount(savedGoalTime);

        // セーブする前に過去のベストタイムを取得して判定する
        float previousBestTime = StageSaveManager.LoadBestTime(stageNumber);

        // 初回プレイ（過去の記録が0秒、または今回のタイムが過去のベストより速い）なら新記録！
        bool isNewRecord = (previousBestTime <= 0f || savedGoalTime < previousBestTime);

        // 判定が終わったらセーブ実行
        StageSaveManager.SaveStars(stageNumber, earnedStars);
        StageSaveManager.SaveBestTime(stageNumber, savedGoalTime);

        if (scoreTimeText != null)
        {
            scoreTimeText.text = "タイム: " + savedGoalTime.ToString("F2") + "s";
        }

        StartCoroutine(ResultStampSequence(earnedStars, isNewRecord));
    }

    // すべてのステージから星の合計を計算し、ボタンの有効・無効を切り替える
    void CheckNextStageLock()
    {
        if (nextStageButton == null) return;

        // もし制限をかけたい特定のステージ（例:10）だったら星のチェックを行う
        if (stageNumber == lockBorderStageNumber)
        {
            int totalStars = 0;
            // 1ステージ〜指定数までの星を全て足し算する
            for (int i = 1; i <= totalStagesToCheck; i++)
            {
                totalStars += StageSaveManager.LoadStars(i);
            }

            Debug.Log("これまでの累計獲得星数: " + totalStars + " / 必要数: " + requiredTotalStars);

            // 必要数に届いていない場合、ボタンを押せなくする
            if (totalStars < requiredTotalStars)
            {
                Button btn = nextStageButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = false; // ボタンをクリック不可能にする（見た目も自動で半透明になります）
                }
                Debug.LogWarning("星が足りないため、次のステージボタンをロックしました。");
            }
        }
    }

    int CalculateStarsCount(float finalTime)
    {
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

        return starCount;
    }

    IEnumerator ResultStampSequence(int starCount, bool isNewRecord)
    {
        yield return new WaitForSeconds(startDelay);

        GameObject targetStarImage = null;
        if (starCount == 1) targetStarImage = star1;
        else if (starCount == 2) targetStarImage = star2;
        else if (starCount == 3) targetStarImage = star3;

        if (targetStarImage != null)
        {
            yield return StartCoroutine(AnimateStampObject(targetStarImage));
        }

        yield return new WaitForSeconds(nextStampDelay);

        if (scoreTimeText != null)
        {
            yield return StartCoroutine(AnimateStampObject(scoreTimeText.gameObject));
        }

        if (isNewRecord && newRecordObject != null)
        {
            yield return new WaitForSeconds(0.25f);

            newRecordObject.SetActive(true);

            Vector3 originalBestScale = newRecordObject.transform.localScale;
            newRecordObject.transform.localScale = originalBestScale * 0.5f;

            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.15f;
                newRecordObject.transform.localScale = Vector3.Lerp(originalBestScale * 0.5f, originalBestScale * 1.1f, t);
                yield return null;
            }
            newRecordObject.transform.localScale = originalBestScale;
        }
    }

    IEnumerator AnimateStampObject(GameObject obj)
    {
        Vector3 defaultScale = obj.transform.localScale;
        Vector3 startScale = defaultScale * startScaleMultiplier;
        obj.transform.localScale = startScale;
        obj.SetActive(true);

        float elapsedTime = 0f;

        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / shrinkDuration;
            t = t * t;
            obj.transform.transform.localScale = Vector3.Lerp(startScale, defaultScale, t);
            yield return null;
        }

        if (useBounceEffect)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(5);
            }
            elapsedTime = 0f;
            float bounceDuration = 0.12f;

            while (elapsedTime < bounceDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / bounceDuration;
                float bounceCurve = Mathf.Sin(t * Mathf.PI) * 0.25f;
                obj.transform.localScale = defaultScale - (defaultScale * bounceCurve * (1f - t));
                yield return null;
            }
        }

        obj.transform.localScale = defaultScale;
    }

    public void ClickRetry()
    {
        StartCoroutine(RetrySequence());
    }
    private IEnumerator RetrySequence()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }

        yield return new WaitForSeconds(0.5f);

        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void ClickBackToSelect()
    {
        StartCoroutine(BackToSelectSequence());
    }

    private IEnumerator BackToSelectSequence()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }

        yield return new WaitForSeconds(0.5f);

        if (!string.IsNullOrEmpty(stageSelectSceneName))
        {
            SceneManager.LoadScene(stageSelectSceneName);
        }
    }

    public void ClickGoToNext()
    {
        StartCoroutine(GoToNextSequence());
    }

    private IEnumerator GoToNextSequence()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }

        yield return new WaitForSeconds(0.5f);

        if (!string.IsNullOrEmpty(nextStageSceneName))
        {
            SceneManager.LoadScene(nextStageSceneName);
        }
    }
}