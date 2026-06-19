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

        // ★実機（Android）向けの衝突エフェクトバグ対策
        if (hitEffectPrefab != null && collision != null && collision.contacts.Length > 0)
        {
            Vector3 hitPoint = collision.contacts[0].point;
            Vector3 hitNormal = collision.contacts[0].normal;
            Quaternion hitRotation = Quaternion.identity; // 初期値は回転なし（計算エラー時の保険）

            // 法線ベクトルがほぼゼロ（計算不可能）でなければ、壁の向きに合わせる
            if (hitNormal.sqrMagnitude > 0.001f)
            {
                hitRotation = Quaternion.LookRotation(hitNormal);
            }

            GameObject effect = Instantiate(hitEffectPrefab, hitPoint, hitRotation);
            Destroy(effect, 2.0f);
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // 指定されたオブジェクトだけを確実に SetActive でオンオフする
    IEnumerator BlinkRoutine()
    {
        // もしインスペクターで何も指定されていなければ、安全のために全消え（以前の方式）で動かす
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

        // 無敵時間中、指定されたオブジェクトだけを確実にチカチカさせる（SuctionAreaはリストに入っていないので消えません！）
        while (isInvincible)
        {
            foreach (GameObject obj in targetBlinkObjects) { if (obj != null) obj.SetActive(false); }
            yield return new WaitForSeconds(blinkInterval);

            foreach (GameObject obj in targetBlinkObjects) { if (obj != null) obj.SetActive(true); }
            yield return new WaitForSeconds(blinkInterval);
        }

        // 最後は必ず確実にすべて表示に戻す
        foreach (GameObject obj in targetBlinkObjects) { if (obj != null) obj.SetActive(true); }
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