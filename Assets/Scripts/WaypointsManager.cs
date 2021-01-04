using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointsManager : MonoBehaviour
{

    public GameObject prefabWaypoint;

    public List<GameObject> waypointList;

    GameObject robot;
    robotScript rb_script;

    int numWaypoints = 1;

    //LineRenderer lineRenderer;

    //lineRenderer colors
    public Color c1 = Color.yellow; //yellow
    public Color c2 = Color.red;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        waypointList = new List<GameObject>(100);
    }

    // Start is called before the first frame update
    void Start()
    {
        robot = GameObject.Find("Robot");
        rb_script = robot.GetComponent<robotScript>();

        // First point will be the position of the robot game object
        waypointList.Add(robot);

        //lineRenderer instantiation

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = .1f;
        lineRenderer.positionCount = rb_script.path_segments + 1;
        lineRenderer.sortingOrder = 2;
        lineRenderer.useWorldSpace = true;

        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;
    }

    // Update is called once per frame
    void Update()
    {
        if (rb_script.isEnded)
        {
            if (!rb_script.isRunning)
                generatePath();
            else
                lineRenderer.positionCount = 0;
        }
    }

    public void SpawnWaypoint()
    {
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
        Vector3 objPos = Camera.main.ScreenToWorldPoint(mousePos);

        waypointList.Add((GameObject)Instantiate(prefabWaypoint, objPos, Quaternion.identity));

        ObjectDrag script = waypointList[numWaypoints].GetComponent<ObjectDrag>();

        script.isAttached = true;

        numWaypoints++;
    }

    public void generatePath()
    {
        // Reset the line renderer
        lineRenderer.positionCount = 0;

        if (numWaypoints > 1)
        {
            // Reset the DLL generator
            rb_script.resetGenerator();

            // First waypoint in the generator will be the robot starting position
            rb_script.addWaypoint(waypointList[0].GetComponent<Transform>().position.x, waypointList[0].GetComponent<Transform>().position.y, 0);

            // Add all subsequent waypoints to the generator, adjusted based on the x and y offset of the starting position
            for(int i = 1; i < numWaypoints; i++)
            {
                rb_script.addWaypoint(waypointList[i].GetComponent<Transform>().position.x,
                                        waypointList[i].GetComponent<Transform>().position.y,
                                        waypointList[i].GetComponent<ObjectDrag>().angle);
            }

            // Generate the path
            rb_script.generatePath();

            // Retrieve the path from the generator
            float[,] path;
            path = rb_script.getPath();

            lineRenderer.positionCount = path.GetLength(0);
            rb_script.path_segments = path.GetLength(0);

            // Use the line renderer to display the generated path
            for(int i = 0; i < path.GetLength(0); i++)
            {
                lineRenderer.SetPosition(i, new Vector3(path[i, 0],
                                                        path[i, 1],
                                                        10f));
            }
            
        }
    }
}
