using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SLMapManager is an implementation of MapManager used to manage multi-level AB maps.
/// </summary>
public class ABMLMapManager : MLMapManager {

    [Header("AB generation")]
    [SerializeField]
    private MapGenerator diggerGeneratorScript;

    private List<bool> usesDiggerGeneratorList;

    public override void ManageMap(bool assembleMap) {
        if (loadMapFromFile) {
            // Load the map.
            LoadMapFromText();
        } else {
            // Extract the genomes.
            List<Genome> genomes = ExtractGenomes(seed);
            // Initialize the generation mode list.
            usesDiggerGeneratorList = new List<bool>();
            // Generate the map.
            for (int i = 0; i < genomes.Count; i++) {
                mapGeneratorScript.ResetMapSize();
                if (ParameterManager.HasInstance()) {
                    if (genomes[i].useDefaultGenerator) {
                        maps.Add(mapGeneratorScript.GenerateMap(genomes[i].genome, false, null));
                        usesDiggerGeneratorList.Add(false);
                    } else {
                        if (i > 0) {
                            maps.Add(diggerGeneratorScript.GenerateMap(genomes[i].genome,
                                maps[i - 1].GetLength(0), maps[i - 1].GetLength(1), false, null));
                        } else {
                            maps.Add(diggerGeneratorScript.GenerateMap(genomes[i].genome, false,
                                null));
                        }
                        usesDiggerGeneratorList.Add(true);
                    }
                } else {
                    maps.Add(mapGeneratorScript.GenerateMap());
                    usesDiggerGeneratorList.Add(false);
                }
                mapGeneratorScript.ResetMapSize();
            }
            // Resize all the maps.
            ResizeAllMaps();
            // Add the stairs.
            stairsGeneratorScript.GenerateStairs(maps, usesDiggerGeneratorList, mapGeneratorScript,
                GenomeHasObjects(seed));
            // Save the map.
            if (export) {
                seed = seed.GetHashCode().ToString();
                SaveMLMapAsText();
            }
        }

        if (assembleMap) {
            // Assemble the map.
            mapAssemblerScript.AssembleMap(maps, mapGeneratorScript.GetWallChar(),
                mapGeneratorScript.GetRoomChar(), stairsGeneratorScript.GetVoidChar());
            // Displace the objects.
            for (int i = 0; i < maps.Count; i++) {
                objectDisplacerScript.DisplaceObjects(maps[i], mapAssemblerScript.GetSquareSize(),
                    mapAssemblerScript.GetWallHeight() * i);
            }
        }
    }

    // Resizes all the maps.
    private void ResizeAllMaps() {
        int maxWidth = 0;
        int maxHeight = 0;

        foreach (char[,] m in maps) {
            if (m.GetLength(0) > maxWidth) {
                maxWidth = m.GetLength(0);
            }
            if (m.GetLength(1) > maxHeight) {
                maxHeight = m.GetLength(1);
            }
        }

        for (int i = 0; i < maps.Count; i++) {
            if (maps[i].GetLength(0) < maxWidth || maps[i].GetLength(1) < maxHeight) {
                maps[i] = ResizeMap(maps[i], maxWidth, maxHeight);
            }
        }
    }

    // Resizes a map.
    private char[,] ResizeMap(char[,] map, int maxWidth, int maxHeight) {
        char[,] resizedMap = new char[maxWidth, maxHeight];

        char wallChar = mapGeneratorScript.GetWallChar();

        for (int x = 0; x < maxWidth; x++) {
            for (int y = 0; y < maxHeight; y++) {
                if (x < map.GetLength(0) && y < map.GetLength(1)) {
                    resizedMap[x, y] = map[x, y];
                } else {
                    resizedMap[x, y] = wallChar;
                }
            }
        }

        return resizedMap;
    }

    // Extracts the genomes from the seed.
    private List<Genome> ExtractGenomes(string seed) {
        List<Genome> genomes = new List<Genome>();

        string processedString = "";
        int genesCount = 1;
        bool counting = true;

        for (int i = 0; i < seed.Length; i++) {
            if (i < seed.Length - 1 && seed[i] == '|' && seed[i + 1] == '|') {
                genomes.Add(CreateNewGenome(processedString, genesCount));
                processedString = "";
                counting = true;
                genesCount = 1;
                i++;
            } else if (i == seed.Length - 1) {
                genomes.Add(CreateNewGenome(processedString + seed[i], genesCount));
            } else if (seed[i] == ',') {
                processedString += seed[i];
                if (counting) {
                    genesCount++;
                }
            } else if (seed[i] == '<') {
                processedString += seed[i];
                if (genesCount > 1) {
                    counting = false;
                }
            } else {
                processedString += seed[i];
            }
        }

        return genomes;
    }

    private Genome CreateNewGenome(string s, int g) {
        return new Genome {
            genome = s,
            useDefaultGenerator = (g == 5) ? false : true
        };
    }

    // Tells if the genome includes objects.
    public static bool GenomeHasObjects(string genome) {
        foreach (char c in genome) {
            if (Char.IsLetter(c)) {
                return true;
            }
        };
        return false;
    }

    private struct Genome {
        public string genome;
        public bool useDefaultGenerator;
    }

}