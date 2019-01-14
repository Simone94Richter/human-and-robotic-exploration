using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ExperimentObjects;

/// <summary>
/// This class is responsible for saving relevant data about the exploration. They are:
/// - Information about the data (the different type of tiles)
/// - Information about the trajectory of the robot, in particular:
///     - position
///     - rotation (related to the Y axis)
///     - time needed to found the desired object
/// </summary>
public class RobotProgress : MonoBehaviour {

    [Header("Char Map case")]
    [Header("Where the file is located (inside the main application folder)")]
    public string pathMap = "/Results/resultMap.json";
    public string pathPos = "/Results/resultPosition.json";

    [Header("Numerical Map case")]
    [Header("Where the file is located (inside the main application folder)")]
    public string pathMapNum = "/Results/resultMapNum.json";
    public string pathPosNum = "/Results/resultPositionNum.json";

    [Header("Only for Robot Agent Test case")]
    [Header("Name of the map")]
    public string map = "open1.map";

    private string filePathMapResChar; //complete path for the file dedicated to store map information (Char case)
    private string filePathPosResChar; //complete path for the file dedicated to store trajectory information (Char case)
    private string filePathMapResNum; //complete path for the file dedicated to store map information (Float case)
    private string filePathPosResNum; //complete path for the file dedicated to store trajectory information (Float case)

    private string posAsJson;
    private string mapAsJson;

#if UNITY_EDITOR
    private JsonRobotData gameRobotData;
    private FileStream fileMap;
    private FileStream filePos;
#endif
#if !UNITY_EDITOR
    private JsonRobotObjects gameDataPos;
#endif
    private JsonMapObjects gameDataMap;
    private RobotConnection rC;

    // Use this for initialization
    void Start () {

        rC = GetComponent<RobotConnection>();

        gameDataMap = new JsonMapObjects();
        gameDataMap.mapName = new List<string>();
#if !UNITY_EDITOR
        gameDataPos = new JsonRobotObjects();
        gameDataPos.position = new List<string>();
        gameDataPos.rotationY = new List<float>();
#endif
        posAsJson = "";
        mapAsJson = "";
    }

    void Awake()
    {
#if UNITY_EDITOR
        gameRobotData = new JsonRobotData
        {
            mapName = map,
            position = new List<string>(),
            rotationY = new List<float>()
        };
#endif
    }

    public void DefiningFolderAndFile()
    {
#if UNITY_EDITOR
        filePathMapResChar = Application.dataPath + pathMap;
        if (!File.Exists(filePathMapResChar))
        {
            File.Create(filePathMapResChar);
        }
        filePathPosResChar = Application.dataPath + pathPos;
        if (!File.Exists(filePathPosResChar))
        {
            File.Create(filePathPosResChar);
        }
        filePathMapResNum = Application.dataPath + pathMapNum;
        if (!File.Exists(filePathMapResNum))
        {
            fileMap = File.Create(filePathMapResNum);
            fileMap.Close();
        }
        filePathPosResNum = Application.dataPath + pathPosNum;
        if (!File.Exists(filePathPosResNum))
        {
            filePos = File.Create(filePathPosResNum);
            filePos.Close();
        }
#endif
    }

    /// <summary>
    /// This method is responsble for saving information about the map, in the case the source map is a char one
    /// </summary>
    /// <param name="robot_map">source map used by the robot</param>
    public void SaveMapChar(char [,] robot_map)
    {
        gameDataMap.u = new List<string>();
        gameDataMap.r = new List<string>();
        gameDataMap.g = new List<string>();
        gameDataMap.w = new List<string>();

        for (int i = 0; i < robot_map.GetLength(0); i++)
        {
            for (int j = 0; j < robot_map.GetLength(1); j++)
            {
                if (robot_map[i, j] == 'u')
                {
                    gameDataMap.u.Add(i.ToString() + "," + j.ToString());
                }else if (robot_map[i, j] == 'r')
                {
                    gameDataMap.r.Add(i.ToString() + "," + j.ToString());
                }
                else if (robot_map[i, j] == 'g')
                {
                    gameDataMap.g.Add(i.ToString() + "," + j.ToString());
                }
                else if (robot_map[i, j] == 'w')
                {
                    gameDataMap.w.Add(i.ToString() + "," + j.ToString());
                }
            }
        }

        mapAsJson = JsonUtility.ToJson(gameDataMap);
#if UNITY_EDITOR
        File.WriteAllText(filePathMapResChar, mapAsJson);
#endif
    }

