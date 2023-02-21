using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Playables;

public class CarController : NetworkBehaviour
{

    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private float currentbreakForce;
    private bool isBreaking;

    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteerAngle;

    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheeTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;


    [SerializeField]
    private NetworkVariable<float> networkAcceleration = new NetworkVariable<float>();
    [SerializeField]
    private NetworkVariable<float> networkSteering = new NetworkVariable<float>();
    [SerializeField]
    private NetworkVariable<float> networkBrakeForce = new NetworkVariable<float>();

    [SerializeField]
    private Vector2 defaultInitialPositionOnPlane = new Vector2(-4, 4);
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
        if(IsClient && IsOwner)
        {
            GetInput();
            
        }
        ClientMove();
        //UpdateWheels();

    }

    /// <summary>
    /// CLIENTSIDE: Set movement for car
    /// </summary>
    private void ClientMove()
    {
        HandleMotor();
        HandleSteering();
    }

    /// <summary>
    /// CLIENTSIDE: 
    /// Receive inputs by user and pass values to server.
    /// </summary>
    private void GetInput()
    {
        horizontalInput = Input.GetAxis(HORIZONTAL);
        verticalInput = Input.GetAxis(VERTICAL);
        isBreaking = Input.GetKey(KeyCode.Space);
        currentbreakForce = isBreaking ? breakForce : 0f;
        UpdateClientServerRpc(verticalInput * motorForce, maxSteerAngle * horizontalInput, currentbreakForce);
    }

    /// <summary>
    /// CLIENTSIDE: 
    /// Set motor with value set by server
    /// </summary>
    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = networkAcceleration.Value;
        frontRightWheelCollider.motorTorque = networkAcceleration.Value;
        ApplyBreaking();
    }

    /// <summary>
    /// CLIENTSIDE: 
    /// Set brakeTorque with value set by server
    /// </summary>
    private void ApplyBreaking()
    {
        frontRightWheelCollider.brakeTorque = networkBrakeForce.Value;
        frontLeftWheelCollider.brakeTorque = networkBrakeForce.Value;
        rearLeftWheelCollider.brakeTorque = networkBrakeForce.Value;
        rearRightWheelCollider.brakeTorque = networkBrakeForce.Value;
    }

    /// <summary>
    /// CLIENTSIDE: 
    /// Set steerAngle with value set by server
    /// </summary>
    private void HandleSteering()
    {
        frontLeftWheelCollider.steerAngle = networkSteering.Value;
        frontRightWheelCollider.steerAngle = networkSteering.Value;
    }

    /// <summary>
    /// CLIENTSIDE: Updates the transform of every wheel based on it's respective wheel collider
    /// </summary>
    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheeTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    /// <summary>
    /// CLINETSIDE: Updates wheelTransform position and rotation based on wheelCollider
    /// </summary>
    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation *= Quaternion.Euler(rot.eulerAngles.x,0,0);
        wheelTransform.position = pos;
    }

    /// <summary>
    /// SERVERSIDE: Set server sync variables with respective params
    /// </summary>
    /// <param name="acceleration"></param>
    /// <param name="steerAngle"></param>
    /// <param name="currentBrakeForce"></param>
    [ServerRpc]
    public void UpdateClientServerRpc(float acceleration, float steerAngle, float currentBrakeForce)
    {
        networkAcceleration.Value = acceleration;
        networkSteering.Value = steerAngle;
        networkBrakeForce.Value = currentBrakeForce;
    }
}
