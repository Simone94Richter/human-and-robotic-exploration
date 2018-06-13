/// <summary>
/// Opponent is a simple implementation of Entity with no extra logic.
/// </summary>
public class Opponent : Entity {

    // Sets up all the opponent parameters and does the same with all its guns.
    public override void SetupEntity(int th, bool[] ag, GameManager gms, int id) {
        activeGuns = ag;
        gameManagerScript = gms;

        totalHealth = th;
        health = th;
        entityID = id;

        for (int i = 0; i < ag.GetLength(0); i++) {
            // Setup the gun.
            guns[i].GetComponent<Gun>().SetupGun(gms, this);
            // Activate if it is one among the active ones which has the lowest rank.
            if (i == GetActiveGun(-1, true)) {
                currentGun = i;
                guns[i].SetActive(true);
            }
        }
    }

    // Sets up all the opponent parameters and does the same with all its guns.
    public override void SetupEntity(GameManager gms, int id) {
        SetupEntity(totalHealth, activeGuns, gms, id);
    }

    // Applies damage to the opponent and eventually manages its death.
    public override void TakeDamage(int damage, int killerID) {
        if (inGame) {
            health -= damage;

            // If the health goes under 0, kill the entity and start the respawn process.
            if (health <= 0f) {
                health = 0;
                // Kill the entity.
                Die(killerID);
            }
        }
    }

    // Heals the opponent.
    public override void Heal(int restoredHealth) {
        if (health + restoredHealth > totalHealth)
            health = totalHealth;
        else
            health += restoredHealth;
    }

    // Kills the opponent.
    protected override void Die(int id) {
        gameManagerScript.AddScore(id, entityID);
        SetInGame(false);
        // Start the respawn process.
        gameManagerScript.MenageEntityDeath(gameObject, this);
    }

    // Respawns the opponent.
    public override void Respawn() {
        health = totalHealth;
        ResetAllAmmo();
        SetInGame(true);
    }

    // Sets if the opponent is in game.
    public override void SetInGame(bool b) {
        SetIgnoreRaycast(!b);
        SetMeshVisible(transform, b);
        inGame = b;
    }

    public override void SlowEntity(float penalty) { }

}