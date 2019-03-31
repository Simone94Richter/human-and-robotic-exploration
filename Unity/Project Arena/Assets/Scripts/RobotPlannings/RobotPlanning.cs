using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class defines the main features of the robot agent's planning
/// </summary>
public class RobotPlanning : MonoBehaviour {

    [Header("Is the map used numeric or char?")]
    public bool isNumeric;
    [Header("The number used to simbolize undiscovered tile")]
    public float unknownCell;
    [Header("The number used to simbolize near-wall tile")]
    public float nearWallCell;
    [Header("The number used to simbolize free tile")]
    public float freeCell;

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

    protected RobotMain mainScipt; //The core component (the one responsible for scanning and decisioning)

    private void Start()
    {
        mainScipt = GetComponent<RobotMain>();
    }

    /// <summary>
    /// This method checks if the destination is in line of sight with the robot agent. If it is, the robot just set to movement steps to reach it
    /// Otherwise, the planning manager defines a path, made of destination points, that the agent has to follow in order to reach it
    /// </summary>
    /// <param name="robot">The robot agent's position</param>
    /// <param name="destination">The desired destination to reach</param>
    /// <returns></returns>
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
