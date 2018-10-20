using MapManipulation;
using System;
using System.IO;
using UnityEngine;

/// <summary>
/// SLMapManager is an implementation of MapManager used to manage single-level maps.
/// </summary>
public class SLMapManager : MapManager {

    [Header("The asset of the file containing the map")]
    public TextAsset mapFile;

    private char[,] map;
    private float[,] numeric_map;

    public override void ManageMap(bool assembleMap) {
        if (loadMapFromFile) {
            // Load the map.
            LoadMapFromText();
            // Flip the map if needed.
            if (flip && !isNumeric) {
                map = MapEdit.FlipMap(map);                
            }
        } else {
            // Generate the map.
            if (ParameterManager.HasInstance()) {
                map = mapGeneratorScript.GenerateMap(seed, export, exportPath);
            } else {
                map = mapGeneratorScript.GenerateMap();
            }
        }

        if (assembleMap) {
            if (!isNumeric)
            {
                // Assemble the map.
                mapAssemblerScript.AssembleMap(map, mapGeneratorScript.GetWallChar(),
                    mapGeneratorScript.GetRoomChar());
                // Displace the objects.
                objectDisplacerScript.DisplaceObjects(map, mapAssemblerScript.GetSquareSize(),
                    mapAssemblerScript.GetWallHeight());
            }
            else
            {
                //Assemble the map
                mapAssemblerScript.AssembleMap(numeric_map, mapGeneratorScript.GetWallNumb(), mapGeneratorScript.GetRoomNumb());
                
                // Displace the objects
                objectDisplacerScript.DisplaceObjects(numeric_map, mapAssemblerScript.GetSquareSize(), mapAssemblerScript.GetWallHeight());
            }
        }

        float floorSize = mapAssemblerScript.GetSquareSize();
        if (robot != null)
        {
            if (!isNumeric)
            {
                robot.SetMap(map, floorSize);
            }
            else
            {
                robot.SetMap(numeric_map, floorSize);
            }
        }
        if (player)
        {
            player.SetSquareSize(floorSize);
        }
    }

    // Loads the map from a text file.
    protected override void LoadMapFromText() {
        if (seed == null) {
            if (textFilePath == null) {
                /*if (mapFile == null)
                {
                    ErrorManager.ErrorBackToMenu(-1);
                }*/
                //else ConvertToMatrix(mapFile.text);
            } else if (!File.Exists(textFilePath)) {
                ErrorManager.ErrorBackToMenu(-1);
            } else {
                try {
                    ConvertToMatrix(File.ReadAllLines(@textFilePath));
                } catch (Exception) {
                    ErrorManager.ErrorBackToMenu(-1);
                }
            }
        } else {
            ConvertToMatrix(seed.Split(new string[] { "\n", "\r\n" },
                StringSplitOptions.RemoveEmptyEntries));
        }
    }

    // Converts the map from a list of lines to a matrix.
    private void ConvertToMatrix(string[] lines) {
        if (!isNumeric)
        {
            map = new char[lines.GetLength(0), lines[0].Length];

            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    map[x, y] = lines[x][y];
                }
            }
        }
        else
        {
            numeric_map = new float[lines.GetLength(0), lines[0].Length];

            for (int x = 0; x < numeric_map.GetLength(0); x++)
            {
                for (int y = 0; y < numeric_map.GetLength(1); y++)
                {
                    numeric_map[x, y] = lines[x][y];
                    //Debug.Log(numeric_map[x, y]);
                }
            }
        }
    }

}