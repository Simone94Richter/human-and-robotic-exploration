using System.Collections;
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

    private AsyncOperation operation; //forse se ne puo' usare uno solo
    private AsyncOperation operation2;

    private byte[] jsonToSend;  //forse se ne puo' usare uno solo
    private byte[] jsonToSend2;

    private float progress;

    private UnityWebRequest postMapRequest;
    private UnityWebRequest postTrajectoryRequest;

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
        //Setting the POST request what should be sent and returned
        postMapRequest = new UnityWebRequest(url, "POST");
        jsonToSend = new System.Text.UTF8Encoding().GetBytes(json1);
        postMapRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        postMapRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        //uwr.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns

        if (loadingBar)
        {
            operation = postMapRequest.SendWebRequest();
            while (!operation.isDone)
            {
                progress = Mathf.Clamp01(operation.progress / .9f);

                loadingBar.value = progress;

                yield return null;
            }
            loadingBar.value = 0;
        }
        else yield return postMapRequest.SendWebRequest();

        //Display error in case the request was not arrived; otherwise, publish the return message
        if (postMapRequest.isNetworkError)
        {
            Debug.Log("Error While Sending: " + postMapRequest.error);
        }
        else
        {
            Debug.Log("Received: " + postMapRequest.downloadHandler.text);
        }

        //Same as before

        postTrajectoryRequest = new UnityWebRequest(url2, "POST");
        jsonToSend2 = new System.Text.UTF8Encoding().GetBytes(json2);
        postTrajectoryRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend2);
        postTrajectoryRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        //uwr.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns

        if (loadingBar)
        {
            operation2 = postTrajectoryRequest.SendWebRequest();
            while (!operation2.isDone)
            {
                progress = Mathf.Clamp01(operation2.progress / .9f);

                loadingBar.value = progress;

                yield return null;
            }
            loadingBar.value = 0;
        }
        else yield return postTrajectoryRequest.SendWebRequest();

        if (postTrajectoryRequest.isNetworkError)
        {
            Debug.Log("Error While Sending: " + postTrajectoryRequest.error);
        }
        else
        {
            Debug.Log("Received: " + postTrajectoryRequest.downloadHandler.text);
            if (!postTrajectoryRequest.downloadHandler.text.Contains("something")) //every error message contains that word. If not, it's an IP address
            {
                ExperimentManager.Instance.SetIpAddress(postTrajectoryRequest.downloadHandler.text);
            }
        }

        //Everything has been sent, so the manager needs to know that the upload is finished
        uploadComplete = true;
    }

}
