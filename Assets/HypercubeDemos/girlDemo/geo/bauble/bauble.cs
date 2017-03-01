using UnityEngine;
using System.Collections;

public class bauble : touchscreenTarget 
{
    public float triggerRange = 20f;
    public float clickTimeTrigger = .4f;

    public float respawnDistance = 300f; //distance at which to move it to a position close to the hypercube

    public baublePop baublePopPrefab;

    float clickTimer;

    cubeMover cube;

    void Start()
    {
        cube = GameObject.FindObjectOfType<cubeMover>();
    }

    void Update()
    {
        clickTimer -= Time.deltaTime;

        if (!utils.isWithinGridRange2D(transform.position, cube.transform.position, respawnDistance))
        {
            respawn();
        }
    }

    public void respawn()
    {
        //move ourselves closer to the cube
        transform.position = utils.getRandomPointAtDistanceFrom(cube.transform.position, respawnDistance);
    }

    public override void onTouchDown(int uid, Vector2 position)
    {
        if (!gameObject.activeSelf)
            return;

        if (utils.isWithinGridRange2D(transform.position, cube.transform.position, triggerRange))
            clickTimer = clickTimeTrigger;
    }

    public override void onTouchUp(int uid, Vector2 position)
    {
        if (!gameObject.activeSelf)
            return;

        if (clickTimer > 0) //pop the bauble!
        {
            Instantiate(baublePopPrefab, transform.position, Quaternion.Euler(270f,0f,0f));
            respawn(); //hide ourselves

            //notify the girl
            girl g = GameObject.FindObjectOfType<girl>();
            g.foundBobble(transform.position);
        }
    }

}
