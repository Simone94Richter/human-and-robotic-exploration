using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class manages the core functionalities of the robot: it gives start to moving, map analyze, saving data and the other features provided
/// </summary>
public class RobotMain : Entity{

    [SerializeField] public Camera robotCamera;
    [SerializeField] public float speed = 10f;
    [SerializeField] public int numbRay;
    //[SerializeField] [Range(10f, 100f)] public float rotationSpeed;
    [SerializeField] [Range(0f, 1f)] public float angleRay;
    //[SerializeField] [Range(0f, 5f)] public float timeBetweenRays;
    //[SerializeField] private float gravity = 100f;
    [SerializeField] [Range(0f, 1000f)] public float rangeRays;
    //[SerializeField] [Range(1.0f, 10.0f)] public float rayCertainty = 8.0f;

    [Header("Is map to be analyzed numeric or char?")]
    public bool isNumeric;

    public bool noPath = false;

    [Header("Arbitrary float used to detect wall after a collision with one of them")]
    public float epsilon = 1f; //for now, 1 is the best

    [Header("Forgetting Factor")]
    public float forgettingFactor;

    [Header("Num paramters for numerical mapping")]
    public float numFreeCell = 0f;
    public float numWallCell = 1f;
    public float numUnknownCell = 2f;
    public float numGoalCell = 3f;

    [Header("Time for scan and decision making")]
    public float timeForScan;
    public float timeForDecision;

    public float penaltyCost;

    //--- Private and Protected Variables ---//
    
    protected bool targetFound; //is the objective detected by mapping?
    private bool isMultiTarget;
    private bool finished;

    private RaycastHit hit;

    private List<Ray> landingRay = new List<Ray>();
    private List<Vector3> posToReach = new List<Vector3>(); //list of frontier points
    private List<Vector3> route = new List<Vector3>();

    private float angle;
    private float dx; // delta between X coordinate of the hit point of the raycast sent by the robot agent and the X coordinate of the robot agent itself
    private float dz; // delta between Z coordinate of the hit point of the raycast sent by the robot agent and the Z coordinate of the robot agent itself
    private float finishingTime; //the time in seconds when the robot found the goal
    private float indexDistance;
    private float indexRangeRays;
    private float pointX; //X coordinate of a point along a ray
    private float pointZ; //Z coordinate of a point along a ray
    private float robotX; //X coordinate of the robot agent
    private float robotZ; //Z coordinate of the robot agent
    private float startingTime; //the time in seconds when the robot starts the simulation
    private float squareSize; //dimension of a cell of the floor, used in order to discretize the final map
    private float targetX;
    private float targetZ;
    private float wallPointX; //X coordinate of the collision point between the ray and a collider
    private float wallPointZ; //Z coordinate of the collision point between the ray and a collider

    private char[,] total_map; //the original map, passed by the system
    private float[,] numeric_total_map; //the original numeric map, passed by the system
    private char[,] robot_map; //the char map updated by the robot
    private float [,] numeric_robot_map; //the numeric map updated by the robot
    private float[,] forgettingCounterCell; //the number of seconds, or scans until putting again the cell as unknown

    private GameObject tempDestination; //the temp point to reach, expressed as a GameObject
    private GameObject destination; //the target, properly

    private int i, j;
    private int indexHeight;
    private int indexRays;
    private int indexWidth;
    private int width; //width of the map given by the manager to the robot
    private int height; //height og the map given by the manager to the robot

    private IEnumerator pointToReach;
    private IEnumerator sendingRays;

    private List<Vector3> goals; //list of frontier points
    protected Vector3 tempGoal; //Vector3 of the temp point to reach

    private Ray rayRight; //ray at the right-side of the robot agent
    private Ray rayLeft; //ray at the left-side of the robot agent

    private RobotConnection rC;
    private RobotDecisionMaking rDM;
    private RobotMovement rM;
    private RobotProgress rP;
    private RobotPlanning rPl;

