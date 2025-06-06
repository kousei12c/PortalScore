using UnityEngine;

public class ControllableItem : MonoBehaviour
{
    [Header("基本設定")]
    public float suckSpeed = 5f;
    public string gridAreaTag = "GridArea";
    public string itemBoxZoneTag = "ItemBoxZone";

    [Header("レイヤー設定")]
    [SerializeField] private LayerMask mouseDetectionLayers = ~0; // すべてのレイヤー（デフォルト）
    [SerializeField] private bool useSpecificLayer = true;
    [SerializeField] private string targetLayerName = "DraggableItems";

    [Header("デバッグ用")]
    public bool enableDebugLogs = true;
    public bool showRaycastDebug = false;
    public bool showLayerInfo = true;

    private Transform itemBoxTransform;
    private Rigidbody2D rb;
    private Camera mainCamera;
    private GridManager gridManager;

    private bool isPlacedOnGrid = false;     // グリッドに正確に配置されているか
    private bool isInGridArea = false;       // グリッドエリア内にいるか
    private bool isInItemBoxZone = false;
    private bool isDragging = false;
    private Vector3 offset;
    private Quaternion initialRotation;

    // マウス検出用
    private Collider2D itemCollider;
    private bool isMouseOver = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) { Debug.LogError("Rigidbody2Dなし", gameObject); enabled = false; return; }

        itemCollider = GetComponent<Collider2D>();
        if (itemCollider == null) { Debug.LogError("Collider2Dなし", gameObject); enabled = false; return; }

        mainCamera = Camera.main;
        if (mainCamera == null) { Debug.LogError("MainCameraなし", gameObject); enabled = false; return; }

        gridManager = FindFirstObjectByType<GridManager>();

        GameObject itemBoxObj = GameObject.Find("ItemBox");
        if (itemBoxObj != null) { itemBoxTransform = itemBoxObj.transform; }
        else { Debug.LogWarning("ItemBox目標なし"); }

        rb.bodyType = RigidbodyType2D.Dynamic;
        initialRotation = transform.rotation;

        // レイヤー設定の初期化
        InitializeLayerSettings();

        CheckCurrentZoneStatus(true);

        // Collider2Dの設定を確認・調整
        if (itemCollider.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: Collider2DがTriggerに設定されています。マウス検出に影響する可能性があります。");
        }
    }

    void InitializeLayerSettings()
    {
        if (useSpecificLayer)
        {
            int layerIndex = LayerMask.NameToLayer(targetLayerName);
            if (layerIndex != -1)
            {
                mouseDetectionLayers = 1 << layerIndex;
                if (enableDebugLogs && showLayerInfo)
                {
                    Debug.Log($"マウス検出レイヤーを'{targetLayerName}'（レイヤー{layerIndex}）に設定しました");
                }

                // オブジェクトのレイヤーも自動的に設定するかチェック
                if (gameObject.layer != layerIndex)
                {
                    if (enableDebugLogs && showLayerInfo)
                    {
                        Debug.LogWarning($"{gameObject.name}: オブジェクトのレイヤーが'{targetLayerName}'ではありません。" +
                                       $"現在のレイヤー: {LayerMask.LayerToName(gameObject.layer)}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"レイヤー'{targetLayerName}'が見つかりません。デフォルト設定を使用します。");
                mouseDetectionLayers = ~0; // すべてのレイヤー
            }
        }

        // レイヤー設定の確認
        if (enableDebugLogs && showLayerInfo)
        {
            CheckLayerSettings();
        }
    }

    void Update()
    {
        // ゾーン状態を常に更新
        UpdateZoneStatus();

        // 独自のマウス検出システム
        HandleMouseInput();

        if (isDragging)
        {
            HandleDragging();
            return;
        }

        HandlePhysicsState();
    }

    // ゾーン状態を常に更新
    void UpdateZoneStatus()
    {
        if (isDragging) return;

        // グリッドエリア内かどうかをチェック
        Collider2D[] hits = Physics2D.OverlapPointAll(transform.position);
        bool wasInGridArea = isInGridArea;
        isInGridArea = false;
        isInItemBoxZone = false;

        foreach (var hit in hits)
        {
            if (hit.CompareTag(gridAreaTag))
            {
                isInGridArea = true;
            }
            if (hit.CompareTag(itemBoxZoneTag))
            {
                isInItemBoxZone = true;
            }
        }

        // グリッドエリアから出た場合、配置状態を解除
        if (wasInGridArea && !isInGridArea && isPlacedOnGrid)
        {
            isPlacedOnGrid = false;
            if (enableDebugLogs)
            {
                Debug.Log($"{gameObject.name} グリッドエリアから出たため配置状態を解除");
            }
        }

        // グリッドに配置されているかどうかを判定（グリッドエリア内でスナップされた位置にいるか）
        if (isInGridArea && gridManager != null)
        {
            Vector2Int gridPos = gridManager.WorldToGridPosition(transform.position);
            Vector3 snapPos = gridManager.GridToWorldPosition(gridPos);
            float distance = Vector3.Distance(transform.position, snapPos);

            // スナップ位置に十分近い場合は配置されていると判定
            if (distance < 0.1f && !isDragging)
            {
                if (!isPlacedOnGrid)
                {
                    isPlacedOnGrid = true;
                    if (enableDebugLogs)
                    {
                        Debug.Log($"{gameObject.name} グリッドに配置されました");
                    }
                }
            }
            else if (isPlacedOnGrid && distance > 0.5f)
            {
                // スナップ位置から離れすぎた場合は配置状態を解除
                isPlacedOnGrid = false;
                if (enableDebugLogs)
                {
                    Debug.Log($"{gameObject.name} グリッド配置から外れました");
                }
            }
        }
    }

    // 重なり問題を解決：最前面のアイテムのみ選択可能
    void HandleMouseInput()
    {
        Vector3 mouseWorldPos = GetMouseWorldPos();

        // クリック位置に重なる全てのColliderを取得
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPos, mouseDetectionLayers);

        // 最前面(ソート順最大 or Z値最小)のアイテムを判定
        Collider2D topCollider = null;
        int topOrder = int.MinValue;
        float topZ = float.MaxValue;

        foreach (var hit in hits)
        {
            // ControllableItemコンポーネントを持つオブジェクトのみ対象
            if (hit.GetComponent<ControllableItem>() != null)
            {
                int order = GetSortingOrder(hit);
                float z = hit.transform.position.z;

                if (order > topOrder || (order == topOrder && z < topZ))
                {
                    topOrder = order;
                    topZ = z;
                    topCollider = hit;
                }
            }
        }

        // 自分が最前面かどうか判定
        isMouseOver = (topCollider == itemCollider);

        if (showRaycastDebug)
        {
            if (isMouseOver)
            {
                Debug.DrawRay(mouseWorldPos, Vector3.forward * 0.1f, Color.green, 0.1f);
                if (enableDebugLogs)
                {
                    Debug.Log($"最前面のアイテム: {gameObject.name}");
                }
            }
            else
            {
                Debug.DrawRay(mouseWorldPos, Vector3.forward * 0.1f, Color.red, 0.1f);
            }
        }

        // マウスクリック処理
        if (Input.GetMouseButtonDown(0) && isMouseOver && !isDragging)
        {
            StartDragging(mouseWorldPos);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            StopDragging();
        }
    }

    // SortingOrder取得（SpriteRendererがなければZ値で判定）
    int GetSortingOrder(Collider2D col)
    {
        SpriteRenderer sr = col.GetComponent<SpriteRenderer>();
        if (sr != null)
            return sr.sortingOrder;
        // なければZ座標をintに変換（より手前＝値が小さい方を大きいとみなす）
        return Mathf.RoundToInt(-col.transform.position.z * 1000);
    }

    void StartDragging(Vector3 mouseWorldPos)
    {
        isDragging = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.None;

        offset = transform.position - mouseWorldPos;

        // ドラッグ開始時に配置状態を解除
        isPlacedOnGrid = false;

        if (enableDebugLogs)
        {
            Debug.Log($"ドラッグ開始: {gameObject.name}");
        }
    }

    void HandleDragging()
    {
        Vector3 mousePos = GetMouseWorldPos() + offset;
        rb.MovePosition(new Vector2(mousePos.x, mousePos.y));
    }

    // グリッドにはドラッグ＆ドロップ時のみスナップ
    void StopDragging()
    {
        if (!isDragging) return;

        isDragging = false;

        if (enableDebugLogs)
        {
            Debug.Log($"ドラッグ終了: {gameObject.name}");
        }

        // ドラッグ終了時のみグリッドスナップ判定
        if (isInGridArea && gridManager != null)
        {
            ApplyGridSnapping();
        }
        else
        {
            ApplyNormalPhysics();
        }
    }

    void ApplyGridSnapping()
    {
        Vector2Int gridPos = gridManager.WorldToGridPosition(transform.position);
        Vector3 snapPos = gridManager.GridToWorldPosition(gridPos);
        transform.position = new Vector3(snapPos.x, snapPos.y, transform.position.z);
        transform.rotation = initialRotation;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // グリッド配置状態を設定
        isPlacedOnGrid = true;

        if (enableDebugLogs)
        {
            Debug.Log($"{gameObject.name} グリッドにスナップしました");
        }
    }

    void ApplyNormalPhysics()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.angularVelocity = 0f;

        // グリッド配置状態をクリア
        isPlacedOnGrid = false;

        if (enableDebugLogs)
        {
            Debug.Log($"{gameObject.name} 通常の物理演算に戻しました");
        }
    }

    // 修正された物理状態管理
    void HandlePhysicsState()
    {
        // 1. グリッドに配置されている場合：完全に静止
        if (isPlacedOnGrid)
        {
            if (rb.bodyType != RigidbodyType2D.Kinematic) rb.bodyType = RigidbodyType2D.Kinematic;
            if (rb.linearVelocity != Vector2.zero) rb.linearVelocity = Vector2.zero;
            if (rb.angularVelocity != 0f) rb.angularVelocity = 0f;
            if ((rb.constraints & RigidbodyConstraints2D.FreezeRotation) == 0)
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            return;
        }

        // 2. グリッドエリア内だが配置されていない場合：ItemBoxに向かう
        if (isInGridArea && !isPlacedOnGrid)
        {
            if (itemBoxTransform != null)
            {
                if (rb.bodyType != RigidbodyType2D.Dynamic) rb.bodyType = RigidbodyType2D.Dynamic;
                rb.angularVelocity = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;

                Vector2 direction = ((Vector3)itemBoxTransform.position - transform.position).normalized;
                rb.linearVelocity = direction * suckSpeed;
            }
            return;
        }

        // 3. ItemBoxZone内：重力のみ
        if (isInItemBoxZone)
        {
            if (rb.bodyType != RigidbodyType2D.Dynamic) rb.bodyType = RigidbodyType2D.Dynamic;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            // 重力のみなので速度は設定しない（自然落下）
            return;
        }

        // 4. それ以外の場所：ItemBoxに向かう
        if (itemBoxTransform != null)
        {
            if (rb.bodyType != RigidbodyType2D.Dynamic) rb.bodyType = RigidbodyType2D.Dynamic;
            rb.angularVelocity = 0f;
            if ((rb.constraints & RigidbodyConstraints2D.FreezeRotation) == 0)
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            Vector2 direction = ((Vector3)itemBoxTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * suckSpeed;
        }
    }

    // 簡素化されたゾーン状態チェック（Start時のみ使用）
    void CheckCurrentZoneStatus(bool isStartMethod)
    {
        if (itemCollider == null) return;

        // Start時は現在のゾーン状態のみチェック
        isInItemBoxZone = false;
        isInGridArea = false;

        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
            (Vector2)transform.position + itemCollider.offset,
            itemCollider.bounds.size,
            0f
        );

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(itemBoxZoneTag)) isInItemBoxZone = true;
            if (hitCollider.CompareTag(gridAreaTag)) isInGridArea = true;
        }

        if (enableDebugLogs && isStartMethod)
        {
            Debug.Log($"Start時ゾーン状態: {gameObject.name} - isInGridArea: {isInGridArea}, isInItemBoxZone: {isInItemBoxZone}");
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        return mainCamera.ScreenToWorldPoint(screenPos);
    }

    // デバッグ用のレイヤー設定確認
    [ContextMenu("レイヤー設定をチェック")]
    public void CheckLayerSettings()
    {
        // オブジェクトのレイヤー情報
        int objectLayer = gameObject.layer;
        string layerName = LayerMask.LayerToName(objectLayer);
        Debug.Log($"=== {gameObject.name} レイヤー設定確認 ===");
        Debug.Log($"オブジェクトレイヤー: {objectLayer} ({layerName})");

        // カメラのCulling Mask確認
        Camera cam = Camera.main;
        if (cam != null)
        {
            bool isLayerVisible = (cam.cullingMask & (1 << objectLayer)) != 0;
            Debug.Log($"カメラでこのレイヤーが表示される: {isLayerVisible}");
            if (!isLayerVisible)
            {
                Debug.LogWarning("カメラのCulling Maskにこのレイヤーが含まれていません！");
            }
        }

        // LayerMask設定確認
        bool isLayerInMask = (mouseDetectionLayers.value & (1 << objectLayer)) != 0;
        Debug.Log($"マウス検出レイヤーマスクにこのレイヤーが含まれる: {isLayerInMask}");
        Debug.Log($"現在のマウス検出レイヤーマスク値: {mouseDetectionLayers.value}");

        if (!isLayerInMask)
        {
            Debug.LogWarning("マウス検出レイヤーマスクにオブジェクトのレイヤーが含まれていません！");
        }

        // 推奨設定の表示
        if (useSpecificLayer)
        {
            int targetLayer = LayerMask.NameToLayer(targetLayerName);
            if (targetLayer != -1)
            {
                Debug.Log($"推奨レイヤー設定: '{targetLayerName}' (レイヤー{targetLayer})");
                if (objectLayer != targetLayer)
                {
                    Debug.LogWarning($"オブジェクトのレイヤーを'{targetLayerName}'に変更することを推奨します");
                }
            }
            else
            {
                Debug.LogError($"指定されたレイヤー名'{targetLayerName}'が存在しません");
            }
        }
    }

    // 自動でレイヤーを修正するメソッド
    [ContextMenu("レイヤーを自動修正")]
    public void AutoFixLayer()
    {
        if (useSpecificLayer)
        {
            int targetLayer = LayerMask.NameToLayer(targetLayerName);
            if (targetLayer != -1)
            {
                gameObject.layer = targetLayer;
                mouseDetectionLayers = 1 << targetLayer;
                Debug.Log($"{gameObject.name}のレイヤーを'{targetLayerName}' (レイヤー{targetLayer})に設定しました");
            }
            else
            {
                Debug.LogError($"レイヤー'{targetLayerName}'が存在しません。Project Settings > Tags and Layersで作成してください");
            }
        }
    }

    // デバッグ用の視覚的確認
    void OnDrawGizmosSelected()
    {
        if (itemCollider != null)
        {
            // 状態に応じて色を変更
            if (isPlacedOnGrid)
                Gizmos.color = Color.blue;      // グリッドに配置済み
            else if (isInGridArea)
                Gizmos.color = Color.cyan;      // グリッドエリア内
            else if (isInItemBoxZone)
                Gizmos.color = Color.red;       // ItemBoxゾーン内
            else
                Gizmos.color = Color.yellow;    // その他

            Gizmos.DrawWireCube(transform.position + (Vector3)itemCollider.offset, itemCollider.bounds.size);
        }

        if (showRaycastDebug)
        {
            Vector3 mousePos = GetMouseWorldPos();
            Gizmos.color = isMouseOver ? Color.green : Color.red;
            Gizmos.DrawSphere(mousePos, 0.1f);
        }
    }
}