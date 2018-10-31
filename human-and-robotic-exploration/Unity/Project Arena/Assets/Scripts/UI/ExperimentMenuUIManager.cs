using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// This class manages the UI of the experiment menu. The exit/quit button is enabled by pressing
/// "e", "s" and "c" at the same time.
/// </summary>
public class ExperimentMenuUIManager : MonoBehaviour {

    [Header("Menu")] [SerializeField] private GameObject menu;
    [SerializeField] private GameObject loading;

    [Header("Other")] [SerializeField] private RotateTranslateByAxis backgroundScript;
    [SerializeField] private Button exitButton;

    private bool exitMustQuit;

    void Start() {
        Cursor.lockState = CursorLockMode.None;

        if (ParameterManager.HasInstance()) {
            backgroundScript.SetRotation(ParameterManager.Instance.BackgroundRotation);
            if (ParameterManager.Instance.Version == ParameterManager.BuildVersion.COMPLETE ||
                ParameterManager.Instance.Version ==
                ParameterManager.BuildVersion.EXPERIMENT_CONTROL) {
                SetExitButton(false);
            } else {
                SetExitButton(true);
            }
        } else {
            SetExitButton(true);
        }
    }

    private void Update() {
        if (Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.C)) {
            if (!exitMustQuit || Application.platform != RuntimePlatform.WebGLPlayer) {
                exitButton.interactable = true;
            }
        }
    }

    // Loads the level indicated by the Experiment Manager.
    public void Play() {
        SetLoadingVisible(true);
        StartCoroutine(ExperimentManager.Instance.StartNewExperiment());
    }

    public void Exit() {
        if (exitMustQuit) {
            Application.Quit();
        } else {
            ParameterManager.Instance.BackgroundRotation = backgroundScript.GetRotation();
            SceneManager.LoadScene(ParameterManager.Instance.ExperimentControlScene);
        }
    }

    public void SetLoadingVisible(bool visible) {
        if (visible) {
            menu.SetActive(false);
            loading.SetActive(true);
        } else {
            menu.SetActive(true);
            loading.SetActive(false);
        }
    }

    private void SetExitButton(bool mustQuit) {
        if (mustQuit) {
            exitButton.GetComponentInChildren<Text>().text = "Quit";
        } else {
            exitButton.GetComponentInChildren<Text>().text = "Back";
        }

        exitButton.interactable = false;
        exitMustQuit = mustQuit;
    }

}