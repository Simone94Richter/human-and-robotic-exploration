using ABObjects;
using MapManipulation;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DiggerMapGenerator is an implementation of MapGenerator that generates the map by using a random
/// digger.
/// </summary>
public class DiggerMapGenerator : MapGenerator {

    [Header("Digger generation")]
    [SerializeField, Range(0, 100)]
    private int forwardProbability;
    [SerializeField, Range(0, 100)]
    private int leftProbability;
    [SerializeField, Range(0, 100)]
    private int rigthProbability;
    [SerializeField, Range(0, 100)]
    private int visitedProbability;
    [SerializeField, Range(0, 100)]
    private int stairProbability;
    [SerializeField, Range(0, 100)]
    private int roomPercentage;

    [Header("Stairs generation")]
    [SerializeField]
    private char[] stairChars = new char[4];

    [Header("AB generation")]
    [SerializeField]
    private bool useABGeneration;

    private List<ABTile> tiles;

    private List<Coord> visitedTiles;

    private int currentX;
    private int currentY;
    private int direction;

    private void Start() {
        SetReady(true);
    }

    // Generates a map.
    public override char[,] GenerateMap() {
        map = new char[width, height];

        // Parse the genome if needed.
        if (useABGeneration) {
            ParseGenome();
        }
        if (tiles != null && tiles.Count > 0) {
            stairProbability = 0;
        }

        ValidateProbabilities();

        InitializePseudoRandomGenerator();

        MapEdit.FillMap(map, wallChar);

        DigMap();

        if (tiles != null && tiles.Count > 0) {
            foreach (ABTile t in tiles) {
                if (MapInfo.IsInMapRange(t.x, t.y, width, height)) {
                    map[t.x, t.y] = t.value;
                }
            }
        } else {
            PopulateMap();
        }

        map = MapEdit.AddBorders(map, borderSize, wallChar);
        width = map.GetLength(0);
        height = map.GetLength(1);

        if (createTextFile && !useABGeneration) {
            SaveMapAsText();
        }

        return map;
    }

    // Digs the map. The direction of the digger is coded as: 1 for up, 2 for rigth, 3 for down and 
    // 4 for left. Turning left means decreasing the direction by 1 in a circular fashion and vice 
    // versa for turning rigth.
    private void DigMap() {
        currentX = width / 2;
        currentY = height / 2;
        direction = 1;

        int stopCount = width * height * roomPercentage / 100;

        visitedTiles = new List<Coord> {
            new Coord(currentX, currentY)
        };
        map[currentX, currentY] = roomChar;

        while (visitedTiles.Count < stopCount) {
            int nextAction = pseudoRandomGen.Next(0, 100);
            if (nextAction < forwardProbability) {
                MoveDiggerForward();
            } else if (nextAction < leftProbability) {
                direction = CircularIncrease(direction, 1, 4, false);
            } else if (nextAction < rigthProbability) {
                direction = CircularIncrease(direction, 1, 4, false);
            } else if (nextAction < visitedProbability) {
                MoveDiggerRandomly();
            } else {
                map[currentX, currentY] = stairChars[CircularIncrease(direction, 1, 4,
                    GetRandomBoolean()) - 1];
            }
        }
    }

    // Moves the digger forward.
    private void MoveDiggerForward() {
        switch (direction) {
            case 1:
                if (MapInfo.IsInMapRange(currentX, currentY - 1, width, height)) {
                    currentY -= 1;
                    map[currentX, currentY] = roomChar;
                } else
                    MoveDiggerRandomly();
                break;
            case 2:
                if (MapInfo.IsInMapRange(currentX + 1, currentY, width, height)) {
                    currentX += 1;
                    map[currentX, currentY] = roomChar;
                } else
                    MoveDiggerRandomly();
                break;
            case 3:
                if (MapInfo.IsInMapRange(currentX, currentY + 1, width, height)) {
                    currentY += 1;
                    map[currentX, currentY] = roomChar;
                } else
                    MoveDiggerRandomly();
                break;
            case 4:
                if (MapInfo.IsInMapRange(currentX - 1, currentY, width, height)) {
                    currentX -= 1;
                    map[currentX, currentY] = roomChar;
                } else
                    MoveDiggerRandomly();
                break;
        }

        map[currentX, currentY] = roomChar;
        visitedTiles.Add(new Coord(currentX, currentY));
    }

