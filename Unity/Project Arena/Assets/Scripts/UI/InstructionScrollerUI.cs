using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstructionScrollerUI : MonoBehaviour {

    [Header("Button used to go to next instructions")]
    public GameObject nextButton;
    [Header("Button used to start the experiment")]
    public GameObject playButton;
    [Header("Button used to go to previous instructions")]
    public GameObject previousButton;

    [Header("The set of instructions to display to the user")]
    public string[] instructions;

    [Header("Text used to display instructions")]
    public Text instructionOnScreen;

    private int index;

    // Use this for initialization
    void Start () {
        playButton.SetActive(false);
        previousButton.SetActive(false);
        instructionOnScreen.text = instructions[0];
        index = 0;
	}

    public void GoToNextInstruction()
    {
        index++;
        if (index == instructions.Length - 1)
        {
            nextButton.SetActive(false);
            playButton.SetActive(true);
        }
        previousButton.SetActive(true);
        instructionOnScreen.text = instructions[index];
    }

    public void GoToPreviousInstruction()
    {
        index--;
        if (index == 0)
        {
            previousButton.SetActive(false);
        }
        nextButton.SetActive(true);
        playButton.SetActive(false);
        instructionOnScreen.text = instructions[index];
    }
}
