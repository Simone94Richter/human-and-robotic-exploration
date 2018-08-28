using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : Entity, IRobot{

    [SerializeField] public Camera robotCamera;
    [SerializeField] public float speed = 10f;
    [SerializeField] public int numbRay;
    [SerializeField] [Range(10f, 100f)] public float rotationSpeed;
    [SerializeField] [Range(0f, 1f)] public float angleRay;
    [SerializeField] [Range(0f, 5f)] public float timeBetweenRays;
    [SerializeField] private float gravity = 100f;
    [SerializeField] [Range(100f, 1000f)] public float rangeRays;
    [SerializeField] [Range(1.0f, 10.0f)] public float rayCertainty = 8.0f;

    [Header("Raycast parameters")] [SerializeField] private bool limitRange = false;
    [SerializeField] private float range = 150f;
    [SerializeField] private GameObject sparkPrefab;
    [SerializeField] private float sparkDuration = 0.01f;
    [SerializeField] private LayerMask ignoredLayers;

    [Header("Is map to be analyzed numeric or char?")]
    public bool isNumeric;

    [Header("Arbitrary float used to detect wall after a collision with one of them")]
    public float epsilon = 1f; //for now, 1 is the best

    [Header("Num paramters for numerical mapping")]
    public float numFreeCell = 0f;
    public float numWallCell = 1.5f;
    public float numUnknownCell = 1f;
    public float numGoalCell = 2f;

    bool targetFound; //is the objective detected by mapping?
    bool goApproach;
    bool goChoosing;
    bool goForward;
    bool goRotation;
    bool goSending;
    bool tempReached;

    Rigidbody rb;
    RaycastHit hit;
    List<Ray> landingRay = new List<Ray>();
    List<Vector3> route;

    float finishingTime;
    float startingTime;
    float squareSize; //dimension of a cell of the floor, used in order to discretize the final map
    float starting_speedX;
    float starting_speedY;
    float starting_speedZ;

    private char[,] total_map; //the original map, passed by the system
    private char[,] robot_map; //the char map updated by the robot
    private float [,] numeric_robot_map; //the numeric map updated by the robot

    GameObject tempDestination; //the temp point to reach, expressed as a GameObject
    GameObject destination; //the target, properly

    private List<Vector3> goals; //list of frontier points
    private Vector3 tempGoal; //Vector3 of the temp point to reach

    RobotMovement rM;
    RobotProgress rP;
    RobotPlanning rPl;

    // Use this for initialization
    void Start()
    {
        targetFound = false;
        tempReached = false;
        goApproach = false;
        goChoosing = false;
        goForward = false;
        goRotation = false;
        goSending = true;
        rb = gameObject.GetComponent<Rigidbody>();
        rM = GetComponent<RobotMovement>();
        rP = GetComponent<RobotProgress>();
        rPl = GetComponent<RobotPlanning>();
        rPl.range = rangeRays;
        starting_speedX = rb.velocity.x;
        starting_speedY = rb.velocity.y;
        starting_speedZ = rb.velocity.z;
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
    void LateUpdate()
    {
        if (goSending)
        {
            goSending = false;
            StartCoroutine(SendingRays());
        }
        
        if (!targetFound && goChoosing)
        {
            ChoosingPointToReach();
        }
        if (!targetFound && goApproach)
        {
            goApproach = false;
            //inserire qui il metodo di planning
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
            route = new List<Vector3>();
            rPl.hasTerminated = false;
            //rPl.CheckVisibility(FixingRound(transform.position.x/squareSize), FixingRound(transform.position.z / squareSize),
            //    FixingRound(tempGoal.x / squareSize), FixingRound(tempGoal.z / squareSize));
            route = rPl.CheckVisibility(transform.position/squareSize, tempGoal/squareSize, route);
            if (route == null || route.Count == 0)
            {
                rM.ApproachingPointToReach(tempGoal);
            }
            else
            {
                int start = 0;
                rM.ApproachingPointToReach(route, start);
            }
        }
        if (targetFound)
        {
            rM.ApproachingGoal(destination);
        }
    }

    /// <summary>
    /// Method called by the GameManager to initialized the map of the robot
    /// </summary>
    /// <param name="map">the original map, held by the GameManaer</param>
    /// <param name="floorSquareSize">Parameter used to discretize the cell of the floor (from Unity world to robot map)</param>
    public void SetMap(char[,] map, float floorSquareSize)
    {
        squareSize = floorSquareSize;
        rPl.squareSize = floorSquareSize;
        total_map = map;
        robot_map = map;
        //per un mero controllo
        int width = total_map.GetLength(0);
        int height = total_map.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //Debug.Log(total_map[x,y] + "" + x + "" + y);
                if(!isNumeric)
                robot_map[x, y] = 'u';
                else numeric_robot_map[x, y] = numUnknownCell;
            }
        }

        startingTime = Time.time;
        StartCoroutine(SavingProgress());
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

    public IEnumerator SendingRays()
    {

        float angle = angleRay;
        yield return new WaitForSeconds(timeBetweenRays);
        landingRay[0] = new Ray(transform.position, transform.forward);
        //UpdatingSpace(landingRay[0]);
        Debug.DrawRay(transform.position, transform.forward * rangeRays, Color.red, 2f);
        for (int i = 1; i < numbRay - 1; i = i + 2) //y rimane invariato, bisogna spaziare intorno a x e z
        {
            Ray rayRight = new Ray(transform.position, Quaternion.AngleAxis(-angle, transform.up) * transform.forward);
            landingRay[i] = rayRight;
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-angle, transform.up) * transform.forward * rangeRays, Color.red, 2f);
            //Debug.DrawRay(transform.position, transform.TransformDirection(Quaternion.AngleAxis(-angle, transform.up) * transform.forward * rangeRays), Color.red, 3.0f);
            //UpdatingSpace(landingRay[i]);
            Ray rayLeft = new Ray(transform.position, Quaternion.AngleAxis(angle, transform.up) * transform.forward);
            landingRay[i + 1] = rayLeft;
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(angle, transform.up) * transform.forward * rangeRays, Color.red, 2f);
            //Debug.DrawRay(transform.position, transform.TransformDirection(Quaternion.AngleAxis(angle, transform.up) * transform.forward * rangeRays), Color.red, 3.0f);
            //UpdatingSpace(landingRay[i + 1]);

            angle = angle + angleRay;
        }
        UpdatingSpace(landingRay);
    }

    public void UpdatingSpace(List<Ray> ray)
    {
        for (int i = 1; i < numbRay - 1; i++)
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
                    robot_map[(int)x_coord, (int)y_coord] = 'r';
                    else numeric_robot_map[(int)x_coord, (int)y_coord] = numFreeCell; 
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
                    SettingR(hit.point, Mathf.Sqrt((dx*dx) + (dz*dz)), ray[i]);
                    float angle = Vector3.Angle(transform.right, ray[i].direction);
                    SettingW(Mathf.Sqrt((dx * dx) + (dz * dz)), ray[i], epsilon, angle);
                    //Debug.Log(x + "" + y);
                }
                else if (hit.collider.gameObject.tag == "Target")
                {
                    Debug.Log("Target found!");
                    targetFound = true;
                    rM.targetFound = true;
                    rM.squareSize = squareSize;
                    destination = hit.collider.gameObject;
  
                    SettingR(hit.point, Vector3.Distance(hit.collider.gameObject.transform.position, this.gameObject.transform.position), ray[i]);

                    x = FixingRound(x);
                    y = FixingRound(y);
                    if (!isNumeric)
                        robot_map[(int)x, (int)y] = 'g';
                    else numeric_robot_map[(int)x, (int)y] = numGoalCell;
                    //Debug.Log(x + "" + y);
                    //portarlo dritto al target
                }
                else
                {
                    //Debug.Log("The ray hit " + hit.transform.name);
                    //robot_map[(int)x, (int)y] = 'r';
                    float dx = hit.collider.gameObject.transform.position.x - this.gameObject.transform.position.x;
                    float dz = hit.collider.gameObject.transform.position.z - this.gameObject.transform.position.z;
                    SettingR(hit.point, Mathf.Sqrt((dx * dx) + (dz * dz)), ray[i]);
                    float angle = Vector3.Angle(ray[i].direction, transform.right);
                    SettingW(Mathf.Sqrt((dx * dx) + (dz * dz)), ray[i], epsilon, angle);
                    //Debug.Log(x + "" + y);
                }
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
        if (!isNumeric)
            robot_map[(int)robot_x, (int)robot_z] = 'p';
        else numeric_robot_map[(int)robot_x, (int)robot_z] = 3f;
        goChoosing = true;
    }

    private void SettingR(Vector3 collisionPoint, float distance, Ray ray)
    {
        //Debug.Log("Max distance: " + distance);
        //Debug.Log(ray.GetPoint(distance) + ", " + (int)(ray.GetPoint(distance).x/squareSize));
        for (float i = 0; i < distance; i++)
        {
            float x = ray.GetPoint(i).x / squareSize;
            float y = ray.GetPoint(i).z / squareSize;
            x = FixingRound(x);
            y = FixingRound(y);
            if (!isNumeric)
                robot_map[(int)x, (int)y] = 'r';
            else numeric_robot_map[(int)x, (int)y] = numFreeCell;
            //Debug.Log(robot_map[(int)x, (int)y] + "" + (int)x + "" + (int)y + " at a distance " + i);
        }
    }

    private void SettingW(float distance, Ray ray, float epsilon, float angle)
    {
        float robotX = transform.position.x / squareSize;
        float robotz = transform.position.z / squareSize;
        robotX = FixingRound(robotX);
        robotz = FixingRound(robotz);
        //float hitPX = collisionPoint.x;
        //float hitPZ = collisionPoint.z;
        float x = ray.GetPoint(distance).x / squareSize;
        float z = ray.GetPoint(distance).z / squareSize;
        x = FixingRound(x);
        z = FixingRound(z);

        float dx = (x - robotX);
        float dz = (z - robotz);
        //Debug.Log(angle);
        //Debug.Log(Mathf.Sin(angle) + Mathf.Cos(angle));
        
        //method 1
        float wallPointX = FixingRound(ray.GetPoint(distance + epsilon).x / squareSize);
        float wallPointZ = FixingRound(ray.GetPoint(distance + epsilon).z / squareSize);

        if (!isNumeric)
            robot_map[(int)wallPointX, (int)wallPointZ] = 'w';
        else numeric_robot_map[(int)wallPointX, (int)wallPointZ] = numWallCell;
        //Debug.Log("(" + wallPointX + ", " + wallPointZ + ")");
        
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

    public void ChoosingPointToReach()
    {
        goChoosing = false;
        List<Vector3> posToReach = new List<Vector3>();
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
                        posToReach.Add(new Vector3(x * squareSize, transform.position.y, y * squareSize));
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
                    if (numeric_robot_map[x, y] == numFreeCell)
                    {
                        posToReach.Add(new Vector3(x * squareSize, transform.position.y, y * squareSize));
                    }
                }
            }
        }
        //for (int x = 0; x < width; x++)
        //{
        //    for (int y = 0; y < height; y++)
        //    {
                //if(robot_map[x,y] == 'r')
                //Debug.Log(robot_map[x,y] + "" + x + "" + y);
        //    }
        //}
        goals = posToReach;
        /*for (int i = 0; i < goals.Count; i++)
        {
            //Debug.Log("Possible point to reach is: " + goals[i]);
        }*/
        int desiredPos = Random.Range(0, goals.Count);
        tempGoal = goals[desiredPos];

        goApproach = true;
    }

    public void SetTempReached()
    {
        tempReached = true;
        goSending = true;
    }

    private float FixingRound(float coordinate)
    {
        if (Mathf.Abs(coordinate - Mathf.Round(coordinate)) >= 0.5f)
        {
            return (Mathf.Round(coordinate) - 1);
        }
        else return Mathf.Round(coordinate);
    }

    private IEnumerator SavingProgress()
    {
        //while (goSending)
        //{
        //    yield return null;
        //}
        while (rM.inGameSession)
        {
            if (!isNumeric)
                rP.SaveMapChar(robot_map);
            else rP.SaveMapNum(numeric_robot_map);
            float x = transform.position.x / squareSize;
            x = FixingRound(x);
            float z = transform.position.z /squareSize;
            z = FixingRound(z);
            rP.SavePos((int)x, (int)z, transform.rotation);
            yield return new WaitForSeconds(1.0f);
        }
        finishingTime = Time.time;
        rP.SaveTime(finishingTime - startingTime);
    }

    /*public void perTestare(List<Vector3> goal)
    {
        goal = goals;
        List<GameObject> testTempPos = new List<GameObject>();
        for (int i = 0; i < goals.Count; i++)
        {
            testTempPos.Add(new GameObject());
            //tempDestination = Resources.Load("Small Target") as GameObject;
            testTempPos[i].transform.position = goals[i];
            Debug.Log("Chosen point to reach is: " + goals[i]);
            testTempPos[i].AddComponent<BoxCollider>();
            testTempPos[i].GetComponent<BoxCollider>().isTrigger = true;
            testTempPos[i].AddComponent<RobotSpoofedDest>();
            testTempPos[i].GetComponent<RobotSpoofedDest>().robot = this.gameObject;
            testTempPos[i].tag = "Opponent";
            testTempPos[i].layer = 8;
        }
    }*/
}
