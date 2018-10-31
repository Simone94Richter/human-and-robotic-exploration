using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ExperimentingTest2GameManager is a class dedicated entirely to experiments where the agent has to find multiple targets in one instance
/// </summary>
public class ExperimentingTest2GameManager : GameManager
{
    // --- Public Variables --- //

    [Header("Contenders")]
    [SerializeField] private GameObject player;
    [SerializeField] private int totalHealthPlayer = 100;
    [SerializeField] private bool[] activeGunsPlayer;
    [Header("The set of targets to be found by the agent")]
    [SerializeField] private GameObject[] target;
    [Header("The text GO where is diplayed the current status of found/not found targets (for Human agent only)")]
    [SerializeField] private Text ScoreOnScreen;

    [Header("Tutorial variables")] [SerializeField] protected TutorialGameUIManager tutorialGameUIManagerScript;
    [Header("Part of the UI used to display time")] public Text finalTime;
    [Header("Part of the UI used to give loading feedback to the player")] public Text loadingText;

    [Header("Distance used to say that the agent is close to the target")]
    public float goalDistance = 5f;
    [Header("GO containing the camera od the scene (used as head of the agent)")]
    public GameObject headPlayer;

    [Header("The level to be loaded after finishing this")]
    public string nextLevel;

    //  --- Private Variables --- //

    private bool tutorialCompleted = false; //is the level completed?
    private bool update = false; //the ending condition has to be updated?

    private float completionTime; //The time used by the agent to fulfill the task

    private List<bool> targetReached = new List<bool>(); //List saying if a certain target has been reached or not
    private List<GameObject> newTarget = new List<GameObject>(); //List used to instance the target
    private List<Vector3> targetPos = new List<Vector3>(); //List containing the updated position of the various targets

    private RaycastHit hit; //hit point of the raycast of the agent

    private Player playerScript;
    private Robot robotScript;

    void Start()
    {
        /* #if UNITY_EDITOR
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        #endif */

        playerScript = player.GetComponent<Player>();
        robotScript = player.GetComponent<Robot>();

        for (int i = 0; i < target.Length; i++)
        {
            targetReached.Add(false);
        }

        tutorialGameUIManagerScript.Fade(0.7f, 1f, true, 0.5f);
    }

