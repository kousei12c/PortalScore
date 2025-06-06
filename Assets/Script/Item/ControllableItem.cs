using UnityEngine;

/// <summary>
/// アイテムの全ての挙動（ドラッグ、グリッドスナップ、物理挙動、isTrigger制御）を管理する統合スクリプト。
/// </summary>
public class ControllableItem : MonoBehaviour
{
    [Header("基本設定")]
    [Tooltip("ItemBoxに引き寄せられる速度")]
    public float suckSpeed = 5f;
    [Tooltip("グリッドエリアを示すオブジェクトのタグ")]
    public string gridAreaTag = "GridArea";
    [Tooltip("引き寄せが停止するゾーンのタグ")]
    public string itemBoxZoneTag = "ItemBoxZone";

    [Header("グリッド上の挙動")]
    [Tooltip("グリッドに配置された時にColliderをTriggerにするか（ボール等をすり抜けさせたいアイテムはオン）")]
    public bool becomeTriggerWhenPlacedOnGrid = false;

    [Header("マウス検出レイヤー設定")]
    [Tooltip("特定のレイヤーのアイテムのみをドラッグ対象にするか")]
    [SerializeField] private bool useSpecificLayerForMouse = true;
    [Tooltip("ドラッグ対象とするアイテムのレイヤー名")]
    [SerializeField] private string draggableLayerName = "DraggableItems";
    [SerializeField] private LayerMask mouseDetectionLayers = ~0;

    [Header("デバッグ用")]
    public bool enableDebugLogs = true;

    // --- 内部状態管理フラグ ---
    private bool isDragging = false;
    private bool isPlacedOnGrid = false;
    private bool isInGridArea = false;
    private bool isInItemBoxZone = false;
    private bool isMouseOver = false;

    // --- 必要なコンポーネントやオブジェクトへの参照 ---
    private Rigidbody2D rb;
    private Camera mainCamera;
    private GridManager gridManager;
    private Transform itemBoxTransform;
    private Collider2D itemCollider;

    // --- ドラッグ & 回転用 ---
    private Vector3 offset;
    private Quaternion initialRotation;

    // --- isTrigger制御用 ---
    private bool originalIsTrigger;

    void Start()
    {
        // コンポーネント取得とエラーチェック
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) { Debug.LogError("Rigidbody2Dが見つかりません。", gameObject); enabled = false; return; }

        itemCollider = GetComponent<Collider2D>();
        if (itemCollider == null) { Debug.LogError("Collider2Dが見つかりません。", gameObject); enabled = false; return; }

        mainCamera = Camera.main;
        if (mainCamera == null) { Debug.LogError("メインカメラが見つかりません。カメラに「MainCamera」タグが付いているか確認してください。", gameObject); enabled = false; return; }

        gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null) { Debug.LogWarning("GridManagerが見つかりません。グリッド機能は動作しません。"); }

        GameObject itemBoxObj = GameObject.Find("ItemBox");
        if (itemBoxObj != null) { itemBoxTransform = itemBoxObj.transform; }
        else { Debug.LogWarning("ItemBox (引き寄せ目標) が見つかりません。吸い込み機能は動作しません。"); }

        // 初期状態設定
        rb.bodyType = RigidbodyType2D.Dynamic;
        initialRotation = transform.rotation;
        originalIsTrigger = itemCollider.isTrigger; // 元のisTrigger状態を記憶

        InitializeMouseDetectionLayer();
        CheckCurrentZoneStatus();
    }

    void Update()
    {
        HandleMouseInput();
        if (isDragging)
        {
            HandleDragging();
        }
    }

    void FixedUpdate()
    {
        if (!isDragging)
        {
            HandlePhysicsState();
        }
    }

    #region 初期化 (Initialization)

    void InitializeMouseDetectionLayer()
    {
        if (useSpecificLayerForMouse)
        {
            int layerIndex = LayerMask.NameToLayer(draggableLayerName);
            if (layerIndex != -1)
            {
                mouseDetectionLayers = 1 << layerIndex;
                if (gameObject.layer != layerIndex && enableDebugLogs)
                {
                    Debug.LogWarning($"{gameObject.name} のレイヤー ({LayerMask.LayerToName(gameObject.layer)}) は、" +
                                     $"指定されたドラッグ可能レイヤー ({draggableLayerName}) と異なります。");
                }
            }
            else
            {
                Debug.LogWarning($"レイヤー '{draggableLayerName}' が見つかりません。すべてのレイヤーを検出対象とします。");
                mouseDetectionLayers = ~0;
            }
        }
    }
    #endregion

    #region マウス入力とドラッグ (Mouse Input & Dragging)

    void HandleMouseInput()
    {
        Vector3 mouseWorldPos = GetMouseWorldPos();
        CheckIfMouseIsOverTopItem(mouseWorldPos);

        if (Input.GetMouseButtonDown(0) && isMouseOver && !isDragging)
        {
            StartDragging(mouseWorldPos);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            StopDragging();
        }
    }

    void CheckIfMouseIsOverTopItem(Vector3 mouseWorldPos)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPos, mouseDetectionLayers);
        Collider2D topCollider = null;
        int topOrder = int.MinValue;
        float topZ = float.MaxValue;

        foreach (var hit in hits)
        {
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
        isMouseOver = (topCollider == itemCollider);
    }

    void StartDragging(Vector3 mouseWorldPos)
    {
        isDragging = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.None;
        offset = transform.position - mouseWorldPos;
        isPlacedOnGrid = false;

        // ドラッグ開始時に isTrigger を元の状態に戻す
        if (itemCollider.isTrigger != originalIsTrigger)
        {
            itemCollider.isTrigger = originalIsTrigger;
            if (enableDebugLogs) Debug.Log($"{gameObject.name} isTrigger を元の状態 ({originalIsTrigger}) に戻しました");
        }

        if (enableDebugLogs) Debug.Log($"ドラッグ開始: {gameObject.name}");
    }

    void HandleDragging()
    {
        Vector3 mousePos = GetMouseWorldPos() + offset;
        rb.MovePosition(new Vector2(mousePos.x, mousePos.y));
    }

    void StopDragging()
    {
        if (!isDragging) return;
        isDragging = false;
        if (enableDebugLogs) Debug.Log($"ドラッグ終了: {gameObject.name}");

        CheckCurrentZoneStatus();

        if (isInGridArea && gridManager != null)
        {
            ApplyGridSnapping();
        }
        else
        {
            ApplyNormalPhysics();
        }
    }

    #endregion

    #region 物理と配置状態 (Physics & Placement)

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

        isPlacedOnGrid = true;

        // 配置確定時に isTrigger を設定
        if (becomeTriggerWhenPlacedOnGrid)
        {
            itemCollider.isTrigger = true;
            if (enableDebugLogs) Debug.Log($"{gameObject.name} グリッドにスナップし、isTrigger を On にしました");
        }
    }

    void ApplyNormalPhysics()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.angularVelocity = 0f;

        // isTrigger を元の状態に戻す
        if (itemCollider.isTrigger != originalIsTrigger)
        {
            itemCollider.isTrigger = originalIsTrigger;
        }

        isPlacedOnGrid = false;
        if (enableDebugLogs) Debug.Log($"{gameObject.name} 通常の物理演算に戻しました");
    }

    void HandlePhysicsState()
    {
        if (isPlacedOnGrid)
        {
            if (rb.bodyType != RigidbodyType2D.Kinematic) rb.bodyType = RigidbodyType2D.Kinematic;
            return;
        }

        if (isInItemBoxZone && !isInGridArea)
        {
            if (rb.bodyType != RigidbodyType2D.Dynamic) rb.bodyType = RigidbodyType2D.Dynamic;
            rb.angularVelocity = 0f;
            if ((rb.constraints & RigidbodyConstraints2D.FreezeRotation) == 0) rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            return;
        }

        if ((rb.constraints & RigidbodyConstraints2D.FreezeRotation) != 0)
        {
            rb.constraints = RigidbodyConstraints2D.None;
        }

        if (itemBoxTransform != null)
        {
            if (rb.bodyType != RigidbodyType2D.Dynamic) rb.bodyType = RigidbodyType2D.Dynamic;
            Vector2 direction = ((Vector3)itemBoxTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * suckSpeed;
        }
    }

    #endregion

    #region ゾーン判定 (Zone Detection)

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDragging) return;
        UpdateZoneFlags(other.tag, true);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (isDragging) return;
        UpdateZoneFlags(other.tag, true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (isDragging) return;
        UpdateZoneFlags(other.tag, false);

        if (other.CompareTag(gridAreaTag))
        {
            if (isPlacedOnGrid)
            {
                isPlacedOnGrid = false;
                ApplyNormalPhysics();
            }
        }
    }

    void UpdateZoneFlags(string tag, bool isInZone)
    {
        if (tag == gridAreaTag)
        {
            isInGridArea = isInZone;
        }
        else if (tag == itemBoxZoneTag)
        {
            if (!isInGridArea)
            {
                isInItemBoxZone = isInZone;
            }
        }
    }

    void CheckCurrentZoneStatus()
    {
        if (itemCollider == null) return;

        isInGridArea = false;
        isInItemBoxZone = false;

        Collider2D[] hitColliders = Physics2D.OverlapBoxAll((Vector2)transform.position + itemCollider.offset, itemCollider.bounds.size, 0f);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(gridAreaTag)) { isInGridArea = true; break; }
        }
        if (!isInGridArea)
        {
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag(itemBoxZoneTag)) { isInItemBoxZone = true; break; }
            }
        }
    }
    #endregion

    #region ヘルパー (Helpers)
    Vector3 GetMouseWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        return mainCamera.ScreenToWorldPoint(screenPos);
    }
    int GetSortingOrder(Collider2D col)
    {
        SpriteRenderer sr = col.GetComponent<SpriteRenderer>();
        if (sr != null) return sr.sortingOrder;
        return Mathf.RoundToInt(-col.transform.position.z * 1000);
    }
    #endregion
}
