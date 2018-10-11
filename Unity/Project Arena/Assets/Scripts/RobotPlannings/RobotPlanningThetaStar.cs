using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotPlanningThetaStar : RobotPlanning {

    public Vector3 destination;

    private bool noPath;

    private int layerMask = 1 << 0;

    private float[,] fScore;
    private float[,] gScore;

    private float tentativeGScore;

    private Vector3[,] cameFrom;
    private List<Vector3> openNodes; 
    private List<Vector3> closedNodes;
    private List<Vector3> returnedPath;
    private List<float> closedNodesValues;

    private RaycastHit hit;

    private Vector3 current;

    public override List<Vector3> CheckVisibility(Vector3 robot, Vector3 destination)
    {
        noPath = false;
        returnedPath = new List<Vector3>();
        this.destination = destination;
        //Debug.DrawLine(robot, destination, Color.green, 4f);
        float distance = Mathf.Sqrt((robot.x - destination.x) * (robot.x - destination.x) + (robot.z - destination.z) * (robot.z - destination.z));
        if (Physics.Linecast(transform.position, destination, out hit, layerMask, QueryTriggerInteraction.Collide) || distance > range)
        {
            if (distance <= range)
            {
               //Debug.Log(hit.transform.gameObject);
               noPath = true;
            }
            if (!isNumeric)
            {
                returnedPath = ThetaStarCharMap(destination);
                //if (noPath && returnedPath == null)
                //{
                    //mainScipt.noPath = true;
                //}
                return returnedPath;
            }
            else return ThetaStarNumMap(destination);
        }

        return null;
    }

    /*private List<Vector3> ThetaStar(Vector3 start, Vector3 goal)//this method works
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
                //if neighbour is near a wall, rise the cost of transaction
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

        total_path = TrulyUpdateVertexPath(total_path);
        //Debug.Log(total_path.Count);
        return total_path;
    }

    private List<Vector3> TrulyUpdateVertexPath(List<Vector3> path)
    {
        //Debug.Log(path.Count);
        List<Vector3> optimal_path = new List<Vector3>();

        //RaycastHit hit;
        optimal_path.Insert(0, path[path.Count-1]);
        bool notContinue = false;
        int i = path.Count-1;
        while (i > 0)
        {
            int j = i;
            while (j>0 && !notContinue)
            {
                float distance = Mathf.Sqrt((path[i].x - path[j-1].x)*(path[i].x - path[j-1].x) + (path[i].z - path[j-1].z)*(path[i].z - path[j-1].z));
                //if (Physics.Linecast(path[i], path[j - 1], out hit, layerMask, QueryTriggerInteraction.UseGlobal) || Vector3.Distance(path[i], path[j - 1]) > range)
                Debug.DrawLine(path[i], path[j - 1], Color.green, 4f);
                if (Physics.Linecast(path[i], path[j - 1], layerMask) || distance > range)
                {
                    //if(Vector3.Distance(path[i], path[j - 1]) <= range)
                    //Debug.Log(hit.transform.gameObject);
                    optimal_path.Insert(0, path[j]);
                    notContinue = true;
                }
                j--;
            }
            notContinue = false;
            i = j;
        }

        //Debug.Log(path.Count);
        return optimal_path;
    }

    */

    private List<Vector3> ThetaStarCharMap(Vector3 destPos)
    {
        closedNodes = new List<Vector3>();
        openNodes = new List<Vector3>();
        cameFrom = new Vector3[robot_map.GetLength(0), robot_map.GetLength(1)];
        gScore = new float[robot_map.GetLength(0), robot_map.GetLength(1)];
        fScore = new float[robot_map.GetLength(0), robot_map.GetLength(1)];

        openNodes.Add(transform.position);//adding the starting tile to the list of tiles to visit

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
        gScore[(int)FixingRound(transform.position.x / squareSize), (int)FixingRound(transform.position.z / squareSize)] = 0;
        fScore[(int)FixingRound(transform.position.x / squareSize), (int)FixingRound(transform.position.z / squareSize)] = Mathf.Sqrt((transform.position.x - destPos.x) * (transform.position.x - destPos.x) + (transform.position.z - destPos.z) * (transform.position.z - destPos.z));


        while (openNodes.Count > 0) //until the list of tiles to visit is not empty
        {
            current = openNodes[0];
            for (int i = 0; i < openNodes.Count; i++)
            {

                float fS = fScore[(int)FixingRound(openNodes[i].x / squareSize), (int)FixingRound(openNodes[i].z / squareSize)];
                float curFScore = fScore[(int)FixingRound(current.x / squareSize), (int)FixingRound(current.z / squareSize)];

                if (i != 0 && fS < curFScore)
                {
                    current = openNodes[i];
                }

            }

            int curr_x = (int)FixingRound(current.x / squareSize);
            int curr_z = (int)FixingRound(current.z / squareSize);
            int dest_x = (int)FixingRound(destPos.x / squareSize);
            int dest_z = (int)FixingRound(destPos.z / squareSize);

            openNodes.Remove(current);
            closedNodes.Add(current);

            if (curr_x == dest_x && curr_z == dest_z)//if the current position is the goal one, we can exit from the procedure
            {
                return ReconstructPath(cameFrom, current, destPos);
            }

            for (int i = -1; i < 2; i++) //checking neighbours on left and right
            {
                if (closedNodes.Contains(new Vector3((curr_x + i) * squareSize, transform.position.y, (curr_z) * squareSize)))
                {
                    continue; //this neighbour has already been evaluated
                }
                else if (((curr_x + i) >= 0 && (curr_x + i) < gScore.GetLength(0)) && ((curr_z) >= 0 && (curr_z) < gScore.GetLength(1)) && (robot_map[curr_x + i, curr_z] != 'u'))
                {
                    float neigh_x = (curr_x + i) * squareSize;
                    float neigh_z = (curr_z) * squareSize;
                    //the distance from start to neighbour
                    tentativeGScore = gScore[curr_x + i, curr_z] + Mathf.Sqrt((current.x - neigh_x) * (current.x - neigh_x) + (current.z - neigh_z) * (current.z - neigh_z));

                    if (!openNodes.Contains(new Vector3((curr_x + i) * squareSize, transform.position.y, (curr_z) * squareSize)))
                    {
                        openNodes.Add(new Vector3((curr_x + i) * squareSize, transform.position.y, (curr_z) * squareSize)); //new tile to investigate
                    }
                    else if (tentativeGScore >= gScore[curr_x + i, curr_z])
                    {
                        continue; //not the better path
                    }

                    //we have found a best path
                    cameFrom[curr_x + i, curr_z] = current;
                    gScore[curr_x + i, curr_z] = tentativeGScore;
                    fScore[curr_x + i, curr_z] = gScore[curr_x + i, curr_z] + GetHeuristic(current, destPos);
                }

            }

            for (int j = -1; j < 2; j++) //checking neighbours up and down
            {
                if (closedNodes.Contains(new Vector3((curr_x) * squareSize, transform.position.y, (curr_z + j) * squareSize)))
                {
                    continue; //this neighbour has already been evaluated
                }
                else if (((curr_x) >= 0 && (curr_x) < gScore.GetLength(0)) && ((curr_z + j) >= 0 && (curr_z + j) < gScore.GetLength(1)) && (robot_map[curr_x, curr_z + j] != 'u'))
                {
                    float neigh_x = (curr_x) * squareSize;
                    float neigh_z = (curr_z + j) * squareSize;
                    //the distance from start to neighbour
                    tentativeGScore = gScore[curr_x, curr_z + j] + Mathf.Sqrt((current.x - neigh_x) * (current.x - neigh_x) + (current.z - neigh_z) * (current.z - neigh_z));

                    if (!openNodes.Contains(new Vector3((curr_x) * squareSize, transform.position.y, (curr_z + j) * squareSize)))
                    {
                        openNodes.Add(new Vector3((curr_x) * squareSize, transform.position.y, (curr_z + j) * squareSize)); //new tile to investigate
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

    private List<Vector3> ThetaStarNumMap(Vector3 destPos) //this method works
    {
        closedNodes = new List<Vector3>();
        openNodes = new List<Vector3>();
        cameFrom = new Vector3[numeric_robot_map.GetLength(0), numeric_robot_map.GetLength(1)];
        gScore = new float[numeric_robot_map.GetLength(0), numeric_robot_map.GetLength(1)];
        fScore = new float[numeric_robot_map.GetLength(0), numeric_robot_map.GetLength(1)];

        openNodes.Add(transform.position);

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

        current = openNodes[0];

        while (openNodes.Count > 0)
        {
            current = openNodes[0];
            for (int i = 0; i < openNodes.Count; i++)
            {

                float fS = fScore[(int)FixingRound(openNodes[i].x / squareSize), (int)FixingRound(openNodes[i].z / squareSize)];
                float curFScore = fScore[(int)FixingRound(current.x / squareSize), (int)FixingRound(current.z / squareSize)];

                if (i != 0 && fS < curFScore)
                {
                    current = openNodes[i];
                }

            }

            int curr_x = (int)FixingRound(current.x / squareSize);
            int curr_z = (int)FixingRound(current.z / squareSize);
            int dest_x = (int)FixingRound(destPos.x / squareSize);
            int dest_z = (int)FixingRound(destPos.z / squareSize);

            openNodes.Remove(current);
            closedNodes.Add(current);

            if (curr_x == dest_x && curr_z == dest_z)//if the current position is the goal one, we can exit from the procedure
            {
                return ReconstructPath(cameFrom, current, destPos);
            }

            for (int i = -1; i < 2; i++)
            {
                if (closedNodes.Contains(new Vector3((curr_x + i) * squareSize, transform.position.y, (curr_z) * squareSize)))
                {
                    continue;
                }
                else if (((curr_x + i) >= 0 && (curr_x + i) < gScore.GetLength(0)) && ((curr_z) >= 0 && (curr_z) < gScore.GetLength(1)) && (numeric_robot_map[curr_x + i, curr_z] != unknownCell))
                {
                    float neigh_x = (curr_x + i) * squareSize;
                    float neigh_z = (curr_z) * squareSize;
                    tentativeGScore = gScore[curr_x + i, curr_z] + Mathf.Sqrt((current.x - neigh_x) * (current.x - neigh_x) + (current.z - neigh_z) * (current.z - neigh_z));

                    if (!openNodes.Contains(new Vector3((curr_x + i) * squareSize, transform.position.y, (curr_z) * squareSize)))
                    {
                        openNodes.Add(new Vector3((curr_x + i) * squareSize, transform.position.y, (curr_z) * squareSize));
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
                if (closedNodes.Contains(new Vector3((curr_x) * squareSize, transform.position.y, (curr_z + j) * squareSize)))
                {
                    continue;
                }
                else if (((curr_x) >= 0 && (curr_x) < gScore.GetLength(0)) && ((curr_z + j) >= 0 && (curr_z + j) < gScore.GetLength(1)) && (numeric_robot_map[curr_x, curr_z + j] != unknownCell))
                {
                    float neigh_x = (curr_x) * squareSize;
                    float neigh_z = (curr_z + j) * squareSize;
                    tentativeGScore = gScore[curr_x, curr_z + j] + Mathf.Sqrt((current.x - neigh_x) * (current.x - neigh_x) + (current.z - neigh_z) * (current.z - neigh_z));

                    if (!openNodes.Contains(new Vector3((curr_x) * squareSize, transform.position.y, (curr_z + j) * squareSize)))
                    {
                        openNodes.Add(new Vector3((curr_x) * squareSize, transform.position.y, (curr_z + j) * squareSize));
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

        for (int i = 0; i < cameFom.GetLength(0); i++)
        {
            for (int j = 0; j < cameFom.GetLength(1); j++)
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
        total_path = TrulyUpdateVertexPath(total_path);
        return total_path;

    }

    private List<Vector3> TrulyUpdateVertexPath(List<Vector3> path)
    {
        //Debug.Log(path.Count);
        List<Vector3> optimal_path = new List<Vector3>();

        //RaycastHit hit;
        optimal_path.Insert(0, path[path.Count - 1]);
        bool notContinue = false;
        int i = path.Count - 1;
        while (i > 0)
        {
            int j = i;
            while (j > 0 && !notContinue)
            {
                float distance = Mathf.Sqrt((path[i].x - path[j - 1].x) * (path[i].x - path[j - 1].x) + (path[i].z - path[j - 1].z) * (path[i].z - path[j - 1].z));
                //if (Physics.Linecast(path[i], path[j - 1], out hit, layerMask, QueryTriggerInteraction.UseGlobal) || Vector3.Distance(path[i], path[j - 1]) > range)
                //Debug.DrawLine(path[i], path[j - 1], Color.green, 4f);
                if (Physics.Linecast(path[i], path[j - 1], layerMask) || distance > range)
                {
                    //if(Vector3.Distance(path[i], path[j - 1]) <= range)
                    //Debug.Log(hit.transform.gameObject);
                    optimal_path.Insert(0, path[j]);
                    notContinue = true;
                }
                j--;
            }
            notContinue = false;
            i = j;
        }

        //Debug.Log(path.Count);
        return optimal_path;
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
