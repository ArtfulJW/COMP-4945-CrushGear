using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CarController : NetworkBehaviour
{

    [SerializeField]
    private Vector2 defaultInitialPositionOnPlane = new Vector2(-4, 4);

    public List<AxleInfo> axleInfos; // the information about each individual axle
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    public float maxSteeringAngle; // maximum steer angle the wheel can have

    [SerializeField]
    private NetworkVariable<Vector3> networkPositionDirection = new NetworkVariable<Vector3>();

    private Vector3 oldInputPosition = Vector3.zero;
    //private Vector3 oldInputRotation = Vector3.zero;

    private void Start()
    {
        if (IsClient && IsOwner)
        {
            transform.position = new Vector3(Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y), 0,
                   Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y));
            CameraController.Instance.FollowPlayer(transform.Find("CameraRoot"));
        }
    }
    private void FixedUpdate()
    {
        if (IsClient && IsOwner)
        {
            clientUpdate();
        }

        //if (IsServer)
        //{
        //    serverUpdate();
        //}
    }

    //private void serverUpdate()
    //{
    //    transform.position = networkPositionDirection.Value;
    //}

    private void clientUpdate()
    {
        if (!IsLocalPlayer || !IsOwner)
            return;
        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
        }
        Vector3 newPosition = transform.TransformDirection(transform.position);
        if (oldInputPosition != newPosition)//|| oldInputRotation != inputRotation)
        {
            oldInputPosition = newPosition;
            UpdateClientPositionAndRotationServerRpc(newPosition);
        }
    }

    [ServerRpc]
    public void UpdateClientPositionAndRotationServerRpc(Vector3 newPosition)
    {
        networkPositionDirection.Value = newPosition;
        //networkRotationDirection.Value = newRotation;
    }
}

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // is this wheel attached to motor?
    public bool steering; // does this wheel apply steer angle?
}