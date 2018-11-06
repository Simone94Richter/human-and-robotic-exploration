using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class defines a decision making process where the point chosen is the one gving more "utility" to the robot agent. This value is
/// computed as the distance between the point and the target to be reached
/// </summary>
public class RobotDMUtilityBased : RobotDecisionMaking {

    //private float candidateUtilityPath;

    private List<float> pathUtility;
    //private List<Vector3> candidatePath;

    //private RobotPlanning rPL;

    // Use this for initialization
    void Start () {
        //candidatePath = new List<Vector3>();
        //rPL = GetComponent<RobotPlanning>();
    }

    /// <summary>
    /// This method returns the point to be chosen
    /// </summary>
    /// <param name="listFrontierPoints">The list of possible poins to reach</param>
    /// <returns></returns>
    public override Vector3 PosToReach(List<Vector3> listFrontierPoints)
    {
        float betterPathUtility;
        int betterOption = 0;
        pathUtility = new List<float>();
        for (int j = 0; j < listFrontierPoints.Count; j++)
        {
            pathUtility.Add(CalculatingUtilityPath(listFrontierPoints[j]));
        }

        betterPathUtility = pathUtility[0];
        for (int j = 0; j < pathUtility.Count; j++)
        {
            if (pathUtility[j] < betterPathUtility)
            {
                betterPathUtility = pathUtility[j];
                betterOption = j;
            }
        }
        return listFrontierPoints[betterOption];
    }

    /// <summary>
    /// This method returns the lest cost for the robot agent to reach a certain point 
    /// </summary>
    /// <param name="dest"></param>
    /// <returns></returns>
    private float CalculatingUtilityPath(Vector3 dest)
    {
        //candidateUtilityPath = 0f;
        /*candidatePath = rPL.CheckVisibility(dest, target);
        if (candidatePath == null)
        {
            candidateUtilityPath = Mathf.Sqrt((target.x - dest.x) * (target.x - dest.x) + (target.z - dest.z) * (target.z - dest.z));
        }
        else
        {
            for (int i = 0; i < candidatePath.Count - 1; i++)
            {
                candidateUtilityPath += Mathf.Sqrt((candidatePath[i].x - candidatePath[i + 1].x) * (candidatePath[i].x - candidatePath[i + 1].x) + (candidatePath[i].z - candidatePath[i + 1].z) * (candidatePath[i].z - candidatePath[i + 1].z));
            }
        }*/

        return Mathf.Sqrt((target.x - dest.x) * (target.x - dest.x) + (target.z - dest.z) * (target.z - dest.z));
    }

    public void SetTargetPosition(Vector3 pos)
    {
        target = pos;
    }
}
