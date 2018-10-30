using System.Collections;
using UnityEngine;

/// <summary>
/// TargetHuntGameManager is an implementation of GameManager. The target hunt game mode consists in 
/// finding and destroying a single target as many times as possible before time runs. After the
/// target is destroyed a new one is spawned at a different spawn point.
/// </summary>
public class TargetHuntGameManager : GameManager {

    [Header("Contenders")] [SerializeField] private GameObject player;
    [SerializeField] private int totalHealthPlayer = 100;
    [SerializeField] private bool[] activeGunsPlayer;
    [SerializeField] private GameObject[] targetList;

    [Header("Target Hunt variables")] [SerializeField] protected TargetHuntGameUIManager targetHuntGameUIManagerScript;

    private Player playerScript;
    private int playerScore = 0;
    private int playerID = 0;

    private int currentTarget = 0;

    private void Start() {
        /* #if UNITY_EDITOR
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        #endif */

        playerScript = player.GetComponent<Player>();

        targetHuntGameUIManagerScript.Fade(0.7f, 1f, true, 0.5f);
    }

    private void Update() {
        if (!IsReady() && mapManagerScript.IsReady() && spawnPointManagerScript.IsReady() &&
            targetHuntGameUIManagerScript.IsReady()) {
            // Generate the map.
            mapManagerScript.ManageMap(true);

            if (!generateOnly) {
                // Set the spawn points.
                spawnPointManagerScript.SetSpawnPoints(mapManagerScript.GetSpawnPoints());

                // Spawn the player.
                Spawn(player);

                // Setup the contenders.
                playerScript.SetupEntity(totalHealthPlayer, activeGunsPlayer, this, playerID);

                playerScript.LockCursor();
                startTime = Time.time;
            }

            // If needed, tell the Experiment Manager it can start loggingGame.
            if (handshaking) {
                ExperimentManager.Instance.StartLogging();
            }

            SetReady(true);
        } else if (IsReady() && !generateOnly) {
            ManageGame();
        }
    }

    // Updates the phase of the game.
    protected override void UpdateGamePhase() {
        int passedTime = (int)(Time.time - startTime);

        if (gamePhase == -1) {
            // Disable the player movement and interactions, activate the ready UI and set the 
            // phase.
            playerScript.SetInGame(false);
            targetHuntGameUIManagerScript.ActivateReadyUI();
            gamePhase = 0;
        } else if (gamePhase == 0 && passedTime >= readyDuration) {
            // Enable the player movement and interactions, activate the fight UI, set the score to 
            // zero, the wave to 1 and set the phase.
            targetHuntGameUIManagerScript.Fade(0.7f, 0f, false, 0.25f);
            targetHuntGameUIManagerScript.SetScore(0);
            spawnPointManagerScript.UpdateLastUsed();
            StartCoroutine(SpawnTarget());
            playerScript.SetInGame(true);
            targetHuntGameUIManagerScript.ActivateFightUI();
            gamePhase = 1;
        } else if (gamePhase == 1 && passedTime >= readyDuration + gameDuration) {
            // Disable the player movement and interactions, activate the score UI, set the winner 
            // and set the phase.
            playerScript.SetInGame(false);
            targetHuntGameUIManagerScript.Fade(0.7f, 0, true, 0.5f);
            targetHuntGameUIManagerScript.SetFinalScore(playerScore);
            targetHuntGameUIManagerScript.ActivateScoreUI();
            gamePhase = 2;
        } else if (gamePhase == 2 && passedTime >= readyDuration + gameDuration + scoreDuration) {
            Quit();
            gamePhase = 3;
        }
    }

    // Manages the gamed depending on the current time.
    protected override void ManageGame() {
        UpdateGamePhase();

        switch (gamePhase) {
            case 0:
                // Update the countdown.
                targetHuntGameUIManagerScript.SetCountdown((int)(startTime + readyDuration -
                    Time.time));
                break;
            case 1:
                // Update the time.
                targetHuntGameUIManagerScript.SetTime((int)(startTime + readyDuration +
                    gameDuration - Time.time));
                // Pause or unpause if needed.
                if (Input.GetKeyDown(KeyCode.Escape)) {
                    Pause();
                }
                break;
            case 2:
                // Do nothing.
                break;
        }
    }

    // Sets the color of the UI.
    public override void SetUIColor(Color c) {
        targetHuntGameUIManagerScript.SetColorAll(c);
    }

    // Called when a target is destroyed, adds score and time and changes wave if the target is the 
    // last one.
    public override void AddScore(int scoreIncrease, int timeIncrease) {
        // I need to ignore the targets call to decrease score.
        if (scoreIncrease > 0) {
            playerScore += scoreIncrease;
            targetHuntGameUIManagerScript.SetScore(playerScore);
            targetHuntGameUIManagerScript.AddScore(scoreIncrease);

            // Spawn a new target.
            StartCoroutine(SpawnTarget());
        }
    }

    // Pauses and unpauses the game.
    public override void Pause() {
        if (!isPaused) {
            targetHuntGameUIManagerScript.Fade(0f, 0.7f, false, 0.25f);
            player.GetComponent<PlayerUIManager>().SetPlayerUIVisible(false);
            playerScript.ShowGun(false);
            targetHuntGameUIManagerScript.ActivatePauseUI(true);
            playerScript.EnableInput(false);
        } else {
            targetHuntGameUIManagerScript.Fade(0f, 0.7f, true, 0.25f);
            player.GetComponent<PlayerUIManager>().SetPlayerUIVisible(true);
            playerScript.ShowGun(true);
            targetHuntGameUIManagerScript.ActivatePauseUI(false);
            playerScript.EnableInput(true);
        }

        isPaused = !isPaused;

        StartCoroutine(FreezeTime(0.25f, isPaused));
    }

    // Spawns a targets.
    private IEnumerator SpawnTarget() {
        yield return new WaitForSeconds(respawnDuration);

        GameObject newTarget = (GameObject)Instantiate(targetList[currentTarget]);
        newTarget.name = targetList[currentTarget].name;
        newTarget.transform.position = spawnPointManagerScript.GetSpawnPosition();
        newTarget.GetComponent<Entity>().SetupEntity(0, null, this, 0);

        currentTarget++;
        if (currentTarget == targetList.GetLength(0)) {
            currentTarget = 0;
        }
    }

    // Ends the game.
    public override void MenageEntityDeath(GameObject g, Entity e) {
        // Start the respawn process.
        StartCoroutine(WaitForRespawn(g, e));
    }

    // Respawns an entity, but only if the game phase is still figth.
    private IEnumerator WaitForRespawn(GameObject g, Entity e) {
        yield return new WaitForSeconds(respawnDuration);

        if (gamePhase == 1) {
            Spawn(g);
            e.Respawn();
        }
    }

}