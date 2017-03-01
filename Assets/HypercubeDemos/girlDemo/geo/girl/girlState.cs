using UnityEngine;
using System.Collections;


public enum girlStateType
{
    ST_INVALID = 0,
    ST_INIT,
    ST_ARRIVING,
    ST_POSE1,
    ST_POSE2,
    ST_POSE3,
    ST_DONE,
    ST_RUNOFF,
    ST_HIDDEN
}

public class girlState : MonoBehaviour {

    public  girlStateType stateType = girlStateType.ST_INVALID;

   
	public virtual void enterState(girl g)
    {
      
    }

    public virtual void updateState(girl g)
    {

    }

    public virtual void endState(girl g)
    {

    }

    public virtual void injectTouch(girl g, Vector3 worldPos)
    {

    }
}
