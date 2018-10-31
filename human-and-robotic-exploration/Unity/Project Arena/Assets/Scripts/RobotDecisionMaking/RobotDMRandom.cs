using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotDMRandom : RobotDecisionMaking {

    private int index;

    public override Vector3 PosToReach(List<Vector3> listFrontierPoints)
    {
        index = Random.Range(0, listFrontierPoints.Count);
        return listFrontierPoints[index];
    }
}
