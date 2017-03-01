using UnityEngine;
using System.Collections;

public class baubleFX : MonoBehaviour {

    public Material mat;
    public float effectSpeed;

    public float rotateSpeedX;
    public float rotateSpeedY;
    public float rotateSpeedZ;

    public Transform effectMesh;

    float uvOffset = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        uvOffset += effectSpeed * Time.deltaTime;
        if (uvOffset > 1f)
            uvOffset -= 1f;
        mat.mainTextureOffset = new Vector2(uvOffset, 0);

        transform.Rotate(rotateSpeedX * Time.deltaTime, rotateSpeedY * Time.deltaTime, rotateSpeedZ * Time.deltaTime);

        effectMesh.rotation = hypercubeCamera.mainCam.transform.rotation; //effect always faces cam.
	}
}
