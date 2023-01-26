using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public Transform lookAt;
    public Transform Player;

    private const float YMin = -50.0f;
    private const float YMax = 0f;

    public float distance = 10.0f;
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    public float rotateSpeed = 5;

    void LateUpdate() {

        currentX += Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
        currentY += Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
        currentY = Mathf.Clamp(currentY, YMin, YMax);

        Vector3 Direction = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(-currentY, currentX, 0);
        transform.position = lookAt.position + rotation * Direction;

        transform.LookAt(lookAt.position);
    }
}
