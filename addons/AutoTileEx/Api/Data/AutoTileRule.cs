using Godot;
using Godot.Collections;

namespace Casiland.AutoTileEx.Api.Data;

[System.Serializable]
public enum NeighbourType
{
    Ignore = 0,
    Nothing = 1,
    Required = 2,
    Other = 3
}

[Tool]
[GlobalClass]
public partial class AutoTileRule : Resource
{
    [Export] public Vector2I TargetTileCoords { get; set; }
    [Export] public int TargetTileSourceId { get; set; }

    /// Neighbours in clockwise order: N, NE, E, SE, S, SW, W, NW
    [Export]
    public Array<NeighbourType> Neighbours { get; set; } = new(new NeighbourType[MaxCellCount]);

    public const int Radius = 3;
    [Export] public int ArrayChunkCount { get; set; }
    public const int MaxCellCount = 49;
    [Export] public Array<ulong> RequiredMask { get; set; } = [];
    [Export] public Array<ulong> EmptyMask { get; set; } = [];
    [Export] public Array<ulong> OtherMask { get; set; } = [];


    public static int GridToIndex(int x, int y, int radius)
    {
        int size = radius * 2 + 1;
        return y * size + x;
    }
    
    private static void SetBit(Array<ulong> chunks, int index)
    {
        int chunk = index / 64;
        int bit   = index % 64;
        chunks[chunk] |= (1UL << bit);
    }

    public void RecomputeMasks()
    {
        RequiredMask.Clear();
        EmptyMask.Clear();
        OtherMask.Clear();
        
        ArrayChunkCount = Mathf.CeilToInt(MaxCellCount / 64f);
        for (int i = 0; i < ArrayChunkCount; i++)
        {
            RequiredMask.Add(0);
            EmptyMask.Add(0);
            OtherMask.Add(0);
        }
        
        for (int i = 0; i < Neighbours.Count; i++)
        {
            switch (Neighbours[i])
            {
                case  NeighbourType.Required:
                    SetBit(RequiredMask, i);
                    break;
                case  NeighbourType.Nothing:
                    SetBit(EmptyMask, i);
                    break;
                case  NeighbourType.Other:
                    SetBit(OtherMask, i);
                    break;
                case NeighbourType.Ignore:
                default:
                    break;
            }
        }
    }
}