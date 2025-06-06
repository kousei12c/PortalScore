using UnityEngine;

/// <summary>
/// GridArea�ɓ���������Collider��isTrigger�𐧌䂷��R���|�[�l���g
/// BounceItem�Ƃ̓����蔻��𖳌����������A�C�e���ɃA�^�b�`���Ďg�p
/// </summary>
public class GridCollisionController : MonoBehaviour
{
    [Header("�ݒ�")]
    [SerializeField] private string gridAreaTag = "GridArea";
    [SerializeField] private bool enableDebugLogs = true;

    [Header("����Ώۂ�Collider")]
    [SerializeField] private Collider2D targetCollider;
    [SerializeField] private bool autoFindCollider = true;

    [Header("����ݒ�")]
    [SerializeField] private bool triggerOnInGridArea = true;  // GridArea����Trigger�ɂ��邩
    [SerializeField] private bool triggerOnOutGridArea = false; // GridArea�O��Trigger�ɂ��邩

    // ��ԊǗ�
    private bool isInGridArea = false;
    private bool originalIsTrigger;
    private bool hasStoredOriginalValue = false;

    void Start()
    {
        // Collider�̎�������
        if (autoFindCollider && targetCollider == null)
        {
            targetCollider = GetComponent<Collider2D>();
        }

        // �G���[�`�F�b�N
        if (targetCollider == null)
        {
            Debug.LogError($"GridCollisionController: {gameObject.name}��Collider2D��������܂���", gameObject);
            enabled = false;
            return;
        }

        // ������Ԃ�ۑ�
        StoreOriginalTriggerState();

        // �J�n���̏�Ԃ��`�F�b�N
        CheckInitialGridAreaStatus();

        if (enableDebugLogs)
        {
            Debug.Log($"GridCollisionController����������: {gameObject.name}, " +
                     $"�Ώ�Collider: {targetCollider.name}, " +
                     $"����isTrigger: {originalIsTrigger}");
        }
    }

    void Update()
    {
        // ���݂�GridArea��Ԃ��`�F�b�N
        CheckGridAreaStatus();
    }

    /// <summary>
    /// ����isTrigger��Ԃ�ۑ�
    /// </summary>
    void StoreOriginalTriggerState()
    {
        if (!hasStoredOriginalValue)
        {
            originalIsTrigger = targetCollider.isTrigger;
            hasStoredOriginalValue = true;
        }
    }

