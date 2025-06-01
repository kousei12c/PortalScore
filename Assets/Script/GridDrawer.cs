using UnityEngine;

public class GridDrawer : MonoBehaviour
{
    private GridPlacer gridPlacer; // GridPlacerの参照（必要に応じて使用）
    private int width = GridPlacer.gridWidth;
    private int height = GridPlacer.gridHeight;
    private float cellSize = GridPlacer.cellSize;
    public Color gridColor = Color.gray;

    void OnDrawGizmos()
    {
        Gizmos.color = gridColor;

        // 横線
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = transform.position + new Vector3(0, y * cellSize, 0);
            Vector3 end = start + new Vector3(width * cellSize, 0, 0);
            Gizmos.DrawLine(start, end);
        }

        // 縦線
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = transform.position + new Vector3(x * cellSize, 0, 0);
            Vector3 end = start + new Vector3(0, height * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }
    }
}
