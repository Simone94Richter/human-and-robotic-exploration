using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRobot{

    IEnumerator SendingRays();

    void UpdatingSpace(List<Ray> ray);

    void ChoosingPointToReach();

    void ApproachingPointToReach();
}
