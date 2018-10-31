using UnityEngine;

/// <summary>
/// ParameterManager allows to exchange information between different scenes.
/// </summary>
public class ParameterManager : SceneSingleton<ParameterManager> {

    public enum BuildVersion { COMPLETE, GAME_ONLY, EXPERIMENT_CONTROL, EXPERIMENT_ONLY };

    // Map data.
    [HideInInspector] public int GenerationMode { get; set; }
    [HideInInspector] public string MapDNA { get; set; }
    [HideInInspector] public bool Flip { get; set; }

    // Export data.
    [HideInInspector] public bool Export { get; set; }
    [HideInInspector] public string ExportPath { get; set; }

    // Error data.
    [HideInInspector] public int ErrorCode { get; set; }
    [HideInInspector] public string ErrorMessage { get; set; }

    // Experiment data.
    [HideInInspector] public bool LogOnline { get; set; }
    [HideInInspector] public bool LogOffline { get; set; }
    [HideInInspector] public bool LogSetted { get; set; }
    [HideInInspector] public string ExperimentControlScene { get; set; }

    // Mouse sensibility data.
    [HideInInspector] public float MinSensibility { get; internal set; }
    [HideInInspector] public float MaxSensibility { get; internal set; }
    [HideInInspector] public float DefaultSensibility { get; internal set; }
    [HideInInspector] public float WebSensibilityDownscale { get; internal set; }

    // Other data.
    [HideInInspector] public Quaternion BackgroundRotation { get; set; }
    [HideInInspector] public BuildVersion Version { get; set; }
    [HideInInspector] public string InitialScene { get; set; }

    void Awake() {
        ErrorCode = 0;

        LogOnline = true;
        LogOffline = true;
        LogSetted = false;

        MinSensibility = 0.75f;
        MaxSensibility = 2.75f;
        DefaultSensibility = 1.75f;
        WebSensibilityDownscale = 3f;

        DontDestroyOnLoad(transform.gameObject);
    }

}