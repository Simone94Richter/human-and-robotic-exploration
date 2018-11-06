using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible to manage the destination points of the path to reach the original desired point
/// </summary>
public class RobotSpoofedDestPath : MonoBehaviour {

    public GameObject robot;
    public int index;
    public List<Vector3> path;

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")//if the GO entering in the trigger area is the robot agent, do the following
        {
            if (index+1 == path.Count)//if it's the last point of the path
            {
                robot.GetComponent<Robot>().SetTempReached();
            }
            else robot.GetComponent<RobotMovement>().ApproachingPointToReach(path, index+1);//if it's not the last point of the path
            //Debug.Log("An agent has reached my position (I'm part of a path)");
        }
    }
}
