using UnityEngine;
using System.Collections;



public class cubeMover : hypercube.touchScreenTarget
{

    public float speed = 1f;
    public float friction = .5f;

    public GameObject cube;

    public Vector3 inertia = new Vector3();
    Vector3 currentFrameInertia = new Vector3();

    public float anchorStrength = .5f;
    bool anchored = false;
    Vector3 anchorPoint;

    void Start()
    {

    }

    public void anchor(bool onOff)
    {
      //  if (onOff)
      //      anchorPoint = transform.position; //lerping anchor

        anchored = onOff;
    }

    void Update()
    {
        if (anchored)
            return;

        currentFrameInertia *= Time.deltaTime;
        inertia += currentFrameInertia;

       inertia = Vector3.Lerp(inertia, Vector3.zero, friction * Time.deltaTime);
       cube.transform.position += inertia * Time.deltaTime;

      //  if (anchored) //lerping anchor
      //      cube.transform.position = Vector3.Lerp(cube.transform.position, anchorPoint, anchorStrength * Time.deltaTime);

       currentFrameInertia = Vector3.zero;
    }


    public override void onTouchMoved(hypercube.touch touch)
    {
        if (anchored)
            return;

        currentFrameInertia += new Vector3(touch.diffX * speed, 0, touch.diffY * speed);
    }



}
