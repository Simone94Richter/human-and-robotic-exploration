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

    private char[,] charMap;
    private float[,] numericMap;

    public override void ManageMap(bool assembleMap) {
        if (loadMapFromFile) {
            // Load the map.
            LoadMapFromText();
            // Flip the map if needed.
            if (flip && !isNumeric) {
                charMap = MapEdit.FlipMap(charMap);                
            }
        } else {
            // Generate the map.
            if (ParameterManager.HasInstance()) {
                charMap = mapGeneratorScript.GenerateMap(seed, export, exportPath);
            } else {
                charMap = mapGeneratorScript.GenerateMap();
            }
        }

        if (assembleMap) {
            if (!isNumeric)
            {
                // Assemble the map.
                mapAssemblerScript.AssembleMap(charMap, mapGeneratorScript.GetWallChar(),
                    mapGeneratorScript.GetRoomChar());
                // Displace the objects.
                objectDisplacerScript.DisplaceObjects(charMap, mapAssemblerScript.GetSquareSize(),
                    mapAssemblerScript.GetWallHeight());
            }
            else
            {
                //Assemble the map
                mapAssemblerScript.AssembleMap(numericMap, mapGeneratorScript.GetWallNumb(), mapGeneratorScript.GetRoomNumb());
                
                // Displace the objects
                objectDisplacerScript.DisplaceObjects(numericMap, mapAssemblerScript.GetSquareSize(), mapAssemblerScript.GetWallHeight());
            }
        }

        float floorSize = mapAssemblerScript.GetSquareSize();
        if (robot != null)
        {
            if (!isNumeric)
            {
                robot.SetMap(charMap, floorSize);
            }
            else
            {
                robot.SetMap(numericMap, floorSize);
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
            charMap = new char[lines.GetLength(0), lines[0].Length];

            for (int x = 0; x < charMap.GetLength(0); x++)
            {
                for (int y = 0; y < charMap.GetLength(1); y++)
                {
                    charMap[x, y] = lines[x][y];
                }
            }
        }
        else
        {
            numericMap = new float[lines.GetLength(0), lines[0].Length];

            for (int x = 0; x < numericMap.GetLength(0); x++)
            {
                for (int y = 0; y < numericMap.GetLength(1); y++)
                {
                    numericMap[x, y] = lines[x][y];
                    //Debug.Log(numeric_map[x, y]);
                }
            }
        }
    }

}