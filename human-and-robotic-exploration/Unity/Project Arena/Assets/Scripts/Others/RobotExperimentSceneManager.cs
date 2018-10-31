using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RobotExperimentSceneManager : MonoBehaviour {

    public string sceneToLoad;
	
	public void Play()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
