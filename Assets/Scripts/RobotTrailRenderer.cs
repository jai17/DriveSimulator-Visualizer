using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RobotTrailRenderer : MonoBehaviour
{
    //lineRenderer colors
    public Color c1 = Color.yellow; //yellow
    public Color c2 = Color.red;

    //access robotScript
    GameObject robot;
    robotScript rb_script;
    Rigidbody2D rb_body;

    // Start is called before the first frame update
    void Start()
    {
        //access robot object with preloaded 
        robot = GameObject.Find("Robot");
        rb_script = robot.GetComponent<robotScript>();
        rb_body = rb_script.robot_body;

        //lineRenderer instantiation
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = .1f;
        lineRenderer.positionCount = rb_script.path_segments + 1;
        lineRenderer.sortingOrder = 1;
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

    //TODO: redundant fixed_update, rendered in WaypointsManager
    // Update is called once per frame
    void FixedUpdate()
    {
        //waypoint animation update with rigidBody position at every frame call
        if (rb_script.isRunning && !rb_script.isEnded)
        {
            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer.positionCount != 0)
                lineRenderer.SetPosition(rb_script.curr_point, new Vector3(rb_body.position.x, rb_body.position.y, 10f));
        }
    }

    public void resetPathVisualization()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = rb_script.path_segments + 1;
    }

    public void clearPathVisualization()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }
}
