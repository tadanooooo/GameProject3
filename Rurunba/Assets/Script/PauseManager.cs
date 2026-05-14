using UnityEngine;
using UnityEngine.SceneManagement; // これを忘れずに！

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI; // ポーズ画面のパネルをここに入れる
    //private bool isPaused = false;

    // 右上のポーズボタン
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // 時間を止める
        //isPaused = true;
    }

    // 再開ボタン用
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // 時間を動かす
        //isPaused = false;
    }

    // Rボタン（リトライ）用
    public void Retry()
    {
        Time.timeScale = 1f; // ロード前に時間を動かすのがコツ
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Stage Selectボタン用
    public void GoToStageSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StageSelectScene");
    }

    // Titleボタン用
    public void GoToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }
}