using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RobotProgress : MonoBehaviour {

    [Header("Where the file is located (inside the main application folder)")]
    public string pathMap = "/Results/resultMap.json";
    public string pathPos = "/Results/resultPosition.json";

    string filePathMapRes;
    string filePathPosRes;

    string posAsJson;
    string mapAsJson;

    private JsonRobotObjects gameDataPos;
    private JsonMapObjects gameDataMap;

    // Use this for initialization
    void Start () {
        filePathMapRes = Application.dataPath + pathMap;
        if (!File.Exists(filePathMapRes))
        {
            File.Create(filePathMapRes);
        }
        filePathPosRes = Application.dataPath + pathPos;
        if (!File.Exists(filePathPosRes))
        {
            File.Create(filePathPosRes);
        }
        gameDataPos = new JsonRobotObjects();
        gameDataMap = new JsonMapObjects();
        //gameDataMap.u = new List<string>();
        //gameDataMap.r = new List<string>();
        //gameDataMap.g = new List<string>();
        posAsJson = "";
        mapAsJson = "";
    }
	
	// Update is called once per frame
	/*void LateUpdate () {
        
	}*/

    public void SaveMap(char [,] robot_map)
    {
        //string textMap = "";

        //AddMapEdge();
        gameDataMap.u = new List<string>();
        gameDataMap.r = new List<string>();
        gameDataMap.g = new List<string>();

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
                //textMap += robot_map[i, j];
            }
            //textMap += "\n";
        }

        mapAsJson = JsonUtility.ToJson(gameDataMap);
        File.WriteAllText(filePathMapRes, mapAsJson);
    }

    public void SavePos(Vector3 position, Quaternion rotation)
    {
        gameDataPos.position = position;
        gameDataPos.rotation = rotation;
        posAsJson = JsonUtility.ToJson(gameDataPos);
        File.WriteAllText(filePathPosRes, posAsJson);
    }
}
