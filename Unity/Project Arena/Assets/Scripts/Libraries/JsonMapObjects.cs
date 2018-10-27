using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// This class defines the JSON containing information about the map explored by robots
/// </summary>
[Serializable]
public class JsonMapObjects
{

    public List<string> u;
    public List<string> r;
    public List<string> g;
    public List<string> w;
    public List<string> mapName;

}