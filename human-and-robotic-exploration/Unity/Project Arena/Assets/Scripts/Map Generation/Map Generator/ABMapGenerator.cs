using ABObjects;
using MapManipulation;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CellularMapGenerator is an implementation of MapGenerator that generates the map by using a
/// cellular automata.
/// </summary>
public class ABMapGenerator : MapGenerator {

    [SerializeField] private int passageWidth = 3;

    private ABRoom mainRoom;
    private List<ABRoom> arenas;
    private List<ABRoom> corridors;
    private List<ABTile> tiles;

    private void Start() {
        originalWidth = width;
        originalHeight = height;

        SetReady(true);
    }

    public override char[,] GenerateMap() {
        InitializePseudoRandomGenerator();

        width = 0;
        height = 0;

        ParseGenome();

        InitializeMap();

        ProcessMap();

        map = MapEdit.AddBorders(map, borderSize, wallChar);
        ResetMapSize(); ;

        if (createTextFile) {
            seed = seed.GetHashCode().ToString();
            SaveMapAsText();
        }

        return map;
    }

    // Decodes the genome populating the lists of arenas, corridors and tiles.
    private void ParseGenome() {
        arenas = new List<ABRoom>();
        corridors = new List<ABRoom>();
        tiles = new List<ABTile>();

        string currentValue = "";
        int currentChar = 0;

        // Parse the arenas.
        while (currentChar < seed.Length && seed[currentChar] == '<') {
            ABRoom arena = new ABRoom();
            currentChar++;

            // Get the x coordinate of the origin.
            while (Char.IsNumber(seed[currentChar])) {
                currentValue += seed[currentChar];
                currentChar++;
            }
            arena.originX = Int32.Parse(currentValue);

            currentValue = "";
            currentChar++;

            // Get the y coordinate of the origin.
            while (Char.IsNumber(seed[currentChar])) {
                currentValue += seed[currentChar];
                currentChar++;
            }
            arena.originY = Int32.Parse(currentValue);

            currentValue = "";
            currentChar++;

            // Get the size of the arena.
            while (Char.IsNumber(seed[currentChar])) {
                currentValue += seed[currentChar];
                currentChar++;
            }
            arena.dimension = Int32.Parse(currentValue);

            // Add the arena to the list.
            UpdateMapSize(arena.originX, arena.originY, arena.dimension, true);
            arenas.Add(arena);

            currentValue = "";
            currentChar++;
        }

        int rollbackCurrentChar = currentChar;

        // Parse the corridors.
        if (currentChar < seed.Length && seed[currentChar] == '|') {
            currentChar++;

            while (currentChar < seed.Length && seed[currentChar] == '<') {
                ABRoom corridor = new ABRoom();
                currentChar++;

                // Get the x coordinate of the origin.
                while (Char.IsNumber(seed[currentChar])) {
                    currentValue += seed[currentChar];
                    currentChar++;
                }
                corridor.originX = Int32.Parse(currentValue);

                currentValue = "";
                currentChar++;

                // Get the y coordinate of the origin.
                while (Char.IsNumber(seed[currentChar])) {
                    currentValue += seed[currentChar];
                    currentChar++;
                }
                corridor.originY = Int32.Parse(currentValue);

                currentValue = "";
                currentChar++;

                // Stop parsing the corridors if what I have is a tile.
                if (!(seed[currentChar] == '-' || Char.IsNumber(seed[currentChar]))) {
                    currentChar = rollbackCurrentChar;
                    break;
                }

                // Get the size of the corridor.
                if (seed[currentChar] == '-') {
                    currentValue += seed[currentChar];
                    currentChar++;
                }
                while (Char.IsNumber(seed[currentChar])) {
                    currentValue += seed[currentChar];
                    currentChar++;
                }
                corridor.dimension = Int32.Parse(currentValue);

                // Add the arena to the list.
                UpdateMapSize(corridor.originX, corridor.originY, corridor.dimension, false);
                corridors.Add(corridor);

                currentValue = "";
                currentChar++;
            }
        }

        // Parse the tiles.
        if (currentChar < seed.Length && seed[currentChar] == '|') {
            currentChar++;

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

        mainRoom = arenas[0];
    }

    // Updates the map size.
    private void UpdateMapSize(int originX, int originY, int dimension, bool isArena) {
        if (isArena) {
            if (originX + dimension > width) {
                width = originX + dimension;
            }
            if (originY + dimension > height) {
                height = originY + dimension;
            }
        } else {
            if (dimension > 0) {
                if (originX + dimension > width) {
                    width = originX + dimension;
                }
                if (originY + passageWidth > height) {
                    height = originY + passageWidth;
                }
            } else {
                if (originX + passageWidth > width) {
                    width = originX + passageWidth;
                }
                if (originY + dimension > height) {
                    height = originY + dimension;
                }
            }
        }
    }

    // Initializes the map adding arenas and corridors.
    private void InitializeMap() {
        map = new char[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                map[x, y] = wallChar;
            }
        }

        foreach (ABRoom a in arenas) {
            for (int x = a.originX; x < a.originX + a.dimension; x++) {
                for (int y = a.originY; y < a.originY + a.dimension; y++) {
                    if (MapInfo.IsInMapRange(x, y, width, height)) {
                        map[x, y] = roomChar;
                    }
                }
            }
        }

        foreach (ABRoom c in corridors) {
            if (c.dimension > 0) {
                for (int x = c.originX; x < c.originX + c.dimension; x++) {
                    for (int y = c.originY; y < c.originY + passageWidth; y++) {
                        if (MapInfo.IsInMapRange(x, y, width, height)) {
                            map[x, y] = roomChar;
                        }
                    }
                }
            } else {
                for (int x = c.originX; x < c.originX + passageWidth; x++) {
                    for (int y = c.originY; y < c.originY - c.dimension; y++) {
                        if (MapInfo.IsInMapRange(x, y, width, height)) {
                            map[x, y] = roomChar;
                        }
                    }
                }
            }
        }
    }

