using UnityEngine;
using System.Collections;

public class bauble : hypercube.touchScreenTarget 
{
    public float triggerRange = 20f;
    public float clickTimeTrigger = .4f;

    public float respawnDistance = 300f; //distance at which to move it to a position close to the hypercube

    public baublePop baublePopPrefab;

    float clickTimer;

    void Update()
    {
        clickTimer -= Time.deltaTime;

        if (!utils.isWithinGridRange2D(transform.position, HoloPlaySDK.HoloPlay.Main.transform.position, respawnDistance))
        {
            respawn();
        }
    }

    public void respawn()
    {
        //move ourselves closer to the cube
        transform.position = utils.getRandomPointAtDistanceFrom(HoloPlaySDK.HoloPlay.Main.transform.position, respawnDistance);
    }

    public override void onTouchDown(hypercube.touch t)
    {
        if (!gameObject.activeSelf)
            return;

        if (utils.isWithinGridRange2D(transform.position, HoloPlaySDK.HoloPlay.Main.transform.position, triggerRange))
            clickTimer = clickTimeTrigger;
    }

    public override void onTouchUp(hypercube.touch t)
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
