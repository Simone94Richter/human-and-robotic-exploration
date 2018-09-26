using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotDecisionMaking : MonoBehaviour {
	
	public virtual Vector3 PosToReach(List<Vector3> listFrontierPoints)
    {
        return Vector3.zero;
    }
}
