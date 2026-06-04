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

    [Header("星画像（それぞれ1~3枚の星が描かれた単一の画像オブジェクト）")]
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    [Header("スクリプト制御・スタンプ演出の設定")]
    [Tooltip("リザルトパネルが開いてから、星が降ってくるまでの溜め時間（秒）")]
    public float startDelay = 0.5f;
    [Tooltip("出現した瞬間の最初の大きさの倍率（3.5倍なら3.5）")]
    public float startScaleMultiplier = 3.5f;
    [Tooltip("ドカンと元のサイズに縮むまでにかかる時間（秒）")]
    public float shrinkDuration = 0.15f;
    [Tooltip("スタンプが叩きつけられた後、少し弾む（バウンドする）演出を入れるか")]
    public bool useBounceEffect = true;

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
        // 最初は星の画像を非表示にしておく
        if (star1 != null) star1.SetActive(false);
        if (star2 != null) star2.SetActive(false);
        if (star3 != null) star3.SetActive(false);

        // 背景パネルや各種ボタンは、リザルトが開いた瞬間にすべて普通に一斉表示
        if (clearPanel != null) clearPanel.SetActive(true);
        if (retryButton != null) retryButton.SetActive(true);
        if (nextStageButton != null) nextStageButton.SetActive(true);
        if (stageSelectButton != null) stageSelectButton.SetActive(true);

        // 獲得した星の数を計算 (1~3)
        int earnedStars = CalculateStarsCount(savedGoalTime);

        // セーブ処理
        StageSaveManager.SaveStars(stageNumber, earnedStars);
        StageSaveManager.SaveBestTime(stageNumber, savedGoalTime);

        // 純粋にスクリプトだけでスケールを動かす演出コルーチンを開始
        StartCoroutine(ScriptScaleStampSequence(earnedStars));
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

    IEnumerator ScriptScaleStampSequence(int starCount)
    {
        // 指定した溜め時間（startDelay秒）だけ待機
        yield return new WaitForSeconds(startDelay);

        GameObject targetStarImage = null;

        // 条件に合う星画像を1枚だけ選定
        if (starCount == 1) targetStarImage = star1;
        else if (starCount == 2) targetStarImage = star2;
        else if (starCount == 3) targetStarImage = star3;

        if (targetStarImage != null)
        {
            // エディタ側であらかじめ綺麗に配置されているデフォルトのサイズ（Scale）を基準値として保存
            Vector3 defaultScale = targetStarImage.transform.localScale;

            // 最初は指定された「超巨大サイズ」に設定してからアクティブ化（パッと大きく出る）
            Vector3 startScale = defaultScale * startScaleMultiplier;
            targetStarImage.transform.localScale = startScale;
            targetStarImage.SetActive(true);

            float elapsedTime = 0f;

            // 巨大サイズから、勢いよくズドンと元のサイズへ縮小
            while (elapsedTime < shrinkDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / shrinkDuration;

                // スタンプらしさを出すため、最初はゆっくり動きだし、激突直前に一気に加速する計算（EaseInQuad）を使用
                t = t * t;

                targetStarImage.transform.localScale = Vector3.Lerp(startScale, defaultScale, t);
                yield return null;
            }

            // スタンプが叩きつけられた衝撃のバウンド演出
            if (useBounceEffect)
            {
                elapsedTime = 0f;
                float bounceDuration = 0.12f; // バウンドが収まるまでの時間

                // グッと押しつぶされて一度少し小さくなり、その後フワッと元のサイズに戻るゴムのようなクッション
                while (elapsedTime < bounceDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = elapsedTime / bounceDuration;

                    // サイン波を使って、叩きつけられた後に一瞬ギュッと縮んで戻る軌道を作る
                    float bounceCurve = Mathf.Sin(t * Mathf.PI) * 0.25f; // 0.25fがクッションの強さ

                    // 本来のサイズから、一時的に少し縮小させる
                    targetStarImage.transform.localScale = defaultScale - (defaultScale * bounceCurve * (1f - t));
                    yield return null;
                }
            }

            // 最後に、寸分の狂いもなく完全にエディタ上の元のサイズに固定する
            targetStarImage.transform.localScale = defaultScale;
        }
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