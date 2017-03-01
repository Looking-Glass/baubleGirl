using UnityEngine;
using System.Collections;

public class GS_runOff : girlState
{
    public float runningTime = 4f;
    public float runSpeed = 1f;

    float timer = 1000f;

    public override void enterState(girl g)
    {
        timer = runningTime;
        g.girlObject.transform.position = utils.getPointOnGroundAt(g.girlPelvis.position);
        g.girlObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        g.girlObject.GetComponent<randomSound>().isPlaying = true; //running sounds
    }

    public override void updateState(girl g)
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            g.setState(girlStateType.ST_HIDDEN);
            return;
        }

        Vector3 newPos = g.girlObject.transform.position;
        newPos.z -= runSpeed * Time.deltaTime;
        newPos = utils.getPointOnGroundAt(newPos); //adjust for terrain
        g.girlObject.transform.position = newPos;
    }

    public override void endState(girl g)
    {

        g.girlObject.transform.rotation = Quaternion.identity;
        g.cube.GetComponent<cubeMover>().anchor(false);
        timer = runningTime;

        g.girlObject.GetComponent<randomSound>().isPlaying = false;
    }

}
