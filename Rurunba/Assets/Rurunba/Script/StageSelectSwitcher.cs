using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // ˆø”‚Æ‚µ‚Ä string (•¶š—ñ) ‚ğó‚¯æ‚é‚æ‚¤‚É‚µ‚Ü‚·
    public void ChangeScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("ƒV[ƒ“–¼‚ª“ü—Í‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
        }
    }
}