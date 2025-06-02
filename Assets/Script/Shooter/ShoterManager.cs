using UnityEngine;

public class ShoterManager : MonoBehaviour
{
    [Header("発射するPrefab")]
    public GameObject stonePrefab;
    public Transform shootPoint;

    [Header("引っ張る力の倍率")]
    public float power = 10f;


    private GameObject tempStone;
    private Rigidbody2D rb;

    // 予測線を描くコンポーネント
    public TrajectoryDrawer trajectoryDrawer;
    private Vector2 startDragPos;

    void Update()
    {
        // 1️⃣ ドラッグ開始
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f;
            startDragPos = worldPos; // Vector2 に自動変換（z は無視される）

            // 玉を作成
            tempStone = Instantiate(stonePrefab, shootPoint.position, Quaternion.identity);
            rb = tempStone.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // 引っ張り中は物理をオフ
        }

        // 2️⃣ ドラッグ中
        if (Input.GetMouseButton(1) && tempStone != null)
        {
            Vector3 dragCurrentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragCurrentPos.z = 0f;

            // BounceItemを軸に、マウスの現在位置への方向
            Vector2 direction = startDragPos - (Vector2)dragCurrentPos;
            Vector2 force = direction * power;
            Vector2 velocity = force / rb.mass;

            trajectoryDrawer.DrawTrajectory(shootPoint.position, velocity);
        }
        else if (trajectoryDrawer != null)
        {
            trajectoryDrawer.Clear();
        }

        // 3️⃣ ドラッグ終了（発射）
        if (Input.GetMouseButtonUp(1) && tempStone != null)
        {
            Vector3 dragEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragEndPos.z = 0f;

            // 方向は「引っ張り量」= (start - end)
            Vector2 direction = startDragPos - (Vector2)dragEndPos;
            Vector2 force = direction * power;

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.AddForce(force, ForceMode2D.Impulse);

            tempStone = null;
            rb = null;
        }

    }

}

