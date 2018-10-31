using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MapManipulation;

/// <summary>
/// MapGenerator is an abstract class used to implement any kind of map generator. A map consist in
/// a matrix of chars.
/// </summary>
public abstract class MapGenerator : CoreComponent {

    [Header("Seed")]
    // Do I have to generate my seed?
    [SerializeField]
    protected bool useRandomSeed = true;
    // Seed used to generate the map.
    [SerializeField]
    protected string seed = null;

    [Header("Generation")]
    // Map width.
    [SerializeField]
    protected int width = 100;
    // Map height.
    [SerializeField]
    protected int height = 100;
    // Minimum distance of an object w.r.t another object.
    [SerializeField]
    protected int objectToObjectDistance = 5;
    // Minimum distance of an object w.r.t a wall.
    [SerializeField]
    protected int objectToWallDistance = 2;
    // Border size.
    [SerializeField]
    protected int borderSize = 5;

    [Header("Representation")]
    // Char that denotes a room
    [SerializeField]
    protected char roomChar = 'r';
    // Char that denotes a wall;
    [SerializeField]
    protected char wallChar = 'w';
    // Float that denotes a room;
    [SerializeField]
    protected float roomNumb = 48f;
    // Float that denotes a wall;
    [SerializeField]
    protected float wallNumb = 49f;
    // Custom objects that will be added to the map.
    [SerializeField]
    protected MapObject[] mapObjects;

    [Header("Export")]
    // Do I have to create a .txt output?
    [SerializeField]
    protected bool createTextFile = false;
    // Path where to save the text map.
    [SerializeField]
    protected string textFilePath = null;

    // Map, defined as a grid of chars.
    protected char[,] map;
    // Map, defined as a grid of numbers
    protected float[,] numeric_map;
    // Hash of the seed.
    protected int hash;
    // Pseudo random generator.
    protected System.Random pseudoRandomGen;

    // Tells if we have to prepare for an AB export.
    protected bool prepareABExport = false;

    protected int originalWidth = 0;
    protected int originalHeight = 0;

    /* MAP GENERATION */

    // Sets the parmaters, generates a map and returns it.
    public char[,] GenerateMap(string s, int w, int h, bool ctf, string e) {
        width = w;
        height = h;

        return GenerateMap(s, ctf, e);
    }

    // Sets the parmaters, generates a map and returns it.
    public char[,] GenerateMap(string s, bool ctf, string e) {
        useRandomSeed = false;
        seed = s;

        createTextFile = ctf;
        textFilePath = e;

        return GenerateMap();
    }

    // Sets the parmaters, generates a map and returns it.
    public char[,] GenerateMap(string s, bool pABe) {
        prepareABExport = pABe;

        return GenerateMap(s, false, null);
    }

    // Generates the map and returns it.
    public abstract char[,] GenerateMap();

    /* OBJECT GENERATION */

    // Erodes the map once, scans custom objects and adds them to the map using the rigth method.
    protected void PopulateMap() {
        if (mapObjects.Length > 0) {
            char[,] restrictedMap = map.Clone() as char[,];

            if (objectToWallDistance > 0) {
                MapEdit.ErodeMap(restrictedMap, wallChar);
            }

            // Place the objects.
            foreach (MapObject o in mapObjects) {
                switch (o.positioningMethod) {
                    case ObjectPositioningMethod.Rain:
                        GenerateObjectsRain(o, restrictedMap);
                        break;
                    case ObjectPositioningMethod.RainDistanced:
                        GenerateObjectsRainDistanced(o, restrictedMap);
                        break;
                    case ObjectPositioningMethod.RainShared:
                        GenerateObjectsRainShared(o, restrictedMap);
                        break;
                }
            }
        }
    }

    protected void GenerateObjectsRain(MapObject o, char[,] restrictedMap) {
        char[,] supportMap = restrictedMap.Clone() as char[,];

        // Restrict again if there are object that need a further restriction.
        if (!o.placeAnywhere && objectToWallDistance > 1) {
            for (int i = 1; i < objectToWallDistance; i++) {
                MapEdit.ErodeMap(supportMap, wallChar);
            }
        }

        List<Coord> roomTiles = MapInfo.GetFreeTiles(supportMap, roomChar);

        for (int i = 0; i < o.numObjPerMap; i++) {
            if (roomTiles.Count > 0) {
                int selected = pseudoRandomGen.Next(0, roomTiles.Count);
                map[roomTiles[selected].tileX, roomTiles[selected].tileY] = o.objectChar;
                restrictedMap[roomTiles[selected].tileX, roomTiles[selected].tileY] = wallChar;
                roomTiles.RemoveAt(selected);
            } else {
                ManageError(Error.SOFT_ERROR, "Error while populating the map, no more free " +
                    "tiles are availabe.");
                return;
            }
        }
    }

