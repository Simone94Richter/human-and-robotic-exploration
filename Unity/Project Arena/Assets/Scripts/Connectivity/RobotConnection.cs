using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RobotConnection : MonoBehaviour {

    public string url = "http://travellersinn.herokuapp.com/tesi/postData.php";

    public void SendDataToServer(string jsonMap, string jsonPositions)
    {

        StartCoroutine(Upload(url, jsonMap, jsonPositions));
    }

    private IEnumerator Upload(string url, string json1, string json2)
    {
        var uwr = new UnityWebRequest(url, "POST");
        Debug.Log(json1);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json1);
        Debug.Log(jsonToSend);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        //uwr.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
        }

        /*WWWForm form = new WWWForm();
        form.AddField("myField", "myData");

        UnityWebRequest www = UnityWebRequest.Post("http://www.my-server.com/myform", form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }*/
    }

}
