using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI; // ポーズ画面のパネルをここに入れる

    // 右上のポーズボタン
    public void Pause()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // 時間を止める
    }

    // 再開ボタン用
    public void Resume()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // 時間を動かす
    }

    public void Retry()
    {
        StartCoroutine(RetrySequence());
    }

    private IEnumerator RetrySequence()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // 待ったあとに時間を元に戻してロード
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToStageSelect()
    {
        StartCoroutine(GoToStageSelectSequence());
    }

    private IEnumerator GoToStageSelectSequence()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        Time.timeScale = 1f;
        SceneManager.LoadScene("1_StageSelectScene");
    }

    public void GoToTitle()
    {
        StartCoroutine(GoToTitleSequence());
    }

    private IEnumerator GoToTitleSequence()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }
}