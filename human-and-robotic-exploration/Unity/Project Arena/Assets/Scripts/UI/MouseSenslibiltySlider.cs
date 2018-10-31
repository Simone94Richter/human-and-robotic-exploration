using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class manages the mouse sensibility slider.
/// </summary>
[RequireComponent(typeof(Slider))]
public class MouseSenslibiltySlider : MonoBehaviour {

    private Slider slider;

    void OnEnable() {
        slider = GetComponent<Slider>();
        slider.minValue = ParameterManager.Instance.MinSensibility;
        slider.maxValue = ParameterManager.Instance.MaxSensibility;

        if (PlayerPrefs.HasKey("MouseSensibility")) {
            slider.value = PlayerPrefs.GetFloat("MouseSensibility");
        } else {
            slider.value = ParameterManager.Instance.DefaultSensibility;
            PlayerPrefs.SetFloat("MouseSensibility", slider.value);
        }
    }

    public void SetMouseSensibility() {
        if (slider != null) {
            PlayerPrefs.SetFloat("MouseSensibility", slider.value);
            if (GameObject.Find("Player") != null && 
                GameObject.Find("Player").GetComponent<Player>() != null) {
                GameObject.Find("Player").GetComponent<Player>().SetSensibility(slider.value);
            }
        }
    }

}
