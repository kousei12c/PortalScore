using UnityEngine;

public class SuckableItem : MonoBehaviour
{
    public float suckSpeed = 5f;            // �z�����ޑ��x
    private string gridAreaTag = "GridArea";   // �O���b�h��̃^�O
    private string itemBoxDropZoneTag = "ItemBoxDropZone"; // ItemBox�̐^��i�Z�b�g�ꏊ�j�̃^�O
    public float arrivalThreshold = 0.1f;  // ItemBox�ɓ��B�����Ƃ݂Ȃ�����

    private Transform itemBoxTransform;     // ItemBox��Transform
    private Rigidbody2D rb;
    private bool isOnGrid = false;          // �A�C�e�����O���b�h��ɂ��邩
    private bool isInDropZone = false;     // �A�C�e����ItemBox�̃h���b�v�]�[���ɂ��邩
    private bool isBeingSucked = false;    // �z�����ݒ���

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject itemBoxObj = GameObject.Find("ItemBox"); // �V�[������ItemBox�𖼑O�Ō���
        if (itemBoxObj != null)
        {
            itemBoxTransform = itemBoxObj.transform;
        }
        else
        {
            Debug.LogError("ItemBox��������܂���B�V�[����ItemBox�Ƃ������O�̃I�u�W�F�N�g��z�u���Ă��������B");
            enabled = false; // �X�N���v�g�𖳌���
            return;
        }

        // ItemBox��������Ȃ���΁A���̃A�C�e���͋z�����ݑΏۊO�Ƃ��邩�A�G���[���o��
        if (itemBoxTransform == null)
        {
            Debug.LogWarning("�z�����ݐ��ItemBox���ݒ肳��Ă��܂���B���̃A�C�e���͋z�����܂�܂���B");
        }
    }

    void FixedUpdate() // Rigidbody�𑀍삷��ꍇ��FixedUpdate������
    {
        if (itemBoxTransform == null) return; // ItemBox���Ȃ���Ή������Ȃ�

        if (isOnGrid) // �O���b�h��ɂ���ꍇ
        {
            isBeingSucked = false;
            // �O���b�h��ł͋z�����܂ꂸ�A������������~������i�܂��͒�R�𑝂₷�Ȃǁj
            if (rb != null && !rb.isKinematic) // isKinematic�łȂ��ꍇ�̂ݐݒ�
            {
                rb.velocity = Vector2.zero; // �O�̂��ߑ��x���[����
                rb.isKinematic = true;      // �������Z�̉e�����ꎞ�I�Ɏ󂯂Ȃ�����
            }
            return; // �O���b�h��ɂ���΁A�ȍ~�̋z�����ݏ����͂��Ȃ�
        }
        else
        {
            // �O���b�h�O�ɏo���畨�����Z���ĊJ
            if (rb != null && rb.isKinematic)
            {
                rb.isKinematic = false;
            }
        }

        // isInDropZone (ItemBox�̐^��A�Z�b�g�ꏊ) �ɂ���ꍇ�͋z�����܂Ȃ�
        if (isInDropZone)
        {
            isBeingSucked = false;
            // �����ł͎��R�����ɔC����i���邢�͓���̐Î~�����j
            return;
        }

        // ��L�̏����i�O���b�h��ł��Ȃ��A�h���b�v�]�[���ł��Ȃ��j�𖞂����΋z������
        isBeingSucked = true;
        Vector2 direction = ((Vector3)itemBoxTransform.position - transform.position).normalized;
        if (rb != null)
        {
            rb.velocity = direction * suckSpeed;
        }

        // ItemBox�ɓ��B�������`�F�b�N
        if (Vector3.Distance(transform.position, itemBoxTransform.position) < arrivalThreshold)
        {
            Debug.Log(gameObject.name + " �� ItemBox �ɓ��B���܂����I");
            // �����ŃA�C�e�����u�Z�b�g�v���鏈���i��F���ł�����AItemBox�Ɋi�[����A�j���[�V�����Ȃǁj
            
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(gridAreaTag))
        {
            isOnGrid = true;
        }
        else if (other.CompareTag(itemBoxDropZoneTag)) // ItemBox�́u�Z�b�g����ꏊ�v�̃g���K�[
        {
            isInDropZone = true;
            if (rb != null)
            {
                // �h���b�v�]�[���ɓ�������A�Ⴆ�΂�����蒅�n������A���x�𗎂Ƃ��Ȃ�
                rb.velocity *= 0.1f; // �}�Ɏ~�܂�ƕs���R�Ȃ̂ŏ������x���c���Ȃ�
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(gridAreaTag))
        {
            isOnGrid = false;
        }
        else if (other.CompareTag(itemBoxDropZoneTag))
        {
            isInDropZone = false;
        }
    }
}