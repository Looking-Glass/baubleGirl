using UnityEngine;
using System.Collections;

//this is a pure idle state

public class GS_init : girlState 
{


    public override void enterState(girl g)
    {
        g.girlObject.SetActive(true);
        g.cube.GetComponent<cubeMover>().anchor(true);
        g.playTrigger("reset");
        g.shinyTrail.SetActive(false);
    }

    public override void updateState(girl g)
    {

    }

    public override void endState(girl g)
    {

    }

    public override void injectTouch(girl g, Vector3 worldPos)  //if a touch occurs greet the player
    {
        g.setState(girlStateType.ST_POSE1);
    }
}
