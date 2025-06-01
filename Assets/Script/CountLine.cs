using UnityEngine;

public class CountLine : MonoBehaviour
{
    private int crossCount = 0;

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("BounceItem"))
        {
            crossCount++;
            Debug.Log("CenterLine crossed! Count: " + crossCount);
        }
    }
}