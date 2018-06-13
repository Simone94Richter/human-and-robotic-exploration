using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// This class manages the UI of the error menu. The error to display is retrieved from the
/// Parameter Manager. The back button loads different scenes depending on the build.
/// </summary>
public class ErrorUIManager : MonoBehaviour {

    [SerializeField] private RotateTranslateByAxis backgroundScript;
    [SerializeField] private Text errorText;

    void Start() {
        if (ParameterManager.HasInstance()) {
            backgroundScript.SetRotation(ParameterManager.Instance.BackgroundRotation);

            SetErrorMessage(ParameterManager.Instance.ErrorCode,
                ParameterManager.Instance.ErrorMessage);
            ParameterManager.Instance.ErrorCode = 0;
        }
    }

    // Sets the error message.
    private void SetErrorMessage(int errorCode, string errorMessage) {
        switch (errorCode) {
            case 1:
                errorText.text = errorMessage;
                break;
            case 2:
                errorText.text = "Error while loading the map.\nThe specified file was not " +
                    "found.\nPlease put the file in the rigth folder.";
                break;
            case 3:
                errorText.text = "Error while loading the map.\nThe map must be rectangular, " +
                    "with at least one spawn point and walls around its border.";
                break;
            case 4:
                errorText.text = "Error while loading the map.\nThe map exceeds the maximum " +
                    "dimension.";
                break;
            case 5:
                errorText.text = "Error while loading the map.\nThe genome doesn't follow the " +
                    "expected convention.";
                break;
            case 6:
                errorText.text = "Error while loading the map.\nEach level must be rectangular " +
                    "and have the same size, with at least one spawn point and walls around its " +
                    "border.";
                break;
            default:
                errorText.text = "Something really bad just happened.";
                break;
        }
    }

    public void Back() {
        ParameterManager.Instance.BackgroundRotation = backgroundScript.GetRotation();

        if (ParameterManager.Instance.InitialScene != null) {
            SceneManager.LoadScene(ParameterManager.Instance.InitialScene);
        } else {
            SceneManager.LoadScene("Start");
        }
    }

}