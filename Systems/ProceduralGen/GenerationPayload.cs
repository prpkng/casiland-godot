using Godot;

namespace Casiland.Systems.ProceduralGen;

/// <summary>
/// A collection of data that is passed by the ProcGen Generate() function to the steps.
/// </summary>
public struct GenerationPayload
{
    public Node2D PropsGroup;
    public TileMapLayer AutoTileLayer;
    public TileMapLayer CollisionsLayer;
}