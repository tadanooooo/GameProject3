using UnityEngine;
using UnityEngine.UI;

public class StageSelectButton : MonoBehaviour
{
    // ステージ番号
    [Header("ステージ番号")]
    public int stageNumber;

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnButtonClick);
        }
    }

    // ボタンが押された瞬間に実行される処理
    void OnButtonClick()
    {
        // 画面に1つだけある共通管理スクリプト（StageInfoDisplay）を探す
        if (StageInfoDisplay.instance != null)
        {
            // ステージ番号（stageNumber）の情報を表示
            StageInfoDisplay.instance.DisplayStageInfo(stageNumber);
        }
    }
}