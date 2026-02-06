using System.Collections.Generic;
using Casiland.AutoTileEx.Api.Data;
using Godot;

namespace Casiland.Systems.ProceduralGen.Steps;

public class GenerationState
{
    public RandomNumberGenerator Rng;
    public Node2D PropsGroup;
    public TileMapLayer TilemapLayer;

    /// <summary>
    /// All rooms generated at the first pass
    /// </summary>
    public List<ProceduralRoom> GeneratedRooms = [];
    /// <summary>
    /// The rooms that were rejected during the selection step.
    /// </summary>
    public List<ProceduralRoom> OtherRooms = [];

    /// <summary>
    /// The main points of interest
    /// </summary>
    public List<ProceduralRoom> MainRooms = [];
    /// <summary>
    /// The minor rooms that connect the main rooms within corridors.
    /// </summary>
    public List<ProceduralRoom> CorridorRooms = [];

    /// <summary>
    /// The complete list of all rooms (main + corridor).
    /// </summary>
    public List<ProceduralRoom> AllRooms;

    public List<LineSegment> MinimumSpanningTree = [];
    public List<LineSegment> CorridorLines = [];
    public List<LineSegment[]> CorridorLineGroups = [];

    public Dictionary<Vector2I, ProceduralRoom> PointToRoom = [];


}