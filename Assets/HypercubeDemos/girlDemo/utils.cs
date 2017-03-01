using UnityEngine;
using System.Collections;

public class utils : MonoBehaviour 
{

    public static void setEmission( ParticleSystem p, float _rate)
    {
             //turn off the particle emission... yes, unity didn't make it simple.
            var curve = new ParticleSystem.MinMaxCurve();
            curve.constantMax = _rate;
            ParticleSystem.EmissionModule em = p.emission;
            em.rate = curve;
    }

    public static bool isWithinGridRange(Vector3 A, Vector3 B, float range)
    {

        float diff = A.x < B.x ? B.x - A.x : A.x - B.x;
        if (diff > range)
            return false;

        diff = A.y < B.y ? B.y - A.y : A.y - B.y;
        if (diff > range)
            return false;

        diff = A.z < B.z ? B.z - A.z : A.z - B.z;
        if (diff > range)
            return false;

        return true;
    }

    public static bool isWithinGridRange2D(Vector3 A, Vector3 B, float range)
    {

        float diff = A.x < B.x ? B.x - A.x : A.x - B.x;
        if (diff > range)
            return false;

        diff = A.z < B.z ? B.z - A.z : A.z - B.z;
        if (diff > range)
            return false;

        return true;
    }

    //public static float getGroundHeightFrom(Vector3 coord)
    //{
    //    RaycastHit r;
    //    if (!Physics.Raycast(coord, Vector3.down, out r))
    //        return 0;
    //    return r.distance;
    //}

    public static Vector3 getPointOnGroundAt(Vector3 coord)
    {
        coord.y = 10f;
        RaycastHit r;
        if (!Physics.Raycast(coord, Vector3.down, out r))
            return coord;
        return r.point;
    }

    public static Vector3 getRandomPointAtDistanceFrom(Vector3 from, float distance)
    {
        float angle = Random.Range(0.0f, Mathf.PI * 2);
        Vector3 randomPosOnCircle = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
        randomPosOnCircle *= distance;
        return from + randomPosOnCircle;
    }
}
