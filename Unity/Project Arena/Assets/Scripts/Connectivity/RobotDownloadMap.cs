using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;

public class RobotDownloadMap : RobotDownload {

    //private int id;
    //private bool keepGoing = true;
    //private IdPost idObject;
    private MapReceived gameDataMap;
    private JsonMapObjects dataMap;
    private List<string> uCell = new List<string>();
    private List<string> wCell = new List<string>();
    private List<string> gCell = new List<string>();

    public void DownloadMap()
    {
        StartCoroutine(Download());
    }

    private IEnumerator Download()
    {
        dataMap = new JsonMapObjects();
        dataMap.u = new List<string>();
        dataMap.w = new List<string>();
        dataMap.g = new List<string>();
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
                gameDataMap = JsonUtility.FromJson<MapReceived>(uwr.downloadHandler.text);
                //Debug.Log(gameDataPos.timerobot);
                gameDataMap.uknown = Regex.Replace(gameDataMap.uknown, @"[()]", "");
                gameDataMap.uknown = Regex.Replace(gameDataMap.uknown, @"[ ]", "");
                string[] u = gameDataMap.uknown.Split(',');
                uCell = new List<string>();
                for (int i = 0; i < u.Length; i = i + 2)
                {
                    //Debug.Log(positions[i] + " " + positions[i+1]);
                    uCell.Add(u[i] + "," + u[i + 1]);
                }
                gameDataMap.wall = Regex.Replace(gameDataMap.wall, @"[()]", "");
                gameDataMap.wall = Regex.Replace(gameDataMap.wall, @"[ ]", "");
                string[] w = gameDataMap.wall.Split(',');
                wCell = new List<string>();
                if(w.Length >= 2)// just to be sure that two coordinates exists
                for (int i = 0; i < w.Length; i = i + 2)
                {
                    //Debug.Log(positions[i] + " " + positions[i+1]);
                    wCell.Add(w[i] + "," + w[i + 1]);
                }
                gameDataMap.goal = Regex.Replace(gameDataMap.goal, @"[()]", "");
                gameDataMap.goal = Regex.Replace(gameDataMap.goal, @"[ ]", "");
                string[] g = gameDataMap.goal.Split(',');
                //Debug.Log(g.Length);
                gCell = new List<string>();
                if(g.Length >= 2)// just to be sure that two coordinates exists
                for (int i = 0; i < g.Length; i = i + 2)
                {
                    gCell.Add(g[i] + "," + g[i + 1]);
                }
                dataMap.u = uCell;
                dataMap.w = wCell;
                dataMap.g = gCell;
                File.WriteAllText(downloadedContentPath + "/Result" + id.ToString() + "m.txt", JsonUtility.ToJson(dataMap));
                id++;
            }
            else
            {
                Debug.Log("Finished downloaded");
                keepGoing = false;
            }
        }

    }
}
