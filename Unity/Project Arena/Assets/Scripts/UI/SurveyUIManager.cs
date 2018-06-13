using JsonObjects.Logging.Survey;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class manages the UI of the survey. A survey is a sequence of screens that starts with an
/// introduction, continues with some questions and ends with a thanks.
/// </summary>
public class SurveyUIManager : MonoBehaviour {

    [Header("Survey fields")] [SerializeField] private GameObject introduction;
    [SerializeField] private GameObject[] questions;
    [SerializeField] private GameObject thanks;

    [Header("Other")] [SerializeField] private RotateTranslateByAxis backgroundScript;

    private int currentQuestion = 0;

    private List<JsonQuestion> jQuestions;
    private List<JsonAnswer> jAnswers;

    private void Start() {
        Cursor.lockState = CursorLockMode.None;

        jQuestions = new List<JsonQuestion>();
        jAnswers = new List<JsonAnswer>();

        backgroundScript.SetRotation(ParameterManager.Instance.BackgroundRotation);
    }

    // Saves the values of the survey and quits
    public void Submit() {
        ParameterManager.Instance.BackgroundRotation = backgroundScript.GetRotation();

        thanks.SetActive(false);

        if (ExperimentManager.Instance.MustSaveSurvey()) {
            ExperimentManager.Instance.SaveSurvey(jQuestions);
        }
        ExperimentManager.Instance.SaveAnswers(jAnswers);
        ExperimentManager.Instance.LoadNextScene();
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
        UpdateValues();

        questions[currentQuestion].SetActive(false);

        if (currentQuestion < questions.Length - 1) {
            currentQuestion++;
            questions[currentQuestion].SetActive(true);
        } else {
            thanks.SetActive(true);
        }
    }
}