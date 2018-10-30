using UnityEngine;

/// <summary>
/// RotateTranslateByAxis allows to perform periodic roto-translations on the gameobject it is 
/// attached to.
/// </summary>
public class RotateTranslateByAxis : MonoBehaviour {

    [SerializeField] private bool xTranslation = false;
    [SerializeField] private float xLength = 0f;
    [SerializeField] private float xTransSpeed = 0f;

    [SerializeField] private bool zTranslation = false;
    [SerializeField] private float zLength = 0f;
    [SerializeField] private float zTransSpeed = 0f;

    [SerializeField] private bool yTranslation = false;
    [SerializeField] private float yLength = 0f;
    [SerializeField] private float yTransSpeed = 0f;

    [SerializeField] private bool xRotation = false;
    [SerializeField] private float xRotSpeed = 0f;

    [SerializeField] private bool zRotation = false;
    [SerializeField] private float zRotSpeed = 0f;

    [SerializeField] private bool yRotation = false;
    [SerializeField] private float yRotSpeed = 0f;

    private Vector3 limitMax;
    private Vector3 limitMin;

    private bool xBack = false;
    private bool zBack = false;
    private bool yBack = false;

    void Start() {
        limitMax.x = transform.position.x + xLength / 2;
        limitMin.x = transform.position.x - xLength / 2;
        limitMax.y = transform.position.y + yLength / 2;
        limitMin.y = transform.position.y - yLength / 2;
        limitMax.z = transform.position.z + zLength / 2;
        limitMin.z = transform.position.z - zLength / 2;
    }

    // Rotates and translates the object with respect to the specified axis.
    void Update() {
        if (xTranslation) {
            if (xBack == false)
                transform.position = transform.position + transform.right * xTransSpeed * Time.deltaTime;
            else
                transform.position = transform.position - transform.right * xTransSpeed * Time.deltaTime;
            if (transform.position.x > limitMax.x && xBack == false)
                xBack = true;
            else if (transform.position.x < limitMin.x && xBack == true)
                xBack = false;
        }

        if (zTranslation) {
            if (zBack == true)
                transform.position = transform.position + transform.forward * zTransSpeed * Time.deltaTime;
            else
                transform.position = transform.position - transform.forward * zTransSpeed * Time.deltaTime;
            if (transform.position.z > limitMax.z && zBack == true)
                zBack = true;
            else if (transform.position.z < limitMin.z && zBack == false)
                zBack = false;
        }

        if (yTranslation) {
            if (yBack == true)
                transform.position = transform.position + transform.up * yTransSpeed * Time.deltaTime;
            else
                transform.position = transform.position - transform.up * yTransSpeed * Time.deltaTime;
            if (transform.position.y > limitMax.y && yBack == true)
                yBack = true;
            else if (transform.position.y < limitMin.y && yBack == false)
                yBack = false;
        }

        if (xRotation)
            transform.Rotate(Vector3.right * Time.deltaTime * xRotSpeed);

        if (zRotation)
            transform.Rotate(Vector3.forward * Time.deltaTime * zRotSpeed);

        if (yRotation)
            transform.Rotate(Vector3.up * Time.deltaTime * yRotSpeed);
    }

    public Quaternion GetRotation() {
        return transform.rotation;
    }

    public void SetRotation(Quaternion rotation) {
        transform.rotation = rotation;
    }

}