using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// GameManager is an abstract class used to define and manage a game mode. It implements an 
/// ILoggable interface that allows to log the game.
/// </summary>
public abstract class GameManager : CoreComponent, ILoggable {

    [Header("Managers")] [SerializeField] protected MapManager mapManagerScript;
    [SerializeField] protected SpawnPointManager spawnPointManagerScript;

    [Header("Game")] [SerializeField] protected bool generateOnly;
    [SerializeField] protected int gameDuration = 600;
    [SerializeField] protected int readyDuration = 3;
    [SerializeField] protected int scoreDuration = 10;
    [SerializeField] protected float respawnDuration = 3;
    [SerializeField] protected Slider loadingBar;

    // Time at which the game started.
    protected float startTime;
    // Current phase of the game: 0 = ready, 1 = figth, 2 = score.
    protected int gamePhase = -1;
    // Is the game paused?
    protected bool isPaused = false;

    // Do I have to handshake with the Experiment Manager?
    protected bool handshaking = false;
    // Do I have to log?
    protected bool loggingGame = false;

    // Moves a gameobject to a free spawn point.
    public void Spawn(GameObject g) {
        g.transform.position = spawnPointManagerScript.GetSpawnPosition() + Vector3.up * 3f;
    }

    // Menages the death of an entity.
    public abstract void MenageEntityDeath(GameObject g, Entity e);

    protected abstract void UpdateGamePhase();

    protected abstract void ManageGame();

    public abstract void AddScore(int i, int j);

    public abstract void SetUIColor(Color c);

    public abstract void Pause();

    public IEnumerator FreezeTime(float wait, bool mustPause) {
        if (mustPause) {
            yield return new WaitForSeconds(wait);
        }
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void Quit() {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        LoadNextScene("Menu");
    }

    // Loads the next scene
    private void LoadNextScene(string def) {
        if (ExperimentManager.HasInstance() && ParameterManager.HasInstance()) {
            ExperimentManager.Instance.SetSlider(loadingBar);
            ExperimentManager.Instance.LoadNextScene();
        } else {
            SceneManager.LoadScene(def);
        }
    }

    // Allows the Game Manager to tell the Experiment Manager when it can start to log.
    public void LoggingHandshake() {
        handshaking = true;
    }

    // Setups stuff for the loggingGame.
    public void SetupLogging() {
        ExperimentManager.Instance.LogMapInfo(mapManagerScript.GetMapGenerator().GetHeight(),
            mapManagerScript.GetMapGenerator().GetWidth(),
            mapManagerScript.GetMapAssembler().GetSquareSize(), mapManagerScript.GetFlip());
        ExperimentManager.Instance.LogGameInfo(gameDuration, SceneManager.GetActiveScene().name);

        loggingGame = true;
    }

    // Tells if it is loggingGame.
    public bool IsLogging() {
        return loggingGame;
    }

}