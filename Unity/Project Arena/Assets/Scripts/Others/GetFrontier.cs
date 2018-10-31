using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Coordinate
{
    private float row;
    private float col;

    public Coordinate(float row, float col)
    {
        this.row = row;
        this.col = col;
    }

    public Coordinate(Coordinate coordinate)
    {
        this.row = coordinate.GetRow();
        this.col = coordinate.GetCol();
    }

    public override bool Equals(object obj)
    {
        if (Object.ReferenceEquals(this, obj))
            return true;

        Coordinate instance = obj as Coordinate;
        if (instance == null)
            return false;

        //return this.row == instance.row && this.col == instance.col;
        return false;
    }

    public override int GetHashCode()
    {
        var hashCode = 1502939027;
        hashCode = hashCode * -1521134295 + row.GetHashCode();
        hashCode = hashCode * -1521134295 + col.GetHashCode();
        return hashCode;
    }

    public override string ToString()
    {
        return "(" + this.row + " , " + this.col + ")";
    }

    public float GetRow()
    {
        return this.row;
    }

    public float GetCol()
    {
        return this.col;
    }

    public void SetRow(int row)
    {
        this.row = row;
    }

    public void SetCol(int col)
    {
        this.col = col;
    }
}



public class Frontier
{
    public static readonly int Max_Coordinates = 10;
    public static readonly int distance = 2;

    private Coordinate centerPt;
    private List<Coordinate> coordinates = new List<Coordinate>();

    public Frontier(Coordinate center, List<Coordinate> list)
    {
        this.centerPt = center;
        this.coordinates = list;
    }

    public Coordinate GetCenter()
    {
        return this.centerPt;
    }

    public List<Coordinate> GetList()
    {
        return this.coordinates;
    }

    public override string ToString()
    {
        if (coordinates != null)
        {
            string list = "{    ";
            foreach (Coordinate elem in coordinates)
            {
                list += elem.ToString() + ", ";
            }
            list += "   }";

            return centerPt.ToString() + "  :  " + list;
        }
        else
            return centerPt.ToString() + "  :  null";
    }

    public bool IsFrontier(int row, int col)
    {
        return (this.centerPt.GetRow() == row) && (this.centerPt.GetCol() == col);
    }

    public int getInfoGain()
    {
        return this.coordinates.Count * 10;
    }
}



public class GetFrontier : MonoBehaviour {


    private char[,] map;
    private MapCreator2 mapCreator;

    private List<Coordinate> points = new List<Coordinate>();
    private List<Frontier> frontiers = new List<Frontier>();
    private int[] targetFrontier; 

    private bool complete = true;

    public float penaltyWall = 1f;

    void Start()
    {
        mapCreator = GetComponent<MapCreator2>();
    }

    // Starting from the position of the player to check all the position of reachable unknown cell 
    private void GetCoordinates()
    {       
        map = mapCreator.GetMap();
        
        VerityNeighbor(mapCreator.GetPlayerRow(), mapCreator.GetPlayerCol());
    }

    // Propagate the search until bump into obstacles or unknown cell
    private void VerityNeighbor(int row, int col)
    {
        if (row < 0 || row > map.GetUpperBound(0))
            return;
        if (col < 0 || col > map.GetUpperBound(1))
            return;

        if (map[row, col] == Global.WallChar || map[row, col] == Global.target ||
            map[row, col] == Global.accessable || map[row, col] == Global.frontier || map[row, col] == Global.player)
            return;

        // if we find the unknown char that means we need to do convert into frontier, so add it into list of coordinate first
        if (map[row, col] == Global.UnknownChar)
        {
            map[row, col] = Global.frontier;
            points.Add(new Coordinate(row, col));
            complete = false;

            return;
        }
        if (map[row, col] == Global.PlayerChar)
            map[row, col] = Global.player;

        if (map[row, col] == Global.FloorChar)
            map[row, col] = Global.accessable;

        if (map[row, col] == Global.TargetChar)
            map[row, col] = Global.target;

        VerityNeighbor(row - 1, col);
        VerityNeighbor(row + 1, col);
        VerityNeighbor(row, col - 1);
        VerityNeighbor(row, col + 1);
    }

