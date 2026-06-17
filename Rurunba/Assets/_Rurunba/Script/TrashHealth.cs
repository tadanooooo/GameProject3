using UnityEngine;

public class TrashHealth : MonoBehaviour
{
    [Header("耐久値（吸い込みに必要な時間")]
    public float hp = 3.0f; // 3秒間吸い込まないと消えない

    public bool IsBroken => hp <= 0; // HPが0になったかチェックする用

    public void TakeDamage(float amount)
    {
        hp -= amount;
    }
}