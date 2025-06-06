using UnityEngine;

/// <summary>
/// ボールに接触した際にボールを加速させ、自身は消滅するアイテム。
/// このスクリプトをSpeedBoostアイテムのプレハブにアタッチしてください。
/// </summary>
[RequireComponent(typeof(Collider2D))] // このコンポーネントにはCollider2Dが必須です
public class SpeedBooster : MonoBehaviour
{
    [Tooltip("ボールの速度に乗算する倍率")]
    public float speedMultiplier = 1.5f;

    [Tooltip("ボールを識別するためのタグ")]
    public string ballTag = "Ball"; // あなたのボールのタグ名に合わせてください

    /// <summary>
    /// このコンポーネントがオブジェクトに初めてアタッチされた時に自動で呼ばれます。
    /// Collider2Dを自動的にTriggerに設定し、設定の手間を省きます。
    /// </summary>

    /// <summary>
    /// 他のColliderがこのオブジェクトのトリガーゾーンに入ってきた時に呼ばれます。
    /// </summary>
    /// <param name="other">接触した相手のCollider2D</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 接触した相手がボールかどうかをタグで確認します
        if (other.CompareTag(ballTag))
        {
            // ボールのRigidbody2Dを取得します
            Rigidbody2D ballRb = other.GetComponent<Rigidbody2D>();
            if (ballRb != null)
            {
                // ボールの現在の速度を取得し、倍率をかけて再設定します
                ballRb.linearVelocity *= speedMultiplier;
                Debug.Log($"{other.name}の速度が{speedMultiplier}倍になりました。");

                // このSpeedBoostオブジェクト自身を消滅させます
                Destroy(gameObject);
            }
        }
    }
}