    // We will use Euclidean distance to compute the result
    private float ComputeDistance(Coordinate first, Coordinate second)
    {
        float diffRow = first.GetRow() - second.GetRow();
        float diffCol = first.GetCol() - second.GetCol();
        return Mathf.Sqrt(Mathf.Pow(diffRow, 2) + Mathf.Pow(diffCol, 2));
    }


    // we check all elements in lower triangular matrix, find the min element lower than max_distance allowable 
    // and the sum of two group is less than max_Coordinate allowable
    private int[] FindMinElem(float[,] matrix, Dictionary<Coordinate, List<Coordinate>> dictionary)
    {
        float min = float.MaxValue;
        float cur;
        int row = 0, col = 0;
        bool checkpoint = false; // check if the min elem is actually found

        for(int i = 0; i < matrix.GetUpperBound(0) + 1; i++)
            for(int j = 0; j < i; j++)
            {
                cur = matrix[i, j];
                if(cur != 0 && cur < min && cur < Frontier.distance)
                {
                    if ((dictionary[points[i]].Count + dictionary[points[j]].Count) <= Frontier.Max_Coordinates)
                    {
                        min = cur;
                        row = i;
                        col = j;
                        checkpoint = true;
                    }
                }
            }

        if(checkpoint)
            return new int[] {row, col};
        return null;
    }

    // join the coordinates at first and second index of list of Coordinates,
    // the new matrix has dimension n-1 x n-1 
    // procedure : 
    //      remove the row and column at index indicated by parameter(2 row and 2 column)
    //      join the coodinates, remove them from list, and add new one with central point betweeen them, then update also dictionary  
    //      the last row of matrix is distance of new Coordinate to others
    private float[,] UpdateDistanceMatrix(int first, int second, float[,] matrix, Dictionary<Coordinate, List<Coordinate>> dictionary)
    {
        // new matrix with size n-1 x n-1
        float[,] tempMatrix = new float[matrix.GetUpperBound(0), matrix.GetUpperBound(1)];

        int shiftRow = 0, shiftCol = 0;

        Coordinate firstC = points[first];
        Coordinate secondC = points[second];
        List<float> minLinkage;
        // copy the all row and col different to that indicated
        for (int i = 0; i < matrix.GetUpperBound(0) + 1; i++)
        {
            if ((i == first || i == second) && i != matrix.GetUpperBound(0))
                shiftRow++;
            else
            {
                shiftCol = 0;
                for (int j = 0; j < matrix.GetUpperBound(1) + 1; j++)
                {
                    if (j == first || j == second)
                        shiftCol++;
                    else
                    {
                        tempMatrix[i - shiftRow, j - shiftCol] = matrix[i, j];

                        if (i == matrix.GetUpperBound(0))
                        {
                            minLinkage = new List<float> { matrix[first, j], matrix[second, j], matrix[j, first], matrix[j, second] };
                            tempMatrix[tempMatrix.GetUpperBound(0), j - shiftCol] = minLinkage.Where(x => x > 0).Min();
                        }
                    }
                }
            }         
        }

        float numFirstC = dictionary[firstC].Count;
        float numSecondC = dictionary[secondC].Count;

        Coordinate newCoordinate = new Coordinate(((numFirstC * firstC.GetRow() + numSecondC * secondC.GetRow()) / (numFirstC + numSecondC)), 
                                                   (numFirstC * firstC.GetCol() + numSecondC * secondC.GetCol()) / (numFirstC + numSecondC));


        points.Remove(firstC);
        points.Remove(secondC);
        points.Add(newCoordinate);

        dictionary.Add(newCoordinate, dictionary[firstC].Concat(dictionary[secondC]).ToList());
        dictionary.Remove(firstC);
        dictionary.Remove(secondC);

      
        return tempMatrix;
    }

