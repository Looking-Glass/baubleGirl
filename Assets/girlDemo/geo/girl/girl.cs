using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HoloPlaySDK;

public class girl : depthTouchTarget
{
    public GameObject girlObject;
    public Transform girlRoot;
    public Transform girlPelvis;
    public GameObject cube;
    public GameObject shinyTrail;
    public girlState currentState;   
    girlState[] allStates;

    public AudioSource voiceSounds;
    public AudioSource sfxSound;
    public AudioClip popBaubleSound;
    public AudioClip touchScreenSound;

    public Animator girlAnim;

    Vector3 cubeStartPos;
    Vector3 girlStartPos;

    bauble[] allBaubles;

    public float resetTime = 30f; //if this time passes with no interaction, the game will reset.
    float resetTimer = 0f;

	void Start () 
    {
        cubeStartPos = cube.transform.position;
        girlStartPos = girlObject.transform.position;

        allStates = GetComponents<girlState>();
        setState(girlStateType.ST_INIT); //MUST BE ST_INIT

        allBaubles = GameObject.FindObjectsOfType<bauble>();

        sfxSound = cube.GetComponent<AudioSource>();
	}


    void Update()
    {
        currentState.updateState(this);

        if (Input.GetKeyDown(KeyCode.R))
            reset();
        else if (Input.GetKeyDown(KeyCode.P))
            setState(girlStateType.ST_POSE1);

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (currentState.stateType == girlStateType.ST_HIDDEN)
        {
            resetTimer += Time.deltaTime;
            if (resetTimer > resetTime)
            {
                reset();
            }
        }
        else
            resetTimer = 0f;

    }

	public override void onDepthTouch(List<depthTouch> touches) { resetTimer = 0f; } //restart the reset timer if any input is received.


    public void doneCompleted()
    {
        setState(girlStateType.ST_RUNOFF);
    }

    public void foundBobble(Vector3 pos)
    {
       
        setState(girlStateType.ST_ARRIVING);
        playSoundEffect(popBaubleSound, .5f);
    }

    public void setState(girlStateType type)
    {
        foreach(girlState s in allStates)
        {
            if (s.stateType == type)
            {
                if (currentState)
                    currentState.endState(this);
                currentState = s;
                currentState.enterState(this);
            }
        }
    }

    public void injectTouch(Vector3 worldTouchPos)
    {
        if (currentState)
            currentState.injectTouch(this, worldTouchPos);

        playTouchScreenSound();
    }

    public void playTrigger(string animTriggerName)
    {
        girlAnim.SetTrigger(animTriggerName);
    }

    public void playVoiceSound(AudioClip c)
    {
        voiceSounds.Stop(); //don't let the girl talk over herself
        voiceSounds.PlayOneShot(c);
    }

    public void reset()
    {
        girlObject.transform.position = girlStartPos;
        cube.transform.position = cubeStartPos;
        setState(girlStateType.ST_INIT);

        foreach (bauble b in allBaubles)
            b.gameObject.SetActive(true);

        resetTimer = 0f;
    }

    public void playTouchScreenSound()
    {
        playSoundEffect(touchScreenSound, .1f);
    }
    public void playSoundEffect(AudioClip c, float volume)
    {
        sfxSound.PlayOneShot(c, volume);
    }
}
