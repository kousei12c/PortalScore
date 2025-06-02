using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;

    private GridManager gridManager;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }

    void OnMouseDown()
    {
        isDragging = true;
        offset = transform.position - GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePos = GetMouseWorldPos() + offset;
            Vector2Int gridPos = gridManager.WorldToGridPosition(mousePos);

            if (gridManager.CanPlaceAt(gridPos, this))
            {
                Vector3 snapPos = gridManager.GridToWorldPosition(gridPos);
                transform.position = new Vector3(snapPos.x, snapPos.y, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z);
            }
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = 10f;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}
