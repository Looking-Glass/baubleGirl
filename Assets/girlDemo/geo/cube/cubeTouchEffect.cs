using UnityEngine;
using System.Collections;

public class cubeTouchEffect : hypercube.touchScreenTarget
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


    public override void onTouchDown(hypercube.touch touch)
    {

        Vector3 worldPos = touch.getWorldPos(hypercubeCamera.mainCam);

        g.injectTouch(worldPos);
        emitTouchParticle(false, worldPos);
    }

    public override void onTouchUp(hypercube.touch touch)
    {

    }

    public override void onTouchMoved(hypercube.touch touch)
    {
        emitTouchParticle(true, touch.posX, touch.posY);
    }

    public void emitTouchParticle(bool timed, float screenPosX, float screenPosY)
    {
        Vector3 worldPos = transform.TransformPoint(screenPosX, screenPosY, particleZ);
        emitTouchParticle(timed, worldPos);
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