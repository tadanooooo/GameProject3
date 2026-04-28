using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    [Header("UI")]
    public TextMeshProUGUI timerText;

    private float elapsedTime = 0f;
    private bool isPaused = false;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (!isPaused)
        {
            elapsedTime += Time.deltaTime;
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        if (timerText != null)
        {
            timerText.text = elapsedTime.ToString("F2") + "s";
        }
    }

    // 他のスクリプトからタイムを止めるための関数
    public void StopTimer()
    {
        isPaused = true;
    }

    // 現在のタイムを取得するための関数
    public float GetTime()
    {
        return elapsedTime;
    }
}