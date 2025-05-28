using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int gridWidth, gridHeight;
    public GameObject cellVisualPrefab;
    private Dictionary<Vector2Int, Cell> grid = new();

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int gridPos = new(x, y);
                Vector3 worldPos = new Vector3(x + 0.5f, y + 0.5f, 0); // 視認性アップ
                Cell cell = new(gridPos, worldPos);
                grid[gridPos] = cell;

                // 可視化プレハブ設置（色とか）
                Instantiate(cellVisualPrefab, worldPos, Quaternion.identity);
            }
        }
    }

    public bool TryGetCell(Vector2Int gridPos, out Cell cell)
    {
        return grid.TryGetValue(gridPos, out cell);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
    }
}
