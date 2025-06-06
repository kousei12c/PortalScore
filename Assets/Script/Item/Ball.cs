using UnityEngine;

/// <summary>
/// ボール（BouncItem）の基本的な物理挙動を管理します。
/// アイテムごとの個別の処理は、各アイテム側のスクリプトに移管されました。
/// </summary>
public class Ball : MonoBehaviour
{
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2Dが見つかりません。", gameObject);
            return;
        }

        // 回転を止める（これはボール自体の特性）
        rb.freezeRotation = true;
    }

    // OnCollisionEnter2Dには、ボール自身が反応すべき物理衝突（壁など）の
    // 処理があれば記述します。アイテムごとの処理は不要になりました。
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 例: if (collision.gameObject.CompareTag("Wall")) { ... }
    }

    // OnTriggerEnter2Dには、ボール自身が反応すべきトリガー接触の
    // 処理があれば記述します。アイテムごとの処理は不要になりました。
    void OnTriggerEnter2D(Collider2D other)
    {
        // 例: if (other.CompareTag("Goal")) { ... }
    }
}
