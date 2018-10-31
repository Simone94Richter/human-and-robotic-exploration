using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MapManager is an abstract class used to implement any kind of map manager. A map manager manages
/// the creation process of a map, which consists in generation (performed by a map generator), 
/// assembling (performed by a map assembler) and population (performed by an object displacer).
/// </summary>
public abstract class MapManager : CoreComponent {

    [Header("Core components")]
    [SerializeField]
    protected MapGenerator mapGeneratorScript;
    [SerializeField]
    protected MapAssembler mapAssemblerScript;
    [SerializeField]
    protected ObjectDisplacer objectDisplacerScript;
    [SerializeField]
    protected Robot robot;
    [SerializeField]
    protected Player player;

    [Header("Import")]
    // Do I have to load the map from a .txt?
    [SerializeField]
    protected bool loadMapFromFile = false;
    //Do I have to load a numeric map?
    [SerializeField]
    protected bool isNumeric = false;
    // Path of the map to be laoded.
    [SerializeField]
    protected string textFilePath = null;

    [Header("Other")]
    // Category of the spawn point gameobjects in the object displacer. 
    [SerializeField]
    protected string spawnPointCategory;

    protected string seed;
    protected bool export;
    protected string exportPath;
    protected bool flip;

    private void Start() {
        ExtractParametersFromManager();
    }

    private void Update() {
        if (!IsReady() && mapAssemblerScript.IsReady() && mapGeneratorScript.IsReady() &&
             objectDisplacerScript.IsReady()) {
            SetReady(true);
        }
    }

    public abstract void ManageMap(bool assembleMap);

    // Returns the spawn points.k
    public List<GameObject> GetSpawnPoints() {
        return objectDisplacerScript.GetObjectsByCategory(spawnPointCategory);
    }

    public List<GameObject> GetTargetPoint()
    {
        return objectDisplacerScript.GetObjectsByCategory("Target Points");
    }

    // Loads the map from a text file.
    protected abstract void LoadMapFromText();

    // Extracts the parameters from the parameter Manager, if any.
    protected void ExtractParametersFromManager() {
        if (ParameterManager.HasInstance()) {
            export = ParameterManager.Instance.Export;
            exportPath = ParameterManager.Instance.ExportPath;
            flip = ParameterManager.Instance.Flip;

            switch (ParameterManager.Instance.GenerationMode) {
                case 0:
                    loadMapFromFile = false;
                    seed = ParameterManager.Instance.MapDNA;
                    break;
                case 1:
                    loadMapFromFile = false;
                    seed = ParameterManager.Instance.MapDNA;
                    break;
                case 2:
                    loadMapFromFile = true;
                    seed = null;
                    textFilePath = ParameterManager.Instance.MapDNA;
                    break;
                case 3:
                    loadMapFromFile = false;
                    seed = ParameterManager.Instance.MapDNA;
                    textFilePath = ParameterManager.Instance.MapDNA;
                    break;
                case 4:
                    loadMapFromFile = true;
                    seed = ParameterManager.Instance.MapDNA;
                    textFilePath = null;
                    break;
            }
        }
    }

    // Returns the Map Generator.
    public MapGenerator GetMapGenerator() {
        return mapGeneratorScript;
    }

    // Returns the Map Assembler.
    public MapAssembler GetMapAssembler() {
        return mapAssemblerScript;
    }

    public bool GetFlip() {
        return flip;
    }

}