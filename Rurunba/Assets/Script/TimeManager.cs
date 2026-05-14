using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    [Header("UI")]
    public TextMeshProUGUI timerText;

    private float elapsedTime = 0f;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        // Unity全体の時間が動いている（0じゃない）時だけタイマーを進める
        if (Time.timeScale > 0)
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

    // ゲームクリア時などに完全にタイマーを止めたい場合はこれを使う
    public void StopTimer()
    {
        // Time.timeScaleを0にすれば、上のUpdate内の判定で勝手に止まります
        // もしクリア演出中に時間を止めたくないなら、別のフラグが必要になります
    }

    public float GetTime()
    {
        return elapsedTime;
    }
}