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
        // すでに消去処理が始まっている場合は何もしない
        if (!trash.CompareTag("Trash")) return;

        // タグを先に外すことで1フレーム内に2回判定されるのを防ぐ
        trash.tag = "Untagged";

        if (GameManager.instance != null)
        {
            GameManager.instance.TrashCollected();
        }

        Destroy(trash);
        Debug.Log("ゴミ残り: " + GameManager.instance.currentTrashCount);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trash"))
        {
            // すでに親が自分なら、それは吸い込み中の本体接触判定とみなす
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
            TrashHealth health = other.GetComponent<TrashHealth>();

            // HPがあるゴミの耐える処理
            if (health != null && !health.IsBroken)
            {
                health.TakeDamage(Time.deltaTime);
                other.transform.position += Random.insideUnitSphere * 0.05f;
                return;
            }

            // 吸い込まれて消える処理

            // 中心に向かって移動
            other.transform.position = Vector3.MoveTowards(
                other.transform.position,
                suctionPoint.position,
                suctionSpeed * Time.deltaTime
            );

            // 回転を加えて吸い込まれてる感を出す
            other.transform.Rotate(0, 0, 500 * Time.deltaTime);

            // 中心までの距離に応じて、サイズを0に近づける
            // 距離が killDistance に近づくほど scale が 0 になるように計算
            float currentDist = Vector3.Distance(other.transform.position, suctionPoint.position);

            // 補間（Lerp）を使って、今のサイズから0へ滑らかに変化
            other.transform.localScale = Vector3.Lerp(
                other.transform.localScale,
                Vector3.zero,
                shrinkSpeed * Time.deltaTime
            );

            // 消去判定（距離が十分近い、またはサイズがほぼ0になったら）
            if (currentDist < killDistance || other.transform.localScale.x <= 0.05f)
            {
                RemoveTrash(other.gameObject);
            }
        }
    }
}