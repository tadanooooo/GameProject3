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

    // 物理判定がONに戻ったときにトリガーを強制的に再起動して目覚めさせる安全弁
    private void OnEnable()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            col.enabled = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trash"))
        {
            // 二重処理防止
            other.tag = "Untagged";

            // メインゲームのGameManagerへの報告
            if (GameManager.instance != null)
            {
                GameManager.instance.TrashCollected();
            }

            // チュートリアル中であれば、吸い込み開始の瞬間に残り数を減らす（ラグ解消）
            if (TutorialManager.instance != null)
            {
                TutorialManager.instance.NotifyTrashCollected();
            }

            // 吸い込みアニメーション開始（コルーチンを呼び出す）
            StartCoroutine(ShrinkAndDestroy(other.gameObject));
        }
    }

    // 徐々に小さくして消す魔法の処理
    IEnumerator ShrinkAndDestroy(GameObject trash)
    {
        // 吸い込み中ループ音
        activeSuctionCount++;
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
            if (suctionPoint != null)
            {
                trash.transform.position = Vector3.MoveTowards(
                    trash.transform.position,
                    suctionPoint.position,
                    0.1f
                );
            }

            trash.transform.localScale = Vector3.Lerp(
                trash.transform.localScale,
                Vector3.zero,
                Time.deltaTime * shrinkSpeed
            );

            yield return null; // 1フレーム待機
        }

        // ゴミが消える瞬間の位置を記録、高さ調整
        Vector3 effectPosition = transform.position;
        if (suctionPoint != null)
        {
            effectPosition = suctionPoint.position;
        }
        else if (trash != null)
        {
            effectPosition = trash.transform.position;
        }

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

        activeSuctionCount--;

        if (activeSuctionCount <= 0)
        {
            activeSuctionCount = 0;

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

    private void ForceStopSuctionSound()
    {
        activeSuctionCount = 0;

        if (IsAudioPlayable())
        {
            try
            {
                AudioManager.Instance.StopSuctionAndPlayEndSE(3);
            }
            catch (System.Exception)
            {
            }
        }
    }

    private bool IsAudioPlayable()
    {
        if (AudioManager.Instance == null) return false;
        if (AudioManager.Instance.gameObject == null) return false;

        AudioSource[] sources = AudioManager.Instance.GetComponentsInChildren<AudioSource>(true);
        if (sources == null || sources.Length == 0) return false;

        foreach (AudioSource source in sources)
        {
            if (source == null || source.Equals(null)) return false;
        }

        return true;
    }
}