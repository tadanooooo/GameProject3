using UnityEngine;
using System.Collections.Generic;

public class ObstacleHider : MonoBehaviour
{
    public Transform player; // プレイヤーのTransform
    public LayerMask obstacleLayer; // 透過させたい障害物のレイヤー

    [Header("透明度の設定")]
    [Range(0f, 1f)]
    public float targetAlpha = 0.3f;

    [Tooltip("透明に変化するスピード")]
    public float fadeSpeed = 5.0f;

    private List<MeshRenderer> hiddenRenderers = new List<MeshRenderer>();
    private Dictionary<MeshRenderer, ObstacleData> obstacleHistory = new Dictionary<MeshRenderer, ObstacleData>();

    private class ObstacleData
    {
        public float currentAlpha;
        public bool isTransparentMode = false;
        public Material[] originalMaterials; // 元の綺麗なマテリアルの配列
        public Material[] runtimeMaterials;  // 透過制御用に複製したマテリアルの配列
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction.normalized, distance, obstacleLayer);

        hiddenRenderers.Clear();

        foreach (RaycastHit hit in hits)
        {
            MeshRenderer renderer = hit.collider.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                hiddenRenderers.Add(renderer);

                if (!obstacleHistory.ContainsKey(renderer))
                {
                    ObstacleData data = new ObstacleData();

                    // 1. 最初期の綺麗なオリジナルをバックアップ（sharedMaterialsを使うのがミソ）
                    data.originalMaterials = renderer.sharedMaterials;

                    // 2. 透過アニメーション用に、現在のマテリアルのコピー（インスタンス）を作成
                    data.runtimeMaterials = renderer.materials;

                    data.currentAlpha = 1.0f;
                    obstacleHistory.Add(renderer, data);
                }
            }
        }

        List<MeshRenderer> keys = new List<MeshRenderer>(obstacleHistory.Keys);
        foreach (MeshRenderer renderer in keys)
        {
            if (renderer == null)
            {
                obstacleHistory.Remove(renderer);
                continue;
            }

            ObstacleData data = obstacleHistory[renderer];
            bool isBlocking = hiddenRenderers.Contains(renderer);
            float finalTargetAlpha = isBlocking ? targetAlpha : 1.0f;

            // 透明化が始まる瞬間に、コピーしたマテリアルを半透明モードにする
            if (isBlocking && !data.isTransparentMode)
            {
                SetMaterialTransparent(data.runtimeMaterials);
                data.isTransparentMode = true;
            }

            data.currentAlpha = Mathf.MoveTowards(data.currentAlpha, finalTargetAlpha, Time.deltaTime * fadeSpeed);

            // コピーしたマテリアルのアルファ値を書き換える
            foreach (Material mat in data.runtimeMaterials)
            {
                if (mat != null)
                {
                    Color newColor = mat.color;
                    newColor.a = data.currentAlpha;
                    mat.color = newColor;
                }
            }

            // 完全に元の不透明（1.0）に戻ったら、バックアップしておいた『元の綺麗なマテリアル』をそのまま上書きして戻す！
            if (!isBlocking && Mathf.Approximately(data.currentAlpha, 1.0f))
            {
                // 用が済んだコピーマテリアルをメモリから削除（メモリリーク対策）
                foreach (Material mat in data.runtimeMaterials)
                {
                    if (mat != null) Destroy(mat);
                }

                // オリジナルを再割り当て（これでピンク化を完全に防ぐ）
                renderer.sharedMaterials = data.originalMaterials;

                obstacleHistory.Remove(renderer);
            }
        }
    }

    private void SetMaterialTransparent(Material[] materials)
    {
        foreach (Material mat in materials)
        {
            if (mat == null) continue;

            // URP用
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1); // 1 = Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            // Built-in用
            else
            {
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var pair in obstacleHistory)
        {
            if (pair.Value != null && pair.Value.runtimeMaterials != null)
            {
                foreach (Material mat in pair.Value.runtimeMaterials)
                {
                    if (mat != null) Destroy(mat);
                }
            }
        }
    }
}