using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System.IO;

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
    //WWW www;
    //WWWForm data;

    public void DownloadData()
    {
        StartCoroutine(Download());
    } 

    private IEnumerator Download()
    {
        data = new JsonRobotObjects();
        data.position = new List<string>();
        data.rotationY = new List<float>();
        data.time = new List<float>();
        id = 1;
        while (keepGoing)
        {
            var uwr = UnityWebRequest.Post(url, "POST");
            idObject = new IdPost(id.ToString());
            //Debug.Log(json1);
            byte[] idToSend = new System.Text.UTF8Encoding().GetBytes(JsonUtility.ToJson(idObject));
            //Debug.Log(jsonToSend);
            uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(idToSend);
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            //uwr.SetRequestHeader("Content-Type", "application/json");

            //Send the request then wait here until it returns
            yield return uwr.SendWebRequest();

            if (uwr.downloadHandler.text != "Id not existent")
            {
                Debug.Log(uwr.downloadHandler.text);
                gameDataPos = JsonUtility.FromJson<TrajectoryReceived>(uwr.downloadHandler.text);
                //Debug.Log(gameDataPos.timerobot);
                gameDataPos.position = Regex.Replace(gameDataPos.position, @"[()]", "");
                gameDataPos.position = Regex.Replace(gameDataPos.position, @"[ ]", "");
                string[] positions = gameDataPos.position.Split(',');
                pos = new List<string>();
                for (int i = 0; i< positions.Length; i=i+2)
                {
                    //Debug.Log(positions[i] + " " + positions[i+1]);
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
                data.position = pos;
                data.rotationY = rot;
                data.time = time;
                File.WriteAllText(downloadedContentPath + "/Result" + id.ToString() + "t.txt", JsonUtility.ToJson(data));
                id++;
            }
            else
            {
                Debug.Log("Finished downloaded");
                keepGoing = false;
            }
        }
    }

    public class IdPost
    {
        public string id;

        public IdPost(string id)
        {
            this.id = id;
        }
    }

    public class TrajectoryReceived
    {
        public string position;
        public string rotation;
        public string timerobot;
    }

    public class MapReceived
    {
        public string uknown;
        public string wall;
        public string goal;
    }
}