    // Use this for initialization
    void Start()
    {
        finished = false;
        pointToReach = ChoosingPointToReach();
        sendingRays = SendingRays();
        //noPath = false;
        targetFound = false;
        rC = GetComponent<RobotConnection>();
        rDM = GetComponent<RobotDecisionMaking>();
        rM = GetComponent<RobotMovement>();
        rP = GetComponent<RobotProgress>();
        rPl = GetComponent<RobotPlanning>();
        rPl.range = rangeRays;
        landingRay.Add(new Ray(transform.position, Vector3.forward));
        angle = angleRay;

        InitializeRays();
    }

    // Update is called once per frame
    void Update()
    {
        if (targetFound)
        {
            rM.ApproachingGoal(destination);
        }
    }

    /// <summary>
    /// This method initialize the rays casted by the robot agent to scan the world around it
    /// </summary>
    private void InitializeRays()
    {
        for (indexRays = 1; indexRays < numbRay - 1; indexRays = indexRays + 2)
        {
            rayRight = new Ray(transform.position, Quaternion.AngleAxis(-angle, transform.up) * transform.forward);
            landingRay.Add(rayRight);

            rayLeft = new Ray(transform.position, Quaternion.AngleAxis(angle, transform.up) * transform.forward);
            landingRay.Add(rayLeft);

            angle = angle + angleRay;
        }
    }

    /// <summary>
    /// Method called by the GameManager to initializ the map of the robot (char case)
    /// </summary>
    /// <param name="map">the original map, held by the GameManaer</param>
    /// <param name="floorSquareSize">Parameter used to discretize the cell of the floor (from Unity world to robot map)</param>
    public void SetMap(char[,] map, float floorSquareSize)
    {
        squareSize = floorSquareSize;
        rPl.squareSize = floorSquareSize;
        rDM.SetSquareSize(floorSquareSize);
        total_map = map;
        robot_map = map;
        forgettingCounterCell = new float[map.GetLength(0), map.GetLength(1)];
        isNumeric = false;
        width = total_map.GetLength(0);
        height = total_map.GetLength(1);

        for (indexWidth = 0; indexWidth < width; indexWidth++)
        {
            for (indexHeight = 0; indexHeight < height; indexHeight++)
            {
                robot_map[indexWidth, indexHeight] = 'u';
            }
        }

        for (indexWidth = 0; indexWidth < width; indexWidth++)
        {
            for (indexHeight = 0; indexHeight < height; indexHeight++)
            {
                forgettingCounterCell[indexWidth, indexHeight] = -1;
            }
        }

        startingTime = Time.time;

        rPl.isNumeric = false;
        rPl.robot_map = robot_map;

        rDM.SetCharMap(robot_map);

        //rM.SetIsMultiTarget(isMultiTarget);

    }

    /// <summary>
    /// Method called by the GameManager to initializ the map of the robot (float case)
    /// </summary>
    /// <param name="map">the original map, held by the GameManaer</param>
    /// <param name="floorSquareSize">Parameter used to discretize the cell of the floor (from Unity world to robot map)</param>
    public void SetMap(float[,] map, float floorSquareSize)
    {
        squareSize = floorSquareSize;
        rPl.squareSize = floorSquareSize;
        rDM.SetSquareSize(floorSquareSize);
        numeric_total_map = map;
        numeric_robot_map = map;
        forgettingCounterCell = new float[map.GetLength(0), map.GetLength(1)];
        isNumeric = true;
        width = numeric_total_map.GetLength(0);
        height = numeric_total_map.GetLength(1);

        for (indexWidth = 0; indexWidth < width; indexWidth++)
        {
            for (indexHeight = 0; indexHeight < height; indexHeight++)
            {
                numeric_robot_map[indexWidth, indexHeight] = numUnknownCell;
            }
        }

        for (indexWidth = 0; indexWidth < width; indexWidth++)
        {
            for (indexHeight = 0; indexHeight < height; indexHeight++)
            {
                forgettingCounterCell[indexWidth, indexHeight] = -1; 
            }
        }

        startingTime = Time.time;

        rPl.isNumeric = true;
        rPl.numeric_robot_map = numeric_robot_map;

        rDM.SetNumericMap(numeric_robot_map);

        //rM.SetIsMultiTarget(isMultiTarget);

    }

