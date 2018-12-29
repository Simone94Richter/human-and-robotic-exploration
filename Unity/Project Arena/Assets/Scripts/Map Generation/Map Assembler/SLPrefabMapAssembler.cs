using MapManipulation;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// SLPrefabMapAssembler is an implementation of PrefabMapAssembler for single-level maps.
/// </summary>
public class SLPrefabMapAssembler : PrefabMapAssembler {

    [SerializeField]
    private List<ColoredTilesPrefab> coloredTiles;

    private List<ProcessedColoredTilePrefab> processedColoredTilePrefabs;

    // Char Map.
    private char[,] map;
    // Float Map
    private float[,] numeric_map;

    private MeshCollider floorCollider;
    private MeshCollider ceilCollider;

    void Start() {
        GameObject childObject;

        childObject = new GameObject("Floor - Collider");
        childObject.transform.parent = transform;
        childObject.transform.localPosition = Vector3.zero;
        floorCollider = childObject.AddComponent<MeshCollider>();

        childObject = new GameObject("Ceil - Collider");
        childObject.transform.parent = transform;
        childObject.transform.localPosition = Vector3.zero;
        ceilCollider = childObject.AddComponent<MeshCollider>();

        SetReady(true);
    }

    public override void AssembleMap(char[,] m, char wChar, char rChar) {
        wallChar = wChar;
        roomChar = rChar;
        width = m.GetLength(0);
        height = m.GetLength(1);
        map = m;

        // Process all the tiles.
        ProcessTiles();

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (map[x, y] != wallChar) {
                    string currentMask = GetNeighbourhoodMask(x, y);
                    foreach (ProcessedTilePrefab p in processedTilePrefabs) {
                        if (p.mask == currentMask) {
                            AddPrefab(p.prefab, x, y, squareSize, p.rotation, wallHeight);
                            break;
                        }
                    }
                }
            }
        }

        // Generate floor and ceil colliders.
        floorCollider.sharedMesh = CreateFlatMesh(width, height, squareSize, wallHeight +
            floorHeight, false);
        ceilCollider.sharedMesh = CreateFlatMesh(width, height, squareSize, wallHeight +
            ceilHeight, true);
    }

    public override void AssembleMap(float[,] m, float wNumb, float rNumb)
    {
        wallNumb = wNumb;
        roomNumb = rNumb;
        width = m.GetLength(0);
        height = m.GetLength(1);
        numeric_map = m;

        //Process all the tiles
        ProcessTilesNumb();
        ProcessColoredTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (numeric_map[x, y] == 53)
                {
                    string currentMask = GetNeighbourhoodMaskNumb(x, y);
                    foreach (ProcessedTilePrefab p in processedColoredTilePrefabs[0].coloredProcessedTilePrefabs)
                    {
                        //Debug.Log(p.mask + ", " + currentMask);
                        if (p.mask == currentMask)
                        {
                            AddPrefab(p.prefab, x, y, squareSize, p.rotation, wallHeight);
                            break;
                        }
                    }
                }
                else if (numeric_map[x, y] == 54)
                {
                    string currentMask = GetNeighbourhoodMaskNumb(x, y);
                    foreach (ProcessedTilePrefab p in processedColoredTilePrefabs[1].coloredProcessedTilePrefabs)
                    {
                        //Debug.Log(p.mask);
                        if (p.mask == currentMask)
                        {
                            AddPrefab(p.prefab, x, y, squareSize, p.rotation, wallHeight);
                            break;
                        }
                    }
                }
                else if (numeric_map[x, y] == 55)
                {
                    string currentMask = GetNeighbourhoodMaskNumb(x, y);
                    foreach (ProcessedTilePrefab p in processedColoredTilePrefabs[2].coloredProcessedTilePrefabs)
                    {
                        //Debug.Log(p.mask);
                        if (p.mask == currentMask)
                        {
                            AddPrefab(p.prefab, x, y, squareSize, p.rotation, wallHeight);
                            break;
                        }
                    }
                }
                else if (numeric_map[x, y] == 56)
                {
                    string currentMask = GetNeighbourhoodMaskNumb(x, y);
                    foreach (ProcessedTilePrefab p in processedColoredTilePrefabs[3].coloredProcessedTilePrefabs)
                    {
                        //Debug.Log(p.mask);
                        if (p.mask == currentMask)
                        {
                            AddPrefab(p.prefab, x, y, squareSize, p.rotation, wallHeight);
                            break;
                        }
                    }
                }
                else if (numeric_map[x, y] != wallNumb)
                {
                    string currentMask = GetNeighbourhoodMaskNumb(x, y);
                    foreach (ProcessedTilePrefab p in processedTilePrefabs)
                    {
                        if (p.mask == currentMask)
                        {
                            //Debug.Log(p.mask);
                            AddPrefab(p.prefab, x, y, squareSize, p.rotation, wallHeight);
                            break;
                        }
                    }
                }
            }
        }

        // Generate floor and ceil colliders.
        floorCollider.sharedMesh = CreateFlatMesh(width, height, squareSize, wallHeight +
            floorHeight, false);
        ceilCollider.sharedMesh = CreateFlatMesh(width, height, squareSize, wallHeight +
            ceilHeight, true);
    }

    private void ProcessColoredTiles()
    {
        processedColoredTilePrefabs = new List<ProcessedColoredTilePrefab>();

        foreach (ColoredTilesPrefab c in coloredTiles)
        {
            List<ProcessedTilePrefab> list = new List<ProcessedTilePrefab>();

            foreach (TilePrefab cT in c.coloredTilePrefabs)
            {
                string convertedMask = cT.binaryMask;
                list.Add(new ProcessedTilePrefab(convertedMask, cT.prefab, rotationCorrection));

                if (cT.binaryMask != "0000" && cT.binaryMask != "1111")
                {
                    //Debug.Log(cT.binaryMask);
                    for (int j = 1; j < 4; j++)
                    {
                        convertedMask = CircularShiftMask(convertedMask);
                        //Debug.Log(convertedMask);
                        list.Add(new ProcessedTilePrefab(convertedMask, cT.prefab, 90 * j + rotationCorrection));
                    }
                }
            }

            processedColoredTilePrefabs.Add(new ProcessedColoredTilePrefab(list));
        }
    }

    public override void AssembleMap(List<char[,]> maps, char wallChar, char roomChar,
        char voidChar) { }

    // Gets the neighbours of a cell as a mask.
    protected string GetNeighbourhoodMask(int gridX, int gridY) {
        char[] mask = new char[4];
        mask[0] = GetTileChar(gridX, gridY + 1);
        mask[1] = GetTileChar(gridX + 1, gridY);
        mask[2] = GetTileChar(gridX, gridY - 1);
        mask[3] = GetTileChar(gridX - 1, gridY);
        return new string(mask);
    }

    protected string GetNeighbourhoodMaskNumb(int gridX, int gridY)
    {
        float[] mask = new float[4];
        mask[0] = GetTileFloat(gridX, gridY + 1);
        mask[1] = GetTileFloat(gridX + 1, gridY);
        mask[2] = GetTileFloat(gridX, gridY - 1);
        mask[3] = GetTileFloat(gridX - 1, gridY);
        //Debug.Log(mask[0].ToString() + mask[1].ToString() + mask[2].ToString() + mask[3].ToString());
        return mask[0].ToString() + mask[1].ToString() + mask[2].ToString() + mask[3].ToString();
    }

    // Returns the char of a tile.
    protected char GetTileChar(int x, int y) {
        if (MapInfo.IsInMapRange(x, y, width, height)) {
            return map[x, y] == wallChar ? wallChar : roomChar;
        } else {
            return wallChar;
        }
    }

    // Returns the char of a tile.
    protected float GetTileFloat(int x, int y)
    {
        if (MapInfo.IsInMapRange(x, y, width, height))
        {
            return numeric_map[x, y] == wallNumb ? 1 : 0;
        }
        else
        {
            return 1;
        }
    }

    [Serializable]
    protected class ColoredTilesPrefab
    {
        public List<TilePrefab> coloredTilePrefabs;
    }

    [Serializable]
    protected class ProcessedColoredTilePrefab
    {
        public List<ProcessedTilePrefab> coloredProcessedTilePrefabs;

        public ProcessedColoredTilePrefab(List<ProcessedTilePrefab> list)
        {
            coloredProcessedTilePrefabs = list;
        }
    }
}