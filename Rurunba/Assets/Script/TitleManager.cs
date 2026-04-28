using UnityEngine;
using UnityEngine.SceneManagement; // シーン管理に必須！

public class TitleManager : MonoBehaviour
{
    // ボタンから呼び出す関数
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");

        Debug.Log("GameStart");
    }
}