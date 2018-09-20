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

    GameObject tempDestination;
    GameObject target;

    RaycastHit hit;

    private Vector3 tempRelatedToRobot;

    // Use this for initialization
    void Start () {
        goForward = false;
        goRotation = false;
        targetFound = false;
        tempReached = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (tempDestination)
        {
            //Debug.Log(transform.InverseTransformPoint(tempDestination.transform.position));
            if (!targetFound && goForward && !goRotation && !tempReached)
            {
                transform.position += transform.forward * Time.deltaTime * speed;
                //rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, speed);
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
        if (targetFound)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(robotCamera);
            Ray center = new Ray(transform.position, transform.forward);
            Debug.DrawRay(transform.position, transform.forward * rangeRays, Color.red, 0.5f);
            if (GeometryUtility.TestPlanesAABB(planes, target.GetComponent<SphereCollider>().bounds) && Physics.Raycast(center, out hit, rangeRays)
                && hit.collider.gameObject.tag == "Target")
            {
                //float x = hit.transform.position.x / squareSize;
                //float y = hit.transform.position.z / squareSize;
                float dx = hit.point.x - this.gameObject.transform.position.x;
                float dz = hit.point.z - this.gameObject.transform.position.z;
                if (Mathf.Sqrt((dx * dx) + (dz * dz)) <= 5.5f)
                {
                    transform.position = transform.position;
                    transform.rotation = transform.rotation;
                    Debug.Log("Mission Complete!");
                    inGameSession = false;
                }
                transform.position += transform.forward * Time.deltaTime * speed;
                //rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, speed);
                transform.rotation = transform.rotation;
            }
            else
            {
                transform.position = transform.position;
                //rb.velocity = new Vector3(0,0,0);
                //Vector3 angularVelocity = new Vector3(0, speed, 0);
                transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
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
        tempDestination.GetComponent<BoxCollider>().size = new Vector3(0.5f, 1f, 0.5f);
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
        tempDestination.transform.position = goals[index];
        //int desiredPos = Random.Range(0, goals.Count);
        //tempDestination = Resources.Load("Small Target") as GameObject;
        //tempDestination.transform.position = goals[desiredPos];
        //Debug.Log("Chosen point to reach is: " + goals[desiredPos]);
        tempDestination.AddComponent<BoxCollider>();
        tempDestination.GetComponent<BoxCollider>().size = new Vector3(0.5f, 1f, 0.5f);
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
        if (tempDestination)
            Destroy(tempDestination);
    }
}
