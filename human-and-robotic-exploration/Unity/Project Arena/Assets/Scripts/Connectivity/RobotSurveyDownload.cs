using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RobotSurveyDownload : MonoBehaviour {

    [Header("URL for loading the map information")]
    public string url = "http://travellersinn.herokuapp.com/tesi/postSurvey.php";
    [Header("Is the upload of data completed?")]
    public bool uploadComplete = false;
    [Header("The Slider object used to show the sending data progress")]
    public Slider loadingBar;

    public void SendDataToServer(string json)
    {
        StartCoroutine(Upload(json));
    }

    /// <summary>
    /// This method is responsible for sending data to the server
    /// </summary>
    /// <param name="json">The content of the JSON about the robot trajectory</param>
    /// <returns></returns>
    private IEnumerator Upload(string json)
    {
        var uwr = new UnityWebRequest(url, "POST");
        Debug.Log(json);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        //Debug.Log(jsonToSend);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        //uwr.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        //yield return uwr.SendWebRequest();
        if (loadingBar)
        {
            AsyncOperation operation = uwr.SendWebRequest();
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

        uploadComplete = true;
    }
}