    // Moves the digger to a random tile.
    private void MoveDiggerRandomly() {
        Coord c = visitedTiles[pseudoRandomGen.Next(0, visitedTiles.Count - 1)];
        currentX = c.tileX;
        currentY = c.tileY;
    }

    // Makes a circular sum.
    private int CircularIncrease(int value, int min, int max, bool increase) {
        if (increase) {
            return (value == max) ? min : value + 1;
        } else {
            return (value == min) ? max : value - 1;
        }
    }

    // Validates the probabilities and corrects them if needed.
    private void ValidateProbabilities() {
        int totalSum = forwardProbability;

        leftProbability = ScaleProbability(leftProbability, totalSum);
        totalSum = leftProbability;

        rigthProbability = ScaleProbability(rigthProbability, totalSum);
        totalSum = rigthProbability;

        visitedProbability = ScaleProbability(visitedProbability, totalSum);
        totalSum = visitedProbability;

        stairProbability = ScaleProbability(stairProbability, totalSum);
        totalSum = stairProbability;

        if (totalSum < 100) {
            visitedProbability += 100 - totalSum;
            stairProbability = 100;
        }
    }

    // Scales a probability.
    private int ScaleProbability(int p, int s) {
        if (s + p > 100) {
            return 100;
        } else {
            return s + p;
        }
    }

    // Decodes the genome setting the probabilities.
    private void ParseGenome() {
        string currentValue = "";
        int currentChar = 1;

        // I've already skipped the first char, now get the forward probability.
        while (Char.IsNumber(seed[currentChar]) || seed[currentChar] == '.') {
            currentValue += seed[currentChar];
            currentChar++;
        }
        forwardProbability = Mathf.FloorToInt(float.Parse(currentValue) * 100);

        currentValue = "";
        currentChar++;

        // Get the left probability.
        while (Char.IsNumber(seed[currentChar]) || seed[currentChar] == '.') {
            currentValue += seed[currentChar];
            currentChar++;
        }
        leftProbability = Mathf.FloorToInt(float.Parse(currentValue) * 100);

        currentValue = "";
        currentChar++;

        // Get the rigth probability.
        while (Char.IsNumber(seed[currentChar]) || seed[currentChar] == '.') {
            currentValue += seed[currentChar];
            currentChar++;
        }
        rigthProbability = Mathf.FloorToInt(float.Parse(currentValue) * 100);

        currentValue = "";
        currentChar++;

        // Get the visited probability.
        while (Char.IsNumber(seed[currentChar]) || seed[currentChar] == '.') {
            currentValue += seed[currentChar];
            currentChar++;
        }
        visitedProbability = Mathf.FloorToInt(float.Parse(currentValue) * 100);

        currentValue = "";
        currentChar++;

        // Get the stair probability.
        while (Char.IsNumber(seed[currentChar]) || seed[currentChar] == '.') {
            currentValue += seed[currentChar];
            currentChar++;
        }
        stairProbability = Mathf.FloorToInt(float.Parse(currentValue) * 100);

        currentValue = "";
        currentChar++;

        // Parse the tiles.
        if (currentChar < seed.Length && seed[currentChar] == '|') {
            currentChar++;

            tiles = new List<ABTile>();

            while (currentChar < seed.Length && seed[currentChar] == '<') {
                ABTile tile = new ABTile();
                currentChar++;

                // Get the x coordinate of the origin.
                while (Char.IsNumber(seed[currentChar])) {
                    currentValue += seed[currentChar];
                    currentChar++;
                }
                tile.x = Int32.Parse(currentValue);

                currentValue = "";
                currentChar++;

                // Get the y coordinate of the origin.
                while (Char.IsNumber(seed[currentChar])) {
                    currentValue += seed[currentChar];
                    currentChar++;
                }
                tile.y = Int32.Parse(currentValue);

                currentValue = "";
                currentChar++;

                // Get the value of the tile.
                tile.value = seed[currentChar];

                // Add the arena to the list.
                tiles.Add(tile);

                currentValue = "";
                currentChar += 2;
            }
        }
    }

    public override string ConvertMapToAB(bool exportObjects = true) {
        string genome = "<" + forwardProbability + "," + leftProbability + "," + rigthProbability + "," +
            visitedProbability + "," + stairProbability + ">";

        if (exportObjects) {
            genome += "|";

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (map[x, y] != wallChar && map[x, y] != roomChar) {
                        genome += "<" + x + ',' + y + ',' + map[x, y] + ">";
                    }
                }
            }
        }

        return genome;
    }

}