using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotPlanningDjikstra : RobotPlanning {

    private int layerMask = 1 << 0;

    public override List<Vector3> CheckVisibility(Vector3 robot, Vector3 destination)
    {
        Debug.DrawLine(robot, destination, Color.green, 4f);
        float distance = Mathf.Sqrt((robot.x - destination.x) * (robot.x - destination.x) + (robot.z - destination.z) * (robot.z - destination.z));
        if (Physics.Linecast(robot, destination, layerMask) || distance > range)
        {
            if (!isNumeric)
            {
                return DjikstraCharMap(robot_map, destination, transform.position);
            }
            else return DijkstraNumMap(numeric_robot_map, destination, transform.position);
        }

        return null;
    }

    private List<Vector3> DijkstraNumMap(float[,] map, Vector3 destination, Vector3 source)
    {
        List<Vector3> positions = new List<Vector3>();// the vertex set of univisited vertexes
        List<Vector3> exploredPositions = new List<Vector3>();
        //List<Vector3> tempDest = new List<Vector3>();
        float[,] distVertex = new float[map.GetLength(0), map.GetLength(1)];
        Vector3[,] prev = new Vector3[map.GetLength(0), map.GetLength(1)];

        Vector3 pos;

        float alt;
        float dist;
        int destX = (int)FixingRound(destination.x / squareSize);
        int destZ = (int)FixingRound(destination.z / squareSize);

        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j] != unknownCell)
                {
                    distVertex[i, j] = Mathf.Infinity;
                    prev[i, j] = Vector3.zero;
                }
            }
        }

        distVertex[(int)FixingRound(source.x / squareSize), (int)FixingRound(source.z / squareSize)] = 0f;
        positions.Add(source);
        while (positions.Count > 0)
        {
            pos = positions[0];
            int posX = (int)FixingRound(pos.x / squareSize);
            int posZ = (int)FixingRound(pos.z / squareSize);
            for (int i = 0; i < positions.Count; i++)
            {
                if (distVertex[posX, posZ] > distVertex[(int)FixingRound(positions[i].x / squareSize), (int)FixingRound(positions[i].z / squareSize)])
                {
                    pos = positions[i];
                    posX = (int)FixingRound(pos.x / squareSize);
                    posZ = (int)FixingRound(pos.z / squareSize);
                }
            }

            positions.Remove(pos);
            exploredPositions.Add(pos);

            if (posX == destX && posZ == destZ)//if the current position is the goal one, we can exit from the procedure
            {
                return ReconstructPath(prev, pos, destination);
            }

            for (int i = -1; i < 2; i++) //checking neighbours on left and right
            {
                Vector3 neighbour = new Vector3((posX + i) * squareSize, transform.position.y, posZ * squareSize);
                if (exploredPositions.Contains(neighbour))
                {
                    continue;
                }
                else if (posX + i >= 0 && posX + i < map.GetLength(0) && posZ >= 0 && posZ < map.GetLength(1) && map[posX + i, posZ] != unknownCell)
                {
                    int neighbourX = (int)FixingRound(neighbour.x / squareSize);
                    int neighbourZ = (int)FixingRound(neighbour.z / squareSize);

                    dist = Mathf.Sqrt((pos.x - neighbour.x) * (pos.x - neighbour.x) + (pos.z - neighbour.z) * (pos.z - neighbour.z));

                    alt = distVertex[posX, posZ] + dist;

                    if (!positions.Contains(neighbour))
                    {
                        positions.Add(neighbour);
                    }
                    if (alt < distVertex[neighbourX, neighbourZ])
                    {
                        distVertex[neighbourX, neighbourZ] = alt;
                        prev[neighbourX, neighbourZ] = pos;
                    }
                }
            }

            for (int j = -1; j < 2; j++) //checking neighbours up and down
            {
                Vector3 neighbour = new Vector3((posX) * squareSize, transform.position.y, (posZ + j) * squareSize);
                if (exploredPositions.Contains(neighbour))
                {
                    continue;
                }
                else if (posX >= 0 && posX < map.GetLength(0) && posZ + j >= 0 && posZ + j < map.GetLength(1) && map[posX, posZ + j] != unknownCell)
                {
                    int neighbourX = (int)FixingRound(neighbour.x / squareSize);
                    int neighbourZ = (int)FixingRound(neighbour.z / squareSize);

                    dist = Mathf.Sqrt((pos.x - neighbour.x) * (pos.x - neighbour.x) + (pos.z - neighbour.z) * (pos.z - neighbour.z));

                    alt = distVertex[posX, posZ] + dist;

                    if (!positions.Contains(neighbour))
                    {
                        positions.Add(neighbour);
                    }
                    if (alt < distVertex[neighbourX, neighbourZ])
                    {
                        distVertex[neighbourX, neighbourZ] = alt;
                        prev[neighbourX, neighbourZ] = pos;
                    }
                }
            }

            return exploredPositions;
        }

        return null;
    }

    private List<Vector3> DjikstraCharMap(char[,] map, Vector3 destination, Vector3 source)
    {
        List<Vector3> positions = new List<Vector3>();// the vertex set of univisited vertexes
        List<Vector3> exploredPositions = new List<Vector3>();
        //List<Vector3> tempDest = new List<Vector3>();
        float[,] distVertex = new float[map.GetLength(0), map.GetLength(1)];
        Vector3[,] prev = new Vector3[map.GetLength(0), map.GetLength(1)];

        Vector3 pos;

        float alt;
        float dist;
        int destX = (int)FixingRound(destination.x / squareSize);
        int destZ = (int)FixingRound(destination.z / squareSize);

        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j] != unknownCell)
                {
                    distVertex[i, j] = Mathf.Infinity;
                    prev[i, j] = Vector3.zero;
                }
            }
        }

        distVertex[(int)FixingRound(source.x / squareSize), (int)FixingRound(source.z / squareSize)] = 0f;
        positions.Add(source);
        while (positions.Count > 0)
        {
            pos = positions[0];
            int posX = (int)FixingRound(pos.x / squareSize);
            int posZ = (int)FixingRound(pos.z / squareSize);
            for (int i = 0; i < positions.Count; i++)
            {
                if (distVertex[posX, posZ] > distVertex[(int)FixingRound(positions[i].x / squareSize), (int)FixingRound(positions[i].z / squareSize)])
                {
                    pos = positions[i];
                    posX = (int)FixingRound(pos.x / squareSize);
                    posZ = (int)FixingRound(pos.z / squareSize);
                }
            }

            positions.Remove(pos);
            exploredPositions.Add(pos);

            if (posX == destX && posZ == destZ)//if the current position is the goal one, we can exit from the procedure
            {
                return ReconstructPath(prev, pos, destination);
            }

            for (int i = -1; i < 2; i++) //checking neighbours on left and right
            {
                Vector3 neighbour = new Vector3((posX + i) * squareSize, transform.position.y, posZ * squareSize);
                if (exploredPositions.Contains(neighbour))
                {
                    continue;
                }
                else if (posX + i >= 0 && posX + i < map.GetLength(0) && posZ >= 0 && posZ < map.GetLength(1) && map[posX + i, posZ] != unknownCell)
                {
                    int neighbourX = (int)FixingRound(neighbour.x / squareSize);
                    int neighbourZ = (int)FixingRound(neighbour.z / squareSize);

                    dist = Mathf.Sqrt((pos.x - neighbour.x) * (pos.x - neighbour.x) + (pos.z - neighbour.z) * (pos.z - neighbour.z));

                    alt = distVertex[posX, posZ] + dist;

                    if (!positions.Contains(neighbour))
                    {
                        positions.Add(neighbour);
                    }
                    if (alt < distVertex[neighbourX, neighbourZ])
                    {
                        distVertex[neighbourX, neighbourZ] = alt;
                        prev[neighbourX, neighbourZ] = pos;
                    }
                }
            }

            for (int j = -1; j < 2; j++) //checking neighbours up and down
            {
                Vector3 neighbour = new Vector3((posX) * squareSize, transform.position.y, (posZ + j) * squareSize);
                if (exploredPositions.Contains(neighbour))
                {
                    continue;
                }
                else if (posX >= 0 && posX < map.GetLength(0) && posZ + j >= 0 && posZ + j < map.GetLength(1) && map[posX, posZ + j] != unknownCell)
                {
                    int neighbourX = (int)FixingRound(neighbour.x / squareSize);
                    int neighbourZ = (int)FixingRound(neighbour.z / squareSize);

                    dist = Mathf.Sqrt((pos.x - neighbour.x) * (pos.x - neighbour.x) + (pos.z - neighbour.z) * (pos.z - neighbour.z));

                    alt = distVertex[posX, posZ] + dist;

                    if (!positions.Contains(neighbour))
                    {
                        positions.Add(neighbour);
                    }
                    if (alt < distVertex[neighbourX, neighbourZ])
                    {
                        distVertex[neighbourX, neighbourZ] = alt;
                        prev[neighbourX, neighbourZ] = pos;
                    }
                }
            }

            return exploredPositions;
        }

        return null;
    }

    private List<Vector3> ReconstructPath(Vector3[,] cameFom, Vector3 current, Vector3 destination)
    {
        List<Vector3> total_path = new List<Vector3>();
        total_path.Add(current);
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
                }
            }
        }

        while (current != transform.position)
        {
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
                    }
                }
            }

        }

        return total_path;
    }
}
