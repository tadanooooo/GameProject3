using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SuctionZone : MonoBehaviour
{
    public float shrinkSpeed = 5.0f; // 小さくなる速さ
    public Transform suctionPoint;   // 吸い込みのゴール地点

    [Header("吸い込みエフェクト設定")]
    [Tooltip("ゴミが消えた時に出すエフェクトのプレファブを割り当ててください")]
    public GameObject suctionEffectPrefab;

    [Tooltip("エフェクトを発生させる高さをどれくらい上げるか(0で吸い込み点と同じ)")]
    public float effectHeightOffset = 0.5f; // 高さ補正用の変数

    // 現在エリア内で吸い込み中のゴミの数をカウントする
    private int activeSuctionCount = 0;

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
        // 吸い込み中ループ音
        activeSuctionCount++; // 吸い込み中のゴミの数を1増やす
        if (IsAudioPlayable())
        {
            AudioManager.Instance.StartSuctionSE(7);
        }

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

        // ゴミが消える瞬間の位置を記録、高さ調整
        Vector3 effectPosition = transform.position; // 念のためのバックアップ
        if (suctionPoint != null)
        {
            effectPosition = suctionPoint.position; // 吸い込みのゴール地点をエフェクト発生場所に
        }
        else if (trash != null)
        {
            effectPosition = trash.transform.position; // ゴミの最後の位置
        }

        // Y軸（高さ）のみ補正
        effectPosition.y += effectHeightOffset;

        // 最後にオブジェクトを消す
        if (trash != null)
        {
            Destroy(trash);
        }

        // 吸い込みエフェクトを発生させる
        if (suctionEffectPrefab != null)
        {
            GameObject effect = Instantiate(suctionEffectPrefab, effectPosition, Quaternion.identity);
            Destroy(effect, 3.0f);
        }

        // 吸い込み後
        activeSuctionCount--; // 吸い込み中のゴミの数を1減らす

        // 他に吸い込み中のゴミがもう無い場合だけ、ループ音停止
        if (activeSuctionCount <= 0)
        {
            activeSuctionCount = 0; // 念のためマイナスにならないよう安全策

            if (IsAudioPlayable())
            {
                AudioManager.Instance.StopSuctionAndPlayEndSE(3);
            }
        }
        else
        {
            if (IsAudioPlayable())
            {
                AudioManager.Instance.PlaySE(3);
            }
        }
    }

    private void OnDisable()
    {
        ForceStopSuctionSound();
    }

    private void OnDestroy()
    {
        ForceStopSuctionSound();
    }

    /// <summary>
    /// 残ってしまっている吸い込みループ音を強制的に停止する安全弁
    /// </summary>
    private void ForceStopSuctionSound()
    {
        activeSuctionCount = 0;

        // オーディオが「本当に、確実に」再生・操作可能な状態のときだけ命令を送る
        if (IsAudioPlayable())
        {
            try
            {
                AudioManager.Instance.StopSuctionAndPlayEndSE(3);
            }
            catch (System.Exception)
            {
                // それでも漏れ出たUnity内部のエラーは完全に無視する（ゲームの進行に影響させない）
            }
        }
    }

    /// <summary>
    /// AudioManagerおよび、その内部にあるすべてのAudioSourceが正常に動作可能かを【徹底的】に検証する
    /// </summary>
    private bool IsAudioPlayable()
    {
        // マネージャー自体が存在するか
        if (AudioManager.Instance == null) return false;
        if (AudioManager.Instance.gameObject == null) return false;

        // AudioManagerオブジェクト、またはその配下に付いているAudioSourceコンポーネントをすべて集める
        AudioSource[] sources = AudioManager.Instance.GetComponentsInChildren<AudioSource>(true);

        // もし1つもスピーカーが見つからない、あるいはすでに壊れている場合は「再生不可」とみなす
        if (sources == null || sources.Length == 0) return false;

        foreach (AudioSource source in sources)
        {
            // 配下のどれか1つでも既に破棄されている（Missing）状態があれば、危険信号として触らない
            // Unityの仕様上、オブジェクトが消滅しかけている時は型比較やnullチェックが特殊になるため、Equals(null)で検知
            if (source == null || source.Equals(null)) return false;
        }

        return true; // すべてのチェックをすり抜けたら、安全に音が鳴らせる状態
    }
}