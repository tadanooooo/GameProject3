using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TitleManager : MonoBehaviour
{    void Awake()
    {
        // スマホのタッチパネル機能をシステムに強制的に登録して認識させる
        InputSystem.AddDevice<Touchscreen>();
    }
    public void GoToStageSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("1_StageSelectScene");
    }

    // ゲーム終了ボタン用
    public void ExitGame()
    {
        Debug.Log("ゲームを終了");

        // ビルドしたアプリを終了
        Application.Quit();

        // Unityエディタ上での実行も停止（動作確認用）
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}