using UnityEngine;

/// <summary>
/// Projectile is an abstract class used to implement any kind of projectile.
/// </summary>
public abstract class Projectile : MonoBehaviour {

    [SerializeField] protected GameObject projectile;

    protected ProjectileGun projectileGunScript;
    protected float shotTime;
    protected float projectileLifeTime;
    protected float projectileSpeed;
    protected bool shot;
    protected float damage;
    protected int shooterID;

    private void Update() {
        if (shot) {
            transform.position += transform.forward * Time.deltaTime * projectileSpeed;

            if (Time.time > shotTime + projectileLifeTime) {
                Recover();
            }
        }
    }

    // Sets the projectile.
    public void SetupProjectile(float plt, float ps, ProjectileGun pg, float d, int s) {
        projectileLifeTime = plt;
        projectileSpeed = ps;
        projectileGunScript = pg;
        damage = d;
        shooterID = s;
    }

    // Fires the projectile.
    public void Fire(Vector3 position, Quaternion direction) {
        transform.position = position;
        transform.rotation = direction;

        shot = true;
        shotTime = Time.time;
        gameObject.SetActive(true);
        projectile.SetActive(true);
    }

    // Recovers the projectile.
    public void Recover() {
        shot = false;
        projectileGunScript.RecoverProjectile(gameObject);
        gameObject.SetActive(false);
    }

}