    /*
    public void SetVariables()
    {
        rP = GetComponent<RobotProgress>();
        rPl = GetComponent<RobotPlanning>();
        rP.SetTimeDecision(timeForDecision);
        rP.SetTimeScan(timeForScan);
        rP.SetPenaltyCost(penaltyCost);
        rPl.penaltyCost = penaltyCost;

    }*/

    public void SetIsMultiTarget(bool boolean)
    {
        isMultiTarget = boolean;
        rM.SetIsMultiTarget(isMultiTarget);
    }

    // parte importata non rilevante


    public override bool Equals(object other)
    {
        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override void Heal(int restoredHealth)
    {
    }

    public override void Respawn()
    {
    }

    public override void SetInGame(bool b)
    {
        inGame = b;
    }

    public override void SetupEntity(int th, bool[] ag, GameManager gms, int id)
    {
        activeGuns = ag;
        gameManagerScript = gms;

        totalHealth = th;
        health = th;
        entityID = id;

        for (int i = 0; i < ag.GetLength(0); i++)
        {
            // Setup the gun.
        }
    }

    public override void SetupEntity(GameManager gms, int id)
    {
        SetupEntity(totalHealth, activeGuns, gms, id);
    }

    public override void SlowEntity(float penalty)
    {
    }

    public override void TakeDamage(int damage, int killerID)
    {
        if (inGame)
        {
            //nothing
        }
    }

    public override string ToString()
    {
        return base.ToString();
    }

    protected override void Die(int id)
    {
    }

    // parte rilevante per la tesi
    
    /// <summary>
    /// This method is called by the GameManager. It starts the countdown for the robot activation
    /// </summary>
    public void StartingCountDown()
    {
        StartCoroutine(WaitingBeforeAction());
    }

    /// <summary>
    /// This method starts all the coroutines of the robot. They are:
    /// - Saving progress locally 
    /// - Scanning the environment to get information and update, as following, its map
    /// - Starts decision making in order to decide the point to reach and how to reach it
    /// </summary>
    /// <returns></returns>
    protected IEnumerator WaitingBeforeAction()
    {
        yield return new WaitForSeconds(4f);
        StartCoroutine(SavingProgress());
        StartCoroutine(sendingRays);
        yield return new WaitForSeconds(2f);
        StartCoroutine(pointToReach);
    }

    /// <summary>
    /// Thies method is responsible to restart the Scanning & Decision phase if the agent, after reaching a target, has not finished the game session
    /// Used in Multi-Target Maps
    /// </summary>
    /// <returns></returns>
    protected IEnumerator RestartAfterReachingObjective()
    {
        sendingRays = SendingRays();
        pointToReach = ChoosingPointToReach();
        yield return new WaitForSeconds(1f);
        StartCoroutine(sendingRays);
        yield return new WaitForSeconds(1f);
        StartCoroutine(pointToReach);
    }

    /// <summary>
    /// This method sends raycast from itself with a certain range and space in order to get information from the environment.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator SendingRays()
    {
        while (!targetFound)
        {
            angle = angleRay;
            landingRay[0] = new Ray(transform.position, transform.forward);
            Debug.DrawRay(transform.position, transform.forward * rangeRays, Color.red, 2f);
            for (indexRays = 1; indexRays < numbRay - 1; indexRays = indexRays + 2) //y rimane invariato, bisogna spaziare intorno a x e z
            {
                rayRight = new Ray(transform.position, Quaternion.AngleAxis(-angle, transform.up) * transform.forward);
                landingRay[indexRays] = rayRight;
                Debug.DrawRay(transform.position, Quaternion.AngleAxis(-angle, transform.up) * transform.forward * rangeRays, Color.red, 2f);
                rayLeft = new Ray(transform.position, Quaternion.AngleAxis(angle, transform.up) * transform.forward);
                landingRay[indexRays + 1] = rayLeft;
                Debug.DrawRay(transform.position, Quaternion.AngleAxis(angle, transform.up) * transform.forward * rangeRays, Color.red, 2f);

                angle = angle + angleRay;
            }
            UpdatingSpace(landingRay);
            ForgivingMemory();
            yield return new WaitForSeconds(timeForScan);
        }
    }

    /// <summary>
    /// It assign a type to a tile in the robot's map. A tile can be : free (so the robot can pass on it without problems), adjacent a wall or containing partially 
    /// it (so the robot is discouraged to pass on it) or containing the goal object (so the robot get maximum priority to approach it)
    /// </summary>
    /// <param name="ray">The list of raycast sent by the robot during the scanning</param>
    private void UpdatingSpace(List<Ray> ray)
    {
        for (indexRays = 0; indexRays < numbRay - 1; indexRays++)
        {
            if (!Physics.Raycast(ray[indexRays], out hit, rangeRays))
            {
                //Debug.Log("Here!");
                for (indexRangeRays = rangeRays; indexRangeRays >= 0; indexRangeRays--)
                {
                    pointX = FixingRound(ray[indexRays].GetPoint(indexRangeRays).x / squareSize);
                    pointZ = FixingRound(ray[indexRays].GetPoint(indexRangeRays).z / squareSize);
                    //pointX = pointX / squareSize;
                    //pointZ = pointZ / squareSize;
                    //pointX = FixingRound(pointX);
                    //pointZ = FixingRound(pointZ);
                    if (!isNumeric && (int)pointX >= 0 && (int)pointX < width && (int)pointZ >= 0 && (int)pointZ < height)
                    {
                        if (robot_map[(int)pointX, (int)pointZ] != 'w') robot_map[(int)pointX, (int)pointZ] = 'r';
                    }
                    else if ((int)pointX >= 0 && (int)pointX < width && (int)pointZ >= 0 && (int)pointZ < height)
                    {
                        if (numeric_robot_map[(int)pointX, (int)pointZ] != numWallCell) numeric_robot_map[(int)pointX, (int)pointZ] = numFreeCell;
                    }
                    if((int)pointX >= 0 && (int)pointX < width && (int)pointZ >= 0 && (int)pointZ < height)
                        forgettingCounterCell[(int)pointX, (int)pointZ] = forgettingFactor;
                    //Debug.Log(numeric_robot_map[(int)x_coord, (int)y_coord] + "" + (int)x_coord + "" + (int)y_coord);
                }
            }
            else
            {
                robotX = hit.transform.position.x / squareSize; //nome variabile non consono?
                robotZ = hit.transform.position.z / squareSize; //nome variabile non consono?
                if (hit.collider.gameObject.tag == "Environment")
                {
                    //Debug.Log("Here!");
                    //Debug.Log("Environment hit!");
                    //Debug.Log("You hit " + hit.collider.gameObject.tag);
                    //Debug.Log(hit.collider.gameObject.transform.position);
                    //Debug.Log(hit.point);
                    dx = hit.point.x - this.gameObject.transform.position.x;
                    dz = hit.point.z - this.gameObject.transform.position.z;

                    SettingR(Mathf.Sqrt((dx*dx) + (dz*dz)), ray[indexRays]);
                    
                    SettingW(Mathf.Sqrt((dx * dx) + (dz * dz)), ray[indexRays], epsilon);
                    //Debug.Log(x + "" + y);
                }
                else if (hit.collider.gameObject.tag == "Target")
                {
                    //Debug.Log("Target found!");
                    targetFound = true;
                    rM.squareSize = squareSize;
                    destination = hit.collider.gameObject;
  
                    SettingR(Vector3.Distance(hit.collider.gameObject.transform.position, this.gameObject.transform.position), ray[indexRays]);

                    robotX = FixingRound(robotX);
                    robotZ = FixingRound(robotZ);
                    if (!isNumeric && (int)pointX >= 0 && (int)pointX < width && (int)pointZ >= 0 && (int)pointZ < height)
                        robot_map[(int)robotX, (int)robotZ] = 'g';
                    else numeric_robot_map[(int)robotX, (int)robotZ] = numGoalCell;
                    forgettingCounterCell[(int)robotX, (int)robotZ] = forgettingFactor;
                    //Debug.Log(x + "" + y);
                    //portarlo dritto al target
                }
            }

        }
        if (destination)
        {
            targetX = FixingRound(destination.transform.position.x/squareSize);
            targetZ = FixingRound(destination.transform.position.z/squareSize);
            if (!isNumeric && (int)pointX >= 0 && (int)pointX < width && (int)pointZ >= 0 && (int)pointZ < height)
                robot_map[(int)targetX, (int)targetZ] = 'g';
            else numeric_robot_map[(int)targetX, (int)targetZ] = numGoalCell;

            forgettingCounterCell[(int)targetX, (int)targetZ] = forgettingFactor;
        }

        if (!isNumeric)
        {
            rPl.isNumeric = false;      //questa chiamata è necessaria?
            rPl.robot_map = robot_map;
        }
        else
        {
            rPl.isNumeric = true;       //questa chiamata è necessaria?
            rPl.numeric_robot_map = numeric_robot_map;
        }
    }

    /// <summary>
    /// This method sets a tile as a free one, giving the dedicated char or float value to it in the robot's map.
    /// If the tile has been already set to a 'near wall' one, it cannot be set as a free one
    /// </summary>
    /// <param name="distance">The distance from the robot until the last point of the ray</param>
    /// <param name="ray">The ray along which is considering the tiles analyzed</param>
    protected void SettingR(float distance, Ray ray)
    {
        for (indexDistance = 0; indexDistance < distance; indexDistance++)
        {
            pointX = ray.GetPoint(indexDistance).x / squareSize;
            pointZ = ray.GetPoint(indexDistance).z / squareSize;
            pointX = FixingRound(pointX);
            pointZ = FixingRound(pointZ);
            if (!isNumeric && (int)pointX >= 0 && (int)pointX < width && (int)pointZ >= 0 && (int)pointZ < height && robot_map[(int)pointX, (int)pointZ] != 'w')
                robot_map[(int)pointX, (int)pointZ] = 'r';
            else if(isNumeric && (int)pointX >= 0 && (int)pointX < width && (int)pointZ >= 0 && (int)pointZ < height && numeric_robot_map[(int)pointX, (int)pointZ] != numWallCell) numeric_robot_map[(int)pointX, (int)pointZ] = numFreeCell;
            forgettingCounterCell[(int)pointX, (int)pointZ] = forgettingFactor;
        }
    }

    /// <summary>
    /// This method sets a tile as a near wall one, giving the dedicated char or float value to it in the robot's map
    /// </summary>
    /// <param name="distance">The distance from the robot until the last point of the ray</param>
    /// <param name="ray">The ray along which is considering the tiles analyzed</param>
    /// <param name="epsilon">Useless (is set to 0)</param>
    protected void SettingW(float distance, Ray ray, float epsilon/*, float angle*/)
    {
        /*robotX = transform.position.x / squareSize;
        robotZ = transform.position.z / squareSize;
        pointX = ray.GetPoint(distance).x / squareSize;
        pointZ = ray.GetPoint(distance).z / squareSize;

        robotX = FixingRound(robotX);
        robotZ = FixingRound(robotZ);
        pointX = FixingRound(pointX);
        pointZ = FixingRound(pointZ);*/
        
        //method 1
        wallPointX = FixingRound(ray.GetPoint(distance + epsilon).x / squareSize);
        wallPointZ = FixingRound(ray.GetPoint(distance + epsilon).z / squareSize);

        if (!isNumeric && (int)pointX >= 0 && (int)pointX < width && (int)pointZ >= 0 && (int)pointZ < height/*&& robot_map[(int)wallPointX,(int)wallPointZ] != 'r'*/)
            robot_map[(int)wallPointX, (int)wallPointZ] = 'w';
        else if(isNumeric && (int)pointX >= 0 && (int)pointX < width && (int)pointZ >= 0 && (int)pointZ < height /*&& numeric_robot_map[(int)wallPointX, (int)wallPointZ] != numFreeCell*/) numeric_robot_map[(int)wallPointX, (int)wallPointZ] = numWallCell;
        forgettingCounterCell[(int)wallPointX, (int)wallPointZ] = forgettingFactor;

        //method 2
        /*if (dx >= 0 && dz >= 0) //primo quadrante
        {
            if (robot_map[(int)x+1,(int)z] == 'u')
            {
                robot_map[(int)x + 1, (int)z] = 'w';
            }
            else if (robot_map[(int)x, (int)z+1] == 'u')
            {
                robot_map[(int)x, (int)z+1] = 'w';
            }
            if (robot_map[(int)x + 1, (int)z + 1] == 'u')
                robot_map[(int)x + 1, (int)z + 1] = 'w';
        }else if (dx >= 0 && dz <= 0) //quarto quadrante
        {
            if (robot_map[(int)x + 1, (int)z] == 'u')
            {
                robot_map[(int)x + 1, (int)z] = 'w';
            }
            else if (robot_map[(int)x, (int)z-1] == 'u')
            {
                robot_map[(int)x, (int)z-1] = 'w';
            }
            if (robot_map[(int)x + 1, (int)z - 1] == 'u')
                robot_map[(int)x + 1, (int)z - 1] = 'w';
        }
        else if (dx <= 0 && dz >= 0) //secondo quadrante
        {
            if (robot_map[(int)x-1, (int)z] == 'u')
            {
                robot_map[(int)x - 1, (int)z] = 'w';
            }
            else if (robot_map[(int)x, (int)z+1] == 'u')
            {
                robot_map[(int)x, (int)z + 1] = 'w';
            }
            if (robot_map[(int)x - 1, (int)z + 1] == 'u')
                robot_map[(int)x - 1, (int)z + 1] = 'w';
        }
        else if(dx <= 0 && dz <= 0)//terzo quadrante
        {
            if (robot_map[(int)x, (int)z - 1] == 'u')
            {
                robot_map[(int)x, (int)z - 1] = 'w';
            }
            else if (robot_map[(int)x-1, (int)z] == 'u')
            {
                robot_map[(int)x-1, (int)z] = 'w';
            }
            if (robot_map[(int)x - 1, (int)z - 1] == 'u')
                robot_map[(int)x - 1, (int)z - 1] = 'w';
        }*/
        /*int i = -1;
        while (i <= 1)
        {
            int j = -1;
            while (j <= 1)
            {
                char cell = robot_map[(int)(x+i),(int)(y+i)];
                if (cell == 'u') robot_map[(int)(x + i), (int)(y + i)] = 'w';
                j++;
            }
            i++;
        }*/
    }


    /// <summary>
    /// This method lowers by one the counter of the memory of a cell to be "resetted" to the standard value, that is the unknown one
    /// </summary>
    private void ForgivingMemory()
    {
        for (indexWidth = 0; indexWidth < width; indexWidth++)
        {
            for (indexHeight = 0; indexHeight < height; indexHeight++)
            {
                if (forgettingCounterCell[indexWidth, indexHeight] > -1)
                {
                    forgettingCounterCell[indexWidth, indexHeight]--;
                }

                if (forgettingCounterCell[indexWidth, indexHeight] == -1)
                {
                    if (isNumeric)
                    {
                        numeric_robot_map[indexWidth, indexHeight] = numUnknownCell;
                    }
                    else robot_map[indexWidth, indexHeight] = 'u';
                }
            }
        }
    }

    /// <summary>
    /// This method is used to decide which tile the robot has to reach. All the potential tiles are the one that are free and near an unknown one
    /// </summary>
    /// <returns></returns>
    public IEnumerator ChoosingPointToReach()
    {
        while (!targetFound)
        {
            //RemovingWrongDetection();
            DetectingFrontierPoints();

            //Debug.Log(posToReach.Count);
            if (posToReach.Count != 0)
            {
                goals = posToReach;
                tempGoal = rDM.PosToReach(goals);

                if (route != null)
                {
                    route.Clear();
                }

                route = rPl.CheckVisibility(transform.position, tempGoal);
                rM.tempReached = false;

                if (route == null && noPath)
                {
                    numeric_robot_map[(int)FixingRound(tempGoal.x / squareSize), (int)FixingRound(tempGoal.z / squareSize)] = numUnknownCell;
                    goals = posToReach;
                    tempGoal = rDM.PosToReach(goals);

                    if (route != null)
                    {
                        route.Clear();
                    }

                    route = rPl.CheckVisibility(transform.position, tempGoal);
                    rM.tempReached = false;
                }

                //Debug.Log(route);
                if (route == null)
                {
                    rM.ApproachingPointToReach(tempGoal);
                }
                else
                {
                    rM.ApproachingPointToReach(route, 0);
                }
            }
            else
            {
                transform.Rotate(Vector3.up * Time.deltaTime * 20f);
            }

            yield return new WaitForSeconds(timeForDecision);
        }
    }

    private void RemovingWrongDetection()
    {
        width = numeric_robot_map.GetLength(0);
        height = numeric_robot_map.GetLength(1);

        for (i = 0; i < width; i++)
        {
            for (j = 0; j < height; j++)
            {
                if (numeric_robot_map[i, j] == numFreeCell && i + 1 < width && i - 1 >= 0 && j + 1 < height && j - 1 >= 0)
                {
                    if (numeric_robot_map[i + 1, j] != numFreeCell && numeric_robot_map[i - 1, j] != numFreeCell && numeric_robot_map[i, j + 1] != numFreeCell 
                        && numeric_robot_map[i, j - 1] != numFreeCell)
                    {
                        numeric_robot_map[i, j] = numUnknownCell;
                    }
                }
            }
        }
    }

    /// <summary>
    /// This method searches around the map created by the robot agent all the points that can be classified ad "frontier" points, adding them in a dedicated list
    /// </summary>
    private void DetectingFrontierPoints()
    {
        posToReach.Clear();

        if (!isNumeric)
        {
            DFPinCharMap();
        }
        else
        {
            DFPinNumMap();
        }

        bool somethingToDelete = false;
        //remove the point where the agent stands (because sometimes it could be a frontier point)
        for (int i  = 0; i < posToReach.Count; i++)
        {
            int posToReachX = (int)FixingRound(posToReach[i].x / squareSize);
            int posToReachZ = (int)FixingRound(posToReach[i].z / squareSize);
            int robotX = (int)FixingRound(transform.position.x / squareSize);
            int robotZ = (int)FixingRound(transform.position.z / squareSize);
            if (posToReachX == robotX && posToReachZ == robotZ)
            {
                somethingToDelete = true;
                //Debug.Log("match");
            }
        }

        if (somethingToDelete)
        {
            Vector3 posToDelete = new Vector3(robotX * squareSize, transform.position.y, robotZ * squareSize);
            posToReach.Remove(posToDelete);
        }
    }

    /// <summary>
    /// This method is responsible for detecting points that can be targeted as frontier points. Char Map case only
    /// </summary>
    private void DFPinCharMap()
    {
        width = robot_map.GetLength(0);
        height = robot_map.GetLength(1);
        for (i = 0; i < width; i++)
        {
            for (j = 0; j < height; j++)
            {
                if (robot_map[i, j] == 'r' && i + 1 < width && i - 1 >= 0 && j + 1 < height && j - 1 >= 0)
                {
                    if (robot_map[i + 1, j] == 'u' || robot_map[i - 1, j] == 'u' || robot_map[i, j + 1] == 'u' || robot_map[i, j - 1] == 'u'
                        || robot_map[i + 1, j + 1] == 'u' || robot_map[i - 1, j - 1] == 'u' || robot_map[i - 1, j + 1] == 'u' || robot_map[i + 1, j - 1] == 'u')
                    {
                        posToReach.Add(new Vector3(i * squareSize, transform.position.y, j * squareSize));
                    }
                }
            }
        }
    }

    /// <summary>
    /// This method is responsible for detecting points that can be targeted as frontier points. Num Map case only
    /// </summary>
    private void DFPinNumMap()
    {
        width = numeric_robot_map.GetLength(0);
        height = numeric_robot_map.GetLength(1);
        for (i = 0; i < width; i++)
        {
            for (j = 0; j < height; j++)
            {
                if (numeric_robot_map[i, j] == numFreeCell && i + 1 < width && i - 1 >= 0 && j + 1 < height && j - 1 >= 0)
                {
                    if (numeric_robot_map[i + 1, j] == numUnknownCell || numeric_robot_map[i - 1, j] == numUnknownCell ||
                        numeric_robot_map[i, j + 1] == numUnknownCell || numeric_robot_map[i, j - 1] == numUnknownCell
                        || numeric_robot_map[i + 1, j + 1] == numUnknownCell || numeric_robot_map[i - 1, j - 1] == numUnknownCell
                        || numeric_robot_map[i - 1, j + 1] == numUnknownCell || numeric_robot_map[i + 1, j - 1] == numUnknownCell)
                    {
                        posToReach.Add(new Vector3(i * squareSize, transform.position.y, j * squareSize));
                    }
                }
            }
        }
    }

    /// <summary>
    /// This method is called to choose another point to approach if the previous one has been reached
    /// </summary>
    public void SetTempReached()
    {
        rM.tempReached = true;
        StopCoroutine(pointToReach);
        StartCoroutine(pointToReach);
    }

    /// <summary>
    /// This method corrects the round of coordinates of a tile. In this way is avoided to assign values to undesired tiles (for example, a wall tile as a free one)
    /// </summary>
    /// <param name="coordinate">The coordinate of a tile</param>
    /// <returns></returns>
    protected float FixingRound(float coordinate)
    {
        if (Mathf.Abs(coordinate - Mathf.Round(coordinate)) >= 0.5f)
        {
            return (Mathf.Round(coordinate) - 1);
        }
        else return Mathf.Round(coordinate);
    }

    /// <summary>
    /// This method allows to save map information and robot trajectory information on files. When the simulation is ended (the goal is reached), these content
    /// is saved on a dedicated server
    /// </summary>
    /// <returns></returns>
    protected IEnumerator SavingProgress()
    {
        while (rM.inGameSession)
        {
            if (!isNumeric)
                rP.SaveMapChar(robot_map);
            else rP.SaveMapNum(numeric_robot_map);

            robotX = FixingRound(transform.position.x / squareSize);
            robotZ = FixingRound(transform.position.z / squareSize);

            if (!isNumeric)
                rP.SavePosChar((int)robotX, (int)robotZ, transform.eulerAngles);
            else rP.SavePosNum((int)robotX, (int)robotZ, transform.eulerAngles);

            yield return new WaitForSeconds(1f);
        }

        finishingTime = Time.time;

        if (!isNumeric)
            rP.SaveTimeChar(finishingTime - startingTime);
        else rP.SaveTimeNum(finishingTime - startingTime);

        //rP.PreparingForServer();
        finished = true;

    }

    /// <summary>
    /// This method allows the GameManager to know if the data has been correctly loaded on the server. In this way the system can return to the menu (or proceed to
    /// a new scene)
    /// </summary>
    /// <returns></returns>
    public bool TargetReached()
    {
        if (finished)
        {
            return true;
        }
        else return false;
    }

    /// <summary>
    /// This method is responsible to reset the target found condition to the default one and the Scanning & Decision phase, in order to continue finding 
    /// other targets if deployed in the map.
    /// Used in Multi-Target Maps
    /// </summary>
    public void ResetTargetFound()
    {
        targetFound = false;
        rM.targetFound = false;
        StartCoroutine(RestartAfterReachingObjective());
    }
}
