using UnityEngine;

public class Cell
{
    public Vector2Int GridPosition { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public GameObject Item { get; set; } // 設置されたアイテム

    public bool IsOccupied => Item != null;

    public Cell(Vector2Int gridPos, Vector3 worldPos)
    {
        GridPosition = gridPos;
        WorldPosition = worldPos;
        Item = null;
    }
}
