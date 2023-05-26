using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheCarController : MonoBehaviour
{
    [SerializeField] private float motorForce;
    [SerializeField]  public float breakForce;
    [SerializeField]  public float maxSteeringAngle;

    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;
    
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";


    private float horizontalInput;
    private float verticalInput;
    private float currentBreakForce;    
    private float currentsteerAngle;    
    private bool isBreaking;
    private void Update()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheel();

    }

  
    void GetInput()
    {
        horizontalInput = Input.GetAxis(HORIZONTAL);
        verticalInput = Input.GetAxis(VERTICAL);
        isBreaking = Input.GetKeyDown(KeyCode.Space);
    }
    void HandleMotor()
    {
        rearLeftWheelCollider.motorTorque = verticalInput * motorForce;
        rearRightWheelCollider.motorTorque = verticalInput * motorForce;

        currentBreakForce = isBreaking ? breakForce : 0f;
        if (isBreaking)
            ApplyBreaking();
    }

    private void ApplyBreaking()
    {
        frontLeftWheelCollider.brakeTorque = currentBreakForce;
        frontRightWheelCollider.brakeTorque = currentBreakForce;
    }

    private void HandleSteering()
    {
        currentsteerAngle = maxSteeringAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentsteerAngle;
        frontRightWheelCollider.steerAngle = currentsteerAngle;
    }

    private void UpdateWheel()
    {
        UpdateSingleWheel(frontLeftWheelCollider,frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider,frontRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider,rearLeftWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider,rearRightWheelTransform);
    }
    private void UpdateSingleWheel(WheelCollider wheelCollider,Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
;    }
}
