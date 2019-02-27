using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class ExplorationIterator : MonoBehaviour{ 

    private GameObject robot;

    private Robot robotScript;
    private RobotDMUtilityCloseWall rDMUtility;
    private RobotPlanning rPl;
    private RobotProgress rP;

    private bool isTheChosenOne = false;
    private bool isMulty = true;

    //private float timeScan;
    //private float timeDecision;
    //private float penaltyCost;
    private float alpha;
    private float beta;
    private int deltaIndex;
    private int[] forgettingFactor = new int[] {30,60,120,180};

    private float maxAlpha = 1f;
    private float maxBeta = 1f;
    private int maxDeltaIndex = 3;

    private float minAlpha = 0f;
    private float minBeta = 0f;
    private int minDeltaIndex = 0;

    private float startingAlpha = 0.5f;
    private float startingBeta = 0.8f;
    private int startingDelta = 2;

    private string pathMapNum = "/Results/ExperimentSamplesMultyTarget1/resultMapNum";
    private string pathPosNum = "/Results/ExperimentSamplesMultyTarget1/resultPositionNum";
    private int iteration = 336;

    private string mapName = "uffici1.map";
    private string filePathPosResNum;
    private string fileContent;

    private IEnumerator timer;

    private FileStream filePos;

    private JsonRobotData trajectory;

    private void OnLevelWasLoaded(int level)
    {
        if (!isTheChosenOne)
        {
            Destroy(this.gameObject);
        }
        else
        {
            iteration = 1 + iteration;
            if (alpha < maxAlpha)
            {
                alpha = 0.1f + alpha;
            }
            else if (beta < maxBeta)
            {
                beta = 0.1f + beta;
                alpha = minAlpha;
            }
            else if (deltaIndex < maxDeltaIndex)
            {
                deltaIndex = deltaIndex + 1;
                alpha = minAlpha;
                beta = minBeta;
            }

            filePathPosResNum = Application.dataPath + pathPosNum;
            if (File.Exists(filePathPosResNum))
            {
                fileContent = File.ReadAllText(filePathPosResNum);
                trajectory = JsonUtility.FromJson<JsonRobotData>(fileContent);
                Debug.Log(trajectory.time);
                filePos.Close();
                if (trajectory.time == 0.0f)
                {
                    CheckIteration();
                }
            }

            if(alpha + beta < 1f){
                CheckIteration();
            }

            timer = Timer();
            robot = GameObject.Find("Robot");
            robotScript = robot.GetComponent<Robot>();
            rPl = robot.GetComponent<RobotPlanning>();
            rP = robot.GetComponent<RobotProgress>();
            rDMUtility = robot.GetComponent<RobotDMUtilityCloseWall>();
            rDMUtility.alpha = alpha;
            rDMUtility.beta = beta;
            robotScript.forgettingFactor = forgettingFactor[deltaIndex];
            rP.pathMapNum = pathMapNum + iteration.ToString() + ".json";
            rP.pathPosNum = pathPosNum + iteration.ToString() + ".json";

            //robotScript.SetVariables();
            rP.DefiningFolderAndFile();
            rP.SetAlpha(alpha);
            rP.SetBeta(beta);
            rP.SetDelta(forgettingFactor[deltaIndex]);
            rP.SetMapName(mapName);

            Debug.Log(alpha);
            Debug.Log(beta);
            Debug.Log(forgettingFactor[deltaIndex]);

            StartCoroutine(timer);
        }
    }

    // Use this for initialization
    void Start()
    {
        isTheChosenOne = true;
        filePathPosResNum = Application.dataPath + pathPosNum;
        if (File.Exists(filePathPosResNum))
        {
            fileContent = File.ReadAllText(filePathPosResNum);
            trajectory = JsonUtility.FromJson<JsonRobotData>(fileContent);
            Debug.Log(trajectory.time);
            filePos.Close();
            if (trajectory.time == 0.0f)
            {
                CheckIteration();
            }
        }

        alpha = startingAlpha;
        beta = startingBeta;

        if (alpha + beta < 1f)
        {
            CheckIteration();
        }

        timer = Timer();
        robot = GameObject.Find("Robot");
        robotScript = robot.GetComponent<Robot>();
        rPl = robot.GetComponent<RobotPlanning>();
        rP = robot.GetComponent<RobotProgress>();
        rDMUtility = robot.GetComponent<RobotDMUtilityCloseWall>(); 

        rDMUtility.alpha = startingAlpha;
        rDMUtility.beta = startingBeta;
        robotScript.forgettingFactor = forgettingFactor[startingDelta];
        rP.pathMapNum = pathMapNum + iteration.ToString() + ".json";
        rP.pathPosNum = pathPosNum + iteration.ToString() + ".json";

        alpha = startingAlpha;
        beta = startingBeta;
        deltaIndex = startingDelta;

        //robotScript.SetVariables();
        rP.DefiningFolderAndFile();
        rP.SetAlpha(alpha);
        rP.SetBeta(beta);
        rP.SetDelta(forgettingFactor[deltaIndex]);
        rP.SetMapName(mapName);

        Debug.Log(rDMUtility.alpha);
        Debug.Log(rDMUtility.beta);
        Debug.Log(robotScript.forgettingFactor);
        
        StartCoroutine(timer);
    }

    public void CheckIteration()
    {
        if(alpha >= maxAlpha && beta >= maxBeta && deltaIndex >= maxDeltaIndex)
        {
            Debug.Log("End");
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
            if (timer != null)
            {
                StopCoroutine(timer);
            }
            if (isMulty)
            {
                SceneManager.LoadScene("Experimenting - Multy Target Robot");
            }
            else SceneManager.LoadScene("Experimenting - Tutorial Robot");
        }
    } 

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(480);
        CheckIteration();
    }
}
