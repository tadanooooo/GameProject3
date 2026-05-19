using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GoalPoint : MonoBehaviour
{
    [Header("UI設定")]
    public GameObject clearPanel;
    public GameObject clearText;

    [Header("星の画像")]
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    [Header("TimeAttack")]
    public float targetTime = 30.0f; // この秒数以内にクリアで星3
    private float elapsedTime = 0f;

    private bool isCleared = false;

    void Update()
    {
        // クリアするまで時間をカウント
        if (!isCleared)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーが触れた、かつ、まだクリアしていない場合
        if (other.CompareTag("Player") && !isCleared)
        {
            isCleared = true; // 二重判定を防止
            ExecuteGameClear();
        }
    }

    void ExecuteGameClear()
    {
        // タイマーを止める（TimeManagerに指示を出す）
        if (TimeManager.instance != null)
        {
            TimeManager.instance.StopTimer();
        }

        Debug.Log("GameClear! TIME: " + elapsedTime);

        // 物理的な停止処理
        Rigidbody rb = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // パネルとテキストを表示
        if (clearPanel != null) clearPanel.SetActive(true);
        if (clearText != null) clearText.SetActive(true);

        // 星の判定計算
        CalculateStars();
    }

    void CalculateStars()
    {
        // タイム判定の数字を TimeManager からもらってくる
        float finalTime = TimeManager.instance.GetTime();

        // 星1: 充電器に到達 (無条件)
        if (star1 != null) star1.SetActive(true);

        // 星2: ゴミを全て取ったか
        bool allCollected = false;
        if (GameManager.instance != null && GameManager.instance.IsAllTrashCollected())
        {
            if (star2 != null) star2.SetActive(true);
            allCollected = true;
            Debug.Log("星2獲得");
        }

        // 星3: 全回収 + 目標タイム以内
        if (allCollected && finalTime <= targetTime)
        {
            if (star3 != null) star3.SetActive(true);
            Debug.Log("星3獲得");
        }
    }
}