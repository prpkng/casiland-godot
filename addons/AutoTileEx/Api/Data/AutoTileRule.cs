using Godot;
using Godot.Collections;

namespace Casiland.AutoTileEx.Api.Data;

[System.Serializable]
public enum NeighbourType
{
    Ignore = 0,
    Nothing = 1,
    Match = 2,
    Other = 3
}

[Tool]
[GlobalClass]
public partial class AutoTileRule : Resource
{
    [Export] public Vector2I SourceTileCoords { get; set; }
    [Export] public int SourceTileSourceId { get; set; }
    [Export] public Vector2I TargetTileCoords { get; set; }
    [Export] public int TargetTileSourceId { get; set; }

    /// Neighbours in clockwise order: N, NE, E, SE, S, SW, W, NW
    [Export] public Array<NeighbourType> Neighbours { get; set; }

    [Export] public byte RequiredMask { get; set; }
    [Export] public byte EmptyMask { get; set; }
    [Export] public byte OtherMask { get; set; }


    public void RecomputeMasks()
    {
        byte req = 0;
        byte empty = 0;
        byte other = 0;

        for (int i = 0; i < Neighbours.Count; i++)
        {
            switch (Neighbours[i])
            {
                case  NeighbourType.Match:
                    req |= (byte)(1 << i);
                    break;
                case  NeighbourType.Nothing:
                    empty |= (byte)(1 << i);
                    break;
                case  NeighbourType.Other:
                    other |= (byte)(1 << i);
                    break;
                case NeighbourType.Ignore:
                default:
                    break;
            }
        }

        RequiredMask = req;
        EmptyMask = empty;
        OtherMask = other;
    }
}