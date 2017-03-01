using UnityEngine;
using System.Collections;

public class eyeAnim : MonoBehaviour {

    public Material m;
    public Transform eyeBone;

    Vector2 temp = new Vector2();
	// Update is called once per frame
	void Update () 
    {
        temp.x = eyeBone.localPosition.y;
        temp.y = eyeBone.localPosition.z;
        m.mainTextureOffset = temp;
	}
}
