using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public void GoToStageSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StageSelectScene");
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