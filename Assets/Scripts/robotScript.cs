using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Math = System.Math;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;
using IntPtr = System.IntPtr;
using System.Runtime.InteropServices;

//vector-specific usages
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class robotScript : MonoBehaviour
{
    public Rigidbody2D robot_body;

    //control arrays, to be filled with linearVel, angularVel, and timestamps from C++ execution
    public float[] linear_vel = new float[]{ 500f, 20f, 2.5f };
    float[] angularVel = new float[]{ .10f, .5f, .25f };
    float[] time_stamps = new float[]{ 7f, 2f, 1f };

    //segment number, to be filled with # of segments in path (# of individual timestamps)
    public int path_segments;
    
    // Trajectory Iteration variables
    public int curr_point = 0; 
    float timeLeft;
    float runningTime = 0;

    public bool isRunning = false;
    public bool isEnded = true;

    // MATHEMATICAL CONSTANTS
    const float RAD_TO_DEG = 180.0f / (float)Math.PI;

    // FIELD CONSTANTS

    public const float INCHES_TO_UNITS = 0.044243f;
    public const float BOTTOM_LEFT_X = -16.83f;
    public const float BOTTOM_LEFT_Y = -9.21f;

    RobotTrailRenderer robotTrailScript;

    WaypointsManager waypointsManager;

    #region DLL Entry Point Functions

    // Pointer to the MotionProfiling DLL instance
    IntPtr pClassObj;

    [DllImport("Trajectory Generator")]
    public static extern IntPtr MotionProfiling_Init();

    [DllImport("Trajectory Generator")]
    public static extern void resetGenerator(IntPtr pClassNameObject);

    [DllImport("Trajectory Generator")]
    public static extern void setTrajectoryConstants(IntPtr pClassNameObject, float maxJerk, float maxAcc, float maxDeacc, float maxVel);

    [DllImport("Trajectory Generator")]
    public static extern void setPathGenerationConstants(IntPtr pClassNameObject, float maxStepX, float maxStepY, float maxStepThetha, float lookaheadDist);

    [DllImport("Trajectory Generator")]
    public static extern void setRobotConstants(IntPtr pClassNameObject, float driveRadius, float driveWidth);

    [DllImport("Trajectory Generator")]
    public static extern void addWaypoint(IntPtr pClassNameObject, double x, double y, double degAngle);

    [DllImport("Trajectory Generator")]
    public static extern bool generatePath(IntPtr pClassNameObject);

    [DllImport("Trajectory Generator")]
    public static extern bool generateTrajectory(IntPtr pClassNameObject, float startVel, float endVel, float curveConst, int lookaheadDist);

    [DllImport("Trajectory Generator")]
    public static extern bool runPurePursuitController(IntPtr pClassNameObject);

    [DllImport("Trajectory Generator")]
    public static extern int getPathSize(IntPtr pClassNameObject);

    [DllImport("Trajectory Generator")]
    public static extern double getPathXPos(IntPtr pClassNameObject, int index);

    [DllImport("Trajectory Generator")]
    public static extern double getPathYPos(IntPtr pClassNameObject, int index);

    [DllImport("Trajectory Generator")]
    public static extern int getTrajectorySize(IntPtr pClassNameObject);

    [DllImport("Trajectory Generator")]
    public static extern float getVel(IntPtr pClassNameObject, int index);

    [DllImport("Trajectory Generator")]
    public static extern float getAngVel (IntPtr pClassNameObject, int index);

    [DllImport("Trajectory Generator")]
    public static extern float getTime(IntPtr pClassNameObject, int index);

    [DllImport("Trajectory Generator")]
    public static extern float updatePurePursuitController(IntPtr pClassNameObject, double robotX, double robotY, double robotTheta);

    [DllImport("Trajectory Generator")]
    public static extern double getNextVel(IntPtr pClassNameObject);

    [DllImport("Trajectory Generator")]
    public static extern double getNextAngVel(IntPtr pClassNameObject);

    #endregion

    Color getColorFromVelocity(float velocity)
    {
        Gradient velocityGradient = new Gradient();
        return velocityGradient.Evaluate(Mathf.Min(velocity/250, 1));
    }
    
    private void Awake()
    {
        // Get the 3D rigid body for the robot game object this script is assigned to
        robot_body = GetComponent<Rigidbody2D>();

        // Initialize the Trajectory DLL
        pClassObj = MotionProfiling_Init();

        // Set initial constants for the generator
        setTrajectoryConstants(pClassObj, 10f, 168f, -72f, 120f);
        setPathGenerationConstants(pClassObj, 2f, 0.25f, 0.1f, 15f);
        setRobotConstants(pClassObj, 12.5f, 25f);

        robotTrailScript = GetComponent<RobotTrailRenderer>();
        waypointsManager = GameObject.Find("AddWaypointButton").GetComponent<WaypointsManager>();

        isEnded = true;
    }

    void FixedUpdate()
    {
        if(isRunning)
        {
            // Update the pure pursuit with the current position 
            float xPos = robot_body.position.x;
            float yPos = robot_body.position.y;
            float angle = robot_body.transform.rotation.z;
            unitsToInchPosition(ref xPos, ref yPos);

            //while main execution hasn't gone through all segments
            if (curr_point < path_segments)
            {
                float output = updatePurePursuitController(pClassObj, (double)xPos, (double)yPos, (double)angle);

                // Currently outputs NaN
                float velocity = (float)getNextVel(pClassObj) * INCHES_TO_UNITS;
                float angularVelocity = (float)getNextAngVel(pClassObj) * RAD_TO_DEG;

                // Use output of the pure pursuit controller to update the velocity and angular velocity

                // When to stop?
                // When the current position is within a certain tolerance of the final position?
                // Indication from DLL?

                // calculate linear velocity in-game from theoretical linear velocity, multiply by relative-right facing vector 
                // and convert from in/s to units/s
                //robot_body.velocity = transform.right * velocity;
                robot_body.velocity = transform.right * (linear_vel[curr_point] * INCHES_TO_UNITS);

                // calculate angular velocity in game from theoretical angular velocity, multiply by rad_to_deg, no physics transform required
                //robot_body.angularVelocity = angularVelocity;
                robot_body.angularVelocity = angularVel[curr_point] * RAD_TO_DEG;

                //subtract last frame delta_t from current segment time
                runningTime += Time.fixedDeltaTime;
                //if current segment time is up, reset timeLeft and proceed to next segment
                if ((timeLeft - runningTime) <= 0)
                {
                    curr_point++;
                    if (curr_point < path_segments)
                    {
                        timeLeft = time_stamps[curr_point];
                    }
                }
            }
            else
            {
                //when finished execution, make robot stationary
                robot_body.velocity = Vector2.zero;
                robot_body.angularVelocity = 0;
                isRunning = false;
                isEnded = true;
                curr_point = 0;
                robotTrailScript.clearPathVisualization();
                timeLeft = 0;
                runningTime = 0;
            }
        }
        
    }

    public void inchesToUnitPosition(ref float x, ref float y)
    {
        x = x * INCHES_TO_UNITS + BOTTOM_LEFT_X;
        y = y * INCHES_TO_UNITS + BOTTOM_LEFT_Y;
    }

    public void unitsToInchPosition(ref float x, ref float y)
    {
        x = (x - (BOTTOM_LEFT_X)) / INCHES_TO_UNITS;
        y = (y - (BOTTOM_LEFT_Y)) / INCHES_TO_UNITS;
    }

    public void resetGenerator()
    {
        resetGenerator(pClassObj);
    }

    public void addWaypoint(float x, float y, double angle)
    {
        unitsToInchPosition(ref x, ref y);
        addWaypoint(pClassObj, x, y, angle);
    }

    public void setTrajectoryConstants(float maxJerk, float maxAcc, float maxDeacc, float maxVel)
    {
        setTrajectoryConstants(pClassObj, maxJerk, maxAcc, maxDeacc, maxVel);
    }

    public void setPathGenerationConstants(float maxStepX, float maxStepY, float maxStepThetha, float lookaheadDist)
    {
        setPathGenerationConstants(pClassObj, maxStepX, maxStepY, maxStepThetha, lookaheadDist);
    }

    public void setRobotConstants(float driveRadius, float driveWidth)
    {
        setRobotConstants(pClassObj,driveRadius, driveWidth);
    }

    public void generatePath()
    {
        bool pathGenerated = generatePath(pClassObj);
        if (!pathGenerated)
            Debug.Log("Path generation failed");
    }

    public float[,] getPath()
    {
        int pathSize = getPathSize(pClassObj);
        float[,] path = new float[pathSize, 2];

        for (int i = 0; i < pathSize; i++)
        {
            path[i, 0] = (float)getPathXPos(pClassObj, i);
            path[i, 1] = (float)getPathYPos(pClassObj, i);

            inchesToUnitPosition(ref path[i, 0], ref path[i, 1]);
        }

        return path;
    }

    public void generateTrajectory()
    {

        bool generated = generateTrajectory(pClassObj, 0, 0, 2, 2);
        //runPurePursuitController(pClassObj);

        int trajectorySize = getTrajectorySize(pClassObj);

        linear_vel = new float[trajectorySize];
        angularVel = new float[trajectorySize];
        time_stamps = new float[trajectorySize];

        for (int i = 0; i < trajectorySize; i++)
        {
            linear_vel[i] = getVel(pClassObj, i);
            time_stamps[i] = getTime(pClassObj, i);
            angularVel[i] = getAngVel(pClassObj, i);
        }

        path_segments = linear_vel.Length;
        timeLeft = time_stamps[0];

        //resetting segment length
        robotTrailScript.resetPathVisualization();
        isRunning = true;
        isEnded = false;
    }
}
