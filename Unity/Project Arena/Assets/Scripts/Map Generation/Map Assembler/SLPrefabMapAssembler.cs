using MapManipulation;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SLPrefabMapAssembler is an implementation of PrefabMapAssembler for single-level maps.
/// </summary>
public class SLPrefabMapAssembler : PrefabMapAssembler {

    // Map.
    private char[,] map;

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

    // Returns the char of a tile.
    protected char GetTileChar(int x, int y) {
        if (MapInfo.IsInMapRange(x, y, width, height)) {
            return map[x, y] == wallChar ? wallChar : roomChar;
        } else {
            return wallChar;
        }
    }

}