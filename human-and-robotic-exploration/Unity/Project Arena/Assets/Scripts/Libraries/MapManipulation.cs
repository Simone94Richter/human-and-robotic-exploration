using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MapManipulation {

    /// <summary>
    /// Methods to edit a map. A map is a char[n,m] matrix composed of chars.
    /// </summary>
    public static class MapEdit {

        // Flips the map.
        public static char[,] FlipMap(char[,] map) {
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            char[,] flippedMap = new char[height, width];

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    flippedMap[height - y - 1, width - x - 1] = map[x, y];
                }
            }

            return flippedMap;
        }

        // Erodes the map as many times as specified.
        public static void ErodeMap(char[,] toBeErodedMap, char wallChar) {
            char[,] originalMap = CloneMap(toBeErodedMap);

            for (int x = 0; x < originalMap.GetLength(0); x++) {
                for (int y = 0; y < originalMap.GetLength(1); y++) {
                    if (MapInfo.GetSurroundingWallCount(x, y, originalMap, wallChar) > 0) {
                        toBeErodedMap[x, y] = wallChar;
                    }
                }
            }
        }

        // Clones a map.
        public static char[,] CloneMap(char[,] toBeClonedMap) {
            char[,] clonedMap = new char[toBeClonedMap.GetLength(0), toBeClonedMap.GetLength(1)];

            for (int x = 0; x < toBeClonedMap.GetLength(0); x++) {
                for (int y = 0; y < toBeClonedMap.GetLength(1); y++) {
                    clonedMap[x, y] = toBeClonedMap[x, y];
                }
            }

            return clonedMap;
        }

        // Draws a circe of a given radius around a point.
        public static void DrawCircle(int centerX, int centerY, int r, char[,] map, char t) {
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            for (int x = -r; x <= r; x++) {
                for (int y = -r; y <= r; y++) {
                    if (x * x + y * y <= r * r) {
                        int drawX = centerX + x;
                        int drawY = centerY + y;

                        if (MapInfo.IsInMapRange(drawX, drawY, width, height))
                            map[drawX, drawY] = t;
                    }
                }
            }
        }

        // Adds borders to the map.
        public static char[,] AddBorders(char[,] map, int borderSize, char wallChar) {
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            char[,] borderedMap = new char[width + borderSize * 2, height + borderSize * 2];

            for (int x = 0; x < borderedMap.GetLength(0); x++) {
                for (int y = 0; y < borderedMap.GetLength(1); y++) {
                    if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                        borderedMap[x, y] = map[x - borderSize, y - borderSize];
                    else
                        borderedMap[x, y] = wallChar;
                }
            }

            return borderedMap;
        }

        // Fills the map with wall cells.
        public static void FillMap(char[,] map, char wallChar) {
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    map[x, y] = wallChar;
                }
            }
        }

    }

    /// <summary>
    /// Methods to retrieve information about a map. A map is a char[n,m] matrix composed of chars.
    /// </summary>
    public static class MapInfo {

        // Return 1 if the tile is a wall, 0 otherwise.
        public static int GetMapTileAsNumber(int x, int y, char[,] gridMap, char wallChar) {
            if (gridMap[x, y] == wallChar)
                return 1;
            else
                return 0;
        }

        // Returns the maximum map size.
        public static int GetMapSize(int width, int height) {
            if (width > height)
                return width;
            else
                return height;
        }

        // Converts coordinates to world position.
        public static Vector3 CoordToWorldPoint(Coord tile, int width, int height) {
            return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
        }

        // Tells if the "general" (full/room) type of two tiles is the same.
        public static bool IsSameGeneralType(char tyleType, char t, char wallChar) {
            if (tyleType == wallChar)
                return t == wallChar;
            else
                return t != wallChar;
        }

        // Tells if a tile is in the map.
        public static bool IsInMapRange(int x, int y, int width, int height) {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        // Returns a list of the free tiles.
        public static List<Coord> GetFreeTiles(char[,] map, char roomChar) {
            List<Coord> roomTiles = new List<Coord>();
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (map[x, y] == roomChar)
                        roomTiles.Add(new Coord(x, y));
                }
            }

            return roomTiles;
        }

        // Gets the number of walls surrounding a cell.
        public static int GetSurroundingWallCount(int gridX, int gridY, char[,] gridMap,
            char wallChar) {
            int width = gridMap.GetLength(0);
            int height = gridMap.GetLength(1);
            int wallCount = 0;

            // Loop on 3x3 grid centered on [gridX, gridY].
            for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
                for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
                    if (MapInfo.IsInMapRange(neighbourX, neighbourY, width, height)) {
                        if (neighbourX != gridX || neighbourY != gridY)
                            wallCount += MapInfo.GetMapTileAsNumber(neighbourX, neighbourY, gridMap, wallChar);
                    } else
                        wallCount++;
                }
            }

            return wallCount;
        }

    }

    /// <summary>
    /// Methods to validate input maps.
    /// </summary>
    public static class MapValidate {

        // Validates a map loaded from a text file as a char matrix.
        public static int ValidateMap(string path, int maxSize = 200) {
            if (path == null) {
                // Debug.Log(path + " is null.");
                return 2;
            } else if (!File.Exists(path)) {
                // Debug.Log(path + " doesn't exist.");
                return 2;
            } else {
                try {
                    string[] lines = File.ReadAllLines(path);
                    int xLenght = lines[0].Length;
                    int yLenght = lines.GetLength(0);

                    if (xLenght > maxSize || yLenght > maxSize)
                        return 4;

                    int spawnPointCount = 0;

                    for (int x = 0; x < xLenght; x++) {
                        for (int y = 0; y < yLenght; y++) {
                            if ((x == 0 || y == 0 || x == xLenght - 1 || y == yLenght - 1) && lines[y][x] != 'w')
                                return 3;
                            if (lines[y][x] == 's')
                                spawnPointCount++;
                        }
                    }

                    if (spawnPointCount == 0)
                        return 3;
                    else
                        return 0;
                } catch (Exception) {
                    return 3;
                }
            }
        }

        // Validates a multi-level map loaded from a text file as a char matrix.
        public static int ValidateMLMap(string path, int maxSize = 200) {
            if (path == null) {
                Debug.Log(path + " is null.");
                return 2;
            } else if (!File.Exists(path)) {
                Debug.Log(path + " doesn't exist.");
                return 2;
            } else {
                try {
                    int mapsCount = 1;

                    string[] lines = File.ReadAllLines(path);

                    int xLenght = lines[0].Length;
                    int yLenght = 0;

                    foreach (string s in lines) {
                        if (s.Length == 0) {
                            mapsCount++;
                        } else if (mapsCount == 1)
                            yLenght++;
                    }

                    if (xLenght > maxSize || yLenght > maxSize)
                        return 4;
                    if (mapsCount < 2)
                        return 6;

                    for (int i = 0; i < mapsCount; i++) {
                        int spawnPointCount = 0;

                        for (int x = 0; x < xLenght; x++) {
                            for (int y = 0; y < yLenght; y++) {
                                if ((x == 0 || y == 0 || x == xLenght - 1 || y == yLenght - 1) && lines[y + i * (yLenght + 1)][x] != 'w')
                                    return 6;
                                if (lines[y + i * (yLenght + 1)][x] == 's')
                                    spawnPointCount++;
                            }
                        }

                        if (spawnPointCount == 0)
                            return 6;
                    };

                    return 0;
                } catch (Exception) {
                    return 6;
                }
            }
        }

        // Validates a loaded from a string as a genome.
        public static int ValidateGeneticMap(string genome) {
            Regex rgx = new Regex(@"(\<\d+,\d+,[1-9]\d*\>)+(\|(\<\d+,\d+,-?[1-9]\d*\>)+)?(\|(\<\d+,\d+,[a-zA-Z]\>)+)?$");

            if (rgx.IsMatch(genome))
                return 0;
            else
                return 5;
        }

        // Validates a multi-level map loaded from a string as a genome.
        public static int ValidateGeneticMLMap(string genome) {
            Regex rgx = new Regex(@"(\<\d+,\d+,[1-9]\d*\>)+(\|(\<\d+,\d+,-?[1-9]\d*\>)+)?(\|(\<\d+,\d+,[a-zA-Z]\>)+)?(\|\|(((\<\d+,\d+,[1-9]\d*\>)+(\|(\<\d+,\d+,-?[1-9]\d*\>)+)?(\|(\<\d+,\d+,[a-zA-Z]\>)+)?)|(\<0.\d+,0.\d+,0.\d+,0.\d+,0.\d+\>)(\|(\<\d+,\d+,[a-zA-Z]\>)+)?))+$");

            if (rgx.IsMatch(genome))
                return 0;
            else
                return 5;
        }

    }

    // Coordinates of a tile.
    public struct Coord {
        public int tileX;
        public int tileY;

        public Coord(int x, int y) {
            tileX = x;
            tileY = y;
        }
    }

}