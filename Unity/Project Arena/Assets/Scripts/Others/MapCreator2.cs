using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// the set of static variables
public static class Global
{
    // set of string of mask
    public static string UnknownString = "Unknow";
    public static string WallString = "Wall";
    public static string FloorString = "Floor";
    public static string TargetString = "Target";
    public static string PlayerString = "Player";

    // set of chat corresponding the string
    public static char UnknownChar = 'u';    // Char that denotes a unknown field
    public static char WallChar = 'w';       // Char that denotes a wall;
    public static char FloorChar = 'r';      // Char that denotes a room
    public static char TargetChar = 't';     // Char that denotes the target
    public static char PlayerChar = 'p';     // Char that denotes the player

    public static char accessable = '1';
    public static char frontier = 'f';
    public static char player = 'P';
    public static char target = 'T';

    // dictionary to check the relation between the string and char for elements of map
    public static Dictionary<string, char> elementStringToChar = new Dictionary<string, char>()
    {
        {TargetString, TargetChar},
        {WallString, WallChar},
        {FloorString, FloorChar},
        {UnknownString, UnknownChar}
    };

    public static string savePath = "C:/map";
}


public class CellComponent
{
    // dictionary that accumulate the value of each element detected during the movement
    private Dictionary<string, float> elements = new Dictionary<string, float>()
    {
        {Global.UnknownString, 0.1f},      // set unknow different to 0, in this way it is the max element by default, we can use it as threshold
        {Global.WallString, 0f},
        {Global.FloorString, 0f},
        {Global.TargetString, 0f}
    };

    string mostProbElement = "";

    private bool isPlayer = false;  
    private bool isChangeable = true;       // if the cell have already been considered as obstacle, then it is not able to be changed


    public void ModifyElement(string element, float change)
    {
        this.elements[element] += change;
    }

    public void SetIsPlayer(bool isPlayer)
    {
        this.isPlayer = isPlayer;
    }

    public void ComputeMostProbElement()
    {
        if (this.mostProbElement == "")
            this.mostProbElement = elements.Where(elem => elem.Value == elements.Values.Max()).First().Key;

        string temp = elements.Where(elem => elem.Value == elements.Values.Max()).First().Key;

        if (!(this.mostProbElement != Global.UnknownString && temp == Global.UnknownString))
            this.mostProbElement = temp;
        
    }
    
    public void ClearElements()
    {
        elements[Global.UnknownString] = 0.1f;
        elements[Global.WallString] = 0f;
        elements[Global.FloorString] = 0f;
        elements[Global.TargetString] = 0f;
    }

    public bool GetIsPlayer()
    {
        return this.isPlayer;
    }

    public bool GetIsChangeable()
    {
        return this.isChangeable;
    }

    public string GetMostProbElement()
    {
        if (this.isChangeable)
        {
            ComputeMostProbElement();
            if (this.mostProbElement == Global.WallString || this.mostProbElement == Global.TargetString)
                this.isChangeable = false;
        }

        return this.mostProbElement;
    }

    public override string ToString()
    {
        string result = "";

        foreach(KeyValuePair<string, float> elem in elements)
        {
            result +=elem.Key + " : " + elements[elem.Key] + Environment.NewLine;
        }

        result += "isPlayer : " + isPlayer;

        return result;
    }
}


public class MapCreator2 : MonoBehaviour {

    [Header("Map Size")]
    public bool isFixedSize = false;
    public int maxRow = 50;
    public int maxCol = 50;

    [Header("Views")]
    public float viewAngle = 90f;
    public float viewRadius = 5f;
    public int viewStep = 90;

    // Variables that denote the probability of certainty of finding an object
    public float maxRayCertainty = 1;
    public float minRayCertainty = 0.1f;

    private Light viewLight;

    private Vector3 prevPlayerPosition;

    private CellComponent[,] elementMap;
    private char[,] charMap;
    private int row;
    private int col;
    private int playerRow;
    private int playerCol;
    
    // Use this for initialization
    void Start () {
       
        // use viewlight to simulate the viewRay
        viewLight = GetComponentInChildren<Light>();
        viewLight.range = viewRadius;
        viewLight.spotAngle = viewAngle;

        //initialize the map
        CreateMap(maxRow, maxCol);

        prevPlayerPosition = transform.position;
       
    }

