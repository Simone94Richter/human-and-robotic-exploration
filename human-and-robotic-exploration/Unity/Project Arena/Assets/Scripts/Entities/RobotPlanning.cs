using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotPlanning_Entity : MonoBehaviour {

    public bool isNumeric;
    public bool hasTerminated = false;

    public float range;

    public float epsilon;

    [Header("Heauristic value")]
    public float heuristic;

    public char[,] robot_map;
    public float[,] numeric_robot_map;

    List<Vector3> pos;
    List<List<Vector3>> possibleRoutes;

    int layerMask = 1 << 0;

    public float squareSize;

    int cost;

    Vector3 robot;
    Vector3 destination;

	// Use this for initialization
	void Start () {
        //layerMask = ~layerMask;
    }

    public List<Vector3> CheckVisibility(Vector3 robot, Vector3 destination, List<Vector3> positions)
    {
        //robot = new Vector3(robot_x, transform.position.y, robot_z);
        //destination = new Vector3(temp_x, transform.position.y, temp_z);
        Debug.DrawLine(robot*squareSize, destination*squareSize, Color.green, 4f);
        Vector3 robotPos = robot * squareSize;
        Vector3 destPos = destination * squareSize;
        float distance = Mathf.Sqrt((robotPos.x - destPos.x)*(robotPos.x - destPos.x) + (robotPos.z - destPos.z) * (robotPos.z - destPos.z));
        if (Physics.Linecast(robot * squareSize, destination * squareSize, layerMask) || distance > range /*&& !hasTerminated*/)
        {
            Debug.Log("Something hidden or too far");
            if (!isNumeric)
            {
                //return DijkstraCharMap(robot_map, destination*squareSize, robot*squareSize);
                return AstarCharMap(robot*squareSize, destination*squareSize);
            }else return DijkstraNumMap(numeric_robot_map, destination*squareSize, robot*squareSize);
        }
        //else hasTerminated = true; 

        return null;
    }

    private List<Vector3> DijkstraNumMap(float[,] map, Vector3 destination, Vector3 robot)
    {
        List<Vector3> allPositions = new List<Vector3>();
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> tempDest = new List<Vector3>();
        Vector3 pos;

        float dx = (robot.x - destination.x) * (robot.x - destination.x); //* squareSize * squareSize;
        float dz = (robot.z - destination.z) * (robot.z - destination.z); //* squareSize * squareSize;
        float dist = Mathf.Sqrt(dx + dz);
        float tempDist;
        float tempDistNext;
        float distNext;

        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i,j] == 0f)
                {
                    allPositions.Add(new Vector3(i*squareSize, transform.position.y, j*squareSize));
                }
            }
        }

        pos = new Vector3(robot.x * squareSize, robot.y * squareSize, robot.z * squareSize);
        allPositions.Remove(pos);
        positions.Add(pos);

        while (pos != destination)
        {
            distNext = Mathf.Sqrt( (pos.x - allPositions[0].x) * (pos.x - allPositions[0].x) + (pos.z - allPositions[0].z) * (pos.x - allPositions[0].z));
            foreach (Vector3 nextPos in allPositions)
            {
                tempDistNext = Mathf.Sqrt((pos.x - nextPos.x) * (pos.x - nextPos.x) + (pos.z - nextPos.z) * (pos.x - nextPos.z));
                if /*((Mathf.Abs(pos.x - nextPos.x) <= epsilon || Mathf.Abs(pos.z - nextPos.z) <= epsilon) 
                    && Mathf.Abs(pos.x - nextPos.x + pos.z - nextPos.z) <= epsilon)//i have found an adjacent tile*/
                    (tempDistNext <= distNext)
                {
                    distNext = tempDistNext;
                    tempDest.Add(nextPos);
                }
            }
            foreach (Vector3 nextPos in tempDest)
            {
                tempDist = Mathf.Sqrt((nextPos.x - destination.x) * (nextPos.x - destination.x) + (nextPos.z - destination.z) * (nextPos.z - destination.z));
                if(tempDist < dist)
                {
                    dist = tempDist;
                    pos = nextPos;
                }
            }

            positions.Add(pos);
            allPositions.Remove(pos);
            tempDest.Clear();
        }

        return positions;
    }
    private List<Vector3> DijkstraCharMap(char[,] map, Vector3 destination, Vector3 robot)
    {
        List<Vector3> allPositionsOriginal = new List<Vector3>();
        List<Vector3> allPositions = new List<Vector3>();
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> tempDest = new List<Vector3>();
        Vector3 pos;

        //int count = 5; //only for testing purpose

        float dx = (transform.position.x - destination.x) * (transform.position.x - destination.x);
        float dz = (transform.position.z - destination.z) * (transform.position.z - destination.z);
        float dist = Mathf.Sqrt(dx + dz);
        //float dist = Mathf.Infinity;
        float tempDist;
        float distNext;
        float tempDistNext;

        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j] == 'r')
                {
                    allPositions.Add(new Vector3(i * squareSize, transform.position.y, j * squareSize));
                }
            }
        }

        //pos = new Vector3(robot.x * squareSize, robot.y * squareSize, robot.z * squareSize);
        pos = transform.position;
        allPositions.Remove(pos);
        allPositionsOriginal = allPositions;
        positions.Add(pos);
        //count = allPositions.Count;

        Debug.Log("Calculating Path");
        while (Physics.Linecast(pos, destination, layerMask))//condition to be changed, it's just to see    count > 0
        {
            //Debug.Log(allPositions.Count);
            Vector3 tempNext = transform.position;
            //distNext = Mathf.Sqrt((pos.x - allPositions[0].x) * (pos.x - allPositions[0].x) + (pos.z - allPositions[0].z) * (pos.z - allPositions[0].z)) + heuristic;
            distNext = Mathf.Infinity;
            //distNext = 10000f;
            foreach (Vector3 nextPos in allPositions)
            {
                tempDistNext = Mathf.Sqrt((pos.x - nextPos.x) * (pos.x - nextPos.x) + (pos.z - nextPos.z) * (pos.z - nextPos.z));
                if /*((Mathf.Abs(pos.x - nextPos.x) <= epsilon || Mathf.Abs(pos.z - nextPos.z) <= epsilon) 
                    && Mathf.Abs(pos.x - nextPos.x + pos.z - nextPos.z) <= epsilon)//i have found an adjacent tile*/
                    (tempDistNext < distNext)
                {
                    //Debug.Log(nextPos);
                    //distNext = tempDistNext;
                    //tempNext = nextPos;
                    tempDest.Add(nextPos);
                }
            }

            //tempDest.Add(tempNext);
            //Debug.Log(tempNext);

            //Debug.Log(tempDest.Count);
            foreach (Vector3 nextPos in tempDest)
            {
                tempDist = Mathf.Sqrt((nextPos.x - destination.x) * (nextPos.x - destination.x) + (nextPos.z - destination.z) * (nextPos.z - destination.z));
                //Debug.Log(tempDist + ", " + dist);
                if (tempDist < dist)
                {
                    dist = tempDist;
                    pos = nextPos;
                }
            }

            //Debug.Log(pos);
            if (!positions.Contains(pos))
            {
                positions.Add(pos);
                allPositions = allPositionsOriginal;
            }
            //allPositions.Remove(pos);
            allPositions.Remove(tempNext);
            tempDest.Clear();

            //count--;
        }

        positions.Add(destination);

        /*foreach (Vector3 finalPos in positions)
        {
            Debug.Log(finalPos);
        }*/
        return positions;
    }

    private List<Vector3> AstarCharMap(Vector3 startPos, Vector3 destPos) //this method works
    {
        List<Vector3> closedSet = new List<Vector3>(), openSet = new List<Vector3>();
        Vector3[,] cameFrom = new Vector3[robot_map.GetLength(0), robot_map.GetLength(1)];
        float[,] gScore = new float[robot_map.GetLength(0), robot_map.GetLength(1)];
        float[,] fScore = new float[robot_map.GetLength(0), robot_map.GetLength(1)];
        Vector3 current;
        float tentativeGScore;

        openSet.Add(transform.position);

        for (int i = 0; i < gScore.GetLength(0); i++)
        {
            for (int j = 0; j < gScore.GetLength(1); j++)
            {
                gScore[i, j] = Mathf.Infinity;
                fScore[i, j] = Mathf.Infinity;
            }
        }

        gScore[(int)FixingRound(transform.position.x/squareSize),(int)FixingRound(transform.position.z/squareSize)] = 0;
        fScore[(int)FixingRound(transform.position.x / squareSize), (int)FixingRound(transform.position.z / squareSize)] = heuristic;

        current = openSet[0];

        //Debug.Log(cameFrom[0,0]);
        //int count = 2; //only for testing purpose
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

            if (curr_x == dest_x && curr_z == dest_z)
            {
                //Debug.Log("Inside while");
                //Debug.Log(current);
                return ReconstructPath(cameFrom, current, destPos);
            }

            for (int i = -1; i<2; i++)
            {
                if (closedSet.Contains(new Vector3((curr_x + i)*squareSize, transform.position.y, (curr_z)*squareSize)))
                {
                    continue;
                }
                else if( ( (curr_x + i) >= 0 && (curr_x + i) < gScore.GetLength(0) ) && ( (curr_z) >= 0 && (curr_z) < gScore.GetLength(1) ) && (robot_map[curr_x+i,curr_z] == 'r'))
                {
                    float neigh_x = (curr_x + i) * squareSize;
                    float neigh_z = (curr_z) * squareSize;
                    tentativeGScore = gScore[curr_x+i,curr_z] + Mathf.Sqrt((current.x - neigh_x) * (current.x - neigh_x) + (current.z - neigh_z) * (current.z - neigh_z));
                    //Debug.Log((curr_x + i) * squareSize + ", " + transform.position.y + ", " + (curr_z + j) * squareSize);
                    //Debug.Log(current.x + squareSize*i + ", " + transform.position.y + ", " + current.z + squareSize*j);
                    if (!openSet.Contains(new Vector3((curr_x + i) * squareSize, transform.position.y, (curr_z) * squareSize)))
                    {
                        openSet.Add(new Vector3((curr_x + i) * squareSize, transform.position.y, (curr_z) * squareSize));
                    }
                    else if (tentativeGScore >= gScore[curr_x +i, curr_z])
                    {
                        continue;
                    }

                    cameFrom[curr_x+i, curr_z] = current;
                    //Debug.Log(current);
                    gScore[curr_x + i, curr_z] = tentativeGScore;
                    fScore[curr_x + i, curr_z] = gScore[curr_x + i, curr_z] + heuristic;
                }
                
            }

            for (int j = -1; j < 2; j++)
            {
                if (closedSet.Contains(new Vector3((curr_x) * squareSize, transform.position.y, (curr_z + j) * squareSize)))
                {
                    continue;
                }
                else if (((curr_x) >= 0 && (curr_x) < gScore.GetLength(0)) && ((curr_z + j) >= 0 && (curr_z + j) < gScore.GetLength(1)) && (robot_map[curr_x, curr_z + j] == 'r'))
                {
                    float neigh_x = (curr_x) * squareSize;
                    float neigh_z = (curr_z + j) * squareSize;
                    tentativeGScore = gScore[curr_x, curr_z + j] + Mathf.Sqrt((current.x - neigh_x) * (current.x - neigh_x) + (current.z - neigh_z) * (current.z - neigh_z));
                    //Debug.Log((curr_x + i) * squareSize + ", " + transform.position.y + ", " + (curr_z + j) * squareSize);
                    //Debug.Log(current.x + squareSize*i + ", " + transform.position.y + ", " + current.z + squareSize*j);
                    if (!openSet.Contains(new Vector3((curr_x) * squareSize, transform.position.y, (curr_z + j) * squareSize)))
                    {
                        openSet.Add(new Vector3((curr_x) * squareSize, transform.position.y, (curr_z + j) * squareSize));
                    }
                    else if (tentativeGScore >= gScore[curr_x, curr_z + j])
                    {
                        continue;
                    }

                    cameFrom[curr_x, curr_z + j] = current;
                    //Debug.Log(current);
                    gScore[curr_x, curr_z + j] = tentativeGScore;
                    fScore[curr_x, curr_z + j] = gScore[curr_x, curr_z + j] + heuristic;
                }
            }

            //count--;
        }

        //Debug.Log("Out of while");
        return null;//ReconstructPath(cameFrom, current);

    }

    private List<Vector3> ReconstructPath(Vector3[,] cameFom, Vector3 current, Vector3 destination) //this method gives problems
    {
        List<Vector3> total_path = new List<Vector3>();
        List<Vector3> optimal_path = new List<Vector3>();
        total_path.Add(current);
        float distance;
        int lastTwo = 2;
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
        for (int k = 0; k < total_path.Count; k++)
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
        
        return optimal_path;

    }

    private float FixingRound(float coordinate)
    {
        if (Mathf.Abs(coordinate - Mathf.Round(coordinate)) >= 0.5f)
        {
            return (Mathf.Round(coordinate) - 1);
        }
        else return Mathf.Round(coordinate);
    }
}
