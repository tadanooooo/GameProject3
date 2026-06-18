using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

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

    [Header("消去するメインUI設定")]
    public GameObject mainUiObject;

    [Header("ゲームオーバーUI設定")]
    public GameObject gameOverImage;
    public GameObject gameOverPanel;
    public string stageSelectSceneName = "1_StageSelectScene";

    [Header("壁衝突時のエフェクト（Prefab）")]
    public GameObject hitEffectPrefab;

    // 点滅用の設定
    [Header("点滅設定")]
    [Tooltip("点滅させる間隔（秒）")]
    public float blinkInterval = 0.2f;

    [Tooltip("Rurunba 1 のすぐ下にある子オブジェクト（Rumba_animation_Correctionなど）をここにドラッグ")]
    public GameObject modelRootChild;

    private CanvasGroup canvasGroup;
    private float visibleTimer = 0f;

    private bool isInvincible = false;
    private float invincibleTimer = 0f;
    private bool isDead = false; // 死亡フラグ

    private Coroutine blinkCoroutine; // コルーチン管理用

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

        if (gameOverImage != null) gameOverImage.gameObject.SetActive(false);
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

    public void TakeDamage(int damage, Collision collision = null)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(1);
        }

        if (isInvincible || isDead) return;

        currentHp -= damage;
        hpSlider.value = currentHp;
        UpdateHpColor();

        Debug.Log("現在HP : " + currentHp);

        canvasGroup.alpha = 1f;
        visibleTimer = visibleTime;

        isInvincible = true;
        invincibleTimer = invincibleTime;

        // ダメージ時に点滅コルーチンを開始
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkRoutine());

        if (hitEffectPrefab != null && collision != null && collision.contacts.Length > 0)
        {
            Vector3 hitPoint = collision.contacts[0].point;
            Quaternion hitRotation = Quaternion.LookRotation(collision.contacts[0].normal);

            GameObject effect = Instantiate(hitEffectPrefab, hitPoint, hitRotation);
            Destroy(effect, 2.0f);
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // 点滅処理
    IEnumerator BlinkRoutine()
    {
        // もしインスペクターで指定されていなければ、自動で最初の子オブジェクトを見つける
        if (modelRootChild == null && transform.childCount > 0)
        {
            modelRootChild = transform.GetChild(0).gameObject;
        }

        if (modelRootChild == null)
        {
            Debug.LogError("【HpManager】点滅させる子オブジェクトが見つかりません。インスペクターで設定してください。");
            yield break;
        }

        while (isInvincible)
        {
            // 子オブジェクトごと見た目を丸ごと消す
            modelRootChild.SetActive(false);
            yield return new WaitForSeconds(blinkInterval);

            // 戻す
            modelRootChild.SetActive(true);
            yield return new WaitForSeconds(blinkInterval);
        }

        // 無敵時間が終わったら必ず表示状態にする
        modelRootChild.SetActive(true);
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

        // 死亡時は点滅を止めモデル表示
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        if (modelRootChild != null)
        {
            modelRootChild.SetActive(true);
        }

        Debug.Log("プレイヤー死亡 ゲームオーバー");
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(8);
        }

        if (TimeManager.instance != null)
        {
            TimeManager.instance.StopTimer();
        }

        Rigidbody rb = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (mainUiObject != null)
        {
            mainUiObject.SetActive(false);
        }

        if (gameOverImage != null) gameOverImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(3.0f);

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