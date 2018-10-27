using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// This class is responsible to manage the operation of uploading usefull data to the server,
/// used later for experimantal study
/// </summary>
public class RobotConnection : MonoBehaviour {

    [Header("URL for loading the map information")]
    public string url = "http://travellersinn.herokuapp.com/tesi/postData.php";
    [Header("URL for loading information about the trajectory of the robot")]
    public string url2 = "http://travellersinn.herokuapp.com/tesi/postTrajectory.php";
    [Header("Is the upload of data completed?")]
    public bool uploadComplete = false;
    [Header("The Slider object used to show the sending data progress")]
    public Slider loadingBar;

    public void SendDataToServer(string jsonMap, string jsonPositions)
    {
        StartCoroutine(Upload(jsonMap, jsonPositions));
    }

    /// <summary>
    /// This method is responsible for sending data to the server
    /// </summary>
    /// <param name="json1">The content of the JSON about the map</param>
    /// <param name="json2">The content of the JSON about the robot trajectory</param>
    /// <returns></returns>
    private IEnumerator Upload(string json1, string json2)
    {
        Debug.Log(SystemInfo.operatingSystem);
        var uwr = new UnityWebRequest(url, "POST");
        //Debug.Log(json1);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json1);
        //Debug.Log(jsonToSend);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        //uwr.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        //yield return uwr.SendWebRequest();
        AsyncOperation operation = uwr.SendWebRequest();

        if (loadingBar)
        {
            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / .9f);

                loadingBar.value = progress;

                yield return null;
            }
            loadingBar.value = 0;
        }
        else yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
        }

        var uwr2 = new UnityWebRequest(url2, "POST");
        //Debug.Log(json2);
        byte[] jsonToSend2 = new System.Text.UTF8Encoding().GetBytes(json2);
        //Debug.Log(jsonToSend2);
        uwr2.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend2);
        uwr2.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        //uwr.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        //yield return uwr2.SendWebRequest();
        AsyncOperation operation2 = uwr2.SendWebRequest();

        if (loadingBar)
        {
            while (!operation2.isDone)
            {
                float progress = Mathf.Clamp01(operation2.progress / .9f);

                loadingBar.value = progress;

                yield return null;
            }
            loadingBar.value = 0;
        }
        else yield return uwr2.SendWebRequest();

        if (uwr2.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr2.error);
        }
        else
        {
            Debug.Log("Received: " + uwr2.downloadHandler.text);
        }

        uploadComplete = true;
    }

}
