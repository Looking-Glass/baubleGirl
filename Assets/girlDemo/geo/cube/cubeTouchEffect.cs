using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using HoloPlaySDK;

public class cubeTouchEffect : HoloPlaySDK.depthTouchTarget
{

    public ParticleSystem touchParticle;
    public float particleZ = -.45f;
    public float touchParticleSize = 10f;
    public float touchParticleLifetime = 1.2f;
    public float touchParticleMaxFreq = .3f;
    public girl g;
    float particleTimer;


    void Update()
    {
        particleTimer -= Time.deltaTime;
        touchParticle.transform.position = transform.position;
    }


	public override void onDepthTouch(List<depthTouch> touches)
	{
		foreach (depthTouch t in touches) 
		{
			emitTouchParticle (true, t.getWorldPos(HoloPlay.Main.transform));
		}
    }

    public void emitTouchParticle(bool timed, Vector3 worldPos)
    {
        if (timed)
        {
            if (particleTimer > 0)
                return;
            particleTimer = touchParticleMaxFreq;
        }

        touchParticle.Emit( worldPos - transform.position, Vector3.zero, touchParticleSize, touchParticleLifetime, new Color32(255, 255, 255, 255));  //emit at least 1 on each onTouchDown
    }

}