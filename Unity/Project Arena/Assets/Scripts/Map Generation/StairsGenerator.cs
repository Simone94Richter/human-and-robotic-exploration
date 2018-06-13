using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StairsGenerator is a class used to generate stairs in multi-level maps.
/// </summary>
public class StairsGenerator : CoreComponent {

    [SerializeField] private int stairsPerLevel = 4;
    [SerializeField] private int stairLength = 4;
    [SerializeField] private char voidChar = 'O';
    [SerializeField] private char stairCharUp = 'W';
    [SerializeField] private char stairCharRigth = 'D';
    [SerializeField] private char stairCharDown = 'S';
    [SerializeField] private char stairCharLeft = 'A';

    private MapGenerator mapGeneratorScript = null;
    private List<char[,]> maps = null;

    private char roomChar;
    private char wallChar;

    private char[] stairChars = new char[4];

    private void Start() {
        stairChars[0] = stairCharUp;
        stairChars[1] = stairCharRigth;
        stairChars[2] = stairCharDown;
        stairChars[3] = stairCharLeft;

        SetReady(true);
    }

    // Places stairs connecting adjacent levels of the map.
    public void GenerateStairs(List<char[,]> ms, MapGenerator mg) {
        SetMapGenerationVariables(mg);
        maps = ms;

        for (int currentLevel = 0; currentLevel < maps.Count; currentLevel++) {
            if (currentLevel > 0) {
                GenerateLevelStairs(GetPossibleStairs(currentLevel), currentLevel);
            }
        }
    }

    // Places stairs connecting adjacent levels of the map.
    public void GenerateStairs(List<char[,]> ms, List<bool> hasPlacedStairs, MapGenerator mg,
        bool validateOnly) {
        SetMapGenerationVariables(mg);
        maps = ms;

        for (int currentLevel = 0; currentLevel < maps.Count; currentLevel++) {
            if (currentLevel > 0) {
                if (hasPlacedStairs[currentLevel]) {
                    GenerateLevelStairs(GetValidatedStairs(currentLevel), currentLevel);
                } else if (!validateOnly) {
                    GenerateLevelStairs(GetPossibleStairs(currentLevel), currentLevel);
                }
            }
        }
    }

    // Sets all the informations about the map generation.
    private void SetMapGenerationVariables(MapGenerator mg) {
        mapGeneratorScript = mg;
        roomChar = mapGeneratorScript.GetRoomChar();
        wallChar = mapGeneratorScript.GetWallChar();
    }

