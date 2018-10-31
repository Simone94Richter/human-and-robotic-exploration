using ExperimentObjects;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ExperimentInitializer allows to define experiments.
/// </summary>
public class ExperimentInitializer : MonoBehaviour {

    [Header("Tutorial")] [SerializeField] private Case tutorial;
    [SerializeField] private bool playTutorial;

    [Header("Experiment")] [SerializeField] private List<Study> studies;
    [SerializeField] private int casesPerUsers;
    [SerializeField] private string experimentName;

    [Header("Survey")] [SerializeField] private Case survey;
    [SerializeField] private bool playSurvey;

    [Header("Logging")] [SerializeField] private bool logOffline;
    [SerializeField] private bool logOnline;
    [SerializeField] private bool logGame;
    [SerializeField] private bool logStatistics;

    void Awake() {
        if (ParameterManager.HasInstance()) {
            logOnline = ParameterManager.Instance.LogOnline;
            logOffline = ParameterManager.Instance.LogOffline;
        }

        if (!ExperimentManager.HasInstance()) {
            ExperimentManager.Instance.Setup(tutorial, playTutorial, studies, casesPerUsers,
                experimentName, survey, playSurvey, logOffline, logOnline, logGame, logStatistics);
        }
    }

}