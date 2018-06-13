using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : Entity {

    [SerializeField] public Camera robotCamera;
    [SerializeField] public float speed = 10f;
    [SerializeField] public int numbRay;
    [SerializeField] [Range(10f, 100f)] public float rotationSpeed;
    [SerializeField] [Range(0f, 1f)] public float angleRay;
    [SerializeField] [Range(0f, 1f)] public float timeBetweenRays;
    [SerializeField] private float gravity = 100f;

    [Header("Raycast parameters")] [SerializeField] private bool limitRange = false;
    [SerializeField] private float range = 100f;
    [SerializeField] private GameObject sparkPrefab;
    [SerializeField] private float sparkDuration = 0.01f;
    [SerializeField] private LayerMask ignoredLayers;

    [Header("Algorithm")]
    [SerializeField] public int algorithmChoice;

    bool targetFound;
    bool goForward;
    bool goRotation;

    Rigidbody rb;
    RaycastHit hit;
    List<Ray> landingRay = new List<Ray>();

    float starting_speedX;
    float starting_speedY;
    float starting_speedZ;

    private char[,] total_map;
    private char[,] robot_map;

    private List<Vector3> goals; 

    // Use this for initialization
    void Start()
    {
        targetFound = false;
        goForward = true;
        goRotation = false;
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
        StartingMonitoring();
    }

    // Update is called once per frame
    void Update()
    {
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
            transform.Rotate(Vector3.up * Time.deltaTime * speed); 
        } 
    }

    void StartingMonitoring()
    {
        StartCoroutine(SendingRays());
    }

    private IEnumerator SendingRays()
    {
        while (!targetFound)
        {
            float angle = angleRay;
            yield return new WaitForSeconds(timeBetweenRays);
            robot_map[(int)transform.position.x/4, (int)transform.position.z/4] = 'r';
            landingRay[0] = new Ray(transform.position, Vector3.forward);
            UpdatingSpace(landingRay[0]);
            for (int i = 1; i < numbRay - 1; i=i+2) //y rimane invariato, bisogna spaziare intorno a x e z
            {
                Ray rayRight = new Ray(transform.position, Quaternion.AngleAxis(-angle, transform.up) * transform.forward);
                landingRay[i] = rayRight;
                UpdatingSpace(landingRay[i]);
                Ray rayLeft = new Ray(transform.position, Quaternion.AngleAxis(angle, transform.up) * transform.forward);
                landingRay[i+1] = rayLeft;
                UpdatingSpace(landingRay[i+1]);

                angle = angle + angleRay;
            }
            ChoosingPointToReach();
            ApproachingPointToReach();
        }
    }

    private void UpdatingSpace(Ray ray)
    {
        if(!Physics.Raycast(ray, out hit, range))
        {
            for (int i = 0; i < range; i++)
            {
                robot_map[(int)ray.GetPoint(i).x/4, (int)ray.GetPoint(i).z/4] = 'r';
            }
        }else
        {
            if (hit.collider.gameObject.tag == "Environment")
            {
            Debug.Log("Environment hit!");
            Debug.Log("You hit " + hit.collider.gameObject.tag);
            //aggiungere al proprio array che quello è un wall ed effettuare un ricalcolo
            }
            else if (hit.collider.gameObject.tag == "Target")
            {
            Debug.Log("Target found!");
            targetFound = true;
            //portarlo dritto al target
            }
        }
    }

        void ChoosingPointToReach()
    {
        List<Vector3> posToReach = new List<Vector3>();
        int width = robot_map.GetLength(0);
        int height = robot_map.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (robot_map[x,y] == 'r')
                {
                    posToReach.Add(new Vector3(x*4, transform.position.y, y*4));
                }
            }
        }
        goals = posToReach;
        for (int i = 0; i < goals.Count; i++)
        {
            Debug.Log("Point to reach is " + goals[i]);
        }
    }

    void ApproachingPointToReach()
    {
        //devo chiedere gli algoritmi, anche se potrei cominciare con il randomico
        switch (algorithmChoice)
        {
            case 0:
                int desiredPos = Random.Range(0, goals.Count);
                Plane[] planes = GeometryUtility.CalculateFrustumPlanes(robotCamera);
                Debug.Log(goals[desiredPos]);
                GameObject pos = new GameObject();
                pos.transform.position = goals[desiredPos];
                pos.AddComponent<BoxCollider>();
                pos.GetComponent<BoxCollider>().isTrigger = true;
                if(!GeometryUtility.TestPlanesAABB(planes, pos.GetComponent<BoxCollider>().bounds))
                {
                    goForward = false;
                    goRotation = true;
                }
                else
                {
                    goForward = true;
                    goRotation = false;
                }
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            default:
                break;
        }
    }

    public void SetMap(char[,] map)
    {
        total_map = map;
        robot_map = map;
        //per un mero controllo
        int width = total_map.GetLength(0);
        int height = total_map.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Debug.Log(total_map[x,y] + "" + x + "" + y);
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
}
