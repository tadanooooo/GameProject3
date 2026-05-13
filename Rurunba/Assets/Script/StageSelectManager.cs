using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    // ボタンごとに飛ばしたいシーン名を入力するための変数
    public void GoToScene(string sceneName)
    {
        // 引数（sceneName）に入ってきた名前のシーンの読み込み
        SceneManager.LoadScene(sceneName);

        Debug.Log(sceneName + " へ移動");
    }
}