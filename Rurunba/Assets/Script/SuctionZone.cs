using UnityEngine;

public class SuctionZone : MonoBehaviour
{
    public float suctionSpeed = 5f;
    public Transform suctionPoint;
    public float shrinkSpeed = 2.0f;
    public float killDistance = 0.1f;

    // ゴミを安全に消去するための共通メソッド
    void RemoveTrash(GameObject trash)
    {
        // すでに消去処理が始まっている（タグが外れている）場合は何もしない
        if (!trash.CompareTag("Trash")) return;

        // タグを先に外すことで1フレーム内に2回判定されるのを防ぐ
        trash.tag = "Untagged";

        if (GameManager.instance != null)
        {
            GameManager.instance.TrashCollected();
        }

        Destroy(trash);
        Debug.Log("ゴミを回収しました。残り: " + GameManager.instance.currentTrashCount);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trash"))
        {
            // すでに親が自分なら、それは吸い込み中の「本体接触」判定とみなす
            if (other.transform.parent == this.transform)
            {
                RemoveTrash(other.gameObject);
                return;
            }

            // 初めて吸い込み範囲に入った時の処理
            other.transform.SetParent(this.transform);

            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            other.isTrigger = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Trash"))
        {
            // 中心へ移動
            other.transform.position = Vector3.MoveTowards(
                other.transform.position,
                suctionPoint.position,
                suctionSpeed * Time.deltaTime
            );

            // 小さくする
            float shrinkAmount = shrinkSpeed * Time.deltaTime;
            other.transform.localScale -= new Vector3(shrinkAmount, shrinkAmount, shrinkAmount);

            // 距離またはサイズによる消去判定
            float distance = Vector3.Distance(other.transform.position, suctionPoint.position);
            if (distance < killDistance || other.transform.localScale.x <= 0.01f)
            {
                RemoveTrash(other.gameObject);
            }
        }
    }
}