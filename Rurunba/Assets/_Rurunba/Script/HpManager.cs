using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HpManager : MonoBehaviour
{
    public static HpManager Instance;

    [Header("HP設定")]
    public float maxHp = 5f;
    private float currentHp;

    [Header("無敵時間（通常の壁衝突用）")]
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

    // インスペクターから確実に点滅させたいオブジェクト直接指定
    [Tooltip("点滅させたい見た目オブジェクト（RuRumba本体など）をここにドラッグしてください。SuctionAreaは絶対に入れないでください。")]
    public List<GameObject> targetBlinkObjects = new List<GameObject>();

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

    public bool IsDead()
    {
        return isDead;
    }

    // 第一引数の damage を float 型に変更
    public void TakeDamage(float damage, Collision collision = null, float customInvincibleTime = -1f)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(1);
        }

        if (customInvincibleTime == 0f)
        {
            if (isDead) return;
        }
        else
        {
            if (isInvincible || isDead) return;
        }

        currentHp -= damage;
        hpSlider.value = currentHp;
        UpdateHpColor();

        // 小数点以下も見やすいようにログ出力を修正
        Debug.Log("現在HP : " + currentHp.ToString("F1"));

        canvasGroup.alpha = 1f;
        visibleTimer = visibleTime;

        if (customInvincibleTime >= 0f)
        {
            invincibleTimer = customInvincibleTime;
            isInvincible = (customInvincibleTime > 0f);
        }
        else
        {
            isInvincible = true;
            invincibleTimer = invincibleTime;
        }

        if (isInvincible)
        {
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }

        if (hitEffectPrefab != null && collision != null && collision.contacts.Length > 0)
        {
            Vector3 hitPoint = collision.contacts[0].point;
            Vector3 hitNormal = collision.contacts[0].normal;
            Quaternion hitRotation = Quaternion.identity;

            if (hitNormal.sqrMagnitude > 0.001f)
            {
                hitRotation = Quaternion.LookRotation(hitNormal);
            }

            GameObject effect = Instantiate(hitEffectPrefab, hitPoint, hitRotation);
            Destroy(effect, 2.0f);
        }

        if (currentHp <= 0f)
        {
            Die();
        }
    }

    IEnumerator BlinkRoutine()
    {
        if (targetBlinkObjects == null || targetBlinkObjects.Count == 0)
        {
            Transform firstChild = transform.childCount > 0 ? transform.GetChild(0) : null;
            if (firstChild == null) yield break;

            while (isInvincible)
            {
                firstChild.gameObject.SetActive(false);
                yield return new WaitForSeconds(blinkInterval);
                firstChild.gameObject.SetActive(true);
                yield return new WaitForSeconds(blinkInterval);
            }
            firstChild.gameObject.SetActive(true);
            yield break;
        }

        while (isInvincible)
        {
            foreach (GameObject obj in targetBlinkObjects) { if (obj != null) obj.SetActive(false); }
            yield return new WaitForSeconds(blinkInterval);

            foreach (GameObject obj in targetBlinkObjects) { if (obj != null) obj.SetActive(true); }
            yield return new WaitForSeconds(blinkInterval);
        }

        foreach (GameObject obj in targetBlinkObjects) { if (obj != null) obj.SetActive(true); }
    }

    void UpdateHpColor()
    {
        // 基準値も小数に対応
        if (currentHp >= 4f) fillImage.color = Color.green;
        else if (currentHp >= 2f) fillImage.color = Color.yellow;
        else fillImage.color = Color.red;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        foreach (GameObject obj in targetBlinkObjects) { if (obj != null) obj.SetActive(true); }

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