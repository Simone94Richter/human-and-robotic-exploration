using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entity is an abstract class used to implement any kind of agent. An entity can spawn, recive
/// damage and die. An entity can also be healed, supplied with ammunitions and have weapons.
/// </summary>
public abstract class Entity : MonoBehaviour {

    [SerializeField]
    protected int totalHealth;
    [SerializeField]
    protected int disabledLayer;
    [SerializeField]
    protected List<GameObject> guns;
    [SerializeField]
    protected bool[] activeGuns;

    protected int health;
    protected int entityID;
    protected int currentGun;
    protected bool inGame = false;
    protected int originalLayer;

    protected GameManager gameManagerScript;

    // Sets all the entity parameters.
    public abstract void SetupEntity(int th, bool[] ag, GameManager gms, int id);

    // Sets all the entity parameters.
    public abstract void SetupEntity(GameManager gms, int id);

    // Applies damage to the entity and eventually manages its death.
    public abstract void TakeDamage(int damage, int killerID);

    // Kills the entity.
    protected abstract void Die(int id);

    // Respawns the entity.
    public abstract void Respawn();

    // Slows down the entity.
    public abstract void SlowEntity(float penalty);

    // Returns the next or the previous active gun.
    protected int GetActiveGun(int currentGun, bool next) {
        if (next) {
            // Try for the guns after it
            for (int i = currentGun + 1; i < guns.Count; i++) {
                if (activeGuns[i]) {
                    return i;
                }
            }
            // Try for the guns before it
            for (int i = 0; i < currentGun; i++) {
                if (activeGuns[i]) {
                    return i;
                }
            }
            // There's no other gun, return itself.
            return currentGun;
        } else {
            // Try for the guns before it
            for (int i = currentGun - 1; i >= 0; i--) {
                if (activeGuns[i]) {
                    return i;
                }
            }
            // Try for the guns after it
            for (int i = guns.Count - 1; i > currentGun; i--) {
                if (activeGuns[i]) {
                    return i;
                }
            }
            // There's no other gun, return itself.
            return currentGun;
        }
    }

    // If the entity is enabled, tells if the it has full health.
    public bool CanBeHealed() {
        return health < totalHealth && inGame;
    }

    // Heals the entity.
    public abstract void Heal(int restoredHealth);

    // If the entity is enabled, tells if any of the weapons passed as 
    // parameters hasn't the maximum ammo.
    public bool CanBeSupllied(bool[] suppliedGuns) {
        if (inGame) {
            for (int i = 0; i < suppliedGuns.GetLength(0); i++) {
                if (suppliedGuns[i] && activeGuns[i] && !guns[i].GetComponent<Gun>().IsFull()) {
                    return true;
                }
            }
        }
        return false;
    }

    // Increases the ammo of the available guns.
    public void SupplyGuns(bool[] suppliedGuns, int[] ammoAmounts) {
        for (int i = 0; i < suppliedGuns.GetLength(0); i++) {
            if (suppliedGuns[i] && activeGuns[i] && !guns[i].GetComponent<Gun>().IsFull()) {
                guns[i].GetComponent<Gun>().AddAmmo(ammoAmounts[i]);
            }
        }
    }

    // Sets if the entity is in game, i.e. if it can move, shoot, interact
    // with object and be hitten.
    abstract public void SetInGame(bool b);

    // Returns the ID of the entity.
    public int GetID() {
        return entityID;
    }

    // Hides/shows the meshe.
    protected void SetMeshVisible(Transform father, bool isVisible) {
        foreach (Transform children in father) {
            if (children.GetComponent<MeshRenderer>()) {
                children.GetComponent<MeshRenderer>().enabled = isVisible;
            }
            SetMeshVisible(children, isVisible);
        }
    }

    // Sets if the entity must be ignored by raycast.
    protected void SetIgnoreRaycast(bool mustIgnore) {
        if (mustIgnore) {
            originalLayer = gameObject.layer;
            gameObject.layer = disabledLayer;
            ChangeLayersRecursively(transform, disabledLayer);
        } else {
            gameObject.layer = originalLayer;
            ChangeLayersRecursively(transform, originalLayer);
        }
    }

    // Changes the layer recursively.
    protected void ChangeLayersRecursively(Transform t, int l) {
        foreach (Transform child in t) {
            child.gameObject.layer = l;
            ChangeLayersRecursively(child, l);
        }
    }

    // Resets the ammo of all the weapons.
    protected void ResetAllAmmo() {
        for (int i = 0; i < guns.Count; i++) {
            if (activeGuns[i]) {
                guns[i].GetComponent<Gun>().ResetAmmo();
            }
        }
    }

}