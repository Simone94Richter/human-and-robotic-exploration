using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible to manage the destination point the robot agent want to reach and when to destroy it
/// </summary>
public class RobotSpoofedDest : MonoBehaviour {

    public GameObject robot;

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")//if the GO is the robot agent, do the following
        {
            robot.GetComponent<Robot>().SetTempReached();
            //Debug.Log("An agent has reached my position");
        }
    }
}
