using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckboxQuestion2 : MonoBehaviour {

    public GameObject question;
    private int numberOfAnswer;
    

    public void SetNumberOfAnswer(int number)
    {
        if(this.numberOfAnswer != number)
        {
            this.numberOfAnswer = number;
            Debug.Log("I chose " + this.numberOfAnswer + " for question " + question);
        }
    }
}
