using Cinemachine;
using UnityEngine;

public class CameraController : Singleton<CameraController> {
    [SerializeField]
    private float amplitudeGain = 0.5f;

    [SerializeField]
    private float frequencyGain = 0.5f;

    private CinemachineFreeLook camera;
    private Transform cameraTarget;

    private void Awake()
    {
        camera = GetComponent<CinemachineFreeLook>();
    }

    //private void LateUpdate()
    //{
    //    xRotation -= mousePosition.y;
    //    xRotation = Mathf.Clamp(xRotation, -30f, 50f);
    //    Vector3 targetRotation = head.transform.eulerAngles;
    //    targetRotation.x = xRotation;
    //    head.eulerAngles = targetRotation;
    //    cameraTarget.eulerAngles = targetRotation;
    //    /*       Debug.Log(playerCamera.eulerAngles);*/
    //    cameraTarget.Rotate(Vector3.up, mousePosition.x * Time.deltaTime * mouseSensitivityX);
    //}

    public void FollowPlayer(Transform transform)
    {
        // not all scenes have a cinemachine virtual camera so return in that's the case
        if (camera == null)
            return;
        cameraTarget = transform;

        camera.Follow = cameraTarget;
        camera.LookAt = cameraTarget;

        //var perlin = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        //perlin.m_AmplitudeGain = amplitudeGain;
        //perlin.m_FrequencyGain = frequencyGain;
    }
}
