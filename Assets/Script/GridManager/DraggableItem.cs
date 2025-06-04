using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    // initialPosition はグリッド範囲外の場合に使用しなくなったため、
    // もし他の目的で使用しないのであれば削除しても構いません。
    // 今回はコメントアウトしておきます。
    // private Vector3 initialPosition;

    private GridManager gridManager;

    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManagerが見つかりません。");
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
        // initialPosition = transform.position; // グリッド範囲外では元の位置に戻さないため不要に
        offset = transform.position - GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePos = GetMouseWorldPos() + offset;
            transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z);
        }
    }

    void OnMouseUp()
    {
        if (isDragging)
        {
            isDragging = false;

            if (gridManager != null)
            {
                Vector2Int gridPos = gridManager.WorldToGridPosition(transform.position);

                if (gridManager.IsInsideGrid(gridPos))
                {
                    // 範囲内なので、グリッドにスナップして配置
                    Vector3 snapPos = gridManager.GridToWorldPosition(gridPos);
                    transform.position = new Vector3(snapPos.x, snapPos.y, transform.position.z);
                    Debug.Log("アイテムをグリッド範囲内に配置しました（スナップ）。");
                }
                else
                {
                    // 範囲外なので、現在の位置（ドラッグした先の位置）にそのまま配置
                    Debug.Log("アイテムをグリッド範囲外に配置しました（自由配置）。");
                    // 特に何もしない (transform.position は OnMouseDrag で更新されたまま)
                }
            }
            else
            {
                // GridManagerが見つからない場合も、現在の位置にそのまま配置
                Debug.LogWarning("GridManagerが利用できません。アイテムは現在の位置に配置されます。");
                // 特に何もしない (transform.position は OnMouseDrag で更新されたまま)
            }
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = Camera.main.nearClipPlane + 10f;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}