    void Update()
    {
        if (!IsReady() && mapManagerScript.IsReady() && spawnPointManagerScript.IsReady()
            && tutorialGameUIManagerScript.IsReady())
        {
            // Generate the map.
            mapManagerScript.ManageMap(true);

            if (!generateOnly)
            {
                // Set the spawn points.
                spawnPointManagerScript.SetSpawnPoints(mapManagerScript.GetSpawnPoints());
                spawnPointManagerScript.SetTargetPoint(mapManagerScript.GetTargetPoint());

                // Spawn the player.
                Spawn(player);

                // Setup the contenders.
                if (playerScript)
                {
                    playerScript.SetupEntity(totalHealthPlayer, activeGunsPlayer, this, 1);
                    playerScript.LockCursor();
                }
                //Robot agent case
                if (robotScript && !IsReady())
                {
                    robotScript.StartingCountDown();
                }

                startTime = Time.time;
            }

            SetReady(true);
        }
        else if (IsReady() && !generateOnly)
        {
            //Robot agent case
            if (robotScript)
            {
                //tutorialCompleted = robotScript.TargetReached();
                Vector3 localDownVector = Quaternion.AngleAxis(headPlayer.transform.eulerAngles.x, player.transform.right) * player.transform.forward;
                //Vector3 downDirection = headPlayer.transform.TransformDirection(localDownVector);
                Ray center = new Ray(headPlayer.transform.position, localDownVector);
                //Debug.DrawRay(headPlayer.transform.position, player.transform.forward * 20f, Color.green, 0.5f);
                Debug.DrawRay(headPlayer.transform.position, localDownVector * 20f, Color.green, 0.5f);
                if (Physics.Raycast(center, out hit, 20f))
                {
                    for (int i = 0; i < targetPos.Count; i++)
                    {
                        //Debug.Log(hit.transform.gameObject.name);
                        //Debug.Log(Mathf.Sqrt((player.transform.position.x - targetPos[i].x) * (player.transform.position.x - targetPos[i].x) + (player.transform.position.z - targetPos[i].z) * (player.transform.position.z - targetPos[i].z)));
                        if (hit.transform.gameObject.tag == "Target" && Mathf.Sqrt((player.transform.position.x - targetPos[i].x) * (player.transform.position.x - targetPos[i].x) + (player.transform.position.z - targetPos[i].z) * (player.transform.position.z - targetPos[i].z)) <= goalDistance)
                        {
                            targetReached[i] = true;
                            newTarget[i].SetActive(false);
                            update = true;
                            //Debug.Log(true);
                        }
                    }
                }

                bool finished = true;

                for (int i = 0; i < targetReached.Count; i++)
                {
                    if (!targetReached[i])
                    {
                        finished = false;
                    }
                }
                Debug.Log(targetReached[0] + "," + targetReached[1] + ", " + targetReached[2] + ", " + targetReached[3]);

                if (finished)
                {
                    tutorialCompleted = true;
                }
                else if(update)
                {
                    tutorialCompleted = false;
                    robotScript.ResetTargetFound();
                    update = false;
                }
            }
            else if (playerScript && !tutorialCompleted)
            {
                Vector3 localDownVector = Quaternion.AngleAxis(headPlayer.transform.eulerAngles.x, player.transform.right) * player.transform.forward;
                //Vector3 downDirection = headPlayer.transform.TransformDirection(localDownVector);
                Ray center = new Ray(headPlayer.transform.position, localDownVector);
                //Debug.DrawRay(headPlayer.transform.position, player.transform.forward * 20f, Color.green, 0.5f);
                Debug.DrawRay(headPlayer.transform.position, localDownVector * 20f, Color.red, 0.5f);
                if (Physics.Raycast(center, out hit, 20f))
                {
                    for (int i = 0; i < targetPos.Count; i++)
                    {
                        //Debug.Log(hit.transform.gameObject.name);
                        //Debug.Log(Mathf.Sqrt((player.transform.position.x - targetPos[i].x) * (player.transform.position.x - targetPos[i].x) + (player.transform.position.z - targetPos[i].z) * (player.transform.position.z - targetPos[i].z)));
                        if (hit.transform.gameObject.tag == "Target" && Mathf.Sqrt((player.transform.position.x - targetPos[i].x) * (player.transform.position.x - targetPos[i].x) + (player.transform.position.z - targetPos[i].z) * (player.transform.position.z - targetPos[i].z)) <= goalDistance)
                        {
                            targetReached[i] = true;
                            newTarget[i].SetActive(false);
                            //Debug.Log(true);
                        }
                    }

                    bool finished = true;
                    int targetsFound = 0;

                    for (int i = 0; i < targetReached.Count; i++)
                    {
                        if (!targetReached[i])
                        {
                            finished = false;
                        }
                        else targetsFound++;
                    }

                    ScoreOnScreen.text = targetsFound + "/4";

                    if (finished)
                    {
                        tutorialCompleted = true;
                    }
                    else tutorialCompleted = false;
                }
            }
            //Debug.Log(gamePhase);
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
                tutorialGameUIManagerScript.SetCountdown((int)(startTime + readyDuration - Time.time));
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
            if (playerScript != null)
            {
                playerScript.SetInGame(false);
            }
            tutorialGameUIManagerScript.ActivateReadyUI();
            gamePhase = 0;
        }
        else if (gamePhase == 0 && passedTime >= readyDuration)
        {
            // Enable the player movement and interactions, activate the fight UI and spawn the 
            // target.
            tutorialGameUIManagerScript.Fade(0.7f, 0f, false, 0.25f);
            spawnPointManagerScript.UpdateLastUsed();
            StartCoroutine(SpawnTarget());
            if (playerScript != null)
            {
                playerScript.SetInGame(true);
                playerScript.StartRecordingData();
            }
            tutorialGameUIManagerScript.ActivateFightUI();
            gamePhase = 1;
        }
        else if (gamePhase == 1 && tutorialCompleted)
        {
            // Disable the player movement and interactions, activate the score UI, set the winner 
            // and set the phase.
            if (robotScript)
            {
                player.GetComponent<RobotMovement>().inGameSession = false;
            }
            if (playerScript != null)
            {
                playerScript.SetGameEnd();
                playerScript.SetInGame(false);
            }
            tutorialGameUIManagerScript.Fade(0.7f, 0, true, 0.5f);

            StartCoroutine(DisplayTime());

            completionTime = Time.time;
            gamePhase = 2;
        }
        else if (gamePhase == 2 && Time.time >= completionTime + scoreDuration && (!player.GetComponent<RobotConnection>() || (player.GetComponent<RobotConnection>() && player.GetComponent<RobotConnection>().uploadComplete)))
        {
            if (loadingText)
            {
                loadingText.text = "Loading Survey...";
            }
            Quit();
            gamePhase = 3;
        }
    }

