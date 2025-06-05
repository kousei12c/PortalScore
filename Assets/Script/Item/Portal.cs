using UnityEngine;

public class Portal : MonoBehaviour
{
    public Transform targetPortal; // 対になるポータルのTransform
    public Transform spawnPoint;   // このポータルからボールが射出される場合の基準点

    private bool isTeleporting = false; // テレレポート処理中フラグ（再帰ワープ防止用）

    private void OnTriggerEnter2D(Collider2D other)
    {
        // テレレポート処理中でなく、相手のポータルが設定されている場合
        if (!isTeleporting && targetPortal != null)
        {
            // 接触したのがボールかどうかをタグで判別（例："Ball"タグ）
            if (other.CompareTag("BounceItem"))
            {
                Portal targetPortalScript = targetPortal.GetComponent<Portal>();
                if (targetPortalScript != null && targetPortalScript.spawnPoint != null)
                {
                    // 相手ポータルを一時的にテレレポート処理中にする
                    targetPortalScript.StartTeleportCooldown();

                    // ボールをターゲットポータルの射出ポイントに移動
                    other.transform.position = targetPortalScript.spawnPoint.position;

                    // ボールの速度を維持しつつ、射出方向を調整する場合 (オプション)
                    Rigidbody2D ballRb = other.GetComponent<Rigidbody2D>();
                    if (ballRb != null)
                    {
                        // 出口ポータルの向きに合わせて速度ベクトルを回転させる例
                        // この部分はゲームの仕様に合わせて調整が必要です
                        Vector2 incomingVelocity = ballRb.linearVelocity;
                        Quaternion rotationDifference = targetPortalScript.spawnPoint.rotation * Quaternion.Inverse(spawnPoint.rotation); // 入口と出口の向きの違い
                        ballRb.linearVelocity = rotationDifference * incomingVelocity;

                        // もし単純に射出ポイントの向きに特定の速度で射出したい場合
                        // float speed = incomingVelocity.magnitude;
                        // ballRb.velocity = targetPortalScript.spawnPoint.right * speed; // spawnPointが右向きを射出方向とする場合
                    }
                    Debug.Log(other.name + " teleported to " + targetPortal.name);
                }
            }
        }
    }

    // テレレポート直後の再トリガーを防ぐための処理開始
    public void StartTeleportCooldown()
    {
        isTeleporting = true;
        // 0.1秒後にフラグを戻す（この時間は調整可能）
        Invoke(nameof(EndTeleportCooldown), 0.02f);
    }

    // テレレポート処理中フラグを解除
    private void EndTeleportCooldown()
    {
        isTeleporting = false;
    }

    // ギズモで射出ポイントの向きを可視化（シーンビューで確認しやすくするため）
    void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + spawnPoint.right * 1.0f); // spawnPointの右向きを可視化
        }
        if (targetPortal != null && spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(spawnPoint.position, targetPortal.GetComponent<Portal>().spawnPoint.position);
        }
    }
}