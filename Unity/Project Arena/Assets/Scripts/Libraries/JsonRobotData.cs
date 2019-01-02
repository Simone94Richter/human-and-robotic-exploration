using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class JsonRobotData{

    public List<string> position;
    public List<float> rotationY;
    public float time;
    public string mapName;
    public float timeDecision;
    public float timeScan;
    public float penalty_cost;
    public string ip;
    public string os;
}
