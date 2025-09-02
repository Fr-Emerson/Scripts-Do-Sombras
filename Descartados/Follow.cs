using UnityEngine;
public class Follow : MonoBehaviour
{
    public Transform target; // The target to follow
    void LateUpdate()
    {
        if (target != null)
        {
            // Calculate the direction to the target
  
            // Move towards the target
            transform.position = target.position;
        }
    }
}