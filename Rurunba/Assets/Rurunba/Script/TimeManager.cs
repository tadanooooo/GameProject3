using UnityEngine;
using TMPro; // TextMeshPro専用
using System.Collections; // コルーチン（時間待ち処理）に必要

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    [Header("UI設定 (TextMeshPro専用)")]
    public TextMeshProUGUI timerTextMeshPro;       // 普段のタイマー用
    public TextMeshProUGUI countdownTextMeshPro;   // カウントダウン用

    private float elapsedTime = 0f;

    // 外部のGyroPlayerから今タイマーが動いているかを確認
    [HideInInspector] public bool isTimerRunning = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // カウントダウンスタート
        StartCoroutine(StartCountdownRoutine());
    }

    // 1秒ずつ待って文字を変えるコルーチン
    IEnumerator StartCountdownRoutine()
    {
        if (countdownTextMeshPro != null) countdownTextMeshPro.gameObject.SetActive(true);

        // 3
        if (countdownTextMeshPro != null) countdownTextMeshPro.text = "3";
        yield return new WaitForSeconds(1.0f);

        // 2
        if (countdownTextMeshPro != null) countdownTextMeshPro.text = "2";
        yield return new WaitForSeconds(1.0f);

        // 1
        if (countdownTextMeshPro != null) countdownTextMeshPro.text = "1";
        yield return new WaitForSeconds(1.0f);

        // GO
        if (countdownTextMeshPro != null) countdownTextMeshPro.text = "GO";

        // ここでタイマーが動き出します
        isTimerRunning = true;

        // GO! の文字を1秒だけ見せてから消す
        yield return new WaitForSeconds(1.0f);
        if (countdownTextMeshPro != null) countdownTextMeshPro.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            DisplayTime();
        }
    }

    void DisplayTime()
    {
        if (timerTextMeshPro != null)
        {
            timerTextMeshPro.text = elapsedTime.ToString("F2");
        }
    }

    public void StopTimer() => isTimerRunning = false;
    public float GetTime() => elapsedTime;
}