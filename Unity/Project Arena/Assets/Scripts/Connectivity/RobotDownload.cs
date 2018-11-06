using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System.IO;

/// <summary>
/// This class is responsible for downloading trajectory results from the server. These results will be written in txt files into two format:
/// a JSON format and a triple format
/// </summary>
public class RobotDownload : MonoBehaviour {

    [Header("The URL used to download data from the webserver")]
    public string url = "http://travellersinn.herokuapp.com/tesi/getTrajectory.php";
    [Header("The path where to download the results of the experiments")]
    public string downloadedContentPath = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/DownloadedResults";

    protected int id;
    protected bool keepGoing = true;
    protected IdPost idObject;
    private TrajectoryReceived gameDataPos;
    private JsonRobotObjects data;
    private List<string> pos = new List<string>();
    private List<float> rot = new List<float>();
    private List<float> time = new List<float>();
    private List<string> mName = new List<string>();

    /// <summary>
    /// This method calls the coroutine to start the procedure to download data
    /// </summary>
    public void DownloadData()
    {
        StartCoroutine(Download());
    } 

    /// <summary>
    /// This method sends a POST request to get the desired data to download and writes them in the created txt file
    /// Not usable in Web build
    /// </summary>
    /// <returns></returns>
    private IEnumerator Download()
    {
        data = new JsonRobotObjects();
        data.position = new List<string>();
        data.rotationY = new List<float>();
        data.time = new List<float>();
        data.mapName = new List<string>();
        id = 1;
        while (keepGoing) //until there are other results to download
        {
            var uwr = UnityWebRequest.Post(url, "POST");
            idObject = new IdPost(id.ToString());
            byte[] idToSend = new System.Text.UTF8Encoding().GetBytes(JsonUtility.ToJson(idObject));
            uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(idToSend);
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            //uwr.SetRequestHeader("Content-Type", "application/json");

            //Send the request then wait here until it returns
            yield return uwr.SendWebRequest();

            if (uwr.downloadHandler.text != "Id not existent")
            {
                Debug.Log(uwr.downloadHandler.text);
                gameDataPos = JsonUtility.FromJson<TrajectoryReceived>(uwr.downloadHandler.text);
                gameDataPos.position = Regex.Replace(gameDataPos.position, @"[()]", "");
                gameDataPos.position = Regex.Replace(gameDataPos.position, @"[ ]", "");
                string[] positions = gameDataPos.position.Split(',');
                pos = new List<string>();
                for (int i = 0; i< positions.Length; i=i+2)
                {
                    pos.Add(positions[i]+","+positions[i+1]);    
                }
                gameDataPos.rotation = Regex.Replace(gameDataPos.rotation, @"[()]", "");
                gameDataPos.rotation = Regex.Replace(gameDataPos.rotation, @"[ ]", "");
                string[] rotString = gameDataPos.rotation.Split(',');
                rot = new List<float>();
                for (int i = 0; i < rotString.Length; i++)
                {
                    rot.Add(float.Parse(rotString[i]));
                }
                gameDataPos.timerobot = Regex.Replace(gameDataPos.timerobot, @"[()]", "");
                gameDataPos.timerobot = Regex.Replace(gameDataPos.timerobot, @"[ ]", "");
                string[] timeRob = gameDataPos.timerobot.Split(',');
                time = new List<float>();
                for (int i = 0; i < timeRob.Length; i++)
                {
                    time.Add(float.Parse(timeRob[i]));
                }
                gameDataPos.mapname = Regex.Replace(gameDataPos.mapname, @"[()]", "");
                gameDataPos.mapname = Regex.Replace(gameDataPos.mapname, @"[ ]", "");
                string[] name = gameDataPos.mapname.Split(',');
                mName = new List<string>();
                for (int i = 0; i < name.Length; i++)
                {
                    mName.Add(name[i]);
                }
                data.position = pos;
                data.rotationY = rot;
                data.time = time;
                data.mapName = mName;
                data.ip = gameDataPos.ip;
                data.os = gameDataPos.os;
                
                File.WriteAllText(downloadedContentPath + "/Result" + id.ToString() + "t.txt", JsonUtility.ToJson(data));

                WriteTupleLog(pos, rot, mName, id);

                id++;
            }
            else
            {
                Debug.Log("Finished download");
                keepGoing = false;
            }
        }
    }

    /// <summary>
    /// This method takes positions, rotations and a map name paramters to be written a triple-format text 
    /// </summary>
    /// <param name="pos">List of positions of the JSON</param>
    /// <param name="rot">List of rotations of the JSON</param>
    /// <param name="name">Name of the map explored by the player/robot</param>
    /// <param name="id">Id of the results according to the database</param>
    private void WriteTupleLog(List<string> pos, List<float> rot, List<string> name, int id)
    {
        string log;
        log = "Map Name: " + name[0];
        for (int i = 0; i < pos.Count; i++)
        {
            log = log + " <" + pos[i] + ", " + rot[i] + ", " + i.ToString() + ">,";
        }
        File.WriteAllText(downloadedContentPath + "/ResultTuple" + id.ToString() + ".txt", log);
    }
    
    /// <summary>
    /// Class defining an id object to be sent for the POST request
    /// </summary>
    public class IdPost
    {
        public string id;

        public IdPost(string id)
        {
            this.id = id;
        }
    }

    /// <summary>
    /// Class defining all the components of the JSON downloaded from the server in the trajectory case
    /// </summary>
    public class TrajectoryReceived
    {
        public string position;
        public string rotation;
        public string timerobot;
        public string mapname;
        public string ip;
        public string os;
    }

    /// <summary>
    /// Class defining all the components of the JSON downloaded from the server in the map case
    /// </summary>
    public class MapReceived
    {
        public string uknown;
        public string wall;
        public string goal;
    }
}
