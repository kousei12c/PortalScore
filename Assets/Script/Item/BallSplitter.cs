using UnityEngine;

public class BallSplitter : MonoBehaviour
{
    [Header("分裂設定")]
    public GameObject ballPrefab; // 複製するボールのプレハブをInspectorから設定
    public int numberOfClones = 1; // 元のボールに加えて、いくつ複製するか (1なら合計2つになる)
    public float splitAngleRange = 30f; // 複製されたボールが広がる角度の範囲（例: 30度なら元の進行方向から±15度）
    public float cloneSpeedMultiplier = 1.0f; // 複製されたボールの速度倍率

    [Header("デバッグ")]
    public bool enableDebugLogs = false; // デバッグログを有効にするかどうか

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 衝突した相手がボールかどうかをタグで確認
        if (collision.gameObject.CompareTag("BounceItem"))
        {
            if (enableDebugLogs)
            {
                Debug.Log($"BallSplitter: {collision.gameObject.name}と衝突しました");
            }

            // 元のボールのRigidbody2Dを取得
            Rigidbody2D originalBallRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (originalBallRb == null)
            {
                Debug.LogError("BallSplitter: 衝突したオブジェクトにRigidbody2Dが見つかりません: " + collision.gameObject.name);
                return;
            }

            if (ballPrefab == null)
            {
                Debug.LogError("BallSplitter: ballPrefabが設定されていません");
                return;
            }

            // 元のボールの情報を取得
            Vector2 originalVelocity = originalBallRb.linearVelocity;
            Vector2 spawnPosition = collision.transform.position;

            // 速度が0に近い場合は、デフォルトの方向を設定
            if (originalVelocity.magnitude < 0.1f)
            {
                originalVelocity = Vector2.right; // デフォルトで右方向
                if (enableDebugLogs)
                {
                    Debug.Log("BallSplitter: 元のボールの速度が小さいため、デフォルト方向を使用");
                }
            }

            // 指定された数だけボールを複製
            for (int i = 0; i < numberOfClones; i++)
            {
                // 新しいボールを生成（少し位置をずらして重複を避ける）
                Vector2 offsetPosition = spawnPosition + Random.insideUnitCircle * 0.1f;
                GameObject clonedBall = Instantiate(ballPrefab, offsetPosition, Quaternion.identity);

                Rigidbody2D clonedBallRb = clonedBall.GetComponent<Rigidbody2D>();
                if (clonedBallRb != null)
                {
                    // 複製されたボールの射出角度を計算
                    float angleOffset = CalculateAngleOffset(i);

                    // 角度を適用して新しい速度を計算
                    Vector2 direction = RotateVector2(originalVelocity.normalized, angleOffset);
                    Vector2 newVelocity = direction * originalVelocity.magnitude * cloneSpeedMultiplier;

                    clonedBallRb.linearVelocity = newVelocity;

                    if (enableDebugLogs)
                    {
                        Debug.Log($"BallSplitter: クローン{i + 1}を生成 - 角度オフセット: {angleOffset}度");
                    }
                }
                else
                {
                    Debug.LogError("BallSplitter: 生成されたボールにRigidbody2Dが見つかりません");
                }
            }

            // （オプション）元のボールの挙動も変更する場合
            // 元のボールも少し角度を変える
            if (numberOfClones > 0)
            {
                float originalBallAngleOffset = Random.Range(-splitAngleRange / 4f, splitAngleRange / 4f);
                Vector2 originalDirection = RotateVector2(originalVelocity.normalized, originalBallAngleOffset);
                originalBallRb.linearVelocity = originalDirection * originalVelocity.magnitude * cloneSpeedMultiplier;
            }

            // この分裂アイテム自身を消滅させる
            if (enableDebugLogs)
            {
                Debug.Log("BallSplitter: 分裂アイテムを破壊します");
            }
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
    void Start()
    {
        ValidateSettings();
    }

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