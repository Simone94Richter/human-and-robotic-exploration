using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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

    private string filePathMapResChar; //complete path for the file dedicated to store map information (Char case)
    private string filePathPosResChar; //complete path for the file dedicated to store trajectory information (Char case)
    private string filePathMapResNum; //complete path for the file dedicated to store map information (Float case)
    private string filePathPosResNum; //complete path for the file dedicated to store trajectory information (Float case)

    private string posAsJson;
    private string mapAsJson;

    private JsonRobotObjects gameDataPos;
    private JsonMapObjects gameDataMap;
    private RobotConnection rC;

    // Use this for initialization
    void Start () {

        rC = GetComponent<RobotConnection>();

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
            File.Create(filePathMapResNum);
        }
        filePathPosResNum = Application.dataPath + pathPosNum;
        if (!File.Exists(filePathPosResNum))
        {
            File.Create(filePathPosResNum);
        }

        gameDataPos = new JsonRobotObjects();
        gameDataPos.position = new List<string>();
        gameDataPos.rotationY = new List<float>();
        gameDataPos.time = new List<float>();
        gameDataMap = new JsonMapObjects();

        posAsJson = "";
        mapAsJson = "";
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
        File.WriteAllText(filePathMapResChar, mapAsJson);
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
        File.WriteAllText(filePathMapResNum, mapAsJson);
    }

    public void SavePosChar(int posX, int posZ, Quaternion rotation)
    {
        gameDataPos.position.Add(posX.ToString() + "," + posZ.ToString());
        gameDataPos.rotationY.Add(rotation.y * Mathf.Rad2Deg);
        posAsJson = JsonUtility.ToJson(gameDataPos);
        File.WriteAllText(filePathPosResChar, posAsJson);
    }

    public void SavePosNum(int posX, int posZ, Quaternion rotation)
    {
        gameDataPos.position.Add(posX.ToString() + "," + posZ.ToString());
        gameDataPos.rotationY.Add(rotation.y * Mathf.Rad2Deg);
        posAsJson = JsonUtility.ToJson(gameDataPos);
        File.WriteAllText(filePathPosResNum, posAsJson);
    }

    public void SaveTimeChar(float time)
    {
        gameDataPos.time.Add(time);
        posAsJson = JsonUtility.ToJson(gameDataPos);
        File.WriteAllText(filePathPosResChar, posAsJson);
    }

    public void SaveTimeNum(float time)
    {
        gameDataPos.time.Add(time);
        posAsJson = JsonUtility.ToJson(gameDataPos);
        File.WriteAllText(filePathPosResNum, posAsJson);
    }

    public void PreparingForServer()
    {
        rC.SendDataToServer(JsonUtility.ToJson(gameDataMap), JsonUtility.ToJson(gameDataPos));
    }
}
