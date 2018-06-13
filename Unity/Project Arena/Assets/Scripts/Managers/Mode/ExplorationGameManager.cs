using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplorationGameManager : GameManager {

    [Header("Contenders")]
    [SerializeField]
    private GameObject robot;
    [SerializeField]
    private int totalHealthPlayer = 100;
    [SerializeField]
    private GameObject target;

    private Player robotScript;
    private bool targetReached = false;
    private float completionTime;

    // Use this for initialization
    void Start () {
        /* #if UNITY_EDITOR
           UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
       #endif */

        robotScript = robot.GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsReady() && mapManagerScript.IsReady() && spawnPointManagerScript.IsReady())
        {
            // Generate the map.
            mapManagerScript.ManageMap(true);

            if (!generateOnly)
            {
                // Set the spawn points.
                spawnPointManagerScript.SetSpawnPoints(mapManagerScript.GetSpawnPoints());

                // Spawn the player.
                Spawn(robot);

                // Setup the contenders.
                //robotScript.SetupEntity(totalHealthPlayer, activeGunsPlayer, this, 1);

                robotScript.LockCursor();
                startTime = Time.time;
            }

            SetReady(true);
        }
        else if (IsReady() && !generateOnly)
        {
            ManageGame();
        }
    }

    protected override void ManageGame()
    {
        UpdateGamePhase();

        switch (gamePhase)
        {
            case 0:
                // Update the countdown.
                //tutorialGameUIManagerScript.SetCountdown((int)(startTime + readyDuration - Time.time));
                break;
            case 1:
                // Pause or unpause if needed.
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Pause();
                }
                break;
            case 2:
                // Do nothing.
                break;
        }
    }

    protected override void UpdateGamePhase()
    {
        int passedTime = (int)(Time.time - startTime);

        if (gamePhase == -1)
        {
            // Disable the player movement and interactions, activate the ready UI and set the 
            // phase.
            robotScript.SetInGame(false);
            //tutorialGameUIManagerScript.ActivateReadyUI();
            gamePhase = 0;
        }
        else if (gamePhase == 0 && passedTime >= readyDuration)
        {
            // Enable the player movement and interactions, activate the fight UI and spawn the 
            // target.
            //tutorialGameUIManagerScript.Fade(0.7f, 0f, false, 0.25f);
            spawnPointManagerScript.UpdateLastUsed();
            StartCoroutine(SpawnTarget());
            robotScript.SetInGame(true);
            //tutorialGameUIManagerScript.ActivateFightUI();
            gamePhase = 1;
        }
        else if (gamePhase == 1 && targetReached)
        {
            // Disable the player movement and interactions, activate the score UI, set the winner 
            // and set the phase.
            robotScript.SetInGame(false);
            //tutorialGameUIManagerScript.Fade(0.7f, 0, true, 0.5f);
            //tutorialGameUIManagerScript.ActivateScoreUI();
            completionTime = Time.time;
            gamePhase = 2;
        }
        else if (gamePhase == 2 && Time.time >= completionTime + scoreDuration)
        {
            Quit();
            gamePhase = 3;
        }
    }

    // Spawns a target.
    private IEnumerator SpawnTarget()
    {
        yield return new WaitForSeconds(2f);

        GameObject newTarget = (GameObject)Instantiate(target);
        newTarget.name = target.name;
        newTarget.transform.position = spawnPointManagerScript.GetSpawnPosition();
        newTarget.GetComponent<Entity>().SetupEntity(0, null, this, 0);
    }

    // Pauses and unpauses the game.
    public override void Pause()
    {
        if (!isPaused)
        {
            //tutorialGameUIManagerScript.Fade(0f, 0.7f, false, 0.25f);
            robot.GetComponent<PlayerUIManager>().SetPlayerUIVisible(false);
            robotScript.ShowGun(false);
            //tutorialGameUIManagerScript.ActivatePauseUI(true);
            robotScript.EnableInput(false);
        }
        else
        {
            //tutorialGameUIManagerScript.Fade(0f, 0.7f, true, 0.25f);
            robot.GetComponent<PlayerUIManager>().SetPlayerUIVisible(true);
            robotScript.ShowGun(true);
            //tutorialGameUIManagerScript.ActivatePauseUI(false);
            robotScript.EnableInput(true);
        }

        isPaused = !isPaused;

        StartCoroutine(FreezeTime(0.25f, isPaused));
    }

    public override void SetUIColor(Color c)
    {
        //tutorialGameUIManagerScript.SetColorAll(c);
    }

    public override void AddScore(int i, int j)
    {
        targetReached = true;
    }

    public override void MenageEntityDeath(GameObject g, Entity e)
    {
    }
}
