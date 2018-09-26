using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotMovement : MonoBehaviour {

    [SerializeField] public Camera robotCamera;
    [SerializeField] [Range(100f, 1000f)] public float rangeRays;
    [SerializeField] [Range(10f, 100f)] public float rotationSpeed;
    [SerializeField] public float speed = 10f;

    bool goForward;
    bool goRotation;
    public bool targetFound;
    public bool tempReached;

    public bool inGameSession = true;

    public float squareSize;

    private int layerMask = 1 << 0;

    GameObject tempDestination;
    GameObject target;

    RaycastHit hit;

    private Vector3 tempRelatedToRobot;
    private RobotPlanning rPL;

    // Use this for initialization
    void Start () {
        goForward = false;
        goRotation = false;
        targetFound = false;
        tempReached = false;
        rPL = GetComponent<RobotPlanning>();
	}
	
	// Update is called once per frame
	void Update () {
        if (tempDestination && !targetFound)
        {
            //Debug.Log(transform.InverseTransformPoint(tempDestination.transform.position));
            if (!targetFound && goForward && !goRotation && !tempReached)
            {
                transform.position += transform.forward * Time.deltaTime * speed;
                //transform.position = Vector3.MoveTowards(transform.position, tempDestination.transform.position, speed * Time.deltaTime);
                transform.rotation = transform.rotation;
            }
            else if (!targetFound && !goForward && goRotation && !tempReached)
            {
                transform.position = transform.position;
                tempRelatedToRobot = transform.InverseTransformPoint(tempDestination.transform.position);
                if (tempRelatedToRobot.x >= 0 && tempRelatedToRobot.z >= 0) //primo quadrante
                {
                    transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
                }
                else if (tempRelatedToRobot.x < 0 && tempRelatedToRobot.z >= 0) //secondo quadrante
                {
                    transform.Rotate(-Vector3.up * Time.deltaTime * rotationSpeed);
                }
                else if(tempRelatedToRobot.x < 0 && tempRelatedToRobot.z < 0) //terzo quadrante
                {
                    transform.Rotate(-Vector3.up * Time.deltaTime * rotationSpeed);
                }
                else if (tempRelatedToRobot.x >= 0 && tempRelatedToRobot.z < 0) // quarto quadrante
                {
                    transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
                }
            }

            //Plane[] planes = GeometryUtility.CalculateFrustumPlanes(robotCamera);
            Ray center = new Ray(transform.position, transform.forward);
            Debug.DrawRay(transform.position, transform.forward * rangeRays, Color.red, 0.5f);
            if (/*GeometryUtility.TestPlanesAABB(planes, tempDestination.GetComponent<BoxCollider>().bounds) &&*/ Physics.Raycast(center, out hit, rangeRays, 1 << 8, QueryTriggerInteraction.Collide)
                && hit.transform.gameObject.name == "New Game Object")
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
        if (targetFound)
        {
            //Plane[] planes = GeometryUtility.CalculateFrustumPlanes(robotCamera);
            Ray center = new Ray(transform.position, transform.forward);
            Debug.DrawRay(transform.position, transform.forward * rangeRays, Color.red, 0.5f);
            if (/*GeometryUtility.TestPlanesAABB(planes, target.GetComponent<SphereCollider>().bounds) &&*/ Physics.Raycast(center, out hit, rangeRays, 1 << 8, QueryTriggerInteraction.UseGlobal)
                && hit.collider.gameObject.tag == "Target")
            {
                Debug.Log(hit.collider.gameObject);
                //float x = hit.transform.position.x / squareSize;
                //float y = hit.transform.position.z / squareSize;
                float dx = target.transform.position.x - transform.position.x;
                float dz = target.transform.position.z - transform.position.z;
                if (Mathf.Sqrt((dx * dx) + (dz * dz)) <= 5.5f)
                {
                    transform.position = transform.position;
                    transform.rotation = transform.rotation;
                    Debug.Log("Mission Complete!");
                    inGameSession = false;
                }else
                {
                    transform.position += transform.forward * Time.deltaTime * speed;
                    //transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
                    transform.rotation = transform.rotation;
                }
            }
            else
            {
                transform.position = transform.position;
                tempRelatedToRobot = transform.InverseTransformPoint(target.transform.position);
                if (tempRelatedToRobot.x >= 0 && tempRelatedToRobot.z >= 0) //primo quadrante
                {
                    transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
                }
                else if (tempRelatedToRobot.x < 0 && tempRelatedToRobot.z >= 0) //secondo quadrante
                {
                    transform.Rotate(-Vector3.up * Time.deltaTime * rotationSpeed);
                }
                else if (tempRelatedToRobot.x < 0 && tempRelatedToRobot.z < 0) //terzo quadrante
                {
                    transform.Rotate(-Vector3.up * Time.deltaTime * rotationSpeed);
                }
                else if (tempRelatedToRobot.x >= 0 && tempRelatedToRobot.z < 0) // quarto quadrante
                {
                    transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
                }
            }
        }
    }

    public void ApproachingPointToReach(Vector3 goals)
    {
        //goApproach = false;
        //perTestare(goals);
        if (tempDestination)
        {
            Destroy(tempDestination);
            tempReached = false;
        }

        tempDestination = new GameObject();

        tempDestination.transform.position = goals;
        //int desiredPos = Random.Range(0, goals.Count);
        //tempDestination = Resources.Load("Small Target") as GameObject;
        //tempDestination.transform.position = goals[desiredPos];
        //Debug.Log("Chosen point to reach is: " + goals[desiredPos]);
        tempDestination.AddComponent<BoxCollider>();
        tempDestination.GetComponent<BoxCollider>().size = new Vector3(1f, 1f, 1f);
        tempDestination.GetComponent<BoxCollider>().isTrigger = true;
        tempDestination.AddComponent<RobotSpoofedDest>();
        tempDestination.GetComponent<RobotSpoofedDest>().robot = this.gameObject;
        tempDestination.tag = "Opponent";
        tempDestination.layer = 8;
        //goForward = true;
    }

    public void ApproachingPointToReach(List<Vector3> goals, int index)
    {
        if (tempDestination)
        {
            Destroy(tempDestination);
            tempReached = false;
        }

        tempDestination = new GameObject();

        Debug.Log("List: " + goals.Count + ", starting with " + index);

        List<Vector3> supplementaryPos = new List<Vector3>();
        float distance = Mathf.Sqrt((transform.position.x - goals[index].x) * (transform.position.x - goals[index].x) + (transform.position.z - goals[index].z) * (transform.position.z - goals[index].z));
        if (Physics.Linecast(transform.position, goals[index], layerMask) || distance > rangeRays)
        {
            supplementaryPos = rPL.CheckVisibility(transform.position, goals[index]);
            if (supplementaryPos != null)
            {
                for (int i = 0; i < supplementaryPos.Count-1; i++)
                {
                    goals.Insert(index, supplementaryPos[i]);
                }
            }
        }

        tempDestination.transform.position = goals[index];
        tempDestination.AddComponent<BoxCollider>();
        tempDestination.GetComponent<BoxCollider>().size = new Vector3(1f, 1f, 1f);
        tempDestination.GetComponent<BoxCollider>().isTrigger = true;
        tempDestination.AddComponent<RobotSpoofedDestPath>();
        tempDestination.GetComponent<RobotSpoofedDestPath>().robot = this.gameObject;
        tempDestination.GetComponent<RobotSpoofedDestPath>().index = index;
        tempDestination.GetComponent<RobotSpoofedDestPath>().path = goals;
        tempDestination.tag = "Opponent";
        tempDestination.layer = 8;
    }

    public void ApproachingGoal(GameObject target)
    {
        this.target = target;
        targetFound = true;
        if (tempDestination)
            Destroy(tempDestination);
    }
}
