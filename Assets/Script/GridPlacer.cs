using UnityEngine;

public class GridPlacer : MonoBehaviour
{
    
    public static int gridWidth = 8;
    public static int gridHeight = 16;
    public static float cellSize = 1f;

    public GameObject itemPrefab;  // 置くアイテムのPrefab
    public Transform gridOriginTransform;  // 左下のグリッド原点オブジェクト

    public Vector2 gridOrigin
    {
        get
        {
            return gridOriginTransform != null ? gridOriginTransform.position : Vector2.zero;
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))  // 左クリック
        {
            Vector2Int gridPos = GetGridPosition();
            if (IsValidPosition(gridPos, itemPrefab))
            {
                PlaceItem(gridPos, itemPrefab);
            }
            else
            {
                Debug.Log("配置不可！");
            }
        }
    }

    // マウス位置をグリッド座標に変換
    Vector2Int GetGridPosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 中央スナップ: cellSize / 2 ずらす
        Vector2 relativePos = mouseWorldPos - (Vector3)gridOrigin - new Vector3(cellSize / 2f, cellSize / 2f, 0f);

        int gridX = Mathf.RoundToInt(relativePos.x / cellSize);
        int gridY = Mathf.RoundToInt(relativePos.y / cellSize);

        return new Vector2Int(gridX, gridY);
    }


    // 配置可能か判定（アイテムサイズに応じて）
    bool IsValidPosition(Vector2Int gridPos, GameObject item)
    {
        // 例: itemの縦横サイズを取得する（アイテムのスクリプトに設定しておく）
        GridItem gridItem = item.GetComponent<GridItem>();
        if (gridItem == null) return false;

        // 配置範囲がグリッド内か判定
        int endX = gridPos.x + gridItem.width - 1;
        int endY = gridPos.y + gridItem.height - 1;

        return gridPos.x >= 0 && gridPos.y >= 0 &&
               endX < gridWidth && endY < gridHeight;
    }

    // アイテムをグリッド位置に配置
    void PlaceItem(Vector2Int gridPos, GameObject itemPrefab)
    {
        // 各マスの中央に配置したいので、cellSize / 2 だけずらす
        Vector2 worldPos = gridOrigin + new Vector2(gridPos.x * cellSize, gridPos.y * cellSize) + new Vector2(cellSize / 2f, cellSize / 2f);

        Instantiate(itemPrefab, worldPos, Quaternion.identity);
    }

}
