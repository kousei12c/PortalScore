using UnityEngine;

public class GridDrawer : MonoBehaviour
{
    public GridManager gridManager;
    public Material lineMaterial;
    public float lineWidth = 0.02f;

    void Start()
    {
        if (gridManager == null)
            gridManager = GetComponent<GridManager>();

        if (gridManager != null)
            DrawGrid();
    }

    void DrawGrid()
    {
        Vector2 origin = gridManager.gridOrigin;
        float cellSize = gridManager.cellSize;
        int width = gridManager.width;
        int height = gridManager.height;

        // ‰¡ü
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = new Vector3(origin.x, origin.y + y * cellSize, 0);
            Vector3 end = start + new Vector3(width * cellSize, 0, 0);
            CreateLineRenderer(start, end);
        }

        // cü
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = new Vector3(origin.x + x * cellSize, origin.y, 0);
            Vector3 end = start + new Vector3(0, height * cellSize, 0);
            CreateLineRenderer(start, end);
        }
    }

    void CreateLineRenderer(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.parent = this.transform;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.widthMultiplier = 0.05f; // ’²®‚µ‚Ä‚ËI
        lr.positionCount = 2;

        // Z²‚ğ‚¿‚å‚Á‚Æ‘O‚Éo‚·
        start.z = 0.1f;
        end.z = 0.1f;

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.useWorldSpace = true;
        lr.startColor = lr.endColor = Color.gray;
    }

}
