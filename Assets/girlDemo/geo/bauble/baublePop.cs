using UnityEngine;
using System.Collections;

//a simple timer for the bauble pop

public class baublePop : MonoBehaviour {

    public float timeToLive = 4f;

    //float timer = 0;

    //public void pop(Vector3 pos)
    //{
    //    timer = timeToLive;
    //    transform.position = pos;
    //    gameObject.SetActive(true);
    //}

	
	// Update is called once per frame
	void Update () 
    {
        timeToLive -= Time.deltaTime;
        if (timeToLive < 0)
            Destroy(gameObject); //destroy ourselves
	
	}
}
