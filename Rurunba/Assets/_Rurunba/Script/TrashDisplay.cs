using UnityEngine;
using TMPro;

public class TrashDisplay : MonoBehaviour
{
    private TextMeshProUGUI trashText;

    void Start()
    {
        trashText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (trashText == null) return;

        // .ゲーム本編（GameManagerがいる時）
        if (GameManager.instance != null)
        {
            trashText.text = GameManager.instance.currentTrashCount + "/" + GameManager.instance.totalTrashCount;
        }
        // チュートリアル（TutorialManagerがいる時）
        else if (TutorialManager.instance != null)
        {
            // 構造が全く同じなので、引っ張る先をTutorialManagerに変える
            trashText.text = TutorialManager.instance.currentTrashCount + "/" + TutorialManager.instance.totalTrashCount;
        }
    }
}