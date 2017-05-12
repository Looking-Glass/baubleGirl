using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public void SwitchScene(int i)
    {
        i = (int)Mathf.Sign(i);
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        sceneIndex += i;
        if (sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            sceneIndex = 0;
        }
        else if (sceneIndex < 0)
        {
            sceneIndex = SceneManager.sceneCountInBuildSettings - 1;
        }
        SceneManager.LoadScene(sceneIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SwitchScene(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SwitchScene(1);
        }
    }
}
