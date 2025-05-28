using UnityEngine;

public class ItemPlacer : MonoBehaviour
{
    public GridManager gridManager;
    public GameObject itemPrefab;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int gridPos = gridManager.WorldToGrid(mouseWorld);

            if (gridManager.TryGetCell(gridPos, out Cell cell) && !cell.IsOccupied)
            {
                GameObject item = Instantiate(itemPrefab, cell.WorldPosition, Quaternion.identity);
                cell.Item = item;
            }
        }
    }
}
