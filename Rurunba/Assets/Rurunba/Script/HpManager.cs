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

    // ----- ここから追加・修正 -----
    [Header("消去するメインUI設定")]
    public GameObject mainUiObject;
    // ----------------------------

    [Header("ゲームオーバーUI設定")]
    public GameObject gameOverTextObject;
    public GameObject gameOverPanel;
    public string stageSelectSceneName = "1_StageSelectScene";

    private CanvasGroup canvasGroup;
    private float visibleTimer = 0f;

    private bool isInvincible = false;
    private float invincibleTimer = 0f;
    private bool isDead = false; // 死亡フラグ

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

        if (gameOverTextObject != null) gameOverTextObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateHpColor();
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0)
            {
                isInvincible = false;
            }
        }

        if (visibleTimer > 0)
        {
            visibleTimer -= Time.deltaTime;
        }
        else
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
            if (canvasGroup.alpha < 0f)
            {
                canvasGroup.alpha = 0f;
            }
        }
    }

    public bool CanTakeDamage()
    {
        return !isInvincible && !isDead;
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || isDead) return;

        currentHp -= damage;
        hpSlider.value = currentHp;
        UpdateHpColor();

        Debug.Log("現在HP : " + currentHp);

        canvasGroup.alpha = 1f;
        visibleTimer = visibleTime;

        isInvincible = true;
        invincibleTimer = invincibleTime;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void UpdateHpColor()
    {
        if (currentHp >= 4) fillImage.color = Color.green;
        else if (currentHp >= 2) fillImage.color = Color.yellow;
        else fillImage.color = Color.red;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("プレイヤー死亡 ゲームオーバー");
        StartCoroutine(GameOverSequence());
    }

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

        // 死亡時MainUI非表示
        if (mainUiObject != null)
        {
            mainUiObject.SetActive(false);
        }
        // -----------------------

        // GAME OVERテキストを表示
        if (gameOverTextObject != null) gameOverTextObject.SetActive(true);

        // 3.0秒待機
        yield return new WaitForSeconds(3.0f);

        // パネルを追加表示（リトライボタンなどが入っているパネル）
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
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
}