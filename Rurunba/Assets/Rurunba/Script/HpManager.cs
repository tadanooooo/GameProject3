using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class HpManager : MonoBehaviour
{
    public static HpManager Instance;

    [Header("HP設定")]
    public int maxHp = 5;
    private int currentHp;

    [Header("無敵時間")]
    public float invincibleTime = 3f;

    [Header("HPのUI")]
    public Slider hpSlider;

    [Header("HPバーの色")]
    public Image fillImage;

    [Header("UIフェード設定")]
    public float visibleTime = 1.5f;
    public float fadeSpeed = 2f;

    // ゲームオーバー用のUI登録を追加
    [Header("ゲームオーバーUI設定")]
    public GameObject gameOverTextObject; 
    public GameObject gameOverPanel;
    public string stageSelectSceneName = "1_StageSelectScene";

    private CanvasGroup canvasGroup;
    private float visibleTimer = 0f;

    private bool isInvincible = false;
    private float invincibleTimer = 0f;
    private bool isDead = false; // 死亡フラグ（二度消し防止など）

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentHp = maxHp;

        hpSlider.maxValue = maxHp;
        hpSlider.value = currentHp;

        canvasGroup = hpSlider.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // ゲーム開始時はゲームオーバーUIを確実に隠しておく
        if (gameOverTextObject != null) gameOverTextObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateHpColor();
    }

    void Update()
    {
        // 無敵時間処理
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;

            if (invincibleTimer <= 0)
            {
                isInvincible = false;
            }
        }

        // UI表示時間
        if (visibleTimer > 0)
        {
            visibleTimer -= Time.deltaTime;
        }
        else
        {
            // フェードアウト
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;

            // 0以下防止
            if (canvasGroup.alpha < 0f)
            {
                canvasGroup.alpha = 0f;
            }
        }
    }

    public bool CanTakeDamage()
    {
        return !isInvincible && !isDead; // 死亡中はダメージを受けない
    }

    public void TakeDamage(int damage)
    {
        // 無敵中、またはすでに死亡しているなら受けない
        if (isInvincible || isDead) return;

        currentHp -= damage;

        hpSlider.value = currentHp;
        UpdateHpColor();

        Debug.Log("現在HP : " + currentHp);

        // UI表示
        canvasGroup.alpha = 1f;
        visibleTimer = visibleTime;

        // 無敵開始
        isInvincible = true;
        invincibleTimer = invincibleTime;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void UpdateHpColor()
    {
        if (currentHp >= 4)
        {
            fillImage.color = Color.green;
        }
        else if (currentHp >= 2)
        {
            fillImage.color = Color.yellow;
        }
        else
        {
            fillImage.color = Color.red;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("プレイヤー死亡 ゲームオーバー演出開始");

        // 時間差演出コルーチンを実行
        StartCoroutine(GameOverSequence());
    }

    //  ゲームオーバー
    IEnumerator GameOverSequence()
    {
        // タイマー止める
        if (TimeManager.instance != null)
        {
            TimeManager.instance.StopTimer();
        }

        // プレイヤーの動きを完全に止め、物理を無効化
        Rigidbody rb = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // GAME OVER
        if (gameOverTextObject != null) gameOverTextObject.SetActive(true);

        // 3.0秒
        yield return new WaitForSeconds(3.0f);

        // パネルを追加表示
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    // リトライ
    public void ClickRetry()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    // ステージ選択
    public void ClickBackToSelect()
    {
        if (!string.IsNullOrEmpty(stageSelectSceneName))
        {
            SceneManager.LoadScene(stageSelectSceneName);
        }
    }
}