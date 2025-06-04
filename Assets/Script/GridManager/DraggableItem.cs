using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    // initialPosition �̓O���b�h�͈͊O�̏ꍇ�Ɏg�p���Ȃ��Ȃ������߁A
    // �������̖ړI�Ŏg�p���Ȃ��̂ł���΍폜���Ă��\���܂���B
    // ����̓R�����g�A�E�g���Ă����܂��B
    // private Vector3 initialPosition;

    private GridManager gridManager;

    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager��������܂���B");
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
        // initialPosition = transform.position; // �O���b�h�͈͊O�ł͌��̈ʒu�ɖ߂��Ȃ����ߕs�v��
        offset = transform.position - GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePos = GetMouseWorldPos() + offset;
            transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z);
        }
    }

    void OnMouseUp()
    {
        if (isDragging)
        {
            isDragging = false;

            if (gridManager != null)
            {
                Vector2Int gridPos = gridManager.WorldToGridPosition(transform.position);

                if (gridManager.IsInsideGrid(gridPos))
                {
                    // �͈͓��Ȃ̂ŁA�O���b�h�ɃX�i�b�v���Ĕz�u
                    Vector3 snapPos = gridManager.GridToWorldPosition(gridPos);
                    transform.position = new Vector3(snapPos.x, snapPos.y, transform.position.z);
                    Debug.Log("�A�C�e�����O���b�h�͈͓��ɔz�u���܂����i�X�i�b�v�j�B");
                }
                else
                {
                    // �͈͊O�Ȃ̂ŁA���݂̈ʒu�i�h���b�O������̈ʒu�j�ɂ��̂܂ܔz�u
                    Debug.Log("�A�C�e�����O���b�h�͈͊O�ɔz�u���܂����i���R�z�u�j�B");
                    // ���ɉ������Ȃ� (transform.position �� OnMouseDrag �ōX�V���ꂽ�܂�)
                }
            }
            else
            {
                // GridManager��������Ȃ��ꍇ���A���݂̈ʒu�ɂ��̂܂ܔz�u
                Debug.LogWarning("GridManager�����p�ł��܂���B�A�C�e���͌��݂̈ʒu�ɔz�u����܂��B");
                // ���ɉ������Ȃ� (transform.position �� OnMouseDrag �ōX�V���ꂽ�܂�)
            }
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = Camera.main.nearClipPlane + 10f;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}