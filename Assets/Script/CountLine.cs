using UnityEngine;

public class CountLine : MonoBehaviour
{
    private int crossCount = 0;

    // crossCountの値を外部から読み取れるようにするためのパブリックプロパティ
    // "get" だけを定義することで、読み取り専用になります。
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