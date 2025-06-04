using UnityEngine;
using System.Collections;

public class ShoterManager : MonoBehaviour
{
    [Header("発射するPrefab")]
    public GameObject stonePrefab;
    public Transform shootPoint; // 本来は固定された発射基点をアタッチすることを想定

    [Header("引っ張る力の倍率")]
    public float power = 1f;

    public TrajectoryDrawer trajectoryDrawer;

    private Vector2 startDragPos;
    private Vector2 calculatedForce;
    private Vector3 initialShootPosition; // ★ ドラッグ開始時の発射位置を記憶する変数

    [Header("連続発射設定")]
    public int instantiateTimes = 1;
    public float delayBetweenShots = 0.1f;

    private bool isDragging = false;

    private GameObject firstStone;
    private Rigidbody2D firstRb;

    void Update()
    {
        // 1️⃣ ドラッグ開始: 最初の玉を生成し見せておく
        if (Input.GetMouseButtonDown(1))
        {
            if (firstStone == null)
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldPos.z = 0f;
                startDragPos = worldPos;
                isDragging = true;

                initialShootPosition = shootPoint.position; // ★ ドラッグ開始時の発射位置を記憶
                firstStone = Instantiate(stonePrefab, initialShootPosition, Quaternion.identity); // ★ 記憶した位置から生成
                firstRb = firstStone.GetComponent<Rigidbody2D>();
                firstRb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        // 2️⃣ ドラッグ中
        if (Input.GetMouseButton(1) && isDragging && firstStone != null)
        {
            Vector3 dragCurrentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragCurrentPos.z = 0f;

            Vector2 direction = startDragPos - (Vector2)dragCurrentPos;
            calculatedForce = direction * power;

            if (trajectoryDrawer != null)
            {
                // 軌道予測の開始位置も initialShootPosition を使うのが望ましい
                trajectoryDrawer.DrawTrajectory(initialShootPosition, calculatedForce);
            }
        }
        else if (Input.GetMouseButtonUp(1) && isDragging && trajectoryDrawer != null) // マウスボタンが離された時にも軌跡を消す
        {
            trajectoryDrawer.Clear();
        }


        // 3️⃣ ドラッグ終了
        if (Input.GetMouseButtonUp(1) && isDragging && firstStone != null)
        {
            isDragging = false;
            // trajectoryDrawer.Clear(); // 上のelse ifで処理されるか、ここで改めて呼ぶ

            // --- 最初の玉を発射 ---
            // (生成時に initialShootPosition を使っているので、発射位置は既に固定されている)
            firstRb.bodyType = RigidbodyType2D.Dynamic;
            firstRb.linearVelocity = Vector2.zero;
            firstRb.angularVelocity = 0f;
            firstRb.AddForce(calculatedForce, ForceMode2D.Impulse);

            firstStone = null;
            firstRb = null;

            // --- 2発目以降の玉を発射するコルーチンを開始 ---
            if (instantiateTimes > 1)
            {
                // ★ 記憶した initialShootPosition をコルーチンに渡す
                StartCoroutine(LaunchRemainingStones(instantiateTimes - 1, calculatedForce, delayBetweenShots, initialShootPosition));
            }
        }
    }

    IEnumerator LaunchRemainingStones(int numberOfStonesToLaunch, Vector2 force, float delay, Vector3 spawnPosition) // ★ 引数に spawnPosition を追加
    {
        for (int i = 0; i < numberOfStonesToLaunch; i++)
        {
            yield return new WaitForSeconds(delay);

            // ★ 記憶された初期位置 (spawnPosition) から新しい玉を生成
            GameObject stone = Instantiate(stonePrefab, spawnPosition, Quaternion.identity);
            Rigidbody2D rb = stone.GetComponent<Rigidbody2D>();

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }
}