using UnityEngine;

public class GoalHit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // もしチュートリアルマネージャーがいれば、チュートリアルのゴール処理を呼ぶ
            if (TutorialManager.instance != null)
            {
                TutorialManager.instance.NotifyGoalReached();
                Debug.Log("チュートリアルのゴールを検知しました");
            }
            // チュートリアルではなく、本編のゴールマネージャーがいれば、本編のゴール処理を呼ぶ
            else if (GoalManager.instance != null)
            {
                GoalManager.instance.StartGoal();
                Debug.Log("本編ステージのゴールを検知しました");
            }
            else
            {
                Debug.LogWarning("ゴール処理を実行できるマネージャーがシーン内に存在しません。");
            }
        }
    }
}