    /// <summary>
    /// This method is called to display the score time one second after the agent has completed the task
    /// </summary>
    /// <returns></returns>
    private IEnumerator DisplayTime()
    {
        yield return new WaitForSeconds(1f);
        if (playerScript != null && finalTime)
        {
            finalTime.text = finalTime.text + player.GetComponent<Player>().GetFinalTime();
        }

        tutorialGameUIManagerScript.ActivateScoreUI();
    }

    // Spawns a target.
    private IEnumerator SpawnTarget()
    {
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < target.Length; i++)
        {
            newTarget.Add((GameObject)Instantiate(target[i]));
            newTarget[i].name = target[i].name;
            //newTarget.transform.position = spawnPointManagerScript.GetSpawnPosition();
            newTarget[i].transform.position = new Vector3(spawnPointManagerScript.GetTargetPosition(i).x, 0.3f, spawnPointManagerScript.GetTargetPosition(i).z);
            newTarget[i].GetComponent<Entity>().SetupEntity(0, null, this, 0);
            targetPos.Add(newTarget[i].transform.position);
        }
        if (player.GetComponent<RobotDMUtilityBased>())
        {
            player.GetComponent<RobotDMUtilityBased>().SetTargetPosition(newTarget[0].transform.position);//da riguardare
        }
    }

    // Pauses and unpauses the game.
    public override void Pause()
    {
        if (!isPaused)
        {
            tutorialGameUIManagerScript.Fade(0f, 0.7f, false, 0.25f);
            player.GetComponent<PlayerUIManager>().SetPlayerUIVisible(false);
            playerScript.ShowGun(false);
            tutorialGameUIManagerScript.ActivatePauseUI(true);
            playerScript.EnableInput(false);
            playerScript.UpdateStartingTime(Time.time);
        }
        else
        {
            tutorialGameUIManagerScript.Fade(0f, 0.7f, true, 0.25f);
            player.GetComponent<PlayerUIManager>().SetPlayerUIVisible(true);
            playerScript.ShowGun(true);
            tutorialGameUIManagerScript.ActivatePauseUI(false);
            playerScript.EnableInput(true);
            playerScript.StartTime();
        }

        isPaused = !isPaused;

        StartCoroutine(FreezeTime(0.25f, isPaused));
    }

    public override void SetUIColor(Color c)
    {
        tutorialGameUIManagerScript.SetColorAll(c);
    }

    public override void AddScore(int i, int j)
    {
        tutorialCompleted = true;
    }

    public override void MenageEntityDeath(GameObject g, Entity e)
    {
    }

}