    // Grouping the coordinate in frontier, based on the max distance and number of coordinates allowable
    public void Coordinate2Frontiers()
    {

        // ensure that  the main variablea are ready to use
        points.Clear();
        frontiers.Clear();
        complete = true;

        GetCoordinates();

        if (!complete)
        {
            // dictionary that keep the group of coordinates and the number of the its menber.
            Dictionary<Coordinate, List<Coordinate>> groupCoordinate = new Dictionary<Coordinate, List<Coordinate>>();

            // matrix with distance between the Coordinate as element
            float[,] distanceMatrix = new float[points.Count, points.Count];

            bool next = true;

            // initialize the dictionary 
            foreach (Coordinate elem in points)
            {
                groupCoordinate.Add(elem, new List<Coordinate> { elem });
            }

            // initialize the matrix of distance with 
            for (int i = 0; i < points.Count; i++)
                for (int j = 0; j < i; j++)
                {
                    distanceMatrix[i, j] = ComputeDistance(points[i], points[j]);
                }

            // the process work until there are not any
            while (next)
            {
                int[] ii = FindMinElem(distanceMatrix, groupCoordinate);
                if (ii != null)
                    distanceMatrix = UpdateDistanceMatrix(ii[0], ii[1], distanceMatrix, groupCoordinate);
                else
                    next = false;
            }

            // put into the list of frontier the nearest coordinate to the center of group, with the list contained
            foreach (Coordinate key in groupCoordinate.Keys)
            {
                float oldDiff = Frontier.distance;
                Coordinate nearestCoord = new Coordinate(key);

                foreach(Coordinate elem in groupCoordinate[key])
                {
                    if(ComputeDistance(key, elem) < oldDiff)
                    {
                        nearestCoord = new Coordinate(elem);
                        oldDiff = ComputeDistance(key, elem);
                    }
                }
                Frontier frontier = new Frontier(nearestCoord, groupCoordinate[key]);
                frontiers.Add(frontier);
            }

        }
    }

    private Vector3 Coordinate2Vector3(Coordinate coord)
    {
        return new Vector3(coord.GetCol(), 0, coord.GetRow());
    }

    // Check wall in the linear path from origin to destination, and return the number of times. 
    public int CheckNumberWall(Vector3 origin, Vector3 des)
    {
        Vector3 middlePoint;

        bool isWall = false;
        int times = 0;

        // relation between length of segment given origin and destination points and its unit verctor 
        float interpolant = Vector3.Magnitude(Vector3.Normalize(des - origin)) / Vector3.Magnitude(des - origin);

        for (float x = 0f; x * interpolant < 1f; x += 1f)
        {
            middlePoint = Vector3.Lerp(origin, des, x * interpolant);

            if ((map[(int)middlePoint.z, (int)middlePoint.x] == Global.WallChar))
            {
                if (!isWall)
                {
                    isWall = true;
                    times++;
                }
            }
            else
                isWall = false;
        }

        return times;
    }


    private void FindMostProbNextFrontier()
    {
        Coordinate playerCoodinate = new Coordinate(mapCreator.GetPlayerRow(), mapCreator.GetPlayerCol());
        float linearDistance, nearestDistance = ComputeDistance(new Coordinate(0, 0), new Coordinate(map.GetUpperBound(0), map.GetUpperBound(1)));
        float temp;
        Coordinate destination = new Coordinate(playerCoodinate);

        foreach(Frontier frontier in frontiers)
        {
            linearDistance = ComputeDistance(playerCoodinate, frontier.GetCenter());
            temp = linearDistance + 
                penaltyWall * CheckNumberWall(Coordinate2Vector3(playerCoodinate), Coordinate2Vector3(frontier.GetCenter()));

            if(temp < nearestDistance)
            {
                nearestDistance = temp;
                destination = frontier.GetCenter();
            }      
        }

        this.targetFrontier = new int[] {(int)destination.GetRow(), (int)destination.GetCol() };
    }

    private void SaveMapAsText(string path)
    {
        string textMap = "";

        for (int i = 0; i < map.GetUpperBound(0) + 1; i++)
        {
            for (int j = 0; j < map.GetUpperBound(1) + 1; j++)
            {
                textMap += map[i, j];
            }
            if (i < map.GetUpperBound(0))
                textMap += "\n";
        }

        File.WriteAllText(path + "/" + Time.time.ToString() + "Movement.txt", textMap);
    }


    public List<Frontier> GetListFrontier()
    {
        return new List<Frontier>(frontiers);
    }

    public int[] GetPlayerPosition()
    {
        return new int[] {mapCreator.GetPlayerRow(), mapCreator.GetPlayerCol()};
    }

    public int[] GetTargetFrontier()
    {
        FindMostProbNextFrontier();
        return this.targetFrontier;
    }

    public char[,] GetMap()
    {
        Coordinate2Frontiers();
        SaveMapAsText(Global.savePath);
        return map;
    }

}



