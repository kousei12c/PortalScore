using UnityEngine;

public class SuckableItem : MonoBehaviour
{
    public float suckSpeed = 5f;            // 吸い込む速度
    private string gridAreaTag = "GridArea";   // グリッド場のタグ
    private string itemBoxDropZoneTag = "ItemBoxDropZone"; // ItemBoxの真上（セット場所）のタグ
    public float arrivalThreshold = 0.1f;  // ItemBoxに到達したとみなす距離

    private Transform itemBoxTransform;     // ItemBoxのTransform
    private Rigidbody2D rb;
    private bool isOnGrid = false;          // アイテムがグリッド上にいるか
    private bool isInDropZone = false;     // アイテムがItemBoxのドロップゾーンにいるか
    private bool isBeingSucked = false;    // 吸い込み中か

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject itemBoxObj = GameObject.Find("ItemBox"); // シーンからItemBoxを名前で検索
        if (itemBoxObj != null)
        {
            itemBoxTransform = itemBoxObj.transform;
        }
        else
        {
            Debug.LogError("ItemBoxが見つかりません。シーンにItemBoxという名前のオブジェクトを配置してください。");
            enabled = false; // スクリプトを無効化
            return;
        }

        // ItemBoxが見つからなければ、このアイテムは吸い込み対象外とするか、エラーを出す
        if (itemBoxTransform == null)
        {
            Debug.LogWarning("吸い込み先のItemBoxが設定されていません。このアイテムは吸い込まれません。");
        }
    }

    void FixedUpdate() // Rigidbodyを操作する場合はFixedUpdateが推奨
    {
        if (itemBoxTransform == null) return; // ItemBoxがなければ何もしない

        if (isOnGrid) // グリッド上にいる場合
        {
            isBeingSucked = false;
            // グリッド上では吸い込まれず、物理挙動も停止させる（または抵抗を増やすなど）
            if (rb != null && !rb.isKinematic) // isKinematicでない場合のみ設定
            {
                rb.velocity = Vector2.zero; // 念のため速度をゼロに
                rb.isKinematic = true;      // 物理演算の影響を一時的に受けなくする
            }
            return; // グリッド上にいれば、以降の吸い込み処理はしない
        }
        else
        {
            // グリッド外に出たら物理演算を再開
            if (rb != null && rb.isKinematic)
            {
                rb.isKinematic = false;
            }
        }

        // isInDropZone (ItemBoxの真上、セット場所) にいる場合は吸い込まない
        if (isInDropZone)
        {
            isBeingSucked = false;
            // ここでは自由落下に任せる（あるいは特定の静止処理）
            return;
        }

        // 上記の条件（グリッド上でもなく、ドロップゾーンでもない）を満たせば吸い込む
        isBeingSucked = true;
        Vector2 direction = ((Vector3)itemBoxTransform.position - transform.position).normalized;
        if (rb != null)
        {
            rb.velocity = direction * suckSpeed;
        }

        // ItemBoxに到達したかチェック
        if (Vector3.Distance(transform.position, itemBoxTransform.position) < arrivalThreshold)
        {
            Debug.Log(gameObject.name + " が ItemBox に到達しました！");
            // ここでアイテムを「セット」する処理（例：消滅させる、ItemBoxに格納するアニメーションなど）
            
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(gridAreaTag))
        {
            isOnGrid = true;
        }
        else if (other.CompareTag(itemBoxDropZoneTag)) // ItemBoxの「セットする場所」のトリガー
        {
            isInDropZone = true;
            if (rb != null)
            {
                // ドロップゾーンに入ったら、例えばゆっくり着地させる、速度を落とすなど
                rb.velocity *= 0.1f; // 急に止まると不自然なので少し速度を残すなど
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(gridAreaTag))
        {
            isOnGrid = false;
        }
        else if (other.CompareTag(itemBoxDropZoneTag))
        {
            isInDropZone = false;
        }
    }
}