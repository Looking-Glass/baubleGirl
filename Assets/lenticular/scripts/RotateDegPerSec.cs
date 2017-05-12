using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateDegPerSec : MonoBehaviour
{
	public Vector3 DegPerSec;

    // Update is called once per frame
    void Update()
    {
		transform.Rotate(DegPerSec * Time.deltaTime);
    }
}
