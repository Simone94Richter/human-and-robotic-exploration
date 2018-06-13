using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// This class manages the UI of the experiment control menu. The UI changes depending on the
/// current version and on the build platform. This class allows to start a new experiment and
/// download all the logs of the current experiment.
/// </summary>
public class ExperimentControlUIManager : MonoBehaviour {

    [Header("UI")] [SerializeField] private Button exitButton;
    [SerializeField] private Button downloadAllButton;
    [SerializeField] private Button resetCompletionButton;
    [SerializeField] private Button onlineExperimentButton;
    [SerializeField] private Button offlineExperimentButton;
    [SerializeField] private Button completeExperimentButton;
    [SerializeField] private Button directoryButton;
    [SerializeField] private RotateTranslateByAxis backgroundScript;

    [Header("Experiment")] [SerializeField] private string experimentScene;

    [Header("Download")] [SerializeField] private bool mergeLogs;

    private string downloadDirectory;
    private bool exitMustQuit;

    void Awake() {
        Cursor.lockState = CursorLockMode.None;

        if (ParameterManager.HasInstance()) {
            backgroundScript.SetRotation(ParameterManager.Instance.BackgroundRotation);
            if (ParameterManager.Instance.Version == ParameterManager.BuildVersion.COMPLETE) {
                SetExitButton(false);
            } else {
                SetExitButton(true);
            }
        } else {
            SetExitButton(true);
        }

        if (Application.platform != RuntimePlatform.WebGLPlayer) {
            offlineExperimentButton.interactable = true;
            completeExperimentButton.interactable = true;
            directoryButton.interactable = true;
            downloadAllButton.interactable = true;
            downloadDirectory = Application.persistentDataPath + "/Downloads";
            if (!Directory.Exists(downloadDirectory))
                Directory.CreateDirectory(downloadDirectory);
        }
    }

    public void Exit() {
        if (exitMustQuit) {
            Application.Quit();
        } else {
            ParameterManager.Instance.BackgroundRotation = backgroundScript.GetRotation();
            SceneManager.LoadScene("Menu");
        }
    }

    public void ResetCompletion() {
        SetButtonsInteractable(false);
        StartCoroutine(ResetCompletionAttempt());
    }

    public void DowloadAll() {
        SetButtonsInteractable(false);
        StartCoroutine(DowloadAllAttempt());
    }

    public void CreateHeatDataset() {
        SetButtonsInteractable(false);
        StartCoroutine(CreateHeatDatasetAttempt());
    }

    private IEnumerator ResetCompletionAttempt() {
        yield return StartCoroutine(ExperimentControlManager.ResetCompletionAttempt());

        SetButtonsInteractable(true);
    }

    private IEnumerator DowloadAllAttempt() {
        yield return StartCoroutine(ExperimentControlManager.DowloadAllAttempt(downloadDirectory,
            mergeLogs));
        SetButtonsInteractable(true);
    }

    private IEnumerator CreateHeatDatasetAttempt() {
        yield return StartCoroutine(
            ExperimentControlManager.CreateHeatDatasetAttempt(downloadDirectory));

        SetButtonsInteractable(true);
    }

    public void OpenDataDirectory() {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() {
            FileName = Application.persistentDataPath.Replace(@"/", @"\"),
            UseShellExecute = true,
            Verb = "open"
        });
    }

    private void SetButtonsInteractable(bool interactable) {
        if (Application.platform != RuntimePlatform.WebGLPlayer) {
            offlineExperimentButton.interactable = interactable;
            completeExperimentButton.interactable = interactable;
            directoryButton.interactable = interactable;
        }
        resetCompletionButton.interactable = interactable;
        downloadAllButton.interactable = interactable;
        onlineExperimentButton.interactable = interactable;
        exitButton.interactable = interactable;
    }

    private void SetExitButton(bool mustQuit) {
        if (mustQuit) {
            exitButton.GetComponentInChildren<Text>().text = "Quit";
            if (Application.platform != RuntimePlatform.WebGLPlayer) {
                exitButton.interactable = true;
            } else {
                exitButton.interactable = false;
            }
        } else {
            exitButton.GetComponentInChildren<Text>().text = "Back";
        }

        exitMustQuit = mustQuit;
    }

    public void LoadOnlineExperiment() {
        ParameterManager.Instance.LogOnline = true;
        ParameterManager.Instance.LogOffline = false;
        ParameterManager.Instance.LogSetted = true;
        ParameterManager.Instance.ExperimentControlScene = SceneManager.GetActiveScene().name;
        ParameterManager.Instance.BackgroundRotation = backgroundScript.GetRotation();
        SceneManager.LoadScene(experimentScene);
    }

    public void LoadOfflineExperiment() {
        ParameterManager.Instance.LogOnline = false;
        ParameterManager.Instance.LogOffline = true;
        ParameterManager.Instance.LogSetted = true;
        ParameterManager.Instance.ExperimentControlScene = SceneManager.GetActiveScene().name;
        ParameterManager.Instance.BackgroundRotation = backgroundScript.GetRotation();
        SceneManager.LoadScene(experimentScene);
    }

    public void LoadCompleteExperiment() {
        ParameterManager.Instance.LogOnline = true;
        ParameterManager.Instance.LogOffline = true;
        ParameterManager.Instance.LogSetted = true;
        ParameterManager.Instance.ExperimentControlScene = SceneManager.GetActiveScene().name;
        ParameterManager.Instance.BackgroundRotation = backgroundScript.GetRotation();
        SceneManager.LoadScene(experimentScene);
    }

}