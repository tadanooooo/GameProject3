using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int totalTrashCount;
    public int currentTrashCount;
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // フレームレートを60fpsに固定
        Application.targetFrameRate = 60;

        // スリープ防止
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // ステージ上の "Trash" タグが付いたオブジェクトを全て数える
        GameObject[] trashes = GameObject.FindGameObjectsWithTag("Trash");
        totalTrashCount = trashes.Length;
        currentTrashCount = totalTrashCount;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(0);
        }
    }

        // ゴミが吸い込まれた時に呼ぶメソッド
    public void TrashCollected()
    {
        currentTrashCount--;
    }

    // 全てのゴミを拾ったか確認する
    public bool IsAllTrashCollected()
    {
        return currentTrashCount <= 0;
    }
}