using UnityEngine;

public class ShootPointFollower : MonoBehaviour
{
    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // 2D�Q�[���̏ꍇ��Z���Œ�
        transform.position = mouseWorldPos;
    }
}
