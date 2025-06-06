using UnityEngine;

/// <summary>
/// GridAreaに入った時にColliderのisTriggerを制御するコンポーネント
/// BounceItemとの当たり判定を無効化したいアイテムにアタッチして使用
/// </summary>
public class GridCollisionController : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private string gridAreaTag = "GridArea";
    [SerializeField] private bool enableDebugLogs = true;

    [Header("制御対象のCollider")]
    [SerializeField] private Collider2D targetCollider;
    [SerializeField] private bool autoFindCollider = true;

    [Header("動作設定")]
    [SerializeField] private bool triggerOnInGridArea = true;  // GridArea内でTriggerにするか
    [SerializeField] private bool triggerOnOutGridArea = false; // GridArea外でTriggerにするか

    // 状態管理
    private bool isInGridArea = false;
    private bool originalIsTrigger;
    private bool hasStoredOriginalValue = false;

    void Start()
    {
        // Colliderの自動検索
        if (autoFindCollider && targetCollider == null)
        {
            targetCollider = GetComponent<Collider2D>();
        }

        // エラーチェック
        if (targetCollider == null)
        {
            Debug.LogError($"GridCollisionController: {gameObject.name}にCollider2Dが見つかりません", gameObject);
            enabled = false;
            return;
        }

        // 初期状態を保存
        StoreOriginalTriggerState();

        // 開始時の状態をチェック
        CheckInitialGridAreaStatus();

        if (enableDebugLogs)
        {
            Debug.Log($"GridCollisionController初期化完了: {gameObject.name}, " +
                     $"対象Collider: {targetCollider.name}, " +
                     $"初期isTrigger: {originalIsTrigger}");
        }
    }

    void Update()
    {
        // 現在のGridArea状態をチェック
        CheckGridAreaStatus();
    }

    /// <summary>
    /// 元のisTrigger状態を保存
    /// </summary>
    void StoreOriginalTriggerState()
    {
        if (!hasStoredOriginalValue)
        {
            originalIsTrigger = targetCollider.isTrigger;
            hasStoredOriginalValue = true;
        }
    }

    /// <summary>
    /// 開始時のGridArea状態をチェック
    /// </summary>
    void CheckInitialGridAreaStatus()
    {
        Vector2 checkPosition = (Vector2)transform.position;
        if (targetCollider != null)
        {
            checkPosition += targetCollider.offset;
        }

        Collider2D[] hitColliders = Physics2D.OverlapPointAll(checkPosition);
        bool foundGridArea = false;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(gridAreaTag))
            {
                foundGridArea = true;
                break;
            }
        }

        isInGridArea = foundGridArea;
        ApplyTriggerState();
    }

    /// <summary>
    /// GridArea内にいるかどうかをチェック
    /// </summary>
    void CheckGridAreaStatus()
    {
        Vector2 checkPosition = (Vector2)transform.position;
        if (targetCollider != null)
        {
            checkPosition += targetCollider.offset;
        }

        Collider2D[] hitColliders = Physics2D.OverlapPointAll(checkPosition);
        bool foundGridArea = false;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(gridAreaTag))
            {
                foundGridArea = true;
                break;
            }
        }

        // 状態が変化した場合のみ処理
        if (isInGridArea != foundGridArea)
        {
            bool wasInGridArea = isInGridArea;
            isInGridArea = foundGridArea;

            if (enableDebugLogs)
            {
                Debug.Log($"{gameObject.name}: GridArea状態変化 - " +
                         $"前: {wasInGridArea}, 現在: {isInGridArea}");
            }

            ApplyTriggerState();
        }
    }

    /// <summary>
    /// 現在の状態に応じてisTriggerを設定
    /// </summary>
    void ApplyTriggerState()
    {
        if (targetCollider == null) return;

        bool shouldBeTrigger;

        if (isInGridArea)
        {
            shouldBeTrigger = triggerOnInGridArea;
        }
        else
        {
            shouldBeTrigger = triggerOnOutGridArea;
        }

        // 状態が変化する場合のみ適用
        if (targetCollider.isTrigger != shouldBeTrigger)
        {
            targetCollider.isTrigger = shouldBeTrigger;

            if (enableDebugLogs)
            {
                Debug.Log($"{gameObject.name}: isTriggerを{shouldBeTrigger}に変更 " +
                         $"(GridArea内: {isInGridArea})");
            }
        }
    }

    /// <summary>
    /// 元の状態に戻す
    /// </summary>
    public void RestoreOriginalTriggerState()
    {
        if (targetCollider != null && hasStoredOriginalValue)
        {
            targetCollider.isTrigger = originalIsTrigger;

            if (enableDebugLogs)
            {
                Debug.Log($"{gameObject.name}: isTriggerを元の状態({originalIsTrigger})に復元");
            }
        }
    }

    /// <summary>
    /// isTrigger状態を強制設定
    /// </summary>
    public void SetTriggerState(bool isTrigger)
    {
        if (targetCollider != null)
        {
            targetCollider.isTrigger = isTrigger;

            if (enableDebugLogs)
            {
                Debug.Log($"{gameObject.name}: isTriggerを手動で{isTrigger}に設定");
            }
        }
    }

    /// <summary>
    /// 現在GridArea内にいるかどうかを取得
    /// </summary>
    public bool IsInGridArea()
    {
        return isInGridArea;
    }

    /// <summary>
    /// 対象Colliderの現在のisTrigger状態を取得
    /// </summary>
    public bool GetCurrentTriggerState()
    {
        return targetCollider != null ? targetCollider.isTrigger : false;
    }

    /// <summary>
    /// 元のisTrigger状態を取得
    /// </summary>
    public bool GetOriginalTriggerState()
    {
        return originalIsTrigger;
    }

    // コンポーネントが無効化される時に元の状態に戻す
    void OnDisable()
    {
        RestoreOriginalTriggerState();
    }

    // オブジェクトが破棄される時に元の状態に戻す
    void OnDestroy()
    {
        RestoreOriginalTriggerState();
    }

    // デバッグ用の手動チェック
    [ContextMenu("GridArea状態を手動チェック")]
    public void ManualCheckGridAreaStatus()
    {
        CheckGridAreaStatus();
        Debug.Log($"手動チェック結果 - GridArea内: {isInGridArea}, " +
                 $"現在のisTrigger: {GetCurrentTriggerState()}");
    }

    // デバッグ用の状態リセット
    [ContextMenu("元の状態に復元")]
    public void ManualRestoreOriginalState()
    {
        RestoreOriginalTriggerState();
    }

    // Inspector上での設定変更時に即座に反映
    void OnValidate()
    {
        if (Application.isPlaying && targetCollider != null)
        {
            ApplyTriggerState();
        }
    }

    // デバッグ用のギズモ表示
    void OnDrawGizmosSelected()
    {
        if (targetCollider != null)
        {
            // GridArea内かどうかで色を変える
            Gizmos.color = isInGridArea ? Color.green : Color.red;

            Vector3 center = transform.position + (Vector3)targetCollider.offset;
            Vector3 size = targetCollider.bounds.size;

            Gizmos.DrawWireCube(center, size);

            // isTrigger状態も表示
            if (targetCollider.isTrigger)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(center, size * 0.1f);
            }
        }
    }
}