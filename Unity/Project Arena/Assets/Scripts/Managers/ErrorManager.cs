using UnityEngine.SceneManagement;

/// <summary>
/// ErrorManager allows to menage errors.
/// </summary>
public static class ErrorManager {

    // Menages errors going back to the main menu.
    public static void ErrorBackToMenu(int errorCode) {
        ParameterManager.Instance.ErrorCode = errorCode;
        SceneManager.LoadScene("Error");
    }

    // Menages errors going back to the main menu.
    public static void ErrorBackToMenu(string errorMessage) {
        ParameterManager.Instance.ErrorCode = 1;
        ParameterManager.Instance.ErrorMessage = errorMessage;
        SceneManager.LoadScene("Error");
    }

}