    /// <summary>
    /// �J�n����GridArea��Ԃ��`�F�b�N
    /// </summary>
    void CheckInitialGridAreaStatus()
    {
        Vector2 checkPosition = (Vector2)transform.position;
        if (targetCollider != null)
        {
            checkPosition += targetCollider.offset;
        }

        Collider2D[] hitColliders = Physics2D.OverlapPointAll(checkPosition);
        bool foundGridArea = false;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(gridAreaTag))
            {
                foundGridArea = true;
                break;
            }
        }

        isInGridArea = foundGridArea;
        ApplyTriggerState();
    }

    /// <summary>
    /// GridArea���ɂ��邩�ǂ������`�F�b�N
    /// </summary>
    void CheckGridAreaStatus()
    {
        Vector2 checkPosition = (Vector2)transform.position;
        if (targetCollider != null)
        {
            checkPosition += targetCollider.offset;
        }

        Collider2D[] hitColliders = Physics2D.OverlapPointAll(checkPosition);
        bool foundGridArea = false;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(gridAreaTag))
            {
                foundGridArea = true;
                break;
            }
        }

        // ��Ԃ��ω������ꍇ�̂ݏ���
        if (isInGridArea != foundGridArea)
        {
            bool wasInGridArea = isInGridArea;
            isInGridArea = foundGridArea;

            if (enableDebugLogs)
            {
                Debug.Log($"{gameObject.name}: GridArea��ԕω� - " +
                         $"�O: {wasInGridArea}, ����: {isInGridArea}");
            }

            ApplyTriggerState();
        }
    }

    /// <summary>
    /// ���݂̏�Ԃɉ�����isTrigger��ݒ�
    /// </summary>
    void ApplyTriggerState()
    {
        if (targetCollider == null) return;

        bool shouldBeTrigger;

        if (isInGridArea)
        {
            shouldBeTrigger = triggerOnInGridArea;
        }
        else
        {
            shouldBeTrigger = triggerOnOutGridArea;
        }

        // ��Ԃ��ω�����ꍇ�̂ݓK�p
        if (targetCollider.isTrigger != shouldBeTrigger)
        {
            targetCollider.isTrigger = shouldBeTrigger;

            if (enableDebugLogs)
            {
                Debug.Log($"{gameObject.name}: isTrigger��{shouldBeTrigger}�ɕύX " +
                         $"(GridArea��: {isInGridArea})");
            }
        }
    }

    /// <summary>
    /// ���̏�Ԃɖ߂�
    /// </summary>
    public void RestoreOriginalTriggerState()
    {
        if (targetCollider != null && hasStoredOriginalValue)
        {
            targetCollider.isTrigger = originalIsTrigger;

            if (enableDebugLogs)
            {
                Debug.Log($"{gameObject.name}: isTrigger�����̏��({originalIsTrigger})�ɕ���");
            }
        }
    }

    /// <summary>
    /// isTrigger��Ԃ������ݒ�
    /// </summary>
    public void SetTriggerState(bool isTrigger)
    {
        if (targetCollider != null)
        {
            targetCollider.isTrigger = isTrigger;

            if (enableDebugLogs)
            {
                Debug.Log($"{gameObject.name}: isTrigger���蓮��{isTrigger}�ɐݒ�");
            }
        }
    }

    /// <summary>
    /// ����GridArea���ɂ��邩�ǂ������擾
    /// </summary>
    public bool IsInGridArea()
    {
        return isInGridArea;
    }

    /// <summary>
    /// �Ώ�Collider�̌��݂�isTrigger��Ԃ��擾
    /// </summary>
    public bool GetCurrentTriggerState()
    {
        return targetCollider != null ? targetCollider.isTrigger : false;
    }

    /// <summary>
    /// ����isTrigger��Ԃ��擾
    /// </summary>
    public bool GetOriginalTriggerState()
    {
        return originalIsTrigger;
    }

    // �R���|�[�l���g������������鎞�Ɍ��̏�Ԃɖ߂�
    void OnDisable()
    {
        RestoreOriginalTriggerState();
    }

    // �I�u�W�F�N�g���j������鎞�Ɍ��̏�Ԃɖ߂�
    void OnDestroy()
    {
        RestoreOriginalTriggerState();
    }

    // �f�o�b�O�p�̎蓮�`�F�b�N
    [ContextMenu("GridArea��Ԃ��蓮�`�F�b�N")]
    public void ManualCheckGridAreaStatus()
    {
        CheckGridAreaStatus();
        Debug.Log($"�蓮�`�F�b�N���� - GridArea��: {isInGridArea}, " +
                 $"���݂�isTrigger: {GetCurrentTriggerState()}");
    }

    // �f�o�b�O�p�̏�ԃ��Z�b�g
    [ContextMenu("���̏�Ԃɕ���")]
    public void ManualRestoreOriginalState()
    {
        RestoreOriginalTriggerState();
    }

    // Inspector��ł̐ݒ�ύX���ɑ����ɔ��f
    void OnValidate()
    {
        if (Application.isPlaying && targetCollider != null)
        {
            ApplyTriggerState();
        }
    }

    // �f�o�b�O�p�̃M�Y���\��
    void OnDrawGizmosSelected()
    {
        if (targetCollider != null)
        {
            // GridArea�����ǂ����ŐF��ς���
            Gizmos.color = isInGridArea ? Color.green : Color.red;

            Vector3 center = transform.position + (Vector3)targetCollider.offset;
            Vector3 size = targetCollider.bounds.size;

            Gizmos.DrawWireCube(center, size);

            // isTrigger��Ԃ��\��
            if (targetCollider.isTrigger)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(center, size * 0.1f);
            }
        }
    }
}