    // Removes rooms that are not reachable from the main one and adds objects.
    private void ProcessMap() {
        // Get the reachability mask.
        bool[,] reachabilityMask = ComputeReachabilityMask();

        // Remove rooms not connected to the main one.
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (!reachabilityMask[x, y]) {
                    map[x, y] = wallChar;
                }
            }
        }

        // Add objects;
        if (tiles.Count > 0) {
            foreach (ABTile t in tiles) {
                if (MapManipulation.MapInfo.IsInMapRange(t.x, t.y, width, height)) {
                    map[t.x, t.y] = t.value;
                }
            }
        } else {
            PopulateMap();
        }
    }

    // Computes a mask of the tiles reachable by the main arena and scales the number of objects to 
    // be displaced.
    private bool[,] ComputeReachabilityMask() {
        bool[,] reachabilityMask = new bool[width, height];
        int floorCount = 0;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                reachabilityMask[x, y] = false;
            }
        }

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(mainRoom.originX, mainRoom.originY));

        while (queue.Count > 0) {
            Coord tile = queue.Dequeue();

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++) {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
                    if (MapInfo.IsInMapRange(x, y, width, height) && (y == tile.tileY ||
                        x == tile.tileX)) {
                        if (reachabilityMask[x, y] == false && map[x, y] == roomChar) {
                            reachabilityMask[x, y] = true;
                            queue.Enqueue(new Coord(x, y));
                            floorCount++;
                        }
                    }
                }
            }
        }

        ScaleObjectsPopulation(floorCount);

        return reachabilityMask;
    }

    // Scales the number of instance of each object depending on the size of the map w.r.t. the 
    // original one.
    private void ScaleObjectsPopulation(int floorCount) {
        float scaleFactor = floorCount / (originalHeight * originalWidth / 3f);

        for (int i = 0; i < mapObjects.Length; i++) {
            mapObjects[i].numObjPerMap = (int)Math.Ceiling(scaleFactor *
                mapObjects[i].numObjPerMap);
        }
    }

    public override string ConvertMapToAB(bool exportObjects = true) {
        throw new NotImplementedException();
    }

}