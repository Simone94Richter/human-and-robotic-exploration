using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotDMCloseWall : RobotDecisionMaking {

    private char[,] char_map;
    private float[,] numeric_map;

    private float distance;
    private float squareSize;
    private float tempDistance;

    private List<Vector3> closestWall = new List<Vector3>();
    private List<Vector3> closestFrontier = new List<Vector3>();
    private List<Vector3> wallList = new List<Vector3>();
    private List<List<Vector3>> frontierZones;

    //private Vector3 tempWall;
    private float wallX;
    private float wallY;

    public override Vector3 PosToReach(List<Vector3> listFrontierPoints)
    {
        frontierZones = new List<List<Vector3>>();

        //obatining position of the wall close to the player
        closestWall = CalculatingClosestWall();

        //defining frontier zones
        DefiningFrontierZones(listFrontierPoints);
        Debug.Log(frontierZones.Count);

        //choosing the frontier zone closer to the selected wall
        closestFrontier = CalculatingClosestFrontier();
        Debug.Log(GetXCoordinateFrontier(closestFrontier) + ", " + GetZCoordinateFrontier(closestFrontier));

        //choosing the frontier point (of the chosen zone) closer to the chosen wall
        Vector3 frontierCell;
        frontierCell = closestFrontier[0];
        Debug.Log(frontierCell);
        distance = Mathf.Sqrt((GetXCoordinateWall(closestWall) - frontierCell.x) * (GetXCoordinateWall(closestWall) - frontierCell.x) + (GetZCoordinateWall(closestWall) - frontierCell.z) * (GetZCoordinateWall(closestWall) - frontierCell.z));
        for (int i = 0; i < closestFrontier.Count; i++)
        {
            //find the closest one
            tempDistance = Mathf.Sqrt((GetXCoordinateWall(closestWall) - closestFrontier[i].x) * (GetXCoordinateWall(closestWall) - closestFrontier[i].x) + (GetZCoordinateWall(closestWall) - closestFrontier[i].z) * (GetZCoordinateWall(closestWall) - closestFrontier[i].z));

            if (distance > tempDistance)
            {
                distance = tempDistance;
                frontierCell = closestFrontier[i];
            }
        }

        //returning the point
        return frontierCell;
    }

    private List<Vector3> CalculatingClosestWall()
    {
        List<Vector3> wallFound = new List<Vector3>(); 

        for (int i = 0; i < numeric_map.GetLength(0) ; i++)
        {
            for (int j = 0; j < numeric_map.GetLength(1); j++)
            {
                if (numeric_map[i,j] == 1f) 
                {
                    wallList.Add(new Vector3(squareSize * i, transform.position.y, squareSize * j));
                }
            }
        }

        Vector3 wallCell;
        wallCell = wallList[0];
        distance = Mathf.Sqrt( (transform.position.x - wallCell.x)*(transform.position.x - wallCell.x) + (transform.position.z - wallCell.z)*(transform.position.z - wallCell.z) );

        for (int i = 0; i < wallList.Count; i++)
        {
            tempDistance = Mathf.Sqrt((transform.position.x - wallList[i].x) * (transform.position.x - wallList[i].x) + (transform.position.z - wallList[i].z) * (transform.position.z - wallList[i].z));
            if (tempDistance < distance)
            {
                wallCell = wallList[i];
                distance = tempDistance;
            }
        }

        wallFound.Add(wallCell);

        AddNeighbourWalls(wallCell, wallFound);

        return wallFound;
    }

    private void DefiningFrontierZones(List<Vector3> frontierPoints)
    {
        List<Vector3> currentZone = new List<Vector3>();
        for (int i = 0; i < frontierPoints.Count; i++)
        {
            if (frontierZones.Count == 0 || !currentZone.Contains(frontierPoints[i]))
            {
                currentZone = new List<Vector3>();
                currentZone.Add(frontierPoints[i]);
                frontierZones.Add(currentZone);

                AddNeighbourFrontier(frontierPoints[i], currentZone);
            }
        }
    }

    private List<Vector3> CalculatingClosestFrontier()
    {
        List<Vector3> frontier = frontierZones[0];
        distance = Mathf.Sqrt( (GetXCoordinateFrontier(frontierZones[0]) - GetXCoordinateWall(closestWall)) * (GetXCoordinateFrontier(frontierZones[0]) - GetXCoordinateWall(closestWall)) + (GetZCoordinateFrontier(frontierZones[0]) - GetZCoordinateWall(closestWall)) * (GetZCoordinateFrontier(frontierZones[0]) - GetZCoordinateWall(closestWall)) );
        for (int i = 0; i < frontierZones.Count; i++)
        {
            tempDistance = Mathf.Sqrt((GetXCoordinateFrontier(frontierZones[i]) - GetXCoordinateWall(closestWall)) * (GetXCoordinateFrontier(frontierZones[i]) - GetXCoordinateWall(closestWall)) + (GetZCoordinateFrontier(frontierZones[i]) - GetZCoordinateWall(closestWall)) * (GetZCoordinateFrontier(frontierZones[i]) - GetZCoordinateWall(closestWall)));
            if (tempDistance < distance)
            {
                distance = tempDistance;
                frontier = frontierZones[i];
            }
        }

        return frontier;
    }

    private void AddNeighbourFrontier(Vector3 frontierPoint, List<Vector3> frontierZone)
    {
        int x, z;
        for (int i = -1; i < 2; i++)
        {
            x = (int)FixingRound(frontierPoint.x / squareSize);
            z = (int)FixingRound(frontierPoint.z / squareSize);
            if (x + i >= 0 && x + i <= numeric_map.GetLength(0) - 1)
            {
                if (numeric_map[x + i, z] == 1 && !frontierZone.Contains(new Vector3(squareSize * (x + i), transform.position.y, squareSize * z)))
                {
                    Vector3 newFrontPoint = new Vector3(squareSize * (x + i), transform.position.y, squareSize * z);
                    frontierZone.Add(newFrontPoint);
                    AddNeighbourWalls(newFrontPoint, frontierZone);
                }
            }
        }

        for (int j = -1; j < 2; j++)
        {
            x = (int)FixingRound(frontierPoint.x / squareSize);
            z = (int)FixingRound(frontierPoint.z / squareSize);
            if (z + j >= 0 && z + j <= numeric_map.GetLength(1) - 1)
            {
                if (numeric_map[x, z + j] == 1 && !frontierZone.Contains(new Vector3(squareSize * x, transform.position.y, squareSize * (z + j))))
                {
                    Vector3 newFrontPoint = new Vector3(squareSize * x, transform.position.y, squareSize * (z + j));
                    frontierZone.Add(newFrontPoint);
                    AddNeighbourWalls(newFrontPoint, frontierZone);
                }
            }
        }
    }

    private void AddNeighbourWalls(Vector3 cell, List<Vector3> wall)
    {
        int x, z;
        for (int i = -1; i < 2; i++)
        {
            x = (int)FixingRound(cell.x / squareSize);
            z = (int)FixingRound(cell.z / squareSize);
            if (x + i >= 0 && x + i <= numeric_map.GetLength(0) - 1)
            {
                if (numeric_map[x + i, z] == 1 && !wall.Contains(new Vector3(squareSize * (x + i), transform.position.y, squareSize * z)))
                {
                    Vector3 newWall = new Vector3(squareSize * (x + i), transform.position.y, squareSize * z);
                    wall.Add(newWall);
                    AddNeighbourWalls(newWall, wall);
                }
            }
        }

        for (int j = -1; j < 2; j++)
        {
            x = (int)FixingRound(cell.x / squareSize);
            z = (int)FixingRound(cell.z / squareSize);
            if (z + j >= 0 && z + j <= numeric_map.GetLength(1) -1)
            {
                if (numeric_map[x, z + j] == 1 && !wall.Contains(new Vector3(squareSize * x, transform.position.y, squareSize * (z + j))))
                {
                    Vector3 newWall = new Vector3(squareSize * x, transform.position.y, squareSize * (z + j));
                    wall.Add(newWall);
                    AddNeighbourWalls(newWall, wall);
                }
            }
        }
    }

    private float GetXCoordinateWall(List<Vector3> wall) 
    {
        float x = 0;
        for (int i = 0; i < wall.Count; i++)
        {
            x = x + wall[i].x;
        }

        x = x / wall.Count;

        return x;
    }

    private float GetXCoordinateFrontier(List<Vector3> frontier)
    {
        float x = 0;
        for (int i = 0; i < frontier.Count; i++)
        {
            x = x + frontier[i].x;
        }

        x = x / frontier.Count;

        return x;
    }

    private float GetZCoordinateWall(List<Vector3> wall)
    {
        float z = 0;
        for (int i = 0; i < wall.Count; i++)
        {
            z = z + wall[i].z;
        }

        z = z / wall.Count;

        return z;
    }

    private float GetZCoordinateFrontier(List<Vector3> frontier)
    {
        float z = 0;
        for (int i = 0; i < frontier.Count; i++)
        {
            z = z + frontier[i].z;
        }

        z = z / frontier.Count;

        return z;
    }

    public void SetMap(float[,] map)
    {
        numeric_map = map;
    }

    public void SetMap(char[,] map)
    {
        char_map = map;
    }

    public void SetSquareSize(float size)
    {
        squareSize = size;
    }

    /// <summary>
    /// This method corrects the round of coordinates of a tile. In this way is avoided to assign values to undesired tiles (for example, a wall tile as a free one)
    /// </summary>
    /// <param name="coordinate">The coordinate of a tile</param>
    /// <returns></returns>
    private float FixingRound(float coordinate)
    {
        if (Mathf.Abs(coordinate - Mathf.Round(coordinate)) >= 0.5f)
        {
            return (Mathf.Round(coordinate) - 1);
        }
        else return Mathf.Round(coordinate);
    }

    public void SetNumericMap(float[,] map)
    {
        numeric_map = map;
    }

}