    protected void GenerateObjectsRainDistanced(MapObject o, char[,] restrictedMap) {
        char[,] supportMap = restrictedMap.Clone() as char[,];

        // Restrict again if there are object that need a further restriction.
        if (!o.placeAnywhere && objectToWallDistance > 1) {
            for (int i = 1; i < objectToWallDistance; i++) {
                MapEdit.ErodeMap(supportMap, wallChar);
            }
        }

        List<Coord> roomTiles = MapInfo.GetFreeTiles(supportMap, roomChar);

        for (int i = 0; i < o.numObjPerMap; i++) {
            if (roomTiles.Count > 0) {
                int selected = pseudoRandomGen.Next(0, roomTiles.Count);
                map[roomTiles[selected].tileX, roomTiles[selected].tileY] = o.objectChar;
                restrictedMap[roomTiles[selected].tileX, roomTiles[selected].tileY] = wallChar;
                MapEdit.DrawCircle(roomTiles[selected].tileX, roomTiles[selected].tileY,
                    objectToObjectDistance, supportMap, wallChar);
                roomTiles = MapInfo.GetFreeTiles(supportMap, roomChar);
            } else {
                ManageError(Error.SOFT_ERROR, "Error while populating the map, no more free " +
                    "tiles are availabe.");
                return;
            }
        }
    }

    protected void GenerateObjectsRainShared(MapObject o, char[,] restrictedMap) {
        char[,] supportMap = restrictedMap.Clone() as char[,];

        // Restrict again if there are object that need a further restriction.
        if (!o.placeAnywhere && objectToWallDistance > 1) {
            for (int i = 1; i < objectToWallDistance; i++) {
                MapEdit.ErodeMap(supportMap, wallChar);
            }
        }

        List<Coord> roomTiles = MapInfo.GetFreeTiles(supportMap, roomChar);

        for (int i = 0; i < o.numObjPerMap; i++) {
            if (roomTiles.Count > 0) {
                int selected = pseudoRandomGen.Next(0, roomTiles.Count);
                map[roomTiles[selected].tileX, roomTiles[selected].tileY] = o.objectChar;
                MapEdit.DrawCircle(roomTiles[selected].tileX, roomTiles[selected].tileY,
                    (o.placeAnywhere) ? 1 : objectToObjectDistance, supportMap, wallChar);
                roomTiles = MapInfo.GetFreeTiles(supportMap, roomChar);
            } else {
                ManageError(Error.SOFT_ERROR, "Error while populating the map, no more free " +
                    "tiles are availabe.");
                return;
            }
        }
    }

    /* HELPERS */

    // Initializes the pseudo random generator.
    protected void InitializePseudoRandomGenerator() {
        if (useRandomSeed) {
            seed = GetDateString();
        }

        hash = seed.GetHashCode();
        pseudoRandomGen = new System.Random(hash);
    }

    // Gets the current date as string.
    public static string GetDateString() {
        return System.DateTime.Now.ToString();
    }

    // Saves the map in a text file.
    protected void SaveMapAsText() {
        if (textFilePath == null && !Directory.Exists(textFilePath)) {
            ManageError(Error.SOFT_ERROR, "Error while retrieving the folder, please insert a " +
                "valid path.");
        } else {
            try {
                string textMap = "";

                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        textMap = textMap + map[x, y];
                    }
                    if (x < width - 1) {
                        textMap = textMap + "\n";
                    }
                }

                File.WriteAllText(@textFilePath + "/" + seed.ToString() + ".map.txt",
                    textMap);
            } catch (Exception) {
                ManageError(Error.SOFT_ERROR, "Error while saving the map at " + @textFilePath +
                    ", please insert a valid path and check its permissions. ");
            }
        }
    }

    // Returns the map in AB notation.
    public abstract string ConvertMapToAB(bool exportObjects = true);

    // Saves the original size of the map.
    public void SaveMapSize() {
        originalWidth = width;
        originalHeight = height;
    }

    // Resets the size of the map.
    public void ResetMapSize() {
        width = originalWidth;
        height = originalHeight;
    }

    /* GETTERS */

    public bool GetRandomBoolean() {
        return (pseudoRandomGen.Next(100) < 50) ? true : false;
    }

    public int GetRandomInteger() {
        return pseudoRandomGen.Next();
    }

    public int GetRandomInteger(int min, int max) {
        return pseudoRandomGen.Next(min, max);
    }

    public char GetWallChar() {
        return wallChar;
    }

    public float GetWallNumb()
    {
        return wallNumb;
    }

    public char GetRoomChar() {
        return roomChar;
    }

    public float GetRoomNumb()
    {
        return roomNumb;
    }

    public float GetHeight() {
        return height;
    }

    public float GetWidth() {
        return width;
    }

    /* CUTSOM OBJECTS */

    public enum ObjectPositioningMethod {
        Rain,
        RainDistanced,
        RainShared
    }

    // Informations about an object. 
    [Serializable]
    protected struct MapObject {
        // Character which defines the object.
        public char objectChar;
        // Number of objects to be put in the map.
        public int numObjPerMap;
        // The object must respect placement restrictions?
        public bool placeAnywhere;
        // Positioning method used for the object.
        public ObjectPositioningMethod positioningMethod;
    }

}