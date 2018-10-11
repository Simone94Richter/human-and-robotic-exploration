using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotSpoofedDestPath : MonoBehaviour {

    public GameObject robot;
    public int index;
    public List<Vector3> path;

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (index+1 == path.Count)
            {
                robot.GetComponent<Robot>().SetTempReached();
            }
            else robot.GetComponent<RobotMovement>().ApproachingPointToReach(path, index+1);
            //Debug.Log("An agent has reached my position (I'm part of a path)");
        }
    }
}
