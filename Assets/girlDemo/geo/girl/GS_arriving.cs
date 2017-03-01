using UnityEngine;
using System.Collections;

public class GS_arriving : girlState 
{

    public float runSpeed = 1f;
    public float targetRange = 5f;
    public float distanceToSpawnGirl = 300f;
    public GameObject runToNode;

    public AudioClip trailSound;

    public override void enterState(girl g)
    {    
        g.shinyTrail.SetActive(true);
        g.shinyTrail.transform.position = runToNode.transform.position;

        //spawn the girl in front of the cube.  It doesn't look good when she spawns behind us
        g.girlObject.SetActive(true);
        Vector3 pointToSpawnGirl = utils.getRandomPointAtDistanceFrom(g.cube.transform.position, distanceToSpawnGirl);
        while (pointToSpawnGirl.z >= g.cube.transform.position.z)
        { pointToSpawnGirl = utils.getRandomPointAtDistanceFrom(g.cube.transform.position, distanceToSpawnGirl); }
        g.girlObject.transform.position = pointToSpawnGirl;

        g.girlObject.transform.LookAt(runToNode.transform.position); //aim at the direction we are running.
        
        g.cube.GetComponent<cubeMover>().anchor(true); //make the cube stand still

        g.playTrigger("run");
        g.playSoundEffect(trailSound, .8f);

        g.girlObject.GetComponent<randomSound>().isPlaying = true; //footsteps sounds
    }

    public override void updateState(girl g)
    {
        g.girlObject.transform.position = Vector3.MoveTowards(g.girlObject.transform.position, runToNode.transform.position, runSpeed * Time.deltaTime);
        

        g.girlObject.transform.position = utils.getPointOnGroundAt(g.girlObject.transform.position); //walk on the hills.

        if (utils.isWithinGridRange2D(g.girlObject.transform.position, runToNode.transform.position, targetRange))
            g.setState(girlStateType.ST_POSE1); //GO straight to greeting when she arrives
    }

    public override void endState(girl g)
    {
        g.girlObject.GetComponent<randomSound>().isPlaying = false;
        g.girlObject.transform.rotation = Quaternion.identity;
        g.shinyTrail.SetActive(false);
    }

}
