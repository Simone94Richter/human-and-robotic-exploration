using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// This class is responsible for downloading map results from the server
/// </summary>
public class RobotDownloadMap : RobotDownload {

    private MapReceived gameDataMap;
    private JsonMapObjects dataMap;
    private List<string> uCell = new List<string>();
    private List<string> wCell = new List<string>();
    private List<string> gCell = new List<string>();

    /// <summary>
    /// This method calls the coroutine to start the procedure to download data
    /// </summary>
    public void DownloadMap()
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
        dataMap = new JsonMapObjects();
        dataMap.u = new List<string>();
        dataMap.w = new List<string>();
        dataMap.g = new List<string>();
        id = 1;
        while (keepGoing)
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
                gameDataMap = JsonUtility.FromJson<MapReceived>(uwr.downloadHandler.text);
                gameDataMap.uknown = Regex.Replace(gameDataMap.uknown, @"[()]", "");
                gameDataMap.uknown = Regex.Replace(gameDataMap.uknown, @"[ ]", "");
                string[] u = gameDataMap.uknown.Split(',');
                uCell = new List<string>();
                if (u.Length >= 2) //just to be that two coordinates exists
                {
                    for (int i = 0; i < u.Length; i = i + 2)
                    {
                        //Debug.Log(positions[i] + " " + positions[i+1]);
                        uCell.Add(u[i] + "," + u[i + 1]);
                    }
                }
                gameDataMap.wall = Regex.Replace(gameDataMap.wall, @"[()]", "");
                gameDataMap.wall = Regex.Replace(gameDataMap.wall, @"[ ]", "");
                string[] w = gameDataMap.wall.Split(',');
                wCell = new List<string>();
                if (w.Length >= 2) // just to be sure that two coordinates exists
                {
                    for (int i = 0; i < w.Length; i = i + 2)
                    {
                        //Debug.Log(positions[i] + " " + positions[i+1]);
                        wCell.Add(w[i] + "," + w[i + 1]);
                    }
                }
                gameDataMap.goal = Regex.Replace(gameDataMap.goal, @"[()]", "");
                gameDataMap.goal = Regex.Replace(gameDataMap.goal, @"[ ]", "");
                string[] g = gameDataMap.goal.Split(',');
                gCell = new List<string>();
                if (g.Length >= 2) // just to be sure that two coordinates exists
                {
                    for (int i = 0; i < g.Length; i = i + 2)
                    {
                        gCell.Add(g[i] + "," + g[i + 1]);
                    }
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
