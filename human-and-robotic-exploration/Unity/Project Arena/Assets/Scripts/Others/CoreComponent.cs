using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// CoreComponent is an abstract class that must be extended by all classes that have to be fully
/// initialized before the game can start. It also provides methods to menage errors.
/// </summary>
public abstract class CoreComponent : MonoBehaviour {

    protected enum Error { SOFT_ERROR, HARD_ERROR, RETRY_ERROR };

    // Has the script completed the execution of the start method?
    private bool ready = false;

    // Has the script completed the execution of the start method?
    protected void SetReady(bool r) {
        ready = r;
    }

    // Tells if the scipt is done loading.
    public bool IsReady() {
        return ready;
    }

    // Menages errors going back to the main menu.
    protected void ManageError(Error error, int errorCode) {
        switch (error) {
            case Error.SOFT_ERROR:
                Debug.LogError("Unexpected soft error with code " + errorCode + ".");
                break;
            case Error.HARD_ERROR:
                ErrorManager.ErrorBackToMenu(errorCode);
                break;
            case Error.RETRY_ERROR:
                Debug.LogError("Unexpected error with code " + errorCode + ". The scene will be" +
                    " reloaded.");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
        }
    }

    // Menages errors going back to the main menu.
    protected void ManageError(Error error, string errorMessage) {
        switch (error) {
            case Error.SOFT_ERROR:
                Debug.LogError("Unexpected soft error. " + errorMessage);
                break;
            case Error.HARD_ERROR:
                ErrorManager.ErrorBackToMenu(errorMessage);
                break;
            case Error.RETRY_ERROR:
                Debug.LogError("Unexpected  error. " + errorMessage + " The scene will be " +
                    "reloaded.");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
        }
    }

}