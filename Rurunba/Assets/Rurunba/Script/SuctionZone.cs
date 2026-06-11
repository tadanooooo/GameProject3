using UnityEngine;
using System.Collections; // コルーチンを使うために必要

public class SuctionZone : MonoBehaviour
{
    public float shrinkSpeed = 5.0f; // 小さくなる速さ
    public Transform suctionPoint;   // 吸い込みのゴール地点

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trash"))
        {
            // 二重処理防止
            other.tag = "Untagged";

            // GameManagerへの報告
            if (GameManager.instance != null)
            {
                GameManager.instance.TrashCollected();
            }

            // 吸い込みアニメーション開始（コルーチンを呼び出す）
            StartCoroutine(ShrinkAndDestroy(other.gameObject));
        }
    }

    // 徐々に小さくして消す魔法の処理
    IEnumerator ShrinkAndDestroy(GameObject trash)
    {
        // 物理挙動を完全に止めて、警告を防ぐ
        Rigidbody rb = trash.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 当たり判定を消して、ノズルに引っかからないようにする
        Collider col = trash.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 小さくなるまでループ
        while (trash != null && trash.transform.localScale.x > 0.01f)
        {
            // suctionPointがあれば、そこに向かって少し移動させる
            if (suctionPoint != null)
            {
                trash.transform.position = Vector3.MoveTowards(
                    trash.transform.position,
                    suctionPoint.position,
                    0.1f
                );
            }

            // サイズを徐々に小さくする
            trash.transform.localScale = Vector3.Lerp(
                trash.transform.localScale,
                Vector3.zero,
                Time.deltaTime * shrinkSpeed
            );

            yield return null; // 1フレーム待機
        }

        // 最後にオブジェクトを消す
        if (trash != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(0);
            }
            Destroy(trash);
        }
    }
}