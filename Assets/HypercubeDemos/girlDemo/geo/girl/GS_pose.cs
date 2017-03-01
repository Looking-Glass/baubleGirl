using UnityEngine;
using System.Collections;

public class GS_pose : girlState 
{

    public GameObject touchTrigger;
    public float touchTriggerRange = 5f;
    public string triggerAnimationState;
    public girlStateType nextState = girlStateType.ST_INVALID;
    public float timeToBored = 7f;

    //public GameObject debugPosition;

    float timer;

    public override void enterState(girl g)
    {
        if (triggerAnimationState != "")
            g.playTrigger(triggerAnimationState);

        timer = timeToBored;
    }

    public override void updateState(girl g)
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            g.playTrigger("bored");
            timer = timeToBored;
        }


        //keep girl from clipping ground
        //g.girlRoot.localPosition.y is the height the girl should be from the ground at this frame.
        Vector3 girlPos = g.girlObject.transform.position;
        Vector3 groundPoint = utils.getPointOnGroundAt(g.girlPelvis.position);
        girlPos.y =  groundPoint.y;
        g.girlObject.transform.position = girlPos;
    }


    public override void endState(girl g)
    {

    }

    public override void injectTouch(girl g, Vector3 worldPos)
    {

        //if (debugPosition)
        //    Instantiate(debugPosition, worldPos, Quaternion.identity);


        timer = timeToBored;
        if (utils.isWithinGridRange(worldPos, touchTrigger.transform.position, touchTriggerRange))
            g.setState(nextState);
        else
            g.playTrigger("mistake");
    }
}
