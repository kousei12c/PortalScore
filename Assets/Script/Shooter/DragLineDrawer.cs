using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryDrawer : MonoBehaviour
{
    public int numPoints = 50;
    public float timeStep = 0.1f;
    public float gravityScale = 1f;

    private LineRenderer lineRenderer;

    // �ŏ��̃X�^�[�g�ʒu���L�^
    private Vector3 initialStartPos;
    private bool hasInitialStartPos = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void DrawTrajectory(Vector3 startPos, Vector3 startVelocity)
    {
        // �����܂������ʒu���L�^���ĂȂ�������ۑ�
        if (!hasInitialStartPos)
        {
            initialStartPos = startPos;
            hasInitialStartPos = true;
        }

        Vector3[] positions = new Vector3[numPoints];

        Vector3 currentPos = initialStartPos; // �L�^���������ʒu���g��
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

        // �O���N���A�̃^�C�~���O�ŏ����ʒu�����Z�b�g
        hasInitialStartPos = false;
    }
}
