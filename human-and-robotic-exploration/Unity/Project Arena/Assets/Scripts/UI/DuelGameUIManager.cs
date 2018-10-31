using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// DuelGameUIManager is an implementation of GameUIManager. The figth UI shows the kill count of
/// each player and the residual time.
/// </summary>
public class DuelGameUIManager : GameUIManager {

    // Elements of the ready UI.
    [Header("Ready UI")] [SerializeField] private GameObject player;
    [SerializeField] private GameObject opponent;
    [SerializeField] private GameObject countdown;

    // Elements of the fight UI.
    [Header("Fight UI")] [SerializeField] private GameObject time;
    [SerializeField] private GameObject playerKills;
    [SerializeField] private GameObject opponentKills;
    [SerializeField] private GameObject separator;

    // Elements of the score UI.
    [Header("Score UI")] [SerializeField] private GameObject playerWins;
    [SerializeField] private GameObject opponentWins;
    [SerializeField] private GameObject tie;

    private string playerName;
    private string opponentName;

    // Text fields of the ready UI.
    private Text countdownText;

    // Text fields of the fight UI.
    private Text timeText;
    private Text playerKillsText;
    private Text opponentKillsText;

    void Start() {
        timeText = time.GetComponent<Text>();
        playerKillsText = playerKills.GetComponent<Text>();
        opponentKillsText = opponentKills.GetComponent<Text>();

        countdownText = countdown.GetComponent<Text>();

        SetReady(true);
    }

    // Sets the player names.
    public void SetPlayersName(string p, string o) {
        playerName = p;
        opponentName = o;
    }

    // Sets the countdown.
    public void SetCountdown(int i) {
        if (i > 0) {
            countdownText.text = i.ToString();
        } else {
            countdownText.text = "Fight!";
        }
    }

    // Sets the ready UI.
    public void SetReadyUI() {
        player.GetComponent<Text>().text = playerName;
        opponent.GetComponent<Text>().text = opponentName;
    }

    // Sets the score UI.
    public void SetScoreUI(int s1, int s2) {
        if (s1 > s2) {
            playerWins.SetActive(true);
            opponentWins.SetActive(false);
            tie.SetActive(false);

            playerWins.GetComponent<Text>().text = playerName + " wins!";
        } else if (s1 < s2) {
            playerWins.SetActive(false);
            opponentWins.SetActive(true);
            tie.SetActive(false);

            opponentWins.GetComponent<Text>().text = opponentName + " wins!";
        } else {
            playerWins.SetActive(false);
            opponentWins.SetActive(false);
            tie.SetActive(true);
        }
    }

    // Sets the remaining time.
    public void SetTime(int time) {
        string minutes = TimeToString(time / 60);
        string seconds = TimeToString(time % 60);

        timeText.text = minutes + ":" + seconds;
    }

    // Sets the kills of the first player.
    public void SetKills(int pk, int ok) {
        playerKillsText.text = pk.ToString();
        opponentKillsText.text = ok.ToString();
    }

    // Sets the color of all the UI elements.
    public override void SetColorAll(Color c) {
        timeText.color = c;
        playerKillsText.color = c;
        opponentKillsText.color = c;
        separator.GetComponent<Text>().color = c;
    }

}