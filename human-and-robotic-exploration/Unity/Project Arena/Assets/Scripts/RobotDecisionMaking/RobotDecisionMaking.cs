using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RobotDecisionMaking : MonoBehaviour {

    protected Vector3 target;

    public abstract Vector3 PosToReach(List<Vector3> listFrontierPoints);

}
