using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TargetRushGameUIManager is an implementation of GameUIManager. The figth UI shows the number of
/// the current wave, the number of alive targets, the score and the residual time.
/// </summary>
public class TargetRushGameUIManager : GameUIManager {

    // Elements of the ready UI.
    [Header("Ready UI")] [SerializeField] private Text countdown;

    // Elements of the fight UI.
    [Header("Fight UI")] [SerializeField] private Text time;
    [SerializeField] private Text additiveTimeText;
    [SerializeField] private Text score;
    [SerializeField] private Text additiveScoreText;
    [SerializeField] private Text wave;
    [SerializeField] private Text targets;

    // Elements of the score UI.
    [Header("Score UI")] [SerializeField] private GameObject gameover;
    [SerializeField] private GameObject victory;
    [SerializeField] private Text finalScore;
    [SerializeField] private Text finalWave;

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
        wave.color = c;
        targets.color = c;
        additiveScoreText.color = c;
        additiveTimeText.color = c;
    }

    public void SetScore(int s) {
        scoreValue = s.ToString();
        score.text = scoreValue + additiveScore;
    }

    public void SetTargets(int t) {
        targets.text = "Targets: " + t;
    }

    public void SetFinalScore(int s) {
        finalScore.text = "Score: " + s;
    }

    public void SetFinalWave(int w) {
        finalWave.text = "Wave: " + w;
    }

    public void SetWave(int w) {
        wave.text = "Wave: " + w;
    }

    public void SetVictory(bool b) {
        if (b) {
            victory.SetActive(true);
            gameover.SetActive(false);
        } else {
            victory.SetActive(false);
            gameover.SetActive(true);
        }
    }

    public void AddTime(int t) {
        additiveTimeQueue.Enqueue(" +" + t);
    }

    public void AddScore(int s) {
        additiveScoreQueue.Enqueue(" +" + s);
    }

}