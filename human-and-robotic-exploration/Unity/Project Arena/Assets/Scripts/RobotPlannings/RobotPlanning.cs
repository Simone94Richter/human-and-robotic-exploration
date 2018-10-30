using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotPlanning : MonoBehaviour {

    [Header("Is the map used numeric or char?")]
    public bool isNumeric;
    [Header("The number used to simbolize undiscovered tile")]
    public float unknownCell;
    [Header("The number used to simbolize near-wall tile")]
    public float nearWallCell;

    [Header("The range of the robot")]
    public float range;
    [Header("Heauristic value")]
    public float heuristic;
    [Header("The size of the tile")]
    public float squareSize;
    [Header("How much should be the 'penalty' for entering in a tile near a wall")]
    public float penaltyCost;

    [Header("The map used")]
    public char[,] robot_map;
    public float[,] numeric_robot_map;

    protected Robot mainScipt;

    private void Start()
    {
        mainScipt = GetComponent<Robot>();
    }

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
