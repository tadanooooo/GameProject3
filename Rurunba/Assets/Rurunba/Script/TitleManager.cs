using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class TitleManager : MonoBehaviour
{

    void Awake()
    {

    }
    void Start()
    {
        // BGM（0番）を再生
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(0);
        }
    }

    // ボタンや画面タップから呼び出す関数
    public void GoToStageSelect()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(1);
        }
        // コルーチン（時間差処理）を開始する
        StartCoroutine(MoveToStageSelectSequence());
    }

    // タイムラグを作ってシーンを切り替える中身（コルーチン）
    private IEnumerator MoveToStageSelectSequence()
    {
        // そのまま待機
        yield return new WaitForSeconds(1.5f);

        // シーンを読込
        SceneManager.LoadScene("1_StageSelectScene");
    }

    // ゲーム終了ボタン用（コルーチンを呼び出す窓口）
    public void ExitGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }

        StartCoroutine(ExitGameSequence());
    }

    // 0.5秒の間隔を作ってからゲームを終了する中身
    private IEnumerator ExitGameSequence()
    {
        // ここで0.5秒（0.5f）待機する
        yield return new WaitForSeconds(0.5f);

        Debug.Log("ゲームを終了");

        // ビルドしたアプリを終了
        Application.Quit();

        // Unityエディタ上での実行も停止（動作確認用）
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}