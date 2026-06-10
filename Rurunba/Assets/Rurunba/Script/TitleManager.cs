using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class TitleManager : MonoBehaviour
{
    [Header("画面タップ時に消したいテキストオブジェクト")]
    public GameObject textToHide;

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
        // コルーチン（時間差処理）を開始する
        StartCoroutine(MoveToStageSelectSequence());
    }

    // タイムラグを作ってシーンを切り替える中身（コルーチン）
    private IEnumerator MoveToStageSelectSequence()
    {
        if (textToHide != null)
        {
            textToHide.SetActive(false);
        }
        // SEを鳴らす
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(1);
        }

        // そのまま3.0秒待機
        yield return new WaitForSeconds(2.0f);

        // 3秒経ったらシーンを読込
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