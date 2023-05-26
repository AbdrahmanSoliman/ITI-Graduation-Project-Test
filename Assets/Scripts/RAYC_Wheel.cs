using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using GPTypes;

namespace GPTypes
{
    public enum Axis { X, Y, Z, _X,_Y,_Z};
}
public class RAYC_Wheel : MonoBehaviour
{

    [Header("GFX The Visible Wheel")]
    [SerializeField] public Transform GFX;
    [SerializeField] public Transform Steerable_GFX_X;
    [SerializeField] public Axis gfx_right_rotation_axis;
    [SerializeField] public Axis gfx_up_rotation_axis;


    [Header("Accelerating/Braking")]
    [SerializeField] bool driving = false;
    [SerializeField] public float Max_Speed;
    [SerializeField] float Max_Brake_Power;
    [SerializeField] AnimationCurve PowerCurve;
    [SerializeField] AnimationCurve BrakingCurve;
    [Range(0,1)]
    [SerializeField] float Idle_friction;
    [SerializeField] float Idle_friction_Base;

    [Header("Steering")]
    [SerializeField] public float MaxSteeringAngle = 30;
    [SerializeField] public bool steerable = false;
    [Range(0,1)]
    [SerializeField] public float Grip;
    [SerializeField] float tireMass = 20;
    

    [Header("Suspention")]
    //not compressed
    public float restLength;
    
    // amount moved 
    public float springTravel;
    public float springStiffness;
    public float damperStiffness;
    //max stretch amount
     float maxLength;
    //most compressed
    float minLength;

    //current
    private float springLength;
    private float springForce;

    private Vector3 suspensionForce;
    //for Damping calculations
    private float lastLength;
    private float damperForce;
    private float springVelocity;


    [Header("Wheel")]
    public float wheelRadius;

    [Header("Debug")]
    //visuals
    //public float ray_length = .3f;
    public Vector3 Forward;
    
    private Vector3 suspention_dir = Vector3.zero;
    private Vector3 current_applied_force_dir = Vector3.zero;


    public Color col_spring = Color.blue;
    public Color col_rest = Color.yellow;

    public Color col_force = Color.green;
    public Color col_suspention = Color.magenta;