    // Tells if its possible to place a stair in that direction.
    private bool CanPlaceStair(int x, int y, int currentLevel, char direction) {
        if (direction == stairCharUp) {
            // Up.
            if (y - (stairLength - 1) > 0) {
                if (maps[currentLevel][x, y] == roomChar &&
                    maps[currentLevel - 1][x, y - (stairLength - 1)] == roomChar) {
                    for (int i = 1; i < stairLength - 1; i++) {
                        if (!((maps[currentLevel][x, y - i] == roomChar ||
                            maps[currentLevel][x, y - i] == wallChar) &&
                            maps[currentLevel - 1][x, y - i] == roomChar)) {
                            return false;
                        }
                    }
                    return true;
                }
            }
        } else if (direction == stairCharRigth) {
            // Rigth.
            if (x + (stairLength - 1) < maps[currentLevel].GetLength(0)) {
                if (maps[currentLevel][x, y] == roomChar &&
                    maps[currentLevel - 1][x + (stairLength - 1), y] == roomChar) {
                    for (int i = 1; i < stairLength - 1; i++) {
                        if (!((maps[currentLevel][x + i, y] == roomChar ||
                            maps[currentLevel][x + i, y] == wallChar) &&
                            maps[currentLevel - 1][x + i, y] == roomChar)) {
                            return false;
                        }
                    }
                    return true;
                }
            }
        } else if (direction == stairCharDown) {
            // Down.
            if (y + (stairLength - 1) < maps[currentLevel].GetLength(1)) {
                if (maps[currentLevel][x, y] == roomChar &&
                    maps[currentLevel - 1][x, y + (stairLength - 1)] == roomChar) {
                    for (int i = 1; i < stairLength - 1; i++) {
                        if (!((maps[currentLevel][x, y + i] == roomChar ||
                            maps[currentLevel][x, y + i] == wallChar) &&
                            maps[currentLevel - 1][x, y + i] == roomChar)) {
                            return false;
                        }
                    }
                    return true;
                }
            }
        } else if (direction == stairCharLeft) {
            // Left.
            if (x - (stairLength - 1) > 0) {
                if (maps[currentLevel][x, y] == roomChar &&
                    maps[currentLevel - 1][x - (stairLength - 1), y] == roomChar) {
                    for (int i = 1; i < stairLength - 1; i++) {
                        if (!((maps[currentLevel][x - i, y] == roomChar ||
                            maps[currentLevel][x - i, y] == wallChar) &&
                            maps[currentLevel - 1][x - i, y] == roomChar)) {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }
        return false;
    }

    // Tells if its possible to place a given stair.
    private bool CanPlaceStair(int originX, int originY, int endX, int endY, int currentLevel) {
        int direction = 0;

        if (originY < endY) {
            direction = 1;
        } else if (originY > endY) {
            direction = 3;
        } else if (originX < endX) {
            direction = 2;
        } else { direction = 4; }

        return CanPlaceStair(originX, originY, currentLevel, stairChars[direction - 1]);
    }

    // Generates the stairs of a single level.
    private void GenerateLevelStairs(List<Stair> stairList, int currentLevel) {
        if (stairList.Count > 2) {
            int placedStairs = 0;
            while (stairList.Count > 0 && placedStairs < stairsPerLevel) {
                Stair currentStair = stairList[mapGeneratorScript.GetRandomInteger(0,
                    stairList.Count)];
                stairList.Remove(currentStair);
                if (CanPlaceStair(currentStair.originX, currentStair.originY, currentStair.endX,
                    currentStair.endY, currentLevel)) {
                    PlaceStair(currentStair, currentLevel);
                    placedStairs++;
                }
            }
            if (placedStairs == 0) {
                ManageError(Error.HARD_ERROR, "Error while populating the map, stairs could not " +
                    "be placed.\nPlease use another input.");
            }
        } else if (stairList.Count == 1) {
            PlaceStair(stairList[0], currentLevel);
        } else {
            ManageError(Error.HARD_ERROR, "Error while populating the map, stairs could not be " +
                "placed.\nPlease use another input.");
        }
    }

    // Returns a list of the stairs of the level that satisfy the validation conditions.
    private List<Stair> GetValidatedStairs(int currentLevel) {
        List<Stair> stairList = new List<Stair>();

        // Detect and valdiate the stairs.
        for (int x = 0; x < maps[currentLevel].GetLength(0); x++) {
            for (int y = 0; y < maps[currentLevel].GetLength(1); y++) {
                if (maps[currentLevel][x, y] == stairCharUp) {
                    maps[currentLevel][x, y] = roomChar;
                    if (CanPlaceStair(x, y, currentLevel, stairCharUp)) {
                        stairList.Add(new Stair {
                            originX = x,
                            originY = y,
                            endX = x,
                            endY = y - (stairLength - 1)
                        });
                    }
                } else if (maps[currentLevel][x, y] == stairCharRigth) {
                    maps[currentLevel][x, y] = roomChar;
                    if (CanPlaceStair(x, y, currentLevel, stairCharRigth)) {
                        stairList.Add(new Stair {
                            originX = x,
                            originY = y,
                            endX = x + (stairLength - 1),
                            endY = y
                        });
                    }
                } else if (maps[currentLevel][x, y] == stairCharDown) {
                    maps[currentLevel][x, y] = roomChar;
                    if (CanPlaceStair(x, y, currentLevel, stairCharDown)) {
                        stairList.Add(new Stair {
                            originX = x,
                            originY = y,
                            endX = x,
                            endY = y + (stairLength - 1)
                        });
                    }
                } else if (maps[currentLevel][x, y] == stairCharLeft) {
                    maps[currentLevel][x, y] = roomChar;
                    if (CanPlaceStair(x, y, currentLevel, stairCharLeft)) {
                        stairList.Add(new Stair {
                            originX = x,
                            originY = y,
                            endX = x - (stairLength - 1),
                            endY = y
                        });
                    }
                }
            }
        }

        return stairList;
    }

    // Returns a list of the possible stairs that can be placed between the two levels passed as 
    // parameter.
    private List<Stair> GetPossibleStairs(int currentLevel) {
        List<Stair> stairList = new List<Stair>();

        for (int x = 0; x < maps[currentLevel].GetLength(0); x++) {
            for (int y = 0; y < maps[currentLevel].GetLength(1); y++) {
                if (maps[currentLevel][x, y] == roomChar) {
                    for (int i = 0; i < 4; i++) {
                        AddToListIfPossible(currentLevel, x, y, stairList, stairChars[i]);
                    }
                }
            }
        }

        return stairList;
    }

    // Places a stair.
    private void PlaceStair(Stair stair, int currentLevel) {
        if (stair.originY == stair.endY) {
            if (stair.originX > stair.endX) {
                maps[currentLevel][stair.originX, stair.originY] = stairCharLeft;
            } else {
                maps[currentLevel][stair.originX, stair.originY] = stairCharRigth;
            }
        } else {
            if (stair.originY > stair.endY) {
                maps[currentLevel][stair.originX, stair.originY] = stairCharUp;
            } else {
                maps[currentLevel][stair.originX, stair.originY] = stairCharDown;
            }
        }

        for (int j = 1; j < stairLength - 1; j++) {
            if (stair.originY == stair.endY) {
                if (stair.originX > stair.endX) {
                    maps[currentLevel][stair.originX - j, stair.originY] = voidChar;
                } else {
                    maps[currentLevel][stair.originX + j, stair.originY] = voidChar;
                }
            } else {
                if (stair.originY > stair.endY) {
                    maps[currentLevel][stair.originX, stair.originY + j] = voidChar;
                } else {
                    maps[currentLevel][stair.originX, stair.originY - j] = voidChar;
                }
            }
        }
    }

    // Adds a new stair to the list, if possible. The origin of the stair is marked as a room tile 
    // because of maps with already placed stair. 
    private void AddToListIfPossible(int currentLevel, int x, int y, List<Stair> stairList,
        char direction) {
        if (direction == stairCharUp) {
            if (CanPlaceStair(x, y, currentLevel, stairCharUp)) {
                stairList.Add(new Stair {
                    originX = x,
                    originY = y,
                    endX = x,
                    endY = y - (stairLength - 1)
                });
            }
            maps[currentLevel][x, y] = roomChar;
        } else if (direction == stairCharRigth) {
            if (CanPlaceStair(x, y, currentLevel, stairCharRigth)) {
                stairList.Add(new Stair {
                    originX = x,
                    originY = y,
                    endX = x + (stairLength - 1),
                    endY = y
                });
            }
            maps[currentLevel][x, y] = roomChar;
        } else if (direction == stairCharDown) {
            if (CanPlaceStair(x, y, currentLevel, stairCharDown)) {
                stairList.Add(new Stair {
                    originX = x,
                    originY = y,
                    endX = x,
                    endY = y + (stairLength - 1)
                });
            }
            maps[currentLevel][x, y] = roomChar;
        } else if (direction == stairCharLeft) {
            if (CanPlaceStair(x, y, currentLevel, stairCharLeft)) {
                stairList.Add(new Stair {
                    originX = x,
                    originY = y,
                    endX = x - (stairLength - 1),
                    endY = y
                });
            }
            maps[currentLevel][x, y] = roomChar;
        }
    }

    public char GetVoidChar() {
        return voidChar;
    }

    private struct Stair {
        public int originX;
        public int endX;
        public int originY;
        public int endY;
    }

}