using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDragRotate : MonoBehaviour
{

    private Vector3 screenPoint;
    private Vector3 offset;

    public bool isAttached = false;

    public bool isVisible = false;

    public float xSpeed;
    public float ySpeed;

    public GameObject pivot;

    private Vector3 zAxis = new Vector3(0, 0, 1);

    private Vector3 prevMousePosition;
    private Vector2 deltaMouse;

    private void Awake()
    {
       
    }

    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

        prevMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

        isAttached = !isAttached;
    }

    private void OnMouseUp()
    {
        //isAttached = !isAttached;
    }

    void OnMouseDrag()
    {

    }

    private void Update()
    {
            bool top = transform.position.y > transform.parent.transform.position.y ? true : false;
            bool right = transform.position.x > transform.parent.transform.position.x ? true : false;
        if (isAttached)
        {
            /*Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            deltaMouse = new Vector2(curScreenPoint.x - prevMousePosition.x, curScreenPoint.y - prevMousePosition.y);
            prevMousePosition = curScreenPoint;

            xSpeed = deltaMouse.x;
            ySpeed = deltaMouse.y;
            

            float speed = 0;
            
            if (right)
                ySpeed = ySpeed > 0 ? -1 : 1;
            else
                ySpeed = ySpeed > 0 ? 1 : -1;

            if (top)
                xSpeed = xSpeed > 0 ? 1 : -1;
            else
                xSpeed = xSpeed > 0 ? -1 : 1;*/

            GetComponent<SpriteRenderer>().color = new Color(0, 0, 255);

            int direction = 0;

            if (Input.GetKey(KeyCode.UpArrow))
                direction = 1;
            else if (Input.GetKey(KeyCode.DownArrow))
                direction = -1;

            transform.RotateAround(pivot.transform.position, zAxis, direction * 100 * Time.deltaTime);

        }
        else
        {
            transform.RotateAround(pivot.transform.position, zAxis, 0);
            GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
        }
            

        Vector2 positionVector = new Vector2(transform.position.x - pivot.transform.position.x, transform.position.y - pivot.transform.position.y);
        float angle = Vector2.Angle(new Vector2(1, 0), positionVector);
        if (!top)
            angle = -angle;
        pivot.GetComponent<ObjectDrag>().angle = angle;
    }


}
