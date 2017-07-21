using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloPlaySDK
{

    public class utils
    {

        public static float angleBetweenPoints(Vector2 v1, Vector2 v2)
        {
            return Mathf.Atan2(v1.x - v2.x, v1.y - v2.y) * Mathf.Rad2Deg;
        }
    }

}
