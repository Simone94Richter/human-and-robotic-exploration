using UnityEngine;

/// <summary>
/// Ammo Pickable is an implementation of Pickable that supplies the player whit ammo.
/// </summary>
public class AmmoPickable : Pickable {

    [SerializeField] private bool[] suppliedGuns;
    [SerializeField] private int[] ammoAmounts;

    protected override bool CanBePicked(GameObject entity) {
        return entity.GetComponent<Entity>().CanBeSupllied(suppliedGuns);
    }

    protected override void PickUp(GameObject entity) {
        entity.GetComponent<Entity>().SupplyGuns(suppliedGuns, ammoAmounts);
    }

}