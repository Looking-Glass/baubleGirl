﻿using UnityEngine;
using System.Collections;

public class randomSound : MonoBehaviour {

    public bool isPlaying = true;
    public float volumeMod = 1f;
    public AudioClip[] sounds;
    public float timeBetweenMin;  //the time between playbacks
    public float timeBetweenMax;
    public AudioSource audio;

    float timer;
    

	// Use this for initialization
	void Start () 
    {
        if (!audio)
            audio = GetComponent<AudioSource>();
        timer = 0;
	}

	
	// Update is called once per frame
	void Update () 
    {
        if (isPlaying)
            timer -= Time.deltaTime;

        if (timer < 0)
        {
            audio.PlayOneShot(sounds[Random.Range(0, sounds.Length)], volumeMod);
            timer = Random.Range(timeBetweenMin, timeBetweenMax);
        }
	}
}