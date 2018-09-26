using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotPlanningAstar : RobotPlanning {

    private float[,] fScore;//cost of the path given by gScore and heuristic
    private float[,] gScore;//cost of the path from the start tile

    private float tentativeGScore;

    private int layerMask = 1 << 0;   

    private List<Vector3> closedSet;//the set of tiles already visited
    private List<Vector3> openSet;//the set of tiles that can be visited

    private Vector3[,] cameFrom;//array of parents of tiles

    private Vector3 current;//current postion checked
    private Vector3 destination;//goal position
    private Vector3 robot;//robot position

    // Use this for initialization
    void Start () {
        //layerMask = ~layerMask;
    }

    public override List<Vector3> CheckVisibility(Vector3 robot, Vector3 destination)
    {
        Debug.DrawLine(robot, destination, Color.green, 4f);
        float distance = Mathf.Sqrt((robot.x - destination.x)*(robot.x - destination.x) + (robot.z - destination.z) * (robot.z - destination.z));
        if (Physics.Linecast(robot, destination, layerMask) || distance > range)
        {
            //Debug.Log("Something hidden or too far");
            if (!isNumeric)
            {
                return AstarCharMap(destination);
            }else return AstarNumMap(destination);
        }

        return null;
    }

    private List<Vector3> AstarCharMap(Vector3 destPos)
    {
        closedSet = new List<Vector3>();
        openSet = new List<Vector3>();
        cameFrom = new Vector3[robot_map.GetLength(0), robot_map.GetLength(1)];
        gScore = new float[robot_map.GetLength(0), robot_map.GetLength(1)];
        fScore = new float[robot_map.GetLength(0), robot_map.GetLength(1)];

        openSet.Add(transform.position);//adding the starting tile to the list of tiles to visit

        //initialization of the gScore and fScore by setting both of them (for every tile) to Infinite
        for (int i = 0; i < gScore.GetLength(0); i++)
        {
            for (int j = 0; j < gScore.GetLength(1); j++)
            {
                gScore[i, j] = Mathf.Infinity;
                fScore[i, j] = Mathf.Infinity;
            }
        }

        //assigning precise values to gScore and fScore of the starting position
        gScore[(int)FixingRound(transform.position.x/squareSize),(int)FixingRound(transform.position.z/squareSize)] = 0;
        fScore[(int)FixingRound(transform.position.x / squareSize), (int)FixingRound(transform.position.z / squareSize)] = Mathf.Sqrt((transform.position.x - destPos.x) * (transform.position.x - destPos.x) + (transform.position.z - destPos.z) * (transform.position.z - destPos.z));


        while (openSet.Count > 0) //until the list of tiles to visit is not empty
        {
            current = openSet[0];
            for (int i = 0; i < openSet.Count; i++)
            {

                float fS = fScore[(int)FixingRound(openSet[i].x / squareSize), (int)FixingRound(openSet[i].z / squareSize)];
                float curFScore = fScore[(int)FixingRound(current.x / squareSize), (int)FixingRound(current.z / squareSize)];

                if (i != 0 && fS < curFScore)
                {
                    current = openSet[i];
                }

            }

            int curr_x = (int)FixingRound(current.x / squareSize);
            int curr_z = (int)FixingRound(current.z / squareSize);
            int dest_x = (int)FixingRound(destPos.x / squareSize);
            int dest_z = (int)FixingRound(destPos.z / squareSize);

            openSet.Remove(current);
            closedSet.Add(current);

            if (curr_x == dest_x && curr_z == dest_z)//if the current position is the goal one, we can exit from the procedure
            {
                return ReconstructPath(cameFrom, current, destPos);
            }

            for (int i = -1; i<2; i++) //checking neighbours on left and right
            {
                if (closedSet.Contains(new Vector3((curr_x + i)*squareSize, transform.position.y, (curr_z)*squareSize)))
                {
                    continue; //this neighbour has already been evaluated
                }
                else if( ( (curr_x + i) >= 0 && (curr_x + i) < gScore.GetLength(0) ) && ( (curr_z) >= 0 && (curr_z) < gScore.GetLength(1) ) && (robot_map[curr_x+i,curr_z] != 'u'))
                {
                    float neigh_x = (curr_x + i) * squareSize;
                    float neigh_z = (curr_z) * squareSize;
                    //the distance from start to neighbour
                    if (robot_map[curr_x+i, curr_z] == 'w')
                    {
                        tentativeGScore = gScore[curr_x + i, curr_z] + Mathf.Sqrt((current.x - neigh_x) * (current.x - neigh_x) + (current.z - neigh_z) * (current.z - neigh_z)) + penaltyCost;
                    }
                    else
                        tentativeGScore = gScore[curr_x+i,curr_z] + Mathf.Sqrt((current.x - neigh_x) * (current.x - neigh_x) + (current.z - neigh_z) * (current.z - neigh_z));

                    if (!openSet.Contains(new Vector3((curr_x + i) * squareSize, transform.position.y, (curr_z) * squareSize)))
                    {
                        openSet.Add(new Vector3((curr_x + i) * squareSize, transform.position.y, (curr_z) * squareSize)); //new tile to investigate
                    }
                    else if (tentativeGScore >= gScore[curr_x +i, curr_z])
                    {
                        continue; //not the better path
                    }

                    //we have found a best path
                    cameFrom[curr_x+i, curr_z] = current;
                    gScore[curr_x + i, curr_z] = tentativeGScore;
                    fScore[curr_x + i, curr_z] = gScore[curr_x + i, curr_z] + GetHeuristic(current, destPos);
                }
                
            }

            for (int j = -1; j < 2; j++) //checking neighbours up and down
            {
                if (closedSet.Contains(new Vector3((curr_x) * squareSize, transform.position.y, (curr_z + j) * squareSize)))
                {
                    continue; //this neighbour has already been evaluated
                }
                else if (((curr_x) >= 0 && (curr_x) < gScore.GetLength(0)) && ((curr_z + j) >= 0 && (curr_z + j) < gScore.GetLength(1)) && (robot_map[curr_x, curr_z + j] != 'u'))
                {
                    float neigh_x = (curr_x) * squareSize;
                    float neigh_z = (curr_z + j) * squareSize;
                    //the distance from start to neighbour
                    if (robot_map[curr_x, curr_z+j] == 'w')
                    {
                        tentativeGScore = gScore[curr_x, curr_z + j] + Mathf.Sqrt((current.x - neigh_x) * (current.x - neigh_x) + (current.z - neigh_z) * (current.z - neigh_z)) + penaltyCost;
                    }
                    else
                        tentativeGScore = gScore[curr_x, curr_z + j] + Mathf.Sqrt((current.x - neigh_x) * (current.x - neigh_x) + (current.z - neigh_z) * (current.z - neigh_z));

                    if (!openSet.Contains(new Vector3((curr_x) * squareSize, transform.position.y, (curr_z + j) * squareSize)))
                    {
                        openSet.Add(new Vector3((curr_x) * squareSize, transform.position.y, (curr_z + j) * squareSize)); //new tile to investigate
                    }
                    else if (tentativeGScore >= gScore[curr_x, curr_z + j])
                    {
                        continue; //not the better path
                    }

                    //we have found a best path
                    cameFrom[curr_x, curr_z + j] = current;
                    gScore[curr_x, curr_z + j] = tentativeGScore;
                    fScore[curr_x, curr_z + j] = gScore[curr_x, curr_z + j] + GetHeuristic(current, destPos);
                }
            }
        }

        return null;
    }

    private List<Vector3> AstarNumMap(Vector3 destPos) //this method works
    {
        closedSet = new List<Vector3>();
        openSet = new List<Vector3>();
        cameFrom = new Vector3[numeric_robot_map.GetLength(0), numeric_robot_map.GetLength(1)];
        gScore = new float[numeric_robot_map.GetLength(0), numeric_robot_map.GetLength(1)];
        fScore = new float[numeric_robot_map.GetLength(0), numeric_robot_map.GetLength(1)];

        openSet.Add(transform.position);

        for (int i = 0; i < gScore.GetLength(0); i++)
        {
            for (int j = 0; j < gScore.GetLength(1); j++)
            {
                gScore[i, j] = Mathf.Infinity;
                fScore[i, j] = Mathf.Infinity;
            }
        }

        gScore[(int)FixingRound(transform.position.x / squareSize), (int)FixingRound(transform.position.z / squareSize)] = 0;
        fScore[(int)FixingRound(transform.position.x / squareSize), (int)FixingRound(transform.position.z / squareSize)] = Mathf.Sqrt((transform.position.x - destPos.x) * (transform.position.x - destPos.x) + (transform.position.z - destPos.z) * (transform.position.z - destPos.z));

        current = openSet[0];

        while (openSet.Count > 0)
        {
            current = openSet[0];
            for (int i = 0; i < openSet.Count; i++)
            {

                float fS = fScore[(int)FixingRound(openSet[i].x / squareSize), (int)FixingRound(openSet[i].z / squareSize)];
                float curFScore = fScore[(int)FixingRound(current.x / squareSize), (int)FixingRound(current.z / squareSize)];

                if (i != 0 && fS < curFScore)
                {
                    current = openSet[i];
                }

            }

            int curr_x = (int)FixingRound(current.x / squareSize);
            int curr_z = (int)FixingRound(current.z / squareSize);
            int dest_x = (int)FixingRound(destPos.x / squareSize);
            int dest_z = (int)FixingRound(destPos.z / squareSize);

            openSet.Remove(current);
            closedSet.Add(current);

            if (curr_x == dest_x && curr_z == dest_z)//if the current position is the goal one, we can exit from the procedure
            {
                return ReconstructPath(cameFrom, current, destPos);
            }

            for (int i = -1; i < 2; i++)
            {
                if (closedSet.Contains(new Vector3((curr_x + i) * squareSize, transform.position.y, (curr_z) * squareSize)))
                {
                    continue;
                }
                else if (((curr_x + i) >= 0 && (curr_x + i) < gScore.GetLength(0)) && ((curr_z) >= 0 && (curr_z) < gScore.GetLength(1)) && (numeric_robot_map[curr_x + i, curr_z] != unknownCell))
                {
                    float neigh_x = (curr_x + i) * squareSize;
                    float neigh_z = (curr_z) * squareSize;

                    if (numeric_robot_map[curr_x + i, curr_z] == nearWallCell)
                    {
                        tentativeGScore = gScore[curr_x + i, curr_z] + Mathf.Sqrt((current.x - neigh_x) * (current.x - neigh_x) + (current.z - neigh_z) * (current.z - neigh_z)) + penaltyCost;
                    }
                    else
                        tentativeGScore = gScore[curr_x + i, curr_z] + Mathf.Sqrt((current.x - neigh_x) * (current.x - neigh_x) + (current.z - neigh_z) * (current.z - neigh_z));

                    if (!openSet.Contains(new Vector3((curr_x + i) * squareSize, transform.position.y, (curr_z) * squareSize)))
                    {
                        openSet.Add(new Vector3((curr_x + i) * squareSize, transform.position.y, (curr_z) * squareSize));
                    }
                    else if (tentativeGScore >= gScore[curr_x + i, curr_z])
                    {
                        continue;
                    }

                    cameFrom[curr_x + i, curr_z] = current;
                    gScore[curr_x + i, curr_z] = tentativeGScore;
                    fScore[curr_x + i, curr_z] = gScore[curr_x + i, curr_z] + GetHeuristic(current, destPos);
                }

            }

            for (int j = -1; j < 2; j++)
            {
                if (closedSet.Contains(new Vector3((curr_x) * squareSize, transform.position.y, (curr_z + j) * squareSize)))
                {
                    continue;
                }
                else if (((curr_x) >= 0 && (curr_x) < gScore.GetLength(0)) && ((curr_z + j) >= 0 && (curr_z + j) < gScore.GetLength(1)) && (numeric_robot_map[curr_x, curr_z + j] != unknownCell))
                {
                    float neigh_x = (curr_x) * squareSize;
                    float neigh_z = (curr_z + j) * squareSize;

                    if (numeric_robot_map[curr_x, curr_z + j] == nearWallCell)
                    {
                        tentativeGScore = gScore[curr_x, curr_z + j] + Mathf.Sqrt((current.x - neigh_x) * (current.x - neigh_x) + (current.z - neigh_z) * (current.z - neigh_z)) + penaltyCost;
                    }
                    else
                        tentativeGScore = gScore[curr_x, curr_z + j] + Mathf.Sqrt((current.x - neigh_x) * (current.x - neigh_x) + (current.z - neigh_z) * (current.z - neigh_z));

                    if (!openSet.Contains(new Vector3((curr_x) * squareSize, transform.position.y, (curr_z + j) * squareSize)))
                    {
                        openSet.Add(new Vector3((curr_x) * squareSize, transform.position.y, (curr_z + j) * squareSize));
                    }
                    else if (tentativeGScore >= gScore[curr_x, curr_z + j])
                    {
                        continue;
                    }

                    cameFrom[curr_x, curr_z + j] = current;
                    gScore[curr_x, curr_z + j] = tentativeGScore;
                    fScore[curr_x, curr_z + j] = gScore[curr_x, curr_z + j] + GetHeuristic(current, destPos);
                }
            }
        }

        return null;
    }

    private List<Vector3> ReconstructPath(Vector3[,] cameFom, Vector3 current, Vector3 destination)
    {
        List<Vector3> total_path = new List<Vector3>();
        //List<Vector3> optimal_path = new List<Vector3>();
        total_path.Add(current);
        //float distance;
        //int lastTwo = 2;
        int i_curr = 0;
        int j_curr = 0;

        for (int i=0; i<cameFom.GetLength(0); i++)
        {
            for (int j=0; j<cameFom.GetLength(1); j++)
            {
                if (i == (int)FixingRound(current.x / squareSize) && j == (int)FixingRound(current.z / squareSize))
                {
                    i_curr = i;
                    j_curr = j;
                    //isContained = true;
                }
            }
        }

        while (current != transform.position)
        {
            //isContained = false;
            current = cameFom[i_curr, j_curr];
            total_path.Insert(0, current);

            for (int i = 0; i < cameFom.GetLength(0); i++)
            {
                for (int j = 0; j < cameFom.GetLength(1); j++)
                {
                    if (i == (int)FixingRound(current.x / squareSize) && j == (int)FixingRound(current.z / squareSize) && cameFom[i, j] != Vector3.zero)
                    {
                        i_curr = i;
                        j_curr = j;
                        //isContained = true;
                    }
                }
            }

        }

        //only cells where the destination cannot be seen are contained
        /*for (int k = 0; k < total_path.Count; k++)
        {
            distance = Mathf.Sqrt((total_path[k].x - destination.x) * (total_path[k].x - destination.x) + (total_path[k].z - destination.z) * (total_path[k].z - destination.z));
            Debug.DrawLine(total_path[k], destination, Color.green, 4f);
            if (Physics.Linecast(total_path[k], destination, layerMask) || distance > range)
            {
                optimal_path.Add(total_path[k]);
            }
            else if(lastTwo > 0)   //is better to have at least two elements before directly approach to the final destination. With only one there is the risk that the robot is not able to reach it
            {
                optimal_path.Add(total_path[k]);
                lastTwo--;
            }
        }

        optimal_path.Add(destination);
        
        return optimal_path;*/
        return total_path;

    }

    private float GetHeuristic(Vector3 current, Vector3 destination)
    {
        int currentx = (int)FixingRound(current.x);
        int currentz = (int)FixingRound(current.z);
        int destinationx = (int)FixingRound(destination.x);
        int destinationz = (int)FixingRound(destination.z);
        return Mathf.Sqrt((currentx - destinationx)*(currentx - destinationx) + (currentz - destinationz)*(currentz - destinationz));
    }
}
