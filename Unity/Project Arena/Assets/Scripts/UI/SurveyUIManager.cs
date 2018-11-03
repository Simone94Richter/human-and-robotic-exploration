using JsonObjects.Logging.Survey;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// This class manages the UI of the survey. A survey is a sequence of screens that starts with an
/// introduction, continues with some questions and ends with a thanks.
/// </summary>
public class SurveyUIManager : MonoBehaviour {

    [Header("Survey fields")] [SerializeField] private GameObject introduction;
    [SerializeField] private GameObject[] questions;
    [SerializeField] private GameObject thanks;
    [SerializeField] private List<MapSet> mapsSet;
    [SerializeField] private TextAsset[] mapsForSurvey;

    [Header("Other")] [SerializeField] private RotateTranslateByAxis backgroundScript;

    private bool finalQuestion = false;

    private int currentQuestion = 0;
    private int setOfMapSelected;

    private List<JsonQuestion> jQuestions;
    private List<JsonAnswer> jAnswers;

    private Answers answers;
    private JsonSurvey survey;

    private RobotSurveyDownload rSD;

    private void Start() {
        answers = new Answers();
        answers.choices = new List<string>();

        Cursor.lockState = CursorLockMode.None;

        jQuestions = new List<JsonQuestion>();
        jAnswers = new List<JsonAnswer>();

        rSD = GetComponent<RobotSurveyDownload>();
        survey = new JsonSurvey();

        backgroundScript.SetRotation(ParameterManager.Instance.BackgroundRotation);
    }

    // Saves the values of the survey and quits
    public void Submit() {
        ParameterManager.Instance.BackgroundRotation = backgroundScript.GetRotation();

        thanks.SetActive(false);

        //if (ExperimentManager.Instance.MustSaveSurvey()) {
        //    ExperimentManager.Instance.SaveSurvey(jQuestions);
        //}
        //ExperimentManager.Instance.SaveAnswers(jAnswers);
        survey.choices = new List<string>();
        survey.choices = answers.choices;
        if (ExperimentManager.HasInstance())
        {
            survey.mapname = ExperimentManager.Instance.GetCaseList()[2].GetCurrentMap().name;
        }

        rSD.SendDataToServer(JsonUtility.ToJson(survey));

        StartCoroutine(FinishingExperiment());
    }

    private IEnumerator FinishingExperiment()
    {
        while (!rSD.uploadComplete)
        {
            yield return null;
        }

        SceneManager.LoadScene("Menu");
    }

    // Updates the values of the survey.
    private void UpdateValues() {
        if (ExperimentManager.Instance.MustSaveSurvey()) {
            jQuestions.Add(questions[currentQuestion].GetComponent<CheckboxQuestion>().
                GetJsonQuestion());
        }
        jAnswers.Add(questions[currentQuestion].GetComponent<CheckboxQuestion>().GetJsonAnswer());
    }

    // Saves the values of the survey and quits.
    private void SaveAndQuit() {

    }

    // Shows the first question.
    public void FirstQuestion() {
        introduction.SetActive(false);
        questions[currentQuestion].SetActive(true);
    }

    // Shows the next question.
    public void NextQuestion() {
        //UpdateValues();

        answers.choices.Add(questions[currentQuestion].GetComponent<CheckboxQuestion>().GetAnswer());

        questions[currentQuestion].SetActive(false);

        if (currentQuestion < questions.Length - 1) {
            currentQuestion++;
            questions[currentQuestion].SetActive(true);

            CheckIfImageQuestion();

        } else {
            finalQuestion = true;
            thanks.SetActive(true);
        }

        if (currentQuestion != 3 || finalQuestion)
        {
            foreach (GameObject g in mapsSet[setOfMapSelected].maps)
            {
                g.SetActive(false);
            }
        }
    }

    public void PrevPage()
    {

        if (currentQuestion > 0)
        {
            if (!finalQuestion)
            {
                answers.choices.RemoveAt(answers.choices.Count - 1);

                questions[currentQuestion].SetActive(false);
                currentQuestion--;
                questions[currentQuestion].SetActive(true);
            }
            else
            {
                answers.choices.RemoveAt(answers.choices.Count - 1);
                finalQuestion = false;
                thanks.SetActive(false);
                questions[currentQuestion].SetActive(true);
            }

            CheckIfImageQuestion();

            if (currentQuestion != 3 || finalQuestion)
            {
                foreach (GameObject g in mapsSet[setOfMapSelected].maps)
                {
                    g.SetActive(false);
                }
            }
        }
    }

    private void CheckIfImageQuestion()
    {
        if (currentQuestion == 3)
        {
            string name;
            if (ExperimentManager.HasInstance())
            {
                name = ExperimentManager.Instance.GetCaseList()[2].GetCurrentMap().name;

                for (int i = 0; i < mapsForSurvey.Length; i++)
                {
                    if (name == mapsForSurvey[i].name)
                    {
                        setOfMapSelected = i;
                        foreach (GameObject g in mapsSet[i].maps)
                        {
                            g.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class Answers
    {
        public List<string> choices;
    }

    [Serializable]
    public class JsonSurvey
    {
        public List<string> choices;
        public string mapname;
    }

    [Serializable]
    public class MapSet
    {
        public GameObject[] maps;
    }

}