    private void Update()
    {
        // movement
    }

    private void LateUpdate()
    {
        int horizontalPlayerShift = ComputeShiftCell(prevPlayerPosition.x, transform.position.x);
        int verticalPlayerShift = ComputeShiftCell(prevPlayerPosition.z, transform.position.z);

        if (horizontalPlayerShift != 0 || verticalPlayerShift != 0)
        {
            UpdateMap_PlayerPosition(horizontalPlayerShift, verticalPlayerShift);
            prevPlayerPosition = transform.position;
        }

        /*
        if(Time.time - prevTime > scanTime)
        {
            ObserveEnviroment();
            prevTime = Time.time;
        }
        */

    }

    // Initialize the 2D array map, and set the player at the center position
    private void CreateMap(int row, int col)
    {
        if (elementMap == null)
        {
            elementMap = new CellComponent[row, col];

            this.row = elementMap.GetUpperBound(0) + 1;
            this.col = elementMap.GetUpperBound(1) + 1;

            for (int i = 0; i < row; i++)
                for (int j = 0; j < col; j++)
                    elementMap[i, j] = new CellComponent();

            elementMap[row / 2, col / 2].SetIsPlayer(true);

            this.playerRow = row / 2;
            this.playerCol = col / 2;
        }
    }

    // if the parameter is more than 0, we just want to keep integer part, otherwise we must displace with one integer smaller.
    private int ConvertToInt(float n)
    {
        if (n % 1 != 0)
            if (n >= 0)
                return (int)n;
            else
                return (int)n - 1;
        else
            return (int)n;
    }

    // Compute the shift of grid in map according to the distance of integer difference
    private int ComputeShiftCell(float origin, float des)
    {
        return ConvertToInt(des) - ConvertToInt(origin);
    }

    // Compute the level of certainty of ray propositional to the distance,
    // the closer to the player,the higher value returned
    private float ComputeRayCertainty(Vector3 origin, Vector3 end)
    {
        float distance = Vector3.Distance(origin, end);
        float norm = distance / viewRadius;
        float sensitivity = maxRayCertainty - minRayCertainty;

        return minRayCertainty + (1 - norm) * sensitivity;
    }

    // set an object with a specific value at cell away from player position
    private void UpdateMap_ObjectPosition(int horizontalShift, int verticalShift, string objectFound, float levelCertainty)
    {
        int horizontalPosition = this.playerCol + horizontalShift;
        int verticalPosition = this.playerRow - verticalShift;

        // Update the map to fit the new size if the position is outside from previous one 
        UpdateMap_Size(horizontalPosition, verticalPosition);

        CellComponent cell = elementMap[this.playerRow - verticalShift, this.playerCol + horizontalShift];
        if (cell.GetIsChangeable())
            cell.ModifyElement(objectFound, levelCertainty);

    }

    // Updata the player's position using the shift from the previous one
    private void UpdateMap_PlayerPosition(int horizontalShift, int verticalShift)
    {
        // Horizontal direction is equal to increase of map's index
        int horizontalPosition = this.playerCol + horizontalShift;
        // Vertical direction is opposite to index
        int verticalPosition = this.playerRow - verticalShift;

        // Update the map to fit the new size if the position is outside from previous one 
        UpdateMap_Size(horizontalPosition, verticalPosition);

        elementMap[this.playerRow, this.playerCol].SetIsPlayer(false);
        this.playerRow -= verticalShift;
        this.playerCol += horizontalShift;
        elementMap[this.playerRow, this.playerCol].SetIsPlayer(true);
    }

    // Fill cells between the player position and the end of view point with floor element
    private void FillCellBetweenPlayerObject(Vector3 origin, Vector3 des)
    {
        Vector3 middlePoint;
        int horizontalShift, verticalShift;
        float levelCertainty;

        // relation between length of segment given origin and destination points and its unit verctor 
        float interpolant = Vector3.Magnitude(Vector3.Normalize(des - origin)) / Vector3.Magnitude(des - origin);

        for (float x = 0f; x * interpolant < 1f; x += 0.5f)
        {
            middlePoint = Vector3.Lerp(origin, des, x * interpolant);
            horizontalShift = ComputeShiftCell(origin.x, middlePoint.x);
            verticalShift = ComputeShiftCell(origin.z, middlePoint.z);

            levelCertainty = 0.1f * ComputeRayCertainty(origin, middlePoint);

            UpdateMap_ObjectPosition(horizontalShift, verticalShift, Global.FloorString, levelCertainty);
        }
    }

