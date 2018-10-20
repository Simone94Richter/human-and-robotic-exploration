using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class manages the core functionalities of the robot: it gives start to moving, map analyze, saving data and the other features provided
/// </summary>
public class Robot : Entity{

    [SerializeField] public Camera robotCamera;
    [SerializeField] public float speed = 10f;
    [SerializeField] public int numbRay;
    //[SerializeField] [Range(10f, 100f)] public float rotationSpeed;
    [SerializeField] [Range(0f, 1f)] public float angleRay;
    //[SerializeField] [Range(0f, 5f)] public float timeBetweenRays;
    //[SerializeField] private float gravity = 100f;
    [SerializeField] [Range(0f, 1000f)] public float rangeRays;
    [SerializeField] [Range(1.0f, 10.0f)] public float rayCertainty = 8.0f;

    [Header("Is map to be analyzed numeric or char?")]
    public bool isNumeric;

    //public bool noPath;

    [Header("Arbitrary float used to detect wall after a collision with one of them")]
    public float epsilon = 1f; //for now, 1 is the best

    [Header("Num paramters for numerical mapping")]
    public float numFreeCell = 0f;
    public float numWallCell = 1.5f;
    public float numUnknownCell = 1f;
    public float numGoalCell = 2f;

    [Header("Time for scan and decision making")]
    public float timeForScan;
    public float timeForDecision;

    private bool targetFound; //is the objective detected by mapping?

    private RaycastHit hit;
    private List<Ray> landingRay = new List<Ray>();
    private List<Vector3> route;

    private float finishingTime; //the time in seconds when the robot found the goal
    private float startingTime; //the time in seconds when the robot starts the simulation
    private float squareSize; //dimension of a cell of the floor, used in order to discretize the final map

    private char[,] total_map; //the original map, passed by the system
    private float[,] numeric_total_map; //the original numeric map, passed by the system
    private char[,] robot_map; //the char map updated by the robot
    private float [,] numeric_robot_map; //the numeric map updated by the robot

    private GameObject tempDestination; //the temp point to reach, expressed as a GameObject
    private GameObject destination; //the target, properly

    private IEnumerator coroutine;

    private List<Vector3> goals; //list of frontier points
    private Vector3 tempGoal; //Vector3 of the temp point to reach

    private RobotConnection rC;
    private RobotDecisionMaking rDM;
    private RobotMovement rM;
    private RobotProgress rP;
    private RobotPlanning rPl;

    // Use this for initialization
    void Start()
    {
        coroutine = ChoosingPointToReach();
        //noPath = false;
        targetFound = false;
        rC = GetComponent<RobotConnection>();
        rDM = GetComponent<RobotDecisionMaking>();
        rM = GetComponent<RobotMovement>();
        rP = GetComponent<RobotProgress>();
        rPl = GetComponent<RobotPlanning>();
        rPl.range = rangeRays;
        landingRay.Add(new Ray(transform.position, Vector3.forward));
        float angle = angleRay;
        for (int i = 1; i < numbRay-1; i=i+2) //y rimane invariato, bisogna spaziare intorno a x e z
        {
            Ray rayRight = new Ray(transform.position, Quaternion.AngleAxis(-angle, transform.up) * transform.forward);
            landingRay.Add(rayRight);
            
            Ray rayLeft = new Ray(transform.position, Quaternion.AngleAxis(angle, transform.up) * transform.forward);
            landingRay.Add(rayLeft);

            angle = angle + angleRay;
        }
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
    /// Method called by the GameManager to initializ the map of the robot (char case)
    /// </summary>
    /// <param name="map">the original map, held by the GameManaer</param>
    /// <param name="floorSquareSize">Parameter used to discretize the cell of the floor (from Unity world to robot map)</param>
    public void SetMap(char[,] map, float floorSquareSize)
    {
        squareSize = floorSquareSize;
        rPl.squareSize = floorSquareSize;
        total_map = map;
        robot_map = map;
        isNumeric = false;
        int width = total_map.GetLength(0);
        int height = total_map.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                robot_map[x, y] = 'u';
            }
        }

        startingTime = Time.time;

