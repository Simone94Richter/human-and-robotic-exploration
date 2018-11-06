using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class defines a decision making component with a random selection of the point to reach
/// </summary>
public class RobotDMRandom : RobotDecisionMaking {

    private int index;

    /// <summary>
    /// This method returns the point to be chosen
    /// </summary>
    /// <param name="listFrontierPoints">The list of possible poins to reach</param>
    /// <returns></returns>
    public override Vector3 PosToReach(List<Vector3> listFrontierPoints)
    {
        index = Random.Range(0, listFrontierPoints.Count);
        return listFrontierPoints[index];
    }
}
