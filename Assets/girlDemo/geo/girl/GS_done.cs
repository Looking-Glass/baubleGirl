using UnityEngine;
using System.Collections;

public class GS_done : girlState
{

    public override void enterState(girl g)
    {      
          g.playTrigger("done");

          g.shinyTrail.SetActive(false);
    }

    public override void updateState(girl g)
    {

    }



}
