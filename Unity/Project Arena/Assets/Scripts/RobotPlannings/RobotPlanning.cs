using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotPlanning : MonoBehaviour {

    [Header("Is the mapused numeric orchar?")]
    public bool isNumeric;
    [Header("Is the mapused numeric orchar?")]
    public float freeCell;

    [Header("The range of the robot")]
    public float range;
    [Header("Heauristic value")]
    public float heuristic;
    [Header("The size of the tile")]
    public float squareSize;


    [Header("The map used")]
    public char[,] robot_map;
    public float[,] numeric_robot_map;

    public virtual List<Vector3> CheckVisibility(Vector3 robot, Vector3 destination)
    {
        return null;
    }

    protected float FixingRound(float coordinate)
    {
        if (Mathf.Abs(coordinate - Mathf.Round(coordinate)) >= 0.5f)
        {
            return (Mathf.Round(coordinate) - 1);
        }
        else return Mathf.Round(coordinate);
    }
}
