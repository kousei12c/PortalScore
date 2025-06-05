using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 8;
    public int height = 16;
    public float cellSize = 1f;

    public Vector2 gridOrigin = new Vector2(-4f, -8f);
    public Color gridColor = Color.gray;

    private void OnDrawGizmos()
    {
        Gizmos.color = gridColor;

        // 横線
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = (Vector3)gridOrigin + new Vector3(0, y * cellSize, 0);
            Vector3 end = start + new Vector3(width * cellSize, 0, 0);
            Gizmos.DrawLine(start, end);
        }

        // 縦線
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = (Vector3)gridOrigin + new Vector3(x * cellSize, 0, 0);
            Vector3 end = start + new Vector3(0, height * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }
    }

    // ワールド座標 → グリッド座標(Vector2Int)
    public Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        Vector2 relative = worldPos - gridOrigin;
        int gridX = Mathf.RoundToInt(relative.x / cellSize);
        int gridY = Mathf.RoundToInt(relative.y / cellSize);
        return new Vector2Int(gridX, gridY);
    }

    // グリッド座標 → ワールド座標(Vector3)
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        Vector2 pos = gridOrigin + new Vector2(gridPos.x * cellSize, gridPos.y * cellSize);
        return new Vector3(pos.x, pos.y, 0f);
    }

    // スナップされたワールド座標を返す（境界制限つき）
    public Vector3 GetSnappedWorldPosition(Vector2 worldPos)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPos);

        // グリッド範囲に制限
        gridPos.x = Mathf.Clamp(gridPos.x, 0, width - 1);
        gridPos.y = Mathf.Clamp(gridPos.y, 0, height - 1);

        return GridToWorldPosition(gridPos);
    }

    public bool IsInsideGrid(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x <= width &&
               gridPos.y >= 0 && gridPos.y <= height;
    }
}

