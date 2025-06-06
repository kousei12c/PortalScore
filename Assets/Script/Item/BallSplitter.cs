using UnityEngine;

public class BallSplitter : MonoBehaviour
{
    [Header("分裂設定")]
    public GameObject ballPrefab;
    public int numberOfClones = 1;
    public float splitAngleRange = 30f;
    public float cloneSpeedMultiplier = 1.0f;

    [Header("デバッグ")]
    public bool enableDebugLogs = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ValidateSettings();
    }

    // ★★★ OnCollisionEnter2D から OnTriggerEnter2D に変更 ★★★
    void OnTriggerEnter2D(Collider2D other)
    {
        // 接触した相手がボールかどうかをタグで確認
        if (other.CompareTag("Ball"))
        {
            if (enableDebugLogs)
            {
                Debug.Log($"BallSplitter: {other.name}がトリガーゾーンに入りました");
            }

            Rigidbody2D originalBallRb = other.GetComponent<Rigidbody2D>();
            if (originalBallRb == null)
            {
                Debug.LogError("BallSplitter: 接触したオブジェクトにRigidbody2Dが見つかりません: " + other.name);
                return;
            }

            if (ballPrefab == null)
            {
                Debug.LogError("BallSplitter: ballPrefabが設定されていません");
                return;
            }

            // 元のボールの情報を取得
            Vector2 originalVelocity = originalBallRb.linearVelocity; // Rigidbody2Dのプロパティ名をvelocityに修正
            Vector2 spawnPosition = other.transform.position;

            if (originalVelocity.magnitude < 0.1f)
            {
                originalVelocity = Vector2.right;
                if (enableDebugLogs) Debug.Log("BallSplitter: 元のボールの速度が小さいため、デフォルト方向を使用");
            }

            // 指定された数だけボールを複製
            for (int i = 0; i < numberOfClones; i++)
            {
                Vector2 offsetPosition = spawnPosition + Random.insideUnitCircle * 0.1f;
                GameObject clonedBall = Instantiate(ballPrefab, offsetPosition, Quaternion.identity);

                Rigidbody2D clonedBallRb = clonedBall.GetComponent<Rigidbody2D>();
                if (clonedBallRb != null)
                {
                    float angleOffset = CalculateAngleOffset(i);
                    Vector2 direction = RotateVector2(originalVelocity.normalized, angleOffset);
                    Vector2 newVelocity = direction * originalVelocity.magnitude * cloneSpeedMultiplier;

                    clonedBallRb.linearVelocity = newVelocity; // Rigidbody2Dのプロパティ名をvelocityに修正

                    if (enableDebugLogs) Debug.Log($"BallSplitter: クローン{i + 1}を生成 - 角度オフセット: {angleOffset}度");
                }
                else
                {
                    Debug.LogError("BallSplitter: 生成されたボールにRigidbody2Dが見つかりません");
                }
            }

            // 元のボールの挙動も変更
            if (numberOfClones > 0)
            {
                float originalBallAngleOffset = Random.Range(-splitAngleRange / 4f, splitAngleRange / 4f);
                Vector2 originalDirection = RotateVector2(originalVelocity.normalized, originalBallAngleOffset);
                originalBallRb.linearVelocity = originalDirection * originalVelocity.magnitude * cloneSpeedMultiplier; // Rigidbody2Dのプロパティ名をvelocityに修正
            }

            if (enableDebugLogs) Debug.Log("BallSplitter: 分裂アイテムを破壊します");
            Destroy(gameObject);
        }
    }


    // 角度オフセットを計算する関数
    private float CalculateAngleOffset(int cloneIndex)
    {
        if (numberOfClones == 1)
        {
            // 1つだけ複製する場合はランダムなオフセット
            return Random.Range(-splitAngleRange / 2f, splitAngleRange / 2f);
        }
        else if (numberOfClones > 1)
        {
            // 複数複製する場合、均等に広げる
            float step = splitAngleRange / (numberOfClones - 1);
            return (cloneIndex * step) - (splitAngleRange / 2f);
        }

        return 0f;
    }

    // Vector2を指定した角度（度）で回転させる関数
    private Vector2 RotateVector2(Vector2 vector, float angleDegrees)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRadians);
        float sin = Mathf.Sin(angleRadians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    // 設定の検証

    private void ValidateSettings()
    {
        if (ballPrefab == null)
        {
            Debug.LogWarning("BallSplitter: ballPrefabが設定されていません");
        }

        if (numberOfClones < 0)
        {
            numberOfClones = 0;
            Debug.LogWarning("BallSplitter: numberOfClonesは0以上である必要があります");
        }

        if (splitAngleRange < 0)
        {
            splitAngleRange = 0;
            Debug.LogWarning("BallSplitter: splitAngleRangeは0以上である必要があります");
        }

        if (cloneSpeedMultiplier <= 0)
        {
            cloneSpeedMultiplier = 1.0f;
            Debug.LogWarning("BallSplitter: cloneSpeedMultiplierは0より大きい値である必要があります");
        }
    }

    // ギズモで分裂アイテムの範囲などを視覚化
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // 分裂角度の範囲を表示
        if (splitAngleRange > 0)
        {
            Gizmos.color = Color.red;
            Vector3 forward = Vector3.right;

            // 分裂範囲の境界線を描画
            Vector3 leftBoundary = Quaternion.Euler(0, 0, splitAngleRange / 2f) * forward;
            Vector3 rightBoundary = Quaternion.Euler(0, 0, -splitAngleRange / 2f) * forward;

            Gizmos.DrawRay(transform.position, leftBoundary);
            Gizmos.DrawRay(transform.position, rightBoundary);
        }
    }
}