using UnityEngine;

public class GridPlacer : MonoBehaviour
{
    
    public static int gridWidth = 8;
    public static int gridHeight = 16;
    public static float cellSize = 1f;

    public GameObject itemPrefab;  // �u���A�C�e����Prefab
    public Transform gridOriginTransform;  // �����̃O���b�h���_�I�u�W�F�N�g

    public Vector2 gridOrigin
    {
        get
        {
            return gridOriginTransform != null ? gridOriginTransform.position : Vector2.zero;
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))  // ���N���b�N
        {
            Vector2Int gridPos = GetGridPosition();
            if (IsValidPosition(gridPos, itemPrefab))
            {
                PlaceItem(gridPos, itemPrefab);
            }
            else
            {
                Debug.Log("�z�u�s�I");
            }
        }
    }

    // �}�E�X�ʒu���O���b�h���W�ɕϊ�
    Vector2Int GetGridPosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // �����X�i�b�v: cellSize / 2 ���炷
        Vector2 relativePos = mouseWorldPos - (Vector3)gridOrigin - new Vector3(cellSize / 2f, cellSize / 2f, 0f);

        int gridX = Mathf.RoundToInt(relativePos.x / cellSize);
        int gridY = Mathf.RoundToInt(relativePos.y / cellSize);

        return new Vector2Int(gridX, gridY);
    }


    // �z�u�\������i�A�C�e���T�C�Y�ɉ����āj
    bool IsValidPosition(Vector2Int gridPos, GameObject item)
    {
        // ��: item�̏c���T�C�Y���擾����i�A�C�e���̃X�N���v�g�ɐݒ肵�Ă����j
        GridItem gridItem = item.GetComponent<GridItem>();
        if (gridItem == null) return false;

        // �z�u�͈͂��O���b�h��������
        int endX = gridPos.x + gridItem.width - 1;
        int endY = gridPos.y + gridItem.height - 1;

        return gridPos.x >= 0 && gridPos.y >= 0 &&
               endX < gridWidth && endY < gridHeight;
    }

    // �A�C�e�����O���b�h�ʒu�ɔz�u
    void PlaceItem(Vector2Int gridPos, GameObject itemPrefab)
    {
        // �e�}�X�̒����ɔz�u�������̂ŁAcellSize / 2 �������炷
        Vector2 worldPos = gridOrigin + new Vector2(gridPos.x * cellSize, gridPos.y * cellSize) + new Vector2(cellSize / 2f, cellSize / 2f);

        Instantiate(itemPrefab, worldPos, Quaternion.identity);
    }

}
