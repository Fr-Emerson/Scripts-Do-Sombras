using UnityEngine;

public class CameraChange : MonoBehaviour
{
    public Camera OldCamera;
    public Camera NewCamera;

    public void Awake()
    {
        OldCamera.enabled = true;
        NewCamera.enabled = false;
    }
    public void OnTriggerEnter(Collider other)
    {

        OldCamera.enabled = false;
        NewCamera.enabled = true;

    }
    public void OnTriggerExit(Collider other)
    {

        OldCamera.enabled = true;
        NewCamera.enabled = false;

    }
}