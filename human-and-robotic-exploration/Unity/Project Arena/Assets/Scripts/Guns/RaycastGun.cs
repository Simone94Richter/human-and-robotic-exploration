using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RaycastGun is an implementation of Gun. Since the raycast gun uses a raycast to find the hit 
/// position, there is no time of fligth for the bullet.
/// </summary>
public class RaycastGun : Gun {

    [Header("Raycast parameters")] [SerializeField] private bool limitRange = false;
    [SerializeField] private float range = 100f;
    [SerializeField] private GameObject sparkPrefab;
    [SerializeField] private float sparkDuration = 0.01f;
    [SerializeField] private LayerMask ignoredLayers;

    private Queue<GameObject> sparkList = new Queue<GameObject>();
    private GameObject sparks;

    private void Start() {
        if (!limitRange) {
            range = Mathf.Infinity;
        }

        ignoredLayers = ~ignoredLayers;

        sparks = new GameObject("Sparks - " + transform.gameObject.name);
        sparks.transform.localPosition = Vector3.zero;
    }

    protected override void Shoot() {
        StartCoroutine(ShowMuzzleFlash());

        ammoInCharger -= 1;

        // Log if needed.
        if (loggingGame) {
            ExperimentManager.Instance.LogShot(transform.root.position.x, transform.root.position.z,
                transform.root.eulerAngles.y, gunId, ammoInCharger, totalAmmo);
        }

        if (hasUI) {
            gunUIManagerScript.SetAmmo(ammoInCharger, infinteAmmo ? -1 : totalAmmo);
        }

        for (int i = 0; i < projectilesPerShot; i++) {
            RaycastHit hit;
            Vector3 direction;

            if (dispersion != 0) {
                direction = GetDeviatedDirection(headCamera.transform.forward, dispersion);
            } else {
                direction = headCamera.transform.forward;
            }

            if (Physics.Raycast(headCamera.transform.position, direction, out hit, range,
                ignoredLayers)) {
                if (!hit.transform.root.GetComponent<Player>()) {
                    StartCoroutine(ShowSpark(hit));
                    Entity entityScript = hit.transform.root.GetComponent<Entity>();
                    if (entityScript != null) {
                        entityScript.TakeDamage(damage, ownerEntityScript.GetID());
                    }
                }
            }
        }

        SetCooldown();
    }

    // Show a spark at the hit point flash.
    private IEnumerator ShowSpark(RaycastHit hit) {
        GameObject spark;
        // Retrive a spark from the list if possible, otherwise create a new one.
        if (sparkList.Count > 0) {
            spark = sparkList.Dequeue();
            spark.SetActive(true);
        } else {
            spark = (GameObject)Instantiate(sparkPrefab);
            spark.transform.parent = sparks.transform;
            spark.name = sparkPrefab.name;
        }
        // Place the spark.
        spark.transform.position = hit.point;
        spark.transform.rotation = Random.rotation;
        // Wait.
        yield return new WaitForSeconds(sparkDuration);
        // Hide the spark and put it back in the list.
        spark.SetActive(false);
        sparkList.Enqueue(spark);
    }

    // Deviates the direction randomly inside a cone with the given aperture.
    private Vector3 GetDeviatedDirection(Vector3 direction, float deviation) {
        direction = headCamera.transform.eulerAngles;
        direction.x += Random.Range(-dispersion / 2, dispersion / 2);
        direction.y += Random.Range(-dispersion / 2, dispersion / 2);
        return Quaternion.Euler(direction) * Vector3.forward;
    }

}