using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class manages the UI of the player. Health and available weapons are always visible, 
/// whereas an hitted indicator and a reload animation can be displayer when needed.
/// </summary>
public class PlayerUIManager : MonoBehaviour {

    [SerializeField] private GameObject health;
    [SerializeField] private GameObject cooldown;
    [SerializeField] private GameObject damage;
    [SerializeField] private GameObject[] gunNumbers;
    [SerializeField] private Color[] gunColors;

    // Variables for the cooldawn.
    private float cooldownDuration = 0f;
    private float cooldownStart = 0f;
    private bool mustCooldown = false;

    // Variables for the damage.
    private float damageDuration = 0.25f;
    private float damageWait = 0.5f;
    private float damageStart = 0f;

    private void Update() {
        // Update the cooldown bar if needed.
        if (mustCooldown) {
            if (Time.time >= cooldownStart + cooldownDuration) {
                cooldown.GetComponent<Image>().fillAmount = 0;
                mustCooldown = false;
            } else {
                cooldown.GetComponent<Image>().fillAmount = (Time.time - cooldownStart) /
                    cooldownDuration;
            }
        }
    }

    // Sets the health.
    public void SetHealth(int currenHealth, int totalHealth) {
        health.GetComponent<Text>().text = currenHealth.ToString() + "/" + totalHealth.ToString();
    }

    // Sets the active guns.
    public void SetActiveGuns(bool[] activeGuns) {
        for (int i = 0; i < activeGuns.GetLength(0); i++) {
            if (activeGuns[i]) {
                SetTextAlpha(gunNumbers[i], 1);
            } else {
                SetTextAlpha(gunNumbers[i], 0.5f);
            }
        }
    }

    // Sets the alpha of the text in the parameter object.
    private void SetTextAlpha(GameObject gameObject, float alpha) {
        Color c = gameObject.GetComponent<Text>().color;
        c.a = alpha;
        gameObject.GetComponent<Text>().color = c;
    }

    // Sets the cooldown.
    public void SetCooldown(float d) {
        cooldownDuration = d;
        cooldownStart = Time.time;
        mustCooldown = true;
    }

    // Stops the reloading.
    public void StopReloading() {
        cooldown.GetComponent<Image>().fillAmount = 0;
        mustCooldown = false;
    }

    // Shows/hidels all the interface elements.
    public void SetPlayerUIVisible(bool b) {
        health.SetActive(b);
        cooldown.SetActive(b);
        foreach (GameObject g in gunNumbers) {
            g.SetActive(b);
        }
    }

    // Returns the color of a gun.
    public Color GetGunColor(int i) {
        return gunColors[i];
    }

    // Sets the color of all the UI elements (except health).
    public void SetColorAll(Color c) {
        cooldown.GetComponent<Image>().color = c;
        foreach (GameObject gn in gunNumbers) {
            gn.GetComponent<Text>().color = new Color(c.r, c.g, c.b,
                gn.GetComponent<Text>().color.a);
        }
    }

    // Shows the damage indicator.
    public void ShowDamage() {
        if (Time.time > damageStart + damageWait + damageDuration) {
            damageStart = Time.time;
            StartCoroutine(DamageAnimation());
        }
    }

    private IEnumerator DamageAnimation() {
        damage.SetActive(true);
        yield return new WaitForSeconds(damageDuration);
        damage.SetActive(false);
    }

}