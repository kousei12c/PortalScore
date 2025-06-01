using UnityEngine;

public class GridLinesRenderer : MonoBehaviour
{

    private int width = GridPlacer.gridWidth;
    private int height = GridPlacer.gridHeight;
    private float cellSize = GridPlacer.cellSize;
    public Transform gridOriginTransform;  // �����̃O���b�h���_�I�u�W�F�N�g

    public Vector2 gridOrigin
    {
        get
        {
            return gridOriginTransform != null ? gridOriginTransform.position : Vector2.zero;
        }
    }
    

    public Material lineMaterial;  // LineRenderer�p�}�e���A��

    void Start()
    {
        DrawGrid();
    }

    void DrawGrid()
    {
        // ����
        for (int y = 0; y <= height; y++)
        {
            Vector3 startPos = new Vector3(gridOrigin.x, gridOrigin.y + y * cellSize, 0);
            Vector3 endPos = new Vector3(gridOrigin.x + width * cellSize, gridOrigin.y + y * cellSize, 0);
            CreateLine(startPos, endPos);
        }

        // �c��
        for (int x = 0; x <= width; x++)
        {
            Vector3 startPos = new Vector3(gridOrigin.x + x * cellSize, gridOrigin.y, 0);
            Vector3 endPos = new Vector3(gridOrigin.x + x * cellSize, gridOrigin.y + height * cellSize, 0);
            CreateLine(startPos, endPos);
        }
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("GridLine");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = lineMaterial;
        lr.startColor = Color.white;
        lr.endColor = Color.white;
    }
}

