using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MapAssembler is an abstract class used to implement any kind of map assembler weapon. A map
/// assembler is used to generate a physical representation of a map starting from its matrix form.
/// </summary>
public abstract class MapAssembler : CoreComponent {

    // Wall height.
    [SerializeField]
    protected float wallHeight = 5f;
    // Square size.
    [SerializeField]
    protected float squareSize = 1f;

    public abstract void AssembleMap(char[,] map, char wallChar, char roomChar);

    public abstract void AssembleMap(List<char[,]> maps, char wallChar, char roomChar,
        char voidChar);

    public float GetSquareSize() {
        return squareSize;
    }

    public float GetWallHeight() {
        return wallHeight;
    }

}