using UnityEngine;

/// <summary>
/// �{�[���ɐڐG�����ۂɃ{�[�������������A���g�͏��ł���A�C�e���B
/// ���̃X�N���v�g��SpeedBoost�A�C�e���̃v���n�u�ɃA�^�b�`���Ă��������B
/// </summary>
[RequireComponent(typeof(Collider2D))] // ���̃R���|�[�l���g�ɂ�Collider2D���K�{�ł�
public class SpeedBooster : MonoBehaviour
{
    [Tooltip("�{�[���̑��x�ɏ�Z����{��")]
    public float speedMultiplier = 1.5f;

    [Tooltip("�{�[�������ʂ��邽�߂̃^�O")]
    public string ballTag = "Ball"; // ���Ȃ��̃{�[���̃^�O���ɍ��킹�Ă�������

    /// <summary>
    /// ���̃R���|�[�l���g���I�u�W�F�N�g�ɏ��߂ăA�^�b�`���ꂽ���Ɏ����ŌĂ΂�܂��B
    /// Collider2D�������I��Trigger�ɐݒ肵�A�ݒ�̎�Ԃ��Ȃ��܂��B
    /// </summary>

    /// <summary>
    /// ����Collider�����̃I�u�W�F�N�g�̃g���K�[�]�[���ɓ����Ă������ɌĂ΂�܂��B
    /// </summary>
    /// <param name="other">�ڐG���������Collider2D</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // �ڐG�������肪�{�[�����ǂ������^�O�Ŋm�F���܂�
        if (other.CompareTag(ballTag))
        {
            // �{�[����Rigidbody2D���擾���܂�
            Rigidbody2D ballRb = other.GetComponent<Rigidbody2D>();
            if (ballRb != null)
            {
                // �{�[���̌��݂̑��x���擾���A�{���������čĐݒ肵�܂�
                ballRb.linearVelocity *= speedMultiplier;
                Debug.Log($"{other.name}�̑��x��{speedMultiplier}�{�ɂȂ�܂����B");

                // ����SpeedBoost�I�u�W�F�N�g���g�����ł����܂�
                Destroy(gameObject);
            }
        }
    }
}
