using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// This class defines a JSON containing all the information about the trajectory done by an agent
/// </summary>
[Serializable]
public class JsonRobotObjects {

    public List<string> position;
    public List<float> rotationY;
    public List<float> time;
    public List<string> mapName;

}
