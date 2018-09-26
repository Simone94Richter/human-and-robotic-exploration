using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotDMLeastCost : RobotDecisionMaking {

    private float candidatePathCost;

    private float[] pathCost;

    private List<Vector3> candidatePath;

    private RobotPlanning rPL;

    private void Start()
    {
        candidatePath = new List<Vector3>();
        rPL = GetComponent<RobotPlanning>();
    }

    public override Vector3 PosToReach(List<Vector3> listFrontierPoints)
    {
        float betterPathCost = 0f;
        int betterOption = 0;
        for (int j = 0; j < listFrontierPoints.Count; j++)
        {
            pathCost[j] = CalculatingPathCost(listFrontierPoints[j]);
        }

        for (int j = 0; j < pathCost.Length; j++)
        {
            if (pathCost[j] < betterPathCost)
            {
                betterPathCost = pathCost[j];
                betterOption = j;
            }
        }
        return listFrontierPoints[betterOption];
    }

    private float CalculatingPathCost(Vector3 dest)
    {
        candidatePathCost = 0f;
        candidatePath = rPL.CheckVisibility(transform.position, dest);
        if (candidatePath == null)
        {
            candidatePathCost = Mathf.Sqrt((transform.position.x - dest.x)*(transform.position.x - dest.x) + (transform.position.z - dest.z)*(transform.position.z - dest.z));
        }
        else
        {
            for (int i = 0; i < candidatePath.Count-1; i++)
            {
                candidatePathCost += Mathf.Sqrt((candidatePath[i].x - candidatePath[i+1].x) * (candidatePath[i].x - candidatePath[i+1].x) + (candidatePath[i].z - candidatePath[i+1].z) * (candidatePath[i].z - candidatePath[i+1].z));
            }
        }

        return candidatePathCost;
    }
}
