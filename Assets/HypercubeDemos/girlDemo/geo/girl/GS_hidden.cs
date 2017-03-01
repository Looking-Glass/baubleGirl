using UnityEngine;
using System.Collections;

public class GS_hidden : girlState
{

    public float cubeStartingInertia = -100f;

    public override void enterState(girl g)
    {
        g.shinyTrail.SetActive(false);
        g.girlObject.SetActive(false);
        g.cube.GetComponent<cubeMover>().inertia.z = cubeStartingInertia;
    }

    public override void updateState(girl g)
    {

    }

    public override void endState(girl g)
    {

    }

    public override void injectTouch(girl g, Vector3 worldPos)  //if a touch occurs greet the player
    {
 
    }
}
