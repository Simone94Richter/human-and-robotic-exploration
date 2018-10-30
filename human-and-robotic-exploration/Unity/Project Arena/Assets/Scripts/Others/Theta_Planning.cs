using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Vertex : IComparable<Vertex>
{
    public int row, col;

    public float cost;
    public float heuristic_cost;
    public Vertex parent;
    public Vertex localParent;
    //public float lowerBound;
    //public float upperBound;

    public bool isWall;

    public Vertex(int row, int col)
    {
        this.row = row;
        this.col = col;

        cost = float.MaxValue;
        heuristic_cost = float.MaxValue;
        parent = null;
        localParent = null;
    }


    public int CompareTo(Vertex other)
    {
        if (this.cost < other.cost) return -1;
        else if (this.cost + this.heuristic_cost> other.cost + other.heuristic_cost) return 1;
        else return 0;
    }

    public override string ToString()
    {
        return this.row + "  " + this.col;
    }

    public override bool Equals(object obj)
    {
        Vertex other = obj as Vertex;

        if (other == null) return false;

        if (UnityEngine.Object.ReferenceEquals(this, obj))
            return true;

        if (this.row == other.row && this.col == other.col)
            return true;

        return false;
    }

    public override int GetHashCode()
    {
        var hashCode = 1083238465;
        hashCode = hashCode * -1521134295 + row.GetHashCode();
        hashCode = hashCode * -1521134295 + col.GetHashCode();
        hashCode = hashCode * -1521134295 + cost.GetHashCode();
        return hashCode;
    }
}


public class PriorityQueue<T> where T : IComparable<T>
{
    private List<T> data;

    public PriorityQueue()
    {
        this.data = new List<T>();
    }

    public void Enqueue(T item)
    {
        data.Add(item);
        int ci = data.Count - 1; // child index; start at end

        int upper = ci;
        int lower = 0;

        int pi = ci / 2;

        for(int i = 0; i < ci + 1; i++)
        {
            if (data[ci].CompareTo(data[pi]) >= 0)
            {
                lower = pi;
                pi = pi + ((upper - pi + 1) / 2);
            }
            else
            {
                upper = pi;
                if (upper - lower > 2)
                    pi = pi - ((pi - lower) / 2);
            }

            if (upper - lower <= 1)
                break;
        }

        data.Remove(item);
        data.Insert(pi, item);
    }

    public T TopElem()
    {
        return this.data[0];
    }

    public T Pop()
    {
        T first = this.data[0];
        this.data.Remove(first);
        return first;
    }

    public List<T> GetList()
    {
        return this.data;
    }
}


public class Theta_Planning : MonoBehaviour {

    private MapCreator mapCreator;
    private GetFrontier getFrontier;
    private Vertex[,] vertexMap;
    private char[,] map;
    private Vertex targetPosition;
    private Vertex playerPosition;

    private int mapRow, mapCol;

    private PriorityQueue<Vertex> open, closed;
    private List<Vector3> pathVector;
    private List<List<int>> pathCoordinate;

    // Use this for initialization
    private void Start () {
        mapCreator = GetComponent<MapCreator>();
        getFrontier = GetComponent<GetFrontier>();
    }

    public void StartPlanning()
    {
        Initialize();

        ComputeShortestPath();

        if (targetPosition.cost != float.MaxValue)
        {
            Debug.Log("path found");
            PathRecostruction();
        }
        else
            Debug.Log("no path found");

    }



    private void Initialize()
    {
        open = new PriorityQueue<Vertex>();
        closed = new PriorityQueue<Vertex>();
        pathVector = new List<Vector3>();
        pathCoordinate = new List<List<int>>();

        map = getFrontier.GetMap();

        mapRow = map.GetUpperBound(0);
        mapCol = map.GetUpperBound(1);
        int[] player = getFrontier.GetPlayerPosition();


        int[] target = getFrontier.GetTargetFrontier();

        // initialize the matrix of Vertex
        vertexMap = new Vertex[mapRow + 1, mapCol + 1];

        for (int i = 0; i < mapRow + 1; i++)
            for (int j = 0; j < mapCol + 1; j++)
            {
                vertexMap[i, j] = new Vertex(i, j);
                vertexMap[i, j].heuristic_cost = ComputeDistance(vertexMap[i, j], new Vertex(target[0], target[1]));
                if (map[i, j] == Global.WallChar)
                    vertexMap[i, j].isWall = true;
            }

        playerPosition = vertexMap[player[0], player[1]];
        targetPosition = vertexMap[target[0], target[1]];

        playerPosition.cost = 0;
        playerPosition.parent = playerPosition;
        playerPosition.localParent = playerPosition;

        open.Enqueue(playerPosition);
    }


    private void ComputeShortestPath()
    {

        Vertex expand;
        List<Vertex> neighbors = new List<Vertex>();

        while(open.GetList().Count != 0 && open.TopElem().cost + open.TopElem().heuristic_cost < targetPosition.cost + targetPosition.heuristic_cost)
        {
            expand = open.Pop();
            closed.Enqueue(expand);

            neighbors = FindVisibleNeighbor(expand);

            foreach(Vertex x in neighbors)
            {
                if (!closed.GetList().Contains(x))
                    UpdateVertex(expand, x);
            }
        }
    }

    private void UpdateVertex(Vertex expand, Vertex child)
    {
        float oldCost = child.cost;
        ComputeCost(expand, child);

        if(child.cost < oldCost)
        {
            if (open.GetList().Contains(child))
                open.GetList().Remove(child);
            open.Enqueue(child);
        }

    }

