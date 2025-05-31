using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryDrawer : MonoBehaviour
{
    public int numPoints = 50;
    public float timeStep = 0.1f;
    public float gravityScale = 1f;

    private LineRenderer lineRenderer;

    // 最初のスタート位置を記録
    private Vector3 initialStartPos;
    private bool hasInitialStartPos = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void DrawTrajectory(Vector3 startPos, Vector3 startVelocity)
    {
        // もしまだ初期位置を記録してなかったら保存
        if (!hasInitialStartPos)
        {
            initialStartPos = startPos;
            hasInitialStartPos = true;
        }

        Vector3[] positions = new Vector3[numPoints];

        Vector3 currentPos = initialStartPos; // 記録した初期位置を使う
        Vector3 currentVelocity = startVelocity;

        for (int i = 0; i < numPoints; i++)
        {
            positions[i] = currentPos;
            currentVelocity += (Vector3)(Physics2D.gravity * gravityScale * timeStep);
            currentPos += currentVelocity * timeStep;
        }

        lineRenderer.positionCount = numPoints;
        lineRenderer.SetPositions(positions);
    }

    public void Clear()
    {
        lineRenderer.positionCount = 0;

        // 軌道クリアのタイミングで初期位置もリセット
        hasInitialStartPos = false;
    }
}
