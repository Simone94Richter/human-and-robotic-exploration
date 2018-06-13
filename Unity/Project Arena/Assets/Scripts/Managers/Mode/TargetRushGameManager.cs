using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// TargetRushGameManager is an implementation of GameManager. The target rush game mode consists in 
/// destroying consecutive waves of targets. The games ends when the player dies, time runs up or 
/// the last wave is destoryed.
/// </summary>
public class TargetRushGameManager : GameManager {

    [Header("Contenders")]
    [SerializeField] private GameObject player;
    [SerializeField] private int totalHealthPlayer = 100;
    [SerializeField] private bool[] activeGunsPlayer;
    [SerializeField] private Wave[] waveList;

    [Header("Target hunt variables")] [SerializeField] protected TargetRushGameUIManager targetRushGameUIManagerScript;

    private Player playerScript;
    private int playerScore = 0;
    private int playerID = 0;

    private int currentWave = 1;
    private int targetsCount = 0;

    private void Start() {
        /* #if UNITY_EDITOR
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        #endif */

        playerScript = player.GetComponent<Player>();

        targetRushGameUIManagerScript.Fade(0.7f, 1f, true, 0.5f);
    }

    private void Update() {
        if (!IsReady() && mapManagerScript.IsReady() && spawnPointManagerScript.IsReady()
            && targetRushGameUIManagerScript.IsReady()) {
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
            targetRushGameUIManagerScript.ActivateReadyUI();
            gamePhase = 0;
        } else if (gamePhase == 0 && passedTime >= readyDuration) {
            // Enable the player movement and interactions, activate the fight UI, set the score to 
            // zero, the wave to 1 and set the phase.
            targetRushGameUIManagerScript.Fade(0.7f, 0f, false, 0.25f);
            targetRushGameUIManagerScript.SetScore(0);
            spawnPointManagerScript.UpdateLastUsed();
            StartCoroutine(SpawnWave());
            playerScript.SetInGame(true);
            targetRushGameUIManagerScript.ActivateFightUI();
            gamePhase = 1;
        } else if (gamePhase == 1 && passedTime >= readyDuration + gameDuration) {
            // Disable the player movement and interactions, activate the score UI, set the winner 
            // and set the phase.
            playerScript.SetInGame(false);
            targetRushGameUIManagerScript.Fade(0.7f, 0, true, 0.5f);
            targetRushGameUIManagerScript.SetFinalScore(playerScore);
            targetRushGameUIManagerScript.SetFinalWave(currentWave);
            targetRushGameUIManagerScript.SetVictory(false);
            targetRushGameUIManagerScript.ActivateScoreUI();
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
                targetRushGameUIManagerScript.SetCountdown((int)(startTime + readyDuration
                    - Time.time));
                break;
            case 1:
                // Update the time.
                targetRushGameUIManagerScript.SetTime((int)(startTime + readyDuration + gameDuration
                    - Time.time));
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
        targetRushGameUIManagerScript.SetColorAll(c);
    }

    // Called when a target is destroyed, adds score and time and changes wave if the target is the 
    // last one.
    public override void AddScore(int scoreIncrease, int timeIncrease) {
        IncreaseTime(timeIncrease);
        IncreaseScore(scoreIncrease);

        targetsCount--;
        targetRushGameUIManagerScript.SetTargets(targetsCount);

        if (targetsCount == 0) {
            EndWave();
        }
    }

    // Pauses and unpauses the game.
    public override void Pause() {
        if (!isPaused) {
            targetRushGameUIManagerScript.Fade(0f, 0.7f, false, 0.25f);
            player.GetComponent<PlayerUIManager>().SetPlayerUIVisible(false);
            playerScript.ShowGun(false);
            targetRushGameUIManagerScript.ActivatePauseUI(true);
            playerScript.EnableInput(false);
        } else {
            targetRushGameUIManagerScript.Fade(0f, 0.7f, true, 0.25f);
            player.GetComponent<PlayerUIManager>().SetPlayerUIVisible(true);
            playerScript.ShowGun(true);
            targetRushGameUIManagerScript.ActivatePauseUI(false);
            playerScript.EnableInput(true);
        }

        isPaused = !isPaused;

        StartCoroutine(FreezeTime(0.25f, isPaused));
    }

    // Ends a wave, gives extra points and time, starts a new wave or ends the game.
    private void EndWave() {
        IncreaseTime(5);
        IncreaseScore(currentWave * 100);

        if (currentWave == waveList.Length) {
            // Disable the player movement and interactions, activate the score UI, set the winner 
            // and set the phase.
            playerScript.SetInGame(false);
            targetRushGameUIManagerScript.Fade(0.7f, 0, true, 0.5f);
            targetRushGameUIManagerScript.SetFinalScore(playerScore + (int)(Time.time - startTime)
                * 10);
            targetRushGameUIManagerScript.SetFinalWave(currentWave);
            targetRushGameUIManagerScript.SetVictory(true);
            targetRushGameUIManagerScript.ActivateScoreUI();
            gamePhase = 2;
        } else {
            currentWave++;
            StartCoroutine(SpawnWave());
        }
    }

    // Spawns a wake of targets.
    private IEnumerator SpawnWave() {
        targetRushGameUIManagerScript.SetWave(currentWave);

        // Set the target count.
        foreach (Target target in waveList[currentWave - 1].targetList) {
            targetsCount += target.count;
        }
        targetRushGameUIManagerScript.SetTargets(targetsCount);
        // Debug.Log("Going to spawn " + targetsCount + " targets in wave " + currentWave + ".");

        yield return new WaitForSeconds(2f);

        // Spawn the targets.
        foreach (Target target in waveList[currentWave - 1].targetList) {
            // Debug.Log("Spawning " + target.count + " " + target.prefab.name + " in wave " 
            // + currentWave + ".");
            for (int i = 0; i < target.count; i++) {
                GameObject newTarget = (GameObject)Instantiate(target.prefab);
                newTarget.name = target.prefab.name;
                newTarget.transform.position = spawnPointManagerScript.GetSpawnPosition();
                newTarget.GetComponent<Entity>().SetupEntity(0, null, this, 0);
            }
        }

        // Debug.Log("Spawned " + targetsCount + " targets in wave " + currentWave + ".");
    }

    // Increases score.
    private void IncreaseScore(int i) {
        playerScore += i;
        targetRushGameUIManagerScript.SetScore(playerScore);
        targetRushGameUIManagerScript.AddScore(i);
    }

    // Increases time.
    private void IncreaseTime(int i) {
        startTime += i;
        targetRushGameUIManagerScript.SetTime((int)(startTime + readyDuration + gameDuration
            - Time.time));
        targetRushGameUIManagerScript.AddTime(i);
    }

    // Ends the game.
    public override void MenageEntityDeath(GameObject g, Entity e) {
        startTime = Time.time - readyDuration - gameDuration;
    }

    // Target object. 
    [Serializable]
    private struct Wave {
        // Target prefab. 
        public Target[] targetList;
    }

    [Serializable]
    // Target prefab. 
    private struct Target {
        // Prefab of the target.
        public GameObject prefab;
        // Number of prefabs to be spawn.
        public int count;
    }

}