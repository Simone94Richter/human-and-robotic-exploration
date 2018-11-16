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
    private List<Vector3> wallList = new List<Vector3>();

    //private Vector3 tempWall;
    private float wallX;
    private float wallY;

    public override Vector3 PosToReach(List<Vector3> listFrontierPoints)
    {
        //obatining position of the wall close to the player
        closestWall = CalculatingClosestWall();

        Vector3 frontierCell;
        frontierCell = listFrontierPoints[0];
        distance = Mathf.Sqrt((GetXCoordinateWall(closestWall) - frontierCell.x) * (GetXCoordinateWall(closestWall) - frontierCell.x) + (GetZCoordinateWall(closestWall) - frontierCell.z) * (GetZCoordinateWall(closestWall) - frontierCell.z));
        //obtaining position of the frontier point close to the wall found before
        for (int i = 0; i < listFrontierPoints.Count; i++)
        {
            //find the closest one
            tempDistance = Mathf.Sqrt((GetXCoordinateWall(closestWall) - listFrontierPoints[i].x) * (GetXCoordinateWall(closestWall) - listFrontierPoints[i].x) + (GetZCoordinateWall(closestWall) - listFrontierPoints[i].z) * (GetZCoordinateWall(closestWall) - listFrontierPoints[i].z));

            if (distance > tempDistance)
            {
                distance = tempDistance;
                frontierCell = listFrontierPoints[i];
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
