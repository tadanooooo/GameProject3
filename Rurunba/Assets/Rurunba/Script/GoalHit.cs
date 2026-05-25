using UnityEngine;

public class GoalHit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GoalManager.instance.StartGoal();
        }
    }
}