    private void ComputeCost(Vertex expand, Vertex child)
    {
        if (!child.isWall)
        {
            if (LineOfSight(expand.parent, child))
            {

                if (expand.parent.cost + ComputeDistance(expand.parent, child) < child.cost)
                {
                    child.parent = expand.parent;
                    child.cost = expand.parent.cost + ComputeDistance(expand.parent, child);
                    child.localParent = expand;
                }
            }
            else
            {
                if (expand.cost + ComputeDistance(expand, child) < child.cost)
                {
                    child.parent = expand;
                    child.cost = expand.cost + ComputeDistance(expand, child);
                    child.localParent = expand;
                }
            }
        }
        else
            child.parent = null;
    }

    private void PathRecostruction()
    {
        Vertex leaf = targetPosition;
        pathVector.Insert(0, new Vector3(leaf.col - leaf.parent.col, 0, -(leaf.row - leaf.parent.row)));
        pathCoordinate.Insert(0, new List<int> {leaf.row, leaf.col});
        
        while (leaf.parent != playerPosition)
        {
            leaf = leaf.parent;
            pathVector.Insert(0, new Vector3(leaf.col - leaf.parent.col, 0, -(leaf.row - leaf.parent.row)));
            pathCoordinate.Insert(0, new List<int> {leaf.row, leaf.col});
        }
    }


    private List<Vertex> FindVisibleNeighbor(Vertex point)
    {
        List<Vertex> neighbors = new List<Vertex>();


        for(int i = -1; i < 2; i++)
            for(int j = -1; j < 2; j++)
            {
                if(!(point.row + i < 0 || point.row + i > mapRow ||
                     point.col + j < 0 || point.col + j > mapCol))
                {
                    neighbors.Add(vertexMap[point.row + i, point.col + j]);
                }
            }
        return neighbors;
    }






    public void RePlanning()
    {
        ReInitialize();
        ReComputeShortestPath();

        if (targetPosition.cost != float.MaxValue)
        {
            Debug.Log("path found");
            //PathRecostruction();
            PathRecostructionWhenBlocked();
        }
        else
            Debug.Log("no path found");
    }

    private void ReInitialize()
    {
        open = new PriorityQueue<Vertex>();
        closed = new PriorityQueue<Vertex>();
        pathVector = new List<Vector3>();
        pathCoordinate = new List<List<int>>();

        map = getFrontier.GetMap();

        int[] player = getFrontier.GetPlayerPosition();
        playerPosition = vertexMap[player[0], player[1]];

        for (int i = 0; i < mapRow + 1; i++)
            for (int j = 0; j < mapCol + 1; j++)
            {
                vertexMap[i, j].cost = float.MaxValue;
                vertexMap[i, j].parent = null;

                if (map[i, j] == Global.WallChar)
                    vertexMap[i, j].isWall = true;
            }

        playerPosition.cost = 0;
        playerPosition.parent = playerPosition;
        playerPosition.localParent = playerPosition;

        open.Enqueue(playerPosition);
    }


    private void ReComputeShortestPath()
    {

        Vertex expand;
        List<Vertex> neighbors = new List<Vertex>();

        while (open.GetList().Count != 0 && open.TopElem().cost + open.TopElem().heuristic_cost < targetPosition.cost + targetPosition.heuristic_cost)
        {
            expand = open.Pop();
            closed.Enqueue(expand);

            neighbors = FindFourNeighbor(expand);

            foreach (Vertex x in neighbors)
            {
                if (!closed.GetList().Contains(x))
                    UpdateVertex(expand, x);
            }
        }
    }

    private void PathRecostructionWhenBlocked()
    {
        Vertex leaf = targetPosition;
        pathVector.Insert(0, new Vector3(leaf.col - leaf.localParent.col, 0, -(leaf.row - leaf.localParent.row)));
        pathCoordinate.Insert(0, new List<int> { leaf.row, leaf.col });

        while (leaf.localParent != playerPosition)
        {
            leaf = leaf.localParent;
            pathVector.Insert(0, new Vector3(leaf.col - leaf.localParent.col, 0, -(leaf.row - leaf.localParent.row)));
            pathCoordinate.Insert(0, new List<int> { leaf.row, leaf.col });
        }
    }

    private List<Vertex> FindFourNeighbor(Vertex point)
    {
        List<Vertex> neighbors = new List<Vertex>();

        for (int i = -1; i < 2; i++)
            for (int j = -1; j < 2; j++)
            {
                if (!(point.row + i < 0 || point.row + i > mapRow ||
                     point.col + j < 0 || point.col + j > mapCol))
                {
                    if(Mathf.Abs(i) != Mathf.Abs(j))
                        neighbors.Add(vertexMap[point.row + i, point.col + j]);
                }
            }

        return neighbors;
    }








    private float ComputeDistance(Vertex first, Vertex second)
    {
        float diffRow = first.row - second.row;
        float diffCol = first.col - second.col;
        return Mathf.Sqrt(Mathf.Pow(diffRow, 2) + Mathf.Pow(diffCol, 2));
    }

    private bool LineOfSight(Vertex start, Vertex end)
    {
        float lengthSight = mapCreator.viewRadius;
        if (ComputeDistance(start, end) > lengthSight)
            return false;
        else
        {
            Vector3 startVector = new Vector3(start.col, 0, start.row);
            Vector3 endVector = new Vector3(end.col, 0, end.row);

            if (getFrontier.CheckNumberWall(startVector, endVector) > 0)
                return false;
            else
                return true;
        }
    }

    private Vector3 Vertex2Vector(Vertex vertex)
    {
        return new Vector3(vertex.col, 0, vertex.row);
    }

    public List<Vector3> GetPath()
    {
        return new List<Vector3>(pathVector);
    }

    public List<List<int>> GetCoordinatePath()
    {
        return new List<List<int>>(pathCoordinate);
    }
}
