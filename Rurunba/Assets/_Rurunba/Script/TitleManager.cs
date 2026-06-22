using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class TitleManager : MonoBehaviour
{
    [Header("シーン名設定")]
    [Tooltip("ステージ選択画面のシーン名")]
    public string stageSelectSceneName = "1_StageSelectScene";
    [Tooltip("チュートリアル画面のシーン名（ここにチュートリアルのシーン名を入れてね）")]
    public string tutorialSceneName = "TutorialScene";

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

    // チュートリアルボタンから呼び出す関数
    public void GoToTutorial()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }
        // チュートリアル用のコルーチンを開始
        StartCoroutine(MoveToTutorialSequence());
    }

    // タイムラグを作ってチュートリアルシーンに切り替える（コルーチン）
    private IEnumerator MoveToTutorialSequence()
    {
        // 1.5秒待機
        yield return new WaitForSeconds(1.5f);

        // シーンを読込
        SceneManager.LoadScene(tutorialSceneName);
    }

    // ボタンや画面タップから呼び出す関数（ステージ選択へ）
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
        SceneManager.LoadScene(stageSelectSceneName);
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