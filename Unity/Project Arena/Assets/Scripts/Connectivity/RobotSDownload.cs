using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;

public class RobotSDownload : RobotDownload {

    //private string surveyReceived;
    private string dataSurvey;
    private string[] surveys;

    public void DownloadSurvey()
    {
        StartCoroutine(Download());
    }

    private IEnumerator Download()
    {
        var uwr = UnityWebRequest.Post(url, "POST");
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        yield return uwr.SendWebRequest();

        if (uwr.downloadHandler.text != null)
        {
            Debug.Log(uwr.downloadHandler.text);
            //list.survey = JsonUtility.FromJson<SurveyList>(uwr.downloadHandler.text).survey;
            dataSurvey = uwr.downloadHandler.text;
            dataSurvey = dataSurvey.Replace("[", "");
            //dataSurvey = Regex.Replace(dataSurvey, @"}]", @"}");
            dataSurvey = dataSurvey.Replace("]", "");
            dataSurvey = Regex.Replace(dataSurvey, @",{", "|{");
            Debug.Log(dataSurvey);

            //dopodichè, si splitta per |
            surveys = dataSurvey.Split('|');

            for (int i = 0; i < surveys.Length; i++)
            {
                File.WriteAllText(downloadedContentPath + "/Survey/Result" + (i+1).ToString() + ".txt", surveys[i]);
            }
        }
    }
}
