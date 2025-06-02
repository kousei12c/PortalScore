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

        // ����
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = (Vector3)gridOrigin + new Vector3(0, y * cellSize, 0);
            Vector3 end = start + new Vector3(width * cellSize, 0, 0);
            Gizmos.DrawLine(start, end);
        }

        // �c��
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = (Vector3)gridOrigin + new Vector3(x * cellSize, 0, 0);
            Vector3 end = start + new Vector3(0, height * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }
    }

    // ���[���h���W �� �O���b�h���W(Vector2Int)
    public Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        Vector2 relative = worldPos - gridOrigin;
        int gridX = Mathf.RoundToInt(relative.x / cellSize);
        int gridY = Mathf.RoundToInt(relative.y / cellSize);
        return new Vector2Int(gridX, gridY);
    }

    // �O���b�h���W �� ���[���h���W(Vector3)
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        Vector2 pos = gridOrigin + new Vector2(gridPos.x * cellSize, gridPos.y * cellSize);
        return new Vector3(pos.x, pos.y, 0f);
    }

    // �X�i�b�v���ꂽ���[���h���W��Ԃ��i���E�������j
    public Vector3 GetSnappedWorldPosition(Vector2 worldPos)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPos);

        // �O���b�h�͈͂ɐ���
        gridPos.x = Mathf.Clamp(gridPos.x, 0, width - 1);
        gridPos.y = Mathf.Clamp(gridPos.y, 0, height - 1);

        return GridToWorldPosition(gridPos);
    }
    public bool CanPlaceAt(Vector2Int gridPos, DraggableItem item)
    {
        // ���ƂŐ���i�͈͊O�E�d�Ȃ�Ȃǁj������
        return true;
    }
}

