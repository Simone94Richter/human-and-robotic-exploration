using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TargetHuntGameUIManager is an implementation of GameUIManager. The figth UI shows the score 
/// and the residual time.
/// </summary>
public class TargetHuntGameUIManager : GameUIManager {

    // Elements of the ready UI.
    [Header("Ready UI")] [SerializeField] private Text countdown;

    // Elements of the fight UI.
    [Header("Fight UI")] [SerializeField] private Text time;
    [SerializeField] private Text additiveTimeText;
    [SerializeField] private Text score;
    [SerializeField] private Text additiveScoreText;

    // Elements of the score UI.
    [Header("Score UI")] [SerializeField] private GameObject gameover;
    [SerializeField] private Text finalScore;

    private Queue<string> additiveTimeQueue = new Queue<string>();
    private Queue<string> additiveScoreQueue = new Queue<string>();
    private float addedTimeTime = 0f;
    private float addedScoreTime = 0f;
    private bool addedTimeDisplayed = false;
    private bool addedScoreDisplayed = false;
    private string additiveTime = "";
    private string additiveScore = "";
    private string timeValue = "";
    private string scoreValue = "";

    public void Start() {
        SetReady(true);
    }

    public void Update() {
        if (fightUI.activeSelf) {
            // Menage the time adder.
            if (addedTimeDisplayed) {
                if (Time.time > addedTimeTime + 0.5f) {
                    additiveTimeText.text = "";
                    addedTimeDisplayed = false;
                }
            } else if (additiveTimeQueue.Count > 0 && Time.time > addedTimeTime + 1f) {
                String additive = additiveTimeQueue.Dequeue();
                additiveTimeText.text =
                    GetSpacesString((additive.Length + time.text.Length) * 2 - 1) + additive;
                addedTimeDisplayed = true;
                addedTimeTime = Time.time;
            }
            // Menage the score adder.
            if (addedScoreDisplayed) {
                if (Time.time > addedScoreTime + 0.5f) {
                    additiveScoreText.text = "";
                    addedScoreDisplayed = false;
                }
            } else if (additiveScoreQueue.Count > 0 && Time.time > addedScoreTime + 1f) {
                String additive = additiveScoreQueue.Dequeue();
                additiveScoreText.text =
                    GetSpacesString((additive.Length + score.text.Length) * 2 - 1) + additive; ;
                addedScoreDisplayed = true;
                addedScoreTime = Time.time;
            }
        }
    }

    private string GetSpacesString(int n) {
        string spaces = "";

        for (int i = 0; i < n; i++) {
            spaces += " ";
        }

        return spaces;
    }

    // Sets the countdown.
    public void SetCountdown(int i) {
        if (i > 0) {
            countdown.text = i.ToString();
        } else {
            countdown.text = "Fight!";
        }
    }

    // Sets the remaining time.
    public void SetTime(int t) {
        timeValue = TimeToString(t / 60) + ":" + TimeToString(t % 60);
        time.text = timeValue + additiveTime;
    }

    public override void SetColorAll(Color c) {
        time.color = c;
        score.color = c;
        additiveScoreText.color = c;
        additiveTimeText.color = c;
    }

    public void SetScore(int s) {
        scoreValue = s.ToString();
        score.text = scoreValue + additiveScore;
    }

    public void SetFinalScore(int s) {
        finalScore.text = "Score: " + s;
    }

    public void SetVictory(bool b) {
        gameover.SetActive(true);
    }

    public void AddTime(int t) {
        additiveTimeQueue.Enqueue(" +" + t);
    }

    public void AddScore(int s) {
        additiveScoreQueue.Enqueue(" +" + s);
    }

}