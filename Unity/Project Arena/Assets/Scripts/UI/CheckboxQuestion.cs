using JsonObjects.Logging.Survey;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CheckboxQuestion allows to menage a list of checkboxes plus a submit button. The exclusive flag
/// imposes to have at most a selected box. The compulsory flag imposes to have at least a selected
/// box.
/// </summary>
public class CheckboxQuestion : MonoBehaviour {

    [SerializeField] private int questionId;
    [SerializeField] private Option[] options;
    [SerializeField] private Button submit;
    [SerializeField] private bool exclusive;
    [SerializeField] private bool compulsory;

    private int activeCount = 0;

    private void Start() {
        if (!compulsory)
            submit.interactable = true;
    }

    // Updates the active answers.
    public void UpdateAnswer(int id) {
        bool activated = GetOptionById(id).toggle.isOn;

        if (exclusive && activated) {
            foreach (Option o in options) {
                if (o.id != id && o.toggle.isOn) {
                    // Updating the isOn value calls again this method, so there is non need
                    // to decrease the active count now.
                    o.toggle.isOn = false;
                }
            }
        }

        activeCount = activated ? activeCount + 1 : activeCount - 1;
        submit.interactable = activeCount > 0 || !compulsory ? true : false;
    }

    // Returns the oprion given its id.
    private Option GetOptionById(int id) {
        foreach (Option o in options) {
            if (o.id == id) {
                return o;
            }
        }
        return new Option();
    }

    // Returns the active answers as an array.
    private int[] GetActiveAnswers() {
        int[] activeAnswers = new int[activeCount];

        int j = 0;
        for (int i = 0; i < options.Length; i++) {
            if (options[i].toggle.isOn) {
                activeAnswers[j] = options[i].id;
                j++;
            }
        }

        return activeAnswers;
    }

    // Converts the answer in Json format.
    public JsonAnswer GetJsonAnswer() {
        return new JsonAnswer(questionId, GetActiveAnswers());
    }

    // Converts the question and the options in Json format.
    public JsonQuestion GetJsonQuestion() {
        return new JsonQuestion(questionId, transform.GetComponentInChildren<Text>().text,
            GetJsonOptions());
    }

    public string GetAnswer()
    {
        int[] activeAnswers = GetActiveAnswers();
        string markedChoiche = "";

        for (int i = 0; i < activeAnswers.Length; i++)
        {
            markedChoiche = markedChoiche + " " + activeAnswers[i].ToString();
        }

        return markedChoiche;
    }

    // Converts the options in Json format.
    private List<JsonOption> GetJsonOptions() {
        List<JsonOption> jOptions = new List<JsonOption>();

        for (int i = 0; i < options.Length; i++) {
            jOptions.Add(new JsonOption((int)options[i].id,
                options[i].toggle.transform.parent.GetComponentInChildren<Text>().text));
        }

        return jOptions;
    }

    [Serializable]
    private struct Option {
        public Toggle toggle;
        public int id;
    }

}