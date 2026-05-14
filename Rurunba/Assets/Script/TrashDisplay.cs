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
        if (GameManager.instance != null)
        {
            //trashText.text = GameManager.instance.currentTrashCount + "/" + GameManager.instance.totalTrashCount;
            trashText.text = GameManager.instance.currentTrashCount.ToString();
        }
    }
}