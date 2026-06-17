using UnityEngine;
using TMPro; // TextMeshPro専用
using System.Collections; // コルーチン（時間待ち処理）に必要
using UnityEngine.UI; // 画像（Image）コンポーネントに必要

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    [Header("UI設定")]
    public TextMeshProUGUI timerTextMeshPro;       // 普段のタイマー用

    [Header("カウントダウン画像設定 (ヒエラルキーのImageを登録)")]
    // Imageの配列を用意（0:「3」, 1:「2」, 2:「1」, 3:「スタート」）
    public Image[] countdownImages;

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

    // 1秒ずつ待ってImageの表示・非表示を切り替えるコルーチン
    IEnumerator StartCountdownRoutine()
    {
        // 配列が正しく設定されているかチェック
        if (countdownImages != null && countdownImages.Length >= 4)
        {
            // 最初にすべてのカウントダウン画像を非表示にしておく（念のため）
            foreach (var img in countdownImages)
            {
                if (img != null) img.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(0.2f);
            yield return new WaitForEndOfFrame();

            // 3 の画像を表示
            if (countdownImages[0] != null) countdownImages[0].gameObject.SetActive(true);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(6);
            }
            yield return new WaitForSeconds(1.0f);
            if (countdownImages[0] != null) countdownImages[0].gameObject.SetActive(false); // 消す

            // 2 の画像を表示
            if (countdownImages[1] != null) countdownImages[1].gameObject.SetActive(true);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(6);
            }
            yield return new WaitForSeconds(1.0f);
            if (countdownImages[1] != null) countdownImages[1].gameObject.SetActive(false); // 消す

            // 1 の画像を表示
            if (countdownImages[2] != null) countdownImages[2].gameObject.SetActive(true);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(6);
            }
            yield return new WaitForSeconds(1.0f);
            if (countdownImages[2] != null) countdownImages[2].gameObject.SetActive(false); // 消す

            // スタート! の画像を表示
            if (countdownImages[3] != null) countdownImages[3].gameObject.SetActive(true);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(4);
            }
        }

        // ここでタイマーが動き出します
        isTimerRunning = true;

        // スタート! の画像を1秒だけ見せてから消す
        yield return new WaitForSeconds(1.0f);
        if (countdownImages != null && countdownImages.Length >= 4 && countdownImages[3] != null)
        {
            countdownImages[3].gameObject.SetActive(false);
        }
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
            timerTextMeshPro.text = elapsedTime.ToString("F2") + "s";
        }
    }

    public void StopTimer() => isTimerRunning = false;
    public float GetTime() => elapsedTime;
}