using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
