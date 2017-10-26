using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneControls : MonoBehaviour
{

    //This script should be attached to the GameObject representing the core of the airplane
    //The "forward" of the attached object (Vector3.forward) is considered the "forward" of the plane

    //Recall Unity is a left handed coordinate system

    const float MAX_PLANE_SPEED = 10.0f; //The maximum speed of the plane
    const float MIN_PLANE_SPEED = 3.0f; //The minimum speed of the plane
    const float PLANE_ACCELERATION = 3.0f; //Determines the acceleration used to accelerate/decelerate
    float currSpeed = MIN_PLANE_SPEED; //Current speed of the plane

    const float MAX_PITCH_DEG_PER_FRAME = 1.0f; //Maximum number of degrees that you can pitch every frame
    const float MAX_YAW_DEG_PER_FRAME = 1.0f; //Maximum number of degrees that you can yaw every frame
    const float MAX_ROLL_DEG_PER_FRAME = 1.0f; //Maximum number of degrees that you can roll every frame

    const float LASER_RANGE = 50.0f; //The effective range of the plane's lasers (distance to send the raycast ray)
    const float MAX_LASER_FRAME_COUNT = 10; //The max amount of frames to have the laser be rendered on the screen
    float currLaserFrameCount = 0; //Counter for how many frames the laser has been rendered on the screen
    bool isLaserRendered = false; //Flag indicating if the laser is currently rendered on the screen
    LineRenderer laserLineRenderer; //The LineRenderer used for rendering the laser on the screen

    //Control how much the thumbsticks/triggers have to be pushed/squeezed before we 
    //  consider it a valid input
    const float CONTROLLER_AXIS_TOLERANCE = 0.1f;

    // Use this for initialization
    void Start()
    {

        //Initialize the laser's line renderer
        laserLineRenderer = gameObject.AddComponent<LineRenderer>();
        laserLineRenderer.widthMultiplier = 0.2f;
        laserLineRenderer.startColor = Color.green;
        laserLineRenderer.endColor = Color.blue;
        laserLineRenderer.positionCount = 2;

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetAxis("RightTrigger") >= CONTROLLER_AXIS_TOLERANCE)
        {
            GameObject struckObject = ShootLaser();
            if (struckObject != null)
            {
                struckObject.SetActive(false);
            }
        }

        //Stop rendering the laser's LineRenderer if it has been on the screen for too long
        if (isLaserRendered)
        {
            currLaserFrameCount++;
            if (currLaserFrameCount >= MAX_LASER_FRAME_COUNT)
            {
                currLaserFrameCount = 0;
                laserLineRenderer.SetPosition(0, Vector3.zero);
                laserLineRenderer.SetPosition(1, Vector3.zero);
                isLaserRendered = false;
            }
        }


        RotatePlane();
        AcceleratePlane();

    }

    /// <summary>
    /// Accelerate/Decelerate the plane by reading the controller input and translating the plane appropriately
    /// 
    /// Note: This function modifies the following global variables
    ///     * currSpeed
    /// </summary>
    private void AcceleratePlane()
    {
        currSpeed = DetermineSpeed(currSpeed);
        transform.Translate(Vector3.forward * Time.deltaTime * currSpeed);
    }

    /// <summary>
    /// Rotates the plane according to the controller input
    /// 
    /// Yaw: Left Joystick L-R
    /// Pitch: Left Joystick U-D
    /// Roll: Right Joystick L-R
    /// 
    /// </summary>
    private void RotatePlane()
    {

        float leftJoystickLRVal = Input.GetAxis("LeftJoystickLR");
        if (Mathf.Abs(leftJoystickLRVal) >= CONTROLLER_AXIS_TOLERANCE)
        {
            YawBy(leftJoystickLRVal * MAX_YAW_DEG_PER_FRAME);
        }

        float leftJoystickUDVal = Input.GetAxis("LeftJoystickUD");
        if (Mathf.Abs(leftJoystickUDVal) >= CONTROLLER_AXIS_TOLERANCE)
        {
            PitchBy(leftJoystickUDVal * MAX_PITCH_DEG_PER_FRAME);
        }

        float rightJoystickLRVal = Input.GetAxis("RightJoystickLR");
        if (Mathf.Abs(rightJoystickLRVal) >= CONTROLLER_AXIS_TOLERANCE)
        {
            RollBy(rightJoystickLRVal * MAX_ROLL_DEG_PER_FRAME);
        }


    }

    /// <summary>
    /// Determine the new speed of the plane by taking into consideration acceleration/deceleration inputs
    ///     of the controller
    ///     
    /// If no controller input is given, currSpeed is returned
    /// </summary>
    /// <param name="currSpeed"></param>
    /// <returns>The new speed of the plane after considering all acceleration/deceleration inputs</returns>
    private float DetermineSpeed(float currSpeed)
    {
        if (Input.GetButton("AccelerateButton"))
        {
            return Mathf.Min(AddAceleration(currSpeed, PLANE_ACCELERATION, Time.deltaTime), MAX_PLANE_SPEED);
        }
        else if (Input.GetButton("DecelerateButton"))
        {
            return Mathf.Max(AddAceleration(currSpeed, -PLANE_ACCELERATION, Time.deltaTime), MIN_PLANE_SPEED);
        }

        return currSpeed;

    }



    /// <summary>
    /// Returns the new speed of the object after acceleration has been applied to it
    /// </summary>
    /// <param name="currSpeed">The current speed of the gameObject</param>
    /// <param name="acceleration">The acceleration you want to use</param>
    /// <param name="deltaTime">The time elapsed between the previous frame (Time.deltaTime)</param>
    /// <returns></returns>
    private float AddAceleration(float currSpeed, float acceleration, float deltaTime)
    {
        return currSpeed + acceleration * deltaTime;
    }


    //Rotate the plane by pitching by pitchAngle 
    private void PitchBy(float pitchAngle)
    {
        transform.Rotate(Vector3.left, pitchAngle);
    }

    //Rotate the plane by rolling by rollAngle
    private void RollBy(float rollAngle)
    {
        transform.Rotate(Vector3.forward, rollAngle);
    }

    //Rotate the plane by yawing by yawAngle
    private void YawBy(float yawAngle)
    {
        transform.Rotate(Vector3.up, yawAngle);
    }

    //TODO: optimize this with layermask
    /// <summary>
    /// Use a raycast to see if we hit anotherh GameObject
    ///     
    /// If we do hit a targert, return the GameObject that we hit.
    /// If we didn't hit a target, return null
    /// </summary>
    /// <param name="origin">The origin point of the ray</param>
    /// <param name="direction">The direction to point the ray (w.r.t local coordinates)</param>
    /// <param name="rayDistance">The distance (in Unity units) to extend they ray</param>
    /// <returns></returns>
    private GameObject RaycastDetect(Vector3 origin, Vector3 direction, float rayDistance)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(origin, direction, out hitInfo, rayDistance))
        {
            return hitInfo.collider.gameObject;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Use Raycasting to see if a ray originating from the plane core, pointing in the direction
    ///     of the plane's cockpit, and of length LASER_RANGE strikes a target.
    ///     
    /// Also, render the laser onto the screen pointing in the direction of the raycast ray
    /// 
    /// Note: This function modifies the following global variables:
    ///     * isLaserRendered is set to true
    /// </summary>
    /// 
    /// <returns>The GameObject that we struck</returns>
    private GameObject ShootLaser()
    {
        GameObject struckObject = RaycastDetect(transform.position, transform.forward, LASER_RANGE);

        //Render the laser
        laserLineRenderer.SetPosition(0, transform.position);
        laserLineRenderer.SetPosition(1, transform.position + transform.forward * LASER_RANGE);
        isLaserRendered = true;

        return struckObject;
    }

}
