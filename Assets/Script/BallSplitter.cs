using UnityEngine;

public class BallSplitter : MonoBehaviour
{
    public GameObject ballPrefab; // 複製するボールのプレハブをInspectorから設定
    public int numberOfClones = 1; // 元のボールに加えて、いくつ複製するか (1なら合計2つになる)
    public float splitAngleRange = 30f; // 複製されたボールが広がる角度の範囲（例: 30度なら元の進行方向から±15度）
    public float cloneSpeedMultiplier = 1.0f; // 複製されたボールの速度倍率

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 衝突した相手がボールかどうかをタグで確認
        if (collision.gameObject.CompareTag("BounceItem"))
        {
            // 元のボールのRigidbody2Dを取得
            Rigidbody2D originalBallRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (originalBallRb == null || ballPrefab == null)
            {
                Debug.LogError("元のボールのRigidbody2DまたはballPrefabが設定されていません。");
                return;
            }

            Vector2 originalVelocity = originalBallRb.velocity;
            Vector2 spawnPosition = collision.transform.position; // 衝突したボールの位置を基準に

            // 指定された数だけボールを複製
            for (int i = 0; i < numberOfClones; i++)
            {
                // 新しいボールを生成
                GameObject clonedBall = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
                Rigidbody2D clonedBallRb = clonedBall.GetComponent<Rigidbody2D>();

                if (clonedBallRb != null)
                {
                    // 複製されたボールの射出角度を計算
                    float angleOffset;
                    if (numberOfClones > 1) // 複数複製する場合、均等に広げるか、ランダム性を加える
                    {
                        // 角度を少しずらす（例：元の進行方向から左右に広がるように）
                        // numberOfClonesが1の場合は、元の進行方向に対して角度をつける
                        angleOffset = (i - (numberOfClones - 1) / 2.0f) * (splitAngleRange / Mathf.Max(1, numberOfClones - 1));
                        if (numberOfClones == 1) angleOffset = Random.Range(-splitAngleRange / 2f, splitAngleRange / 2f); // 1つだけ複製する場合はランダムなオフセット
                    }
                    else // numberOfClones が 0 (つまり、元のボールと合わせて1つだけ複製) の場合は単純に角度をつける
                    {
                        angleOffset = Random.Range(-splitAngleRange / 2f, splitAngleRange / 2f);
                    }


                    Quaternion rotation = Quaternion.Euler(0, 0, angleOffset);
                    Vector2 newVelocity = rotation * originalVelocity.normalized * originalVelocity.magnitude * cloneSpeedMultiplier;

                    clonedBallRb.velocity = newVelocity;
                }
            }

            // （オプション）元のボールの挙動も変更する場合
            // 例えば、元のボールの速度を少し変えるなど
            // float originalBallAngleOffset = Random.Range(-splitAngleRange / 2f, splitAngleRange / 2f);
            // Quaternion originalRotation = Quaternion.Euler(0, 0, originalBallAngleOffset);
            // originalBallRb.velocity = originalRotation * originalVelocity.normalized * originalVelocity.magnitude * cloneSpeedMultiplier;


            // この分裂アイテム自身を消滅させる
            Destroy(gameObject);
        }
    }

    // ギズモで分裂アイテムの範囲などを視覚化（任意）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // 例えば、分裂アイテムの位置からsplitAngleRangeの範囲を扇状に表示するなど
    }
}