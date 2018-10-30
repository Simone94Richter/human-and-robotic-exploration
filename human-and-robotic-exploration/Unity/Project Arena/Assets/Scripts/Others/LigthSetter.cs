using UnityEngine;

/// <summary>
/// LigthSetter sets the main ligth of the current scene depending on the build.
/// </summary>
[RequireComponent(typeof(Light))]
public class LigthSetter : MonoBehaviour {

    [Header("Standalone settings")] [SerializeField] LightShadows standaloneShadowType;
    [SerializeField] float standaloneLightIntensity;

    [Header("Web settings")] [SerializeField] LightShadows webShadowType;
    [SerializeField] float webLightIntensity;

    private Light sceneLigth;

    void Start() {
        sceneLigth = gameObject.GetComponent<Light>();

        if (Application.platform == RuntimePlatform.WebGLPlayer) {
            sceneLigth.shadows = webShadowType;
            sceneLigth.intensity = webLightIntensity;
        } else {
            sceneLigth.shadows = standaloneShadowType;
            sceneLigth.intensity = standaloneLightIntensity;
        }
    }

}