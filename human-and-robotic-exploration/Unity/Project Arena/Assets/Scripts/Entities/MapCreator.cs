using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class MapCreator : MonoBehaviour {

    public float viewAngle = 120f;
    public float viewRadius = 2f;
    public int viewStep = 120;

    // Variables that denote the probability of certainty of finding an object
    public float maxRayCertainty = 10;
    public float minRayCertainty = 1;
    public float threshold = 50;

    public float timeSaveTxtMap = 10;

    [Header("Representation")]
    // Char that denotes a room
    [SerializeField]
    protected static char roomChar = 'r';
    // Char that denotes a wall;
    [SerializeField]
    protected static char wallChar = 'w';
    // Char that denotes a unknown field
    [SerializeField]
    protected static char unknownChar = 'u';
    // Char that denotes the player
    [SerializeField]
    protected static char playerChar = 'p';
    // Char that denotes the target
    [SerializeField]
    protected static char targetChar = 't';


    private Light viewLight;
    private SphereCollider viewCollider;

    private Vector3 previousPosition;       // save the previous position of player to compare the movement
    private float previousTime;

    private static string floorString = "Floor";
    private static string wallString = "Wall";
    private static string targetString = "Target";
    private static string unknownString = "Unknown";

    private Dictionary<string, char> elementStringToChar = new Dictionary<string, char>()
    {
        {targetString, targetChar},
        {wallString, wallChar},
        {floorString, roomChar},
        {unknownString, unknownChar}
    };

    private char[,] map;        // array to memorize the map in grid
    private int row;
    private int column;


    // Use this for initialization
    void Start () {

        viewCollider = GetComponent<SphereCollider>();
        viewCollider.radius = viewRadius;

        viewLight = GetComponentInChildren<Light>();
        viewLight.range = viewRadius;
        viewLight.spotAngle = viewAngle;

        previousPosition = transform.position;
        previousTime = Time.time;

        CreateMap();
        CreateFieldView();

    }

    private void Update()
    {

    }

    void LateUpdate()
    {

        int horizontalDev = ComputeShiftCell(previousPosition.x, transform.position.x);
        int verticalDev = ComputeShiftCell(previousPosition.z, transform.position.z);

        if (horizontalDev != 0 || verticalDev != 0)
        {
            UpdateMap_PlayerPosition(horizontalDev, verticalDev);
            previousPosition = transform.position;
        }

        CreateFieldView();

        if (Time.time - previousTime > timeSaveTxtMap)
        {
            Debug.Log("time");
            SaveMapAsText();
            previousTime = Time.time;
        }
    }


    // Display the lines which simulate the field of view and update the grid map using them 
    private void CreateFieldView()
    {
        float levelCertainty = 0;
        string objectFound;     // string of object that is hit by ray
        string mostProbObject;
        char objectCharInMap = '0';       // save the char that denotes the specified object

        // By default the first element is unknownString
        Dictionary<string, float> elements = new Dictionary<string, float>()
        {
            {unknownString, 0f},
            {floorString, 0f},
            {wallString, 0f},
            {targetString, 0f}
        };

        Vector3 previousRayPos = transform.position;

        // Get the leftmost ray
        Vector3 forward_left = Quaternion.Euler(0f, -viewAngle / 2, 0f) * transform.forward * viewRadius;

        for (int i = 0; i < viewStep; i++)
        {
            // Get the terminal point of view assuming without obstacles 
            Vector3 dir = Quaternion.Euler(0f, (viewAngle / viewStep) * i, 0f) * forward_left;
            Vector3 pos = transform.position + dir + viewCollider.center;
            // Set the origin point where ray is launched 
            Vector3 origin = transform.position + viewCollider.center;


            // Generate a ray to detect the object
            Ray ray = new Ray(origin, dir);
            RaycastHit rayHit = new RaycastHit();
            int mask = LayerMask.GetMask(floorString, wallString, targetString);
            Physics.Raycast(ray, out rayHit, viewRadius, mask);


            if (rayHit.transform != null)
            {
                pos = rayHit.point;
                objectFound = rayHit.transform.gameObject.tag;
            }
            else
                objectFound = floorString;
            

            // Save the first end-of-ray's position 
            if (i == 0)
                previousRayPos = pos;
            else
            {

                int horizontalShift = ComputeShiftCell(origin.x, pos.x);
                int verticalShift = ComputeShiftCell(origin.z, pos.z);

                // compute the level of certainty add to the correspond value the dictionary if two rays are stayed in the same cell 
                if (ComputeShiftCell(previousRayPos.x, pos.x) == 0 && ComputeShiftCell(previousRayPos.z, pos.z) == 0 && i != viewStep - 1)
                {
                    objectCharInMap = GetMapElementFromPlayer(horizontalShift,verticalShift);
                    
                    //assuming that wall elements are unchangeable and the player's cell is 
                    if(objectCharInMap != wallChar && objectCharInMap != playerChar)
                    {                       
                        levelCertainty += ComputeRayCertainty(origin, pos);

                        elements[objectFound] += levelCertainty ;
                      
                    }
                }
                else
                {                  
                    // Once it has changed the cell, we get the string with the higest value of certainty
                    mostProbObject = elements.Where(elem => elem.Value == elements.Values.Max()).First().Key;

                    // Change the element in the grid map only if it's different from initial one which is not wallchar, targetchar, playerchar
                    if(elementStringToChar[mostProbObject] != objectCharInMap 
                        || objectCharInMap != wallChar 
                        || objectCharInMap != targetChar
                        || objectCharInMap != playerChar)
                    {
                        if(elements[mostProbObject] > threshold)
                        {
                            int previousHorizontalShift = ComputeShiftCell(origin.x, previousRayPos.x);
                            int previousVerticalShift = ComputeShiftCell(origin.z, previousRayPos.z);

                            UpdateMap_ObjectPosition(elementStringToChar[mostProbObject], previousHorizontalShift, previousVerticalShift);
                            FillCellBetweenPlayer(previousHorizontalShift, previousVerticalShift);
                        }
                    }

                    List<string> keys = new List<string>(elements.Keys);
                    foreach (string key in keys)
                        elements[key] = 0f;

                    previousRayPos = pos;
                }
            }

            // Draw the line in the console from the centre of sphereColleder to the point of rayhit
            Debug.DrawLine(origin, pos, Color.red);

        }
    }

    // if the parameter is more than 0, we just want to keep integer part, otherwise we must displace with one integer smaller.
    private int ConvertToInt(float n)
    {
        if (n >= 0)
            return (int)n;
        else
            return (int)n - 1;
    }

    // Compute the shift of grid in map according to the distance of integer difference
    private int ComputeShiftCell(float origin, float des)
    {
        return ConvertToInt(des) - ConvertToInt(origin);
    }


    // Compute the level of certainty of ray propositional to the distance , return the max if the end is closed to the origin
    private float ComputeRayCertainty(Vector3 origin, Vector3 end)
    {
        float distance = Vector3.Distance(origin, end);
        float norm = distance / viewRadius;
        float sensitivity = maxRayCertainty - minRayCertainty;

        return minRayCertainty + (1 - norm) * sensitivity;
    }

    // Initialize the map 2D array with unknown field and player position placed in the center 
    private void CreateMap()
    {
        if (map == null)
        {
            map = new char[5, 5];

            row = map.GetUpperBound(0) + 1;
            column = map.GetUpperBound(1) + 1;

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    map[i, j] = unknownChar;
                }
            }
            map[row / 2, column / 2] = playerChar;
        }
    }

    // Set the objectChar to the specific position
    private void UpdateMap_ObjectPosition(char objectChar, int horizontalShift, int verticalShift)
    {
        for(int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                if(map[i, j] == playerChar)
                {
                    map[i - verticalShift, j + horizontalShift] = objectChar;
                }
            }
        }
    }

    // Updata the player's position using the shift from the previous one
    private void UpdateMap_PlayerPosition(int horizontalShift, int verticalShift)
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                if (map[i, j] == playerChar)
                {
                    // Clear the previous cell
                    map[i, j] = roomChar;

                    int row_maxindex = map.GetUpperBound(0);
                    int col_maxindex = map.GetUpperBound(1);

                    // Horizontal direction is equal to increase of map's index
                    int horizontalDistance = j + horizontalShift;
                    // Vertical direction is opposite to index
                    int verticalDistance = i - verticalShift;

                    // Update the map to fit the new size if player is moved into a new area
                    // Update also the index after adding a new row/column which modify the previous index
                    if (horizontalDistance < 0)
                    {
                        AddMapColunm(horizontalDistance);
                        j -= horizontalDistance;
                    }
                    if (horizontalDistance > col_maxindex)
                        AddMapColunm(horizontalDistance - col_maxindex);
                    
                    if (verticalDistance < 0)
                    {
                        AddMapRow(verticalDistance);
                        i -= verticalDistance;
                    }
                    if (verticalDistance > row_maxindex)
                        AddMapRow(verticalDistance - row_maxindex);

                    map[i - verticalShift, j + horizontalShift] = playerChar;

                    return;
                }
            }
        }
    }


    
    // get the element of map considering center as player position, x and z is its coordinate 
    // add new row and/or column if the initial map has not cell needed
    private char GetMapElementFromPlayer(int horizontalShift, int verticalShift)
    {

        for(int i = 0; i < row; i++)
        {
            for(int j = 0; j < column; j++)
            {
                if(map[i, j] == playerChar)
                {
                    // Horizontal direction is equal to increase of map's index
                    int horizontalDistance = j + horizontalShift;
                    // Vertical direction is opposite to index
                    int verticalDistance = i - verticalShift;

                    // Update the map to fit the new size
                    // Update also the index after adding a new row/column which modify the previous index
                    if (horizontalDistance < 0)
                    {
                        AddMapColunm(horizontalDistance);
                        j -= horizontalDistance;
                    }
                    if (horizontalDistance > column - 1)
                    {
                        AddMapColunm(horizontalDistance - column + 1);
                    }

                    if (verticalDistance < 0)
                    {
                        AddMapRow(verticalDistance);
                        i -= verticalDistance;
                    }
                    if (verticalDistance > row -1)
                    {
                        AddMapRow(verticalDistance - row + 1);
                    }

                    return map[i - verticalShift, j + horizontalShift];
                }
            }
        }
        return unknownChar;
    }

    // Fill Unknown cells between the player and given shift as RoomChar as RoomChar
    private void FillCellBetweenPlayer(int horizontalShift, int verticalShift)
    {
        for(int i = 0; i < row; i++)
        {
            for(int j = 0; j < column; j++)
            {
                if(map[i, j] == playerChar)
                {
                    while(horizontalShift != 0 && verticalShift != 0)
                    {
                        if (horizontalShift > 0)
                            horizontalShift -= 1;
                        if (horizontalShift < 0)
                            horizontalShift += 1;
                        if (verticalShift > 0)
                            verticalShift -= 1;
                        if (verticalShift < 0)
                            verticalShift += 1;

                        if (map[i - verticalShift, j + horizontalShift] == unknownChar)
                            map[i - verticalShift, j + horizontalShift] = roomChar;
                    }
                }
            }
        }
    }

    // Add a new column to the map. If n is positive, then we add to the bottom, otherwise to the top; 
    private void AddMapColunm(int n)
    {
        char[,] tempMap = new char[row, column + Mathf.Abs(n)];
        int tempRow = tempMap.GetUpperBound(0) + 1;
        int tempCol = tempMap.GetUpperBound(1) + 1;

        if (n < 0)
        {
            for (int i = 0; i < tempRow; i++)
                for (int j = 0; j < tempCol; j++)
                {
                    if (j < -n)
                        tempMap[i, j] = unknownChar;
                    else
                        tempMap[i, j] = map[i, j + n];
                }
        }
        else
        {
            for (int i = 0; i < tempRow; i++)
                for (int j = 0; j < tempCol; j++)
                {
                    if (j >= column)
                        tempMap[i, j] = unknownChar;
                    else
                        tempMap[i, j] = map[i, j];
                }
        }

        map = tempMap;
        column += Mathf.Abs(n);

        //Debug.Log("add a column in " + n + "    and I have " + column + " columns");
    }

    // Add a new row to the map. If n is positive, then we add to the bottom, otherwise to the top; 
    private void AddMapRow(int n)
    {
        char[,] tempMap = new char[row + Mathf.Abs(n), column];
        int tempRow = tempMap.GetUpperBound(0) + 1;
        int tempCol = tempMap.GetUpperBound(1) + 1;

        if (n < 0)
        {
            for (int i = 0; i < tempRow; i++)
                for (int j = 0; j < tempCol; j++)
                {
                    if (i < -n)
                        tempMap[i, j] = unknownChar;
                    else
                        tempMap[i, j] = map[i + n, j];
                }
        }
        else
        {
            for (int i = 0; i < tempRow; i++)
                for (int j = 0; j < tempCol; j++)
                {
                    if (i < row)
                        tempMap[i, j] = map[i, j];
                    else
                        tempMap[i, j] = unknownChar;
                }
        }

        map = tempMap;
        row +=  Mathf.Abs(n);

        //Debug.Log("add a row in " + n + "   and I have " + row + " rows.");
    }

    // Before save map as text file, we add edge to guarantee the presence of unknown chacter.
    private void AddMapEdge()
    {
        int i;
        bool addRowColumn = false;

        // check and add row at the top
        i = 0;
        for (int j = 0; j < column; j++)
            if (map[i, j] != unknownChar)
                addRowColumn = true;
        if (addRowColumn)
            AddMapRow(-1);
        addRowColumn = false;

        // check and add column at the left
        for (int j = 0; j < row; j++)
            if (map[j, i] != unknownChar)
                addRowColumn = true;
        if (addRowColumn)
            AddMapColunm(-1);
        addRowColumn = false;

        // check and add row at to bottom
        i = row -1;
        for (int j = 0; j < column; j++)
            if (map[i, j] != unknownChar)
                addRowColumn = true;
        if (addRowColumn)
            AddMapRow(1);
        addRowColumn = false;

        // check and add column at right
        i = column - 1;
        for (int j = 0; j < row; j++)
            if (map[j, i] != unknownChar)
                addRowColumn = true;
        if (addRowColumn)
            AddMapColunm(1);
    }


    private void SaveMapAsText()
    {
        string textMap = "";
        //string path = "C:/Users/zhan yuan/Desktop/polimi/magistrale/Tesi/Text";

        AddMapEdge();

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                textMap += map[i, j];
            }
            if (i < row - 1)
                textMap += "\n";
        }

        //File.WriteAllText(path + "/" + Time.time.ToString() + ".txt", textMap);
    }


    public char[,] GetMap()
    {
        return (char[,])map.Clone();
    }

    public char getPlayerChar()
    {
        return playerChar;
    }

    public char getWallChar()
    {
        return wallChar;
    }

    public char getRoomChar()
    {
        return roomChar;
    }

    public char getTargetChar()
    {
        return targetChar;
    }

    public char getUnknownChar()
    {
        return unknownChar;
    }
}
