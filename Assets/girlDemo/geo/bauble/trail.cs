using UnityEngine;
using System.Collections;


public class trail : MonoBehaviour {

    public Transform target;
    public trailSparkle sparklePrefab;
    public float sparkleTime = .4f;
    public float sparkleLifetime = 5f;

    float spawnTimer = 0f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {

        spawnTimer -= Time.deltaTime;
        if (spawnTimer < 0)
        {
            spawnTimer = sparkleTime;

            GameObject o = Instantiate(sparklePrefab.gameObject, transform.position, Quaternion.identity) as GameObject;
            trailSparkle newSparkle = o.GetComponent<trailSparkle>(); //make a sparkle
            newSparkle.target = target;

        }
            
	}
}
