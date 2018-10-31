using UnityEngine;

/// <summary>
/// Pickable is an abstract class used to implement any kind of object that can be picked up by 
/// the player.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public abstract class Pickable : MonoBehaviour {

    [SerializeField] protected float cooldown = 30f;
    [SerializeField] protected GameObject pickable;

    protected float pickedUpTime = 0f;
    protected bool isActive = true;

    protected float lastCheck = 0f;
    protected float checkWait = 0.1f;

    protected bool defaultShaderSet = false;

    // Use this for initialization
    protected void Start() {
        ActivatePickable();
    }

    // Update is called once per frame
    protected void Update() {
        if (!isActive && Time.time > pickedUpTime + cooldown) {
            ActivatePickable();
        }
    }

    protected void OnTriggerStay(Collider other) {
        // Menage the interaction with the player.
        if (other.gameObject.tag == "Player" && Time.time > lastCheck + checkWait) {
            if (CanBePicked(other.gameObject) && isActive) {
                PickUp(other.gameObject);
                DeactivatePickable();
            }

            lastCheck = Time.time;
        }
    }

    // Tells if the player really needs the pickable.
    abstract protected bool CanBePicked(GameObject player);

    // Gives to the player the content of the pickable.
    abstract protected void PickUp(GameObject player);

    // Activates the pickable.
    protected void ActivatePickable() {
        pickable.SetActive(true);
        isActive = true;
    }

    // Deactivate the pickable.
    protected void DeactivatePickable() {
        pickable.SetActive(false);
        pickedUpTime = Time.time;
        isActive = false;
    }

}