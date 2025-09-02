using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraController: MonoBehaviour
{
    [Header("Character")]
    public Transform character;
    [Header("Camera Settings")]
    public float smoothSpeed = 2f;
    public Vector3 offset = new Vector3(0, 13, -15);
    public void Update()
    {

        if (character == null) return;

        Vector3 desiredPosition = character.position + offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        //transform.LookAt(character);
    }
}
