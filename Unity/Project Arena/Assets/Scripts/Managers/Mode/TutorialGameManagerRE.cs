using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// TutorialGameManager is an implementation of GameManager. The tutorial game mode consists in 
/// finding and destroying a single target.
/// </summary>
public class TutorialGameManagerRE : GameManager
{

    [Header("Contenders")]
    [SerializeField]
    private GameObject player;
    [SerializeField] private int totalHealthPlayer = 100;
    [SerializeField] private bool[] activeGunsPlayer;
    [SerializeField] private GameObject target;

    [Header("In Game UI")] [SerializeField] protected TutorialGameUIManager tutorialGameUIManagerScript;
    [Header("Part of the UI used to display time")] public Text finalTime;
    [Header("Part of the UI used to give loading feedback to the player")] public Text loadingText;

    public float goalDistance = 5f;
    public GameObject headPlayer;

    [Header("The level to be loaded after finishing this")]
    public string nextLevel;

    private RaycastHit hit;

    private Player playerScript;
    private Robot robotScript;
    private bool tutorialCompleted = false;
    private float completionTime;

    private Vector3 targetPos;

    void Start()
    {
        /* #if UNITY_EDITOR
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        #endif */

        playerScript = player.GetComponent<Player>();
        robotScript = player.GetComponent<Robot>();

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
                if (robotScript && !IsReady())
                {
                    robotScript.SetIsMultiTarget(false);
                    robotScript.StartingCountDown();
                }

                startTime = Time.time;
            }

            SetReady(true);
        }
        else if (IsReady() && !generateOnly)
        {
            if (robotScript)
            {
                tutorialCompleted = robotScript.TargetReached();
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
                    //Debug.Log(hit.transform.gameObject.name);
                    //Debug.Log(Mathf.Sqrt((player.transform.position.x - targetPos.x) * (player.transform.position.x - targetPos.x) + (player.transform.position.z - targetPos.z) * (player.transform.position.z - targetPos.z)));
                    if (hit.transform.gameObject.tag == "Target" && Mathf.Sqrt((player.transform.position.x - targetPos.x) * (player.transform.position.x - targetPos.x) + (player.transform.position.z - targetPos.z) * (player.transform.position.z - targetPos.z)) <= goalDistance)
                    {
                        tutorialCompleted = true;
                    }
                    else tutorialCompleted = false;
                }
                else
                {
                    tutorialCompleted = false;
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
            if (playerScript)
            {
                playerScript.StartTime();
                playerScript.StartRecordingData();
            }
            tutorialGameUIManagerScript.Fade(0.7f, 0f, false, 0.25f);
            spawnPointManagerScript.UpdateLastUsed();
            StartCoroutine(SpawnTarget());
            if (playerScript != null)
            {
                playerScript.SetInGame(true);
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
                loadingText.text = "Loading next stage...";
            }
            Quit();
            gamePhase = 3;
        }
    }

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

        GameObject newTarget = (GameObject)Instantiate(target);
        newTarget.name = target.name;
        //newTarget.transform.position = spawnPointManagerScript.GetSpawnPosition();
        newTarget.transform.position = new Vector3(spawnPointManagerScript.GetTargetPosition().x, 0.3f, spawnPointManagerScript.GetTargetPosition().z);
        newTarget.GetComponent<Entity>().SetupEntity(0, null, this, 0);
        targetPos = newTarget.transform.position;
        if (player.GetComponent<RobotDMUtilityBased>())
        {
            player.GetComponent<RobotDMUtilityBased>().SetTargetPosition(newTarget.transform.position);
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

    /*private void LoadNextLeve()
    {
        SceneManager.LoadScene(nextLevel);
    }*/

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