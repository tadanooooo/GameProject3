using UnityEngine;
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

    private CanvasGroup canvasGroup;

    private float visibleTimer = 0f;

    private bool isInvincible = false;
    private float invincibleTimer = 0f;

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
        return !isInvincible;
    }

    public void TakeDamage(int damage)
    {
        // 無敵中なら受けない
        if (isInvincible) return;

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
        // HP 5,4
        if (currentHp >= 4)
        {
            fillImage.color = Color.green;
        }
        // HP 3,2
        else if (currentHp >= 2)
        {
            fillImage.color = Color.yellow;
        }
        // HP 1
        else
        {
            fillImage.color = Color.red;
        }
    }

    void Die()
    {
        Debug.Log("プレイヤー死亡");
    }
}