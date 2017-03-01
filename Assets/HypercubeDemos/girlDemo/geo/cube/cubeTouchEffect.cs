using UnityEngine;
using System.Collections;

public class cubeTouchEffect : touchscreenTarget
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


    public override void onTouchDown(int uid, Vector2 position)
    {
        Vector2 mapped = mapToRange(position.x, position.y);
        Vector3 worldPos = transform.TransformPoint(mapped.x, mapped.y, particleZ); //convert the touch to world space so it can collide with other things
 
        g.injectTouch(worldPos);
        emitTouchParticle(false, worldPos);
    }

    public override void onTouchUp(int uid, Vector2 position)
    {

    }

    public override void onTouchMoved(int uid, Vector2 position, Vector2 size)
    {
        emitTouchParticle(true, position.x, position.y);
    }

    public void emitTouchParticle(bool timed, float screenPosX, float screenPosY)
    {

        Vector2 mapped = mapToRange(screenPosX, screenPosY);
        Vector3 worldPos = transform.TransformPoint(mapped.x, mapped.y, particleZ);
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