    // Display the lines which simulate the field of view and update the grid map using them 
    public void ObserveEnviroment()
    {
        float levelCertainty = 0;
        string objectFound;     // string of object that is hit by ray


        Vector3 previousRayPos = transform.position;

        // Get the terminal of the leftmost ray
        Vector3 forward_left = Quaternion.Euler(0f, -viewAngle / 2, 0f) * transform.forward * viewRadius;

        for (int i = 0; i < viewStep; i++)
        {
            // Get the terminal point of view assuming without obstacles 
            Vector3 dir = Quaternion.Euler(0f, (viewAngle / viewStep) * i, 0f) * forward_left;
            Vector3 pos = dir + viewLight.transform.position;
            // Set the origin point where ray is launched 
            Vector3 origin = viewLight.transform.position;


            // Generate a ray to detect the object
            Ray ray = new Ray(origin, dir);
            RaycastHit rayHit = new RaycastHit();
            int mask = LayerMask.GetMask(Global.FloorString, Global.WallString, Global.TargetString);
            Physics.Raycast(ray, out rayHit, viewRadius, mask);


            // update the variable "pos" when a ray bump into an obstacle, and save the obstable found
            if (rayHit.transform != null)
            {
                pos = rayHit.point;
                objectFound = rayHit.transform.gameObject.tag;
            }
            else
                objectFound = Global.FloorString;

            int horizontalShift = ComputeShiftCell(origin.x, pos.x);
            int verticalShift = ComputeShiftCell(origin.z, pos.z);

            levelCertainty = ComputeRayCertainty(origin, pos);

            UpdateMap_ObjectPosition(horizontalShift, verticalShift, objectFound, levelCertainty);

            FillCellBetweenPlayerObject(transform.position, pos);

            // Draw the line in the console from the centre of sphereColleder to the point of rayhit
            Debug.DrawLine(origin, pos, Color.red);

        }
    }

    // Update the size of map to have enough space to contain the given position 
    private void UpdateMap_Size(int horizontalPosition, int verticalPosition)
    {
        if (horizontalPosition < 0)
            AddMapColunm(horizontalPosition);
        if (horizontalPosition > elementMap.GetUpperBound(1))
            AddMapColunm(horizontalPosition - elementMap.GetUpperBound(1));

        if (verticalPosition < 0)
            AddMapRow(verticalPosition);
        if (verticalPosition > elementMap.GetUpperBound(0))
            AddMapRow(verticalPosition - elementMap.GetUpperBound(0));
    }


    // Add a new column to the map. If n is positive, then we add to the right, otherwise to the left; 
    private void AddMapColunm(int n)
    {

        CellComponent[,] tempMap = new CellComponent[this.row, this.col + Mathf.Abs(n)];
        int tempRow = tempMap.GetUpperBound(0) + 1;
        int tempCol = tempMap.GetUpperBound(1) + 1;

        if (n < 0)
        {
            for (int i = 0; i < tempRow; i++)
                for (int j = 0; j < tempCol; j++)
                {
                    if (j < -n)
                        tempMap[i, j] = new CellComponent();
                    else
                        tempMap[i, j] = this.elementMap[i, j + n];
                }
            // update the player's coordinate 
            this.playerCol -= n;
        }
        else
        {
            for (int i = 0; i < tempRow; i++)
                for (int j = 0; j < tempCol; j++)
                {
                    if (j >= this.col)
                        tempMap[i, j] = new CellComponent();
                    else
                        tempMap[i, j] = this.elementMap[i, j];
                }
        }

        this.elementMap = tempMap;
        this.col += Mathf.Abs(n);

    }

