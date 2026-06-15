using UnityEngine;

// タイトル画面のプレイヤー専用：動いてゴミを吸い込むだけのスクリプト
[RequireComponent(typeof(Rigidbody))]
public class TitleScenePlayer : MonoBehaviour
{
    [Header("動きの設定")]
    public float moveSpeed = 3.0f; // 移動スピード
    public float changeDirectionTime = 3.0f; // 方向転換する時間（秒）

    [Header("吸い込むゴミのタグ名")]
    public string trashTag = "Trash";

    [Header("壁（障害物）のタグ名")]
    public string wallTag = "Wall";
    private Rigidbody rb;
    private float timer;
    private Vector2 randomDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false; // 物理演算をオン
        var gyroScript = GetComponent("GyroPlayer");
        if (gyroScript != null) (gyroScript as MonoBehaviour).enabled = false;

        timer = changeDirectionTime;
        SetRandomDirection();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SetRandomDirection();
            timer = changeDirectionTime;
        }
    }

    void FixedUpdate()
    {
        // 勝手にウロウロ動く
        rb.linearVelocity = new Vector3(randomDirection.x * moveSpeed, rb.linearVelocity.y, randomDirection.y * moveSpeed);

        if (randomDirection != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(randomDirection.x, randomDirection.y) * Mathf.Rad2Deg;
            rb.rotation = Quaternion.Euler(0, targetAngle, 0);
        }
    }

    // ランダムな移動方向を決める
    void SetRandomDirection()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);
        randomDirection = new Vector2(x, y).normalized;
        // たまに立ち止まる（動きを自然に）
        if (Random.Range(0, 10) > 7) randomDirection = Vector2.zero;
    }

    // 壁にぶつかったときに180度反転
    private void OnCollisionEnter(Collision collision)
    {
        // ぶつかった相手が壁（または設定したタグ）だった場合
        // ※もし壁にタグをつけていない場合は、collision.gameObject.name.Contains("Wall") などにするか、
        // 単に collision.gameObject.CompareTag("Player") 以外なら全部反転、としてもOKです。
        if (collision.gameObject.CompareTag(wallTag) || collision.gameObject.name.Contains("Wall"))
        {
            // ぶつかった壁の面の向き（法線）を取得
            Vector3 hitNormal = collision.contacts[0].normal;

            // 3Dの向きから2D（X, Z）の向きを取り出す
            Vector2 normal2D = new Vector2(hitNormal.x, hitNormal.z).normalized;

            // 今の進行方向を、壁の向きに合わせて完全に跳ね返らせる（反射ベクトルを計算）
            Vector2 currentDir = randomDirection;
            randomDirection = Vector2.Reflect(currentDir, normal2D).normalized;

            // もし完全に真逆に戻したい場合や、反射がうまくいかない場合はこちらでも代用可能：
            // randomDirection = -randomDirection;

            // 方向転換タイマーをリセットして、すぐまた別の壁に連続でぶつかるのを防ぐ
            timer = changeDirectionTime;

            // 跳ね返った瞬間にすぐに新しい方向を向かせる
            if (randomDirection != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(randomDirection.x, randomDirection.y) * Mathf.Rad2Deg;
                rb.rotation = Quaternion.Euler(0, targetAngle, 0);
            }

            Debug.Log("壁にぶつかったので反転");
        }
    }

    // 吸い込み処理（トリガー判定で消す）
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(trashTag))
        {
            Destroy(other.gameObject); // ゴミを消す（吸い込んだ演出）
        }
    }
}