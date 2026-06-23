using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    private enum TutorialPhase
    {
        OpeningNovel,
        Countdown,
        Phase1_Gyro,
        Phase2_Damage,
        Phase3_Trash,
        Phase4_Goal,
        EndingNovel
    }

    private TutorialPhase currentPhase = TutorialPhase.OpeningNovel;

    [Header("UI設定（ノベル・タイマー）")]
    public GameObject novelPanel;
    public TextMeshProUGUI novelText;
    public TextMeshProUGUI timerText;

    [Header("カウントダウン画像設定 (3→2→1→Start)")]
    public List<GameObject> countdownImages = new List<GameObject>();

    [Header("目標テキストオブジェクトの設定")]
    public GameObject objectiveText1;
    public GameObject objectiveText2;
    public GameObject objectiveText3;
    public GameObject objectiveText4;

    [Header("床オブジェクトの設定")]
    public GameObject floor1;
    public GameObject floor2;
    public GameObject floor3;

    [Header("ステージ内のターゲット設定")]
    public List<GameObject> obstacles = new List<GameObject>();
    public GameObject trashGroup;
    public string titleSceneName = "TitleScene";

    [Header("ゴミカウント")]
    public int totalTrashCount = 0;
    public int currentTrashCount = 0;

    [Header("ゴール演出UI設定")]
    public GameObject goalTextObject;

    private List<string> openingLines = new List<string>()
    {
        "チュートリアルへようこそ！",
        "このゲームは、スマホを傾けるジャイロ操作でロボットを動かします。",
        "基本ルールを覚えるために、いくつかの条件をクリアしてもらうよ！",
        "準備はいいですか？ それではスタート！"
    };

    private List<string> endingLines = new List<string>()
    {
        "お疲れ様でした！",
        "これでチュートリアルを終了します。",
        "ちなみに、クリアタイムが早いほどいい評価を得られるよ！",
        "それでは、ゲーム本編にチャレンジしてみよう！"
    };

    private int currentLineIndex = 0;
    private float gyroTimer = 0f;
    private float tutorialTimer = 0f;

    [HideInInspector] public bool isTimerRunning = false;

    private int damageCount = 0;
    private const int RequiredDamageCount = 2;

    private GameObject playerObject;
    private bool isCleared = false;
    private float savedGoalTime = 0f;

    private bool isTransitioningPhase2 = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(0);
        }

        novelPanel.SetActive(true);

        if (objectiveText1 != null) objectiveText1.SetActive(false);
        if (objectiveText2 != null) objectiveText2.SetActive(false);
        if (objectiveText3 != null) objectiveText3.SetActive(false);
        if (objectiveText4 != null) objectiveText4.SetActive(false);
        if (goalTextObject != null) goalTextObject.SetActive(false);

        foreach (GameObject img in countdownImages) { if (img != null) img.SetActive(false); }
        if (timerText != null) timerText.text = "00:00";

        GameObject[] trashes = GameObject.FindGameObjectsWithTag("Trash");
        totalTrashCount = trashes.Length;
        currentTrashCount = totalTrashCount;

        SetSuctionZonesActive(false);

        ShowOpeningLine();
        SetPlayerScriptEnabled(false);
        SetPlayerPhysicsLocked(true);
    }

    void EnsureSolidFloor(GameObject floorObj)
    {
        if (floorObj != null)
        {
            Collider col = floorObj.GetComponent<Collider>();
            if (col != null) col.isTrigger = false;
        }
    }

    void WakeUpAllTrashes()
    {
        GameObject[] trashes = GameObject.FindGameObjectsWithTag("Trash");
        foreach (GameObject trash in trashes)
        {
            if (trash != null)
            {
                Rigidbody rb = trash.GetComponent<Rigidbody>();
                if (rb != null) rb.WakeUp();
            }
        }
    }

    void SetSuctionZonesActive(bool isActive)
    {
        foreach (var mono in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mono != null && (mono.GetType().Name.Contains("Suction") || mono.gameObject.name.Contains("Suction")))
            {
                Collider col = mono.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = isActive;
                }
            }
        }
    }

    IEnumerator HeavyWakeUpRoutine()
    {
        for (int i = 0; i < 5; i++)
        {
            WakeUpAllTrashes();
            yield return new WaitForSeconds(0.2f);
        }
    }

    void Update()
    {
        if (currentPhase == TutorialPhase.OpeningNovel ||
            currentPhase == TutorialPhase.Countdown ||
            currentPhase == TutorialPhase.EndingNovel)
        {
            SetPlayerScriptEnabled(false);
            SetPlayerPhysicsLocked(true);
            HandleNovelInput();
            return;
        }

        // プレイヤーが動くフェーズ（1~4）は物理ロックを解除
        SetPlayerPhysicsLocked(false);

        if (isTimerRunning && timerText != null)
        {
            tutorialTimer += Time.deltaTime;
            int minutes = Mathf.FloorToInt(tutorialTimer / 60F);
            int seconds = Mathf.FloorToInt(tutorialTimer % 60F);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        switch (currentPhase)
        {
            case TutorialPhase.Phase1_Gyro:
                HandleGyroPhase();
                break;
            case TutorialPhase.Phase2_Damage:
                HandleDamagePhase();
                break;
        }
    }

    void SetPlayerPhysicsLocked(bool isLocked)
    {
        if (playerObject == null) playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            Rigidbody rb = playerObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (rb.isKinematic == isLocked) return;

                if (isLocked)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                rb.isKinematic = isLocked;
            }
        }
    }

    void HandleNovelInput()
    {
        if (Mathf.Approximately(Time.timeScale, 0f)) return;

        if (UnityEngine.InputSystem.Pointer.current != null &&
            UnityEngine.InputSystem.Pointer.current.press.wasPressedThisFrame)
        {
            if (EventSystem.current != null)
            {
                GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

                if (currentSelected != null && currentSelected.name.Contains("PauseButton"))
                {
                    return;
                }
            }

            if (currentPhase == TutorialPhase.OpeningNovel)
            {
                currentLineIndex++;
                ShowOpeningLine();
            }
            else if (currentPhase == TutorialPhase.EndingNovel)
            {
                currentLineIndex++;
                ShowEndingLine();
            }
        }
    }

    void ShowOpeningLine()
    {
        if (currentLineIndex < openingLines.Count)
        {
            novelText.text = openingLines[currentLineIndex];
        }
        else
        {
            if (novelText != null) novelText.text = "";
            novelPanel.SetActive(false);
            StartCoroutine(CountdownRoutine());
        }
    }

    IEnumerator CountdownRoutine()
    {
        currentPhase = TutorialPhase.Countdown;

        for (int i = 0; i < 4; i++)
        {
            if (countdownImages.Count > i && countdownImages[i] != null)
            {
                countdownImages[i].SetActive(true);

                if (AudioManager.Instance != null)
                {
                    if (i == 3)
                    {
                        AudioManager.Instance.PlaySE(4);
                        isTimerRunning = true;
                    }
                    else
                    {
                        AudioManager.Instance.PlaySE(6);
                    }
                }
                else if (i == 3)
                {
                    isTimerRunning = true;
                }

                yield return new WaitForSeconds(1f);
                countdownImages[i].SetActive(false);
            }
        }

        currentPhase = TutorialPhase.Phase1_Gyro;
        SetPlayerScriptEnabled(true);
        SetPlayerPhysicsLocked(false);

        if (objectiveText1 != null) objectiveText1.SetActive(true);
        UpdateGyroText();
    }

    void SetPlayerScriptEnabled(bool isEnabled)
    {
        if (playerObject == null) playerObject = GameObject.FindWithTag("Player");

        if (playerObject != null)
        {
            GyroPlayer gyroScript = playerObject.GetComponent<GyroPlayer>();
            if (gyroScript != null) gyroScript.enabled = isEnabled;
        }
    }

    void HandleGyroPhase()
    {
        gyroTimer += Time.deltaTime;
        UpdateGyroText();

        if (gyroTimer >= 5f)
        {
            if (floor1 != null) Destroy(floor1);
            if (objectiveText1 != null) Destroy(objectiveText1);

            StartCoroutine(HeavyWakeUpRoutine());

            currentPhase = TutorialPhase.Phase2_Damage;
            SetPlayerScriptEnabled(true);

            if (objectiveText2 != null) objectiveText2.SetActive(true);
            UpdateDamageObjectiveText();
        }
    }

    void UpdateGyroText()
    {
        if (objectiveText1 == null) return;
        TextMeshProUGUI t = objectiveText1.GetComponent<TextMeshProUGUI>();
        if (t != null)
        {
            int remainingTime = Mathf.CeilToInt(Mathf.Max(0f, 5f - gyroTimer));
            t.text = $"目標1：スマホを傾けて自由に動いてみよう！ (あと {remainingTime} 秒)";
        }
    }

    void HandleDamagePhase()
    {
    }

    private bool isCooldown = false;

    public void NotifyDamage()
    {
        if (isTransitioningPhase2) return;

        if (currentPhase == TutorialPhase.Phase2_Damage && !isCooldown)
        {
            damageCount++;
            UpdateDamageObjectiveText();

            if (damageCount >= RequiredDamageCount)
            {
                StartCoroutine(ClearPhase2Routine());
            }
            else
            {
                StartCoroutine(DamageCooldownRoutine());
            }
        }
    }

    IEnumerator DamageCooldownRoutine()
    {
        isCooldown = true;
        yield return new WaitForSeconds(0.5f);
        isCooldown = false;
    }

    IEnumerator ClearPhase2Routine()
    {
        isTransitioningPhase2 = true;
        yield return new WaitForSeconds(1.0f);

        if (floor2 != null) Destroy(floor2);
        if (objectiveText2 != null) Destroy(objectiveText2);
        foreach (GameObject obstacle in obstacles) { if (obstacle != null) Destroy(obstacle); }

        StartCoroutine(HeavyWakeUpRoutine());

        SetSuctionZonesActive(true);

        currentPhase = TutorialPhase.Phase3_Trash;
        SetPlayerScriptEnabled(true);

        if (currentTrashCount <= 0)
        {
            CheckTrashPhaseClear();
        }
        else
        {
            if (objectiveText3 != null) objectiveText3.SetActive(true);
        }

        isTransitioningPhase2 = false;
    }

    void UpdateDamageObjectiveText()
    {
        if (objectiveText2 == null) return;
        TextMeshProUGUI t = objectiveText2.GetComponent<TextMeshProUGUI>();
        if (t != null)
        {
            t.text = $"目標2：わざと障害物にぶつかってダメージを受けてみよう！ ({damageCount} / {RequiredDamageCount})";
        }
    }

    public void NotifyTrashCollected()
    {
        currentTrashCount--;

        if (currentTrashCount <= 0)
        {
            currentTrashCount = 0;

            if (currentPhase == TutorialPhase.Phase3_Trash)
            {
                CheckTrashPhaseClear();
            }
        }
    }

    private void CheckTrashPhaseClear()
    {
        if (floor3 != null) Destroy(floor3);
        if (objectiveText3 != null) Destroy(objectiveText3);

        currentPhase = TutorialPhase.Phase4_Goal;
        if (objectiveText4 != null) objectiveText4.SetActive(true);
    }

    public void NotifyGoalReached()
    {
        if (currentPhase == TutorialPhase.Phase4_Goal && !isCleared)
        {
            isCleared = true;
            isTimerRunning = false;

            savedGoalTime = tutorialTimer;
            Debug.Log("チュートリアルクリアタイム : " + savedGoalTime);

            if (objectiveText4 != null) Destroy(objectiveText4);

            StartCoroutine(TutorialGoalSequence());
        }
    }

    IEnumerator TutorialGoalSequence()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(2);
        }

        // 実際にゴールに触れたら安全にロックする
        SetPlayerScriptEnabled(false);
        SetPlayerPhysicsLocked(true);

        if (goalTextObject != null) goalTextObject.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        if (goalTextObject != null) goalTextObject.SetActive(false);

        novelPanel.SetActive(true);
        currentLineIndex = 0;
        currentPhase = TutorialPhase.EndingNovel;
        ShowEndingLine();
    }

    void ShowEndingLine()
    {
        if (currentLineIndex < endingLines.Count)
        {
            novelText.text = endingLines[currentLineIndex];
        }
        else
        {
            SceneManager.LoadScene(titleSceneName);
        }
    }

    public void RetryTutorial()
    {
        StopAllCoroutines();
        instance = null;

        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}