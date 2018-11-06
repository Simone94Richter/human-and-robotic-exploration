using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class defines the common elements for every decision making component in each robot agent
/// </summary>
public abstract class RobotDecisionMaking : MonoBehaviour {

    protected Vector3 target;

    public abstract Vector3 PosToReach(List<Vector3> listFrontierPoints);

}
