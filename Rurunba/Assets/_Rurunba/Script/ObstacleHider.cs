using UnityEngine;
using System.Collections.Generic;

public class ObstacleHider : MonoBehaviour
{
    public Transform player; // プレイヤーのTransform
    public LayerMask obstacleLayer; // 透過させたい障害物のレイヤー

    // 現在非表示にしているRendererのリスト
    private List<MeshRenderer> hiddenRenderers = new List<MeshRenderer>();
    private List<MeshRenderer> previousHiddenRenderers = new List<MeshRenderer>();

    void LateUpdate()
    {
        if (player == null) return;

        // カメラからプレイヤーへの方向と距離を計算
        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        // カメラとプレイヤーの間にレイ（光線）を飛ばし、当たったすべての障害物を取得
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction.normalized, distance, obstacleLayer);

        // 今回当たったRendererをリストにまとめる
        hiddenRenderers.Clear();
        foreach (RaycastHit hit in hits)
        {
            MeshRenderer renderer = hit.collider.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                hiddenRenderers.Add(renderer);
            }
        }

        //新しく障害物になったものを非表示にする
        foreach (MeshRenderer renderer in hiddenRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = false; // 見た目を消す（パッと消える）
            }
        }

        // 障害物じゃなくなったものを元に戻す
        foreach (MeshRenderer renderer in previousHiddenRenderers)
        {
            // 前回のリストにあって、今回のリストに無いものは、もうカメラを遮っていない
            if (renderer != null && !hiddenRenderers.Contains(renderer))
            {
                renderer.enabled = true; // 見た目を元に戻す
            }
        }

        // 今回のリストを次回用に保存
        previousHiddenRenderers = new List<MeshRenderer>(hiddenRenderers);
    }
}