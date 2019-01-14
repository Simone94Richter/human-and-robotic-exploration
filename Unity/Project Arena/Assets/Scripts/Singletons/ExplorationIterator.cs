using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExplorationIterator : MonoBehaviour{ 

    private GameObject robot;

    private Robot robotScript;
    private RobotPlanning rPl;
    private RobotProgress rP;

    private bool isTheChosenOne = false;

    private float timeScan;
    private float timeDecision;
    private float penaltyCost;

    private float maxTimeScan = 1.0f;
    private float maxTimeDecision = 10.0f;
    private float maxPenaltyCost = 50.0f;

    private float minTimeScan = 0.1f;
    private float minTimeDecision = 1f;
    private float minPenaltyCost = 0;

    private float startingTimeScan = 0.4f;
    private float startingTimeDecision = 1f;
    private float startingPenaltyCost = 0;

    private string pathMapNum = "/Results/resultMapNum";
    private string pathPosNum = "/Results/resultPositionNum";
    private int iteration = 31;

    private IEnumerator timer;

    private void OnLevelWasLoaded(int level)
    {
        if (!isTheChosenOne)
        {
            Destroy(this.gameObject);
        }
        else
        {
            timer = Timer();
            iteration = 1 + iteration;
            if (timeDecision < maxTimeDecision)
            {
                timeDecision = 1 + timeDecision;
            }
            else if (timeScan < maxTimeScan)
            {
                timeScan = 0.1f + timeScan;
                timeDecision = minTimeDecision;
            }else if (penaltyCost < maxPenaltyCost)
            {
                penaltyCost = 1 + penaltyCost;
                timeScan = minTimeScan;
                timeDecision = minTimeDecision;
            }

            robot = GameObject.Find("Robot");
            robotScript = robot.GetComponent<Robot>();
            rPl = robot.GetComponent<RobotPlanning>();
            rP = robot.GetComponent<RobotProgress>();
            robotScript.timeForScan = timeScan;
            robotScript.timeForDecision = timeDecision;
            robotScript.penaltyCost = penaltyCost;
            rP.pathMapNum = pathMapNum + iteration.ToString() + ".json";
            rP.pathPosNum = pathPosNum + iteration.ToString() + ".json";

            robotScript.SetVariables();
            rP.DefiningFolderAndFile();

            Debug.Log(timeScan);
            Debug.Log(timeDecision);
            Debug.Log(penaltyCost);

            StartCoroutine(timer);
        }
    }

    // Use this for initialization
    void Start()
    {
        isTheChosenOne = true;
        timer = Timer();
        robot = GameObject.Find("Robot");
        robotScript = robot.GetComponent<Robot>();
        rPl = robot.GetComponent<RobotPlanning>();
        rP = robot.GetComponent<RobotProgress>();

        robotScript.timeForScan = startingTimeScan;
        robotScript.timeForDecision = startingTimeDecision;
        robotScript.penaltyCost = startingPenaltyCost;
        rP.pathMapNum = pathMapNum + iteration.ToString() + ".json";
        rP.pathPosNum = pathPosNum + iteration.ToString() + ".json";

        robotScript.SetVariables();
        rP.DefiningFolderAndFile();

        timeScan = startingTimeScan;
        timeDecision = startingTimeDecision;
        penaltyCost = startingPenaltyCost;

        Debug.Log(robotScript.timeForScan);
        Debug.Log(robotScript.timeForDecision);
        Debug.Log(robotScript.penaltyCost);
        
        StartCoroutine(timer);
    }

    public void CheckIteration()
    {
        if(timeDecision == maxTimeDecision && timeScan == maxTimeScan)
        {
            Debug.Log("End");
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
            StopCoroutine(timer);
            SceneManager.LoadScene("Experimenting - Tutorial Robot");
        }
    } 

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(300);
        CheckIteration();
    }
}
