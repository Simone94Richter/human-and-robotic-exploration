using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotPlanningThetaStar : RobotPlanning {

    public Vector3 destination;

    private int layerMask = 1 << 0;

    private float[,] gScore;

    private Vector3[,] cameFrom;
    private List<Vector3> openNodes; 
    private float[,] openNodesValue;
    private List<Vector3> closedNodes;
    private List<float> closedNodesValues;

    private Vector3 s;

    public override List<Vector3> CheckVisibility(Vector3 robot, Vector3 destination)
    {
        this.destination = destination;
        Debug.DrawLine(robot, destination, Color.green, 4f);
        float distance = Mathf.Sqrt((robot.x - destination.x) * (robot.x - destination.x) + (robot.z - destination.z) * (robot.z - destination.z));
        if (Physics.Linecast(robot, destination, layerMask) || distance > range)
        {
            if (!isNumeric)
            {
                return ThetaStar(transform.position, destination);
            }
            else return ThetaStar(transform.position, destination);
        }

        return null;
    }

    private List<Vector3> ThetaStar(Vector3 start, Vector3 goal)//this method works
    {
        cameFrom = new Vector3[robot_map.GetLength(0), robot_map.GetLength(1)];
        openNodesValue = new float[robot_map.GetLength(0), robot_map.GetLength(1)];
        gScore = new float[robot_map.GetLength(0), robot_map.GetLength(1)];
 
        gScore[(int)FixingRound(start.x/squareSize), (int)FixingRound(start.z/squareSize)] = 0f;
        cameFrom[(int)FixingRound(start.x/squareSize), (int)FixingRound(start.z/squareSize)] = start;
        // Initializing open and closed sets. The open set is initialized 
        // with the start node and an initial cost
        openNodes = new List<Vector3>();
        openNodes.Add(start);
        openNodesValue[(int)FixingRound(start.x/squareSize), (int)FixingRound(start.z/squareSize)] = gScore[(int)FixingRound(start.x/squareSize), (int)FixingRound(start.z/squareSize)] + GetHeuristic(start, goal);
        // gScore(node) is the current shortest distance from the start node to node
        // heuristic(node) is the estimated distance of node from the goal node
        // there are many options for the heuristic such as Euclidean or Manhattan 
        closedNodes = new List<Vector3>();
        closedNodesValues = new List<float>();
        while(openNodes.Count > 0)
        {
            int count = openNodes.Count;
            s = openNodes[count-1];

            int sX = (int)FixingRound(s.x/squareSize);
            int sZ = (int)FixingRound(s.z/squareSize);
            int goalX = (int)FixingRound(goal.x/squareSize);
            int goalZ = (int)FixingRound(goal.z/squareSize);

            if (sX == goalX && sZ == goalZ)
            {
                return ReconstructPath(s);
            }
            else
            {
                openNodes.Remove(s);
                closedNodes.Add(s);

                for (int i = -1; i < 2; i++)
                {
                    if (closedNodes.Contains(new Vector3((sX + i) * squareSize, transform.position.y, (sZ) * squareSize)))
                    {
                        continue;
                    }
                    else if (((sX + i) >= 0 && (sX + i) < gScore.GetLength(0)) && ((sZ) >= 0 && (sZ) < gScore.GetLength(1)) && (robot_map[(int)FixingRound(sX+i), (int)FixingRound(sZ)] == 'r'))
                    {
                        float neigh_x = (sX + i) * squareSize;
                        float neigh_z = (sZ) * squareSize;

                        if (!openNodes.Contains(new Vector3(neigh_x, transform.position.y, neigh_z)))
                        {
                            cameFrom[(int)FixingRound(sX+i), (int)FixingRound(sZ)] = Vector3.zero;
                            gScore[(int)FixingRound(sX+i), (int)FixingRound(sZ)] = Mathf.Infinity;
                        }

                        UpdateVertex(s, new Vector3((sX+i)*squareSize, transform.position.y, sZ*squareSize));
                    }
                }


                for (int j = -1; j < 2; j++)
                {
                    if (closedNodes.Contains(new Vector3((sX) * squareSize, transform.position.y, (sZ + j) * squareSize)))
                    {
                        continue;
                    }
                    else if (((sX) >= 0 && (sX) < gScore.GetLength(0)) && ((sZ + j) >= 0 && (sZ + j) < gScore.GetLength(1)) && (robot_map[(int)FixingRound(sX), (int)FixingRound(sZ + j)] == 'r'))
                    {
                        float neigh_x = (sX) * squareSize;
                        float neigh_z = (sZ + j) * squareSize;

                        if (!openNodes.Contains(new Vector3(neigh_x, transform.position.y, neigh_z)))
                        {
                            cameFrom[(int)FixingRound(sX), (int)FixingRound(sZ+j)] = Vector3.zero;
                            gScore[(int)FixingRound(sX), (int)FixingRound(sZ+j)] = Mathf.Infinity;
                        }

                        UpdateVertex(s, new Vector3(sX*squareSize, transform.position.y, (sZ + j)*squareSize));
                    }
                }
            }
        }

        return null;
    }

    private void UpdateVertex(Vector3 s, Vector3 neighbour)
    {
        if (Physics.Linecast(s, neighbour, layerMask))
        {
            Vector3 parent = cameFrom[(int)FixingRound(s.x/squareSize), (int)FixingRound(s.z/squareSize)];

            int neighX = (int)FixingRound(neighbour.x/squareSize);
            int neighZ = (int)FixingRound(neighbour.z/squareSize);

            float euclideanDistance = Mathf.Sqrt((parent.x - neighbour.x)*(parent.x - neighbour.x) + (parent.z - neighbour.z)*(parent.z - neighbour.z));
            if (gScore[(int)FixingRound(parent.x/squareSize), (int)FixingRound(parent.z/squareSize)] + euclideanDistance < gScore[neighX, neighZ])
            {
                gScore[neighX, neighZ] = gScore[(int)FixingRound(parent.x/squareSize), (int)FixingRound(parent.z/squareSize)] + euclideanDistance;
                cameFrom[neighX, neighZ] = cameFrom[(int)FixingRound(s.x/squareSize), (int)FixingRound(s.z/squareSize)];

                if (openNodes.Contains(new Vector3(neighbour.x, transform.position.y, neighbour.z)))
                {
                    openNodes.Remove(new Vector3(neighbour.x, transform.position.y, neighbour.z));
                }

                openNodes.Add(neighbour);
                openNodesValue[neighX, neighZ] = gScore[neighX, neighZ] + GetHeuristic(neighbour, destination);
            }
        }
        else
        {
            float euclideanDistance = Mathf.Sqrt((s.x - neighbour.x) * (s.x - neighbour.x) + (s.z - neighbour.z) * (s.z - neighbour.z));
            if (gScore[(int)FixingRound(s.x/squareSize), (int)FixingRound(s.z/squareSize)] + euclideanDistance < gScore[(int)FixingRound(neighbour.x/squareSize), (int)FixingRound(neighbour.z/squareSize)])
            {
                gScore[(int)FixingRound(neighbour.x/squareSize), (int)FixingRound(neighbour.z/squareSize)] = gScore[(int)FixingRound(s.x/squareSize), (int)FixingRound(s.z/squareSize)] + euclideanDistance;
                cameFrom[(int)FixingRound(neighbour.x/squareSize), (int)FixingRound(neighbour.z/squareSize)] = s;

                if (openNodes.Contains(new Vector3(neighbour.x, transform.position.y, neighbour.z)))
                {
                    openNodes.Remove(new Vector3(neighbour.x, transform.position.y, neighbour.z));
                }

                openNodes.Add(neighbour);
                openNodesValue[(int)FixingRound(neighbour.x/squareSize), (int)FixingRound(neighbour.z/squareSize)] = gScore[(int)FixingRound(neighbour.x/squareSize), (int)FixingRound(neighbour.z/squareSize)] + GetHeuristic(neighbour, destination);
            }
        }
    }

    private List<Vector3> ReconstructPath(Vector3 node) //this method should work
    {
        List<Vector3> total_path = new List<Vector3>();
        total_path.Add(node);
        int i_curr = 0;
        int j_curr = 0;

        for (int i = 0; i < cameFrom.GetLength(0); i++)
        {
            for (int j = 0; j < cameFrom.GetLength(1); j++)
            {
                if (i == (int)FixingRound(node.x / squareSize) && j == (int)FixingRound(node.z / squareSize))
                {
                    i_curr = i;
                    j_curr = j;
                    //isContained = true;
                }
            }
        }

        while (node != transform.position)
        {
            //isContained = false;
            node = cameFrom[i_curr, j_curr];
            total_path.Insert(0, node);

            for (int i = 0; i < cameFrom.GetLength(0); i++)
            {
                for (int j = 0; j < cameFrom.GetLength(1); j++)
                {
                    if (i == (int)FixingRound(node.x / squareSize) && j == (int)FixingRound(node.z / squareSize) && cameFrom[i, j] != Vector3.zero)
                    {
                        i_curr = i;
                        j_curr = j;
                        //isContained = true;
                    }
                }
            }

        }

        return total_path;
    }

    private float GetHeuristic(Vector3 current, Vector3 destination)
    {
        int currentx = (int)FixingRound(current.x);
        int currentz = (int)FixingRound(current.z);
        int destinationx = (int)FixingRound(destination.x);
        int destinationz = (int)FixingRound(destination.z);
        return Mathf.Sqrt((currentx - destinationx) * (currentx - destinationx) + (currentz - destinationz) * (currentz - destinationz));
    }
}
