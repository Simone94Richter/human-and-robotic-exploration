using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GameUIManager is an abstract class used to implement any kind of game UI manager. A game UI
/// consists in different UIs activated in different phases of the game. Ready UI contains 
/// an explenation of the current game mode and a countdown. Figth UI contains information
/// about the current game (i.e. score, residual time). Score UI shows the outcome of the
/// current game. Pause UI is a simple pause screen.
/// </summary>
public abstract class GameUIManager : CoreComponent {

    [SerializeField] protected GameObject readyUI;
    [SerializeField] protected GameObject fightUI;
    [SerializeField] protected GameObject scoreUI;
    [SerializeField] protected GameObject pauseUI;

    [SerializeField] protected FadeUI fadeUIScript;

    // Activates the ready UI.
    public void ActivateReadyUI() {
        readyUI.SetActive(true);
        fightUI.SetActive(false);
        scoreUI.SetActive(false);
    }

    // Activates the fight UI.
    public void ActivateFightUI() {
        readyUI.SetActive(false);
        fightUI.SetActive(true);
        scoreUI.SetActive(false);
    }

    // Activates the score UI.
    public void ActivateScoreUI() {
        pauseUI.SetActive(false);
        readyUI.SetActive(false);
        fightUI.SetActive(false);
        scoreUI.SetActive(true);
    }

    // Activates or deactivates the pause UI.
    public void ActivatePauseUI(bool b) {
        fightUI.SetActive(!b);
        pauseUI.SetActive(b);
    }

    // Converts seconds and minutes to text and adds extra 0 if needed.
    protected string TimeToString(int t) {
        string s = t.ToString();

        if (t < 0) {
            return "00";
        } else if (s.Length > 1) {
            return s;
        } else {
            return "0" + s;
        }
    }

    public void Fade(float min, float max, bool mustLigthen, float duration) {
        fadeUIScript.StartFade(min, max, mustLigthen, duration);
    }

    public abstract void SetColorAll(Color c);

}