        rPl.isNumeric = false;
        rPl.robot_map = robot_map;
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
        numeric_total_map = map;
        numeric_robot_map = map;
        isNumeric = true;
        int width = numeric_total_map.GetLength(0);
        int height = numeric_total_map.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                numeric_robot_map[x, y] = numUnknownCell;
            }
        }

        startingTime = Time.time;

        rPl.isNumeric = true;
        rPl.numeric_robot_map = numeric_robot_map;
        
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
    private IEnumerator WaitingBeforeAction()
    {
        yield return new WaitForSeconds(4f);
        StartCoroutine(SavingProgress());
        StartCoroutine(SendingRays());
        yield return new WaitForSeconds(2f);
        StartCoroutine(coroutine);
    }

    /// <summary>
    /// This method sends raycast from itself with a certain range and space in order to get information from the environment.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendingRays()
    {
        while (!targetFound)
        {
            float angle = angleRay;
            landingRay[0] = new Ray(transform.position, transform.forward);
            //Debug.DrawRay(transform.position, transform.forward * rangeRays, Color.red, 2f);
            for (int i = 1; i < numbRay - 1; i = i + 2) //y rimane invariato, bisogna spaziare intorno a x e z
            {
                Ray rayRight = new Ray(transform.position, Quaternion.AngleAxis(-angle, transform.up) * transform.forward);
                landingRay[i] = rayRight;
                //Debug.DrawRay(transform.position, Quaternion.AngleAxis(-angle, transform.up) * transform.forward * rangeRays, Color.red, 2f);
                Ray rayLeft = new Ray(transform.position, Quaternion.AngleAxis(angle, transform.up) * transform.forward);
                landingRay[i + 1] = rayLeft;
                //Debug.DrawRay(transform.position, Quaternion.AngleAxis(angle, transform.up) * transform.forward * rangeRays, Color.red, 2f);

                angle = angle + angleRay;
            }
            UpdatingSpace(landingRay);
            yield return new WaitForSeconds(timeForScan);
        }
    }

    /// <summary>
    /// It assign a type to a tile in the robot's map. A tile can be : free (so the robot can pass on it without problems), adjacent a wall or containing partially 
    /// it (so the robot is discouraged to pass on it) or containing the goal object (so the robot get maximum priority to approach it)
    /// </summary>
    /// <param name="ray">The list of raycast sent by the robot during the scanning</param>
    public void UpdatingSpace(List<Ray> ray)
    {
        for (int i = 0; i < numbRay - 1; i++)
        {
            if (!Physics.Raycast(ray[i], out hit, rangeRays))
            {
                for (float j = rangeRays; j >= 0; j--)
                {
                    float x_coord = ray[i].GetPoint(j).x;
                    float y_coord = ray[i].GetPoint(j).z;
                    x_coord = x_coord / squareSize;
                    y_coord = y_coord / squareSize;
                    x_coord = FixingRound(x_coord);
                    y_coord = FixingRound(y_coord);
                    if(!isNumeric)
                    if(robot_map[(int)x_coord, (int)y_coord] != 'w') robot_map[(int)x_coord, (int)y_coord] = 'r';
                    else if (numeric_robot_map[(int)x_coord, (int)y_coord] != numWallCell) numeric_robot_map[(int)x_coord, (int)y_coord] = numFreeCell; 
                    //Debug.Log(robot_map[(int)x_coord, (int)y_coord] + "" + (int)x_coord + "" + (int)y_coord);
                }
            }
            else
            {
                float x = hit.transform.position.x / squareSize;
                float y = hit.transform.position.z / squareSize;
                if (hit.collider.gameObject.tag == "Environment")
                {
                    //Debug.Log("Environment hit!");
                    //Debug.Log("You hit " + hit.collider.gameObject.tag);
                    //Debug.Log(hit.collider.gameObject.transform.position);
                    //Debug.Log(hit.point);
                    //robot_map[(int)x, (int)y] = 'r';
                    //float dx = hit.collider.gameObject.transform.position.x - this.gameObject.transform.position.x;
                    float dx = hit.point.x - this.gameObject.transform.position.x;
                    float dz = hit.point.z - this.gameObject.transform.position.z;
                    //float dz = hit.collider.gameObject.transform.position.z - this.gameObject.transform.position.z;
                    SettingR(Mathf.Sqrt((dx*dx) + (dz*dz)), ray[i]);
                    //float angle = Vector3.Angle(transform.right, ray[i].direction);
                    SettingW(Mathf.Sqrt((dx * dx) + (dz * dz)), ray[i], epsilon/*, angle*/);
                    //Debug.Log(x + "" + y);
                }
                else if (hit.collider.gameObject.tag == "Target")
                {
                    //Debug.Log("Target found!");
                    targetFound = true;
                    rM.squareSize = squareSize;
                    destination = hit.collider.gameObject;
  
                    SettingR(Vector3.Distance(hit.collider.gameObject.transform.position, this.gameObject.transform.position), ray[i]);

                    x = FixingRound(x);
                    y = FixingRound(y);
                    if (!isNumeric)
                        robot_map[(int)x, (int)y] = 'g';
                    else numeric_robot_map[(int)x, (int)y] = numGoalCell;
                    //Debug.Log(x + "" + y);
                    //portarlo dritto al target
                }
                //the following case doesn't work because it adds improper nearWall tiles
                /*else
                {
                    //Debug.Log("The ray hit " + hit.transform.name);
                    //robot_map[(int)x, (int)y] = 'r';
                    float dx = hit.collider.gameObject.transform.position.x - this.gameObject.transform.position.x;
                    float dz = hit.collider.gameObject.transform.position.z - this.gameObject.transform.position.z;
                    SettingR(Mathf.Sqrt((dx * dx) + (dz * dz)), ray[i]);
                    //float angle = Vector3.Angle(ray[i].direction, transform.right);
                    SettingW(Mathf.Sqrt((dx * dx) + (dz * dz)), ray[i], epsilon/*, angle);
                    //Debug.Log(x + "" + y);
                }*/
            }
        }
        if (destination)
        {
            float x = FixingRound(destination.transform.position.x/squareSize);
            float z = FixingRound(destination.transform.position.z/squareSize);
            if (!isNumeric)
                robot_map[(int)x, (int)z] = 'g';
            else numeric_robot_map[(int)x, (int)z] = numGoalCell;
        }
        float robot_x = transform.position.x;
        float robot_z = transform.position.z;
        robot_x = robot_x / squareSize;
        robot_z = robot_z / squareSize;
        robot_x = FixingRound(robot_x);
        robot_z = FixingRound(robot_z);
        //if (!isNumeric)
        //    robot_map[(int)robot_x, (int)robot_z] = 'r';
        //else numeric_robot_map[(int)robot_x, (int)robot_z] = numFreeCell;

        if (!isNumeric)
        {
            rPl.isNumeric = false;
            rPl.robot_map = robot_map;
        }
        else
        {
            rPl.isNumeric = true;
            rPl.numeric_robot_map = numeric_robot_map;
        }
    }

    /// <summary>
    /// This method sets a tile as a free one, giving the dedicated char or float value to it in the robot's map.
    /// If the tile has been already set to a 'near wall' one, it cannot be set as a free one
    /// </summary>
    /// <param name="distance">The distance from the robot until the last point of the ray</param>
    /// <param name="ray">The ray along which is considering the tiles analyzed</param>
    private void SettingR(float distance, Ray ray)
    {
        for (float i = 0; i < distance; i++)
        {
            float x = ray.GetPoint(i).x / squareSize;
            float y = ray.GetPoint(i).z / squareSize;
            x = FixingRound(x);
            y = FixingRound(y);
            if (!isNumeric && robot_map[(int)x, (int)y] != 'w')
                robot_map[(int)x, (int)y] = 'r';
            else if(isNumeric && numeric_robot_map[(int)x, (int)y] != numWallCell) numeric_robot_map[(int)x, (int)y] = numFreeCell;
        }
    }

    /// <summary>
    /// This method sets a tile as a near wall one, giving the dedicated char or float value to it in the robot's map
    /// </summary>
    /// <param name="distance">The distance from the robot until the last point of the ray</param>
    /// <param name="ray">The ray along which is considering the tiles analyzed</param>
    /// <param name="epsilon">Useless (is set to 0)</param>
    private void SettingW(float distance, Ray ray, float epsilon/*, float angle*/)
    {
        float robotX = transform.position.x / squareSize;
        float robotz = transform.position.z / squareSize;
        float x = ray.GetPoint(distance).x / squareSize;
        float z = ray.GetPoint(distance).z / squareSize;

        robotX = FixingRound(robotX);
        robotz = FixingRound(robotz);
        x = FixingRound(x);
        z = FixingRound(z);
        
        //method 1
        float wallPointX = FixingRound(ray.GetPoint(distance + epsilon).x / squareSize);
        float wallPointZ = FixingRound(ray.GetPoint(distance + epsilon).z / squareSize);

        if (!isNumeric /*&& robot_map[(int)wallPointX,(int)wallPointZ] != 'r'*/)
            robot_map[(int)wallPointX, (int)wallPointZ] = 'w';
        else if(isNumeric /*&& numeric_robot_map[(int)wallPointX, (int)wallPointZ] != numFreeCell*/) numeric_robot_map[(int)wallPointX, (int)wallPointZ] = numWallCell;
        
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
    /// This method is used to decide which tile the robot has to reach. All the potential tiles are the one that are free and near an unknown one
    /// </summary>
    /// <returns></returns>
    public IEnumerator ChoosingPointToReach()
    {
        while (!targetFound)
        {
            List<Vector3> posToReach = new List<Vector3>();
            //List<Vector3> optionalPosToReach = new List<Vector3>();
            if (!isNumeric)
            {
                int width = robot_map.GetLength(0);
                int height = robot_map.GetLength(1);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (robot_map[x, y] == 'r')
                        {
                            if (robot_map[x + 1, y] == 'u' || robot_map[x - 1, y] == 'u' || robot_map[x, y + 1] == 'u' || robot_map[x, y - 1] == 'u')
                            {
                                posToReach.Add(new Vector3(x * squareSize, transform.position.y, y * squareSize));
                            }
                            //optionalPosToReach.Add(new Vector3(x * squareSize, transform.position.y , y * squareSize));
                        }
                    }
                }
            }
            else
            {
                int width = numeric_robot_map.GetLength(0);
                int height = numeric_robot_map.GetLength(1);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (numeric_robot_map[x, y] == numFreeCell && x+1 < width && x-1 >= 0 && y+1 < height && y-1 >= 0)
                        {
                            if (numeric_robot_map[x + 1, y] == numUnknownCell || numeric_robot_map[x - 1, y] == numUnknownCell || numeric_robot_map[x, y + 1] == numUnknownCell || numeric_robot_map[x, y - 1] == numUnknownCell)
                            {
                                posToReach.Add(new Vector3(x * squareSize, transform.position.y, y * squareSize));
                            }
                            //optionalPosToReach.Add(new Vector3(x * squareSize, transform.position.y, y * squareSize));
                        }
                    }
                }
            }

            if (posToReach.Count != 0)
            {
                goals = posToReach;
                tempGoal = rDM.PosToReach(goals);

                route = new List<Vector3>();
                route = rPl.CheckVisibility(transform.position, tempGoal);
                rM.tempReached = false;
                if (route == null)
                {
                    rM.ApproachingPointToReach(tempGoal);
                }
                else
                {
                    int start = 0;
                    rM.ApproachingPointToReach(route, start);
                }
            }
            else
            {
                transform.Rotate(Vector3.up * Time.deltaTime * 20f);
            }

            yield return new WaitForSeconds(timeForDecision);
        }
    }

    /// <summary>
    /// This method is called to choose another point to approach if the previous one has been reached
    /// </summary>
    public void SetTempReached()
    {
        rM.tempReached = true;
        StopCoroutine(coroutine);
        StartCoroutine(coroutine);
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

    /// <summary>
    /// This method allows to save map information and robot trajectory information on files. When the simulation is ended (the goal is reached), these content
    /// is saved on a dedicated server
    /// </summary>
    /// <returns></returns>
    private IEnumerator SavingProgress()
    {
        while (rM.inGameSession)
        {
            if (!isNumeric)
                rP.SaveMapChar(robot_map);
            else rP.SaveMapNum(numeric_robot_map);
            float x = transform.position.x / squareSize;
            x = FixingRound(x);
            float z = transform.position.z /squareSize;
            z = FixingRound(z);
            if (!isNumeric)
                rP.SavePosChar((int)x, (int)z, transform.eulerAngles);
            else rP.SavePosNum((int)x, (int)z, transform.eulerAngles);
            yield return new WaitForSeconds(1f);
        }
        finishingTime = Time.time;
        if (!isNumeric)
            rP.SaveTimeChar(finishingTime - startingTime);
        else rP.SaveTimeNum(finishingTime - startingTime);
        rP.PreparingForServer();

    }

    /// <summary>
    /// This method allows the GameManager to know if the data has been correctly loaded on the server. In this way the system can return to the menu (or proceed to
    /// a new scene)
    /// </summary>
    /// <returns></returns>
    public bool TargetReached()
    {
        if (rC.uploadComplete)
        {
            return true;
        }
        else return false;
    }
}
