using UnityEngine;
using System.Collections;

public class trailSparkle : MonoBehaviour {

    public Transform target;
    public float speed = 1f;
    public float height = .5f;
    public float distanceTrigger = 1f;

    public ParticleSystem loopParticle;

    float dieTime = 1f;  //once the sparkle hits the target, it 'dies'.  It is given some time for the particles to dissipate before destroying the system.
    bool dying = false;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (!target)
            return;

        if (dying)
        {
            dieTime -= Time.deltaTime;
            if (dieTime < 0 )
                Destroy(gameObject);

            return;
        }

        Vector3 arcingPosition = target.position;
        arcingPosition.y += height * Vector3.Distance(target.position, transform.position);
        transform.position = Vector3.MoveTowards(transform.position, arcingPosition, speed);

        if (utils.isWithinGridRange(transform.position, target.position, distanceTrigger)) //we finished.
        {
            //turn off the particle emission... yes, unity didn't make it simple.
            utils.setEmission(gameObject.GetComponent<ParticleSystem>(), 0f);
            utils.setEmission(loopParticle, 0f);
            dying = true;
        }
	}

}
