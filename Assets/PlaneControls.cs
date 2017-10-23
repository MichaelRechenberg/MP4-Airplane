using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneControls : MonoBehaviour
{

    //This script is attached to the GameObject representing the core of the airplane

    //Recall Unity is a left handed coordinate system

    const float MAX_PLANE_SPEED = 10.0f; //The maximum speed of the plane
    const float MIN_PLANE_SPEED = 3.0f; //The minimum speed of the plane
    const float PLANE_ACCELERATION = 3.0f; //Determines the acceleration used to accelerate/decelerate
    float currSpeed = MIN_PLANE_SPEED; //Current speed of the plane

    const float MAX_PITCH_DEG_PER_FRAME = 1.0f; //Maximum number of degrees that you can pitch every frame
    const float MAX_YAW_DEG_PER_FRAME = 1.0f; //Maximum number of degrees that you can yaw every frame
    const float MAX_ROLL_DEG_PER_FRAME = 1.0f; //Maximum number of degrees that you can roll every frame

    const float LASER_RANGE = 50.0f; //The effective range of the plane's lasers (distance to send the raycast ray)
    const float LASER_NUM_FRAMES = 5; //The number of frames to have the laser be rendered on the screen
    float currLaserFrameCount = 0; //Counter for how many frames the laser has been rendered on the screen
    bool isLaserRendered = false; //Is the laser currently rendered on the screen?
    LineRenderer laserLineRenderer; //Renderer for actually rendering the laser

    // Use this for initialization
    void Start()
    {

        //Initialize the laser's line renderer
        laserLineRenderer = gameObject.AddComponent<LineRenderer>();
        laserLineRenderer.widthMultiplier = 0.2f;
        laserLineRenderer.startColor = Color.green;
        laserLineRenderer.endColor = Color.blue;
        laserLineRenderer.positionCount = 2;

        Debug.Log(Input.GetJoystickNames()[0]);
        Debug.Log(Input.GetJoystickNames().Length);
    }

    // Update is called once per frame
    void Update()
    {





        //Handle rotation
        //TODO: Replace these calls to calls using XBOX Controller
        if (Input.GetKey(KeyCode.W))
        {
            pitchBy(1 * MAX_PITCH_DEG_PER_FRAME);
        }
        if (Input.GetKey(KeyCode.S))
        {
            pitchBy(-1 * MAX_PITCH_DEG_PER_FRAME);
        }

        if (Input.GetKey(KeyCode.D))
        {
            yawBy(MAX_YAW_DEG_PER_FRAME);
        }
        if (Input.GetKey(KeyCode.A))
        {
            yawBy(-MAX_YAW_DEG_PER_FRAME);
        }

        if (Input.GetKey(KeyCode.E))
        {
            rollBy(MAX_ROLL_DEG_PER_FRAME);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            rollBy(-MAX_ROLL_DEG_PER_FRAME);
        }

        //Shoot a raycast and see if we hit the target
        //Render the laser as well
        if (Input.GetKey(KeyCode.V))
        {
            GameObject struckObject = raycastDetect(transform.position, transform.forward, LASER_RANGE);

            //Render the laser
            laserLineRenderer.SetPosition(0, transform.position);
            laserLineRenderer.SetPosition(1, transform.position + transform.forward * LASER_RANGE);
            isLaserRendered = true;

            //Remove the object we hit
            if (struckObject != null)
            {
                Debug.Log("YOU HIT A TARGET!!!");
                currLaserFrameCount = 0;
                struckObject.SetActive(false);
            }
        }

        //Disable the laser renderer if it has been on screen too long
        if (isLaserRendered)
        {
            currLaserFrameCount++;
            if (currLaserFrameCount >= LASER_NUM_FRAMES)
            {
                currLaserFrameCount = 0;
                laserLineRenderer.SetPosition(0, Vector3.zero);
                laserLineRenderer.SetPosition(1, Vector3.zero);
                isLaserRendered = false;
            }
        }

        if (Input.GetAxis("LeftJoystickLR") != 0)
        {
            //TODO: Yaw
            Debug.Log("Left joystick pressed LR");
            float leftJoyVal = Input.GetAxis("LeftJoyStickLR");
            Debug.Log(leftJoyVal);
        }
        if (Input.GetAxis("LeftJoystickUD") != 0)
        {
            //TODO: Pitch
            Debug.Log("Left joystick pressed UD");
            float leftJoyVal = Input.GetAxis("LeftJoyStickUD");
            Debug.Log(leftJoyVal);
        }
        if (Input.GetAxis("RightTrigger") != 0)
        {
            //TODO: Positive Roll
            Debug.Log("Right trigger was pressed");
            float triggerVal = Input.GetAxis("RightTrigger");
            Debug.Log(triggerVal);
        }
        if (Input.GetAxis("LeftTrigger") != 0)
        {
            //TODO: Positive Roll
            Debug.Log("Left trigger was pressed");
            float triggerVal = Input.GetAxis("LeftTrigger");
            Debug.Log(triggerVal);
        }




        //Handle acceleration
        //TODO: refactor the Math.Min stuff out
        if (Input.GetButton("AccelerateButton"))
        {
            Debug.Log("The acceleration button was pressed");
            currSpeed = Mathf.Min(ApplyAcceleration(currSpeed, PLANE_ACCELERATION, Time.deltaTime), MAX_PLANE_SPEED);
        }
        else if (Input.GetButton("DecelerateButton"))
        {
            Debug.Log("The deceleration button was pressed");
            currSpeed = Mathf.Max(ApplyAcceleration(currSpeed, -PLANE_ACCELERATION, Time.deltaTime), MIN_PLANE_SPEED);
        }

        //Move the plane forward at its current speed
        transform.Translate(Vector3.forward * Time.deltaTime * currSpeed);

    }

    /// <summary>
    ///     Returns the speed of the object after the acceleration has been applied
    /// </summary>
    /// <param name="currSpeed">The current speed of the gameObject</param>
    /// <param name="acceleration">The acceleration you want to use</param>
    /// <param name="deltaTime">The time elapsed between the previous frame (Time.deltaTime)</param>
    /// <returns></returns>
    private float ApplyAcceleration(float currSpeed, float acceleration, float deltaTime)
    {
        return currSpeed + acceleration * deltaTime;
    }


    //TODO: Docstring this?
    //Rotate the plane by pitching by a pitchAngle 
    private void pitchBy(float pitchAngle)
    {
        transform.Rotate(Vector3.left, pitchAngle);
    }

    //Rotate the plane by rolling by a follAngle
    private void rollBy(float rollAngle)
    {
        transform.Rotate(Vector3.forward, rollAngle);
    }

    //Rotate the plane by yawing by a yawAngle
    private void yawBy(float yawAngle)
    {
        transform.Rotate(Vector3.up, yawAngle);
    }

    //TODO: optimize this with layermask
    /// <summary>
    ///     Use a raycast to see if we hit a target.
    ///     If we do hit a targert, return the gameObject that we hit.
    ///     If we hit no targets, return null
    /// </summary>
    /// <param name="origin">The origin point of the ray</param>
    /// <param name="direction">The direction to point the ray (w.r.t local coordinates)</param>
    /// <param name="rayDistance">The distance (in Unity units) to extend they ray</param>
    /// <returns></returns>
    private GameObject raycastDetect(Vector3 origin, Vector3 direction, float rayDistance)
    {
        //TODO: remove debug info
        //Debug.DrawRay(origin, direction * rayDistance, Color.red, 5.0f);
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
}
