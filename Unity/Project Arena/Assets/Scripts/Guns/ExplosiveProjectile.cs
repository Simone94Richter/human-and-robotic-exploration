using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ExplosiveProjectile is an implementation of Projectule. An explosive projectile explodes on
/// impact and deals area damage.
/// </summary>
public class ExplosiveProjectile : Projectile {

    [Header("Raycast parameters")] [SerializeField] private List<GameObject> explosionList;
    [SerializeField] private float explosionDuration;
    [SerializeField] private float explosionRadius;

    private void OnTriggerEnter(Collider other) {
        // Ignore all the ignore raycast objects.
        if (other.gameObject.layer != 2) {
            shot = false;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider c in hitColliders) {
                Entity entityScript = c.gameObject.transform.root.GetComponent<Entity>();
                if (entityScript != null) {
                    int scaledDamage = (int)(damage * (1 - Vector3.Distance(c.transform.position, 
                        transform.position) / explosionRadius));
                    if (scaledDamage > 0) {
                        entityScript.TakeDamage(scaledDamage, shooterID);
                    }
                }
            }

            StartCoroutine(AnimateExplosion());
        }
    }

    // Animates the explosion effect activating an explosion at a time.
    public IEnumerator AnimateExplosion() {
        projectile.SetActive(false);

        for (int i = 0; i < explosionList.Count; i++) {
            if (i > 0) {
                explosionList[i - 1].SetActive(false);
            }
            explosionList[i].SetActive(true);
            yield return new WaitForSeconds(explosionDuration / explosionList.Count);
        }

        explosionList[explosionList.Count - 1].SetActive(false);

        Recover();
    }

}