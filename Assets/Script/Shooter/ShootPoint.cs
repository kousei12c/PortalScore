using UnityEngine;

public class ShootPointFollower : MonoBehaviour
{
    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // 2Dゲームの場合はZを固定
        transform.position = mouseWorldPos;
    }
}