    private float current_steer_angle;
    Vector3 rotation ;
    bool can_accelerate = true;
    private Rigidbody rb;

    
    void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();
        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;
    }

    private void Update()
    {
        Forward = transform.forward;
        //ProcessSuspension();
        //GripTheGround();
    }
    void FixedUpdate()
    {
        ProcessSuspension();
        GripTheGround();
    }



    void ProcessSuspension() 
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength + wheelRadius))
        {
            lastLength = springLength;

            springLength = hit.distance - wheelRadius;

            springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;

            springLength = Mathf.Clamp(springLength, minLength, maxLength);

            springForce = springStiffness * (restLength - springLength);
            damperForce = damperStiffness * springVelocity;
            suspensionForce = (springForce + damperForce) * transform.up;
            rb.AddForceAtPosition(suspensionForce, hit.point);
            //Debug.DrawRay(hit.point, suspensionForce, Color.yellow, Vector3.SqrMagnitude(suspensionForce));

            suspention_dir = suspensionForce;
        }
    }

    void GripTheGround()
    {
        
            // the direction the tire hates to slide into
            
            Vector3 steerDir = transform.right;
            
            // the speed vector of this tire
            Vector3 tireWorldVel = rb.GetPointVelocity(transform.position);
            
            //the speed value in the sliding direction :to get it we had to break the tireWorldVel into two vectors
            // the tire's forward (ok to slide on that one) and the right direction (not ok to slide)
            // 
            float steeringVal = Vector3.Dot(steerDir, tireWorldVel);
            // to eliminate this force we just counter it by reversing its direction and maybe add a bit of 
            // range (0-1) to how much % we want to counter this unwanted movement 
            float desiredVelChange = -steeringVal * Grip;

            // turn the change in velocity to an acceleration using the formula (accel = vel/time)
            //this generates the necessary acceleration to change the velocity by desiredchange in 1
            // physics step
            float desiredAccel = desiredVelChange / Time.fixedDeltaTime;


            //Force = mass * acceleration
            //Debug.Log(steerDir * tireMass * desiredAccel);
            rb.AddForceAtPosition((steerDir * tireMass* desiredAccel), transform.position);
            current_applied_force_dir += (steerDir * tireMass * desiredAccel);


    }


    //<summary>
    //pass input to apply acceleration | leave empty and call to handle frictions
    //</summary>
    public void Accelerate(float Input) 
    {

        
        Vector3 AccelDir = this.transform.forward;
        float car_speed = Vector3.Dot(rb.transform.forward, rb.velocity);
        if (Input != 0 && driving && can_accelerate)
        {

            float normalized_speed = Mathf.Clamp01(Mathf.Abs(car_speed / Max_Speed));
            float available_torque = PowerCurve.Evaluate(normalized_speed) * Input;
            //Debug.Log(normalized_speed);

            Vector3 force_applied = (AccelDir * available_torque * Max_Speed);
            rb.AddForceAtPosition(force_applied, this.transform.position);
            current_applied_force_dir += force_applied;
        }



        if(Input == 0 ) 
        //car not taking any input but it has 
        {

           
            
            Vector3 dir;

            Vector3 tireWorldVel = rb.GetPointVelocity(transform.position);
            if (Vector3.Dot(tireWorldVel, transform.forward) > 0) 
            {
                //going forwrd
                dir = transform.forward;
                
            }
            else 
            {
                //going backwards
                dir = -transform.forward;
               
            }
           
            float val_on_dir = Vector3.Dot(dir, tireWorldVel);
            float desiredVelChange = val_on_dir * Idle_friction;

          
            float desiredAccel = desiredVelChange / Time.fixedDeltaTime;


            //Force = mass * acceleration
            Vector3 force_applied = (-dir *Idle_friction_Base * tireMass * desiredAccel);
           
            rb.AddForceAtPosition(force_applied, transform.position);
            current_applied_force_dir += force_applied;
           // Debug.DrawRay(transform.position, force_applied,Color.magenta,Vector3.SqrMagnitude(force_applied));
        }
    }
    public void Steer(float Input) 
    {
        if (!steerable)
            return;

        rotation = Vector3.zero;

        current_steer_angle = MaxSteeringAngle * Input;


        rotation.y = current_steer_angle+ transform.parent.transform.eulerAngles.y;

        this.transform.eulerAngles = rotation;
        
    }
    

    //<Summary>
    // set ranged_input to true if you are using a vr device with input from 0 -> 1
    // otherwise it would use the animation curve to get value
    //</Summary>
    public void Brake(float Input,bool ranged_input = false) 
    {

        if (Input <= 0.01)
        {
            can_accelerate = true;
            return;
        }
        else 
        {
            can_accelerate = false;
        }

        Vector3 fwd = transform.forward;
        Vector3 tireWorldVel = rb.GetPointVelocity(transform.position);

        float brake_mult;

        if(ranged_input)
            brake_mult = BrakingCurve.Evaluate(Input); 
        else
            brake_mult = BrakingCurve.Evaluate((tireWorldVel / Max_Speed).magnitude);



        Vector3 dir;

        
        if (Vector3.Dot(tireWorldVel, transform.forward) > 0)
        {
            //going forwrd
            dir = transform.forward;

        }
        else
        {
            //going backwards
            dir = -transform.forward;

        }

        float val_on_dir = Vector3.Dot(dir, tireWorldVel);
        float desiredVelChange = val_on_dir * Idle_friction;


        float desiredAccel = desiredVelChange / Time.fixedDeltaTime;


        //Force = mass * acceleration
        Vector3 force_applied = (dir * brake_mult * tireMass * desiredAccel);

        rb.AddForceAtPosition(force_applied, transform.position);
        current_applied_force_dir += force_applied;
    }



    #region GIZMOS
#if UNITY_EDITOR

    private void OnDrawGizmos()
    {

        //full spring   
        Debug.DrawLine
        (
            transform.position,
            transform.position + Vector3.down * restLength,
            col_rest
        );


        //actual spring
        Debug.DrawLine
        (
           transform.position,
           transform.position + Vector3.down * ((2 * wheelRadius)),
            col_spring
        ) ;


        //check for playmode 
        if (Application.isPlaying)
        {
            //applied force   
            Debug.DrawLine
            (
                transform.position,
                transform.position+current_applied_force_dir.normalized,
                col_force
            );


            //applied suspension   
            Debug.DrawLine
            (
                transform.position,
                transform.position+suspention_dir.normalized,
                col_suspention

            );
        }

    }
#endif
#endregion
    }
