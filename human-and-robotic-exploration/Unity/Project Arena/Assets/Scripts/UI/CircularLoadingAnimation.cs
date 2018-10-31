using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CircularLoadingAnimation applies a period circular fill animation to an image.
/// </summary>
[RequireComponent(typeof(Image))]
public class CircularLoadingAnimation : MonoBehaviour {

    [SerializeField] private float speed = 1f;

    private Image circle;

    private void Awake() {
        circle = transform.GetComponent<Image>();
    }

    private void OnEnable() {
        circle.fillAmount = 0;
        circle.fillClockwise = true;
    }

    void Update () {
        if (circle.fillClockwise) {
            circle.fillAmount += Time.deltaTime * speed;
            if (circle.fillAmount >= 1) {
                circle.fillAmount = 1;
                circle.fillClockwise = false;
            }
        } else {
            circle.fillAmount -= Time.deltaTime * speed;
            if (circle.fillAmount <= 0) {
                circle.fillAmount = 0;
                circle.fillClockwise = true;
            }
        }
    }

}