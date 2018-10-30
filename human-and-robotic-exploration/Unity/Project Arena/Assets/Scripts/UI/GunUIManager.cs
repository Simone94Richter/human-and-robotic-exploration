using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class manages the UI of a gun, i.e. its ammo.
/// </summary>
public class GunUIManager : MonoBehaviour {

    [SerializeField] private GameObject ammo;

    private string infinite;

    private void Awake() {
        if (Application.platform == RuntimePlatform.WebGLPlayer) {
            infinite = "INF";
        } else {
            infinite = "∞";
        }
    }

    // Sets the ammo.
    public void SetAmmo(int charger, int tot) {
        ammo.GetComponent<Text>().text = charger.ToString() + "/" +
            ((tot == -1) ? infinite : tot.ToString());
    }

}