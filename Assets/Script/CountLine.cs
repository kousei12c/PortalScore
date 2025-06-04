using UnityEngine;

public class CountLine : MonoBehaviour
{
    private int crossCount = 0;

    // crossCount�̒l���O������ǂݎ���悤�ɂ��邽�߂̃p�u���b�N�v���p�e�B
    // "get" �������`���邱�ƂŁA�ǂݎ���p�ɂȂ�܂��B
    public int CrossCount
    {
        get { return crossCount; }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("BounceItem"))
        {
            crossCount++;
            Debug.Log("CenterLine crossed! Count: " + crossCount);
         
        }
    }
}