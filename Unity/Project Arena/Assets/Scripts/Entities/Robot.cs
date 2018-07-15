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

    [Header("Raycast parameters")] [SerializeField] private bool limitRange = false;
    [SerializeField] private float range = 150f;
    [SerializeField] private GameObject sparkPrefab;
    [SerializeField] private float sparkDuration = 0.01f;
    [SerializeField] private LayerMask ignoredLayers;

    [Header("Algorithm")]
    [SerializeField] public int algorithmChoice;

    bool targetFound;
    bool goApproach;
    bool goChoosing;
    bool goForward;
    bool goRotation;
    bool goSending;
    bool tempReached;

    Rigidbody rb;
    RaycastHit hit;
    List<Ray> landingRay = new List<Ray>();

    float squareSize;
    float starting_speedX;
    float starting_speedY;
    float starting_speedZ;

    private char[,] total_map;
    private char[,] robot_map;

    GameObject tempDestination;

    private List<Vector3> goals; 

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
        //StartingMonitoring();
    }

    // Update is called once per frame
    void Update()
    {
        if (goSending)
        {
            goSending = false;
            StartCoroutine(SendingRays());
        }
        if (!targetFound && goForward && !goRotation)
        {
            transform.position += transform.forward * Time.deltaTime * speed;
            //rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, speed);
            transform.rotation = transform.rotation;
        }
        else if (!targetFound && !goForward && goRotation)
        {
            transform.position = transform.position;
            //rb.velocity = new Vector3(0,0,0);
            //Vector3 angularVelocity = new Vector3(0, speed, 0);
            transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed); 
        }
        if (!targetFound && goChoosing)
        {
            ChoosingPointToReach();
        }
        if (!targetFound && goApproach)
        {
            ApproachingPointToReach();
        }
        if (tempDestination)
        {
            if (tempReached)
            {
                Destroy(tempDestination);
                tempReached = false;
            }
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(robotCamera);
            Ray center = new Ray(transform.position, transform.forward);
            Debug.DrawRay(transform.position, transform.forward * rangeRays, Color.red, 0.5f);
            if (GeometryUtility.TestPlanesAABB(planes, tempDestination.GetComponent<BoxCollider>().bounds) && Physics.Raycast(center, out hit, rangeRays) 
                && hit.collider.gameObject.name == "New Game Object")
            { 
                goForward = true;
                goRotation = false;
            }
            else
            {
                goForward = false;
                goRotation = true;
            }
        }
        else
        {
            goForward = false;
            goRotation = false;
        }
    }

    public void SetMap(char[,] map, float floorSquareSize)
    {
        squareSize = floorSquareSize;
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
                robot_map[x, y] = 'u';
            }
        }
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
        float robot_x = transform.position.x;
        float robot_z = transform.position.z;
        robot_x = robot_x / squareSize;
        robot_z = robot_z / squareSize;
        robot_map[(int)robot_x, (int)robot_z] = 'r';
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
                    robot_map[(int)x_coord, (int)y_coord] = 'r';
                    Debug.Log(robot_map[(int)x_coord, (int)y_coord] + "" + (int)x_coord + "" + (int)y_coord);
                }
            }
            else
            {
                float x = hit.transform.position.x / squareSize;
                float y = hit.transform.position.z / squareSize;
                if (hit.collider.gameObject.tag == "Environment")
                {
                    Debug.Log("Environment hit!");
                    Debug.Log("You hit " + hit.collider.gameObject.tag);
                    //Debug.Log(hit.collider.gameObject.transform.position);
                    Debug.Log(hit.point);
                    //robot_map[(int)x, (int)y] = 'r';
                    //float dx = hit.collider.gameObject.transform.position.x - this.gameObject.transform.position.x;
                    float dx = hit.point.x - this.gameObject.transform.position.x;
                    float dz = hit.point.z - this.gameObject.transform.position.z;
                    //float dz = hit.collider.gameObject.transform.position.z - this.gameObject.transform.position.z;
                    SettingR(hit.point, Mathf.Sqrt((dx*dx) + (dz*dz)), ray[i]);
                    //Debug.Log(x + "" + y);
                    //aggiungere al proprio array che quello è un wall ed effettuare un ricalcolo
                }
                else if (hit.collider.gameObject.tag == "Target")
                {
                    Debug.Log("Target found!");
                    targetFound = true;
                    robot_map[(int)x, (int)y] = 'g';
                    SettingR(hit.point, Vector3.Distance(hit.collider.gameObject.transform.position, this.gameObject.transform.position), ray[i]);
                    //Debug.Log(x + "" + y);
                    //portarlo dritto al target
                }
                else
                {
                    Debug.Log("The ray hit " + hit.transform.name);
                    //robot_map[(int)x, (int)y] = 'r';
                    float dx = hit.collider.gameObject.transform.position.x - this.gameObject.transform.position.x;
                    float dz = hit.collider.gameObject.transform.position.z - this.gameObject.transform.position.z;
                    SettingR(hit.point, Mathf.Sqrt((dx * dx) + (dz * dz)), ray[i]);
                    //Debug.Log(x + "" + y);
                }
            }
        }
        goChoosing = true;
    }

    private void SettingR(Vector3 collisionPoint, float distance, Ray ray)
    {
        Debug.Log("Max distance: " + distance);
        for (float i = 0; i < distance; i++)
        {
            float x = ray.GetPoint(i).x / squareSize;
            float y = ray.GetPoint(i).z / squareSize;
            robot_map[(int)x, (int)y] = 'r';
            Debug.Log(robot_map[(int)x, (int)y] + "" + (int)x + "" + (int)y + " at a distance " + i);
        }
    }

    public void ChoosingPointToReach()
    {
        goChoosing = false;
        List<Vector3> posToReach = new List<Vector3>();
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
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(robot_map[x,y] == 'r')
                Debug.Log(robot_map[x,y] + "" + x + "" + y);
            }
        }
        goals = posToReach;
        for (int i = 0; i < goals.Count; i++)
        {
            Debug.Log("Possible point to reach is: " + goals[i]);
        }
        goApproach = true;
    }

    public void ApproachingPointToReach()
    {
        goApproach = false;
        perTestare(goals);
        if (tempDestination)
        {
            Destroy(tempDestination);
            tempReached = false;
        }
        int desiredPos = Random.Range(0, goals.Count);
        tempDestination = new GameObject();
        //tempDestination = Resources.Load("Small Target") as GameObject;
        tempDestination.transform.position = goals[desiredPos];
        Debug.Log("Chosen point to reach is: " + goals[desiredPos]);
        tempDestination.AddComponent<BoxCollider>();
        tempDestination.GetComponent<BoxCollider>().isTrigger = true;
        tempDestination.AddComponent<RobotSpoofedDest>();
        tempDestination.GetComponent<RobotSpoofedDest>().robot = this.gameObject;
        tempDestination.tag = "Opponent";
        tempDestination.layer = 8;
        //goForward = true;
    }

    public void SetTempReached()
    {
        tempReached = true;
        goSending = true;
    }

    public void perTestare(List<Vector3> goal)
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
    }
}
