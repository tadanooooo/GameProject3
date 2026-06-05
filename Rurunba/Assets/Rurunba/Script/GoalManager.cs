using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // TextMeshProを使用するために追加

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

    [Header("星画像（それぞれ1〜3枚の星が描かれた単一の画像オブジェクト）")]
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    [Header("【新規】スコア・ベスト更新UI設定")]
    [Tooltip("今回のリザルトタイムを表示するTextMeshProテキストオブジェクト")]
    public TextMeshProUGUI scoreTimeText;
    [Tooltip("ベストタイムを更新した時に表示する『ベスト更新！』のオブジェクト（テキストや画像など）")]
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
        // 最初は演出パーツ（星・スコア・ベスト文字）をすべて非表示にしておく
        if (star1 != null) star1.SetActive(false);
        if (star2 != null) star2.SetActive(false);
        if (star3 != null) star3.SetActive(false);
        if (scoreTimeText != null) scoreTimeText.gameObject.SetActive(false);
        if (newRecordObject != null) newRecordObject.SetActive(false);

        // 背景パネルや各種ボタンは、リザルトが開いた瞬間にすべて普通に一斉表示
        if (clearPanel != null) clearPanel.SetActive(true);
        if (retryButton != null) retryButton.SetActive(true);
        if (nextStageButton != null) nextStageButton.SetActive(true);
        if (stageSelectButton != null) stageSelectButton.SetActive(true);

        // 獲得した星の数を計算 (1~3)
        int earnedStars = CalculateStarsCount(savedGoalTime);

        // ロードして、今回のタイムがベスト更新（ハイスコア）か判定する
        float previousBestTime = StageSaveManager.LoadBestTime(stageNumber);
        bool isNewRecord = (savedGoalTime < previousBestTime);

        // セーブ処理
        StageSaveManager.SaveStars(stageNumber, earnedStars);
        StageSaveManager.SaveBestTime(stageNumber, savedGoalTime);

        // 今回のスコアテキストにタイムを代入しておく（非表示状態のまま）
        if (scoreTimeText != null)
        {
            scoreTimeText.text = "タイム: " + savedGoalTime.ToString("F2") + "s";
        }

        // 連動したスタンプリザルトシーケンスを開始
        StartCoroutine(ResultStampSequence(earnedStars, isNewRecord));
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

    // 星スタンプ → スコアスタンプ → ベスト更新を時間差で連続処理する決定版コルーチン
    IEnumerator ResultStampSequence(int starCount, bool isNewRecord)
    {
        //---------------------------------------------------------
        // 第1のスタンプ：星の画像
        //---------------------------------------------------------
        yield return new WaitForSeconds(startDelay);

        GameObject targetStarImage = null;
        if (starCount == 1) targetStarImage = star1;
        else if (starCount == 2) targetStarImage = star2;
        else if (starCount == 3) targetStarImage = star3;

        if (targetStarImage != null)
        {
            // 星のスタンプアニメーションを実行
            yield return StartCoroutine(AnimateStampObject(targetStarImage));
        }

        // 今回のスコア（タイム）
        // 星が押し込まれてから、次のスコアが降ってくるまでの時間差
        yield return new WaitForSeconds(nextStampDelay);

        if (scoreTimeText != null)
        {
            // スコアテキストのスタンプアニメーションを実行
            yield return StartCoroutine(AnimateStampObject(scoreTimeText.gameObject));
        }

        // ベスト更新（ニューレコード）の表示
        if (isNewRecord && newRecordObject != null)
        {
            // スコアがドンッと押し込まれた直後、ワンテンポ置いてからベスト文字をパッと出す
            yield return new WaitForSeconds(0.25f);

            newRecordObject.SetActive(true);

            // ベスト更新文字処理
            Vector3 originalBestScale = newRecordObject.transform.localScale;
            newRecordObject.transform.localScale = originalBestScale * 0.5f;

            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.15f;
                // イージングを使って滑らかに大きくする
                newRecordObject.transform.localScale = Vector3.Lerp(originalBestScale * 0.5f, originalBestScale * 1.1f, t);
                yield return null;
            }
            newRecordObject.transform.localScale = originalBestScale;
        }
    }

    // 大きく表示 → 滑らかに縮小（スタンプ） → バウンドさせる共通化関数
    IEnumerator AnimateStampObject(GameObject obj)
    {
        // もともと配置されている本来のサイズ（Scale）を基準値として保存
        Vector3 defaultScale = obj.transform.localScale;

        // 最初は指定された「超巨大サイズ」に設定してからアクティブ化（パッと大きく出る）
        Vector3 startScale = defaultScale * startScaleMultiplier;
        obj.transform.localScale = startScale;
        obj.SetActive(true);

        float elapsedTime = 0f;

        // 【巨大サイズから、勢いよくズドンと元のサイズへ縮小】
        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / shrinkDuration;

            t = t * t; // 最初ゆっくり、直前に一気に加速（EaseInQuad）

            obj.transform.transform.localScale = Vector3.Lerp(startScale, defaultScale, t);
            yield return null;
        }

        // スタンプが叩きつけられた衝撃のバウンド演出
        if (useBounceEffect)
        {
            elapsedTime = 0f;
            float bounceDuration = 0.12f;

            while (elapsedTime < bounceDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / bounceDuration;

                float bounceCurve = Mathf.Sin(t * Mathf.PI) * 0.25f; // つぶれ具合の強さ

                obj.transform.localScale = defaultScale - (defaultScale * bounceCurve * (1f - t));
                yield return null;
            }
        }

        // 最後に本来のサイズに綺麗に固定
        obj.transform.localScale = defaultScale;
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