    // Add a new row to the map. If n is positive, then we add to the bottom, otherwise to the top; 
    private void AddMapRow(int n)
    {
        CellComponent[,] tempMap = new CellComponent[this.row + Mathf.Abs(n), col];
        int tempRow = tempMap.GetUpperBound(0) + 1;
        int tempCol = tempMap.GetUpperBound(1) + 1;

        if (n < 0)
        {
            for (int i = 0; i < tempRow; i++)
                for (int j = 0; j < tempCol; j++)
                {
                    if (i < -n)
                        tempMap[i, j] = new CellComponent();
                    else
                        tempMap[i, j] = this.elementMap[i + n, j];
                }

            // update the player's coordinate 
            this.playerRow -= n;
        }
        else
        {
            for (int i = 0; i < tempRow; i++)
                for (int j = 0; j < tempCol; j++)
                {
                    if (i < this.row)
                        tempMap[i, j] = this.elementMap[i, j];
                    else
                        tempMap[i, j] = new CellComponent();
                }
        }

        this.elementMap = tempMap;
        this.row += Mathf.Abs(n);
    }

    // Before save map as text file, we add edge to guarantee the presence of unknown chacter.
    private void AddMapEdge()
    {
        int i;
        bool addRowColumn = false;

        // check and add row at the top
        i = 0;
        for (int j = 0; j < this.col; j++)
            if (this.elementMap[i, j].GetMostProbElement() != Global.UnknownString)
                addRowColumn = true;
        if (addRowColumn)
            AddMapRow(-1);
        addRowColumn = false;

        // check and add column at the left
        for (int j = 0; j < this.row; j++)
            if (this.elementMap[j, i].GetMostProbElement() != Global.UnknownString)
                addRowColumn = true;
        if (addRowColumn)
            AddMapColunm(-1);
        addRowColumn = false;

        // check and add row at to bottom
        i = this.row - 1;
        for (int j = 0; j < this.col; j++)
            if (this.elementMap[i, j].GetMostProbElement() != Global.UnknownString)
                addRowColumn = true;
        if (addRowColumn)
            AddMapRow(1);
        addRowColumn = false;

        // check and add column at right
        i = this.col - 1;
        for (int j = 0; j < this.row; j++)
            if (this.elementMap[j, i].GetMostProbElement() != Global.UnknownString)
                addRowColumn = true;
        if (addRowColumn)
            AddMapColunm(1);
    }

    private void Convert2CharMap()
    {
        String mostProbString;
        Char mostProbChar;
        
        if(!isFixedSize)
            AddMapEdge();

        charMap = new char[row, col];

        for (int i = 0; i < row; i++)
            for(int j = 0; j < col; j++)
            {
                mostProbString = elementMap[i, j].GetMostProbElement();
                mostProbChar = Global.elementStringToChar[mostProbString];

                if (elementMap[i, j].GetIsPlayer())
                    mostProbChar = Global.PlayerChar;

                elementMap[i, j].ClearElements();
                charMap[i, j] = mostProbChar;
            }
    }

    public String CheckStringAtPosition(int row, int col)
    {
        return elementMap[row, col].GetMostProbElement();
    }

    public bool CheckNeighborAtPositionIsUnknown(int row, int col)
    {
        bool result = false;
        for(int i = -1; i < 2; i++)
            for(int j  = -1; j < 2; j++)
            {
                if(elementMap[row + i, col + j].GetMostProbElement() == Global.UnknownString)
                    result = true;
            }

        return result;
    }

    public bool CheckLineOfSight(Vector3 direction, float distance)
    {

        Vector3 origin = new Vector3(this.playerCol, 0, this.playerRow);
        Vector3 target = origin + direction;
        Vector3 middlePoint;

        bool visible = true;

        // relation between length of segment given origin and destination points and its unit verctor 
        float interpolant = Vector3.Magnitude(Vector3.Normalize(direction)) / distance;

        for (float x = 0f; x * interpolant < 1f; x += 1f)
        {
            middlePoint = Vector3.Lerp(origin, target, x * interpolant);
         
            if (!elementMap[(int)middlePoint.z, (int)middlePoint.x].GetIsChangeable())
            {
                visible = false;
                break;
            }
        }

        return visible;
    }


    private void SaveMapAsText(string path)
    {
        Debug.Log("save map");
        string textMap = "";

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                textMap += charMap[i, j];
            }
            if (i < row - 1)
                textMap += "\n";
        }
        File.WriteAllText(path + "/" + Time.time.ToString() + ".txt", textMap);
    }

    public char[,] GetMap()
    {
        Convert2CharMap();
        return (char[,])this.charMap.Clone();
    }

    public int GetPlayerRow()
    {
        return this.playerRow;
    }

    public int GetPlayerCol()
    {
        return this.playerCol;
    }

}
