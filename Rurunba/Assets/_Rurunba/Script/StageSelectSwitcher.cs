using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneSwitcher : MonoBehaviour
{
    // ボタンの On Click() からはこれを呼び出します
    public void ChangeScene(string sceneName)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            // 直接切り替えるのではなく、コルーチンをスタートさせる
            StartCoroutine(ChangeSceneSequence(sceneName));
        }
        else
        {
            // 引数が空の時、コルーチンではなく直接エラーログを出す
            Debug.LogError("シーン名が入力されていません");
        }
    }

    // 間隔を作ってからシーンを切り替える中身
    private IEnumerator ChangeSceneSequence(string sceneName)
    {
        // ここで待機する
        yield return new WaitForSeconds(0.5f);

        // 待ったあとにシーンを読み込む
        SceneManager.LoadScene(sceneName);
    }
}