using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPTypes;

using UnityEditorInternal;
using Unity.VisualScripting;

namespace GPTypes
{
   

    public delegate void GearShiftDelegate(int NewGear);

    public static class ExtensionMethods
    {

        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }


        //<summary> clamps the Value in the range (clamp_min <-> clamp_max) then remaps
        // it from 
        //(clamp_min <-> clamp_max) to (from2 <-> to2) 
        //
        //</summary>
        public static float ClampRemap(this float value, float clamp_min, float clamp_max, float from2, float to2) 
        {
            value = Mathf.Clamp(value, clamp_min, clamp_max);
            return value.Remap(clamp_min,clamp_max,from2,to2);
        }
    }
}

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    public event GearShiftDelegate GearShifted;


    
    
    [SerializeField] private RAYC_Wheel wheel_controller_fr;
    [SerializeField] private RAYC_Wheel wheel_controller_fl;
    [SerializeField] private RAYC_Wheel wheel_controller_rr;
    [SerializeField] private RAYC_Wheel wheel_controller_rl;

    [SerializeField] private AnimationCurve Grip_over_speed;
    
    // a curve traversed when a gear shift happens
    [SerializeField] private AnimationCurve Gear_Shif_Curve;
    
    // we have to give each wheel extra max speed so it can reach 
    // its gear max_speed cuz if we dont the car will never reach 
    // the up shift point
    [SerializeField] private float MaxSpeedOffset = 50;

    //the same way here upshift_tolerence makes closen the speed to do an upshift
    [SerializeField] private const float  UPShift_tolerance = .9f;
    [SerializeField] private float WheelRotationSpeed = 2;

    [SerializeField] private Vector2[] car_gears;

    

    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";


    private float horizontalInput;
    private float verticalInput;
    private float Braking = 0;


    private float currentBreakForce;    
    private float currentsteerAngle;    


   
    private Vector3 current_rigidbody_vel;
    bool moving_fwd;
    int current_gear;

    Rigidbody rigidbody;

    
    private void Start()
    {
       
        rigidbody = GetComponent<Rigidbody>();
        //wheels = GetComponentsInChildren<RAYC_Wheel>();

        InitTransmition();
        //delete me 
        this.GearShifted += (int x) => 
        {
            Debug.Log(x+1);
        };
    }
    private void Update()
    {
        if (Vector3.Dot(current_rigidbody_vel, transform.forward) > 0)
            moving_fwd = true;
        else 
            moving_fwd = false;


        SetInput();
        HandleSteering();
        HandleMotor();
        UpdateWheel();
        HandleShift();




        //get grip percent from the curve
        //   (grip :ammount of friction with road (High=>No Drift) | (Low->To Much Drift)
        
        float grip = Grip_over_speed.Evaluate(rigidbody.velocity.magnitude / car_gears[current_gear].y);
        AdjustGrip(wheel_controller_fr,grip);
        AdjustGrip(wheel_controller_fl, grip);
        AdjustGrip(wheel_controller_rr, grip);
        AdjustGrip(wheel_controller_rl, grip);
        

    }

  
    void SetInput(float horizontal_input=0,float vertical_input=0,float braking_input=0)
    {
        if(horizontal_input == 0 )
            horizontalInput = Input.GetAxis(HORIZONTAL);
        
        if(vertical_input == 0 )
        verticalInput = Input.GetAxis(VERTICAL);
        
        
        
        
        if (braking_input == 0)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                Braking = 1;
                wheel_controller_fr.Brake(Braking);
                wheel_controller_fl.Brake(Braking);
                //wheel_controller_rr.Brake(Braking);
                //wheel_controller_rl.Brake(Braking);

            }
            else
            { 
                Braking = 0;
                wheel_controller_fr.Brake(Braking);
                wheel_controller_fl.Brake(Braking);
                //wheel_controller_rr.Brake(Braking);
                //wheel_controller_rl.Brake(Braking);
            }


            
       
            
        }

        
          

        
       
    }


    void InitTransmition (int gear=0)
    {
        GearShifted?.Invoke(current_gear);
        wheel_controller_fl.Max_Speed = car_gears[gear].y + MaxSpeedOffset;
        wheel_controller_fr.Max_Speed = car_gears[gear].y + MaxSpeedOffset;
        wheel_controller_rl.Max_Speed = car_gears[gear].y + MaxSpeedOffset;
        wheel_controller_rr.Max_Speed = car_gears[gear].y + MaxSpeedOffset;
    }
    void HandleShift() 
    {
        if ((int)rigidbody.velocity.magnitude > car_gears[current_gear].y*.8 && current_gear + 1 != car_gears.Length)
        {
            //shifting code here
            current_gear++;
            GearShifted?.Invoke(current_gear);

            wheel_controller_fl.Max_Speed = car_gears[current_gear].y + MaxSpeedOffset; 
            wheel_controller_fr.Max_Speed = car_gears[current_gear].y + MaxSpeedOffset; 
            wheel_controller_rl.Max_Speed = car_gears[current_gear].y + MaxSpeedOffset; 
            wheel_controller_rr.Max_Speed = car_gears[current_gear].y + MaxSpeedOffset; 
        
        }

        if ((int)rigidbody.velocity.magnitude < car_gears[current_gear].x && current_gear -1 != -1)
        {
            //shifting code here
            current_gear--;
            GearShifted?.Invoke(current_gear);

            wheel_controller_fl.Max_Speed = car_gears[current_gear].y + MaxSpeedOffset;
            wheel_controller_fr.Max_Speed = car_gears[current_gear].y + MaxSpeedOffset;
            wheel_controller_rl.Max_Speed = car_gears[current_gear].y + MaxSpeedOffset;
            wheel_controller_rr.Max_Speed = car_gears[current_gear].y + MaxSpeedOffset;

        }

    }
    
    
    
    //
    //   BEGIN HAVOK HERE !! :D 
    //      ! KEEP AWAY FROM CODE BELOW PLZ FOR YOUR OWN SAFETY !
    void HandleMotor()
    {
      
        if (Mathf.Abs(verticalInput) > 0.01)
        {
            wheel_controller_fr.Accelerate(verticalInput);
            wheel_controller_fl.Accelerate(verticalInput);
            wheel_controller_rr.Accelerate(verticalInput);
            wheel_controller_rl.Accelerate(verticalInput);
        }
        else 
        {
            // you have to pass 0 otherwise the car will roll forever 
            wheel_controller_fr.Accelerate(0);
            wheel_controller_fl.Accelerate(0);
            wheel_controller_rr.Accelerate(0);
            wheel_controller_rl.Accelerate(0);
        }
    }
    
    //<summary> Handles Physics Steering Dont Play with </summary>
    private void HandleSteering() 
    {

        wheel_controller_fr.Steer(horizontalInput); 
        wheel_controller_fl.Steer(horizontalInput); 
        wheel_controller_rr.Steer(horizontalInput);
        wheel_controller_rl.Steer(horizontalInput);

        UpdateSingleWheelGFXSteerable(wheel_controller_fl);
        UpdateSingleWheelGFXSteerable(wheel_controller_fr);
    } 
    private void UpdateWheel()
    {
        UpdateSingleWheelGFX(wheel_controller_fr);
        UpdateSingleWheelGFX(wheel_controller_fl);
        UpdateSingleWheelGFX(wheel_controller_rr);
        UpdateSingleWheelGFX(wheel_controller_rl);
    }
    
    private void UpdateSingleWheelGFX(RAYC_Wheel wheel)
    {
       
        Vector3 rotation = Vector3.zero;
        current_rigidbody_vel = rigidbody.velocity;

        if (wheel.gameObject.name == "FR_C")
        {
            //Debug.Log("LOL");
        }
        switch (wheel.gfx_right_rotation_axis) 
        {
            case Axis.X:
                {
                    rotation.x += current_rigidbody_vel.magnitude;
                    break;
                }
            case Axis.Y:
                {
                    rotation.y += current_rigidbody_vel.magnitude;
                    break;
                }
            case Axis.Z:
                {
                    rotation.z += current_rigidbody_vel.magnitude;
                    break;
                }
            case Axis._X:
                {
                    rotation.x += -current_rigidbody_vel.magnitude;
                    break;
                }
            case Axis._Y:
                {
                    rotation.y += -current_rigidbody_vel.magnitude;
                    break;
                }
            case Axis._Z:
                {
                    rotation.z += -current_rigidbody_vel.magnitude;
                    break;
                }
        }

        //Debug.Log(rotation);
        //wheel.GFX.Rotate(  (moving_fwd? 1:-1) * rotation*WheelRotationSpeed) ; 
        if(wheel.steerable)
            wheel.Steerable_GFX_X.rotation *= Quaternion.Euler(  (moving_fwd? 1:-1) * Vector3.right*WheelRotationSpeed*current_rigidbody_vel.magnitude) ; 
        else
            wheel.GFX.rotation *= Quaternion.Euler(  (moving_fwd? 1:-1) * Vector3.right*WheelRotationSpeed*current_rigidbody_vel.magnitude) ; 
    
    }
    //steer effect
    private void UpdateSingleWheelGFXSteerable(RAYC_Wheel wheel)
    {

        Quaternion q = transform.rotation;
        Vector3 rotation = q.eulerAngles;
        rotation.y = wheel.MaxSteeringAngle * horizontalInput + wheel.transform.parent.eulerAngles.y;
     
        wheel.GFX.rotation = Quaternion.Euler(rotation); 

    }

    public void AdjustGrip(RAYC_Wheel wheel, float grip)
    {
        grip = Mathf.Clamp01(grip);
        wheel.Grip = grip;
    }

    #region GIZMOS
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        RAYC_Wheel[] wheels = GetComponentsInChildren<RAYC_Wheel>();
        foreach (RAYC_Wheel wheel in wheels) 
        {
            Debug.DrawRay(wheel.transform.position, wheel.Forward);
        }
    }
#endif
#endregion

}
