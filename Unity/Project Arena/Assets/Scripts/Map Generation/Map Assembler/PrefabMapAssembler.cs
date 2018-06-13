using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PrefabMapAssembler is an implementation of MapAssembler that assembles the maps using differet 
/// prefabs.
/// </summary>
public abstract class PrefabMapAssembler : MapAssembler {

    [Header("Prefab parameters")]
    // Ceil height.
    [SerializeField]
    protected float ceilHeight = 0;
    // Floor height.
    [SerializeField]
    protected float floorHeight = 0;
    // Rotation correction angle.
    [SerializeField]
    protected int rotationCorrection = 0;
    // List of prefabs.
    [SerializeField]
    protected List<TilePrefab> tilePrefabs;

    // List of processed prefabs.
    protected List<ProcessedTilePrefab> processedTilePrefabs;
    // Char that denotes a wall tile.
    protected char wallChar;
    // Char that denotes a room tile.
    protected char roomChar;
    // Map width.
    protected int width;
    // Map heigth.
    protected int height;

    // Adds a prefab to the map.
    protected void AddPrefab(GameObject gameObject, int x, int y, float squareSize, float rotation,
        float prefabHeight) {
        GameObject childObject = (GameObject)Instantiate(gameObject);
        childObject.name = gameObject.name;
        childObject.transform.parent = transform;
        childObject.transform.position = new Vector3(squareSize * x, prefabHeight,
            squareSize * y);
        childObject.transform.eulerAngles = new Vector3(0, rotation, 0);
    }

    // For each tile, converts its binary mask into a char array and creates three rotated copies.
    protected void ProcessTiles() {
        processedTilePrefabs = new List<ProcessedTilePrefab>();

        // For each tile create three rotated copies.
        foreach (TilePrefab t in tilePrefabs) {
            string convertedMask = ConvertMask(t.binaryMask);
            processedTilePrefabs.Add(new ProcessedTilePrefab(convertedMask, t.prefab,
                rotationCorrection));
            // Debug.Log("Added mask " + convertedMask + ".");
            if (t.binaryMask != "0000" && t.binaryMask != "1111") {
                for (int i = 1; i < 4; i++) {
                    convertedMask = CircularShiftMask(convertedMask);
                    processedTilePrefabs.Add(new ProcessedTilePrefab(convertedMask, t.prefab,
                        90 * i + rotationCorrection));
                    // Debug.Log("Added mask " + convertedMask + ".");
                }
            }
        }
    }

    // Converts the mask form a binary string to char array.
    protected string ConvertMask(string binaryMask) {
        binaryMask = binaryMask.Replace('0', roomChar);
        binaryMask = binaryMask.Replace('1', wallChar);
        return binaryMask;
    }

    // Performs a circular shift of the mask.
    protected string CircularShiftMask(string mask) {
        char[] shiftedMask = { mask[3], mask[0], mask[1], mask[2] };
        return new string(shiftedMask);
    }

    // Creates a flat mesh.
    protected Mesh CreateFlatMesh(int sizeX, int sizeY, float squareSize, float height,
        bool inverted) {
        Mesh flatMesh = new Mesh();

        Vector3[] floorVertices = new Vector3[4];

        floorVertices[0] = new Vector3(-squareSize, height, -squareSize);
        floorVertices[1] = new Vector3(-squareSize, height, sizeY * squareSize + squareSize);
        floorVertices[2] = new Vector3(sizeX * squareSize + squareSize, height, -squareSize);
        floorVertices[3] = new Vector3(sizeX * squareSize + squareSize, height,
            sizeY * squareSize + squareSize);

        int[] floorTriangles;

        if (inverted) {
            floorTriangles = new int[] { 3, 1, 2, 2, 1, 0 };
        } else {
            floorTriangles = new int[] { 0, 1, 2, 2, 1, 3 };
        }

        flatMesh.vertices = floorVertices;
        flatMesh.triangles = floorTriangles;
        flatMesh.RecalculateNormals();

        return flatMesh;
    }

    // Custom prefab. 
    [Serializable]
    protected struct TilePrefab {
        // Mask of the tile.
        public string binaryMask;
        // Prefab of the tile.
        public GameObject prefab;
    }

    // Custom processed prefab. 
    [Serializable]
    public class ProcessedTilePrefab {
        // Mask of the tile.
        public string mask;
        // Prefab of the tile.
        public GameObject prefab;
        // Rotation
        public int rotation;

        public ProcessedTilePrefab(string m, GameObject p, int r) {
            mask = m;
            prefab = p;
            rotation = r;
        }
    }

}