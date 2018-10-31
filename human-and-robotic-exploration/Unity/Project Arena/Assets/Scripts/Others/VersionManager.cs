using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VersionManager : MonoBehaviour {

    [Header("Version")] [SerializeField] private ParameterManager.BuildVersion version;
    [SerializeField]
    private string[] versionInitialScene =
        new string[Enum.GetNames(typeof(ParameterManager.BuildVersion)).Length];

    [Header("Debug")] [SerializeField] private bool resetPlayerPref = false;

    [Header("Other")] [SerializeField] private FadeUI fadeScript;    

    private void Start() {
        if (resetPlayerPref) {
            PlayerPrefs.DeleteAll();
            resetPlayerPref = false;
        }

        ParameterManager.Instance.Version = version;
        ParameterManager.Instance.InitialScene = versionInitialScene[(int)version];

        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad() {
        fadeScript.StartFade(true);
        while (!fadeScript.HasFaded()) {
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);

        fadeScript.StartFade(false);
        while (!fadeScript.HasFaded()) {
            yield return new WaitForSeconds(0.1f);
        }

        SceneManager.LoadScene(versionInitialScene[(int)version]);
    }

}