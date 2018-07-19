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
    bool targetFound;
    bool tempReached;

    GameObject tempDestination;

    RaycastHit hit;

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

    public void ApproachingPointToReach(List<Vector3> goals)
    {
        //goApproach = false;
        //perTestare(goals);
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
}