    public void SaveMapNum(float[,] robot_map)
    {
        gameDataMap.u = new List<string>();
        gameDataMap.r = new List<string>();
        gameDataMap.g = new List<string>();
        gameDataMap.w = new List<string>();

        for (int i = 0; i < robot_map.GetLength(0); i++)
        {
            for (int j = 0; j < robot_map.GetLength(1); j++)
            {
                if (robot_map[i, j] == 2f)
                {
                    gameDataMap.u.Add(i.ToString() + "," + j.ToString());
                }
                else if (robot_map[i, j] == 0f)
                {
                    gameDataMap.r.Add(i.ToString() + "," + j.ToString());
                }
                else if (robot_map[i, j] == 3f)
                {
                    gameDataMap.g.Add(i.ToString() + "," + j.ToString());
                }
                else if (robot_map[i, j] == 1f)
                {
                    gameDataMap.w.Add(i.ToString() + "," + j.ToString());
                }
                //textMap += robot_map[i, j];
            }
            //textMap += "\n";
        }

        mapAsJson = JsonUtility.ToJson(gameDataMap);
#if UNITY_EDITOR
        File.WriteAllText(filePathMapResNum, mapAsJson);
#endif
    }

    public void SavePosChar(int posX, int posZ, Vector3 rotation)
    {
#if !UNITY_EDITOR
        gameDataPos.position.Add(posX.ToString() + "," + posZ.ToString());
        gameDataPos.rotationY.Add(rotation.y);
        posAsJson = JsonUtility.ToJson(gameDataPos);
#endif
#if UNITY_EDITOR
        gameRobotData.position.Add(posX.ToString() + "," + posZ.ToString());
        gameRobotData.rotationY.Add(rotation.y);
        posAsJson = JsonUtility.ToJson(gameRobotData);
        File.WriteAllText(filePathPosResChar, posAsJson);
#endif
    }

    public void SavePosNum(int posX, int posZ, Vector3 rotation)
    {
#if !UNITY_EDITOR
        gameDataPos.position.Add(posX.ToString() + "," + posZ.ToString());
        gameDataPos.rotationY.Add(rotation.y);
        posAsJson = JsonUtility.ToJson(gameDataPos);
#endif
#if UNITY_EDITOR
        gameRobotData.position.Add(posX.ToString() + "," + posZ.ToString());
        gameRobotData.rotationY.Add(rotation.y);
        posAsJson = JsonUtility.ToJson(gameRobotData);
        File.WriteAllText(filePathPosResNum, posAsJson);
#endif
    }

    public void SaveTimeChar(float time)
    {
#if !UNITY_EDITOR
        gameDataPos.time = time;
        posAsJson = JsonUtility.ToJson(gameDataPos);
#endif
#if UNITY_EDITOR
        gameRobotData.time = time;
        posAsJson = JsonUtility.ToJson(gameRobotData);
        File.WriteAllText(filePathPosResChar, posAsJson);
#endif
    }

    public void SaveTimeNum(float time)
    {
#if !UNITY_EDITOR
        gameDataPos.time = time;
        if (ExperimentManager.HasInstance())
        {
            int index = ExperimentManager.Instance.GetCaseIndex();
            Case currentCase = ExperimentManager.Instance.GetCaseList()[index];
            //Debug.Log(currentCase.GetCurrentMap().name);
            gameDataPos.mapName = currentCase.GetCurrentMap().name;
            gameDataMap.mapName.Add(currentCase.GetCurrentMap().name);

            if (!string.IsNullOrEmpty(ExperimentManager.Instance.GetIpAddress()))
            {
                gameDataPos.ip = ExperimentManager.Instance.GetIpAddress();
            }
        }
        gameDataPos.os = SystemInfo.operatingSystem;
        posAsJson = JsonUtility.ToJson(gameDataPos);
        mapAsJson = JsonUtility.ToJson(gameDataMap);
#endif
#if UNITY_EDITOR
        gameRobotData.time = time;
        if (ExperimentManager.HasInstance())
        {
            int index = ExperimentManager.Instance.GetCaseIndex();
            Case currentCase = ExperimentManager.Instance.GetCaseList()[index];
            //Debug.Log(currentCase.GetCurrentMap().name);
            gameDataMap.mapName.Add(currentCase.GetCurrentMap().name);

            if (!string.IsNullOrEmpty(ExperimentManager.Instance.GetIpAddress()))
            {
                gameRobotData.ip = ExperimentManager.Instance.GetIpAddress();
            }
        }
        gameRobotData.os = SystemInfo.operatingSystem;
        posAsJson = JsonUtility.ToJson(gameRobotData);
        mapAsJson = JsonUtility.ToJson(gameDataMap);
        File.WriteAllText(filePathPosResNum, posAsJson);
        File.WriteAllText(filePathMapResNum, mapAsJson);
#endif
    }

    public void PreparingForServer()
    {
#if !UNITY_EDITOR
        rC.SendDataToServer(JsonUtility.ToJson(gameDataMap), JsonUtility.ToJson(gameDataPos));
#endif
    }

    public void SetPenaltyCost(float cost)
    {
#if UNITY_EDITOR
        gameRobotData.penalty_cost = cost;
#endif
    }

    public void SetTimeDecision(float time)
    {
#if UNITY_EDITOR
        gameRobotData.timeDecision = time;
#endif
    }

    public void SetTimeScan(float time)
    {
#if UNITY_EDITOR
        gameRobotData.timeScan = time;
#endif
    }
}
